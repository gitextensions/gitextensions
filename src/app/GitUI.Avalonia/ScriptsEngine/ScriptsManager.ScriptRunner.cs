using System.Text.RegularExpressions;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.HelperDialogs;

namespace GitUI.ScriptsEngine;

partial class ScriptsManager
{
    /// <summary>Runs scripts.</summary>
    internal static partial class ScriptRunner
    {
        private const string PluginPrefix = "plugin:";
        private const string NavigateToPrefix = "navigateTo:";
        private const string userInput = "UserInput";
        private const string userFiles = "UserFiles";

        [GeneratedRegex(@"\{UserInput:(?<label>[^}=]+)(=(?<defaultValue>[^{}]*(({[^{}]+})+[^{}]*)*))?\}", RegexOptions.ExplicitCapture)]
        private static partial Regex UserInputRegex { get; }

        [GeneratedRegex(@"\{plugin.(?<name>.+)\}", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture)]
        private static partial Regex PluginRegex { get; }

        public static bool RunScript(ScriptInfo script, IWin32Window owner, IGitUICommands commands, IScriptOptionsProvider scriptOptionsProvider)
        {
            try
            {
                return RunScriptInternal(script, owner, commands, scriptOptionsProvider);
            }
            catch (ExternalOperationException ex) when (ex is not UserExternalOperationException)
            {
                throw new UserExternalOperationException($"{TranslatedStrings.ScriptErrorFailedToExecute}: '{script.GetDisplayName()}'", ex);
            }
        }

        internal static (string? arguments, bool abort, bool cancel) ParseUserInputs(
            string scriptName,
            string? arguments,
            IGitUICommands uiCommands,
            IWin32Window owner,
            IScriptOptionsProvider scriptOptionsProvider)
        {
            if (arguments is null)
            {
                return (arguments: null, abort: false, cancel: false);
            }

            string userInputCaption = string.Format(TranslatedStrings.ScriptUserInputCaption, scriptName);
            Match match;
            while ((match = UserInputRegex.Match(arguments)).Success)
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
                using IUserInputPrompt prompt = uiCommands.GetRequiredService<ISimplePromptCreator>()
                    .Create(userInputCaption, label, defaultValue.arguments);
                DialogResult result = prompt.ShowDialog(owner);
                if (result != DialogResult.OK)
                {
                    return (arguments: null, abort: false, cancel: true);
                }

                arguments = ScriptOptionsParser.ReplaceOption($"UserInput:{label}", arguments, [prompt.UserInput]);
                arguments = ScriptOptionsParser.ReplaceOption(match.Value[1..^1], arguments, [prompt.UserInput]);
            }

            if (ScriptOptionsParser.Contains(arguments, userInput))
            {
                using IUserInputPrompt prompt = uiCommands.GetRequiredService<ISimplePromptCreator>()
                    .Create(userInputCaption, label: null, defaultValue: string.Empty);
                DialogResult result = prompt.ShowDialog(owner);
                if (result == DialogResult.Cancel)
                {
                    return (arguments: null, abort: false, cancel: true);
                }

                arguments = ScriptOptionsParser.ReplaceOption(userInput, arguments, [prompt.UserInput]);
            }

            if (ScriptOptionsParser.Contains(arguments, userFiles))
            {
                using IUserInputPrompt prompt = uiCommands.GetRequiredService<IFilePromptCreator>().Create();
                if (prompt.ShowDialog(owner) != DialogResult.OK)
                {
                    return (arguments: null, abort: false, cancel: true);
                }

                arguments = ScriptOptionsParser.ReplaceOption(userFiles, arguments, [prompt.UserInput]);
            }

            return (arguments, abort: false, cancel: false);
        }

