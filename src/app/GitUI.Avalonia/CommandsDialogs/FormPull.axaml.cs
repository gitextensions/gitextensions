using System.Text.RegularExpressions;
using Avalonia.Controls;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.Remotes;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI.Compat;
using GitUI.HelperDialogs;
using GitUI.Infrastructure;
using GitUI.ScriptsEngine;
using ResourceManager;
using CancelEventArgs = System.ComponentModel.CancelEventArgs;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

public sealed partial class FormPull : GitExtensionsDialog
{
    // Available: IJKQVXYZ
    // A Fetch all tags
    // B Browse...
    // C Stash changes
    // D Prune remote branches and tags
    // E Rebase
    // F Do not merge, only fetch
    // G Manage remotes
    // H Auto stash
    // L Local branch
    // M Merge
    // N Fetch no tag
    // O Remote branch
    // P Prune remote branches
    // R Remote
    // S Solve conflicts
    // T Follow tagopt
    // U URL
    // W Download full history
    private readonly TranslationString _areYouSureYouWantToRebaseMerge = new(
        "The current commit is a merge." + Environment.NewLine
        + "Are you sure you want to rebase this merge?");
    private const string BranchMergeSetting = "branch.{0}.merge";
    private const string PullFromRemoteToolTip = "Remote repository to pull from";
    private const string PullFromUrlToolTip = "Url to pull from";
    private const string LocalBranchToolTip = "Local branch to create or reset to the remote branch selected.";
    private const string RemoteBranchToolTip = "Remote branch to pull. Leave empty to pull all branches.";
    private const string PruneToolTip = "Removes remote tracking branches that no longer exist on the remote (e.g. if someone else deleted them).\r\n\r\nActual command line (if checked): --prune --force\r\n";
    private const string PruneTagsToolTip = "Before fetching, remove any local tags that no longer exist on the remote if --prune is enabled.";
    private readonly TranslationString _areYouSureYouWantToRebaseMergeCaption = new("Rebase merge commit?");
    private readonly TranslationString _allMergeConflictSolvedQuestion = new("Are all merge conflicts solved? Do you want to commit?");
    private readonly TranslationString _allMergeConflictSolvedQuestionCaption = new("Conflicts solved");
    private readonly TranslationString _applyStashedItemsAgain = new("Apply stashed items to working directory again?");
    private readonly TranslationString _applyStashedItemsAgainCaption = new("Auto stash");
    private readonly TranslationString _fetchAllBranchesCanOnlyWithFetch = new(
        "You can only fetch all remote branches (*) without merge or rebase."
        + Environment.NewLine + "If you want to fetch all remote branches, choose fetch."
        + Environment.NewLine + "If you want to fetch and merge a branch, choose a specific branch.");
    private readonly TranslationString _selectRemoteRepository = new("Please select a remote repository");
    private readonly TranslationString _selectSourceDirectory = new("Please select a source directory");
    private readonly TranslationString _questionInitSubmodules = new(
        "The pulled has submodules configured." + Environment.NewLine
        + "Do you want to initialize the submodules?" + Environment.NewLine
        + "This will initialize and update all submodules recursive.");
    private readonly TranslationString _questionInitSubmodulesCaption = new("Submodules");
    private readonly TranslationString _notOnBranch = new(
        "You cannot \"pull\" when git head detached."
        + Environment.NewLine + Environment.NewLine + "Do you want to continue?");
    private readonly TranslationString _noRemoteBranch = new("You didn't specify a remote branch");
    private readonly TranslationString _noRemoteBranchMainInstruction = new(
        "You asked to pull from the remote '{0}'," + Environment.NewLine
        + "but did not specify a remote branch." + Environment.NewLine
        + "Because this is not the default configured remote for your local branch," + Environment.NewLine
        + "you must specify a remote branch.");
    private readonly TranslationString _noRemoteBranchForFetchMainInstruction = new(
        "You asked to fetch from the remote '{0}'," + Environment.NewLine
        + "but did not specify a remote branch." + Environment.NewLine
        + "Because this is not the current branch, you must specify a remote branch.");
    private readonly TranslationString _noRemoteBranchButton = new("Pull from {0}");
    private readonly TranslationString _noRemoteBranchForFetchButton = new("Fetch from {0}");
    private readonly TranslationString _noRemoteBranchCaption = new("Remote branch not specified");
    private readonly TranslationString _pruneBranchesCaption = new("Pull was rejected");
    private readonly TranslationString _pruneBranchesMainInstruction = new("Remote branch no longer exist");
    private readonly TranslationString _pruneBranchesBranch = new("Do you want to delete all stale remote-tracking branches?");
    private readonly TranslationString _pruneFromCaption = new("Prune remote branches from {0}");
    private readonly TranslationString _hoverShowImageLabelText = new("Hover to see scenario when fast forward is possible.");
    private readonly TranslationString _formTitlePull = new("Pull ({0})");
    private readonly TranslationString _formTitleFetch = new("Fetch ({0})");
    private readonly TranslationString _buttonPull = new("&Pull");
    private readonly TranslationString _buttonFetch = new("&Fetch");
    private readonly TranslationString _pullFetchPruneAllConfirmation = new(
        "Warning! The fetch with prune will remove all the remote-tracking references which no longer exist on remotes. Do you want to proceed?");
    private const string AllRemotes = "[ All ]";

    private readonly IConfigFileRemoteSettingsManager _remotesManager = null!;
    private readonly IFullPathResolver _fullPathResolver = null!;
    private readonly string _branch = string.Empty;
    private List<IGitRef>? _heads;
    private bool _bInternalUpdate;
    private bool _runtimeInitialized;
    private GitPullAction _pullAction;

    [GeneratedRegex(@"Your configuration specifies to .* the ref '.*'[\r]?[\n]from the remote, but no such ref was fetched.", RegexOptions.ExplicitCapture)]
    private static partial Regex IsRefRemoved { get; }

