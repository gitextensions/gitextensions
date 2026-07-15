using Avalonia.Controls;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.Compat;
using GitUI.HelperDialogs;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

// Reduced twin: merge/rebase pull and fetch-only from a selected configured remote.
// URL pulls, remote management, auto-stash, tag/prune/unshallow options, scripts,
// submodule follow-up, and conflict recovery remain with later increments.
public sealed partial class FormPull : GitExtensionsDialog
{
    private const string AllRemotes = "[ All ]";

    private readonly TranslationString _buttonFetch = new("&Fetch");
    private readonly TranslationString _buttonPull = new("&Pull");
    private readonly TranslationString _formTitleFetch = new("Fetch ({0})");
    private readonly TranslationString _formTitlePull = new("Pull ({0})");
    private readonly TranslationString _notOnBranch = new("You cannot \"pull\" when git head detached." +
        Environment.NewLine + Environment.NewLine + "Do you want to continue?");
    private readonly TranslationString _selectRemoteRepository = new("Please select a remote repository");
    private readonly string? _branch;
    private GitPullAction _pullAction;
    private bool _runtimeInitialized;

    public FormPull()
    {
        InitializeComponent();
        InitializeComplete();
    }

    public FormPull(GitUICommands commands, string? defaultRemoteBranch, string? defaultRemote, GitPullAction pullAction)
        : base(commands, enablePositionRestore: false)
    {
        InitializeComponent();

        Pull.Click += PullClick;
        Merge.IsCheckedChanged += PullActionChanged;
        Rebase.IsCheckedChanged += PullActionChanged;
        Fetch.IsCheckedChanged += PullActionChanged;
        _NO_TRANSLATE_Remotes.SelectionChanged += (_, _) => UpdateActionState();
        Branches.TextChanged += (_, _) => UpdateActionState();

        _branch = Module.GetSelectedBranch();
        localBranch.Text = _branch;
        Branches.Text = defaultRemoteBranch ?? GetConfiguredRemoteBranch() ?? _branch;
        BindRemotes(defaultRemote);
        SetPullAction(pullAction);
        _runtimeInitialized = true;

        InitializeComplete();
        RemoveLabelMnemonicMarkers();
        UpdateActionState();
    }

    public bool ErrorOccurred { get; private set; }

    public WinFormsShims.DialogResult PullAndShowDialogWhenFailed(
        WinFormsShims.IWin32Window? owner,
        string? remote,
        GitPullAction pullAction)
    {
        WinFormsShims.DialogResult result = PullChanges(owner);
        return result == WinFormsShims.DialogResult.No
            ? ShowDialog(owner)
            : result;
    }

