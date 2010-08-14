using System;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;

namespace GitUI
{
    public partial class Open : GitExtensionsForm
    {
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

            if (browseDialog.ShowDialog() == DialogResult.OK)
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
                MessageBox.Show("Directory does not exist.", "Error");
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