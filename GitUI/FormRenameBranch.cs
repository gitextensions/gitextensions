using System;
using System.Diagnostics;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormRenameBranch : GitExtensionsForm
    {
        private readonly TranslationString _branchRenamed = new TranslationString("Command executed");

        private readonly string _defaultBranch;
        private readonly TranslationString _renameBranchCaption = new TranslationString("Rename branch");

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
                MessageBox.Show(this, this._branchRenamed.Text + Environment.NewLine + renameBranchResult,
                                this._renameBranchCaption.Text);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            Close();
        }
    }
}