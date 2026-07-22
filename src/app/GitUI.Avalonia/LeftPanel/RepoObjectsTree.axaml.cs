using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI.Properties;
using GitUIPluginInterfaces;

using ResourceManager;

namespace GitUI.LeftPanel;

// TODO(avalonia-port): nested branch folders, submodules, worktrees, favorites, filtering,
// and their context menus arrive in later browse-feature subphases.
public partial class RepoObjectsTree : GitModuleControl
{
    private StashTree? _stashTree;

    public RepoObjectsTree()
    {
        InitializeComponent();

        treeMain.SelectionChanged += (_, _) => SelectionChanged?.Invoke(this, EventArgs.Empty);
        repoObjectsContextMenu.Opening += RepoObjectsContextMenuOpening;
        mnubtnStashAllFromRootNode.Click += (_, _) => _stashTree?.StashAll(this);
        mnubtnStashStagedFromRootNode.Click += (_, _) => _stashTree?.StashStaged(this);
        mnubtnManageStashFromRootNode.Click += (_, _) => _stashTree?.OpenStash(this);
        mnubtnOpenStash.Click += (_, _) => SelectedStashNode?.OpenStash(this);
        mnubtnApplyStash.Click += (_, _) => SelectedStashNode?.ApplyStash(this);
        mnubtnPopStash.Click += (_, _) => SelectedStashNode?.PopStash(this);
        mnubtnDropStash.Click += (_, _) => SelectedStashNode?.DropStash(this);

        InitializeComplete();
    }

    /// <summary>
    ///  Occurs when the selected node changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    ///  Gets the ref of the selected node, or <see langword="null"/> for group nodes.
    /// </summary>
    public IGitRef? SelectedRef => (treeMain.SelectedItem as TreeViewItem)?.Tag as IGitRef;

    /// <summary>
    ///  Gets the object id represented by the selected ref or stash node.
    /// </summary>
    public ObjectId? SelectedRevisionObjectId
        => (treeMain.SelectedItem as TreeViewItem)?.Tag switch
        {
            IGitRef gitRef => gitRef.ObjectId,
            StashNode stashNode => stashNode.ObjectId,
            _ => null,
        };

    private StashNode? SelectedStashNode => (treeMain.SelectedItem as TreeViewItem)?.Tag as StashNode;

    /// <summary>
    ///  Fills the tree from the repository refs.
    /// </summary>
    public void SetRefs(IReadOnlyList<IGitRef> refs)
        => SetRefs(refs, [], includeStashes: false);

    /// <summary>
    ///  Fills the tree from the repository refs and stash revisions.
    /// </summary>
    public void SetRefs(IReadOnlyList<IGitRef> refs, IReadOnlyCollection<GitRevision> stashes)
        => SetRefs(refs, stashes, includeStashes: true);

    private void SetRefs(
        IReadOnlyList<IGitRef> refs,
        IReadOnlyCollection<GitRevision> stashes,
        bool includeStashes)
    {
        TreeViewItem branchesNode = CreateGroupNode(
            "Branches",
            Images.BranchLocalRoot,
            refs.Where(gitRef => gitRef.IsHead && !gitRef.IsTag));
        TreeViewItem remotesNode = CreateGroupNode(
            "Remotes",
            Images.BranchRemoteRoot,
            refs.Where(gitRef => gitRef.IsRemote));
        TreeViewItem tagsNode = CreateGroupNode(
            "Tags",
            Images.TagHorizontal,
            refs.Where(gitRef => gitRef.IsTag));
        _stashTree = new StashTree(this, stashes);
        branchesNode.IsExpanded = true;

        TreeViewItem[] roots = includeStashes && AppSettings.RepoObjectsTreeShowStashes
            ? [branchesNode, remotesNode, tagsNode, _stashTree.TreeViewNode]
            : [branchesNode, remotesNode, tagsNode];
        treeMain.ItemsSource = roots;

        return;

        static TreeViewItem CreateGroupNode(string caption, IImage groupIcon, IEnumerable<IGitRef> groupRefs)
        {
            IGitRef[] refs =
            [
                .. groupRefs.OrderBy(gitRef => gitRef.Name, StringComparer.OrdinalIgnoreCase),
            ];
            TreeViewItem[] children =
            [
                .. refs.Select(gitRef => new TreeViewItem
                {
                    Header = CreateHeader(
                        gitRef.Name,
                        gitRef.IsTag
                            ? Images.TagHorizontal
                            : gitRef.IsRemote
                                ? Images.BranchRemote
                                : Images.BranchLocal),
                    Tag = gitRef,
                }),
            ];
            return new TreeViewItem
            {
                Header = CreateHeader($"{caption} ({children.Length})", groupIcon),
                ItemsSource = children,
            };
        }
    }

