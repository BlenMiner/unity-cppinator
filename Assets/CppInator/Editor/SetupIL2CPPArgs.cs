using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor;

namespace CppInator.Editor
{
    public class SetupIL2CPPArgs : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0; 

        public void OnPreprocessBuild(BuildReport report)
        {
            var cmd = CCompiler.GetHeadersIncludeCmd(); 
            PlayerSettings.SetAdditionalIl2CppArgs($"--compiler-flags=\"{cmd}\"");
        }
    }
}
