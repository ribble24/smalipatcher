using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using SmaliLib.Patches;

namespace SmaliLib.Steps
{
    internal static class ModulePacker
    {
        private static string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static void Pack(IPlatform platform, IPatch[] patches, bool skipCleanup, bool removeFramework)
        {
            platform.Log("Creating archive");
            string file = $"SmaliPatcherModule-{Version}-fOmey@XDA.zip";
            if (File.Exists(file))
                File.Delete(file);
            using (ZipArchive archive = ZipFile.Open(file, ZipArchiveMode.Create))
            {
                string files = "";
                archive.CreateEntry("META-INF/com/google/android/");
                archive.CreateEntry("system/framework/");
                archive.CreateEntry("system/framework/arm/");
                archive.CreateEntry("system/framework/arm64/");
                archive.CreateEntry("system/framework/oat/arm/");
                archive.CreateEntry("system/framework/oat/arm64/");
                if (File.Exists(Path.Combine("apk", "services.jar")))
                {
                    archive.CreateEntryFromFile(Path.Combine("apk", "services.jar"),
                        "system/framework/services.jar");
                    archive.CreateEntry("system/framework/services.odex");
                    archive.CreateEntry("system/framework/arm/services.odex");
                    archive.CreateEntry("system/framework/arm64/services.odex");
                    archive.CreateEntry("system/framework/oat/arm/services.odex");
                    archive.CreateEntry("system/framework/oat/arm64/services.odex");
                    archive.CreateEntry("system/framework/oat/arm64/services.vdex");
                    files += "/system/framework/services.jar\n"
                             + "/system/framework/services.odex\n"
                             + "/system/framework/arm/services.odex\n"
                             + "/system/framework/arm64/services.odex\n"
                             + "/system/framework/oat/arm/services.odex\n"
                             + "/system/framework/oat/arm64/services.odex\n"
                             + "/system/framework/oat/arm64/services.vdex\n";
                }
                if (File.Exists(Path.Combine("apk", "framework.jar")))
                {
                    archive.CreateEntryFromFile(Path.Combine("apk", "framework.jar"),
                        "system/framework/framework.jar");
                    archive.CreateEntry("system/framework/framework.odex");
                    archive.CreateEntry("system/framework/arm/framework.odex");
                    archive.CreateEntry("system/framework/arm64/framework.odex");
                    files = files + "/system/framework/framework.jar\n"
                                  + "/system/framework/framework.odex\n"
                                  + "/system/framework/arm/framework.odex\n"
                                  + "/system/framework/arm64/framework.odex\n";
                }
                if (File.Exists(Path.Combine("bin", "module_installer.sh")))
                    archive.CreateEntryFromFile(Path.Combine("bin", "module_installer.sh"), "META-INF/com/google/android/update-binary");
                else
                {
                    platform.Warning("Could not find module_installer.sh, writing blank dummy file");
                    File.WriteAllText("tmp/magisk/META-INF/com/google/android/update-binary", "", Encoding.ASCII);
                }
                ZipArchiveEntry entry = archive.CreateEntry("META-INF/com/google/android/updater-script");
                using (Stream s = entry.Open())
                {
                    using StreamWriter writer = new StreamWriter(s, Encoding.ASCII);
                    writer.Write("#MAGISK");
                }
                string str2 = $"\nPOSTFSDATA={patches.Aggregate(false, ((b, patch) => b | patch.PostFsData)).ToString().ToLower()}\n";
                foreach (IPatch patch in patches) patch.PackModuleStep(archive);
                entry = archive.CreateEntry("install.sh");
                using (Stream s = entry.Open())
                {
                    const string _installSh0 = "SKIPMOUNT=false\nPROPFILE=false";
                    const string _installSh1 = "LATESTARTSERVICE=false\n\nREPLACE = \"";
                    const string _installSh2 =
                        "\"\n\nprint_modname() {\n   ui_print \"*******************************\"\n   ui_print \"          Smali Patcher        \"\n   ui_print \"           fOmey @ XDA         \"\n   ui_print \"*******************************\"\n}\n\non_install() {\n   ui_print \"- Extracting module files\"\n   unzip -o \"$ZIPFILE\" 'system/*' -d $MODPATH >&2\n}\n\nset_permissions() {\n  set_perm_recursive $MODPATH 0 0 0755 0644\n}";
                    using StreamWriter writer = new StreamWriter(s, Encoding.ASCII);
                    writer.Write(_installSh0);
                    writer.Write(str2);
                    writer.Write(_installSh1);
                    writer.Write(files);
                    writer.Write(_installSh2);
                }
                entry = archive.CreateEntry("module.prop");
                using (Stream s = entry.Open())
                {
                    using StreamWriter writer = new StreamWriter(s, Encoding.ASCII);
                    writer.Write($"id=fomey.smalipatcher\nname=Smali Patcher\nversion=v{Version}\nversionCode=1\nauthor=fOmeyyy\ndescription=Collection of framework patches.");
                }
            }
            platform.Log("Cleaning up");
            if (!skipCleanup)
            {
                if (Directory.Exists("tmp"))
                    Directory.Delete("tmp", true);
                if (removeFramework)
                    if (Directory.Exists("adb"))
                        Directory.Delete("adb", true);
                if (Directory.Exists("smali"))
                    Directory.Delete("smali", true);
                if (Directory.Exists("apk"))
                    Directory.Delete("apk", true);
            }
            if (PlatformCheck.IsWindows)
                platform.ShowOutput(Path.GetFullPath(file));
        }
    }
}
