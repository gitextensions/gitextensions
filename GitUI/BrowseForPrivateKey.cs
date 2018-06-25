using System.IO;
using System.Windows.Forms;
using GitCommands;
using JetBrains.Annotations;

namespace GitUI
{
    /// <summary>
    /// Shows a dialog to let the user browse for a SSH key.
    /// </summary>
    public static class BrowseForPrivateKey
    {
        /// <summary>
        /// Prompts the user to browse for a key, and attempts to load it. Returns the path to the key, if successful.
        /// </summary>
        [CanBeNull]
        public static string BrowseAndLoad(IWin32Window parent)
        {
            var path = Browse(parent);
            if (!string.IsNullOrEmpty(path))
            {
                if (LoadKey(parent, path))
                {
                    return path;
                }
            }

            return null;
        }

        /// <summary>
        /// Prompts the user to browse for a key. Returns the path chosen, or null.
        /// </summary>
        [CanBeNull]
        public static string Browse(IWin32Window parent)
        {
            using (var dialog = new OpenFileDialog
            {
                Filter = " (*.ppk)|*.ppk",
                InitialDirectory = ".",
                Title = "Browse for key"
            })
            {
                if (dialog.ShowDialog(parent) == DialogResult.OK)
                {
                    return dialog.FileName;
                }

                return null;
            }
        }

        /// <summary>
        /// Tries to load the given key. Returns whether successful.
        /// </summary>
        public static bool LoadKey(IWin32Window parent, string path)
        {
            if (!File.Exists(AppSettings.Pageant))
            {
                MessageBoxes.PAgentNotFound(parent);
                return false;
            }

            GitModule.StartPageantWithKey(path);
            return true;
        }
    }
}