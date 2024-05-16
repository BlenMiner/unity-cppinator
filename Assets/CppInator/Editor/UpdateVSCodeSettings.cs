using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CppInator.Editor
{
    [System.Serializable]
    public struct Configuration
    {
        public string name;
        public string[] includePath;
    }
    
    [System.Serializable]
    public struct Configurations
    {
        public Configuration[] configurations;
        public int version;
    }
    
    public static class UpdateVSCodeSettings
    {
        static void UpdateUnityConfig(ref Configurations configs, string[] includePaths)
        {
            int unityConfigIndex = -1;

            if (configs.configurations == null)
            {
                configs.configurations = new Configuration[1];
                unityConfigIndex = 0;
            }
            else
            {
                for (int i = 0; i < configs.configurations.Length; i++)
                {
                    if (configs.configurations[i].name == "Unity3D")
                    {
                        unityConfigIndex = i;
                        break;
                    }
                }
            }

            if (unityConfigIndex == -1)
            {
                var newConfigs = new Configuration[configs.configurations.Length + 1];
                configs.configurations.CopyTo(newConfigs, 0);
                unityConfigIndex = newConfigs.Length - 1;
                newConfigs[unityConfigIndex] = new Configuration();
                configs.configurations = newConfigs;
            }
            
            configs.configurations[unityConfigIndex].name = "Unity3D";
            configs.configurations[unityConfigIndex].includePath = includePaths;
        }
        
        public static void Update(IEnumerable<string> includePaths)
        {
            try
            {
                const string vscodePath = ".vscode";
                const string propertiersPath = vscodePath + "/c_cpp_properties.json";

                var processedPaths = new List<string>();

                foreach (var path in includePaths)
                {
                    if (path.StartsWith("Assets"))
                    {
                        var newPath = path.Replace("Assets", "${workspaceFolder}/Assets");
                        processedPaths.Add(newPath);
                    }
                    else
                    {
                        processedPaths.Add(path);
                    }
                }

                Configurations configurations;

                try
                {
                    configurations = File.Exists(propertiersPath) ? 
                        JsonUtility.FromJson<Configurations>(File.ReadAllText(propertiersPath)) : 
                        new Configurations();
                }
                catch
                {
                    configurations = new Configurations();
                }

                UpdateUnityConfig(ref configurations, processedPaths.ToArray());

                var jsonStr = JsonUtility.ToJson(configurations, true);

                Directory.CreateDirectory(vscodePath);

                File.WriteAllText(propertiersPath, jsonStr);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}
