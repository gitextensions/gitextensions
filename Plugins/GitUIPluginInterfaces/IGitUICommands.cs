using System;
using System.Drawing;

namespace GitUIPluginInterfaces
{
    [Serializable]
    public delegate void GitUIEventHandler(object sender, GitUIBaseEventArgs e);
    [Serializable]
    public delegate void GitUIPostActionEventHandler(object sender, GitUIPostActionEventArgs e);

    public interface IGitUICommands
    {
        event GitUIPostActionEventHandler PostAddFiles;
        event GitUIPostActionEventHandler PostApplyPatch;
        event GitUIPostActionEventHandler PostArchive;
        event GitUIPostActionEventHandler PostBlame;
        event GitUIEventHandler PostBrowse;
        event GitUIPostActionEventHandler PostCheckoutBranch;
        event GitUIPostActionEventHandler PostCheckoutRevision;
        event GitUIPostActionEventHandler PostCherryPick;
        event GitUIPostActionEventHandler PostClone;
        event GitUIPostActionEventHandler PostCommit;
        event GitUIPostActionEventHandler PostCompareRevisions;
        event GitUIPostActionEventHandler PostCreateBranch;
        event GitUIPostActionEventHandler PostCreateTag;
        event GitUIPostActionEventHandler PostDeleteBranch;
        event GitUIPostActionEventHandler PostDeleteTag;
        event GitUIPostActionEventHandler PostEditGitAttributes;
        event GitUIPostActionEventHandler PostEditGitIgnore;
        event GitUIPostActionEventHandler PostFileHistory;
        event GitUIPostActionEventHandler PostFormatPatch;
        event GitUIPostActionEventHandler PostInitialize;
        event GitUIPostActionEventHandler PostMailMap;
        event GitUIPostActionEventHandler PostMergeBranch;
        event GitUIPostActionEventHandler PostPull;
        event GitUIPostActionEventHandler PostPush;
        event GitUIPostActionEventHandler PostRebase;
        event GitUIPostActionEventHandler PostRename;
        event GitUIPostActionEventHandler PostRemotes;
        event GitUIEventHandler PostRepositoryChanged;
        event GitUIPostActionEventHandler PostResolveConflicts;
        event GitUIPostActionEventHandler PostRevertCommit;
        event GitUIPostActionEventHandler PostSettings;
        event GitUIPostActionEventHandler PostStash;
        event GitUIPostActionEventHandler PostSvnClone;
        event GitUIPostActionEventHandler PostSvnDcommit;
        event GitUIPostActionEventHandler PostSvnFetch;
        event GitUIPostActionEventHandler PostSvnRebase;
        event GitUIPostActionEventHandler PostSubmodulesEdit;
        event GitUIPostActionEventHandler PostSyncSubmodules;
        event GitUIPostActionEventHandler PostUpdateSubmodules;
        event GitUIPostActionEventHandler PostVerifyDatabase;
        event GitUIPostActionEventHandler PostViewPatch;
        event GitUIEventHandler PostBrowseInitialize;
        event GitUIEventHandler PostRegisterPlugin;
        event GitUIEventHandler PreAddFiles;
        event GitUIEventHandler PreApplyPatch;
        event GitUIEventHandler PreArchive;
        event GitUIEventHandler PreBlame;
        event GitUIEventHandler PreBrowse;
        event GitUIEventHandler PreCheckoutBranch;
        event GitUIEventHandler PreCheckoutRevision;
        event GitUIEventHandler PreCherryPick;
        event GitUIEventHandler PreClone;
        event GitUIEventHandler PreCommit;
        event GitUIEventHandler PreCompareRevisions;
        event GitUIEventHandler PreCreateBranch;
        event GitUIEventHandler PreCreateTag;
        event GitUIEventHandler PreDeleteBranch;
        event GitUIEventHandler PreDeleteTag;
        event GitUIEventHandler PreEditGitAttributes;
        event GitUIEventHandler PreEditGitIgnore;
        event GitUIEventHandler PreFileHistory;
        event GitUIEventHandler PreFormatPatch;
        event GitUIEventHandler PreInitialize;
        event GitUIEventHandler PreMailMap;
        event GitUIEventHandler PreMergeBranch;
        event GitUIEventHandler PrePull;
        event GitUIEventHandler PrePush;
        event GitUIEventHandler PreRebase;
        event GitUIEventHandler PreRename;
        event GitUIEventHandler PreRemotes;
        event GitUIEventHandler PreResolveConflicts;
        event GitUIEventHandler PreRevertCommit;
        event GitUIEventHandler PreSettings;
        event GitUIEventHandler PreStash;
        event GitUIEventHandler PreSvnClone;
        event GitUIEventHandler PreSvnDcommit;
        event GitUIEventHandler PreSvnFetch;
        event GitUIEventHandler PreSvnRebase;
        event GitUIEventHandler PreSubmodulesEdit;
        event GitUIEventHandler PreSyncSubmodules;
        event GitUIEventHandler PreUpdateSubmodules;
        event GitUIEventHandler PreVerifyDatabase;
        event GitUIEventHandler PreViewPatch;
        event GitUIEventHandler PreBrowseInitialize;
        
        IGitModule GitModule { get; }
        string GitCommand(string arguments);
        string CommandLineCommand(string cmd, string arguments);
        IGitRemoteCommand CreateRemoteCommand();
        void CacheAvatar(string email);
        Icon FormIcon { get; }
        IBrowseRepo BrowseRepo { get; }
        /// <summary>
        /// RepoChangedNotifier.Notify() should be called after each action that changess repo state
        /// </summary>
        ILockableNotifier RepoChangedNotifier { get; }

        bool StartCommandLineProcessDialog(object ownerForm, string command, string arguments);
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
        bool StartEditGitIgnoreDialog();
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
        bool StartRebaseDialog(string branch);
        bool StartRemotesDialog();
        bool StartResolveConflictsDialog();
        bool StartSettingsDialog();
        bool StartSettingsDialog(IGitPlugin gitPlugin);
        bool StartStashDialog();
        bool StartSvnCloneDialog();
        bool StartSvnDcommitDialog();
        bool StartSvnFetchDialog();
        bool StartSvnRebaseDialog();
        bool StartSubmodulesDialog();
        bool StartSyncSubmodulesDialog();
        bool StartUpdateSubmodulesDialog();
        bool StartVerifyDatabaseDialog();
        bool StartViewPatchDialog();
    }
}