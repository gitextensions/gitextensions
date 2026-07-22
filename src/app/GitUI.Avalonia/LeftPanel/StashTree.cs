using Avalonia.Controls;
using Avalonia.Input;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI.Properties;
using GitUIPluginInterfaces;

using ResourceManager;

namespace GitUI.LeftPanel;

/// <summary>Repository-tree root for stored stashes.</summary>
internal sealed class StashTree
{
    private readonly RepoObjectsTree _owner;

    public StashTree(RepoObjectsTree owner, IReadOnlyCollection<GitRevision> stashes)
    {
        _owner = owner;
        StashNode[] nodes =
        [
            .. stashes
                .Where(stash => !string.IsNullOrEmpty(stash.ReflogSelector))
                .Select(stash => new StashNode(this, stash.ObjectId, stash.ReflogSelector!, stash.Subject)),
        ];
        TreeViewItem[] items = [.. nodes.Select(CreateTreeViewItem)];
        TreeViewNode = new TreeViewItem
        {
            Header = RepoObjectsTree.CreateHeader($"{TranslatedStrings.Stashes} ({items.Length})", Images.Stash),
            ItemsSource = items,
            Tag = this,
        };
        TreeViewNode.PointerPressed += (_, e) => SelectOnContextClick(TreeViewNode, e);
    }

    public TreeViewItem TreeViewNode { get; }

    internal IGitUICommands UICommands => _owner.UICommands;

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

    private TreeViewItem CreateTreeViewItem(StashNode node)
    {
        TreeViewItem item = new()
        {
            Header = RepoObjectsTree.CreateHeader(node.DisplayName, Images.Stash),
            Tag = node,
        };
        item.PointerPressed += (_, e) => SelectOnContextClick(item, e);
        item.DoubleTapped += (_, _) => node.OpenStash(_owner);
        return item;
    }

    private void SelectOnContextClick(TreeViewItem item, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(item).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
        {
            _owner.SelectTreeViewItem(item);
        }
    }
}
