using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using SmaliLib.Patches;

namespace SmaliLib.Steps
{
    internal class FrameworkPatcher
    {
        private bool _hasBeenDeodexed;
        private bool _dexPatcherCoreRequired;
        private string _dexPatcherTarget;
        private IPatch[] _patches;
        private FrameworkPatcher(IPatch[] patches) => _patches = patches;

        public static void Patch(IPlatform platform, string folderPath, IPatch[] patches)
        {
            string api = File.Exists(Path.Combine(folderPath, "build.prop"))
                ? (File.ReadAllLines(Path.Combine(folderPath, "build.prop"))
                    .FirstOrDefault(s => s.Contains("ro.build.version.sdk=")) ?? "ro.build.version.sdk=00")
                .Substring(21, 2)
                : "00";
            platform.Log("Processing framework");
            List<string> processedFiles = new List<string>();
            FrameworkPatcher patcher = new FrameworkPatcher(patches);
            foreach (IPatch patch in patches)
                if (!processedFiles.Contains(patch.TargetFile))
                {
                    processedFiles.Add(patch.TargetFile);
                    string path = "";
                    string targetFile = patch.TargetFile;
                    string str1 = targetFile.Split('.')[0];
                    string[] files1 = Directory.GetFiles(folderPath, targetFile, SearchOption.AllDirectories);
                    if (files1.Length == 1)
                        path = Path.GetDirectoryName(files1[0]);
                    else if (files1.Length > 1)
                    {
                        platform.ErrorCritical($"Multiple instances of {targetFile} found.");
                        return;
                    }
                    if (Directory.Exists(path))
                    {
                        bool flag = false;
                        using (ZipArchive zipFile = ZipFile.OpenRead(files1[0]))
                            if (zipFile.GetEntry("classes.dex") != null)
                                flag = true;
                        if (api != "00")
                            platform.Log("Detected API: " + api);
                        string[] files2 = Directory.GetFiles(path, str1 + ".odex", SearchOption.AllDirectories);
                        if (files2.Length == 0)
                            files2 = Directory.GetFiles(path, "boot-" + str1 + ".odex", SearchOption.AllDirectories);
                        string[] files3 = Directory.GetFiles(path, str1 + ".vdex", SearchOption.AllDirectories);
                        if (files3.Length == 0)
                            files3 = Directory.GetFiles(path, "boot-" + str1 + ".vdex", SearchOption.AllDirectories);
                        string[] files4 = Directory.GetFiles(path, str1 + ".oat", SearchOption.AllDirectories);
                        if (files4.Length == 0)
                            files4 = Directory.GetFiles(path, "boot-" + str1 + ".oat", SearchOption.AllDirectories);
                        string str2 = "";
                        string c = Path.DirectorySeparatorChar.ToString();
                        if (c == "\\")
                            c = "\\\\";
                        if (files2.Length != 0 && files2[0].Contains("arm"))
                        {
                            string str3 = new Regex($"\\b{c}arm.*{c}\\b").Match(files2[0]).Value;
                            str2 = str3.Substring(0, str3.Length - 1);
                        }
                        if (str2 == "" && files4.Length != 0)
                            foreach (string input in files4)
                                if (input.Contains("arm"))
                                {
                                    string str3 = new Regex("\\b\\\\arm.*\\\\\\b").Match(input).Value;
                                    str2 = str3.Substring(0, str3.Length - 1);
                                }
                        if (File.Exists(Path.Combine(path, targetFile)))
                        {
                            long num1 = 0;
                            long num2 = 0;
                            long num3 = 0;
                            foreach (string fileName in files2)
                                num1 += new FileInfo(fileName).Length;
                            foreach (string fileName in files3)
                                num2 += new FileInfo(fileName).Length;
                            foreach (string fileName in files4)
                                num3 += new FileInfo(fileName).Length;
                            if (flag)
                                patcher.JarDecompile(platform, Path.Combine(path, targetFile));
                            else if (files2.Length != 0 && files3.Length == 0 && num1 > 0L)
                                patcher.OdexDeodex(platform, files2[0], targetFile, path + str2, api);
                            else if (files3.Length != 0 && num2 > 0L)
                                patcher.VdexExtract(platform, files3[0], files1[0]);
                            else if (!flag && files4.Length != 0 && num3 > 0L)
                                patcher.OdexDeodex(platform, Path.Combine(path, str2, Path.GetFileName(files4[0])), targetFile, Path.Combine(path, str2), api);
                            else if (!flag && files2.Length == 0 && files3.Length == 0 && files4.Length == 0)
                            {
                                platform.ErrorCritical("Incomplete framework dump, required files missing (You can try running the patcher while booted into recovery mode with /system mounted, it may fix this)");
                                return;
                            }
                        }
                        else if (!File.Exists(Path.Combine(path, targetFile)))
                        {
                            platform.ErrorCritical($"{targetFile} not found");
                            return;
                        }
                    }
                    else if (!Directory.Exists(path))
                    {
                        platform.ErrorCritical("Base directory not found");
                        return;
                    }
                }
        }

