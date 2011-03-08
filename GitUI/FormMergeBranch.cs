using System;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormMergeBranch : GitExtensionsForm
    {
        private readonly TranslationString _strategyTooltipText = new TranslationString("resolve " + Environment.NewLine + "This can only resolve two heads (i.e. the current branch and another branch you pulled from) using a 3-way merge algorithm." + Environment.NewLine + "It tries to carefully detect criss-cross merge ambiguities and is considered generally safe and fast. " + Environment.NewLine + "" + Environment.NewLine + "recursive " + Environment.NewLine + "This can only resolve two heads using a 3-way merge algorithm. When there is more than one common ancestor that can be " + Environment.NewLine + "used for 3-way merge, it creates a merged tree of the common ancestors and uses that as the reference tree for the 3-way" + Environment.NewLine + "merge. Additionally this can detect and handle merges involving renames. This is the default merge strategy when pulling or " + Environment.NewLine + "merging one branch. " + Environment.NewLine + "" + Environment.NewLine + "octopus " + Environment.NewLine + "This resolves cases with more than two heads, but refuses to do a complex merge that needs manual resolution. It is " + Environment.NewLine + "primarily meant to be used for bundling topic branch heads together. This is the default merge strategy when pulling or " + Environment.NewLine + "merging more than one branch. " + Environment.NewLine + "" + Environment.NewLine + "ours " + Environment.NewLine + "This resolves any number of heads, but the resulting tree of the merge is always that of the current branch head, effectively " + Environment.NewLine + "ignoring all changes from all other branches. It is meant to be used to supersede old development history of side branches." + Environment.NewLine + "" + Environment.NewLine + "subtree " + Environment.NewLine + "This is a modified recursive strategy. When merging trees A and B, if B corresponds to a subtree of A, B is first adjusted to " + Environment.NewLine + "match the tree structure of A, instead of reading the trees at the same level. This adjustment is also done to the common " + Environment.NewLine + "ancestor tree. ");
        private readonly string _defaultBranch;

        public FormMergeBranch(string defaultBranch)
        {
            InitializeComponent();
            Translate();
            _defaultBranch = defaultBranch;

            advanced_CheckedChanged(null, null);
        }

        private void FormMergeBranchLoad(object sender, EventArgs e)
        {
            var selectedHead = GitCommandHelpers.GetSelectedBranch();
            currentBranchLabel.Text = selectedHead;

            Branches.DisplayMember = "Name";
            Branches.DataSource = GitCommandHelpers.GetHeads(true, true);

            if (_defaultBranch != null)
                Branches.Text = _defaultBranch;

            Branches.Select();
        }

        private void OkClick(object sender, EventArgs e)
        {
            var process = new FormProcess(GitCommandHelpers.MergeBranchCmd(Branches.Text, fastForward.Checked, squash.Checked, noCommit.Checked, _NO_TRANSLATE_mergeStrategy.Text));
            process.ShowDialog();

            MergeConflictHandler.HandleMergeConflicts();

            if (!process.ErrorOccurred())
                Close();
        }

        private void NonDefaultMergeStrategy_CheckedChanged(object sender, EventArgs e)
        {
            _NO_TRANSLATE_mergeStrategy.Visible = NonDefaultMergeStrategy.Checked;
            strategyHelp.Visible = NonDefaultMergeStrategy.Checked;

            if (!_NO_TRANSLATE_mergeStrategy.Visible)
                _NO_TRANSLATE_mergeStrategy.Text = "";
        }

        private void strategyHelp_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            strategyToolTip.SetToolTip(strategyHelp, _strategyTooltipText.Text);
        }

        private void advanced_CheckedChanged(object sender, EventArgs e)
        {
            if (advanced.Checked)
                this.Height = 320;
            else
                this.Height = 280;

            NonDefaultMergeStrategy.Visible = advanced.Checked;
            NonDefaultMergeStrategy_CheckedChanged(null, null);
            squash.Visible = advanced.Checked;
            noCommit.Visible = advanced.Checked;

            if (!advanced.Checked)
            {
                NonDefaultMergeStrategy.Checked = false;
                squash.Checked = false;
                noCommit.Checked = false;
            }
        }
    }
}