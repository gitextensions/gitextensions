using System;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;

namespace GitUI
{
    public partial class FormLoadPuttySshKey : GitExtensionsForm
    {
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
                MessageBox.Show("Cannot load SSH key. PuTTY is not configured properly.", "PuTTY");
                return;
            }

            if (string.IsNullOrEmpty(PrivateKeypath.Text))
            {
                MessageBox.Show("Could not load key.", "PuTTY");
                return;
            }

            GitCommandHelpers.StartPageantWithKey(PrivateKeypath.Text);
            Close();
        }

        private void BrowseClick(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog
                             {
                                 Filter = "Private key (*.ppk)|*.ppk",
                                 InitialDirectory = ".",
                                 Title = "Select ssh key file"
                             };
            if (dialog.ShowDialog() == DialogResult.OK)
                PrivateKeypath.Text = dialog.FileName;
        }
    }
}