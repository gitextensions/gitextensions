using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Threading;
using GitUI;
using GitUI.Compat;
using GitUI.Theming;
using GitUI.UserControls.RevisionGrid;

namespace GitExtensionsTests;

[TestFixture]
public sealed class GitUIExtensionsTests
{
    [AvaloniaTest]
    public void Mask_should_add_one_full_surface_loading_control_with_the_resolved_workspace_color()
    {
        Grid content = new()
        {
            RowDefinitions = RowDefinitions.Parse("40,*"),
            ColumnDefinitions = ColumnDefinitions.Parse("80,*"),
        };
        Window form = new()
        {
            Content = content,
            Height = 240,
            Width = 320,
        };
        form.Show();

        form.Mask();
        form.Mask();
        Dispatcher.UIThread.RunJobs();

        LoadingControl mask = content.Children.Should().ContainSingle().Which.Should().BeOfType<LoadingControl>().Which;
        mask.IsAnimating.Should().BeTrue();
        mask.HorizontalAlignment.Should().Be(Avalonia.Layout.HorizontalAlignment.Stretch);
        mask.VerticalAlignment.Should().Be(Avalonia.Layout.VerticalAlignment.Stretch);
        Grid.GetRowSpan(mask).Should().Be(content.RowDefinitions.Count);
        Grid.GetColumnSpan(mask).Should().Be(content.ColumnDefinitions.Count);
        mask.Bounds.Should().Be(content.Bounds);
        SolidColorBrush brush = mask.Background.Should().BeOfType<SolidColorBrush>().Which;
        brush.Color.Should().Be(AvaloniaThemeResources.ToMediaColor(
            AvaloniaThemeResources.ResolveSystemColor(ThemeModule.Settings, System.Drawing.KnownColor.AppWorkspace)));

        form.Close();
    }

    [AvaloniaTest]
    public void UnMask_should_stop_and_remove_the_loading_control()
    {
        Grid content = new();
        Window form = new() { Content = content };
        form.Mask();
        LoadingControl mask = content.Children.OfType<LoadingControl>().Single();

        form.UnMask();
        form.UnMask();

        mask.IsAnimating.Should().BeFalse();
        content.Children.Should().BeEmpty();
    }
}
