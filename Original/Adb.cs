using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace SmaliPatcher
{
    internal class Adb
    {
        private Download _download;
        private string _lastOutput = "";
        private MainForm _mainForm;
        private Patch _patch;
        public List<Patches> Patches;
        private List<string> _processedFiles = new List<string>();

        public void Init(object sender)
        {
            if (_mainForm == null)
                _mainForm = (MainForm) sender;
            if (_patch == null)
            {
                _patch = new Patch();
                _patch.Init(_mainForm);
            }
            if (_download != null)
                return;
            _download = new Download();
            _download.Init(_mainForm);
        }

        public void PullFileset()
        {
            if (!Directory.Exists("bin") || !File.Exists("bin\\adb.exe") || _mainForm.SimulateAdb)
                return;
            if (Directory.Exists("adb"))
                Directory.Delete("adb", true);
            string[,] strArray = new string[4, 2]
            {
                {
                    "/system/build.prop",
                    "adb"
                },
                {
                    "/system/framework/",
                    "adb"
                },
                {
                    "/system/system/build.prop",
                    "adb"
                },
                {
                    "/system/system/framework/",
                    "adb"
                }
            };
            for (int index = 0; index < strArray.Length / 2; ++index)
                Pull(strArray[index, 0], strArray[index, 1], false);
            _mainForm.DebugUpdate("\n==> Dumped framework");
            _patch.ProcessFrameworkDirectory("adb");
        }

        public void Pull(string filePath, string destination, bool redirectOutput)
        {
            if (!Directory.Exists("bin") || !File.Exists("bin\\adb.exe"))
                return;
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);
            _mainForm.StatusUpdate("Pulling files..");
            StartProcess("bin\\adb.exe", "pull \"" + filePath + "\" \"" + destination + "\"",
                (redirectOutput ? 1 : 0) != 0);
        }

        public void Push(string filePath, string destination, bool redirectOutput)
        {
            if (!Directory.Exists("bin") || !File.Exists("bin\\adb.exe"))
                return;
            _mainForm.StatusUpdate("Pushing files..");
            StartProcess("bin\\adb.exe", "push \"" + filePath + "\" \"" + destination + "\"",
                (redirectOutput ? 1 : 0) != 0);
        }

        public void Shell(string cmd, bool redirectOutput)
        {
            if (!Directory.Exists("bin") || !File.Exists("bin\\adb.exe"))
                return;
            _mainForm.StatusUpdate("Executing shell command..");
            StartProcess("bin\\adb.exe", "shell \"" + cmd + "\"", redirectOutput);
        }

        private bool GetPatchStatus(string patchTitle)
        {
            Patches = _mainForm.Patches;
            return Patches.Find(x => x.PatchTitle.Contains(patchTitle)).Status;
        }

        private string GetPatchTargetFile(string patchTitle)
        {
            Patches = _mainForm.Patches;
            return Patches.Find(x => x.PatchTitle.Contains(patchTitle)).TargetFile;
        }

        private void StartProcess(string exe, string args, bool redirectOutput)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.Arguments = args;
                process.StartInfo.FileName = exe;
                process.StartInfo.UseShellExecute = false;
                if (redirectOutput)
                {
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.OutputDataReceived += ProcessOutputDataReceived;
                    process.ErrorDataReceived += ProcessErrorDataReceived;
                    process.Exited += ProcessExited;
                }
                process.EnableRaisingEvents = true;
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                _mainForm.DebugUpdate("\n!!! ERROR: " + ex.Message);
                _mainForm.StatusUpdate("ERROR..");
            }
        }

        private void ProcessExited(object sender, EventArgs e)
        {
        }

        private void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string data = e.Data;
            if (data == null || !(data != _lastOutput))
                return;
            _lastOutput = data;
            if (data.Length > 0 && data.Substring(0, 1) == "[")
            {
                string str = new Regex("\\[.*\\]").Match(data).Value;
                _mainForm.StatusUpdate("Dumping framework.. " + str.Substring(1, str.Length - 2).Replace(" ", ""));
            }
            else
            {
                if (data.Length <= 0 || !(data.Substring(0, 11) != "adb: error:") ||
                    !(data.Substring(data.Length - 14, 14) != "does not exist"))
                    return;
                _mainForm.DebugUpdate("\n" + data);
            }
        }

        private void ProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            string data = e.Data;
            if (data == null || !(data != _lastOutput))
                return;
            _lastOutput = data;
            if (data.Length > 0 && data.Substring(0, 1) == "[")
            {
                string str = new Regex("\\[.*\\]").Match(data).Value;
                _mainForm.StatusUpdate("Dumping framework.. " + str.Substring(1, str.Length - 2).Replace(" ", ""));
            }
            else
            {
                if (data.Length <= 0 || !(data.Substring(0, 11) != "adb: error:") ||
                    !(data.Substring(data.Length - 14, 14) != "does not exist"))
                    return;
                _mainForm.DebugUpdate("\n" + data);
            }
        }
    }
}