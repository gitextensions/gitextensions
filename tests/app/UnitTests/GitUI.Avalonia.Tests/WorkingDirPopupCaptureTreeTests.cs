using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.ParityCapture;
using GitUI;
using GitUI.CommandsDialogs.Menus;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class WorkingDirPopupCaptureTreeTests
{
    [SetUp]
    public void SetUp() => ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public void Native_owned_filter_should_report_its_actual_current_popup_parent_geometry(bool rightToLeft, bool overflow)
    {
        using NativeToolStrip strip = new()
        {
            Name = "ToolStripMain",
            Width = overflow ? 50 : 400,
            Height = 27,
        };
        WorkingDirectoryToolStripSplitButton selector = new()
        {
            Name = "_NO_TRANSLATE_WorkingDir",
            UseNativeToolStripLayout = true,
            Width = 140,
        };
        WorkingDirectoryToolStripSplitButton.TestAccessor accessor = selector.GetTestAccessor();
        accessor.PrepareDropDown([], []);
        strip.Items.Add(selector);
        Window window = new()
        {
            Width = 800,
            Height = 300,
            FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            Content = new Canvas { Children = { strip } },
        };
        try
        {
            window.Show();
            Settle(window);
            strip.GetItemPlacement(selector).Should().Be(overflow
                ? NativeToolStripItemPlacement.Overflow
                : NativeToolStripItemPlacement.Main);
            if (overflow)
            {
                strip.ShowOverflow();
                Settle(window);
            }

            selector.ShowDropDown();
            Settle(window);
            accessor.Menu.IsOpen.Should().BeTrue();
            MenuFlyoutPresenter popup = accessor.Filter.GetVisualAncestors()
                .OfType<MenuFlyoutPresenter>().First();
            AssertActualGeometry(window, selector, accessor.Filter, popup);

            // An authored diagnostic inset must remain measurable; the capture must not
            // replace it with native-looking coordinates or conceal the live margin debt.
            accessor.Filter.Margin = new Thickness(7, 3, 5, 4);
            Settle(window);
            popup.UpdateLayout();
            Settle(window);
            AssertActualGeometry(window, selector, accessor.Filter, popup);
        }
        finally
        {
            accessor.Menu.Hide();
            strip.CloseOverflow();
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Generic_non_native_hosted_menu_projection_should_remain_unchanged()
    {
        TextBox filter = new() { Width = 100 };
        MenuFlyout flyout = new()
        {
            Items = { new MenuItem { Header = filter, Focusable = false } },
        };
        DropDownButton owner = new() { Content = "Generic hosted menu", Flyout = flyout };
        Window window = new() { Width = 400, Height = 200, Content = owner };
        try
        {
            window.Show();
            Settle(window);
            flyout.ShowAt(owner);
            Settle(window);
            MenuFlyoutPresenter popup = filter.GetVisualAncestors().OfType<MenuFlyoutPresenter>().First();
            CaptureNode node = new AvaloniaControlTreeReader(window, renderScale: 1)
                .ReadSurface(popup, "popup:0", new PixelRect(0, 0, (int)popup.Bounds.Width, (int)popup.Bounds.Height))
                .Root.Children.Single(child => child.Type == "System.Windows.Forms.ToolStripTextBox");

            node.BoundsDip.X.Should().Be(34);
            node.BoundsDip.Y.Should().Be(3);
            node.BoundsDip.Width.Should().Be(Round(filter.Bounds.Width));
            node.BoundsDip.Height.Should().Be(Round(filter.Bounds.Height));
        }
        finally
        {
            flyout.Hide();
            window.Close();
        }
    }

    private static void AssertActualGeometry(
        Window window,
        WorkingDirectoryToolStripSplitButton selector,
        TextBox filter,
        MenuFlyoutPresenter popup)
    {
        Point origin = filter.TranslatePoint(default, popup)
            ?? throw new InvalidOperationException("The actual hosted input must share its rendered popup parent.");
        AvaloniaControlTreeReader reader = new(window, renderScale: 1);
        CaptureNode primary = reader.ReadPrimary(window,
            new PixelSize((int)window.Bounds.Width, (int)window.Bounds.Height)).Root;
        CaptureNode logicalOwner = Flatten(primary).Single(node => node.Name == selector.Name);
        CaptureNode primaryFilter = logicalOwner.Children.Single(
            node => node.Type == "System.Windows.Forms.ToolStripTextBox");
        CaptureNode popupFilter = reader.ReadSurface(popup, "popup:0",
            new PixelRect(0, 0, (int)popup.Bounds.Width, (int)popup.Bounds.Height))
            .Root.Children.Single(node => node.Type == "System.Windows.Forms.ToolStripTextBox");
        foreach (CaptureNode node in new[] { primaryFilter, popupFilter })
        {
            node.BoundsDip.X.Should().Be(Round(origin.X));
            node.BoundsDip.Y.Should().Be(Round(origin.Y));
            node.BoundsDip.Width.Should().Be(Round(filter.Bounds.Width));
            node.BoundsDip.Height.Should().Be(Round(filter.Bounds.Height));
            node.Margin.Dip.Left.Should().Be(Round(filter.Margin.Left));
            node.Margin.Dip.Top.Should().Be(Round(filter.Margin.Top));
            node.Margin.Dip.Right.Should().Be(Round(filter.Margin.Right));
            node.Margin.Dip.Bottom.Should().Be(Round(filter.Margin.Bottom));
        }
    }

    private static decimal Round(double value) => decimal.Round((decimal)value, 4);

    private static IEnumerable<CaptureNode> Flatten(CaptureNode node)
    {
        yield return node;
        foreach (CaptureNode child in node.Children)
        {
            foreach (CaptureNode descendant in Flatten(child))
            {
                yield return descendant;
            }
        }
    }

    private static void Settle(Window window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }
}
