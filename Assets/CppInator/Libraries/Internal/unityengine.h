#pragma once

#ifndef UNITYENGINE_H
#define UNITYENGINE_H

#include <string>
#include <functional>

#if defined(__CYGWIN32__) || defined(WIN32) || defined(_WIN32) || defined(__WIN32__) || defined(_WIN64) || defined(WINAPI_FAMILY)
#define EXPORT_ATTRIBUTE  __declspec(dllexport) 
#elif defined(__MACH__) || defined(__ANDROID__) || defined(__linux__) || defined(LUMIN)
#define EXPORT_ATTRIBUTE  __attribute__ ((visibility ("default"))) 
#else
#define EXPORT_ATTRIBUTE 
#endif

using IntPtr = intptr_t;

#define EXPORT(T) extern "C" EXPORT_ATTRIBUTE T __cdecl
#define SEND_EXCEPTION_TO_UNITY catch (std::exception &e) { CS_EXCEPTION(e.what()); } catch (const char* e) { CS_EXCEPTION(e); } catch (char* e) { CS_EXCEPTION(e); } catch (std::string e) { CS_EXCEPTION(e.c_str()); } catch (...) { CS_EXCEPTION("Unknown error"); }

typedef void (*DebugLogDelegate)(const char*, const char*, int);

#define DebugLog(X) _LogInfo(X, __FILE__, __LINE__)
#define DebugWarning(X) _LogWarning(X, __FILE__, __LINE__)
#define DebugError(X) _LogError(X, __FILE__, __LINE__)

#define CS_EXCEPTION(X) _Throw(X, __FILE__, __LINE__)

void _Throw(const char* message, const char* file, int line);
void _Throw(std::string message, const char* file, int line);

void _LogInfo(const char* message, const char* file, int line);
void _LogWarning(const char* message, const char* file, int line);
void _LogError(const char* message, const char* file, int line);

void _LogInfo(std::string message, const char* file, int line);
void _LogWarning(std::string message, const char* file, int line);
void _LogError(std::string message, const char* file, int line);

void QueueToMainThread(std::function<void()> function);

#endif // UNITYENGINE_H
