using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CppInator.Editor
{
    public static class CmdRunner
    {
        public static int RunCommand(string command)
        {
            return RunCommand(command, out _);
        }

        public static int RunCommand(string command, out string error)
        {
            // Split the command into the name and the arguments
            var splitIndex = command.IndexOf(' ');
            string fileName = splitIndex < 0 ? command : command.Substring(0, splitIndex);
            string arguments = splitIndex < 0 ? "" : command.Substring(splitIndex + 1);

            // On Unix-like systems, use /bin/bash; on Windows, use cmd.exe
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                fileName = "/bin/bash";
                arguments = $"-c \"{command}\"";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                fileName = "cmd.exe";
                arguments = $"/c {command}";
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using (var process = Process.Start(processStartInfo))
            {
                if (process != null)
                {
                    error = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    return process.ExitCode;
                }
            }

            error = "Failed to start process";
            return -1;
        }
    }
}