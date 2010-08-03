using System;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormMergeBranch : GitExtensionsForm
    {
        private readonly TranslationString _currentBranch = new TranslationString("Current branch: ");
        private readonly string _defaultBranch;

        public FormMergeBranch(string defaultBranch)
        {
            InitializeComponent();
            Translate();
            _defaultBranch = defaultBranch;
        }

        private void FormMergeBranchLoad(object sender, EventArgs e)
        {
            var selectedHead = GitCommands.GitCommands.GetSelectedBranch();
            Currentbranch.Text = _currentBranch.Text + selectedHead;

            Branches.DisplayMember = "Name";
            Branches.DataSource = GitCommands.GitCommands.GetHeads(true, true);

            if (_defaultBranch != null)
                Branches.Text = _defaultBranch;

            Branches.Select();
        }

        private void OkClick(object sender, EventArgs e)
        {
            var process = new FormProcess(GitCommands.GitCommands.MergeBranchCmd(Branches.Text, fastForward.Checked));
            process.ShowDialog();

            MergeConflictHandler.HandleMergeConflicts();

            if (!process.ErrorOccured())
                Close();
        }
    }
}