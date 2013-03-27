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
            RepoChangedNotifier = new ActionNotifier(
                () =>
                {
                    InvokeEvent(null, PostRepositoryChanged);
                }
            );
            Notifications = NotificationManager.Get(this);
        }

        public GitUICommands(string workingDir)
            : this(new GitModule(workingDir)) { }

        #region IGitUICommands Members

        public event GitUIEventHandler PreBrowse;
        public event GitUIEventHandler PostBrowse;

        public event GitUIEventHandler PreDeleteBranch;
        public event GitUIPostActionEventHandler PostDeleteBranch;

        public event GitUIEventHandler PreCheckoutRevision;
        public event GitUIPostActionEventHandler PostCheckoutRevision;

        public event GitUIEventHandler PreCheckoutBranch;
        public event GitUIPostActionEventHandler PostCheckoutBranch;

        public event GitUIEventHandler PreFileHistory;
        public event GitUIPostActionEventHandler PostFileHistory;

        public event GitUIEventHandler PreCompareRevisions;
        public event GitUIPostActionEventHandler PostCompareRevisions;

        public event GitUIEventHandler PreAddFiles;
        public event GitUIPostActionEventHandler PostAddFiles;

        public event GitUIEventHandler PreCreateBranch;
        public event GitUIPostActionEventHandler PostCreateBranch;

        public event GitUIEventHandler PreClone;
        public event GitUIPostActionEventHandler PostClone;

        public event GitUIEventHandler PreSvnClone;
        public event GitUIPostActionEventHandler PostSvnClone;

        public event GitUIEventHandler PreCommit;
        public event GitUIPostActionEventHandler PostCommit;

        public event GitUIEventHandler PreSvnDcommit;
        public event GitUIPostActionEventHandler PostSvnDcommit;

        public event GitUIEventHandler PreSvnRebase;
        public event GitUIPostActionEventHandler PostSvnRebase;

        public event GitUIEventHandler PreSvnFetch;
        public event GitUIPostActionEventHandler PostSvnFetch;

        public event GitUIEventHandler PreInitialize;
        public event GitUIPostActionEventHandler PostInitialize;

        public event GitUIEventHandler PrePush;
        public event GitUIPostActionEventHandler PostPush;

        public event GitUIEventHandler PrePull;
        public event GitUIPostActionEventHandler PostPull;

        public event GitUIEventHandler PreViewPatch;
        public event GitUIPostActionEventHandler PostViewPatch;

        public event GitUIEventHandler PreApplyPatch;
        public event GitUIPostActionEventHandler PostApplyPatch;

        public event GitUIEventHandler PreFormatPatch;
        public event GitUIPostActionEventHandler PostFormatPatch;

        public event GitUIEventHandler PreStash;
        public event GitUIPostActionEventHandler PostStash;

        public event GitUIEventHandler PreResolveConflicts;
        public event GitUIPostActionEventHandler PostResolveConflicts;

        public event GitUIEventHandler PreCherryPick;
        public event GitUIPostActionEventHandler PostCherryPick;

        public event GitUIEventHandler PreMergeBranch;
        public event GitUIPostActionEventHandler PostMergeBranch;

        public event GitUIEventHandler PreCreateTag;
        public event GitUIPostActionEventHandler PostCreateTag;

        public event GitUIEventHandler PreDeleteTag;
        public event GitUIPostActionEventHandler PostDeleteTag;

        public event GitUIEventHandler PreEditGitIgnore;
        public event GitUIPostActionEventHandler PostEditGitIgnore;

        public event GitUIEventHandler PreSettings;
        public event GitUIPostActionEventHandler PostSettings;

        public event GitUIEventHandler PreArchive;
        public event GitUIPostActionEventHandler PostArchive;

        public event GitUIEventHandler PreMailMap;
        public event GitUIPostActionEventHandler PostMailMap;

        public event GitUIEventHandler PreVerifyDatabase;
        public event GitUIPostActionEventHandler PostVerifyDatabase;

        public event GitUIEventHandler PreRemotes;
        public event GitUIPostActionEventHandler PostRemotes;

        public event GitUIEventHandler PreRebase;
        public event GitUIPostActionEventHandler PostRebase;

        public event GitUIEventHandler PreRename;
        public event GitUIPostActionEventHandler PostRename;

        public event GitUIEventHandler PreSubmodulesEdit;
        public event GitUIPostActionEventHandler PostSubmodulesEdit;

        public event GitUIEventHandler PreUpdateSubmodules;
        public event GitUIPostActionEventHandler PostUpdateSubmodules;

        public event GitUIEventHandler PreSyncSubmodules;
        public event GitUIPostActionEventHandler PostSyncSubmodules;

        public event GitUIEventHandler PreBlame;
        public event GitUIPostActionEventHandler PostBlame;

        public event GitUIEventHandler PreEditGitAttributes;
        public event GitUIPostActionEventHandler PostEditGitAttributes;

        public event GitUIEventHandler PreBrowseInitialize;
        public event GitUIEventHandler PostBrowseInitialize;
        /// <summary>
        /// listeners for changes being made to repository
        /// </summary>
        public event GitUIEventHandler PostRepositoryChanged;

        public event GitUIEventHandler PostRegisterPlugin;

        public ILockableNotifier RepoChangedNotifier { get; private set; }
        public IBrowseRepo BrowseRepo { get; set; }

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
            bool executed;

            if (cmd.AccessesRemote())
                executed = FormRemoteProcess.ShowDialog(parentForm, Module, cmd.ToLine());
            else
                executed = FormProcess.ShowDialog(parentForm, Module, cmd.ToLine());

            if (executed && cmd.ChangesRepoState())
                RepoChangedNotifier.Notify();

            return executed;
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
            return DoActionOnRepo(owner, true, false, PreDeleteBranch, PostDeleteBranch, () =>
                {
                    using (var form = new FormDeleteBranch(this, branch))
                        form.ShowDialog(owner);
                    return true;
                }
            );
        }

        public bool StartDeleteBranchDialog(string branch)
        {
            return StartDeleteBranchDialog(null, branch);
        }

        public bool StartCheckoutRevisionDialog(IWin32Window owner)
        {
            return DoActionOnRepo(owner, true, true, PreCheckoutRevision, PostCheckoutRevision, () =>
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
        /// 
        /// </summary>
        /// <param name="requiresValidWorkingDir">If action requires valid working directory</param>
        /// <param name="owner">Owner window</param>
        /// <param name="changesRepo">if successfuly done action changes repo state</param>
        /// <param name="preEvent">Event invoked before performing action</param>
        /// <param name="postEvent">Event invoked after performing action</param>
        /// <param name="action">Action to do</param>
        /// <returns>true if action was done, false otherwise</returns>
        public bool DoActionOnRepo(IWin32Window owner, bool requiresValidWorkingDir, bool changesRepo, 
            GitUIEventHandler preEvent, GitUIPostActionEventHandler postEvent, Func<bool> action)
        {
            bool actionDone = false;            
            RepoChangedNotifier.Lock();
            try
            {
                if (requiresValidWorkingDir && !RequiresValidWorkingDir(owner))
                    return false;

                if (preEvent != null)
                    if (!InvokeEvent(owner, preEvent))
                        return false;
                try
                {
                    actionDone = action();
                }
                finally
                {
                    if (postEvent != null)
                        InvokePostEvent(owner, actionDone, postEvent);
                }
            }
            finally
            {
                RepoChangedNotifier.UnLock(changesRepo && actionDone);
            }

            return actionDone;
        }

        public void DoActionOnRepo(Action action) 
        {
            Func<bool> fnc = () =>
                {
                    action();
                    return true;
                };
            DoActionOnRepo(null, false, false, null, null, fnc);
        }

        public void DoActionOnRepo(Func<bool> action)
        {
            DoActionOnRepo(null, false, true, null, null, action);
        }

        #region Checkout

        public bool StartCheckoutBranchDialog(IWin32Window owner, string branch, bool remote, string containRevison)
        {
            return DoActionOnRepo(owner, true, true, PreCheckoutBranch, PostCheckoutBranch, () =>
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

        public bool StartCheckoutBranchDialog(string branch, bool remote)
        {            
            return StartCheckoutBranchDialog(null, branch, remote);
        }

        #endregion Checkout

        public bool StartCompareRevisionsDialog(IWin32Window owner)
        {
            Func<bool> action = () =>
            {
                using (var form = new FormDiff(this))
                   form.ShowDialog(owner);
                return true;
            };

            return DoActionOnRepo(owner, true, false, PreCompareRevisions, PostCompareRevisions, action);
        }

        public bool StartCompareRevisionsDialog()
        {
            return StartCompareRevisionsDialog(null);
        }

        public bool StartAddFilesDialog(IWin32Window owner, string addFiles)
        {
            return DoActionOnRepo(owner, true, true, PreAddFiles, PostAddFiles, () =>
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
            Func<bool> action = () =>
            {
                using (var form = new FormBranch(this))
                    form.ShowDialog(owner);
                return true;
            };

            return DoActionOnRepo(owner, true, false, PreCreateBranch, PostCreateBranch, action);
        }

        public bool StartCreateBranchDialog()
        {
            return StartCreateBranchDialog(null);
        }

        public bool StartCloneDialog(IWin32Window owner, string url, bool openedFromProtocolHandler, GitModuleChangedEventHandler GitModuleChanged)
        {
            Func<bool> action = () =>
            {
                using (var form = new FormClone(this, url, openedFromProtocolHandler, GitModuleChanged))
                    form.ShowDialog(owner);
                return true;
            };

            return DoActionOnRepo(owner, false, false, PreClone, PostClone, action);
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
            Func<bool> action = () =>
            {
                using (var form = new FormSvnClone(this, GitModuleChanged))
                    form.ShowDialog(owner);
                return true;
            };

            return DoActionOnRepo(owner, false, false, PreSvnClone, PostSvnClone, action);
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
            Func<bool> action = () =>
            {
                using (var form = new FormCommit(this))
                {
                    if (showOnlyWhenChanges)
                        form.ShowDialogWhenChanges(owner);
                    else
                        form.ShowDialog(owner);
                }
                return true;
            };

            return DoActionOnRepo(owner, true, false, PreCommit, PostCommit, action);
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
            Func<bool> action = () =>
            {
                return FormProcess.ShowDialog(owner, Module, Settings.GitCommand, GitSvnCommandHelpers.DcommitCmd());
            };

            return DoActionOnRepo(owner, true, true, PreSvnDcommit, PostSvnDcommit, action);
        }

        public bool StartSvnDcommitDialog()
        {
            return StartSvnDcommitDialog(null);
        }

        public bool StartSvnRebaseDialog(IWin32Window owner)
        {
            Func<bool> action = () =>
            {
                FormProcess.ShowDialog(owner, Module, Settings.GitCommand, GitSvnCommandHelpers.RebaseCmd());
                return true;
            };

            return DoActionOnRepo(owner, true, true, PreSvnRebase, PostSvnRebase, action);
        }

        public bool StartSvnRebaseDialog()
        {
            return StartSvnRebaseDialog(null);
        }

        public bool StartSvnFetchDialog(IWin32Window owner)
        {
            Func<bool> action = () =>
            {
                return FormProcess.ShowDialog(owner, Module, Settings.GitCommand, GitSvnCommandHelpers.FetchCmd());
            };

            return DoActionOnRepo(owner, true, true, PreSvnFetch, PostSvnFetch, action);
        }

        public bool StartSvnFetchDialog()
        {
            return StartSvnFetchDialog(null);
        }

        public bool StartInitializeDialog(IWin32Window owner, GitModuleChangedEventHandler GitModuleChanged)
        {
            return StartInitializeDialog(owner, null, GitModuleChanged);
        }

        public bool StartInitializeDialog()
        {
            return StartInitializeDialog((IWin32Window)null, null);
        }

        public bool StartInitializeDialog(IWin32Window owner, string dir, GitModuleChangedEventHandler GitModuleChanged)
        {
            Func<bool> action = () =>
            {
                if (dir == null)
                    dir = Module.ValidWorkingDir() ? Module.WorkingDir : string.Empty;
                using (var frm = new FormInit(dir, GitModuleChanged)) frm.ShowDialog(owner);
                return true;
            };

            return DoActionOnRepo(owner, false, true, PreInitialize, PostInitialize, action);
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
            var pulled = false;

            Func<bool> action = () =>
            {
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
                        pulled = !formPull.ErrorOccurred;
                    }

                    return dlgResult == DialogResult.OK;
                }
            };

            bool done = DoActionOnRepo(owner, true, true, PrePull, PostPull, action);

            pullCompleted = pulled;

            return done;
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
            Func<bool> action = () =>
            {
                using (var viewPatch = new FormViewPatch(this))
                {
                    if (!String.IsNullOrEmpty(patchFile))
                        viewPatch.LoadPatch(patchFile);
                    viewPatch.ShowDialog(owner);
                }
                return true;
            };

            return DoActionOnRepo(owner, false, false, PreViewPatch, PostViewPatch, action);
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
            Func<bool> action = () =>
            {
                using (var form = new FormFormatPatch(this))
                    form.ShowDialog(owner);

                return true;
            };

            return DoActionOnRepo(owner, true, false, PreFormatPatch, PostFormatPatch, action);
        }

        public bool StartFormatPatchDialog()
        {
            return StartFormatPatchDialog(null);
        }

        public bool StartStashDialog(IWin32Window owner)
        {
            Func<bool> action = () =>
            {
                using (var form = new FormStash(this))
                    form.ShowDialog(owner);

                return true;
            };

            return DoActionOnRepo(owner, true, false, PreStash, PostStash, action);
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
            Func<bool> action = () =>
            {
                using (var form = new FormResolveConflicts(this, offerCommit))
                    form.ShowDialog(owner);

                return true;
            };

            return DoActionOnRepo(owner, true, true, PreResolveConflicts, PostResolveConflicts, action);
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
            Func<bool> action = () =>
            {
                using (var form = new FormCherryPick(this, revision))
                {
                    return form.ShowDialog(owner) == DialogResult.OK;
                }
            };

            return DoActionOnRepo(owner, true, true, PreCherryPick, PostCherryPick, action);
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

            Func<bool> action = () =>
            {
                using (var form = new FormMergeBranch(this, branch))
                    form.ShowDialog(owner);

                return true;
            };

            return DoActionOnRepo(owner, true, false, PreMergeBranch, PostMergeBranch, action);
        }

        /// <summary>Start Merge dialog, using the specified branch.</summary>
        /// <param name="branch">Branch to merge into the current branch.</param>
        public bool StartMergeBranchDialog(string branch)
        {
            return StartMergeBranchDialog(null, branch);
        }

        public bool StartCreateTagDialog(IWin32Window owner)
        {
            Func<bool> action = () =>
            {
                using (var form = new FormTag(this))
                    form.ShowDialog(owner);

                return true;
            };

            return DoActionOnRepo(owner, true, false, PreCreateTag, PostCreateTag, action);
        }

        public bool StartCreateTagDialog()
        {
            return StartCreateTagDialog(null);
        }

        public bool StartDeleteTagDialog(IWin32Window owner, string tag)
        {
            Func<bool> action = () =>
            {
                using (var form = new FormDeleteTag(this, tag))
                {
                    return form.ShowDialog(owner) == DialogResult.OK;
                }

            };

            return DoActionOnRepo(owner, true, true, PreDeleteTag, PostDeleteTag, action);
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
            Func<bool> action = () =>
            {
                using (var form = new FormGitIgnore(this))
                    form.ShowDialog(owner);

                return true;
            };

            return DoActionOnRepo(owner, true, false, PreEditGitIgnore, PostEditGitIgnore, action);
        }

        public bool StartEditGitIgnoreDialog()
        {
            return StartEditGitIgnoreDialog(null);
        }

        public bool StartAddToGitIgnoreDialog(IWin32Window owner, string filePattern)
        {

            Func<bool> action = () =>
            {
                using (var frm = new FormAddToGitIgnore(this, filePattern))
                    frm.ShowDialog(owner);

                return true;
            };

            return DoActionOnRepo(owner, true, false, PreEditGitIgnore, PostEditGitIgnore, action);
        }

        public bool StartSettingsDialog(IWin32Window owner, SettingsPageReference initalPage = null)
        {
            Func<bool> action = () =>
            {
                using (var form = new FormSettings(this, initalPage))
                {
                    form.ShowDialog(owner);
                }

                return true;
            };

            return DoActionOnRepo(owner, true, true, PreSettings, PostSettings, action);
        }

        public bool StartSettingsDialog()
        {
            return StartSettingsDialog(null);
        }

        public bool StartArchiveDialog(IWin32Window owner, GitRevision revision)
        {
            return DoActionOnRepo(owner, true, false, PreArchive, PostArchive, () =>
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
            Func<bool> action = () =>
            {
                using (var form = new FormMailMap(this))
                    form.ShowDialog(owner);

                return true;
            };

            return DoActionOnRepo(owner, true, false, PreMailMap, PostMailMap, action);
        }

        public bool StartMailMapDialog()
        {
            return StartMailMapDialog(null);
        }

        public bool StartVerifyDatabaseDialog(IWin32Window owner)
        {
            Func<bool> action = () =>
            {
                using (var form = new FormVerify(this))
                    form.ShowDialog(owner);
                
                return true;
            };

            //TODO: move Notify to FormVerify and friends
            return DoActionOnRepo(owner, true, true, PreVerifyDatabase, PostVerifyDatabase, action);
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
            Func<bool> action = () =>
            {
                using (var form = new FormRemotes(this))
                {
                    form.PreselectRemoteOnLoad = preselectRemote;
                    form.ShowDialog(owner);
                }

                return true;
            };

            return DoActionOnRepo(owner, true, true, PreRemotes, PostRemotes, action);
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
            return StartRebaseDialog(owner, string.Empty, null, branch);
        }

        public bool StartRebaseDialog(IWin32Window owner, string from, string to, string onto)
        {
            Func<bool> action = () =>
            {
                using (var form = new FormRebase(this, from, to, onto))
                    form.ShowDialog(owner);

                return true;
            };

            return DoActionOnRepo(owner, true, true, PreRebase, PostRebase, action);
        }


        public bool StartRenameDialog(string branch)
        {
            return StartRenameDialog(null, branch);
        }

        public bool StartRenameDialog(IWin32Window owner, string branch)
        {
            Func<bool> action = () =>
            {
                using (var form = new FormRenameBranch(this, branch))
                {

                    return form.ShowDialog(owner) == DialogResult.OK;
                }

            };

            return DoActionOnRepo(owner, true, true, PreRename, PostRename, action);
        }

        public bool StartRebaseDialog(string branch)
        {
            return StartRebaseDialog(null, branch);
        }

        public bool StartSubmodulesDialog(IWin32Window owner)
        {
            Func<bool> action = () =>
            {
                using (var form = new FormSubmodules(this))
                    form.ShowDialog(owner);

                return true;
            };

            return DoActionOnRepo(owner, true, true, PreSubmodulesEdit, PostSubmodulesEdit, action);
        }

        public bool StartSubmodulesDialog()
        {
            return StartSubmodulesDialog(null);
        }

        public bool StartUpdateSubmodulesDialog(IWin32Window owner)
        {
            Func<bool> action = () =>
            {
                return FormProcess.ShowDialog(owner, Module, GitCommandHelpers.SubmoduleUpdateCmd(""));
            };

            return DoActionOnRepo(owner, true, true, PreUpdateSubmodules, PostUpdateSubmodules, action);
        }

        public bool StartUpdateSubmodulesDialog()
        {
            return StartUpdateSubmodulesDialog(null);
        }

        public bool StartSyncSubmodulesDialog(IWin32Window owner)
        {
            Func<bool> action = () =>
            {
                return FormProcess.ShowDialog(owner, Module, GitCommandHelpers.SubmoduleSyncCmd(""));
            };

            return DoActionOnRepo(owner, true, true, PreSyncSubmodules, PostSyncSubmodules, action);
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
            if (!InvokeEvent(owner, PreBrowse))
                return false;

            using (var form = new FormBrowse(this, filter))
                form.ShowDialog(owner);

            InvokeEvent(owner, PostBrowse);

            return true;
        }

        public bool StartBrowseDialog(string filter)
        {
            return StartBrowseDialog(null, filter);
        }

        public bool StartFileHistoryDialog(IWin32Window owner, string fileName, GitRevision revision, bool filterByRevision, bool showBlame)
        {
            Func<bool> action = () =>
                {
                    using (var form = new FormFileHistory(this, fileName, revision, filterByRevision))
                    {
                        if (showBlame)
                            form.SelectBlameTab();
                        form.ShowDialog(owner);
                    }
                    return true;
                };

            return DoActionOnRepo(owner, true, false, PreFileHistory, PostFileHistory, action);
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
            bool pushed = false;

            Func<bool> action = () =>
            {
                using (var form = new FormPush(this))
                {
                    DialogResult dlgResult;
                    if (pushOnShow)
                        dlgResult = form.PushAndShowDialogWhenFailed(owner);
                    else
                        dlgResult = form.ShowDialog(owner);

                    if (dlgResult == DialogResult.OK)
                        pushed = !form.ErrorOccurred;

                    return dlgResult == DialogResult.OK;
                }                
            };

            bool done = DoActionOnRepo(owner, true, true, PrePush, PostPush, action);

            pushCompleted = pushed;

            return done;
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
            return DoActionOnRepo(owner, true, false, PreApplyPatch, PostApplyPatch, () =>
                {
                    using (var form = new FormApplyPatch(this))
                    {
                        if (Directory.Exists(patchFile))
                            form.SetPatchDir(patchFile);
                        else
                            form.SetPatchFile(patchFile);
                        
                        form.ShowDialog(owner);

                        return true;
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
            Func<bool> action = () =>
            {
                using (var form = new FormGitAttributes(this))
                {
                    form.ShowDialog(owner);
                }
                return true;
            };

            return DoActionOnRepo(owner, true, false, PreEditGitAttributes, PostEditGitAttributes, action);
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
            return DoActionOnRepo(owner, true, false, PreBlame, PostBlame, () =>
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
                    if (Module.WorkingDir.TrimEnd('\\') == Path.GetFullPath(args[2]))
                        Module = Module.SuperprojectModule;
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

        public void BrowseGoToRevision(string revision)
        {
            if (BrowseRepo != null)
                BrowseRepo.GoToRevision(revision);
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