    public FormPull()
    {
        InitializeComponent();
        ApplySourceAutoSize();
        InitializeComplete();
    }

    public FormPull(GitUICommands commands, string? defaultRemoteBranch, string? defaultRemote, GitPullAction pullAction)
        : base(commands, enablePositionRestore: false)
    {
        InitializeComponent();
        ApplySourceAutoSize();
        WireControls();

        _remotesManager = new ConfigFileRemoteSettingsManager(() => Module);
        _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
        _branch = Module.GetSelectedBranch();
        PanelLeftImage.IsVisible = !AppSettings.DontShowHelpImages;
        PanelLeftImage.IsOnHoverShowImage2NoticeText = _hoverShowImageLabelText.Text;
        folderBrowserButton1.PathShowingControl = comboBoxPullSource;

        BindRemotesDropDown(defaultRemote);
        Branches.Text = defaultRemoteBranch ?? GetConfiguredRemoteBranch() ?? string.Empty;
        SetPullAction(pullAction, defaultRemote);
        AutoStash.IsChecked = AppSettings.AutoStash;

        // If this repo is shallow, show an option to Unshallow
        // Detect by presence of the shallow file, not 100% sure it's the best way, but it's created upon shallow cloning and removed upon unshallowing
        Unshallow.IsVisible = File.Exists(commands.Module.ResolveGitInternalPath("shallow"));
        _runtimeInitialized = true;

        InitializeComplete();
        UpdateFormTitleAndButton();
        UpdateActionState();
    }

    private void ApplySourceAutoSize()
    {
        WinFormsAutoSizeContentControl.Attach(PullFromRemote, 25, 19);
        WinFormsAutoSizeContentControl.Attach(PullFromUrl, 25, 19);
        WinFormsAutoSizeContentControl.Attach(Merge, 41, 21);
        WinFormsAutoSizeContentControl.Attach(Rebase, 41, 21);
        WinFormsAutoSizeContentControl.Attach(Fetch, 25, 21);
        WinFormsAutoSizeContentControl.Attach(ReachableTags, 25, 21);
        WinFormsAutoSizeContentControl.Attach(NoTags, 25, 21);
        WinFormsAutoSizeContentControl.Attach(AllTags, 25, 21);
        WinFormsAutoSizeContentControl.Attach(Unshallow, 26, 19);
        WinFormsAutoSizeContentControl.Attach(Prune, 26, 19);
        WinFormsAutoSizeContentControl.Attach(PruneTags, 26, 19);
        WinFormsAutoSizeContentControl.Attach(AutoStash, 26, 19);
        WinFormsAutoSizeContentControl.Attach(lblLocalBranch, 7, 15);
        WinFormsAutoSizeContentControl.Attach(lblRemoteBranch, 7, 15);
    }

