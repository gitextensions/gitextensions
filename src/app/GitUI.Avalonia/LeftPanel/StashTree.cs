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
        Refresh(new Lazy<IReadOnlyCollection<GitRevision>>(() => stashes));
    }

    internal void Refresh(Lazy<IReadOnlyCollection<GitRevision>> getStashRevs)
    {
        if (!IsAttached)
        {
            return;
        }

        OwnerControl.UpdateNodes(() =>
        {
            HashSet<string> selected = OwnerControl.CaptureSelectedNodeIdentities(this);
            bool wasExpanded = TreeViewNode.IsExpanded;
            TreeViewNode.Items.Clear();
            Nodes.Clear();
            Nodes.AddNodes(FillStashTree(getStashRevs.Value, CancellationToken.None));
            Complete(TranslatedStrings.Stashes, Images.Stash, expanded: wasExpanded);
            OwnerControl.RestoreSelectedNodes(this, selected);
        });
    }

    private Task<Nodes> LoadNodesAsync(Lazy<IReadOnlyCollection<GitRevision>> getStashRevs, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return Task.FromResult(FillStashTree(getStashRevs.Value, token));
    }

    private Nodes FillStashTree(IReadOnlyCollection<GitRevision> stashes, CancellationToken token)
    {
        Nodes nodes = new(this);
        foreach (GitRevision stash in stashes.Where(stash => !string.IsNullOrEmpty(stash.ReflogSelector)))
        {
            token.ThrowIfCancellationRequested();

            // Visibility is set after the grid is loaded
            nodes.AddNode(new StashNode(this, this, stash.ObjectId, stash.ReflogSelector!, stash.Subject, visible: false));
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
