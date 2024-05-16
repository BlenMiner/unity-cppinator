#include <unityengine.h>

DebugLogDelegate g_logMessage = nullptr;
DebugLogDelegate g_logWarning = nullptr;
DebugLogDelegate g_logError = nullptr;
DebugLogDelegate g_exception = nullptr;

EXPORT(void) SetDebugLog(int type, DebugLogDelegate delegate)
{
    switch (type)
    {
    case 0: g_logMessage = delegate; break;
    case 1: g_logWarning = delegate; break;
    case 2: g_logError = delegate; break;
    default:
        break;
    }
}

EXPORT(void) SetExceptionHandler(int type, DebugLogDelegate delegate)
{
    g_exception = delegate;
}

void _Throw(std::string message, const char* file, int line)
{
    _Throw(message.c_str(), file, line);
} 

void _Throw(const char* message, const char* file, int line)
{
    if (g_exception != nullptr)
        g_exception(message, file, line);
}

void _LogInfo(const char* message, const char* file, int line)
{
    if (g_logMessage != nullptr) {
        g_logMessage(message, file, line);
    }
}

void _LogWarning(const char* message, const char* file, int line)
{
    if (g_logWarning != nullptr) {
        g_logWarning(message, file, line);
    }
}

void _LogError(const char* message, const char *file, int line)
{
    if (g_logError != nullptr) {
        g_logError(message, file, line);
    }
}

void _LogInfo(std::string message, const char* file, int line)
{
    _LogInfo(message.c_str(), file, line);
}

void _LogWarning(std::string message, const char* file, int line)
{
    _LogWarning(message.c_str(), file, line);
}

void _LogError(std::string message, const char* file, int line)
{
    _LogError(message.c_str(), file, line);
}