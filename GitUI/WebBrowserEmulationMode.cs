﻿using System.ComponentModel;
using GitExtUtils;
using Microsoft.Win32;

namespace GitUI
{
    public static class WebBrowserEmulationMode
    {
        public static void SetBrowserFeatureControl()
        {
            // Fix for issue #2654:
            // Set the emulation mode for the embedded .NET WebBrowser control to the IE version installed on the machine.
            // http://msdn.microsoft.com/en-us/library/ee330720(v=vs.85).aspx

            // Only when not running inside Visual Studio Designer
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime)
            {
                return;
            }

            // FeatureControl settings are per-process
            string appName = System.IO.Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

            const string featureControlRegKey = @"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Main\FeatureControl\";

            if (TryGetBrowserEmulationMode(out uint emulationMode))
            {
                try
                {
                    Registry.SetValue(featureControlRegKey + "FEATURE_BROWSER_EMULATION", appName, emulationMode, RegistryValueKind.DWord);
                }
                catch (UnauthorizedAccessException)
                {
                    // Don't fail when user have no rights to update the registry...
                }
            }
        }

        private static bool TryGetBrowserEmulationMode(out uint emulationMode)
        {
            // https://msdn.microsoft.com/en-us/library/ee330730(v=vs.85).aspx#browser_emulation
            // http://stackoverflow.com/questions/28526826/web-browser-control-emulation-issue-feature-browser-emulation/28626667#28626667

            emulationMode = 11000; // Internet Explorer 11. Webpages containing standards-based !DOCTYPE directives are displayed in IE11 Standards mode.
            try
            {
                int browserVersion;
                using (RegistryKey ieKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer",
                    RegistryKeyPermissionCheck.ReadSubTree,
                    System.Security.AccessControl.RegistryRights.QueryValues))
                {
                    object version = ieKey.GetValue("svcVersion");
                    if (version is null)
                    {
                        version = ieKey.GetValue("Version");
                        if (version is null)
                        {
                            return false;
                        }
                    }

                    int.TryParse(version.ToString().LazySplit('.').First(), out browserVersion);
                }

                emulationMode = browserVersion switch
                {
                    7 => 7000, // Webpages containing standards-based !DOCTYPE directives are displayed in IE7 Standards mode.
                    8 => 8000, // Webpages containing standards-based !DOCTYPE directives are displayed in IE8 mode.
                    9 => 9000, // Internet Explorer 9. Webpages containing standards-based !DOCTYPE directives are displayed in IE9 mode.
                    10 => 10000, // Internet Explorer 10.
                    _ => emulationMode
                };

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
