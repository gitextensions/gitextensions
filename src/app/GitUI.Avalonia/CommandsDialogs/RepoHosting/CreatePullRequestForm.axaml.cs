using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Threading;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtUtils;
using GitExtUtils.GitUI;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.RepoHosting;

public partial class CreatePullRequestForm : GitModuleForm
{
    private readonly TranslationString _strLoading = new("Loading...");
    private readonly TranslationString _strYouMustSpecifyATitle = new("You must specify a title.");
    private readonly TranslationString _strPullRequest = new("Pull request");
    private readonly TranslationString _strFailedToCreatePullRequest = new("Failed to create pull request.");
    private readonly TranslationString _strPleaseCloneGitHubRep = new("Please clone GitHub repository before pull request.");
    private readonly TranslationString _strDone = new("Done");
    private readonly TranslationString _strRemoteFailToLoadBranches = new("Fail to load target branches");
    private readonly TranslationString _strFailedToLoadTemplate = new("Failed to load PR template from file.");

    private readonly IRepositoryHostPlugin _repoHost = null!;
    private IHostedRemote? _currentHostedRemote;
    private readonly string? _chooseRemote;
    private IReadOnlyList<IHostedRemote>? _hostedRemotes;
    private string? _currentBranch;
    private string? _prevTitle;

    // Avalonia's designer constructs views before the application initializes ThreadHelper.
    private readonly TaskManager _operations = GitUI.Compat.DesignTimeTaskManager.Create();
    private readonly CancellationTokenSequence _targetBranchesSequence = new();
    private readonly CancellationTokenSequence _sourceBranchesSequence = new();
    private readonly CancellationTokenSequence _templateSequence = new();
    private readonly CancellationTokenSource _lifetimeCancellation = new();
    private bool _ignoreFirstRemoteLoading = true;

    public CreatePullRequestForm()
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    public CreatePullRequestForm(
        IGitUICommands commands,
        IRepositoryHostPlugin repoHost,
        string? chooseRemote,
        string? chooseBranch)
        : base(commands, enablePositionRestore: true)
    {
        _repoHost = repoHost;
        _chooseRemote = chooseRemote;
        _currentBranch = chooseBranch;
        InitializeComponent();
        WireControls();
        InitializeComplete();
        _prevTitle = _titleTB.Text ?? string.Empty;
    }

    private void WireControls()
    {
        _pullReqTargetsCB.ItemTemplate = new FuncDataTemplate<IHostedRemote>(
            (remote, _) => new TextBlock { Text = remote?.DisplayData ?? string.Empty },
            supportsRecycling: false);
        _pullReqTargetsCB.SelectionChanged += _pullReqTargetsCB_SelectedIndexChanged;
        _yourBranchesCB.SelectionChanged += _yourBranchCB_SelectedIndexChanged;
        _remoteBranchesCB.SelectionChanged += _yourBranchCB_SelectedIndexChanged;
        _createBtn.Click += _createBtn_Click;
        _createBtn.IsEnabled = false;
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);
        CreatePullRequestForm_Load(this, e);
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="e">The close event data.</param>
    protected override void OnClosed(EventArgs e)
    {
        _lifetimeCancellation.Cancel();
        _targetBranchesSequence.CancelCurrent();
        _sourceBranchesSequence.CancelCurrent();
        _templateSequence.CancelCurrent();
        _operations.JoinPendingOperations();
        _targetBranchesSequence.Dispose();
        _sourceBranchesSequence.Dispose();
        _templateSequence.Dispose();
        _lifetimeCancellation.Dispose();
        base.OnClosed(e);
    }

    private void CreatePullRequestForm_Load(object sender, EventArgs e)
    {
        _createBtn.IsEnabled = false;
        _yourBranchesCB.PlaceholderText = _strLoading.Text;
        _operations.FileAndForget(() => InitializeAsync(_lifetimeCancellation.Token));
    }

    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<IHostedRemote> hostedRemotes = await Task.Run(
            () => _repoHost.GetHostedRemotesForModule(),
            cancellationToken);
        IHostedRemote[] foreignHostedRemotes = hostedRemotes
            .Where(remote => !remote.IsOwnedByMe)
            .ToArray();

        string currentBranch = Module.IsValidGitWorkingDir()
            ? await Task.Run(() => Module.GetSelectedBranch(), cancellationToken)
            : string.Empty;