        private static bool RunScriptInternal(
            ScriptInfo script,
            IWin32Window owner,
            IGitUICommands uiCommands,
            IScriptOptionsProvider scriptOptionsProvider)
        {
            if (string.IsNullOrEmpty(script.Command))
            {
                return false;
            }

            string? arguments = script.Arguments;
            if (!string.IsNullOrEmpty(arguments) && uiCommands.BrowseRepo is null)
            {
                string? selectedRevisionOption = ScriptOptionsParser.Options.FirstOrDefault(
                    option => ScriptOptionsParser.DependsOnSelectedRevision(option)
                              && ScriptOptionsParser.Contains(arguments, option));
                if (selectedRevisionOption is not null)
                {
                    throw new UserExternalOperationException(
                        $"{TranslatedStrings.ScriptText}: '{script.GetDisplayName()}'{Environment.NewLine}'{selectedRevisionOption}' {TranslatedStrings.ScriptErrorOptionWithoutRevisionGridText}",
                        new ExternalOperationException(script.Command, arguments, uiCommands.Module.WorkingDir));
                }
            }

            if (script.AskConfirmation
                && MessageBoxes.Show(
                    owner,
                    $"{TranslatedStrings.ScriptConfirmExecute}: '{script.GetDisplayName()}'?",
                    TranslatedStrings.ScriptText,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.No)
            {
                return false;
            }

            string originalCommand = script.Command;
            (arguments, bool abort, bool cancelled) = ParseUserInputs(
                script.GetDisplayName() ?? "<_nameless_script_>",
                script.Arguments,
                uiCommands,
                owner,
                scriptOptionsProvider);

            if (cancelled)
            {
                MessageBoxes.Show(owner, TranslatedStrings.ScriptUserCanceledRun, script.GetDisplayName(), MessageBoxButtons.OK);
                return false;
            }

            (string? argument, abort) = abort
                ? (null, true)
                : ScriptOptionsParser.Parse(arguments, uiCommands, owner, scriptOptionsProvider);
            if (abort)
            {
                throw new UserExternalOperationException(
                    $"{TranslatedStrings.ScriptText}: '{script.GetDisplayName()}'{Environment.NewLine}{TranslatedStrings.ScriptErrorOptionWithoutRevisionText}",
                    new ExternalOperationException(script.Command, arguments, uiCommands.Module.WorkingDir));
            }

            if (originalCommand.Equals("{openurl}", StringComparison.CurrentCultureIgnoreCase))
            {
                OsShellUtil.OpenUrlInDefaultBrowser(argument);
                return true;
            }

            string command = ExpandCommandVariables(OverrideCommandWhenNecessary(originalCommand), uiCommands.Module);
            if (script.IsPowerShell)
            {
                PowerShellHelper.RunPowerShell(command, argument, uiCommands.Module.WorkingDir, script.RunInBackground);
                return true;
            }

            if (command.StartsWith(PluginPrefix))
            {
                // The WinForms runner also returns false when no matching plugin is loaded.
                // Avalonia has no PluginRegistry until its plugin discovery is enabled.
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
                    string? revisionRef = result.StandardOutput.Split('\n').FirstOrDefault();
                    if (revisionRef is not null)
                    {
                        uiCommands.BrowseRepo.GoToRef(revisionRef, true);
                    }
                }

                return true;
            }

            if (!script.RunInBackground)
            {
                bool success = FormProcess.ShowDialog(owner, uiCommands, argument, uiCommands.Module.WorkingDir, null, true, process: command);
                if (!success)
                {
                    return false;
                }

                uiCommands.RepoChangedNotifier.Notify();
            }
            else
            {
                new Executable(command, uiCommands.Module.WorkingDir).Start(argument ?? string.Empty);
            }

            return true;
        }

        private static string ExpandCommandVariables(string originalCommand, IGitModule module)
            => originalCommand.Replace("{WorkingDir}", module.WorkingDir);

        private static string OverrideCommandWhenNecessary(string originalCommand)
        {
            if (originalCommand.Equals("git", StringComparison.CurrentCultureIgnoreCase)
                || originalCommand.Equals("{git}", StringComparison.CurrentCultureIgnoreCase))
            {
                return AppSettings.GitCommand;
            }

            if (originalCommand.Equals("gitextensions", StringComparison.CurrentCultureIgnoreCase)
                || originalCommand.Equals("{gitextensions}", StringComparison.CurrentCultureIgnoreCase)
                || originalCommand.Equals("gitex", StringComparison.CurrentCultureIgnoreCase)
                || originalCommand.Equals("{gitex}", StringComparison.CurrentCultureIgnoreCase))
            {
                return Environment.ProcessPath
                    ?? throw new ExternalOperationException(originalCommand, string.Empty, AppContext.BaseDirectory);
            }

            Match match = PluginRegex.Match(originalCommand);
            return match.Success && match.Groups.Count > 1
                ? $"{PluginPrefix}{match.Groups["name"].Value.ToLower()}"
                : originalCommand;
        }
    }
}
