using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using UnityEditor;

namespace CppInator.Editor
{
    public class DisableCppCodeInBuild : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    { 
        [InitializeOnLoadMethod]
        public static void CheckCleanup()
        {
            if (SessionState.GetBool("CppInator.Cleanup", true))
            {
                DoCleanup();
                SessionState.SetBool("CppInator.Cleanup", false);
            }
        }

        private static void OnUpdate()
        {
            if (BuildPipeline.isBuildingPlayer)
                return;

            CheckCleanup();
        }

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            SessionState.SetBool("CppInator.Cleanup", true);
            
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;

            foreach (var asm in CppAssembliesTracker.Assemblies)
            {
                if (!asm.includeInBuild)
                {
                    var path = CppAssembliesTracker.GetAsmPath(asm);
                    
                    var cppFiles = Directory.GetFiles(path, "*.cpp", SearchOption.AllDirectories);
                    var hFiles = Directory.GetFiles(path, "*.h", SearchOption.AllDirectories);

                    foreach (var file in cppFiles)
                    {
                        File.Move(file, file + ".disabled");

                        if (File.Exists(file + ".meta"))
                            File.Move(file + ".meta", file + ".meta.disabled");
                    }
                    
                    foreach (var file in hFiles)
                    {
                        File.Move(file, file + ".disabled");

                        if (File.Exists(file + ".meta"))
                            File.Move(file + ".meta", file + ".meta.disabled");
                    }
                }
            }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            DoCleanup();
        }

        static void DoCleanup()
        {
            var allDisabled = Directory.GetFiles("./Assets", "*.disabled", SearchOption.AllDirectories);
            var allDisabledLibrary = Directory.GetFiles("./Library/PackageCache", "*.disabled", SearchOption.AllDirectories);

            if (allDisabled.Length > 0)
            {
                foreach (var file in allDisabled)
                {
                    var newPath = file[..^9];
                    File.Move(file, newPath);
                }
                
                foreach (var file in allDisabledLibrary)
                {
                    var newPath = file[..^9];
                    File.Move(file, newPath);
                }
            }
            
            AssetDatabase.Refresh();
        }
    }
}
