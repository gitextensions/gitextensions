using System.Text.RegularExpressions;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitUI.HelperDialogs;
using GitUI.NBugReports;
using GitUIPluginInterfaces;

namespace GitUI.ScriptsEngine
{
    partial class ScriptsManager
    {
        /// <summary>Runs scripts.</summary>
        internal static partial class ScriptRunner
        {
            private const string PluginPrefix = "plugin:";
            private const string NavigateToPrefix = "navigateTo:";
            private const string userInput = "UserInput";
            private const string userFiles = "UserFiles";

            // Regex that ensure that in the default value, there is the same number of '{' than '}' to find the right end of the default value expression.
            [GeneratedRegex(@"\{UserInput:(?<label>[^}=]+)(=(?<defaultValue>[^{}]*(({[^{}]+})+[^{}]*)*))?\}", RegexOptions.ExplicitCapture)]
            private static partial Regex UserInputRegex();
            [GeneratedRegex(@"\{plugin.(?<name>.+)\}", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture)]
            private static partial Regex PluginRegex();

            public static bool RunScript(ScriptInfo script, IWin32Window owner, IGitUICommands commands, IScriptOptionsProvider? scriptOptionsProvider = null)
            {
                try
                {
                    return RunScriptInternal(script, owner, commands, scriptOptionsProvider);
                }
                catch (ExternalOperationException ex) when (ex is not UserExternalOperationException)
                {
                    throw new UserExternalOperationException($"{TranslatedStrings.ScriptErrorFailedToExecute}: '{script.Name}'", ex);
                }
            }

            internal static (string? arguments, bool abort, bool cancel) ParseUserInputs(string scriptName, string? arguments, IGitUICommands uiCommands, IWin32Window owner, IScriptOptionsProvider? scriptOptionsProvider = null)
            {
                string userInputCaption = string.Format(TranslatedStrings.ScriptUserInputCaption, scriptName);

                // Specific handling of "UserInput" because the value entered should replace only "UserInput" with same label
                Match match;
                while ((match = UserInputRegex().Match(arguments)).Success)
                {
                    Group defaultValueMatch = match.Groups["defaultValue"];
                    (string? arguments, bool abort) defaultValue = defaultValueMatch is null
                        ? (string.Empty, false)
                        : ScriptOptionsParser.Parse(defaultValueMatch.Value ?? string.Empty, uiCommands, owner, scriptOptionsProvider);

                    if (defaultValue.abort)
                    {
                        return (arguments: null, abort: true, cancel: false);
                    }

                    string label = match.Groups["label"].Value;

                    using (IUserInputPrompt prompt = uiCommands.GetService<ISimplePromptCreator>().Create(userInputCaption, label, defaultValue.arguments))
                    {
                        DialogResult result = prompt.ShowDialog(owner);
                        if (result != DialogResult.OK)
                        {
                            return (arguments: null, abort: false, cancel: true);
                        }

                        arguments = ScriptOptionsParser.ReplaceOption($"UserInput:{label}", arguments, [prompt.UserInput]);
                        arguments = ScriptOptionsParser.ReplaceOption(match.Value.Substring(1, match.Value.Length - 2), arguments, [prompt.UserInput]);
                    }
                }

                if (ScriptOptionsParser.Contains(arguments, userInput))
                {
                    userInputCaption = string.Format(TranslatedStrings.ScriptUserInputCaption, scriptName);
                    using (IUserInputPrompt prompt = uiCommands.GetService<ISimplePromptCreator>().Create(userInputCaption, label: null, defaultValue: string.Empty))
                    {
                        DialogResult result = prompt.ShowDialog(owner);
                        if (result == DialogResult.Cancel)
                        {
                            return (arguments: null, abort: false, cancel: true);
                        }

                        arguments = ScriptOptionsParser.ReplaceOption(userInput, arguments, [prompt.UserInput]);
                    }
                }

                if (ScriptOptionsParser.Contains(arguments, userFiles))
                {
                    using (IUserInputPrompt prompt = uiCommands.GetService<IFilePromptCreator>().Create())
                    {
                        if (prompt.ShowDialog(owner) != DialogResult.OK)
                        {
                            return (arguments: null, abort: false, cancel: true);
                        }

                        arguments = ScriptOptionsParser.ReplaceOption(userFiles, arguments, [prompt.UserInput]);
                    }
                }

                return (arguments, abort: false, cancel: false);
            }

