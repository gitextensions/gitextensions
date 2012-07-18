using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public sealed partial class FormDeleteBranch : GitExtensionsForm
    {
        private readonly string _defaultBranch;
        private readonly TranslationString _deleteBranchCaption = new TranslationString("Delete branches");

        private readonly TranslationString _deleteBranchQuestion = new TranslationString(
                "Are you sure you want to delete selected branches?" + Environment.NewLine + "Deleting a branch can cause commits to be deleted too!");

        public FormDeleteBranch(string defaultBranch)
        {
            InitializeComponent();
            Translate();
            _defaultBranch = defaultBranch;
        }

        private void FormDeleteBranchLoad(object sender, EventArgs e)
        {
            Branches.BranchesToSelect = Settings.Module.GetHeads(true, true).Where(h => h.IsHead && !h.IsRemote).ToList();

            if (_defaultBranch != null)
                Branches.SetSelectedText(_defaultBranch);
        }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show(this, _deleteBranchQuestion.Text, _deleteBranchCaption.Text, MessageBoxButtons.YesNo) ==
                    DialogResult.Yes)
                {
                    var cmd = new GitDeleteBranchCmd(Branches.GetSelectedBranches(), ForceDelete.Checked);
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