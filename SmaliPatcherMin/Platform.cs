using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using SmaliLib;

namespace SmaliPatcherMin
{
    public class Platform : IPlatform
    {
        public void ErrorCritical(string message)
        {
            Console.WriteLine(new string('-', message.Split('\n').OrderByDescending(s => s.Length).First().Length));
            Console.WriteLine("A CRITICAL ERROR OCCURED, THE PROGRAM WILL NOW EXIT");
            Console.WriteLine(message);
            Console.WriteLine(new string('-', message.Split('\n').OrderByDescending(s => s.Length).First().Length));
            Environment.Exit(0);
        }

        public void Warning(string message)
        {
            Console.WriteLine("WARNING:");
            Console.WriteLine(message);
        }

        public void Log(string message)
        {
            Console.WriteLine($"<> {message}");
        }

        public void LogIncremental(string message)
        {
            Console.WriteLine(message);
        }

        public byte[] Download(string url, string fancyName)
        {
            Log($"Downloading {fancyName}");
            using WebClient client = new WebClient();
            client.DownloadProgressChanged += (sender, args) =>
            {
                Console.CursorLeft = 0;
                Console.Write($"{args.BytesReceived * 100 / args.TotalBytesToReceive}%");
            };
            byte[] data = client.DownloadDataTaskAsync(url).Result;
            Console.WriteLine();
            return data;
        }

        public void ShowOutput(string path)
        {
            //TODO use this in the Forms UI
            //Process.Start("explorer.exe", $"/select,{path}");
            Console.WriteLine($"Complete: {path}");
        }
    }
}
