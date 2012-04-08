using System;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using ResourceManager.Translation;

namespace GitUI
{
    /// <summary>
    /// Shows a dialog to let the user browse for a SSH key, and tries to load it.
    /// </summary>
    public partial class FormLoadPuttySshKey
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
        /// Prompts the user to browse to a key, and attempts to load it. Returns whether successfull.
        /// </summary>
        public static bool BrowseForKey(IWin32Window parent)
        {
            var dialog = new OpenFileDialog
            {
                Filter = " (*.ppk)|*.ppk",
                InitialDirectory = ".",
                Title = "Browse for key"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
                return LoadKey(parent, dialog.FileName);
            else
                return false;
        }

        /// <summary>
        /// Tries to load the given key. Returns whether successfull.
        /// </summary>
        private static bool LoadKey(IWin32Window parent, string path)
        {
            if (!File.Exists(Settings.Pageant))
            {
                MessageBox.Show(parent, _pageantNotFound.Text, _pageantNotFoundCaption.Text);
                return false;
            }

            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show(parent, _loadKeyFailed.Text, _loadKeyFailedCaption.Text);
                return false;
            }

            Settings.Module.StartPageantWithKey(path);
            return true;
        }
    }
}