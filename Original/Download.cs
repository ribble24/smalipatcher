using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;

namespace SmaliPatcher
{
    public class Download
    {
        private double _adbBytesIn;
        private double _adbTotalBytes;
        private double _apktoolBytesIn;
        private double _apktoolTotalBytes;
        private string _apktoolUrl;
        private double _baksmaliBytesIn;
        private double _baksmaliTotalBytes;
        private string _baksmaliUrl;
        private double _cdexBytesIn;
        private double _cdexTotalBytes;
        private double _dexPatcherBytesIn;
        private double _dexPatcherTotalBytes;
        private Magisk _magisk;
        private MainForm _mainForm;
        private double _moduleInstallerBytesIn;
        private double _moduleInstallerTotalBytes;
        private double _smaliBytesIn;
        private double _smaliTotalBytes;
        private string _smaliUrl;
        private double _vdexBytesIn;
        private double _vdexTotalBytes;

        public void Init(object sender)
        {
            ServicePointManager.DefaultConnectionLimit = 5;
            ServicePointManager.ServerCertificateValidationCallback = (param1, param2, param3, param4) => true;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            if (_mainForm == null)
                _mainForm = (MainForm) sender;
            if (_magisk != null)
                return;
            _magisk = new Magisk();
            _magisk.Init(_mainForm);
        }