    internal static Control CreateHeader(string caption, IImage icon)
        => new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 2,
            Children =
            {
                new Image
                {
                    Width = 16,
                    Height = 16,
                    Stretch = Stretch.Uniform,
                    Source = icon,
                },
                new TextBlock
                {
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Text = caption,
                },
            },
        };

    private void RepoObjectsContextMenuOpening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        bool stashTreeSelected = (treeMain.SelectedItem as TreeViewItem)?.Tag is StashTree;
        bool stashSelected = SelectedStashNode is not null;
        bool canRunCommands = TryGetUICommandsDirect(out IGitUICommands? commands);
        bool canChangeWorkingTree = canRunCommands && !commands!.Module.IsBareRepository();

        mnubtnStashAllFromRootNode.IsVisible = stashTreeSelected;
        mnubtnStashStagedFromRootNode.IsVisible = stashTreeSelected;
        mnubtnManageStashFromRootNode.IsVisible = stashTreeSelected;
        mnubtnOpenStash.IsVisible = stashSelected;
        mnubtnApplyStash.IsVisible = stashSelected;
        mnubtnPopStash.IsVisible = stashSelected;
        mnubtnDropStash.IsVisible = stashSelected;

        mnubtnStashAllFromRootNode.IsEnabled = canRunCommands;
        mnubtnStashStagedFromRootNode.IsEnabled = canRunCommands;
        mnubtnManageStashFromRootNode.IsEnabled = canRunCommands;
        mnubtnOpenStash.IsEnabled = canChangeWorkingTree;
        mnubtnApplyStash.IsEnabled = canChangeWorkingTree;
        mnubtnPopStash.IsEnabled = canChangeWorkingTree;
        mnubtnDropStash.IsEnabled = canChangeWorkingTree;
        e.Cancel = !stashTreeSelected && !stashSelected;
    }

    internal void SelectTreeViewItem(TreeViewItem item)
        => treeMain.SelectedItem = item;

    internal TestAccessor GetTestAccessor()
        => new(this);

    internal readonly struct TestAccessor(RepoObjectsTree control)
    {
        internal TreeView Tree => control.treeMain;

        internal ContextMenu ContextMenu => control.repoObjectsContextMenu;

        internal MenuItem StashAllMenuItem => control.mnubtnStashAllFromRootNode;

        internal MenuItem StashStagedMenuItem => control.mnubtnStashStagedFromRootNode;

        internal MenuItem ManageStashesMenuItem => control.mnubtnManageStashFromRootNode;

        internal MenuItem OpenStashMenuItem => control.mnubtnOpenStash;

        internal MenuItem ApplyStashMenuItem => control.mnubtnApplyStash;

        internal MenuItem PopStashMenuItem => control.mnubtnPopStash;

        internal MenuItem DropStashMenuItem => control.mnubtnDropStash;

        internal bool UpdateContextMenu()
        {
            System.ComponentModel.CancelEventArgs eventArgs = new();
            control.RepoObjectsContextMenuOpening(control.repoObjectsContextMenu, eventArgs);
            return !eventArgs.Cancel;
        }
    }
}

