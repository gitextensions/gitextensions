using System.IO;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    /// <summary>
    /// Shows a dialog to let the user browse for a SSH key.
    /// </summary>
    public class BrowseForPrivateKey : Translate
    {
        private static readonly TranslationString _pageantNotFound =
            new TranslationString("Cannot load SSH key. PuTTY is not configured properly.");
        private static readonly TranslationString _pageantNotFoundCaption =
            new TranslationString("PuTTY");

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
            using (var dialog = new OpenFileDialog
            {
                Filter = " (*.ppk)|*.ppk",
                InitialDirectory = ".",
                Title = "Browse for key"
            })
            {

                if (dialog.ShowDialog(parent) == DialogResult.OK)
                    return dialog.FileName;

                return null;
            }
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

            GitModule.Current.StartPageantWithKey(path);
            return true;
        }
    }
}