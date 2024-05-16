using System;
using UnityEngine;

namespace CppInator.Runtime
{
    [Flags]
    public enum EditorOS
    {
        All,
        OSX = 1,
        Windows = 2,
        Linux = 4
    }

    [CreateAssetMenu(fileName = "CppAssembly", menuName = "CppAssembly Definition", order = 32)]
    public class CppAssembly : ScriptableObject
    {
        public static event Action<CppAssembly> OnAssemblyCreated;
        public static event Action<CppAssembly> OnAssemblyModified;
        
        [SerializeField] bool _includeHeaders = true;
        [SerializeField] bool _includeSources = true;

        [Header("Build Settings")]
        [SerializeField] bool _includeInBuild = true;

        [Header("Editor OS")]
        [SerializeField] bool _includeWindowsEditor = true;
        [SerializeField] bool _includeLinuxEditor = true;
        [SerializeField] bool _includeOsxEditor = true;

        public bool includeInBuild => _includeInBuild;

        public bool ShouldIncludeInEditor()
        {
            return Application.platform switch
            {
                RuntimePlatform.OSXEditor => _includeOsxEditor,
                RuntimePlatform.WindowsEditor => _includeWindowsEditor,
                RuntimePlatform.LinuxEditor => _includeLinuxEditor,
                _ => false,
            };
        }
        
        public bool includeHeaders => _includeHeaders;
        
        public bool includeSources => _includeSources;
        
        bool _created;

#if UNITY_EDITOR
        private void Reset()
        {
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif
        
        private void OnValidate()
        {
            if (!_created)
            {
                OnAssemblyCreated?.Invoke(this);
                _created = true;
            }
            else
            {
                OnAssemblyModified?.Invoke(this);
            }
        }
    }
}