    public WinFormsShims.DialogResult PullChanges(WinFormsShims.IWin32Window? owner)
    {
        ErrorOccurred = false;
        if (_NO_TRANSLATE_Remotes.SelectedItem is not string remote)
        {
            MessageBoxes.Show(owner, _selectRemoteRepository.Text, TranslatedStrings.Error, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
            return WinFormsShims.DialogResult.No;
        }

        bool fetch = _pullAction is GitPullAction.Fetch or GitPullAction.FetchAll or GitPullAction.FetchPruneAll;
        if (!fetch && (string.IsNullOrWhiteSpace(_branch) || Module.IsDetachedHead()))
        {
            MessageBoxes.Show(owner, _notOnBranch.Text, TranslatedStrings.Error, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
            return WinFormsShims.DialogResult.No;
        }

        if (!fetch && string.IsNullOrWhiteSpace(Branches.Text))
        {
            return WinFormsShims.DialogResult.No;
        }

        string source = remote == AllRemotes ? "--all" : remote;
        ArgumentString arguments = fetch
            ? Module.FetchCmd(
                source,
                remoteBranch: null,
                localBranch: null,
                fetchTags: null,
                isUnshallow: false,
                pruneRemoteBranches: _pullAction == GitPullAction.FetchPruneAll,
                pruneRemoteBranchesAndTags: false)
            : Module.PullCmd(
                source,
                Branches.Text,
                rebase: _pullAction == GitPullAction.Rebase,
                fetchTags: null,
                isUnshallow: false);

        bool success = FormProcess.ShowDialog(
            owner,
            UICommands,
            arguments,
            Module.WorkingDir,
            input: null,
            useDialogSettings: true);
        ErrorOccurred = !success;
        Module.InvalidateGitSettings();
        return success ? WinFormsShims.DialogResult.OK : WinFormsShims.DialogResult.No;
    }

    private void BindRemotes(string? defaultRemote)
    {
        foreach (string remote in Module.GetRemoteNames())
        {
            _NO_TRANSLATE_Remotes.Items.Add(remote);
        }

        string? configuredRemote = defaultRemote;
        if (string.IsNullOrWhiteSpace(configuredRemote) && !string.IsNullOrWhiteSpace(_branch))
        {
            configuredRemote = Module.GetSetting($"branch.{_branch}.remote");
        }

        _NO_TRANSLATE_Remotes.SelectedItem = _NO_TRANSLATE_Remotes.Items
            .OfType<string>()
            .FirstOrDefault(remote => StringComparer.OrdinalIgnoreCase.Equals(remote, configuredRemote))
            ?? _NO_TRANSLATE_Remotes.Items
                .OfType<string>()
                .FirstOrDefault(remote => StringComparer.OrdinalIgnoreCase.Equals(remote, "origin"));
        if (_NO_TRANSLATE_Remotes.SelectedIndex < 0 && _NO_TRANSLATE_Remotes.ItemCount > 0)
        {
            _NO_TRANSLATE_Remotes.SelectedIndex = 0;
        }
    }

    private string? GetConfiguredRemoteBranch()
    {
        if (string.IsNullOrWhiteSpace(_branch))
        {
            return null;
        }

        string? mergeRef = Module.GetSetting($"branch.{_branch}.merge");
        return mergeRef?.StartsWith("refs/heads/", StringComparison.Ordinal) == true
            ? mergeRef["refs/heads/".Length..]
            : mergeRef;
    }

    private void SetPullAction(GitPullAction pullAction)
    {
        if (pullAction is GitPullAction.None or GitPullAction.Default)
        {
            pullAction = AppSettings.DefaultPullAction;
        }

        _pullAction = pullAction switch
        {
            GitPullAction.Rebase => GitPullAction.Rebase,
            GitPullAction.Fetch => GitPullAction.Fetch,
            GitPullAction.FetchAll => GitPullAction.FetchAll,
            GitPullAction.FetchPruneAll => GitPullAction.FetchPruneAll,
            _ => GitPullAction.Merge,
        };

        if (_pullAction is GitPullAction.FetchAll or GitPullAction.FetchPruneAll)
        {
            if (!_NO_TRANSLATE_Remotes.Items.OfType<string>().Contains(AllRemotes, StringComparer.Ordinal))
            {
                _NO_TRANSLATE_Remotes.Items.Insert(0, AllRemotes);
            }

            _NO_TRANSLATE_Remotes.SelectedItem = AllRemotes;
        }

        Merge.IsChecked = _pullAction == GitPullAction.Merge;
        Rebase.IsChecked = _pullAction == GitPullAction.Rebase;
        Fetch.IsChecked = _pullAction is GitPullAction.Fetch or GitPullAction.FetchAll or GitPullAction.FetchPruneAll;
    }

    private void PullActionChanged(object? sender, EventArgs e)
    {
        if (Merge.IsChecked == true)
        {
            _pullAction = GitPullAction.Merge;
        }
        else if (Rebase.IsChecked == true)
        {
            _pullAction = GitPullAction.Rebase;
        }
        else if (Fetch.IsChecked == true && _pullAction is not (GitPullAction.FetchAll or GitPullAction.FetchPruneAll))
        {
            _pullAction = GitPullAction.Fetch;
        }

        UpdateActionState();
    }

    private void PullClick(object? sender, EventArgs e)
    {
        WinFormsShims.DialogResult result = PullChanges(this);
        if (result == WinFormsShims.DialogResult.OK)
        {
            DialogResult = result;
        }
    }

    private void UpdateActionState()
    {
        bool fetch = _pullAction is GitPullAction.Fetch or GitPullAction.FetchAll or GitPullAction.FetchPruneAll;
        Pull.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(fetch ? _buttonFetch.Text : _buttonPull.Text);
        if (_runtimeInitialized)
        {
            Title = string.Format(fetch ? _formTitleFetch.Text : _formTitlePull.Text, Module.WorkingDir);
        }

        localBranch.IsEnabled = fetch;
        Pull.IsEnabled = _NO_TRANSLATE_Remotes.SelectedItem is string
            && (fetch || !string.IsNullOrWhiteSpace(Branches.Text));
    }

    private void RemoveLabelMnemonicMarkers()
    {
        lblLocalBranch.Text = AvaloniaTranslationUtils.RemoveAvaloniaMnemonics(lblLocalBranch.Text ?? string.Empty);
        lblRemoteBranch.Text = AvaloniaTranslationUtils.RemoveAvaloniaMnemonics(lblRemoteBranch.Text ?? string.Empty);
    }

    public override void TranslateItems(GitExtensions.Extensibility.Translations.ITranslation translation)
    {
        base.TranslateItems(translation);
        RemoveLabelMnemonicMarkers();
        UpdateActionState();
    }
}
