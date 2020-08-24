using System.IO;
using System.IO.Compression;
using System.Text;
using SmaliLib.Steps;

namespace SmaliLib.Patches
{
    public class SamsungKnox : IPatch
    {
        public SamsungKnox() => PostFsData = true;
        public override string Title { get; } = "Samsung Knox";
        public override string Description { get; } = "Bypass Samsung knox-trip protection (secure folder)";
        public override string TargetFile { get; } = "services.jar";

        public override void JarCompileStep(IPlatform platform)
        {
        }

        public override string PatchFileStep(IPlatform platform, string baseStr)
        {
            string path = Path.Combine("com", "android", "server", "KnoxFileHandler.smali");
            baseStr = FrameworkPatcher.GetPath(path);
            if (File.Exists(Path.Combine(baseStr, path)))
            {
                string str2 = File.ReadAllText(Path.Combine(baseStr, path));
                using (StreamWriter streamWriter =
                    new StreamWriter(Path.Combine(baseStr, path + ".new")))
                {
                    if (str2.Contains(".method public isTimaAvailable("))
                    {
                        int num = str2.LastIndexOf(".method public isTimaAvailable(");
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
                        platform.Log("Patched knox function");
                    }
                    streamWriter.Write(str2);
                }
                File.Replace(Path.Combine(baseStr, path + ".new"),
                    Path.Combine(baseStr, path), null);
                path = Path.Combine("com", "android", "server", "pm", "PersonaServiceHelper.smali");
                baseStr = FrameworkPatcher.GetPath(path);
                if (File.Exists(Path.Combine(baseStr, path)))
                {
                    string str3 = File.ReadAllText(Path.Combine(baseStr, path));
                    using (StreamWriter streamWriter =
                        new StreamWriter(Path.Combine(baseStr, path + ".new")))
                    {
                        if (str3.Contains(".method public static isTimaAvailable("))
                        {
                            int num = str3.LastIndexOf(".method public static isTimaAvailable(");
                            while (str3.Substring(num, 7) != ".locals" && str3.Substring(num, 10) != ".registers")
                                ++num;
                            if (str3.Substring(num, 7) == ".locals")
                                num += 8;
                            if (str3.Substring(num, 10) == ".registers")
                                num += 11;
                            while (int.TryParse(str3.Substring(num, 1), out _))
                                ++num;
                            int startIndex = num;
                            while (str3.Substring(startIndex, 11) != ".end method")
                                ++startIndex;
                            str3 = str3.Substring(0, num) + "\n\n    const/4 v0, 0x1\n\n    return v0\n\n" +
                                   str3.Substring(startIndex);
                            platform.Log("Patched knox function");
                        }
                        streamWriter.Write(str3);
                    }
                    File.Replace(Path.Combine(baseStr, path + ".new"),
                        Path.Combine(baseStr, path), null);
                    path = Path.Combine("com", "android", "server", "pm", "TimaHelper.smali");
                    baseStr = FrameworkPatcher.GetPath(path);
                    if (File.Exists(Path.Combine(baseStr, path)))
                    {
                        string str4 = File.ReadAllText(Path.Combine(baseStr, path));
                        using (StreamWriter streamWriter =
                            new StreamWriter(Path.Combine(baseStr, path + ".new")))
                        {
                            if (str4.Contains(".method public isTimaAvailable("))
                            {
                                int num = str4.LastIndexOf(".method public isTimaAvailable(");
                                while (str4.Substring(num, 7) != ".locals" &&
                                       str4.Substring(num, 10) != ".registers")
                                    ++num;
                                if (str4.Substring(num, 7) == ".locals")
                                    num += 8;
                                if (str4.Substring(num, 10) == ".registers")
                                    num += 11;
                                while (int.TryParse(str4.Substring(num, 1), out _))
                                    ++num;
                                int startIndex = num;
                                while (str4.Substring(startIndex, 11) != ".end method")
                                    ++startIndex;
                                str4 = str4.Substring(0, num) + "\n\n    const/4 v0, 0x1\n\n    return v0\n\n" +
                                       str4.Substring(startIndex);
                                platform.Log("Patched knox function");
                            }
                            streamWriter.Write(str4);
                        }
                        File.Replace(Path.Combine(baseStr, path + ".new"),
                            Path.Combine(baseStr, path), null);
                    }
                    else
                    {
                        platform.Warning("Knox patch class not found");
                        return baseStr;
                    }
                }
                else
                {
                    platform.Warning("Knox patch class not found");
                    return baseStr;
                }
            }
            else
            {
                platform.Warning("Knox patch class not found");
                return baseStr;
            }
            return baseStr;
        }

        public override void PackModuleStep(ZipArchive archive)
        {
            archive.CreateEntry("common/");
            ZipArchiveEntry entry = archive.CreateEntry("common/post-fs-data.sh");
            using Stream s = entry.Open();
            using StreamWriter writer = new StreamWriter(s, Encoding.ASCII);
            writer.Write("MODDIR=${0%/*}\nresetprop --delete ro.config.iccc_version");
        }
    }
}