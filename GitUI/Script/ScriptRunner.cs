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
        /// <summary>Tries to run scripts identified by a <paramref name="command"/>
        /// and returns true if any executed.</summary>
        public static bool ExecuteScriptCommand(IWin32Window owner, GitModule module, int command, RevisionGridControl revisionGrid = null)
        {
            var anyScriptExecuted = false;

            foreach (var script in ScriptManager.GetScripts())
            {
                if (script.HotkeyCommandIdentifier == command)
                {
                    RunScript(owner, module, script.Name, revisionGrid);
                    anyScriptExecuted = true;
                }
            }

            return anyScriptExecuted;
        }

        public static bool RunScript(IWin32Window owner, GitModule module, string scriptKey, RevisionGridControl revisionGrid)
        {
            if (string.IsNullOrEmpty(scriptKey))
            {
                return false;
            }

            var script = ScriptManager.GetScript(scriptKey);

            if (script == null)
            {
                MessageBox.Show(owner, "Cannot find script: " + scriptKey, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrEmpty(script.Command))
            {
                return false;
            }

            string argument = script.Arguments;
            foreach (string option in ScriptOptionsParser.Options)
            {
                if (string.IsNullOrEmpty(argument) || !argument.Contains(option))
                {
                    continue;
                }

                if (!option.StartsWith("{s"))
                {
                    continue;
                }

                if (revisionGrid != null)
                {
                    continue;
                }

                MessageBox.Show(owner,
                    $"Option {option} is only supported when started from revision grid.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return RunScript(owner, module, script, revisionGrid);
        }

        private static bool RunScript(IWin32Window owner, GitModule module, ScriptInfo scriptInfo, RevisionGridControl revisionGrid)
        {
            if (scriptInfo.AskConfirmation && MessageBox.Show(owner, $"Do you want to execute '{scriptInfo.Name}'?", "Script", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return false;
            }

            string originalCommand = scriptInfo.Command;
            (string argument, bool abort) = ScriptOptionsParser.Parse(scriptInfo.Arguments, module, owner, revisionGrid);
            if (abort)
            {
                return false;
            }

            string command = OverrideCommandWhenNecessary(originalCommand);
            command = ExpandCommandVariables(command, module);

            if (scriptInfo.IsPowerShell)
            {
                PowerShellHelper.RunPowerShell(command, argument, module.WorkingDir, scriptInfo.RunInBackground);
                return false;
            }

            if (command.StartsWith(PluginPrefix))
            {
                command = command.Replace(PluginPrefix, "");
                foreach (var plugin in PluginRegistry.Plugins)
                {
                    if (plugin.Description.ToLower().Equals(command, StringComparison.CurrentCultureIgnoreCase))
                    {
                        var eventArgs = new GitUIEventArgs(owner, revisionGrid.UICommands);
                        return plugin.Execute(eventArgs);
                    }
                }

                return false;
            }

            if (command.StartsWith(NavigateToPrefix))
            {
                command = command.Replace(NavigateToPrefix, string.Empty);
                if (!command.IsNullOrEmpty())
                {
                    var revisionRef = new Executable(command, module.WorkingDir).GetOutputLines(argument).FirstOrDefault();

                    if (revisionRef != null)
                    {
                        revisionGrid.GoToRef(revisionRef, true);
                    }
                }

                return false;
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
                    new Executable(command, module.WorkingDir).Start(argument);
                }
            }

            return !scriptInfo.RunInBackground;
        }

        private static string ExpandCommandVariables(string originalCommand, GitModule module)
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