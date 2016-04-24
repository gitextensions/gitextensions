using System;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public partial class FormOpenDirectory : GitExtensionsForm
    {
        private readonly TranslationString _warningOpenFailed =
            new TranslationString("Directory does not exist.");

        private readonly TranslationString _warningOpenFailedCaption =
            new TranslationString("Error");

        private GitModule choosenModule = null;

        public FormOpenDirectory()
        {
            InitializeComponent();
            Translate();

            _NO_TRANSLATE_Directory.DataSource = Repositories.RepositoryHistory.Repositories;
            _NO_TRANSLATE_Directory.DisplayMember = "Path";

            Load.Select();

            _NO_TRANSLATE_Directory.Focus();
            _NO_TRANSLATE_Directory.Select();
        }

        public static GitModule OpenModule(IWin32Window owner)
        {
            using (var open = new FormOpenDirectory())
            {
                open.ShowDialog(owner);
                return open.choosenModule;
            }
        }

        private void LoadClick(object sender, EventArgs e)
        {
            _NO_TRANSLATE_Directory.Text = _NO_TRANSLATE_Directory.Text.Trim();
            if (Directory.Exists(_NO_TRANSLATE_Directory.Text))
            {
                choosenModule = new GitModule(_NO_TRANSLATE_Directory.Text);
                Repositories.AddMostRecentRepository(choosenModule.WorkingDir);
                Close();
            }
            else
            {
                MessageBox.Show(this, _warningOpenFailed.Text, _warningOpenFailedCaption.Text);
            }
        }

        private void DirectoryKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                LoadClick(null, null);
            }
        }
    }
}