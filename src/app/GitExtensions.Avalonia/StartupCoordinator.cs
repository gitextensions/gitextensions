using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitUI;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;

namespace GitExtensions;

internal sealed class StartupCoordinator
{
    private readonly Func<bool> _solveGitCommand;
    private readonly Func<CommonLogic, bool> _autoSolve;
    private readonly Func<CommonLogic, bool> _checkSettings;
    private readonly Func<IGitUICommands, SettingsPageReference, bool> _showSettings;

    internal StartupCoordinator()
        : this(
            () => CheckSettingsLogic.SolveGitCommand(),
            commonLogic => new CheckSettingsLogic(commonLogic).AutoSolveAllSettings(),
            CheckSettings,
            (commands, page) => commands.StartSettingsDialog(owner: null, page))
    {
    }

    internal StartupCoordinator(
        Func<bool> solveGitCommand,
        Func<CommonLogic, bool> autoSolve,
        Func<CommonLogic, bool> checkSettings,
        Func<IGitUICommands, SettingsPageReference, bool> showSettings)
    {
        _solveGitCommand = solveGitCommand;
        _autoSolve = autoSolve;
        _checkSettings = checkSettings;
        _showSettings = showSettings;
    }

    internal bool EnsurePrerequisites(IGitUICommands commands, IReadOnlyList<string> args)
    {
        if (args.Count >= 2 && args[1] == "uninstall")
        {
            return true;
        }

        if (!_solveGitCommand())
        {
            _showSettings(commands, GitSettingsPage.GetPageReference());
            if (!_solveGitCommand())
            {
                return false;
            }
        }

        CommonLogic commonLogic = new(commands.Module);
        if (!AppSettings.CheckSettings)
        {
            CheckSettingsLogic.SolveEditor(commonLogic);
            return true;
        }

        if (_checkSettings(commonLogic))
        {
            return true;
        }

        if (_autoSolve(commonLogic) && _checkSettings(commonLogic))
        {
            return true;
        }

        _showSettings(commands, ChecklistSettingsPage.GetPageReference());
        return _solveGitCommand();
    }

    private static bool CheckSettings(CommonLogic commonLogic)
    {
        CheckSettingsLogic checkSettingsLogic = new(commonLogic);
        SettingsPageHostMock pageHost = new(checkSettingsLogic);
        ChecklistSettingsPage page = SettingsPageBase.Create<ChecklistSettingsPage>(
            pageHost,
            Program.ServiceContainer);
        return page.CheckSettings();
    }
}
