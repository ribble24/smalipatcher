using System.IO;
using System.IO.Compression;

namespace SmaliLib.Patches
{
    public abstract class IPatch
    {
        public bool DexPatcherCoreRequired;
        public string DexPatcherTarget;
        public abstract string Title { get; }
        public abstract string Description { get; }
        public abstract string TargetFile { get; }
        public bool PostFsData { get; protected set; }
        public abstract bool IsDefault { get; }
        public abstract void JarCompileStep(IPlatform platform);
        public abstract string PatchFileStep(IPlatform platform, string baseStr);
        public abstract void PackModuleStep(ZipArchive archive);

        protected void DexPatcher(IPlatform platform, string jar, string dexPatch)
        {
            if (!Directory.Exists("bin") || !File.Exists(Path.Combine("bin", "dexpatcher.jar")))
                return;
            if (Directory.Exists("classes"))
                Directory.Delete("classes", true);
            if (!Directory.Exists("classes"))
                Directory.CreateDirectory("classes");
            platform.Log("Patching classes");
            BinW.LogIncremental(platform, BinW.RunCommand(Bin.java,
                $"-Xms1024m -Xmx1024m -jar {Path.Combine("bin", "dexpatcher.jar")} --multi-dex --output classes {jar} {dexPatch}"));
            using (ZipArchive zipFile = ZipFile.Open(jar, ZipArchiveMode.Update))
            {
                string[] files = Directory.GetFiles("classes", "classes*");
                foreach (string t in files)
                {
                    zipFile.GetEntry(Path.GetFileName(t))?.Delete();
                    zipFile.CreateEntry(t);
                }
            }
            if (Directory.Exists("classes"))
                Directory.Delete("classes", true);
            platform.Log($"Merged patch: {Path.GetFileNameWithoutExtension(dexPatch)}");
        }
    }
}