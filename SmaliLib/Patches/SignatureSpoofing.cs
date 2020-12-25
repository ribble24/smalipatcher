using System;
using System.IO;
using System.IO.Compression;
using SmaliLib.Steps;

namespace SmaliLib.Patches
{
    public class SignatureSpoofing : IPatch
    {
        public override string Title { get; } = "Signature spoofing";
        public override string Description { get; } = "Allow app signature spoofing permission";
        public override string TargetFile { get; } = "services.jar";
        public override bool IsDefault { get; } = false;

        public override void JarCompileStep(IPlatform platform)
        {
            if (DexPatcherCoreRequired)
            {
                DexPatcher(platform, Path.Combine("tmp", "dist", "services.jar"), DexPatcherTarget);
                DexPatcher(platform, Path.Combine("tmp", "dist", "services.jar"),
                    Path.Combine("bin", "sigspoof_core.dex"));
            }
            else
                platform.Log("\n==> Signature spoofing patch already enabled.");
        }

        public override string PatchFileStep(IPlatform platform, string baseStr)
        {
            string path = Path.Combine("com", "android", "server", "pm", "PackageManagerService.smali");
            baseStr = FrameworkPatcher.GetPath(path);
            if (File.Exists(Path.Combine(baseStr, path)))
            {
                string str2 = File.ReadAllText(Path.Combine(baseStr, path));
                if (str2.Contains(".method private generatePackageInfo(") ||
                    str2.Contains(".method generatePackageInfo("))
                {
                    int startIndex1 = str2.LastIndexOf(".method private generatePackageInfo(");
                    if (startIndex1 == -1)
                        startIndex1 = str2.LastIndexOf(".method generatePackageInfo(");
                    int startIndex2 = startIndex1;
                    while (str2.Substring(startIndex2, 2) != $";{Environment.NewLine[0]}")
                        startIndex2 += 2;
                    int num = startIndex2 + 1;
                    if (str2.Substring(startIndex1, num - startIndex1).Contains("PackageParser"))
                        DexPatcherTarget = Path.Combine("bin", "sigspoof_4.1-6.0.dex");
                    else if (str2.Substring(startIndex1, num - startIndex1).Contains("PackageSetting"))
                        DexPatcherTarget = Path.Combine("bin", "sigspoof_7.0-9.0.dex");
                }
                path = Path.Combine("com", "android", "server", "pm", "GeneratePackageInfoHook.smali");
                if (!File.Exists(Path.Combine(FrameworkPatcher.GetPath(path), path)))
                    DexPatcherCoreRequired = true;
            }
            else
                platform.Warning("Signature spoof class not found");
            return baseStr;
        }

        public override void PackModuleStep(ZipArchive archive)
        {
        }
    }
}