using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI.Properties;
using GitUIPluginInterfaces;

using ResourceManager;

namespace GitUI.LeftPanel;

/// <summary>Repository-tree root for stored stashes.</summary>
internal sealed class StashTree : Tree
{
    public StashTree(RepoObjectsTree owner, IReadOnlyCollection<GitRevision> stashes)
        : base(owner, RepoTreeKind.Stashes, TranslatedStrings.Stashes, Images.Stash)
    {
        StashNode[] nodes =
        [
            .. stashes
                .Where(stash => !string.IsNullOrEmpty(stash.ReflogSelector))
                .Select(stash => new StashNode(this, this, stash.ObjectId, stash.ReflogSelector!, stash.Subject)),
        ];
        foreach (StashNode node in nodes)
        {
            TreeViewNode.Items.Add(node.TreeViewNode);
        }

        Complete(TranslatedStrings.Stashes, Images.Stash, nodes.Length, expanded: false);
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
