using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CppInator.Runtime
{
    
    public readonly struct CppFunctionSignature
    {
        public readonly int argsCount;
        public readonly long returnSizeInBytes;
        public readonly long totalArgsSizeInBytes;
        public readonly long[] argsSizesInBytes;
    
        static bool TryGetPostfix(string functionName, string postFix, out long size)
        {
            var funcName = $"GENERATED_{functionName}_{postFix}";
            var ptr = Native.CachedPtr(funcName);

            if (ptr == IntPtr.Zero)
            {
                size = 0;
                return false;
            }
        
            var method = Marshal.GetDelegateForFunctionPointer<getSizeOf>(ptr);
            size = method();
        
            return true;
        }

        static void TryGetArgsSize(string functionName, out long size)
        {
            TryGetPostfix(functionName, "argsSize", out size);
        }
    
        static void TryGetReturnSize(string functionName, out long size)
        {
            TryGetPostfix(functionName, "returnSize", out size);
        }
    
        static bool TryGetArgsCount(string functionName, out long size)
        {
            return TryGetPostfix(functionName, "argsCount", out size);
        }
    
        static bool TryGetArgsCountSize(string functionName, int index, out long size)
        {
            return TryGetPostfix(functionName, $"arg{index}Size", out size);
        }
    
        private delegate long getSizeOf();

        public CppFunctionSignature(string functionName)
        {
            if (TryGetArgsCount(functionName, out var res))
                argsCount = (int) res;
            else
            {
                argsCount = -1;
                returnSizeInBytes = -1;
                totalArgsSizeInBytes = -1;
                argsSizesInBytes = Array.Empty<long>();
                return;
            }
            
            TryGetReturnSize(functionName, out returnSizeInBytes);
            TryGetArgsSize(functionName, out totalArgsSizeInBytes);
        
            argsSizesInBytes = new long[argsCount];
        
            for (var i = 0; i < argsCount; i++)
            {
                if (TryGetArgsCountSize(functionName, i, out var argSize))
                    argsSizesInBytes[i] = argSize;
                else argsSizesInBytes[i] = -1;
            }
        }
    }

    public class NativeDllInstance : IDisposable
    {
        IntPtr _ptr;

        public NativeDllInstance()
        {
            if (NativeDllLoader.LibraryPtr != IntPtr.Zero) return;

             var dllPath = Path.GetFullPath(Native.DLL_PATH);
            _ptr = NativeDllLoader.Load(dllPath);
            
            NativeDllLoader.LibraryPtr = _ptr;
            NativeInitializer.Initialize(true);
        }

        public void Dispose()
        {
            if (_ptr == IntPtr.Zero) return;

            NativeDllLoader.LibraryPtr = IntPtr.Zero;
            NativeDllLoader.Free(_ptr);
            _ptr = IntPtr.Zero;
        }
    }

    internal static class NativeDllLoader
    {
        public static IntPtr LibraryPtr;

        static readonly Dictionary<Delegate, IntPtr> _functions = new ();
        static readonly Dictionary<string, IntPtr> _functionsByName = new ();

        static Delegate _lastFetchDel;
        static IntPtr _lastFetchPtr = IntPtr.Zero;
        
        public static IntPtr CachedPtr(string functionName)
        {
            if (_functionsByName.TryGetValue(functionName, out var cachedPtr))
                return cachedPtr;

            var ptr = GetFunc(functionName);
            _functionsByName.Add(functionName, ptr);
            return ptr;
        }
        
        
        static readonly HashSet<int> _validFunctions = new ();

        static void AssertValidSignature(Delegate del, IntPtr nativePtr)
        {
            int hash = del.GetHashCode() ^ nativePtr.GetHashCode();
            
            if (_validFunctions.Contains(hash))
                return;
            
            var method = del.Method;
            var signature = new CppFunctionSignature(method.Name);

            if (signature.argsCount == -1)
            {
                _validFunctions.Add(hash);
                return;
            }

            var parameters = method.GetParameters();
            
            if (method.ReturnType != typeof(void))
            {
                if (signature.returnSizeInBytes == -1)
                    Debug.LogWarning("Return size is unknown, skipping size check");
                else
                {
                    var returnSize = Marshal.SizeOf(method.ReturnType);
                    if (returnSize != signature.returnSizeInBytes)
                        throw new InteropException($"Invalid return size, expected {signature.returnSizeInBytes} bytes, got {returnSize} bytes");
                }
            }
            else if (signature.returnSizeInBytes != 0)
                throw new InteropException($"Invalid return size, expected {signature.returnSizeInBytes} bytes, got void(0) instead.");
            
            if (parameters.Length != signature.argsCount)
                throw new InteropException($"Mismatch in argument count for '{method.Name}'. Expected count: {signature.argsCount}, actual count: {parameters.Length}");

            for (var i = 0; i < parameters.Length; i++)
            {
                if (signature.argsSizesInBytes[i] == -1)
                {
                    Debug.LogWarning($"Size for argument {i} ('{parameters[i].Name}' of type '{parameters[i].ParameterType.Name}') is unknown. Size check will be skipped.");
                    continue;
                }

                var argSize = parameters[i].ParameterType.IsValueType ?
                    Marshal.SizeOf(parameters[i].ParameterType) : 
                    IntPtr.Size;
                
                if (argSize != signature.argsSizesInBytes[i])
                    throw new InteropException($"Mismatch in size for argument {i} ('{parameters[i].ParameterType.Name}' named '{parameters[i].Name}'). Expected size: {signature.argsSizesInBytes[i]} bytes, actual size: {argSize} bytes.");
            }
            
            _validFunctions.Add(hash);
        }
        
        public static IntPtr CachedPtr<T>(T function) where T : Delegate
        {
            if (_lastFetchDel == function)
            {
#if DEBUG
                AssertValidSignature(function, _lastFetchPtr);
#endif
                return _lastFetchPtr;
            }

            if (function == null) 
                throw new ArgumentNullException(nameof(function));

#if !UNITY_EDITOR
            Debug.LogError($"NativeDllLoader.CachedInvoke shouldn't be called in a build\n{function.Method.Name}");
#endif

            if (_functions.TryGetValue(function, out var cachedDel))
            {
                _lastFetchDel = function;
                _lastFetchPtr = cachedDel;
                
#if DEBUG
                AssertValidSignature(function, cachedDel);
#endif
                return cachedDel;
            }

            var methodName = function.Method.Name;
            var functionPtr = GetFunc(function.Method.Name);

            if (functionPtr == IntPtr.Zero)
                throw new EntryPointNotFoundException($"Failed to find function {methodName}");

            _functions.Add(function, functionPtr);
            _lastFetchDel = function;
            _lastFetchPtr = functionPtr;
            
#if DEBUG
            AssertValidSignature(function, functionPtr);
#endif
            return functionPtr;
        }

#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
        [DllImport("libdl")]
        private static extern IntPtr dlopen(string filename, int flags);
        [DllImport("libdl")]
        private static extern int dlclose(IntPtr handle);
#elif UNITY_EDITOR_WIN
        [DllImport("kernel32", SetLastError = true)]
        static extern bool FreeLibrary(IntPtr hModule);
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibrary(string lpFileName);
#endif

#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
        [DllImport("libdl")]
        private static extern IntPtr dlsym(IntPtr handle, string symbol);
#elif UNITY_EDITOR_WIN
        [DllImport("kernel32")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
#endif

        static IntPtr GetFunc(string name)
        {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            return dlsym(LibraryPtr, name);
#elif UNITY_EDITOR_WIN
            return GetProcAddress(LibraryPtr, name);
#else
            return IntPtr.Zero;
#endif
        }

        public static IntPtr Load(string name)
        {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            return dlopen(name, 2);
#elif UNITY_EDITOR_WIN
            return LoadLibrary(name);
#else
            return IntPtr.Zero;
#endif
        }

        public static bool Free(IntPtr ptr)
        {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            return dlclose(ptr) == 0;
#elif UNITY_EDITOR_WIN
            return FreeLibrary(ptr);
#else
            return false;
#endif
        }
 
#if UNITY_EDITOR
        static void ModeChangedArgs(UnityEditor.PlayModeStateChange mode)
        {
            if (mode == UnityEditor.PlayModeStateChange.EnteredEditMode)
            {
                Dispose();
                UnityEditor.EditorApplication.playModeStateChanged -= ModeChangedArgs;
            }
        }
#endif
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            _functions.Clear();
            _functionsByName.Clear();
            _validFunctions.Clear();
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += ModeChangedArgs;

            Dispose();
            
            if (LibraryPtr != IntPtr.Zero) return;

            var dllPath = Path.GetFullPath(Native.DLL_PATH);
            LibraryPtr = Load(dllPath);
            
            if (LibraryPtr == IntPtr.Zero)
                Debug.LogError("Failed to load native library, fix any compilation errors.");
            else NativeInitializer.Initialize(false);
#else
            NativeInitializer.Initialize(false);
#endif
        }
        
        static void Dispose()
        {
#if UNITY_EDITOR
            if (LibraryPtr == IntPtr.Zero) return;
            
            if (!Free(LibraryPtr))
                Debug.LogError("Failed to unload native library");
            LibraryPtr = IntPtr.Zero;
#endif
        }

        internal static void ClearCache()
        {
            _lastFetchDel = null;
            _functions.Clear();
            _functionsByName.Clear();
            _validFunctions.Clear();
        }
    }
}
