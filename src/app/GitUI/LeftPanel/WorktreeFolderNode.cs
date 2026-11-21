using System.Diagnostics;

namespace GitUI.LeftPanel;

/// <summary>
///  A folder of the Worktrees tree, shown like a branch folder. A chain of single-child folders is shown as one, e.g. "a/b/c".
/// </summary>
[DebuggerDisplay("(Worktree folder) FullPath = {FullPath}")]
internal sealed class WorktreeFolderNode(Tree tree, string fullPath, string displayName) : BasePathNode(tree, fullPath)
{
    protected override string DisplayText()
        => displayName;
}
