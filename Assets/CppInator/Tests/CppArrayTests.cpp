#include "unityengine.h"
#include <vector>

EXPORT(IntPtr) createArray(int length)
{
    std::vector<int> *a = new std::vector<int>(length);
    a->reserve(length);
    return (IntPtr)a;
}

EXPORT(void) fillWithNumbers(IntPtr array)
{
    try
    {
        std::vector<int> *a = (std::vector<int> *)array;

        int len = a->size();

        for (int i = 0; i < len; ++i)
            a->at(i) = i;
    }
    SEND_EXCEPTION_TO_UNITY;

    return;
}

EXPORT(int) readAt(IntPtr array, int idx)
{
    try
    {
        std::vector<int> *a = (std::vector<int> *)array;
        return a->at(idx);
    }
    catch (std::exception &e)
    {
        CS_EXCEPTION(e.what());
    }

    return -1;
}

EXPORT(void) freeArray(IntPtr array)
{
    std::vector<int> *a = (std::vector<int> *)array;
    delete a;
    return;
} 