        public void DownloadBinary()
        {
            if (!Directory.Exists("bin"))
                Directory.CreateDirectory("bin");
            if (Directory.Exists("tmp"))
                Directory.Delete("tmp", true);
            if ((!File.Exists("bin\\apktool.jar") || !File.Exists("bin\\baksmali.jar") ||
                 !File.Exists("bin\\smali.jar") || new FileInfo("bin\\apktool.jar").Length == 0L ||
                 new FileInfo("bin\\baksmali.jar").Length == 0L || new FileInfo("bin\\smali.jar").Length == 0L) &&
                (_apktoolUrl == null || _smaliUrl == null || _baksmaliUrl == null))
                GetUrls();
            if (!File.Exists("bin\\apktool.jar") || new FileInfo("bin\\apktool.jar").Length == 0L)
            {
                _mainForm.DisableControls();
                _apktoolBytesIn = 0.0;
                _apktoolTotalBytes = 1.0;
                WebClient webClient = new WebClient();
                webClient.DownloadProgressChanged +=
                    (DownloadProgressChangedEventHandler) ((sender, e) => ProgressChanged(sender, e, "apktool"));
                webClient.DownloadFileCompleted +=
                    (AsyncCompletedEventHandler) ((sender, e) => FileCompleted(sender, e, "apktool"));
                webClient.DownloadFileAsync(new Uri(_apktoolUrl), "bin\\apktool.jar");
            }
            if (!File.Exists("bin\\baksmali.jar") || new FileInfo("bin\\baksmali.jar").Length == 0L)
            {
                _mainForm.DisableControls();
                _baksmaliBytesIn = 0.0;
                _baksmaliTotalBytes = 1.0;
                WebClient webClient = new WebClient();
                webClient.DownloadProgressChanged +=
                    (DownloadProgressChangedEventHandler) ((sender, e) => ProgressChanged(sender, e, "baksmali"));
                webClient.DownloadFileCompleted +=
                    (AsyncCompletedEventHandler) ((sender, e) => FileCompleted(sender, e, "baksmali"));
                webClient.DownloadFileAsync(new Uri(_baksmaliUrl), "bin\\baksmali.jar");
            }
            if (!File.Exists("bin\\smali.jar") || new FileInfo("bin\\smali.jar").Length == 0L)
            {
                _mainForm.DisableControls();
                _smaliBytesIn = 0.0;
                _smaliTotalBytes = 1.0;
                WebClient webClient = new WebClient();
                webClient.DownloadProgressChanged +=
                    (DownloadProgressChangedEventHandler) ((sender, e) => ProgressChanged(sender, e, "smali"));
                webClient.DownloadFileCompleted +=
                    (AsyncCompletedEventHandler) ((sender, e) => FileCompleted(sender, e, "smali"));
                webClient.DownloadFileAsync(new Uri(_smaliUrl), "bin\\smali.jar");
            }
            if (!File.Exists("bin\\dexpatcher.jar") || !File.Exists("bin\\sigspoof_core.dex") ||
                !File.Exists("bin\\sigspoof_4.1-6.0.dex") || !File.Exists("bin\\sigspoof_7.0-9.0.dex") ||
                new FileInfo("bin\\dexpatcher.jar").Length == 0L ||
                new FileInfo("bin\\sigspoof_core.dex").Length == 0L ||
                new FileInfo("bin\\sigspoof_4.1-6.0.dex").Length == 0L ||
                new FileInfo("bin\\sigspoof_7.0-9.0.dex").Length == 0L)
            {
                if (!Directory.Exists("tmp"))
                    Directory.CreateDirectory("tmp");
                _mainForm.DisableControls();
                _dexPatcherBytesIn = 0.0;
                _dexPatcherTotalBytes = 1.0;
                WebClient webClient = new WebClient();
                webClient.DownloadProgressChanged +=
                    (DownloadProgressChangedEventHandler) ((sender, e) => ProgressChanged(sender, e, "dexpatcher"));
                webClient.DownloadFileCompleted +=
                    (AsyncCompletedEventHandler) ((sender, e) => FileCompleted(sender, e, "dexpatcher"));
                webClient.DownloadFileAsync(new Uri("https://github.com/fOmey/dexpatcher/raw/master/dexpatcher.zip"),
                    "tmp\\dexPatcher.zip");
            }
            if (!File.Exists("bin\\adb.exe") || !File.Exists("bin\\AdbWinApi.dll") ||
                !File.Exists("bin\\AdbWinUsbApi.dll") || new FileInfo("bin\\adb.exe").Length == 0L ||
                new FileInfo("bin\\AdbWinApi.dll").Length == 0L || new FileInfo("bin\\AdbWinUsbApi.dll").Length == 0L)
            {
                if (!Directory.Exists("tmp"))
                    Directory.CreateDirectory("tmp");
                _mainForm.DisableControls();
                _adbBytesIn = 0.0;
                _adbTotalBytes = 1.0;
                WebClient webClient = new WebClient();
                webClient.DownloadProgressChanged +=
                    (DownloadProgressChangedEventHandler) ((sender, e) => ProgressChanged(sender, e, "adb"));
                webClient.DownloadFileCompleted +=
                    (AsyncCompletedEventHandler) ((sender, e) => FileCompleted(sender, e, "adb"));
                webClient.DownloadFileAsync(
                    new Uri("http://dl.google.com/android/repository/platform-tools-latest-windows.zip"),
                    "tmp\\platform-tools.zip");
            }
            if (!File.Exists("bin\\vdexExtractor.exe") || !File.Exists("bin\\cygz.dll") ||
                !File.Exists("bin\\cygwin1.dll") || !File.Exists("bin\\cyggcc_s-seh-1.dll") ||
                new FileInfo("bin\\vdexExtractor.exe").Length == 0L || new FileInfo("bin\\cygz.dll").Length == 0L ||
                new FileInfo("bin\\cygwin1.dll").Length == 0L || new FileInfo("bin\\cyggcc_s-seh-1.dll").Length == 0L)
            {
                if (!Directory.Exists("tmp"))
                    Directory.CreateDirectory("tmp");
                _mainForm.DisableControls();
                _vdexBytesIn = 0.0;
                _vdexTotalBytes = 1.0;
                WebClient webClient = new WebClient();
                webClient.DownloadProgressChanged +=
                    (DownloadProgressChangedEventHandler) ((sender, e) => ProgressChanged(sender, e, "vdex"));
                webClient.DownloadFileCompleted +=
                    (AsyncCompletedEventHandler) ((sender, e) => FileCompleted(sender, e, "vdex"));
                webClient.DownloadFileAsync(
                    new Uri("https://github.com/fOmey/vdexExtractor/raw/master/bin/vdexExtractor_x86_64.zip"),
                    "tmp\\vdexExtractor.zip");
            }
            if (File.Exists("bin\\compact_dex_converter"))
                return;
            if (!Directory.Exists("tmp"))
                Directory.CreateDirectory("tmp");
            _mainForm.DisableControls();
            _cdexBytesIn = 0.0;
            _cdexTotalBytes = 1.0;
            WebClient webClient1 = new WebClient();
            webClient1.DownloadProgressChanged +=
                (DownloadProgressChangedEventHandler) ((sender, e) => ProgressChanged(sender, e, "cdex"));
            webClient1.DownloadFileCompleted +=
                (AsyncCompletedEventHandler) ((sender, e) => FileCompleted(sender, e, "cdex"));
            webClient1.DownloadFileAsync(
                new Uri(
                    "https://github.com/fOmey/compact_dex_converter/raw/master/compact_dex_converter_android_arm64-v8a.zip"),
                "tmp\\compact_dex_converter.zip");
        }

