using System.ComponentModel.Design;
using GitCommands.Submodules;
using GitExtensions.Extensibility.Git;
using GitExtUtils;

namespace GitCommands;

public static class ServiceContainerRegistry
{
    public static void RegisterServices(ServiceContainer serviceContainer)
    {
        GitExecutorProvider executorProvider = new();
        serviceContainer.AddService<IGitExecutorProvider>(executorProvider);
        serviceContainer.AddService<ISubmoduleStatusProvider>(new SubmoduleStatusProvider(executorProvider));
    }
}
