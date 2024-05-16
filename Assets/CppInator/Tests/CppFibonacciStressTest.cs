using System.Runtime.InteropServices;
using CppInator.Runtime;
using NUnit.Framework;

namespace CppInator.Tests
{
    public class CppFibonacciStressTest
    {
        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        static extern long CppCalculateFibRec(long n);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        static extern long CppCalculateFibIter(long n);

        const long REPEAT = 100;
        const long N = 35;
        const long FIB_OF_N = 9227465;

        static long CalculateRec(long n)
        {
            if (n < 2)
                 return n;
            return CalculateRec(n - 1) + CalculateRec(n - 2);
        }

        static long CalculateIter(long n)
        {
            long a = 0;
            long b = 1;
            for (int i = 0; i < n; i++)
            {
                long temp = a;
                a = b;
                b = temp + b;
            }
            return a;
        }

        [Test]
        public void FibonacciRecurssiveCSharp()
        {
            for (int i = 0; i < REPEAT; i++)
                Assert.AreEqual(CalculateRec(N), FIB_OF_N);
        }

        [Test]
        public void FibonacciRecurssiveCPlusPlus()
        {
            using var _ = new NativeDllInstance();

            for (int i = 0; i < REPEAT; i++) 
                Assert.AreEqual(Native.Invoke(CppCalculateFibRec, N), FIB_OF_N);
        }

        [Test]
        public void FibonacciIterativeCSharp()
        {
            for (int i = 0; i < REPEAT; i++)
                Assert.AreEqual(CalculateIter(N), FIB_OF_N);
        }

        [Test]
        public void FibonacciIterativeCPlusPlus()
        {
            using var _ = new NativeDllInstance();

            for (int i = 0; i < REPEAT; i++)
                Assert.AreEqual(Native.Invoke(CppCalculateFibIter, N), FIB_OF_N);
        }
    }
}
