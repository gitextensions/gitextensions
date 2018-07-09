﻿using System;
using System.Collections.Generic;
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
using JetBrains.Annotations;

namespace GitUI
{
    /// <summary>Contains methods to invoke GitEx forms, dialogs, etc.</summary>
    public sealed class GitUICommands : IGitUICommands
    {
        private readonly ICommitTemplateManager _commitTemplateManager;
        private readonly IFullPathResolver _fullPathResolver;
        private readonly IFindFilePredicateProvider _findFilePredicateProvider;

        public GitModule Module { get; private set; }
        public ILockableNotifier RepoChangedNotifier { get; }
        public IBrowseRepo BrowseRepo { get; set; }

        public GitUICommands([NotNull] GitModule module)
        {
            Module = module;
            _commitTemplateManager = new CommitTemplateManager(module);
            RepoChangedNotifier = new ActionNotifier(
                () => InvokeEvent(null, PostRepositoryChanged));

            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
            _findFilePredicateProvider = new FindFilePredicateProvider();
        }

        public GitUICommands([CanBeNull] string workingDir)
            : this(new GitModule(workingDir))
        {
        }

        public IGitModule GitModule => Module;

        #region IGitUICommands Members

        public event EventHandler<GitUIEventArgs> PreCheckoutRevision;
        public event EventHandler<GitUIPostActionEventArgs> PostCheckoutRevision;

        public event EventHandler<GitUIEventArgs> PreCheckoutBranch;
        public event EventHandler<GitUIPostActionEventArgs> PostCheckoutBranch;

        public event EventHandler<GitUIEventArgs> PreCommit;
        public event EventHandler<GitUIPostActionEventArgs> PostCommit;

        public event EventHandler<GitUIPostActionEventArgs> PostEditGitIgnore;

        public event EventHandler<GitUIPostActionEventArgs> PostSettings;

        public event EventHandler<GitUIPostActionEventArgs> PostUpdateSubmodules;

        public event EventHandler<GitUIEventArgs> PostBrowseInitialize;

        /// <summary>
        /// listeners for changes being made to repository
        /// </summary>
        public event EventHandler<GitUIEventArgs> PostRepositoryChanged;

        public event EventHandler<GitUIEventArgs> PostRegisterPlugin;

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

        public bool StartBatchFileProcessDialog(string batchFile)
        {
            string tempFileName = Path.ChangeExtension(Path.GetTempFileName(), ".cmd");
            using (var writer = new StreamWriter(tempFileName))
            {
                writer.WriteLine("@prompt $G");
                writer.Write(batchFile);
            }

            FormProcess.ShowDialog(null, Module, "cmd.exe", "/C \"" + tempFileName + "\"");
            File.Delete(tempFileName);
            return true;
        }

        public bool StartCommandLineProcessDialog(IWin32Window owner, IGitCommand command)
        {
            var executed = command.AccessesRemote()
                ? FormRemoteProcess.ShowDialog(owner, Module, command.ToLine())
                : FormProcess.ShowDialog(owner, Module, command.ToLine());

            if (executed && command.ChangesRepoState())
            {
                RepoChangedNotifier.Notify();
            }

            return executed;
        }

        public bool StartCommandLineProcessDialog(IWin32Window owner, string command, string arguments)
        {
            FormProcess.ShowDialog(owner, Module, command, arguments);
            return true;
        }

        public bool StartGitCommandProcessDialog(IWin32Window owner, string arguments)
        {
            FormProcess.ShowDialog(owner, Module, arguments);
            return true;
        }

        public bool StartDeleteBranchDialog(IWin32Window owner, string branch)
        {
            return StartDeleteBranchDialog(owner, new[] { branch });
        }

        public bool StartDeleteBranchDialog(IWin32Window owner, IEnumerable<string> branches)
        {
            return DoActionOnRepo(owner, true, false, null, null, () =>
            {
                using (var form = new FormDeleteBranch(this, branches))
                {
                    form.ShowDialog(owner);
                }

                return true;
            });
        }

        public bool StartDeleteRemoteBranchDialog(IWin32Window owner, string remoteBranch)
        {
            return DoActionOnRepo(owner, true, false, null, null, () =>
                {
                    using (var form = new FormDeleteRemoteBranch(this, remoteBranch))
                    {
                        form.ShowDialog(owner);
                    }

                    return true;
                });
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
            EventHandler<GitUIEventArgs> preEvent, EventHandler<GitUIPostActionEventArgs> postEvent, Func<Form> provideForm)
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
        private bool DoActionOnRepo(IWin32Window owner, bool requiresValidWorkingDir, bool changesRepo,
            EventHandler<GitUIEventArgs> preEvent, EventHandler<GitUIPostActionEventArgs> postEvent,
            [InstantHandle] Func<bool> action)
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

