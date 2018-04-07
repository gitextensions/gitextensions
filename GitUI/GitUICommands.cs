using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Settings;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.RepoHosting;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.RepositoryHosts;
using Gravatar;
using JetBrains.Annotations;

namespace GitUI
{
    /// <summary>Contains methods to invoke GitEx forms, dialogs, etc.</summary>
    public sealed class GitUICommands : IGitUICommands
    {
        private readonly IAvatarService _gravatarService;
        private readonly ICommitTemplateManager _commitTemplateManager;
        private readonly IFullPathResolver _fullPathResolver;
        private readonly IFindFilePredicateProvider _fildFilePredicateProvider;

        public GitUICommands(GitModule module)
        {
            Module = module;
            _commitTemplateManager = new CommitTemplateManager(module);
            RepoChangedNotifier = new ActionNotifier(
                () => InvokeEvent(null, PostRepositoryChanged));

            IImageCache avatarCache = new DirectoryImageCache(AppSettings.GravatarCachePath, AppSettings.AuthorImageCacheDays);
            _gravatarService = new GravatarService(avatarCache);
            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
            _fildFilePredicateProvider = new FindFilePredicateProvider();
        }

        public GitUICommands(string workingDir)
            : this(new GitModule(workingDir))
        {
        }

        #region IGitUICommands Members

        public event EventHandler<GitUIBaseEventArgs> PreBrowse;
        public event EventHandler<GitUIBaseEventArgs> PostBrowse;

        public event EventHandler<GitUIBaseEventArgs> PreDeleteBranch;
        public event EventHandler<GitUIPostActionEventArgs> PostDeleteBranch;

        public event EventHandler<GitUIBaseEventArgs> PreDeleteRemoteBranch;
        public event EventHandler<GitUIPostActionEventArgs> PostDeleteRemoteBranch;

        public event EventHandler<GitUIBaseEventArgs> PreCheckoutRevision;
        public event EventHandler<GitUIPostActionEventArgs> PostCheckoutRevision;

        public event EventHandler<GitUIBaseEventArgs> PreCheckoutBranch;
        public event EventHandler<GitUIPostActionEventArgs> PostCheckoutBranch;

        public event EventHandler<GitUIBaseEventArgs> PreFileHistory;
        public event EventHandler<GitUIPostActionEventArgs> PostFileHistory;

        public event EventHandler<GitUIBaseEventArgs> PreCompareRevisions;
        public event EventHandler<GitUIPostActionEventArgs> PostCompareRevisions;

        public event EventHandler<GitUIBaseEventArgs> PreAddFiles;
        public event EventHandler<GitUIPostActionEventArgs> PostAddFiles;

        public event EventHandler<GitUIBaseEventArgs> PreCreateBranch;
        public event EventHandler<GitUIPostActionEventArgs> PostCreateBranch;

        public event EventHandler<GitUIBaseEventArgs> PreClone;
        public event EventHandler<GitUIPostActionEventArgs> PostClone;

        public event EventHandler<GitUIBaseEventArgs> PreCommit;
        public event EventHandler<GitUIPostActionEventArgs> PostCommit;

        public event EventHandler<GitUIBaseEventArgs> PreInitialize;
        public event EventHandler<GitUIPostActionEventArgs> PostInitialize;

        public event EventHandler<GitUIBaseEventArgs> PrePush;
        public event EventHandler<GitUIPostActionEventArgs> PostPush;

        public event EventHandler<GitUIBaseEventArgs> PrePull;
        public event EventHandler<GitUIPostActionEventArgs> PostPull;

        public event EventHandler<GitUIBaseEventArgs> PreViewPatch;
        public event EventHandler<GitUIPostActionEventArgs> PostViewPatch;

        public event EventHandler<GitUIBaseEventArgs> PreApplyPatch;
        public event EventHandler<GitUIPostActionEventArgs> PostApplyPatch;

        public event EventHandler<GitUIBaseEventArgs> PreFormatPatch;
        public event EventHandler<GitUIPostActionEventArgs> PostFormatPatch;

        public event EventHandler<GitUIBaseEventArgs> PreStash;
        public event EventHandler<GitUIPostActionEventArgs> PostStash;

        public event EventHandler<GitUIBaseEventArgs> PreResolveConflicts;
        public event EventHandler<GitUIPostActionEventArgs> PostResolveConflicts;

        public event EventHandler<GitUIBaseEventArgs> PreCherryPick;
        public event EventHandler<GitUIPostActionEventArgs> PostCherryPick;

        public event EventHandler<GitUIBaseEventArgs> PreRevertCommit;
        public event EventHandler<GitUIPostActionEventArgs> PostRevertCommit;

        public event EventHandler<GitUIBaseEventArgs> PreMergeBranch;
        public event EventHandler<GitUIPostActionEventArgs> PostMergeBranch;

        public event EventHandler<GitUIBaseEventArgs> PreCreateTag;
        public event EventHandler<GitUIPostActionEventArgs> PostCreateTag;

        public event EventHandler<GitUIBaseEventArgs> PreDeleteTag;
        public event EventHandler<GitUIPostActionEventArgs> PostDeleteTag;

        public event EventHandler<GitUIBaseEventArgs> PreEditGitIgnore;
        public event EventHandler<GitUIPostActionEventArgs> PostEditGitIgnore;

        public event EventHandler<GitUIBaseEventArgs> PreSettings;
        public event EventHandler<GitUIPostActionEventArgs> PostSettings;

        public event EventHandler<GitUIBaseEventArgs> PreArchive;
        public event EventHandler<GitUIPostActionEventArgs> PostArchive;

        public event EventHandler<GitUIBaseEventArgs> PreMailMap;
        public event EventHandler<GitUIPostActionEventArgs> PostMailMap;

        public event EventHandler<GitUIBaseEventArgs> PreVerifyDatabase;
        public event EventHandler<GitUIPostActionEventArgs> PostVerifyDatabase;

        public event EventHandler<GitUIBaseEventArgs> PreRemotes;
        public event EventHandler<GitUIPostActionEventArgs> PostRemotes;

        public event EventHandler<GitUIBaseEventArgs> PreRebase;
        public event EventHandler<GitUIPostActionEventArgs> PostRebase;

        public event EventHandler<GitUIBaseEventArgs> PreRename;
        public event EventHandler<GitUIPostActionEventArgs> PostRename;

        public event EventHandler<GitUIBaseEventArgs> PreSubmodulesEdit;
        public event EventHandler<GitUIPostActionEventArgs> PostSubmodulesEdit;

        public event EventHandler<GitUIBaseEventArgs> PreUpdateSubmodules;
        public event EventHandler<GitUIPostActionEventArgs> PostUpdateSubmodules;

        public event EventHandler<GitUIBaseEventArgs> PreSyncSubmodules;
        public event EventHandler<GitUIPostActionEventArgs> PostSyncSubmodules;

        public event EventHandler<GitUIBaseEventArgs> PreBlame;
        public event EventHandler<GitUIPostActionEventArgs> PostBlame;

