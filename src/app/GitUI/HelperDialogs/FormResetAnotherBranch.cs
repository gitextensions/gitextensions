using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.HelperDialogs;

public partial class FormResetAnotherBranch : GitModuleForm
{
    private readonly CancellationTokenSequence _cancellationTokenSequence = new();
    private readonly TranslationString _localRefInvalid = new("The entered value '{0}' is not the name of an existing local branch.");
    private readonly GitRevision _revision;

    private IGitRef[]? _localGitRefs;
    private string? _validatedBranch;

    public static FormResetAnotherBranch Create(IGitUICommands commands, GitRevision revision)
        => new(commands, revision ?? throw new NotSupportedException(TranslatedStrings.NoRevision));

    private FormResetAnotherBranch(IGitUICommands commands, GitRevision revision)
        : base(commands)
    {
        _revision = revision;

        InitializeComponent();

        pictureBox1.Image = DpiUtil.Scale(pictureBox1.Image);
        lblResetBranchWarning.AutoSize = true;
        lblResetBranchWarning.Dock = DockStyle.Fill;
        lblResetBranchWarning.SetForeColorForBackColor();

        Height = tableLayoutPanel1.Height + tableLayoutPanel1.Top;
        tableLayoutPanel1.Dock = DockStyle.Fill;

        ActiveControl = Branches;

        InitializeComplete();

        cbxCheckoutBranch.Checked = AppSettings.CheckoutOtherBranchAfterReset.Value;
        cbxCheckoutBranch.CheckedChanged += (s, e) => AppSettings.CheckoutOtherBranchAfterReset.Value = cbxCheckoutBranch.Checked;

        Ok.Enabled = false;
    }

    private void Application_Idle(object sender, EventArgs e)
    {
        Application.Idle -= Application_Idle;

        if (Branches.Text.Length == 0)
        {
            Branches.DroppedDown = true;
        }
    }

    private void InitLocalBranchesWithoutCurrent()
    {
        string currentBranch = Module.GetSelectedBranch();
        bool isDetachedHead = currentBranch == DetachedHeadParser.DetachedBranch;

        List<IGitRef> selectedRevisionRemotes = _revision.Refs.Where(r => r.IsRemote).ToList();

        _localGitRefs = Module.GetRefs(RefsFilter.Heads)
            .Where(r => r.IsHead)
            .Where(r => isDetachedHead || r.LocalName != currentBranch)
            .Where(r => _revision.ObjectId != r.ObjectId) // Don't display local branches already at this revision
            .OrderByDescending(r => selectedRevisionRemotes.Any(r.IsTrackingRemote)) // Put local branches that track these remotes first
            .ThenByDescending(r => selectedRevisionRemotes.Any(r2 => r2.LocalName == r.LocalName)) // Put local branches with same name as remotes first
            .ToArray();

        if (selectedRevisionRemotes.Count == 1)
        {
            IGitRef availableRemote = selectedRevisionRemotes[0];
            IGitRef[] defaultCandidateRefs = _localGitRefs
                .Where(r => r.IsTrackingRemote(availableRemote) || r.LocalName == availableRemote.LocalName).ToArray();
            if (defaultCandidateRefs.Length == 1)
            {
                Branches.Text = defaultCandidateRefs[0].Name;
            }
        }
    }

    private void FormResetAnotherBranch_Load(object sender, EventArgs e)
    {
        InitLocalBranchesWithoutCurrent();

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

        ArgumentString command = Commands.UpdateRef(gitRefToReset.CompleteName, _revision.ObjectId);
        bool success = FormProcess.ShowDialog(this, UICommands, arguments: command, Module.WorkingDir, input: null, useDialogSettings: true);
        if (success)
        {
            if (cbxCheckoutBranch.Checked)
            {
                UICommands.StartCheckoutBranch(this, gitRefToReset.Name);
            }

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

    private void Validate(object sender, EventArgs e)
    {
        string branch = Branches.Text;

        if (_localGitRefs is null || (branch == _validatedBranch && !ForceReset.Checked))
        {
            return;
        }

        _validatedBranch = null;
        CancellationToken cancellationToken = _cancellationTokenSequence.Next();

        IGitRef? gitRefToReset = _localGitRefs.FirstOrDefault(b => b.Name == branch);
        Branches.BackColor = gitRefToReset is null && ActiveControl != Branches ? Color.LightCoral.AdaptBackColor() : SystemColors.Window;

        Ok.Enabled = gitRefToReset is not null && ForceReset.Checked;
        Ok.BackColor = SystemColors.ButtonFace;

        if (gitRefToReset is null || ForceReset.Checked)
        {
            return;
        }

        _validatedBranch = branch;

        ThreadHelper.FileAndForget(async () =>
        {
            ArgumentString command = Commands.PushLocal(gitRefToReset.CompleteName, _revision.ObjectId, Module.WorkingDir, Module.GetPathForGitExecution, ForceReset.Checked, dryRun: true);
            ExecutionResult executionResult = await Module.GitExecutable.ExecuteAsync(command, throwOnErrorExit: false, cancellationToken: cancellationToken);

            await this.SwitchToMainThreadAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Ok.Enabled = executionResult.ExitedSuccessfully;
            if (!executionResult.ExitedSuccessfully)
            {
                Ok.BackColor = Color.LightCoral.AdaptBackColor();
            }
        });
    }
}
