using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.Script
{
    /// <summary>Runs scripts.</summary>
    public static class ScriptRunner
    {
        /// <summary>Tries to run scripts identified by a <paramref name="command"/></summary>
        public static CommandStatus ExecuteScriptCommand(IWin32Window owner, GitModule module, int command, IGitUICommands uiCommands, RevisionGridControl revisionGrid)
        {
            var anyScriptExecuted = false;
            var needsGridRefresh = false;

            foreach (var script in ScriptManager.GetScripts())
            {
                if (script.HotkeyCommandIdentifier == command)
                {
                    var result = RunScript(owner, module, script.Name, uiCommands, revisionGrid);
                    anyScriptExecuted = true;
                    needsGridRefresh |= result.NeedsGridRefresh;
                }
            }

            return new CommandStatus(anyScriptExecuted, needsGridRefresh);
        }

        public static CommandStatus RunScript(IWin32Window owner, IGitModule module, string scriptKey, IGitUICommands uiCommands, RevisionGridControl revisionGrid)
        {
            return RunScript(owner, module, scriptKey, uiCommands, revisionGrid,
                msg => MessageBox.Show(owner, msg, Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error));
        }

        public static CommandStatus RunScript(IWin32Window owner, IGitModule module, string scriptKey, IGitUICommands uiCommands,
            RevisionGridControl revisionGrid, Action<string> showError)
        {
            if (string.IsNullOrEmpty(scriptKey))
            {
                return false;
            }

            var script = ScriptManager.GetScript(scriptKey);

            if (script == null)
            {
                showError("Cannot find script: " + scriptKey);
                return false;
            }

            if (string.IsNullOrEmpty(script.Command))
            {
                return false;
            }

            string arguments = script.Arguments;
            if (!string.IsNullOrEmpty(arguments) && revisionGrid == null)
            {
                string optionDependingOnSelectedRevision
                    = ScriptOptionsParser.Options.FirstOrDefault(option => ScriptOptionsParser.DependsOnSelectedRevision(option)
                        && ScriptOptionsParser.Contains(arguments, option));
                if (optionDependingOnSelectedRevision != null)
                {
                    showError($"Option {optionDependingOnSelectedRevision} is only supported when started with revision grid available.");
                    return false;
                }
            }

            return RunScript(owner, module, script, uiCommands, revisionGrid, showError);
        }

        private static CommandStatus RunScript(IWin32Window owner, IGitModule module, ScriptInfo scriptInfo, IGitUICommands uiCommands,
            RevisionGridControl revisionGrid, Action<string> showError)
        {
            if (scriptInfo.AskConfirmation && MessageBox.Show(owner, $"Do you want to execute '{scriptInfo.Name}'?", "Script", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return false;
            }

            string originalCommand = scriptInfo.Command;
            (string argument, bool abort) = ScriptOptionsParser.Parse(scriptInfo.Arguments, module, owner, revisionGrid);
            if (abort)
            {
                showError($"There must be a revision in order to substitute the argument option(s) for the script to run.");
                return false;
            }

            string command = OverrideCommandWhenNecessary(originalCommand);
            command = ExpandCommandVariables(command, module);

            if (scriptInfo.IsPowerShell)
            {
                PowerShellHelper.RunPowerShell(command, argument, module.WorkingDir, scriptInfo.RunInBackground);

                // 'RunPowerShell' always runs the script detached (yet).
                // Hence currently, it does not make sense to set 'needsGridRefresh' to '!scriptInfo.RunInBackground'.
                return new CommandStatus(executed: true, needsGridRefresh: false);
            }

            if (command.StartsWith(PluginPrefix))
            {
                command = command.Replace(PluginPrefix, "");

                lock (PluginRegistry.Plugins)
                {
                    foreach (var plugin in PluginRegistry.Plugins)
                    {
                        if (plugin.Description.ToLower().Equals(command, StringComparison.CurrentCultureIgnoreCase))
                        {
                            var eventArgs = new GitUIEventArgs(owner, uiCommands);
                            return new CommandStatus(executed: true, needsGridRefresh: plugin.Execute(eventArgs));
                        }
                    }
                }

                return false;
            }

            if (command.StartsWith(NavigateToPrefix))
            {
                if (revisionGrid == null)
                {
                    return false;
                }

                command = command.Replace(NavigateToPrefix, string.Empty);
                if (!string.IsNullOrEmpty(command))
                {
                    var revisionRef = new Executable(command, module.WorkingDir).GetOutputLines(argument).FirstOrDefault();

                    if (revisionRef != null)
                    {
                        revisionGrid.GoToRef(revisionRef, true);
                    }
                }

                return new CommandStatus(executed: true, needsGridRefresh: false);
            }

            if (!scriptInfo.RunInBackground)
            {
                FormProcess.ShowStandardProcessDialog(owner, command, argument, module.WorkingDir, null, true);
            }
            else
            {
                if (originalCommand.Equals("{openurl}", StringComparison.CurrentCultureIgnoreCase))
                {
                    Process.Start(argument);
                }
                else
                {
                    // It is totally valid to have a command without an argument, e.g.:
                    //    Command  : myscript.cmd
                    //    Arguments: <blank>
                    new Executable(command, module.WorkingDir).Start(argument ?? string.Empty);
                }
            }

            return new CommandStatus(executed: true, needsGridRefresh: !scriptInfo.RunInBackground);
        }

        private static string ExpandCommandVariables(string originalCommand, IGitModule module)
        {
            return originalCommand.Replace("{WorkingDir}", module.WorkingDir);
        }

        private const string PluginPrefix = "plugin:";
        private const string NavigateToPrefix = "navigateTo:";

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
