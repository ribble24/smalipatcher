namespace SmaliLib
{
    public interface IPlatform
    {
        void ErrorCritical(string message);
        void Warning(string message);
        void Log(string message);
        void LogIncremental(string message);
        byte[] Download(string url, string fancyName);
        void ShowOutput(string path);
    }
}