            private static bool RunScriptInternal(ScriptInfo script, IWin32Window owner, IGitUICommands uiCommands, IScriptOptionsProvider? scriptOptionsProvider)
            {
                if (string.IsNullOrEmpty(script.Command))
                {
                    return false;
                }

                string? arguments = script.Arguments;
                if (!string.IsNullOrEmpty(arguments) && uiCommands.BrowseRepo is null)
                {
                    string? optionDependingOnSelectedRevision
                        = ScriptOptionsParser.Options.FirstOrDefault(option => ScriptOptionsParser.DependsOnSelectedRevision(option)
                                                                            && ScriptOptionsParser.Contains(arguments, option));
                    if (optionDependingOnSelectedRevision is not null)
                    {
                        throw new UserExternalOperationException($"{TranslatedStrings.ScriptText}: '{script.Name}'{Environment.NewLine}'{optionDependingOnSelectedRevision}' {TranslatedStrings.ScriptErrorOptionWithoutRevisionGridText}",
                            new ExternalOperationException(script.Command, arguments, uiCommands.Module.WorkingDir));
                    }
                }

                if (script.AskConfirmation &&
                    MessageBox.Show(owner, $"{TranslatedStrings.ScriptConfirmExecute}: '{script.Name}'?", TranslatedStrings.ScriptText,
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return false;
                }

                string? originalCommand = script.Command;

                (arguments, bool abort, bool cancelled) = ParseUserInputs(script.Name, script.Arguments, uiCommands, owner, scriptOptionsProvider);

                if (cancelled)
                {
                    MessageBox.Show(owner, TranslatedStrings.ScriptUserCanceledRun, script.Name, MessageBoxButtons.OK);
                    return false;
                }

                (string? argument, abort) = abort
                    ? (null, true)
                    : ScriptOptionsParser.Parse(arguments, uiCommands, owner, scriptOptionsProvider);
                if (abort)
                {
                    throw new UserExternalOperationException($"{TranslatedStrings.ScriptText}: '{script.Name}'{Environment.NewLine}{TranslatedStrings.ScriptErrorOptionWithoutRevisionText}",
                        new ExternalOperationException(script.Command, arguments, uiCommands.Module.WorkingDir));
                }

                string command = OverrideCommandWhenNecessary(originalCommand);
                command = ExpandCommandVariables(command, uiCommands.Module);

                if (script.IsPowerShell)
                {
                    PowerShellHelper.RunPowerShell(command, argument, uiCommands.Module.WorkingDir, script.RunInBackground);

                    // 'RunPowerShell' always runs the script detached (yet).
                    // Hence currently, it does not make sense to trigger the 'RepoChangedNotifier' if '!scriptInfo.RunInBackground'.
                    return true;
                }

                if (command.StartsWith(PluginPrefix))
                {
                    command = command.Replace(PluginPrefix, string.Empty);

                    lock (PluginRegistry.Plugins)
                    {
                        foreach (IGitPlugin plugin in PluginRegistry.Plugins)
                        {
                            if (string.Equals(plugin.Name, command, StringComparison.CurrentCultureIgnoreCase))
                            {
                                GitUIEventArgs eventArgs = new(owner, uiCommands);
                                if (plugin.Execute(eventArgs))
                                {
                                    uiCommands.RepoChangedNotifier.Notify();
                                }

                                return true;
                            }
                        }
                    }

                    return false;
                }

                if (command.StartsWith(NavigateToPrefix))
                {
                    if (uiCommands.BrowseRepo is null)
                    {
                        return false;
                    }

                    command = command.Replace(NavigateToPrefix, string.Empty);
                    if (!string.IsNullOrEmpty(command))
                    {
                        ExecutionResult result = new Executable(command, uiCommands.Module.WorkingDir).Execute(argument);
                        string revisionRef = result.StandardOutput.Split('\n').FirstOrDefault();

                        if (revisionRef is not null)
                        {
                            uiCommands.BrowseRepo.GoToRef(revisionRef, true);
                        }
                    }

                    return true;
                }

                if (!script.RunInBackground)
                {
                    // TODO: Remove downcast from IGitUICommands to GitUICommands after https://github.com/gitextensions/gitextensions/pull/11269
                    bool success = FormProcess.ShowDialog(owner, uiCommands as GitUICommands, argument, uiCommands.Module.WorkingDir, null, true, process: command);
                    if (!success)
                    {
                        return false;
                    }

                    uiCommands.RepoChangedNotifier.Notify();
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
                        new Executable(command, uiCommands.Module.WorkingDir).Start(argument ?? string.Empty);
                    }
                }

                return true;
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
                Match match = PluginRegex().Match(originalCommand);
                if (match.Success && match.Groups.Count > 1)
                {
                    originalCommand = $"{PluginPrefix}{match.Groups["name"].Value.ToLower()}";
                }

                return originalCommand;
            }
        }
    }
}
