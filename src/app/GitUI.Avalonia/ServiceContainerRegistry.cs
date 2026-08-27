using System.ComponentModel.Design;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using GitUI.Compat;
using GitUI.ConsoleEmulation;
using GitUI.ConsoleEmulation.PlainText;
using GitUI.Hotkey;
using GitUI.Models;
using GitUI.ScriptsEngine;
using GitUI.Shells;
using ResourceManager;

namespace GitUI;

// ConEmu and Mintty are Windows-only, so Avalonia uses plain-text console emulation.
public static class ServiceContainerRegistry
{
    public static void RegisterServices(ServiceContainer serviceContainer)
    {
        ScriptsManager scriptsManager = new();
        OutputHistoryModel outputHistoryModel = new(AppSettings.OutputHistoryDepth.Value);
        serviceContainer.GetRequiredService<ISubscribableTraceListener>().TraceReceived += (in string message) =>
        {
            // In release builds, all Trace.Write* output is recorded.
            // In debug builds, forward only exceptions and DebugHelper.Trace messages but not all the noisy Debug.Write* output.
#if DEBUG
            const char noBreakSpace = '\u00a0';
            if (message.Contains("Exception") || message.Contains($":{noBreakSpace}"))
#endif
            {
                outputHistoryModel.RecordHistory(message);
            }
        };

        serviceContainer.AddService<IConsoleEmulatorsRegistry>(PlainTextConsoleEmulatorsRegistry.Instance);
        serviceContainer.AddService<IScriptsManager>(scriptsManager);
        serviceContainer.AddService<IScriptsRunner>(scriptsManager);
        serviceContainer.AddService<ISimplePromptCreator>(new SimplePromptCreator());
        serviceContainer.AddService<IFilePromptCreator>(new FilePromptCreator());
        HotkeySettingsManager hotkeySettingsManager = new(scriptsManager);
        serviceContainer.AddService<IHotkeySettingsLoader>(hotkeySettingsManager);
        serviceContainer.AddService<IHotkeySettingsManager>(hotkeySettingsManager);
        serviceContainer.AddService<IOutputHistoryProvider>(outputHistoryModel);
        serviceContainer.AddService<IOutputHistoryRecorder>(outputHistoryModel);
        serviceContainer.AddService<ITerminalLauncher>(new TerminalLauncher());

        IRepositoryCurrentBranchNameCache branchNameCache = new RepositoryCurrentBranchNameCache(
            new RepositoryCurrentBranchNameProvider(serviceContainer.GetRequiredService<IGitExecutorProvider>()));
        IInvalidRepositoryRemover invalidRepositoryRemover = new InvalidRepositoryRemover();
        serviceContainer.AddService<IRepositoryCurrentBranchNameCache>(branchNameCache);
        serviceContainer.AddService<IInvalidRepositoryRemover>(invalidRepositoryRemover);
        serviceContainer.AddService<IUserRepositoriesListController>(
            new UserRepositoriesListController(RepositoryHistoryManager.Locals, invalidRepositoryRemover, branchNameCache));
        serviceContainer.AddService<IRepositoryHistoryUIService>(
            new RepositoryHistoryUIService(branchNameCache, invalidRepositoryRemover));

        serviceContainer.AddService<IShellProvider>(new ShellProvider());
    }
}
