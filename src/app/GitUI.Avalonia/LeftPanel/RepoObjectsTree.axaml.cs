using Avalonia.Controls;
using GitExtensions.Extensibility.Git;
using GitUIPluginInterfaces;

using ResourceManager;

namespace GitUI.LeftPanel;

// TODO(avalonia-port): milestone M1.4b — a read-only branches/remotes/tags panel.
// The WinForms RepoObjectsTree's nested branch folders, submodules, stashes, favorites,
// filtering, and context menus arrive in later milestones.
public partial class RepoObjectsTree : GitExtensionsControl
{
    public RepoObjectsTree()
    {
        InitializeComponent();

        treeMain.SelectionChanged += (_, _) => SelectionChanged?.Invoke(this, EventArgs.Empty);

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
    ///  Fills the tree from the repository refs.
    /// </summary>
    public void SetRefs(IReadOnlyList<IGitRef> refs)
    {
        TreeViewItem branchesNode = CreateGroupNode("Branches", refs.Where(gitRef => gitRef.IsHead && !gitRef.IsTag));
        TreeViewItem remotesNode = CreateGroupNode("Remotes", refs.Where(gitRef => gitRef.IsRemote));
        TreeViewItem tagsNode = CreateGroupNode("Tags", refs.Where(gitRef => gitRef.IsTag));
        branchesNode.IsExpanded = true;

        treeMain.ItemsSource = new[] { branchesNode, remotesNode, tagsNode };

        return;

        static TreeViewItem CreateGroupNode(string caption, IEnumerable<IGitRef> groupRefs)
        {
            IGitRef[] refs =
            [
                .. groupRefs.OrderBy(gitRef => gitRef.Name, StringComparer.OrdinalIgnoreCase),
            ];
            TreeViewItem[] children =
            [
                .. refs.Select((gitRef, index) => new TreeViewItem
                {
                    Header = CreateChildHeader(gitRef.Name, index == refs.Length - 1),
                    Tag = gitRef,
                }),
            ];
            return new TreeViewItem
            {
                Header = $"{caption} ({children.Length})",
                ItemsSource = children,
            };
        }

        static Grid CreateChildHeader(string caption, bool isLastSibling)
        {
            TreeConnectorControl connector = new(isLastSibling);
            TextBlock text = new()
            {
                Text = caption,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };
            Grid.SetColumn(text, 1);

            return new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("18,*"),
                Children =
                {
                    connector,
                    text,
                },
            };
        }
    }

    internal sealed class TreeConnectorControl : Grid
    {
        internal TreeConnectorControl(bool isLastSibling)
        {
            IsLastSibling = isLastSibling;
            RowDefinitions = new RowDefinitions("*,*");
            ColumnDefinitions = new ColumnDefinitions("4,*");

            Border vertical = new()
            {
                Width = 1,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            };
            vertical.Classes.Add("gitextensions-tree-connector");
            SetRowSpan(vertical, isLastSibling ? 1 : 2);

            Border horizontal = new()
            {
                Height = 1,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
            };
            horizontal.Classes.Add("gitextensions-tree-connector");
            SetRow(horizontal, 0);
            SetColumn(horizontal, 1);

            Children.Add(vertical);
            Children.Add(horizontal);
        }

        internal bool IsLastSibling { get; }
    }
}
