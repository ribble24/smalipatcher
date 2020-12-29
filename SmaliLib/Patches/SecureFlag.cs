using System.IO;
using System.IO.Compression;
using SmaliLib.Steps;

namespace SmaliLib.Patches
{
    public class SecureFlag : IPatch
    {
        public override string Title { get; } = "Secure flag";
        public override string Description { get; } = "Allow screenshots/screensharing in secure apps";
        public override string TargetFile { get; } = "services.jar";
        public override bool IsDefault { get; } = false;

        public override void JarCompileStep(IPlatform platform)
        {
        }

        public override string PatchFileStep(IPlatform platform, string baseStr)
        {
            string path = Path.Combine("com", "android", "server", "wm", "WindowManagerService.smali");
            baseStr = FrameworkPatcher.GetPath(path);
            if (File.Exists(Path.Combine(baseStr, path)))
            {
                string str2 = File.ReadAllText(Path.Combine(baseStr, path));
                using (StreamWriter streamWriter =
                    new StreamWriter(Path.Combine(baseStr, path + ".new")))
                {
                    if (str2.Contains(".method isSecureLocked"))
                    {
                        int num = str2.LastIndexOf(".method isSecureLocked(");
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
                        platform.Log("Patched secure flag function");
                    }
                    if (str2.Contains("setScreenCaptureDisabled("))
                    {
                        int num = str2.LastIndexOf("setScreenCaptureDisabled(");
                        while (str2.Substring(num, 17) != "SparseArray;->put")
                            ++num;
                        while (str2.Substring(num, 1) != "v")
                            --num;
                        string str3 = str2.Substring(num, 2);
                        while (str2.Substring(num, 14) != "invoke-virtual")
                            --num;
                        str2 = str2.Substring(0, num) + "const/4 " + str3 + ", 0x0\n\n    " + str2.Substring(num);
                        platform.Log("Patched screen capture boolean");
                    }
                    streamWriter.Write(str2);
                }
                File.Replace(Path.Combine(baseStr, path + ".new"),
                    Path.Combine(baseStr, path), null);
            }
            path = Path.Combine("com", "android", "server", "wm", "ScreenshotController.smali");
            baseStr = FrameworkPatcher.GetPath(path);
            if (File.Exists(Path.Combine(baseStr, path)))
            {
                string str2 = File.ReadAllText(Path.Combine(baseStr, path));
                using (StreamWriter streamWriter =
                    new StreamWriter(Path.Combine(baseStr, path + ".new")))
                {
                    if (str2.Contains(".method private preventTakingScreenshotToTargetWindow"))
                    {
                        int num = str2.LastIndexOf(".method private preventTakingScreenshotToTargetWindow(");
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
                        str2 = str2.Substring(0, num) + "\n\n    const/4 v0, 0x0\n\n    return v0\n\n" + str2.Substring(startIndex);
                        platform.Log("Patched screenshot controller");
                    }
                    streamWriter.Write(str2);
                }
                File.Replace(Path.Combine(baseStr, path + ".new"),
                    Path.Combine(baseStr, path), null);
            }
            path = Path.Combine("com", "android", "server", "wm", "WindowSurfaceController.smali");
            baseStr = FrameworkPatcher.GetPath(path);
            if (File.Exists(Path.Combine(baseStr, path)))
            {
                string str2 = File.ReadAllText(Path.Combine(baseStr, path));
                using (StreamWriter streamWriter =
                    new StreamWriter(Path.Combine(baseStr, path + ".new")))
                {
                    if (str2.Contains(".method setSecure("))
                    {
                        int num = str2.LastIndexOf(".method setSecure(");
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
                        str2 = str2.Substring(0, num) + "\n\n    return-void\n\n" + str2.Substring(startIndex);
                        platform.Log("Patched set secure function");
                    }
                    streamWriter.Write(str2);
                }
                File.Replace(Path.Combine(baseStr, path + ".new"),
                    Path.Combine(baseStr, path), null);
            }
            path = Path.Combine("com", "android", "server", "devicepolicy", "DevicePolicyManagerService.smali");
            baseStr = FrameworkPatcher.GetPath(path);
            if (File.Exists(Path.Combine(baseStr, path)))
            {
                string str2 =
                    File.ReadAllText(Path.Combine(baseStr, path));
                using (StreamWriter streamWriter =
                    new StreamWriter(Path.Combine(baseStr, path + ".new")))
                {
                    if (str2.Contains(".method public setScreenCaptureDisabled("))
                    {
                        int num = str2.LastIndexOf(".method public setScreenCaptureDisabled(");
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
                        str2 = str2.Substring(0, num) + "\n\n    return-void\n\n" + str2.Substring(startIndex);
                        platform.Log("Patched capture function");
                    }
                    if (str2.Contains(".method public getScreenCaptureDisabled("))
                    {
                        int num = str2.LastIndexOf(".method public getScreenCaptureDisabled(");
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
                        platform.Log("Patched get capture function");
                    }
                    streamWriter.Write(str2);
                }
                File.Replace(Path.Combine(baseStr, path + ".new"),
                    Path.Combine(baseStr, path), null);
            }
            path = Path.Combine("com", "android", "server", "devicepolicy", "DevicePolicyCacheImpl.smali");
            baseStr = FrameworkPatcher.GetPath(path);
            if (File.Exists(Path.Combine(baseStr, path)))
            {
                string str2 =
                    File.ReadAllText(Path.Combine(baseStr, path));
                using (StreamWriter streamWriter =
                    new StreamWriter(Path.Combine(baseStr, path + ".new")))
                {
                    if (str2.Contains(".method public setScreenCaptureDisabled("))
                    {
                        int num = str2.LastIndexOf(".method public setScreenCaptureDisabled(");
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
                        str2 = str2.Substring(0, num) + "\n\n    return-void\n\n" + str2.Substring(startIndex);
                        platform.Log("Patched capture function");
                    }
                    if (str2.Contains(".method public getScreenCaptureDisabled("))
                    {
                        int num = str2.LastIndexOf(".method public getScreenCaptureDisabled(");
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
                        platform.Log("Patched get capture function");
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