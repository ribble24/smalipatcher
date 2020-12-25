using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace SmaliPatcher
{
    internal class Check
    {
        private MainForm _mainForm;

        public void Init(object sender)
        {
            if (_mainForm != null)
                return;
            _mainForm = (MainForm) sender;
        }

        public bool CheckJava()
        {
            try
            {
                Process process = Process.Start(new ProcessStartInfo
                {
                    Arguments = "-version",
                    FileName = "java.exe",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                });
                process.WaitForExit();
                string s = process.StandardOutput.ReadToEnd();
                string end = process.StandardError.ReadToEnd();
                if (s.Length == 0 && end.Length > 0)
                    s = end;
                StringReader stringReader = new StringReader(s);
                string input;
                while ((input = stringReader.ReadLine()) != null)
                    if (input.Contains("version \""))
                    {
                        _mainForm.DebugUpdate("\n==> Detected java: " +
                                              new Regex("\".*\"").Match(input).Value.Trim().Trim('"'));
                        return true;
                    }
                _mainForm.DebugUpdate("\n!!! ERROR: Java not detected.");
                _mainForm.DebugUpdate("\n!!! Try running as administrator.");
                _mainForm.StatusUpdate("ERROR..");
                return false;
            }
            catch (Exception ex)
            {
                _mainForm.DebugUpdate("\n!!! ERROR: Java not detected.");
                _mainForm.DebugUpdate("\n!!! ERROR: " + ex.Message);
                _mainForm.StatusUpdate("ERROR..");
                return false;
            }
        }

        public int CheckAdb()
        {
            try
            {
                if (_mainForm.SimulateAdb)
                    return 1;
                int num = 0;
                Process process = Process.Start(new ProcessStartInfo
                {
                    Arguments = "devices",
                    FileName = "bin\\adb.exe",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                });
                process.WaitForExit();
                string s = process.StandardOutput.ReadToEnd();
                string end = process.StandardError.ReadToEnd();
                if (s.Length == 0 && end.Length > 0)
                    s = end;
                StringReader stringReader = new StringReader(s);
                bool flag = false;
                string input;
                while ((input = stringReader.ReadLine()) != null)
                    if (!flag && input.Contains("List of devices attached"))
                        flag = true;
                    else if (flag)
                    {
                        string str1 = new Regex("[a-z,A-Z,0-9]*\tdevice").Match(input).Value;
                        string str2 = new Regex("[a-z,A-Z,0-9]*\trecovery").Match(input).Value;
                        string str3 = new Regex("[a-z,A-Z,0-9]*\tunauthorized").Match(input).Value;
                        if (str1 != "" || str2 != "")
                        {
                            if (str3 == "")
                                ++num;
                        }
                        else if (str3 != "")
                            return -1;
                    }
                return num;
            }
            catch (Exception ex)
            {
                _mainForm.DebugUpdate("\n!!! ERROR: " + ex.Message);
                _mainForm.StatusUpdate("ERROR..");
                return 0;
            }
        }

        public void CheckAdministrator()
        {
            if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                return;
            _mainForm.DebugUpdate("\nWARNING: Could not detect administrator privileges.");
        }
    }
}