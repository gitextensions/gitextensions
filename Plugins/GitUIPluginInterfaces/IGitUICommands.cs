using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GitUIPluginInterfaces
{
    public interface IGitUICommands
    {
        event EventHandler<GitUIPostActionEventArgs> PostAddFiles;
        event EventHandler<GitUIPostActionEventArgs> PostApplyPatch;
        event EventHandler<GitUIPostActionEventArgs> PostArchive;
        event EventHandler<GitUIPostActionEventArgs> PostBlame;
        event EventHandler<GitUIBaseEventArgs> PostBrowse;
        event EventHandler<GitUIPostActionEventArgs> PostCheckoutBranch;
        event EventHandler<GitUIPostActionEventArgs> PostCheckoutRevision;
        event EventHandler<GitUIPostActionEventArgs> PostCherryPick;
        event EventHandler<GitUIPostActionEventArgs> PostClone;
        event EventHandler<GitUIPostActionEventArgs> PostCommit;
        event EventHandler<GitUIPostActionEventArgs> PostCompareRevisions;
        event EventHandler<GitUIPostActionEventArgs> PostCreateBranch;
        event EventHandler<GitUIPostActionEventArgs> PostCreateTag;
        event EventHandler<GitUIPostActionEventArgs> PostDeleteBranch;
        event EventHandler<GitUIPostActionEventArgs> PostDeleteTag;
        event EventHandler<GitUIPostActionEventArgs> PostEditGitAttributes;
        event EventHandler<GitUIPostActionEventArgs> PostEditGitIgnore;
        event EventHandler<GitUIPostActionEventArgs> PostFileHistory;
        event EventHandler<GitUIPostActionEventArgs> PostFormatPatch;
        event EventHandler<GitUIPostActionEventArgs> PostInitialize;
        event EventHandler<GitUIPostActionEventArgs> PostMailMap;
        event EventHandler<GitUIPostActionEventArgs> PostMergeBranch;
        event EventHandler<GitUIPostActionEventArgs> PostPull;
        event EventHandler<GitUIPostActionEventArgs> PostPush;
        event EventHandler<GitUIPostActionEventArgs> PostRebase;
        event EventHandler<GitUIPostActionEventArgs> PostRename;
        event EventHandler<GitUIPostActionEventArgs> PostRemotes;
        event EventHandler<GitUIBaseEventArgs> PostRepositoryChanged;
        event EventHandler<GitUIPostActionEventArgs> PostResolveConflicts;
        event EventHandler<GitUIPostActionEventArgs> PostRevertCommit;
        event EventHandler<GitUIPostActionEventArgs> PostSettings;
        event EventHandler<GitUIPostActionEventArgs> PostStash;
        event EventHandler<GitUIPostActionEventArgs> PostSubmodulesEdit;
        event EventHandler<GitUIPostActionEventArgs> PostSyncSubmodules;
        event EventHandler<GitUIPostActionEventArgs> PostUpdateSubmodules;
        event EventHandler<GitUIPostActionEventArgs> PostVerifyDatabase;
        event EventHandler<GitUIPostActionEventArgs> PostViewPatch;
        event EventHandler<GitUIPostActionEventArgs> PostSparseWorkingCopy;
        event EventHandler<GitUIBaseEventArgs> PostBrowseInitialize;
        event EventHandler<GitUIBaseEventArgs> PostRegisterPlugin;
        event EventHandler<GitUIBaseEventArgs> PreAddFiles;
        event EventHandler<GitUIBaseEventArgs> PreApplyPatch;
        event EventHandler<GitUIBaseEventArgs> PreArchive;
        event EventHandler<GitUIBaseEventArgs> PreBlame;
        event EventHandler<GitUIBaseEventArgs> PreBrowse;
        event EventHandler<GitUIBaseEventArgs> PreCheckoutBranch;
        event EventHandler<GitUIBaseEventArgs> PreCheckoutRevision;
        event EventHandler<GitUIBaseEventArgs> PreCherryPick;
        event EventHandler<GitUIBaseEventArgs> PreClone;
        event EventHandler<GitUIBaseEventArgs> PreCommit;
        event EventHandler<GitUIBaseEventArgs> PreCompareRevisions;
        event EventHandler<GitUIBaseEventArgs> PreCreateBranch;
        event EventHandler<GitUIBaseEventArgs> PreCreateTag;
        event EventHandler<GitUIBaseEventArgs> PreDeleteBranch;
        event EventHandler<GitUIBaseEventArgs> PreDeleteTag;
        event EventHandler<GitUIBaseEventArgs> PreEditGitAttributes;
        event EventHandler<GitUIBaseEventArgs> PreEditGitIgnore;
        event EventHandler<GitUIBaseEventArgs> PreFileHistory;
        event EventHandler<GitUIBaseEventArgs> PreFormatPatch;
        event EventHandler<GitUIBaseEventArgs> PreInitialize;
        event EventHandler<GitUIBaseEventArgs> PreMailMap;
        event EventHandler<GitUIBaseEventArgs> PreMergeBranch;
        event EventHandler<GitUIBaseEventArgs> PrePull;
        event EventHandler<GitUIBaseEventArgs> PrePush;
        event EventHandler<GitUIBaseEventArgs> PreRebase;
        event EventHandler<GitUIBaseEventArgs> PreRename;
        event EventHandler<GitUIBaseEventArgs> PreRemotes;
        event EventHandler<GitUIBaseEventArgs> PreResolveConflicts;
        event EventHandler<GitUIBaseEventArgs> PreRevertCommit;
        event EventHandler<GitUIBaseEventArgs> PreSettings;
        event EventHandler<GitUIBaseEventArgs> PreStash;
        event EventHandler<GitUIBaseEventArgs> PreSubmodulesEdit;
        event EventHandler<GitUIBaseEventArgs> PreSyncSubmodules;
        event EventHandler<GitUIBaseEventArgs> PreUpdateSubmodules;
        event EventHandler<GitUIBaseEventArgs> PreVerifyDatabase;
        event EventHandler<GitUIBaseEventArgs> PreViewPatch;
        event EventHandler<GitUIBaseEventArgs> PreBrowseInitialize;
        event EventHandler<GitUIBaseEventArgs> PreSparseWorkingCopy;

        IGitModule GitModule { get; }
        string GitCommand(string arguments);
        Task<string> CommandLineCommandAsync(string cmd, string arguments);
        IGitRemoteCommand CreateRemoteCommand();
        void CacheAvatar(string email);
        Icon FormIcon { get; }
        IBrowseRepo BrowseRepo { get; }

        /// <summary>
        /// RepoChangedNotifier.Notify() should be called after each action that changess repo state
        /// </summary>
        ILockableNotifier RepoChangedNotifier { get; }

        bool StartCommandLineProcessDialog(object ownerForm, string command, string arguments);
        bool StartCommandLineProcessDialog(IGitCommand cmd, IWin32Window parentForm);
        bool StartCommandLineProcessDialog(string command, string arguments);
        bool StartBatchFileProcessDialog(object ownerForm, string batchFile);
        bool StartBatchFileProcessDialog(string batchFile);

        bool StartAddFilesDialog();
        bool StartApplyPatchDialog();
        bool StartArchiveDialog();
        bool StartBrowseDialog();
        bool StartCheckoutBranch();
        bool StartCheckoutRevisionDialog();
        bool StartCherryPickDialog();
        bool StartCloneDialog();
        bool StartCloneDialog(string url);
        bool StartCommitDialog();
        bool StartCompareRevisionsDialog();
        bool StartCreateBranchDialog();
        bool StartCreateTagDialog();
        bool StartDeleteBranchDialog(string branch);
        bool StartDeleteTagDialog();
        bool StartEditGitIgnoreDialog(bool localExcludes);
        void StartFileHistoryDialog(string fileName);
        bool StartFormatPatchDialog();
        bool StartGitCommandProcessDialog(string arguments);
        bool StartInitializeDialog();
        bool StartInitializeDialog(string dir);
        bool StartMailMapDialog();
        bool StartMergeBranchDialog(string branch);
        bool StartPluginSettingsDialog();
        bool StartPullDialog();
        bool StartPushDialog();
        bool StartRebaseDialog(IWin32Window owner, string onto);
        bool StartRemotesDialog();
        bool StartResolveConflictsDialog();
        bool StartSettingsDialog();
        bool StartSettingsDialog(IGitPlugin gitPlugin);
        bool StartStashDialog();
        bool StartSubmodulesDialog();
        bool StartSyncSubmodulesDialog();
        bool StartUpdateSubmodulesDialog();
        bool StartVerifyDatabaseDialog();
        bool StartViewPatchDialog();
        bool StartSparseWorkingCopyDialog();
        void AddCommitTemplate(string key, Func<string> addingText);
        void RemoveCommitTemplate(string key);
    }
}