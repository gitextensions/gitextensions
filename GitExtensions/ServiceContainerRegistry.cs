using System.ComponentModel.Design;
using System.IO.Abstractions;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitExtensions;

internal static class ServiceContainerRegistry
{
    public static void RegisterServices(ServiceContainer serviceContainer)
    {
        FileSystem fileSystem = new();
        GitDirectoryResolver gitDirectoryResolver = new(fileSystem);
        RepositoryDescriptionProvider repositoryDescriptionProvider = new(gitDirectoryResolver);

        serviceContainer.AddService<IFileSystem>(fileSystem);
        serviceContainer.AddService<IGitDirectoryResolver>(gitDirectoryResolver);
        serviceContainer.AddService<IRepositoryDescriptionProvider>(repositoryDescriptionProvider);
        serviceContainer.AddService<IAppTitleGenerator>(new AppTitleGenerator(repositoryDescriptionProvider));
        serviceContainer.AddService<ILinkFactory>(new LinkFactory());

        GitCommands.ServiceContainerRegistry.RegisterServices(serviceContainer);
        GitUI.ServiceContainerRegistry.RegisterServices(serviceContainer);
    }
}
