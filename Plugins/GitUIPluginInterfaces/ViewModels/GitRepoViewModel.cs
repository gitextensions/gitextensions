using CommunityToolkit.Mvvm.ComponentModel;
using GitUIPluginInterfaces;

namespace Plugins.GitUIPluginInterfaces.ViewModels;

public partial class GitRepoViewModel : ObservableObject
{
    public IEnumerable<GitRevision> Revisions
    {
        get => throw new NotImplementedException();
    }

    public void RefreshRevisions()
    {
        OnPropertyChanged(nameof(Revisions));
    }
}
