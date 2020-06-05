using System;
using System.Diagnostics;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI.Theming;
using ResourceManager;

namespace GitUI.HelperDialogs
{
    public partial class FormResetCurrentBranch : GitModuleForm
    {
        private readonly TranslationString _branchInfo = new TranslationString("Reset branch '{0}' to revision:");
        private readonly TranslationString _resetHardWarning = new TranslationString("You are about to discard ALL local changes, are you sure?");
        private readonly TranslationString _resetCaption = new TranslationString("Reset branch");

        public enum ResetType
        {
            Soft,
            Mixed,
            Keep,
            Merge,
            Hard
        }

        public static FormResetCurrentBranch Create(GitUICommands commands, GitRevision revision, ResetType resetType = ResetType.Mixed)
            => new FormResetCurrentBranch(commands, revision ?? throw new NotSupportedException(Strings.NoRevision), resetType);

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormResetCurrentBranch()
        {
            InitializeComponent();
        }

        private FormResetCurrentBranch(GitUICommands commands, GitRevision revision, ResetType resetType)
            : base(commands)
        {
            Revision = revision;

            InitializeComponent();
            Soft.SetForeColorForBackColor();
            Hard.SetForeColorForBackColor();
            Mixed.SetForeColorForBackColor();
            Merge.SetForeColorForBackColor();
            Keep.SetForeColorForBackColor();
            InitializeComplete();

            switch (resetType)
            {
                case ResetType.Soft:
                    Soft.Checked = true;
                    break;
                case ResetType.Mixed:
                    Mixed.Checked = true;
                    break;
                case ResetType.Keep:
                    Keep.Checked = true;
                    break;
                case ResetType.Merge:
                    Merge.Checked = true;
                    break;
                case ResetType.Hard:
                    Hard.Checked = true;
                    break;
            }
        }

        public GitRevision Revision { get; set; }

        private void FormResetCurrentBranch_Load(object sender, EventArgs e)
        {
            _NO_TRANSLATE_BranchInfo.Text = string.Format(_branchInfo.Text, Module.GetSelectedBranch());
            commitSummaryUserControl1.Revision = Revision;
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            if (Soft.Checked)
            {
                FormProcess.ShowDialog(this, GitCommandHelpers.ResetCmd(ResetMode.Soft, Revision.Guid));
            }
            else if (Mixed.Checked)
            {
                FormProcess.ShowDialog(this, GitCommandHelpers.ResetCmd(ResetMode.Mixed, Revision.Guid));
            }
            else if (Hard.Checked)
            {
                if (MessageBox.Show(this, _resetHardWarning.Text, _resetCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    var currentCheckout = Module.GetCurrentCheckout();
                    if (FormProcess.ShowDialog(this, GitCommandHelpers.ResetCmd(ResetMode.Hard, Revision.Guid)))
                    {
                        if (currentCheckout != Revision.ObjectId)
                        {
                            UICommands.UpdateSubmodules(this);
                        }
                    }
                }
                else
                {
                    return;
                }
            }
            else if (Merge.Checked)
            {
                FormProcess.ShowDialog(this, GitCommandHelpers.ResetCmd(ResetMode.Merge, Revision.Guid));
            }
            else if (Keep.Checked)
            {
                FormProcess.ShowDialog(this, GitCommandHelpers.ResetCmd(ResetMode.Keep, Revision.Guid));
            }

            UICommands.RepoChangedNotifier.Notify();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void FormResetCurrentBranch_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string helpSection = default;
            if (Soft.Checked)
            {
                helpSection = "--soft";
            }
            else if (Mixed.Checked)
            {
                helpSection = "--mixed";
            }
            else if (Keep.Checked)
            {
                helpSection = "--keep";
            }
            else if (Merge.Checked)
            {
                helpSection = "--merge";
            }
            else if (Hard.Checked)
            {
                helpSection = "--hard";
            }

            Process.Start($"https://git-scm.com/docs/git-reset#Documentation/git-reset.txt-{helpSection}");
        }
    }
}
