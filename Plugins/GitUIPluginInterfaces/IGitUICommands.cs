﻿using System;

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
        event GitUIPostActionEventHandler PostBrowse;
        event GitUIPostActionEventHandler PostCheckoutBranch;
        event GitUIPostActionEventHandler PostCheckoutRevision;
        event GitUIEventHandler PostCherryPick;
        event GitUIEventHandler PostClone;
        event GitUIEventHandler PostCommit;
        event GitUIEventHandler PostCompareRevisions;
        event GitUIEventHandler PostCreateBranch;
        event GitUIEventHandler PostCreateTag;
        event GitUIEventHandler PostDeleteBranch;
        event GitUIEventHandler PostDeleteTag;
        event GitUIEventHandler PostEditGitAttributes;
        event GitUIEventHandler PostEditGitIgnore;
        event GitUIEventHandler PostFileHistory;
        event GitUIEventHandler PostFormatPatch;
        event GitUIEventHandler PostInitialize;
        event GitUIEventHandler PostMailMap;
        event GitUIEventHandler PostMergeBranch;
        event GitUIEventHandler PostPull;
        event GitUIEventHandler PostPush;
        event GitUIEventHandler PostRebase;
        event GitUIEventHandler PostRename;
        event GitUIEventHandler PostRemotes;
        event GitUIEventHandler PostResolveConflicts;
        event GitUIEventHandler PostSettings;
        event GitUIEventHandler PostStash;
        event GitUIEventHandler PostSvnClone;
        event GitUIEventHandler PostSvnDcommit;
        event GitUIEventHandler PostSvnFetch;
        event GitUIEventHandler PostSvnRebase;
        event GitUIEventHandler PostSubmodulesEdit;
        event GitUIEventHandler PostSyncSubmodules;
        event GitUIEventHandler PostUpdateSubmodules;
        event GitUIEventHandler PostVerifyDatabase;
        event GitUIEventHandler PostViewPatch;
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
        event GitUIEventHandler BrowseInitialize;
        
        IGitModule GitModule { get; }
        string GitCommand(string arguments);
        string CommandLineCommand(string cmd, string arguments);
        IGitRemoteCommand CreateRemoteCommand();
        void RaiseBrowseInitialize();
        void CacheAvatar(string email);

        bool StartCommandLineProcessDialog(object ownerForm, string command, string arguments);
        bool StartCommandLineProcessDialog(string command, string arguments);
        bool StartBatchFileProcessDialog(object ownerForm, string batchFile);
        bool StartBatchFileProcessDialog(string batchFile);

        bool StartAddFilesDialog();
        bool StartApplyPatchDialog();
        bool StartArchiveDialog();
        bool StartBrowseDialog();
        bool StartCheckoutBranchDialog();
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
        bool StartFileHistoryDialog(string fileName);
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