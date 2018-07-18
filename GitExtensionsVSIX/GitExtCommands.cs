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

            _package = package ?? throw new ArgumentNullException(nameof(package));
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
            ////RegisterCommand(new ToolbarCommand<OpenWithDifftool>(runForSelection: true));
            RegisterCommand(new ToolbarCommand<OpenWithDifftool>(), gitExtDiffCommand);
            ////RegisterCommand(new ToolbarCommand<FileHistory>(runForSelection: true));
            RegisterCommand(new ToolbarCommand<FileHistory>(), gitExtHistoryCommand);
            ////RegisterCommand(new ToolbarCommand<Revert>(runForSelection: true));
            RegisterCommand(new ToolbarCommand<Revert>(), gitExtResetFileCommand);
            RegisterCommand(new ToolbarCommand<Browse>(), gitExtBrowseCommand);
            RegisterCommand(new ToolbarCommand<Clone>(), gitExtCloneCommand);
            RegisterCommand(new ToolbarCommand<Init>(), gitExtNewCommand);
            RegisterCommand(new Commit(), gitExtCommitCommand);
            RegisterCommand(new ToolbarCommand<Pull>(), gitExtPullCommand);
            RegisterCommand(new ToolbarCommand<Push>(), gitExtPushCommand);
            RegisterCommand(new ToolbarCommand<Stash>(), gitExtStashCommand);
            RegisterCommand(new ToolbarCommand<Remotes>(), gitExtRemotesCommand);
            RegisterCommand(new ToolbarCommand<GitIgnore>(), gitExtGitIgnoreCommand);
            RegisterCommand(new ToolbarCommand<ApplyPatch>(), gitExtApplyPatchCommand);
            RegisterCommand(new ToolbarCommand<FormatPatch>(), gitExtFormatPatchCommand);
            RegisterCommand(new ToolbarCommand<ViewChanges>(), gitExtViewChangesCommand);
            RegisterCommand(new ToolbarCommand<Blame>(), gitExtBlameCommand);
            RegisterCommand(new ToolbarCommand<FindFile>(), gitExtFindFileCommand);
            RegisterCommand(new ToolbarCommand<SwitchBranch>(), gitExtCheckoutCommand);
            RegisterCommand(new ToolbarCommand<CreateBranch>(), gitExtCreateBranchCommand);
            RegisterCommand(new ToolbarCommand<Merge>(), gitExtMergeCommand);
            RegisterCommand(new ToolbarCommand<Rebase>(), gitExtRebaseCommand);
            RegisterCommand(new ToolbarCommand<SolveMergeConflicts>(), gitExtSolveConflictsCommand);
            RegisterCommand(new ToolbarCommand<Cherry>(), gitExtCherryPickCommand);
            RegisterCommand(new ToolbarCommand<Bash>(), gitExtBashCommand);
            RegisterCommand(new ToolbarCommand<Settings>(), gitExtSettingsCommand);
            RegisterCommand(new ToolbarCommand<About>(), gitExtAboutCommand);
        }

        private void RegisterCommand(CommandBase command, int id)
        {
            var commandId = new CommandID(CommandSet, id);
            var menuCommand = new OleMenuCommand(MenuItemCallback, commandId);
            menuCommand.BeforeQueryStatus += MenuCommand_BeforeQueryStatus;
            _commandService.AddCommand(menuCommand);
            _commands[id] = new VsixCommandBase(command);
        }

        private void MenuCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            var guiCommand = (OleMenuCommand)sender;
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

        public OutputWindowPane OutputPane => _outputPane ?? (_outputPane = PluginHelpers.AcquireOutputPane(_application, Vsix.Name));

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
