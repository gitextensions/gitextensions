using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
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

        public static FormResetAnotherBranch Create(IGitUICommands commands, GitRevision revision)
            => new(commands, revision ?? throw new NotSupportedException(TranslatedStrings.NoRevision));

        private FormResetAnotherBranch(IGitUICommands commands, GitRevision revision)
            : base(commands)
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
            string currentBranch = Module.GetSelectedBranch();
            bool isDetachedHead = currentBranch == DetachedHeadParser.DetachedBranch;

            List<IGitRef> selectedRevisionRemotes = _revision.Refs.Where(r => r.IsRemote).ToList();

            IGitRef[] resetableLocalRefs = Module.GetRefs(RefsFilter.Heads)
                .Where(r => r.IsHead)
                .Where(r => isDetachedHead || r.LocalName != currentBranch)
                .Where(r => _revision.ObjectId != r.ObjectId) // Don't display local branches already at this revision
                .OrderByDescending(r => selectedRevisionRemotes.Any(r.IsTrackingRemote)) // Put local branches that track these remotes first
                .ThenByDescending(r => selectedRevisionRemotes.Any(r2 => r2.LocalName == r.LocalName)) // Put local branches with same name as remotes first
                .ToArray();

            if (selectedRevisionRemotes.Count == 1)
            {
                IGitRef availableRemote = selectedRevisionRemotes[0];
                IGitRef[] defaultCandidateRefs = resetableLocalRefs
                    .Where(r => r.IsTrackingRemote(availableRemote) || r.LocalName == availableRemote.LocalName).ToArray();
                if (defaultCandidateRefs.Length == 1)
                {
                    Branches.Text = defaultCandidateRefs[0].Name;
                }
            }

            return resetableLocalRefs;
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
            IGitRef gitRefToReset = _localGitRefs.FirstOrDefault(b => b.Name == Branches.Text);
            if (gitRefToReset is null)
            {
                MessageBox.Show(string.Format(_localRefInvalid.Text, Branches.Text), TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ArgumentString command = Commands.PushLocal(gitRefToReset.CompleteName, _revision.ObjectId, Module.WorkingDir, Module.GetPathForGitExecution, ForceReset.Checked);
            bool success = FormProcess.ShowDialog(this, UICommands, arguments: command, Module.WorkingDir, input: null, useDialogSettings: true);
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

        private void Branches_KeyUp(object sender, KeyEventArgs e)
        {
            if (!Branches.DroppedDown && e.KeyCode is not (Keys.Menu or Keys.Enter or Keys.Escape))
            {
                string text = Branches.Text;
                int selectionStart = Branches.SelectionStart;
                int selectionLength = Branches.SelectionLength;

                Branches.DroppedDown = true;

                Branches.Text = text;
                Branches.SelectionStart = selectionStart;
                Branches.SelectionLength = selectionLength;
            }
        }
    }
}
