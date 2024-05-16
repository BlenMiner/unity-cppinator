#include "unityengine.h"
#include <stdint.h>

EXPORT(int64_t) CppCalculateFibRec(int64_t n)
{
    if (n < 2)
        return n;
    else return CppCalculateFibRec(n - 1) + CppCalculateFibRec(n - 2);
}

EXPORT(int64_t) CppCalculateFibIter(int64_t n)
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