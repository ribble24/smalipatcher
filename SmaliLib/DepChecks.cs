using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace SmaliLib
{
    internal static class DepChecks
    {
        public static int GetDevices(IPlatform platform)
        {
#if ANDROID_NATIVE
            return 1;
#else
            platform.Log("Checking for devices");
            try
            {
                int num = 0;
                Process process = BinW.RunCommand(Bin.adb, "devices");
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
            catch (Exception e)
            {
                platform.ErrorCritical($"Could not get attached devices ({e.Message})");
                return 0;
            }
#endif
        }

        public static bool CheckJava(IPlatform platform)
        {
            try
            {
                Process process = BinW.RunCommand(Bin.java, "-version");
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
                        platform.Log($"Detected java: {new Regex("\".*\"").Match(input).Value.Trim().Trim('"')}");
                        return true;
                    }
                platform.ErrorCritical("Java not detected. Try running as administrator");
                return false;
            }
            catch (Exception e)
            {
                platform.ErrorCritical($"Failed when detecting java ({e.Message})");
                return false;
            }
        }
    }
}