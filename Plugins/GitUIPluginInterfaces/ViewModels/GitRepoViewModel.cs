using System.ComponentModel;
using System.ComponentModel.Design;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using GitUIPluginInterfaces;

namespace Plugins.GitUIPluginInterfaces.ViewModels;

public interface IGitRepoViewModel : INotifyPropertyChanged
{
    IEnumerable<GitRevision> Revisions { get; }
}

internal partial class GitRepoViewModel : ObservableObject, IGitRepoViewModel, IRecipient<RepoModifiedMessage>
{
    internal static void Register(IServiceContainer serviceContainer)
    {
        IGitRepoViewModel model = new GitRepoViewModel();
        serviceContainer.AddService(model);
        serviceContainer.GetRequiredService<IMessenger>().RegisterAll(model);
    }

    public IEnumerable<GitRevision> Revisions
    {
        get => throw new NotImplementedException();
    }

    public void Receive(RepoModifiedMessage message)
    {
        OnPropertyChanged(nameof(Revisions));
    }
}
