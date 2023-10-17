using System.ComponentModel.Design;
using Plugins.GitUIPluginInterfaces.ViewModels;

namespace GitUIPluginInterfaces;

public static class ServiceContainerRegistry
{
    public static void RegisterServices(ServiceContainer serviceContainer)
    {
        GitRepoViewModel.Register(serviceContainer);
    }
}
