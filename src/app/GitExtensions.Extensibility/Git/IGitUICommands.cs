using System.Security.Cryptography;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Settings;
using GitUIPluginInterfaces;

namespace GitExtensions.Extensibility.Git;

public interface IGitUICommands : IServiceProvider
{
    event EventHandler<GitUIEventArgs>? PostBrowseInitialize;
    event EventHandler<GitUIPostActionEventArgs>? PostCheckoutBranch;
    event EventHandler<GitUIPostActionEventArgs>? PostCheckoutRevision;
    event EventHandler<GitUIPostActionEventArgs>? PostCommit;
    event EventHandler<GitUIPostActionEventArgs>? PostEditGitIgnore;
    event EventHandler<GitUIEventArgs>? PostRegisterPlugin;
    event EventHandler<GitUIEventArgs>? PostRepositoryChanged;
    event EventHandler<GitUIPostActionEventArgs>? PostSettings;
    event EventHandler<GitUIPostActionEventArgs>? PostUpdateSubmodules;
    event EventHandler<GitUIEventArgs>? PreCheckoutBranch;
    event EventHandler<GitUIEventArgs>? PreCheckoutRevision;
    event EventHandler<GitUIEventArgs>? PreCommit;

    IBrowseRepo? BrowseRepo { get; set; }

    IGitModule Module { get; }

    /// <summary>
    /// RepoChangedNotifier.Notify() should be called after each action that changes repo state
    /// </summary>
    ILockableNotifier RepoChangedNotifier { get; }

    void AddCommitTemplate(string key, Func<string> addingText, Image? icon);
    void AddUpstreamRemote(IWin32Window? owner, IRepositoryHostPlugin gitHoster);
    IGitRemoteCommand CreateRemoteCommand();
    bool DoActionOnRepo(Func<bool> action);
    void OpenWithDifftool(IWin32Window? owner, IReadOnlyList<GitRevision?> revisions, string fileName, string? oldFileName, RevisionDiffKind diffKind, bool isTracked, string? customTool = null);
    void RaisePostBrowseInitialize(IWin32Window? owner);
    void RaisePostRegisterPlugin(IWin32Window? owner);
    void RemoveCommitTemplate(string key);
    bool RunCommand(IReadOnlyList<string> args);
    void ShowModelessForm(IWin32Window? owner, bool requiresValidWorkingDir, EventHandler<GitUIEventArgs>? preEvent, EventHandler<GitUIPostActionEventArgs>? postEvent, Func<Form> provideForm);
    bool StartAddFilesDialog(IWin32Window? owner, string? addFiles = null);
    bool StartAddToGitIgnoreDialog(IWin32Window? owner, bool localExclude, params string[] filePattern);
    bool StartAmendCommitDialog(IWin32Window? owner, GitRevision revision);
    bool StartApplyPatchDialog(IWin32Window? owner, string? patchFile = null);
    bool StartArchiveDialog(IWin32Window? owner = null, GitRevision? revision = null, GitRevision? revision2 = null, string? path = null);
    void StartBatchFileProcessDialog(string batchFile);
    bool StartBrowseDialog(IWin32Window? owner, BrowseArguments? args = null);
    bool StartCheckoutBranch(IWin32Window? owner, IReadOnlyList<ObjectId>? containRevisions);
    bool StartCheckoutBranch(IWin32Window? owner, string branch = "", bool remote = false, IReadOnlyList<ObjectId>? containRevisions = null);
    bool StartCheckoutRemoteBranch(IWin32Window? owner, string branch);
    bool StartCheckoutRevisionDialog(IWin32Window? owner, string? revision = null);
    bool StartCherryPickDialog(IWin32Window? owner = null, GitRevision? revision = null);
    bool StartCherryPickDialog(IWin32Window? owner, IEnumerable<GitRevision> revisions);
    bool StartCleanupRepositoryDialog(IWin32Window? owner = null, string? path = null);
    bool StartCloneDialog(IWin32Window? owner, string url, EventHandler<GitModuleEventArgs> gitModuleChanged);
    bool StartCloneDialog(IWin32Window? owner, string? url = null, bool openedFromProtocolHandler = false, EventHandler<GitModuleEventArgs>? gitModuleChanged = null);
    void StartCloneForkFromHoster(IWin32Window? owner, IRepositoryHostPlugin gitHoster, EventHandler<GitModuleEventArgs>? gitModuleChanged);
    bool StartCommandLineProcessDialog(IWin32Window? owner, IGitCommand command);
    bool StartCommandLineProcessDialog(IWin32Window? owner, string? command, ArgumentString arguments);
    bool StartCommitDialog(IWin32Window? owner, string? commitMessage = null, bool showOnlyWhenChanges = false);
    bool StartCompareRevisionsDialog(IWin32Window? owner = null);
    bool StartCreateBranchDialog(IWin32Window? owner = null, ObjectId? objectId = null, string? newBranchNamePrefix = null);
    bool StartCreateBranchDialog(IWin32Window? owner, string? branch);
    void StartCreatePullRequest(IWin32Window? owner);
    void StartCreatePullRequest(IWin32Window? owner, IRepositoryHostPlugin gitHoster, string? chooseRemote = null, string? chooseBranch = null);
    bool StartCreateTagDialog(IWin32Window? owner = null, GitRevision? revision = null);
    bool StartDeleteBranchDialog(IWin32Window? owner, IEnumerable<string> branches);
    bool StartDeleteBranchDialog(IWin32Window? owner, string branch);
    bool StartDeleteRemoteBranchDialog(IWin32Window? owner, string remoteBranch);
    bool StartDeleteTagDialog(IWin32Window? owner, string? tag);
    bool StartEditGitAttributesDialog(IWin32Window? owner = null);
    bool StartEditGitIgnoreDialog(IWin32Window? owner, bool localExcludes);
    bool StartFileEditorDialog(string? filename, bool showWarning = false, int? lineNumber = null);
    void StartFileHistoryDialog(IWin32Window? owner, string fileName, GitRevision? revision = null, bool filterByRevision = false, bool showBlame = false);
    bool StartFixupCommitDialog(IWin32Window? owner, GitRevision revision);
    bool StartFormCommitDiff(ObjectId objectId);
    bool StartFormatPatchDialog(IWin32Window? owner = null);
    bool StartGeneralSettingsDialog(IWin32Window? owner);
    bool StartGitCommandProcessDialog(IWin32Window? owner, ArgumentString arguments);
    bool StartInitializeDialog(IWin32Window? owner = null, string? dir = null, EventHandler<GitModuleEventArgs>? gitModuleChanged = null);
    bool StartInteractiveRebase(IWin32Window? owner, string onto);
    bool StartMailMapDialog(IWin32Window? owner = null);
    bool StartMergeBranchDialog(IWin32Window? owner, string? branch);
    bool StartPluginSettingsDialog(IWin32Window? owner);
    bool StartPullDialog(IWin32Window? owner = null, string? remoteBranch = null, string? remote = null, GitPullAction pullAction = GitPullAction.None);
    bool StartPullDialogAndPullImmediately(IWin32Window? owner = null, string? remoteBranch = null, string? remote = null, GitPullAction pullAction = GitPullAction.None);
    bool StartPullDialogAndPullImmediately(out bool pullCompleted, IWin32Window? owner = null, string? remoteBranch = null, string? remote = null, GitPullAction pullAction = GitPullAction.None);
    void StartPullRequestsDialog(IWin32Window? owner, IRepositoryHostPlugin gitHoster);
    bool StartPushDialog(IWin32Window? owner, bool pushOnShow);
    bool StartPushDialog(IWin32Window? owner, bool pushOnShow, bool forceWithLease, out bool pushCompleted);
    bool StartRebase(IWin32Window? owner, string onto);
    bool StartRebaseDialog(IWin32Window? owner, string? from, string? to, string? onto, bool interactive = false, bool startRebaseImmediately = true);
    bool StartRebaseDialog(IWin32Window? owner, string? onto);
    bool StartRebaseDialogWithAdvOptions(IWin32Window? owner, string onto, string from = "");

