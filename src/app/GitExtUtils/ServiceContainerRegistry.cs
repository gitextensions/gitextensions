using System.ComponentModel.Design;

namespace GitExtUtils;

public static class ServiceContainerRegistry
{
    public static void RegisterServices(ServiceContainer serviceContainer)
    {
        serviceContainer.AddService<ISubscribableTraceListener>(new SubscribableTraceListener());
    }
}
