using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace CppInator.Editor
{
    public class CreateCppFile : MonoBehaviour
    {
        static string GetRightClickPath()
        {
            foreach (var g in Selection.assetGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                if (Directory.Exists(path))
                    return path;
            }

            return null;
        }
        
        [MenuItem("Assets/Create/C++ Source", priority = 30)]
        public static void CreateSource()
        {
            var clickPath = GetRightClickPath();
            
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0, 
                ScriptableObject.CreateInstance<DoCreateCpp>(),
                clickPath + "/cppScript.cpp",
                null,
                null
            );
        }
        
        [MenuItem("Assets/Create/C++ Header", priority = 31)]
        public static void CreateHeader()
        {
            var clickPath = GetRightClickPath();
            
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0, 
                ScriptableObject.CreateInstance<DoCreateH>(),
                clickPath + "/cppScript.h",
                null,
                null
            );
        }
    }
    
    public class DoCreateCpp : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            string directory = Path.GetDirectoryName(pathName);
            string packageName = Path.GetFileName(pathName);

            pathName = Path.Combine(directory ?? string.Empty, packageName);
            
            File.WriteAllText($"{pathName}", "#include \"Headers/unityengine.h\"\n");

            AssetDatabase.Refresh();
            
            var assets = AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));

            ProjectWindowUtil.ShowCreatedAsset(assets);

            AssetDatabase.SaveAssets();
        }
    }
    
    public class DoCreateH : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            string directory = Path.GetDirectoryName(pathName);
            string packageName = Path.GetFileName(pathName);

            pathName = Path.Combine(directory ?? string.Empty, packageName);
            
            File.WriteAllText($"{pathName}", "#pragma once\n");

            AssetDatabase.Refresh();
            
            var assets = AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));

            ProjectWindowUtil.ShowCreatedAsset(assets);
        }
    }
}
