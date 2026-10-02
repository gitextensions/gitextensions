using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitExtensions.ParityCapture;
using GitExtUtils.GitUI.Theming;
using GitUI.Compat;
using GitUI.LeftPanel;
using GitUI.Theming;
using DrawingColor = System.Drawing.Color;
using KnownColor = System.Drawing.KnownColor;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class NativeTreePaintTests
{
    [Test]
    [Category("P8.6i.126")]
    [TestCase(false, false, "#FFFFFF", "#CCE8FF", "#0078D4")]
    [TestCase(false, true, "#FFFFFF", "#D9D9D9", "#949494")]
    [TestCase(false, false, "#AAD282", "#88C49B", "#0078D4")]
    [TestCase(false, true, "#AAD282", "#91B36F", "#637A4B")]
    [TestCase(true, false, "#2B2D3A", "#626262", "#60CDFF")]
    [TestCase(true, true, "#2B2D3A", "#333333", "#383943")]
    [TestCase(true, true, "#646464", "#333333", "#646464")]
    public void Explorer_image_roles_should_composite_native_samples_over_the_source_backdrop(
        bool dark, bool inactive, string backdrop, string background, string border)
    {
        NativeTreePaintPalette palette = NativeTreePaintPalette.Resolve(dark, Color.Parse(backdrop), inactive);
        palette.Background.Should().Be(Color.Parse(background));
        palette.Border.Should().Be(Color.Parse(border));
        palette.Foreground.Should().Be(dark ? Colors.White : Colors.Black);
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(12, "Branches", false)]
    [TestCase(12, "main", true)]
    [TestCase(12, "Wörk & branches", true)]
    [TestCase(44d / 3, "Branches", false)]
    [TestCase(44d / 3, "main", true)]
    [TestCase(44d / 3, "Wörk & branches", true)]
    public void Explorer_label_extents_should_use_the_ambient_control_font_not_the_node_font(double fontSize, string caption, bool styled)
    {
        NativeTreeTextBlock text = new()
        {
            Text = caption,
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = fontSize,
            FontWeight = styled ? FontWeight.Bold : FontWeight.Normal,
            FontStyle = styled ? FontStyle.Italic : FontStyle.Normal,
        };
        TreeView tree = new()
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = fontSize,
            FontWeight = FontWeight.Normal,
            FontStyle = FontStyle.Normal,
            Items = { new TreeViewItem { Header = text } },
        };
        Window window = new() { Width = 360, Height = 160, Content = tree };
        try
        {
            window.Show();
            window.UpdateLayout();
            text.DesiredSize.Width.Should().Be(GetNativeLabelWidth(tree, caption));
            text.Padding.Should().Be(new Thickness(2, 0));
            text.StyleKey.Should().Be(typeof(TextBlock), "native labels retain the original gray/deleted-node TextBlock selectors");
            text.FontWeight.Should().Be(styled ? FontWeight.Bold : FontWeight.Normal);
            text.FontStyle.Should().Be(styled ? FontStyle.Italic : FontStyle.Normal);
            tree.FontSize = 24;
            window.UpdateLayout();
            text.DesiredSize.Width.Should().Be(GetNativeLabelWidth(tree, caption));
            text.FontSize.Should().Be(fontSize, "the node's authored paint font remains distinct from the tree's sizing font");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Explorer_label_font_tracking_should_follow_its_current_tree_after_reparenting()
    {
        NativeTreeTextBlock text = new() { Text = "Branches" };
        TreeViewItem item = new() { Header = text };
        TreeView first = new() { FontSize = 12, Items = { item } };
        TreeView second = new() { FontSize = 24 };
        Window window = new() { Width = 360, Height = 160, Content = first };
        try
        {
            window.Show();
            window.UpdateLayout();
            text.DesiredSize.Width.Should().Be(GetNativeLabelWidth(first, text.Text));
            first.Items.Remove(item);
            second.Items.Add(item);
            window.Content = second;
            window.UpdateLayout();
            text.DesiredSize.Width.Should().Be(GetNativeLabelWidth(second, text.Text));
            first.FontSize = 48;
            second.FontSize = 16;
            window.UpdateLayout();
            text.DesiredSize.Width.Should().Be(GetNativeLabelWidth(second, text.Text));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Explorer_selection_should_refresh_custom_backdrop_without_acquiring_a_hover_accent()
    {
        Application application = Application.Current!;
        bool hadLight = application.Resources.ThemeDictionaries.TryGetValue(ThemeVariant.Light, out IThemeVariantProvider? originalLight);
        ThemeSettings originalSettings = ThemeModule.Settings;
        application.Resources.ThemeDictionaries[ThemeVariant.Light] = new ResourceDictionary();
        RepoObjectsTree control = new();
        control.SetRefs([]);
        TreeView tree = control.GetTestAccessor().Tree;
        TreeViewItem item = tree.Items.Cast<TreeViewItem>().First();
        tree.SelectedItem = item;
        Window window = new() { Width = 360, Height = 560, RequestedThemeVariant = ThemeVariant.Light, Content = control };
        try
        {
            AvaloniaThemeResources.Apply(application, ThemeSettings.Default);
            window.Show();
            window.UpdateLayout();
            tree.Focus().Should().BeTrue();
            Border selection = item.GetVisualDescendants().OfType<Border>()
                .Single(border => border.Name == "PART_NativeSelectionBorder" && border.FindAncestorOfType<TreeViewItem>() == item);
            ((ISolidColorBrush)selection.Background!).Color.Should().Be(Color.Parse("#CCE8FF"));
            Theme theme = new(
                new Dictionary<AppColor, DrawingColor> { [AppColor.PanelBackground] = DrawingColor.FromArgb(170, 210, 130) },
                new Dictionary<KnownColor, DrawingColor> { [KnownColor.Highlight] = DrawingColor.Purple },
                new ThemeId("native-tree-custom-light"));
            AvaloniaThemeResources.Apply(application, new ThemeSettings(theme, Theme.Default, ThemeVariations.None, useSystemVisualStyle: true));
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            ((ISolidColorBrush)tree.Background!).Color.Should().Be(Color.Parse("#AAD282"));
            ((ISolidColorBrush)selection.Background!).Color.Should().Be(Color.Parse("#88C49B"));
            Avalonia.Point origin = selection.TranslatePoint(default, window)!.Value;
            window.MouseMove(origin + new Avalonia.Vector(10, 5));
            Dispatcher.UIThread.RunJobs();
            ((ISolidColorBrush)selection.Background!).Color.Should().Be(Color.Parse("#88C49B"));
            ((ISolidColorBrush)selection.BorderBrush!).Color.Should().Be(Color.Parse("#0078D4"));
            tree.SelectedItem.Should().BeSameAs(item);
            AvaloniaThemeResources.Apply(application, ThemeSettings.Default);
            Dispatcher.UIThread.RunJobs();
            ((ISolidColorBrush)selection.Background!).Color.Should().Be(Color.Parse("#CCE8FF"));
        }
        finally
        {
            window.Close();
            if (hadLight)
            {
                application.Resources.ThemeDictionaries[ThemeVariant.Light] = originalLight!;
            }
            else
            {
                application.Resources.ThemeDictionaries.Remove(ThemeVariant.Light);
            }

            ColorHelper.ThemeSettings = originalSettings;
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(false, false, true)]
    [TestCase(false, true, true)]
    [TestCase(true, false, true)]
    [TestCase(true, true, true)]
    [TestCase(false, false, false)]
    [TestCase(true, false, false)]
    public void Explorer_selection_should_use_native_paint_roles_and_hide_when_focus_leaves(bool dark, bool focused, bool hideSelection)
    {
        ThemeVariant variant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
        RepoObjectsTree control = new();
        control.SetRefs([]);
        TreeView tree = control.GetTestAccessor().Tree;
        if (!hideSelection)
        {
            tree.Classes.Remove("gitextensions-hide-tree-selection");
        }

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
            else if (hideSelection)
            {
                ((ISolidColorBrush)selection.Background!).Color.A.Should().Be(0);
                ((ISolidColorBrush)selection.BorderBrush!).Color.A.Should().Be(0);
                tree.SelectedItem.Should().BeSameAs(item, "HideSelection changes paint, not the selected node");
            }
            else
            {
                Color backdrop = ((ISolidColorBrush)tree.Background!).Color;
                NativeTreePaintPalette inactive = NativeTreePaintPalette.Resolve(dark, backdrop, inactive: true);
                ((ISolidColorBrush)selection.Background!).Color.Should().Be(inactive.Background);
                ((ISolidColorBrush)selection.BorderBrush!).Color.Should().Be(inactive.Border);
                ((ISolidColorBrush)presenter.Foreground!).Color.Should().Be(inactive.Foreground);
                tree.SelectedItem.Should().BeSameAs(item);
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

    private static double GetNativeLabelWidth(TreeView tree, string caption)
    {
        TextBlock ambient = new()
        {
            FontFamily = tree.FontFamily,
            FontSize = tree.FontSize,
            FontWeight = tree.FontWeight,
            FontStyle = tree.FontStyle,
        };
        return (WinFormsRichEditTextMeasurer.TryGetTextWidth(ambient, caption, out double width)
            ? width
            : WinFormsTextMeasurer.MeasureSize(ambient, caption).Width) + 4;
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
