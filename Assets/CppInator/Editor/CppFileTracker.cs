using CppInator.Runtime;
using UnityEditor;
using UnityEngine;

namespace CppInator.Editor
{
    public enum FileAction
    {
        Created,
        Deleted,
        Moved
    }
    
    public class CppFileTracker : AssetPostprocessor
    {
        public static event System.Action<FileAction, string> OnCppFileChanged;
        public static event System.Action<FileAction, string> OnHeaderFileChanged;
        public static event System.Action<CppAssembly> OnCppAssemblyChanged;
        
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            for (var i = 0; i < importedAssets.Length; i++)
            {
                var path = importedAssets[i];
                if (path.EndsWith(".asset") && AssetDatabase.LoadAssetAtPath<ScriptableObject>(path) is CppAssembly asm)
                    OnCppAssemblyChanged?.Invoke(asm);
            }
            
            for (var i = 0; i < movedAssets.Length; i++)
            {
                var path = movedAssets[i];
                if (path.EndsWith(".asset") && AssetDatabase.LoadAssetAtPath<ScriptableObject>(path) is CppAssembly asm)
                    OnCppAssemblyChanged?.Invoke(asm);
            }
            
            for (var i = 0; i < deletedAssets.Length; i++)
            {
                var path = deletedAssets[i];
                if (path.EndsWith(".asset") && AssetDatabase.LoadAssetAtPath<ScriptableObject>(path) is CppAssembly asm)
                    OnCppAssemblyChanged?.Invoke(asm);
            }
            
            for (var i = 0; i < movedFromAssetPaths.Length; i++)
            {
                var path = movedFromAssetPaths[i];
                if (path.EndsWith(".asset") && AssetDatabase.LoadAssetAtPath<ScriptableObject>(path) is CppAssembly asm)
                    OnCppAssemblyChanged?.Invoke(asm);
            }
            
            for (var i = 0; i < importedAssets.Length; i++)
            {
                var path = importedAssets[i];
                if (path.EndsWith(".cpp"))
                    OnCppFileChanged?.Invoke(FileAction.Created, path.Replace("\\", "/"));
                else if (path.EndsWith(".h"))
                    OnHeaderFileChanged?.Invoke(FileAction.Created, path.Replace("\\", "/"));
            }
            
            
            for (var i = 0; i < deletedAssets.Length; i++)
            {
                var path = deletedAssets[i];
                if (path.EndsWith(".cpp"))
                    OnCppFileChanged?.Invoke(FileAction.Deleted, path.Replace("\\", "/"));
                else if (path.EndsWith(".h"))
                    OnHeaderFileChanged?.Invoke(FileAction.Deleted, path.Replace("\\", "/"));
            }
            
            for (var i = 0; i < movedFromAssetPaths.Length; i++)
            {
                var path = movedFromAssetPaths[i];
                if (path.EndsWith(".cpp"))
                    OnCppFileChanged?.Invoke(FileAction.Deleted, path.Replace("\\", "/"));
                else if (path.EndsWith(".h"))
                    OnHeaderFileChanged?.Invoke(FileAction.Deleted, path.Replace("\\", "/"));
            }

            
            for (var i = 0; i < movedAssets.Length; i++)
            {
                var path = movedAssets[i];
                if (path.EndsWith(".cpp"))
                    OnCppFileChanged?.Invoke(FileAction.Moved, path.Replace("\\", "/"));
                else if (path.EndsWith(".h"))
                    OnHeaderFileChanged?.Invoke(FileAction.Moved, path.Replace("\\", "/"));
            }
        }
    }
}
