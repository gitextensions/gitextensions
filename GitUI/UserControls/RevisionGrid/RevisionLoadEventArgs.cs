using GitExtensions.Extensibility.Git;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid
{
    public class RevisionLoadEventArgs : GitUIEventArgs
    {
        public RevisionLoadEventArgs(IWin32Window? ownerForm, IGitUICommands gitUICommands, Lazy<IReadOnlyList<IGitRef>> getRefs, Lazy<IReadOnlyCollection<GitRevision>> getStashRevs, bool forceRefresh)
            : base(ownerForm, gitUICommands, getRefs)
        {
            GetStashRevs = getStashRevs;
            ForceRefresh = forceRefresh;
        }

        public Lazy<IReadOnlyCollection<GitRevision>> GetStashRevs { get; init; }
        public bool ForceRefresh { get; init; }
    }
}
