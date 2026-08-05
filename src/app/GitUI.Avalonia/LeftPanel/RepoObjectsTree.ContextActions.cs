using Avalonia.Controls;
using Avalonia.Media;
using GitUI.Compat;
using GitUI.LeftPanel.ContextMenu;
using GitUI.LeftPanel.Interfaces;
using ResourceManager;

namespace GitUI.LeftPanel;

partial class RepoObjectsTree : IMenuItemFactory
{
    private GitRefsSortOrderContextMenuItem _sortOrderContextMenuItem;
    private GitRefsSortByContextMenuItem _sortByContextMenuItem;

    /// <summary>
    /// Local branch context menu [git ref / rename / delete] actions.
    /// </summary>
    private LocalBranchMenuItems<LocalBranchNode> _localBranchMenuItems;

    /// <summary>
    /// Remote branch context menu [git ref / rename / delete] actions.
    /// </summary>
    private MenuItemsGenerator<RemoteBranchNode> _remoteBranchMenuItems;

    /// <summary>
    /// Tags context menu [git ref] actions.
    /// </summary>
    private MenuItemsGenerator<TagNode> _tagNodeMenuItems;

    private static void EnableMenuItems(bool enabled, params MenuItem[] items)
    {
        foreach (MenuItem item in items)
        {
            item.IsVisible = enabled;
            item.IsEnabled = enabled;
        }
    }

    private static void EnableMenuItems<TNode>(MenuItemsGenerator<TNode> generator, Func<ToolStripItemWithKey, bool> isEnabled) where TNode : class, INode
    {
        foreach (ToolStripItemWithKey item in generator)
        {
            item.Item.Enable(isEnabled(item));
        }
    }

    /* add Expand All / Collapse All menu entry
     * depending on whether node is expanded or collapsed and has child nodes at all */
    private void EnableExpandCollapseContextMenu(NodeBase[] selectedNodes)
    {
        NodeBase[] multiSelectedParents = [.. selectedNodes.HavingChildren()];
        SetAction(RepoAction.Expand, multiSelectedParents.Length > 0, multiSelectedParents.Expandable().Any());
        SetAction(RepoAction.Collapse, multiSelectedParents.Length > 0, multiSelectedParents.Collapsible().Any());
    }

    private void EnableMoveTreeUpDownContexMenu(bool hasSingleSelection, NodeBase? selectedNode)
    {
        Tree[] visibleTrees = [.. _trees.Where(tree => tree.IsEnabled).OrderBy(tree => tree.PositionIndex)];
        int index = selectedNode is Tree tree ? Array.IndexOf(visibleTrees, tree) : -1;
        SetAction(RepoAction.MoveUp, hasSingleSelection && index >= 0, index > 0);
        SetAction(RepoAction.MoveDown, hasSingleSelection && index >= 0, index < visibleTrees.Length - 1);
    }

    private static void RegisterClick(MenuItem item, Action onClick)
    {
        item.Click += (o, e) => onClick();
    }

    private void RegisterClick<T>(MenuItem item, Action<T> onClick) where T : class, INode
    {
        item.Click += (o, e) => Node.OnNode(treeMain.SelectedItem as TreeViewItem, onClick);
    }

    // parity-scaffolding: Lets headless tests locate generator-owned menu items by the legacy action names.
    private MenuItem GetActionMenuItem(string action)
    {
        if (Enum.TryParse(action, out RepoAction repoAction)
            && _actionItems.TryGetValue(repoAction, out MenuItem? menuItem))
        {
            return menuItem;
        }

        return action switch
        {
            "CheckoutLocal" => GetMenuItem(_localBranchMenuItems, MenuItemKey.GitRefCheckout),
            "CheckoutRemote" => GetMenuItem(_remoteBranchMenuItems, MenuItemKey.GitRefCheckout),
            "Merge" => GetMenuItem(_localBranchMenuItems, MenuItemKey.GitRefMerge),
            "RebaseLocal" => GetMenuItem(_localBranchMenuItems, MenuItemKey.GitRefRebase),
            "RebaseRemote" => GetMenuItem(_remoteBranchMenuItems, MenuItemKey.GitRefRebase),
            "RebaseTag" => GetMenuItem(_tagNodeMenuItems, MenuItemKey.GitRefRebase),
            "CreateBranch" => GetMenuItem(_localBranchMenuItems, MenuItemKey.GitRefCreateBranch),
            "Reset" => GetMenuItem(_localBranchMenuItems, MenuItemKey.GitRefReset),
            "RenameBranch" => GetMenuItem(_localBranchMenuItems, MenuItemKey.Rename),
            "DeleteBranch" => GetMenuItem(_localBranchMenuItems, MenuItemKey.Delete),
            "DeleteRemoteBranch" => GetMenuItem(_remoteBranchMenuItems, MenuItemKey.Delete),
            "CheckoutTag" => GetMenuItem(_tagNodeMenuItems, MenuItemKey.GitRefCheckout),
            "DeleteTag" => GetMenuItem(_tagNodeMenuItems, MenuItemKey.Delete),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };
    }

    private static MenuItem GetMenuItem<TNode>(MenuItemsGenerator<TNode> generator, MenuItemKey key) where TNode : class, INode
    {
        return generator.TryGetMenuItem(key, out Control? item) && item is MenuItem menuItem
            ? menuItem
            : throw new ArgumentOutOfRangeException(nameof(key), key, null);
    }

    /// <inheritdoc />
    public TMenuItem CreateMenuItem<TMenuItem, TNode>(Action<TNode> onClick, TranslationString text, TranslationString toolTip, IImage? icon = null)
        where TMenuItem : MenuItem, new()
        where TNode : class, INode
    {
        TMenuItem result = new()
        {
            Header = AvaloniaTranslationUtils.ToAvaloniaMnemonics(text.Text),
            Icon = icon is null ? null : new Image { Width = 16, Height = 16, Source = icon, Stretch = Stretch.Uniform },
        };
        ToolTip.SetTip(result, toolTip.Text);
        RegisterClick(result, onClick);
        return result;
    }
}
