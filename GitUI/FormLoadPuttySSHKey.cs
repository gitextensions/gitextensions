using System;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormLoadPuttySshKey : GitExtensionsForm
    {
        private readonly TranslationString _pageantNotFound =
            new TranslationString("Cannot load SSH key. PuTTY is not configured properly.");
        private readonly TranslationString _pageantNotFoundCaption =
            new TranslationString("PuTTY");

        private readonly TranslationString _loadKeyFailed =
            new TranslationString("Could not load key.");
        private readonly TranslationString _loadKeyFailedCaption =
            new TranslationString("PuTTY");

        private readonly TranslationString _browsePrivateKeyFilter =
            new TranslationString("Private key");
        private readonly TranslationString _browsePrivateKeyCaption =
            new TranslationString("Select SSH key file");

        public FormLoadPuttySshKey()
        {
            InitializeComponent();
            Translate();
        }

        private void PrivateKeypathDropDown(object sender, EventArgs e)
        {
            PrivateKeypath.DataSource = Repositories.RepositoryHistory.Repositories;
            PrivateKeypath.DisplayMember = "Path";
        }

        private void LoadSshKeyClick(object sender, EventArgs e)
        {
            if (!File.Exists(Settings.Pageant))
            {
                MessageBox.Show(this, _pageantNotFound.Text, _pageantNotFoundCaption.Text);
                return;
            }

            if (string.IsNullOrEmpty(PrivateKeypath.Text))
            {
                MessageBox.Show(this, _loadKeyFailed.Text, _loadKeyFailedCaption.Text);
                return;
            }

            Settings.Module.StartPageantWithKey(PrivateKeypath.Text);
            Close();
        }

        private void BrowseClick(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog
                             {
                                 Filter = _browsePrivateKeyFilter.Text + " (*.ppk)|*.ppk",
                                 InitialDirectory = ".",
                                 Title = _browsePrivateKeyCaption.Text
                             };
            if (dialog.ShowDialog(this) == DialogResult.OK)
                PrivateKeypath.Text = dialog.FileName;
        }
    }
}