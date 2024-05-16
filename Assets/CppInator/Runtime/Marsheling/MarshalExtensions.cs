using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Debug = UnityEngine.Debug;

namespace CppInator.Runtime
{
    public class UnfreedMemoryException : Exception
    {
        readonly StackTrace _trace;

        public UnfreedMemoryException(string message, StackTrace trace) : base(message)
        {
            _trace = trace;
        }
        
        public override string StackTrace => _trace.ToString();
    }
    
    public readonly struct DisposablePtr : IDisposable
    {
        public readonly IntPtr value;

        public DisposablePtr(IntPtr value)
        {
            this.value = value;
        }

        public void Dispose()
        {
            MarshalExtensions.allocatedPtrs.Remove(value);
            Marshal.FreeHGlobal(value);
        }
        
        public static implicit operator IntPtr(DisposablePtr ptr) => ptr.value;
    }
    
    public static class MarshalExtensions
    {
        internal static readonly Dictionary<IntPtr, StackTrace> allocatedPtrs = new ();
        
        internal static void FreeAllAllocatedPtrs()
        {
            if (allocatedPtrs.Count == 0)
                return;

            foreach (var (ptr, stack) in allocatedPtrs)
            {
                Marshal.FreeHGlobal(ptr);
                Debug.LogException(new UnfreedMemoryException($"Unfreed pointer {ptr}", stack));
            }
            
            allocatedPtrs.Clear();
        }

        public static DisposablePtr ArrayToPtr<T>(T[] source, bool skipFreeChecks = false) where T : unmanaged
        {
            IntPtr ptr;
            
            unsafe
            {
                var totalSize = source.Length * sizeof(T);
                ptr = Marshal.AllocHGlobal(totalSize);
                
                fixed (T* sourcePtr = source)
                {
                    Buffer.MemoryCopy(
                        sourcePtr, 
                        (void*)ptr, 
                        totalSize, 
                        totalSize
                    );
                }
            }

            if (!skipFreeChecks)
                allocatedPtrs.Add(ptr, new StackTrace(1, true));
            
            return new DisposablePtr(ptr);
        }
        
        public static T[] PtrToArray<T>(IntPtr source, int length) where T : unmanaged
        {
            var arr = new T[length];
            
            unsafe
            {
                var totalSize = length * sizeof(T);

                fixed (T* destPtr = arr)
                {
                    Buffer.MemoryCopy(
                        (void*)source, 
                        destPtr, 
                        totalSize, 
                        totalSize
                    );
                }
            }
            
            return arr;
        }
    }
}
