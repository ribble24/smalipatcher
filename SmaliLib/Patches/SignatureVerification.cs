using System.IO;
using System.IO.Compression;
using SmaliLib.Steps;

namespace SmaliLib.Patches
{
    public class SignatureVerification : IPatch
    {
        public override string Title { get; } = "Signature verification";
        public override string Description { get; } = "Disable apk signature verification";
        public override string TargetFile { get; } = "services.jar";
        public override bool IsDefault { get; } = false;

        public override void JarCompileStep(IPlatform platform)
        {
        }

        public override string PatchFileStep(IPlatform platform, string baseStr)
        {
            string path = Path.Combine("com", "android", "server", "pm", "PackageManagerService.smali");
            baseStr = FrameworkPatcher.GetPath(path);
            if (File.Exists(Path.Combine(baseStr, path)))
            {
                string str2 = File.ReadAllText(Path.Combine(baseStr, path));
                using (StreamWriter streamWriter =
                    new StreamWriter(Path.Combine(baseStr, path + ".new")))
                {
                    if (str2.Contains(".method static compareSignatures("))
                    {
                        int num = str2.LastIndexOf(".method static compareSignatures(");
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
                        str2 = str2.Substring(0, num) + "\n\n    const/4 v0, 0x0\n\n    return v0\n\n" +
                               str2.Substring(startIndex);
                        platform.Log("Patched signature verification function");
                    }
                    if (str2.Contains(".method public checkSignatures("))
                    {
                        int num = str2.LastIndexOf(".method public checkSignatures(");
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
                        str2 = str2.Substring(0, num) + "\n\n    const/4 v0, 0x0\n\n    return v0\n\n" +
                               str2.Substring(startIndex);
                        platform.Log("Patched signature verification function");
                    }
                    streamWriter.Write(str2);
                }
                File.Replace(Path.Combine(baseStr, path + ".new"),
                    Path.Combine(baseStr, path), null);
            }
            path = Path.Combine("com", "android", "server", "pm", "PackageManagerServiceUtils.smali");
            if (File.Exists(Path.Combine(baseStr, path)))
            {
                string str2 = File.ReadAllText(Path.Combine(baseStr, path));
                using (StreamWriter streamWriter =
                    new StreamWriter(Path.Combine(baseStr, path + ".new")))
                {
                    if (str2.Contains(".method public static compareSignatures("))
                    {
                        int num = str2.LastIndexOf(".method public static compareSignatures(");
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
                        str2 = str2.Substring(0, num) + "\n\n    const/4 v0, 0x0\n\n    return v0\n\n" +
                               str2.Substring(startIndex);
                        platform.Log("Patched signature verification util function");
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