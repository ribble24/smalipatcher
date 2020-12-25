using System;
using System.IO;
using System.IO.Compression;
using SmaliLib.Steps;

namespace SmaliLib.Patches
{
    public class MockProviders : IPatch
    {
        public override string Title { get; } = "Mock providers";
        public override string Description { get; } = "Allow creation of mock providers without mock permissions.";
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
                string str2 = File.ReadAllText(Path.Combine(baseStr, path));
                using (StreamWriter streamWriter =
                    new StreamWriter(Path.Combine(baseStr, path + ".new")))
                {
                    if (str2.Contains(".method private canCallerAccessMockLocation"))
                    {
                        int num = str2.LastIndexOf(".method private canCallerAccessMockLocation(");
                        while (str2.Substring(num, 7) != ".locals" && str2.Substring(num, 10) != ".registers")
                            ++num;
                        if (str2.Substring(num, 7) == ".locals")
                            num += 8;
                        if (str2.Substring(num, 10) == ".registers")
                            num += 11;
                        while (int.TryParse(str2.Substring(num, 1), out _))
                            ++num;
                        int startIndex = num;
                        while (str2.Substring(startIndex, 11) != ".end method")
                            ++startIndex;
                        str2 = str2.Substring(0, num) + "\n\n    const/4 v0, 0x1\n\n    return v0\n\n" +
                               str2.Substring(startIndex);
                        platform.Log("Patched mock providers function");
                    }
                    streamWriter.Write(str2);
                }
                File.Replace(Path.Combine(baseStr, path + ".new"),
                    Path.Combine(baseStr, path), null);
            }
            else
                platform.Warning("Mock providers class not found");
            return baseStr;
        }

        public override void PackModuleStep(ZipArchive archive)
        {
        }
    }
}