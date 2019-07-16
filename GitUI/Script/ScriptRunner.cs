using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.Browsing;
using GitUI.Browsing.Dialogs;
using GitUIPluginInterfaces;

namespace GitUI.Script
{
    internal interface IScriptRunner
    {
        /// <summary>
        /// Tries to run scripts identified by a <paramref name="command"/>
        /// </summary>
        CommandStatus RunScript(int command);

        CommandStatus RunScript(string scriptKey);

        CommandStatus RunScripts(ScriptEvent scriptEvent);
    }

    internal sealed class ScriptRunner : IScriptRunner
    {
        private const string PluginPrefix = "plugin:";
        private const string NavigateToPrefix = "navigateTo:";

        private readonly Func<IGitModule> _getModule;
        private readonly GitUIEventArgs _gitUIEventArgs;
        private readonly IScriptOptionsParser _scriptOptionsParser;
        private readonly ISimpleDialog _simpleDialog;
        private readonly IScriptManager _scriptManager;
        private readonly ICanGoToRef _canGoToRef;

        public ScriptRunner(
            Func<IGitModule> getModule,
            GitUIEventArgs gitUIEventArgs,
            IScriptOptionsParser scriptOptionsParser,
            ISimpleDialog simpleDialog,
            IScriptManager scriptManager,
            ICanGoToRef canGoToRef = null)
        {
            _getModule = getModule ?? throw new ArgumentNullException(nameof(getModule));
            _gitUIEventArgs = gitUIEventArgs ?? throw new ArgumentNullException(nameof(gitUIEventArgs));
            _scriptOptionsParser = scriptOptionsParser ?? throw new ArgumentNullException(nameof(scriptOptionsParser));
            _simpleDialog = simpleDialog ?? throw new ArgumentNullException(nameof(simpleDialog));
            _scriptManager = scriptManager ?? throw new ArgumentNullException(nameof(scriptManager));
            _canGoToRef = canGoToRef;
        }

        /// <inheritdoc />
        public CommandStatus RunScript(int command)
        {
            var anyScriptExecuted = false;
            var needsGridRefresh = false;

            foreach (var script in _scriptManager.GetScripts())
            {
                if (script.HotkeyCommandIdentifier == command)
                {
                    var result = RunScript(script);

                    anyScriptExecuted = true;
                    needsGridRefresh |= result.NeedsGridRefresh;
                }
            }

            return new CommandStatus(anyScriptExecuted, needsGridRefresh);
        }

        public CommandStatus RunScript(string scriptKey)
        {
            if (string.IsNullOrEmpty(scriptKey))
            {
                return false;
            }

            var script = _scriptManager.GetScript(scriptKey);

            if (script == null)
            {
                _simpleDialog.ShowOkDialog($"Cannot find script: {scriptKey}", "Error", MessageBoxIcon.Error);

                return false;
            }

            return RunScript(script);
        }

        public CommandStatus RunScripts(ScriptEvent scriptEvent)
        {
            if (scriptEvent == ScriptEvent.ShowInUserMenuBar)
            {
                // TODO: handle more gracefully
                throw new NotSupportedException();
            }

            bool executed = false;
            bool refreshRequired = false;
            foreach (var script in _scriptManager.GetScripts()
                .Where(x => x.Enabled && x.OnEvent == scriptEvent))
            {
                var result = RunScript(script);
                executed |= result.Executed;
                refreshRequired |= result.NeedsGridRefresh;
            }

            return new CommandStatus(executed, refreshRequired);
        }

