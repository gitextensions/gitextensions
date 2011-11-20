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

        public FormRenameBranch(string defaultBranch)
        {
            InitializeComponent();
            Translate();
            Branches.Text = defaultBranch;
            _defaultBranch = defaultBranch;
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