using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Shims.WinForms;
using GitExtUtils;
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
        // TODO(avalonia-port): use FormRemoteProcess for remote commands once it is ported.
        bool success = FormProcess.ShowDialog(owner, this, arguments: command.Arguments, Module.WorkingDir, input: null, useDialogSettings: true);

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

    private void InvokeEvent(IWin32Window? ownerForm, EventHandler<GitUIEventArgs>? gitUIEventHandler)
    {
        try
        {
            gitUIEventHandler?.Invoke(this, new GitUIEventArgs(ownerForm, this));
        }
        catch (Exception ex)
        {
            MessageBoxes.ShowError(ownerForm, $"{ex.Message}{Environment.NewLine}{ex.StackTrace}", "Error");
        }
    }

    private static NotImplementedException NotPorted(string member)
        => new($"{member} is not ported to the Avalonia UI yet.");

    #region Not ported yet

    public void AddCommitTemplate(string key, Func<string> addingText, Image? icon, bool isRegex = false) => throw NotPorted(nameof(AddCommitTemplate));
    public void AddUpstreamRemote(IWin32Window? owner, IRepositoryHostPlugin gitHoster) => throw NotPorted(nameof(AddUpstreamRemote));
    public IGitRemoteCommand CreateRemoteCommand() => throw NotPorted(nameof(CreateRemoteCommand));
    public bool DoActionOnRepo(Func<bool> action) => throw NotPorted(nameof(DoActionOnRepo));
    public void OpenWithDifftool(IWin32Window? owner, IReadOnlyList<GitRevision?> revisions, string fileName, string? oldFileName, RevisionDiffKind diffKind, bool isTracked, string? customTool = null) => throw NotPorted(nameof(OpenWithDifftool));
    public void RaisePostBrowseInitialize(IWin32Window? owner) => InvokeEvent(owner, PostBrowseInitialize);
    public void RaisePostRegisterPlugin(IWin32Window? owner) => InvokeEvent(owner, PostRegisterPlugin);
    public void RemoveCommitTemplate(string key) => throw NotPorted(nameof(RemoveCommitTemplate));
    public bool RunCommand(IReadOnlyList<string> args) => throw NotPorted(nameof(RunCommand));
    public void ShowModelessForm(IWin32Window? owner, bool requiresValidWorkingDir, EventHandler<GitUIEventArgs>? preEvent, EventHandler<GitUIPostActionEventArgs>? postEvent, Func<Form> provideForm) => throw NotPorted(nameof(ShowModelessForm));
    public bool StartAddFilesDialog(IWin32Window? owner, string? addFiles = null) => throw NotPorted(nameof(StartAddFilesDialog));
    public bool StartAddToGitIgnoreDialog(IWin32Window? owner, bool localExclude, params string[] filePattern) => throw NotPorted(nameof(StartAddToGitIgnoreDialog));
    public bool StartAmendCommitDialog(IWin32Window? owner, GitRevision revision) => throw NotPorted(nameof(StartAmendCommitDialog));
    public bool StartApplyPatchDialog(IWin32Window? owner, string? patchFile = null) => throw NotPorted(nameof(StartApplyPatchDialog));
    public bool StartArchiveDialog(IWin32Window? owner = null, GitRevision? revision = null, GitRevision? revision2 = null, string? path = null) => throw NotPorted(nameof(StartArchiveDialog));
    public void StartBatchFileProcessDialog(string batchFile) => throw NotPorted(nameof(StartBatchFileProcessDialog));
    public bool StartBrowseDialog(IWin32Window? owner, BrowseArguments? args = null) => throw NotPorted(nameof(StartBrowseDialog));
    public bool StartCheckoutBranch(IWin32Window? owner, IReadOnlyList<ObjectId>? containObjectIds) => throw NotPorted(nameof(StartCheckoutBranch));
    public bool StartCheckoutBranch(IWin32Window? owner, string branch = "", bool remote = false, IReadOnlyList<ObjectId>? containObjectIds = null) => throw NotPorted(nameof(StartCheckoutBranch));
    public bool StartCheckoutRemoteBranch(IWin32Window? owner, string branch) => throw NotPorted(nameof(StartCheckoutRemoteBranch));
    public bool StartCheckoutRevisionDialog(IWin32Window? owner, string? revision = null) => throw NotPorted(nameof(StartCheckoutRevisionDialog));
    public bool StartCherryPickDialog(IWin32Window? owner = null, GitRevision? revision = null) => throw NotPorted(nameof(StartCherryPickDialog));
    public bool StartCherryPickDialog(IWin32Window? owner, IEnumerable<GitRevision> revisions) => throw NotPorted(nameof(StartCherryPickDialog));
    public bool StartCleanupRepositoryDialog(IWin32Window? owner = null, string? path = null) => throw NotPorted(nameof(StartCleanupRepositoryDialog));
    public bool StartCloneDialog(IWin32Window? owner, string url, EventHandler<GitModuleEventArgs> gitModuleChanged) => throw NotPorted(nameof(StartCloneDialog));
    public bool StartCloneDialog(IWin32Window? owner, string? url = null, bool openedFromProtocolHandler = false, EventHandler<GitModuleEventArgs>? gitModuleChanged = null) => throw NotPorted(nameof(StartCloneDialog));
    public void StartCloneForkFromHoster(IWin32Window? owner, IRepositoryHostPlugin gitHoster, EventHandler<GitModuleEventArgs>? gitModuleChanged) => throw NotPorted(nameof(StartCloneForkFromHoster));
    public bool StartCommitDialog(IWin32Window? owner, string? commitMessage = null, bool showOnlyWhenChanges = false) => throw NotPorted(nameof(StartCommitDialog));
    public bool StartCompareRevisionsDialog(IWin32Window? owner = null) => throw NotPorted(nameof(StartCompareRevisionsDialog));
    public bool StartCreateBranchDialog(IWin32Window? owner = null, ObjectId objectId = default, string? newBranchNamePrefix = null) => throw NotPorted(nameof(StartCreateBranchDialog));
    public bool StartCreateBranchDialog(IWin32Window? owner, string? branch) => throw NotPorted(nameof(StartCreateBranchDialog));
    public void StartCreatePullRequest(IWin32Window? owner) => throw NotPorted(nameof(StartCreatePullRequest));
    public void StartCreatePullRequest(IWin32Window? owner, IRepositoryHostPlugin gitHoster, string? chooseRemote = null, string? chooseBranch = null) => throw NotPorted(nameof(StartCreatePullRequest));
    public bool StartCreateTagDialog(IWin32Window? owner = null, GitRevision? revision = null) => throw NotPorted(nameof(StartCreateTagDialog));
    public bool StartDeleteBranchDialog(IWin32Window? owner, IEnumerable<string> branches) => throw NotPorted(nameof(StartDeleteBranchDialog));
    public bool StartDeleteBranchDialog(IWin32Window? owner, string branch) => throw NotPorted(nameof(StartDeleteBranchDialog));
    public bool StartDeleteRemoteBranchDialog(IWin32Window? owner, string remoteBranch) => throw NotPorted(nameof(StartDeleteRemoteBranchDialog));
    public bool StartDeleteTagDialog(IWin32Window? owner, string? tag) => throw NotPorted(nameof(StartDeleteTagDialog));
    public bool StartEditGitAttributesDialog(IWin32Window? owner = null) => throw NotPorted(nameof(StartEditGitAttributesDialog));
    public bool StartEditGitIgnoreDialog(IWin32Window? owner, bool localExcludes) => throw NotPorted(nameof(StartEditGitIgnoreDialog));
    public bool StartFileEditorDialog(string? filename, bool showWarning = false, int? lineNumber = null) => throw NotPorted(nameof(StartFileEditorDialog));
    public void StartFileHistoryDialog(IWin32Window? owner, string fileName, GitRevision? revision = null, bool filterByRevision = false, bool showBlame = false) => throw NotPorted(nameof(StartFileHistoryDialog));
    public bool StartFixupCommitDialog(IWin32Window? owner, GitRevision revision) => throw NotPorted(nameof(StartFixupCommitDialog));
    public bool StartFormCommitDiff(ObjectId objectId) => throw NotPorted(nameof(StartFormCommitDiff));
    public bool StartFormatPatchDialog(IWin32Window? owner = null) => throw NotPorted(nameof(StartFormatPatchDialog));
    public bool StartGeneralSettingsDialog(IWin32Window? owner) => throw NotPorted(nameof(StartGeneralSettingsDialog));
    public bool StartInitializeDialog(IWin32Window? owner = null, string? dir = null, EventHandler<GitModuleEventArgs>? gitModuleChanged = null) => throw NotPorted(nameof(StartInitializeDialog));
    public bool StartInteractiveRebase(IWin32Window? owner, string onto) => throw NotPorted(nameof(StartInteractiveRebase));
    public bool StartMailMapDialog(IWin32Window? owner = null) => throw NotPorted(nameof(StartMailMapDialog));
    public bool StartMergeBranchDialog(IWin32Window? owner, string? branch) => throw NotPorted(nameof(StartMergeBranchDialog));
    public bool StartPluginSettingsDialog(IWin32Window? owner) => throw NotPorted(nameof(StartPluginSettingsDialog));
    public bool StartPullDialog(IWin32Window? owner = null, string? remoteBranch = null, string? remote = null, GitPullAction pullAction = GitPullAction.None) => throw NotPorted(nameof(StartPullDialog));
    public bool StartPullDialogAndPullImmediately(IWin32Window? owner = null, string? remoteBranch = null, string? remote = null, GitPullAction pullAction = GitPullAction.None) => throw NotPorted(nameof(StartPullDialogAndPullImmediately));
    public bool StartPullDialogAndPullImmediately(out bool pullCompleted, IWin32Window? owner = null, string? remoteBranch = null, string? remote = null, GitPullAction pullAction = GitPullAction.None) => throw NotPorted(nameof(StartPullDialogAndPullImmediately));
    public void StartPullRequestsDialog(IWin32Window? owner, IRepositoryHostPlugin gitHoster) => throw NotPorted(nameof(StartPullRequestsDialog));
    public bool StartPushDialog(IWin32Window? owner, bool pushOnShow) => throw NotPorted(nameof(StartPushDialog));
    public bool StartPushDialog(IWin32Window? owner, bool pushOnShow, bool forceWithLease, out bool pushCompleted, string? branchName = null) => throw NotPorted(nameof(StartPushDialog));
    public bool StartRebase(IWin32Window? owner, string onto) => throw NotPorted(nameof(StartRebase));
    public bool StartRebaseDialog(IWin32Window? owner, string? from, string? to, string? onto, bool interactive = false, bool startRebaseImmediately = true) => throw NotPorted(nameof(StartRebaseDialog));
    public bool StartRebaseDialog(IWin32Window? owner, string? onto) => throw NotPorted(nameof(StartRebaseDialog));
    public bool StartRebaseDialogWithAdvOptions(IWin32Window? owner, string onto, string from = "") => throw NotPorted(nameof(StartRebaseDialogWithAdvOptions));
    public bool StartRemotesDialog(IWin32Window? owner, string? preselectRemote = null, string? preselectLocal = null) => throw NotPorted(nameof(StartRemotesDialog));
    public bool StartRenameDialog(IWin32Window? owner, string branch) => throw NotPorted(nameof(StartRenameDialog));
    public bool StartRepoSettingsDialog(IWin32Window? owner) => throw NotPorted(nameof(StartRepoSettingsDialog));
    public bool StartResetChangesDialog(IWin32Window? owner, IReadOnlyCollection<GitItemStatus> workTreeFiles, bool onlyWorkTree) => throw NotPorted(nameof(StartResetChangesDialog));
    public bool StartResetCurrentBranchDialog(IWin32Window? owner, string branch) => throw NotPorted(nameof(StartResetCurrentBranchDialog));
    public bool StartResolveConflictsDialog(IWin32Window? owner = null, bool offerCommit = true) => throw NotPorted(nameof(StartResolveConflictsDialog));
    public bool StartRevertCommitDialog(IWin32Window? owner, GitRevision revision) => throw NotPorted(nameof(StartRevertCommitDialog));
    public bool StartSettingsDialog(IGitPlugin gitPlugin) => throw NotPorted(nameof(StartSettingsDialog));
    public bool StartSettingsDialog(IWin32Window? owner, SettingsPageReference? initialPage = null) => throw NotPorted(nameof(StartSettingsDialog));
    public bool StartSettingsDialog(Type pageType) => throw NotPorted(nameof(StartSettingsDialog));
    public bool StartSparseWorkingCopyDialog(IWin32Window? owner) => throw NotPorted(nameof(StartSparseWorkingCopyDialog));
    public bool StartSquashCommitDialog(IWin32Window? owner, GitRevision revision) => throw NotPorted(nameof(StartSquashCommitDialog));
    public bool StartStashDialog(IWin32Window? owner = null, bool manageStashes = true, string? initialStash = null) => throw NotPorted(nameof(StartStashDialog));
    public bool StartSubmodulesDialog(IWin32Window? owner) => throw NotPorted(nameof(StartSubmodulesDialog));
    public bool StartSyncSubmodulesDialog(IWin32Window? owner) => throw NotPorted(nameof(StartSyncSubmodulesDialog));
    public bool StartTheContinueRebaseDialog(IWin32Window? owner) => throw NotPorted(nameof(StartTheContinueRebaseDialog));
    public bool StartUpdateSubmoduleDialog(IWin32Window? owner, string submoduleLocalPath, string submoduleParentPath) => throw NotPorted(nameof(StartUpdateSubmoduleDialog));
    public bool StartUpdateSubmodulesDialog(IWin32Window? owner, string submoduleLocalPath = "") => throw NotPorted(nameof(StartUpdateSubmodulesDialog));
    public bool StartVerifyDatabaseDialog(IWin32Window? owner = null) => throw NotPorted(nameof(StartVerifyDatabaseDialog));
    public bool StartViewPatchDialog(IWin32Window? owner, string? patchFile = null) => throw NotPorted(nameof(StartViewPatchDialog));
    public bool StartViewPatchDialog(string patchFile) => throw NotPorted(nameof(StartViewPatchDialog));
    public bool StashApply(IWin32Window? owner, string stashName) => throw NotPorted(nameof(StashApply));
    public bool StashDrop(IWin32Window? owner, string stashName) => throw NotPorted(nameof(StashDrop));
    public bool StashPop(IWin32Window? owner, string stashName = "") => throw NotPorted(nameof(StashPop));
    public bool StashSave(IWin32Window? owner, bool includeUntrackedFiles, bool keepIndex = false, string message = "", IReadOnlyList<string>? selectedFiles = null) => throw NotPorted(nameof(StashSave));
    public bool StashStaged(IWin32Window? owner) => throw NotPorted(nameof(StashStaged));
    public void UpdateSubmodules(IWin32Window? owner) => throw NotPorted(nameof(UpdateSubmodules));
    public bool WorktreeCreate(IWin32Window? owner, string mainWorktreePath) => throw NotPorted(nameof(WorktreeCreate));
    public bool WorktreeDelete(IWin32Window? owner, string worktreePath) => throw NotPorted(nameof(WorktreeDelete));
    public bool WorktreeSwitch(IWin32Window? owner, string worktreePath) => throw NotPorted(nameof(WorktreeSwitch));

    #endregion
}
