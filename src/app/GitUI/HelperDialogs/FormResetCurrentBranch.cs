using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI.Theming;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.HelperDialogs
{
    public partial class FormResetCurrentBranch : GitModuleForm
    {
        private readonly TranslationString _branchInfo = new("Reset branch '{0}' to revision:");
        private readonly TranslationString _resetHardWarning = new("You are about to discard ALL local changes, are you sure?");
        private readonly TranslationString _resetCaption = new("Reset branch");

        public enum ResetType
        {
            Soft,
            Mixed,
            Keep,
            Merge,
            Hard
        }

        public static FormResetCurrentBranch Create(IGitUICommands commands, GitRevision revision, ResetType resetType = ResetType.Soft)
            => new(commands, revision ?? throw new NotSupportedException(TranslatedStrings.NoRevision), resetType);

        private FormResetCurrentBranch(IGitUICommands commands, GitRevision revision, ResetType resetType)
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

        public GitRevision Revision { get; }

        private void FormResetCurrentBranch_Load(object sender, EventArgs e)
        {
            _NO_TRANSLATE_BranchInfo.Text = string.Format(_branchInfo.Text, Module.GetSelectedBranch());
            commitSummaryUserControl1.Revision = Revision;
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            if (Soft.Checked)
            {
                FormProcess.ShowDialog(this, UICommands, arguments: Commands.Reset(ResetMode.Soft, Revision.Guid), Module.WorkingDir, input: null, useDialogSettings: true);
            }
            else if (Mixed.Checked)
            {
                FormProcess.ShowDialog(this, UICommands, arguments: Commands.Reset(ResetMode.Mixed, Revision.Guid), Module.WorkingDir, input: null, useDialogSettings: true);
            }
            else if (Hard.Checked)
            {
                if (MessageBox.Show(this, _resetHardWarning.Text, _resetCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    ObjectId currentCheckout = Module.GetCurrentCheckout();
                    bool success = FormProcess.ShowDialog(this, UICommands, arguments: Commands.Reset(ResetMode.Hard, Revision.Guid), Module.WorkingDir, input: null, useDialogSettings: true);
                    if (success)
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
                FormProcess.ShowDialog(this, UICommands, arguments: Commands.Reset(ResetMode.Merge, Revision.Guid), Module.WorkingDir, input: null, useDialogSettings: true);
            }
            else if (Keep.Checked)
            {
                FormProcess.ShowDialog(this, UICommands, arguments: Commands.Reset(ResetMode.Keep, Revision.Guid), Module.WorkingDir, input: null, useDialogSettings: true);
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
            string? helpSection = default;
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

            OsShellUtil.OpenUrlInDefaultBrowser(@$"https://git-scm.com/docs/git-reset#Documentation/git-reset.txt-{helpSection}");
        }
    }
}
