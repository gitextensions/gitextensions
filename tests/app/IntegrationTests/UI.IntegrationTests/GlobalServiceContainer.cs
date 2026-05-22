using System.ComponentModel.Design;
using GitCommands;
using GitCommands.Git;
using GitCommands.Submodules;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.ConsoleEmulation;
using GitUI.ConsoleEmulation.PlainText;
using GitUI.Hotkey;
using GitUI.Models;
using GitUI.ScriptsEngine;
using NSubstitute;
using ResourceManager;

namespace GitExtensions.UITests;

public static class GlobalServiceContainer
{
    public static ServiceContainer CreateDefaultMockServiceContainer()
    {
        ServiceContainer serviceContainer = new();

        serviceContainer.AddService(Substitute.For<IOutputHistoryProvider>());

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

        IGitBranchNameNormaliser branchNameNormaliser = Substitute.For<IGitBranchNameNormaliser>();
        branchNameNormaliser.Normalise(Arg.Any<string?>(), Arg.Any<GitBranchNameOptions>())
            .Returns(callInfo => callInfo.Arg<string?>());
        serviceContainer.AddService(branchNameNormaliser);

        serviceContainer.AddService<IGitExecutorProvider>(new GitExecutorProvider(new GitDirectoryResolver()));

        serviceContainer.AddService<IConsoleEmulatorsRegistry>(PlainTextConsoleEmulatorsRegistry.Instance);

        return serviceContainer;
    }
}