        public void DownloadMagisk()
        {
            if (!Directory.Exists("tmp"))
                Directory.CreateDirectory("tmp");
            if (File.Exists("tmp\\module_installer.sh") && new FileInfo("tmp\\module_installer.sh").Length != 0L)
                return;
            _mainForm.DisableControls();
            _moduleInstallerBytesIn = 0.0;
            _moduleInstallerTotalBytes = 1.0;
            WebClient webClient = new WebClient();
            webClient.DownloadProgressChanged +=
                (DownloadProgressChangedEventHandler) ((sender, e) => ProgressChanged(sender, e, "moduleInstaller"));
            webClient.DownloadFileCompleted +=
                (AsyncCompletedEventHandler) ((sender, e) => FileCompleted(sender, e, "moduleInstaller"));
            webClient.DownloadFileAsync(
                new Uri("https://raw.githubusercontent.com/topjohnwu/Magisk/master/scripts/module_installer.sh"),
                "tmp\\module_installer.sh");
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e, string file)
        {
            switch (file)
            {
                case "adb":
                    _adbBytesIn = double.Parse(e.BytesReceived.ToString());
                    _adbTotalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                    break;
                case "apktool":
                    _apktoolBytesIn = double.Parse(e.BytesReceived.ToString());
                    _apktoolTotalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                    break;
                case "baksmali":
                    _baksmaliBytesIn = double.Parse(e.BytesReceived.ToString());
                    _baksmaliTotalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                    break;
                case "cdex":
                    _cdexBytesIn = double.Parse(e.BytesReceived.ToString());
                    _cdexTotalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                    break;
                case "dexpatcher":
                    _dexPatcherBytesIn = double.Parse(e.BytesReceived.ToString());
                    _dexPatcherTotalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                    break;
                case "moduleInstaller":
                    _moduleInstallerBytesIn = double.Parse(e.BytesReceived.ToString());
                    _moduleInstallerTotalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                    break;
                case "smali":
                    _smaliBytesIn = double.Parse(e.BytesReceived.ToString());
                    _smaliTotalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                    break;
                case "vdex":
                    _vdexBytesIn = double.Parse(e.BytesReceived.ToString());
                    _vdexTotalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                    break;
            }
            double num1 = _apktoolBytesIn + _baksmaliBytesIn + _smaliBytesIn + _dexPatcherBytesIn + _adbBytesIn +
                          _vdexBytesIn + _cdexBytesIn + _moduleInstallerBytesIn;
            double num2 = _apktoolTotalBytes + _baksmaliTotalBytes + _smaliTotalBytes + _dexPatcherTotalBytes +
                          _adbTotalBytes + _vdexTotalBytes + _cdexTotalBytes + _moduleInstallerTotalBytes;
            double d = num1 / num2 * 100.0;
            if (num1 > num2)
                return;
            if (file == "moduleInstaller")
                _mainForm.StatusUpdate("Magisk Template Download: " + int.Parse(Math.Truncate(d).ToString()) + "%");
            else
                _mainForm.StatusUpdate("Binary Download: " + int.Parse(Math.Truncate(d).ToString()) + "%");
        }

        private void FileCompleted(object sender, AsyncCompletedEventArgs e, string file)
        {
            if (file == "moduleInstaller")
            {
                _mainForm.EnableControls();
                if (_moduleInstallerBytesIn < _moduleInstallerTotalBytes)
                    return;
                _mainForm.DebugUpdate("\n==> Magisk template download complete");
                _magisk.Generate();
            }
            else
            {
                if (_apktoolBytesIn < _apktoolTotalBytes || _baksmaliBytesIn < _baksmaliTotalBytes ||
                    _smaliBytesIn < _smaliTotalBytes || _dexPatcherBytesIn < _dexPatcherTotalBytes ||
                    _adbBytesIn < _adbTotalBytes || _vdexBytesIn < _vdexTotalBytes || _cdexBytesIn < _cdexTotalBytes)
                    return;
                _mainForm.EnableControls();
                _mainForm.DebugUpdate("\n==> Binary download complete");
                UnpackBinarys();
            }
        }

