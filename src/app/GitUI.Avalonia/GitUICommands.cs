using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Shims.WinForms;
using GitExtUtils;
using GitUI.CommandsDialogs;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;

namespace GitUI;

// Twin of GitUI/GitUICommands.cs. Members are implemented as their dialogs get ported;
// everything else throws NotImplementedException naming the member, so a missing port
// surfaces clearly instead of failing silently.

/// <summary>Contains methods to invoke Git Extensions forms, dialogs, etc.</summary>
public sealed class GitUICommands : IGitUICommands
{
    private readonly IServiceProvider _serviceProvider;

    public IGitModule Module { get; private set; }
    public ILockableNotifier RepoChangedNotifier { get; }
    public IBrowseRepo? BrowseRepo { get; set; }

    public GitUICommands(IServiceProvider serviceProvider, IGitModule module)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(module);

        _serviceProvider = serviceProvider;
        Module = module;

        RepoChangedNotifier = new ActionNotifier(
            () => InvokeEvent(null, PostRepositoryChanged));
    }

    #region Events

    // Raised like WinForms as the corresponding actions get ported.
#pragma warning disable CS0067 // The event is never used
    public event EventHandler<GitUIEventArgs>? PreCheckoutRevision;
    public event EventHandler<GitUIPostActionEventArgs>? PostCheckoutRevision;

    public event EventHandler<GitUIEventArgs>? PreCheckoutBranch;
    public event EventHandler<GitUIPostActionEventArgs>? PostCheckoutBranch;

    public event EventHandler<GitUIEventArgs>? PreCommit;
    public event EventHandler<GitUIPostActionEventArgs>? PostCommit;

    public event EventHandler<GitUIPostActionEventArgs>? PostEditGitIgnore;

    public event EventHandler<GitUIPostActionEventArgs>? PostSettings;

    public event EventHandler<GitUIPostActionEventArgs>? PostUpdateSubmodules;

    public event EventHandler<GitUIEventArgs>? PostBrowseInitialize;

    /// <summary>
    /// listeners for changes being made to repository
    /// </summary>
    public event EventHandler<GitUIEventArgs>? PostRepositoryChanged;

    public event EventHandler<GitUIEventArgs>? PostRegisterPlugin;
