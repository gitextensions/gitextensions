using System.ComponentModel.Design;
using CommunityToolkit.Mvvm.Messaging;
using Plugins.GitUIPluginInterfaces.ViewModels;

namespace GitUIPluginInterfaces;

public static class ServiceContainerRegistry
{
    public static void RegisterServices(ServiceContainer serviceContainer)
    {
        IMessenger messenger = serviceContainer.GetService<IMessenger>();
        serviceContainer.AddService<IGitRepoViewModel>(new GitRepoViewModel(messenger));
    }
}
