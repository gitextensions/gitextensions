using System;
using System.Diagnostics;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public sealed partial class FormRenameBranch : GitExtensionsForm
    {
        private readonly TranslationString _branchRenameFailed = new TranslationString("Rename failed.");

        private readonly string oldName;

        public FormRenameBranch(string defaultBranch)
        {
            InitializeComponent();
            Translate();
            Branches.Text = defaultBranch;
            oldName = defaultBranch;
        }

        private void OkClick(object sender, EventArgs e)
        {
            var newName = Branches.Text;
            if (newName.Equals(oldName))
            {
                DialogResult = DialogResult.Cancel;
                return;
            }

            try
            {
                var renameBranchResult = Settings.Module.Rename(oldName, newName);

                if (!string.IsNullOrEmpty(renameBranchResult))
                    MessageBox.Show(this, _branchRenameFailed.Text + Environment.NewLine + renameBranchResult, Text,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            DialogResult = DialogResult.OK;
        }
    }
}