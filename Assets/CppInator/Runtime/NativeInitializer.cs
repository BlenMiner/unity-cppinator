using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace CppInator.Runtime
{
    delegate void DebugDelegate(string message, string file, int line);

    public class CppException : Exception
    {
        readonly string _fileAndLine;

        readonly System.Diagnostics.StackTrace _trace;

        public CppException(string message, string fileAndLine) : base(message) 
        {
            _fileAndLine = fileAndLine;
#if UNITY_EDITOR
            _trace = new System.Diagnostics.StackTrace(4, true);
#else
            _trace = new System.Diagnostics.StackTrace(3, true);
#endif
        }
        public override string ToString() => $"{Message}\n{_fileAndLine}";

        public override string StackTrace => _trace.ToString();
    }

    public static class NativeExceptions
    {

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        static extern void SetExceptionHandler(int type, DebugDelegate cb);

        [MonoPInvokeCallback(typeof(DebugDelegate))]
        static void ThrowCallback(string message, string file, int line)
        {
            throw new CppException(message, file.ToLinkPath(line));
        }

        internal static void Init()
        {
            Native.Invoke(SetExceptionHandler, 0, (DebugDelegate)ThrowCallback);
        }
    }

    public static class StringUtilsForPathLink
    {
        public static string ToLinkPath(this string file, int line)
        {
            var assetsIdx = file.LastIndexOf("Assets", StringComparison.Ordinal);
            if (assetsIdx != -1)
                file = file[assetsIdx..];

#if UNITY_EDITOR
            return $"<a href=\"{file}\" line=\"{file}\">{file}:{line}</a>";
#else
            return $"{file}:{line}";
#endif
        }
    }

    [DefaultExecutionOrder(int.MaxValue)]
    internal class NativeInitializer : MonoBehaviour
    {
        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        static extern void SetDebugLog(int type, DebugDelegate cb);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        static extern void InternalUpdateUnityFunction();

        [MonoPInvokeCallback(typeof(DebugDelegate))]
        static void LogMessage(string message, string file, int line)
        {
            Debug.Log($"{message}\n{file.ToLinkPath(line)}");
        }

        [MonoPInvokeCallback(typeof(DebugDelegate))]
        static void LogError(string message, string file, int line)
        {
            Debug.LogError($"{message}\n{file.ToLinkPath(line)}");
        }

        [MonoPInvokeCallback(typeof(DebugDelegate))]
        static void LogWarning(string message, string file, int line)
        {
            Debug.LogWarning($"{message}\n{file.ToLinkPath(line)}");
        }

        void LateUpdate() => Native.Invoke(InternalUpdateUnityFunction);

        private void OnDestroy()
        {
            MarshalExtensions.FreeAllAllocatedPtrs();
        }

        public static void Initialize(bool editorMode)
        {
            NativeDllLoader.ClearCache();

            Native.Invoke(SetDebugLog, 0, (DebugDelegate)LogMessage);
            Native.Invoke(SetDebugLog, 1, (DebugDelegate)LogWarning);
            Native.Invoke(SetDebugLog, 2, (DebugDelegate)LogError);

            NativeExceptions.Init();

            if (!editorMode)
            {
                _ = new GameObject("CPinator", typeof(NativeInitializer))
                {
                    hideFlags = HideFlags.HideInHierarchy
                };
            }
        }
    }
}
