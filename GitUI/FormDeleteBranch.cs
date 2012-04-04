using System;
using System.Diagnostics;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;
using System.Collections.Generic;

namespace GitUI
{
    public partial class FormDeleteBranch : GitExtensionsForm
    {
        private readonly string _defaultBranch;
        private readonly TranslationString _deleteBranchCaption = new TranslationString("Delete branches");

        private readonly TranslationString _deleteBranchQuestion =
            new TranslationString(
                "Are you sure you want to delete selected branches?" + Environment.NewLine + "Deleting a branch can cause commits to be deleted too!");

        private List<GitHead> Heads;

        public FormDeleteBranch(string defaultBranch)
        {
            InitializeComponent();
            Translate();
            _defaultBranch = defaultBranch;
        }

        private void FormDeleteBranchLoad(object sender, EventArgs e)
        {
            Heads = Settings.Module.GetHeads(true, true);
            List<GitHead> branchList = Heads.FindAll(h => h.IsHead == true && h.IsRemote == false);
            Branches.BranchesToSelect = branchList;

            if (_defaultBranch != null)
            {
                Branches.SetSelectedText(_defaultBranch);
            }
        }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show(this, _deleteBranchQuestion.Text, _deleteBranchCaption.Text, MessageBoxButtons.YesNo) ==
                    DialogResult.Yes)
                {
                    GitDeleteBranchCmd cmd = new GitDeleteBranchCmd();
                    cmd.Force = ForceDelete.Checked;
                    foreach (GitHead head in Branches.GetSelectedBranches())
                        cmd.AddBranch(head.Name, head.IsRemote);

                    GitUICommands.Instance.StartCommandLineProcessDialog(cmd, this);  
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            Close();
        }
    }
}