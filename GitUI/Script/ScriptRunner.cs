using System;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils;
using GitUI.HelperDialogs;
using GitUI.NBugReports;
using GitUIPluginInterfaces;
using Microsoft;

namespace GitUI.Script
{
    /// <summary>Runs scripts.</summary>
    public static class ScriptRunner
    {
        /// <summary>Tries to run scripts identified by a <paramref name="command"/>.</summary>
        public static CommandStatus ExecuteScriptCommand(IWin32Window owner, GitModule module, int command, IGitUICommands uiCommands, RevisionGridControl? revisionGrid)
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

        public static CommandStatus RunScript(IWin32Window owner, IGitModule module, string? scriptKey, IGitUICommands uiCommands, RevisionGridControl? revisionGrid)
        {
            try
            {
                return RunScriptInternal(owner, module, scriptKey, uiCommands, revisionGrid);
            }
            catch (ExternalOperationException ex) when (ex is not UserExternalOperationException)
            {
                ThreadHelper.AssertOnUIThread();
                throw new UserExternalOperationException($"{TranslatedStrings.ScriptErrorFailedToExecute}: '{scriptKey}'", ex);
            }
        }

        private static CommandStatus RunScriptInternal(IWin32Window owner, IGitModule module, string? scriptKey, IGitUICommands uiCommands, RevisionGridControl? revisionGrid)
        {
            if (string.IsNullOrEmpty(scriptKey))
            {
                return false;
            }

            ScriptInfo? scriptInfo = ScriptManager.GetScript(scriptKey);
            if (scriptInfo is null)
            {
                ThreadHelper.AssertOnUIThread();
                throw new UserExternalOperationException($"{TranslatedStrings.ScriptErrorCantFind}: '{scriptKey}'",
                    new ExternalOperationException(command: null, arguments: null, module.WorkingDir, innerException: null));
            }

            if (string.IsNullOrEmpty(scriptInfo.Command))
            {
                return false;
            }

            string? arguments = scriptInfo.Arguments;
            if (!string.IsNullOrEmpty(arguments) && revisionGrid is null)
            {
                string? optionDependingOnSelectedRevision
                    = ScriptOptionsParser.Options.FirstOrDefault(option => ScriptOptionsParser.DependsOnSelectedRevision(option)
                                                                        && ScriptOptionsParser.Contains(arguments, option));
                if (optionDependingOnSelectedRevision is not null)
                {
                    ThreadHelper.AssertOnUIThread();
                    throw new UserExternalOperationException($"{TranslatedStrings.ScriptText}: '{scriptKey}'{Environment.NewLine}'{optionDependingOnSelectedRevision}' {TranslatedStrings.ScriptErrorOptionWithoutRevisionGridText}",
                        new ExternalOperationException(scriptInfo.Command, arguments, module.WorkingDir, innerException: null));
                }
            }

            if (scriptInfo.AskConfirmation &&
                MessageBox.Show(owner, $"{TranslatedStrings.ScriptConfirmExecute}: '{scriptInfo.Name}'?", TranslatedStrings.ScriptText,
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return false;
            }

            string? originalCommand = scriptInfo.Command;
            (string? argument, bool abort) = ScriptOptionsParser.Parse(scriptInfo.Arguments, module, owner, revisionGrid);
            if (abort)
            {
                ThreadHelper.AssertOnUIThread();
                throw new UserExternalOperationException($"{TranslatedStrings.ScriptText}: '{scriptKey}'{Environment.NewLine}{TranslatedStrings.ScriptErrorOptionWithoutRevisionText}",
                    new ExternalOperationException(scriptInfo.Command, arguments, module.WorkingDir, innerException: null));
            }

            Validates.NotNull(argument);

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
                command = command.Replace(PluginPrefix, string.Empty);

                lock (PluginRegistry.Plugins)
                {
                    foreach (var plugin in PluginRegistry.Plugins)
                    {
                        if (string.Equals(plugin.Name, command, StringComparison.CurrentCultureIgnoreCase))
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
                if (revisionGrid is null)
                {
                    return false;
                }

                command = command.Replace(NavigateToPrefix, string.Empty);
                if (!string.IsNullOrEmpty(command))
                {
                    var revisionRef = new Executable(command, module.WorkingDir).GetOutputLines(argument).FirstOrDefault();

                    if (revisionRef is not null)
                    {
                        revisionGrid.GoToRef(revisionRef, true);
                    }
                }

                return new CommandStatus(executed: true, needsGridRefresh: false);
            }

            if (!scriptInfo.RunInBackground)
            {
                bool success = FormProcess.ShowDialog(owner, command, argument, module.WorkingDir, null, true);
                if (!success)
                {
                    return false;
                }
            }
            else
            {
                if (originalCommand.Equals("{openurl}", StringComparison.CurrentCultureIgnoreCase))
                {
                    OsShellUtil.OpenUrlInDefaultBrowser(argument);
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
