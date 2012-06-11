using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.Blame;
using GitUI.Plugin;
using GitUI.RepoHosting;
using GitUI.Tag;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.RepositoryHosts;
using PatchApply;

namespace GitUI
{
    public sealed class GitUICommands : IGitUICommands
    {
        private static GitUICommands instance;

        public static GitUICommands Instance
        {
            [DebuggerStepThrough]
            get { return instance ?? (instance = new GitUICommands()); }
        }

        #region IGitUICommands Members

        public event GitUIEventHandler PreBrowse;
        public event GitUIEventHandler PostBrowse;

        public event GitUIEventHandler PreDeleteBranch;
        public event GitUIEventHandler PostDeleteBranch;

        public event GitUIEventHandler PreCheckoutRevision;
        public event GitUIEventHandler PostCheckoutRevision;

        public event GitUIEventHandler PreCheckoutBranch;
        public event GitUIEventHandler PostCheckoutBranch;

        public event GitUIEventHandler PreFileHistory;
        public event GitUIEventHandler PostFileHistory;

        public event GitUIEventHandler PreCompareRevisions;
        public event GitUIEventHandler PostCompareRevisions;

        public event GitUIEventHandler PreAddFiles;
        public event GitUIEventHandler PostAddFiles;

        public event GitUIEventHandler PreCreateBranch;
        public event GitUIEventHandler PostCreateBranch;

        public event GitUIEventHandler PreClone;
        public event GitUIEventHandler PostClone;

        public event GitUIEventHandler PreSvnClone;
        public event GitUIEventHandler PostSvnClone;

        public event GitUIEventHandler PreCommit;
        public event GitUIEventHandler PostCommit;

        public event GitUIEventHandler PreSvnDcommit;
        public event GitUIEventHandler PostSvnDcommit;

        public event GitUIEventHandler PreSvnRebase;
        public event GitUIEventHandler PostSvnRebase;

        public event GitUIEventHandler PreSvnFetch;
        public event GitUIEventHandler PostSvnFetch;

        public event GitUIEventHandler PreInitialize;
        public event GitUIEventHandler PostInitialize;

        public event GitUIEventHandler PrePush;
        public event GitUIEventHandler PostPush;

        public event GitUIEventHandler PrePull;
        public event GitUIEventHandler PostPull;

        public event GitUIEventHandler PreViewPatch;
        public event GitUIEventHandler PostViewPatch;

        public event GitUIEventHandler PreApplyPatch;
        public event GitUIEventHandler PostApplyPatch;

        public event GitUIEventHandler PreFormatPatch;
        public event GitUIEventHandler PostFormatPatch;

        public event GitUIEventHandler PreStash;
        public event GitUIEventHandler PostStash;

        public event GitUIEventHandler PreResolveConflicts;
        public event GitUIEventHandler PostResolveConflicts;

        public event GitUIEventHandler PreCherryPick;
        public event GitUIEventHandler PostCherryPick;

        public event GitUIEventHandler PreMergeBranch;
        public event GitUIEventHandler PostMergeBranch;

        public event GitUIEventHandler PreCreateTag;
        public event GitUIEventHandler PostCreateTag;

        public event GitUIEventHandler PreDeleteTag;
        public event GitUIEventHandler PostDeleteTag;

        public event GitUIEventHandler PreEditGitIgnore;
        public event GitUIEventHandler PostEditGitIgnore;

        public event GitUIEventHandler PreSettings;
        public event GitUIEventHandler PostSettings;

        public event GitUIEventHandler PreArchive;
        public event GitUIEventHandler PostArchive;

        public event GitUIEventHandler PreMailMap;
        public event GitUIEventHandler PostMailMap;

        public event GitUIEventHandler PreVerifyDatabase;
        public event GitUIEventHandler PostVerifyDatabase;

        public event GitUIEventHandler PreRemotes;
        public event GitUIEventHandler PostRemotes;

        public event GitUIEventHandler PreRebase;
        public event GitUIEventHandler PostRebase;

        public event GitUIEventHandler PreRename;
        public event GitUIEventHandler PostRename;

