using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
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
        Grid content = new();
        Window form = new() { Content = content };

        form.Mask();
        form.Mask();

        LoadingControl mask = content.Children.Should().ContainSingle().Which.Should().BeOfType<LoadingControl>().Which;
        mask.IsAnimating.Should().BeTrue();
        mask.HorizontalAlignment.Should().Be(Avalonia.Layout.HorizontalAlignment.Stretch);
        mask.VerticalAlignment.Should().Be(Avalonia.Layout.VerticalAlignment.Stretch);
        SolidColorBrush brush = mask.Background.Should().BeOfType<SolidColorBrush>().Which;
        brush.Color.Should().Be(AvaloniaThemeResources.ToMediaColor(
            AvaloniaThemeResources.ResolveSystemColor(ThemeModule.Settings, System.Drawing.KnownColor.AppWorkspace)));
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
