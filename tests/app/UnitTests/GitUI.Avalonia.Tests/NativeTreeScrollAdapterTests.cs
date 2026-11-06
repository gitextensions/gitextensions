using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI.Compat;
using GitUI.LeftPanel;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class NativeTreeScrollAdapterTests
{
    [AvaloniaTest]
    [TestCase(1, 0, KeyModifiers.None, 3, -45)]
    [TestCase(-1, 0, KeyModifiers.None, 3, 45)]
    [TestCase(0, 1, KeyModifiers.Shift, 3, -45)]
    [TestCase(0, -1, KeyModifiers.Shift | KeyModifiers.Control, 3, 45)]
    [TestCase(-0.01, 0, KeyModifiers.None, 3, 5)]
    [TestCase(-1, 0, KeyModifiers.None, 0, 5)]
    [TestCase(0, 0, KeyModifiers.Shift, 3, 0)]
    public void Horizontal_wheel_should_use_native_line_units_and_preserve_vertical_offset(
        double x, double y, KeyModifiers modifiers, int lines, double expected)
    {
        TreeView tree = CreateScrollableTree();
        NativeTreeScrollAdapter adapter = new(tree, () => lines);
        Window window = new() { Width = 240, Height = 140, Content = tree };
        try
        {
            window.Show();
            window.UpdateLayout();
            ScrollViewer scroll = GetScroll(tree);
            scroll.Offset = new Vector(300, 50);
            double vertical = scroll.Offset.Y;

            adapter.ScrollHorizontally(new Vector(x, y), modifiers).Should().BeTrue();

            scroll.Offset.X.Should().Be(300 + expected);
            scroll.Offset.Y.Should().Be(vertical);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(-1)]
    [TestCase(-0.01)]
    public void Page_wheel_should_use_one_native_viewport_minus_line_step(double delta)
    {
        TreeView tree = CreateScrollableTree();
        NativeTreeScrollAdapter adapter = new(tree, () => -1);
        Window window = new() { Width = 240, Height = 140, Content = tree };
        try
        {
            window.Show();
            window.UpdateLayout();
            ScrollViewer scroll = GetScroll(tree);
            adapter.ScrollHorizontally(new Vector(delta, 0), KeyModifiers.None).Should().BeTrue();
            scroll.Offset.X.Should().Be(scroll.Viewport.Width - 5);
            scroll.Offset.Y.Should().Be(0);
            adapter.ScrollHorizontally(new Vector(100, 0), KeyModifiers.None).Should().BeTrue();
            scroll.Offset.X.Should().Be(0, "the source page direction is applied once, even for a larger delta");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Ordinary_vertical_wheel_should_remain_on_the_framework_route_and_horizontal_limits_should_clamp()
    {
        TreeView tree = CreateScrollableTree();
        NativeTreeScrollAdapter adapter = new(tree, () => 3);
        Window window = new() { Width = 240, Height = 140, Content = tree };
        try
        {
            window.Show();
            window.UpdateLayout();
            ScrollViewer scroll = GetScroll(tree);
            adapter.ScrollHorizontally(new Vector(0, 1), KeyModifiers.None).Should().BeFalse();
            adapter.ScrollHorizontally(new Vector(-1000, 0), KeyModifiers.None).Should().BeTrue();
            scroll.Offset.X.Should().Be(scroll.Extent.Width - scroll.Viewport.Width);
            adapter.ScrollHorizontally(new Vector(1000, 0), KeyModifiers.None).Should().BeTrue();
            scroll.Offset.X.Should().Be(0);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Non_Windows_wheel_preference_should_use_the_documented_three_line_substitute()
    {
        if (!OperatingSystem.IsWindows())
        {
            NativeTreeScrollAdapter.GetWheelScrollLines().Should().Be(3);
        }
        else
        {
            NativeTreeScrollAdapter.GetWheelScrollLines().Should().BeGreaterThanOrEqualTo(-1);
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Ref_selection_and_search_should_reset_horizontal_scroll_after_revealing_the_target(bool search)
    {
        bool showBranches = AppSettings.RepoObjectsTreeShowBranches;
        RepoObjectsTree control = new();
        Window window = new() { Width = 240, Height = 140, Content = control };
        try
        {
            AppSettings.RepoObjectsTreeShowBranches = true;
            string target = "long-path/" + new string('W', 100) + "/target";
            IGitModule module = Substitute.For<IGitModule>();
            control.SetRefs([new GitRef(module, ObjectId.Random(), "refs/heads/" + target)], [], string.Empty);
            window.Show();
            window.UpdateLayout();
            RepoObjectsTree.TestAccessor accessor = control.GetTestAccessor();
            control.SelectGitRef(target);
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            ScrollViewer scroll = GetScroll(accessor.Tree);
            scroll.Extent.Width.Should().BeGreaterThan(scroll.Viewport.Width);
            scroll.Offset = new Vector(100, scroll.Offset.Y);
            if (search)
            {
                accessor.SearchBox.Text = "target";
                accessor.Search();
            }
            else
            {
                control.SelectGitRef(target);
            }

            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            scroll.Offset.X.Should().Be(0);
            ((NodeBase)((TreeViewItem)accessor.Tree.SelectedItem!).Tag!).SearchText.Should().Be(target);
        }
        finally
        {
            window.Close();
            AppSettings.RepoObjectsTreeShowBranches = showBranches;
        }
    }

    private static TreeView CreateScrollableTree()
    {
        TreeView tree = new()
        {
            Classes = { "gitextensions-native-tree" },
        };
        ScrollViewer.SetHorizontalScrollBarVisibility(tree, Avalonia.Controls.Primitives.ScrollBarVisibility.Auto);
        ScrollViewer.SetVerticalScrollBarVisibility(tree, Avalonia.Controls.Primitives.ScrollBarVisibility.Auto);
        for (int index = 0; index < 50; index++)
        {
            tree.Items.Add(new TreeViewItem { Header = new string('W', 200) });
        }

        return tree;
    }

    private static ScrollViewer GetScroll(TreeView tree)
        => tree.GetVisualDescendants().OfType<ScrollViewer>().First();
}
