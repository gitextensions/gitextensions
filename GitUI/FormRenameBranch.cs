using System;
using System.Diagnostics;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;
using System.Collections.Generic;

namespace GitUI
{
    public partial class FormRenameBranch : GitExtensionsForm
    {
        private readonly TranslationString _branchDeleted = new TranslationString("Command executed");

        private readonly string _defaultBranch;
        private readonly TranslationString _deleteBranchCaption = new TranslationString("Delete branch");

        private readonly TranslationString _deleteBranchQuestion =
            new TranslationString(
                "Are you sure you want to delete this branch?" + Environment.NewLine + "Deleting a branch can cause commits to be deleted too!");

        private List<GitHead> Heads;

        public FormRenameBranch(string defaultBranch)
        {
            InitializeComponent();
            Translate();
            _defaultBranch = defaultBranch;
        }

        private void FormDeleteBranchLoad(object sender, EventArgs e)
        {
            Branches.DisplayMember = "Name";
            Heads = Settings.Module.GetHeads(true, true);
            Branches.DataSource = Heads.FindAll(h => h.IsHead == true && h.IsRemote == false);

            if (_defaultBranch != null)
                Branches.Text = _defaultBranch;
        }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                var renameBranchResult = Settings.Module.Rename(_defaultBranch, Branches.Text);
                MessageBox.Show(this, _branchDeleted.Text + Environment.NewLine + renameBranchResult,
                                _deleteBranchCaption.Text);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            Close();
        }
    }
}