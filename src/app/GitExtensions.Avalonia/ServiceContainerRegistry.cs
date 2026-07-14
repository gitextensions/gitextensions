using System.ComponentModel.Design;
using System.IO.Abstractions;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtUtils;
using ResourceManager;

namespace GitExtensions;

// Twin of GitExtensions/ServiceContainerRegistry.cs without the WinForms
// GitUI.ServiceContainerRegistry; GitUI.Avalonia services are added here as they are ported.
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

        GitCommands.ServiceContainerRegistry.RegisterServices(serviceContainer);
    }
}