        private CommandStatus RunScript(ScriptInfo script)
        {
            if (string.IsNullOrEmpty(script.Command))
            {
                return false;
            }

            if (_canGoToRef == null && !string.IsNullOrEmpty(script.Arguments))
            {
                var option = ScriptOptions.All
                    .Where(x => script.Arguments.Contains(x))
                    .FirstOrDefault(x => x.StartsWith("s"));

                if (option != null)
                {
                    _simpleDialog.ShowOkDialog($"Option {option} is only supported when started from revision grid.", "Error", MessageBoxIcon.Error);

                    return false;
                }
            }

            if (script.AskConfirmation)
            {
                var dialogResult = _simpleDialog
                    .ShowYesNoDialog($"Do you want to execute '{script.Name}'?", "Script", MessageBoxIcon.Question);

                if (dialogResult == DialogResult.No)
                {
                    return false;
                }
            }

            var originalCommand = script.Command;
            (string argument, bool abort) = _scriptOptionsParser.Parse(script.Arguments);

            if (abort)
            {
                return false;
            }

            var command = OverrideCommandWhenNecessary(originalCommand);

            command = ExpandCommandVariables(command, _getModule());

            if (script.IsPowerShell)
            {
                PowerShellHelper.RunPowerShell(command, argument, _getModule().WorkingDir, script.RunInBackground);
                return new CommandStatus(true, false);
            }

            if (command.StartsWith(PluginPrefix))
            {
                command = command.Replace(PluginPrefix, "");
                foreach (var plugin in PluginRegistry.Plugins)
                {
                    if (plugin.Description.ToLower().Equals(command, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return new CommandStatus(true, plugin.Execute(_gitUIEventArgs));
                    }
                }

                return false;
            }

            if (command.StartsWith(NavigateToPrefix))
            {
                if (_canGoToRef == null)
                {
                    return false;
                }

                command = command.Replace(NavigateToPrefix, string.Empty);
                if (!command.IsNullOrEmpty())
                {
                    var revisionRef = new Executable(command, _getModule().WorkingDir).GetOutputLines(argument).FirstOrDefault();

                    if (revisionRef != null)
                    {
                        _canGoToRef.GoToRef(revisionRef, true);
                    }
                }

                return new CommandStatus(true, false);
            }

            if (!script.RunInBackground)
            {
                _simpleDialog.ShowStandardProcessDialog(command, argument, _getModule().WorkingDir, null, true);
            }
            else
            {
                if (originalCommand.Equals("{openurl}", StringComparison.CurrentCultureIgnoreCase))
                {
                    Process.Start(argument);
                }
                else
                {
                    new Executable(command, _getModule().WorkingDir).Start(argument);
                }
            }

            return new CommandStatus(true, !script.RunInBackground);
        }

        private static string ExpandCommandVariables(string originalCommand, IGitModule module)
        {
            return originalCommand.Replace("{WorkingDir}", module.WorkingDir);
        }

        private static string OverrideCommandWhenNecessary(string originalCommand)
        {
            // Make sure we are able to run git, even if git is not in the path
            if (originalCommand.Equals("git", StringComparison.CurrentCultureIgnoreCase) ||
                originalCommand.Equals("{git}", StringComparison.CurrentCultureIgnoreCase))
            {
                return AppSettings.GitCommand;
            }

            if (originalCommand.Equals("gitextensions", StringComparison.CurrentCultureIgnoreCase) ||
                originalCommand.Equals("{gitextensions}", StringComparison.CurrentCultureIgnoreCase) ||
                originalCommand.Equals("gitex", StringComparison.CurrentCultureIgnoreCase) ||
                originalCommand.Equals("{gitex}", StringComparison.CurrentCultureIgnoreCase))
            {
                return AppSettings.GetGitExtensionsFullPath();
            }

            if (originalCommand.Equals("{openurl}", StringComparison.CurrentCultureIgnoreCase))
            {
                return "explorer";
            }

            // Prefix should be {plugin:pluginname},{plugin=pluginname}
            var match = System.Text.RegularExpressions.Regex.Match(originalCommand, @"\{plugin.(.+)\}", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success && match.Groups.Count > 1)
            {
                originalCommand = $"{PluginPrefix}{match.Groups[1].Value.ToLower()}";
            }

            return originalCommand;
        }
    }
}