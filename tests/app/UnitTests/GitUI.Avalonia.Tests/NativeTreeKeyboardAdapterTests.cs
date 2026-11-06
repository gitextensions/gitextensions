using System.Reflection;
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
using GitUI;
using GitUI.Compat;
using GitUI.LeftPanel;
using GitUI.UserControls;
using Microsoft.VisualStudio.Threading;
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
    [TestCase(Key.Home)]
    [TestCase(Key.End)]
    [TestCase(Key.PageUp)]
    [TestCase(Key.PageDown)]
    [TestCase(Key.Left)]
    [TestCase(Key.Right)]
    public void Ctrl_navigation_should_preserve_the_caret_focus_and_opposite_scroll_axis(Key key)
    {
        using TreeFixture fixture = new();
        fixture.Tree.SelectedItem = fixture.Tags.Items.OfType<TreeViewItem>().ElementAt(15);
        TreeViewItem caret = (TreeViewItem)fixture.Tree.SelectedItem!;
        caret.Focus().Should().BeTrue();
        fixture.Window.UpdateLayout();
        ScrollViewer scroll = GetScroll(fixture.Tree);
        double maximumX = Math.Max(0, scroll.Extent.Width - scroll.Viewport.Width);
        maximumX.Should().BeGreaterThan(20);
        scroll.Offset = new Vector(maximumX / 2, 200);
        double oldX = scroll.Offset.X;
        double oldY = scroll.Offset.Y;
        double rowHeight = caret.GetVisualDescendants().OfType<Border>()
            .First(border => border.Name == "PART_LayoutRoot").Bounds.Height;
        int pageStep = Math.Max(1, (int)Math.Floor(scroll.Viewport.Height / rowHeight) - 1);
        double maximumY = Math.Max(0, scroll.Extent.Height - scroll.Viewport.Height);
        fixture.ResetActivations();

        fixture.Press(key, KeyModifiers.Control);

        double expectedY = key switch
        {
            Key.Home => 0,
            Key.End => maximumY,
            Key.PageUp => Math.Max(0, oldY - (pageStep * rowHeight)),
            Key.PageDown => Math.Min(maximumY, oldY + (pageStep * rowHeight)),
            _ => oldY,
        };
        double expectedX = oldX + (key == Key.Left ? -5 : key == Key.Right ? 5 : 0);
        scroll.Offset.Should().Be(new Vector(expectedX, expectedY));
        fixture.Tree.SelectedItem.Should().BeSameAs(caret);
        caret.IsFocused.Should().BeTrue();
        fixture.Tree.SelectedItems.Cast<object>().Should().ContainSingle();
        fixture.Activations.Should().Be(0);
    }

    [AvaloniaTest]
    public void Backspace_should_move_to_the_parent_without_collapsing_it_and_activate_outside_the_arrow_interval()
    {
        using TreeFixture fixture = new();
        fixture.Tree.SelectedItem = fixture.DeepLeaf;
        fixture.ResetActivations();
        ScrollViewer scroll = GetScroll(fixture.Tree);
        scroll.Offset = new Vector(150, scroll.Offset.Y);
        double oldX = scroll.Offset.X;

        fixture.Press(Key.Back);
        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.Nested);
        fixture.Nested.IsExpanded.Should().BeTrue();
        fixture.Press(Key.Back);
        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.Feature);
        fixture.Feature.IsExpanded.Should().BeTrue();
        fixture.Press(Key.Back);
        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.Branches);
        fixture.Press(Key.Back);
        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.Branches);
        scroll.Offset.X.Should().Be(oldX);
        fixture.Activations.Should().Be(3);
    }

    [AvaloniaTest]
    [TestCase(Key.Down, "maintenance", 0)]
    [TestCase(Key.Home, "Branches", 1)]
    [TestCase(Key.End, "Stashes", 1)]
    public void Shift_navigation_should_still_use_one_caret_not_range_selection(Key key, string caption, int activations)
    {
        using TreeFixture fixture = new();
        fixture.Tree.SelectedItem = fixture.Main;
        fixture.ResetActivations();

        fixture.Press(key, KeyModifiers.Shift);

        ((TreeViewItem)fixture.Tree.SelectedItem!).Header.Should().Be(caption);
        fixture.Tree.SelectedItems.Cast<object>().Should().ContainSingle();
        fixture.Activations.Should().Be(activations);
    }

    [AvaloniaTest]
    [TestCase(KeyModifiers.Alt)]
    [TestCase(KeyModifiers.Meta)]
    public void Native_keyboard_adapter_should_leave_platform_shortcut_modifiers_to_their_owner(KeyModifiers modifier)
    {
        TreeViewItem item = new() { Header = "caret" };
        TreeView tree = new() { Items = { item }, SelectedItem = item };
        NativeTreeKeyboardAdapter adapter = new(tree);
        KeyEventArgs args = new() { Key = Key.Home, KeyModifiers = modifier };
        MethodInfo handler = typeof(NativeTreeKeyboardAdapter).GetMethod("OnKeyDown", BindingFlags.Instance | BindingFlags.NonPublic)!;

        handler.Invoke(adapter, [tree, args]);

        args.Handled.Should().BeFalse("this scoped adapter must not hijack Alt or platform Meta shortcuts");
        tree.SelectedItem.Should().BeSameAs(item);
    }

    [AvaloniaTest]
    public void Ctrl_navigation_should_clamp_scroll_limits_and_leave_an_offscreen_caret_alone()
    {
        using TreeFixture fixture = new();
        fixture.Tree.SelectedItem = fixture.Tags.Items.OfType<TreeViewItem>().ElementAt(15);
        fixture.Window.UpdateLayout();
        ScrollViewer scroll = GetScroll(fixture.Tree);
        TreeViewItem caret = (TreeViewItem)fixture.Tree.SelectedItem!;
        fixture.ResetActivations();
        scroll.Offset = default;

        fixture.Press(Key.Left, KeyModifiers.Control);
        fixture.Press(Key.Up, KeyModifiers.Control);
        fixture.Press(Key.PageUp, KeyModifiers.Control);
        scroll.Offset.Should().Be(default(Vector));
        fixture.Press(Key.End, KeyModifiers.Control);
        double bottom = scroll.Offset.Y;
        fixture.Press(Key.Down, KeyModifiers.Control);
        fixture.Press(Key.PageDown, KeyModifiers.Control);
        scroll.Offset.Y.Should().Be(bottom);
        scroll.Offset = new Vector(scroll.Extent.Width - scroll.Viewport.Width, bottom);
        double right = scroll.Offset.X;
        fixture.Press(Key.Right, KeyModifiers.Control);
        scroll.Offset.X.Should().Be(right);
        fixture.Tree.SelectedItem.Should().BeSameAs(caret);
        fixture.Activations.Should().Be(0);
    }

    [AvaloniaTest]
    public void Mixed_case_prefixes_should_not_be_normalized_into_repeated_character_cycles_and_should_follow_live_captions()
    {
        using TreeFixture fixture = new();
        fixture.Press(Key.Home);
        fixture.Type("M");
        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.Main);
        fixture.Type("m");
        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.Main);
        fixture.Type("a");
        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.Main, "literal native Mma is not a repeated-initial fallback");
        fixture.Press(Key.Home);
        fixture.Type("mé");
        ((TreeViewItem)fixture.Tree.SelectedItem!).Header.Should().Be("mémoire");
        fixture.Main.Header = "master";
        fixture.Press(Key.Home);
        fixture.Type("mas");
        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.Main);
    }

    [AvaloniaTest]
    [TestCase(true)]
    [TestCase(false)]
    public void Collapsing_a_caret_ancestor_should_retain_that_ancestor_and_the_existing_container_focus(bool focused)
    {
        using TreeFixture fixture = new();
        fixture.Tree.SelectedItem = fixture.DeepLeaf;
        fixture.DeepLeaf.Focus().Should().BeTrue();
        Button other = new() { Content = "Other" };
        DockPanel panel = new() { Children = { other } };
        fixture.Window.Content = panel;
        panel.Children.Add(fixture.Tree);
        fixture.Window.UpdateLayout();
        if (focused)
        {
            fixture.DeepLeaf.Focus().Should().BeTrue();
        }
        else
        {
            other.Focus().Should().BeTrue();
        }

        fixture.Feature.IsExpanded = false;

        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.Feature);
        fixture.Tree.SelectedItems.Cast<object>().Should().ContainSingle();
        fixture.Tree.IsKeyboardFocusWithin.Should().Be(focused);
        if (!focused)
        {
            other.IsFocused.Should().BeTrue();
        }
    }

    [AvaloniaTest]
    public void Removing_the_selected_leaf_or_first_root_should_retain_the_native_parent_or_next_root_caret()
    {
        using TreeFixture fixture = new();
        fixture.Tree.SelectedItem = fixture.DeepLeaf;
        fixture.DeepLeaf.Focus().Should().BeTrue();

        fixture.Nested.Items.Remove(fixture.DeepLeaf);
        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.Nested);
        fixture.Tree.SelectedItem = fixture.Main;
        fixture.Tree.Items.Remove(fixture.Branches);
        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.Tags);
        fixture.Tree.SelectedItems.Cast<object>().Should().ContainSingle();
        fixture.Tree.IsKeyboardFocusWithin.Should().BeTrue();

        fixture.Feature.IsExpanded = false;
        fixture.Branches.Items.Clear();
        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.Tags, "removed nodes no longer own this tree's collection handlers");
        fixture.Tree.Items.Clear();
        fixture.Tree.SelectedItem.Should().BeNull();
        fixture.Tree.SelectedItems.Cast<object>().Should().BeEmpty();
        TreeViewItem replacement = new() { Header = "replacement", Items = { new TreeViewItem { Header = "replacement-leaf" } }, IsExpanded = true };
        fixture.Tree.Items.Add(replacement);
        fixture.Tree.SelectedItem = replacement.Items[0];
        replacement.IsExpanded = false;
        fixture.Tree.SelectedItem.Should().BeSameAs(replacement, "dynamic additions must acquire the same collapse/caret contract");
    }

    [AvaloniaTest]
    [TestCase("middleChild")]
    [TestCase("lastChild")]
    [TestCase("lastRoot")]
    [TestCase("onlyRoot")]
    public void Removing_a_caret_should_prefer_adjacent_siblings_and_clear_the_only_root(string removal)
    {
        using TreeFixture fixture = new();
        TreeViewItem selected;
        TreeViewItem? expected;
        ItemsControl owner;
        switch (removal)
        {
            case "middleChild":
                selected = (TreeViewItem)fixture.Tags.Items[15]!;
                expected = (TreeViewItem)fixture.Tags.Items[16]!;
                owner = fixture.Tags;
                break;
            case "lastChild":
                selected = (TreeViewItem)fixture.Tags.Items[^1]!;
                expected = (TreeViewItem)fixture.Tags.Items[^2]!;
                owner = fixture.Tags;
                break;
            case "lastRoot":
                selected = fixture.Stashes;
                expected = fixture.Tags;
                owner = fixture.Tree;
                break;
            default:
                fixture.Tree.Items.Remove(fixture.Branches);
                fixture.Tree.Items.Remove(fixture.Stashes);
                selected = fixture.Tags;
                expected = null;
                owner = fixture.Tree;
                break;
        }

        fixture.Tree.SelectedItem = selected;
        selected.Focus().Should().BeTrue();

        owner.Items.Remove(selected);

        fixture.Tree.SelectedItem.Should().BeSameAs(expected);
        if (expected is not null)
        {
            fixture.Tree.SelectedItems.Cast<object>().Should().ContainSingle();
            fixture.Tree.IsKeyboardFocusWithin.Should().BeTrue();
        }
        else
        {
            fixture.Tree.SelectedItems.Cast<object>().Should().BeEmpty();
        }
    }

    [AvaloniaTest]
    public void Owner_reconstruction_should_restore_its_caret_before_ordinary_deletion_fallback_resumes()
    {
        bool updating = false;
        using TreeFixture fixture = new(isUpdating: () => updating);
        fixture.Tree.SelectedItem = fixture.Main;
        TreeViewItem replacement = new() { Header = "main" };
        List<TreeViewItem> transientCarets = [];
        fixture.Tree.PropertyChanged += (_, args) =>
        {
            if (updating && args.Property == TreeView.SelectedItemProperty
                && fixture.Tree.SelectedItem is TreeViewItem selected)
            {
                transientCarets.Add(selected);
            }
        };

        updating = true;
        try
        {
            fixture.Branches.Items.Clear();
            fixture.Branches.Items.Add(replacement);
            transientCarets.Should().NotContain(fixture.Branches, "the owner will restore the source same-name caret after reconstruction");
            fixture.Tree.SelectedItem = replacement;
        }
        finally
        {
            updating = false;
        }

        fixture.Tree.SelectedItem.Should().BeSameAs(replacement);
        fixture.Tree.SelectedItems.Cast<object>().Should().ContainSingle().Which.Should().BeSameAs(replacement);
        fixture.Window.UpdateLayout();
        replacement.Focus().Should().BeTrue();

        fixture.Branches.Items.Remove(replacement);

        fixture.Tree.SelectedItem.Should().BeSameAs(fixture.Branches, "normal native deletion fallback resumes after the owner's update ends");
        fixture.Tree.SelectedItems.Cast<object>().Should().ContainSingle().Which.Should().BeSameAs(fixture.Branches);
    }

    [AvaloniaTest]
    [TestCase(true)]
    [TestCase(false)]
    public void Owner_reconstruction_without_the_caret_identity_should_not_activate_an_unrelated_collection_mutation(bool addItem)
    {
        bool updating = false;
        using TreeFixture fixture = new(isUpdating: () => updating);
        fixture.Tree.SelectedItem = fixture.Main;
        fixture.Main.Focus().Should().BeTrue();

        updating = true;
        try
        {
            fixture.Branches.Items.Clear();
            fixture.Branches.Items.Add(new TreeViewItem { Header = "replacement" });
        }
        finally
        {
            updating = false;
        }

        fixture.Tree.SelectedItem.Should().BeNull();
        fixture.Tree.SelectedItems.Cast<object>().Should().BeEmpty();
        fixture.ResetActivations();

        if (addItem)
        {
            fixture.Tags.Items.Add(new TreeViewItem { Header = "unrelated-tag" });
        }
        else
        {
            fixture.Tags.Items.RemoveAt(0);
        }

        fixture.Tree.SelectedItem.Should().BeNull("the owner's removed identity is not a pending deletion in another collection");
        fixture.Tree.SelectedItems.Cast<object>().Should().BeEmpty();
        fixture.Activations.Should().Be(0);
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
        JoinableTaskContext? previousContext = ThreadHelper.HasJoinableTaskContext ? ThreadHelper.JoinableTaskContext : null;
        using JoinableTaskContext context = new();
        ThreadHelper.JoinableTaskContext = context;
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
            ThreadHelper.JoinableTaskContext = previousContext!;
        }
    }

    private static ScrollViewer GetScroll(TreeView tree)
        => tree.GetVisualDescendants().OfType<ScrollViewer>().First(scroll => scroll.FindAncestorOfType<TreeView>() == tree);

    private sealed class TreeFixture : IDisposable
    {
        private readonly DateTime _decoratorTime = new(2026, 10, 3, 12, 0, 0);

        internal TreeFixture(Func<long>? getTextInputTimestamp = null, Func<bool>? isUpdating = null)
        {
            DeepLeaf = new TreeViewItem { Header = "deep-leaf" };
            Nested = new TreeViewItem { Header = "nested", IsExpanded = true, Items = { DeepLeaf } };
            Feature = new TreeViewItem { Header = "feature", IsExpanded = true, Items = { Nested } };
            Main = new TreeViewItem { Header = "main" };
            Branches = new TreeViewItem
            {
                Header = "Branches", IsExpanded = true,
                Items =
                {
                    new TreeViewItem { Header = "alpha" }, Feature, Main, new TreeViewItem { Header = "maintenance" },
                    new TreeViewItem { Header = "mémoire" }, new TreeViewItem { Header = "wide-" + new string('W', 80) }
                }
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
            ScrollViewer.SetHorizontalScrollBarVisibility(Tree, Avalonia.Controls.Primitives.ScrollBarVisibility.Auto);
            _ = new NativeTreeKeyboardAdapter(Tree, getTextInputTimestamp, isUpdating);
            NativeTreeViewExplorerNavigationDecorator decorator = new(Tree, () => _decoratorTime);
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

        internal TreeViewItem Feature { get; }

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
