using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Ionic.Zip;
using Ionic.Zlib;

namespace SmaliPatcher
{
    public class Patch
    {
        private Adb _adb;
        private bool _dexPatcherCoreRequired;
        private string _dexPatcherTarget;
        private Download _download;
        private bool _hasBeenDeodexed;
        private MainForm _mainForm;
        private readonly List<string> _processedFiles = new List<string>();
        public List<Patches> Patches;

        public void Init(object sender)
        {
            if (_mainForm != null)
                return;
            _mainForm = (MainForm) sender;
        }

        public void ProcessFrameworkDirectory(string folderPath)
        {
            if (_download == null)
            {
                _download = new Download();
                _download.Init(_mainForm);
            }
            if (Directory.Exists("apk"))
                Directory.Delete("apk", true);
            string api = "00";
            if (File.Exists(folderPath + "\\build.prop"))
            {
                string[] strArray = File.ReadAllLines(folderPath + "\\build.prop");
                if (strArray.Length != 0)
                    for (int index = 0; index < strArray.Length; ++index)
                        if (strArray[index].Contains("ro.build.version.sdk="))
                        {
                            api = strArray[index].Substring(21, 2);
                            break;
                        }
            }
            _mainForm.DebugUpdate("\n==> Processing framework");
            _processedFiles.Clear();
            foreach (Patches patch in _mainForm.Patches)
                if (GetPatchStatus(patch.PatchTitle) && !_processedFiles.Contains(patch.TargetFile))
                {
                    _processedFiles.Add(patch.TargetFile);
                    string path = "";
                    string targetFile = patch.TargetFile;
                    string str1 = targetFile.Split('.')[0];
                    string[] files1 = Directory.GetFiles(folderPath, targetFile, SearchOption.AllDirectories);
                    if (files1.Length == 1)
                        path = Path.GetDirectoryName(files1[0]);
                    else if (files1.Length > 1)
                    {
                        _mainForm.DebugUpdate("\n!!! ERROR: Multiple " + targetFile + " found.");
                        _mainForm.StatusUpdate("ERROR..");
                        return;
                    }
                    if (Directory.Exists(path))
                    {
                        bool flag = false;
                        using (ZipFile zipFile = ZipFile.Read(files1[0]))
                            if (zipFile.ContainsEntry("classes.dex"))
                                flag = true;
                        if (api != "00")
                            _mainForm.DebugUpdate("\n==> Detected API: " + api);
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
                        if (files2.Length != 0 && files2[0].Contains("arm"))
                        {
                            string str3 = new Regex("\\b\\\\arm.*\\\\\\b").Match(files2[0]).Value;
                            str2 = str3.Substring(0, str3.Length - 1);
                        }
                        if (str2 == "" && files4.Length != 0)
                            foreach (string input in files4)
                                if (input.Contains("arm"))
                                {
                                    string str3 = new Regex("\\b\\\\arm.*\\\\\\b").Match(input).Value;
                                    str2 = str3.Substring(0, str3.Length - 1);
                                }
                        if (File.Exists(path + "\\" + targetFile))
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
                                JarDecompile(path + "\\" + targetFile);
                            else if (files2.Length != 0 && files3.Length == 0 && num1 > 0L)
                                OdexDeodex(files2[0], targetFile, path + str2, api);
                            else if (files3.Length != 0 && num2 > 0L)
                                VdexExtract(files3[0], files1[0]);
                            else if (!flag && files4.Length != 0 && num3 > 0L)
                                OdexDeodex(path + str2 + "\\" + Path.GetFileName(files4[0]), targetFile, path + str2,
                                    api);
                            else if (!flag && files2.Length == 0 && files3.Length == 0 && files4.Length == 0)
                            {
                                _mainForm.DebugUpdate(
                                    "\n!!! ERROR: Incomplete framework dump, required files missing.");
                                _mainForm.DebugUpdate(
                                    "\n\nYou can try running the patcher while booted into recovery mode with /system mounted, it may fix this.");
                                _mainForm.StatusUpdate("ERROR..");
                                return;
                            }
                        }
                        else if (!File.Exists(path + "\\" + targetFile))
                        {
                            _mainForm.DebugUpdate("\n!!! ERROR: " + targetFile + " not found.");
                            _mainForm.StatusUpdate("ERROR..");
                            return;
                        }
                    }
                    else if (!Directory.Exists(path))
                    {
                        _mainForm.DebugUpdate("\n!!! ERROR: Base directory not found.");
                        _mainForm.StatusUpdate("ERROR..");
                        return;
                    }
                }
            if (!Directory.Exists("apk") || Directory.GetFiles("apk").Length == 0)
                return;
            _download.DownloadMagisk();
        }

        public void JarDecompile(string jarAddress)
        {
            string fileName = Path.GetFileName(jarAddress);
            if (!Directory.Exists("bin") || !File.Exists("bin\\apktool.jar"))
                return;
            _mainForm.StatusUpdate("Decompiling..");
            StartProcess("java.exe", "-Xms1024m -Xmx1024m -jar bin\\apktool.jar d \"" + jarAddress + "\" -o tmp -f");
            if (_hasBeenDeodexed)
            {
                if (File.Exists("classes.dex"))
                {
                    if (Directory.Exists("tmp"))
                        File.Move("classes.dex", "tmp\\classes.dex");
                }
                else
                {
                    _mainForm.DebugUpdate("\n!!! ERROR: Deodex failed - classes.dex not found.");
                    _mainForm.StatusUpdate("ERROR..");
                    return;
                }
            }
            if (!_hasBeenDeodexed)
            {
                _mainForm.StatusUpdate("Patching..");
                PatchFiles(fileName, jarAddress);
            }
            else if (_hasBeenDeodexed)
                JarCompile("tmp\\dist\\" + fileName, "tmp");
            _hasBeenDeodexed = false;
        }

