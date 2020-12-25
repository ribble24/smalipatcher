using System;
using System.Reflection;
using SmaliLib.Patches;
using SmaliLib.Steps;

namespace SmaliLib
{
    public class SmaliLibMain
    {
        private readonly IPlatform _platform;
        public SmaliLibMain(IPlatform platform) => _platform = platform;

        public void DownloadDeps() => DepDownloader.Download(_platform);

        public bool CheckResources()
        {
#if !ANDROID_NATIVE
            try
            {
                BinW.RunCommand(Bin.adb, "version").WaitForExit();
            }
            catch (Exception e)
            {
                _platform.ErrorCritical(PlatformCheck.IsWindows
                    ? $"Could not run adb (https://developer.android.com/studio/command-line/adb). Please make sure your AV allows execution ({e.Message})"
                    : $"Could not run adb (https://developer.android.com/studio/command-line/adb). You need to install it system-wide on non-windows platforms ({e.Message})");
                return false;
            }
            _platform.Log("ADB works");
#endif
            try
            {
                BinW.RunCommand(Bin.vdexExtractor, "--help").WaitForExit();
            }
            catch (Exception e)
            {
                _platform.ErrorCritical(PlatformCheck.IsWindows
                    ? $"Could not run vdexExtractor (https://github.com/anestisb/vdexExtractor). Please make sure your AV allows execution ({e.Message})"
                    : $"Could not run vdexExtractor (https://github.com/anestisb/vdexExtractor). You need to install it system-wide on non-windows platforms ({e.Message})");
                return false;
            }
            _platform.Log("vdexExtractor works");
            if (DepChecks.CheckJava(_platform))
            {
                _platform.Log("All deps seem good");
                return true;
            }
            return false;
        }

        public string DumpFramework() => FrameworkDumper.Dump(_platform);

        public int CheckAdb() => DepChecks.GetDevices(_platform);

        public void PatchFramework(string path, IPatch[] patches) => FrameworkPatcher.Patch(_platform, path, patches);

        public IPatch[] GetPatches() => new IPatch[]
        {
            new MockLocations(),
            new MockProviders(),
            new SecureFlag(),
            new SignatureVerification(),
            new SignatureSpoofing(),
            new RecoveryReboot(),
            new SamsungKnox(),
            new HighVolumeWarning(),
        };

        public void PackModule(IPatch[] patches, bool skipCleanup, bool removeFramework = true) =>
            ModulePacker.Pack(_platform, patches, skipCleanup, removeFramework);

        public Version GetVersion() => Assembly.GetExecutingAssembly().GetName().Version;
    }
}