using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace CppInator.Editor
{
    public class EnableIL2CPPThreads : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.WebGL) return;
            
            PlayerSettings.WebGL.threadsSupport = true;
        }
    }
}
