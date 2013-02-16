using System;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using ResourceManager.Translation;

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
            if (Directory.Exists(_NO_TRANSLATE_Directory.Text))
            {
                choosenModule = new GitModule(_NO_TRANSLATE_Directory.Text);

                Repositories.AddMostRecentRepository(choosenModule.WorkingDir);
            }
            else
            {
                DialogResult = DialogResult.None;
                MessageBox.Show(this, _warningOpenFailed.Text, _warningOpenFailedCaption.Text);
            }
        }

        private void Open_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.None)
                e.Cancel = true;
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