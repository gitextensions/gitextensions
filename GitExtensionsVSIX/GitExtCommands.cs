using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

using EnvDTE;
using EnvDTE80;

using GitPluginShared;
using GitPluginShared.Commands;

using Microsoft.VisualStudio.Shell;

using Constants = EnvDTE.Constants;
using static GitExtensionsVSIX.PackageIds;

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

        private readonly Dictionary<string, CommandBase> _commandsByName = new Dictionary<string, CommandBase>();
        private readonly Dictionary<int, CommandBase> _commands = new Dictionary<int, CommandBase>();

        private readonly DTE2 _application;
        private OutputWindowPane _outputPane;
        private OleMenuCommandService _commandService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitExtCommands"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private GitExtCommands(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            _package = package;
            _application = (DTE2)ServiceProvider.GetService(typeof(DTE));
            _commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            try
            {
                //RegisterCommand("Difftool_Selection", new ToolbarCommand<OpenWithDiftool>(runForSelection: true));
                RegisterCommand("Difftool", new ToolbarCommand<OpenWithDiftool>(), gitExtDiffCommand);
                //RegisterCommand("ShowFileHistory_Selection", new ToolbarCommand<FileHistory>(runForSelection: true));
                RegisterCommand("ShowFileHistory", new ToolbarCommand<FileHistory>(), gitExtHistoryCommand);
                //RegisterCommand("ResetChanges_Selection", new ToolbarCommand<Revert>(runForSelection: true));
                RegisterCommand("ResetChanges", new ToolbarCommand<Revert>(), gitExtResetFileCommand);
                RegisterCommand("Browse", new ToolbarCommand<Browse>(), gitExtBrowseCommand);
                RegisterCommand("Clone", new ToolbarCommand<Clone>(), gitExtCloneCommand);
                RegisterCommand("CreateNewRepository", new ToolbarCommand<Init>(), gitExtNewCommand);
                RegisterCommand("Commit", new ToolbarCommand<Commit>(), gitExtCommitCommand);
                RegisterCommand("Pull", new ToolbarCommand<Pull>(), gitExtPullCommand);
                RegisterCommand("Push", new ToolbarCommand<Push>(), gitExtPushCommand);
                RegisterCommand("Stash", new ToolbarCommand<Stash>(), gitExtStashCommand);
                RegisterCommand("Remotes", new ToolbarCommand<Remotes>(), gitExtRemotesCommand);
                RegisterCommand("GitIgnore", new ToolbarCommand<GitIgnore>(), gitExtGitIgnoreCommand);
                RegisterCommand("ApplyPatch", new ToolbarCommand<ApplyPatch>(), gitExtApplyPatchCommand);
                RegisterCommand("FormatPatch", new ToolbarCommand<FormatPatch>(), gitExtFormatPatchCommand);
                RegisterCommand("ViewChanges", new ToolbarCommand<ViewChanges>(), gitExtViewChangesCommand);
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
            catch (Exception ex)
            {
                if (OutputPane != null)
                    OutputPane.OutputString("Error adding commands: " + ex);
            }
        }

        private void RegisterCommand(string commandName, CommandBase command, int id)
        {
            _commandsByName[commandName] = command;
            var commandId = new CommandID(CommandSet, id);
            var menuCommand = new MenuCommand(MenuItemCallback, commandId);
            _commandService.AddCommand(menuCommand);
            _commands[id] = command;
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static GitExtCommands Instance { get; private set; }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get { return _package; }
        }

        public OutputWindowPane OutputPane
        {
            get { return _outputPane ?? (_outputPane = PluginHelpers.AquireOutputPane(_application, Vsix.Name)); }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new GitExtCommands(package);
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
            CommandBase command;
            if (!_commands.TryGetValue(guiCommand.CommandID.ID, out command))
                return;
            command.OnCommand(_application, OutputPane);
        }
    }
}