        public event EventHandler<GitUIBaseEventArgs> PreEditGitAttributes;
        public event EventHandler<GitUIPostActionEventArgs> PostEditGitAttributes;

        public event EventHandler<GitUIBaseEventArgs> PreBrowseInitialize;
        public event EventHandler<GitUIBaseEventArgs> PostBrowseInitialize;

        public event EventHandler<GitUIBaseEventArgs> PreSparseWorkingCopy;
        public event EventHandler<GitUIPostActionEventArgs> PostSparseWorkingCopy;

        /// <summary>
        /// listeners for changes being made to repository
        /// </summary>
        public event EventHandler<GitUIBaseEventArgs> PostRepositoryChanged;

        public event EventHandler<GitUIBaseEventArgs> PostRegisterPlugin;

        public ILockableNotifier RepoChangedNotifier { get; }
        public IBrowseRepo BrowseRepo { get; set; }

        #endregion

        public string GitCommand(string arguments)
        {
            return Module.RunGitCmd(arguments);
        }

        public async Task<string> CommandLineCommandAsync(string cmd, string arguments)
        {
            return await Module.RunCmdAsync(cmd, arguments).ConfigureAwait(false);
        }

        private bool RequiresValidWorkingDir(object owner)
        {
            if (!Module.IsValidGitWorkingDir())
            {
                MessageBoxes.NotValidGitDirectory(owner as IWin32Window);
                return false;
            }

            return true;
        }

        public void CacheAvatar(string email)
        {
            _gravatarService.GetAvatarAsync(email, AppSettings.AuthorImageSize, AppSettings.GravatarDefaultImageType);
        }

        public Icon FormIcon => GitExtensionsForm.ApplicationIcon;

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