        private void JarDecompile(IPlatform platform, string jarAddress)
        {
            string fileName = Path.GetFileName(jarAddress);
            if (!Directory.Exists("bin") || !File.Exists(Path.Combine("bin", "apktool.jar")))
                return;
            platform.Log($"Decompiling {jarAddress}");
            BinW.LogIncremental(platform, BinW.RunCommand(Bin.java, $"-Xms1024m -Xmx1024m -jar {Path.Combine("bin", "apktool.jar")} d \"{jarAddress}\" -o tmp -f"));
            if (_hasBeenDeodexed)
            {
                if (File.Exists("classes.dex"))
                {
                    if (Directory.Exists("tmp"))
                        File.Move("classes.dex", Path.Combine("tmp", "classes.dex"));
                }
                else
                {
                    platform.ErrorCritical("Deodex failed - classes.dex not found");
                    return;
                }
            }
            if (!_hasBeenDeodexed)
            {
                platform.Log($"Patching {jarAddress}");
                PatchFiles(platform, fileName, jarAddress);
            }
            else if (_hasBeenDeodexed)
                JarCompile(platform, Path.Combine("tmp", "dist", fileName), "tmp");
            _hasBeenDeodexed = false;
        }

        private void JarCompile(IPlatform platform, string outputFile, string sourceDirectory)
        {
            if (!Directory.Exists("bin") || !File.Exists(Path.Combine("bin", "apktool.jar")))
                return;
            string fileName = Path.GetFileName(outputFile);
            platform.Log($"Recompiling {sourceDirectory}");
            BinW.LogIncremental(platform, BinW.RunCommand(Bin.java,
                $"-Xms1024m -Xmx1024m -jar {Path.Combine("bin", "apktool.jar")} b -o {outputFile} {sourceDirectory}"));
            if (!File.Exists(Path.Combine("tmp", "dist", fileName)))
                platform.ErrorCritical($"Compile failed - {fileName} not found");
            else
            {
                foreach (IPatch patch in _patches)
                {
                    if (patch.TargetFile == fileName)
                    {
                        patch.DexPatcherTarget = _dexPatcherTarget;
                        patch.DexPatcherCoreRequired = _dexPatcherCoreRequired;
                        patch.JarCompileStep(platform);
                    }
                }
                if (!Directory.Exists("apk"))
                    Directory.CreateDirectory("apk");
                if (!File.Exists(Path.Combine("tmp", "dist", fileName)))
                    return;
                if (File.Exists(Path.Combine("apk", fileName)))
                    File.Delete(Path.Combine("apk", fileName));
                File.Move(Path.Combine("tmp", "dist", fileName), Path.Combine("apk", fileName));
            }
        }

