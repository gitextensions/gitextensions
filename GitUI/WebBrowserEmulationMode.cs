using System.ComponentModel;
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
            var appName = System.IO.Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

            const string featureControlRegKey = @"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Main\FeatureControl\";

            if (TryGetBrowserEmulationMode(out var emulationMode))
            {
                Registry.SetValue(featureControlRegKey + "FEATURE_BROWSER_EMULATION", appName, emulationMode, RegistryValueKind.DWord);
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
                using (var ieKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer",
                    RegistryKeyPermissionCheck.ReadSubTree,
                    System.Security.AccessControl.RegistryRights.QueryValues))
                {
                    var version = ieKey.GetValue("svcVersion");
                    if (version == null)
                    {
                        version = ieKey.GetValue("Version");
                        if (version == null)
                        {
                            return false;
                        }
                    }

                    int.TryParse(version.ToString().Split('.')[0], out browserVersion);
                }

                switch (browserVersion)
                {
                    case 7:
                        emulationMode = 7000; // Webpages containing standards-based !DOCTYPE directives are displayed in IE7 Standards mode.
                        break;
                    case 8:
                        emulationMode = 8000; // Webpages containing standards-based !DOCTYPE directives are displayed in IE8 mode.
                        break;
                    case 9:
                        emulationMode = 9000; // Internet Explorer 9. Webpages containing standards-based !DOCTYPE directives are displayed in IE9 mode.
                        break;
                    case 10:
                        emulationMode = 10000; // Internet Explorer 10.
                        break;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
