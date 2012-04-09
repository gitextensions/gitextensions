using System;
using System.Collections.Generic;
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

        public event GitUIEventHandler PreUpdateSubmodulesRecursive;
        public event GitUIEventHandler PostUpdateSubmodulesRecursive;

        public event GitUIEventHandler PreInitSubmodules;
        public event GitUIEventHandler PostInitSubmodules;

        public event GitUIEventHandler PreInitSubmodulesRecursive;
        public event GitUIEventHandler PostInitSubmodulesRecursive;

        public event GitUIEventHandler PreSyncSubmodules;
        public event GitUIEventHandler PostSyncSubmodules;

        public event GitUIEventHandler PreSyncSubmodulesRecursive;
        public event GitUIEventHandler PostSyncSubmodulesRecursive;

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

        public bool StartBatchFileProcessDialog(IWin32Window owner, string batchFile)
        {
            string tempFileName = Path.ChangeExtension(Path.GetTempFileName(), ".cmd");
            using (var writer = new StreamWriter(tempFileName))
            {
                writer.WriteLine("@prompt $G");
                writer.Write(batchFile);
            }
            var process = new FormProcess("cmd.exe", "/C \"" + tempFileName + "\"");
            bool result = process.ShowDialog(owner) == DialogResult.OK;
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

        public bool StartCommandLineProcessDialog(IWin32Window owner, string command, string arguments)
        {
            var process = new FormProcess(command, arguments);
            process.ShowDialog(owner);
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

            if (!InvokeEvent(PreDeleteBranch))
                return false;

            var form = new FormDeleteBranch(branch);
            form.ShowDialog(owner);

            InvokeEvent(PostDeleteBranch);

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

            if (!InvokeEvent(PreCheckoutRevision))
                return false;

            var form = new FormCheckout();
            form.ShowDialog(owner);

            InvokeEvent(PostCheckoutRevision);

            return true;
        }
        public bool StartCheckoutRevisionDialog()
        {
            return StartCheckoutRevisionDialog(null);
        }

        public bool StartCheckoutBranchDialog(IWin32Window owner)
        {
            return StartCheckoutBranchDialog(owner, "", false);
        }


        public bool CheckForDirtyDir(IWin32Window owner, out bool needRefresh)
        {
            needRefresh = false;
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
                default:
                    return false;
            }
        }

        public void Stash(IWin32Window owner)
        {
            var arguments = "stash save";
            if (Settings.IncludeUntrackedFilesInAutoStash && GitCommandHelpers.VersionInUse.StashUntrackedFilesSupported)
                arguments += " -u";

            new FormProcess(arguments).ShowDialog(owner);
        }

        public bool StartCheckoutBranchDialog()
        {
            return StartCheckoutBranchDialog(null);
        }

        public bool StartCheckoutBranchDialog(IWin32Window owner, string branch, bool remote)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(PreCheckoutBranch))
                return false;

            bool needRefresh;
            if (CheckForDirtyDir(owner, out needRefresh))
                return needRefresh;

            var form = new FormCheckoutBranch(branch, remote);
            form.ShowDialog(owner);

            InvokeEvent(PostCheckoutBranch);

            return true;
        }

        public bool StartCheckoutBranchDialog(string branch, bool remote)
        {
            return StartCheckoutBranchDialog(null, branch, remote);
        }

        public bool StartCompareRevisionsDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(PreCompareRevisions))
                return false;

            var form = new FormDiff();
            form.ShowDialog(owner);

            InvokeEvent(PostCompareRevisions);

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

            if (!InvokeEvent(PreAddFiles))
                return false;

            var form = new FormAddFiles();
            form.ShowDialog(owner);

            InvokeEvent(PostAddFiles);

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

            if (!InvokeEvent(PreCreateBranch))
                return false;

            var form = new FormBranch();
            form.ShowDialog(owner);

            InvokeEvent(PostCreateBranch);

            return true;
        }

        public bool StartCreateBranchDialog()
        {
            return StartCreateBranchDialog(null);
        }

        public bool StartCloneDialog(IWin32Window owner, string url)
        {
            if (!InvokeEvent(PreClone))
                return false;

            var form = new FormClone(url);
            form.ShowDialog(owner);

            InvokeEvent(PostClone);

            return true;
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
            if (!InvokeEvent(PreSvnClone))
                return false;

            var form = new FormSvnClone();
            form.ShowDialog(owner);

            InvokeEvent(PostSvnClone);

            return true;
        }

        public bool StartCommitDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(PreCommit))
                return true;

            var form = new FormCommit();
            form.ShowDialog(owner);

            InvokeEvent(PostCommit);

            if (!form.NeedRefresh)
                return false;

            return true;
        }

        public bool StartSvnDcommitDialog(IWin32Window owner)
        {
            if (!RequiredValidGitSvnWorikingDir())
                return false;

            if (!InvokeEvent(PreSvnDcommit))
                return true;

            var fromProcess = new FormProcess(Settings.GitCommand, GitSvnCommandHelpers.DcommitCmd());
            fromProcess.ShowDialog(owner);

            InvokeEvent(PostSvnDcommit);

            return true;
        }

        public bool StartSvnRebaseDialog(IWin32Window owner)
        {
            if (!RequiredValidGitSvnWorikingDir())
                return false;

            if (!InvokeEvent(PreSvnRebase))
                return true;

            var fromProcess = new FormProcess(Settings.GitCommand, GitSvnCommandHelpers.RebaseCmd());
            fromProcess.ShowDialog(owner);

            InvokeEvent(PostSvnRebase);

            return true;
        }

        public bool StartSvnFetchDialog(IWin32Window owner)
        {
            if (!RequiredValidGitSvnWorikingDir())
                return false;

            if (!InvokeEvent(PreSvnFetch))
                return true;

            var fromProcess = new FormProcess(Settings.GitCommand, GitSvnCommandHelpers.FetchCmd());
            fromProcess.ShowDialog(owner);

            InvokeEvent(PostSvnFetch);

            return true;
        }

        public bool StartCommitDialog()
        {
            return StartCommitDialog(null);
        }

        public bool StartCommitDialog(IWin32Window owner, bool showWhenNoChanges)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(PreCommit))
                return true;

            var form = new FormCommit();
            if (showWhenNoChanges)
                form.ShowDialogWhenChanges(owner);
            else
                form.ShowDialog(owner);

            InvokeEvent(PostCommit);

            if (!form.NeedRefresh)
                return false;

            return true;
        }

        public bool StartCommitDialog(bool showWhenNoChanges)
        {
            return StartCommitDialog(null, showWhenNoChanges);
        }

        public bool StartInitializeDialog(IWin32Window owner)
        {
            if (!InvokeEvent(PreInitialize))
                return true;

            new FormInit().ShowDialog(owner);

            InvokeEvent(PostInitialize);

            return true;
        }

        public bool StartInitializeDialog()
        {
            return StartInitializeDialog((IWin32Window)null);
        }

        public bool StartInitializeDialog(IWin32Window owner, string dir)
        {
            if (!InvokeEvent(PreInitialize))
                return true;

            new FormInit(dir).ShowDialog(owner);

            InvokeEvent(PostInitialize);

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
        public bool StartPullDialog(IWin32Window owner, bool pullOnShow, out bool pullCompleted)
        {
            pullCompleted = false;

            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(PrePull))
                return true;

            FormPull formPull = new FormPull();
            DialogResult dlgResult;
            if (pullOnShow)
                dlgResult = formPull.PullAndShowDialogWhenFailed(owner);
            else
                dlgResult = formPull.ShowDialog(owner);

            if (dlgResult == DialogResult.OK)
            {
                InvokeEvent(PostPull);
                pullCompleted = !formPull.ErrorOccurred;
            }

            return true;//maybe InvokeEvent should have 'needRefresh' out parameter?
        }

        public bool StartPullDialog(IWin32Window owner, bool pullOnShow)
        {
            bool errorOccurred;
            return StartPullDialog(owner, pullOnShow, out errorOccurred);
        }

        public bool StartPullDialog(bool pullOnShow, out bool pullCompleted)
        {
            return StartPullDialog(null, pullOnShow, out pullCompleted);
        }

        public bool StartPullDialog(bool pullOnShow)
        {
            bool errorOccurred;
            return StartPullDialog(pullOnShow, out errorOccurred);
        }

        public bool StartPullDialog(IWin32Window owner)
        {
            bool errorOccurred;
            return StartPullDialog(owner, false, out errorOccurred);
        }

        public bool StartPullDialog()
        {
            return StartPullDialog(false);
        }

        public bool StartViewPatchDialog(IWin32Window owner)
        {
            if (!InvokeEvent(PreViewPatch))
                return true;

            var applyPatch = new ViewPatch();
            applyPatch.ShowDialog(owner);

            InvokeEvent(PostViewPatch);

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

            if (!InvokeEvent(PreFormatPatch))
                return true;

            var form = new FormFormatPatch();
            form.ShowDialog(owner);

            InvokeEvent(PostFormatPatch);

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

            if (!InvokeEvent(PreStash))
                return true;

            var form = new FormStash();
            form.ShowDialog(owner);

            InvokeEvent(PostStash);

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

            if (!InvokeEvent(PreResolveConflicts))
                return true;

            var form = new FormResolveConflicts();
            form.ShowDialog(owner);

            InvokeEvent(PostResolveConflicts);

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

            if (!InvokeEvent(PreCherryPick))
                return true;

            var form = new FormCherryPick();
            form.ShowDialog(owner);

            InvokeEvent(PostCherryPick);

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

            if (!InvokeEvent(PreMergeBranch))
                return true;

            var form = new FormMergeBranch(branch);
            form.ShowDialog(owner);

            InvokeEvent(PostMergeBranch);

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

            if (!InvokeEvent(PreCreateTag))
                return true;

            var form = new FormTag();
            form.ShowDialog(owner);

            InvokeEvent(PostCreateTag);

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

            if (!InvokeEvent(PreDeleteTag))
                return true;

            var form = new FormDeleteTag();
            form.ShowDialog(owner);

            InvokeEvent(PostDeleteTag);

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

            if (!InvokeEvent(PreEditGitIgnore))
                return true;

            var form = new FormGitIgnore();
            form.ShowDialog(owner);

            InvokeEvent(PostEditGitIgnore);

            return false;
        }

        public bool StartEditGitIgnoreDialog()
        {
            return StartEditGitIgnoreDialog(null);
        }

        public bool StartSettingsDialog(IWin32Window owner)
        {
            if (!InvokeEvent(PreSettings))
                return true;

            var form = new FormSettings();
            form.ShowDialog(owner);

            InvokeEvent(PostSettings);

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

            if (!InvokeEvent(PreArchive))
                return true;

            var form = new FormArchive();
            form.ShowDialog(owner);

            InvokeEvent(PostArchive);

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

            if (!InvokeEvent(PreMailMap))
                return true;

            var form = new FormMailMap();
            form.ShowDialog(owner);

            InvokeEvent(PostMailMap);

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

            if (!InvokeEvent(PreVerifyDatabase))
                return true;

            var form = new FormVerify();
            form.ShowDialog(owner);

            InvokeEvent(PostVerifyDatabase);

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

            if (!InvokeEvent(PreRemotes))
                return true;

            var form = new FormRemotes();
            form.ShowDialog(owner);

            InvokeEvent(PostRemotes);

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

            if (!InvokeEvent(PreRebase))
                return true;

            var form = new FormRebase(branch);
            form.ShowDialog(owner);

            InvokeEvent(PostRebase);

            return true;
        }

        public bool StartRenameDialog(string branch)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(PreRename))
                return true;

            var form = new FormRenameBranch(branch);

            if (form.ShowDialog() != DialogResult.OK)
                return false;

            InvokeEvent(PostRename);

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

            if (!InvokeEvent(PreSubmodulesEdit))
                return true;

            var form = new FormSubmodules();
            form.ShowDialog(owner);

            InvokeEvent(PostSubmodulesEdit);

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

            if (!InvokeEvent(PreUpdateSubmodules))
                return true;

            var process = new FormProcess(GitCommandHelpers.SubmoduleUpdateCmd(""));
            process.ShowDialog(owner);

            InvokeEvent(PostUpdateSubmodules);

            return true;
        }

        public bool StartUpdateSubmodulesDialog()
        {
            return StartUpdateSubmodulesDialog(null);
        }

        public bool StartUpdateSubmodulesRecursiveDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(PreUpdateSubmodulesRecursive))
                return true;

            var process = new FormProcess(GitCommandHelpers.SubmoduleUpdateCmd(""));
            process.ShowDialog(owner);
            ForEachSubmodulesRecursive(GitCommandHelpers.SubmoduleUpdateCmd(""));

            InvokeEvent(PostUpdateSubmodulesRecursive);

            return true;
        }

        public bool StartUpdateSubmodulesRecursiveDialog()
        {
            return StartUpdateSubmodulesRecursiveDialog(null);
        }

        public bool StartInitSubmodulesDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(PreInitSubmodules))
                return true;

            var process = new FormProcess(GitCommandHelpers.SubmoduleInitCmd(""));
            process.ShowDialog(owner);

            InvokeEvent(PostInitSubmodules);

            return true;
        }

        public bool StartInitSubmodulesDialog()
        {
            return StartInitSubmodulesDialog(null);
        }

        public bool StartInitSubmodulesRecursiveDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(PreInitSubmodulesRecursive))
                return true;

            var process = new FormProcess(GitCommandHelpers.SubmoduleInitCmd(""));
            process.ShowDialog(owner);
            ForEachSubmodulesRecursive(GitCommandHelpers.SubmoduleInitCmd(""));

            InvokeEvent(PostInitSubmodulesRecursive);

            return true;
        }

        public bool StartInitSubmodulesRecursiveDialog()
        {
            return StartInitSubmodulesRecursiveDialog(null);
        }

        public bool StartSyncSubmodulesDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(PreSyncSubmodules))
                return true;

            var process = new FormProcess(GitCommandHelpers.SubmoduleSyncCmd(""));
            process.ShowDialog(owner);

            InvokeEvent(PostSyncSubmodules);

            return true;
        }

        public bool StartSyncSubmodulesDialog()
        {
            return StartSyncSubmodulesDialog(null);
        }

        public bool StartSyncSubmodulesRecursiveDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(PreSyncSubmodulesRecursive))
                return true;

            var process = new FormProcess(GitCommandHelpers.SubmoduleSyncCmd(""));
            process.ShowDialog(owner);
            ForEachSubmodulesRecursive(GitCommandHelpers.SubmoduleSyncCmd(""));

            InvokeEvent(PostSyncSubmodulesRecursive);

            return true;
        }

        public bool StartSyncSubmodulesRecursiveDialog()
        {
            return StartSyncSubmodulesRecursiveDialog(null);
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

        #endregion

        public event GitUIEventHandler PreBlame;
        public event GitUIEventHandler PostBlame;
        public event GitUIEventHandler PreEditGitAttributes;
        public event GitUIEventHandler PostEditGitAttributes;

        public bool StartBrowseDialog(IWin32Window owner, string filter)
        {
            if (!InvokeEvent(PreBrowse))
                return false;

            var form = new FormBrowse(filter);
            form.ShowDialog();

            InvokeEvent(PostBrowse);

            return true;
        }

        public bool StartBrowseDialog(string filter)
        {
            return StartBrowseDialog(null, filter);
        }

        public bool StartFileHistoryDialog(IWin32Window owner, string fileName, GitRevision revision)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(PreFileHistory))
                return false;

            var form = new FormFileHistory(fileName, revision);
            form.ShowDialog(owner);

            InvokeEvent(PostFileHistory);

            return false;
        }

        public bool StartFileHistoryDialog(IWin32Window owner, string fileName)
        {
            return StartFileHistoryDialog(owner, fileName, null);
        }

        public bool StartFileHistoryDialog(string fileName, GitRevision revision)
        {
            return StartFileHistoryDialog(null, fileName, revision);
        }

        public bool StartFileHistoryDialog(string fileName)
        {
            return StartFileHistoryDialog(fileName, null);
        }

        public bool StartPushDialog(IWin32Window owner, bool pushOnShow)
        {
            if (!RequiresValidWorkingDir())
                return false;

            if (!InvokeEvent(PrePush))
                return true;

            var form = new FormPush();
            if (pushOnShow)
                form.PushAndShowDialogWhenFailed(owner);
            else
                form.ShowDialog(owner);

            InvokeEvent(PostPush);

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

            if (!InvokeEvent(PreApplyPatch))
                return true;

            var form = new FormApplyPatch();
            form.SetPatchFile(patchFile);
            form.ShowDialog(owner);

            InvokeEvent(PostApplyPatch);

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

            if (!InvokeEvent(PreEditGitAttributes))
                return true;

            var form = new FormGitAttributes();
            form.ShowDialog(owner);

            InvokeEvent(PostEditGitAttributes);

            return false;
        }

        public bool StartEditGitAttributesDialog()
        {
            return StartEditGitAttributesDialog(null);
        }

        private static void ForEachSubmodulesRecursive(IWin32Window owner, string cmd)
        {
            var oldworkingdir = Settings.WorkingDir;

            foreach (GitSubmodule submodule in (new GitCommandsInstance()).GetSubmodules())
            {
                if (string.IsNullOrEmpty(submodule.LocalPath))
                    continue;

                Settings.WorkingDir = oldworkingdir + submodule.LocalPath;

                if (Settings.WorkingDir != oldworkingdir && File.Exists(Settings.WorkingDir + ".gitmodules"))
                {
                    var process = new FormProcess(cmd);
                    process.ShowDialog(owner);

                    ForEachSubmodulesRecursive(owner, cmd);
                }

                Settings.WorkingDir = oldworkingdir;
            }

            Settings.WorkingDir = oldworkingdir;
        }

        private static void ForEachSubmodulesRecursive(string cmd)
        {
            ForEachSubmodulesRecursive(null, cmd);
        }

        private bool InvokeEvent(GitUIEventHandler gitUIEventHandler)
        {
            return InvokeEvent(this, gitUIEventHandler);
        }

        internal static bool InvokeEvent(object sender, GitUIEventHandler gitUIEventHandler)
        {
            try
            {
                var e = new GitUIEventArgs(Instance);
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

            if (!InvokeEvent(PreBlame))
                return false;

            new FormBlame(fileName, revision).ShowDialog(owner);

            InvokeEvent(PostBlame);

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
                var eventArgs = new GitUIEventArgs(Instance);
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