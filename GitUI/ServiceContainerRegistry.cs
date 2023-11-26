﻿using System.ComponentModel.Design;
using GitCommands.UserRepositoryHistory;
using GitUI.Hotkey;
using GitUI.ScriptsEngine;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI;

public static class ServiceContainerRegistry
{
    public static void RegisterServices(ServiceContainer serviceContainer)
    {
        ScriptsManager scriptsManager = new();
        HotkeySettingsManager hotkeySettingsManager = new(scriptsManager);

        serviceContainer.AddService<IWindowsJumpListManager>(new WindowsJumpListManager(serviceContainer.GetRequiredService<IRepositoryDescriptionProvider>()));
        serviceContainer.AddService<IScriptsManager>(scriptsManager);
        serviceContainer.AddService<IScriptsRunner>(scriptsManager);
        serviceContainer.AddService<IHotkeySettingsManager>(hotkeySettingsManager);
        serviceContainer.AddService<IHotkeySettingsLoader>(hotkeySettingsManager);
        serviceContainer.AddService<ISimplePromptCreator>(new SimplePromptCreator());
        serviceContainer.AddService<IFilePromptCreator>(new FilePromptCreator());
    }
}