/// <summary>
///  Draws the dotted hierarchy lines supplied by the native WinForms tree but absent from
///  Avalonia's Fluent TreeView template.
/// </summary>
internal sealed class TreeConnectorControl : Control
{
    private const double ChevronCenter = 10;
    private const double ChevronGapHalfHeight = 6;
    private const double ChevronGapHalfWidth = 6;
    private const double Indent = 18;
    private static readonly DashStyle DottedLine = new([1, 1], 0);

    internal TreeViewItem? Item => this.FindAncestorOfType<TreeViewItem>();

    internal bool IsLastSibling
    {
        get
        {
            if (Item is not TreeViewItem item)
            {
                return true;
            }

            (int index, int count) = GetSiblingPosition(item);
            return index == count - 1;
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        TreeViewItem? item = Item;
        if (item is null || Bounds.Height <= 0)
        {
            return;
        }

        IBrush brush = GetConnectorBrush();
        Pen pen = new(brush, 1, DottedLine, PenLineCap.Flat, PenLineJoin.Miter, 10);
        double middle = Bounds.Height / 2;
        (int index, int count) = GetSiblingPosition(item);
        double x = ChevronCenter + (item.Level * Indent);
        double top = item.Level == 0 && index == 0 ? middle : 0;
        double bottom = index == count - 1 ? middle : Bounds.Height;
        bool hasChevron = item.Items.Count > 0;

        foreach ((Avalonia.Point start, Avalonia.Point end) in GetCurrentItemLines(x, top, bottom, middle, hasChevron))
        {
            context.DrawLine(pen, start, end);
        }

        for (TreeViewItem? ancestor = GetParentItem(item);
             ancestor is not null;
             ancestor = GetParentItem(ancestor))
        {
            (int ancestorIndex, int ancestorCount) = GetSiblingPosition(ancestor);
            if (ancestorIndex < ancestorCount - 1)
            {
                double ancestorX = ChevronCenter + (ancestor.Level * Indent);
                context.DrawLine(
                    pen,
                    new Avalonia.Point(ancestorX, 0),
                    new Avalonia.Point(ancestorX, Bounds.Height));
            }
        }
    }

    internal static IEnumerable<(Avalonia.Point Start, Avalonia.Point End)> GetCurrentItemLines(
        double x,
        double top,
        double bottom,
        double middle,
        bool hasChevron)
    {
        if (!hasChevron)
        {
            yield return (new Avalonia.Point(x, top), new Avalonia.Point(x, bottom));
            yield return (new Avalonia.Point(x, middle), new Avalonia.Point(x + ChevronCenter, middle));
            yield break;
        }

        double upperEnd = Math.Min(bottom, middle - ChevronGapHalfHeight);
        if (top < upperEnd)
        {
            yield return (new Avalonia.Point(x, top), new Avalonia.Point(x, upperEnd));
        }

        double lowerStart = Math.Max(top, middle + ChevronGapHalfHeight);
        if (lowerStart < bottom)
        {
            yield return (new Avalonia.Point(x, lowerStart), new Avalonia.Point(x, bottom));
        }

        yield return (
            new Avalonia.Point(x + ChevronGapHalfWidth, middle),
            new Avalonia.Point(x + ChevronCenter, middle));
    }

    private static TreeViewItem? GetParentItem(TreeViewItem item)
        => item.GetVisualAncestors().OfType<TreeViewItem>().FirstOrDefault();

    private static (int Index, int Count) GetSiblingPosition(TreeViewItem item)
    {
        TreeViewItem? parentItem = GetParentItem(item);
        ItemCollection siblings = parentItem is not null
            ? parentItem.Items
            : item.GetVisualAncestors().OfType<TreeView>().First().Items;
        return (Math.Max(siblings.IndexOf(item), 0), siblings.Count);
    }

    private IBrush GetConnectorBrush()
        => Application.Current?.TryGetResource(
                "GitExtensionsTreeConnectorBrush",
                ActualThemeVariant,
                out object? resource) == true
            && resource is IBrush brush
                ? brush
                : Brushes.Gray;
}
