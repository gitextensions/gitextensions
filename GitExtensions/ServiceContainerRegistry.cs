using System.ComponentModel.Design;
using System.IO.Abstractions;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using ResourceManager;

namespace GitExtensions;

internal static class ServiceContainerRegistry
{
    public static void RegisterServices(ServiceContainer serviceContainer)
    {
        GitExtUtils.ServiceContainerRegistry.RegisterServices(serviceContainer);

        FileSystem fileSystem = new();
        GitDirectoryResolver gitDirectoryResolver = new(fileSystem);
        RepositoryDescriptionProvider repositoryDescriptionProvider = new(gitDirectoryResolver);

        serviceContainer.AddService<IFileSystem>(fileSystem);
        serviceContainer.AddService<IGitDirectoryResolver>(gitDirectoryResolver);
        serviceContainer.AddService<IRepositoryDescriptionProvider>(repositoryDescriptionProvider);
        serviceContainer.AddService<IAppTitleGenerator>(new AppTitleGenerator(repositoryDescriptionProvider));
        serviceContainer.AddService<ILinkFactory>(new LinkFactory());

        GitUIPluginInterfaces.ServiceContainerRegistry.RegisterServices(serviceContainer);
        GitCommands.ServiceContainerRegistry.RegisterServices(serviceContainer);
        GitUI.ServiceContainerRegistry.RegisterServices(serviceContainer);
    }
}
