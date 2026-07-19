using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.Remotes;
using GitCommands.Settings;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI.Compat;
using GitUI.HelperDialogs;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

public sealed partial class FormPush : GitModuleForm
{
    private const string HeadText = "HEAD";
    private const string AllRefs = "[ All ]";
    private const string LocalColumnName = "Local";
    private const string RemoteColumnName = "Remote";
    private const string AheadColumnName = "New";
    private const string PushColumnName = "Push";
    private const string ForceColumnName = "Force";
    private const string DeleteColumnName = "Delete";
    private const string BranchTabToolTip = "Push branches and commits to remote repository.";
    private const string TagTabToolTip = "Push tags to remote repository";
    private const string PushToRemoteToolTip = "Remote repository to push to";
    private const string PushToUrlToolTip = "Url to push to";

    private readonly TranslationString _branchNewForRemote = new(
        "The branch you are about to push seems to be a new branch for the remote."
        + Environment.NewLine + "Are you sure you want to push this branch?");
    private readonly TranslationString _noCurrentBranch = new("No branch is selected, cannot push.");
    private readonly TranslationString _pushCaption = new("Push");
    private readonly TranslationString _pushToCaption = new("Push to {0}");
    private readonly TranslationString _selectDestinationDirectory = new("Please select a destination directory");
    private readonly TranslationString _errorPushToRemoteCaption = new("Push to remote");
    private readonly TranslationString _configureRemote = new(
        $"Please configure a remote repository first.{Environment.NewLine}Would you like to do it now?");
    private readonly TranslationString _selectTag = new("You need to select a tag to push or select \"Push all tags\".");
    private readonly TranslationString _updateTrackingReference = new(
        "The branch {0} does not have a tracking reference. Do you want to add a tracking reference to {1}?");
    private readonly TranslationString _pullRepositoryMainMergeInstruction = new("Pull latest changes from remote repository");
    private readonly TranslationString _pullRepositoryMainForceInstruction = new("Push rejected");
    private readonly TranslationString _pullRepositoryMergeInstruction = new(
        "The push was rejected because the tip of your current branch is behind its remote counterpart. "
        + "Merge the remote changes before pushing again.");
    private readonly TranslationString _pullRepositoryForceInstruction = new(
        "The push was rejected because the tip of your current branch is behind its remote counterpart");
    private readonly TranslationString _pullDefaultButton = new("&Pull with the default pull action ({0})");
    private readonly TranslationString _pullRebaseButton = new("Pull with &rebase");
    private readonly TranslationString _pullMergeButton = new("Pull with &merge");
    private readonly TranslationString _pushForceButton = new("&Force push with lease");
    private readonly TranslationString _pullActionNone = new("none");
    private readonly TranslationString _pullActionFetch = new("fetch");
    private readonly TranslationString _pullActionRebase = new("rebase");
    private readonly TranslationString _pullActionMerge = new("merge");
    private readonly TranslationString _pullRepositoryCaption = new("Push was rejected from \"{0}\"");
    private readonly TranslationString _useForceWithLeaseInstead = new(
        "Force push may overwrite changes since your last fetch. Do you want to use the safer force with lease instead?");
    private readonly TranslationString _forceWithLeaseTooltips = new(
        "Force with lease is a safer way to force push. It ensures you only overwrite work that you have seen in your local repository");

    private readonly IConfigFileRemoteSettingsManager _remotesManager = null!;
    private IReadOnlyList<IGitRef> _gitRefs = [];
    private readonly List<BranchPushRow> _branchRows = [];
    private List<ConfigFileRemote> _userGitRemotes = [];
    private string? _currentBranchName;
    private ConfigFileRemote? _currentBranchRemote;
    private ConfigFileRemote? _selectedRemote;
    private string? _selectedBranch;
    private string? _selectedRemoteBranchName;
    private bool _candidateForRebasingMergeCommit;
    private bool _updatingForceOptions;

    public FormPush()
    {
        InitializeComponent();
        WireControls();
        PopulateRecursiveSubmoduleOptions();
        InitializeComplete();
    }

    public FormPush(IGitUICommands commands, string? branchName = null)
        : base(commands, enablePositionRestore: true)
    {
        InitializeComponent();
        WireControls();

        _remotesManager = new ConfigFileRemoteSettingsManager(() => Module);
        _gitRefs = Module.GetRefs(RefsFilter.Heads | RefsFilter.Remotes);
        _currentBranchName = Module.GetSelectedBranch();
        _userGitRemotes = [.. _remotesManager.LoadRemotes(loadDisabled: false)];

        PopulateRecursiveSubmoduleOptions();
        RecursiveSubmodules.SelectedIndex = Math.Clamp(AppSettings.RecursiveSubmodules, 0, 2);
        BindRemotesDropDown(selectedRemoteName: null);
        UpdateBranchDropDown();
        SelectBranch(DetachedHeadParser.IsDetachedHead(branchName ?? _currentBranchName) ? HeadText : branchName ?? _currentBranchName);
        UpdateRemoteBranchDropDown();
        BranchSelectedValueChanged(this, EventArgs.Empty);

        if (AppSettings.AlwaysShowAdvOpt)
        {
            ShowOptionsClick(this, EventArgs.Empty);
        }

        InitializeComplete();
        UpdatePushButton();
    }

    public bool ErrorOccurred { get; private set; }

    public WinFormsShims.DialogResult PushAndShowDialogWhenFailed(WinFormsShims.IWin32Window? owner = null)
    {
        if (!PushChanges(owner))
        {
            return ShowDialog(owner);
        }

        return WinFormsShims.DialogResult.OK;
    }