#pragma warning restore CS0067

    #endregion

    public object? GetService(Type serviceType) => _serviceProvider.GetService(serviceType);

    public IGitUICommands WithGitModule(IGitModule module) => new GitUICommands(_serviceProvider, module);

    public IGitUICommands WithWorkingDirectory(string? workingDirectory)
        => new GitUICommands(_serviceProvider, new GitModule(this.GetRequiredService<IGitExecutorProvider>(), workingDirectory));

    public bool StartCommandLineProcessDialog(IWin32Window? owner, IGitCommand command)
    {
        bool success = command.AccessesRemote
            ? FormRemoteProcess.ShowDialog(owner, this, command.Arguments)
            : FormProcess.ShowDialog(owner, this, arguments: command.Arguments, Module.WorkingDir, input: null, useDialogSettings: true);

        if (success && command.ChangesRepoState)
        {
            RepoChangedNotifier.Notify();
        }

        return success;
    }

    public bool StartCommandLineProcessDialog(IWin32Window? owner, string? command, ArgumentString arguments)
    {
        return FormProcess.ShowDialog(owner, this, arguments, Module.WorkingDir, input: null, useDialogSettings: true, process: command);
    }

    public bool StartGitCommandProcessDialog(IWin32Window? owner, ArgumentString arguments)
    {
        return FormProcess.ShowDialog(owner, this, arguments, Module.WorkingDir, input: null, useDialogSettings: true);
    }

    public bool StashSave(IWin32Window? owner, bool includeUntrackedFiles, bool keepIndex = false, string message = "", IReadOnlyList<string>? selectedFiles = null)
    {
        bool Action()
        {
            ArgumentString arguments = Commands.StashSave(includeUntrackedFiles, keepIndex, message, selectedFiles);
            FormProcess.ShowDialog(owner, this, arguments, Module.WorkingDir, input: null, useDialogSettings: true);

            // git-stash may have changed commits also if aborted, the grid must be refreshed
            return true;
        }

        return DoActionOnRepo(owner, Action);
    }

    public bool StashStaged(IWin32Window? owner)
    {
        bool Action()
        {
            FormProcess.ShowDialog(owner, this, arguments: "stash --staged", Module.WorkingDir, input: null, useDialogSettings: true);

            // git-stash may have changed commits also if aborted, the grid must be refreshed
            return true;
        }

        return DoActionOnRepo(owner, Action);
    }

    public bool StashPop(IWin32Window? owner, string stashName = "")
    {
        bool Action()
        {
            FormProcess.ShowDialog(owner, this, arguments: $"stash pop {stashName.QuoteNE()}", Module.WorkingDir, input: null, useDialogSettings: true);
            MergeConflictHandler.HandleMergeConflicts(this, owner, false, false);

            // git-stash may have changed commits also if aborted, the grid must be refreshed
            return true;
        }

        return DoActionOnRepo(owner, Action);
    }

    public bool StashDrop(IWin32Window? owner, string stashName)
    {
        bool Action()
        {
            FormProcess.ShowDialog(owner, this, arguments: $"stash drop {stashName.Quote()}", Module.WorkingDir, input: null, useDialogSettings: true);

            // git-stash may have changed commits also if aborted, the grid must be refreshed
            return true;
        }

        return DoActionOnRepo(owner, Action);
    }

    public bool StashApply(IWin32Window? owner, string stashName)
    {
        bool Action()
        {
            FormProcess.ShowDialog(owner, this, arguments: $"stash apply {stashName.Quote()}", Module.WorkingDir, input: null, useDialogSettings: true);
            MergeConflictHandler.HandleMergeConflicts(this, owner, false, false);

            // git-stash may have changed commits also if aborted, the grid must be refreshed
            return true;
        }

        return DoActionOnRepo(owner, Action);
    }

    private bool InvokeEvent(IWin32Window? ownerForm, EventHandler<GitUIEventArgs>? gitUIEventHandler)
    {
        try
        {
            GitUIEventArgs eventArgs = new(ownerForm, this);
            gitUIEventHandler?.Invoke(this, eventArgs);
            return !eventArgs.Cancel;
        }
        catch (Exception ex)
        {
            MessageBoxes.ShowError(ownerForm, $"{ex.Message}{Environment.NewLine}{ex.StackTrace}", "Error");
            return false;
        }
    }

    private void InvokePostEvent(IWin32Window? ownerForm, bool actionDone, EventHandler<GitUIPostActionEventArgs>? gitUIEventHandler)
    {
        gitUIEventHandler?.Invoke(this, new GitUIPostActionEventArgs(ownerForm, this, actionDone));
    }

    private bool DoActionOnRepo(
        IWin32Window? owner,
        Func<bool> action,
        bool requiresValidWorkingDir = true,
        bool changesRepo = true,
        EventHandler<GitUIEventArgs>? preEvent = null,
        EventHandler<GitUIPostActionEventArgs>? postEvent = null)
    {
        bool actionDone = false;
        RepoChangedNotifier.Lock();
        try
        {
            if (requiresValidWorkingDir && !Module.IsValidGitWorkingDir())
            {
                return false;
            }

            if (!InvokeEvent(owner, preEvent))
            {
                return false;
            }

            try
            {
                actionDone = action();
            }
            finally
            {
                InvokePostEvent(owner, actionDone, postEvent);
            }
        }
        finally
        {
            bool requestNotify = actionDone && changesRepo && Module.IsValidGitWorkingDir();
            RepoChangedNotifier.UnLock(requestNotify);
        }

        return actionDone;
    }

    private static NotImplementedException NotPorted(string member)
        => new($"{member} is not ported to the Avalonia UI yet.");

    public void OpenWithDifftool(IWin32Window? owner, IReadOnlyList<GitRevision?> revisions, string fileName, string? oldFileName, RevisionDiffKind diffKind, bool isTracked, string? customTool = null)
    {
        // Note: Order in revisions is that first clicked is last in array.
        if (!RevisionDiffInfoProvider.TryGet(revisions, diffKind, out string? firstRevision, out string? secondRevision, out string? error))
        {
            MessageBoxes.Show(owner, error, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else
        {
            Module.OpenWithDifftool(fileName, oldFileName, firstRevision, secondRevision, isTracked: isTracked, customTool: customTool);
        }
    }

    #region Not ported yet

    public void AddCommitTemplate(string key, Func<string> addingText, Image? icon, bool isRegex = false) => throw NotPorted(nameof(AddCommitTemplate));
    public void AddUpstreamRemote(IWin32Window? owner, IRepositoryHostPlugin gitHoster) => throw NotPorted(nameof(AddUpstreamRemote));
    public IGitRemoteCommand CreateRemoteCommand() => throw NotPorted(nameof(CreateRemoteCommand));
    public bool DoActionOnRepo(Func<bool> action)
        => DoActionOnRepo(owner: null, action, requiresValidWorkingDir: false);
    public void RaisePostBrowseInitialize(IWin32Window? owner) => InvokeEvent(owner, PostBrowseInitialize);
    public void RaisePostRegisterPlugin(IWin32Window? owner) => InvokeEvent(owner, PostRegisterPlugin);
    public void RemoveCommitTemplate(string key) => throw NotPorted(nameof(RemoveCommitTemplate));
    public bool RunCommand(IReadOnlyList<string> args) => throw NotPorted(nameof(RunCommand));
    public void ShowModelessForm(IWin32Window? owner, bool requiresValidWorkingDir, EventHandler<GitUIEventArgs>? preEvent, EventHandler<GitUIPostActionEventArgs>? postEvent, Func<Form> provideForm) => throw NotPorted(nameof(ShowModelessForm));
    public bool StartAddFilesDialog(IWin32Window? owner, string? addFiles = null)
    {
        bool Action()
        {
            using FormAddFiles form = new(this, addFiles);
            form.ShowDialog(owner);
            return true;
        }

        return DoActionOnRepo(owner, Action);
    }

    public bool StartAddToGitIgnoreDialog(IWin32Window? owner, bool localExclude, params string[] filePattern) => throw NotPorted(nameof(StartAddToGitIgnoreDialog));
    public bool StartAmendCommitDialog(IWin32Window? owner, GitRevision revision) => throw NotPorted(nameof(StartAmendCommitDialog));
    public bool StartApplyPatchDialog(IWin32Window? owner, string? patchFile = null) => throw NotPorted(nameof(StartApplyPatchDialog));
    public bool StartArchiveDialog(IWin32Window? owner = null, GitRevision? revision = null, GitRevision? revision2 = null, string? path = null) => throw NotPorted(nameof(StartArchiveDialog));
    public void StartBatchFileProcessDialog(string batchFile) => throw NotPorted(nameof(StartBatchFileProcessDialog));
    public bool StartBrowseDialog(IWin32Window? owner, BrowseArguments? args = null) => throw NotPorted(nameof(StartBrowseDialog));
    public bool StartCheckoutBranch(IWin32Window? owner, IReadOnlyList<ObjectId>? containObjectIds)
        => StartCheckoutBranch(owner, string.Empty, remote: false, containObjectIds);

    public bool StartCheckoutBranch(IWin32Window? owner, string branch = "", bool remote = false, IReadOnlyList<ObjectId>? containObjectIds = null)
    {
        return DoActionOnRepo(owner, action: () =>
        {
            using CommandsDialogs.FormCheckoutBranch form = new(this, branch, remote, containObjectIds);
            return form.DoDefaultActionOrShow(owner) == DialogResult.OK;
        }, preEvent: PreCheckoutBranch, postEvent: PostCheckoutBranch);
    }

    public bool StartCheckoutRemoteBranch(IWin32Window? owner, string branch)
    {
        return StartCheckoutBranch(owner, branch, true);
    }

    public bool StartCheckoutRevisionDialog(IWin32Window? owner, string? revision = null) => throw NotPorted(nameof(StartCheckoutRevisionDialog));
    public bool StartCherryPickDialog(IWin32Window? owner = null, GitRevision? revision = null) => throw NotPorted(nameof(StartCherryPickDialog));
    public bool StartCherryPickDialog(IWin32Window? owner, IEnumerable<GitRevision> revisions) => throw NotPorted(nameof(StartCherryPickDialog));
    public bool StartCleanupRepositoryDialog(IWin32Window? owner = null, string? path = null) => throw NotPorted(nameof(StartCleanupRepositoryDialog));
    public bool StartCloneDialog(IWin32Window? owner, string url, EventHandler<GitModuleEventArgs> gitModuleChanged)
    {
        return StartCloneDialog(owner, url, false, gitModuleChanged);
    }

    public bool StartCloneDialog(IWin32Window? owner, string? url = null, bool openedFromProtocolHandler = false, EventHandler<GitModuleEventArgs>? gitModuleChanged = null)
    {
        bool Action()
        {
            CommandsDialogs.FormClone form = new(this, url, openedFromProtocolHandler, gitModuleChanged);
            form.ShowDialog(owner);
            return true;
        }

        return DoActionOnRepo(owner, Action, requiresValidWorkingDir: false, changesRepo: false);
    }

    public void StartCloneForkFromHoster(IWin32Window? owner, IRepositoryHostPlugin gitHoster, EventHandler<GitModuleEventArgs>? gitModuleChanged) => throw NotPorted(nameof(StartCloneForkFromHoster));
    public bool StartCommitDialog(IWin32Window? owner, string? commitMessage = null, bool showOnlyWhenChanges = false)
    {
        if (Module.IsBareRepository())
        {
            return false;
        }

        return DoActionOnRepo(owner, action: () =>
        {
            if (showOnlyWhenChanges && Module.GetAllChangedFilesWithSubmodulesStatus(CancellationToken.None).Count == 0)
            {
                return true;
            }

            using CommandsDialogs.FormCommit form = new(this, commitMessage: commitMessage);
            form.ShowDialog(owner);
            return true;
        }, changesRepo: false, preEvent: PreCommit, postEvent: PostCommit);
    }

    public bool StartCompareRevisionsDialog(IWin32Window? owner = null) => throw NotPorted(nameof(StartCompareRevisionsDialog));
    public bool StartCreateBranchDialog(IWin32Window? owner, string? branch)
    {
        ObjectId objectId = Module.RevParse(branch!);
        if (objectId.IsZero)
        {
            MessageBoxes.Show($"Branch \"{branch}\" could not be resolved.", TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        return StartCreateBranchDialog(owner, objectId);
    }

    public bool StartCreateBranchDialog(IWin32Window? owner = null, ObjectId objectId = default, string? newBranchNamePrefix = null)
    {
        if (Module.IsBareRepository() || objectId.IsArtificial)
        {
            return false;
        }

        return DoActionOnRepo(owner, action: () =>
        {
            using CommandsDialogs.FormCreateBranch form = new(this, objectId, newBranchNamePrefix);
            return form.ShowDialog(owner) == DialogResult.OK;
        });
    }

    public void StartCreatePullRequest(IWin32Window? owner) => throw NotPorted(nameof(StartCreatePullRequest));
    public void StartCreatePullRequest(IWin32Window? owner, IRepositoryHostPlugin gitHoster, string? chooseRemote = null, string? chooseBranch = null) => throw NotPorted(nameof(StartCreatePullRequest));
    public bool StartCreateTagDialog(IWin32Window? owner = null, GitRevision? revision = null)
    {
        if (revision?.IsArtificial is true)
        {
            return false;
        }

        return DoActionOnRepo(owner, action: () =>
        {
            using CommandsDialogs.FormCreateTag form = new(this, revision?.ObjectId ?? default);
            return form.ShowDialog(owner) == DialogResult.OK;
        });
    }

    public bool StartDeleteBranchDialog(IWin32Window? owner, string branch)
    {
        return StartDeleteBranchDialog(owner, new[] { branch });
    }

    public bool StartDeleteBranchDialog(IWin32Window? owner, IEnumerable<string> branches)
    {
        return DoActionOnRepo(owner, action: () =>
        {
            using CommandsDialogs.FormDeleteBranch form = new(this, branches);
            form.ShowDialog(owner);
            return true;
        }, changesRepo: false);
    }

    public bool StartDeleteRemoteBranchDialog(IWin32Window? owner, string remoteBranch) => throw NotPorted(nameof(StartDeleteRemoteBranchDialog));
    public bool StartDeleteTagDialog(IWin32Window? owner, string? tag)
    {
        bool Action()
        {
            using CommandsDialogs.FormDeleteTag form = new(this, tag);
            return form.ShowDialog(owner) == DialogResult.OK;
        }

        return DoActionOnRepo(owner, Action);
    }

    public bool StartEditGitAttributesDialog(IWin32Window? owner = null) => throw NotPorted(nameof(StartEditGitAttributesDialog));
    public bool StartEditGitIgnoreDialog(IWin32Window? owner, bool localExcludes) => throw NotPorted(nameof(StartEditGitIgnoreDialog));
    public bool StartFileEditorDialog(string? filename, bool showWarning = false, int? lineNumber = null) => throw NotPorted(nameof(StartFileEditorDialog));
    public void StartFileHistoryDialog(IWin32Window? owner, string fileName, GitRevision? revision = null, bool filterByRevision = false, bool showBlame = false)
    {
        // The WinForms client launches a separate process (or reuses Browse) for file
        // history; this twin opens the window in-process, non-modal like that process.
        DoActionOnRepo(owner, action: () =>
        {
            CommandsDialogs.FormFileHistory form = new(this, fileName, revision, filterByRevision, showBlame);
            form.Show();
            return true;
        }, changesRepo: false);
    }

    public bool StartFixupCommitDialog(IWin32Window? owner, GitRevision revision) => throw NotPorted(nameof(StartFixupCommitDialog));
    public bool StartFormCommitDiff(ObjectId objectId)
    {
        bool Action()
        {
            using FormCommitDiff viewPatch = new(this, objectId);
            viewPatch.ShowDialog(null);
            return true;
        }

        return DoActionOnRepo(null, Action, requiresValidWorkingDir: false, changesRepo: false);
    }

    public bool StartFormatPatchDialog(IWin32Window? owner = null) => throw NotPorted(nameof(StartFormatPatchDialog));
    public bool StartGeneralSettingsDialog(IWin32Window? owner) => throw NotPorted(nameof(StartGeneralSettingsDialog));
    public bool StartInitializeDialog(IWin32Window? owner = null, string? dir = null, EventHandler<GitModuleEventArgs>? gitModuleChanged = null)
    {
        bool Action()
        {
            dir ??= Module.IsValidGitWorkingDir() ? Module.WorkingDir : string.Empty;

            CommandsDialogs.FormInit frm = new(this, dir, gitModuleChanged);
            frm.ShowDialog(owner);
            return true;
        }

        return DoActionOnRepo(owner, Action, requiresValidWorkingDir: false, changesRepo: false);
    }

    public bool StartInteractiveRebase(IWin32Window? owner, string onto)
    {
        return StartRebaseDialog(
            owner,
            from: string.Empty,
            to: null,
            onto,
            interactive: true,
            startRebaseImmediately: true);
    }

    public bool StartMailMapDialog(IWin32Window? owner = null) => throw NotPorted(nameof(StartMailMapDialog));
    public bool StartMergeBranchDialog(IWin32Window? owner, string? branch)
    {
        bool Action()
        {
            using FormMergeBranch form = new(this, branch);
            form.ShowDialog(owner);
            return true;
        }

        return DoActionOnRepo(owner, Action, changesRepo: false);
    }

    public bool StartPluginSettingsDialog(IWin32Window? owner) => throw NotPorted(nameof(StartPluginSettingsDialog));
    public bool StartPullDialog(IWin32Window? owner = null, string? remoteBranch = null, string? remote = null, GitPullAction pullAction = GitPullAction.None)
        => StartPullDialogInternal(owner, pullOnShow: false, out _, remoteBranch, remote, pullAction);

    public bool StartPullDialogAndPullImmediately(IWin32Window? owner = null, string? remoteBranch = null, string? remote = null, GitPullAction pullAction = GitPullAction.None)
        => StartPullDialogAndPullImmediately(out _, owner, remoteBranch, remote, pullAction);

    public bool StartPullDialogAndPullImmediately(out bool pullCompleted, IWin32Window? owner = null, string? remoteBranch = null, string? remote = null, GitPullAction pullAction = GitPullAction.None)
        => StartPullDialogInternal(owner, pullOnShow: true, out pullCompleted, remoteBranch, remote, pullAction);

    private bool StartPullDialogInternal(
        IWin32Window? owner,
        bool pullOnShow,
        out bool pullCompleted,
        string? remoteBranch,
        string? remote,
        GitPullAction pullAction)
    {
        bool pulled = false;
        bool done = DoActionOnRepo(owner, action: () =>
        {
            using CommandsDialogs.FormPull form = new(this, remoteBranch, remote, pullAction);
            DialogResult result = pullOnShow
                ? form.PullAndShowDialogWhenFailed(owner, remote, pullAction)
                : form.ShowDialog(owner);
            pulled = result == DialogResult.OK && !form.ErrorOccurred;
            return result == DialogResult.OK;
        });

        pullCompleted = pulled;
        return done;
    }

    public void StartPullRequestsDialog(IWin32Window? owner, IRepositoryHostPlugin gitHoster) => throw NotPorted(nameof(StartPullRequestsDialog));
    public bool StartPushDialog(IWin32Window? owner, bool pushOnShow)
        => StartPushDialog(owner, pushOnShow, forceWithLease: false, out _);

    public bool StartPushDialog(IWin32Window? owner, bool pushOnShow, bool forceWithLease, out bool pushCompleted, string? branchName = null)
    {
        bool pushed = false;
        bool done = DoActionOnRepo(owner, action: () =>
        {
            using CommandsDialogs.FormPush form = new(this, branchName);
            if (forceWithLease)
            {
                form.CheckForceWithLease();
            }

            DialogResult result = pushOnShow
                ? form.PushAndShowDialogWhenFailed(owner)
                : form.ShowDialog(owner);
            pushed = result == DialogResult.OK && !form.ErrorOccurred;
            return result == DialogResult.OK;
        });

        pushCompleted = pushed;
        return done;
    }

    public bool StartRebase(IWin32Window? owner, string onto)
    {
        return StartRebaseDialog(
            owner,
            from: string.Empty,
            to: null,
            onto,
            interactive: false,
            startRebaseImmediately: true);
    }

    public bool StartRebaseDialog(
        IWin32Window? owner,
        string? from,
        string? to,
        string? onto,
        bool interactive = false,
        bool startRebaseImmediately = true)
    {
        bool Action()
        {
            using FormRebase form = new(this, from, to, onto, interactive, startRebaseImmediately);
            form.ShowDialog(owner);
            return true;
        }

        return DoActionOnRepo(owner, Action);
    }

    public bool StartRebaseDialog(IWin32Window? owner, string? onto)
    {
        return StartRebaseDialog(
            owner,
            from: string.Empty,
            to: null,
            onto,
            interactive: false,
            startRebaseImmediately: false);
    }

    public bool StartRebaseDialogWithAdvOptions(IWin32Window? owner, string onto, string from = "")
    {
        return StartRebaseDialog(
            owner,
            from,
            to: null,
            onto,
            interactive: false,
            startRebaseImmediately: false);
    }

    public bool StartRemotesDialog(IWin32Window? owner, string? preselectRemote = null, string? preselectLocal = null)
    {
        bool Action()
        {
            CommandsDialogs.FormRemotes form = new(this)
            {
                PreselectRemoteOnLoad = preselectRemote,
                PreselectLocalOnLoad = preselectLocal
            };
            form.ShowDialog(owner);
            return true;
        }

        return DoActionOnRepo(owner, Action);
    }

    public bool StartRenameDialog(IWin32Window? owner, string branch)
    {
        bool Action()
        {
            using CommandsDialogs.FormRenameBranch form = new(this, branch);
            return form.ShowDialog(owner) == DialogResult.OK;
        }

        return DoActionOnRepo(owner, Action);
    }

    public bool StartRepoSettingsDialog(IWin32Window? owner) => throw NotPorted(nameof(StartRepoSettingsDialog));
    public bool StartResetChangesDialog(IWin32Window? owner, IReadOnlyCollection<GitItemStatus> workTreeFiles, bool onlyWorkTree)
    {
        // Show a form asking the user if they want to reset the changes.
        FormResetChanges.ActionEnum resetType = FormResetChanges.ShowResetDialog(owner, hasExistingFiles: workTreeFiles.Any(item => !item.IsNew), hasNewFiles: workTreeFiles.Any(item => item.IsNew));

        if (resetType == FormResetChanges.ActionEnum.Cancel)
        {
            return false;
        }

        return DoActionOnRepo(owner, Action);

        bool Action()
        {
            return Module.ResetAllChanges(clean: resetType == FormResetChanges.ActionEnum.ResetAndDelete, onlyWorkTree);
        }
    }

    public bool StartResetCurrentBranchDialog(IWin32Window? owner, string branch) => throw NotPorted(nameof(StartResetCurrentBranchDialog));
    public bool StartResolveConflictsDialog(IWin32Window? owner = null, bool offerCommit = true)
    {
        bool Action()
        {
            using CommandsDialogs.FormResolveConflicts form = new(this, offerCommit);
            form.ShowDialog(owner);
            return true;
        }

        return DoActionOnRepo(owner, Action);
    }

    public bool StartRevertCommitDialog(IWin32Window? owner, GitRevision revision) => throw NotPorted(nameof(StartRevertCommitDialog));
    public bool StartSettingsDialog(IGitPlugin gitPlugin) => throw NotPorted(nameof(StartSettingsDialog));
    public bool StartSettingsDialog(IWin32Window? owner, SettingsPageReference? initialPage = null) => throw NotPorted(nameof(StartSettingsDialog));
    public bool StartSettingsDialog(Type pageType) => throw NotPorted(nameof(StartSettingsDialog));
    public bool StartSparseWorkingCopyDialog(IWin32Window? owner) => throw NotPorted(nameof(StartSparseWorkingCopyDialog));
    public bool StartSquashCommitDialog(IWin32Window? owner, GitRevision revision) => throw NotPorted(nameof(StartSquashCommitDialog));
    public bool StartStashDialog(IWin32Window? owner = null, bool manageStashes = true, string? initialStash = null)
    {
        bool Action()
        {
            using FormStash form = new(this, initialStash) { ManageStashes = manageStashes };
            form.ShowDialog(owner);
            return true;
        }

        return DoActionOnRepo(owner, Action, changesRepo: false);
    }

    public bool StartSubmodulesDialog(IWin32Window? owner) => throw NotPorted(nameof(StartSubmodulesDialog));
    public bool StartSyncSubmodulesDialog(IWin32Window? owner) => throw NotPorted(nameof(StartSyncSubmodulesDialog));
    public bool StartTheContinueRebaseDialog(IWin32Window? owner)
    {
        return StartRebaseDialog(
            owner,
            from: string.Empty,
            to: null,
            onto: null,
            interactive: false,
            startRebaseImmediately: false);
    }

    public bool StartUpdateSubmoduleDialog(IWin32Window? owner, string submoduleLocalPath, string submoduleParentPath) => throw NotPorted(nameof(StartUpdateSubmoduleDialog));
    public bool StartUpdateSubmodulesDialog(IWin32Window? owner, string submoduleLocalPath = "")
    {
        bool Action()
        {
            return FormProcess.ShowDialog(owner, this, arguments: Commands.SubmoduleUpdate(submoduleLocalPath), Module.WorkingDir, input: null, useDialogSettings: true);
        }

        return DoActionOnRepo(owner, Action, postEvent: PostUpdateSubmodules);
    }

    public bool StartVerifyDatabaseDialog(IWin32Window? owner = null) => throw NotPorted(nameof(StartVerifyDatabaseDialog));
    public bool StartViewPatchDialog(IWin32Window? owner, string? patchFile = null)
    {
        bool Action()
        {
            using FormViewPatch viewPatch = new(this);
            if (!string.IsNullOrEmpty(patchFile))
            {
                viewPatch.LoadPatch(patchFile);
            }

            viewPatch.ShowDialog(owner);
            return true;
        }

        return DoActionOnRepo(owner, Action, requiresValidWorkingDir: false, changesRepo: false);
    }

    public bool StartViewPatchDialog(string patchFile)
    {
        return StartViewPatchDialog(null, patchFile);
    }

    public void UpdateSubmodules(IWin32Window? owner)
    {
        if (!Module.HasSubmodules())
        {
            return;
        }

        bool updateSubmodules = AppSettings.UpdateSubmodulesOnCheckout ?? (AppSettings.DontConfirmUpdateSubmodulesOnCheckout ?? MessageBoxes.ConfirmUpdateSubmodules(owner));

        if (updateSubmodules)
        {
            StartUpdateSubmodulesDialog(owner);
        }
    }

    public bool WorktreeCreate(IWin32Window? owner, string mainWorktreePath) => throw NotPorted(nameof(WorktreeCreate));
    public bool WorktreeDelete(IWin32Window? owner, string worktreePath) => throw NotPorted(nameof(WorktreeDelete));
    public bool WorktreeSwitch(IWin32Window? owner, string worktreePath) => throw NotPorted(nameof(WorktreeSwitch));

    #endregion
}
