using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CppInator.Runtime;
using UnityEditor;
using UnityEditor.Compilation;

namespace CppInator.Editor
{
    public static class RegenerateBindingsIfSignatureChanges
    {
        [InitializeOnLoadMethod]
        static void Init()
        {
            foreach (var asm in CppAssembliesTracker.Assemblies)
                OnCreatedAssembly(asm);
            
            CppFileTracker.OnCppFileChanged -= OnFilesChanged;
            CppFileTracker.OnCppFileChanged += OnFilesChanged;

            CppFileTracker.OnHeaderFileChanged -= OnFilesChanged;
            CppFileTracker.OnHeaderFileChanged += OnFilesChanged;
            
            CppAssembly.OnAssemblyCreated -= OnCreatedAssembly;
            CppAssembly.OnAssemblyCreated += OnCreatedAssembly;
        }
        
        static readonly Dictionary<string, int> _fileToHash = new();
        
        public static string RemoveCommentsFromString(string script)
        {
            var sb = new StringBuilder();

            var caret = 0;

            while (caret < script.Length)
            {
                if (script[caret] == '/' && caret + 1 < script.Length)
                {
                    switch (script[caret + 1])
                    {
                        case '/':
                        {
                            caret += 2;

                            while (caret < script.Length && script[caret] != '\n')
                                ++caret;

                            continue;
                        }
                        case '*':
                        {
                            caret += 2;

                            while (caret < script.Length && (script[caret] != '*' || caret + 1 < script.Length && script[caret + 1] != '/'))
                                ++caret;

                            if (caret < script.Length)
                                caret += 2;

                            continue;
                        }
                    }
                }

                sb.Append(script[caret]);
                ++caret;
            }

            return sb.ToString();
        }

        
        static void OnCreatedAssembly(CppAssembly assembly) 
        {
            var path = CppAssembliesTracker.GetAsmPath(assembly);
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            
            for (var i = 0; i < files.Length; i++)
            {
                var file = files[i];
                if (file.EndsWith(".cpp") || file.EndsWith(".h"))
                    _fileToHash[file.Replace("\\", "/")] = GatherSignaturesHash(file);
            }
        }

        static readonly StringBuilder _sb = new();

        static int GatherSignaturesHash(string file)
        {
            _sb.Clear();
            
            int caret = 0;
            int prev = -1;
            
            var script = RemoveCommentsFromString(File.ReadAllText(file));
            const string EXPORT = "EXPORT";
            
            while (caret < script.Length && caret >= prev)
            {
                prev = caret;
                
                var nextExport = script.IndexOf(EXPORT, caret, StringComparison.Ordinal);
                
                if (nextExport == -1)
                    break;
                
                caret = nextExport + EXPORT.Length;
                
                if (script[caret] != '(')
                    continue;
                
                var nextCloseParen = script.IndexOf(')', caret);
                
                if (nextCloseParen == -1)
                    break;
                 
                var nextNextCloseParen = script.IndexOf(')', nextCloseParen + 1);
                
                if (nextNextCloseParen == -1)
                    break;
                
                var signature = script.Substring(nextExport, nextNextCloseParen - nextExport + 1);
                var parCount = 0;
                var parDepth = 0;
                bool valid = true;
                
                for (int j = 0; j < signature.Length; j++)
                {
                    if (signature[j] == '\n' && parCount < 3)
                    {
                        valid = false;
                        break;
                    }
                    
                    switch (signature[j])
                    {
                        case '(':
                            parCount++;
                            parDepth++;
                            break;
                        case ')':
                            parCount++;
                            parDepth--;
                            break;
                    }
                }
                 
                if (parCount >= 4 && parDepth == 0 && valid)
                    _sb.AppendLine(signature.Replace("  ", " "));
            }
            
            return _sb.Length == 0 ? 0 : _sb.ToString().GetHashCode();
        }
        
        static void RegenerateBindings()
        {
            // TODO: Regenerate bindings
            //CompilationPipeline.RequestScriptCompilation();
        }
        
        static void OnFilesChanged(FileAction action, string path)
        {
            if (!File.Exists(path))
            {
                if (_fileToHash.TryGetValue(path, out var h))
                {
                    _fileToHash.Remove(path);
                    if (h != 0) RegenerateBindings();
                }
                return;
            }
            
            if (_fileToHash.TryGetValue(path, out var hash))
            {
                var newHash = GatherSignaturesHash(path);
                if (newHash == hash) return;
                RegenerateBindings();
            }
            else
            {
                hash = GatherSignaturesHash(path);
                _fileToHash[path] = hash;
                if (hash != 0) RegenerateBindings();
            }
        }
    }
}
