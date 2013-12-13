using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace GitUI
{
    public static class OsShellUtil
    {
        public static void OpenAs(string file)
        {
            Process.Start(new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                FileName = "rundll32.exe",
                Arguments = "shell32.dll,OpenAs_RunDLL " + file
            });
        }

        public static void SelectPathInFileExplorer(string filePath)
        {
            Process.Start("explorer.exe", "/select, " + filePath);
        }

        public static void OpenWithFileExplorer(string filePath)
        {
            Process.Start("explorer.exe", filePath);
        }

        /// <summary>
        /// opens urls even with anchor
        /// </summary>
        public static void OpenUrlInDefaultBrowser(string url)
        {
            // Process.Start(url); / does not work with anchors: http://stackoverflow.com/questions/2404449/process-starturl-with-anchor-in-the-url
            var openSubKey = Registry.ClassesRoot.OpenSubKey(@"\http\shell\open\command\");
            if (openSubKey != null)
            {
                var browserRegistryString  = openSubKey.GetValue("").ToString();
                var defaultBrowserPath = Regex.Match(browserRegistryString, @"(\"".*?\"")").Captures[0].ToString();
                Process.Start(defaultBrowserPath, url);
            }
        }
    }
}