        public event GitUIEventHandler PreSubmodulesEdit;
        public event GitUIEventHandler PostSubmodulesEdit;

        public event GitUIEventHandler PreUpdateSubmodules;
        public event GitUIEventHandler PostUpdateSubmodules;

        public event GitUIEventHandler PreSyncSubmodules;
        public event GitUIEventHandler PostSyncSubmodules;

        public event GitUIEventHandler PreBlame;
        public event GitUIEventHandler PostBlame;

        public event GitUIEventHandler PreEditGitAttributes;
        public event GitUIEventHandler PostEditGitAttributes;

        #endregion

        public string GitCommand(string arguments)
        {
            return Settings.Module.RunGitCmd(arguments);
        }

        public string CommandLineCommand(string cmd, string arguments)
        {
            return Settings.Module.RunCmd(cmd, arguments);
        }

        private bool RequiresValidWorkingDir()
        {
            if (!Settings.Module.ValidWorkingDir())
            {
                MessageBox.Show("The current directory is not a valid git repository.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private bool RequiredValidGitSvnWorikingDir()
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!GitSvnCommandHelpers.ValidSvnWorkingDir())
            {
                MessageBox.Show("The current directory is not a valid git-svn repository.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!GitSvnCommandHelpers.CheckRefsRemoteSvn())
            {
                MessageBox.Show("Unable to determine upstream SVN information.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        public bool StartBatchFileProcessDialog(object owner, string batchFile)
        {
            string tempFileName = Path.ChangeExtension(Path.GetTempFileName(), ".cmd");
            using (var writer = new StreamWriter(tempFileName))
            {
                writer.WriteLine("@prompt $G");
                writer.Write(batchFile);
            }
            var process = new FormProcess("cmd.exe", "/C \"" + tempFileName + "\"");
            process.ShowDialog(owner as IWin32Window);
            File.Delete(tempFileName);
            return true;
        }

        public bool StartBatchFileProcessDialog(string batchFile)
        {
            return StartBatchFileProcessDialog(null, batchFile);
        }

        public bool StartCommandLineProcessDialog(GitCommand cmd, Form parentForm)
        {
            FormProcess process;
            if (cmd.AccessesRemote())
                process = new FormRemoteProcess(cmd.ToLine());
            else
                process = new FormProcess(cmd.ToLine());
            process.ShowDialog(parentForm);
            return true;
        }

        public bool StartCommandLineProcessDialog(object owner, string command, string arguments)
        {
            var process = new FormProcess(command, arguments);
            process.ShowDialog(owner as IWin32Window);
            return true;
        }

        public bool StartCommandLineProcessDialog(string command, string arguments)
        {
            return StartCommandLineProcessDialog(null, command, arguments);
        }

        public bool StartGitCommandProcessDialog(IWin32Window owner, string arguments)
        {
            var process = new FormProcess(arguments);
            process.ShowDialog(owner);
            return true;
        }

        public bool StartGitCommandProcessDialog(string arguments)
        {
            return StartGitCommandProcessDialog(null, arguments);
        }

        public bool StartBrowseDialog()
        {
            return StartBrowseDialog("");
        }

        public bool StartDeleteBranchDialog(IWin32Window owner, string branch)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreDeleteBranch))
                return false;

            var form = new FormDeleteBranch(branch);
            form.ShowDialog(owner);

            InvokeEvent(owner, PostDeleteBranch);

            return true;
        }

        public bool StartDeleteBranchDialog(string branch)
        {
            return StartDeleteBranchDialog(null, branch);
        }

        public bool StartCheckoutRevisionDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreCheckoutRevision))
                return false;

            var form = new FormCheckout();
            form.ShowDialog(owner);

            InvokeEvent(owner, PostCheckoutRevision);

            return true;
        }
        public bool StartCheckoutRevisionDialog()
        {
            return StartCheckoutRevisionDialog(null);
        }

