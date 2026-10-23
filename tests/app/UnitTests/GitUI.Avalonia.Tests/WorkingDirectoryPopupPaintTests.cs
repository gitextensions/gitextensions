using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands.UserRepositoryHistory;
using GitUI;
using GitUI.CommandsDialogs.Menus;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using SkiaSharp;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class WorkingDirectoryPopupPaintTests
{
    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void Working_directory_popup_should_render_the_source_System_or_Professional_surface_not_its_raw_BackColor(
        bool dark, bool custom)
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        WorkingDirectoryToolStripSplitButton selector = new();
        WorkingDirectoryToolStripSplitButton.TestAccessor accessor = selector.GetTestAccessor();
        Repository favouriteRepository = new(Path.Combine(Path.GetTempPath(), "GitExtensions.WorkingDirectoryPaint")) { Category = "Paint probe" };
        accessor.PrepareDropDown([favouriteRepository], []);
        Window window = new()
        {
            Width = 1000,
            Height = 400,
            Content = selector,
            RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light,
        };
        if (custom)
        {
            window.Resources["GitExtensionsMenuBackgroundBrush"] = Brushes.Magenta;
            window.Resources["GitExtensionsControlBorderBrush"] = Brushes.Cyan;
        }

        try
        {
            window.Show();
            window.MouseMove(new Point(-1, -1));
            Dispatcher.UIThread.RunJobs();
            accessor.Menu.ShowAt(selector);
            Dispatcher.UIThread.RunJobs();
            MenuFlyoutPresenter presenter = accessor.Menu.Popup.Child as MenuFlyoutPresenter
                ?? throw new AssertionException("The actual retained working-directory popup must have its native-shaped presenter.");
            Color expectedBackground = Color.Parse(dark ? "#FF2F2F2F" : "#FFF0F0F0");
            Color expectedBorder = Color.Parse(dark ? "#FF6B6B6B" : "#FFA0A0A0");
            ColorOf(presenter.Background).Should().Be(expectedBackground);
            ColorOf(presenter.BorderBrush).Should().Be(expectedBorder);
            NativeToolStripDropDownMenuItem.GetUseSystemVisualStyle(presenter).Should().Be(!dark);
            MenuItem last = accessor.Menu.Items.OfType<MenuItem>().Last();
            Border row = last.GetVisualDescendants().OfType<Border>().Single(part => part.Name == "PART_LayoutRoot");
            ColorOf(row.Background).Should().Be(expectedBackground,
                "ordinary native rows use the same renderer surface, not the unrelated generic near-white menu body");

            AssertSurfacePixels(presenter, expectedBackground, expectedBorder);

            MenuItem favourite = accessor.Menu.Items.OfType<MenuItem>().Single(item => item.Items.Count > 0);
            favourite.IsSubMenuOpen = true;
            Dispatcher.UIThread.RunJobs();
            Popup childPopup = favourite.GetVisualDescendants().OfType<Popup>().Single(part => part.Name == "PART_Popup");
            Panel child = childPopup.Child as Panel
                ?? throw new AssertionException("The original favourite row must expose its real owned submenu canvas.");
            ColorOf(child.Background).Should().Be(expectedBackground);
            Border childBorder = child.Children.OfType<Border>().Single(part => part.Name == "PART_DropDownBorder");
            ColorOf(childBorder.BorderBrush).Should().Be(expectedBorder);
            AssertSurfacePixels(child, expectedBackground, expectedBorder);
            favourite.IsSubMenuOpen = false;
        }
        finally
        {
            accessor.Menu.Hide();
            window.Close();
        }
    }

    private static void AssertSurfacePixels(Control canvas, Color expectedBackground, Color expectedBorder)
    {
        TopLevel popupRoot = TopLevel.GetTopLevel(canvas)
            ?? throw new AssertionException("The opened source menu must belong to a rendered popup root.");
        Point origin = canvas.TranslatePoint(default, popupRoot)
            ?? throw new AssertionException("The real menu canvas must have a popup-relative pixel position.");
        using WriteableBitmap frame = popupRoot.CaptureRenderedFrame()
            ?? throw new AssertionException("The actual opened working-directory menu must render a frame.");
        using MemoryStream stream = new();
        frame.Save(stream, PngBitmapEncoderOptions.Default);
        stream.Position = 0;
        using SKBitmap pixels = SKBitmap.Decode(stream)
            ?? throw new AssertionException("The retained popup frame must decode.");
        int x = (int)origin.X;
        int y = (int)origin.Y;
        pixels.GetPixel(x, y).Should().Be(ToSkia(expectedBorder));
        pixels.GetPixel(x + (int)canvas.Bounds.Width - 2, y + (int)canvas.Bounds.Height - 2)
            .Should().Be(ToSkia(expectedBackground),
                "the source native96 captures independently prove the border and a blank point inside the final command row");
    }

    private static Color ColorOf(IBrush? brush) => brush.Should().BeAssignableTo<ISolidColorBrush>().Which.Color;

    private static SKColor ToSkia(Color color) => new(color.R, color.G, color.B, color.A);
}
