using System;
using System.Collections.Generic;
using System.IO;
using CppInator.Runtime;
using UnityEditor;
using Object = UnityEngine.Object;
using System.Linq;
using UnityEngine;

namespace CppInator.Editor
{
    public class CppAssembliesTracker : AssetModificationProcessor
    {
        static readonly HashSet<CppAssembly> _assemblies = new ();
        
        public static IReadOnlyCollection<CppAssembly> Assemblies => _assemblies;
        
        static readonly Dictionary<CppAssembly, string[]> _assemblyFiles = new();
        
        public static IReadOnlyDictionary<CppAssembly, string[]> AssemblyFiles => _assemblyFiles;
        
        static bool _asmDirty;

        [InitializeOnLoadMethod]
        static void OnLoad()
        {
            var allAssemblies = AssetDatabase.FindAssets($"t:{nameof(CppAssembly)}");

            foreach (var path in allAssemblies)
            {
                var asm = AssetDatabase.LoadAssetAtPath<CppAssembly>(AssetDatabase.GUIDToAssetPath(path));
                _assemblies.Add(asm);
            }
            
            CppFileTracker.OnCppFileChanged -= OnCppFileChanged;
            CppFileTracker.OnCppFileChanged += OnCppFileChanged;
            
            CppFileTracker.OnCppAssemblyChanged -= OnAssemblyModified;
            CppFileTracker.OnCppAssemblyChanged += OnAssemblyModified;
             
            CppFileTracker.OnHeaderFileChanged -= OnHeaderFileChanged;
            CppFileTracker.OnHeaderFileChanged += OnHeaderFileChanged;
            
            CppAssembly.OnAssemblyCreated -= OnAssemblyCreatedOnce;
            CppAssembly.OnAssemblyCreated += OnAssemblyCreatedOnce;
            
            CppAssembly.OnAssemblyModified -= OnAssemblyModified;
            CppAssembly.OnAssemblyModified += OnAssemblyModified;
            
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
            
            _asmDirty = true;
        }
        
        public static bool areAssembliesDirty => _asmDirty;

        private static void OnUpdate()
        {
            if (_asmDirty)
            {
                AssembliesDirty();
                _asmDirty = false;
            }
        }

        private static void OnHeaderFileChanged(FileAction action, string path)
        {
            CCompiler.SetHeaderDirty(Path.GetFullPath(path).Replace('\\', '/'));
            BuildDependencyGraph();
        }
        
        static bool TryGetAsm(string filePath, out CppAssembly asm)
        {
            foreach (var assembly in _assemblies)
            {
                var path = GetAsmPath(assembly);
                
                if (path != null && filePath.StartsWith(path))
                {
                    asm = assembly;
                    return true;
                }
            }

            asm = null;
            return false;
        }
        
        public static string GetHeadersIncludeCmd()
        {
            var headers = string.Empty;

            foreach (var asm in _assemblies)
            {
                if (asm.includeHeaders && asm.includeInBuild)
                    headers += $"-I{Path.GetFullPath(GetAsmPath(asm)).Replace('\\', '/')} ";
            }
            
            return headers.Trim();
        }

        private static void OnCppFileChanged(FileAction type, string path)
        {
            if (TryGetAsm(path, out var asm))
            {
                bool isNewOrDeleted = false;
                
                switch (type)
                {
                    case FileAction.Deleted:
                        CCompiler.RemoveSourceFile(path);
                        isNewOrDeleted = true;
                        break;
                    default:
                        if (CCompiler.AddSourceFile(path))
                            isNewOrDeleted = true;
                        break;
                }

                if (isNewOrDeleted)
                    _assemblyFiles[asm] = Directory.GetFiles(GetAsmPath(asm), "*.cpp", SearchOption.AllDirectories);
            }

            BuildDependencyGraph();
        }

        public static string GetAsmPath(Object asm)
        {
            var filePath = AssetDatabase.GetAssetPath(asm);

            if (!File.Exists(filePath))
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                filePath = AssetDatabase.GetAssetPath(asm);
                
                if (!File.Exists(filePath))
                    Debug.LogWarning("[CppInator] AssetDatabase.GetAssetPath returned a path that doesn't exist. This is a bug in Unity. If it persists, please restart Unity.");
            }

