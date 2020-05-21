using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.HelperDialogs
{
    public partial class FormResetAnotherBranch : GitModuleForm
    {
        private IGitRef[] _localGitRefs;
        private readonly GitRevision _revision;
        private readonly TranslationString _localRefInvalid = new TranslationString("The entered value '{0}' is not the name of an existing local branch.");

        public static FormResetAnotherBranch Create(GitUICommands gitUiCommands, GitRevision revision)
            => new FormResetAnotherBranch(gitUiCommands, revision ?? throw new NotSupportedException(Strings.NoRevision));

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormResetAnotherBranch()
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

            return Module.GetRefs(false)
                .Where(r => r.IsHead && (isDetachedHead || r.LocalName != currentBranch))
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
            if (gitRefToReset == null)
            {
                MessageBox.Show(string.Format(_localRefInvalid.Text, Branches.Text), Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var repoPath = Path.GetFullPath(Module.WorkingDir);
            var args = GitCommandHelpers.PushLocalCmd(repoPath, gitRefToReset.CompleteName, _revision.ObjectId, ForceReset.Checked);
            var isSuccess = FormProcess.ShowDialog(this, args);

            if (isSuccess)
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
