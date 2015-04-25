namespace GitUI
{
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    using Microsoft.Win32;
#if !__MonoCS__
    using Microsoft.WindowsAPICodePack.Dialogs;
#endif

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

        /// <summary>
        /// Prompts the user to select a directory.
        /// </summary>
        /// <param name="ownerWindow">The owner window.</param>
        /// <param name="selectedPath">The initially selected path.</param>
        /// <returns>The path selected by the user, or null if the user cancels the dialog.</returns>
        public static string PickFolder(IWin32Window ownerWindow, string selectedPath = null)
        {
#if !__MonoCS__
            if (GitCommands.Utils.EnvUtils.IsWindowsVistaOrGreater())
            {
                // use Vista+ dialog
                using (var dialog = new CommonOpenFileDialog())
                {
                    dialog.IsFolderPicker = true;

                    if (selectedPath != null)
                        dialog.InitialDirectory = selectedPath;

                    var result = dialog.ShowDialog(ownerWindow.Handle);

                    if (result == CommonFileDialogResult.Ok)
                        return dialog.FileName;
                }
            }
            else
            {
#endif
                // use XP-era dialog
                using (var dialog = new FolderBrowserDialog())
                {
                    if (selectedPath != null)
                        dialog.SelectedPath = selectedPath;

                    var result = dialog.ShowDialog(ownerWindow);

                    if (result == DialogResult.OK)
                        return dialog.SelectedPath;
                }
#if !__MonoCS__
            }
#endif

            // return null if the user cancelled
            return null;
        }
    }
}
