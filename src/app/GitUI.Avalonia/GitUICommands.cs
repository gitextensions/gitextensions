using System.Text;
using Avalonia.Controls.ApplicationLifetimes;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Shims.WinForms;
using GitExtUtils;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.WorktreeDialog;
using GitUI.Compat;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using AvaloniaApplication = Avalonia.Application;
using ShutdownMode = Avalonia.Controls.ShutdownMode;
using Window = Avalonia.Controls.Window;

namespace GitUI;

// Twin of GitUI/GitUICommands.cs. Members are implemented as their dialogs get ported;
// everything else throws NotImplementedException naming the member, so a missing port
// surfaces clearly instead of failing silently.

/// <summary>Contains methods to invoke Git Extensions forms, dialogs, etc.</summary>
public sealed class GitUICommands : IGitUICommands
{
    private const string BlameHistoryCommand = "blamehistory";
    private const string FileHistoryCommand = "filehistory";

    private const string FilterByRevisionArg = "--filter-by-revision";
    private const string PathFilterArg = "--pathFilter";

    private readonly IServiceProvider _serviceProvider;

    public IGitModule Module { get; private set; }
    public ILockableNotifier RepoChangedNotifier { get; }
    public IBrowseRepo? BrowseRepo { get; set; }

    public GitUICommands(IServiceProvider serviceProvider, IGitModule module)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(module);

        _serviceProvider = serviceProvider;
        Module = module;

        RepoChangedNotifier = new ActionNotifier(
            () => InvokeEvent(null, PostRepositoryChanged));
    }

    #region Events

    // Raised like WinForms as the corresponding actions get ported.
