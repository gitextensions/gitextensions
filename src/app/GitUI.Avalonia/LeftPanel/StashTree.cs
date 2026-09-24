using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI.Properties;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;

using ResourceManager;

namespace GitUI.LeftPanel;

/// <summary>Repository-tree root for stored stashes.</summary>
internal sealed class StashTree : BaseRevisionTree
{
    public StashTree(RepoObjectsTree owner, IReadOnlyCollection<GitRevision> stashes, ICheckRefs? refsSource = null)
        : base(owner, RepoTreeKind.Stashes, TranslatedStrings.Stashes, Images.Stash, refsSource)
    {
        Nodes.AddNodes(FillStashTree(stashes, CancellationToken.None));

        Complete(TranslatedStrings.Stashes, Images.Stash, expanded: false);
    }

    private Nodes FillStashTree(IReadOnlyCollection<GitRevision> stashes, CancellationToken token)
    {
        Nodes nodes = new(this);
        foreach (GitRevision stash in stashes.Where(stash => !string.IsNullOrEmpty(stash.ReflogSelector)))
        {
            token.ThrowIfCancellationRequested();
            nodes.AddNode(new StashNode(this, this, stash.ObjectId, stash.ReflogSelector!, stash.Subject));
        }

        return nodes;
    }

    protected override void PostFillTreeViewNode(bool firstTime)
    {
        if (firstTime)
        {
            TreeViewNode.IsExpanded = false;
        }
    }

    public void StashAll(IWin32Window owner)
    {
        UICommands.StashSave(owner, AppSettings.IncludeUntrackedFilesInManualStash);
    }

    public void StashStaged(IWin32Window owner)
    {
        UICommands.StashStaged(owner);
    }

    public void OpenStash(IWin32Window owner)
    {
        UICommands.StartStashDialog(owner, manageStashes: true);
    }
}