        private void OdexDeodex(IPlatform platform, string odexPath, string targetJarPath, string frameworkPath, string api)
        {
            if (!Directory.Exists("bin") || !File.Exists(Path.Combine("bin", "baksmali.jar")))
                return;
            string fileName = Path.GetFileName(odexPath);
            if (Directory.Exists(frameworkPath))
            {
                if (Directory.Exists("smali"))
                    Directory.Delete("smali", true);
                Directory.CreateDirectory("smali");
                platform.Log("Deodexing");
                _hasBeenDeodexed = true;
                if (api != "00" && odexPath.Contains(".odex"))
                    BinW.LogIncremental(platform, BinW.RunCommand(Bin.java,
                        $"-Xms1024m -Xmx1024m -jar {Path.Combine("bin", "baksmali.jar")} x \"{Path.GetFullPath(odexPath)}\" -a {api} -d \"{Path.GetFullPath(frameworkPath)}\" -o smali"));
                else if (api == "00" && odexPath.Contains(".odex"))
                    BinW.LogIncremental(platform, BinW.RunCommand(Bin.java,
                        $"-Xms1024m -Xmx1024m -jar {Path.Combine("bin", "baksmali.jar")} x \"{Path.GetFullPath(odexPath)}\" -d \"{Path.GetFullPath(frameworkPath)}\" -o smali"));
                else if (odexPath.Contains(".oat"))
                    BinW.LogIncremental(platform, BinW.RunCommand(Bin.java,
                        $"-Xms1024m -Xmx1024m -jar {Path.Combine("bin", "baksmali.jar")} x \"{Path.GetFullPath(odexPath)}\" -o smali"));
                platform.Log("Deodexed " + fileName);
                platform.Log("Patching");
                PatchFiles(platform, Path.GetFileName(targetJarPath), frameworkPath);
            }
            else
                platform.ErrorCritical("Framework directory missing");
        }

        private void OdexCompile(IPlatform platform, string targetFileNoExt, string basePath)
        {
            if (!Directory.Exists("bin") || !File.Exists(Path.Combine("bin", "smali.jar")))
                return;
            platform.Log("Generating classes");
            if (Directory.Exists("smali"))
            {
                BinW.LogIncremental(platform, BinW.RunCommand(Bin.java, $"-Xms1024m -Xmx1024m -jar {Path.Combine("bin", "smali.jar")} a --verbose smali -o classes.dex"));
                if (File.Exists("classes.dex"))
                {
                    platform.Log("Generated classes.dex");
                    if (!File.Exists(Path.Combine(basePath, targetFileNoExt + ".jar")))
                        platform.ErrorCritical("Deodex target file missing");
                    else
                        JarDecompile(platform, Path.Combine(basePath, targetFileNoExt + ".jar"));
                }
                else
                    platform.ErrorCritical("Generating classes.dex failed");
            }
            else
                platform.ErrorCritical("Smali directory missing");
        }

