using System;
using Microsoft.Win32;

namespace GitCommands.Utils
{
    public static class EnvUtils
    {
        public static bool RunningOnWindows()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsWindowsVistaOrGreater()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT
                   && Environment.OSVersion.Version.CompareTo(new Version(6, 0)) >= 0;
        }

        public static bool IsWindows7OrGreater()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT
                   && Environment.OSVersion.Version.CompareTo(new Version(6, 1)) >= 0;
        }

        public static bool IsWindows8OrGreater()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT
                   && Environment.OSVersion.Version.CompareTo(new Version(6, 2)) >= 0;
        }

        public static bool IsWindows8Point1OrGreater()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT
                   && Environment.OSVersion.Version.CompareTo(new Version(6, 3)) >= 0;
        }

        public static bool RunningOnUnix()
        {
            return Environment.OSVersion.Platform == PlatformID.Unix;
        }

        public static bool RunningOnMacOSX()
        {
            return Environment.OSVersion.Platform == PlatformID.MacOSX;
        }

        public static bool IsNet4FullOrHigher()
        {
            if (Environment.Version.Major > 4)
            {
                return true;
            }

            if (Environment.Version.Major == 4)
            {
                if (Environment.Version.Minor >= 5)
                {
                    return true;
                }

                try
                {
                    RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full", false);
                    if (registryKey != null)
                    {
                        using (registryKey)
                        {
                            var v = registryKey.GetValue("Install");
                            return v != null && v.ToString() == "1";
                        }
                    }
                }
                catch (UnauthorizedAccessException e)
                {
                    System.Diagnostics.Trace.WriteLine(e);
                }
            }

            return false;
        }

        public static string ReplaceLinuxNewLinesDependingOnPlatform(string s)
        {
            if (s.IsNullOrEmpty())
            {
                return s;
            }

            if (RunningOnUnix())
            {
                return s;
            }

            return s.Replace("\n", Environment.NewLine);
        }

        public static char EnvVariableSeparator => RunningOnWindows() ? ';' : ':';
    }
}