    /// <summary>
    /// Opens the FormRemotes.
    /// </summary>
    /// <param name="preselectRemote">Makes the FormRemotes initially select the given remote.</param>
    /// <param name="preselectLocal">Makes the FormRemotes initially show the tab "Default push behavior" and select the given local.</param>
    bool StartRemotesDialog(IWin32Window? owner, string? preselectRemote = null, string? preselectLocal = null);
    bool StartRenameDialog(IWin32Window? owner, string branch);
    bool StartRepoSettingsDialog(IWin32Window? owner);
    bool StartResetChangesDialog(IWin32Window? owner, IReadOnlyCollection<GitItemStatus> workTreeFiles, bool onlyWorkTree);
    bool StartResetCurrentBranchDialog(IWin32Window? owner, string branch);
    bool StartResolveConflictsDialog(IWin32Window? owner = null, bool offerCommit = true);
    bool StartRevertCommitDialog(IWin32Window? owner, GitRevision revision);
    bool StartSettingsDialog(IGitPlugin gitPlugin);
    bool StartSettingsDialog(IWin32Window? owner, SettingsPageReference? initialPage = null);
    bool StartSettingsDialog(Type pageType);
    bool StartSparseWorkingCopyDialog(IWin32Window? owner);
    bool StartSquashCommitDialog(IWin32Window? owner, GitRevision revision);
    bool StartStashDialog(IWin32Window? owner = null, bool manageStashes = true, string initialStash = null);
    bool StartSubmodulesDialog(IWin32Window? owner);
    bool StartSyncSubmodulesDialog(IWin32Window? owner);
    bool StartTheContinueRebaseDialog(IWin32Window? owner);
    bool StartUpdateSubmoduleDialog(IWin32Window? owner, string submoduleLocalPath, string submoduleParentPath);
    bool StartUpdateSubmodulesDialog(IWin32Window? owner, string submoduleLocalPath = "");
    bool StartVerifyDatabaseDialog(IWin32Window? owner = null);
    bool StartViewPatchDialog(IWin32Window? owner, string? patchFile = null);
    bool StartViewPatchDialog(string patchFile);
    bool StashApply(IWin32Window? owner, string stashName);
    bool StashDrop(IWin32Window? owner, string stashName);
    bool StashPop(IWin32Window? owner, string stashName = "");
    bool StashSave(IWin32Window? owner, bool includeUntrackedFiles, bool keepIndex = false, string message = "", IReadOnlyList<string>? selectedFiles = null);
    bool StashStaged(IWin32Window? owner);
    void UpdateSubmodules(IWin32Window? owner);
    IGitUICommands WithGitModule(IGitModule module);
    IGitUICommands WithWorkingDirectory(string? workingDirectory);
}
