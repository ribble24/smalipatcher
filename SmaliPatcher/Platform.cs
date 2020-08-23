using SmaliLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SmaliPatcher
{
    class Platform : IPlatform
    {
        private readonly MainForm _form;
        public byte[] Download(string url, string fancyName)
        {
            Log($"Downloading {fancyName}");
            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += (sender, e) =>
                {
                    _form.Invoke((Action)(() =>
                    {
                        _form.statusLabel.Text = $"Downloading {fancyName}: {e.ProgressPercentage}%";
                    }));
                };
                return client.DownloadDataTaskAsync(url).Result;
            }
        }

        public void ErrorCritical(string message)
        {
            LogIncremental($"\r\nERROR!\r\n{message}");
        }

        public void Log(string message)
        {
            _form.Invoke((Action)(() => {
                _form.logBox.AppendText($"\r\n<> {message}");
                _form.logBox.ScrollToCaret();
                if (_form.setStatus)
                {
                    _form.statusLabel.Text = message;
                }
            }));
        }

        public void LogIncremental(string message)
        {
            _form.Invoke((Action)(() => {
                _form.logBox.AppendText(message);
                _form.logBox.ScrollToCaret();
            }));
        }

        public void ShowOutput(string path)
        {
            Process.Start("explorer.exe", $"/select,{path}");
        }

        public void Warning(string message)
        {
            LogIncremental($"\r\nWARNING:\r\n{message}");
        }

        public Platform(MainForm form) => _form = form;
    }
}
