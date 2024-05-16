using System;

namespace CppInator.Runtime
{
    public static class Native
    {
        const string DLL_NAME = "cpinator-compiled";
        
        public const string DLL_PATH = "./obj/CPinator/" + DLL_NAME + ".dll";

        public static unsafe void Invoke(Action action)
        {
            ((delegate* unmanaged[Cdecl]<void>)NativeDllLoader.CachedPtr(action))();
        }

        public static unsafe void Invoke<T1>(Action<T1> action, T1 arg)
        {
            ((delegate* unmanaged[Cdecl]<T1, void>)NativeDllLoader.CachedPtr(action))(arg);
        }

        public static unsafe void Invoke<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            ((delegate* unmanaged[Cdecl]<T1, T2, void>)NativeDllLoader.CachedPtr(action))(arg1, arg2);
        }

        public static unsafe void Invoke<T1, T2, T3>(Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
        {
            ((delegate* unmanaged[Cdecl]<T1, T2, T3, void>)NativeDllLoader.CachedPtr(action))(arg1, arg2, arg3);
        }

        public static unsafe void Invoke<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            ((delegate* unmanaged[Cdecl]<T1, T2, T3, T4, void>)NativeDllLoader.CachedPtr(action))(arg1, arg2, arg3, arg4);
        }

        public static unsafe void Invoke<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            ((delegate* unmanaged[Cdecl]<T1, T2, T3, T4, T5, void>)NativeDllLoader.CachedPtr(action))(arg1, arg2, arg3, arg4, arg5);
        }

        public static unsafe void Invoke<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            ((delegate* unmanaged[Cdecl]<T1, T2, T3, T4, T5, T6, void>)NativeDllLoader.CachedPtr(action))(arg1, arg2, arg3, arg4, arg5, arg6);
        }

        public static unsafe void Invoke<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            ((delegate* unmanaged[Cdecl]<T1, T2, T3, T4, T5, T6, T7, void>)NativeDllLoader.CachedPtr(action))(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        public static unsafe void Invoke<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            ((delegate* unmanaged[Cdecl]<T1, T2, T3, T4, T5, T6, T7, T8, void>)NativeDllLoader.CachedPtr(action))(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        public static unsafe void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            ((delegate* unmanaged[Cdecl]<T1, T2, T3, T4, T5, T6, T7, T8, T9, void>)NativeDllLoader.CachedPtr(action))(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }

        public static unsafe void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            ((delegate* unmanaged[Cdecl]<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, void>)NativeDllLoader.CachedPtr(action))(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }

        public static unsafe R Invoke<R>(Func<R> action)
        {
            return ((delegate* unmanaged[Cdecl]<R>)NativeDllLoader.CachedPtr(action))();
        }

        public static unsafe R Invoke<T1, R>(Func<T1, R> action, T1 arg)
        {
            return ((delegate* unmanaged[Cdecl]<T1, R>)NativeDllLoader.CachedPtr(action))(arg);
        }

        public static unsafe R Invoke<T1, T2, R>(Func<T1, T2, R> action, T1 arg1, T2 arg2)
        {
            return ((delegate* unmanaged[Cdecl]<T1, T2, R>)NativeDllLoader.CachedPtr(action))(arg1, arg2);
        }

        public static unsafe R Invoke<T1, T2, T3, R>(Func<T1, T2, T3, R> action, T1 arg1, T2 arg2, T3 arg3)
        {
            return ((delegate* unmanaged[Cdecl]<T1, T2, T3, R>)NativeDllLoader.CachedPtr(action))(arg1, arg2, arg3);
        }

        public static unsafe R Invoke<T1, T2, T3, T4, R>(Func<T1, T2, T3, T4, R> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return ((delegate* unmanaged[Cdecl]<T1, T2, T3, T4, R>)NativeDllLoader.CachedPtr(action))(arg1, arg2, arg3, arg4);
        }

        public static unsafe R Invoke<T1, T2, T3, T4, T5, R>(Func<T1, T2, T3, T4, T5, R> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            return ((delegate* unmanaged[Cdecl]<T1, T2, T3, T4, T5, R>)NativeDllLoader.CachedPtr(action))(arg1, arg2, arg3, arg4, arg5);
        }

        public static unsafe R Invoke<T1, T2, T3, T4, T5, T6, R>(Func<T1, T2, T3, T4, T5, T6, R> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            return ((delegate* unmanaged[Cdecl]<T1, T2, T3, T4, T5, T6, R>)NativeDllLoader.CachedPtr(action))(arg1, arg2, arg3, arg4, arg5, arg6);
        }

        public static unsafe R Invoke<T1, T2, T3, T4, T5, T6, T7, R>(Func<T1, T2, T3, T4, T5, T6, T7, R> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            return ((delegate* unmanaged[Cdecl]<T1, T2, T3, T4, T5, T6, T7, R>)NativeDllLoader.CachedPtr(action))(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        public static unsafe R Invoke<T1, T2, T3, T4, T5, T6, T7, T8, R>(Func<T1, T2, T3, T4, T5, T6, T7, T8, R> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            return ((delegate* unmanaged[Cdecl]<T1, T2, T3, T4, T5, T6, T7, T8, R>)NativeDllLoader.CachedPtr(action))(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        public static unsafe R Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, R> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            return ((delegate* unmanaged[Cdecl]<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>)NativeDllLoader.CachedPtr(action))(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }

        public static unsafe R Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            return ((delegate* unmanaged[Cdecl]<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R>)NativeDllLoader.CachedPtr(action))(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }

        /// <summary>
        /// Fetches a function pointer from the native library
        /// </summary>
        /// <param name="functionName">The name of the function to fetch</param>
        /// <returns>The function pointer</returns>
        public static IntPtr CachedPtr(string functionName)
        {
            return NativeDllLoader.CachedPtr(functionName);
        }
    }
}