        await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        _hostedRemotes = hostedRemotes;
        _currentBranch = currentBranch;
        if (foreignHostedRemotes.Length == 0)
        {
            MessageBoxes.Show(
                this,
                _strFailedToCreatePullRequest.Text + Environment.NewLine + _strPleaseCloneGitHubRep.Text,
                string.Empty,
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Error);
            Dispatcher.UIThread.Post(Close);
            return;
        }

        LoadRemotes(foreignHostedRemotes);
        LoadMyBranches();
        LoadPRTemplate();
    }

    private void LoadRemotes(IHostedRemote[] foreignHostedRemotes)
    {
        _pullReqTargetsCB.ItemsSource = foreignHostedRemotes;
        int selectedIndex = !string.IsNullOrEmpty(_chooseRemote)
            ? Array.FindIndex(
                foreignHostedRemotes,
                remote => string.Equals(remote.Name, _chooseRemote, StringComparison.Ordinal))
            : -1;
        _pullReqTargetsCB.SelectedIndex = selectedIndex >= 0 ? selectedIndex : 0;
        _ignoreFirstRemoteLoading = false;
        _pullReqTargetsCB_SelectedIndexChanged(this, EventArgs.Empty);
    }

    private void LoadPRTemplate()
    {
        CancellationToken cancellationToken = _templateSequence.Next();
        _operations.FileAndForget(() => LoadPRTemplateAsync(cancellationToken));
    }

    private async Task LoadPRTemplateAsync(CancellationToken cancellationToken)
    {
        string templatePath = Path.Join(Module.WorkingDir, ".github", "PULL_REQUEST_TEMPLATE.md");
        if (!File.Exists(templatePath))
        {
            return;
        }

        try
        {
            string template = await File.ReadAllTextAsync(templatePath, cancellationToken);
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            _bodyTB.Text = template;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!cancellationToken.IsCancellationRequested)
            {
                MessageBoxes.Show(
                    this,
                    _strFailedToLoadTemplate.Text + Environment.NewLine + ex.Message,
                    TranslatedStrings.Error,
                    WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Error);
            }
        }
    }

    private void _pullReqTargetsCB_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_ignoreFirstRemoteLoading)
        {
            return;
        }

        _currentHostedRemote = _pullReqTargetsCB.SelectedItem as IHostedRemote;
        if (_currentHostedRemote is null)
        {
            _remoteBranchesCB.ItemsSource = Array.Empty<string>();
            return;
        }

        PopulateBranchesComboAndEnableCreateButton(_currentHostedRemote, _remoteBranchesCB);
    }

    private IHostedRemote? MyRemote => _hostedRemotes!.FirstOrDefault(remote => remote.IsOwnedByMe);

    private void LoadMyBranches()
    {
        IHostedRemote? myRemote = MyRemote;
        if (myRemote is null)
        {
            _yourBranchesCB.ItemsSource = Array.Empty<string>();
            return;
        }

        PopulateBranchesComboAndEnableCreateButton(myRemote, _yourBranchesCB);
    }

    private void PopulateBranchesComboAndEnableCreateButton(IHostedRemote remote, ComboBox comboBox)
    {
        bool sourceBranches = ReferenceEquals(comboBox, _yourBranchesCB);
        CancellationTokenSequence sequence = sourceBranches
            ? _sourceBranchesSequence
            : _targetBranchesSequence;
        CancellationToken cancellationToken = sequence.Next();
        comboBox.ItemsSource = Array.Empty<string>();
        comboBox.PlaceholderText = _strLoading.Text;
        _operations.FileAndForget(
            () => PopulateBranchesComboAndEnableCreateButtonAsync(
                remote,
                comboBox,
                sourceBranches ? _currentBranch : null,
                cancellationToken));
    }

    private async Task PopulateBranchesComboAndEnableCreateButtonAsync(
        IHostedRemote remote,
        ComboBox comboBox,
        string? preferredBranch,
        CancellationToken cancellationToken)
    {
        try
        {
            BranchSnapshot snapshot = await Task.Run(
                () =>
                {
                    IHostedRepository repository = remote.GetHostedRepository();
                    IReadOnlyList<IHostedBranch> branches = repository.GetBranches();
                    return new BranchSnapshot(
                        branches.Select(branch => branch.Name).ToArray(),
                        repository.GetDefaultBranch());
                },
                cancellationToken);

            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            string[] branches = snapshot.Branches;
            comboBox.ItemsSource = branches;
            int selectedIndex = !string.IsNullOrEmpty(preferredBranch)
                ? Array.FindIndex(
                    branches,
                    branch => string.Equals(branch, preferredBranch, StringComparison.Ordinal))
                : -1;
            if (selectedIndex < 0)
            {
                selectedIndex = Array.FindIndex(
                    branches,
                    branch => string.Equals(branch, snapshot.DefaultBranch, StringComparison.Ordinal));
            }

            string? previousTitle = _titleTB.Text;
            comboBox.SelectedIndex = selectedIndex >= 0 ? selectedIndex : branches.Length > 0 ? 0 : -1;

            // Avalonia may retain index zero while replacing ItemsSource and omit the original selection event.
            if (ReferenceEquals(comboBox, _yourBranchesCB) && _titleTB.Text == previousTitle)
            {
                _yourBranchCB_SelectedIndexChanged(comboBox, EventArgs.Empty);
            }

            _createBtn.IsEnabled = true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!cancellationToken.IsCancellationRequested)
            {
                comboBox.ItemsSource = Array.Empty<string>();
                MessageBoxes.Show(
                    this,
                    string.Format(TranslatedStrings.RemoteInError, ex.Message, remote.DisplayData),
                    _strRemoteFailToLoadBranches.Text,
                    WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Error);
            }
        }
    }

    private void _yourBranchCB_SelectedIndexChanged(object sender, EventArgs e)
    {
        string yourBranch = _yourBranchesCB.SelectedItem as string ?? _yourBranchesCB.Text ?? string.Empty;
        if (string.Equals(_prevTitle, _titleTB.Text ?? string.Empty, StringComparison.Ordinal)
            && !string.IsNullOrWhiteSpace(yourBranch)
            && MyRemote is not null)
        {
            string? lastMsg = Module.GetPreviousCommitMessages(
                    count: 1,
                    revision: MyRemote.Name.Combine("/", yourBranch)!,
                    authorPattern: string.Empty)
                .FirstOrDefault();
            _titleTB.Text = lastMsg?.SubstringUntil('\n') ?? string.Empty;
            _prevTitle = _titleTB.Text;
        }
    }

    private void _createBtn_Click(object sender, EventArgs e)
    {
        if (_currentHostedRemote is null)
        {
            return;
        }

        string title = _titleTB.Text?.Trim() ?? string.Empty;
        string body = _bodyTB.Text.Trim();
        if (title.Length == 0)
        {
            MessageBoxes.Show(
                this,
                _strYouMustSpecifyATitle.Text,
                TranslatedStrings.Error,
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Error);
            return;
        }

        try
        {
            IHostedRepository hostedRepo = _currentHostedRemote.GetHostedRepository();

            string sourceBranch = _yourBranchesCB.SelectedItem as string ?? _yourBranchesCB.Text ?? string.Empty;
            string targetBranch = _remoteBranchesCB.SelectedItem as string ?? _remoteBranchesCB.Text ?? string.Empty;
            hostedRepo.CreatePullRequest(sourceBranch, targetBranch, title, body);
            MessageBoxes.Show(
                this,
                _strDone.Text,
                _strPullRequest.Text,
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Information);
            Close();
        }
        catch (Exception ex)
        {
            MessageBoxes.Show(
                this,
                _strFailedToCreatePullRequest.Text + Environment.NewLine + ex.Message,
                TranslatedStrings.Error,
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Error);
        }
    }

    // parity-scaffolding: Exposes repository-host state and actions to the cross-platform parity suite.
    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(CreatePullRequestForm form)
    {
        public ComboBox TargetRepositories => form._pullReqTargetsCB;

        public ComboBox SourceBranches => form._yourBranchesCB;

        public ComboBox TargetBranches => form._remoteBranchesCB;

        public string Title
        {
            get => form._titleTB.Text ?? string.Empty;
            set => form._titleTB.Text = value;
        }

        public string Body
        {
            get => form._bodyTB.Text;
            set => form._bodyTB.Text = value;
        }

        public bool CreateEnabled => form._createBtn.IsEnabled;

        public Task InitializeAsync(CancellationToken cancellationToken = default)
            => form.InitializeAsync(cancellationToken);

        public Task JoinOperationsAsync(CancellationToken cancellationToken = default)
            => form._operations.JoinPendingOperationsAsync(cancellationToken);

        public void Create() => form._createBtn_Click(form, EventArgs.Empty);
    }

    private sealed record BranchSnapshot(string[] Branches, string DefaultBranch);
}