    public void CheckForceWithLease()
    {
        ckForceWithLease.IsChecked = true;
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);
        Title = $"{_pushCaption.Text} ({Module.WorkingDir})";
        _NO_TRANSLATE_Remotes.Focus();
    }

    private void WireControls()
    {
        Push.Click += PushClick;
        Pull.Click += PullClick;
        AddRemote.Click += AddRemoteClick;
        ShowOptions.Click += ShowOptionsClick;
        PushToUrl.IsCheckedChanged += PushToUrlCheckedChanged;
        PushToRemote.IsCheckedChanged += PushToUrlCheckedChanged;
        _NO_TRANSLATE_Remotes.SelectionChanged += RemotesUpdated;
        _NO_TRANSLATE_Branch.SelectionChanged += BranchSelectedValueChanged;
        RemoteBranch.SelectionChanged += (_, _) => UpdatePushButton();
        TagComboBox.SelectionChanged += (_, _) => UpdatePushButton();
        TabControlTagBranch.SelectionChanged += TabControlTagBranchSelected;
        ckForceWithLease.IsCheckedChanged += ForceWithLeaseCheckedChanged;
        ForcePushBranches.IsCheckedChanged += ForcePushBranchesCheckedChanged;
        ForcePushTags.IsCheckedChanged += ForcePushTagsCheckedChanged;
        unselectAllToolStripMenuItem.Click += (_, _) => SetBranchesPushCheckboxesState(_ => false);
        selectTrackedToolStripMenuItem.Click += (_, _) => SetBranchesPushCheckboxesState(row => row.IsTracked);
        selectAllToolStripMenuItem.Click += (_, _) => SetBranchesPushCheckboxesState(row => row.CanPush);

        PushDestination.PropertyChanged += (_, args) =>
        {
            if (args.Property == ComboBox.TextProperty)
            {
                UpdatePushButton();
            }
        };
        _NO_TRANSLATE_Branch.PropertyChanged += (_, args) =>
        {
            if (args.Property == ComboBox.TextProperty)
            {
                BranchSelectedValueChanged(this, EventArgs.Empty);
            }
        };
        RemoteBranch.PropertyChanged += (_, args) =>
        {
            if (args.Property == ComboBox.TextProperty)
            {
                UpdatePushButton();
            }
        };
        TagComboBox.PropertyChanged += (_, args) =>
        {
            if (args.Property == ComboBox.TextProperty)
            {
                UpdatePushButton();
            }
        };

        BranchGrid.ItemTemplate = new FuncDataTemplate<BranchPushRow>(
            (row, _) => row is null ? new TextBlock() : CreateBranchRow(row),
            supportsRecycling: false);
        folderBrowserButton1.PathShowingControl = PushDestination;
        Push.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(TranslatedStrings.ButtonPush);
    }

    private bool CheckIfRemoteExist()
    {
        if (_userGitRemotes.Count > 0)
        {
            return true;
        }

        if (MessageBoxes.Show(
                this,
                _configureRemote.Text,
                _errorPushToRemoteCaption.Text,
                WinFormsShims.MessageBoxButtons.YesNo,
                WinFormsShims.MessageBoxIcon.Error) == WinFormsShims.DialogResult.Yes)
        {
            OpenRemotesDialogAndRefreshList(selectedRemoteName: null);
            return _userGitRemotes.Count > 0;
        }

        return false;
    }

    private void OpenRemotesDialogAndRefreshList(string? selectedRemoteName)
    {
        if (!UICommands.StartRemotesDialog(this, selectedRemoteName))
        {
            return;
        }

        _userGitRemotes = [.. _remotesManager.LoadRemotes(loadDisabled: false)];
        _gitRefs = Module.GetRefs(RefsFilter.Heads | RefsFilter.Remotes);
        BindRemotesDropDown(selectedRemoteName);
    }

    private void PushClick(object? sender, EventArgs e)
    {
        DialogResult = PushChanges(this)
            ? WinFormsShims.DialogResult.OK
            : WinFormsShims.DialogResult.None;
    }

    private bool PushChanges(WinFormsShims.IWin32Window? owner)
    {
        ErrorOccurred = false;
        bool pushToUrl = PushToUrl.IsChecked == true;
        string destination;
        string remote = string.Empty;

        if (pushToUrl)
        {
            destination = PushDestination.Text?.Trim() ?? string.Empty;
            if (!IsValidPushDestination(destination))
            {
                MessageBoxes.Show(owner, _selectDestinationDirectory.Text, TranslatedStrings.Error, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
                return false;
            }
        }
        else
        {
            if (!CheckIfRemoteExist() || _NO_TRANSLATE_Remotes.SelectedItem is not string selectedRemoteName)
            {
                ErrorOccurred = true;
                return false;
            }

            _selectedRemote = _userGitRemotes.FirstOrDefault(item => StringComparer.OrdinalIgnoreCase.Equals(item.Name, selectedRemoteName));
            if (_selectedRemote?.Name is null)
            {
                ErrorOccurred = true;
                return false;
            }

            destination = _selectedRemote.Name;
            remote = destination.Trim();
        }

        if (TabControlTagBranch.SelectedItem == TagTab && string.IsNullOrWhiteSpace(TagComboBox.Text))
        {
            MessageBoxes.Show(owner, _selectTag.Text, TranslatedStrings.Error, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
            return false;
        }

        string localBranch = GetSelectedBranchName();
        string remoteBranch = RemoteBranch.Text?.Trim() ?? string.Empty;
        if (TabControlTagBranch.SelectedItem == BranchTab
            && localBranch != AllRefs
            && (string.IsNullOrWhiteSpace(localBranch)
                || localBranch == DetachedHeadParser.DetachedBranch
                || string.IsNullOrWhiteSpace(remoteBranch)
                || remoteBranch == DetachedHeadParser.DetachedBranch))
        {
            MessageBoxes.Show(owner, _noCurrentBranch.Text, TranslatedStrings.Error, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
            return false;
        }

        if (!pushToUrl
            && TabControlTagBranch.SelectedItem == BranchTab
            && localBranch != AllRefs
            && !Module.IsBareRepository()
            && _selectedRemote is not null
            && remoteBranch != _remotesManager.GetDefaultPushRemote(_selectedRemote, localBranch)
            && !IsBranchKnownToRemote(_selectedRemote.Name, remoteBranch)
            && !AppSettings.DontConfirmPushNewBranch
            && MessageBoxes.Show(owner, _branchNewForRemote.Text, _pushCaption.Text, WinFormsShims.MessageBoxButtons.YesNo, WinFormsShims.MessageBoxIcon.Question) == WinFormsShims.DialogResult.No)
        {
            return false;
        }

        bool? trackingChoice = pushToUrl ? false : ShouldUpdateTrackingReference(owner, localBranch, remoteBranch);
        if (trackingChoice is null)
        {
            return false;
        }

        bool track = trackingChoice.Value;
        if (!ConfirmForcePush(owner))
        {
            return false;
        }

        if (pushToUrl)
        {
            ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Remotes.AddAsMostRecentAsync(destination));
        }

        AppSettings.RecursiveSubmodules = Math.Max(RecursiveSubmodules.SelectedIndex, 0);
        ArgumentString pushArguments = CreatePushArguments(destination, track);
        if (string.IsNullOrWhiteSpace(pushArguments.ToString()))
        {
            return false;
        }

        _selectedBranch = localBranch;
        _selectedRemoteBranchName = remoteBranch;
        _candidateForRebasingMergeCommit = !pushToUrl && localBranch != AllRefs && TabControlTagBranch.SelectedItem == BranchTab;

        // Native Git hooks remain active. Git Extensions before/after push event scripts join
        // when the shared scripts engine is available to the Avalonia application.
        using FormRemoteProcess form = new(UICommands, pushArguments)
        {
            Remote = remote,
            Text = string.Format(_pushToCaption.Text, destination),
            HandleOnExitCallback = HandlePushOnExit,
        };
        form.ShowDialog(owner);
        ErrorOccurred = form.ErrorOccurred();
        Module.InvalidateGitSettings();
        return !Module.InTheMiddleOfAction() && !ErrorOccurred;
    }

    private bool? ShouldUpdateTrackingReference(WinFormsShims.IWin32Window? owner, string localBranch, string remoteBranch)
    {
        bool track = ReplaceTrackingReference.IsChecked == true;
        if (track || localBranch == AllRefs || string.IsNullOrWhiteSpace(remoteBranch) || _selectedRemote is null)
        {
            return track;
        }

        IGitRef? selectedLocalBranch = _gitRefs.FirstOrDefault(branch => branch.IsHead && branch.Name == localBranch);
        track = selectedLocalBranch is not null
            && string.IsNullOrEmpty(selectedLocalBranch.TrackingRemote)
            && !_userGitRemotes.Any(remote => localBranch.StartsWith(remote.Name + "/", StringComparison.OrdinalIgnoreCase));
        if (string.Equals(Module.GetEffectiveSetting("branch.autosetupmerge"), "false", StringComparison.OrdinalIgnoreCase))
        {
            track = false;
        }

        if (!track || AppSettings.DontConfirmAddTrackingRef)
        {
            return track;
        }

        WinFormsShims.DialogResult result = MessageBoxes.Show(
            owner,
            string.Format(_updateTrackingReference.Text, selectedLocalBranch!.Name, remoteBranch),
            _pushCaption.Text,
            WinFormsShims.MessageBoxButtons.YesNoCancel,
            WinFormsShims.MessageBoxIcon.Question,
            WinFormsShims.MessageBoxDefaultButton.Button1);
        return result switch
        {
            WinFormsShims.DialogResult.Yes => true,
            WinFormsShims.DialogResult.No => false,
            _ => null,
        };
    }

    private bool ConfirmForcePush(WinFormsShims.IWin32Window? owner)
    {
        if (ForcePushBranches.IsChecked != true)
        {
            return true;
        }

        WinFormsShims.DialogResult choice = MessageBoxes.Show(
            owner,
            _useForceWithLeaseInstead.Text,
            "Question",
            WinFormsShims.MessageBoxButtons.YesNoCancel,
            WinFormsShims.MessageBoxIcon.Question,
            WinFormsShims.MessageBoxDefaultButton.Button1);
        if (choice == WinFormsShims.DialogResult.Yes)
        {
            ForcePushBranches.IsChecked = false;
            ckForceWithLease.IsChecked = true;
        }

        return choice != WinFormsShims.DialogResult.Cancel;
    }

    private ArgumentString CreatePushArguments(string destination, bool track)
    {
        if (TabControlTagBranch.SelectedItem == BranchTab)
        {
            string localBranch = GetSelectedBranchName();
            return localBranch == AllRefs
                ? Commands.PushAll(destination, GetForcePushOption(), track, RecursiveSubmodules.SelectedIndex)
                : Commands.Push(
                    destination,
                    Module.FormatBranchName(localBranch),
                    RemoteBranch.Text?.Trim(),
                    GetForcePushOption(),
                    track,
                    RecursiveSubmodules.SelectedIndex);
        }

        if (TabControlTagBranch.SelectedItem == TagTab)
        {
            string tag = TagComboBox.Text?.Trim() ?? string.Empty;
            bool pushAllTags = tag == AllRefs;
            return Commands.PushTag(destination, pushAllTags ? string.Empty : tag, pushAllTags, GetForcePushOption());
        }

        List<GitPushAction> pushActions = [];
        foreach (BranchPushRow row in _branchRows)
        {
            string? remoteBranch = string.IsNullOrWhiteSpace(row.RemoteBranch) ? row.LocalBranch : row.RemoteBranch;
            if (string.IsNullOrWhiteSpace(remoteBranch))
            {
                continue;
            }

            if (row.Push || row.Force)
            {
                pushActions.Add(new GitPushAction(row.LocalBranch, remoteBranch, row.Force));
            }
            else if (row.Delete)
            {
                pushActions.Add(GitPushAction.DeleteRemoteBranch(remoteBranch));
            }
        }

        return pushActions.Count == 0 ? "" : Commands.PushMultiple(destination, pushActions);
    }

    private ForcePushOptions GetForcePushOption()
    {
        if (ForcePushBranches.IsChecked == true
            || (TabControlTagBranch.SelectedItem == TagTab && ForcePushTags.IsChecked == true))
        {
            return ForcePushOptions.Force;
        }

        return ckForceWithLease.IsChecked == true
            ? ForcePushOptions.ForceWithLease
            : ForcePushOptions.DoNotForce;
    }

    private bool IsRebasingMergeCommit()
    {
        if (AppSettings.DefaultPullAction != GitPullAction.Rebase
            || !_candidateForRebasingMergeCommit
            || _selectedBranch != _currentBranchName
            || _selectedRemote != _currentBranchRemote
            || string.IsNullOrWhiteSpace(_selectedRemote?.Name))
        {
            return false;
        }

        return Module.ExistsMergeCommit($"{_selectedRemote.Name}/{_selectedRemoteBranchName}", _selectedBranch);
    }

    private bool HandlePushOnExit(ref bool isError, FormProcess form)
    {
        if (!isError
            || _selectedBranch != _currentBranchName
            || PushToRemote.IsChecked != true
            || string.IsNullOrWhiteSpace(_currentBranchName))
        {
            return false;
        }

        Regex rejected = new($"! \\[rejected\\]\\s*((?<currBranch>{Regex.Escape(_currentBranchName)})|.*) -> ");
        Match match = rejected.Match(form.GetOutputString());
        if (!match.Success || Module.IsBareRepository())
        {
            return false;
        }

        (GitPullAction pullAction, bool forcePush) = AskForAutoPullOnPushRejectedAction(form, match.Groups["currBranch"].Success);
        if (forcePush)
        {
            string arguments = form.ProcessArguments ?? string.Empty;
            if (!arguments.Contains("--force-with-lease", StringComparison.Ordinal))
            {
                int position = arguments.IndexOf("push ", StringComparison.Ordinal);
                if (position < 0)
                {
                    return false;
                }

                form.ProcessArguments = arguments.Insert(position + "push ".Length, "--force-with-lease ");
            }

            form.Retry();
            return true;
        }

        if (pullAction == GitPullAction.Default)
        {
            pullAction = AppSettings.DefaultPullAction;
        }

        if (pullAction == GitPullAction.None)
        {
            return false;
        }

        if (pullAction is not (GitPullAction.Merge or GitPullAction.Rebase))
        {
            MessageBoxes.ShowError(form, "Automatical pull can only be performed, when the default pull action is either set to Merge or Rebase.");
            return false;
        }

        if (IsRebasingMergeCommit())
        {
            MessageBoxes.ShowError(
                form,
                "Can not perform automatical pull, when the pull action is set to Rebase "
                + "and one of the commits that are about to be rebased is a merge commit.");
            return false;
        }

        if (_selectedRemote?.Name is null)
        {
            return false;
        }

        UICommands.StartPullDialogAndPullImmediately(
            out bool pullCompleted,
            form,
            _selectedRemoteBranchName,
            _selectedRemote.Name,
            pullAction);
        if (!pullCompleted)
        {
            return false;
        }

        form.Retry();
        return true;
    }

    private (GitPullAction PullAction, bool ForcePush) AskForAutoPullOnPushRejectedAction(
        WinFormsShims.IWin32Window owner,
        bool allOptions)
    {
        GitPullAction? pullAction = AppSettings.AutoPullOnPushRejectedAction;
        if (pullAction is not null)
        {
            return (pullAction.Value, false);
        }

        string defaultAction = AppSettings.DefaultPullAction switch
        {
            GitPullAction.Fetch or GitPullAction.FetchAll or GitPullAction.FetchPruneAll => _pullActionFetch.Text,
            GitPullAction.Merge => _pullActionMerge.Text,
            GitPullAction.Rebase => _pullActionRebase.Text,
            _ => _pullActionNone.Text,
        };
        TaskDialogPage page = new()
        {
            Text = allOptions ? _pullRepositoryMergeInstruction.Text : _pullRepositoryForceInstruction.Text,
            Heading = allOptions ? _pullRepositoryMainMergeInstruction.Text : _pullRepositoryMainForceInstruction.Text,
            Caption = string.Format(_pullRepositoryCaption.Text, _NO_TRANSLATE_Remotes.SelectedItem as string ?? string.Empty),
            Icon = TaskDialogIcon.Error,
            Verification = new TaskDialogVerificationCheckBox { Text = TranslatedStrings.DontShowAgain },
            AllowCancel = true,
            SizeToContent = true,
        };
        page.Buttons.Add(TaskDialogButton.Cancel);
        TaskDialogCommandLinkButton pullDefault = new(string.Format(_pullDefaultButton.Text, defaultAction));
        TaskDialogCommandLinkButton pullRebase = new(_pullRebaseButton.Text);
        TaskDialogCommandLinkButton pullMerge = new(_pullMergeButton.Text);
        TaskDialogCommandLinkButton pushForce = new(_pushForceButton.Text);
        if (allOptions)
        {
            page.Buttons.Add(pullDefault);
            page.Buttons.Add(pullRebase);
            page.Buttons.Add(pullMerge);
        }

        page.Buttons.Add(pushForce);
        TaskDialogButton result = TaskDialog.ShowDialog(owner, page);
        bool forcePush = result == pushForce;
        pullAction = result == pullDefault
            ? GitPullAction.Default
            : result == pullRebase
                ? GitPullAction.Rebase
                : result == pullMerge
                    ? GitPullAction.Merge
                    : GitPullAction.None;
        if (page.Verification.Checked && !forcePush)
        {
            AppSettings.AutoPullOnPushRejectedAction = pullAction;
        }

        return (pullAction.Value, forcePush);
    }

    private void BindRemotesDropDown(string? selectedRemoteName)
    {
        _NO_TRANSLATE_Remotes.Items.Clear();
        foreach (ConfigFileRemote remote in _userGitRemotes)
        {
            if (!string.IsNullOrWhiteSpace(remote.Name))
            {
                _NO_TRANSLATE_Remotes.Items.Add(remote.Name);
            }
        }

        selectedRemoteName ??= string.IsNullOrWhiteSpace(_currentBranchName)
            ? null
            : Module.GetSetting(string.Format(SettingKeyString.BranchRemote, _currentBranchName));
        _currentBranchRemote = _userGitRemotes.FirstOrDefault(remote => StringComparer.OrdinalIgnoreCase.Equals(remote.Name, selectedRemoteName));
        string? selected = _currentBranchRemote?.Name
            ?? _userGitRemotes.FirstOrDefault(remote => StringComparer.OrdinalIgnoreCase.Equals(remote.Name, "origin"))?.Name
            ?? _userGitRemotes.FirstOrDefault()?.Name;
        _NO_TRANSLATE_Remotes.SelectedItem = selected;
        RemotesUpdated(this, EventArgs.Empty);
    }

    private bool IsBranchKnownToRemote(string? remote, string branch)
        => GetRemoteBranches(remote).Any(reference => reference.LocalName == branch)
            || _gitRefs.Any(reference => reference.IsHead && reference.Name == branch && reference.TrackingRemote == remote);

    private IEnumerable<IGitRef> GetLocalBranches() => _gitRefs.Where(reference => reference.IsHead);

    private IEnumerable<IGitRef> GetRemoteBranches(string? remoteName)
        => _gitRefs.Where(reference => reference.IsRemote && reference.Remote == remoteName);

    private void UpdateBranchDropDown()
    {
        string selected = GetSelectedBranchName();
        _NO_TRANSLATE_Branch.Items.Clear();
        _NO_TRANSLATE_Branch.Items.Add(AllRefs);
        _NO_TRANSLATE_Branch.Items.Add(HeadText);
        foreach (string branch in GetLocalBranches().Select(reference => reference.Name).OrderBy(name => name))
        {
            _NO_TRANSLATE_Branch.Items.Add(branch);
        }

        SelectBranch(selected);
    }

    private void SelectBranch(string? branch)
    {
        string value = string.IsNullOrWhiteSpace(branch) ? string.Empty : branch;
        _NO_TRANSLATE_Branch.SelectedItem = _NO_TRANSLATE_Branch.Items.OfType<string>().FirstOrDefault(item => item == value);
        _NO_TRANSLATE_Branch.Text = value;
    }

    private string GetSelectedBranchName()
        => _NO_TRANSLATE_Branch.SelectedItem as string ?? _NO_TRANSLATE_Branch.Text?.Trim() ?? string.Empty;

    private void UpdateRemoteBranchDropDown()
    {
        string previous = RemoteBranch.Text?.Trim() ?? string.Empty;
        RemoteBranch.Items.Clear();
        string localBranch = GetSelectedBranchName();
        if (!string.IsNullOrEmpty(localBranch) && !DetachedHeadParser.IsDetachedHead(localBranch) && localBranch != HeadText && localBranch != AllRefs)
        {
            RemoteBranch.Items.Add(localBranch);
        }

        if (_selectedRemote is not null)
        {
            foreach (string branch in GetRemoteBranches(_selectedRemote.Name).Select(reference => reference.LocalName).Where(name => name != localBranch).OrderBy(name => name))
            {
                RemoteBranch.Items.Add(branch);
            }
        }

        RemoteBranch.Text = previous;
    }

    private void BranchSelectedValueChanged(object? sender, EventArgs e)
    {
        string localBranch = GetSelectedBranchName();
        RemoteBranch.IsEnabled = localBranch != AllRefs;
        if (localBranch == AllRefs)
        {
            RemoteBranch.Text = string.Empty;
            UpdatePushButton();
            return;
        }

        if (localBranch != HeadText)
        {
            IGitRef? selectedBranch = _gitRefs.FirstOrDefault(reference => reference.IsHead && reference.Name == localBranch);
            if (PushToRemote.IsChecked == true && selectedBranch is not null && _selectedRemote is not null)
            {
                string? defaultRemoteBranch = _remotesManager.GetDefaultPushRemote(_selectedRemote, selectedBranch.Name);
                if (!string.IsNullOrEmpty(defaultRemoteBranch))
                {
                    RemoteBranch.Text = defaultRemoteBranch;
                    UpdatePushButton();
                    return;
                }

                if (selectedBranch.TrackingRemote.Equals(_selectedRemote.Name, StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrEmpty(selectedBranch.MergeWith))
                {
                    RemoteBranch.Text = selectedBranch.MergeWith;
                    UpdatePushButton();
                    return;
                }
            }

            RemoteBranch.Text = $"{_selectedRemote?.Prefix}{localBranch}";
        }

        UpdatePushButton();
    }

    private void RemotesUpdated(object? sender, EventArgs e)
    {
        string? selectedName = _NO_TRANSLATE_Remotes.SelectedItem as string;
        _selectedRemote = _userGitRemotes.FirstOrDefault(remote => StringComparer.OrdinalIgnoreCase.Equals(remote.Name, selectedName));
        if (_selectedRemote is null)
        {
            UpdatePushButton();
            return;
        }

        PushDestination.Text = string.IsNullOrEmpty(_selectedRemote.PushUrl) ? _selectedRemote.Url : _selectedRemote.PushUrl;
        UpdateRemoteBranchDropDown();
        BranchSelectedValueChanged(this, EventArgs.Empty);
        if (TabControlTagBranch.SelectedItem == MultipleBranchTab)
        {
            UpdateMultiBranchView();
        }

        UpdatePushButton();
    }

    private void PushToUrlCheckedChanged(object? sender, EventArgs e)
    {
        bool pushToUrl = PushToUrl.IsChecked == true;
        PushDestination.IsEnabled = pushToUrl;
        folderBrowserButton1.IsEnabled = pushToUrl;
        _NO_TRANSLATE_Remotes.IsEnabled = !pushToUrl;
        AddRemote.IsEnabled = !pushToUrl;
        if (pushToUrl)
        {
            string previous = PushDestination.Text ?? string.Empty;
            IList<Repository> history = ThreadHelper.JoinableTaskFactory.Run(RepositoryHistoryManager.Remotes.LoadRecentHistoryAsync);
            PushDestination.ItemsSource = history.Select(repository => repository.Path).ToList();
            PushDestination.Text = previous;
            BranchSelectedValueChanged(this, EventArgs.Empty);
        }
        else
        {
            RemotesUpdated(sender, e);
        }

        UpdatePushButton();
    }

    private void AddRemoteClick(object? sender, EventArgs e)
        => OpenRemotesDialogAndRefreshList(_selectedRemote?.Name);

    private void PullClick(object? sender, EventArgs e)
        => UICommands.StartPullDialog(this);

    private void ShowOptionsClick(object? sender, EventArgs e)
    {
        PushOptionsPanel.IsVisible = true;
        ShowOptions.IsVisible = false;
    }

    private void TabControlTagBranchSelected(object? sender, EventArgs e)
    {
        if (e is SelectionChangedEventArgs selectionChanged
            && selectionChanged.Source != TabControlTagBranch)
        {
            return;
        }

        if (TabControlTagBranch.SelectedItem == MultipleBranchTab)
        {
            UpdateMultiBranchView();
        }
        else if (TabControlTagBranch.SelectedItem == TagTab)
        {
            FillTagDropDown();
        }
        else
        {
            UpdateBranchDropDown();
            UpdateRemoteBranchDropDown();
            BranchSelectedValueChanged(this, EventArgs.Empty);
        }

        UpdatePushButton();
    }

    private void FillTagDropDown()
    {
        string selected = TagComboBox.Text ?? string.Empty;
        TagComboBox.Items.Clear();
        TagComboBox.Items.Add(AllRefs);
        foreach (string tag in Module.GetRefs(RefsFilter.Tags).Select(reference => reference.Name))
        {
            TagComboBox.Items.Add(tag);
        }

        TagComboBox.Text = selected;
    }

    private void ForceWithLeaseCheckedChanged(object? sender, EventArgs e)
    {
        if (_updatingForceOptions)
        {
            return;
        }

        _updatingForceOptions = true;
        if (ckForceWithLease.IsChecked == true)
        {
            ForcePushBranches.IsChecked = false;
        }

        ForcePushTags.IsChecked = ckForceWithLease.IsChecked;
        _updatingForceOptions = false;
    }

    private void ForcePushBranchesCheckedChanged(object? sender, EventArgs e)
    {
        if (_updatingForceOptions || ForcePushBranches.IsChecked != true)
        {
            return;
        }

        _updatingForceOptions = true;
        ckForceWithLease.IsChecked = false;
        ForcePushTags.IsChecked = false;
        _updatingForceOptions = false;
    }

    private void ForcePushTagsCheckedChanged(object? sender, EventArgs e)
    {
        if (_updatingForceOptions)
        {
            return;
        }

        _updatingForceOptions = true;
        ckForceWithLease.IsChecked = ForcePushTags.IsChecked;
        if (ForcePushTags.IsChecked == true)
        {
            ForcePushBranches.IsChecked = false;
        }

        _updatingForceOptions = false;
    }

    private void UpdateMultiBranchView()
    {
        _branchRows.Clear();
        BranchGrid.ItemsSource = null;
        if (_selectedRemote?.Name is null)
        {
            UpdatePushButton();
            return;
        }

        IReadOnlyList<IGitRef> remoteHeads;
        if (DetailedSettings.GetRemoteBranchesDirectlyFromRemote.ValueOrDefault(Module.GetEffectiveSettings()))
        {
            using FormRemoteProcess form = new(UICommands, $"ls-remote --heads \"{_selectedRemote.Name}\"")
            {
                Remote = _selectedRemote.Name,
            };
            form.ShowDialog(this);
            if (form.ErrorOccurred())
            {
                return;
            }

            string output = CleanCommandOutput(form.GetOutputString());
            remoteHeads = Module.ParseRefs(output);
        }
        else
        {
            remoteHeads = [.. Module.GetRemoteBranches().Where(reference => reference.Remote == _selectedRemote.Name)];
        }

        ProcessHeads(remoteHeads, _selectedRemote.Name);
        BranchGrid.ItemsSource = _branchRows;
        UpdatePushButton();
    }

    private static string CleanCommandOutput(string processOutput)
    {
        int firstTabIndex = processOutput.IndexOf('\t');
        return firstTabIndex == 40
            ? processOutput
            : firstTabIndex > 40
                ? processOutput[(firstTabIndex - 40)..]
                : string.Empty;
    }

    private void ProcessHeads(IReadOnlyList<IGitRef> remoteHeads, string remote)
    {
        List<IGitRef> localHeads = [.. GetLocalBranches()];
        Dictionary<string, IGitRef> remoteBranches = remoteHeads.ToDictionary(head => head.LocalName, head => head);
        AheadBehindDataProvider provider = new(() => Module.GitExecutable);
        IReadOnlyDictionary<string, AheadBehindData>? aheadBehindData = provider.GetData();
        foreach (IGitRef head in localHeads)
        {
            string remoteName = head.Remote == remote ? head.MergeWith ?? head.Name : string.Empty;
            bool isKnownAtRemote = remoteBranches.TryGetValue(head.Name, out IGitRef? remoteBranch);
            AheadBehindData aheadBehind = default;
            bool isAheadRemote = aheadBehindData is not null
                && aheadBehindData.TryGetValue(head.Name, out aheadBehind)
                && GitRefName.GetRemoteName(aheadBehind.RemoteRef) == remote;
            string destination = isAheadRemote ? GitRefName.GetRemoteBranch(aheadBehind.RemoteRef) : remoteName;
            string ahead = isAheadRemote
                ? aheadBehindData![head.Name].ToDisplay()
                : !isKnownAtRemote
                    ? string.Empty
                    : head.ObjectId == remoteBranch!.ObjectId
                        ? "="
                        : "<>";
            _branchRows.Add(new BranchPushRow(head.Name, destination, ahead));
        }

        foreach (IGitRef remoteHead in remoteHeads.Where(remoteHead => localHeads.All(local => local.Name != remoteHead.LocalName)))
        {
            _branchRows.Add(new BranchPushRow(localBranch: string.Empty, remoteHead.LocalName, ahead: string.Empty));
        }
    }

    private Control CreateBranchRow(BranchPushRow row)
    {
        Grid grid = new()
        {
            ColumnDefinitions = new ColumnDefinitions("2*,2*,110,65,65,120"),
        };
        TextBox local = new() { Text = row.LocalBranch, BorderThickness = new Thickness(0), IsReadOnly = !row.CanPush };
        TextBox remote = new() { Text = row.RemoteBranch, BorderThickness = new Thickness(0) };
        TextBlock ahead = new() { Text = row.Ahead, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
        CheckBox push = new() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, IsEnabled = row.CanPush };
        CheckBox force = new() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, IsEnabled = row.CanPush };
        CheckBox delete = new() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
        Grid.SetColumn(remote, 1);
        Grid.SetColumn(ahead, 2);
        Grid.SetColumn(push, 3);
        Grid.SetColumn(force, 4);
        Grid.SetColumn(delete, 5);
        grid.Children.Add(local);
        grid.Children.Add(remote);
        grid.Children.Add(ahead);
        grid.Children.Add(push);
        grid.Children.Add(force);
        grid.Children.Add(delete);
        Border border = new()
        {
            BorderBrush = LocalColumn.BorderBrush,
            BorderThickness = new Thickness(1, 0, 1, 1),
            Child = grid,
        };

        row.Attach(local, remote, push, force, delete, UpdatePushButton);
        return border;
    }

    private void SetBranchesPushCheckboxesState(Func<BranchPushRow, bool> willPush)
    {
        foreach (BranchPushRow row in _branchRows)
        {
            row.SetPush(willPush(row));
        }

        UpdatePushButton();
    }

    private void UpdatePushButton()
    {
        bool hasDestination = PushToUrl.IsChecked == true
            ? IsValidPushDestination(PushDestination.Text)
            : _NO_TRANSLATE_Remotes.SelectedItem is string;
        bool hasSource = TabControlTagBranch.SelectedItem == TagTab
            ? !string.IsNullOrWhiteSpace(TagComboBox.Text)
            : TabControlTagBranch.SelectedItem == MultipleBranchTab
                ? _branchRows.Any(row => row.Push || row.Force || row.Delete)
                : GetSelectedBranchName() == AllRefs
                    || (!string.IsNullOrWhiteSpace(GetSelectedBranchName()) && !string.IsNullOrWhiteSpace(RemoteBranch.Text));
        Push.IsEnabled = hasDestination && hasSource;
    }

    private static bool IsValidPushDestination(string? destination)
    {
        if (string.IsNullOrWhiteSpace(destination))
        {
            return false;
        }

        return Path.IsPathRooted(destination)
            || Uri.IsWellFormedUriString(destination, UriKind.Absolute)
            || Regex.IsMatch(destination, @"^[^\s@]+@[^\s:]+:.+$");
    }

    private void PopulateRecursiveSubmoduleOptions(ITranslation? translation = null)
    {
        int selectedIndex = RecursiveSubmodules.SelectedIndex;
        string[] items = ["None", "Check", "On-demand"];
        for (int index = 0; index < items.Length && translation is not null; index++)
        {
            items[index] = translation.TranslateItem(nameof(FormPush), nameof(RecursiveSubmodules), $"Item{index}", () => items[index]) ?? items[index];
        }

        RecursiveSubmodules.Items.Clear();
        foreach (string item in items)
        {
            RecursiveSubmodules.Items.Add(item);
        }

        RecursiveSubmodules.SelectedIndex = selectedIndex;
    }

    public override void AddTranslationItems(ITranslation translation)
    {
        object? forceTip = ToolTip.GetTip(ckForceWithLease);
        object? remoteTip = ToolTip.GetTip(PushToRemote);
        object? urlTip = ToolTip.GetTip(PushToUrl);
        object? branchTip = ToolTip.GetTip(BranchTab);
        object? tagTip = ToolTip.GetTip(TagTab);
        ToolTip.SetTip(ckForceWithLease, null);
        ToolTip.SetTip(PushToRemote, null);
        ToolTip.SetTip(PushToUrl, null);
        ToolTip.SetTip(BranchTab, null);
        ToolTip.SetTip(TagTab, null);
        try
        {
            base.AddTranslationItems(translation);
        }
        finally
        {
            ToolTip.SetTip(ckForceWithLease, forceTip);
            ToolTip.SetTip(PushToRemote, remoteTip);
            ToolTip.SetTip(PushToUrl, urlTip);
            ToolTip.SetTip(BranchTab, branchTip);
            ToolTip.SetTip(TagTab, tagTip);
        }

        translation.AddTranslationItem(nameof(FormPush), nameof(LocalColumn), "HeaderText", "Local Branch");
        translation.AddTranslationItem(nameof(FormPush), nameof(RemoteColumn), "HeaderText", "Remote Branch");
        translation.AddTranslationItem(nameof(FormPush), nameof(NewColumn), "HeaderText", "Ahead/Behind");
        translation.AddTranslationItem(nameof(FormPush), nameof(PushColumn), "HeaderText", "Push");
        translation.AddTranslationItem(nameof(FormPush), nameof(ForceColumn), "HeaderText", "Force");
        translation.AddTranslationItem(nameof(FormPush), nameof(DeleteColumn), "HeaderText", "Delete Remote Branch");
        translation.AddTranslationItem(nameof(FormPush), nameof(RecursiveSubmodules), "Item0", "None");
        translation.AddTranslationItem(nameof(FormPush), nameof(RecursiveSubmodules), "Item1", "Check");
        translation.AddTranslationItem(nameof(FormPush), nameof(RecursiveSubmodules), "Item2", "On-demand");
        translation.AddTranslationItem(nameof(FormPush), nameof(folderBrowserButton1), "Text", "Bro&wse...");
        translation.AddTranslationItem(nameof(FormPush), nameof(PushToRemote), "toolTip1", PushToRemoteToolTip);
        translation.AddTranslationItem(nameof(FormPush), nameof(PushToUrl), "toolTip1", PushToUrlToolTip);
        translation.AddTranslationItem(nameof(FormPush), nameof(BranchTab), "ToolTipText", BranchTabToolTip);
        translation.AddTranslationItem(nameof(FormPush), nameof(TagTab), "ToolTipText", TagTabToolTip);
    }

    public override void TranslateItems(ITranslation translation)
    {
        ToolTip.SetTip(ckForceWithLease, null);
        ToolTip.SetTip(PushToRemote, null);
        ToolTip.SetTip(PushToUrl, null);
        ToolTip.SetTip(BranchTab, null);
        ToolTip.SetTip(TagTab, null);
        base.TranslateItems(translation);
        Push.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(TranslatedStrings.ButtonPush);
        PopulateRecursiveSubmoduleOptions(translation);
        TranslateHeader(translation, nameof(LocalColumn), LocalColumn, "Local Branch");
        TranslateHeader(translation, nameof(RemoteColumn), RemoteColumn, "Remote Branch");
        TranslateHeader(translation, nameof(NewColumn), NewColumn, "Ahead/Behind");
        TranslateHeader(translation, nameof(PushColumn), PushColumn, "Push");
        TranslateHeader(translation, nameof(ForceColumn), ForceColumn, "Force");
        TranslateHeader(translation, nameof(DeleteColumn), DeleteColumn, "Delete Remote Branch");
        folderBrowserButton1.Text = translation.TranslateItem(
            nameof(FormPush),
            nameof(folderBrowserButton1),
            "Text",
            () => "Bro&wse...") ?? "Bro&wse...";
        ToolTip.SetTip(ckForceWithLease, _forceWithLeaseTooltips.Text);
        ToolTip.SetTip(PushToRemote, Translate(PushToRemote, "toolTip1", PushToRemoteToolTip));
        ToolTip.SetTip(PushToUrl, Translate(PushToUrl, "toolTip1", PushToUrlToolTip));
        ToolTip.SetTip(BranchTab, Translate(BranchTab, "ToolTipText", BranchTabToolTip));
        ToolTip.SetTip(TagTab, Translate(TagTab, "ToolTipText", TagTabToolTip));

        string Translate(Control control, string property, string fallback)
            => translation.TranslateItem(nameof(FormPush), control.Name!, property, () => fallback) ?? fallback;
    }

    private static void TranslateHeader(ITranslation translation, string fieldName, Border header, string defaultText)
    {
        string? text = translation.TranslateItem(nameof(FormPush), fieldName, "HeaderText", () => defaultText);
        if (!string.IsNullOrEmpty(text) && header.Child is TextBlock textBlock)
        {
            textBlock.Text = text;
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormPush form)
    {
        internal IReadOnlyList<BranchPushRow> BranchRows => form._branchRows;
        internal ArgumentString CreatePushArguments(string destination, bool track = false) => form.CreatePushArguments(destination, track);
        internal ForcePushOptions GetForcePushOption() => form.GetForcePushOption();
        internal void UpdateMultiBranchView() => form.UpdateMultiBranchView();
    }

    internal sealed class BranchPushRow
    {
        private bool _updating;
        private TextBox? _local;
        private TextBox? _remote;
        private CheckBox? _push;
        private CheckBox? _force;
        private CheckBox? _delete;
        private Action? _changed;

        internal BranchPushRow(string localBranch, string remoteBranch, string ahead)
        {
            LocalBranch = localBranch;
            RemoteBranch = remoteBranch;
            Ahead = ahead;
        }

        internal string LocalBranch { get; private set; }
        internal string RemoteBranch { get; private set; }
        internal string Ahead { get; }
        internal bool Push { get; private set; }
        internal bool Force { get; private set; }
        internal bool Delete { get; private set; }
        internal bool CanPush => !string.IsNullOrWhiteSpace(LocalBranch);
        internal bool IsTracked => CanPush && !string.IsNullOrWhiteSpace(RemoteBranch);

        internal void Attach(TextBox local, TextBox remote, CheckBox push, CheckBox force, CheckBox delete, Action changed)
        {
            _local = local;
            _remote = remote;
            _push = push;
            _force = force;
            _delete = delete;
            _changed = changed;
            local.TextChanged += (_, _) => LocalBranch = local.Text ?? string.Empty;
            remote.TextChanged += (_, _) => RemoteBranch = remote.Text ?? string.Empty;
            push.IsCheckedChanged += (_, _) => SetPush(push.IsChecked == true);
            force.IsCheckedChanged += (_, _) => SetForce(force.IsChecked == true);
            delete.IsCheckedChanged += (_, _) => SetDelete(delete.IsChecked == true);
            SyncControls();
        }

        internal void SetPush(bool value)
        {
            if (_updating)
            {
                return;
            }

            Push = value && CanPush;
            if (Push)
            {
                Force = false;
                Delete = false;
            }

            SyncControls();
        }

        internal void SetForce(bool value)
        {
            if (_updating)
            {
                return;
            }

            Force = value && CanPush;
            if (Force)
            {
                Push = false;
                Delete = false;
            }

            SyncControls();
        }

        internal void SetDelete(bool value)
        {
            if (_updating)
            {
                return;
            }

            Delete = value;
            if (Delete)
            {
                Push = false;
                Force = false;
            }

            SyncControls();
        }

        private void SyncControls()
        {
            _updating = true;
            if (_push is not null)
            {
                _push.IsChecked = Push;
            }

            if (_force is not null)
            {
                _force.IsChecked = Force;
            }

            if (_delete is not null)
            {
                _delete.IsChecked = Delete;
            }

            _updating = false;
            _changed?.Invoke();
        }
    }
}
