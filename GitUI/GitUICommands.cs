using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.Blame;
using GitUI.Notifications;
using GitUI.RepoHosting;
using GitUI.Tag;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.Notifications;
using GitUIPluginInterfaces.RepositoryHosts;
using Gravatar;
using PatchApply;
using Settings = GitCommands.Settings;
using GitUI.SettingsDialog;
using GitUI.SettingsDialog.Pages;

namespace GitUI
{
    /// <summary>Contains methods to invoke GitEx forms, dialogs, etc.</summary>
    public sealed class GitUICommands : IGitUICommands
    {
        public GitUICommands(GitModule module)
        {
            Module = module;
            Notifications = NotificationManager.Get(this);
        }

        public GitUICommands(string workingDir)
            : this(new GitModule(workingDir)) { }

        #region IGitUICommands Members

        public event GitUIEventHandler PreBrowse;
        public event GitUIPostActionEventHandler PostBrowse;

        public event GitUIEventHandler PreDeleteBranch;
        public event GitUIEventHandler PostDeleteBranch;

        public event GitUIEventHandler PreCheckoutRevision;
        public event GitUIPostActionEventHandler PostCheckoutRevision;

        public event GitUIEventHandler PreCheckoutBranch;
        public event GitUIPostActionEventHandler PostCheckoutBranch;

        public event GitUIEventHandler PreFileHistory;
        public event GitUIEventHandler PostFileHistory;

        public event GitUIEventHandler PreCompareRevisions;
        public event GitUIEventHandler PostCompareRevisions;

        public event GitUIEventHandler PreAddFiles;
        public event GitUIPostActionEventHandler PostAddFiles;

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
        public event GitUIPostActionEventHandler PostApplyPatch;

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
        public event GitUIPostActionEventHandler PostArchive;

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
        public event GitUIPostActionEventHandler PostBlame;

        public event GitUIEventHandler PreEditGitAttributes;
        public event GitUIEventHandler PostEditGitAttributes;

        public event GitUIEventHandler PreBrowseInitialize;
        public event GitUIEventHandler PostBrowseInitialize;
        public event GitUIEventHandler BrowseInitialize;
        /// <summary>
        /// listeners for changes being made to repository
        /// </summary>
        public event GitUIEventHandler PostRepositoryChanged;

        public event GitUIEventHandler PostRegisterPlugin;

        #endregion

        public string GitCommand(string arguments)
        {
            return Module.RunGitCmd(arguments);
        }

        public string CommandLineCommand(string cmd, string arguments)
        {
            return Module.RunCmd(cmd, arguments);
        }

        private bool RequiresValidWorkingDir(object owner)
        {
            if (!Module.ValidWorkingDir())
            {
                MessageBoxes.NotValidGitDirectory(owner as IWin32Window);
                return false;
            }

            return true;
        }

        private bool RequiredValidGitSvnWorikingDir(object owner)
        {
            if (!RequiresValidWorkingDir(owner))
                return false;

            if (!GitSvnCommandHelpers.ValidSvnWorkingDir(Module))
            {
                MessageBoxes.NotValidGitSVNDirectory(owner as IWin32Window);
                return false;
            }

            if (!GitSvnCommandHelpers.CheckRefsRemoteSvn(Module))
            {
                MessageBoxes.UnableGetSVNInformation(owner as IWin32Window);
                return false;
            }

            return true;
        }

        public void CacheAvatar(string email)
        {
            FallBackService gravatarFallBack = FallBackService.Identicon;
            try
            {
                gravatarFallBack =
                    (FallBackService)Enum.Parse(typeof(FallBackService), Settings.GravatarFallbackService);
            }
            catch
            {
                Settings.GravatarFallbackService = gravatarFallBack.ToString();
            }
            GravatarService.CacheImage(email + ".png", email, Settings.AuthorImageSize,
                gravatarFallBack);
        }

        public Icon FormIcon { get { return GitExtensionsForm.ApplicationIcon; } }

        /// <summary>Gets notifications implementation.</summary>
        public INotifications Notifications { get; private set; }

        public bool StartBatchFileProcessDialog(object owner, string batchFile)
        {
            string tempFileName = Path.ChangeExtension(Path.GetTempFileName(), ".cmd");
            using (var writer = new StreamWriter(tempFileName))
            {
                writer.WriteLine("@prompt $G");
                writer.Write(batchFile);
            }
            FormProcess.ShowDialog(owner as IWin32Window, Module, "cmd.exe", "/C \"" + tempFileName + "\"");
            File.Delete(tempFileName);
            return true;
        }

        public bool StartBatchFileProcessDialog(string batchFile)
        {
            return StartBatchFileProcessDialog(null, batchFile);
        }

        public bool StartCommandLineProcessDialog(GitCommand cmd, IWin32Window parentForm)
        {
            if (cmd.AccessesRemote())
                return FormRemoteProcess.ShowDialog(parentForm, Module, cmd.ToLine());
            else
                return FormProcess.ShowDialog(parentForm, Module, cmd.ToLine());
        }

        public bool StartCommandLineProcessDialog(object owner, string command, string arguments)
        {
            FormProcess.ShowDialog(owner as IWin32Window, Module, command, arguments);
            return true;
        }

        public bool StartCommandLineProcessDialog(string command, string arguments)
        {
            return StartCommandLineProcessDialog(null, command, arguments);
        }

        public bool StartGitCommandProcessDialog(IWin32Window owner, string arguments)
        {
            FormProcess.ShowDialog(owner, Module, arguments);
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
            if (!RequiresValidWorkingDir(owner))
                return false;

            if (!InvokeEvent(owner, PreDeleteBranch))
                return false;

            using (var form = new FormDeleteBranch(this, branch))
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
            return DoAction(owner, true, PreCheckoutRevision, PostCheckoutRevision, () =>
                {
                    using (var form = new FormCheckout(this))
                        form.ShowDialog(owner);
                    return true;
                }
            );
        }

