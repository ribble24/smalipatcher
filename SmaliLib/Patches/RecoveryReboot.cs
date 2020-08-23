using System.IO;
using System.IO.Compression;
using SmaliLib.Steps;

namespace SmaliLib.Patches
{
    public class RecoveryReboot : IPatch
    {
        public override string Title { get; } = "Recovery reboot";
        public override string Description { get; } = "Reboot directly back into recovery from powermenu";
        public override string TargetFile { get; } = "services.jar";
        public override void JarCompileStep(IPlatform platform)
        {
        }

        public override string PatchFileStep(IPlatform platform, string baseStr)
        {
            string path = Path.Combine("com", "android", "server", "statusbar", "StatusBarManagerService.smali");
            baseStr = FrameworkPatcher.GetPath(path);
            if (File.Exists(Path.Combine(baseStr, path)))
            {
                string str2 =
                    File.ReadAllText(Path.Combine(baseStr, path));
                using (StreamWriter streamWriter =
                    new StreamWriter(Path.Combine(baseStr, path + ".new")))
                {
                    if (str2.Contains("lambda$reboot$"))
                    {
                        int startIndex1 = str2.IndexOf("lambda$reboot$");
                        while (str2.Substring(startIndex1, 23) != "const-string/jumbo v2, ")
                            ++startIndex1;
                        int length = startIndex1 + 24;
                        int startIndex2 = length;
                        while (str2.Substring(startIndex2, 1) != "\"")
                            ++startIndex2;
                        str2 = str2.Substring(0, length) + "recovery" + str2.Substring(startIndex2);
                        platform.Log("Patched recovery reboot function");
                    }
                    streamWriter.Write(str2);
                }
                File.Replace(Path.Combine(baseStr, path + ".new"),
                    Path.Combine(baseStr, path), null);
                path = Path.Combine("com", "android", "server", "wm", "WindowManagerService.smali");
                baseStr = FrameworkPatcher.GetPath(path);
                if (File.Exists(Path.Combine(baseStr, path)))
                {
                    string str3 = File.ReadAllText(Path.Combine(baseStr, path));
                    using (StreamWriter streamWriter =
                        new StreamWriter(Path.Combine(baseStr, path + ".new")))
                    {
                        if (str3.Contains("reboot(Z)V"))
                        {
                            int startIndex1 = str3.LastIndexOf("reboot(Z)V");
                            while (str3.Substring(startIndex1, 11) != ".end method")
                                ++startIndex1;
                            int num1 = startIndex1 + 11;
                            int num2 = str3.LastIndexOf("reboot(Z)V");
                            if (str3.Substring(num2, num1 - num2).Contains("const-string/jumbo v1, "))
                            {
                                while (str3.Substring(num2, 23) != "const-string/jumbo v1, ")
                                    ++num2;
                                num2 += 24;
                            }
                            if (str3.Substring(num2, num1 - num2).Contains("const-string v1, "))
                            {
                                while (str3.Substring(num2, 17) != "const-string v1, ")
                                    ++num2;
                                num2 += 18;
                            }
                            int startIndex2 = num2;
                            while (str3.Substring(startIndex2, 1) != "\"")
                                ++startIndex2;
                            str3 = str3.Substring(0, num2) + "recovery" + str3.Substring(startIndex2);
                            platform.Log("Patched recovery reboot function");
                        }
                        streamWriter.Write(str3);
                    }
                    File.Replace(Path.Combine(baseStr, path + ".new"),
                        Path.Combine(baseStr, path), null);
                }
                else
                {
                    platform.Warning("Reboot behaviour status class not found");
                    return baseStr;
                }
            }
            else
            {
                platform.Warning("Reboot behaviour status class not found");
                return baseStr;
            }
            return baseStr;
        }

        public override void PackModuleStep(ZipArchive archive)
        {
        }
    }
}