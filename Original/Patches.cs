namespace SmaliPatcher
{
    public struct Patches
    {
        public bool Status;
        public string PatchTitle;
        public string PatchDescription;
        public string TargetFile;

        public Patches(bool s, string pT, string pD, string tF)
        {
            Status = s;
            PatchTitle = pT;
            PatchDescription = pD;
            TargetFile = tF;
        }
    }
}