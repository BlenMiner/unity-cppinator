using System;
using System.Runtime.InteropServices;
using CppInator.Runtime;
using NUnit.Framework;

namespace CppInator.Tests
{
    public class CppArrayTests
    {
        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        public static extern void fillWithNumbers(IntPtr array);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr createArray(int len);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        public static extern void freeArray(IntPtr array);

        
        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        public static extern int readAt(IntPtr array, int idx);

        [Test]
        public void CppArraySimplePasses()
        {
            using var _ = new NativeDllInstance();

            var array = Native.Invoke(createArray, 10);

            Native.Invoke(fillWithNumbers, array);

            for (int i = 0; i < 10; i++)
                Assert.AreEqual(i, Native.Invoke(readAt, array, i));

            Native.Invoke(freeArray, array);
        }

        // [PerformanceUnityTest]
    }
}
