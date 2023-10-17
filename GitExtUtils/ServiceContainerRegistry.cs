using System.ComponentModel.Design;
using CommunityToolkit.Mvvm.Messaging;

namespace GitExtUtils;

public static class ServiceContainerRegistry
{
    public static void RegisterServices(ServiceContainer serviceContainer)
    {
        IMessenger messenger = WeakReferenceMessenger.Default;
        serviceContainer.AddService(messenger);
    }
}
