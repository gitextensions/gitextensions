using GitUI.Infrastructure;

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
        public static string? BrowseAndLoad(IWin32Window parent)
        {
            string? sshKeyFile = Browse(parent);
            if (!string.IsNullOrEmpty(sshKeyFile))
            {
                if (PuttyHelpers.StartPageantIfConfigured(() => sshKeyFile))
                {
                    return sshKeyFile;
                }
            }

            return null;
        }

        /// <summary>
        /// Prompts the user to browse for a key. Returns the path chosen, or null.
        /// </summary>
        public static string? Browse(IWin32Window parent)
        {
            using OpenFileDialog dialog = new()
            {
                Filter = " (*.ppk)|*.ppk",
                InitialDirectory = ".",
                Title = "Browse for key"
            };
            if (dialog.ShowDialog(parent) == DialogResult.OK)
            {
                return dialog.FileName;
            }

            return null;
        }
    }
}