        public bool StartCommandLineProcessDialog(IGitCommand cmd, IWin32Window parentForm)
        {
            var executed = cmd.AccessesRemote()
                ? FormRemoteProcess.ShowDialog(parentForm, Module, cmd.ToLine())
                : FormProcess.ShowDialog(parentForm, Module, cmd.ToLine());

            if (executed && cmd.ChangesRepoState())
            {
                RepoChangedNotifier.Notify();
            }

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
                    {
                        form.ShowDialog(owner);
                    }

                    return true;
                });
        }

        public bool StartDeleteRemoteBranchDialog(IWin32Window owner, string remoteBranch)
        {
            return DoActionOnRepo(owner, true, false, PreDeleteRemoteBranch, PostDeleteRemoteBranch, () =>
                {
                    using (var form = new FormDeleteRemoteBranch(this, remoteBranch))
                    {
                        form.ShowDialog(owner);
                    }

                    return true;
                });
        }

        public bool StartDeleteBranchDialog(string branch)
        {
            return StartDeleteBranchDialog(null, branch);
        }

        public bool StartCheckoutRevisionDialog(IWin32Window owner, string revision = null)
        {
            return DoActionOnRepo(owner, true, true, PreCheckoutRevision, PostCheckoutRevision, () =>
                {
                    using (var form = new FormCheckoutRevision(this))
                    {
                        form.SetRevision(revision);
                        return form.ShowDialog(owner) == DialogResult.OK;
                    }
                });
        }

        public bool StartCheckoutRevisionDialog()
        {
            return StartCheckoutRevisionDialog(null);
        }

        public bool StashSave(IWin32Window owner, bool includeUntrackedFiles, bool keepIndex = false, string message = "", IReadOnlyList<string> selectedFiles = null)
        {
            bool Action()
            {
                var arguments = GitCommandHelpers.StashSaveCmd(includeUntrackedFiles, keepIndex, message, selectedFiles);
                FormProcess.ShowDialog(owner, Module, arguments);
                return true;
            }

            return DoActionOnRepo(owner, true, true, null, null, Action);
        }

        public bool StashPop(IWin32Window owner)
        {
            bool Action()
            {
                FormProcess.ShowDialog(owner, Module, "stash pop");
                MergeConflictHandler.HandleMergeConflicts(this, owner, false, false);
                return true;
            }

            return DoActionOnRepo(owner, true, true, null, null, Action);
        }

        /// <summary>Creates and checks out a new branch starting from the commit at which the stash was originally created.
        /// Applies the changes recorded in the stash to the new working directory and index.</summary>
        public bool StashBranch(IWin32Window owner, string branchName, string stash = null)
        {
            bool Action()
            {
                FormProcess.ShowDialog(owner, Module, "stash branch " + branchName.Quote().Combine(" ", stash.QuoteNE()));
                return true;
            }

            return DoActionOnRepo(owner, true, true, null, null, Action);
        }

        public bool StashDrop(IWin32Window owner, string stashName)
        {
            bool Action()
            {
                FormProcess.ShowDialog(owner, Module, "stash drop " + stashName.Quote());
                return true;
            }

            return DoActionOnRepo(owner, true, true, null, null, Action);
        }

        public bool StashApply(IWin32Window owner, string stashName)
        {
            bool Action()
            {
                FormProcess.ShowDialog(owner, Module, "stash apply " + stashName.Quote());
                MergeConflictHandler.HandleMergeConflicts(this, owner, false, false);
                return true;
            }

            return DoActionOnRepo(owner, true, true, null, null, Action);
        }

        public void ShowModelessForm(IWin32Window owner, bool requiresValidWorkingDir,
            EventHandler<GitUIBaseEventArgs> preEvent, EventHandler<GitUIPostActionEventArgs> postEvent, Func<Form> provideForm)
        {
            if (requiresValidWorkingDir && !RequiresValidWorkingDir(owner))
            {
                return;
            }

            if (!InvokeEvent(owner, preEvent))
            {
                return;
            }

            Form form = provideForm();

            void FormClosed(object sender, FormClosedEventArgs e)
            {
                form.FormClosed -= FormClosed;
                InvokePostEvent(owner, true, postEvent);
            }

            form.FormClosed += FormClosed;
            form.ShowInTaskbar = true;

            if (Application.OpenForms.Count > 0)
            {
                form.Show();
            }
            else
            {
                form.ShowDialog();
            }
        }

        /// <param name="requiresValidWorkingDir">If action requires valid working directory</param>
        /// <param name="owner">Owner window</param>
        /// <param name="changesRepo">if successfully done action changes repo state</param>
        /// <param name="preEvent">Event invoked before performing action</param>
        /// <param name="postEvent">Event invoked after performing action</param>
        /// <param name="action">Action to do. Return true to indicate that the action was successfully done.</param>
        /// <returns>true if action was successfully done, false otherwise</returns>
        public bool DoActionOnRepo(IWin32Window owner, bool requiresValidWorkingDir, bool changesRepo,
            EventHandler<GitUIBaseEventArgs> preEvent, EventHandler<GitUIPostActionEventArgs> postEvent, Func<bool> action)
        {
            bool actionDone = false;
            RepoChangedNotifier.Lock();
            try
            {
                if (requiresValidWorkingDir && !RequiresValidWorkingDir(owner))
                {
                    return false;
                }

                if (!InvokeEvent(owner, preEvent))
                {
                    return false;
                }

                try
                {
                    actionDone = action();
                }
                finally
                {
                    InvokePostEvent(owner, actionDone, postEvent);
                }
            }
            finally
            {
                RepoChangedNotifier.UnLock(changesRepo && actionDone);
            }

            return actionDone;
        }

        public bool DoActionOnRepo(Func<bool> action)
        {
            return DoActionOnRepo(null, false, true, null, null, action);
        }

        #region Checkout

        public bool StartCheckoutBranch(IWin32Window owner, string branch = "", bool remote = false, string[] containRevisons = null)
        {
            return DoActionOnRepo(owner, true, true, PreCheckoutBranch, PostCheckoutBranch, () =>
            {
                using (var form = new FormCheckoutBranch(this, branch, remote, containRevisons))
                {
                    return form.DoDefaultActionOrShow(owner) != DialogResult.Cancel;
                }
            });
        }

        public bool StartCheckoutBranch(IWin32Window owner, string[] containRevisons)
        {
            return StartCheckoutBranch(owner, "", false, containRevisons);
        }

        public bool StartCheckoutBranch(string branch, bool remote)
        {
            return StartCheckoutBranch(null, branch, remote);
        }

        public bool StartCheckoutBranch()
        {
            return StartCheckoutBranch(null);
        }

        public bool StartCheckoutRemoteBranch(IWin32Window owner, string branch)
        {
            return StartCheckoutBranch(owner, branch, true);
        }

        #endregion Checkout

        public bool StartCompareRevisionsDialog(IWin32Window owner)
        {
            bool Action()
            {
                using (var form = new FormLog(this))
                {
                    return form.ShowDialog(owner) == DialogResult.OK;
                }
            }

            return DoActionOnRepo(owner, true, true, PreCompareRevisions, PostCompareRevisions, Action);
        }

        public bool StartCompareRevisionsDialog()
        {
            return StartCompareRevisionsDialog(null);
        }

        public bool StartAddFilesDialog(IWin32Window owner, string addFiles = null)
        {
            return DoActionOnRepo(owner, true, true, PreAddFiles, PostAddFiles, () =>
            {
                using (var form = new FormAddFiles(this, addFiles))
                {
                    form.ShowDialog(owner);
                }

                return true;
            });
        }

        public bool StartAddFilesDialog(string addFiles)
        {
            return StartAddFilesDialog(null, addFiles);
        }

        public bool StartAddFilesDialog()
        {
            return StartAddFilesDialog(null, null);
        }

        public bool StartCreateBranchDialog(IWin32Window owner, GitRevision revision)
        {
            bool Action()
            {
                using (var form = new FormCreateBranch(this, revision))
                {
                    return form.ShowDialog(owner) == DialogResult.OK;
                }
            }

            return DoActionOnRepo(owner, true, true, PreCreateBranch, PostCreateBranch, Action);
        }

        public bool StartCreateBranchDialog()
        {
            return StartCreateBranchDialog(null, null);
        }

        public bool StartCloneDialog(IWin32Window owner, string url = null, bool openedFromProtocolHandler = false, EventHandler<GitModuleEventArgs> gitModuleChanged = null)
        {
            bool Action()
            {
                using (var form = new FormClone(this, url, openedFromProtocolHandler, gitModuleChanged))
                {
                    form.ShowDialog(owner);
                }

                return true;
            }

            return DoActionOnRepo(owner, false, false, PreClone, PostClone, Action);
        }

        public bool StartCloneDialog(IWin32Window owner, string url, EventHandler<GitModuleEventArgs> gitModuleChanged)
        {
            return StartCloneDialog(owner, url, false, gitModuleChanged);
        }

        public bool StartCloneDialog(string url)
        {
            return StartCloneDialog(null, url);
        }

        public bool StartCloneDialog()
        {
            return StartCloneDialog(null, null);
        }

        public void StartCleanupRepositoryDialog(IWin32Window owner = null, string path = null)
        {
            using (var form = new FormCleanupRepository(this))
            {
                form.SetPathArgument(path);
                form.ShowDialog(owner);
            }
        }

        public bool StartSquashCommitDialog(IWin32Window owner, GitRevision revision)
        {
            bool Action()
            {
                using (var form = new FormCommit(this, CommitKind.Squash, revision))
                {
                    form.ShowDialog(owner);
                }

                return true;
            }

            return DoActionOnRepo(Action);
        }

        public bool StartFixupCommitDialog(IWin32Window owner, GitRevision revision)
        {
            bool Action()
            {
                using (var form = new FormCommit(this, CommitKind.Fixup, revision))
                {
                    form.ShowDialog(owner);
                }

                return true;
            }

            return DoActionOnRepo(Action);
        }

        public bool StartCommitDialog(IWin32Window owner, bool showOnlyWhenChanges = false)
        {
            bool Action()
            {
                using (var form = new FormCommit(this))
                {
                    if (showOnlyWhenChanges)
                    {
                        form.ShowDialogWhenChanges(owner);
                    }
                    else
                    {
                        form.ShowDialog(owner);
                    }
                }

                return true;
            }

            return DoActionOnRepo(owner, true, false, PreCommit, PostCommit, Action);
        }

        public bool StartCommitDialog(bool showOnlyWhenChanges)
        {
            return StartCommitDialog(null, showOnlyWhenChanges);
        }

        public bool StartCommitDialog()
        {
            return StartCommitDialog(null);
        }

        public bool StartInitializeDialog(IWin32Window owner, EventHandler<GitModuleEventArgs> gitModuleChanged)
        {
            return StartInitializeDialog(owner, null, gitModuleChanged);
        }

        public bool StartInitializeDialog()
        {
            return StartInitializeDialog(null, null);
        }

        public bool StartInitializeDialog(IWin32Window owner, string dir, EventHandler<GitModuleEventArgs> gitModuleChanged)
        {
            bool Action()
            {
                if (dir == null)
                {
                    dir = Module.IsValidGitWorkingDir() ? Module.WorkingDir : string.Empty;
                }

                using (var frm = new FormInit(dir, gitModuleChanged))
                {
                    frm.ShowDialog(owner);
                }

                return true;
            }

            return DoActionOnRepo(owner, false, true, PreInitialize, PostInitialize, Action);
        }

        public bool StartInitializeDialog(string dir)
        {
            return StartInitializeDialog(null, dir, null);
        }

        /// <summary>
        /// Starts pull dialog
        /// </summary>
        /// <param name="owner">An implementation of IWin32Window that will own the modal dialog box.</param>
        /// <param name="pullCompleted">true if pull completed with no errors</param>
        /// <returns>if revision grid should be refreshed</returns>
        public bool StartPullDialog(IWin32Window owner, bool pullOnShow, string remoteBranch, string remote, out bool pullCompleted, bool fetchAll)
        {
            var pulled = false;

            bool Action()
            {
                using (FormPull formPull = new FormPull(this, remoteBranch, remote))
                {
                    if (fetchAll)
                    {
                        formPull.SetForFetchAll();
                    }

                    var dlgResult = pullOnShow
                        ? formPull.PullAndShowDialogWhenFailed(owner)
                        : formPull.ShowDialog(owner);

                    if (dlgResult == DialogResult.OK)
                    {
                        pulled = !formPull.ErrorOccurred;
                    }

                    return dlgResult == DialogResult.OK;
                }
            }

            bool done = DoActionOnRepo(owner, true, true, PrePull, PostPull, Action);

            pullCompleted = pulled;

            return done;
        }

        public bool StartPullDialog(IWin32Window owner, bool pullOnShow, out bool pullCompleted, bool fetchAll)
        {
            return StartPullDialog(owner, pullOnShow, null, null, out pullCompleted, fetchAll);
        }

        public bool StartPullDialog(IWin32Window owner, bool pullOnShow, out bool pullCompleted)
        {
            return StartPullDialog(owner, pullOnShow, out pullCompleted, false);
        }

        public bool StartPullDialog(IWin32Window owner, bool pullOnShow)
        {
            return StartPullDialog(owner, pullOnShow, out _, false);
        }

        public bool StartPullDialog(bool pullOnShow, out bool pullCompleted)
        {
            return StartPullDialog(null, pullOnShow, out pullCompleted, false);
        }

        public bool StartPullDialog(bool pullOnShow, string remoteBranch, out bool pullCompleted)
        {
            return StartPullDialog(null, pullOnShow, remoteBranch, null, out pullCompleted, false);
        }

        public bool StartPullDialog(bool pullOnShow, string remoteBranch = null)
        {
            return StartPullDialog(pullOnShow, remoteBranch, out _);
        }

        public bool StartPullDialog(IWin32Window owner)
        {
            return StartPullDialog(owner, false, out _, false);
        }

        public bool StartPullDialog()
        {
            return StartPullDialog(false);
        }

        public bool StartViewPatchDialog(IWin32Window owner, string patchFile = null)
        {
            bool Action()
            {
                using (var viewPatch = new FormViewPatch(this))
                {
                    if (!string.IsNullOrEmpty(patchFile))
                    {
                        viewPatch.LoadPatch(patchFile);
                    }

                    viewPatch.ShowDialog(owner);
                }

                return true;
            }

            return DoActionOnRepo(owner, false, false, PreViewPatch, PostViewPatch, Action);
        }

        public bool StartViewPatchDialog(string patchFile)
        {
            return StartViewPatchDialog(null, patchFile);
        }

        public bool StartViewPatchDialog()
        {
            return StartViewPatchDialog(null, null);
        }

        public bool StartSparseWorkingCopyDialog()
        {
            return StartSparseWorkingCopyDialog(null);
        }

        public bool StartSparseWorkingCopyDialog([CanBeNull] IWin32Window owner)
        {
            bool Action()
            {
                using (var form = new FormSparseWorkingCopy(this))
                {
                    form.ShowDialog(owner);
                }

                return true;
            }

            return DoActionOnRepo(owner, true, false, PreSparseWorkingCopy, PostSparseWorkingCopy, Action);
        }

        public void AddCommitTemplate(string key, Func<string> addingText)
        {
            _commitTemplateManager.Register(key, addingText);
        }

        public void RemoveCommitTemplate(string key)
        {
            _commitTemplateManager.Unregister(key);
        }

        public bool StartFormatPatchDialog(IWin32Window owner)
        {
            bool Action()
            {
                using (var form = new FormFormatPatch(this))
                {
                    form.ShowDialog(owner);
                }

                return true;
            }

            return DoActionOnRepo(owner, true, false, PreFormatPatch, PostFormatPatch, Action);
        }

        public bool StartFormatPatchDialog()
        {
            return StartFormatPatchDialog(null);
        }

        public bool StartStashDialog(IWin32Window owner, bool manageStashes = true)
        {
            bool Action()
            {
                using (var form = new FormStash(this) { ManageStashes = manageStashes })
                {
                    form.ShowDialog(owner);
                }

                return true;
            }

            return DoActionOnRepo(owner, true, false, PreStash, PostStash, Action);
        }

        public bool StartStashDialog()
        {
            return StartStashDialog(null);
        }

        public bool StartResetChangesDialog(IWin32Window owner = null)
        {
            var unstagedFiles = Module.GetUnstagedFiles();
            return StartResetChangesDialog(owner, unstagedFiles, false);
        }

        public bool StartResetChangesDialog(IWin32Window owner, IEnumerable<GitItemStatus> unstagedFiles, bool onlyUnstaged)
        {
            // Show a form asking the user if they want to reset the changes.
            FormResetChanges.ActionEnum resetAction = FormResetChanges.ShowResetDialog(owner, unstagedFiles.Any(item => !item.IsNew), unstagedFiles.Any(item => item.IsNew));

            if (resetAction == FormResetChanges.ActionEnum.Cancel)
            {
                return false;
            }

            bool Action()
            {
                if (onlyUnstaged)
                {
                    Module.RunGitCmd("checkout -- .");
                }
                else
                {
                    // Reset all changes.
                    Module.ResetHard("");
                }

                if (resetAction == FormResetChanges.ActionEnum.ResetAndDelete)
                {
                    Module.RunGitCmd("clean -df");
                }

                return true;
            }

            return DoActionOnRepo(owner, true, true, null, null, Action);
        }

        public bool StartResetChangesDialog(IWin32Window owner, string fileName)
        {
            // Show a form asking the user if they want to reset the changes.
            FormResetChanges.ActionEnum resetAction = FormResetChanges.ShowResetDialog(owner, true, false);

            if (resetAction == FormResetChanges.ActionEnum.Cancel)
            {
                return false;
            }

            using (WaitCursorScope.Enter())
            {
                // Reset all changes.
                Module.ResetFile(fileName);

                // Also delete new files, if requested.
                if (resetAction == FormResetChanges.ActionEnum.ResetAndDelete)
                {
                    try
                    {
                        string path = _fullPathResolver.Resolve(fileName);
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                        else
                        {
                            Directory.Delete(path, true);
                        }
                    }
                    catch (IOException)
                    {
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                }
            }

            return true;
        }

        public bool StartResetChangesDialog(string fileName)
        {
            return StartResetChangesDialog(null, fileName);
        }

        public bool StartRevertCommitDialog(IWin32Window owner, GitRevision revision)
        {
            bool Action()
            {
                using (var form = new FormRevertCommit(this, revision))
                {
                    return form.ShowDialog(owner) == DialogResult.OK;
                }
            }

            return DoActionOnRepo(owner, true, true, PreRevertCommit, PostRevertCommit, Action);
        }

        public bool StartResolveConflictsDialog(IWin32Window owner, bool offerCommit = true)
        {
            bool Action()
            {
                using (var form = new FormResolveConflicts(this, offerCommit))
                {
                    form.ShowDialog(owner);
                }

                return true;
            }

            return DoActionOnRepo(owner, true, true, PreResolveConflicts, PostResolveConflicts, Action);
        }

        public bool StartResolveConflictsDialog(bool offerCommit)
        {
            return StartResolveConflictsDialog(null, offerCommit);
        }

        public bool StartResolveConflictsDialog()
        {
            return StartResolveConflictsDialog(null);
        }

        public bool StartCherryPickDialog(IWin32Window owner, GitRevision revision = null)
        {
            bool Action()
            {
                using (var form = new FormCherryPick(this, revision))
                {
                    return form.ShowDialog(owner) == DialogResult.OK;
                }
            }

            return DoActionOnRepo(owner, true, true, PreCherryPick, PostCherryPick, Action);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "It seems that all prevForm variable values are different so there is not a double dispose here. However the logic is better to be rewritten")]
        public bool StartCherryPickDialog(IWin32Window owner, IEnumerable<GitRevision> revisions)
        {
            if (revisions == null)
            {
                throw new ArgumentNullException(nameof(revisions));
            }

            bool Action()
            {
                FormCherryPick prevForm = null;

                try
                {
                    bool repoChanged = false;

                    foreach (var r in revisions)
                    {
                        var frm = new FormCherryPick(this, r);
                        if (prevForm != null)
                        {
                            frm.CopyOptions(prevForm);
                            prevForm.Dispose();
                        }

                        prevForm = frm;
                        if (frm.ShowDialog(owner) == DialogResult.OK)
                        {
                            repoChanged = true;
                        }
                        else
                        {
                            return repoChanged;
                        }
                    }

                    return repoChanged;
                }
                finally
                {
                    prevForm?.Dispose();
                }
            }

            return DoActionOnRepo(owner, true, true, PreCherryPick, PostCherryPick, Action);
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
            bool Action()
            {
                using (var form = new FormMergeBranch(this, branch))
                {
                    form.ShowDialog(owner);
                }

                return true;
            }

            return DoActionOnRepo(owner, true, false, PreMergeBranch, PostMergeBranch, Action);
        }

        /// <summary>Start Merge dialog, using the specified branch.</summary>
        /// <param name="branch">Branch to merge into the current branch.</param>
        public bool StartMergeBranchDialog(string branch)
        {
            return StartMergeBranchDialog(null, branch);
        }

        public bool StartCreateTagDialog(IWin32Window owner)
        {
            bool Action()
            {
                using (var form = new FormCreateTag(this, null))
                {
                    return form.ShowDialog(owner) == DialogResult.OK;
                }
            }

            return DoActionOnRepo(owner, true, true, PreCreateTag, PostCreateTag, Action);
        }

        public bool StartCreateTagDialog()
        {
            return StartCreateTagDialog(null);
        }

        public bool StartDeleteTagDialog(IWin32Window owner, string tag)
        {
            bool Action()
            {
                using (var form = new FormDeleteTag(this, tag))
                {
                    return form.ShowDialog(owner) == DialogResult.OK;
                }
            }

            return DoActionOnRepo(owner, true, true, PreDeleteTag, PostDeleteTag, Action);
        }

        public bool StartDeleteTagDialog(string tag)
        {
            return StartDeleteTagDialog(null, tag);
        }

        public bool StartDeleteTagDialog()
        {
            return StartDeleteTagDialog(null, "");
        }

        public bool StartEditGitIgnoreDialog(IWin32Window owner, bool localExcludes)
        {
            bool Action()
            {
                using (var form = new FormGitIgnore(this, localExcludes))
                {
                    form.ShowDialog(owner);
                }

                return true;
            }

            return DoActionOnRepo(owner, true, false, PreEditGitIgnore, PostEditGitIgnore, Action);
        }

        public bool StartEditGitIgnoreDialog(bool localExcludes)
        {
            return StartEditGitIgnoreDialog(null, localExcludes);
        }

        public bool StartAddToGitIgnoreDialog(IWin32Window owner, bool localExclude, params string[] filePattern)
        {
            bool Action()
            {
                using (var frm = new FormAddToGitIgnore(this, localExclude, filePattern))
                {
                    frm.ShowDialog(owner);
                }

                return true;
            }

            return DoActionOnRepo(owner, true, false, PreEditGitIgnore, PostEditGitIgnore, Action);
        }

        public bool StartSettingsDialog(IWin32Window owner, SettingsPageReference initialPage = null)
        {
            bool Action()
            {
                FormSettings.ShowSettingsDialog(this, owner, initialPage);

                return true;
            }

            return DoActionOnRepo(owner, false, true, PreSettings, PostSettings, Action);
        }

        public bool StartSettingsDialog()
        {
            return StartSettingsDialog(null, null);
        }

        public bool StartSettingsDialog(IGitPlugin gitPlugin)
        {
            // TODO: how to pass the main dialog as owner of the SettingsDialog (first parameter):
            return StartSettingsDialog(null, new SettingsPageReferenceByPlugin(gitPlugin));
        }

        /// <summary>
        /// Open the archive dialog
        /// </summary>
        /// <param name="revision">Revision to create an archive from</param>
        /// <param name="revision2">Revision for differencial archive </param>
        /// <param name="path">Files path for archive</param>
        public bool StartArchiveDialog(IWin32Window owner = null, GitRevision revision = null, GitRevision revision2 = null, string path = null)
        {
            return DoActionOnRepo(owner, true, false, PreArchive, PostArchive, () =>
                {
                    using (var form = new FormArchive(this))
                    {
                        form.SelectedRevision = revision;
                        form.SetDiffSelectedRevision(revision2);
                        form.SetPathArgument(path);
                        form.ShowDialog(owner);
                    }

                    return true;
                });
        }

        public bool StartArchiveDialog()
        {
            return StartArchiveDialog(null);
        }

        public bool StartMailMapDialog(IWin32Window owner)
        {
            bool Action()
            {
                using (var form = new FormMailMap(this))
                {
                    form.ShowDialog(owner);
                }

                return true;
            }

            return DoActionOnRepo(owner, true, false, PreMailMap, PostMailMap, Action);
        }

        public bool StartMailMapDialog()
        {
            return StartMailMapDialog(null);
        }

        public bool StartVerifyDatabaseDialog(IWin32Window owner)
        {
            bool Action()
            {
                using (var form = new FormVerify(this))
                {
                    form.ShowDialog(owner);
                }

                return true;
            }

            // TODO: move Notify to FormVerify and friends
            return DoActionOnRepo(owner, true, true, PreVerifyDatabase, PostVerifyDatabase, Action);
        }

        public bool StartVerifyDatabaseDialog()
        {
            return StartVerifyDatabaseDialog(null);
        }

        /// <param name="preselectRemote">makes the FormRemotes initialially select the given remote</param>
        public bool StartRemotesDialog(IWin32Window owner, string preselectRemote = null)
        {
            bool Action()
            {
                using (var form = new FormRemotes(this))
                {
                    form.PreselectRemoteOnLoad = preselectRemote;
                    form.ShowDialog(owner);
                }

                return true;
            }

            return DoActionOnRepo(owner, true, true, PreRemotes, PostRemotes, Action);
        }

        public bool StartRemotesDialog()
        {
            return StartRemotesDialog(null);
        }

        private bool StartRebaseDialog(IWin32Window owner, string onto, bool interactive = false,
            bool startRebaseImmediately = true)
        {
            return StartRebaseDialog(owner, string.Empty, null, onto, interactive, startRebaseImmediately);
        }

        public bool StartRebase(IWin32Window owner, string onto)
        {
            return StartRebaseDialog(owner, onto, interactive: false);
        }

        public bool StartTheContinueRebaseDialog(IWin32Window owner)
        {
            return StartRebaseDialog(owner, onto: null,
                interactive: false, startRebaseImmediately: false);
        }

        public bool StartInteractiveRebase(IWin32Window owner, string onto)
        {
            return StartRebaseDialog(owner, onto, interactive: true);
        }

        public bool StartRebaseDialogWithAdvOptions(IWin32Window owner, string onto)
        {
            return StartRebaseDialog(owner, onto,
                interactive: false, startRebaseImmediately: false);
        }

        public bool StartRebaseDialog(IWin32Window owner, string from, string to, string onto,
            bool interactive = false, bool startRebaseImmediately = true)
        {
            bool Action()
            {
                using (var form = new FormRebase(this, @from, to, onto, interactive, startRebaseImmediately))
                {
                    form.ShowDialog(owner);
                }

                return true;
            }

            return DoActionOnRepo(owner, true, true, PreRebase, PostRebase, Action);
        }

        public bool StartRebaseDialog(IWin32Window owner, string onto)
        {
            return StartRebaseDialog(owner, onto, interactive: false, startRebaseImmediately: false);
        }

        public bool StartRenameDialog(string branch)
        {
            return StartRenameDialog(null, branch);
        }

        public bool StartRenameDialog(IWin32Window owner, string branch)
        {
            bool Action()
            {
                using (var form = new FormRenameBranch(this, branch))
                {
                    return form.ShowDialog(owner) == DialogResult.OK;
                }
            }

            return DoActionOnRepo(owner, true, true, PreRename, PostRename, Action);
        }

        public bool StartSubmodulesDialog(IWin32Window owner)
        {
            bool Action()
            {
                using (var form = new FormSubmodules(this))
                {
                    form.ShowDialog(owner);
                }

                return true;
            }

            return DoActionOnRepo(owner, true, true, PreSubmodulesEdit, PostSubmodulesEdit, Action);
        }

        public bool StartSubmodulesDialog()
        {
            return StartSubmodulesDialog(null);
        }

        public bool StartUpdateSubmodulesDialog(IWin32Window owner)
        {
            bool Action()
            {
                return FormProcess.ShowDialog(owner, Module, GitCommandHelpers.SubmoduleUpdateCmd(""));
            }

            return DoActionOnRepo(owner, true, true, PreUpdateSubmodules, PostUpdateSubmodules, Action);
        }

        public bool StartUpdateSubmodulesDialog()
        {
            return StartUpdateSubmodulesDialog(null);
        }

        public bool StartSyncSubmodulesDialog(IWin32Window owner)
        {
            bool Action()
            {
                return FormProcess.ShowDialog(owner, Module, GitCommandHelpers.SubmoduleSyncCmd(""));
            }

            return DoActionOnRepo(owner, true, true, PreSyncSubmodules, PostSyncSubmodules, Action);
        }

        public bool StartSyncSubmodulesDialog()
        {
            return StartSyncSubmodulesDialog(null);
        }

        public void UpdateSubmodules(IWin32Window win)
        {
            if (!Module.HasSubmodules())
            {
                return;
            }

            var updateSubmodules = AppSettings.UpdateSubmodulesOnCheckout ?? MessageBoxes.ConfirmUpdateSubmodules(win);

            if (updateSubmodules)
            {
                StartUpdateSubmodulesDialog(win);
            }
        }

        public bool StartPluginSettingsDialog(IWin32Window owner)
        {
            return StartSettingsDialog(owner, PluginsSettingsGroup.GetPageReference());
        }

        public bool StartPluginSettingsDialog()
        {
            return StartPluginSettingsDialog(null);
        }

        public bool StartRepoSettingsDialog(IWin32Window owner)
        {
            return StartSettingsDialog(owner, CommandsDialogs.SettingsDialog.Pages.GitConfigSettingsPage.GetPageReference());
        }

        public bool StartBrowseDialog(IWin32Window owner, string filter, string selectedCommit)
        {
            if (!InvokeEvent(owner, PreBrowse))
            {
                return false;
            }

            var form = new FormBrowse(this, filter, selectedCommit);

            if (Application.MessageLoop)
            {
                form.Show();
            }
            else
            {
                Application.Run(form);
            }

            InvokeEvent(owner, PostBrowse);
            return true;
        }

        public bool StartBrowseDialog(string filter)
        {
            return StartBrowseDialog(null, filter, null);
        }

        public bool StartBrowseDialog(string filter, string selectedCommit)
        {
            return StartBrowseDialog(null, filter, selectedCommit);
        }

        public void StartFileHistoryDialog(IWin32Window owner, string fileName, GitRevision revision = null, bool filterByRevision = false, bool showBlame = false)
        {
            Form ProvideForm()
            {
                var form = new FormFileHistory(this, fileName, revision, filterByRevision);

                if (showBlame)
                {
                    form.SelectBlameTab();
                }
                else
                {
                    form.SelectDiffTab();
                }

                return form;
            }

            ShowModelessForm(owner, true, PreFileHistory, PostFileHistory, ProvideForm);
        }

        public void StartFileHistoryDialog(string fileName, GitRevision revision)
        {
            StartFileHistoryDialog(null, fileName, revision);
        }

        public void StartFileHistoryDialog(string fileName)
        {
            StartFileHistoryDialog(fileName, null);
        }

        public bool StartPushDialog()
        {
            return StartPushDialog(false);
        }

        public bool StartPushDialog(IWin32Window owner, bool pushOnShow, out bool pushCompleted)
        {
            return StartPushDialog(owner, pushOnShow, false, out pushCompleted);
        }

        public bool StartPushDialog(IWin32Window owner, bool pushOnShow, bool forceWithLease, out bool pushCompleted)
        {
            bool pushed = false;

            bool Action()
            {
                using (var form = new FormPush(this))
                {
                    if (forceWithLease)
                    {
                        form.CheckForceWithLease();
                    }

                    var dlgResult = pushOnShow
                        ? form.PushAndShowDialogWhenFailed(owner)
                        : form.ShowDialog(owner);

                    if (dlgResult == DialogResult.OK)
                    {
                        pushed = !form.ErrorOccurred;
                    }

                    return dlgResult == DialogResult.OK;
                }
            }

            bool done = DoActionOnRepo(owner, true, true, PrePush, PostPush, Action);

            pushCompleted = pushed;

            return done;
        }

        public bool StartPushDialog(IWin32Window owner, bool pushOnShow)
        {
            return StartPushDialog(owner, pushOnShow, out _);
        }

        public bool StartPushDialog(bool pushOnShow)
        {
            return StartPushDialog(null, pushOnShow);
        }

        public bool StartApplyPatchDialog(IWin32Window owner, string patchFile = null)
        {
            return DoActionOnRepo(owner, true, false, PreApplyPatch, PostApplyPatch, () =>
                {
                    using (var form = new FormApplyPatch(this))
                    {
                        if (Directory.Exists(patchFile))
                        {
                            form.SetPatchDir(patchFile);
                        }
                        else
                        {
                            form.SetPatchFile(patchFile);
                        }

                        form.ShowDialog(owner);

                        return true;
                    }
                });
        }

        public bool StartApplyPatchDialog(string patchFile)
        {
            return StartApplyPatchDialog(null, patchFile);
        }

        public bool StartApplyPatchDialog()
        {
            return StartApplyPatchDialog(null, null);
        }

        public bool StartEditGitAttributesDialog(IWin32Window owner = null)
        {
            bool Action()
            {
                using (var form = new FormGitAttributes(this))
                {
                    form.ShowDialog(owner);
                }

                return true;
            }

            return DoActionOnRepo(owner, true, false, PreEditGitAttributes, PostEditGitAttributes, Action);
        }

        private bool InvokeEvent(IWin32Window ownerForm, EventHandler<GitUIBaseEventArgs> gitUIEventHandler)
        {
            return InvokeEvent(this, ownerForm, gitUIEventHandler);
        }

        public GitModule Module { get; private set; }

        public IGitModule GitModule => Module;

        private void InvokePostEvent(IWin32Window ownerForm, bool actionDone, EventHandler<GitUIPostActionEventArgs> gitUIEventHandler)
        {
            if (gitUIEventHandler != null)
            {
                var e = new GitUIPostActionEventArgs(ownerForm, this, actionDone);
                gitUIEventHandler(this, e);
            }
        }

        internal bool InvokeEvent(object sender, IWin32Window ownerForm, EventHandler<GitUIBaseEventArgs> gitUIEventHandler)
        {
            try
            {
                var e = new GitUIEventArgs(ownerForm, this);
                gitUIEventHandler?.Invoke(sender, e);

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

        private bool StartBlameDialog(IWin32Window owner, string fileName, GitRevision revision, int? initialLine = null)
        {
            return DoActionOnRepo(owner, true, false, PreBlame, PostBlame, () =>
                {
                    using (var frm = new FormBlame(this, fileName, revision, initialLine))
                    {
                        frm.ShowDialog(owner);
                    }

                    return true;
                });
        }

        public bool StartBlameDialog(string fileName, int? initialLine = null)
        {
            return StartBlameDialog(null, fileName, null, initialLine);
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

        public void StartCloneForkFromHoster(IWin32Window owner, IRepositoryHostPlugin gitHoster, EventHandler<GitModuleEventArgs> gitModuleChanged)
        {
            WrapRepoHostingCall("View pull requests", gitHoster, gh =>
            {
                using (var frm = new ForkAndCloneForm(gitHoster, gitModuleChanged))
                {
                    frm.ShowDialog(owner);
                }
            });
        }

        internal void StartPullRequestsDialog(IWin32Window owner, IRepositoryHostPlugin gitHoster)
        {
            WrapRepoHostingCall("View pull requests", gitHoster,
                                gh =>
                                {
                                    var frm = new ViewPullRequestsForm(this, gitHoster) { ShowInTaskbar = true };
                                    frm.Show();
                                });
        }

        internal void StartPullRequestsDialog(IRepositoryHostPlugin gitHoster)
        {
            StartPullRequestsDialog(null, gitHoster);
        }

        public void StartCreatePullRequest(IWin32Window owner)
        {
            List<IRepositoryHostPlugin> relevantHosts =
                RepoHosts.GitHosters.Where(gh => gh.GitModuleIsRelevantToMe(Module)).ToList();

            if (relevantHosts.Count == 0)
            {
                MessageBox.Show(owner, "Could not find any repo hosts for current working directory");
            }
            else if (relevantHosts.Count == 1)
            {
                StartCreatePullRequest(owner, relevantHosts.First());
            }
            else
            {
                MessageBox.Show("StartCreatePullRequest:Selection not implemented!");
            }
        }

        public void StartCreatePullRequest(IRepositoryHostPlugin gitHoster = null)
        {
            StartCreatePullRequest(null, gitHoster);
        }

        public void StartCreatePullRequest(IRepositoryHostPlugin gitHoster, string chooseRemote, string chooseBranch)
        {
            StartCreatePullRequest(null, gitHoster, chooseRemote, chooseBranch);
        }

        public void StartCreatePullRequest(IWin32Window owner, IRepositoryHostPlugin gitHoster, string chooseRemote = null, string chooseBranch = null)
        {
            WrapRepoHostingCall(
                "Create pull request",
                gitHoster,
                gh =>
                {
                    var form = new CreatePullRequestForm(this, gitHoster, chooseRemote, chooseBranch)
                    {
                        ShowInTaskbar = true
                    };

                    form.Show();
                });
        }

        public void RunCommand(string[] args)
        {
            var arguments = InitializeArguments(args);

            if (args.Length <= 1)
            {
                return;
            }

            var command = args[1];

            if (command == "blame" && args.Length <= 2)
            {
                MessageBox.Show("Cannot open blame, there is no file selected.", "Blame");
                return;
            }

            if (command == "difftool" && args.Length <= 2)
            {
                MessageBox.Show("Cannot open difftool, there is no file selected.", "Difftool");
                return;
            }

            if (command == "filehistory" && args.Length <= 2)
            {
                MessageBox.Show("Cannot open file history, there is no file selected.", "File history");
                return;
            }

            if (command == "fileeditor" && args.Length <= 2)
            {
                MessageBox.Show("Cannot open file editor, there is no file selected.", "File editor");
                return;
            }

            if (command == "revert" && args.Length <= 2)
            {
                MessageBox.Show("Cannot open revert, there is no file selected.", "Revert");
                return;
            }

            RunCommandBasedOnArgument(args, arguments);
        }

        // Please update FormCommandlineHelp if you add or change commands
        private void RunCommandBasedOnArgument(string[] args, Dictionary<string, string> arguments)
        {
            #pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
            switch (args[1])
            {
                case "about":
                    var frm = new AboutBox();
                    frm.StartPosition = FormStartPosition.CenterScreen;
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
                    StartCheckoutBranch();
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
                    if (Module.WorkingDir.TrimEnd('\\') == Path.GetFullPath(args[2]) && Module.SuperprojectModule != null)
                    {
                        Module = Module.SuperprojectModule;
                    }

                    RunFileHistoryCommand(args);
                    return;
                case "fileeditor":  // filename
                    if (!StartFileEditorDialog(args[2]))
                    {
                        Environment.ExitCode = -1;
                    }

                    return;
                case "formatpatch":
                    StartFormatPatchDialog();
                    return;
                case "gitbash":
                    Module.RunBash();
                    return;
                case "gitignore":
                    StartEditGitIgnoreDialog(false);
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
                case "pull":        // [--rebase] [--merge] [--fetch] [--quiet] [--remotebranch name]
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
                case "revert":
                case "reset":
                    StartResetChangesDialog(args.Length == 3 ? args[2] : "");
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
                case "uninstall":
                    Uninstall();
                    return;
                default:
                    if (args[1].StartsWith("git://") || args[1].StartsWith("http://") || args[1].StartsWith("https://"))
                    {
                        StartCloneDialog(null, args[1], true);
                        return;
                    }

                    if (args[1].StartsWith("github-windows://openRepo/"))
                    {
                        StartCloneDialog(null, args[1].Replace("github-windows://openRepo/", ""), true);
                        return;
                    }

                    if (args[1].StartsWith("github-mac://openRepo/"))
                    {
                        StartCloneDialog(null, args[1].Replace("github-mac://openRepo/", ""), true);
                        return;
                    }

                    break;
            }
            #pragma warning restore SA1025 // Code should not contain multiple whitespace in a row

            Application.Run(new FormCommandlineHelp { StartPosition = FormStartPosition.CenterScreen });
        }

        private static void Uninstall()
        {
            var configFileGlobalSettings = ConfigFileSettings.CreateGlobal(false);

            var coreEditor = configFileGlobalSettings.GetValue("core.editor");
            if (coreEditor.ToLowerInvariant().Contains(AppSettings.GetInstallDir().ToPosixPath().ToLowerInvariant()))
            {
                configFileGlobalSettings.SetValue("core.editor", "");
            }

            configFileGlobalSettings.Save();
        }

        private void RunMergeCommand(Dictionary<string, string> arguments)
        {
            string branch = null;
            if (arguments.ContainsKey("branch"))
            {
                branch = arguments["branch"];
            }

            StartMergeBranchDialog(branch);
        }

        private void RunSearchFileCommand()
        {
            var searchWindow = new SearchWindow<string>(FindFileMatches);
            Application.Run(searchWindow);
            if (searchWindow.SelectedItem != null)
            {
                // We need to return the file that has been found, the visual studio plugin uses the return value
                // to open the selected file.
                Console.WriteLine(Path.Combine(Module.WorkingDir, searchWindow.SelectedItem));
            }
        }

        private void RunBrowseCommand(string[] args)
        {
            StartBrowseDialog(GetParameterOrEmptyStringAsDefault(args, "-filter"), GetParameterOrEmptyStringAsDefault(args, "-commit"));
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
                    string path = File.ReadAllText(args[2]).Trim().Split(new[] { '\n' }, 1).FirstOrDefault();
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
            {
                branch = arguments["branch"];
            }

            StartRebaseDialog(owner: null, onto: branch);
        }

        public bool StartFileEditorDialog(string filename, bool showWarning = false)
        {
            using (var formEditor = new FormEditor(this, filename, showWarning))
            {
                return formEditor.ShowDialog() != DialogResult.Cancel;
            }
        }

        private void RunFileHistoryCommand(string[] args)
        {
            // Remove working directory from filename. This is to prevent filenames that are too
            // long while there is room left when the workingdir was not in the path.
            string fileHistoryFileName = string.IsNullOrEmpty(Module.WorkingDir) ? args[2] :
                args[2].Replace(Module.WorkingDir, "").ToPosixPath();

            StartFileHistoryDialog(fileHistoryFileName);
        }

        private void RunCloneCommand(string[] args)
        {
            if (args.Length > 2)
            {
                StartCloneDialog(args[2]);
            }
            else
            {
                StartCloneDialog();
            }
        }

        private void RunInitCommand(string[] args)
        {
            if (args.Length > 2)
            {
                StartInitializeDialog(args[2]);
            }
            else
            {
                StartInitializeDialog();
            }
        }

        private void RunBlameCommand(string[] args)
        {
            // Remove working directory from filename. This is to prevent filenames that are too
            // long while there is room left when the workingdir was not in the path.
            string filenameFromBlame = args[2].Replace(Module.WorkingDir, "").ToPosixPath();

            int? initialLine = null;
            if (args.Length >= 4)
            {
                if (int.TryParse(args[3], out var temp))
                {
                    initialLine = temp;
                }
            }

            StartBlameDialog(filenameFromBlame, initialLine);
        }

        private void RunMergeToolOrConflictCommand(Dictionary<string, string> arguments)
        {
            if (!arguments.ContainsKey("quiet") || Module.InTheMiddleOfConflictedMerge())
            {
                StartResolveConflictsDialog();
            }
        }

        private static Dictionary<string, string> InitializeArguments(string[] args)
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>();

            for (int i = 2; i < args.Length; i++)
            {
                if (args[i].StartsWith("--") && i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                {
                    arguments.Add(args[i].TrimStart('-'), args[++i]);
                }
                else if (args[i].StartsWith("--"))
                {
                    arguments.Add(args[i].TrimStart('-'), null);
                }
            }

            return arguments;
        }

        private IReadOnlyList<string> FindFileMatches(string name)
        {
            var candidates = Module.GetFullTree("HEAD");

            var predicate = _fildFilePredicateProvider.Get(name, Module.WorkingDir);

            return candidates.Where(predicate).ToList();
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
            {
                remoteBranch = arguments["remotebranch"];
            }

            StartPullDialog(arguments.ContainsKey("quiet"), remoteBranch);
        }

        private static void UpdateSettingsBasedOnArguments(Dictionary<string, string> arguments)
        {
            if (arguments.ContainsKey("merge"))
            {
                AppSettings.FormPullAction = AppSettings.PullAction.Merge;
            }

            if (arguments.ContainsKey("rebase"))
            {
                AppSettings.FormPullAction = AppSettings.PullAction.Rebase;
            }

            if (arguments.ContainsKey("fetch"))
            {
                AppSettings.FormPullAction = AppSettings.PullAction.Fetch;
            }

            if (arguments.ContainsKey("autostash"))
            {
                AppSettings.AutoStash = true;
            }
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

        public void BrowseGoToRef(string refName, bool showNoRevisionMsg)
        {
            BrowseRepo?.GoToRef(refName, showNoRevisionMsg);
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

            public readonly GitModule Module;

            public event EventHandler<GitRemoteCommandCompletedEventArgs> Completed;

            internal GitRemoteCommand(GitModule module)
            {
                Module = module;
            }

            public void Execute()
            {
                if (CommandText == null)
                {
                    throw new InvalidOperationException("CommandText is required");
                }

                using (var form = new FormRemoteProcess(Module, CommandText))
                {
                    if (Title != null)
                    {
                        form.Text = Title;
                    }

                    if (Remote != null)
                    {
                        form.Remote = Remote;
                    }

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

                Completed?.Invoke(form, e);

                isError = e.IsError;

                return e.Handled;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is GitUICommands other && Equals(other);
        }

        private bool Equals(GitUICommands other)
        {
            return Equals(Module, other.Module);
        }

        public override int GetHashCode()
        {
            return Module.GetHashCode();
        }
    }
}