    public bool ErrorOccurred { get; private set; }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);
        FormPullLoad(this, e);
    }

    private void WireControls()
    {
        Pull.Click += PullClick;
        Mergetool.Click += MergetoolClick;
        Stash.Click += StashClick;
        AddRemote.Click += AddRemoteClick;
        PullFromRemote.IsCheckedChanged += PullFromRemoteCheckedChanged;
        PullFromUrl.IsCheckedChanged += PullFromUrlCheckedChanged;
        Merge.IsCheckedChanged += MergeCheckedChanged;
        Rebase.IsCheckedChanged += RebaseCheckedChanged;
        Fetch.IsCheckedChanged += FetchCheckedChanged;
        Prune.IsCheckedChanged += Prune_CheckedChanged;
        PruneTags.IsCheckedChanged += PruneTags_CheckedChanged;
        Branches.DropDownOpened += BranchesDropDown;
        localBranch.LostFocus += localBranch_Leave;
        _NO_TRANSLATE_Remotes.SelectionChanged += Remotes_TextChanged;
        _NO_TRANSLATE_Remotes.LostFocus += (_, _) => RemotesValidating(_NO_TRANSLATE_Remotes, new CancelEventArgs());

        _NO_TRANSLATE_Remotes.PropertyChanged += (_, args) =>
        {
            if (args.Property == ComboBox.TextProperty && !_bInternalUpdate)
            {
                Remotes_TextChanged(_NO_TRANSLATE_Remotes, EventArgs.Empty);
            }
        };
        comboBoxPullSource.PropertyChanged += (_, args) =>
        {
            if (args.Property == ComboBox.TextProperty)
            {
                PullSourceValidating(comboBoxPullSource, new CancelEventArgs());
                UpdateActionState();
            }
        };
        Branches.PropertyChanged += (_, args) =>
        {
            if (args.Property == ComboBox.TextProperty)
            {
                UpdateActionState();
            }
        };
    }

    private void BindRemotesDropDown(string? selectedRemoteName)
    {
        // refresh registered git remotes
        List<ConfigFileRemote> remotes = [.. _remotesManager.LoadRemotes(loadDisabled: false)];
        _bInternalUpdate = true;
        _NO_TRANSLATE_Remotes.Items.Clear();
        _NO_TRANSLATE_Remotes.Items.Add(AllRemotes);
        foreach (ConfigFileRemote remote in remotes)
        {
            if (!string.IsNullOrWhiteSpace(remote.Name))
            {
                _NO_TRANSLATE_Remotes.Items.Add(remote.Name);
            }
        }

        selectedRemoteName ??= string.IsNullOrWhiteSpace(_branch)
            ? null
            : Module.GetSetting(string.Format(SettingKeyString.BranchRemote, _branch));
        string selected = _NO_TRANSLATE_Remotes.Items
            .OfType<string>()
            .FirstOrDefault(remote => StringComparer.OrdinalIgnoreCase.Equals(remote, selectedRemoteName))
            ?? _NO_TRANSLATE_Remotes.Items.OfType<string>().FirstOrDefault(remote => remote != AllRemotes)
            ?? AllRemotes;

        // we couldn't find the default assigned remote for the selected branch
        // it is usually gets mapped via FormRemotes -> "default pull behavior" tab
        // so pick the default user remote
        _NO_TRANSLATE_Remotes.SelectedItem = selected;
        _NO_TRANSLATE_Remotes.Text = selected;
        _bInternalUpdate = false;
        RemotesValidating(_NO_TRANSLATE_Remotes, new CancelEventArgs());
    }

    public WinFormsShims.DialogResult PullAndShowDialogWhenFailed(
        WinFormsShims.IWin32Window? owner,
        string? remote,
        GitPullAction pullAction)
    {
        // This path executes before the window is attached, so preserve the requested radio selection explicitly.
        SetPullAction(pullAction, remote);

        // Special case for "Fetch and prune" and "Fetch and prune all" to make sure user confirms the action.
        if (pullAction == GitPullAction.FetchPruneAll)
        {
            string messageBoxTitle = string.Format(_pruneFromCaption.Text, string.IsNullOrEmpty(remote) ? AllRemotes : remote);
            bool isActionConfirmed = MessageBoxes.ConfirmSuppressible(owner ?? this, _pullFetchPruneAllConfirmation.Text, messageBoxTitle, AppSettings.DontConfirmFetchAndPruneAll);
            if (!isActionConfirmed)
            {
                return WinFormsShims.DialogResult.Cancel;
            }
        }

        WinFormsShims.DialogResult result = PullChanges(owner);
        if (result == WinFormsShims.DialogResult.No)
        {
            return ShowDialog(owner);
        }

        Close();
        return result;
    }

    private void MergetoolClick(object? sender, EventArgs e)
    {
        this.InvokeAndForget(async () =>
        {
            using (FormBusyScope.Enter(this))
            {
                await Task.Run(() => Module.RunMergeTool());
            }

            if (MessageBoxes.Show(
                    this,
                    _allMergeConflictSolvedQuestion.Text,
                    _allMergeConflictSolvedQuestionCaption.Text,
                    WinFormsShims.MessageBoxButtons.YesNo,
                    WinFormsShims.MessageBoxIcon.Question) == WinFormsShims.DialogResult.Yes)
            {
                UICommands.StartCommitDialog(this);
            }
        });
    }

    private void BranchesDropDown(object? sender, EventArgs e)
    {
        string remote = GetSelectedRemoteName();
        if ((PullFromUrl.IsChecked == true && string.IsNullOrEmpty(comboBoxPullSource.Text))
            || (PullFromRemote.IsChecked == true && string.IsNullOrEmpty(remote)))
        {
            Branches.Items.Clear();
            return;
        }

        using (WaitCursorScope.Enter())
        {
            LoadPuttyKey();

            if (_heads is null)
            {
                // The line below is the most reliable way to get a list containing
                // all remote branches but it is also the slowest.
                // Heads = GitCommands.GitCommands.GetRemoteHeads(Remotes.Text, false, true);

                // The code below is a quick way to get a list contains all remote branches.
                // It only returns the heads that are already known to the repository. This
                // doesn't return heads that are new on the server. This can be updated using
                // update branch info in the manage remotes dialog.
                _heads = PullFromUrl.IsChecked == true
                    ? [.. Module.GetRefs(RefsFilter.Heads)]
                    : [.. Module.GetRefs(RefsFilter.Remotes)
                        .Where(head => head.Remote.Equals(remote, StringComparison.OrdinalIgnoreCase))];
            }

            string selected = Branches.Text ?? string.Empty;
            Branches.Items.Clear();
            Branches.Items.Add(string.Empty);
            foreach (string head in _heads.Select(head => head.LocalName).Distinct(StringComparer.Ordinal).OrderBy(head => head, StringComparer.Ordinal))
            {
                Branches.Items.Add(head);
            }

            Branches.Text = selected;
        }
    }

    private void PullClick(object? sender, EventArgs e)
    {
        WinFormsShims.DialogResult dialogResult = PullChanges(this);
        if (dialogResult != WinFormsShims.DialogResult.No)
        {
            DialogResult = dialogResult;
        }
    }

    public WinFormsShims.DialogResult PullChanges(WinFormsShims.IWin32Window? owner)
    {
        ErrorOccurred = false;
        if (!ShouldPullChanges(owner))
        {
            return WinFormsShims.DialogResult.No;
        }

        UpdateSettingsDuringPull();
        WinFormsShims.DialogResult rebaseChoice = ShouldRebaseMergeCommit(owner);
        if (rebaseChoice != WinFormsShims.DialogResult.Yes)
        {
            return rebaseChoice;
        }

        if (Fetch.IsChecked != true && string.IsNullOrWhiteSpace(Branches.Text) && Module.IsDetachedHead())
        {
            TaskDialogPage page = new()
            {
                Text = _notOnBranch.Text,
                Heading = TranslatedStrings.ErrorInstructionNotOnBranch,
                Caption = TranslatedStrings.ErrorCaptionNotOnBranch,
                Buttons = { TaskDialogButton.Cancel },
                Icon = TaskDialogIcon.Error,
                AllowCancel = true,
                SizeToContent = true,
            };
            TaskDialogCommandLinkButton btnCheckout = new(TranslatedStrings.ButtonCheckoutBranch);
            TaskDialogCommandLinkButton btnContinue = new(TranslatedStrings.ButtonContinue);
            page.Buttons.Add(btnCheckout);
            page.Buttons.Add(btnContinue);
            TaskDialogButton result = TaskDialog.ShowDialog(owner, page);
            if (result == TaskDialogButton.Cancel)
            {
                return WinFormsShims.DialogResult.Cancel;
            }

            if (result == btnCheckout && !UICommands.StartCheckoutBranch(owner))
            {
                return WinFormsShims.DialogResult.Cancel;
            }
        }

        if (PullFromUrl.IsChecked == true && Directory.Exists(comboBoxPullSource.Text))
        {
            ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Remotes.AddAsMostRecentAsync(comboBoxPullSource.Text!));
        }

        string source = CalculateSource();
        if (!CalculateLocalBranch(owner, source, out string? curLocalBranch, out string? curRemoteBranch))
        {
            return WinFormsShims.DialogResult.No;
        }

        if (!ExecuteBeforeScripts())
        {
            return WinFormsShims.DialogResult.No;
        }

        bool stashed = CalculateStashedValue(owner);
        using FormProcess form = CreateFormProcess(source, curLocalBranch, curRemoteBranch);
        if (!IsPullAll())
        {
            form.Remote = source;
        }

        form.ShowDialog(owner);
        ErrorOccurred = form.ErrorOccurred();
        Module.InvalidateGitSettings();
        bool executeScripts = false;
        try
        {
            bool aborted = form.DialogResult == WinFormsShims.DialogResult.Abort;
            executeScripts = !aborted && !ErrorOccurred;
            if (!aborted && Fetch.IsChecked != true)
            {
                if (!ErrorOccurred)
                {
                    // If the "Update submodules on checkout" option is `true`, initialize and update
                    // all submodules. If it's `false` don't initialize/update the submodules. If it's
                    // indeterminate, ask the user what they'd like to do.
                    if (!InitModules(owner))
                    {
                        UICommands.UpdateSubmodules(owner);
                    }
                }
                else
                {
                    executeScripts |= CheckMergeConflictsOnError(owner);
                }
            }
        }
        finally
        {
            if (stashed)
            {
                PopStash(owner);
            }

            if (executeScripts)
            {
                ExecuteAfterScripts();
            }
        }

        return WinFormsShims.DialogResult.OK;

        void ExecuteAfterScripts()
        {
            ScriptsRunner.RunEventScripts(ScriptEvent.AfterFetch, this);

            // Request to pull/merge in addition to the fetch
            if (Fetch.IsChecked != true)
            {
                ScriptsRunner.RunEventScripts(ScriptEvent.AfterPull, this);
            }
        }

        bool ExecuteBeforeScripts()
        {
            // Request to pull/merge in addition to the fetch
            if (Fetch.IsChecked != true
                && !ScriptsRunner.RunEventScripts(ScriptEvent.BeforePull, this))
            {
                return false;
            }

            return ScriptsRunner.RunEventScripts(ScriptEvent.BeforeFetch, this);
        }
    }

    private bool ShouldPullChanges(WinFormsShims.IWin32Window? owner)
    {
        if (PullFromUrl.IsChecked == true && string.IsNullOrWhiteSpace(comboBoxPullSource.Text))
        {
            MessageBoxes.Show(owner, _selectSourceDirectory.Text, TranslatedStrings.Error, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
            return false;
        }

        if (PullFromRemote.IsChecked == true && string.IsNullOrWhiteSpace(GetSelectedRemoteName()) && !IsPullAll())
        {
            MessageBoxes.Show(owner, _selectRemoteRepository.Text, TranslatedStrings.Error, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
            return false;
        }

        if (Fetch.IsChecked != true && Branches.Text == "*")
        {
            MessageBoxes.Show(owner, _fetchAllBranchesCanOnlyWithFetch.Text, TranslatedStrings.Error, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
            return false;
        }

        return true;
    }

    private string CalculateSource()
    {
        if (PullFromUrl.IsChecked == true)
        {
            return comboBoxPullSource.Text?.Trim() ?? string.Empty;
        }

        LoadPuttyKey();
        return IsPullAll() ? "--all" : GetSelectedRemoteName();
    }

    private bool InitModules(WinFormsShims.IWin32Window? owner)
    {
        if (!File.Exists(_fullPathResolver.Resolve(".gitmodules")))
        {
            return false;
        }

        // Fast submodules check
        bool initialized = Module.GetSubmodulesLocalPaths()
            .Select(submoduleName => Module.GetSubmodule(submoduleName))
            .All(submodule => submodule.IsValidGitWorkingDir());
        if (initialized)
        {
            return false;
        }

        bool shouldInitialize = AppSettings.UpdateSubmodulesOnCheckout
            ?? AppSettings.DontConfirmUpdateSubmodulesOnCheckout
            ?? MessageBoxes.Show(
                owner,
                _questionInitSubmodules.Text,
                _questionInitSubmodulesCaption.Text,
                WinFormsShims.MessageBoxButtons.YesNo,
                WinFormsShims.MessageBoxIcon.Question) == WinFormsShims.DialogResult.Yes;
        if (shouldInitialize)
        {
            UICommands.StartUpdateSubmodulesDialog(owner);
        }

        return true;
    }

    private bool CheckMergeConflictsOnError(WinFormsShims.IWin32Window? owner)
    {
        // Rebase failed -> special 'rebase' merge conflict
        if (Rebase.IsChecked == true && Module.InTheMiddleOfRebase())
        {
            return UICommands.StartTheContinueRebaseDialog(owner);
        }

        return Module.InTheMiddleOfAction() && MergeConflictHandler.HandleMergeConflicts(UICommands, owner);
    }

    private void PopStash(WinFormsShims.IWin32Window? owner)
    {
        if (ErrorOccurred || Module.InTheMiddleOfAction())
        {
            return;
        }

        bool? popStash = AppSettings.AutoPopStashAfterPull;
        if (popStash is null)
        {
            TaskDialogPage page = new()
            {
                Text = _applyStashedItemsAgain.Text,
                Caption = _applyStashedItemsAgainCaption.Text,
                Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
                Icon = TaskDialogIcon.Information,
                Verification = new TaskDialogVerificationCheckBox { Text = TranslatedStrings.DontShowAgain },
                SizeToContent = true,
            };
            popStash = TaskDialog.ShowDialog(owner, page) == TaskDialogButton.Yes;
            if (page.Verification.Checked)
            {
                AppSettings.AutoPopStashAfterPull = popStash;
            }
        }

        if (popStash == true)
        {
            UICommands.StashPop(owner);
        }
    }

    private void UpdateSettingsDuringPull()
    {
        AppSettings.FormPullAction = Merge.IsChecked == true
            ? GitPullAction.Merge
            : Rebase.IsChecked == true
                ? GitPullAction.Rebase
                : Fetch.IsChecked == true
                    ? GitPullAction.Fetch
                    : GitPullAction.Default;
        AppSettings.AutoStash = AutoStash.IsChecked == true;
    }

    private WinFormsShims.DialogResult ShouldRebaseMergeCommit(WinFormsShims.IWin32Window? owner)
    {
        // ask only if exists commit not pushed to remote yet
        if (Rebase.IsChecked == true && PullFromRemote.IsChecked == true && MergeCommitExists())
        {
            return MessageBoxes.Show(
                owner,
                _areYouSureYouWantToRebaseMerge.Text,
                _areYouSureYouWantToRebaseMergeCaption.Text,
                WinFormsShims.MessageBoxButtons.YesNoCancel,
                WinFormsShims.MessageBoxIcon.Warning,
                WinFormsShims.MessageBoxDefaultButton.Button2);
        }

        return WinFormsShims.DialogResult.Yes;
    }

    private bool CalculateStashedValue(WinFormsShims.IWin32Window? owner)
    {
        if (Fetch.IsChecked != true
            && AutoStash.IsChecked == true
            && !Module.IsBareRepository()
            && Module.GitStatus(UntrackedFilesMode.No, IgnoreSubmodulesMode.All).Count > 0)
        {
            UICommands.StashSave(owner, AppSettings.IncludeUntrackedFilesInAutoStash);
            return true;
        }

        return false;
    }

    private FormProcess CreateFormProcess(string source, string? curLocalBranch, string? curRemoteBranch)
    {
        if (Fetch.IsChecked == true)
        {
            return new FormRemoteProcess(
                UICommands,
                Module.FetchCmd(source, curRemoteBranch, curLocalBranch, GetTagsArgument(), Unshallow.IsChecked == true, Prune.IsChecked == true, PruneTags.IsChecked == true));
        }

        ArgumentString arguments = Module.PullCmd(
            source,
            curRemoteBranch,
            Rebase.IsChecked == true,
            GetTagsArgument(),
            Unshallow.IsChecked == true);
        if (Merge.IsChecked == true)
        {
            arguments = $"-c pull.rebase=false {arguments}";
        }

        FormRemoteProcess form = new(
            UICommands,
            arguments)
        {
            HandleOnExitCallback = HandlePullOnExit,
        };
        return form;

        bool? GetTagsArgument()
            => AllTags.IsChecked == true
                ? true
                : NoTags.IsChecked == true ? false : null;

        bool HandlePullOnExit(ref bool isError, FormProcess process)
        {
            if (!isError || PullFromRemote.IsChecked != true || string.IsNullOrWhiteSpace(GetSelectedRemoteName()))
            {
                return false;
            }

            // auto pull only if current branch was rejected
            if (IsRefRemoved.IsMatch(process.GetOutputString()))
            {
                TaskDialogPage page = new()
                {
                    Text = _pruneBranchesBranch.Text,
                    Caption = _pruneBranchesCaption.Text,
                    Heading = _pruneBranchesMainInstruction.Text,
                    Buttons = { TaskDialogButton.Yes, TaskDialogButton.No, TaskDialogButton.Cancel },
                    Icon = TaskDialogIcon.Information,
                    DefaultButton = TaskDialogButton.No,
                    SizeToContent = true,
                };
                if (TaskDialog.ShowDialog(process, page) == TaskDialogButton.Yes)
                {
                    string remote = GetSelectedRemoteName();
                    GitArgumentBuilder pruneArguments = new("remote")
                    {
                        "prune",
                        remote.QuoteNE(),
                    };
                    using FormRemoteProcess formPrune = new(UICommands, pruneArguments)
                    {
                        Remote = remote,
                        Text = string.Format(_pruneFromCaption.Text, remote),
                    };
                    formPrune.ShowDialog(process);
                }
            }

            return false;
        }
    }

    private bool CalculateLocalBranch(
        WinFormsShims.IWin32Window? owner,
        string remote,
        out string? curLocalBranch,
        out string? curRemoteBranch)
    {
        if (IsPullAll())
        {
            curLocalBranch = null;
            curRemoteBranch = null;
            return true;
        }

        curRemoteBranch = Branches.Text;
        if (DetachedHeadParser.IsDetachedHead(_branch))
        {
            curLocalBranch = null;
            return true;
        }

        string localBranchName = localBranch.Text?.Trim() ?? string.Empty;
        Lazy<string> currentBranchRemote = new(() => Module.GetSetting(string.Format(SettingKeyString.BranchRemote, localBranchName)));
        if (_branch == localBranchName)
        {
            // if local branch eq to current branch and remote branch is not specified
            // then run fetch with no refspec
            curLocalBranch = remote == currentBranchRemote.Value || string.IsNullOrEmpty(currentBranchRemote.Value)
                ? string.IsNullOrEmpty(Branches.Text) ? null : _branch
                : localBranchName;
        }
        else
        {
            curLocalBranch = localBranchName;
        }

        if (string.IsNullOrEmpty(Branches.Text)
            && !string.IsNullOrEmpty(curLocalBranch)
            && remote != currentBranchRemote.Value
            && Fetch.IsChecked != true)
        {
            TaskDialogPage page = new()
            {
                Text = string.Format(_noRemoteBranchMainInstruction.Text, remote),
                Caption = _noRemoteBranchCaption.Text,
                Heading = _noRemoteBranch.Text,
                Buttons = { TaskDialogButton.Cancel },
                Icon = TaskDialogIcon.Information,
                Verification = new TaskDialogVerificationCheckBox { Text = TranslatedStrings.DontShowAgain },
                AllowCancel = false,
                SizeToContent = true,
            };
            TaskDialogCommandLinkButton btnPullFrom = new(string.Format(_noRemoteBranchButton.Text, remote + "/" + curLocalBranch));
            page.Buttons.Add(btnPullFrom);
            if (TaskDialog.ShowDialog(owner, page) == btnPullFrom)
            {
                curRemoteBranch = curLocalBranch;
                return true;
            }

            return false;
        }

        if (string.IsNullOrEmpty(Branches.Text) && !string.IsNullOrEmpty(curLocalBranch) && Fetch.IsChecked == true)
        {
            if (_branch == curLocalBranch)
            {
                curLocalBranch = null;
                return true;
            }

            TaskDialogPage page = new()
            {
                Text = string.Format(_noRemoteBranchForFetchMainInstruction.Text, remote),
                Caption = _noRemoteBranchCaption.Text,
                Heading = _noRemoteBranch.Text,
                Buttons = { TaskDialogButton.Cancel },
                Icon = TaskDialogIcon.Information,
                AllowCancel = false,
                SizeToContent = true,
            };
            TaskDialogCommandLinkButton btnPullFrom = new(string.Format(_noRemoteBranchForFetchButton.Text, remote + "/" + curLocalBranch));
            page.Buttons.Add(btnPullFrom);
            TaskDialogButton result = TaskDialog.ShowDialog(owner, page);
            if (result == TaskDialogButton.Cancel)
            {
                return false;
            }

            if (result == btnPullFrom)
            {
                curRemoteBranch = curLocalBranch;
                return true;
            }
        }

        return true;
    }

    private bool MergeCommitExists()
        => Module.ExistsMergeCommit(CalculateRemoteBranchName(), _branch);

    private string CalculateRemoteBranchName()
    {
        string remoteBranchName = CalculateRemoteBranchNameBasedOnBranchesText();
        return string.IsNullOrEmpty(remoteBranchName)
            ? remoteBranchName
            : GetSelectedRemoteName() + "/" + remoteBranchName;
    }

    private string CalculateRemoteBranchNameBasedOnBranchesText()
    {
        if (!string.IsNullOrEmpty(Branches.Text))
        {
            return Branches.Text;
        }

        string remoteBranchName = Module.GetSetting(string.Format(BranchMergeSetting, _branch));
        if (!string.IsNullOrEmpty(remoteBranchName))
        {
            GitArgumentBuilder arguments = new("name-rev")
            {
                "--name-only",
                remoteBranchName.QuoteNE(),
            };
            remoteBranchName = Module.GitExecutable.GetOutput(arguments).Trim();
        }

        return remoteBranchName;
    }

    private void LoadPuttyKey()
    {
        if (!GitSshHelpers.IsPlink)
        {
            return;
        }

        HashSet<string> files = new(new PathEqualityComparer());
        foreach (string remote in GetSelectedRemotes())
        {
            string sshKeyFile = Module.GetPuttyKeyFileForRemote(remote);
            if (!string.IsNullOrEmpty(sshKeyFile))
            {
                files.Add(sshKeyFile);
            }
        }

        foreach (string sshKeyFile in files)
        {
            PuttyHelpers.StartPageantIfConfigured(() => sshKeyFile);
        }

        return;

        IEnumerable<string> GetSelectedRemotes()
        {
            if (PullFromUrl.IsChecked == true)
            {
                yield break;
            }

            if (IsPullAll())
            {
                foreach (string remote in _NO_TRANSLATE_Remotes.Items.OfType<string>())
                {
                    if (!string.IsNullOrWhiteSpace(remote) && remote != AllRemotes)
                    {
                        yield return remote;
                    }
                }
            }
            else if (!string.IsNullOrWhiteSpace(GetSelectedRemoteName()))
            {
                yield return GetSelectedRemoteName();
            }
        }
    }

    private void FormPullLoad(object sender, EventArgs e)
    {
        _NO_TRANSLATE_Remotes.Focus();

        // Reapply the requested action after Avalonia attaches and normalizes the radio group.
        if (_runtimeInitialized)
        {
            SetPullAction(_pullAction, GetSelectedRemoteName());
        }

        UpdateFormTitleAndButton();
    }

    private void UpdateFormTitleAndButton()
    {
        bool fetch = Fetch.IsChecked == true;
        Pull.Content = AvaloniaTranslationUtils.ToAvaloniaMnemonics(fetch ? _buttonFetch.Text : _buttonPull.Text);
        if (_runtimeInitialized)
        {
            Title = string.Format(fetch ? _formTitleFetch.Text : _formTitlePull.Text, PathUtil.GetDisplayPath(Module.WorkingDir));
        }
    }

    private void StashClick(object? sender, EventArgs e)
        => UICommands.StartStashDialog(this);

    private void PullFromRemoteCheckedChanged(object? sender, EventArgs e)
    {
        if (PullFromRemote.IsChecked != true)
        {
            return;
        }

        ResetRemoteHeads();
        comboBoxPullSource.IsEnabled = false;
        folderBrowserButton1.IsEnabled = false;
        _NO_TRANSLATE_Remotes.IsEnabled = true;
        AddRemote.IsEnabled = true;
        Merge.IsEnabled = !IsPullAll();
        Rebase.IsEnabled = !IsPullAll();
        UpdateActionState();
    }

    private bool IsPullAll()
        => GetSelectedRemoteName().Equals(AllRemotes, StringComparison.InvariantCultureIgnoreCase);

    private string GetSelectedRemoteName()
        => _NO_TRANSLATE_Remotes.Text?.Trim()
            ?? _NO_TRANSLATE_Remotes.SelectedItem as string
            ?? string.Empty;

    private void PullFromUrlCheckedChanged(object? sender, EventArgs e)
    {
        if (PullFromUrl.IsChecked != true)
        {
            return;
        }

        ResetRemoteHeads();
        comboBoxPullSource.IsEnabled = true;
        folderBrowserButton1.IsEnabled = true;
        _NO_TRANSLATE_Remotes.IsEnabled = false;
        AddRemote.IsEnabled = false;
        Merge.IsEnabled = true;
        Rebase.IsEnabled = true;
        string previous = comboBoxPullSource.Text ?? string.Empty;
        IList<Repository> history = ThreadHelper.JoinableTaskFactory.Run(RepositoryHistoryManager.Remotes.LoadRecentHistoryAsync);
        comboBoxPullSource.ItemsSource = history.Select(repository => repository.Path).ToList();
        comboBoxPullSource.Text = previous;
        UpdateActionState();
    }

    private void AddRemoteClick(object? sender, EventArgs e)
    {
        string selectedRemote = IsPullAll() ? string.Empty : GetSelectedRemoteName();
        if (!UICommands.StartRemotesDialog(this, string.IsNullOrEmpty(selectedRemote) ? null : selectedRemote))
        {
            return;
        }

        BindRemotesDropDown(selectedRemote);
    }

    private void MergeCheckedChanged(object? sender, EventArgs e)
    {
        if (Merge.IsChecked != true)
        {
            return;
        }

        localBranch.IsEnabled = false;
        localBranch.Text = _branch;
        PanelLeftImage.Image1 = Properties.Images.HelpPullMerge.AdaptLightness();
        PanelLeftImage.Image2 = Properties.Images.HelpPullMergeFastForward.AdaptLightness();
        PanelLeftImage.IsOnHoverShowImage2 = true;
        AllTags.IsEnabled = false;
        Prune.IsEnabled = false;
        PruneTags.IsEnabled = false;
        if (AllTags.IsChecked == true)
        {
            ReachableTags.IsChecked = true;
        }

        UpdateFormTitleAndButton();
        UpdateActionState();
    }

    private void RebaseCheckedChanged(object? sender, EventArgs e)
    {
        if (Rebase.IsChecked != true)
        {
            return;
        }

        localBranch.IsEnabled = false;
        localBranch.Text = _branch;
        PanelLeftImage.Image1 = Properties.Images.HelpPullRebase.AdaptLightness();
        PanelLeftImage.IsOnHoverShowImage2 = false;
        AllTags.IsEnabled = false;
        Prune.IsEnabled = false;
        PruneTags.IsEnabled = false;
        if (AllTags.IsChecked == true)
        {
            ReachableTags.IsChecked = true;
        }

        UpdateFormTitleAndButton();
        UpdateActionState();
    }

    private void FetchCheckedChanged(object? sender, EventArgs e)
    {
        if (Fetch.IsChecked != true)
        {
            return;
        }

        localBranch.IsEnabled = true;
        localBranch.Text = string.Empty;
        PanelLeftImage.Image1 = Properties.Images.HelpPullFetch.AdaptLightness();
        PanelLeftImage.IsOnHoverShowImage2 = false;
        AllTags.IsEnabled = true;
        Prune.IsEnabled = true;
        PruneTags.IsEnabled = true;
        UpdateFormTitleAndButton();
        UpdateActionState();
    }

    private void SetPullAction(GitPullAction pullAction, string? defaultRemote)
    {
        if (pullAction is GitPullAction.None or GitPullAction.Default)
        {
            pullAction = AppSettings.DefaultPullAction;
        }

        _pullAction = pullAction;

        // Avalonia normalizes radio groups after attachment; clear siblings explicitly to match WinForms pre-show selection.
        switch (pullAction)
        {
            case GitPullAction.Rebase:
                Merge.IsChecked = false;
                Fetch.IsChecked = false;
                Rebase.IsChecked = true;
                break;
            case GitPullAction.Fetch:
                Merge.IsChecked = false;
                Rebase.IsChecked = false;
                Fetch.IsChecked = true;
                break;
            case GitPullAction.FetchAll:
                Merge.IsChecked = false;
                Rebase.IsChecked = false;
                Fetch.IsChecked = true;
                SelectRemote(AllRemotes);
                break;
            case GitPullAction.FetchPruneAll:
                Merge.IsChecked = false;
                Rebase.IsChecked = false;
                Fetch.IsChecked = true;
                Prune.IsChecked = true;
                PruneTags.IsChecked = false;
                SelectRemote(string.IsNullOrEmpty(defaultRemote) ? AllRemotes : defaultRemote);

                break;
            default:
                Rebase.IsChecked = false;
                Fetch.IsChecked = false;
                Merge.IsChecked = true;
                break;
        }

        if (Merge.IsChecked == true)
        {
            MergeCheckedChanged(this, EventArgs.Empty);
        }
        else if (Rebase.IsChecked == true)
        {
            RebaseCheckedChanged(this, EventArgs.Empty);
        }
        else
        {
            FetchCheckedChanged(this, EventArgs.Empty);
        }
    }

    private void SelectRemote(string remote)
    {
        _bInternalUpdate = true;
        _NO_TRANSLATE_Remotes.SelectedItem = _NO_TRANSLATE_Remotes.Items.OfType<string>().FirstOrDefault(item => item == remote);
        _NO_TRANSLATE_Remotes.Text = remote;
        _bInternalUpdate = false;
        RemotesValidating(_NO_TRANSLATE_Remotes, new CancelEventArgs());
    }

    private void PullSourceValidating(object sender, CancelEventArgs e)
    {
        ResetRemoteHeads();
    }

    private void Remotes_TextChanged(object sender, EventArgs e)
    {
        if (!_bInternalUpdate)
        {
            RemotesValidating(this, new CancelEventArgs());
        }
    }

    private void RemotesValidating(object sender, CancelEventArgs e)
    {
        ResetRemoteHeads();
        string remote = GetSelectedRemoteName();
        if (!string.IsNullOrEmpty(remote) && remote != AllRemotes)
        {
            // update the text box of the Remote Url combobox to show the URL of selected remote
            comboBoxPullSource.Text = Module.GetSetting(string.Format(SettingKeyString.RemoteUrl, remote));
        }

        // update merge options radio buttons
        Merge.IsEnabled = !IsPullAll() || PullFromUrl.IsChecked == true;
        Rebase.IsEnabled = !IsPullAll() || PullFromUrl.IsChecked == true;
        if (IsPullAll() && PullFromRemote.IsChecked == true)
        {
            Fetch.IsChecked = true;
        }
    }

    private void ResetRemoteHeads()
    {
        _heads = null;
        Branches.Items.Clear();
    }

    private void localBranch_Leave(object sender, EventArgs e)
    {
        if (_branch != localBranch.Text?.Trim() && string.IsNullOrWhiteSpace(Branches.Text))
        {
            Branches.Text = localBranch.Text;
        }
    }

    private void Prune_CheckedChanged(object sender, EventArgs e)
    {
        PruneTags.IsChecked = Prune.IsChecked == true && PruneTags.IsChecked == true;
    }

    private void PruneTags_CheckedChanged(object sender, EventArgs e)
    {
        Prune.IsChecked = Prune.IsChecked == true || PruneTags.IsChecked == true;
        AllTags.IsChecked = AllTags.IsChecked == true || PruneTags.IsChecked == true;
    }

    private string? GetConfiguredRemoteBranch()
    {
        if (string.IsNullOrWhiteSpace(_branch))
        {
            return null;
        }

        string? mergeRef = Module.GetSetting(string.Format(BranchMergeSetting, _branch));
        return mergeRef?.StartsWith("refs/heads/", StringComparison.Ordinal) == true
            ? mergeRef["refs/heads/".Length..]
            : mergeRef;
    }

    private void UpdateActionState()
    {
        bool sourceSelected = PullFromUrl.IsChecked == true
            ? !string.IsNullOrWhiteSpace(comboBoxPullSource.Text)
            : !string.IsNullOrWhiteSpace(GetSelectedRemoteName());
        Pull.IsEnabled = sourceSelected && (Fetch.IsChecked == true || !string.IsNullOrWhiteSpace(Branches.Text));
    }

    public override void AddTranslationItems(ITranslation translation)
    {
        (Control Control, object? Tip)[] toolTips =
        [
            (PullFromRemote, ToolTip.GetTip(PullFromRemote)),
            (PullFromUrl, ToolTip.GetTip(PullFromUrl)),
            (lblLocalBranch, ToolTip.GetTip(lblLocalBranch)),
            (lblRemoteBranch, ToolTip.GetTip(lblRemoteBranch)),
            (Prune, ToolTip.GetTip(Prune)),
            (PruneTags, ToolTip.GetTip(PruneTags)),
        ];
        foreach ((Control control, _) in toolTips)
        {
            ToolTip.SetTip(control, null);
        }

        try
        {
            base.AddTranslationItems(translation);
        }
        finally
        {
            foreach ((Control control, object? tip) in toolTips)
            {
                ToolTip.SetTip(control, tip);
            }
        }

        translation.AddTranslationItem(nameof(FormPull), nameof(PullFromRemote), "Tooltip", PullFromRemoteToolTip);
        translation.AddTranslationItem(nameof(FormPull), nameof(PullFromUrl), "Tooltip", PullFromUrlToolTip);
        translation.AddTranslationItem(nameof(FormPull), nameof(lblLocalBranch), "Tooltip", LocalBranchToolTip);
        translation.AddTranslationItem(nameof(FormPull), nameof(lblRemoteBranch), "Tooltip", RemoteBranchToolTip);
        translation.AddTranslationItem(nameof(FormPull), nameof(Prune), "Tooltip", PruneToolTip);
        translation.AddTranslationItem(nameof(FormPull), nameof(PruneTags), "Tooltip", PruneTagsToolTip);
    }

    public override void TranslateItems(ITranslation translation)
    {
        ToolTip.SetTip(PullFromRemote, null);
        ToolTip.SetTip(PullFromUrl, null);
        ToolTip.SetTip(lblLocalBranch, null);
        ToolTip.SetTip(lblRemoteBranch, null);
        ToolTip.SetTip(Prune, null);
        ToolTip.SetTip(PruneTags, null);
        base.TranslateItems(translation);
        ToolTip.SetTip(PullFromRemote, Translate(PullFromRemote, PullFromRemoteToolTip));
        ToolTip.SetTip(PullFromUrl, Translate(PullFromUrl, PullFromUrlToolTip));
        ToolTip.SetTip(lblLocalBranch, Translate(lblLocalBranch, LocalBranchToolTip));
        ToolTip.SetTip(lblRemoteBranch, Translate(lblRemoteBranch, RemoteBranchToolTip));
        ToolTip.SetTip(Prune, Translate(Prune, PruneToolTip));
        ToolTip.SetTip(PruneTags, Translate(PruneTags, PruneTagsToolTip));
        PanelLeftImage.IsOnHoverShowImage2NoticeText = _hoverShowImageLabelText.Text;
        UpdateFormTitleAndButton();

        string Translate(Control control, string fallback)
            => translation.TranslateItem(nameof(FormPull), control.Name!, "Tooltip", () => fallback) ?? fallback;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormPull form)
    {
        internal string Title => form.Title ?? string.Empty;
        internal string FetchTitleText => form._formTitleFetch.Text;
        internal string PullTitleText => form._formTitlePull.Text;
        internal RadioButton Merge => form.Merge;
        internal RadioButton Rebase => form.Rebase;
        internal RadioButton Fetch => form.Fetch;
        internal CheckBox AutoStash => form.AutoStash;
        internal CheckBox Prune => form.Prune;
        internal CheckBox PruneTags => form.PruneTags;
        internal ComboBox Remotes => form._NO_TRANSLATE_Remotes;
        internal TextBox LocalBranch => form.localBranch;
        internal FormProcess CreateFormProcess(string source, string? localBranch, string? remoteBranch)
            => form.CreateFormProcess(source, localBranch, remoteBranch);
        internal bool CheckMergeConflictsOnError(WinFormsShims.IWin32Window? owner)
            => form.CheckMergeConflictsOnError(owner);
        internal void UpdateSettingsDuringPull() => form.UpdateSettingsDuringPull();
    }
}
