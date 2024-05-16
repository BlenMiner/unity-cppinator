#if !UNITY_MONO_CECIL
using UnityEditor;
using UnityEditor.PackageManager;

namespace Unity.CppInator.Codegen
{
    public static class MonoCecilInstaller
    {
        private static bool _failed;
        
        [InitializeOnLoadMethod]
        static void Install()
        {
            Client.Add("com.unity.nuget.mono-cecil");
        }
    }
}
#endif