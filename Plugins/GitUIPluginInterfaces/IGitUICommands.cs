﻿using System;

namespace GitUIPluginInterfaces
{
    [Serializable]
    public delegate void GitUIEventHandler(object sender, GitUIBaseEventArgs e);

    public interface IGitUICommands
    {
        event GitUIEventHandler PostAddFiles;
        event GitUIEventHandler PostApplyPatch;
        event GitUIEventHandler PostArchive;
        event GitUIEventHandler PostBrowse;
        event GitUIEventHandler PostCheckoutBranch;
        event GitUIEventHandler PostCheckoutRevision;
        event GitUIEventHandler PostCherryPick;
        event GitUIEventHandler PostClone;
        event GitUIEventHandler PostCommit;
        event GitUIEventHandler PostCompareRevisions;
        event GitUIEventHandler PostCreateBranch;
        event GitUIEventHandler PostCreateTag;
        event GitUIEventHandler PostDeleteBranch;
        event GitUIEventHandler PostDeleteTag;
        event GitUIEventHandler PostEditGitIgnore;
        event GitUIEventHandler PostFileHistory;
        event GitUIEventHandler PostFormatPatch;
        event GitUIEventHandler PostInitialize;
        event GitUIEventHandler PostMailMap;
        event GitUIEventHandler PostMergeBranch;
        event GitUIEventHandler PostPull;
        event GitUIEventHandler PostPush;
        event GitUIEventHandler PostRebase;
        event GitUIEventHandler PostRemotes;
        event GitUIEventHandler PostResolveConflicts;
        event GitUIEventHandler PostSettings;
        event GitUIEventHandler PostStash;
        event GitUIEventHandler PostSubmodulesEdit;
        event GitUIEventHandler PostUpdateSubmodules;
        event GitUIEventHandler PostUpdateSubmodulesRecursive;
        event GitUIEventHandler PostVerifyDatabase;
        event GitUIEventHandler PostViewPatch;
        event GitUIEventHandler PreAddFiles;
        event GitUIEventHandler PreApplyPatch;
        event GitUIEventHandler PreArchive;
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
        event GitUIEventHandler PreEditGitIgnore;
        event GitUIEventHandler PreFileHistory;
        event GitUIEventHandler PreFormatPatch;
        event GitUIEventHandler PreInitialize;
        event GitUIEventHandler PreMailMap;
        event GitUIEventHandler PreMergeBranch;
        event GitUIEventHandler PrePull;
        event GitUIEventHandler PrePush;
        event GitUIEventHandler PreRebase;
        event GitUIEventHandler PreRemotes;
        event GitUIEventHandler PreResolveConflicts;
        event GitUIEventHandler PreSettings;
        event GitUIEventHandler PreStash;
        event GitUIEventHandler PreSubmodulesEdit;
        event GitUIEventHandler PreUpdateSubmodules;
        event GitUIEventHandler PreUpdateSubmodulesRecursive;
        event GitUIEventHandler PreVerifyDatabase;
        event GitUIEventHandler PreViewPatch;

        string GitCommand(string arguments);
        string CommandLineCommand(string cmd, string arguments);

        bool StartAddFilesDialog();
        bool StartApplyPatchDialog();
        bool StartArchiveDialog();
        bool StartBrowseDialog();
        bool StartCheckoutBranchDialog();
        bool StartCheckoutRevisionDialog();
        bool StartCherryPickDialog();
        bool StartCloneDialog(string url = null);
        bool StartCommandLineProcessDialog(string command, string arguments);
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
        bool StartSubmodulesDialog();
        bool StartUpdateSubmodulesDialog();
        bool StartUpdateSubmodulesRecursiveDialog();
        bool StartVerifyDatabaseDialog();
        bool StartViewPatchDialog();
    }
}