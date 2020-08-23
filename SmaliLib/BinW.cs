using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace SmaliLib
{
    internal static class BinW
    {
        public static Process RunCommand(Bin bin, string args)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = PlatformCheck.IsWindows && File.Exists(Path.Combine("bin", $"{bin}.exe")) ? Path.Combine("bin", $"{bin}.exe") : bin.ToString(),
                Arguments = args,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
        }

        public static void LogIncremental(IPlatform platform, Process process)
        {
            while (!process.HasExited)
            {
                string str;
                if ((str = process.StandardOutput.ReadLine()) != null ||
                    (str = process.StandardError.ReadLine()) != null)
                    platform.LogIncremental("\n" + str);
            }
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal enum Bin
    {
        adb,
        vdexExtractor,
        java
    }
}