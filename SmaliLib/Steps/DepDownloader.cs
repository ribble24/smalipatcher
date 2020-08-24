using System.IO;
using System.IO.Compression;
using System.Net;

namespace SmaliLib.Steps
{
    internal static class DepDownloader
    {
        private static string _apktoolUrl;
        private static string _smaliUrl;
        private static string _baksmaliUrl;

        public static void Download(IPlatform platform)
        {
            if (Directory.Exists("bin"))
                Directory.Delete("bin", true);
            Directory.CreateDirectory("bin");
            if (_apktoolUrl == null || _smaliUrl == null || _baksmaliUrl == null)
                GetUrls(platform);

            File.WriteAllBytes(Path.Combine("bin", "apktool.jar"), platform.Download(_apktoolUrl, "apktool"));
            File.WriteAllBytes(Path.Combine("bin", "baksmali.jar"), platform.Download(_apktoolUrl, "baksmali"));
            File.WriteAllBytes(Path.Combine("bin", "smali.jar"), platform.Download(_apktoolUrl, "smali"));

            Unpack(platform.Download("https://github.com/fOmey/dexpatcher/raw/master/dexpatcher.zip", "dexpatcher"));
            File.WriteAllBytes(Path.Combine("bin", "dexpatcher.jar"),
                platform.Download(
                    "https://github.com/DexPatcher/dexpatcher-tool/releases/download/v1.7.0/dexpatcher-1.7.0.jar",
                    "dex bin"));
            if (PlatformCheck.IsWindows)
            {
                byte[] platformTools =
                    platform.Download("http://dl.google.com/android/repository/platform-tools-latest-windows.zip",
                        "adb");
                using MemoryStream stream = new MemoryStream(platformTools);
                using ZipArchive file = new ZipArchive(stream);
                foreach (ZipArchiveEntry entry in file.Entries)
                    if (entry.FullName.ToLower().Replace("\\", "/").StartsWith("platform-tools/adb"))
                        entry.ExtractToFile(Path.Combine("bin", entry.Name), true);
            }
            else
                platform.Log("Skipping ADB download as this is not windows");
            if (PlatformCheck.IsWindows)
                Unpack(platform.Download(
                    "https://github.com/fOmey/vdexExtractor/raw/master/bin/vdexExtractor_x86_64.zip", "vdex"));
            else
                platform.Log(
                    "Skipping vdexExtractor download as this is not windows, you may need to build it yourself: https://github.com/anestisb/vdexExtractor");
            Unpack(platform.Download(
                "https://github.com/fOmey/compact_dex_converter/raw/master/compact_dex_converter_android_arm64-v8a.zip",
                "cdex"));
            File.WriteAllBytes(Path.Combine("bin", "module_installer.sh"),
                platform.Download(
                    "https://raw.githubusercontent.com/topjohnwu/Magisk/master/scripts/module_installer.sh",
                    "moduleInstaller"));
        }

        private static void Unpack(byte[] zip)
        {
            using MemoryStream stream = new MemoryStream(zip);
            using ZipArchive file = new ZipArchive(stream);
            foreach (ZipArchiveEntry entry in file.Entries) entry.ExtractToFile(Path.Combine("bin", entry.Name), true);
            //.NET Standard 2.1 also supports
            //file.ExtractToDirectory("bin", true);
            //But .NET Framework doesn't support that version and is needed for MaterialSkin
        }

        private static void GetUrls(IPlatform platform)
        {
            platform.Log("Fetching releases");
            using (WebClient webClient = new WebClient())
            {
                string str = webClient.DownloadString("https://bitbucket.org/iBotPeaches/apktool/downloads/");
                int startIndex1 = str.IndexOf("/iBotPeaches/apktool/downloads/apktool_");
                int startIndex2 = startIndex1;
                while (str.Substring(startIndex2, 3) != "jar")
                    ++startIndex2;
                _apktoolUrl = $"https://bitbucket.org{str.Substring(startIndex1, startIndex2 + 3 - startIndex1)}";
            }
            using (WebClient webClient = new WebClient())
            {
                string str = webClient.DownloadString("https://bitbucket.org/JesusFreke/smali/downloads/");
                int startIndex1 = str.IndexOf("/JesusFreke/smali/downloads/smali-");
                int startIndex2 = startIndex1;
                while (str.Substring(startIndex2, 3) != "jar")
                    ++startIndex2;
                _smaliUrl = $"https://bitbucket.org{str.Substring(startIndex1, startIndex2 + 3 - startIndex1)}";
                int startIndex3 = str.IndexOf("/JesusFreke/smali/downloads/baksmali-");
                int startIndex4 = startIndex3;
                while (str.Substring(startIndex4, 3) != "jar")
                    ++startIndex4;
                _baksmaliUrl = $"https://bitbucket.org{str.Substring(startIndex3, startIndex4 + 3 - startIndex3)}";
            }
        }
    }
}