        public void JarCompile(string outputFile, string sourceDirectory)
        {
            if (!Directory.Exists("bin") || !File.Exists("bin\\apktool.jar"))
                return;
            string fileName = Path.GetFileName(outputFile);
            _mainForm.StatusUpdate("Recompiling..");
            StartProcess("java.exe",
                "-Xms1024m -Xmx1024m -jar bin\\apktool.jar b -o " + outputFile + " " + sourceDirectory);
            if (!File.Exists("tmp\\dist\\" + fileName))
            {
                _mainForm.DebugUpdate("\n!!! ERROR: Compile failed - " + fileName + " not found.");
                _mainForm.StatusUpdate("ERROR..");
            }
            else
            {
                if (GetPatchStatus("Signature spoofing") && GetPatchTargetFile("Signature spoofing") == fileName)
                {
                    if (_dexPatcherCoreRequired)
                    {
                        DexPatcher("tmp\\dist\\services.jar", _dexPatcherTarget);
                        DexPatcher("tmp\\dist\\services.jar", "bin\\sigspoof_core.dex");
                    }
                    else
                        _mainForm.DebugUpdate("\n==> Signature spoofing patch already enabled.");
                }
                if (!Directory.Exists("apk"))
                    Directory.CreateDirectory("apk");
                if (!File.Exists("tmp\\dist\\" + fileName))
                    return;
                File.Move("tmp\\dist\\" + fileName, "apk\\" + fileName);
            }
        }

        private void OdexDeodex(
            string odexPath,
            string targetJarPath,
            string frameworkPath,
            string api)
        {
            if (!Directory.Exists("bin") || !File.Exists("bin\\baksmali.jar"))
                return;
            string fileName = Path.GetFileName(odexPath);
            if (!Directory.Exists(frameworkPath))
            {
                _mainForm.StatusUpdate("!!! ERROR: Framework directory missing..");
                _mainForm.StatusUpdate("ERROR..");
            }
            else
            {
                if (Directory.Exists("smali"))
                    Directory.Delete("smali", true);
                Directory.CreateDirectory("smali");
                _mainForm.StatusUpdate("Deodexing..");
                _hasBeenDeodexed = true;
                if (api != "00" && odexPath.Contains(".odex"))
                    StartProcess("java.exe",
                        "-Xms1024m -Xmx1024m -jar bin\\baksmali.jar x \"" + Path.GetFullPath(odexPath) + "\" -a " +
                        api + " -d \"" + Path.GetFullPath(frameworkPath) + "\" -o smali");
                else if (api == "00" && odexPath.Contains(".odex"))
                    StartProcess("java.exe",
                        "-Xms1024m -Xmx1024m -jar bin\\baksmali.jar x \"" + Path.GetFullPath(odexPath) + "\" -d \"" +
                        Path.GetFullPath(frameworkPath) + "\" -o smali");
                else if (odexPath.Contains(".oat"))
                    StartProcess("java.exe",
                        "-Xms1024m -Xmx1024m -jar bin\\baksmali.jar x \"" + Path.GetFullPath(odexPath) + "\" -o smali");
                _mainForm.DebugUpdate("\n==> Deodexed " + fileName);
                _mainForm.StatusUpdate("Patching..");
                PatchFiles(Path.GetFileName(targetJarPath), frameworkPath);
            }
        }

        public void OdexCompile(string targetFileNoExt, string basePath)
        {
            if (!Directory.Exists("bin") || !File.Exists("bin\\smali.jar"))
                return;
            _mainForm.StatusUpdate("Generating classes..");
            if (Directory.Exists("smali"))
            {
                StartProcess("java.exe", "-Xms1024m -Xmx1024m -jar bin\\smali.jar a --verbose smali -o classes.dex");
                if (File.Exists("classes.dex"))
                {
                    _mainForm.DebugUpdate("\n==> Generated classes.dex");
                    if (!File.Exists(basePath + "\\" + targetFileNoExt + ".jar"))
                    {
                        _mainForm.StatusUpdate("!!! ERROR: Deodex target file missing..");
                        _mainForm.StatusUpdate("ERROR..");
                    }
                    else
                        JarDecompile(basePath + "\\" + targetFileNoExt + ".jar");
                }
                else
                {
                    _mainForm.StatusUpdate("!!! ERROR: Generating classes.dex failed..");
                    _mainForm.StatusUpdate("ERROR..");
                }
            }
            else
            {
                _mainForm.StatusUpdate("!!! ERROR: Smali directory missing..");
                _mainForm.StatusUpdate("ERROR..");
            }
        }

        public void DexPatcher(string jar, string dexPatch)
        {
            if (!Directory.Exists("bin") || !File.Exists("bin\\dexpatcher.jar"))
                return;
            if (Directory.Exists("classes"))
                Directory.Delete("classes", true);
            if (!Directory.Exists("classes"))
                Directory.CreateDirectory("classes");
            _mainForm.StatusUpdate("Patching classes..");
            StartProcess("java.exe",
                "-Xms1024m -Xmx1024m -jar bin\\dexpatcher.jar --multi-dex --output classes " + jar + " " + dexPatch);
            using (ZipFile zipFile = ZipFile.Read(jar))
            {
                string[] files = Directory.GetFiles("classes", "classes*");
                for (int index = 0; index < files.Length; ++index)
                {
                    if (zipFile.ContainsEntry(Path.GetFileName(files[index])))
                        zipFile.RemoveEntry(Path.GetFileName(files[index]));
                    zipFile.AddFile(files[index], "/");
                }
                zipFile.CompressionLevel = CompressionLevel.None;
                zipFile.Save();
            }
            if (Directory.Exists("classes"))
                Directory.Delete("classes", true);
            _mainForm.DebugUpdate("\n==> Merged patch: " + Path.GetFileNameWithoutExtension(dexPatch));
        }

