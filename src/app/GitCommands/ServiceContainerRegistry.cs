using System.ComponentModel.Design;
using GitCommands.Submodules;
using GitExtUtils;

namespace GitCommands;

public static class ServiceContainerRegistry
{
    public static void RegisterServices(ServiceContainer serviceContainer)
    {
        serviceContainer.AddService<ISubmoduleStatusProvider>(new SubmoduleStatusProvider((path) => new GitModule(path)));
    }
}
