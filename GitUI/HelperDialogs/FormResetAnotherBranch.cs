using System;
using System.Linq;
using System.Windows.Forms;
using GitCommands.Git;
using GitCommands.Git.Commands;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.HelperDialogs
{
    public partial class FormResetAnotherBranch : GitModuleForm
    {
        private IGitRef[]? _localGitRefs;
        private readonly GitRevision _revision;
        private readonly TranslationString _localRefInvalid = new("The entered value '{0}' is not the name of an existing local branch.");

        public static FormResetAnotherBranch Create(GitUICommands gitUiCommands, GitRevision revision)
            => new(gitUiCommands, revision ?? throw new NotSupportedException(TranslatedStrings.NoRevision));

        [Obsolete("For VS designer and translation test only. Do not remove.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private FormResetAnotherBranch()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            InitializeComponent();
        }

        private FormResetAnotherBranch(GitUICommands gitUiCommands, GitRevision revision)
            : base(gitUiCommands)
        {
            _revision = revision;

            InitializeComponent();

            pictureBox1.Image = DpiUtil.Scale(pictureBox1.Image);
            labelResetBranchWarning.AutoSize = true;
            labelResetBranchWarning.Dock = DockStyle.Fill;

            Height = tableLayoutPanel1.Height + tableLayoutPanel1.Top;
            tableLayoutPanel1.Dock = DockStyle.Fill;

            ActiveControl = Branches;

            InitializeComplete();

            labelResetBranchWarning.SetForeColorForBackColor();
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            Application.Idle -= Application_Idle;

            if (Branches.Text.Length == 0)
            {
                Branches.DroppedDown = true;
            }
        }

        private IGitRef[] GetLocalBranchesWithoutCurrent()
        {
            var currentBranch = Module.GetSelectedBranch();
            var isDetachedHead = currentBranch == DetachedHeadParser.DetachedBranch;

            var selectedRevisionRemotes = _revision.Refs.Where(r => r.IsRemote).ToList();

            return Module.GetRefs(RefsFilter.Heads)
                .Where(r => r.IsHead)
                .Where(r => isDetachedHead || r.LocalName != currentBranch)
                .OrderByDescending(r => selectedRevisionRemotes.Any(r.IsTrackingRemote)) // Put local branches that track these remotes first
                .ToArray();
        }

        private void FormResetAnotherBranch_Load(object sender, EventArgs e)
        {
            _localGitRefs = GetLocalBranchesWithoutCurrent();

            Branches.DisplayMember = nameof(IGitRef.Name);
            Branches.Items.AddRange(_localGitRefs);

            commitSummaryUserControl.Revision = _revision;

            Application.Idle += Application_Idle;
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            var gitRefToReset = _localGitRefs.FirstOrDefault(b => b.Name == Branches.Text);
            if (gitRefToReset is null)
            {
                MessageBox.Show(string.Format(_localRefInvalid.Text, Branches.Text), TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var command = GitCommandHelpers.PushLocalCmd(gitRefToReset.CompleteName, _revision.ObjectId, ForceReset.Checked);
            bool success = FormProcess.ShowDialog(this, process: null, arguments: command, Module.WorkingDir, input: null, useDialogSettings: true);
            if (success)
            {
                UICommands.RepoChangedNotifier.Notify();
                Close();
            }
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
