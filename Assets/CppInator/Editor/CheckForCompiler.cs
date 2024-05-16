using System;
using System.IO;
using System.Net;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace CppInator.Editor
{
    public static class CheckForCompiler
    {
        const string DOWNLOAD_URL = "https://github.com/BlenMiner/mingw64/releases/download/v8.1.0/mingw-64-full.zip";

        const string INSTALL_PATH = "obj";
        
        const string INSTALL_PATH_CLANG = INSTALL_PATH + "/mingw64-win";

        const string INSTALL_BIN_PATH = INSTALL_PATH_CLANG + "/bin";

        const string COMPILER_PATH = INSTALL_BIN_PATH + "/g++.exe";
        
        static bool _isInstalled;

        static bool _isNativeInstalled;

        public static string GetCompilerPath()
        {
            return _isNativeInstalled ? "g++" : Path.GetFullPath(COMPILER_PATH);
        }

        public static bool Installed()
        {
            if (_isInstalled) return true;
            
            if (CmdRunner.RunCommand("g++ --version") == 0)
            {
                _isInstalled = true;
                _isNativeInstalled = true;
                return true;
            }

            _isInstalled = Directory.Exists(INSTALL_PATH_CLANG);
            return _isInstalled;
        }

        private static bool _isInstalling;
        
        [InitializeOnLoadMethod]
        public static void Check()
        {
            if (Installed() || _isInstalling) 
                return;

#if UNITY_EDITOR_WIN
            EditorUtility.DisplayProgressBar("CPinator", "Checking for C++ compiler", 0f);
            _isInstalling = true;

            try
            {
                EditorApplication.LockReloadAssemblies();
                EditorUtility.DisplayProgressBar("CPinator", "Downloading g++ compiler", 0f);
                
                var tmpPath = $"{Path.GetTempPath()}mingw-win-binary.zip";

                using var wclient = new WebClient();
                
                wclient.DownloadProgressChanged += (_, e) =>
                {
                    float p = e.BytesReceived / (float)e.TotalBytesToReceive;
                    EditorUtility.DisplayProgressBar("CPinator", $"Downloading g++ compiler {p}", p);
                };
                
                wclient.DownloadFile(DOWNLOAD_URL, tmpPath);

                EditorUtility.DisplayProgressBar("CPinator", "Unzipping g++", 0.5f);

                Directory.CreateDirectory(INSTALL_PATH);
                System.IO.Compression.ZipFile.ExtractToDirectory(tmpPath, INSTALL_PATH_CLANG);
                File.Delete(tmpPath);
                
                if (Installed())
                    CCompiler.SetAllDirty();
                else Debug.LogError("Failed to install mingw64-win");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            _isInstalling = false;
            EditorUtility.ClearProgressBar();
            EditorApplication.UnlockReloadAssemblies();
#elif UNITY_EDITOR_LINUX
            Debug.LogError("CPinator: clang compiler is not installed. Please install it manually:\n" +
                           "<b>sudo apt update\nsudo apt install build-essential</b>");
#elif UNITY_EDITOR_OSX
            Debug.LogError("CPinator: clang compiler is not installed. Please install <a href=\"https://apps.apple.com/us/app/xcode/id497799835?mt=12\">XCode</a>.");
#endif
        }
    }
}