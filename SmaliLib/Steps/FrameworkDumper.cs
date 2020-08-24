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
            BinW.LogIncremental(platform, BinW.RunCommand(Bin.adb, $"pull \"{source}\" \"{destination}\""));
        }

        public static void Push(IPlatform platform, string filePath, string destination)
        {
            platform.Log("Pushing files..");
            BinW.LogIncremental(platform, BinW.RunCommand(Bin.adb, $"push \"{filePath}\" \"{destination}\""));
        }

        public static void Shell(IPlatform platform, string cmd)
        {
            platform.Log("Executing shell command..");
            BinW.LogIncremental(platform, BinW.RunCommand(Bin.adb, $"shell \"{cmd}\""));
        }
    }
}