            try
            {
                var dirPath = Path.GetDirectoryName(filePath);

                return string.IsNullOrEmpty(dirPath) ? null : dirPath.Replace("\\", "/");
            }
            catch
            {
                return null;
            }
        }

        private static void AssembliesDirty()
        {
            _assemblyFiles.Clear();
            
            foreach (var asm in _assemblies)
                OnAssemblyCreated(asm);
            BuildDependencyGraph();
            
            CCompiler.SetVSCodeDirty();
        }

        static readonly HashSet<string> _visited = new();

        private static void BuildDependencyGraph()
        {
            EditorUtility.DisplayProgressBar("CppInator", "Building C++ depency graph", 0.75f);
            _visited.Clear();

            CCompiler.ClearDepGraph();

            foreach (var (_, cppFiles) in _assemblyFiles)
            {
                foreach (var cppFile in cppFiles)
                {
                    _visited.Add(cppFile);
                    var dependencies = GetHeaderDependencies(cppFile);

                    foreach (var dep in dependencies)
                        CCompiler.AddDependency(cppFile.Replace('\\', '/'), dep.Replace('\\', '/'));
                }
            }
            
            EditorUtility.ClearProgressBar();
        }

        public static char GetNextNonWhitespaceChar(string content, ref int carret)
        {
            while (char.IsWhiteSpace(content[carret]))
                carret++;

            return content[carret];
        }

        static string[] GetHeaderDependencies(string cppFile)
        {
            const string includePattern = "#include";
            var paths = new HashSet<string>();

            var content = RegenerateBindingsIfSignatureChanges.RemoveCommentsFromString(
                File.ReadAllText(cppFile)
            );
            
            var carret = 0;

            while (carret < content.Length)
            {
                var nextInlude = content.IndexOf(includePattern, carret, StringComparison.Ordinal);

                if (nextInlude == -1)
                    break;

                carret = nextInlude + includePattern.Length;

                var c = GetNextNonWhitespaceChar(content, ref carret);

                if (c != '<' && c != '"')
                    continue;

                var start = carret + 1;
                var end = content.IndexOf(c == '<' ? '>' : '"', start);

                var dependency = content[start..end];
                
                if (CCompiler.IsFileFromIncludePath(dependency, out var includePath))
                {
                    if (!_visited.Contains(includePath))
                    {
                        _visited.Add(includePath);
                        var itsDeps = GetHeaderDependencies(includePath);
                        paths.UnionWith(itsDeps);
                    }

                    paths.Add(includePath);
                }
            }

            return paths.ToArray();
        }
        
        private static void OnAssemblyModified(CppAssembly obj)
        {
            _asmDirty = true;
        }
        
        private static void OnAssemblyCreatedOnce(CppAssembly obj)
        {
            OnAssemblyCreated(obj);
            BuildDependencyGraph();

            CCompiler.SetVSCodeDirty();
        }

        private static void OnAssemblyCreated(CppAssembly obj)
        {
            _assemblies.Add(obj);
            
            var path = GetAsmPath(obj);
            var getAllFiles = Directory.GetFiles(path, "*.cpp", SearchOption.AllDirectories);

            _assemblyFiles[obj] = getAllFiles;
            
            if (obj.includeSources)
            {
                foreach (var file in getAllFiles)
                    CCompiler.AddSourceFile(file.Replace("\\", "/"));
            }
            else
            {
                foreach (var file in getAllFiles)
                    CCompiler.RemoveSourceFile(file.Replace("\\", "/"));
            }

            if (obj.includeHeaders && obj.ShouldIncludeInEditor())
            {
                CCompiler.AddIncludePath(path);
            }
            else
            {
                CCompiler.RemoveIncludePath(path);
            }
        }
        
        private static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        {
            if (assetPath.EndsWith(".cpp") || assetPath.EndsWith(".h"))
                CCompiler.RemoveSourceFile(assetPath);
            
            var assembly = AssetDatabase.LoadAssetAtPath<CppAssembly>(assetPath);

            if (assembly != null)
            {
                AssembliesDirty();
            }
            
            return AssetDeleteResult.DidNotDelete;
        }
    }
}