        public bool StartCheckoutBranch(IWin32Window owner, string branch = "", bool remote = false, string[] containRevisions = null)
        {
            return DoActionOnRepo(owner, true, true, PreCheckoutBranch, PostCheckoutBranch, () =>
            {
                using (var form = new FormCheckoutBranch(this, branch, remote, containRevisions))
                {
                    return form.DoDefaultActionOrShow(owner) != DialogResult.Cancel;
                }
            });
        }

        public bool StartCheckoutBranch(IWin32Window owner, string[] containRevisions)
        {
            return StartCheckoutBranch(owner, "", false, containRevisions);
        }

        public bool StartCheckoutBranch(string branch, bool remote)
        {
            return StartCheckoutBranch(null, branch, remote);
        }

        public bool StartCheckoutRemoteBranch(IWin32Window owner, string branch)
        {
            return StartCheckoutBranch(owner, branch, true);
        }

        #endregion Checkout

        public bool StartCompareRevisionsDialog(IWin32Window owner = null)
        {
            bool Action()
            {
                using (var form = new FormLog(this))
                {
                    return form.ShowDialog(owner) == DialogResult.OK;
                }
            }

            return DoActionOnRepo(owner, true, true, null, null, Action);
        }

        public bool StartAddFilesDialog(IWin32Window owner, string addFiles = null)
        {
            return DoActionOnRepo(owner, true, true, null, null, () =>
            {
                using (var form = new FormAddFiles(this, addFiles))
                {
                    form.ShowDialog(owner);
                }

                return true;
            });
        }

        public bool StartCreateBranchDialog(IWin32Window owner = null, GitRevision revision = null)
        {
            bool Action()
            {
                using (var form = new FormCreateBranch(this, revision))
                {
                    return form.ShowDialog(owner) == DialogResult.OK;
                }
            }

            return DoActionOnRepo(owner, true, true, null, null, Action);
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

            return DoActionOnRepo(owner, false, false, null, null, Action);
        }

        public bool StartCloneDialog(IWin32Window owner, string url, EventHandler<GitModuleEventArgs> gitModuleChanged)
        {
            return StartCloneDialog(owner, url, false, gitModuleChanged);
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

        public bool StartInitializeDialog(IWin32Window owner = null, string dir = null, EventHandler<GitModuleEventArgs> gitModuleChanged = null)
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

            return DoActionOnRepo(owner, false, true, null, null, Action);
        }

        public bool StartPullDialogAndPullImmediately(IWin32Window owner = null, string remoteBranch = null, string remote = null, bool fetchAll = false)
        {
            return StartPullDialogAndPullImmediately(out _, owner, remoteBranch, remote, fetchAll);
        }

        /// <param name="pullCompleted">true if pull completed with no errors</param>
        /// <returns>if revision grid should be refreshed</returns>
        public bool StartPullDialogAndPullImmediately(out bool pullCompleted, IWin32Window owner = null, string remoteBranch = null, string remote = null, bool fetchAll = false)
        {
            return StartPullDialogInternal(owner, pullOnShow: true, out pullCompleted, remoteBranch, remote, fetchAll);
        }

        public bool StartPullDialog(IWin32Window owner = null, string remoteBranch = null, string remote = null, bool fetchAll = false)
        {
            return StartPullDialogInternal(owner, pullOnShow: false, out _, remoteBranch, remote, fetchAll);
        }

        private bool StartPullDialogInternal(IWin32Window owner, bool pullOnShow, out bool pullCompleted, string remoteBranch, string remote, bool fetchAll)
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

            bool done = DoActionOnRepo(owner, true, true, null, null, Action);

            pullCompleted = pulled;

            return done;
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

            return DoActionOnRepo(owner, false, false, null, null, Action);
        }

        public bool StartViewPatchDialog(string patchFile)
        {
            return StartViewPatchDialog(null, patchFile);
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

            return DoActionOnRepo(owner, true, false, null, null, Action);
        }

        public void AddCommitTemplate(string key, Func<string> addingText)
        {
            _commitTemplateManager.Register(key, addingText);
        }

        public void RemoveCommitTemplate(string key)
        {
            _commitTemplateManager.Unregister(key);
        }

