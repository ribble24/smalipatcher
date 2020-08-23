using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Ionic.Zip;
using Ionic.Zlib;

namespace SmaliPatcher
{
    internal class Magisk
    {
        private readonly string _installSh0 = "SKIPMOUNT=false\r\nPROPFILE=false";
        private readonly string _installSh1 = "LATESTARTSERVICE=false\r\n\r\nREPLACE = \"";

        private readonly string _installSh2 =
            "\"\r\n\r\nprint_modname() {\r\n   ui_print \"*******************************\"\r\n   ui_print \"          Smali Patcher        \"\r\n   ui_print \"           fOmey @ XDA         \"\r\n   ui_print \"*******************************\"\r\n}\r\n\r\non_install() {\r\n   ui_print \"- Extracting module files\"\r\n   unzip -o \"$ZIPFILE\" 'system/*' -d $MODPATH >&2\r\n}\r\n\r\nset_permissions() {\r\n  set_perm_recursive $MODPATH 0 0 0755 0644\r\n}";

        private MainForm _mainForm;

        private readonly string _moduleProp = "id=fomey.smalipatcher\r\nname=Smali Patcher\r\nversion=v" +
                                             Application.ProductVersion +
                                             "\r\nversionCode=1\r\nauthor=fOmeyyy\r\ndescription=Collection of framework patches.";

        private readonly string _postFsData = "MODDIR=${0%/*}\r\nresetprop --delete ro.config.iccc_version";

        public void Init(object sender)
        {
            if (_mainForm != null)
                return;
            _mainForm = (MainForm) sender;
        }

        public bool Generate()
        {
            if (Directory.Exists("tmp\\magisk"))
                Directory.Delete("tmp\\magisk", true);
            if (!Directory.Exists("tmp\\magisk"))
            {
                string str1 = "";
                Directory.CreateDirectory("tmp\\magisk\\META-INF\\com\\google\\android");
                Directory.CreateDirectory("tmp\\magisk\\system\\framework");
                Directory.CreateDirectory("tmp\\magisk\\system\\framework\\arm");
                Directory.CreateDirectory("tmp\\magisk\\system\\framework\\arm64");
                Directory.CreateDirectory("tmp\\magisk\\system\\framework\\oat\\arm");
                Directory.CreateDirectory("tmp\\magisk\\system\\framework\\oat\\arm64");
                if (File.Exists("apk\\services.jar"))
                {
                    File.Move("apk\\services.jar", "tmp\\magisk\\system\\framework\\services.jar");
                    File.Create("tmp\\magisk\\system\\framework\\services.odex").Dispose();
                    File.Create("tmp\\magisk\\system\\framework\\arm\\services.odex").Dispose();
                    File.Create("tmp\\magisk\\system\\framework\\arm64\\services.odex").Dispose();
                    File.Create("tmp\\magisk\\system\\framework\\oat\\arm\\services.odex").Dispose();
                    File.Create("tmp\\magisk\\system\\framework\\oat\\arm64\\services.odex").Dispose();
                    File.Create("tmp\\magisk\\system\\framework\\oat\\arm64\\services.vdex").Dispose();
                    str1 = str1 + "/system/framework/services.jar\n" + "/system/framework/services.odex\n" +
                           "/system/framework/arm/services.odex\n" + "/system/framework/arm64/services.odex\n" +
                           "/system/framework/oat/arm/services.odex\n" + "/system/framework/oat/arm64/services.odex\n" +
                           "/system/framework/oat/arm64/services.vdex\n";
                }
                if (File.Exists("apk\\framework.jar"))
                {
                    File.Move("apk\\framework.jar", "tmp\\magisk\\system\\framework\\framework.jar");
                    File.Create("tmp\\magisk\\system\\framework\\framework.odex").Dispose();
                    File.Create("tmp\\magisk\\system\\framework\\arm\\framework.odex").Dispose();
                    File.Create("tmp\\magisk\\system\\framework\\arm64\\framework.odex").Dispose();
                    str1 = str1 + "/system/framework/framework.jar\n" + "/system/framework/framework.odex\n" +
                           "/system/framework/arm/framework.odex\n" + "/system/framework/arm64/framework.odex\n";
                }
                if (File.Exists("tmp\\module_installer.sh"))
                    File.Move("tmp\\module_installer.sh", "tmp/magisk/META-INF/com/google/android/update-binary");
                else
                {
                    _mainForm.DebugUpdate(
                        "\n!!! WARNING: Could not find module_installer.sh, writing blank dummy file.");
                    File.WriteAllText("tmp/magisk/META-INF/com/google/android/update-binary", "", Encoding.ASCII);
                }
                File.WriteAllText("tmp/magisk/META-INF/com/google/android/updater-script", "#MAGISK", Encoding.ASCII);
                string str2 = "\nPOSTFSDATA=false\n";
                if (GetPatchStatus("Samsung Knox"))
                {
                    str2 = "\nPOSTFSDATA=true\n";
                    Directory.CreateDirectory("tmp\\magisk\\common");
                    File.WriteAllText("tmp\\magisk\\common\\post-fs-data.sh", _postFsData.Replace("\r\n", "\n"),
                        Encoding.ASCII);
                }
                
                File.WriteAllText("tmp\\magisk\\install.sh",
                    _installSh0.Replace("\r\n", "\n") + str2.Replace("\r\n", "\n") + _installSh1.Replace("\r\n", "\n") +
                    str1.Replace("\r\n", "\n") + _installSh2.Replace("\r\n", "\n"), Encoding.ASCII);
                File.WriteAllText("tmp\\magisk\\module.prop", _moduleProp.Replace("\r\n", "\n"), Encoding.ASCII);
                if (File.Exists("SmaliPatcher-" + Application.ProductVersion + "-fOmey@XDA.zip"))
                    File.Delete("SmaliPatcher-" + Application.ProductVersion + "-fOmey@XDA.zip");
                using (ZipFile zipFile = new ZipFile())
                {
                    zipFile.AddDirectory("tmp\\magisk");
                    zipFile.CompressionLevel = CompressionLevel.None;
                    zipFile.Save("SmaliPatcherModule-" + Application.ProductVersion + "-fOmey@XDA.zip");
                }
                _mainForm.DebugUpdate("\n==> Generated magisk module zip file");
                _mainForm.StatusUpdate("Cleaning up..");
                if (!_mainForm.SkipCleanUp)
                {
                    Directory.Delete("tmp", true);
                    if (!_mainForm.SimulateAdb)
                        Directory.Delete("adb", true);
                    Directory.Delete("smali", true);
                    Directory.Delete("apk", true);
                }
                if (File.Exists("SmaliPatcherModule-" + Application.ProductVersion + "-fOmey@XDA.zip"))
                    Process.Start("explorer.exe",
                        "/select," +
                        Path.GetFullPath("SmaliPatcherModule-" + Application.ProductVersion + "-fOmey@XDA.zip"));
                _mainForm.DebugUpdate("\n*** Complete ***");
                _mainForm.StatusUpdate("Idle..");
                return true;
            }
            _mainForm.DebugUpdate("\n!!! ERROR: Unable to cleanup old magisk template directory.");
            _mainForm.StatusUpdate("ERROR..");
            return false;
        }

        private bool GetPatchStatus(string patchTitle) =>
            _mainForm.Patches.Find(x => x.PatchTitle.Contains(patchTitle)).Status;
    }
}