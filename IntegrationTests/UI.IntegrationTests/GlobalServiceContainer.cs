using System.ComponentModel;
using System.ComponentModel.Design;
using GitCommands;
using GitUI;
using GitUI.Hotkey;
using GitUI.ScriptsEngine;
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

            IScriptsManager scriptsManager = Substitute.For<IScriptsManager>();
            scriptsManager.GetScripts().Returns(new BindingList<ScriptInfo>());
            serviceContainer.AddService(scriptsManager);

            serviceContainer.AddService(Substitute.For<IScriptsRunner>());

            serviceContainer.AddService(Substitute.For<IHotkeySettingsManager>());
            serviceContainer.AddService(Substitute.For<IHotkeySettingsLoader>());

            return serviceContainer;
        }
    }
}
