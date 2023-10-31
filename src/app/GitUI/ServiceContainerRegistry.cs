using System.ComponentModel.Design;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtUtils;
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
        serviceContainer.GetRequiredService<ISubscribableTraceListener>().TraceReceived += outputHistoryModel.Trace;

        serviceContainer.AddService<IWindowsJumpListManager>(new WindowsJumpListManager(serviceContainer.GetRequiredService<IRepositoryDescriptionProvider>()));
        serviceContainer.AddService<IScriptsManager>(scriptsManager);
        serviceContainer.AddService<IScriptsRunner>(scriptsManager);
        serviceContainer.AddService<IHotkeySettingsManager>(hotkeySettingsManager);
        serviceContainer.AddService<IHotkeySettingsLoader>(hotkeySettingsManager);
        serviceContainer.AddService<ISimplePromptCreator>(new SimplePromptCreator());
        serviceContainer.AddService<IFilePromptCreator>(new FilePromptCreator());
        serviceContainer.AddService<IOutputHistoryModel>(outputHistoryModel);
    }
}
