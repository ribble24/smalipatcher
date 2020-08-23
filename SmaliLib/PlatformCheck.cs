using System;

namespace SmaliLib
{
    internal static class PlatformCheck
    {
        public static bool IsWindows => Environment.OSVersion.Platform == PlatformID.Win32NT;
        public static bool IsUnix => Environment.OSVersion.Platform == PlatformID.Unix;
        public static bool IsMac => Environment.OSVersion.Platform == PlatformID.MacOSX;
    }
}