        public void VdexExtract(string vdexAddress, string jarFile)
        {
            if (!Directory.Exists("bin") || !File.Exists("bin\\vdexExtractor.exe"))
                return;
            Path.GetFileName(vdexAddress);
            string withoutExtension = Path.GetFileNameWithoutExtension(vdexAddress);
            if (withoutExtension.Length > 5 && withoutExtension.Substring(0, 5) == "boot-")
                withoutExtension.Substring(5);
            _mainForm.StatusUpdate("Extracting vdex..");
            File.Copy(Path.GetFullPath(vdexAddress), "bin\\" + Path.GetFileName(vdexAddress), true);
            StartProcess("bin\\vdexExtractor.exe",
                "-i \"bin\\" + Path.GetFileName(vdexAddress) + "\" --ignore-crc-error");
            File.Delete("bin\\" + Path.GetFileName(vdexAddress));
            if (File.Exists("bin\\" + Path.GetFileNameWithoutExtension(vdexAddress) + ".apk_classes.dex"))
            {
                if (File.Exists("bin\\classes.dex"))
                    File.Delete("bin\\classes.dex");
                File.Move("bin\\" + Path.GetFileNameWithoutExtension(vdexAddress) + ".apk_classes.dex",
                    "bin\\classes.dex");
            }
            string[] files1 = Directory.GetFiles("bin",
                Path.GetFileNameWithoutExtension(vdexAddress) + ".apk__classes*.dex");
            for (int index = 0; index < files1.Length; ++index)
            {
                if (File.Exists("bin\\classes" + (index + 2) + ".dex"))
                    File.Delete("bin\\classes" + (index + 2) + ".dex");
                File.Move(files1[index], "bin\\classes" + (index + 2) + ".dex");
            }
            if (File.Exists("bin\\" + Path.GetFileNameWithoutExtension(vdexAddress) + "_classes.dex"))
            {
                if (File.Exists("bin\\classes.dex"))
                    File.Delete("bin\\classes.dex");
                File.Move("bin\\" + Path.GetFileNameWithoutExtension(vdexAddress) + "_classes.dex", "bin\\classes.dex");
            }
            string[] files2 =
                Directory.GetFiles("bin", Path.GetFileNameWithoutExtension(vdexAddress) + "_classes*.dex");
            for (int index = 0; index < files2.Length; ++index)
            {
                if (File.Exists("bin\\classes" + (index + 2) + ".dex"))
                    File.Delete("bin\\classes" + (index + 2) + ".dex");
                File.Move(files2[index], "bin\\classes" + (index + 2) + ".dex");
            }
            if (File.Exists("bin\\" + Path.GetFileNameWithoutExtension(vdexAddress) + "_classes.cdex"))
            {
                if (File.Exists("bin\\classes.cdex"))
                    File.Delete("bin\\classes.cdex");
                File.Move("bin\\" + Path.GetFileNameWithoutExtension(vdexAddress) + "_classes.cdex",
                    "bin\\classes.cdex");
            }
            string[] files3 =
                Directory.GetFiles("bin", Path.GetFileNameWithoutExtension(vdexAddress) + "_classes*.cdex");
            for (int index = 0; index < files3.Length; ++index)
            {
                if (File.Exists("bin\\classes" + (index + 2) + ".cdex"))
                    File.Delete("bin\\classes" + (index + 2) + ".cdex");
                File.Move(files3[index], "bin\\classes" + (index + 2) + ".cdex");
            }
            if (File.Exists("bin\\classes.cdex"))
            {
                _mainForm.DebugUpdate("\n==> Extracting classes.cdex");
                CdexToDex("bin\\classes.cdex");
                if (!File.Exists("bin\\classes.dex"))
                {
                    _mainForm.DebugUpdate("\n!!! ERROR: Failed extracting classes.cdex");
                    _mainForm.StatusUpdate("ERROR..");
                    return;
                }
            }
            string[] files4 = Directory.GetFiles("bin", "classes*.cdex");
            for (int index = 0; index < files4.Length; ++index)
            {
                _mainForm.DebugUpdate("\n==> Extracting " + Path.GetFileName(files4[index]));
                CdexToDex(files4[index]);
                if (!File.Exists("bin\\" + Path.GetFileNameWithoutExtension(files4[index]) + ".dex"))
                {
                    _mainForm.DebugUpdate("\n!!! ERROR: Failed extracting " + Path.GetFileName(files4[index]));
                    _mainForm.StatusUpdate("ERROR..");
                    return;
                }
            }
            using (ZipFile zipFile = ZipFile.Read(jarFile))
            {
                string[] files5 = Directory.GetFiles("bin", "classes*.dex");
                for (int index = 0; index < files5.Length; ++index)
                {
                    if (zipFile.ContainsEntry(Path.GetFileName(files5[index])))
                        zipFile.RemoveEntry(Path.GetFileName(files5[index]));
                    zipFile.AddFile(files5[index], "/");
                }
                zipFile.CompressionLevel = CompressionLevel.None;
                zipFile.Save();
            }
            if (File.Exists("bin\\classes.dex"))
            {
                _mainForm.DebugUpdate("\n==> Extracted classes.dex");
                File.Delete("bin\\classes.dex");
            }
            string[] files6 = Directory.GetFiles("bin", "classes*.dex");
            for (int index = 0; index < files6.Length; ++index)
            {
                _mainForm.DebugUpdate("\n==> Extracted " + Path.GetFileName(files6[index]));
                File.Delete("bin\\" + Path.GetFileName(files6[index]));
            }
            JarDecompile(jarFile);
        }

        private void CdexToDex(string cdexAddress)
        {
            if (_adb == null)
            {
                _adb = new Adb();
                _adb.Init(_mainForm);
            }
            if (Directory.Exists("bin") && File.Exists("bin\\compact_dex_converter"))
            {
                _adb.Push("bin\\compact_dex_converter", "/data/local/tmp/", true);
                _adb.Shell("chmod 777 /data/local/tmp/compact_dex_converter", true);
                _adb.Push(cdexAddress, "/data/local/tmp/", true);
                _adb.Shell("/data/local/tmp/compact_dex_converter /data/local/tmp/" + Path.GetFileName(cdexAddress),
                    true);
                _adb.Pull("/data/local/tmp/" + Path.GetFileName(cdexAddress) + ".new", "bin", true);
                _adb.Shell("rm -f /data/local/tmp/compact_dex_converter", true);
                _adb.Shell("rm -f /data/local/tmp/" + Path.GetFileName(cdexAddress), true);
                _adb.Shell("rm -f /data/local/tmp/" + Path.GetFileName(cdexAddress) + ".new", true);
                if (File.Exists(cdexAddress))
                    File.Delete(cdexAddress);
                if (File.Exists("bin\\" + Path.GetFileNameWithoutExtension(cdexAddress) + ".dex"))
                    File.Delete("bin\\" + Path.GetFileNameWithoutExtension(cdexAddress) + ".dex");
                if (!File.Exists("bin\\" + Path.GetFileName(cdexAddress) + ".new"))
                    return;
                File.Move("bin\\" + Path.GetFileName(cdexAddress) + ".new",
                    "bin\\" + Path.GetFileNameWithoutExtension(cdexAddress) + ".dex");
            }
            else
            {
                _mainForm.DebugUpdate("\n!!! ERROR: compact_dex_converter missing..");
                _mainForm.StatusUpdate("ERROR..");
            }
        }

