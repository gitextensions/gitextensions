using System.ComponentModel.Design;
using System.IO.Abstractions;
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
            // In debug builds, forward only exceptions but not all the noisy Debug.Write* output.
#if DEBUG
            if (message.Contains("Exception"))
#endif
            {
                outputHistoryModel.RecordHistory(message);
            }
        };

        if (serviceContainer.GetService(typeof(IFileSystem)) == null)
        {
            serviceContainer.AddService<IFileSystem>(new FileSystem());
        }

        serviceContainer.AddService<IWindowsJumpListManager>(new WindowsJumpListManager(serviceContainer.GetRequiredService<IRepositoryDescriptionProvider>()));
        serviceContainer.AddService<IScriptsManager>(scriptsManager);
        serviceContainer.AddService<IScriptsRunner>(scriptsManager);
        serviceContainer.AddService<IHotkeySettingsManager>(hotkeySettingsManager);
        serviceContainer.AddService<IHotkeySettingsLoader>(hotkeySettingsManager);
        serviceContainer.AddService<ISimplePromptCreator>(new SimplePromptCreator());
        serviceContainer.AddService<IFilePromptCreator>(new FilePromptCreator());
        serviceContainer.AddService<IOutputHistoryProvider>(outputHistoryModel);
        serviceContainer.AddService<IOutputHistoryRecorder>(outputHistoryModel);

        RepositoryCurrentBranchNameProvider repositoryCurrentBranchNameProvider = new();
        InvalidRepositoryRemover invalidRepositoryRemover = new();
        serviceContainer.AddService<IRepositoryCurrentBranchNameProvider>(repositoryCurrentBranchNameProvider);
        serviceContainer.AddService<IInvalidRepositoryRemover>(invalidRepositoryRemover);
        serviceContainer.AddService<IRepositoryHistoryUIService>(new RepositoryHistoryUIService(repositoryCurrentBranchNameProvider, invalidRepositoryRemover));
    }
}