        public bool StartCheckoutRevisionDialog()
        {
            return StartCheckoutRevisionDialog(null);
        }

        public void Stash(IWin32Window owner)
        {
            var arguments = GitCommandHelpers.StashSaveCmd(Settings.IncludeUntrackedFilesInAutoStash);
            FormProcess.ShowDialog(owner, Module, arguments);
        }

        public void InvokeEventOnClose(Form form, GitUIEventHandler ev)
        {
            form.FormClosed += (o, ea) =>
            {
                InvokeEvent(form == null ? null : form.Owner, ev);
            };

        }

        /// <summary>
        ///  notify listeners that changes were made to repository
        /// </summary>
        public void FirePostRepositoryChanged(IWin32Window owner)
        {
            InvokeEvent(owner, PostRepositoryChanged);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requiresValidWorkingDir">If action requires valid working directory</param>
        /// <param name="owner">Owner window</param>
        /// <param name="preEvent">Event invoked before performing action</param>
        /// <param name="postEvent">Event invoked after performing action</param>
        /// <param name="action">Action to do</param>
        /// <returns>true if action was done, false otherwise</returns>
        public bool DoAction(IWin32Window owner, bool requiresValidWorkingDir, GitUIEventHandler preEvent, GitUIPostActionEventHandler postEvent, Func<bool> action)
        {
            if (requiresValidWorkingDir && !RequiresValidWorkingDir(owner))
                return false;

            if (!InvokeEvent(owner, preEvent))
                return false;

            bool actionDone = action();

            InvokePostEvent(owner, actionDone, postEvent);

            return actionDone;
        }

        #region Checkout

        public bool StartCheckoutBranchDialog(IWin32Window owner, string branch, bool remote, string containRevison)
        {
            return DoAction(owner, true, PreCheckoutBranch, PostCheckoutBranch, () =>
            {
                using (var form = new FormCheckoutBranch(this, branch, remote, containRevison))
                    return form.DoDefaultActionOrShow(owner) != DialogResult.Cancel;
            }
            );
        }

        public bool StartCheckoutBranchDialog(IWin32Window owner, string branch, bool remote)
        {
            return StartCheckoutBranchDialog(owner, branch, remote, null);
        }

        public bool StartCheckoutBranchDialog(IWin32Window owner, string containRevison)
        {
            return StartCheckoutBranchDialog(owner, "", false, containRevison);
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
            return StartCheckoutBranchDialog(owner, branch, true);
        }

        public bool StartCheckoutBranchDialog(string branch)
        {
            throw new NotImplementedException();
            return StartCheckoutBranchDialog(null, branch);
        }

        #endregion Checkout

        public bool StartCompareRevisionsDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir(owner))
                return false;

            if (!InvokeEvent(owner, PreCompareRevisions))
                return false;

            using (var form = new FormDiff(this))
                form.ShowDialog(owner);

            InvokeEvent(owner, PostCompareRevisions);

            return false;
        }

        public bool StartCompareRevisionsDialog()
        {
            return StartCompareRevisionsDialog(null);
        }

        public bool StartAddFilesDialog(IWin32Window owner, string addFiles)
        {
            return DoAction(owner, true, PreAddFiles, PostAddFiles, () =>
            {
                using (var form = new FormAddFiles(this, addFiles))
                    form.ShowDialog(owner);

                return true;
            }
            );
        }

        public bool StartAddFilesDialog(IWin32Window owner)
        {
            return StartAddFilesDialog(owner, null);
        }

        public bool StartAddFilesDialog(string addFiles)
        {
            return StartAddFilesDialog(null, addFiles);
        }

        public bool StartAddFilesDialog()
        {
            return StartAddFilesDialog(null, null);
        }

