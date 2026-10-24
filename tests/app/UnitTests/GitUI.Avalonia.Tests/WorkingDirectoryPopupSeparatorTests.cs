using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
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
public sealed class WorkingDirectoryPopupSeparatorTests
{
    [AvaloniaTest]
    [TestCase(false, false, false)]
    [TestCase(false, false, true)]
    [TestCase(true, false, false)]
    [TestCase(true, false, true)]
    [TestCase(true, true, false)]
    [TestCase(true, true, true)]
    public void Working_directory_separators_should_paint_the_source_renderer_without_a_false_background_or_etched_pair(
        bool dark, bool custom, bool rightToLeft)
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        WorkingDirectoryToolStripSplitButton selector = new()
        {
            FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
        };
        WorkingDirectoryToolStripSplitButton.TestAccessor accessor = selector.GetTestAccessor();
        Repository favourite = new(Path.Combine(Path.GetTempPath(), "GitExtensions.PopupSeparatorFavourite")) { Category = "Separator probe" };
        Repository recent = new(Path.Combine(Path.GetTempPath(), "GitExtensions.PopupSeparatorRecent"));
        accessor.PrepareDropDown([favourite], [recent]);
        Window window = new()
        {
            Width = 1000,
            Height = 500,
            Content = selector,
            RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light,
        };
        if (custom)
        {
            window.Resources["GitExtensionsControlBorderBrush"] = Brushes.Magenta;
            window.Resources["GitExtensionsControlBackgroundBrush"] = Brushes.Cyan;
        }

        try
        {
            window.Show();
            window.MouseMove(new Point(-1, -1));
            Dispatcher.UIThread.RunJobs();
            accessor.Menu.ShowAt(selector);
            Dispatcher.UIThread.RunJobs();
            MenuFlyoutPresenter presenter = accessor.Menu.Popup.Child as MenuFlyoutPresenter
                ?? throw new AssertionException("The actual working-directory selector must create its retained popup presenter.");
            NativeToolStripDropDownSeparator[] separators = accessor.Menu.Items.OfType<NativeToolStripDropDownSeparator>().ToArray();
            separators.Length.Should().BeGreaterThan(1);
            Color background = Color.Parse(dark ? "#FF2F2F2F" : "#FFF0F0F0");
            Color line = Color.Parse(dark ? "#FF404040" : "#FFBDBDBD");
            foreach (NativeToolStripDropDownSeparator separator in separators)
            {
                NativeToolStripDropDownLayout.Group group = NativeToolStripDropDownLayout.GetGroup(separator)
                    ?? throw new AssertionException("The real retained source menu must supply each separator's native padding and width.");
                group.RightToLeft.Should().Be(rightToLeft);
                separator.UseSystemVisualStyle.Should().Be(!dark);
                separator.Bounds.Height.Should().Be(6);
                separator.Bounds.X.Should().Be(2);
                separator.Bounds.Width.Should().Be(group.PopupWidth - 4);
                separator.Focusable.Should().BeFalse();
                separator.IsHitTestVisible.Should().BeFalse();
                window.TryFindResource("GitExtensionsControlBackgroundBrush", window.ActualThemeVariant, out object? rawBackground).Should().BeTrue();
                separator.Background.Should().BeSameAs(rawBackground,
                    "the native raw BackColor remains separate from the renderer's popup body and separator pens");
                AssertSeparatorPixels(separator, background, line);
                separator.IsEnabled = false;
                Dispatcher.UIThread.RunJobs();
                AssertSeparatorPixels(separator, background, line);
                separator.IsEnabled = true;
            }

            AssertBorderPixels(presenter, Color.Parse(dark ? "#FF6B6B6B" : "#FFA0A0A0"));
        }
        finally
        {
            accessor.Menu.Hide();
            window.Close();
        }
    }

    private static void AssertSeparatorPixels(NativeToolStripDropDownSeparator separator, Color background, Color dark)
    {
        NativeToolStripDropDownLayout.Group group = NativeToolStripDropDownLayout.GetGroup(separator)
            ?? throw new AssertionException("The source menu separator must retain its actual group.");
        using SKBitmap pixels = Capture(separator, out PixelPoint origin);
        int width = (int)separator.Bounds.Width;
        int first = group.RightToLeft ? 2 : (int)group.Padding.Left - 2;
        int last = group.RightToLeft ? group.PopupWidth - (int)group.Padding.Right - 1 : group.PopupWidth - 1;
        for (int y = 0; y < (int)separator.Bounds.Height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color expected = background;
                if (separator.UseSystemVisualStyle && x >= 2 && x < width - 2)
                {
                    expected = y == 2 ? Color.Parse("#FF848484")
                        : y == 3 ? Color.Parse("#FFF5F5F5") : background;
                }
                else if (!separator.UseSystemVisualStyle && y == 3 && x >= first && x <= last)
                {
                    expected = dark;
                }

                pixels.GetPixel(origin.X + x, origin.Y + y).Should().Be(ToSkia(expected),
                    $"the actual {separator.FlowDirection} popup separator must retain its source renderer at {x},{y}");
            }
        }
    }

    private static void AssertBorderPixels(MenuFlyoutPresenter presenter, Color border)
    {
        using SKBitmap pixels = Capture(presenter, out PixelPoint origin);
        int width = (int)presenter.Bounds.Width;
        int height = (int)presenter.Bounds.Height;
        for (int x = 0; x < width; x++)
        {
            pixels.GetPixel(origin.X + x, origin.Y).Should().Be(ToSkia(border));
            pixels.GetPixel(origin.X + x, origin.Y + height - 1).Should().Be(ToSkia(border));
        }

        for (int y = 0; y < height; y++)
        {
            pixels.GetPixel(origin.X, origin.Y + y).Should().Be(ToSkia(border));
            pixels.GetPixel(origin.X + width - 1, origin.Y + y).Should().Be(ToSkia(border));
        }
    }

    private static SKBitmap Capture(Control control, out PixelPoint origin)
    {
        TopLevel root = TopLevel.GetTopLevel(control)
            ?? throw new AssertionException("The opened separator must belong to a rendered popup root.");
        Point position = control.TranslatePoint(default, root)
            ?? throw new AssertionException("The actual menu separator must have a popup-relative origin.");
        origin = new PixelPoint((int)position.X, (int)position.Y);
        using WriteableBitmap frame = root.CaptureRenderedFrame()
            ?? throw new AssertionException("The opened working-directory popup must render a real frame.");
        using MemoryStream stream = new();
        frame.Save(stream, PngBitmapEncoderOptions.Default);
        stream.Position = 0;
        return SKBitmap.Decode(stream) ?? throw new AssertionException("The actual popup capture must decode.");
    }

    private static SKColor ToSkia(Color color) => new(color.R, color.G, color.B, color.A);
}
