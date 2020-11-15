using System.Diagnostics;
using System.IO;

namespace SmaliLib.Steps
{
    internal static class FrameworkDumper
    {
        public static string Dump(IPlatform platform)
        {
            platform.Log("Dumping framework");
            string[,] strArray =
            {
                {
                    "/system/build.prop",
                    "adb"
                },
                {
                    "/system/framework/",
                    "adb"
                },
                {
                    "/system/system/build.prop",
                    "adb"
                },
                {
                    "/system/system/framework/",
                    "adb"
                }
            };
            for (int index = 0; index < strArray.Length / 2; ++index)
                Pull(platform, strArray[index, 0], strArray[index, 1]);
            return "adb";
        }

        public static void Pull(IPlatform platform, string source, string destination)
        {
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);
            platform.Log($"Pulling {source}");
#if ANDROID_NATIVE
            Process.Start("cp", $"-R \"{source}\" \"{destination}\"")?.WaitForExit();
#else
            BinW.LogIncremental(platform, BinW.RunCommand(Bin.adb, $"pull \"{source}\" \"{destination}\""));
#endif
        }

        public static void Push(IPlatform platform, string source, string destination)
        {
            platform.Log("Pushing files..");
#if ANDROID_NATIVE
            Process.Start("cp", $"-R \"{source}\" \"{destination}\"")?.WaitForExit();
#else
            BinW.LogIncremental(platform, BinW.RunCommand(Bin.adb, $"push \"{source}\" \"{destination}\""));
#endif
        }

        public static void Shell(IPlatform platform, string cmd)
        {
            platform.Log("Executing shell command..");
#if ANDROID_NATIVE
            Process.Start("sh", $"-c \"{cmd}\"")?.WaitForExit();
#else
            BinW.LogIncremental(platform, BinW.RunCommand(Bin.adb, $"shell \"{cmd}\""));
#endif
        }
    }
}