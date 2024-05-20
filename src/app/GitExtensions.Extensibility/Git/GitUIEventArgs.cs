using System.ComponentModel;

namespace GitExtensions.Extensibility.Git;

public class GitUIEventArgs : CancelEventArgs
{
    private readonly IFilteredGitRefsProvider _getRefs;

    public GitUIEventArgs(IWin32Window? ownerForm, IGitUICommands gitUICommands, Lazy<IReadOnlyList<IGitRef>>? getRefs = null)
        : base(cancel: false)
    {
        OwnerForm = ownerForm;
        GitUICommands = gitUICommands;
        if (getRefs is null)
        {
            _getRefs = new FilteredGitRefsProvider(GitModule);
        }
        else
        {
            _getRefs = new FilteredGitRefsProvider(getRefs);
        }
    }

    public IGitUICommands GitUICommands { get; }

    public IWin32Window? OwnerForm { get; }

    public IGitModule GitModule => GitUICommands.Module;

    public IReadOnlyList<IGitRef> GetRefs(RefsFilter filter) => _getRefs.GetRefs(filter);
}
