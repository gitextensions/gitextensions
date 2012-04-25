using System;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using ResourceManager.Translation;

namespace GitUI
{
    /// <summary>
    /// Shows a dialog to let the user browse for a SSH key.
    /// </summary>
    public partial class BrowseForPrivateKey
    {
        private static readonly TranslationString _pageantNotFound =
            new TranslationString("Cannot load SSH key. PuTTY is not configured properly.");
        private static readonly TranslationString _pageantNotFoundCaption =
            new TranslationString("PuTTY");

        private static readonly TranslationString _loadKeyFailed =
            new TranslationString("Could not load key.");
        private static readonly TranslationString _loadKeyFailedCaption =
            new TranslationString("PuTTY");

        private static readonly TranslationString _browsePrivateKeyFilter =
            new TranslationString("Private key");
        private static readonly TranslationString _browsePrivateKeyCaption =
            new TranslationString("Select SSH key file");

        /// <summary>
        /// Prompts the user to browse for a key, and attempts to load it. Returns the path to the key, if successful.
        /// </summary>
        public static string BrowseAndLoad(IWin32Window parent)
        {
            var path = Browse(parent);
            if (!string.IsNullOrEmpty(path))
                if (LoadKey(parent, path))
                    return path;

            return null;
        }

        /// <summary>
        /// Prompts the user to browse for a key. Returns the path chosen, or null.
        /// </summary>
        public static string Browse(IWin32Window parent)
        {
            var dialog = new OpenFileDialog
            {
                Filter = " (*.ppk)|*.ppk",
                InitialDirectory = ".",
                Title = "Browse for key"
            };

            if (dialog.ShowDialog(parent) == DialogResult.OK)
                return dialog.FileName;

            return null;
        }

        /// <summary>
        /// Tries to load the given key. Returns whether successful.
        /// </summary>
        public static bool LoadKey(IWin32Window parent, string path)
        {
            if (!File.Exists(Settings.Pageant))
            {
                MessageBox.Show(parent, _pageantNotFound.Text, _pageantNotFoundCaption.Text);
                return false;
            }

            Settings.Module.StartPageantWithKey(path);
            return true;
        }
    }
}