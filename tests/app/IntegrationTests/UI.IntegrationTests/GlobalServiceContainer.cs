using System.ComponentModel.Design;
using GitCommands;
using GitCommands.Submodules;
using GitUI;
using GitUI.Hotkey;
using GitUI.ScriptsEngine;
using GitUIPluginInterfaces;
using NSubstitute;
using ResourceManager;

namespace GitExtensions.UITests
{
    public static class GlobalServiceContainer
    {
        public static ServiceContainer CreateDefaultMockServiceContainer()
        {
            ServiceContainer serviceContainer = new();
            serviceContainer.AddService(Substitute.For<IAppTitleGenerator>());
            serviceContainer.AddService(Substitute.For<IWindowsJumpListManager>());
            serviceContainer.AddService(Substitute.For<ILinkFactory>());
            serviceContainer.AddService(Substitute.For<IRepositoryHistoryUIService>());

            IScriptsManager scriptsManager = Substitute.For<IScriptsManager>();
            scriptsManager.GetScripts().Returns([]);
            serviceContainer.AddService(scriptsManager);

            serviceContainer.AddService(Substitute.For<IScriptsRunner>());

            serviceContainer.AddService(Substitute.For<IHotkeySettingsManager>());
            serviceContainer.AddService(Substitute.For<IHotkeySettingsLoader>());

            serviceContainer.AddService(Substitute.For<ISubmoduleStatusProvider>());

            return serviceContainer;
        }
    }
}