        private void GetUrls()
        {
            _mainForm.StatusUpdate("Fetching latest binary releases..");
            using (WebClient webClient = new WebClient())
            {
                string str = webClient.DownloadString("https://bitbucket.org/iBotPeaches/apktool/downloads/");
                int startIndex1 = str.IndexOf("/iBotPeaches/apktool/downloads/apktool_");
                int startIndex2 = startIndex1;
                while (str.Substring(startIndex2, 3) != "jar")
                    ++startIndex2;
                _apktoolUrl = "https://bitbucket.org" + str.Substring(startIndex1, startIndex2 + 3 - startIndex1);
            }
            using (WebClient webClient = new WebClient())
            {
                string str = webClient.DownloadString("https://bitbucket.org/JesusFreke/smali/downloads/");
                int startIndex1 = str.IndexOf("/JesusFreke/smali/downloads/smali-");
                int startIndex2 = startIndex1;
                while (str.Substring(startIndex2, 3) != "jar")
                    ++startIndex2;
                _smaliUrl = "https://bitbucket.org" + str.Substring(startIndex1, startIndex2 + 3 - startIndex1);
                int startIndex3 = str.IndexOf("/JesusFreke/smali/downloads/baksmali-");
                int startIndex4 = startIndex3;
                while (str.Substring(startIndex4, 3) != "jar")
                    ++startIndex4;
                _baksmaliUrl = "https://bitbucket.org" + str.Substring(startIndex3, startIndex4 + 3 - startIndex3);
            }
        }

        private void UnpackBinarys()
        {
            bool flag = false;
            if (Directory.Exists("tmp\\platform-tools"))
                Directory.Delete("tmp\\platform-tools", true);
            if (!Directory.Exists("tmp\\platform-tools") && File.Exists("tmp\\platform-tools.zip") &&
                new FileInfo("tmp\\platform-tools.zip").Length > 0L)
            {
                _mainForm.StatusUpdate("Unpacking adb..");
                ZipFile.ExtractToDirectory("tmp\\platform-tools.zip", "tmp");
                if (Directory.GetDirectories("tmp", "platform-tools").Length != 0)
                {
                    string[] files = Directory.GetFiles("tmp\\platform-tools", "adb*");
                    for (int index = 0; index < files.Length; ++index)
                    {
                        if (File.Exists("bin\\" + Path.GetFileName(files[index])))
                            File.Delete("bin\\" + Path.GetFileName(files[index]));
                        File.Move(files[index], "bin\\" + Path.GetFileName(files[index]));
                    }
                }
                flag = true;
            }
            if (File.Exists("tmp\\dexPatcher.zip") && new FileInfo("tmp\\dexPatcher.zip").Length > 0L)
            {
                _mainForm.StatusUpdate("Unpacking dexPatcher..");
                Unpack("tmp\\dexPatcher.zip");
                flag = true;
            }
            if (File.Exists("tmp\\vdexExtractor.zip") && new FileInfo("tmp\\vdexExtractor.zip").Length > 0L)
            {
                _mainForm.StatusUpdate("Unpacking vdexExtractor..");
                Unpack("tmp\\vdexExtractor.zip");
                flag = true;
            }
            if (File.Exists("tmp\\compact_dex_converter.zip") && new FileInfo("tmp\\compact_dex_converter.zip").Length > 0L)
            {
                _mainForm.StatusUpdate("Unpacking cdexConverter..");
                Unpack("tmp\\compact_dex_converter.zip");
                flag = true;
            }
            if (flag)
            {
                //Directory.Delete("tmp", true);
                _mainForm.DebugUpdate("\n==> Unpacked binarys");
            }
            _mainForm.StatusUpdate("Idle..");
        }

        private void Unpack(string zip)
        {
            using ZipArchive file = ZipFile.OpenRead(zip);
            foreach (ZipArchiveEntry entry in file.Entries)
            {
                if (File.Exists("bin\\" + entry))
                    File.Delete("bin\\" + entry);
            }
            file.ExtractToDirectory("bin");
        }
    }
}