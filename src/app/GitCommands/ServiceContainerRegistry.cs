using System.ComponentModel.Design;
using GitCommands.Submodules;
using GitUIPluginInterfaces;

namespace GitCommands;

public static class ServiceContainerRegistry
{
    public static void RegisterServices(ServiceContainer serviceContainer)
    {
        serviceContainer.AddService<ISubmoduleStatusProvider>(new SubmoduleStatusProvider());
    }
}