        public bool CheckForDirtyDir(IWin32Window owner, out bool needRefresh, out bool force)
        {
            needRefresh = false;
            force = false;
            if (!Settings.DirtyDirWarnBeforeCheckoutBranch || Settings.Module.GitStatus(UntrackedFilesMode.All, IgnoreSubmodulesMode.Default).Count == 0)
                return false;
            switch (new FormDirtyDirWarn().ShowDialog(owner))
            {
                case DialogResult.Cancel:
                    return true;
                case DialogResult.Yes:
                    Stash(owner);
                    return false;
                case DialogResult.Abort:
                    needRefresh = StartCommitDialog(owner);
                    return true;
                case DialogResult.Retry:
                    force = true;
                    return false;
                default:
                    return false;
            }
        }

        public void Stash(IWin32Window owner)
        {
            var arguments = GitCommandHelpers.StashSaveCmd(Settings.IncludeUntrackedFilesInAutoStash);

            new FormProcess(arguments).ShowDialog(owner);
        }

        public bool StartCheckoutBranchDialog(IWin32Window owner, string branch, bool remote, string containRevison)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreCheckoutBranch))
                return false;

            bool needRefresh;
            bool force;
            if (CheckForDirtyDir(owner, out needRefresh, out force))
                return needRefresh;

            var form = new FormCheckoutBranch(branch, remote, containRevison, force);
            if (form.ShowDialog(owner) == DialogResult.Cancel)
                return false;

            InvokeEvent(owner, PostCheckoutBranch);

            return true;
        }

        public bool StartCheckoutBranchDialog(IWin32Window owner, string branch, bool remote)
        {
            return StartCheckoutBranchDialog(null, branch, remote, null);
        }

        public bool StartCheckoutBranchDialog(string branch, bool remote)
        {
            return StartCheckoutBranchDialog(null, branch, remote, null);
        }

        public bool StartCheckoutBranchDialog(string containRevison)
        {
            return StartCheckoutBranchDialog(null, "", false, containRevison);
        }

        public bool StartCheckoutBranchDialog(IWin32Window owner)
        {
            return StartCheckoutBranchDialog(owner, "", false, null);
        }

        public bool StartCheckoutBranchDialog()
        {
            return StartCheckoutBranchDialog(null, "", false, null);
        }

        public bool StartCheckoutRemoteBranchDialog(IWin32Window owner, string branch)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreCheckoutBranch))
                return false;

            bool needRefresh;
            bool force;
            if (CheckForDirtyDir(owner, out needRefresh, out force))
                return needRefresh;

            var form = new FormCheckoutRemoteBranch(branch, force);
            if (form.ShowDialog(owner) == DialogResult.Cancel)
                return false;

            InvokeEvent(owner, PostCheckoutBranch);

            return true;
        }

        public bool StartCompareRevisionsDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreCompareRevisions))
                return false;

            var form = new FormDiff();
            form.ShowDialog(owner);

            InvokeEvent(owner, PostCompareRevisions);

            return false;
        }

        public bool StartCompareRevisionsDialog()
        {
            return StartCompareRevisionsDialog(null);
        }

        public bool StartAddFilesDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreAddFiles))
                return false;

            var form = new FormAddFiles();
            form.ShowDialog(owner);

            InvokeEvent(owner, PostAddFiles);

            return false;
        }

        public bool StartAddFilesDialog()
        {
            return StartAddFilesDialog(null);
        }

        public bool StartCreateBranchDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreCreateBranch))
                return false;

            var form = new FormBranch();
            form.ShowDialog(owner);

            InvokeEvent(owner, PostCreateBranch);

            return true;
        }

        public bool StartCreateBranchDialog()
        {
            return StartCreateBranchDialog(null);
        }

        public bool StartCloneDialog(IWin32Window owner, string url, bool openedFromProtocolHandler)
        {
            if (!InvokeEvent(owner, PreClone))
                return false;

            var form = new FormClone(url, openedFromProtocolHandler);
            form.ShowDialog(owner);

            InvokeEvent(owner, PostClone);

            return true;
        }

        public bool StartCloneDialog(IWin32Window owner, string url)
        {
            return StartCloneDialog(owner, url, false);
        }

        public bool StartCloneDialog(IWin32Window owner)
        {
            return StartCloneDialog(owner, null);
        }

        public bool StartCloneDialog(string url)
        {
            return StartCloneDialog(null, url);
        }

        public bool StartCloneDialog()
        {
            return StartCloneDialog(null, null);
        }

        public bool StartSvnCloneDialog(IWin32Window owner)
        {
            if (!InvokeEvent(owner, PreSvnClone))
                return false;

            var form = new FormSvnClone();
            form.ShowDialog(owner);

            InvokeEvent(owner, PostSvnClone);

            return true;
        }

        public bool StartSvnCloneDialog()
        {
            return StartSvnCloneDialog(null);
        }

        public bool StartCommitDialog(IWin32Window owner, bool showWhenNoChanges)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreCommit))
                return true;

            var form = new FormCommit();
            if (showWhenNoChanges)
                form.ShowDialogWhenChanges(owner);
            else
                form.ShowDialog(owner);

            InvokeEvent(owner, PostCommit);

            if (!form.NeedRefresh)
                return false;

            return true;
        }

        public bool StartCommitDialog(IWin32Window owner)
        {
            return StartCommitDialog(owner, false);
        }

        public bool StartCommitDialog(bool showWhenNoChanges)
        {
            return StartCommitDialog(null, showWhenNoChanges);
        }

        public bool StartCommitDialog()
        {
            return StartCommitDialog(null, false);
        }

        public bool StartSvnDcommitDialog(IWin32Window owner)
        {
            if (!RequiredValidGitSvnWorikingDir())
                return false;

            if (!InvokeEvent(owner, PreSvnDcommit))
                return true;

            var fromProcess = new FormProcess(Settings.GitCommand, GitSvnCommandHelpers.DcommitCmd());
            fromProcess.ShowDialog(owner);

            InvokeEvent(owner, PostSvnDcommit);

            return true;
        }

        public bool StartSvnDcommitDialog()
        {
            return StartSvnDcommitDialog(null);
        }

        public bool StartSvnRebaseDialog(IWin32Window owner)
        {
            if (!RequiredValidGitSvnWorikingDir())
                return false;

            if (!InvokeEvent(owner, PreSvnRebase))
                return true;

            var fromProcess = new FormProcess(Settings.GitCommand, GitSvnCommandHelpers.RebaseCmd());
            fromProcess.ShowDialog(owner);

            InvokeEvent(owner, PostSvnRebase);

            return true;
        }

        public bool StartSvnRebaseDialog()
        {
            return StartSvnRebaseDialog(null);
        }

        public bool StartSvnFetchDialog(IWin32Window owner)
        {
            if (!RequiredValidGitSvnWorikingDir())
                return false;

            if (!InvokeEvent(owner, PreSvnFetch))
                return true;

            var fromProcess = new FormProcess(Settings.GitCommand, GitSvnCommandHelpers.FetchCmd());
            fromProcess.ShowDialog(owner);

            InvokeEvent(owner, PostSvnFetch);

            return true;
        }

        public bool StartSvnFetchDialog()
        {
            return StartSvnFetchDialog(null);
        }

        public bool StartInitializeDialog(IWin32Window owner)
        {
            if (!InvokeEvent(owner, PreInitialize))
                return true;

            new FormInit().ShowDialog(owner);

            InvokeEvent(owner, PostInitialize);

            return true;
        }

        public bool StartInitializeDialog()
        {
            return StartInitializeDialog((IWin32Window)null);
        }

        public bool StartInitializeDialog(IWin32Window owner, string dir)
        {
            if (!InvokeEvent(owner, PreInitialize))
                return true;

            new FormInit(dir).ShowDialog(owner);

            InvokeEvent(owner, PostInitialize);

            return true;
        }

        public bool StartInitializeDialog(string dir)
        {
            return StartInitializeDialog(null, dir);
        }

        public bool StartPushDialog()
        {
            return StartPushDialog(false);
        }

        /// <summary>
        /// Starts pull dialog
        /// </summary>
        /// <param name="owner">An implementation of IWin32Window that will own the modal dialog box.</param>
        /// <param name="pullOnShow"></param>
        /// <param name="pullCompleted">true if pull completed with no errors</param>
        /// <returns>if revision grid should be refreshed</returns>
        public bool StartPullDialog(IWin32Window owner, bool pullOnShow, out bool pullCompleted, ConfigureFormPull configProc)
        {
            pullCompleted = false;

            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PrePull))
                return true;

            FormPull formPull = new FormPull();
            if (configProc != null)
                configProc(formPull);

            DialogResult dlgResult;
            if (pullOnShow)
                dlgResult = formPull.PullAndShowDialogWhenFailed(owner);
            else
                dlgResult = formPull.ShowDialog(owner);

            if (dlgResult == DialogResult.OK)
            {
                InvokeEvent(owner, PostPull);
                pullCompleted = !formPull.ErrorOccurred;
            }

            return true;//maybe InvokeEvent should have 'needRefresh' out parameter?
        }

        public bool StartPullDialog(IWin32Window owner, bool pullOnShow, out bool pullCompleted)
        {
            return StartPullDialog(owner, pullOnShow, out pullCompleted, null);
        }

        public bool StartPullDialog(IWin32Window owner, bool pullOnShow)
        {
            bool errorOccurred;
            return StartPullDialog(owner, pullOnShow, out errorOccurred, null);
        }

        public bool StartPullDialog(bool pullOnShow, out bool pullCompleted)
        {
            return StartPullDialog(null, pullOnShow, out pullCompleted, null);
        }

        public bool StartPullDialog(bool pullOnShow)
        {
            bool errorOccurred;
            return StartPullDialog(pullOnShow, out errorOccurred);
        }

        public bool StartPullDialog(IWin32Window owner)
        {
            bool errorOccurred;
            return StartPullDialog(owner, false, out errorOccurred, null);
        }

        public bool StartPullDialog()
        {
            return StartPullDialog(false);
        }

        public bool StartViewPatchDialog(IWin32Window owner)
        {
            if (!InvokeEvent(owner, PreViewPatch))
                return true;

            var applyPatch = new ViewPatch();
            applyPatch.ShowDialog(owner);

            InvokeEvent(owner, PostViewPatch);

            return true;
        }

        public bool StartViewPatchDialog()
        {
            return StartViewPatchDialog(null);
        }

        public bool StartFormatPatchDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreFormatPatch))
                return true;

            var form = new FormFormatPatch();
            form.ShowDialog(owner);

            InvokeEvent(owner, PostFormatPatch);

            return false;
        }

        public bool StartFormatPatchDialog()
        {
            return StartFormatPatchDialog(null);
        }

        public bool StartStashDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreStash))
                return true;

            var form = new FormStash();
            form.ShowDialog(owner);

            InvokeEvent(owner, PostStash);

            return true;
        }

        public bool StartStashDialog()
        {
            return StartStashDialog(null);
        }

        public bool StartResolveConflictsDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreResolveConflicts))
                return true;

            var form = new FormResolveConflicts();
            form.ShowDialog(owner);

            InvokeEvent(owner, PostResolveConflicts);

            return true;
        }

        public bool StartResolveConflictsDialog()
        {
            return StartResolveConflictsDialog(null);
        }

        public bool StartCherryPickDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreCherryPick))
                return true;

            var form = new FormCherryPick();
            form.ShowDialog(owner);

            InvokeEvent(owner, PostCherryPick);

            return true;
        }

        public bool StartCherryPickDialog()
        {
            return StartCherryPickDialog(null);
        }

        public bool StartMergeBranchDialog(IWin32Window owner, string branch)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreMergeBranch))
                return true;

            var form = new FormMergeBranch(branch);
            form.ShowDialog(owner);

            InvokeEvent(owner, PostMergeBranch);

            return true;
        }

        public bool StartMergeBranchDialog(string branch)
        {
            return StartMergeBranchDialog(null, branch);
        }

        public bool StartCreateTagDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreCreateTag))
                return true;

            var form = new FormTag();
            form.ShowDialog(owner);

            InvokeEvent(owner, PostCreateTag);

            return true;
        }

        public bool StartCreateTagDialog()
        {
            return StartCreateTagDialog(null);
        }

        public bool StartDeleteTagDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreDeleteTag))
                return true;

            var form = new FormDeleteTag();
            form.ShowDialog(owner);

            InvokeEvent(owner, PostDeleteTag);

            return true;
        }

        public bool StartDeleteTagDialog()
        {
            return StartDeleteTagDialog(null);
        }

        public bool StartEditGitIgnoreDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreEditGitIgnore))
                return true;

            var form = new FormGitIgnore();
            form.ShowDialog(owner);

            InvokeEvent(owner, PostEditGitIgnore);

            return false;
        }

        public bool StartEditGitIgnoreDialog()
        {
            return StartEditGitIgnoreDialog(null);
        }

        public bool StartSettingsDialog(IWin32Window owner)
        {
            if (!InvokeEvent(owner, PreSettings))
                return true;

            var form = new FormSettings();
            form.ShowDialog(owner);

            InvokeEvent(owner, PostSettings);

            return false;
        }

        public bool StartSettingsDialog()
        {
            return StartSettingsDialog(null);
        }

        public bool StartArchiveDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreArchive))
                return true;

            var form = new FormArchive();
            form.ShowDialog(owner);

            InvokeEvent(owner, PostArchive);

            return false;
        }

        public bool StartArchiveDialog()
        {
            return StartArchiveDialog(null);
        }

        public bool StartMailMapDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreMailMap))
                return true;

            var form = new FormMailMap();
            form.ShowDialog(owner);

            InvokeEvent(owner, PostMailMap);

            return true;
        }

        public bool StartMailMapDialog()
        {
            return StartMailMapDialog(null);
        }

        public bool StartVerifyDatabaseDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreVerifyDatabase))
                return true;

            var form = new FormVerify();
            form.ShowDialog(owner);

            InvokeEvent(owner, PostVerifyDatabase);

            return true;
        }

        public bool StartVerifyDatabaseDialog()
        {
            return StartVerifyDatabaseDialog(null);
        }

        public bool StartRemotesDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreRemotes))
                return true;

            var form = new FormRemotes();
            form.ShowDialog(owner);

            InvokeEvent(owner, PostRemotes);

            return true;
        }

        public bool StartRemotesDialog()
        {
            return StartRemotesDialog(null);
        }

        public bool StartRebaseDialog(IWin32Window owner, string branch)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreRebase))
                return true;

            var form = new FormRebase(branch);
            form.ShowDialog(owner);

            InvokeEvent(owner, PostRebase);

            return true;
        }

        public bool StartRenameDialog(string branch)
        {
            return StartRenameDialog(null, branch);
        }

        public bool StartRenameDialog(IWin32Window owner, string branch)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreRename))
                return true;

            var form = new FormRenameBranch(branch);

            if (form.ShowDialog(owner) != DialogResult.OK)
                return false;

            InvokeEvent(owner, PostRename);

            return true;
        }

        public bool StartRebaseDialog(string branch)
        {
            return StartRebaseDialog(null, branch);
        }

        public bool StartSubmodulesDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreSubmodulesEdit))
                return true;

            var form = new FormSubmodules();
            form.ShowDialog(owner);

            InvokeEvent(owner, PostSubmodulesEdit);

            return true;
        }

        public bool StartSubmodulesDialog()
        {
            return StartSubmodulesDialog(null);
        }

        public bool StartUpdateSubmodulesDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreUpdateSubmodules))
                return true;

            var process = new FormProcess(GitCommandHelpers.SubmoduleUpdateCmd(""));
            process.ShowDialog(owner);

            InvokeEvent(owner, PostUpdateSubmodules);

            return true;
        }

        public bool StartUpdateSubmodulesDialog()
        {
            return StartUpdateSubmodulesDialog(null);
        }

        public bool StartSyncSubmodulesDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreSyncSubmodules))
                return true;

            var process = new FormProcess(GitCommandHelpers.SubmoduleSyncCmd(""));
            process.ShowDialog(owner);

            InvokeEvent(owner, PostSyncSubmodules);

            return true;
        }

        public bool StartSyncSubmodulesDialog()
        {
            return StartSyncSubmodulesDialog(null);
        }

        public bool StartPluginSettingsDialog(IWin32Window owner)
        {
            new FormPluginSettings().ShowDialog(owner);
            return true;
        }

        public bool StartPluginSettingsDialog()
        {
            return StartPluginSettingsDialog(null);
        }

        public bool StartBrowseDialog(IWin32Window owner, string filter)
        {
            if (!InvokeEvent(owner, PreBrowse))
                return false;

            var form = new FormBrowse(filter);
            form.ShowDialog();

            InvokeEvent(owner, PostBrowse);

            return true;
        }

        public bool StartBrowseDialog(string filter)
        {
            return StartBrowseDialog(null, filter);
        }

        public bool StartFileHistoryDialog(IWin32Window owner, string fileName, GitRevision revision, bool filterByRevision, bool showBlame)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreFileHistory))
                return false;

            var form = new FormFileHistory(fileName, revision, filterByRevision);
            if (showBlame)
                form.SelectBlameTab();
            form.ShowDialog(owner);

            InvokeEvent(owner, PostFileHistory);

            return false;
        }

        public bool StartFileHistoryDialog(IWin32Window owner, string fileName, GitRevision revision, bool filterByRevision)
        {           
            return StartFileHistoryDialog(owner, fileName, revision, filterByRevision, false);
        }

        public bool StartFileHistoryDialog(IWin32Window owner, string fileName, GitRevision revision)
        {
            return StartFileHistoryDialog(owner, fileName, revision, false);
        }

        public bool StartFileHistoryDialog(IWin32Window owner, string fileName)
        {
            return StartFileHistoryDialog(owner, fileName, null, false);
        }

        public bool StartFileHistoryDialog(string fileName, GitRevision revision)
        {
            return StartFileHistoryDialog(null, fileName, revision, false);
        }

        public bool StartFileHistoryDialog(string fileName)
        {
            return StartFileHistoryDialog(fileName, null);
        }

        public bool StartPushDialog(IWin32Window owner, bool pushOnShow)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PrePush))
                return true;

            var form = new FormPush();
            if (pushOnShow)
                form.PushAndShowDialogWhenFailed(owner);
            else
                form.ShowDialog(owner);

            InvokeEvent(owner, PostPush);

            return true;
        }

        public bool StartPushDialog(bool pushOnShow)
        {
            return StartPushDialog(null, pushOnShow);
        }

        public bool StartApplyPatchDialog(IWin32Window owner, string patchFile)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreApplyPatch))
                return true;

            var form = new FormApplyPatch();
            form.SetPatchFile(patchFile);
            form.ShowDialog(owner);

            InvokeEvent(owner, PostApplyPatch);

            return true;
        }

        public bool StartApplyPatchDialog(string patchFile)
        {
            return StartApplyPatchDialog(null, patchFile);
        }

        public bool StartApplyPatchDialog(IWin32Window owner)
        {
            return StartApplyPatchDialog(owner, null);
        }

        public bool StartApplyPatchDialog()
        {
            return StartApplyPatchDialog(null, null);
        }

        public bool StartEditGitAttributesDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreEditGitAttributes))
                return true;

            var form = new FormGitAttributes();
            form.ShowDialog(owner);

            InvokeEvent(owner, PostEditGitAttributes);

            return false;
        }

        public bool StartEditGitAttributesDialog()
        {
            return StartEditGitAttributesDialog(null);
        }
        
        private bool InvokeEvent(IWin32Window ownerForm, GitUIEventHandler gitUIEventHandler)
        {
            return InvokeEvent(this, ownerForm, gitUIEventHandler);
        }

        internal static bool InvokeEvent(object sender, IWin32Window ownerForm, GitUIEventHandler gitUIEventHandler)
        {
            try
            {
                var e = new GitUIEventArgs(ownerForm, Instance);
                if (gitUIEventHandler != null)
                    gitUIEventHandler(sender, e);

                return !e.Cancel;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception");
            }
            return true;
        }

        public bool StartBlameDialog(IWin32Window owner, string fileName)
        {
            return StartBlameDialog(owner, fileName, null);
        }

        private bool StartBlameDialog(IWin32Window owner, string fileName, GitRevision revision)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(owner, PreBlame))
                return false;

            new FormBlame(fileName, revision).ShowDialog(owner);

            InvokeEvent(owner, PostBlame);

            return false;
        }

        public bool StartBlameDialog(string fileName)
        {
            return StartBlameDialog(null, fileName, null);
        }

        private bool StartBlameDialog(string fileName, GitRevision revision)
        {
            return StartBlameDialog(null, fileName, revision);
        }

        private static void WrapRepoHostingCall(string name, IRepositoryHostPlugin gitHoster,
                                                Action<IRepositoryHostPlugin> call)
        {
            if (!gitHoster.ConfigurationOk)
            {
                var eventArgs = new GitUIEventArgs(null, Instance);
                gitHoster.Execute(eventArgs);
            }

            if (gitHoster.ConfigurationOk)
            {
                try
                {
                    call(gitHoster);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        string.Format("ERROR: {0} failed. Message: {1}\r\n\r\n{2}", name, ex.Message, ex.StackTrace),
                        "Error! :(");
                }
            }
        }

        public void StartCloneForkFromHoster(IWin32Window owner, IRepositoryHostPlugin gitHoster)
        {
            WrapRepoHostingCall("View pull requests", gitHoster, gh => (new ForkAndCloneForm(gitHoster)).ShowDialog(owner));
        }

        public void StartCloneForkFromHoster(IRepositoryHostPlugin gitHoster)
        {
            StartCloneForkFromHoster(null, gitHoster);
        }

        internal void StartPullRequestsDialog(IWin32Window owner, IRepositoryHostPlugin gitHoster)
        {
            WrapRepoHostingCall("View pull requests", gitHoster,
                                gh => (new ViewPullRequestsForm(gitHoster)).ShowDialog(owner));
        }

        internal void StartPullRequestsDialog(IRepositoryHostPlugin gitHoster)
        {
            StartPullRequestsDialog(null, gitHoster);
        }

        public void StartCreatePullRequest(IWin32Window owner)
        {
            List<IRepositoryHostPlugin> relevantHosts =
                (from gh in RepoHosts.GitHosters where gh.CurrentWorkingDirRepoIsRelevantToMe select gh).ToList();
            if (relevantHosts.Count == 0)
                MessageBox.Show(owner, "Could not find any repo hosts for current working directory");
            else if (relevantHosts.Count == 1)
                StartCreatePullRequest(owner, relevantHosts.First());
            else
                MessageBox.Show("StartCreatePullRequest:Selection not implemented!");
        }

        public void StartCreatePullRequest()
        {
            StartCreatePullRequest((IRepositoryHostPlugin)null);
        }

        public void StartCreatePullRequest(IWin32Window owner, IRepositoryHostPlugin gitHoster)
        {
            StartCreatePullRequest(owner, gitHoster, null, null);
        }

        public void StartCreatePullRequest(IRepositoryHostPlugin gitHoster)
        {
            StartCreatePullRequest(null, gitHoster, null, null);
        }

        public void StartCreatePullRequest(IRepositoryHostPlugin gitHoster, string chooseRemote, string chooseBranch)
        {
            StartCreatePullRequest(null, gitHoster, chooseRemote, chooseBranch);
        }

        public void StartCreatePullRequest(IWin32Window owner, IRepositoryHostPlugin gitHoster, string chooseRemote, string chooseBranch)
        {
            WrapRepoHostingCall("Create pull request", gitHoster,
                                gh => (new CreatePullRequestForm(gitHoster, chooseRemote, chooseBranch)).ShowDialog(owner));
        }
    }
}