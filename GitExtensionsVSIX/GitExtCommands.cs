using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Threading;
using EnvDTE;
using GitExtensionsVSIX.Commands;
using Microsoft.VisualStudio.Shell;
using static GitExtensionsVSIX.PackageIds;
using Task = System.Threading.Tasks.Task;

namespace GitExtensionsVSIX
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class GitExtCommands
    {
        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        private static readonly Guid CommandSet = PackageGuids.guidGitExtensionsPackageCmdSet;

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package _package;

        private readonly Dictionary<string, VsixCommandBase> _commandsByName = new Dictionary<string, VsixCommandBase>();
        private readonly Dictionary<int, VsixCommandBase> _commands = new Dictionary<int, VsixCommandBase>();

        private readonly _DTE _application;
        private OutputWindowPane _outputPane;
        private readonly IMenuCommandService _commandService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitExtCommands"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private GitExtCommands(Package package, _DTE dte, IMenuCommandService menuCommandService)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            _package = package;
            _application = dte;
            _commandService = menuCommandService;

            try
            {
                RegisterCommands();
                PluginHelpers.AllowCaptionUpdate = true;
            }
            catch (Exception ex)
            {
                OutputPane?.OutputString("Error adding commands: " + ex);
            }
        }

        private void RegisterCommands()
        {
            ////RegisterCommand("Difftool_Selection", new ToolbarCommand<OpenWithDiftool>(runForSelection: true));
            RegisterCommand("Difftool", new ToolbarCommand<OpenWithDiftool>(), gitExtDiffCommand);
            ////RegisterCommand("ShowFileHistory_Selection", new ToolbarCommand<FileHistory>(runForSelection: true));
            RegisterCommand("ShowFileHistory", new ToolbarCommand<FileHistory>(), gitExtHistoryCommand);
            ////RegisterCommand("ResetChanges_Selection", new ToolbarCommand<Revert>(runForSelection: true));
            RegisterCommand("ResetChanges", new ToolbarCommand<Revert>(), gitExtResetFileCommand);
            RegisterCommand("Browse", new ToolbarCommand<Browse>(), gitExtBrowseCommand);
            RegisterCommand("Clone", new ToolbarCommand<Clone>(), gitExtCloneCommand);
            RegisterCommand("CreateNewRepository", new ToolbarCommand<Init>(), gitExtNewCommand);
            RegisterCommand("Commit", new Commit(), gitExtCommitCommand);
            RegisterCommand("Pull", new ToolbarCommand<Pull>(), gitExtPullCommand);
            RegisterCommand("Push", new ToolbarCommand<Push>(), gitExtPushCommand);
            RegisterCommand("Stash", new ToolbarCommand<Stash>(), gitExtStashCommand);
            RegisterCommand("Remotes", new ToolbarCommand<Remotes>(), gitExtRemotesCommand);
            RegisterCommand("GitIgnore", new ToolbarCommand<GitIgnore>(), gitExtGitIgnoreCommand);
            RegisterCommand("ApplyPatch", new ToolbarCommand<ApplyPatch>(), gitExtApplyPatchCommand);
            RegisterCommand("FormatPatch", new ToolbarCommand<FormatPatch>(), gitExtFormatPatchCommand);
            RegisterCommand("ViewChanges", new ToolbarCommand<ViewChanges>(), gitExtViewChangesCommand);
            RegisterCommand("Blame", new ToolbarCommand<Blame>(), gitExtBlameCommand);
            RegisterCommand("FindFile", new ToolbarCommand<FindFile>(), gitExtFindFileCommand);
            RegisterCommand("SwitchBranch", new ToolbarCommand<SwitchBranch>(), gitExtCheckoutCommand);
            RegisterCommand("CreateBranch", new ToolbarCommand<CreateBranch>(), gitExtCreateBranchCommand);
            RegisterCommand("Merge", new ToolbarCommand<Merge>(), gitExtMergeCommand);
            RegisterCommand("Rebase", new ToolbarCommand<Rebase>(), gitExtRebaseCommand);
            RegisterCommand("SolveMergeConflicts", new ToolbarCommand<SolveMergeConflicts>(), gitExtSolveConflictsCommand);
            RegisterCommand("CherryPick", new ToolbarCommand<Cherry>(), gitExtCherryPickCommand);
            RegisterCommand("Bash", new ToolbarCommand<Bash>(), gitExtBashCommand);
            RegisterCommand("Settings", new ToolbarCommand<Settings>(), gitExtSettingsCommand);
            RegisterCommand("About", new ToolbarCommand<About>(), gitExtAboutCommand);
        }

        private void RegisterCommand(string commandName, CommandBase command, int id)
        {
            RegisterCommand(commandName, new VsixCommandBase(command), id);
        }

        private void RegisterCommand(string commandName, VsixCommandBase command, int id)
        {
            _commandsByName[commandName] = command;
            var commandId = new CommandID(CommandSet, id);
            var menuCommand = new OleMenuCommand(MenuItemCallback, commandId);
            menuCommand.BeforeQueryStatus += MenuCommand_BeforeQueryStatus;
            _commandService.AddCommand(menuCommand);
            _commands[id] = command;
        }

        private void MenuCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand guiCommand = (OleMenuCommand)sender;
            if (_commands.TryGetValue(guiCommand.CommandID.ID, out var command))
            {
                command.BeforeQueryStatus(_application, guiCommand);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static GitExtCommands Instance { get; private set; }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider => _package;

        public OutputWindowPane OutputPane => _outputPane ?? (_outputPane = PluginHelpers.AquireOutputPane(_application, Vsix.Name));

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package, CancellationToken cancellationToken)
        {
            var dteObject = await package.GetServiceAsync(typeof(DTE));
            var menuCommandServiceObject = await package.GetServiceAsync(typeof(IMenuCommandService));

            cancellationToken.ThrowIfCancellationRequested();
            Debug.Assert(dteObject != null, $"Assertion failed: {nameof(dteObject)} != null");
            Debug.Assert(menuCommandServiceObject != null, $"Assertion failed: {nameof(menuCommandServiceObject)} != null");

            await package.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            Instance = new GitExtCommands(package, (_DTE)dteObject, (IMenuCommandService)menuCommandServiceObject);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var guiCommand = (MenuCommand)sender;
            if (_commands.TryGetValue(guiCommand.CommandID.ID, out var command))
            {
                command.BaseCommand.OnCommand(_application, OutputPane);
            }
        }
    }
}