        public bool StartCreateBranchDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir(owner))
                return false;

            if (!InvokeEvent(owner, PreCreateBranch))
                return false;

            using (var form = new FormBranch(this))
                form.ShowDialog(owner);

            InvokeEvent(owner, PostCreateBranch);

            return true;
        }

        public bool StartCreateBranchDialog()
        {
            return StartCreateBranchDialog(null);
        }

        public bool StartCloneDialog(IWin32Window owner, string url, bool openedFromProtocolHandler, GitModuleChangedEventHandler GitModuleChanged)
        {
            if (!InvokeEvent(owner, PreClone))
                return false;

            using (var form = new FormClone(this, url, openedFromProtocolHandler, GitModuleChanged))
                form.ShowDialog(owner);

            InvokeEvent(owner, PostClone);

            return true;
        }

        public bool StartCloneDialog(IWin32Window owner, string url)
        {
            return StartCloneDialog(owner, url, false, null);
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

        public bool StartSvnCloneDialog(IWin32Window owner, GitModuleChangedEventHandler GitModuleChanged)
        {
            if (!InvokeEvent(owner, PreSvnClone))
                return false;

            using (var form = new FormSvnClone(this, GitModuleChanged))
                form.ShowDialog(owner);

            InvokeEvent(owner, PostSvnClone);

            return true;
        }

        public bool StartSvnCloneDialog()
        {
            return StartSvnCloneDialog(null, null);
        }

        public void StartCleanupRepositoryDialog(IWin32Window owner)
        {
            using (var frm = new FormCleanupRepository(this))
                frm.ShowDialog(owner);
        }

        public void StartCleanupRepositoryDialog()
        {
            StartCleanupRepositoryDialog(null);
        }

        public bool StartCommitDialog(IWin32Window owner, bool showOnlyWhenChanges)
        {
            if (!RequiresValidWorkingDir(owner))
                return false;

            if (!InvokeEvent(owner, PreCommit))
                return true;

            using (var form = new FormCommit(this))
            {
                if (showOnlyWhenChanges)
                    form.ShowDialogWhenChanges(owner);
                else
                    form.ShowDialog(owner);

                InvokeEvent(owner, PostCommit);

                if (!form.NeedRefresh)
                    return false;
            }

            return true;
        }

        public bool StartCommitDialog(IWin32Window owner)
        {
            return StartCommitDialog(owner, false);
        }

        public bool StartCommitDialog(bool showOnlyWhenChanges)
        {
            return StartCommitDialog(null, showOnlyWhenChanges);
        }

        public bool StartCommitDialog()
        {
            return StartCommitDialog(null, false);
        }

        public bool StartSvnDcommitDialog(IWin32Window owner)
        {
            if (!RequiredValidGitSvnWorikingDir(owner))
                return false;

            if (!InvokeEvent(owner, PreSvnDcommit))
                return true;

            FormProcess.ShowDialog(owner, Module, Settings.GitCommand, GitSvnCommandHelpers.DcommitCmd());

            InvokeEvent(owner, PostSvnDcommit);

            return true;
        }

        public bool StartSvnDcommitDialog()
        {
            return StartSvnDcommitDialog(null);
        }

        public bool StartSvnRebaseDialog(IWin32Window owner)
        {
            if (!RequiredValidGitSvnWorikingDir(owner))
                return false;

            if (!InvokeEvent(owner, PreSvnRebase))
                return true;

            FormProcess.ShowDialog(owner, Module, Settings.GitCommand, GitSvnCommandHelpers.RebaseCmd());

            InvokeEvent(owner, PostSvnRebase);

            return true;
        }

        public bool StartSvnRebaseDialog()
        {
            return StartSvnRebaseDialog(null);
        }

        public bool StartSvnFetchDialog(IWin32Window owner)
        {
            if (!RequiredValidGitSvnWorikingDir(owner))
                return false;

            if (!InvokeEvent(owner, PreSvnFetch))
                return true;

            FormProcess.ShowDialog(owner, Module, Settings.GitCommand, GitSvnCommandHelpers.FetchCmd());

            InvokeEvent(owner, PostSvnFetch);

            return true;
        }

        public bool StartSvnFetchDialog()
        {
            return StartSvnFetchDialog(null);
        }

        public bool StartInitializeDialog(IWin32Window owner, GitModuleChangedEventHandler GitModuleChanged)
        {
            if (!InvokeEvent(owner, PreInitialize))
                return true;

            string dir = Module.ValidWorkingDir() ? Module.WorkingDir : string.Empty;
            using (var frm = new FormInit(dir, GitModuleChanged)) frm.ShowDialog(owner);

            InvokeEvent(owner, PostInitialize);

            return true;
        }

        public bool StartInitializeDialog()
        {
            return StartInitializeDialog((IWin32Window)null, null);
        }

        public bool StartInitializeDialog(IWin32Window owner, string dir, GitModuleChangedEventHandler GitModuleChanged)
        {
            if (!InvokeEvent(owner, PreInitialize))
                return true;

            using (var frm = new FormInit(dir, GitModuleChanged)) frm.ShowDialog(owner);

            InvokeEvent(owner, PostInitialize);

            return true;
        }

        public bool StartInitializeDialog(string dir)
        {
            return StartInitializeDialog(null, dir, null);
        }

        /// <summary>
        /// Starts pull dialog
        /// </summary>
        /// <param name="owner">An implementation of IWin32Window that will own the modal dialog box.</param>
        /// <param name="pullOnShow"></param>
        /// <param name="pullCompleted">true if pull completed with no errors</param>
        /// <returns>if revision grid should be refreshed</returns>
        public bool StartPullDialog(IWin32Window owner, bool pullOnShow, string remoteBranch, out bool pullCompleted, ConfigureFormPull configProc)
        {
            pullCompleted = false;

            if (!RequiresValidWorkingDir(owner))
                return false;

            if (!InvokeEvent(owner, PrePull))
                return true;

            using (FormPull formPull = new FormPull(this, remoteBranch))
            {
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
            }

            return true;//maybe InvokeEvent should have 'needRefresh' out parameter?
        }

        public bool StartPullDialog(IWin32Window owner, bool pullOnShow, out bool pullCompleted, ConfigureFormPull configProc)
        {
            return StartPullDialog(owner, pullOnShow, null, out pullCompleted, configProc);
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

        public bool StartPullDialog(bool pullOnShow, string remoteBranch, out bool pullCompleted)
        {
            return StartPullDialog(null, pullOnShow, remoteBranch, out pullCompleted, null);
        }

        public bool StartPullDialog(bool pullOnShow)
        {
            return StartPullDialog(pullOnShow, null);
        }

        public bool StartPullDialog(bool pullOnShow, string remoteBranch)
        {
            bool errorOccurred;
            return StartPullDialog(pullOnShow, remoteBranch, out errorOccurred);
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

        public bool StartViewPatchDialog(IWin32Window owner, string patchFile)
        {
            if (!InvokeEvent(owner, PreViewPatch))
                return true;

            using (var viewPatch = new FormViewPatch(this))
            {
                if (!String.IsNullOrEmpty(patchFile))
                    viewPatch.LoadPatch(patchFile);
                viewPatch.ShowDialog(owner);
            }

            InvokeEvent(owner, PostViewPatch);

            return true;
        }

        public bool StartViewPatchDialog(string patchFile)
        {
            return StartViewPatchDialog(null, patchFile);
        }

        public bool StartViewPatchDialog(IWin32Window owner)
        {
            return StartViewPatchDialog(owner, null);
        }

        public bool StartViewPatchDialog()
        {
            return StartViewPatchDialog(null, null);
        }

        public bool StartFormatPatchDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir(owner))
                return false;

            if (!InvokeEvent(owner, PreFormatPatch))
                return true;

            using (var form = new FormFormatPatch(this))
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
            if (!RequiresValidWorkingDir(owner))
                return false;

            if (!InvokeEvent(owner, PreStash))
                return true;

            using (var form = new FormStash(this))
                form.ShowDialog(owner);

            InvokeEvent(owner, PostStash);

            return true;
        }

        public bool StartStashDialog()
        {
            return StartStashDialog(null);
        }

        public bool StartResetChangesDialog(IWin32Window owner)
        {
            var unstagedFiles = Module.GetUnstagedFiles();
            // Show a form asking the user if they want to reset the changes.
            FormResetChanges.ActionEnum resetAction = FormResetChanges.ShowResetDialog(owner, unstagedFiles.Any(item => !item.IsNew), unstagedFiles.Any(item => item.IsNew));

            if (resetAction == FormResetChanges.ActionEnum.Cancel)
            {
                return false;
            }

            // Reset all changes.
            Module.ResetHard("");

            // Also delete new files, if requested.
            if (resetAction == FormResetChanges.ActionEnum.ResetAndDelete)
            {
                foreach (var item in unstagedFiles.Where(item => item.IsNew))
                {
                    try
                    {
                        string path = Path.Combine(Module.WorkingDir, item.Name);
                        if (File.Exists(path))
                            File.Delete(path);
                        else
                            Directory.Delete(path, true);
                    }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                }
            }

            return true;
        }

        public bool StartResetChangesDialog()
        {
            return StartResetChangesDialog(null);
        }

        public void StartRevertDialog(IWin32Window owner, string fileName)
        {
            using (var form = new FormRevert(this, fileName))
                form.ShowDialog(owner);
        }

        public void StartRevertDialog(string fileName)
        {
            StartRevertDialog(null, fileName);
        }

        public bool StartResolveConflictsDialog(IWin32Window owner, bool offerCommit)
        {
            if (!RequiresValidWorkingDir(owner))
                return false;

            if (!InvokeEvent(owner, PreResolveConflicts))
                return true;

            using (var form = new FormResolveConflicts(this, offerCommit))
                form.ShowDialog(owner);

            InvokeEvent(owner, PostResolveConflicts);

            return true;
        }

        public bool StartResolveConflictsDialog(IWin32Window owner)
        {
            return StartResolveConflictsDialog(owner, true);
        }

        public bool StartResolveConflictsDialog(bool offerCommit)
        {
            return StartResolveConflictsDialog(null, offerCommit);
        }

        public bool StartResolveConflictsDialog()
        {
            return StartResolveConflictsDialog(null, true);
        }

        public bool StartCherryPickDialog(IWin32Window owner, GitRevision revision)
        {
            // TODO: compare this code with StartArchiveDialog(...). Which one is to use?

            if (!RequiresValidWorkingDir(owner))
            {
                return false;
            }

            if (!InvokeEvent(owner, PreCherryPick))
            {
                return true;
            }

            using (var form = new FormCherryPick(this, revision))
            {
                if (form.ShowDialog(owner) == DialogResult.OK)
                {
                    InvokeEvent(owner, PostCherryPick);
                    return true;
                }
                return false;
            }
        }

        public bool StartCherryPickDialog(IWin32Window owner)
        {
            return StartCherryPickDialog(owner, null);
        }

        public bool StartCherryPickDialog()
        {
            return StartCherryPickDialog(null);
        }

        /// <summary>Start Merge dialog, using the specified branch.</summary>
        /// <param name="owner">Owner of the dialog.</param>
        /// <param name="branch">Branch to merge into the current branch.</param>
        public bool StartMergeBranchDialog(IWin32Window owner, string branch)
        {
            if (!RequiresValidWorkingDir(owner))
                return false;

            if (!InvokeEvent(owner, PreMergeBranch))
                return true;

            using (var form = new FormMergeBranch(this, branch))
                form.ShowDialog(owner);

            InvokeEvent(owner, PostMergeBranch);

            return true;
        }

        /// <summary>Start Merge dialog, using the specified branch.</summary>
        /// <param name="branch">Branch to merge into the current branch.</param>
        public bool StartMergeBranchDialog(string branch)
        {
            return StartMergeBranchDialog(null, branch);
        }

        public bool StartCreateTagDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir(owner))
                return false;

            if (!InvokeEvent(owner, PreCreateTag))
                return true;

            using (var form = new FormTag(this))
                form.ShowDialog(owner);

            InvokeEvent(owner, PostCreateTag);

            return true;
        }

        public bool StartCreateTagDialog()
        {
            return StartCreateTagDialog(null);
        }

        public bool StartDeleteTagDialog(IWin32Window owner, string tag)
        {
            if (!RequiresValidWorkingDir(owner))
                return false;

            if (!InvokeEvent(owner, PreDeleteTag))
                return true;

            using (var form = new FormDeleteTag(this, tag))
            {
                if (form.ShowDialog(owner) != DialogResult.OK)
                {
                    return false;
                }
            }

            InvokeEvent(owner, PostDeleteTag);

            return true;
        }

        public bool StartDeleteTagDialog(string tag)
        {
            return StartDeleteTagDialog(null, tag);
        }

        public bool StartDeleteTagDialog()
        {
            return StartDeleteTagDialog(null, "");
        }

        public bool StartEditGitIgnoreDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir(owner))
                return false;

            if (!InvokeEvent(owner, PreEditGitIgnore))
                return true;

            using (var form = new FormGitIgnore(this))
                form.ShowDialog(owner);

            InvokeEvent(owner, PostEditGitIgnore);

            return false;
        }

        public bool StartEditGitIgnoreDialog()
        {
            return StartEditGitIgnoreDialog(null);
        }

        public bool StartAddToGitIgnoreDialog(IWin32Window owner, string filePattern)
        {
            if (!RequiresValidWorkingDir(this))
                return false;

            try
            {
                if (!InvokeEvent(owner, PreEditGitIgnore))
                    return false;

                using (var frm = new FormAddToGitIgnore(this, filePattern))
                    frm.ShowDialog(owner);
            }
            finally
            {
                InvokeEvent(owner, PostEditGitIgnore);
            }

            return false;
        }

        public bool StartSettingsDialog(IWin32Window owner, SettingsPageReference initalPage = null)
        {
            if (!InvokeEvent(owner, PreSettings))
                return true;

            using (var form = new FormSettings(this, initalPage))
            {
                form.ShowDialog(owner);
            }

            InvokeEvent(owner, PostSettings);

            return true;
        }

        public bool StartSettingsDialog()
        {
            return StartSettingsDialog(null);
        }

        public bool StartArchiveDialog(IWin32Window owner, GitRevision revision)
        {
            return DoAction(owner, true, PreArchive, PostArchive, () =>
                {
                    using (var form = new FormArchive(this))
                    {
                        form.SelectedRevision = revision;
                        form.ShowDialog(owner);
                    }

                    return true;
                }
            );
        }

        public bool StartArchiveDialog(IWin32Window owner)
        {
            return StartArchiveDialog(owner, null);
        }

        public bool StartArchiveDialog()
        {
            return StartArchiveDialog(null);
        }

        public bool StartMailMapDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir(owner))
                return false;

            if (!InvokeEvent(owner, PreMailMap))
                return true;

            using (var form = new FormMailMap(this))
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
            if (!RequiresValidWorkingDir(owner))
                return false;

            if (!InvokeEvent(owner, PreVerifyDatabase))
                return true;

            using (var form = new FormVerify(this))
                form.ShowDialog(owner);

            InvokeEvent(owner, PostVerifyDatabase);

            return true;
        }

        public bool StartVerifyDatabaseDialog()
        {
            return StartVerifyDatabaseDialog(null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="preselectRemote">makes the FormRemotes initialially select the given remote</param>
        /// <returns></returns>
        public bool StartRemotesDialog(IWin32Window owner, string preselectRemote)
        {
            if (!RequiresValidWorkingDir(owner))
                return false;

            if (!InvokeEvent(owner, PreRemotes))
                return true;

            using (var form = new FormRemotes(this))
            {
                form.PreselectRemoteOnLoad = preselectRemote;
                form.ShowDialog(owner);
            }

            InvokeEvent(owner, PostRemotes);

            return true;
        }

        public bool StartRemotesDialog(IWin32Window owner)
        {
            return StartRemotesDialog(owner, null);
        }

        public bool StartRemotesDialog()
        {
            return StartRemotesDialog(null);
        }

        public bool StartRebaseDialog(IWin32Window owner, string branch)
        {
            if (!RequiresValidWorkingDir(owner))
                return false;

            if (!InvokeEvent(owner, PreRebase))
                return true;

            using (var form = new FormRebase(this, branch))
                form.ShowDialog(owner);

            InvokeEvent(owner, PostRebase);

            return true;
        }

        public bool StartRebaseDialog(IWin32Window owner, string from, string to, string onto)
        {
            if (!RequiresValidWorkingDir(owner))
                return false;

            if (!InvokeEvent(owner, PreRebase))
                return true;

            using (var form = new FormRebase(this, from, to, onto))
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
            if (!RequiresValidWorkingDir(owner))
                return false;

            if (!InvokeEvent(owner, PreRename))
                return true;

            using (var form = new FormRenameBranch(this, branch))
            {

                if (form.ShowDialog(owner) != DialogResult.OK)
                    return false;
            }

            InvokeEvent(owner, PostRename);

            return true;
        }

        public bool StartRebaseDialog(string branch)
        {
            return StartRebaseDialog(null, branch);
        }

        public bool StartSubmodulesDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir(owner))
                return false;

            if (!InvokeEvent(owner, PreSubmodulesEdit))
                return true;

            using (var form = new FormSubmodules(this))
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
            if (!RequiresValidWorkingDir(owner))
                return false;

            if (!InvokeEvent(owner, PreUpdateSubmodules))
                return true;

            FormProcess.ShowDialog(owner, Module, GitCommandHelpers.SubmoduleUpdateCmd(""));

            InvokeEvent(owner, PostUpdateSubmodules);

            return true;
        }

        public bool StartUpdateSubmodulesDialog()
        {
            return StartUpdateSubmodulesDialog(null);
        }

        public bool StartSyncSubmodulesDialog(IWin32Window owner)
        {
            if (!RequiresValidWorkingDir(owner))
                return false;

            if (!InvokeEvent(owner, PreSyncSubmodules))
                return true;

            FormProcess.ShowDialog(owner, Module, GitCommandHelpers.SubmoduleSyncCmd(""));

            InvokeEvent(owner, PostSyncSubmodules);

            return true;
        }

        public bool StartSyncSubmodulesDialog()
        {
            return StartSyncSubmodulesDialog(null);
        }

        public bool StartPluginSettingsDialog(IWin32Window owner)
        {
            return StartSettingsDialog(owner, PluginsSettingsGroup.GetPageReference());
        }

        public bool StartPluginSettingsDialog()
        {
            return StartPluginSettingsDialog(null);
        }

        public bool StartBrowseDialog(IWin32Window owner, string filter)
        {
            return DoAction(owner, false, PreBrowse, PostBrowse, () =>
                {
                    using (var form = new FormBrowse(this, filter))
                        form.ShowDialog(owner);

                    return true;
                }
            );
        }

        public bool StartBrowseDialog(string filter)
        {
            return StartBrowseDialog(null, filter);
        }

        public bool StartFileHistoryDialog(IWin32Window owner, string fileName, GitRevision revision, bool filterByRevision, bool showBlame)
        {
            if (!RequiresValidWorkingDir(owner))
                return false;

            if (!InvokeEvent(owner, PreFileHistory))
                return false;

            using (var form = new FormFileHistory(this, fileName, revision, filterByRevision))
            {
                if (showBlame)
                    form.SelectBlameTab();
                form.ShowDialog(owner);
            }

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

        public bool StartPushDialog()
        {
            return StartPushDialog(false);
        }

        public bool StartPushDialog(IWin32Window owner, bool pushOnShow, out bool pushCompleted)
        {
            pushCompleted = false;
            if (!RequiresValidWorkingDir(owner))
                return false;

            if (!InvokeEvent(owner, PrePush))
                return true;

            using (var form = new FormPush(this))
            {
                DialogResult dlgResult;
                if (pushOnShow)
                    dlgResult = form.PushAndShowDialogWhenFailed(owner);
                else
                    dlgResult = form.ShowDialog(owner);

                if (dlgResult == DialogResult.OK)
                    pushCompleted = !form.ErrorOccurred;
            }

            InvokeEvent(owner, PostPush);

            return true;
        }

        public bool StartPushDialog(IWin32Window owner, bool pushOnShow)
        {
            bool pushCompleted;
            return StartPushDialog(owner, pushOnShow, out pushCompleted);
        }

        public bool StartPushDialog(bool pushOnShow)
        {
            return StartPushDialog(null, pushOnShow);
        }

        public bool StartApplyPatchDialog(IWin32Window owner, string patchFile)
        {
            return DoAction(owner, true, PreApplyPatch, PostApplyPatch, () =>
                {
                    using (var form = new FormApplyPatch(this))
                    {
                        if (Directory.Exists(patchFile))
                            form.SetPatchDir(patchFile);
                        else
                            form.SetPatchFile(patchFile);
                        return form.ShowDialog(owner) != DialogResult.Cancel;
                    }
                }
            );
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
            if (!RequiresValidWorkingDir(owner))
                return false;

            if (!InvokeEvent(owner, PreEditGitAttributes))
                return true;

            using (var form = new FormGitAttributes(this))
            {
                form.ShowDialog(owner);
            }

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

        public GitModule Module { get; private set; }

        public IGitModule GitModule
        {
            get
            {
                return Module;
            }
        }

        private void InvokePostEvent(IWin32Window ownerForm, bool actionDone, GitUIPostActionEventHandler gitUIEventHandler)
        {

            if (gitUIEventHandler != null)
            {
                var e = new GitUIPostActionEventArgs(ownerForm, this, actionDone);
                gitUIEventHandler(this, e);
            }
        }

        internal bool InvokeEvent(object sender, IWin32Window ownerForm, GitUIEventHandler gitUIEventHandler)
        {
            try
            {
                var e = new GitUIEventArgs(ownerForm, this);
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
            return StartBlameDialog(owner, fileName, null, null);
        }

        private bool StartBlameDialog(IWin32Window owner, string fileName, GitRevision revision, List<string> children)
        {
            return DoAction(owner, true, PreBlame, PostBlame, () =>
                {
                    using (var frm = new FormBlame(this, fileName, revision, children))
                        frm.ShowDialog(owner);

                    return true;
                }
            );
        }

        public bool StartBlameDialog(string fileName)
        {
            return StartBlameDialog(null, fileName, null, null);
        }

        private bool StartBlameDialog(string fileName, GitRevision revision, List<string> children)
        {
            return StartBlameDialog(null, fileName, revision, children);
        }

        private void WrapRepoHostingCall(string name, IRepositoryHostPlugin gitHoster,
                                                Action<IRepositoryHostPlugin> call)
        {
            if (!gitHoster.ConfigurationOk)
            {
                var eventArgs = new GitUIEventArgs(null, this);
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

        public void StartCloneForkFromHoster(IWin32Window owner, IRepositoryHostPlugin gitHoster, GitModuleChangedEventHandler GitModuleChanged)
        {
            WrapRepoHostingCall("View pull requests", gitHoster, gh =>
            {
                using (var frm = new ForkAndCloneForm(gitHoster, GitModuleChanged)) frm.ShowDialog(owner);
            });
        }

        internal void StartPullRequestsDialog(IWin32Window owner, IRepositoryHostPlugin gitHoster)
        {
            WrapRepoHostingCall("View pull requests", gitHoster,
                                gh =>
                                {
                                    using (var frm = new ViewPullRequestsForm(this, gitHoster)) frm.ShowDialog(owner);
                                });
        }

        internal void StartPullRequestsDialog(IRepositoryHostPlugin gitHoster)
        {
            StartPullRequestsDialog(null, gitHoster);
        }

        public void StartCreatePullRequest(IWin32Window owner)
        {
            List<IRepositoryHostPlugin> relevantHosts =
                (from gh in RepoHosts.GitHosters where gh.GitModuleIsRelevantToMe(Module) select gh).ToList();
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
                                gh => new CreatePullRequestForm(this, gitHoster, chooseRemote, chooseBranch).Show(owner));
        }

        public void RunCommand(string[] args)
        {
            var arguments = InitializeArguments(args);

            if (args.Length <= 1)
                return;

            if (args[1].Equals("blame") && args.Length <= 2)
            {
                MessageBox.Show("Cannot open blame, there is no file selected.", "Blame");
                return;
            }
            if (args[1].Equals("difftool") && args.Length <= 2)
            {
                MessageBox.Show("Cannot open difftool, there is no file selected.", "Difftool");
                return;
            }
            if (args[1].Equals("filehistory") && args.Length <= 2)
            {
                MessageBox.Show("Cannot open file history, there is no file selected.", "File history");
                return;
            }
            if (args[1].Equals("fileeditor") && args.Length <= 2)
            {
                MessageBox.Show("Cannot open file editor, there is no file selected.", "File editor");
                return;
            }
            if (args[1].Equals("revert") && args.Length <= 2)
            {
                MessageBox.Show("Cannot open revert, there is no file selected.", "Revert");
                return;
            }

            RunCommandBasedOnArgument(args, arguments);
        }


        // Please update FormCommandlineHelp if you add or change commands
        private void RunCommandBasedOnArgument(string[] args, Dictionary<string, string> arguments)
        {
            switch (args[1])
            {
                case "about":
                    var frm = new AboutBox { StartPosition = FormStartPosition.CenterScreen };
                    Application.Run(frm);
                    return;
                case "add":
                case "addfiles":
                    StartAddFilesDialog(args.Length == 3 ? args[2] : ".");
                    return;
                case "apply":       // [filename]
                case "applypatch":
                    StartApplyPatchDialog(args.Length == 3 ? args[2] : "");
                    return;
                case "blame":       // filename
                    RunBlameCommand(args);
                    return;
                case "branch":
                    StartCreateBranchDialog();
                    return;
                case "browse":      // [path] [-filter]
                    RunBrowseCommand(args);
                    return;
                case "checkout":
                case "checkoutbranch":
                    StartCheckoutBranchDialog();
                    return;
                case "checkoutrevision":
                    StartCheckoutRevisionDialog();
                    return;
                case "cherry":
                    StartCherryPickDialog();
                    return;
                case "cleanup":
                    StartCleanupRepositoryDialog();
                    return;
                case "clone":       // [path]
                    RunCloneCommand(args);
                    return;
                case "commit":      // [--quiet]
                    Commit(arguments);
                    return;
                case "difftool":      // filename
                    Module.OpenWithDifftool(args[2]);
                    return;
                case "filehistory": // filename
                    RunFileHistoryCommand(args);
                    return;
                case "fileeditor":  // filename
                    RunFileEditorCommand(args);
                    return;
                case "formatpatch":
                    StartFormatPatchDialog();
                    return;
                case "gitbash":
                    Module.RunBash();
                    return;
                case "gitignore":
                    StartEditGitIgnoreDialog();
                    return;
                case "init":        // [path]
                    RunInitCommand(args);
                    return;
                case "merge":       // [--branch name]
                    RunMergeCommand(arguments);
                    return;
                case "mergeconflicts": // [--quiet]
                case "mergetool":
                    RunMergeToolOrConflictCommand(arguments);
                    return;
                case "openrepo":    // [path]
                    RunOpenRepoCommand(args);
                    return;
                case "pull":        //  [--rebase] [--merge] [--fetch] [--quiet] [--remotebranch name]
                    Pull(arguments);
                    return;
                case "push":        // [--quiet]
                    Push(arguments);
                    return;
                case "rebase":      // [--branch name]
                    RunRebaseCommand(arguments);
                    return;
                case "remotes":
                    StartRemotesDialog();
                    return;
                case "reset":
                    StartResetChangesDialog();
                    return;
                case "revert":      // filename
                    StartRevertDialog(args[2]);
                    return;
                case "searchfile":
                    RunSearchFileCommand();
                    return;
                case "settings":
                    StartSettingsDialog();
                    return;
                case "stash":
                    StartStashDialog();
                    return;
                case "synchronize": // [--rebase] [--merge] [--fetch] [--quiet]
                    RunSynchronizeCommand(arguments);
                    return;
                case "tag":
                    StartCreateTagDialog();
                    return;
                case "viewdiff":
                    StartCompareRevisionsDialog();
                    return;
                case "viewpatch":   // [filename]
                    StartViewPatchDialog(args.Length == 3 ? args[2] : "");
                    return;
                default:
                    if (args[1].StartsWith("git://") || args[1].StartsWith("http://") || args[1].StartsWith("https://"))
                    {
                        StartCloneDialog(null, args[1], true, null);
                        return;
                    }
                    if (args[1].StartsWith("github-windows://openRepo/"))
                    {
                        StartCloneDialog(null, args[1].Replace("github-windows://openRepo/", ""), true, null);
                        return;
                    }
                    break;
            }
            var frmCmdLine = new FormCommandlineHelp { StartPosition = FormStartPosition.CenterScreen };
            Application.Run(frmCmdLine);
        }

        private void RunMergeCommand(Dictionary<string, string> arguments)
        {
            string branch = null;
            if (arguments.ContainsKey("branch"))
                branch = arguments["branch"];
            StartMergeBranchDialog(branch);
        }

        private void RunSearchFileCommand()
        {
            var searchWindow = new SearchWindow<string>(FindFileMatches);
            Application.Run(searchWindow);
            if (searchWindow.SelectedItem != null)
                Console.WriteLine(Path.Combine(Module.WorkingDir, searchWindow.SelectedItem));
        }

        private void RunBrowseCommand(string[] args)
        {
            StartBrowseDialog(GetParameterOrEmptyStringAsDefault(args, "-filter"));
        }

        private static string GetParameterOrEmptyStringAsDefault(string[] args, string paramName)
        {
            for (int i = 2; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg.StartsWith(paramName + "="))
                {
                    return arg.Replace(paramName + "=", "");
                }
            }

            return string.Empty;
        }

        private void RunOpenRepoCommand(string[] args)
        {
            GitUICommands c = this;
            if (args.Length > 2)
            {
                if (File.Exists(args[2]))
                {
                    string path = File.ReadAllText(args[2]).Trim().Split(new char[] { '\n' }, 1).FirstOrDefault();
                    if (Directory.Exists(path))
                    {
                        c = new GitUICommands(path);
                    }
                }
            }

            c.StartBrowseDialog(GetParameterOrEmptyStringAsDefault(args, "-filter"));
        }

        private void RunSynchronizeCommand(Dictionary<string, string> arguments)
        {
            Commit(arguments);
            Pull(arguments);
            Push(arguments);
        }

        private void RunRebaseCommand(Dictionary<string, string> arguments)
        {
            string branch = null;
            if (arguments.ContainsKey("branch"))
                branch = arguments["branch"];
            StartRebaseDialog(branch);
        }

        private void RunFileEditorCommand(string[] args)
        {
            using (var formEditor = new FormEditor(this, args[2]))
            {
                if (formEditor.ShowDialog() == DialogResult.Cancel)
                    Environment.ExitCode = -1;
            }
        }

        private void RunFileHistoryCommand(string[] args)
        {
            //Remove working dir from filename. This is to prevent filenames that are too
            //long while there is room left when the workingdir was not in the path.
            string fileHistoryFileName = args[2].Replace(Module.WorkingDir, "").Replace('\\', '/');

            StartFileHistoryDialog(fileHistoryFileName);
        }

        private void RunCloneCommand(string[] args)
        {
            if (args.Length > 2)
                StartCloneDialog(args[2]);
            else
                StartCloneDialog();
        }

        private void RunInitCommand(string[] args)
        {
            if (args.Length > 2)
                StartInitializeDialog(args[2]);
            else
                StartInitializeDialog();
        }

        private void RunBlameCommand(string[] args)
        {
            // Remove working dir from filename. This is to prevent filenames that are too
            // long while there is room left when the workingdir was not in the path.
            string filenameFromBlame = args[2].Replace(Module.WorkingDir, "").Replace('\\', '/');
            StartBlameDialog(filenameFromBlame);
        }

        private void RunMergeToolOrConflictCommand(Dictionary<string, string> arguments)
        {
            if (!arguments.ContainsKey("quiet") || Module.InTheMiddleOfConflictedMerge())
                StartResolveConflictsDialog();
        }

        private static Dictionary<string, string> InitializeArguments(string[] args)
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>();

            for (int i = 2; i < args.Length; i++)
            {
                if (args[i].StartsWith("--") && i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                    arguments.Add(args[i].TrimStart('-'), args[++i]);
                else if (args[i].StartsWith("--"))
                    arguments.Add(args[i].TrimStart('-'), null);
            }
            return arguments;
        }

        private IList<string> FindFileMatches(string name)
        {
            var candidates = Module.GetFullTree("HEAD");

            string nameAsLower = name.ToLower();

            return candidates.Where(fileName => fileName.ToLower().Contains(nameAsLower)).ToList();
        }

        private void Commit(Dictionary<string, string> arguments)
        {
            StartCommitDialog(arguments.ContainsKey("quiet"));
        }

        private void Push(Dictionary<string, string> arguments)
        {
            StartPushDialog(arguments.ContainsKey("quiet"));
        }

        private void Pull(Dictionary<string, string> arguments)
        {
            UpdateSettingsBasedOnArguments(arguments);

            string remoteBranch = null;
            if (arguments.ContainsKey("remotebranch"))
                remoteBranch = arguments["remotebranch"];

            StartPullDialog(arguments.ContainsKey("quiet"), remoteBranch);
        }

        private static void UpdateSettingsBasedOnArguments(Dictionary<string, string> arguments)
        {
            if (arguments.ContainsKey("merge"))
                Settings.PullMerge = Settings.PullAction.Merge;
            if (arguments.ContainsKey("rebase"))
                Settings.PullMerge = Settings.PullAction.Rebase;
            if (arguments.ContainsKey("fetch"))
                Settings.PullMerge = Settings.PullAction.Fetch;
            if (arguments.ContainsKey("autostash"))
                Settings.AutoStash = true;
        }

        internal void RaisePreBrowseInitialize(IWin32Window owner)
        {
            InvokeEvent(owner, PreBrowseInitialize);
        }

        internal void RaisePostBrowseInitialize(IWin32Window owner)
        {
            InvokeEvent(owner, PostBrowseInitialize);
        }

        internal void RaisePostRegisterPlugin(IWin32Window owner)
        {
            InvokeEvent(owner, PostRegisterPlugin);
        }

        public void RaiseBrowseInitialize()
        {
            InvokeEvent(null, BrowseInitialize);
        }

        public IGitRemoteCommand CreateRemoteCommand()
        {
            return new GitRemoteCommand(Module);
        }

        private class GitRemoteCommand : IGitRemoteCommand
        {
            public object OwnerForm { get; set; }

            public string Remote { get; set; }

            public string Title { get; set; }

            public string CommandText { get; set; }

            public bool ErrorOccurred { get; private set; }

            public string CommandOutput { get; private set; }

            readonly GitModule Module;

            public event GitRemoteCommandCompletedEventHandler Completed;

            internal GitRemoteCommand(GitModule aModule)
            {
                Module = aModule;
            }

            public void Execute()
            {
                if (CommandText == null)
                    throw new InvalidOperationException("CommandText is required");

                using (var form = new FormRemoteProcess(Module, CommandText))
                {
                    if (Title != null)
                        form.Text = Title;
                    if (Remote != null)
                        form.Remote = Remote;

                    form.HandleOnExitCallback = HandleOnExit;

                    form.ShowDialog(OwnerForm as IWin32Window);

                    ErrorOccurred = form.ErrorOccurred();
                    CommandOutput = form.GetOutputString();
                }
            }

            private bool HandleOnExit(ref bool isError, FormProcess form)
            {
                CommandOutput = form.GetOutputString();

                var e = new GitRemoteCommandCompletedEventArgs(this, isError, false);

                if (Completed != null)
                    Completed(form, e);

                isError = e.IsError;

                return e.Handled;
            }
        }

        public override bool Equals(object obj)
        {
            GitUICommands other = obj as GitUICommands;
            return (other != null) && Equals(other);
        }

        bool Equals(GitUICommands other)
        {
            return Equals(Module, other.Module);
        }

        public override int GetHashCode()
        {
            return Module.GetHashCode();
        }
    }
}
