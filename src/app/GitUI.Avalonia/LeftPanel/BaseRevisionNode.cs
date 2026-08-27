using Avalonia.Media;
using GitExtensions.Extensibility.Git;

namespace GitUI.LeftPanel;

internal abstract class BaseRevisionNode : Node
{
    protected BaseRevisionNode(Tree tree, NodeBase parent, string fullPath, IGitRef? gitRef, IImage icon, bool isBold = false)
        : base(tree, parent, GetName(fullPath), icon, isBold)
    {
        FullPath = fullPath;
        GitRef = gitRef;
        ObjectId = gitRef?.ObjectId ?? default;
    }

    public string Name => GetName(FullPath);

    protected string ParentPath
    {
        get
        {
            int separator = FullPath.LastIndexOf('/');
            return separator < 0 ? string.Empty : FullPath[..separator];
        }
    }

    public IGitRef? GitRef { get; }

    public string FullPath { get; }

    public override string SearchText => FullPath;

    public ObjectId ObjectId { get; protected init; }

    public bool Rebase()
        => UICommands.StartRebaseDialog(Owner, FullPath);

    public bool Reset()
        => UICommands.StartResetCurrentBranchDialog(Owner, FullPath);

    private static string GetName(string fullPath)
    {
        int separator = fullPath.LastIndexOf('/');
        return separator < 0 ? fullPath : fullPath[(separator + 1)..];
    }
}
