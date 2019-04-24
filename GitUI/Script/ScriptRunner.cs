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
    }

    internal sealed class ScriptRunner : IScriptRunner
    {
        private const string PluginPrefix = "plugin:";
        private const string NavigateToPrefix = "navigateTo:";

        private readonly IGitModule _module;
        private readonly GitUIEventArgs _gitUIEventArgs;
        private readonly IScriptOptionsParser _scriptOptionsParser;
        private readonly ISimpleDialog _simpleDialog;
        private readonly IScriptManager _scriptManager;
        private readonly ICanGoToRef _canGoToRef;

        public ScriptRunner(
            IGitModule module,
            GitUIEventArgs gitUIEventArgs,
            IScriptOptionsParser scriptOptionsParser,
            ISimpleDialog simpleDialog,
            IScriptManager scriptManager,
            ICanGoToRef canGoToRef = null)
        {
            _module = module ?? throw new ArgumentNullException(nameof(module));
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
                    var result = RunScript(script.Name);

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

            if (string.IsNullOrEmpty(script.Command))
            {
                return false;
            }

            var argument = script.Arguments;

            foreach (var option in ScriptOptions.All)
            {
                if (string.IsNullOrEmpty(argument) || !argument.Contains(option))
                {
                    continue;
                }

                if (!option.StartsWith("s"))
                {
                    continue;
                }

                if (_canGoToRef != null)
                {
                    continue;
                }

                _simpleDialog.ShowOkDialog($"Option {option} is only supported when started from revision grid.", "Error", MessageBoxIcon.Error);

                return false;
            }

            return RunScript(_module, script);
        }

        private CommandStatus RunScript(IGitModule module, ScriptInfo scriptInfo)
        {
            if (scriptInfo.AskConfirmation)
            {
                var dialogResult = _simpleDialog
                    .ShowYesNoDialog($"Do you want to execute '{scriptInfo.Name}'?", "Script", MessageBoxIcon.Question);

                if (dialogResult == DialogResult.No)
                {
                    return false;
                }
            }

            var originalCommand = scriptInfo.Command;
            (string argument, bool abort) = _scriptOptionsParser.Parse(scriptInfo.Arguments, module);

            if (abort)
            {
                return false;
            }

            var command = OverrideCommandWhenNecessary(originalCommand);

            command = ExpandCommandVariables(command, module);

            if (scriptInfo.IsPowerShell)
            {
                PowerShellHelper.RunPowerShell(command, argument, module.WorkingDir, scriptInfo.RunInBackground);
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
                    var revisionRef = new Executable(command, module.WorkingDir).GetOutputLines(argument).FirstOrDefault();

                    if (revisionRef != null)
                    {
                        _canGoToRef.GoToRef(revisionRef, true);
                    }
                }

                return new CommandStatus(true, false);
            }

            if (!scriptInfo.RunInBackground)
            {
                _simpleDialog.ShowStandardProcessDialog(command, argument, module.WorkingDir, null, true);
            }
            else
            {
                if (originalCommand.Equals("{openurl}", StringComparison.CurrentCultureIgnoreCase))
                {
                    Process.Start(argument);
                }
                else
                {
                    new Executable(command, module.WorkingDir).Start(argument);
                }
            }

            return new CommandStatus(true, !scriptInfo.RunInBackground);
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