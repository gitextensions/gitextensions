using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitUI.Compat;
using GitUI.UserControls;

namespace GitExtensionsTests;

[TestFixture]
[Category("P8.6i.126")]
public sealed class NativeTreeViewDecoratorTests
{
    [AvaloniaTest]
    [TestCase(Key.Up)]
    [TestCase(Key.Down)]
    [TestCase(Key.Left)]
    [TestCase(Key.Right)]
    public void Arrow_navigation_should_highlight_without_activating_a_node(Key key)
    {
        DateTime now = new(2026, 10, 1, 12, 0, 0);
        TreeViewItem first = new() { Header = "first" };
        TreeViewItem second = new() { Header = "second" };
        TreeView tree = new() { Items = { first, second } };
        NativeTreeViewExplorerNavigationDecorator decorator = new(tree, () => now);
        int activations = 0;
        decorator.AfterSelect += (_, _) => activations++;

        tree.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = key });
        tree.SelectedItem = second;

        tree.SelectedItem.Should().BeSameAs(second);
        activations.Should().Be(0);
        now = now.AddMilliseconds(500);
        tree.SelectedItem = first;
        activations.Should().Be(1, "the source navigation guard ends at 500 milliseconds");
    }

    [AvaloniaTest]
    [TestCase(Key.Space)]
    [TestCase(Key.Enter)]
    public void Explicit_activation_should_keep_the_highlight_and_activate_once(Key key)
    {
        TreeViewItem item = new() { Header = "branch" };
        TreeView tree = new() { Items = { item } };
        NativeTreeViewExplorerNavigationDecorator decorator = new(tree);
        tree.SelectedItem = item;
        int activations = 0;
        decorator.AfterSelect += (_, _) => activations++;

        KeyEventArgs args = new() { RoutedEvent = InputElement.KeyDownEvent, Key = key };
        tree.RaiseEvent(args);

        tree.SelectedItem.Should().BeSameAs(item);
        activations.Should().Be(1);
        args.Handled.Should().BeTrue();
    }

    [AvaloniaTest]
    public void Mouse_click_should_activate_a_keyboard_highlighted_node_without_changing_selection()
    {
        DateTime now = new(2026, 10, 1, 12, 0, 0);
        TreeViewItem item = new() { Header = "branch" };
        TreeView tree = new() { Items = { item } };
        NativeTreeViewExplorerNavigationDecorator decorator = new(tree, () => now);
        Window window = new() { Width = 280, Height = 150, Content = tree };
        int activations = 0;
        decorator.AfterSelect += (_, _) => activations++;
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            tree.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Down });
            tree.SelectedItem = item;
            activations.Should().Be(0);
            now = now.AddMilliseconds(500);
            Point position = item.TranslatePoint(new Point(60, 8), window)!.Value;
            window.MouseDown(position, MouseButton.Left);
            window.MouseUp(position, MouseButton.Left);

            activations.Should().Be(1);
            tree.SelectedItem.Should().BeSameAs(item);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Release_over_another_node_should_not_reselect_it()
    {
        TreeViewItem first = new() { Header = "first" };
        TreeViewItem second = new() { Header = "second" };
        TreeView tree = new() { Items = { first, second } };
        NativeTreeViewExplorerNavigationDecorator decorator = new(tree);
        Window window = new() { Width = 280, Height = 160, Content = tree };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            ClickHeader(window, first);
            int activations = 0;
            decorator.AfterSelect += (_, _) => activations++;
            Point press = GetHeaderPosition(window, first);
            Point release = GetHeaderPosition(window, second);

            window.MouseDown(press, MouseButton.Left);
            window.MouseMove(release, RawInputModifiers.LeftMouseButton);
            window.MouseUp(release, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            tree.SelectedItem.Should().BeSameAs(first);
            activations.Should().Be(0);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Expander_click_should_preserve_the_source_node_reselection()
    {
        TreeViewItem parent = new() { Header = "parent", Items = { new TreeViewItem { Header = "child" } } };
        TreeViewItem other = new() { Header = "other" };
        TreeView tree = new() { Items = { parent, other } };
        NativeTreeViewExplorerNavigationDecorator decorator = new(tree);
        Window window = new() { Width = 280, Height = 180, Content = tree };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            ClickHeader(window, other);
            tree.SelectedItem.Should().BeSameAs(other);
            int activations = 0;
            decorator.AfterSelect += (_, _) => activations++;
            Avalonia.Controls.Primitives.ToggleButton expander = parent.GetVisualDescendants()
                .OfType<Avalonia.Controls.Primitives.ToggleButton>().First();
            Point position = expander.TranslatePoint(new Point(expander.Bounds.Width / 2, expander.Bounds.Height / 2), window)!.Value;

            window.MouseDown(position, MouseButton.Left);
            window.MouseUp(position, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            parent.IsExpanded.Should().BeTrue();
            tree.SelectedItem.Should().BeSameAs(parent);
            activations.Should().Be(1);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Pointer_reselection_should_obey_the_original_navigation_guard()
    {
        DateTime now = new(2026, 10, 1, 12, 0, 0);
        TreeViewItem first = new() { Header = "first" };
        TreeViewItem second = new() { Header = "second" };
        TreeView tree = new() { Items = { first, second } };
        NativeTreeViewExplorerNavigationDecorator decorator = new(tree, () => now);
        Window window = new() { Width = 280, Height = 160, Content = tree };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            ClickHeader(window, first);
            tree.SelectedItem.Should().BeSameAs(first);
            int activations = 0;
            decorator.AfterSelect += (_, _) => activations++;

            window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, keySymbol: null);
            window.KeyRelease(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, keySymbol: null);
            Dispatcher.UIThread.RunJobs();
            tree.SelectedItem.Should().BeSameAs(second);
            activations.Should().Be(0);

            ClickHeader(window, second);
            activations.Should().Be(0, "the original mouse reselection passes through the same 500 ms AfterSelect guard");
            now = now.AddMilliseconds(500);
            ClickHeader(window, second);

            activations.Should().Be(1);
            tree.SelectedItem.Should().BeSameAs(second);
            tree.IsKeyboardFocusWithin.Should().BeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Inner_node_double_tap_should_expose_the_source_cancellation_boundary(bool cancel)
    {
        TreeViewItem item = new NativeTreeViewItem { Header = "parent", Items = { new TreeViewItem { Header = "child" } } };
        TreeView tree = new() { Items = { item } };
        NativeTreeViewDoubleClickDecorator decorator = new(tree);
        int notifications = 0;
        decorator.BeforeDoubleClickExpandCollapse += (_, e) =>
        {
            notifications++;
            e.Cancel = cancel;
        };
        Window window = new() { Width = 280, Height = 180, Content = tree };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Point header = GetHeaderPosition(window, item);
            window.MouseDown(header, MouseButton.Left);
            window.MouseUp(header, MouseButton.Left);
            window.MouseDown(header, MouseButton.Left);
            window.MouseUp(header, MouseButton.Left);

            notifications.Should().Be(1);
            item.IsExpanded.Should().Be(!cancel);
            Avalonia.Controls.Primitives.ToggleButton expander = item.GetVisualDescendants()
                .OfType<Avalonia.Controls.Primitives.ToggleButton>().First();
            Point toggle = expander.TranslatePoint(new Point(expander.Bounds.Width / 2, expander.Bounds.Height / 2), window)!.Value;
            window.MouseDown(toggle, MouseButton.Left);
            window.MouseUp(toggle, MouseButton.Left);
            notifications.Should().Be(1, "expansion-toggle taps must not open submodules or other node actions");
            item.IsExpanded.Should().Be(cancel, "the plus/minus control must still toggle the node");
        }
        finally
        {
            window.Close();
        }
    }

    private static void ClickHeader(Window window, TreeViewItem item)
    {
        Point position = GetHeaderPosition(window, item);
        window.MouseDown(position, MouseButton.Left);
        window.MouseUp(position, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private static Point GetHeaderPosition(Window window, TreeViewItem item)
    {
        TextBlock label = item.GetVisualDescendants().OfType<TextBlock>().First(text => text.Text == item.Header?.ToString());
        return label.TranslatePoint(new Point(label.Bounds.Width / 2, label.Bounds.Height / 2), window)!.Value;
    }
}
