using System.ComponentModel.Design;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtUtils;
using GitUI.CommandsDialogs;
using GitUI.Hotkey;
using GitUI.Models;
using GitUI.ScriptsEngine;
using ResourceManager;

namespace GitUI;

public static class ServiceContainerRegistry
{
    public static void RegisterServices(ServiceContainer serviceContainer)
    {
        ScriptsManager scriptsManager = new();
        HotkeySettingsManager hotkeySettingsManager = new(scriptsManager);

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

        serviceContainer.AddService<IWindowsJumpListManager>(new WindowsJumpListManager(serviceContainer.GetRequiredService<IRepositoryDescriptionProvider>()));
        serviceContainer.AddService<IScriptsManager>(scriptsManager);
        serviceContainer.AddService<IScriptsRunner>(scriptsManager);
        serviceContainer.AddService<IHotkeySettingsManager>(hotkeySettingsManager);
        serviceContainer.AddService<IHotkeySettingsLoader>(hotkeySettingsManager);
        serviceContainer.AddService<ISimplePromptCreator>(new SimplePromptCreator());
        serviceContainer.AddService<IFilePromptCreator>(new FilePromptCreator());
        serviceContainer.AddService<IOutputHistoryProvider>(outputHistoryModel);
        serviceContainer.AddService<IOutputHistoryRecorder>(outputHistoryModel);

        RepositoryCurrentBranchNameProvider repositoryCurrentBranchNameProvider = new((path) => new GitModule(path));
        InvalidRepositoryRemover invalidRepositoryRemover = new();
        serviceContainer.AddService<IRepositoryCurrentBranchNameProvider>(repositoryCurrentBranchNameProvider);
        serviceContainer.AddService<IInvalidRepositoryRemover>(invalidRepositoryRemover);
        serviceContainer.AddService<IRepositoryHistoryUIService>(new RepositoryHistoryUIService(repositoryCurrentBranchNameProvider, invalidRepositoryRemover));
    }
}