#pragma warning disable CS0067 // The event is never used
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
#pragma warning restore CS0067

    #endregion

    public object? GetService(Type serviceType) => _serviceProvider.GetService(serviceType);

    public IGitUICommands WithGitModule(IGitModule module) => new GitUICommands(_serviceProvider, module);

    public IGitUICommands WithWorkingDirectory(string? workingDirectory)
        => new GitUICommands(_serviceProvider, new GitModule(this.GetRequiredService<IGitExecutorProvider>(), workingDirectory));

    /// <summary>Launches a new Git Extensions Avalonia process.</summary>
    public static IProcess Launch(string arguments, string workingDir = "")
        => new Executable(Application.ExecutablePath, workingDir).Start(arguments);

    /// <summary>Launches the repository browser in a new process.</summary>
    internal static void LaunchBrowse(string workingDir = "", ObjectId selectedId = default, ObjectId firstId = default)
    {
        if (!Directory.Exists(workingDir))
        {
            MessageBoxes.GitExtensionsDirectoryDoesNotExist(owner: null, workingDir);
            return;
        }

        StringBuilder arguments = new("browse");
        if (selectedId.IsZero)
        {
            selectedId = firstId;
            firstId = default;
        }

        if (!selectedId.IsZero)
        {
            arguments.Append(" -commit=").Append(selectedId);
            if (!firstId.IsZero)
            {
                arguments.Append(',').Append(firstId);
            }
        }

        Launch(arguments.ToString(), workingDir);
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

    private bool InvokeEvent(IWin32Window? ownerForm, EventHandler<GitUIEventArgs>? gitUIEventHandler)
    {
        try
        {
            GitUIEventArgs eventArgs = new(ownerForm, this);
            gitUIEventHandler?.Invoke(this, eventArgs);
            return !eventArgs.Cancel;
        }
        catch (Exception ex)
        {
            MessageBoxes.ShowError(ownerForm, $"{ex.Message}{Environment.NewLine}{ex.StackTrace}", "Error");
            return false;
        }
    }

    private void InvokePostEvent(IWin32Window? ownerForm, bool actionDone, EventHandler<GitUIPostActionEventArgs>? gitUIEventHandler)
    {
        gitUIEventHandler?.Invoke(this, new GitUIPostActionEventArgs(ownerForm, this, actionDone));
    }

    private bool DoActionOnRepo(
        IWin32Window? owner,
        Func<bool> action,
        bool requiresValidWorkingDir = true,
        bool changesRepo = true,
        EventHandler<GitUIEventArgs>? preEvent = null,
        EventHandler<GitUIPostActionEventArgs>? postEvent = null)
    {
        bool actionDone = false;
        RepoChangedNotifier.Lock();
        try
        {
            if (requiresValidWorkingDir && !Module.IsValidGitWorkingDir())
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
            bool requestNotify = actionDone && changesRepo && Module.IsValidGitWorkingDir();
            RepoChangedNotifier.UnLock(requestNotify);
        }

        return actionDone;
    }

    private static NotImplementedException NotPorted(string member)
        => new($"{member} is not ported to the Avalonia UI yet.");

    public void OpenWithDifftool(IWin32Window? owner, IReadOnlyList<GitRevision?> revisions, string fileName, string? oldFileName, RevisionDiffKind diffKind, bool isTracked, string? customTool = null)
    {
        // Note: Order in revisions is that first clicked is last in array.
        if (!RevisionDiffInfoProvider.TryGet(revisions, diffKind, out string? firstRevision, out string? secondRevision, out string? error))
        {
            MessageBoxes.Show(owner, error, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else
        {
            Module.OpenWithDifftool(fileName, oldFileName, firstRevision, secondRevision, isTracked: isTracked, customTool: customTool);
        }
    }

    #region Not ported yet

    public void AddCommitTemplate(string key, Func<string> addingText, Image? icon, bool isRegex = false) => throw NotPorted(nameof(AddCommitTemplate));
    public void AddUpstreamRemote(IWin32Window? owner, IRepositoryHostPlugin gitHoster) => throw NotPorted(nameof(AddUpstreamRemote));
    public IGitRemoteCommand CreateRemoteCommand() => throw NotPorted(nameof(CreateRemoteCommand));
    public bool DoActionOnRepo(Func<bool> action)
        => DoActionOnRepo(owner: null, action, requiresValidWorkingDir: false);
    public void RaisePostBrowseInitialize(IWin32Window? owner) => InvokeEvent(owner, PostBrowseInitialize);
    public void RaisePostRegisterPlugin(IWin32Window? owner) => InvokeEvent(owner, PostRegisterPlugin);
    public void RemoveCommitTemplate(string key) => throw NotPorted(nameof(RemoveCommitTemplate));
    public bool RunCommand(IReadOnlyList<string> args)
    {
        IReadOnlyDictionary<string, string?> arguments = InitializeArguments(args);

        if (args.Count <= 1)
        {
            return false;
        }

        string command = args[1];
        if (command == "difftool" && args.Count <= 2)
        {
            MessageBoxes.Show("Cannot open difftool, there is no file selected.", "Difftool", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (command is BlameHistoryCommand or FileHistoryCommand && args.Count <= 2)
        {
            MessageBoxes.Show("Cannot open blame / file history, there is no file selected.", "Blame / file history", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (command == "fileeditor" && args.Count <= 2)
        {
            MessageBoxes.Show("Cannot open file editor, there is no file selected.", "File editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        return RunCommandBasedOnArgument(args, arguments);
    }

    // Please update FormCommandlineHelp if you add or change commands.
    private bool RunCommandBasedOnArgument(IReadOnlyList<string> args, IReadOnlyDictionary<string, string?> arguments)
    {
        string command = args[1];
        switch (command)
        {
            case "add":
            case "addfiles":
                return StartAddFilesDialog(owner: null, addFiles: args.Count < 3 ? "." : string.Join(' ', args.Skip(2).Select(file => file.Quote())));
            case "apply":
            case "applypatch":
                return StartApplyPatchDialog(null, args.Count == 3 ? args[2] : string.Empty);
            case "branch":
                return StartCreateBranchDialog();
            case "browse":
                return RunBrowseCommand(args);
            case "checkout":
            case "checkoutbranch":
                return StartCheckoutBranch(null);
            case "cherry":
                return StartCherryPickDialog();
            case "clone":
                return StartCloneDialog(null, args.Count > 2 ? args[2] : null);
            case "commit":
                return Commit(arguments);
            case "difftool" when args.Count > 2:
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
                return RunFileHistoryCommand(args, showBlame: command == BlameHistoryCommand);
            case "fileeditor":
                return StartFileEditorDialog(args[2]);
            case "init":
                return StartInitializeDialog(null, args.Count > 2 ? args[2] : null);
            case "merge":
                arguments.TryGetValue("branch", out string? branch);
                return StartMergeBranchDialog(null, branch);
            case "mergeconflicts":
            case "mergetool":
                return RunMergeToolOrConflictCommand(arguments);
            case "openrepo":
                return RunOpenRepoCommand(args);
            case "pull":
                return Pull(arguments);
            case "push":
                return Push(arguments);
            case "rebase":
                arguments.TryGetValue("branch", out string? onto);
                return StartRebaseDialog(owner: null, onto);
            case "remotes":
                return StartRemotesDialog(owner: null);
            case "settings":
                return StartSettingsDialog(owner: null);
            case "stash":
                return StartStashDialog();
            case "synchronize":
                return Commit(arguments) & Pull(arguments) & Push(arguments);
            case "tag":
                return StartCreateTagDialog();
            case "viewpatch":
                return StartViewPatchDialog(args.Count == 3 ? args[2] : string.Empty);
            default:
                if (command.StartsWith("git://") || command.StartsWith("http://") || command.StartsWith("https://"))
                {
                    return StartCloneDialog(null, command, openedFromProtocolHandler: true);
                }

                const string GitHubWindowsPrefix = "github-windows://openRepo/";
                const string GitHubMacPrefix = "github-mac://openRepo/";
                if (command.StartsWith(GitHubWindowsPrefix))
                {
                    return StartCloneDialog(null, command.Replace(GitHubWindowsPrefix, string.Empty), openedFromProtocolHandler: true);
                }

                if (command.StartsWith(GitHubMacPrefix))
                {
                    return StartCloneDialog(null, command.Replace(GitHubMacPrefix, string.Empty), openedFromProtocolHandler: true);
                }

                string? dir = !string.IsNullOrWhiteSpace(command) && File.Exists(command) ? Path.GetDirectoryName(command) : command;
                if (args.Count == 2 && Directory.Exists(dir))
                {
                    return WithWorkingDirectory(dir).StartBrowseDialog(owner: null);
                }

                string message = $"The command \"{command}\" is not available in the Avalonia port yet.";
                Console.Error.WriteLine(message);
                MessageBoxes.ShowError(owner: null, message, "Unsupported command");
                return false;
        }
    }

    private bool RunBrowseCommand(IReadOnlyList<string> args)
    {
        string commit = GetParameterOrEmptyStringAsDefault(args, "-commit");
        BrowseArguments browseArguments = new()
        {
            RevFilter = GetParameterOrEmptyStringAsDefault(args, "-filter"),
            PathFilter = GetParameterOrEmptyStringAsDefault(args, PathFilterArg),
            IsFileHistoryMode = args.Any(arg => arg.StartsWith(PathFilterArg))
        };

        if (commit == string.Empty)
        {
            return StartBrowseDialog(owner: null, browseArguments);
        }

        if (!TryGetObjectIds(commit, Module, out ObjectId selectedId, out ObjectId firstId))
        {
            Console.Error.WriteLine($"No commit found matching: {commit}");
            return false;
        }

        return StartBrowseDialog(owner: null, browseArguments with
        {
            SelectedId = selectedId,
            FirstId = firstId
        });

        static bool TryGetObjectIds(string value, IGitModule module, out ObjectId selectedId, out ObjectId firstId)
        {
            selectedId = default;
            firstId = default;
            foreach (string part in value.LazySplit(','))
            {
                if (!module.TryResolvePartialCommitId(part, out ObjectId objectId))
                {
                    return false;
                }

                if (selectedId.IsZero)
                {
                    selectedId = objectId;
                }
                else if (firstId.IsZero)
                {
                    firstId = objectId;
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
                return arg.Replace(withEquals, string.Empty);
            }
        }

        return string.Empty;
    }

    private bool RunOpenRepoCommand(IReadOnlyList<string> args)
    {
        IGitUICommands commands = this;
        if (args.Count > 2 && File.Exists(args[2]))
        {
            string? path = File.ReadAllText(args[2]).Trim().LazySplit('\n').FirstOrDefault();
            if (Directory.Exists(path))
            {
                commands = WithWorkingDirectory(path);
            }
        }

        return commands.StartBrowseDialog(owner: null, new BrowseArguments
        {
            RevFilter = GetParameterOrEmptyStringAsDefault(args, "-filter"),
            PathFilter = GetParameterOrEmptyStringAsDefault(args, PathFilterArg)
        });
    }

    private bool RunFileHistoryCommand(IReadOnlyList<string> args, bool showBlame)
    {
        string fileName = NormalizeFileName(args[2]);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        GitRevision? revision = null;
        if (args.Count > 3)
        {
            if (!ObjectId.TryParse(args[3], out ObjectId objectId))
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

        if (AppSettings.UseBrowseForFileHistory.Value)
        {
            return StartBrowseDialog(owner: null, new BrowseArguments
            {
                RevFilter = filterByRevision ? revision?.ObjectId.ToString() : null,
                PathFilter = fileName,
                SelectedId = revision?.ObjectId ?? default,
                IsFileHistoryMode = true
            });
        }

        return DoActionOnRepo(owner: null, action: () =>
        {
            FormFileHistory form = new(this, fileName.QuoteNE(), revision, filterByRevision, showBlame);
            ShowModelessWindow(form, owner: null);
            return true;
        }, changesRepo: false);
    }

    private string NormalizeFileName(string fileName)
    {
        fileName = fileName.ToPosixPath();
        return string.IsNullOrEmpty(Module.WorkingDir) ? fileName : fileName.Replace(Module.WorkingDir.ToPosixPath(), string.Empty);
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

    private bool Commit(IReadOnlyDictionary<string, string?> arguments)
    {
        arguments.TryGetValue("message", out string? overridingMessage);
        return StartCommitDialog(null, overridingMessage, arguments.ContainsKey("quiet"));
    }

    private bool Push(IReadOnlyDictionary<string, string?> arguments)
        => StartPushDialog(null, arguments.ContainsKey("quiet"));

    private bool Pull(IReadOnlyDictionary<string, string?> arguments)
    {
        UpdateSettingsBasedOnArguments(arguments);
        arguments.TryGetValue("remotebranch", out string? remoteBranch);
        return arguments.ContainsKey("quiet")
            ? StartPullDialogAndPullImmediately(remoteBranch: remoteBranch)
            : StartPullDialog(remoteBranch: remoteBranch);
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

    public void ShowModelessForm(IWin32Window? owner, bool requiresValidWorkingDir, EventHandler<GitUIEventArgs>? preEvent, EventHandler<GitUIPostActionEventArgs>? postEvent, Func<Form> provideForm) => throw NotPorted(nameof(ShowModelessForm));
    public bool StartAddFilesDialog(IWin32Window? owner, string? addFiles = null)
    {
        bool Action()
        {
            using FormAddFiles form = new(this, addFiles);
            form.ShowDialog(owner);
            return true;
        }

        return DoActionOnRepo(owner, Action);
    }

    public bool StartAddToGitIgnoreDialog(IWin32Window? owner, bool localExclude, params string[] filePattern) => throw NotPorted(nameof(StartAddToGitIgnoreDialog));
    public bool StartAmendCommitDialog(IWin32Window? owner, GitRevision revision)
    {
        bool Action()
        {
            using CommandsDialogs.FormCommit form = new(this, CommandsDialogs.CommitKind.Amend, revision);
            form.ShowDialog(owner);
            return true;
        }

        return DoActionOnRepo(owner, Action);
    }

    public bool StartApplyPatchDialog(IWin32Window? owner, string? patchFile = null)
    {
        return DoActionOnRepo(owner, action: () =>
        {
            using CommandsDialogs.FormApplyPatch form = new(this);
            if (Directory.Exists(patchFile!))
            {
                form.SetPatchDir(patchFile!);
            }
            else
            {
                form.SetPatchFile(patchFile ?? string.Empty);
            }

            form.ShowDialog(owner);
            return true;
        }, changesRepo: false);
    }

    public bool StartArchiveDialog(IWin32Window? owner = null, GitRevision? revision = null, GitRevision? revision2 = null, string? path = null)
    {
        return DoActionOnRepo(owner, action: () =>
        {
            using CommandsDialogs.FormArchive form = new(this)
            {
                SelectedRevision = revision,
            };
            form.SetDiffSelectedRevision(revision2);
            form.SetPathArgument(path);
            form.ShowDialog(owner);
            return true;
        }, changesRepo: false);
    }

    public void StartBatchFileProcessDialog(string batchFile) => throw NotPorted(nameof(StartBatchFileProcessDialog));
    public bool StartBrowseDialog(IWin32Window? owner, BrowseArguments? args = null)
    {
        FormBrowse form = new(this, args ?? new BrowseArguments());
        ShowModelessWindow(form, owner);
        return true;
    }

    public bool StartCheckoutBranch(IWin32Window? owner, IReadOnlyList<ObjectId>? containObjectIds)
        => StartCheckoutBranch(owner, string.Empty, remote: false, containObjectIds);

    public bool StartCheckoutBranch(IWin32Window? owner, string branch = "", bool remote = false, IReadOnlyList<ObjectId>? containObjectIds = null)
    {
        return DoActionOnRepo(owner, action: () =>
        {
            using CommandsDialogs.FormCheckoutBranch form = new(this, branch, remote, containObjectIds);
            return form.DoDefaultActionOrShow(owner) == DialogResult.OK;
        }, preEvent: PreCheckoutBranch, postEvent: PostCheckoutBranch);
    }

    public bool StartCheckoutRemoteBranch(IWin32Window? owner, string branch)
    {
        return StartCheckoutBranch(owner, branch, true);
    }

    public bool StartCheckoutRevisionDialog(IWin32Window? owner, string? revision = null) => throw NotPorted(nameof(StartCheckoutRevisionDialog));
    public bool StartCherryPickDialog(IWin32Window? owner = null, GitRevision? revision = null)
    {
        bool Action()
        {
            using CommandsDialogs.FormCherryPick form = new(this, revision);
            return form.ShowDialog(owner) == DialogResult.OK;
        }

        return DoActionOnRepo(owner, Action);
    }

    public bool StartCherryPickDialog(IWin32Window? owner, IEnumerable<GitRevision> revisions)
    {
        ArgumentNullException.ThrowIfNull(revisions);

        bool Action()
        {
            CommandsDialogs.FormCherryPick? previousForm = null;
            try
            {
                bool repoChanged = false;
                foreach (GitRevision revision in revisions)
                {
                    CommandsDialogs.FormCherryPick form = new(this, revision);
                    if (previousForm is not null)
                    {
                        form.CopyOptions(previousForm);
                        ((IDisposable)previousForm).Dispose();
                    }

                    previousForm = form;
                    if (form.ShowDialog(owner) == DialogResult.OK)
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
                if (previousForm is not null)
                {
                    ((IDisposable)previousForm).Dispose();
                }
            }
        }

        return DoActionOnRepo(owner, Action);
    }

    public bool StartCleanupRepositoryDialog(IWin32Window? owner = null, string? path = null) => throw NotPorted(nameof(StartCleanupRepositoryDialog));
    public bool StartCloneDialog(IWin32Window? owner, string url, EventHandler<GitModuleEventArgs> gitModuleChanged)
    {
        return StartCloneDialog(owner, url, false, gitModuleChanged);
    }

    public bool StartCloneDialog(IWin32Window? owner, string? url = null, bool openedFromProtocolHandler = false, EventHandler<GitModuleEventArgs>? gitModuleChanged = null)
    {
        bool Action()
        {
            CommandsDialogs.FormClone form = new(this, url, openedFromProtocolHandler, gitModuleChanged);
            form.ShowDialog(owner);
            return true;
        }

        return DoActionOnRepo(owner, Action, requiresValidWorkingDir: false, changesRepo: false);
    }

    public void StartCloneForkFromHoster(IWin32Window? owner, IRepositoryHostPlugin gitHoster, EventHandler<GitModuleEventArgs>? gitModuleChanged) => throw NotPorted(nameof(StartCloneForkFromHoster));
    public bool StartCommitDialog(IWin32Window? owner, string? commitMessage = null, bool showOnlyWhenChanges = false)
    {
        if (Module.IsBareRepository())
        {
            return false;
        }

        return DoActionOnRepo(owner, action: () =>
        {
            if (showOnlyWhenChanges && Module.GetAllChangedFilesWithSubmodulesStatus(CancellationToken.None).Count == 0)
            {
                return true;
            }

            using CommandsDialogs.FormCommit form = new(this, commitMessage: commitMessage);
            form.ShowDialog(owner);
            return true;
        }, changesRepo: false, preEvent: PreCommit, postEvent: PostCommit);
    }

    public bool StartCompareRevisionsDialog(IWin32Window? owner = null) => throw NotPorted(nameof(StartCompareRevisionsDialog));
    public bool StartCreateBranchDialog(IWin32Window? owner, string? branch)
    {
        ObjectId objectId = Module.RevParse(branch!);
        if (objectId.IsZero)
        {
            MessageBoxes.Show($"Branch \"{branch}\" could not be resolved.", TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        return StartCreateBranchDialog(owner, objectId);
    }

    public bool StartCreateBranchDialog(IWin32Window? owner = null, ObjectId objectId = default, string? newBranchNamePrefix = null)
    {
        if (Module.IsBareRepository() || objectId.IsArtificial)
        {
            return false;
        }

        return DoActionOnRepo(owner, action: () =>
        {
            using CommandsDialogs.FormCreateBranch form = new(this, objectId, newBranchNamePrefix);
            return form.ShowDialog(owner) == DialogResult.OK;
        });
    }

    public void StartCreatePullRequest(IWin32Window? owner) => throw NotPorted(nameof(StartCreatePullRequest));
    public void StartCreatePullRequest(IWin32Window? owner, IRepositoryHostPlugin gitHoster, string? chooseRemote = null, string? chooseBranch = null) => throw NotPorted(nameof(StartCreatePullRequest));
    public bool StartCreateTagDialog(IWin32Window? owner = null, GitRevision? revision = null)
    {
        if (revision?.IsArtificial is true)
        {
            return false;
        }

        return DoActionOnRepo(owner, action: () =>
        {
            using CommandsDialogs.FormCreateTag form = new(this, revision?.ObjectId ?? default);
            return form.ShowDialog(owner) == DialogResult.OK;
        });
    }

    public bool StartDeleteBranchDialog(IWin32Window? owner, string branch)
    {
        return StartDeleteBranchDialog(owner, new[] { branch });
    }

    public bool StartDeleteBranchDialog(IWin32Window? owner, IEnumerable<string> branches)
    {
        return DoActionOnRepo(owner, action: () =>
        {
            using CommandsDialogs.FormDeleteBranch form = new(this, branches);
            form.ShowDialog(owner);
            return true;
        }, changesRepo: false);
    }

    public bool StartDeleteRemoteBranchDialog(IWin32Window? owner, string remoteBranch) => throw NotPorted(nameof(StartDeleteRemoteBranchDialog));
    public bool StartDeleteTagDialog(IWin32Window? owner, string? tag)
    {
        bool Action()
        {
            using CommandsDialogs.FormDeleteTag form = new(this, tag);
            return form.ShowDialog(owner) == DialogResult.OK;
        }

        return DoActionOnRepo(owner, Action);
    }

    public bool StartEditGitAttributesDialog(IWin32Window? owner = null) => throw NotPorted(nameof(StartEditGitAttributesDialog));
    public bool StartEditGitIgnoreDialog(IWin32Window? owner, bool localExcludes) => throw NotPorted(nameof(StartEditGitIgnoreDialog));
    public bool StartFileEditorDialog(string? filename, bool showWarning = false, int? lineNumber = null)
    {
        using FormEditor formEditor = new(this, filename, showWarning, lineNumber: lineNumber);
        return formEditor.ShowDialog() != DialogResult.Cancel;
    }

    public void StartFileHistoryDialog(IWin32Window? owner, string fileName, GitRevision? revision = null, bool filterByRevision = false, bool showBlame = false)
    {
        // The WinForms client launches a separate process (or reuses Browse) for file
        // history; this twin opens the window in-process, non-modal like that process.
        DoActionOnRepo(owner, action: () =>
        {
            CommandsDialogs.FormFileHistory form = new(this, fileName, revision, filterByRevision, showBlame);
            ShowModelessWindow(form, owner);
            return true;
        }, changesRepo: false);
    }

    private static void ShowModelessWindow(Window form, IWin32Window? owner)
    {
        if (AvaloniaApplication.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is null)
        {
            desktop.MainWindow = form;
            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
            form.Show();
            return;
        }

        if (owner is Window ownerWindow && ownerWindow.IsVisible)
        {
            form.Show(ownerWindow);
        }
        else
        {
            form.Show();
        }
    }

    public bool StartFixupCommitDialog(IWin32Window? owner, GitRevision revision)
    {
        bool Action()
        {
            using CommandsDialogs.FormCommit form = new(this, CommandsDialogs.CommitKind.Fixup, revision);
            form.ShowDialog(owner);
            return true;
        }

        return DoActionOnRepo(owner, Action);
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

    public bool StartFormatPatchDialog(IWin32Window? owner = null) => throw NotPorted(nameof(StartFormatPatchDialog));
    public bool StartGeneralSettingsDialog(IWin32Window? owner)
        => StartSettingsDialog(owner, CommandsDialogs.SettingsDialog.Pages.GeneralSettingsPage.GetPageReference());
    public bool StartInitializeDialog(IWin32Window? owner = null, string? dir = null, EventHandler<GitModuleEventArgs>? gitModuleChanged = null)
    {
        bool Action()
        {
            dir ??= Module.IsValidGitWorkingDir() ? Module.WorkingDir : string.Empty;

            CommandsDialogs.FormInit frm = new(this, dir, gitModuleChanged);
            frm.ShowDialog(owner);
            return true;
        }

        return DoActionOnRepo(owner, Action, requiresValidWorkingDir: false, changesRepo: false);
    }

    public bool StartInteractiveRebase(IWin32Window? owner, string onto)
    {
        return StartRebaseDialog(
            owner,
            from: string.Empty,
            to: null,
            onto,
            interactive: true,
            startRebaseImmediately: true);
    }

    public bool StartMailMapDialog(IWin32Window? owner = null) => throw NotPorted(nameof(StartMailMapDialog));
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

    public bool StartPluginSettingsDialog(IWin32Window? owner) => StartSettingsDialog(owner);
    public bool StartPullDialog(IWin32Window? owner = null, string? remoteBranch = null, string? remote = null, GitPullAction pullAction = GitPullAction.None)
        => StartPullDialogInternal(owner, pullOnShow: false, out _, remoteBranch, remote, pullAction);

    public bool StartPullDialogAndPullImmediately(IWin32Window? owner = null, string? remoteBranch = null, string? remote = null, GitPullAction pullAction = GitPullAction.None)
        => StartPullDialogAndPullImmediately(out _, owner, remoteBranch, remote, pullAction);

    public bool StartPullDialogAndPullImmediately(out bool pullCompleted, IWin32Window? owner = null, string? remoteBranch = null, string? remote = null, GitPullAction pullAction = GitPullAction.None)
        => StartPullDialogInternal(owner, pullOnShow: true, out pullCompleted, remoteBranch, remote, pullAction);

    private bool StartPullDialogInternal(
        IWin32Window? owner,
        bool pullOnShow,
        out bool pullCompleted,
        string? remoteBranch,
        string? remote,
        GitPullAction pullAction)
    {
        bool pulled = false;
        bool done = DoActionOnRepo(owner, action: () =>
        {
            using CommandsDialogs.FormPull form = new(this, remoteBranch, remote, pullAction);
            DialogResult result = pullOnShow
                ? form.PullAndShowDialogWhenFailed(owner, remote, pullAction)
                : form.ShowDialog(owner);
            pulled = result == DialogResult.OK && !form.ErrorOccurred;
            return result == DialogResult.OK;
        });

        pullCompleted = pulled;
        return done;
    }

    public void StartPullRequestsDialog(IWin32Window? owner, IRepositoryHostPlugin gitHoster) => throw NotPorted(nameof(StartPullRequestsDialog));
    public bool StartPushDialog(IWin32Window? owner, bool pushOnShow)
        => StartPushDialog(owner, pushOnShow, forceWithLease: false, out _);

    public bool StartPushDialog(IWin32Window? owner, bool pushOnShow, bool forceWithLease, out bool pushCompleted, string? branchName = null)
    {
        bool pushed = false;
        bool done = DoActionOnRepo(owner, action: () =>
        {
            using CommandsDialogs.FormPush form = new(this, branchName);
            if (forceWithLease)
            {
                form.CheckForceWithLease();
            }

            DialogResult result = pushOnShow
                ? form.PushAndShowDialogWhenFailed(owner)
                : form.ShowDialog(owner);
            pushed = result == DialogResult.OK && !form.ErrorOccurred;
            return result == DialogResult.OK;
        });

        pushCompleted = pushed;
        return done;
    }

    public bool StartRebase(IWin32Window? owner, string onto)
    {
        return StartRebaseDialog(
            owner,
            from: string.Empty,
            to: null,
            onto,
            interactive: false,
            startRebaseImmediately: true);
    }

    public bool StartRebaseDialog(
        IWin32Window? owner,
        string? from,
        string? to,
        string? onto,
        bool interactive = false,
        bool startRebaseImmediately = true)
    {
        bool Action()
        {
            using FormRebase form = new(this, from, to, onto, interactive, startRebaseImmediately);
            form.ShowDialog(owner);
            return true;
        }

        return DoActionOnRepo(owner, Action);
    }

    public bool StartRebaseDialog(IWin32Window? owner, string? onto)
    {
        return StartRebaseDialog(
            owner,
            from: string.Empty,
            to: null,
            onto,
            interactive: false,
            startRebaseImmediately: false);
    }

    public bool StartRebaseDialogWithAdvOptions(IWin32Window? owner, string onto, string from = "")
    {
        return StartRebaseDialog(
            owner,
            from,
            to: null,
            onto,
            interactive: false,
            startRebaseImmediately: false);
    }

    public bool StartRemotesDialog(IWin32Window? owner, string? preselectRemote = null, string? preselectLocal = null)
    {
        bool Action()
        {
            CommandsDialogs.FormRemotes form = new(this)
            {
                PreselectRemoteOnLoad = preselectRemote,
                PreselectLocalOnLoad = preselectLocal
            };
            form.ShowDialog(owner);
            return true;
        }

        return DoActionOnRepo(owner, Action);
    }

    public bool StartRenameDialog(IWin32Window? owner, string branch)
    {
        bool Action()
        {
            using CommandsDialogs.FormRenameBranch form = new(this, branch);
            return form.ShowDialog(owner) == DialogResult.OK;
        }

        return DoActionOnRepo(owner, Action);
    }

    public bool StartRepoSettingsDialog(IWin32Window? owner) => StartSettingsDialog(owner);
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

    public bool StartResetCurrentBranchDialog(IWin32Window? owner, string branch)
    {
        ObjectId objectId = Module.RevParse(branch);
        if (objectId.IsZero)
        {
            MessageBoxes.Show($"Branch \"{branch}\" could not be resolved.", TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        using FormResetCurrentBranch form = FormResetCurrentBranch.Create(this, Module.GetRevision(objectId));
        return form.ShowDialog(owner) == DialogResult.OK;
    }

    public bool StartResolveConflictsDialog(IWin32Window? owner = null, bool offerCommit = true)
    {
        bool Action()
        {
            using CommandsDialogs.FormResolveConflicts form = new(this, offerCommit);
            form.ShowDialog(owner);
            return true;
        }

        return DoActionOnRepo(owner, Action);
    }

    public bool StartRevertCommitDialog(IWin32Window? owner, GitRevision revision)
    {
        bool Action()
        {
            using CommandsDialogs.FormRevertCommit form = new(this, revision);
            return form.ShowDialog(owner) == DialogResult.OK;
        }

        return DoActionOnRepo(owner, Action);
    }

    public bool StartSettingsDialog(IGitPlugin gitPlugin)
        => StartSettingsDialog(owner: null, new SettingsPageReferenceByPlugin(gitPlugin));

    public bool StartSettingsDialog(IWin32Window? owner, SettingsPageReference? initialPage = null)
    {
        bool Action()
            => FormSettings.ShowSettingsDialog(this, owner, initialPage) is DialogResult.OK;

        return DoActionOnRepo(owner, Action, requiresValidWorkingDir: false, postEvent: PostSettings);
    }

    public bool StartSettingsDialog(Type pageType)
        => StartSettingsDialog(owner: null, new SettingsPageReferenceByType(pageType));
    public bool StartSparseWorkingCopyDialog(IWin32Window? owner) => throw NotPorted(nameof(StartSparseWorkingCopyDialog));
    public bool StartSquashCommitDialog(IWin32Window? owner, GitRevision revision)
    {
        bool Action()
        {
            using CommandsDialogs.FormCommit form = new(this, CommandsDialogs.CommitKind.Squash, revision);
            form.ShowDialog(owner);
            return true;
        }

        return DoActionOnRepo(owner, Action);
    }

    public bool StartStashDialog(IWin32Window? owner = null, bool manageStashes = true, string? initialStash = null)
    {
        bool Action()
        {
            using FormStash form = new(this, initialStash) { ManageStashes = manageStashes };
            form.ShowDialog(owner);
            return true;
        }

        return DoActionOnRepo(owner, Action, changesRepo: false);
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

    public bool StartSyncSubmodulesDialog(IWin32Window? owner)
    {
        bool Action()
        {
            return FormProcess.ShowDialog(owner, this, arguments: Commands.SubmoduleSync(""), Module.WorkingDir, input: null, useDialogSettings: true);
        }

        return DoActionOnRepo(owner, Action);
    }

    public bool StartTheContinueRebaseDialog(IWin32Window? owner)
    {
        return StartRebaseDialog(
            owner,
            from: string.Empty,
            to: null,
            onto: null,
            interactive: false,
            startRebaseImmediately: false);
    }

    public bool StartUpdateSubmoduleDialog(IWin32Window? owner, string submoduleLocalPath, string submoduleParentPath)
    {
        bool Action()
        {
            // Execute the submodule update command from the submodule's parent directory
            return FormProcess.ShowDialog(owner, this, arguments: Commands.SubmoduleUpdate(submoduleLocalPath), submoduleParentPath, null, true);
        }

        return DoActionOnRepo(owner, Action, postEvent: PostUpdateSubmodules);
    }

    public bool StartUpdateSubmodulesDialog(IWin32Window? owner, string submoduleLocalPath = "")
    {
        bool Action()
        {
            return FormProcess.ShowDialog(owner, this, arguments: Commands.SubmoduleUpdate(submoduleLocalPath), Module.WorkingDir, input: null, useDialogSettings: true);
        }

        return DoActionOnRepo(owner, Action, postEvent: PostUpdateSubmodules);
    }

    public bool StartVerifyDatabaseDialog(IWin32Window? owner = null)
    {
        bool Action()
        {
            using FormVerify form = new(this);
            form.ShowDialog(owner);
            return true;
        }

        return DoActionOnRepo(owner, Action);
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

    public bool StartViewPatchDialog(string patchFile)
    {
        return StartViewPatchDialog(null, patchFile);
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

    public bool WorktreeCreate(IWin32Window? owner, string mainWorktreePath)
    {
        return DoActionOnRepo(owner, action: () =>
        {
            using FormCreateWorktree form = new(this, mainWorktreePath);
            if (form.ShowDialog(owner) != DialogResult.OK)
            {
                return false;
            }

            if (form.OpenWorktree)
            {
                GitModule newModule = new(this.GetRequiredService<IGitExecutorProvider>(), form.WorktreeDirectory);
                if (newModule.IsValidGitWorkingDir() && FindFormBrowse(owner) is FormBrowse browse)
                {
                    browse.SetWorkingDir(Path.GetFullPath(form.WorktreeDirectory));
                }
            }

            return true;
        });
    }

    public bool WorktreeDelete(IWin32Window? owner, string worktreePath)
    {
        return DoActionOnRepo(owner, action: () =>
        {
            TaskDialogButton result = TaskDialog.ShowDialog(owner!, new TaskDialogPage
            {
                Text = string.Format(TranslatedStrings.DeleteWorktreeConfirmation, worktreePath),
                Caption = TranslatedStrings.DeleteWorktreeCaption,
                Heading = TranslatedStrings.CannotBeUndone,
                Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
                Icon = TaskDialogIcon.Warning,
                SizeToContent = true,
            });

            if (result != TaskDialogButton.Yes)
            {
                return false;
            }

            if (!worktreePath.TryDeleteDirectory(out string? errorMessage))
            {
                TaskDialog.ShowDialog(owner!, new TaskDialogPage
                {
                    Text = $"{string.Format(TranslatedStrings.DeleteWorktreeFailed, worktreePath)}\n{errorMessage}",
                    Caption = TranslatedStrings.Error,
                    Icon = TaskDialogIcon.Error,
                    SizeToContent = true,
                });

                return false;
            }

            StartCommandLineProcessDialog(owner, command: null, "worktree prune");
            return true;
        });
    }

    public bool WorktreeSwitch(IWin32Window? owner, string worktreePath)
    {
        if (!AppSettings.DontConfirmSwitchWorktree)
        {
            TaskDialogButton result = TaskDialog.ShowDialog(owner!, new TaskDialogPage
            {
                Text = string.Format(TranslatedStrings.SwitchWorktreeConfirmation, worktreePath),
                Caption = TranslatedStrings.SwitchWorktreeCaption,
                Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
                Icon = TaskDialogIcon.Information,
                SizeToContent = true,
            });

            if (result != TaskDialogButton.Yes)
            {
                return false;
            }
        }

        if (!Directory.Exists(worktreePath))
        {
            return false;
        }

        if (FindFormBrowse(owner) is FormBrowse browse)
        {
            browse.SetWorkingDir(Path.GetFullPath(worktreePath));
        }

        return true;
    }

    private static FormBrowse? FindFormBrowse(IWin32Window? window)
    {
        if (window is FormBrowse browse)
        {
            return browse;
        }

        if (window is Avalonia.Controls.WindowBase avaloniaWindow)
        {
            while (avaloniaWindow.Owner is not null)
            {
                if (avaloniaWindow.Owner is FormBrowse ownerBrowse)
                {
                    return ownerBrowse;
                }

                avaloniaWindow = avaloniaWindow.Owner;
            }
        }

        return null;
    }

    #endregion
}