        private void VdexExtract(IPlatform platform, string vdexAddress, string jarFile)
        {
            platform.Log("Extracting vdex");
            File.Copy(Path.GetFullPath(vdexAddress), Path.Combine("bin", Path.GetFileName(vdexAddress)), true);
            BinW.LogIncremental(platform, BinW.RunCommand(Bin.vdexExtractor,
                $"-i \"{Path.Combine("bin", Path.GetFileName(vdexAddress))}\" --ignore-crc-error"));
            File.Delete(Path.Combine("bin", Path.GetFileName(vdexAddress)));
            if (File.Exists(Path.Combine("bin", Path.GetFileNameWithoutExtension(vdexAddress) + ".apk_classes.dex")))
            {
                if (File.Exists(Path.Combine("bin", "classes.dex")))
                    File.Delete(Path.Combine("bin", "classes.dex"));
                File.Move(Path.Combine("bin", Path.GetFileNameWithoutExtension(vdexAddress) + ".apk_classes.dex"),
                    Path.Combine("bin", "classes.dex"));
            }
            string[] files1 = Directory.GetFiles("bin",
                Path.GetFileNameWithoutExtension(vdexAddress) + ".apk__classes*.dex");
            for (int index = 0; index < files1.Length; ++index)
            {
                if (File.Exists(Path.Combine("bin", $"classes{index + 2}.dex")))
                    File.Delete(Path.Combine("bin", $"classes{index + 2}.dex"));
                File.Move(files1[index], Path.Combine("bin", $"classes{index + 2}.dex"));
            }
            if (File.Exists(Path.Combine("bin", Path.GetFileNameWithoutExtension(vdexAddress) + "_classes.dex")))
            {
                if (File.Exists(Path.Combine("bin", "classes.dex")))
                    File.Delete(Path.Combine("bin", "classes.dex"));
                File.Move(Path.Combine("bin", Path.GetFileNameWithoutExtension(vdexAddress) + "_classes.dex"), Path.Combine("bin", "classes.dex"));
            }
            string[] files2 =
                Directory.GetFiles("bin", Path.GetFileNameWithoutExtension(vdexAddress) + "_classes*.dex");
            for (int index = 0; index < files2.Length; ++index)
            {
                if (File.Exists(Path.Combine("bin", $"classes{index + 2}.dex")))
                    File.Delete(Path.Combine("bin", $"classes{index + 2}.dex"));
                File.Move(files2[index], Path.Combine("bin", $"classes{index + 2}.dex"));
            }
            if (File.Exists(Path.Combine("bin", Path.GetFileNameWithoutExtension(vdexAddress) + "_classes.cdex")))
            {
                if (File.Exists(Path.Combine("bin", "classes.cdex")))
                    File.Delete(Path.Combine("bin", "classes.cdex"));
                File.Move(Path.Combine("bin", Path.GetFileNameWithoutExtension(vdexAddress) + "_classes.cdex"),
                    Path.Combine("bin", "classes.cdex"));
            }
            string[] files3 =
                Directory.GetFiles("bin", Path.GetFileNameWithoutExtension(vdexAddress) + "_classes*.cdex");
            for (int index = 0; index < files3.Length; ++index)
            {
                if (File.Exists(Path.Combine("bin", $"classes{index + 2}.cdex")))
                    File.Delete(Path.Combine("bin", $"classes{index + 2}.cdex"));
                File.Move(files3[index], Path.Combine("bin", $"classes{index + 2}.cdex"));
            }
            if (File.Exists(Path.Combine("bin", "classes.cdex")))
            {
                platform.Log("Extracting classes.cdex");
                CdexToDex(platform, Path.Combine("bin", "classes.cdex"));
                if (!File.Exists(Path.Combine("bin", "classes.dex")))
                {
                    platform.ErrorCritical("Failed extracting classes.cdex");
                    return;
                }
            }
            string[] files4 = Directory.GetFiles("bin", "classes*.cdex");
            foreach (string t in files4)
            {
                platform.Log($"Extracting {Path.GetFileName(t)}");
                CdexToDex(platform, t);
                if (!File.Exists(Path.Combine("bin", Path.ChangeExtension(t, "dex"))))
                {
                    platform.ErrorCritical($"Failed extracting {Path.GetFileName(t)}");
                    return;
                }
            }
            using (ZipArchive zipFile = ZipFile.Open(jarFile, ZipArchiveMode.Update))
            {
                string[] files5 = Directory.GetFiles("bin", "classes*.dex");
                foreach (string t in files5)
                {
                    zipFile.GetEntry(Path.GetFileName(t))?.Delete();
                    zipFile.CreateEntry(t);
                }
            }
            if (File.Exists(Path.Combine("bin", "classes.dex")))
            {
                platform.Log("Extracted classes.dex");
                File.Delete(Path.Combine("bin", "classes.dex"));
            }
            string[] files6 = Directory.GetFiles("bin", "classes*.dex");
            foreach (string t in files6)
            {
                platform.Log("Extracted " + Path.GetFileName(t));
                File.Delete(Path.Combine("bin", Path.GetFileName(t)));
            }
            JarDecompile(platform, jarFile);
        }