        public bool StartFormatPatchDialog(IWin32Window owner = null)
        {
            bool Action()
            {
                using (var form = new FormFormatPatch(this))
                {
                    form.ShowDialog(owner);
                }

                return true;
            }

            return DoActionOnRepo(owner, true, false, null, null, Action);
        }

        public bool StartStashDialog(IWin32Window owner = null, bool manageStashes = true)
        {
            bool Action()
            {
                using (var form = new FormStash(this) { ManageStashes = manageStashes })
                {
                    form.ShowDialog(owner);
                }

                return true;
            }

            return DoActionOnRepo(owner, true, false, null, null, Action);
        }

        public bool StartResetChangesDialog(IWin32Window owner = null)
        {
            var unstagedFiles = Module.GetUnstagedFiles();
            return StartResetChangesDialog(owner, unstagedFiles, false);
        }

        public bool StartResetChangesDialog(IWin32Window owner, IReadOnlyCollection<GitItemStatus> unstagedFiles, bool onlyUnstaged)
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

        private bool StartResetChangesDialog(string fileName)
        {
            // Show a form asking the user if they want to reset the changes.
            FormResetChanges.ActionEnum resetAction = FormResetChanges.ShowResetDialog(null, true, false);

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

        public bool StartRevertCommitDialog(IWin32Window owner, GitRevision revision)
        {
            bool Action()
            {
                using (var form = new FormRevertCommit(this, revision))
                {
                    return form.ShowDialog(owner) == DialogResult.OK;
                }
            }

            return DoActionOnRepo(owner, true, true, null, null, Action);
        }

        public bool StartResolveConflictsDialog(IWin32Window owner = null, bool offerCommit = true)
        {
            bool Action()
            {
                using (var form = new FormResolveConflicts(this, offerCommit))
                {
                    form.ShowDialog(owner);
                }

                return true;
            }

            return DoActionOnRepo(owner, true, true, null, null, Action);
        }

        public bool StartCherryPickDialog(IWin32Window owner = null, GitRevision revision = null)
        {
            bool Action()
            {
                using (var form = new FormCherryPick(this, revision))
                {
                    return form.ShowDialog(owner) == DialogResult.OK;
                }
            }

            return DoActionOnRepo(owner, true, true, null, null, Action);
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

                    // ReSharper disable once PossibleMultipleEnumeration
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

            return DoActionOnRepo(owner, true, true, null, null, Action);
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

            return DoActionOnRepo(owner, true, false, null, null, Action);
        }

        public bool StartCreateTagDialog(IWin32Window owner = null)
        {
            bool Action()
            {
                using (var form = new FormCreateTag(this, null))
                {
                    return form.ShowDialog(owner) == DialogResult.OK;
                }
            }

            return DoActionOnRepo(owner, true, true, null, null, Action);
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

            return DoActionOnRepo(owner, true, true, null, null, Action);
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

            return DoActionOnRepo(owner, true, false, null, PostEditGitIgnore, Action);
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

            return DoActionOnRepo(owner, true, false, null, PostEditGitIgnore, Action);
        }

        public bool StartSettingsDialog(IWin32Window owner = null, SettingsPageReference initialPage = null)
        {
            bool Action()
            {
                FormSettings.ShowSettingsDialog(this, owner, initialPage);

                return true;
            }

            return DoActionOnRepo(owner, false, true, null, PostSettings, Action);
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
            return DoActionOnRepo(owner, true, false, null, null, () =>
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

        public bool StartMailMapDialog(IWin32Window owner = null)
        {
            bool Action()
            {
                using (var form = new FormMailMap(this))
                {
                    form.ShowDialog(owner);
                }

                return true;
            }

            return DoActionOnRepo(owner, true, false, null, null, Action);
        }

        public bool StartVerifyDatabaseDialog(IWin32Window owner = null)
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
            return DoActionOnRepo(owner, true, true, null, null, Action);
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

            return DoActionOnRepo(owner, true, true, null, null, Action);
        }

        public bool StartRemotesDialog()
        {
            return StartRemotesDialog(null);
        }

        public bool StartRebase(IWin32Window owner, string onto)
        {
            return StartRebaseDialog(owner, from: "", to: null, onto: onto,
                interactive: false, startRebaseImmediately: AppSettings.SkipRebaseDialog);
        }

        public bool StartTheContinueRebaseDialog(IWin32Window owner)
        {
            return StartRebaseDialog(owner, from: "", to: null, onto: null, interactive: false, startRebaseImmediately: false);
        }

        public bool StartInteractiveRebase(IWin32Window owner, string onto)
        {
            return StartRebaseDialog(owner, from: "", to: null, onto: onto,
                interactive: true, startRebaseImmediately: AppSettings.SkipRebaseDialog);
        }

        public bool StartRebaseDialogWithAdvOptions(IWin32Window owner, string onto)
        {
            return StartRebaseDialog(owner, from: "", to: null, onto, interactive: false, startRebaseImmediately: false);
        }

        public bool StartRebaseDialog(IWin32Window owner, string onto)
        {
            return StartRebaseDialog(owner, from: "", to: null, onto, interactive: false, startRebaseImmediately: false);
        }

        public bool StartRebaseDialog(IWin32Window owner, string from, string to, string onto,
            bool interactive = false, bool startRebaseImmediately = true)
        {
            bool Action()
            {
                using (var form = new FormRebase(this, from, to, onto, interactive, startRebaseImmediately))
                {
                    form.ShowDialog(owner);
                }

                return true;
            }

            return DoActionOnRepo(owner, true, true, null, null, Action);
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

            return DoActionOnRepo(owner, true, true, null, null, Action);
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

            return DoActionOnRepo(owner, true, true, null, null, Action);
        }

        public bool StartUpdateSubmodulesDialog(IWin32Window owner)
        {
            bool Action()
            {
                return FormProcess.ShowDialog(owner, Module, GitCommandHelpers.SubmoduleUpdateCmd(""));
            }

            return DoActionOnRepo(owner, true, true, null, PostUpdateSubmodules, Action);
        }

        public bool StartSyncSubmodulesDialog(IWin32Window owner)
        {
            bool Action()
            {
                return FormProcess.ShowDialog(owner, Module, GitCommandHelpers.SubmoduleSyncCmd(""));
            }

            return DoActionOnRepo(owner, true, true, null, null, Action);
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

        public bool StartRepoSettingsDialog(IWin32Window owner)
        {
            return StartSettingsDialog(owner, CommandsDialogs.SettingsDialog.Pages.GitConfigSettingsPage.GetPageReference());
        }

        public bool StartBrowseDialog(IWin32Window owner = null, string filter = "", string selectedCommit = null, bool startWithDashboard = false)
        {
            var form = new FormBrowse(this, filter, selectedCommit, startWithDashboard);

            if (Application.MessageLoop)
            {
                form.Show(owner);
            }
            else
            {
                Application.Run(form);
            }

            return true;
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

            ShowModelessForm(owner, true, null, null, ProvideForm);
        }

        public void OpenWithDifftool(IWin32Window owner, IReadOnlyList<GitRevision> revisions, string fileName, string oldFileName, RevisionDiffKind diffKind, bool isTracked)
        {
            // Note: Order in revisions is that first clicked is last in array

            string error = RevisionDiffInfoProvider.Get(revisions, diffKind,
                out var extraDiffArgs, out var firstRevision, out var secondRevision);

            if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show(owner, error);
            }
            else
            {
                string output = Module.OpenWithDifftool(fileName, oldFileName, firstRevision, secondRevision, extraDiffArgs, isTracked);
                if (!string.IsNullOrEmpty(output))
                {
                    MessageBox.Show(owner, output);
                }
            }
        }

        public FormDiff ShowFormDiff(bool firstParentIsValid, ObjectId baseCommitSha,
            ObjectId headCommitSha, string baseCommitDisplayStr, string headCommitDisplayStr)
        {
            var diffForm = new FormDiff(this, firstParentIsValid, baseCommitSha.ToString(),
                headCommitSha.ToString(), baseCommitDisplayStr, headCommitDisplayStr);
            diffForm.Show();
            diffForm.ShowInTaskbar = true;

            return diffForm;
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

            bool done = DoActionOnRepo(owner, true, true, null, null, Action);

            pushCompleted = pushed;

            return done;
        }

        public bool StartPushDialog(IWin32Window owner, bool pushOnShow)
        {
            return StartPushDialog(owner, pushOnShow, forceWithLease: false, out _);
        }

        public bool StartApplyPatchDialog(IWin32Window owner, string patchFile = null)
        {
            return DoActionOnRepo(owner, true, false, null, null, () =>
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

            return DoActionOnRepo(owner, true, false, null, null, Action);
        }

        private bool InvokeEvent(IWin32Window ownerForm, [CanBeNull] EventHandler<GitUIEventArgs> gitUIEventHandler)
        {
            return InvokeEvent(this, ownerForm, gitUIEventHandler);
        }

        private void InvokePostEvent(IWin32Window ownerForm, bool actionDone, EventHandler<GitUIPostActionEventArgs> gitUIEventHandler)
        {
            if (gitUIEventHandler != null)
            {
                var e = new GitUIPostActionEventArgs(ownerForm, this, actionDone);
                gitUIEventHandler(this, e);
            }
        }

        private bool InvokeEvent(object sender, IWin32Window ownerForm, [CanBeNull] EventHandler<GitUIEventArgs> gitUIEventHandler)
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
                                    frm.Show(owner);
                                });
        }

