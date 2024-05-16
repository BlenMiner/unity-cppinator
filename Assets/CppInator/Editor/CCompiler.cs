using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using CppInator.Runtime;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace CppInator.Editor
{
    public static class CCompiler
    {
        private const string OBJECTS_PATH = "./obj/CPinator/ObjectFiles";
        private const string COMPILED_PATH = "./obj/CPinator";

        static readonly HashSet<string> _sourceFiles = new();
        static readonly Dictionary<string, string> _objectFiles = new();
        static readonly HashSet<string> _headerPaths = new();
        static readonly HashSet<string> _dirtyFiles = new();
        static readonly Dictionary<string, HashSet<string>> _hToCpp = new();
        
        static bool _shouldLink;

        const string gFlags = 
            "-Werror=implicit-function-declaration " +
            "-Werror " +
            "-Wno-c++11-extensions " +
            "-std=c++11 " +
            "-fexceptions " +
            "-fno-rtti " +
            "-fpermissive " +
            "-O3 " +
            "-g "+
            "-pthread ";
        
        [MenuItem("Tools/CppInator/Build")]
        public static void PassiveLink()
        {
            SetAllDirty();
        }
        
        [MenuItem("Tools/CppInator/Clean Build")]
        public static void ForceLink()
        {
            SetAllDirty();

            foreach (var cppFile in _sourceFiles)
            {
                var guid = AssetDatabase.GUIDFromAssetPath(cppFile);
                var objPath = OBJECTS_PATH + "/" + guid + ".o";
                var fullObjPath = Path.GetFullPath(objPath);
                
                if (File.Exists(fullObjPath))
                    File.Delete(fullObjPath);
            }
        }

        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            if (!Directory.Exists(COMPILED_PATH))
                Directory.CreateDirectory(COMPILED_PATH);
            
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            if (!EditorApplication.isFocused || EditorApplication.isPlayingOrWillChangePlaymode) return;
            
            if (CppAssembliesTracker.areAssembliesDirty) return;
            
            bool showingProgressBar = false;

            if (_dirtyFiles.Count == 0 && !_shouldLink) return;

            if (_vsCodeDirty)
            {
                UpdateVSCodeSettings.Update(_headerPaths);
            }

            if (!CheckForCompiler.Installed())
            {
                _shouldLink = false;
                _dirtyFiles.Clear();
                CheckForCompiler.Check();
                return;
            }

            if (_dirtyFiles.Count > 0)
            {
                foreach (var path in _dirtyFiles)
                {
                    if (!showingProgressBar)
                    {
                        EditorUtility.DisplayProgressBar("CppInator",
                            "Checking C++ source files for changes.", 0f);
                        showingProgressBar = true;
                    }

                    try
                    {
                        CompileObject(path);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }

                _dirtyFiles.Clear();
            }

            if (_shouldLink)
            {
                try
                {
                    LinkDll(true);
                }
                catch (UnauthorizedAccessException)
                {
                    // ignore
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    _shouldLink = false;
                }
            }

            if (showingProgressBar)
                EditorUtility.ClearProgressBar();
        }

        static int RunCMD(string file, string args, out string error)
        {
            if (Path.IsPathFullyQualified(file))
            {
                try
                {
                    var p = new Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.FileName = file;
                    p.StartInfo.Arguments = args;
                    p.Start();

                    error = p.StandardError.ReadToEnd();
                    p.WaitForExit();
                    return p.ExitCode;
                }
                catch
                {
                    error = string.Empty;
                    return -1;
                }
            }
            else
            {
                return CmdRunner.RunCommand(file + " " + args, out error);
            }
        }

        public static void ClearDepGraph()
        {
            _hToCpp.Clear();
        }

        public static void AddDependency(string cppFile, string hFile)
        {
            if (_hToCpp.TryGetValue(hFile, out var deps))
                 deps.Add(cppFile);
            else _hToCpp.Add(hFile, new HashSet<string> {cppFile});
        }

        public static void AddIncludePath(string path)
        {
            if (_headerPaths.Add(path))
                _shouldLink = true;
        }
        
        public static void RemoveIncludePath(string path)
        {
            if (_headerPaths.Remove(path))
                _shouldLink = true;
        }

        public static void RemoveSourceFile(string path)
        {
            var guid = AssetDatabase.GUIDFromAssetPath(path);
            
            var objPath = OBJECTS_PATH + "/" + guid + ".o";
            var fullObjPath = Path.GetFullPath(objPath);

            if (_objectFiles.ContainsKey(fullObjPath))
            {
                File.Delete(fullObjPath);
                _objectFiles.Remove(fullObjPath);
            }
            
            _sourceFiles.Remove(path);
            _shouldLink = true;
        }
        
        public static bool AddSourceFile(string path)
        {
            bool added = _sourceFiles.Add(path);
            DirtySourceFile(path);
            
            return added;
        }
        
        static void DirtySourceFile(string path)
        {
            if (!_sourceFiles.Contains(path)) 
                return;

            _dirtyFiles.Add(path);
        }
        
        public static void SetLinkDirty()
        {
            _shouldLink = true;
        }

        static void LogError(string msg, string file, int line, int col)
        {
            try
            {
                typeof(Debug).GetMethod("LogPlayerBuildError", 
                    BindingFlags.NonPublic | 
                    BindingFlags.InvokeMethod | 
                    BindingFlags.Static)
                ?.Invoke(null, new object[] {msg, file, line, col});
            }
            catch
            {
                Debug.LogError(msg);
            }
        }

        static string GetHeadersCompilerArg()
        {
            var headers = string.Empty;

            foreach (var paths in _headerPaths)
                headers += $"-I\"{Path.GetFullPath(paths)}\" ";

            return headers;
        }
        
        public static string GetHeadersIncludeCmd()
        {
            return CppAssembliesTracker.GetHeadersIncludeCmd();
        }
        
        static void CompileObject(string path)
        {
            if (!File.Exists(path))
                return;

            Directory.CreateDirectory(OBJECTS_PATH);

            var guid = AssetDatabase.GUIDFromAssetPath(path);
            var objPath = OBJECTS_PATH + "/" + guid + ".o";
            var fullObjPath = Path.GetFullPath(objPath);

            _objectFiles[fullObjPath] = path; 

            var cppWriteTime = File.GetLastWriteTime(path);
            var objWriteTime = File.Exists(fullObjPath) ? File.GetLastWriteTime(fullObjPath) : DateTime.MinValue;

            if (objWriteTime >= cppWriteTime)
                return;

            // ensure we don't link old object files
            if (File.Exists(fullObjPath))
                File.Delete(fullObjPath);
            
            var fullPath = Path.GetFullPath(path);
            var headers = GetHeadersCompilerArg();
            var gcc = CheckForCompiler.GetCompilerPath();
            var cmd = $"{gFlags}-m64 -c \"{fullPath}\" -o \"{fullObjPath}\" {headers}";

            EditorUtility.DisplayProgressBar("CppInator",
                        "Compiling " + Path.GetFileName(path), 0.25f);

            var result = RunCMD(gcc, cmd, out var error);

            EditorUtility.ClearProgressBar();
            
            if (result != 0)
            {
                var formatedError = FormatError(error, out var line, out var col);
                LogError(
                    formatedError, 
                    path, 
                    line, 
                    col
                );

                var dllPath = Path.GetFullPath(Native.DLL_PATH);

                if (File.Exists(dllPath))
                    File.Delete(dllPath);

                _shouldLink = false;
            }
            else
            {
                File.SetLastAccessTime(fullObjPath, cppWriteTime);
                _shouldLink = true;
            }
        }

        private static string FormatError(string error, out int firstLine, out int firstCol)
        {
            var assetsPath = Path.Combine(Directory.GetCurrentDirectory());
            var caret = 0;
            int last = -1;

            bool assigned = false;
            firstLine = 0;
            firstCol = 0;
            
            while (caret < error.Length)
            {
                if (caret <= last)
                    break;
                
                last = caret;
                caret = error.IndexOf(assetsPath, caret, StringComparison.Ordinal);
                
                if (caret == -1)
                    break;
                
                var startOfPath = caret;
                caret += assetsPath.Length;

                var end = error.IndexOf(":", caret, StringComparison.Ordinal);

                if (end == -1)
                {
                    caret++;
                    continue;
                }
                
                var path = error.Substring(startOfPath, end - startOfPath);
                var afterEnd = error.IndexOf(":", end + 1, StringComparison.Ordinal);
                var relativePath = path[(assetsPath.Length + 1)..];

                if (afterEnd == -1)
                {
                    caret++;
                    continue;
                }
                
                var line = error.Substring(end + 1, afterEnd - end - 1);
                
                if (int.TryParse(line, out var lineVal))
                {
                    var col = error.IndexOf(":", afterEnd + 1, StringComparison.Ordinal);

                    if (col == -1)
                    {
                        caret++;
                        continue;
                    }
                    
                    var colVal = error.Substring(afterEnd + 1, col - afterEnd - 1);

                    if (int.TryParse(colVal, out var colInt))
                    {
                        error = error.Remove(startOfPath, col + 1 - startOfPath);
                        error = error.Insert(startOfPath, $"<a href=\"{path}\" line=\"{lineVal}\" col=\"{colVal}\">{relativePath}:{lineVal}:{colVal}</a>");

                        if (!assigned)
                        {
                            assigned = true;
                            firstLine = lineVal;
                            firstCol = colInt;
                        }
                    }
                }
            }

            return error;
        }
        
        static void LinkDll(bool enableProgressBar)
        {
            if (!CheckForCompiler.Installed())
            {
                CheckForCompiler.Check();
                _shouldLink = false;
                return;
            }
            
            UpdateVSCodeSettings.Update(_headerPaths);
            
            if (enableProgressBar)
                EditorUtility.DisplayProgressBar("CppInator", "Linking C++", 1f);

            var dllPath = Path.GetFullPath(Native.DLL_PATH);
            var gcc = CheckForCompiler.GetCompilerPath();

            // ensure we don't load old dlls if linking fails
            if (File.Exists(dllPath))
                File.Delete(dllPath);

            string objFiles = string.Empty;

            List<string> keys = new (_objectFiles.Keys);
            List<string> values = new (_objectFiles.Values);

            for (int i = 0; i < keys.Count; i++)
            {
                string obj = keys[i];

                if (!File.Exists(obj))
                    CompileObject(values[i]);

                objFiles += $"\"{obj}\" ";
            }

            var dllLink = RunCMD(gcc, $"-shared -g -m64 -o \"{dllPath}\" {objFiles}",
                out var linkError);

            if (dllLink != 0)
                Debug.LogError("Failed to link C++ dll\n" + linkError);

            if (enableProgressBar)
                EditorUtility.ClearProgressBar();
            
            _shouldLink = false;
        }

        public static void SetAllDirty()
        {
            _shouldLink = true;
            _dirtyFiles.Clear();
            
            foreach (var path in _sourceFiles)
                DirtySourceFile(path);
        }

        internal static bool IsFileFromIncludePath(string dep, out string includePath)
        {
            foreach (var path in _headerPaths)
            {
                var fullPath = Path.GetFullPath(path);
                var depPath = fullPath + "/" + dep;

                if (File.Exists(depPath))
                {
                    includePath = depPath;
                    return true;
                }
            }

            includePath = null;
            return false;
        }

        internal static void SetHeaderDirty(string v)
        {
            if (_hToCpp.TryGetValue(v, out var cppFiles))
            {
                foreach (var cpp in cppFiles)
                {
                    var guid = AssetDatabase.GUIDFromAssetPath(cpp);
                    var objPath = OBJECTS_PATH + "/" + guid + ".o";
                    var fullObjPath = Path.GetFullPath(objPath);

                    if (File.Exists(fullObjPath))
                        File.Delete(fullObjPath);

                    DirtySourceFile(cpp);
                }
            }
        }

        private static bool _vsCodeDirty;

        public static void SetVSCodeDirty()
        {
            _vsCodeDirty = true;
        }
    }
}
