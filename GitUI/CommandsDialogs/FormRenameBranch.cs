﻿using System.Diagnostics;
using GitCommands;
using GitCommands.Git;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormRenameBranch : GitModuleForm
    {
        private readonly TranslationString _branchRenameFailed = new("Rename failed.");
        private readonly IGitBranchNameNormaliser _branchNameNormaliser;
        private readonly GitBranchNameOptions _gitBranchNameOptions = new(AppSettings.AutoNormaliseSymbol);
        private readonly string _oldName;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private FormRenameBranch()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            InitializeComponent();
        }

        public FormRenameBranch(GitUICommands commands, string defaultBranch)
            : base(commands)
        {
            _branchNameNormaliser = new GitBranchNameNormaliser();

            InitializeComponent();
            InitializeComplete();
            BranchNameTextBox.Text = defaultBranch;
            _oldName = defaultBranch;
        }

        private void BranchNameTextBox_Leave(object sender, EventArgs e)
        {
            if (!AppSettings.AutoNormaliseBranchName || !BranchNameTextBox.Text.Any(GitBranchNameNormaliser.IsValidChar))
            {
                return;
            }

            var caretPosition = BranchNameTextBox.SelectionStart;
            var branchName = _branchNameNormaliser.Normalise(BranchNameTextBox.Text, _gitBranchNameOptions);
            BranchNameTextBox.Text = branchName;
            BranchNameTextBox.SelectionStart = caretPosition;
        }

        private void OkClick(object sender, EventArgs e)
        {
            // Ok button set as the "AcceptButton" for the form
            // if the user hits [Enter] at any point, we need to trigger BranchNameTextBox Leave event
            Ok.Focus();

            var newName = BranchNameTextBox.Text;

            if (newName == _oldName)
            {
                DialogResult = DialogResult.Cancel;
                return;
            }

            try
            {
                var renameBranchResult = Module.RenameBranch(_oldName, newName);

                if (!string.IsNullOrEmpty(renameBranchResult))
                {
                    MessageBox.Show(this, _branchRenameFailed.Text + Environment.NewLine + renameBranchResult, Text,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            DialogResult = DialogResult.OK;
        }
    }
}