        public void StartCreatePullRequest(IWin32Window owner)
        {
            List<IRepositoryHostPlugin> relevantHosts =
                PluginRegistry.GitHosters.Where(gh => gh.GitModuleIsRelevantToMe(Module)).ToList();

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

                    form.Show(owner);
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
            // TODO most of these calls should check return values and set the exit code accordingly
#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
            switch (args[1])
            {
                case "about":
                    Application.Run(new FormAbout
                    {
                        StartPosition = FormStartPosition.CenterScreen
                    });
                    return;
                case "add":
                case "addfiles":
                    StartAddFilesDialog(null, args.Length == 3 ? args[2] : ".");
                    return;
                case "apply":       // [filename]
                case "applypatch":
                    StartApplyPatchDialog(null, args.Length == 3 ? args[2] : "");
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
                    StartCheckoutBranch(null);
                    return;
                case "checkoutrevision":
                    StartCheckoutRevisionDialog(null);
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
                    StartEditGitIgnoreDialog(null, false);
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

            StartMergeBranchDialog(null, branch);
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
            StartBrowseDialog(null, GetParameterOrEmptyStringAsDefault(args, "-filter"), GetParameterOrEmptyStringAsDefault(args, "-commit"));
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

            c.StartBrowseDialog(null, GetParameterOrEmptyStringAsDefault(args, "-filter"));
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

            StartFileHistoryDialog(null, fileHistoryFileName);
        }

        private void RunCloneCommand(string[] args)
        {
            StartCloneDialog(null, args.Length > 2 ? args[2] : null);
        }

        private void RunInitCommand(string[] args)
        {
            StartInitializeDialog(null, args.Length > 2 ? args[2] : null);
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

            DoActionOnRepo(null, true, false, null, null, () =>
            {
                using (var frm = new FormBlame(this, filenameFromBlame, null, initialLine))
                {
                    frm.ShowDialog(null);
                }

                return true;
            });
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
            var arguments = new Dictionary<string, string>();

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

        private IEnumerable<string> FindFileMatches(string name)
        {
            var candidates = Module.GetFullTree("HEAD");

            var predicate = _findFilePredicateProvider.Get(name, Module.WorkingDir);

            return candidates.Where(predicate);
        }

        private void Commit(Dictionary<string, string> arguments)
        {
            StartCommitDialog(null, arguments.ContainsKey("quiet"));
        }

        private void Push(Dictionary<string, string> arguments)
        {
            StartPushDialog(null, arguments.ContainsKey("quiet"));
        }

        private void Pull(Dictionary<string, string> arguments)
        {
            UpdateSettingsBasedOnArguments(arguments);

            string remoteBranch = null;
            if (arguments.ContainsKey("remotebranch"))
            {
                remoteBranch = arguments["remotebranch"];
            }

            var isQuiet = arguments.ContainsKey("quiet");

            if (isQuiet)
            {
                StartPullDialogAndPullImmediately(remoteBranch: remoteBranch);
            }
            else
            {
                StartPullDialog(remoteBranch: remoteBranch);
            }
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

        #region Nested class: GitRemoteCommand

        private sealed class GitRemoteCommand : IGitRemoteCommand
        {
            public object OwnerForm { get; set; }
            public string Remote { get; set; }
            public string Title { get; set; }
            public string CommandText { get; set; }
            public bool ErrorOccurred { get; private set; }
            public string CommandOutput { get; private set; }

            private readonly GitModule _module;

            public event EventHandler<GitRemoteCommandCompletedEventArgs> Completed;

            internal GitRemoteCommand(GitModule module)
            {
                _module = module;
            }

            public void Execute()
            {
                if (CommandText == null)
                {
                    throw new InvalidOperationException("CommandText is required");
                }

                using (var form = new FormRemoteProcess(_module, CommandText))
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

        #endregion
    }
}
