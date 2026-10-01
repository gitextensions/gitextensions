using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitUI.UserControls;
using NSubstitute;

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
        TreeViewItem item = new() { Header = "branch" };
        TreeView tree = new() { Items = { item } };
        NativeTreeViewExplorerNavigationDecorator decorator = new(tree);
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
    [TestCase(false)]
    [TestCase(true)]
    public void Inner_node_double_tap_should_expose_the_source_cancellation_boundary(bool cancel)
    {
        TreeViewItem item = new() { Header = "parent", Items = { new TreeViewItem { Header = "child" } } };
        TreeView tree = new() { Items = { item } };
        NativeTreeViewDoubleClickDecorator decorator = new(tree);
        int notifications = 0;
        decorator.BeforeDoubleClickExpandCollapse += (_, e) =>
        {
            notifications++;
            e.Cancel = cancel;
        };
        Window window = new() { Content = tree };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            TappedEventArgs args = CreateDoubleTap(item);
            item.RaiseEvent(args);

            notifications.Should().Be(1);
            item.IsExpanded.Should().Be(!cancel);
            Avalonia.Controls.Primitives.ToggleButton expander = item.GetVisualDescendants()
                .OfType<Avalonia.Controls.Primitives.ToggleButton>().First();
            expander.RaiseEvent(CreateDoubleTap(expander));
            notifications.Should().Be(1, "expansion-toggle taps must not open submodules or other node actions");
        }
        finally
        {
            window.Close();
        }
    }

    private static TappedEventArgs CreateDoubleTap(Control source)
        => new(InputElement.DoubleTappedEvent, new PointerEventArgs(
            null, source, Substitute.For<IPointer>(), null, default, 0, default, KeyModifiers.None))
        {
            Source = source,
        };
}
