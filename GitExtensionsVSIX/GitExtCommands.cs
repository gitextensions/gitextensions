using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

using EnvDTE;
using EnvDTE80;

using GitPlugin.Commands;

using Microsoft.VisualStudio.Shell;

using Constants = EnvDTE.Constants;

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
        public static readonly Guid CommandSet = new Guid("8bd71b0f-f446-442d-a92a-bbefd8e60202");

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
                RegisterCommand("Difftool", new ToolbarCommand<OpenWithDiftool>(), 0x1100);
                //RegisterCommand("ShowFileHistory_Selection", new ToolbarCommand<FileHistory>(runForSelection: true));
                RegisterCommand("ShowFileHistory", new ToolbarCommand<FileHistory>(), 0x1101);
                //RegisterCommand("ResetChanges_Selection", new ToolbarCommand<Revert>(runForSelection: true));
                RegisterCommand("ResetChanges", new ToolbarCommand<Revert>(), 0x1102);
                RegisterCommand("Browse", new ToolbarCommand<Browse>(), 0x1103);
                RegisterCommand("Clone", new ToolbarCommand<Clone>(), 0x1104);
                RegisterCommand("CreateNewRepository", new ToolbarCommand<Init>(), 0x1105);
                RegisterCommand("Commit", new ToolbarCommand<Commit>(), 0x1106);
                RegisterCommand("Pull", new ToolbarCommand<Pull>(), 0x1107);
                RegisterCommand("Push", new ToolbarCommand<Push>(), 0x1108);
                RegisterCommand("Stash", new ToolbarCommand<Stash>(), 0x1109);
                RegisterCommand("Remotes", new ToolbarCommand<Remotes>(), 0x110a);
                RegisterCommand("GitIgnore", new ToolbarCommand<GitIgnore>(), 0x110b);
                RegisterCommand("ApplyPatch", new ToolbarCommand<ApplyPatch>(), 0x110c);
                RegisterCommand("FormatPatch", new ToolbarCommand<FormatPatch>(), 0x110d);
                RegisterCommand("ViewChanges", new ToolbarCommand<ViewChanges>(), 0x110e);
                RegisterCommand("FindFile", new ToolbarCommand<FindFile>(), 0x110f);
                RegisterCommand("SwitchBranch", new ToolbarCommand<SwitchBranch>(), 0x1110);
                RegisterCommand("CreateBranch", new ToolbarCommand<CreateBranch>(), 0x1111);
                RegisterCommand("Merge", new ToolbarCommand<Merge>(), 0x1112);
                RegisterCommand("Rebase", new ToolbarCommand<Rebase>(), 0x1113);
                RegisterCommand("SolveMergeConflicts", new ToolbarCommand<SolveMergeConflicts>(), 0x1114);
                RegisterCommand("CherryPick", new ToolbarCommand<Cherry>(), 0x1115);
                RegisterCommand("Bash", new ToolbarCommand<Bash>(), 0x1116);
                RegisterCommand("Settings", new ToolbarCommand<Settings>(), 0x1117);
                RegisterCommand("About", new ToolbarCommand<About>(), 0x1118);
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
            get { return _outputPane ?? (_outputPane = AquireOutputPane(_application, "GitExtensions")); }
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

        private static OutputWindowPane AquireOutputPane(DTE2 app, string name)
        {
            try
            {
                if (name == "")
                    return null;

                OutputWindowPane result = Plugin.FindOutputPane(app, name);
                if (result != null)
                    return result;

                var outputWindow = (OutputWindow)app.Windows.Item(Constants.vsWindowKindOutput).Object;
                OutputWindowPanes panes = outputWindow.OutputWindowPanes;
                return panes.Add(name);
            }
            catch (Exception)
            {
                //ignore!!
                return null;
            }
        }
    }
}
