using System;
using GitCommands;
using ResourceManager.Translation;
using System.Text;

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
            btnStrategy.ToolTipText = _strategyTooltipText.Text;
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
            tbxAdvMergeOptions.Text = GitCommands.Settings.AdvancedmargeCmd;
        }

        private void AddMergeOption(string[] options, string[] values, string[] conflitwith)
        {
            string mergeOption = tbxAdvMergeOptions.Text;
            string option1;
            int pos=-1;
            int lengthOption=-1;
            int nextpos=-1;
            //Build new merge command
            for (int i = 0; i<options.Length; i++)
            {
                if (values != null)
                {
                    option1 = "--" + options[i] + "=";
                    pos = mergeOption.IndexOf(option1);
                    if (pos > -1)
                    {
                        lengthOption = option1.Length;
                        nextpos = mergeOption.IndexOf("--", pos + 1);
                        if (nextpos == -1)
                            nextpos = mergeOption.Length;
                        else
                            nextpos--;
                        mergeOption = mergeOption.Remove(pos, nextpos - pos);
                        mergeOption = mergeOption.Insert(pos, option1 + values[i]);
                    }
                    else
                        mergeOption += " " + option1 + values[i];

                }
                else
                {
                    option1 = "--" + options[i];
                    pos = mergeOption.IndexOf(option1);
                    if (pos == -1)
                        mergeOption += " " + option1;
                }

            }
            //remove conflit option
            if (conflitwith != null)
                for (int i = 0; i < conflitwith.Length; i++)
                {
                    option1 = "--" + conflitwith[i];
                    pos = mergeOption.IndexOf(option1);
                    if (pos > -1)
                    {
                        lengthOption = option1.Length;
                        nextpos = mergeOption.IndexOf("--", pos + 1);
                        if (nextpos == -1)
                            nextpos = mergeOption.Length;
                        else
                            nextpos--;
                        if (pos > 0)
                            pos--;//for delete space before --
                        mergeOption = mergeOption.Remove(pos, nextpos - pos);
                    }

                }
            tbxAdvMergeOptions.Text = mergeOption;
        }

        private void OkClick(object sender, EventArgs e)
        {
            GitCommandHelpers.AllowMerge merge = GitCommandHelpers.AllowMerge.FastForward;
            if (noFastForward.Checked)
                merge = GitCommandHelpers.AllowMerge.NoFastForward;
            else if (advMergeOptions.Checked)
                merge = GitCommandHelpers.AllowMerge.Advanced;

            var process = new FormProcess(GitCommandHelpers.MergeBranchCmd(Branches.Text, merge, tbxAdvMergeOptions.Text));
            process.ShowDialog();

            MergeConflictHandler.HandleMergeConflicts();

            if (!process.ErrorOccurred())
                Close();
            GitCommands.Settings.AdvancedmargeCmd = tbxAdvMergeOptions.Text;
        }

         private void btnStrategy_ButtonClick(object sender, EventArgs e)
        {
        }

        private void advMergeOptions_CheckedChanged(object sender, EventArgs e)
        {
            gbxAdvMergeOptions.Visible = advMergeOptions.Checked;
        }

        private void strategyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.ToolStripMenuItem tsmiSender = (System.Windows.Forms.ToolStripMenuItem)sender;
            AddMergeOption(new string[] { "strategy" }, new string[] { tsmiSender.Text }, null); 
        }

        private void btnSquash_Click(object sender, EventArgs e)
        {
            AddMergeOption(new string[] { "squash" }, null, new string[] { "no-ff" }); 
        }

        private void btnNoCommit_Click(object sender, EventArgs e)
        {
            AddMergeOption(new string[] { "no-ff", "no-commit" }, null, new string[] { "squash" }); 
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            tbxAdvMergeOptions.Text = "";
        }

        private void btnnoff_Click(object sender, EventArgs e)
        {
            AddMergeOption(new string[] { "no-ff"}, null, new string[] { "no-commit" , "squash" }); 
        }


    }
}