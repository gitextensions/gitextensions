using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using GitExtensions.ParityCapture;
using GitUI.LeftPanel;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class NativeTreePaintTests
{
    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void Explorer_selection_should_use_native_paint_roles_and_hide_when_focus_leaves(bool dark, bool focused)
    {
        ThemeVariant variant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
        RepoObjectsTree control = new();
        control.SetRefs([]);
        TreeView tree = control.GetTestAccessor().Tree;
        TreeViewItem item = tree.Items.Cast<TreeViewItem>().First();
        tree.SelectedItem = item;
        Button other = new() { Content = "Other" };
        Window window = new() { Width = 360, Height = 560, RequestedThemeVariant = variant, Content = new StackPanel { Children = { control, other } } };
        try
        {
            window.Show();
            window.UpdateLayout();
            (focused ? (InputElement)tree : other).Focus().Should().BeTrue();
            window.UpdateLayout();
            Border selection = item.GetVisualDescendants().OfType<Border>()
                .Single(border => border.Name == "PART_NativeSelectionBorder" && border.FindAncestorOfType<TreeViewItem>() == item);
            ContentPresenter presenter = item.GetVisualDescendants().OfType<ContentPresenter>()
                .Single(presenter => presenter.Name == "PART_HeaderPresenter" && presenter.FindAncestorOfType<TreeViewItem>() == item);
            Grid header = item.GetVisualDescendants().OfType<Grid>()
                .Single(grid => grid.Name == "PART_NativeHeaderContent" && grid.FindAncestorOfType<TreeViewItem>() == item);
            selection.Bounds.Height.Should().Be(18);
            selection.BorderThickness.Should().Be(new Thickness(1));
            ((ISolidColorBrush)presenter.Background!).Color.A.Should().Be(0);
            header.Background.Should().BeNull();
            if (focused)
            {
                ((ISolidColorBrush)selection.Background!).Color.Should().Be(Color.Parse(dark ? "#626262" : "#CCE8FF"));
                ((ISolidColorBrush)selection.BorderBrush!).Color.Should().Be(Color.Parse(dark ? "#60CDFF" : "#0078D4"));
                ((ISolidColorBrush)presenter.Foreground!).Color.Should().Be(dark ? Colors.White : Colors.Black);
            }
            else
            {
                ((ISolidColorBrush)selection.Background!).Color.A.Should().Be(0);
                ((ISolidColorBrush)selection.BorderBrush!).Color.A.Should().Be(0);
                tree.SelectedItem.Should().BeSameAs(item, "HideSelection changes paint, not the selected node");
            }

            Image icon = presenter.GetVisualDescendants().OfType<Image>().Single();
            TextBlock text = presenter.GetVisualDescendants().OfType<TextBlock>().Single();
            icon.TranslatePoint(default, item)!.Value.X.Should().Be(22);
            text.TranslatePoint(default, item)!.Value.X.Should().Be(41);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Explorer_capture_colors_should_report_rendered_roles_not_system_Highlight()
    {
        RepoObjectsTree control = new();
        Window window = new() { Width = 360, Height = 560, RequestedThemeVariant = ThemeVariant.Light, Content = control };
        try
        {
            window.Show();
            window.UpdateLayout();
            CaptureNode root = new AvaloniaControlTreeReader(control, 1)
                .ReadPrimary(control, PixelSize.FromSize(control.Bounds.Size, 1)).Root;
            CaptureNode tree = Flatten(root).Single(node => node.Name == "treeMain");
            tree.Colors.SelectionBackground.Should().Be("#FFCCE8FF");
            tree.Colors.SelectionForeground.Should().Be("#FF000000");
            tree.Colors.Additional["selectionBorder"].Should().Be("#FF0078D4");
            tree.Colors.InactiveSelectionBackground.Should().Be("#00FFFFFF");
            tree.Colors.Additional["inactiveSelectionBorder"].Should().Be("#00FFFFFF");
        }
        finally
        {
            window.Close();
        }
    }

    private static IEnumerable<CaptureNode> Flatten(CaptureNode node)
    {
        yield return node;
        foreach (CaptureNode child in node.Children.SelectMany(Flatten))
        {
            yield return child;
        }
    }
}
