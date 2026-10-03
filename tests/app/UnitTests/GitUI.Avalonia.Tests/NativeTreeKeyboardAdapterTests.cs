using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI.Compat;
using GitUI.LeftPanel;
using GitUI.UserControls;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class NativeTreeKeyboardAdapterTests
{
    [AvaloniaTest]
    [TestCase(Key.Home, "Branches")]
    [TestCase(Key.End, "Stashes")]
    public void Home_and_End_should_choose_the_visible_preorder_endpoints_and_activate(Key key, string expected)
    {
        using TreeFixture fixture = new();
        fixture.Tree.SelectedItem = fixture.Main;
        fixture.ResetActivations();

        fixture.Press(key);

        ((TreeViewItem)fixture.Tree.SelectedItem!).Header.Should().Be(expected);
        fixture.Tree.SelectedItems.Cast<object>().Should().ContainSingle();
        fixture.Activations.Should().Be(1, "only arrow previews start the source explorer suppression interval");
    }

    [AvaloniaTest]
    [TestCase(Key.Up, "deep-leaf")]
    [TestCase(Key.Down, "maintenance")]
    public void Vertical_arrows_should_follow_every_expanded_descendant_without_activation(Key key, string expected)
    {
        using TreeFixture fixture = new();
        fixture.Tree.SelectedItem = fixture.Main;
        fixture.ResetActivations();

        fixture.Press(key);

        ((TreeViewItem)fixture.Tree.SelectedItem!).Header.Should().Be(expected);
        fixture.Tree.SelectedItems.Cast<object>().Should().ContainSingle();
        fixture.Activations.Should().Be(0);
    }

    [AvaloniaTest]
    public void Left_and_Right_should_collapse_expand_and_move_to_parent_or_first_child_without_activation()
    {
        using TreeFixture fixture = new();
        fixture.Tree.SelectedItem = fixture.DeepLeaf;
        fixture.ResetActivations();

        fixture.Press(Key.Left);
        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.Nested);
        fixture.Press(Key.Left);
        fixture.Nested.IsExpanded.Should().BeFalse();
        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.Nested);
        fixture.Press(Key.Right);
        fixture.Nested.IsExpanded.Should().BeTrue();
        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.Nested);
        fixture.Press(Key.Right);
        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.DeepLeaf);
        fixture.Activations.Should().Be(0);
    }

    [AvaloniaTest]
    [TestCase(Key.Space)]
    [TestCase(Key.Enter)]
    public void Source_activation_should_reselect_the_arrow_highlight_exactly_once(Key key)
    {
        using TreeFixture fixture = new();
        fixture.Tree.SelectedItem = fixture.Main;
        fixture.ResetActivations();
        fixture.Press(Key.Down);
        fixture.Activations.Should().Be(0);
        TreeViewItem caret = (TreeViewItem)fixture.Tree.SelectedItem!;

        fixture.Press(key);

        fixture.Tree.SelectedItem.Should().BeSameAs(caret);
        fixture.Activations.Should().Be(1);
    }

    [AvaloniaTest]
    [TestCase(Key.Up, -1)]
    [TestCase(Key.Down, 1)]
    public void Ctrl_arrows_should_scroll_one_arranged_header_without_moving_or_activating_the_caret(Key key, int direction)
    {
        using TreeFixture fixture = new();
        fixture.Tree.SelectedItem = fixture.Tags.Items.OfType<TreeViewItem>().ElementAt(10);
        fixture.Window.UpdateLayout();
        ScrollViewer scroll = GetScroll(fixture.Tree);
        scroll.Offset = new Vector(0, 100);
        TreeViewItem selected = (TreeViewItem)fixture.Tree.SelectedItem!;
        double rowHeight = selected.GetVisualDescendants().OfType<Border>()
            .First(border => border.Name == "PART_LayoutRoot").Bounds.Height;
        rowHeight.Should().BeGreaterThan(0);
        double oldY = scroll.Offset.Y;
        fixture.ResetActivations();

        fixture.Press(key, KeyModifiers.Control);

        scroll.Offset.Y.Should().Be(oldY + (direction * rowHeight));
        scroll.Offset.X.Should().Be(0);
        fixture.Tree.SelectedItem.Should().BeSameAs(selected);
        fixture.Activations.Should().Be(0);
    }

    [AvaloniaTest]
    public void Navigation_should_rebuild_visible_preorder_after_collapse_or_item_changes()
    {
        using TreeFixture fixture = new();
        fixture.Branches.IsExpanded = false;
        fixture.Tree.SelectedItem = fixture.Tags;
        fixture.Press(Key.Up);
        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.Branches);
        TreeViewItem last = new() { Header = "New root" };
        fixture.Tree.Items.Add(last);
        fixture.Press(Key.End);
        fixture.Tree.SelectedItem.Should().BeSameAs(last);
        fixture.Tree.Items.Remove(last);
        fixture.Press(Key.End);
        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.Stashes);
    }

    [AvaloniaTest]
    [TestCase(Key.PageUp, -1)]
    [TestCase(Key.PageDown, 1)]
    public void Page_keys_should_move_the_caret_by_whole_visible_items_minus_one(Key key, int direction)
    {
        using TreeFixture fixture = new();
        TreeViewItem[] tags = fixture.Tags.Items.OfType<TreeViewItem>().ToArray();
        fixture.Tree.SelectedItem = tags[15];
        fixture.Window.UpdateLayout();
        ScrollViewer scroll = GetScroll(fixture.Tree);
        double rowHeight = tags[15].GetVisualDescendants().OfType<Border>()
            .First(border => border.Name == "PART_LayoutRoot").Bounds.Height;
        int step = Math.Max(1, (int)Math.Floor(scroll.Viewport.Height / rowHeight) - 1);
        fixture.ResetActivations();

        fixture.Press(key);
        fixture.Tree.SelectedItem.Should().BeSameAs(tags[15 + (direction * step)]);
        fixture.Press(key);
        fixture.Tree.SelectedItem.Should().BeSameAs(tags[15 + (direction * step * 2)]);
        fixture.Activations.Should().Be(2);
    }

    [AvaloniaTest]
    public void Prefix_search_should_accumulate_caption_characters_and_preserve_an_unmatched_caret()
    {
        using TreeFixture fixture = new();
        fixture.Press(Key.Home);
        fixture.ResetActivations();

        fixture.Type("m");
        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.Main);
        fixture.Type("a");
        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.Main);
        fixture.Type("a");
        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.Main);
        fixture.Activations.Should().Be(1);
    }

    [AvaloniaTest]
    public void Repeated_initial_characters_should_cycle_matching_captions_but_remain_in_the_accumulated_prefix()
    {
        using TreeFixture fixture = new();
        fixture.Press(Key.Home);

        fixture.Type("m");
        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.Main);
        fixture.Type("m");
        ((TreeViewItem)fixture.Tree.SelectedItem!).Header.Should().Be("maintenance");
        fixture.Type("a");
        ((TreeViewItem)fixture.Tree.SelectedItem!).Header.Should().Be("maintenance", "native 'mma' does not silently fall back to a fresh 'ma' prefix");
        fixture.Type("zé");
        ((TreeViewItem)fixture.Tree.SelectedItem!).Header.Should().Be("maintenance");
    }

    [AvaloniaTest]
    [TestCase(969, false)]
    [TestCase(1062, true)]
    public void Prefix_idle_substitute_should_preserve_the_independently_probed_native_timing_points(int elapsed, bool reset)
    {
        long timestamp = 10000;
        using TreeFixture fixture = new(() => timestamp);
        fixture.Press(Key.Home);
        fixture.Type("m");
        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.Main);
        timestamp += elapsed;

        fixture.Type("a");

        ((TreeViewItem)fixture.Tree.SelectedItem!).Header.Should().Be(reset ? "alpha" : "main");
    }

    [AvaloniaTest]
    public void Navigation_and_leaving_native_tree_focus_should_reset_the_incremental_caption_search()
    {
        using TreeFixture fixture = new();
        fixture.Press(Key.Home);
        fixture.Type("ma");
        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.Main);
        Button other = new() { Content = "Other" };
        DockPanel panel = new() { Children = { other } };
        fixture.Window.Content = panel;
        panel.Children.Add(fixture.Tree);
        fixture.Window.UpdateLayout();
        fixture.Main.Focus().Should().BeTrue();
        other.Focus().Should().BeTrue();
        fixture.Tree.IsKeyboardFocusWithin.Should().BeFalse();
        fixture.Main.Focus().Should().BeTrue();
        fixture.Type("m");
        ((TreeViewItem)fixture.Tree.SelectedItem!).Header.Should().Be("maintenance");

        fixture.Press(Key.Home);
        fixture.Type("m");
        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.Main);
    }

    [AvaloniaTest]
    [TestCase(Key.Home)]
    [TestCase(Key.End)]
    [TestCase(Key.Up)]
    [TestCase(Key.Down)]
    [TestCase(Key.Left)]
    [TestCase(Key.Right)]
    [TestCase(Key.PageUp)]
    [TestCase(Key.PageDown)]
    public void Repository_caret_navigation_should_not_replace_logical_filter_flags(Key key)
    {
        bool showBranches = AppSettings.RepoObjectsTreeShowBranches;
        bool showTags = AppSettings.RepoObjectsTreeShowTags;
        Window window = new() { Width = 300, Height = 250 };
        try
        {
            AppSettings.RepoObjectsTreeShowBranches = true;
            AppSettings.RepoObjectsTreeShowTags = true;
            IGitModule module = Substitute.For<IGitModule>();
            RepoObjectsTree control = new();
            control.SetRefs(
            [
                new GitRef(module, ObjectId.Random(), "refs/heads/main"),
                new GitRef(module, ObjectId.Random(), "refs/heads/feature"),
                new GitRef(module, ObjectId.Random(), "refs/tags/v1")
            ], [], "main");
            RepoObjectsTree.TestAccessor accessor = control.GetTestAccessor();
            accessor.SelectNode<LocalBranchNode>(["Branches", "main"]);
            accessor.SelectNode<TagNode>(["Tags", "v1"], multiple: true);
            TreeViewItem[] logical = accessor.LogicalSelection.ToArray();
            window.Content = control;
            window.Show();
            window.UpdateLayout();
            accessor.Tree.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = key });

            accessor.LogicalSelection.Should().BeEquivalentTo(logical);
            accessor.Tree.SelectedItems.Cast<object>().Should().ContainSingle();
            foreach (TreeViewItem item in logical)
            {
                ((StackPanel)item.Header!).Children.OfType<TextBlock>().First().TextDecorations
                    .Should().BeSameAs(TextDecorations.Underline);
            }
        }
        finally
        {
            window.Close();
            AppSettings.RepoObjectsTreeShowBranches = showBranches;
            AppSettings.RepoObjectsTreeShowTags = showTags;
        }
    }

    private static ScrollViewer GetScroll(TreeView tree)
        => tree.GetVisualDescendants().OfType<ScrollViewer>().First(scroll => scroll.FindAncestorOfType<TreeView>() == tree);

    private sealed class TreeFixture : IDisposable
    {
        internal TreeFixture(Func<long>? getTextInputTimestamp = null)
        {
            DeepLeaf = new TreeViewItem { Header = "deep-leaf" };
            Nested = new TreeViewItem { Header = "nested", IsExpanded = true, Items = { DeepLeaf } };
            TreeViewItem feature = new() { Header = "feature", IsExpanded = true, Items = { Nested } };
            Main = new TreeViewItem { Header = "main" };
            Branches = new TreeViewItem
            {
                Header = "Branches", IsExpanded = true,
                Items = { new TreeViewItem { Header = "alpha" }, feature, Main, new TreeViewItem { Header = "maintenance" } }
            };
            Tags = new TreeViewItem { Header = "Tags", IsExpanded = true };
            for (int index = 0; index < 30; index++)
            {
                Tags.Items.Add(new TreeViewItem { Header = $"tag-{index:00}" });
            }

            Stashes = new TreeViewItem { Header = "Stashes", Items = { new TreeViewItem { Header = "stash-one" } } };
            Tree = new TreeView
            {
                Classes = { "gitextensions-native-tree", "gitextensions-explorer-tree" },
                Items = { Branches, Tags, Stashes }
            };
            _ = new NativeTreeKeyboardAdapter(Tree, getTextInputTimestamp);
            NativeTreeViewExplorerNavigationDecorator decorator = new(Tree, () => new DateTime(2026, 10, 3, 12, 0, 0));
            decorator.AfterSelect += (_, _) => Activations++;
            Window = new Window { Width = 260, Height = 160, Content = Tree };
            Window.Show();
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
        }

        internal TreeView Tree { get; }

        internal Window Window { get; }

        internal TreeViewItem Branches { get; }

        internal TreeViewItem Nested { get; }

        internal TreeViewItem DeepLeaf { get; }

        internal TreeViewItem Main { get; }

        internal TreeViewItem Tags { get; }

        internal TreeViewItem Stashes { get; }

        internal int Activations { get; private set; }

        internal void ResetActivations()
            => Activations = 0;

        internal void Press(Key key, KeyModifiers modifiers = KeyModifiers.None)
        {
            KeyEventArgs args = new() { RoutedEvent = InputElement.KeyDownEvent, Key = key, KeyModifiers = modifiers };
            Tree.RaiseEvent(args);
            args.Handled.Should().BeTrue();
        }

        internal void Type(string text)
        {
            TextInputEventArgs args = new() { RoutedEvent = InputElement.TextInputEvent, Text = text };
            Tree.RaiseEvent(args);
            args.Handled.Should().BeTrue();
        }

        public void Dispose()
            => Window.Close();
    }
}
