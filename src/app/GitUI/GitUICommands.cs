using System.ComponentModel.Design;
using System.Diagnostics;
using System.Text;
using GitCommands;
using GitCommands.Git;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Settings;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.RepoHosting;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI
{
    /// <summary>Contains methods to invoke GitEx forms, dialogs, etc.</summary>
    public sealed class GitUICommands : IGitUICommands
    {
        private const string BlameHistoryCommand = "blamehistory";
        private const string FileHistoryCommand = "filehistory";

        private const string FilterByRevisionArg = "--filter-by-revision";
        private const string PathFilterArg = "--pathFilter";

        private readonly IServiceProvider _serviceProvider;
        private readonly ICommitTemplateManager _commitTemplateManager;
        private readonly IFullPathResolver _fullPathResolver;
        private readonly IFindFilePredicateProvider _findFilePredicateProvider;

        public static IServiceProvider EmptyServiceProvider = new ServiceContainer();

        public IGitModule Module { get; private set; }
        public ILockableNotifier RepoChangedNotifier { get; }
        public IBrowseRepo? BrowseRepo { get; set; }

        public GitUICommands(IServiceProvider serviceProvider, IGitModule module)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            ArgumentNullException.ThrowIfNull(module);

            _serviceProvider = serviceProvider;
            Module = module;

            _commitTemplateManager = new CommitTemplateManager(() => module);
            RepoChangedNotifier = new ActionNotifier(
                () => InvokeEvent(null, PostRepositoryChanged));

            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
            _findFilePredicateProvider = new FindFilePredicateProvider();
        }

        #region Events

        public event EventHandler<GitUIEventArgs>? PreCheckoutRevision;
        public event EventHandler<GitUIPostActionEventArgs>? PostCheckoutRevision;

        public event EventHandler<GitUIEventArgs>? PreCheckoutBranch;
        public event EventHandler<GitUIPostActionEventArgs>? PostCheckoutBranch;

        public event EventHandler<GitUIEventArgs>? PreCommit;
        public event EventHandler<GitUIPostActionEventArgs>? PostCommit;

        public event EventHandler<GitUIPostActionEventArgs>? PostEditGitIgnore;

        public event EventHandler<GitUIPostActionEventArgs>? PostSettings;

        public event EventHandler<GitUIPostActionEventArgs>? PostUpdateSubmodules;

        public event EventHandler<GitUIEventArgs>? PostBrowseInitialize;

        /// <summary>
        /// listeners for changes being made to repository
        /// </summary>
        public event EventHandler<GitUIEventArgs>? PostRepositoryChanged;

        public event EventHandler<GitUIEventArgs>? PostRegisterPlugin;

        #endregion

        public object? GetService(Type serviceType) => _serviceProvider.GetService(serviceType);

        private bool RequiresValidWorkingDir(object? owner)
        {
            if (!Module.IsValidGitWorkingDir())
            {
                MessageBoxes.NotValidGitDirectory(owner as IWin32Window);
                return false;
            }

            return true;
        }

        public void StartBatchFileProcessDialog(string batchFile)
        {
            string tempFile = Path.Combine(Path.GetTempPath(), $"GitExtensions-{Guid.NewGuid():N}.cmd");

            try
            {
                using (StreamWriter writer = new(tempFile))
                {
                    writer.WriteLine("@prompt $G");
                    writer.Write(batchFile);
                }

                FormProcess.ShowDialog(owner: null, this, arguments: $"/C \"{tempFile}\"", Module.WorkingDir, input: null, useDialogSettings: true, process: "cmd.exe");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        public bool StartCommandLineProcessDialog(IWin32Window? owner, IGitCommand command)
        {
            bool success = command.AccessesRemote
                ? FormRemoteProcess.ShowDialog(owner, this, command.Arguments)
                : FormProcess.ShowDialog(owner, this, arguments: command.Arguments, Module.WorkingDir, input: null, useDialogSettings: true);

            if (success && command.ChangesRepoState)
            {
                RepoChangedNotifier.Notify();
            }

            return success;
        }

        public bool StartCommandLineProcessDialog(IWin32Window? owner, string? command, ArgumentString arguments)
        {
            return FormProcess.ShowDialog(owner, this, arguments, Module.WorkingDir, input: null, useDialogSettings: true, process: command);
        }

        public bool StartGitCommandProcessDialog(IWin32Window? owner, ArgumentString arguments)
        {
            return FormProcess.ShowDialog(owner, this, arguments, Module.WorkingDir, input: null, useDialogSettings: true);
        }

        public bool StartDeleteBranchDialog(IWin32Window? owner, string branch)
        {
            return StartDeleteBranchDialog(owner, new[] { branch });
        }

        public bool StartDeleteBranchDialog(IWin32Window? owner, IEnumerable<string> branches)
        {
            return DoActionOnRepo(owner, action: () =>
            {
                using FormDeleteBranch form = new(this, branches);
                form.ShowDialog(owner);
                return true;
            }, changesRepo: false);
        }

        public bool StartDeleteRemoteBranchDialog(IWin32Window? owner, string remoteBranch)
        {
            return DoActionOnRepo(owner, action: () =>
            {
                using FormDeleteRemoteBranch form = new(this, remoteBranch);
                form.ShowDialog(owner);
                return true;
            }, changesRepo: false);
        }

        public bool StartCheckoutRevisionDialog(IWin32Window? owner, string? revision = null)
        {
            return DoActionOnRepo(owner, action: () =>
            {
                using FormCheckoutRevision form = new(this);
                form.SetRevision(revision);
                return form.ShowDialog(owner) == DialogResult.OK;
            }, preEvent: PreCheckoutRevision, postEvent: PostCheckoutRevision);
        }

        public bool StartResetCurrentBranchDialog(IWin32Window? owner, string branch)
        {
            ObjectId objectId = Module.RevParse(branch);
            if (objectId is null)
            {
                MessageBox.Show($"Branch \"{branch}\" could not be resolved.", TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            using FormResetCurrentBranch form = FormResetCurrentBranch.Create(this, Module.GetRevision(objectId));
            return form.ShowDialog(owner) == DialogResult.OK;
        }

        public bool StashSave(IWin32Window? owner, bool includeUntrackedFiles, bool keepIndex = false, string message = "", IReadOnlyList<string>? selectedFiles = null)
        {
            bool Action()
            {
                ArgumentString arguments = Commands.StashSave(includeUntrackedFiles, keepIndex, message, selectedFiles);
                FormProcess.ShowDialog(owner, this, arguments, Module.WorkingDir, input: null, useDialogSettings: true);

                // git-stash may have changed commits also if aborted, the grid must be refreshed
                return true;
            }

            return DoActionOnRepo(owner, Action);
        }

        public bool StashStaged(IWin32Window? owner)
        {
            bool Action()
            {
                FormProcess.ShowDialog(owner, this, arguments: "stash --staged", Module.WorkingDir, input: null, useDialogSettings: true);

                // git-stash may have changed commits also if aborted, the grid must be refreshed
                return true;
            }

            return DoActionOnRepo(owner, Action);
        }

        public bool StashPop(IWin32Window? owner, string stashName = "")
        {
            bool Action()
            {
                FormProcess.ShowDialog(owner, this, arguments: $"stash pop {stashName.QuoteNE()}", Module.WorkingDir, input: null, useDialogSettings: true);
                MergeConflictHandler.HandleMergeConflicts(this, owner, false, false);

                // git-stash may have changed commits also if aborted, the grid must be refreshed
                return true;
            }

            return DoActionOnRepo(owner, Action);
        }

        public bool StashDrop(IWin32Window? owner, string stashName)
        {
            bool Action()
            {
                FormProcess.ShowDialog(owner, this, arguments: $"stash drop {stashName.Quote()}", Module.WorkingDir, input: null, useDialogSettings: true);

                // git-stash may have changed commits also if aborted, the grid must be refreshed
                return true;
            }

            return DoActionOnRepo(owner, Action);
        }

        public bool StashApply(IWin32Window? owner, string stashName)
        {
            bool Action()
            {
                FormProcess.ShowDialog(owner, this, arguments: $"stash apply {stashName.Quote()}", Module.WorkingDir, input: null, useDialogSettings: true);
                MergeConflictHandler.HandleMergeConflicts(this, owner, false, false);

                // git-stash may have changed commits also if aborted, the grid must be refreshed
                return true;
            }

            return DoActionOnRepo(owner, Action);
        }

        public void ShowModelessForm(IWin32Window? owner, bool requiresValidWorkingDir,
            EventHandler<GitUIEventArgs>? preEvent, EventHandler<GitUIPostActionEventArgs>? postEvent, Func<Form> provideForm)
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

        /// <param name="requiresValidWorkingDir">If action requires valid working directory.</param>
        /// <param name="owner">Owner window.</param>
        /// <param name="changesRepo">if successfully done action changes repo state.</param>
        /// <param name="preEvent">Event invoked before performing action.</param>
        /// <param name="postEvent">Event invoked after performing action.</param>
        /// <param name="action">Action to do. Return true to indicate that the action was successfully done.</param>
        /// <returns>true if action was successfully done, false otherwise.</returns>
        private bool DoActionOnRepo(
            IWin32Window? owner,
            [InstantHandle] Func<bool> action,
            bool requiresValidWorkingDir = true,
            bool changesRepo = true,
            EventHandler<GitUIEventArgs>? preEvent = null,
            EventHandler<GitUIPostActionEventArgs>? postEvent = null)
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
                // The action may not have required a valid working directory to run, but if there isn't one,
                // we shouldn't send a "repo changed" notify.
                bool requestNotify = actionDone && changesRepo && Module.IsValidGitWorkingDir();
                RepoChangedNotifier.UnLock(requestNotify);
            }

            return actionDone;
        }

        public bool DoActionOnRepo(Func<bool> action)
        {
            return DoActionOnRepo(owner: null, action, requiresValidWorkingDir: false);
        }

        #region Checkout

        public bool StartCheckoutBranch(IWin32Window? owner, string branch = "", bool remote = false, IReadOnlyList<ObjectId>? containRevisions = null)
        {
            return DoActionOnRepo(owner, action: () =>
            {
                using FormCheckoutBranch form = new(this, branch, remote, containRevisions);
                return form.DoDefaultActionOrShow(owner) != DialogResult.Cancel;
            }, preEvent: PreCheckoutBranch, postEvent: PostCheckoutBranch);
        }

        public bool StartCheckoutBranch(IWin32Window? owner, IReadOnlyList<ObjectId>? containRevisions)
        {
            return StartCheckoutBranch(owner, "", false, containRevisions);
        }

        public bool StartCheckoutRemoteBranch(IWin32Window? owner, string branch)
        {
            return StartCheckoutBranch(owner, branch, true);
        }

        #endregion

        /// <summary>
        /// Launches a new GE instance.
        /// </summary>
        /// <param name="arguments">The command line arguments.</param>
        /// <param name="workingDir">The working directory for the new process.</param>
        /// <returns>The <see cref="IProcess"/> object for controlling the launched instance.</returns>
        public static IProcess Launch(string arguments, string workingDir = "")
            => new Executable(Application.ExecutablePath, workingDir).Start(arguments);

        /// <summary>
        /// Launch FormBrowse in a new GE instance.
        /// </summary>
        /// <param name="workingDir">The working directory for the new process.</param>
        /// <param name="selectedId">The optional commit to be selected.</param>
        /// <param name="firstId">The first commit to be selected, the first commit in a diff.</param>
        internal static void LaunchBrowse(string workingDir = "", ObjectId? selectedId = null, ObjectId? firstId = null)
        {
            if (!Directory.Exists(workingDir))
            {
                MessageBoxes.GitExtensionsDirectoryDoesNotExist(owner: null, workingDir);
                return;
            }

            StringBuilder arguments = new("browse");

            if (selectedId is null)
            {
                selectedId = firstId;
                firstId = null;
            }

            if (selectedId is not null)
            {
                arguments.Append(" -commit=").Append(selectedId);
                if (firstId is not null)
                {
                    arguments.Append(',').Append(firstId);
                }
            }

            Launch(arguments.ToString(), workingDir);
        }

        public bool StartCompareRevisionsDialog(IWin32Window? owner = null)
        {
            bool Action()
            {
                using FormLog form = new(this);
                return form.ShowDialog(owner) == DialogResult.OK;
            }

            return DoActionOnRepo(owner, Action);
        }

        public bool StartAddFilesDialog(IWin32Window? owner, string? addFiles = null)
        {
            return DoActionOnRepo(owner, action: () =>
            {
                using FormAddFiles form = new(this, addFiles);
                form.ShowDialog(owner);
                return true;
            });
        }

        public bool StartCreateBranchDialog(IWin32Window? owner, string? branch)
        {
            ObjectId objectId = Module.RevParse(branch);
            if (objectId is null)
            {
                MessageBox.Show($"Branch \"{branch}\" could not be resolved.", TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return StartCreateBranchDialog(owner, objectId);
        }

        public bool StartCreateBranchDialog(IWin32Window? owner = null, ObjectId? objectId = null, string? newBranchNamePrefix = null)
        {
            if (Module.IsBareRepository() || objectId?.IsArtificial is true)
            {
                return false;
            }

            bool Action()
            {
                using FormCreateBranch form = new(this, objectId, newBranchNamePrefix);
                return form.ShowDialog(owner) == DialogResult.OK;
            }

            return DoActionOnRepo(owner, Action);
        }

        public bool StartCloneDialog(IWin32Window? owner, string? url = null, bool openedFromProtocolHandler = false, EventHandler<GitModuleEventArgs>? gitModuleChanged = null)
        {
            bool Action()
            {
                using FormClone form = new(this, url, openedFromProtocolHandler, gitModuleChanged);
                form.ShowDialog(owner);
                return true;
            }

            return DoActionOnRepo(owner, Action, requiresValidWorkingDir: false, changesRepo: false);
        }

        public bool StartCloneDialog(IWin32Window? owner, string url, EventHandler<GitModuleEventArgs> gitModuleChanged)
        {
            return StartCloneDialog(owner, url, false, gitModuleChanged);
        }

        public bool StartCleanupRepositoryDialog(IWin32Window? owner = null, string? path = null)
        {
            using FormCleanupRepository form = new(this);
            form.SetPathArgument(path);
            form.ShowDialog(owner);

            return true;
        }

        public bool StartSquashCommitDialog(IWin32Window? owner, GitRevision revision)
        {
            bool Action()
            {
                using FormCommit form = new(this, CommitKind.Squash, revision);
                form.ShowDialog(owner);
                return true;
            }

            return DoActionOnRepo(Action);
        }

        public bool StartFixupCommitDialog(IWin32Window? owner, GitRevision revision)
        {
            bool Action()
            {
                using FormCommit form = new(this, CommitKind.Fixup, revision);
                form.ShowDialog(owner);
                return true;
            }

            return DoActionOnRepo(Action);
        }

        public bool StartAmendCommitDialog(IWin32Window? owner, GitRevision revision)
        {
            bool Action()
            {
                using FormCommit form = new(this, CommitKind.Amend, revision);
                form.ShowDialog(owner);
                return true;
            }

            return DoActionOnRepo(Action);
        }

        public bool StartCommitDialog(IWin32Window? owner, string? commitMessage = null, bool showOnlyWhenChanges = false)
        {
            if (Module.IsBareRepository())
            {
                return false;
            }

            // Commit dialog can be opened on its own without the main form
            // If it is opened by itself, we need to ensure plugins are loaded because some of them
            // may have hooks into the commit flow
            bool werePluginsRegistered = PluginRegistry.PluginsRegistered;

            try
            {
                // Load plugins synchronously
                // if the commit dialog is opened from the main form, all plugins are already loaded and we return instantly,
                // if the dialog is loaded on its own, plugins need to be loaded before we load the form
                if (!werePluginsRegistered)
                {
                    PluginRegistry.InitializeForCommitForm();
                    PluginRegistry.Register(this);
                }
            }
            catch (Exception exception)
            {
                // Nothing: we don't want plugin loading to crash the application here
                Trace.WriteLine(exception);
            }

            bool Action()
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                using FormCommit form = new(this, commitMessage: commitMessage);
                if (showOnlyWhenChanges)
                {
                    form.ShowDialogWhenChanges(owner);
                }
                else
                {
                    form.ShowDialog(owner);
                }

                return true;
            }

            try
            {
                return DoActionOnRepo(owner, Action, changesRepo: false, preEvent: PreCommit, postEvent: PostCommit);
            }
            finally
            {
                try
                {
                    if (!werePluginsRegistered)
                    {
                        PluginRegistry.Unregister(this);
                    }
                }
                catch (Exception exception)
                {
                    // Nothing: we don't want plugin loading to crash the application here
                    Trace.WriteLine(exception);
                }
            }
        }

        public bool StartInitializeDialog(IWin32Window? owner = null, string? dir = null, EventHandler<GitModuleEventArgs>? gitModuleChanged = null)
        {
            bool Action()
            {
                dir ??= Module.IsValidGitWorkingDir() ? Module.WorkingDir : string.Empty;

                using FormInit frm = new(dir, gitModuleChanged);
                frm.ShowDialog(owner);
                return true;
            }

            return DoActionOnRepo(owner, Action, requiresValidWorkingDir: false, changesRepo: false);
        }

        public bool StartPullDialogAndPullImmediately(IWin32Window? owner = null, string? remoteBranch = null, string? remote = null, GitPullAction pullAction = GitPullAction.None)
        {
            return StartPullDialogAndPullImmediately(out _, owner, remoteBranch, remote, pullAction);
        }

        /// <param name="pullCompleted">true if pull completed with no errors.</param>
        /// <returns>if revision grid should be refreshed.</returns>
        public bool StartPullDialogAndPullImmediately(out bool pullCompleted, IWin32Window? owner = null, string? remoteBranch = null, string? remote = null, GitPullAction pullAction = GitPullAction.None)
        {
            return StartPullDialogInternal(owner, pullOnShow: true, out pullCompleted, remoteBranch, remote, pullAction);
        }

        public bool StartPullDialog(IWin32Window? owner = null, string? remoteBranch = null, string? remote = null, GitPullAction pullAction = GitPullAction.None)
        {
            return StartPullDialogInternal(owner, pullOnShow: false, out _, remoteBranch, remote, pullAction);
        }

        private bool StartPullDialogInternal(IWin32Window? owner, bool pullOnShow, out bool pullCompleted, string? remoteBranch, string? remote, GitPullAction pullAction)
        {
            bool pulled = false;

            bool Action()
            {
                using FormPull formPull = new(this, remoteBranch, remote, pullAction);
                DialogResult dlgResult = pullOnShow
                    ? formPull.PullAndShowDialogWhenFailed(owner, remote, pullAction)
                    : formPull.ShowDialog(owner);

                if (dlgResult == DialogResult.OK)
                {
                    pulled = !formPull.ErrorOccurred;
                }

                return dlgResult == DialogResult.OK;
            }

            bool done = DoActionOnRepo(owner, Action);

            pullCompleted = pulled;

            return done;
        }

        public bool StartViewPatchDialog(IWin32Window? owner, string? patchFile = null)
        {
            bool Action()
            {
                using FormViewPatch viewPatch = new(this);
                if (!string.IsNullOrEmpty(patchFile))
                {
                    viewPatch.LoadPatch(patchFile);
                }

                viewPatch.ShowDialog(owner);

                return true;
            }

            return DoActionOnRepo(owner, Action, requiresValidWorkingDir: false, changesRepo: false);
        }

        public bool StartFormCommitDiff(ObjectId objectId)
        {
            bool Action()
            {
                using FormCommitDiff viewPatch = new(this, objectId);
                viewPatch.ShowDialog(null);
                return true;
            }

            return DoActionOnRepo(null, Action, requiresValidWorkingDir: false, changesRepo: false);
        }

        public bool StartViewPatchDialog(string patchFile)
        {
            return StartViewPatchDialog(null, patchFile);
        }

        public bool StartSparseWorkingCopyDialog(IWin32Window? owner)
        {
            bool Action()
            {
                using FormSparseWorkingCopy form = new(this);
                form.ShowDialog(owner);
                return true;
            }

            return DoActionOnRepo(owner, Action, changesRepo: false);
        }

        public void AddCommitTemplate(string key, Func<string> addingText, Image? icon)
        {
            _commitTemplateManager.Register(key, addingText, icon);
        }

        public void RemoveCommitTemplate(string key)
        {
            _commitTemplateManager.Unregister(key);
        }

        public bool StartFormatPatchDialog(IWin32Window? owner = null)
        {
            bool Action()
            {
                using FormFormatPatch form = new(this);
                form.ShowDialog(owner);
                return true;
            }

            return DoActionOnRepo(owner, Action, changesRepo: false);
        }

        public bool StartStashDialog(IWin32Window? owner = null, bool manageStashes = true, string initialStash = null)
        {
            bool Action()
            {
                using FormStash form = new(this, initialStash) { ManageStashes = manageStashes };
                form.ShowDialog(owner);
                return true;
            }

            return DoActionOnRepo(owner, Action, changesRepo: false);
        }

        /// <summary>
        /// Reset all changes to HEAD.
        /// </summary>
        /// <param name="owner">Owner window.</param>
        /// <param name="workTreeFiles">Worktree files, to determine the status for the popup dialog.</param>
        /// <param name="onlyWorkTree">Only reset worktree files.</param>
        /// <returns><see langword="true"/> if executed.</returns>
        public bool StartResetChangesDialog(IWin32Window? owner, IReadOnlyCollection<GitItemStatus> workTreeFiles, bool onlyWorkTree)
        {
            // Show a form asking the user if they want to reset the changes.
            FormResetChanges.ActionEnum resetType = FormResetChanges.ShowResetDialog(owner, hasExistingFiles: workTreeFiles.Any(item => !item.IsNew), hasNewFiles: workTreeFiles.Any(item => item.IsNew));

            if (resetType == FormResetChanges.ActionEnum.Cancel)
            {
                return false;
            }

            return DoActionOnRepo(owner, Action);

            bool Action()
            {
                return Module.ResetAllChanges(clean: resetType == FormResetChanges.ActionEnum.ResetAndDelete, onlyWorkTree);
            }
        }

        private bool StartResetChangesDialog(string fileName)
        {
            // Show a form asking the user if they want to reset the changes.
            FormResetChanges.ActionEnum resetType = FormResetChanges.ShowResetDialog(null, hasExistingFiles: true, hasNewFiles: false);

            if (resetType == FormResetChanges.ActionEnum.Cancel)
            {
                return false;
            }

            using (WaitCursorScope.Enter())
            {
                // Reset all changes.
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return Module.ResetAllChanges(clean: resetType == FormResetChanges.ActionEnum.ResetAndDelete, onlyWorkTree: false);
                }

                string filePath = Path.GetRelativePath(Module.WorkingDir, fileName).ToPosixPath();
                List<GitItemStatus> selectedItems = Module.GetAllChangedFilesWithSubmodulesStatus(cancellationToken: default).Where(item => item.Name == filePath).ToList();
                if (selectedItems.Count < 1)
                {
                    return false;
                }

                Module.ResetChanges(resetId: null, selectedItems, resetAndDelete: resetType == FormResetChanges.ActionEnum.ResetAndDelete, _fullPathResolver, out StringBuilder output, progressAction: null);
                if (output.Length > 0)
                {
                    MessageBox.Show(null, output.ToString(), TranslatedStrings.ResetChangesCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return true;
        }

        public bool StartRevertCommitDialog(IWin32Window? owner, GitRevision revision)
        {
            bool Action()
            {
                using FormRevertCommit form = new(this, revision);
                return form.ShowDialog(owner) == DialogResult.OK;
            }

            return DoActionOnRepo(owner, Action);
        }

        public bool StartResolveConflictsDialog(IWin32Window? owner = null, bool offerCommit = true)
        {
            bool Action()
            {
                using FormResolveConflicts form = new(this, offerCommit);
                form.ShowDialog(owner);
                return true;
            }

            return DoActionOnRepo(owner, Action);
        }

        public bool StartCherryPickDialog(IWin32Window? owner = null, GitRevision? revision = null)
        {
            bool Action()
            {
                using FormCherryPick form = new(this, revision);
                return form.ShowDialog(owner) == DialogResult.OK;
            }

            return DoActionOnRepo(owner, Action);
        }

        public bool StartCherryPickDialog(IWin32Window? owner, IEnumerable<GitRevision> revisions)
        {
            if (revisions is null)
            {
                throw new ArgumentNullException(nameof(revisions));
            }

            bool Action()
            {
                FormCherryPick? prevForm = null;

                try
                {
                    bool repoChanged = false;

                    // ReSharper disable once PossibleMultipleEnumeration
                    foreach (GitRevision r in revisions)
                    {
                        FormCherryPick frm = new(this, r);
                        if (prevForm is not null)
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

            return DoActionOnRepo(owner, Action);
        }

        /// <summary>Start Merge dialog, using the specified branch.</summary>
        /// <param name="owner">Owner of the dialog.</param>
        /// <param name="branch">Branch to merge into the current branch.</param>
        public bool StartMergeBranchDialog(IWin32Window? owner, string? branch)
        {
            bool Action()
            {
                using FormMergeBranch form = new(this, branch);
                form.ShowDialog(owner);
                return true;
            }

            return DoActionOnRepo(owner, Action, changesRepo: false);
        }

        public bool StartCreateTagDialog(IWin32Window? owner = null, GitRevision? revision = null)
        {
            if (revision?.IsArtificial is true)
            {
                return false;
            }

            bool Action()
            {
                using FormCreateTag form = new(this, revision?.ObjectId);
                return form.ShowDialog(owner) == DialogResult.OK;
            }

            return DoActionOnRepo(owner, Action);
        }

        public bool StartDeleteTagDialog(IWin32Window? owner, string? tag)
        {
            bool Action()
            {
                using FormDeleteTag form = new(this, tag);
                return form.ShowDialog(owner) == DialogResult.OK;
            }

            return DoActionOnRepo(owner, Action);
        }

        public bool StartEditGitIgnoreDialog(IWin32Window? owner, bool localExcludes)
        {
            bool Action()
            {
                using FormGitIgnore form = new(this, localExcludes);
                form.ShowDialog(owner);
                return true;
            }

            return DoActionOnRepo(owner, Action, changesRepo: false, postEvent: PostEditGitIgnore);
        }

        public bool StartAddToGitIgnoreDialog(IWin32Window? owner, bool localExclude, params string[] filePattern)
        {
            bool Action()
            {
                using FormAddToGitIgnore frm = new(this, localExclude, filePattern);
                frm.ShowDialog(owner);
                return true;
            }

            return DoActionOnRepo(owner, Action, changesRepo: false, postEvent: PostEditGitIgnore);
        }

        public bool StartSettingsDialog(IWin32Window? owner, SettingsPageReference? initialPage = null)
        {
            bool Action()
            {
                return FormSettings.ShowSettingsDialog(this, owner, initialPage)
                    is DialogResult.OK;
            }

            return DoActionOnRepo(owner, Action, requiresValidWorkingDir: false, postEvent: PostSettings);
        }

        public bool StartSettingsDialog(IGitPlugin gitPlugin)
        {
            // TODO: how to pass the main dialog as owner of the SettingsDialog (first parameter):
            return StartSettingsDialog(null, new SettingsPageReferenceByPlugin(gitPlugin));
        }

        public bool StartSettingsDialog(Type pageType)
        {
            return StartSettingsDialog(null, new SettingsPageReferenceByType(pageType));
        }

        /// <summary>
        /// Open the archive dialog.
        /// </summary>
        /// <param name="revision">Revision to create an archive from.</param>
        /// <param name="revision2">Revision for differential archive.</param>
        /// <param name="path">Files path for archive.</param>
        public bool StartArchiveDialog(IWin32Window? owner = null, GitRevision? revision = null, GitRevision? revision2 = null, string? path = null)
        {
            return DoActionOnRepo(owner, action: () =>
                {
                    using FormArchive form = new(this)
                    {
                        SelectedRevision = revision,
                    };
                    form.SetDiffSelectedRevision(revision2);
                    form.SetPathArgument(path);
                    form.ShowDialog(owner);

                    return true;
                }, changesRepo: false);
        }

        public bool StartMailMapDialog(IWin32Window? owner = null)
        {
            bool Action()
            {
                using FormMailMap form = new(this);
                form.ShowDialog(owner);
                return true;
            }

            return DoActionOnRepo(owner, Action, changesRepo: false);
        }

        public bool StartVerifyDatabaseDialog(IWin32Window? owner = null)
        {
            bool Action()
            {
                using FormVerify form = new(this);
                form.ShowDialog(owner);
                return true;
            }

            // TODO: move Notify to FormVerify and friends
            return DoActionOnRepo(owner, Action);
        }

        /// <inheritdoc/>
        public bool StartRemotesDialog(IWin32Window? owner, string? preselectRemote = null, string? preselectLocal = null)
        {
            bool Action()
            {
                using FormRemotes form = new(this)
                {
                    PreselectRemoteOnLoad = preselectRemote,
                    PreselectLocalOnLoad = preselectLocal
                };
                form.ShowDialog(owner);
                return true;
            }

            return DoActionOnRepo(owner, Action);
        }

        public bool StartRebase(IWin32Window? owner, string onto)
        {
            return StartRebaseDialog(owner, from: "", to: null, onto: onto,
                interactive: false, startRebaseImmediately: true);
        }

        public bool StartTheContinueRebaseDialog(IWin32Window? owner)
        {
            return StartRebaseDialog(owner, from: "", to: null, onto: null, interactive: false, startRebaseImmediately: false);
        }

        public bool StartInteractiveRebase(IWin32Window? owner, string onto)
        {
            return StartRebaseDialog(owner, from: "", to: null, onto: onto,
                interactive: true, startRebaseImmediately: true);
        }

        public bool StartRebaseDialogWithAdvOptions(IWin32Window? owner, string onto, string from = "")
        {
            return StartRebaseDialog(owner, from: from, to: null, onto, interactive: false, startRebaseImmediately: false);
        }

        public bool StartRebaseDialog(IWin32Window? owner, string? onto)
        {
            return StartRebaseDialog(owner, from: "", to: null, onto, interactive: false, startRebaseImmediately: false);
        }

        public bool StartRebaseDialog(IWin32Window? owner, string? from, string? to, string? onto, bool interactive = false, bool startRebaseImmediately = true)
        {
            bool Action()
            {
                using FormRebase form = new(this, from, to, onto, interactive, startRebaseImmediately);
                form.ShowDialog(owner);
                return true;
            }

            return DoActionOnRepo(owner, Action);
        }

        public bool StartRenameDialog(IWin32Window? owner, string branch)
        {
            bool Action()
            {
                using FormRenameBranch form = new(this, branch);
                return form.ShowDialog(owner) == DialogResult.OK;
            }

            return DoActionOnRepo(owner, Action);
        }

        public bool StartSubmodulesDialog(IWin32Window? owner)
        {
            bool Action()
            {
                using FormSubmodules form = new(this);
                form.ShowDialog(owner);
                return true;
            }

            return DoActionOnRepo(owner, Action);
        }

        public bool StartUpdateSubmodulesDialog(IWin32Window? owner, string submoduleLocalPath = "")
        {
            bool Action()
            {
                return FormProcess.ShowDialog(owner, this, arguments: Commands.SubmoduleUpdate(submoduleLocalPath), Module.WorkingDir, input: null, useDialogSettings: true);
            }

            return DoActionOnRepo(owner, Action, postEvent: PostUpdateSubmodules);
        }

        public bool StartUpdateSubmoduleDialog(IWin32Window? owner, string submoduleLocalPath, string submoduleParentPath)
        {
            bool Action()
            {
                // Execute the submodule update comment from the submodule's parent directory
                return FormProcess.ShowDialog(owner, this, arguments: Commands.SubmoduleUpdate(submoduleLocalPath), submoduleParentPath, null, true);
            }

            return DoActionOnRepo(owner, Action, postEvent: PostUpdateSubmodules);
        }

        public bool StartSyncSubmodulesDialog(IWin32Window? owner)
        {
            bool Action()
            {
                return FormProcess.ShowDialog(owner, this, arguments: Commands.SubmoduleSync(""), Module.WorkingDir, input: null, useDialogSettings: true);
            }

            return DoActionOnRepo(owner, Action);
        }

        public void UpdateSubmodules(IWin32Window? owner)
        {
            if (!Module.HasSubmodules())
            {
                return;
            }

            bool updateSubmodules = AppSettings.UpdateSubmodulesOnCheckout ?? (AppSettings.DontConfirmUpdateSubmodulesOnCheckout ?? MessageBoxes.ConfirmUpdateSubmodules(owner));

            if (updateSubmodules)
            {
                StartUpdateSubmodulesDialog(owner);
            }
        }

        public bool StartGeneralSettingsDialog(IWin32Window? owner)
        {
            return StartSettingsDialog(owner, CommandsDialogs.SettingsDialog.Pages.GeneralSettingsPage.GetPageReference());
        }

        public bool StartPluginSettingsDialog(IWin32Window? owner)
        {
            return StartSettingsDialog(owner, PluginsSettingsGroup.GetPageReference());
        }

        public bool StartRepoSettingsDialog(IWin32Window? owner)
        {
            return StartSettingsDialog(owner, CommandsDialogs.SettingsDialog.Pages.GitConfigSettingsPage.GetPageReference());
        }

        /// <summary>
        /// Open Browse - main GUI including dashboard.
        /// </summary>
        /// <param name="owner">current window owner.</param>
        /// <param name="args">The start up arguments.</param>
        public bool StartBrowseDialog(IWin32Window? owner, BrowseArguments? args = null)
        {
            FormBrowse form = new(this, args ?? new BrowseArguments());

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

        public void StartFileHistoryDialog(IWin32Window? owner, string fileName, GitRevision? revision = null, bool filterByRevision = false, bool showBlame = false)
        {
            string arguments = AppSettings.UseBrowseForFileHistory.Value ? $"browse {PathFilterArg}={fileName.Quote()} -commit={revision?.ObjectId}"
                : $"{(showBlame ? BlameHistoryCommand : FileHistoryCommand)} {fileName.Quote()} {revision?.ObjectId} {(filterByRevision ? FilterByRevisionArg : string.Empty)}";
            Launch(arguments, Module.WorkingDir);
        }

        public void OpenWithDifftool(IWin32Window? owner, IReadOnlyList<GitRevision?> revisions, string fileName, string? oldFileName, RevisionDiffKind diffKind, bool isTracked, string? customTool = null)
        {
            // Note: Order in revisions is that first clicked is last in array

            if (!RevisionDiffInfoProvider.TryGet(revisions, diffKind, out string firstRevision, out string secondRevision, out string error))
            {
                MessageBox.Show(owner, error, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                Module.OpenWithDifftool(fileName, oldFileName, firstRevision, secondRevision, isTracked: isTracked, customTool: customTool);
            }
        }

        public bool StartPushDialog(IWin32Window? owner, bool pushOnShow, bool forceWithLease, out bool pushCompleted)
        {
            bool pushed = false;

            bool Action()
            {
                using FormPush form = new(this);
                if (forceWithLease)
                {
                    form.CheckForceWithLease();
                }

                DialogResult dlgResult = pushOnShow
                    ? form.PushAndShowDialogWhenFailed(owner)
                    : form.ShowDialog(owner);

                if (dlgResult == DialogResult.OK)
                {
                    pushed = !form.ErrorOccurred;
                }

                return dlgResult == DialogResult.OK;
            }

            bool done = DoActionOnRepo(owner, Action);

            pushCompleted = pushed;

            return done;
        }

        public bool StartPushDialog(IWin32Window? owner, bool pushOnShow)
        {
            return StartPushDialog(owner, pushOnShow, forceWithLease: false, out _);
        }

        public bool StartApplyPatchDialog(IWin32Window? owner, string? patchFile = null)
        {
            return DoActionOnRepo(owner, action: () =>
                {
                    using FormApplyPatch form = new(this);
                    if (Directory.Exists(patchFile!))
                    {
                        form.SetPatchDir(patchFile!);
                    }
                    else
                    {
                        form.SetPatchFile(patchFile ?? "");
                    }

                    form.ShowDialog(owner);

                    return true;
                }, changesRepo: false);
        }

        public bool StartEditGitAttributesDialog(IWin32Window? owner = null)
        {
            bool Action()
            {
                using FormGitAttributes form = new(this);
                form.ShowDialog(owner);
                return true;
            }

            return DoActionOnRepo(owner, Action, changesRepo: false);
        }

        private bool InvokeEvent(IWin32Window? ownerForm, EventHandler<GitUIEventArgs>? gitUIEventHandler)
        {
            try
            {
                GitUIEventArgs e = new(ownerForm, this);
                gitUIEventHandler?.Invoke(this, e);
                return !e.Cancel;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return true;
        }

        private void InvokePostEvent(IWin32Window? ownerForm, bool actionDone, EventHandler<GitUIPostActionEventArgs>? gitUIEventHandler)
        {
            if (gitUIEventHandler is not null)
            {
                GitUIPostActionEventArgs e = new(ownerForm, this, actionDone);
                gitUIEventHandler(this, e);
            }
        }

        private void WrapRepoHostingCall(string name, IRepositoryHostPlugin gitHoster, Action<IRepositoryHostPlugin> call)
        {
            if (!gitHoster.ConfigurationOk)
            {
                GitUIEventArgs eventArgs = new(null, this);
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
                        "Error! :(", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void StartCloneForkFromHoster(IWin32Window? owner, IRepositoryHostPlugin gitHoster, EventHandler<GitModuleEventArgs>? gitModuleChanged)
        {
            WrapRepoHostingCall(TranslatedStrings.ForkCloneRepo, gitHoster, gh =>
            {
                using ForkAndCloneForm frm = new(this, gh, gitModuleChanged);
                frm.ShowDialog(owner);
            });
        }

        public void StartPullRequestsDialog(IWin32Window? owner, IRepositoryHostPlugin gitHoster)
        {
            WrapRepoHostingCall(TranslatedStrings.ViewPullRequest, gitHoster,
                                gh =>
                                {
                                    ViewPullRequestsForm frm = new(this, gh) { ShowInTaskbar = true };
                                    frm.Show(owner);
                                });
        }

        public void AddUpstreamRemote(IWin32Window? owner, IRepositoryHostPlugin gitHoster)
        {
            WrapRepoHostingCall(TranslatedStrings.AddUpstreamRemote, gitHoster,
                                gh =>
                                {
                                    ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                                    {
                                        string remoteName = await gh.AddUpstreamRemoteAsync();
                                        if (!string.IsNullOrEmpty(remoteName))
                                        {
                                            StartPullDialogAndPullImmediately(owner, remoteBranch: null, remoteName, GitPullAction.Fetch);
                                        }
                                    }).FileAndForget();
                                });
        }

        public void StartCreatePullRequest(IWin32Window? owner)
        {
            List<IRepositoryHostPlugin> relevantHosts =
                PluginRegistry.GitHosters.Where(gh => gh.GitModuleIsRelevantToMe()).ToList();

            if (relevantHosts.Count == 0)
            {
                MessageBox.Show(owner, "Could not find any repo hosts for current working directory", TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (relevantHosts.Count == 1)
            {
                StartCreatePullRequest(owner, relevantHosts[0]);
            }
            else
            {
                MessageBox.Show("StartCreatePullRequest:Selection not implemented!", TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void StartCreatePullRequest(IWin32Window? owner, IRepositoryHostPlugin gitHoster, string? chooseRemote = null, string? chooseBranch = null)
        {
            WrapRepoHostingCall(
                TranslatedStrings.CreatePullRequest,
                gitHoster,
                gh =>
                {
                    CreatePullRequestForm form = new(this, gh, chooseRemote, chooseBranch)
                    {
                        ShowInTaskbar = true
                    };

                    form.Show(owner);
                });
        }

        public bool RunCommand(IReadOnlyList<string> args)
        {
            IReadOnlyDictionary<string, string?> arguments = InitializeArguments(args);

            if (args.Count <= 1)
            {
                return false;
            }

            string command = args[1];

            if (command == "blame" && args.Count <= 2)
            {
                MessageBox.Show("Cannot open blame, there is no file selected.", "Blame", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (command == "difftool" && args.Count <= 2)
            {
                MessageBox.Show("Cannot open difftool, there is no file selected.", "Difftool", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (command is (BlameHistoryCommand or FileHistoryCommand) && args.Count <= 2)
            {
                MessageBox.Show("Cannot open blame / file history, there is no file selected.", "Blame / file history", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (command == "fileeditor" && args.Count <= 2)
            {
                MessageBox.Show("Cannot open file editor, there is no file selected.", "File editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (command == "revert" && args.Count <= 2)
            {
                MessageBox.Show("Cannot open revert, there is no file selected.", "Revert", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return RunCommandBasedOnArgument(args, arguments);
        }

        // Please update FormCommandlineHelp if you add or change commands
        private bool RunCommandBasedOnArgument(IReadOnlyList<string> args, IReadOnlyDictionary<string, string?> arguments)
        {
#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
            string command = args[1];
            switch (command)
            {
                case "about":
                    Application.Run(new FormAbout
                    {
                        StartPosition = FormStartPosition.CenterScreen
                    });
                    return true;
                case "add":
                case "addfiles":
                    // If filenames have been specified, quote them and pass them to the dialog, else pass '.' for current dir.
                    return StartAddFilesDialog(owner: null, addFiles: args.Count < 3 ? "." : string.Join(' ', args.Skip(2).Select(file => file.Quote())));
                case "apply":       // [filename]
                case "applypatch":
                    return StartApplyPatchDialog(null, args.Count == 3 ? args[2] : "");
                case "blame":       // filename
                    return RunBlameCommand(args);
                case "branch":
                    return StartCreateBranchDialog();
                case "browse":      // [path] [--pathFilter=filname] [-filter] [-commit=selected[,first]]
                    return RunBrowseCommand(args);
                case "checkout":
                case "checkoutbranch":
                    return StartCheckoutBranch(null);
                case "checkoutrevision":
                    return StartCheckoutRevisionDialog(null);
                case "cherry":
                    return StartCherryPickDialog();
                case "cleanup":
                    return StartCleanupRepositoryDialog();
                case "clone":       // [path]
                    return RunCloneCommand(args);
                case "commit":      // [--quiet]
                    return Commit(arguments);
                case "difftool":    // filename
                    try
                    {
                        Module.OpenWithDifftool(args[2]);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }

                case BlameHistoryCommand:
                case FileHistoryCommand:
                    // filename [revision [--filter-by-revision]]
                    if (Module.WorkingDir.TrimEnd('\\') == Path.GetFullPath(args[2]) && Module.SuperprojectModule is not null)
                    {
                        Module = Module.SuperprojectModule;
                    }

                    return RunFileHistoryCommand(args, showBlame: command == BlameHistoryCommand);
                case "fileeditor":  // filename
                    return StartFileEditorDialog(args[2]);
                case "formatpatch":
                    return StartFormatPatchDialog();
                case "gitignore":
                    return StartEditGitIgnoreDialog(null, false);
                case "init":        // [path]
                    return RunInitCommand(args);
                case "merge":       // [--branch name]
                    return RunMergeCommand(arguments);
                case "mergeconflicts":
                case "mergetool":   // [--quiet]
                    return RunMergeToolOrConflictCommand(arguments);
                case "openrepo":    // [path]
                    return RunOpenRepoCommand(args);
                case "pull":        // [--rebase] [--merge] [--fetch] [--quiet] [--remotebranch name]
                    return Pull(arguments);
                case "push":        // [--quiet]
                    return Push(arguments);
                case "rebase":      // [--branch name]
                    return RunRebaseCommand(arguments);
                case "remotes":
                    return StartRemotesDialog(owner: null);
                case "revert":
                case "reset":
                    return StartResetChangesDialog(args.Count == 3 ? args[2] : "");
                case "searchfile":
                    return RunSearchFileCommand();
                case "settings":
                    return StartSettingsDialog(owner: null);
                case "stash":
                    return StartStashDialog();
                case "synchronize": // [--rebase] [--merge] [--fetch] [--quiet]
                    return RunSynchronizeCommand(arguments);
                case "tag":
                    return StartCreateTagDialog();
                case "viewdiff":
                    return StartCompareRevisionsDialog();
                case "viewpatch":   // [filename]
                    return StartViewPatchDialog(args.Count == 3 ? args[2] : "");
                case "uninstall":
                    return UninstallEditor();
                default:
                    if (args[1].StartsWith("git://") || args[1].StartsWith("http://") || args[1].StartsWith("https://"))
                    {
                        return StartCloneDialog(null, args[1], true);
                    }

                    if (args[1].StartsWith("github-windows://openRepo/"))
                    {
                        return StartCloneDialog(null, args[1].Replace("github-windows://openRepo/", ""), true);
                    }

                    if (args[1].StartsWith("github-mac://openRepo/"))
                    {
                        return StartCloneDialog(null, args[1].Replace("github-mac://openRepo/", ""), true);
                    }

                    // User supplied a path. Open the repository if its a valid path
                    string dir = !string.IsNullOrWhiteSpace(command) && File.Exists(command) ? Path.GetDirectoryName(command) : command;
                    if (args.Count == 2 && Directory.Exists(dir))
                    {
                        LaunchBrowse(dir);
                        return true;
                    }

                    break;
            }
#pragma warning restore SA1025 // Code should not contain multiple whitespace in a row

            Application.Run(new FormCommandlineHelp { StartPosition = FormStartPosition.CenterScreen });
            return true;
        }

        private static bool UninstallEditor()
        {
            ConfigFileSettings configFileGlobalSettings = ConfigFileSettings.CreateGlobal(false);

            string coreEditor = configFileGlobalSettings.GetValue("core.editor");
            string path = AppSettings.GetInstallDir().ToPosixPath();
            if (path is not null && coreEditor.ToLowerInvariant().Contains(path.ToLowerInvariant()))
            {
                configFileGlobalSettings.SetValue("core.editor", "");
            }

            configFileGlobalSettings.Save();

            return true;
        }

        private bool RunMergeCommand(IReadOnlyDictionary<string, string?> arguments)
        {
            string? branch = null;
            if (arguments.ContainsKey("branch"))
            {
                branch = arguments["branch"];
            }

            return StartMergeBranchDialog(null, branch);
        }

        private bool RunSearchFileCommand()
        {
            SearchWindow<string> searchWindow = new(FindFileMatches);
            Application.Run(searchWindow);
            if (searchWindow.SelectedItem is not null)
            {
                // We need to return the file that has been found, the visual studio plugin uses the return value
                // to open the selected file.
                Console.WriteLine(Path.Combine(Module.WorkingDir, searchWindow.SelectedItem));
                return true;
            }

            return false;
        }

        private bool RunBrowseCommand(IReadOnlyList<string> args)
        {
            string arg = GetParameterOrEmptyStringAsDefault(args, "-commit");
            if (arg == "")
            {
                return StartBrowseDialog(owner: null,
                    new BrowseArguments
                    {
                        RevFilter = GetParameterOrEmptyStringAsDefault(args, "-filter"),
                        PathFilter = GetParameterOrEmptyStringAsDefault(args, PathFilterArg),
                        IsFileHistoryMode = args.Any(arg => arg.StartsWith(PathFilterArg))
                    });
            }

            if (TryGetObjectIds(arg, Module, out ObjectId? selectedId, out ObjectId? firstId))
            {
                return StartBrowseDialog(owner: null,
                    new BrowseArguments
                    {
                        RevFilter = GetParameterOrEmptyStringAsDefault(args, "-filter"),
                        PathFilter = GetParameterOrEmptyStringAsDefault(args, PathFilterArg),
                        SelectedId = selectedId,
                        FirstId = firstId,
                        IsFileHistoryMode = args.Any(arg => arg.StartsWith(PathFilterArg))
                    });
            }

            Console.Error.WriteLine($"No commit found matching: {arg}");
            return false;

            static bool TryGetObjectIds(string arg, IGitModule module, out ObjectId? selectedId, out ObjectId? firstId)
            {
                selectedId = null;
                firstId = null;
                foreach (string part in arg.LazySplit(','))
                {
                    if (!module.TryResolvePartialCommitId(part, out ObjectId? objectId))
                    {
                        return false;
                    }

                    if (selectedId is null)
                    {
                        selectedId = objectId;
                    }
                    else if (firstId is null)
                    {
                        firstId = objectId;

                        // just ignore further commits
                        break;
                    }
                }

                return true;
            }
        }

        private static string GetParameterOrEmptyStringAsDefault(IReadOnlyList<string> args, string paramName)
        {
            string withEquals = paramName + "=";

            for (int i = 2; i < args.Count; i++)
            {
                string arg = args[i];
                if (arg.StartsWith(withEquals))
                {
                    return arg.Replace(withEquals, "");
                }
            }

            return "";
        }

        private bool RunOpenRepoCommand(IReadOnlyList<string> args)
        {
            IGitUICommands c = this;
            if (args.Count > 2)
            {
                if (File.Exists(args[2]))
                {
                    string? path = File.ReadAllText(args[2]).Trim().LazySplit('\n').FirstOrDefault();
                    if (Directory.Exists(path))
                    {
                        c = WithWorkingDirectory(path);
                    }
                }
            }

            return c.StartBrowseDialog(owner: null,
                new BrowseArguments
                {
                    RevFilter = GetParameterOrEmptyStringAsDefault(args, "-filter"),
                    PathFilter = GetParameterOrEmptyStringAsDefault(args, PathFilterArg)
                });
        }

        private bool RunSynchronizeCommand(IReadOnlyDictionary<string, string?> arguments)
        {
            bool successful = true;
            successful = Commit(arguments) && successful;
            successful = Pull(arguments) && successful;
            successful = Push(arguments) && successful;
            return successful;
        }

        private bool RunRebaseCommand(IReadOnlyDictionary<string, string?> arguments)
        {
            string? branch = null;
            if (arguments.ContainsKey("branch"))
            {
                branch = arguments["branch"];
            }

            return StartRebaseDialog(owner: null, onto: branch);
        }

        public bool StartFileEditorDialog(string? filename, bool showWarning = false, int? lineNumber = null)
        {
            using FormEditor formEditor = new(this, filename, showWarning, lineNumber: lineNumber);
            return formEditor.ShowDialog() != DialogResult.Cancel;
        }

        /// <summary>
        /// Remove working directory from filename and convert to POSIX path.
        /// This is to prevent filenames that are too long while there is room left when the workingdir was not in the path.
        /// </summary>
        private string NormalizeFileName(string fileName)
        {
            fileName = fileName.ToPosixPath();
            return string.IsNullOrEmpty(Module.WorkingDir) ? fileName : fileName.Replace(Module.WorkingDir.ToPosixPath(), "");
        }

        /// <returns>false on error.</returns>
        private bool RunFileHistoryCommand(IReadOnlyList<string> args, bool showBlame)
        {
            string fileHistoryFileName = args[2];
            if (new FormFileHistoryController().TryGetExactPath(_fullPathResolver.Resolve(fileHistoryFileName), out string exactFileName))
            {
                fileHistoryFileName = NormalizeFileName(exactFileName);
            }

            if (string.IsNullOrWhiteSpace(fileHistoryFileName))
            {
                return false;
            }

            GitRevision? revision = null;
            if (args.Count > 3)
            {
                if (!ObjectId.TryParse(args[3], out ObjectId? objectId))
                {
                    return false;
                }

                revision = new GitRevision(objectId);
            }

            bool filterByRevision = false;
            if (args.Count > 4)
            {
                if (args[4] != FilterByRevisionArg)
                {
                    return false;
                }

                filterByRevision = true;
            }

            // Similar to StartFileHistoryDialog()
            if (AppSettings.UseBrowseForFileHistory.Value)
            {
                // NOTE: fileHistoryFileName doesn't need to be quoted, as it the filter will get quoted
                // when the filter gets set.

                ShowModelessForm(owner: null, requiresValidWorkingDir: true, preEvent: null, postEvent: null,
                                 () => new FormBrowse(commands: this, new BrowseArguments
                                 {
                                     RevFilter = filterByRevision ? revision?.ObjectId.ToString() : null,
                                     PathFilter = fileHistoryFileName,
                                     SelectedId = revision?.ObjectId,
                                     IsFileHistoryMode = true
                                 }));
            }
            else
            {
                // NOTE: fileHistoryFileName must be quoted.

                ShowModelessForm(owner: null, requiresValidWorkingDir: true, preEvent: null, postEvent: null,
                                 () => new FormFileHistory(this, fileHistoryFileName.QuoteNE(), revision, filterByRevision, showBlame));
            }

            return true;
        }

        private bool RunCloneCommand(IReadOnlyList<string> args)
            => StartCloneDialog(null, args.Count > 2 ? args[2] : null);

        private bool RunInitCommand(IReadOnlyList<string> args)
            => StartInitializeDialog(null, args.Count > 2 ? args[2] : null);

        /// <returns>false on error.</returns>
        private bool RunBlameCommand(IReadOnlyList<string> args)
        {
            string blameFileName = NormalizeFileName(args[2]);

            int? initialLine = null;
            if (args.Count > 3)
            {
                if (int.TryParse(args[3], out int temp))
                {
                    initialLine = temp;
                }
            }

            return DoActionOnRepo(owner: null, action: () =>
            {
                using FormBlame frm = new(this, blameFileName, null, initialLine);
                frm.ShowDialog(null);
                return true;
            }, changesRepo: false);
        }

        private bool RunMergeToolOrConflictCommand(IReadOnlyDictionary<string, string?> arguments)
        {
            if (!arguments.ContainsKey("quiet") || Module.InTheMiddleOfConflictedMerge())
            {
                return StartResolveConflictsDialog();
            }

            return true;
        }

        private static IReadOnlyDictionary<string, string?> InitializeArguments(IReadOnlyList<string> args)
        {
            Dictionary<string, string?> arguments = [];

            for (int i = 2; i < args.Count; i++)
            {
                if (args[i].StartsWith("--") && i + 1 < args.Count && !args[i + 1].StartsWith("--"))
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
            IReadOnlyList<string> candidates = Module.GetFullTree("HEAD");

            Func<string?, bool> predicate = _findFilePredicateProvider.Get(name, Module.WorkingDir);

            return candidates.Where(predicate);
        }

        private bool Commit(IReadOnlyDictionary<string, string?> arguments)
        {
            arguments.TryGetValue("message", out string? overridingMessage);
            bool showOnlyWhenChanges = arguments.ContainsKey("quiet");
            return StartCommitDialog(null, overridingMessage, showOnlyWhenChanges);
        }

        private bool Push(IReadOnlyDictionary<string, string?> arguments)
            => StartPushDialog(null, arguments.ContainsKey("quiet"));

        private bool Pull(IReadOnlyDictionary<string, string?> arguments)
        {
            UpdateSettingsBasedOnArguments(arguments);

            string? remoteBranch = null;
            if (arguments.ContainsKey("remotebranch"))
            {
                remoteBranch = arguments["remotebranch"];
            }

            bool isQuiet = arguments.ContainsKey("quiet");

            if (isQuiet)
            {
                return StartPullDialogAndPullImmediately(remoteBranch: remoteBranch);
            }

            return StartPullDialog(remoteBranch: remoteBranch);
        }

        private static void UpdateSettingsBasedOnArguments(IReadOnlyDictionary<string, string?> arguments)
        {
            if (arguments.ContainsKey("merge"))
            {
                AppSettings.DefaultPullAction = GitPullAction.Merge;
            }

            if (arguments.ContainsKey("rebase"))
            {
                AppSettings.DefaultPullAction = GitPullAction.Rebase;
            }

            if (arguments.ContainsKey("fetch"))
            {
                AppSettings.DefaultPullAction = GitPullAction.Fetch;
            }

            if (arguments.ContainsKey("autostash"))
            {
                AppSettings.AutoStash = true;
            }
        }

        public void RaisePostBrowseInitialize(IWin32Window? owner)
        {
            InvokeEvent(owner, PostBrowseInitialize);
        }

        public void RaisePostRegisterPlugin(IWin32Window? owner)
        {
            InvokeEvent(owner, PostRegisterPlugin);
        }

        public IGitRemoteCommand CreateRemoteCommand()
        {
            return new GitRemoteCommand(this);
        }

        /// <summary>
        ///  Creates a new instance of <see cref="IGitUICommands"/> for a git repository specified by <paramref name="module"/>.
        /// </summary>
        /// <param name="module">The git repository.</param>
        /// <returns>A new instance of <see cref="IGitUICommands"/>.</returns>
        public IGitUICommands WithGitModule(IGitModule module) => new GitUICommands(_serviceProvider, module);

        /// <summary>
        ///  Creates a new instance of <see cref="IGitUICommands"/> for a git repository specified by <paramref name="workingDirectory"/>.
        /// </summary>
        /// <param name="workingDirectory">The git repository working directory.</param>
        /// <returns>A new instance of <see cref="IGitUICommands"/>.</returns>
        public IGitUICommands WithWorkingDirectory(string? workingDirectory) => new GitUICommands(_serviceProvider, new GitModule(workingDirectory));

        #region Nested class: GitRemoteCommand

        private sealed class GitRemoteCommand : IGitRemoteCommand
        {
            public object? OwnerForm { get; set; }
            public string? Remote { get; set; }
            public string? Title { get; set; }
            public string? CommandText { get; set; }
            public bool ErrorOccurred { get; private set; }
            public string? CommandOutput { get; private set; }

            private readonly IGitUICommands _commands;

            public event EventHandler<GitRemoteCommandCompletedEventArgs>? Completed;

            internal GitRemoteCommand(IGitUICommands commands)
            {
                _commands = commands;
            }

            public void Execute()
            {
                if (CommandText is null)
                {
                    throw new InvalidOperationException("CommandText is required");
                }

                using FormRemoteProcess form = new(_commands, CommandText);
                if (Title is not null)
                {
                    form.Text = Title;
                }

                if (Remote is not null)
                {
                    form.Remote = Remote;
                }

                form.HandleOnExitCallback = HandleOnExit;

                form.ShowDialog(OwnerForm as IWin32Window);

                ErrorOccurred = form.ErrorOccurred();
                CommandOutput = form.GetOutputString();
            }

            private bool HandleOnExit(ref bool isError, FormProcess form)
            {
                CommandOutput = form.GetOutputString();

                GitRemoteCommandCompletedEventArgs e = new(this, isError, false);

                Completed?.Invoke(form, e);

                isError = e.IsError;

                return e.Handled;
            }
        }

        #endregion

        internal TestAccessor GetTestAccessor() => new(this);

        internal struct TestAccessor
        {
            private readonly GitUICommands _commands;

            internal TestAccessor(GitUICommands commands)
            {
                _commands = commands;
            }

            internal string NormalizeFileName(string fileName) => _commands.NormalizeFileName(fileName);

            internal bool RunCommandBasedOnArgument(string[] args) => _commands.RunCommandBasedOnArgument(args, InitializeArguments(args));

            internal void ShowFileHistoryDialog(string fileName)
                => _commands.RunFileHistoryCommand(args: new string[] { "", "", fileName }, showBlame: false);
        }
    }
}
