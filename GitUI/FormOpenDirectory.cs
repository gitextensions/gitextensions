using System;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class Open : GitExtensionsForm
    {
        private readonly TranslationString _warningOpenFailed =
            new TranslationString("Directory does not exist.");

        private readonly TranslationString _warningOpenFailedCaption =
            new TranslationString("Error");

        public Open()
        {
            InitializeComponent();
            Translate();

            _NO_TRANSLATE_Directory.DataSource = Repositories.RepositoryHistory.Repositories;
            _NO_TRANSLATE_Directory.DisplayMember = "Path";

            Load.Select();

            _NO_TRANSLATE_Directory.Focus();
            _NO_TRANSLATE_Directory.Select();
        }

        private void BrowseClick(object sender, EventArgs e)
        {
            var browseDialog = new FolderBrowserDialog {SelectedPath = _NO_TRANSLATE_Directory.Text};

            if (browseDialog.ShowDialog(this) == DialogResult.OK)
            {
                _NO_TRANSLATE_Directory.Text = browseDialog.SelectedPath;
            }
        }

        private void LoadClick(object sender, EventArgs e)
        {
            if (Directory.Exists(_NO_TRANSLATE_Directory.Text))
            {
                Settings.WorkingDir = _NO_TRANSLATE_Directory.Text;

                Repositories.RepositoryHistory.AddMostRecentRepository(Settings.WorkingDir);

                Close();
            }
            else
            {
                MessageBox.Show(this, _warningOpenFailed.Text, _warningOpenFailedCaption.Text);
            }
        }

        private void DirectoryKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char) Keys.Enter)
            {
                LoadClick(null, null);
            }
        }
    }
}