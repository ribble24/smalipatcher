using System.IO;
using System.IO.Compression;
using SmaliLib.Steps;

namespace SmaliLib.Patches
{
    public class MockLocations : IPatch
    {
        public override string Title { get; } = "Mock locations";
        public override string Description { get; } = "Treat mock locations as genuine location updates";
        public override string TargetFile { get; } = "services.jar";
        public override bool IsDefault { get; } = true;

        public override void JarCompileStep(IPlatform platform)
        {
        }

        public override string PatchFileStep(IPlatform platform, string baseStr)
        {
            string path = Path.Combine("com", "android", "server", "LocationManagerService.smali");
            baseStr = FrameworkPatcher.GetPath(path);
            if (File.Exists(Path.Combine(baseStr, path)))
            {
                using (StreamReader streamReader =
                    File.OpenText(Path.Combine(baseStr, path)))
                using (StreamWriter streamWriter =
                    new StreamWriter(Path.Combine(baseStr, path + ".new")))
                {
                    string str2 = "";
                    string str3;
                    while ((str3 = streamReader.ReadLine()) != null)
                    {
                        str2 = str2 + str3 + "\n";
                        if (str3.Contains("setIsFromMockProvider"))
                        {
                            int num = str2.LastIndexOf("setIsFromMockProvider");
                            while (str2.Substring(num - 1, 2) != "0x")
                                --num;
                            str2.Substring(num - 1, 3);
                            platform.Log("Patched mock location boolean");
                            str2 = str2.Substring(0, num - 1) + "0x0\n" + str2.Substring(num + 3);
                        }
                    }
                    streamWriter.Write(str2);
                }
                File.Replace(Path.Combine(baseStr, path + ".new"),
                    Path.Combine(baseStr, path), null);
            }
            path = Path.Combine("com", "android", "server", "location", "MockProvider.smali");
            baseStr = FrameworkPatcher.GetPath(path);
            if (File.Exists(Path.Combine(baseStr, path)))
            {
                using (StreamReader streamReader =
                    File.OpenText(Path.Combine(baseStr, path)))
                using (StreamWriter streamWriter =
                    new StreamWriter(Path.Combine(baseStr, path + ".new")))
                {
                    string str2 = "";
                    string str3;
                    while ((str3 = streamReader.ReadLine()) != null)
                    {
                        str2 = str2 + str3 + "\n";
                        if (str3.Contains("setIsFromMockProvider"))
                        {
                            int num = str2.LastIndexOf("setIsFromMockProvider");
                            while (str2.Substring(num - 1, 2) != "0x")
                                --num;
                            str2.Substring(num - 1, 3);
                            platform.Log("Patched mock location boolean");
                            str2 = str2.Substring(0, num - 1) + "0x0\n" + str2.Substring(num + 3);
                        }
                    }
                    streamWriter.Write(str2);
                }
                File.Replace(Path.Combine(baseStr, path + ".new"),
                    Path.Combine(baseStr, path), null);
            }
            return baseStr;
        }

        public override void PackModuleStep(ZipArchive archive)
        {
        }
    }
}