        private void CdexToDex(IPlatform platform, string cdexAddress)
        {
            if (Directory.Exists("bin") && File.Exists(Path.Combine("bin", "compact_dex_converter")))
            {
                FrameworkDumper.Push(platform, Path.Combine("bin", "compact_dex_converter"), "/data/local/tmp/");
                FrameworkDumper.Shell(platform, "chmod 777 /data/local/tmp/compact_dex_converter");
                FrameworkDumper.Push(platform, cdexAddress, "/data/local/tmp/");
                FrameworkDumper.Shell(platform, "/data/local/tmp/compact_dex_converter /data/local/tmp/" + Path.GetFileName(cdexAddress));
                FrameworkDumper.Pull(platform, "/data/local/tmp/" + Path.GetFileName(cdexAddress) + ".new", "bin");
                FrameworkDumper.Shell(platform, "rm -f /data/local/tmp/compact_dex_converter");
                FrameworkDumper.Shell(platform, "rm -f /data/local/tmp/" + Path.GetFileName(cdexAddress));
                FrameworkDumper.Shell(platform, "rm -f /data/local/tmp/" + Path.GetFileName(cdexAddress) + ".new");
                if (File.Exists(cdexAddress))
                    File.Delete(cdexAddress);
                if (File.Exists(Path.Combine("bin", Path.GetFileNameWithoutExtension(cdexAddress) + ".dex")))
                    File.Delete(Path.Combine("bin", Path.GetFileNameWithoutExtension(cdexAddress) + ".dex"));
                if (!File.Exists(Path.Combine("bin", Path.GetFileName(cdexAddress) + ".new")))
                    return;
                File.Move(Path.Combine("bin", Path.GetFileName(cdexAddress) + ".new"),
                    Path.Combine("bin", Path.GetFileNameWithoutExtension(cdexAddress) + ".dex"));
            }
            else
                platform.ErrorCritical("compact_dex_converter missing");
        }
        
        public static string GetPath(string file)
        {
            string str = "";
            if (string.IsNullOrEmpty(file))
            {
                if (Directory.Exists(Path.Combine("tmp", "smali")))
                    str = Path.Combine("tmp", "smali") + Path.DirectorySeparatorChar;
                else if (Directory.Exists("smali"))
                    str = "smali" + Path.DirectorySeparatorChar;
            }
            else if (File.Exists(Path.Combine("tmp", file)))
                str = "tmp" + Path.DirectorySeparatorChar;
            else if (File.Exists(Path.Combine("tmp", "smali", file)))
                str = Path.Combine("tmp", "smali") + Path.DirectorySeparatorChar;
            else if (File.Exists(Path.Combine("tmp", "smali_classes2", file)))
                str = Path.Combine("tmp", "smali_classes2") + Path.DirectorySeparatorChar;
            else if (File.Exists(Path.Combine("tmp", "smali_classes3", file)))
                str = Path.Combine("tmp", "smali_classes3") + Path.DirectorySeparatorChar;
            else if (File.Exists(Path.Combine("tmp", "smali_classes4", file)))
                str = Path.Combine("tmp", "smali_classes4") + Path.DirectorySeparatorChar;
            else if (File.Exists(Path.Combine("smali", file)))
                str = "smali" + Path.DirectorySeparatorChar;
            return str;
        }

        private void PatchFiles(IPlatform platform, string targetFile, string targetFilePath)
        {
            string baseStr = "";
            string withoutExtension = Path.GetFileNameWithoutExtension(targetFile);
            foreach (IPatch patch in _patches)
            {
                if (patch.TargetFile == withoutExtension + ".jar")
                {
                    patch.DexPatcherTarget = _dexPatcherTarget;
                    patch.DexPatcherCoreRequired = _dexPatcherCoreRequired;
                    baseStr = patch.PatchFileStep(platform, baseStr);
                }
            }
            if (baseStr == "")
                baseStr = GetPath("");
            if (baseStr.Contains("tmp"))
                JarCompile(platform, Path.Combine("tmp", "dist", withoutExtension + ".jar"), "tmp");
            else
            {
                if (baseStr == $"smali{Path.DirectorySeparatorChar}")
                    OdexCompile(platform, withoutExtension, Path.GetDirectoryName(targetFilePath));
            }
        }
    }
}