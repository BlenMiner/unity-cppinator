#include "unityengine.h"
#include "concurrentQueue.h"

ThreadSafeQueue<std::function<void()>> g_queueToMainThread;

void QueueToMainThread(std::function<void()> function)
{
    g_queueToMainThread.push(function);
}

// This is called from C# from the main thread (Update function)
EXPORT(void) InternalUpdateUnityFunction()
{
    std::function<void()> task;
    while (g_queueToMainThread.try_pop(task)) {
        task();
    }
}