        private void PatchFiles(string targetFile, string targetFilePath)
        {
            string str1 = "";
            string withoutExtension = Path.GetFileNameWithoutExtension(targetFile);
            if (GetPatchStatus("Mock locations") && GetPatchTargetFile("Mock locations") == withoutExtension + ".jar")
            {
                string path = GetPath("com\\android\\server\\LocationManagerService.smali");
                if (File.Exists(path + "com\\android\\server\\LocationManagerService.smali"))
                {
                    using (StreamReader streamReader =
                        File.OpenText(path + "com\\android\\server\\LocationManagerService.smali"))
                    using (StreamWriter streamWriter =
                        new StreamWriter(path + "com\\android\\server\\LocationManagerService.smali.new"))
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
                                _mainForm.DebugUpdate("\n==> Patched mock location boolean");
                                str2 = str2.Substring(0, num - 1) + "0x0\n" + str2.Substring(num + 3);
                            }
                        }
                        streamWriter.Write(str2);
                    }
                    File.Replace(path + "com\\android\\server\\LocationManagerService.smali.new",
                        path + "com\\android\\server\\LocationManagerService.smali", null);
                }
                str1 = GetPath("com\\android\\server\\location\\MockProvider.smali");
                if (File.Exists(str1 + "com\\android\\server\\location\\MockProvider.smali"))
                {
                    using (StreamReader streamReader =
                        File.OpenText(str1 + "com\\android\\server\\location\\MockProvider.smali"))
                    using (StreamWriter streamWriter =
                        new StreamWriter(str1 + "com\\android\\server\\location\\MockProvider.smali.new"))
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
                                _mainForm.DebugUpdate("\n==> Patched mock location boolean");
                                str2 = str2.Substring(0, num - 1) + "0x0\n" + str2.Substring(num + 3);
                            }
                        }
                        streamWriter.Write(str2);
                    }
                    File.Replace(str1 + "com\\android\\server\\location\\MockProvider.smali.new",
                        str1 + "com\\android\\server\\location\\MockProvider.smali", null);
                }
            }
            int result;
            if (GetPatchStatus("Mock providers") && GetPatchTargetFile("Mock providers") == withoutExtension + ".jar")
            {
                str1 = GetPath("com\\android\\server\\LocationManagerService.smali");
                if (File.Exists(str1 + "com\\android\\server\\LocationManagerService.smali"))
                {
                    string str2 = File.ReadAllText(str1 + "com\\android\\server\\LocationManagerService.smali");
                    using (StreamWriter streamWriter =
                        new StreamWriter(str1 + "com\\android\\server\\LocationManagerService.smali.new"))
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
                            while (int.TryParse(str2.Substring(num, 1), out result))
                                ++num;
                            int startIndex = num;
                            while (str2.Substring(startIndex, 11) != ".end method")
                                ++startIndex;
                            str2 = str2.Substring(0, num) + "\n\n    const/4 v0, 0x1\n\n    return v0\n\n" +
                                   str2.Substring(startIndex);
                            _mainForm.DebugUpdate("\n==> Patched mock providers function");
                        }
                        streamWriter.Write(str2);
                    }
                    File.Replace(str1 + "com\\android\\server\\LocationManagerService.smali.new",
                        str1 + "com\\android\\server\\LocationManagerService.smali", null);
                }
            }
            if (GetPatchStatus("GNSS updates") && GetPatchTargetFile("GNSS updates") == withoutExtension + ".jar")
            {
                str1 = GetPath("com\\android\\server\\location\\GnssLocationProvider.smali");
                if (File.Exists(str1 + "com\\android\\server\\location\\GnssLocationProvider.smali"))
                {
                    string str2 = File.ReadAllText(str1 + "com\\android\\server\\location\\GnssLocationProvider.smali");
                    using (StreamWriter streamWriter =
                        new StreamWriter(str1 + "com\\android\\server\\location\\GnssLocationProvider.smali.new"))
                    {
                        if (str2.Contains(".method private reportLocation"))
                        {
                            int num = str2.LastIndexOf(".method private reportLocation(");
                            while (str2.Substring(num, 7) != ".locals" && str2.Substring(num, 10) != ".registers")
                                ++num;
                            if (str2.Substring(num, 7) == ".locals")
                                num += 8;
                            if (str2.Substring(num, 10) == ".registers")
                                num += 11;
                            while (int.TryParse(str2.Substring(num, 1), out result))
                                ++num;
                            int startIndex = num;
                            while (str2.Substring(startIndex, 11) != ".end method")
                                ++startIndex;
                            str2 = str2.Substring(0, num) + "\n\n    return-void\n\n" + str2.Substring(startIndex);
                            _mainForm.DebugUpdate("\n==> Patched gnss updates function");
                        }
                        streamWriter.Write(str2);
                    }
                    File.Replace(str1 + "com\\android\\server\\location\\GnssLocationProvider.smali.new",
                        str1 + "com\\android\\server\\location\\GnssLocationProvider.smali", null);
                }
            }
            if (GetPatchStatus("Secure flag") && GetPatchTargetFile("Secure flag") == withoutExtension + ".jar")
            {
                string path1 = GetPath("com\\android\\server\\wm\\WindowManagerService.smali");
                if (File.Exists(path1 + "com\\android\\server\\wm\\WindowManagerService.smali"))
                {
                    string str2 = File.ReadAllText(path1 + "com\\android\\server\\wm\\WindowManagerService.smali");
                    using (StreamWriter streamWriter =
                        new StreamWriter(path1 + "com\\android\\server\\wm\\WindowManagerService.smali.new"))
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
                            while (int.TryParse(str2.Substring(num, 1), out result))
                                ++num;
                            int startIndex = num;
                            while (str2.Substring(startIndex, 11) != ".end method")
                                ++startIndex;
                            str2 = str2.Substring(0, num) + "\n\n    const/4 v0, 0x0\n\n    return v0\n\n" +
                                   str2.Substring(startIndex);
                            _mainForm.DebugUpdate("\n==> Patched secure flag function");
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
                            _mainForm.DebugUpdate("\n==> Patched screen capture boolean");
                        }
                        streamWriter.Write(str2);
                    }
                    File.Replace(path1 + "com\\android\\server\\wm\\WindowManagerService.smali.new",
                        path1 + "com\\android\\server\\wm\\WindowManagerService.smali", null);
                }
                string path2 = GetPath("com\\android\\server\\wm\\ScreenshotController.smali");
                if (File.Exists(path2 + "com\\android\\server\\wm\\ScreenshotController.smali"))
                {
                    string str2 = File.ReadAllText(path2 + "com\\android\\server\\wm\\ScreenshotController.smali");
                    using (StreamWriter streamWriter =
                        new StreamWriter(path2 + "com\\android\\server\\wm\\ScreenshotController.smali.new"))
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
                            while (int.TryParse(str2.Substring(num, 1), out result))
                                ++num;
                            int startIndex = num;
                            while (str2.Substring(startIndex, 11) != ".end method")
                                ++startIndex;
                            str2 = str2.Substring(0, num) + "\n\n    const/4 v0, 0x0\n\n    return v0\n\n" +
                                   str2.Substring(startIndex);
                            _mainForm.DebugUpdate("\n==> Patched screenshot controller");
                        }
                        streamWriter.Write(str2);
                    }
                    File.Replace(path2 + "com\\android\\server\\wm\\ScreenshotController.smali.new",
                        path2 + "com\\android\\server\\wm\\ScreenshotController.smali", null);
                }
                string path3 = GetPath("com\\android\\server\\wm\\WindowSurfaceController.smali");
                if (File.Exists(path3 + "com\\android\\server\\wm\\WindowSurfaceController.smali"))
                {
                    string str2 = File.ReadAllText(path3 + "com\\android\\server\\wm\\WindowSurfaceController.smali");
                    using (StreamWriter streamWriter =
                        new StreamWriter(path3 + "com\\android\\server\\wm\\WindowSurfaceController.smali.new"))
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
                            while (int.TryParse(str2.Substring(num, 1), out result))
                                ++num;
                            int startIndex = num;
                            while (str2.Substring(startIndex, 11) != ".end method")
                                ++startIndex;
                            str2 = str2.Substring(0, num) + "\n\n    return-void\n\n" + str2.Substring(startIndex);
                            _mainForm.DebugUpdate("\n==> Patched set secure function");
                        }
                        streamWriter.Write(str2);
                    }
                    File.Replace(path3 + "com\\android\\server\\wm\\WindowSurfaceController.smali.new",
                        path3 + "com\\android\\server\\wm\\WindowSurfaceController.smali", null);
                }
                string path4 = GetPath("com\\android\\server\\devicepolicy\\DevicePolicyManagerService.smali");
                if (File.Exists(path4 + "com\\android\\server\\devicepolicy\\DevicePolicyManagerService.smali"))
                {
                    string str2 =
                        File.ReadAllText(path4 +
                                         "com\\android\\server\\devicepolicy\\DevicePolicyManagerService.smali");
                    using (StreamWriter streamWriter =
                        new StreamWriter(path4 +
                                         "com\\android\\server\\devicepolicy\\DevicePolicyManagerService.smali.new"))
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
                            while (int.TryParse(str2.Substring(num, 1), out result))
                                ++num;
                            int startIndex = num;
                            while (str2.Substring(startIndex, 11) != ".end method")
                                ++startIndex;
                            str2 = str2.Substring(0, num) + "\n\n    return-void\n\n" + str2.Substring(startIndex);
                            _mainForm.DebugUpdate("\n==> Patched capture function");
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
                            while (int.TryParse(str2.Substring(num, 1), out result))
                                ++num;
                            int startIndex = num;
                            while (str2.Substring(startIndex, 11) != ".end method")
                                ++startIndex;
                            str2 = str2.Substring(0, num) + "\n\n    const/4 v0, 0x1\n\n    return v0\n\n" +
                                   str2.Substring(startIndex);
                            _mainForm.DebugUpdate("\n==> Patched get capture function");
                        }
                        streamWriter.Write(str2);
                    }
                    File.Replace(path4 + "com\\android\\server\\devicepolicy\\DevicePolicyManagerService.smali.new",
                        path4 + "com\\android\\server\\devicepolicy\\DevicePolicyManagerService.smali", null);
                }
                str1 = GetPath("com\\android\\server\\devicepolicy\\DevicePolicyCacheImpl.smali");
                if (File.Exists(str1 + "com\\android\\server\\devicepolicy\\DevicePolicyCacheImpl.smali"))
                {
                    string str2 =
                        File.ReadAllText(str1 + "com\\android\\server\\devicepolicy\\DevicePolicyCacheImpl.smali");
                    using (StreamWriter streamWriter =
                        new StreamWriter(str1 + "com\\android\\server\\devicepolicy\\DevicePolicyCacheImpl.smali.new"))
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
                            while (int.TryParse(str2.Substring(num, 1), out result))
                                ++num;
                            int startIndex = num;
                            while (str2.Substring(startIndex, 11) != ".end method")
                                ++startIndex;
                            str2 = str2.Substring(0, num) + "\n\n    return-void\n\n" + str2.Substring(startIndex);
                            _mainForm.DebugUpdate("\n==> Patched capture function");
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
                            while (int.TryParse(str2.Substring(num, 1), out result))
                                ++num;
                            int startIndex = num;
                            while (str2.Substring(startIndex, 11) != ".end method")
                                ++startIndex;
                            str2 = str2.Substring(0, num) + "\n\n    const/4 v0, 0x1\n\n    return v0\n\n" +
                                   str2.Substring(startIndex);
                            _mainForm.DebugUpdate("\n==> Patched get capture function");
                        }
                        streamWriter.Write(str2);
                    }
                    File.Replace(str1 + "com\\android\\server\\devicepolicy\\DevicePolicyCacheImpl.smali.new",
                        str1 + "com\\android\\server\\devicepolicy\\DevicePolicyCacheImpl.smali", null);
                }
            }
            if (GetPatchStatus("Signature verification") &&
                GetPatchTargetFile("Signature verification") == withoutExtension + ".jar")
            {
                string path = GetPath("com\\android\\server\\pm\\PackageManagerService.smali");
                if (File.Exists(path + "com\\android\\server\\pm\\PackageManagerService.smali"))
                {
                    string str2 = File.ReadAllText(path + "com\\android\\server\\pm\\PackageManagerService.smali");
                    using (StreamWriter streamWriter =
                        new StreamWriter(path + "com\\android\\server\\pm\\PackageManagerService.smali.new"))
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
                            while (int.TryParse(str2.Substring(num, 1), out result))
                                ++num;
                            int startIndex = num;
                            while (str2.Substring(startIndex, 11) != ".end method")
                                ++startIndex;
                            str2 = str2.Substring(0, num) + "\n\n    const/4 v0, 0x0\n\n    return v0\n\n" +
                                   str2.Substring(startIndex);
                            _mainForm.DebugUpdate("\n==> Patched signature verification function");
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
                            while (int.TryParse(str2.Substring(num, 1), out result))
                                ++num;
                            int startIndex = num;
                            while (str2.Substring(startIndex, 11) != ".end method")
                                ++startIndex;
                            str2 = str2.Substring(0, num) + "\n\n    const/4 v0, 0x0\n\n    return v0\n\n" +
                                   str2.Substring(startIndex);
                            _mainForm.DebugUpdate("\n==> Patched signature verification function");
                        }
                        streamWriter.Write(str2);
                    }
                    File.Replace(path + "com\\android\\server\\pm\\PackageManagerService.smali.new",
                        path + "com\\android\\server\\pm\\PackageManagerService.smali", null);
                }
                str1 = GetPath("com\\android\\server\\pm\\PackageManagerServiceUtils.smali");
                if (File.Exists(str1 + "com\\android\\server\\pm\\PackageManagerServiceUtils.smali"))
                {
                    string str2 = File.ReadAllText(str1 + "com\\android\\server\\pm\\PackageManagerServiceUtils.smali");
                    using (StreamWriter streamWriter =
                        new StreamWriter(str1 + "com\\android\\server\\pm\\PackageManagerServiceUtils.smali.new"))
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
                            while (int.TryParse(str2.Substring(num, 1), out result))
                                ++num;
                            int startIndex = num;
                            while (str2.Substring(startIndex, 11) != ".end method")
                                ++startIndex;
                            str2 = str2.Substring(0, num) + "\n\n    const/4 v0, 0x0\n\n    return v0\n\n" +
                                   str2.Substring(startIndex);
                            _mainForm.DebugUpdate("\n==> Patched signature verification util function");
                        }
                        streamWriter.Write(str2);
                    }
                    File.Replace(str1 + "com\\android\\server\\pm\\PackageManagerServiceUtils.smali.new",
                        str1 + "com\\android\\server\\pm\\PackageManagerServiceUtils.smali", null);
                }
            }
            if (GetPatchStatus("Signature spoofing") &&
                GetPatchTargetFile("Signature spoofing") == withoutExtension + ".jar")
            {
                str1 = GetPath("com\\android\\server\\pm\\PackageManagerService.smali");
                if (File.Exists(str1 + "com\\android\\server\\pm\\PackageManagerService.smali"))
                {
                    string str2 = File.ReadAllText(str1 + "com\\android\\server\\pm\\PackageManagerService.smali");
                    if (str2.Contains(".method private generatePackageInfo(") ||
                        str2.Contains(".method generatePackageInfo("))
                    {
                        int startIndex1 = str2.LastIndexOf(".method private generatePackageInfo(");
                        if (startIndex1 == -1)
                            startIndex1 = str2.LastIndexOf(".method generatePackageInfo(");
                        int startIndex2 = startIndex1;
                        while (str2.Substring(startIndex2, 2) != ";\r")
                            startIndex2 += 2;
                        int num = startIndex2 + 1;
                        if (str2.Substring(startIndex1, num - startIndex1).Contains("PackageParser"))
                            _dexPatcherTarget = "bin/sigspoof_4.1-6.0.dex";
                        else if (str2.Substring(startIndex1, num - startIndex1).Contains("PackageSetting"))
                            _dexPatcherTarget = "bin/sigspoof_7.0-9.0.dex";
                    }
                }
                else if (!File.Exists(str1 + "com\\android\\server\\pm\\PackageManagerService.smali"))
                {
                    _mainForm.DebugUpdate("\n!!! ERROR: Signature spoof class not found.");
                    _mainForm.StatusUpdate("ERROR..");
                    return;
                }
                if (!File.Exists(GetPath("com\\android\\server\\pm\\GeneratePackageInfoHook.smali") +
                                 "com\\android\\server\\pm\\GeneratePackageInfoHook.smali"))
                    _dexPatcherCoreRequired = true;
            }
            if (GetPatchStatus("Recovery reboot") && GetPatchTargetFile("Recovery reboot") == withoutExtension + ".jar")
            {
                string path = GetPath("com\\android\\server\\statusbar\\StatusBarManagerService.smali");
                if (File.Exists(path + "com\\android\\server\\statusbar\\StatusBarManagerService.smali"))
                {
                    string str2 =
                        File.ReadAllText(path + "com\\android\\server\\statusbar\\StatusBarManagerService.smali");
                    using (StreamWriter streamWriter =
                        new StreamWriter(path + "com\\android\\server\\statusbar\\StatusBarManagerService.smali.new"))
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
                            _mainForm.DebugUpdate("\n==> Patched recovery reboot function");
                        }
                        streamWriter.Write(str2);
                    }
                    File.Replace(path + "com\\android\\server\\statusbar\\StatusBarManagerService.smali.new",
                        path + "com\\android\\server\\statusbar\\StatusBarManagerService.smali", null);
                    str1 = GetPath("com\\android\\server\\wm\\WindowManagerService.smali");
                    if (File.Exists(str1 + "com\\android\\server\\wm\\WindowManagerService.smali"))
                    {
                        string str3 = File.ReadAllText(str1 + "com\\android\\server\\wm\\WindowManagerService.smali");
                        using (StreamWriter streamWriter =
                            new StreamWriter(str1 + "com\\android\\server\\wm\\WindowManagerService.smali.new"))
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
                                _mainForm.DebugUpdate("\n==> Patched recovery reboot function");
                            }
                            streamWriter.Write(str3);
                        }
                        File.Replace(str1 + "com\\android\\server\\wm\\WindowManagerService.smali.new",
                            str1 + "com\\android\\server\\wm\\WindowManagerService.smali", null);
                    }
                    else
                    {
                        _mainForm.DebugUpdate("\n!!! ERROR: Reboot behaviour window class not found.");
                        _mainForm.StatusUpdate("ERROR..");
                        return;
                    }
                }
                else
                {
                    _mainForm.DebugUpdate("\n!!! ERROR: Reboot behaviour status class not found.");
                    _mainForm.StatusUpdate("ERROR..");
                    return;
                }
            }
            if (GetPatchStatus("Samsung Knox") && GetPatchTargetFile("Samsung Knox") == withoutExtension + ".jar")
            {
                string path1 = GetPath("com\\android\\server\\KnoxFileHandler.smali");
                if (File.Exists(path1 + "com\\android\\server\\KnoxFileHandler.smali"))
                {
                    string str2 = File.ReadAllText(path1 + "com\\android\\server\\KnoxFileHandler.smali");
                    using (StreamWriter streamWriter =
                        new StreamWriter(path1 + "com\\android\\server\\KnoxFileHandler.smali.new"))
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
                            while (int.TryParse(str2.Substring(num, 1), out result))
                                ++num;
                            int startIndex = num;
                            while (str2.Substring(startIndex, 11) != ".end method")
                                ++startIndex;
                            str2 = str2.Substring(0, num) + "\n\n    const/4 v0, 0x1\n\n    return v0\n\n" +
                                   str2.Substring(startIndex);
                            _mainForm.DebugUpdate("\n==> Patched knox function");
                        }
                        streamWriter.Write(str2);
                    }
                    File.Replace(path1 + "com\\android\\server\\KnoxFileHandler.smali.new",
                        path1 + "com\\android\\server\\KnoxFileHandler.smali", null);
                    string path2 = GetPath("com\\android\\server\\pm\\PersonaServiceHelper.smali");
                    if (File.Exists(path2 + "com\\android\\server\\pm\\PersonaServiceHelper.smali"))
                    {
                        string str3 = File.ReadAllText(path2 + "com\\android\\server\\pm\\PersonaServiceHelper.smali");
                        using (StreamWriter streamWriter =
                            new StreamWriter(path2 + "com\\android\\server\\pm\\PersonaServiceHelper.smali.new"))
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
                                while (int.TryParse(str3.Substring(num, 1), out result))
                                    ++num;
                                int startIndex = num;
                                while (str3.Substring(startIndex, 11) != ".end method")
                                    ++startIndex;
                                str3 = str3.Substring(0, num) + "\n\n    const/4 v0, 0x1\n\n    return v0\n\n" +
                                       str3.Substring(startIndex);
                                _mainForm.DebugUpdate("\n==> Patched knox function");
                            }
                            streamWriter.Write(str3);
                        }
                        File.Replace(path2 + "com\\android\\server\\pm\\PersonaServiceHelper.smali.new",
                            path2 + "com\\android\\server\\pm\\PersonaServiceHelper.smali", null);
                        str1 = GetPath("com\\android\\server\\pm\\TimaHelper.smali");
                        if (File.Exists(str1 + "com\\android\\server\\pm\\TimaHelper.smali"))
                        {
                            string str4 = File.ReadAllText(str1 + "com\\android\\server\\pm\\TimaHelper.smali");
                            using (StreamWriter streamWriter =
                                new StreamWriter(str1 + "com\\android\\server\\pm\\TimaHelper.smali.new"))
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
                                    while (int.TryParse(str4.Substring(num, 1), out result))
                                        ++num;
                                    int startIndex = num;
                                    while (str4.Substring(startIndex, 11) != ".end method")
                                        ++startIndex;
                                    str4 = str4.Substring(0, num) + "\n\n    const/4 v0, 0x1\n\n    return v0\n\n" +
                                           str4.Substring(startIndex);
                                    _mainForm.DebugUpdate("\n==> Patched knox function");
                                }
                                streamWriter.Write(str4);
                            }
                            File.Replace(str1 + "com\\android\\server\\pm\\TimaHelper.smali.new",
                                str1 + "com\\android\\server\\pm\\TimaHelper.smali", null);
                        }
                        else
                        {
                            _mainForm.DebugUpdate("\n!!! ERROR: Knox patch class not found.");
                            _mainForm.StatusUpdate("ERROR..");
                            return;
                        }
                    }
                    else
                    {
                        _mainForm.DebugUpdate("\n!!! ERROR: Knox patch class not found.");
                        _mainForm.StatusUpdate("ERROR..");
                        return;
                    }
                }
                else
                {
                    _mainForm.DebugUpdate("\n!!! ERROR: Knox patch class not found.");
                    _mainForm.StatusUpdate("ERROR..");
                    return;
                }
            }
            if (GetPatchStatus("High volume warning") &&
                GetPatchTargetFile("High volume warning") == withoutExtension + ".jar")
            {
                str1 = GetPath("com\\android\\server\\audio\\AudioService.smali");
                if (File.Exists(str1 + "com\\android\\server\\audio\\AudioService.smali"))
                {
                    string str2 = File.ReadAllText(str1 + "com\\android\\server\\audio\\AudioService.smali");
                    using (StreamWriter streamWriter =
                        new StreamWriter(str1 + "com\\android\\server\\audio\\AudioService.smali.new"))
                    {
                        if (str2.Contains(".method private checkSafeMediaVolume("))
                        {
                            int num = str2.LastIndexOf(".method private checkSafeMediaVolume(");
                            while (str2.Substring(num, 7) != ".locals" && str2.Substring(num, 10) != ".registers")
                                ++num;
                            if (str2.Substring(num, 7) == ".locals")
                                num += 8;
                            if (str2.Substring(num, 10) == ".registers")
                                num += 11;
                            while (int.TryParse(str2.Substring(num, 1), out result))
                                ++num;
                            int startIndex = num;
                            while (str2.Substring(startIndex, 11) != ".end method")
                                ++startIndex;
                            str2 = str2.Substring(0, num) + "\n\n    const/4 v0, 0x1\n\n    return v0\n\n" +
                                   str2.Substring(startIndex);
                            _mainForm.DebugUpdate("\n==> Patched high volume warning function");
                        }
                        streamWriter.Write(str2);
                    }
                    File.Replace(str1 + "com\\android\\server\\audio\\AudioService.smali.new",
                        str1 + "com\\android\\server\\audio\\AudioService.smali", null);
                }
                else
                {
                    _mainForm.DebugUpdate("\n!!! ERROR: High volume warning class not found.");
                    _mainForm.StatusUpdate("ERROR..");
                    return;
                }
            }
            if (GetPatchStatus("Mock locations V2") &&
                GetPatchTargetFile("Mock locations V2") == withoutExtension + ".jar")
            {
                str1 = GetPath("android\\location\\Location.smali");
                if (File.Exists(str1 + "android\\location\\Location.smali"))
                {
                    string str2 = File.ReadAllText(str1 + "android\\location\\Location.smali");
                    using (StreamWriter streamWriter = new StreamWriter(str1 + "android\\location\\Location.smali.new"))
                    {
                        if (str2.Contains(".method public isFromMockProvider("))
                        {
                            int num = str2.LastIndexOf(".method public isFromMockProvider(");
                            while (str2.Substring(num, 7) != ".locals" && str2.Substring(num, 10) != ".registers")
                                ++num;
                            if (str2.Substring(num, 7) == ".locals")
                                num += 8;
                            if (str2.Substring(num, 10) == ".registers")
                                num += 11;
                            while (int.TryParse(str2.Substring(num, 1), out result))
                                ++num;
                            int startIndex = num;
                            while (str2.Substring(startIndex, 11) != ".end method")
                                ++startIndex;
                            str2 = str2.Substring(0, num) + "\n\n    const/4 v0, 0x0\n\n    return v0\n\n" +
                                   str2.Substring(startIndex);
                            _mainForm.DebugUpdate("\n==> Patched mock location function");
                        }
                        if (str2.Contains(".method public setIsFromMockProvider("))
                        {
                            int num = str2.LastIndexOf(".method public setIsFromMockProvider(");
                            while (str2.Substring(num, 7) != ".locals" && str2.Substring(num, 10) != ".registers")
                                ++num;
                            if (str2.Substring(num, 7) == ".locals")
                                num += 8;
                            if (str2.Substring(num, 10) == ".registers")
                                num += 11;
                            while (int.TryParse(str2.Substring(num, 1), out result))
                                ++num;
                            int startIndex = num;
                            while (str2.Substring(startIndex, 11) != ".end method")
                                ++startIndex;
                            str2 = str2.Substring(0, num) + "\n\n    return-void\n\n" + str2.Substring(startIndex);
                            _mainForm.DebugUpdate("\n==> Patched mock location function");
                        }
                        streamWriter.Write(str2);
                    }
                    File.Replace(str1 + "android\\location\\Location.smali.new",
                        str1 + "android\\location\\Location.smali", null);
                }
            }
            if (str1 == "")
                str1 = GetPath("");
            if (str1.Contains("tmp"))
                JarCompile("tmp\\dist\\" + withoutExtension + ".jar", "tmp");
            else
            {
                if (!(str1 == "smali\\"))
                    return;
                OdexCompile(withoutExtension, Path.GetDirectoryName(targetFilePath));
            }
        }

        private bool GetPatchStatus(string patchTitle)
        {
            Patches = _mainForm.Patches;
            return Patches.Find(x => x.PatchTitle.Equals(patchTitle)).Status;
        }

        private string GetPatchTargetFile(string patchTitle)
        {
            Patches = _mainForm.Patches;
            return Patches.Find(x => x.PatchTitle.Equals(patchTitle)).TargetFile;
        }

        private string GetPath(string file)
        {
            string str = "";
            if (file == "" || file == null)
            {
                if (Directory.Exists("tmp\\smali"))
                    str = "tmp\\smali\\";
                else if (Directory.Exists("smali"))
                    str = "smali\\";
            }
            else if (File.Exists("tmp\\" + file))
                str = "tmp\\";
            else if (File.Exists("tmp\\smali\\" + file))
                str = "tmp\\smali\\";
            else if (File.Exists("tmp\\smali_classes2\\" + file))
                str = "tmp\\smali_classes2\\";
            else if (File.Exists("tmp\\smali_classes3\\" + file))
                str = "tmp\\smali_classes3\\";
            else if (File.Exists("tmp\\smali_classes4\\" + file))
                str = "tmp\\smali_classes4\\";
            else if (File.Exists("smali\\" + file))
                str = "smali\\";
            return str;
        }

        private void StartProcess(string exe, string args)
        {
            try
            {
                using (Process process = Process.Start(new ProcessStartInfo
                {
                    Arguments = args,
                    FileName = exe,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }))
                    while (!process.HasExited)
                    {
                        string str;
                        if ((str = process.StandardOutput.ReadLine()) != null ||
                            (str = process.StandardError.ReadLine()) != null)
                            _mainForm.DebugUpdate("\n" + str);
                    }
            }
            catch (Exception ex)
            {
                _mainForm.DebugUpdate("\n!!! ERROR: " + ex.Message);
                _mainForm.StatusUpdate("ERROR..");
            }
        }
    }
}