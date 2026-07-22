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

    public string FullPath { get; }

    public string Name => GetName(FullPath);

    public IGitRef? GitRef { get; }

    public ObjectId ObjectId { get; protected init; }

    public override string SearchText => FullPath;

    protected string ParentPath
    {
        get
        {
            int separator = FullPath.LastIndexOf('/');
            return separator < 0 ? string.Empty : FullPath[..separator];
        }
    }

    public bool Rebase()
        => UICommands.StartRebaseDialog(Owner, FullPath);

    private static string GetName(string fullPath)
    {
        int separator = fullPath.LastIndexOf('/');
        return separator < 0 ? fullPath : fullPath[(separator + 1)..];
    }
}
