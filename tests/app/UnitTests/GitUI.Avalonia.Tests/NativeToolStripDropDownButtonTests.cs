using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitUI;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using Size = Avalonia.Size;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class NativeToolStripDropDownButtonTests
{
    [AvaloniaTest]
    [TestCase(false, 22)]
    [TestCase(true, 22)]
    [TestCase(false, 24)]
    [TestCase(true, 24)]
    public void Image_only_dropdown_should_keep_native_image_and_arrow_allocations_without_Fluent_squeeze(bool rightToLeft, int height)
    {
        IconDropDownButton button = NewButton(rightToLeft);
        button.Height = height;
        Window window = NewWindow(button);
        try
        {
            window.Show();
            Settle(window);
            button.UseNativeToolStripLayout.Should().BeTrue();
            button.Bounds.Size.Should().Be(new Size(29, height));
            Image image = button.GetVisualDescendants().OfType<Image>().Single();
            int x = rightToLeft ? 11 : 2;
            int y = (height - 16) / 2;
            image.TranslatePoint(default, button).Should().Be(new Point(x, y));
            image.Bounds.Size.Should().Be(new Size(16, 16));
            button.GetVisualDescendants().OfType<PathIcon>().Should().BeEmpty("the native filled arrow must not retain Fluent's oversized hollow glyph column");
            button.Content.Should().Be("Settings", "DisplayStyle.Image keeps its original translation text");
            using WriteableBitmap frame = Capture(window);
            ReadPixel(frame, x, y).Should().Be(Colors.Magenta);
            ReadPixel(frame, x + 15, y + 15).Should().Be(Colors.Magenta);
            int arrowLeft = rightToLeft ? 2 : 20;
            AssertNativeArrow(frame, arrowLeft, (height / 2) - 1, Colors.Black, Colors.White);
            window.MouseMove(new Point(10, height / 2));
            Settle(window);
            image.TranslatePoint(default, button).Should().Be(new Point(x, y), "hover border is paint, not an inset into source image layout");
            image.Bounds.Size.Should().Be(new Size(16, 16));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void FileStatusList_settings_should_use_source_owner_paint_and_native_image_bounds_with_actual_popup_option_hover()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        JoinableTaskContext? previousContext = ThreadHelper.HasJoinableTaskContext ? ThreadHelper.JoinableTaskContext : null;
        using JoinableTaskContext context = new(Thread.CurrentThread, SynchronizationContext.Current);
        ThreadHelper.JoinableTaskContext = context;
        Window window = new() { Width = 900, Height = 300 };
        try
        {
            FileStatusList control = new();
            window.Content = control;
            window.Show();
            Settle(window);
            IconDropDownButton button = control.FindControl<IconDropDownButton>("btnSettings")
                ?? throw new AssertionException("The original settings owner is missing.");
            AssertSourceConsumer(window, button);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
            ThreadHelper.JoinableTaskContext = previousContext!;
        }
    }

    internal static void AssertSourceConsumer(Window window, IconDropDownButton button)
    {
        NativeToolStrip.GetFreezeDropDownOwnerPaint(button).Should().BeTrue();
        button.UseNativeToolStripLayout.Should().BeTrue();
        button.Bounds.Width.Should().Be(29);
        Image image = button.GetVisualDescendants().OfType<Image>().Single();
        Point imageOrigin = image.TranslatePoint(default, button)!.Value;
        image.Bounds.Size.Should().Be(new Size(16, 16));
        imageOrigin.Should().Be(new Point(2, ((int)button.Bounds.Height - 16) / 2));
        MenuFlyout menu = button.Flyout as MenuFlyout ?? throw new AssertionException("The original menu is missing.");
        Point ownerPoint = button.TranslatePoint(new Rect(button.Bounds.Size).Center, window)!.Value;
        window.MouseMove(ownerPoint);
        Settle(window);
        Border chrome = button.GetVisualDescendants().OfType<Border>().Single(part => part.Name == "RootBorder");
        IBrush? background = chrome.Background;
        IBrush? border = chrome.BorderBrush;
        Point ownerOrigin = button.TranslatePoint(default, window)!.Value;
        using (WriteableBitmap frame = Capture(window))
        {
            int arrowLeft = (int)Math.Round(ownerOrigin.X) + (int)button.Bounds.Width - 9;
            int arrowTop = (int)Math.Round(ownerOrigin.Y) + ((int)button.Bounds.Height / 2) - 1;
            Color foreground = (button.Foreground as ISolidColorBrush)?.Color
                ?? throw new AssertionException("The native foreground role is not a solid brush.");
            AssertNativeArrow(frame, arrowLeft, arrowTop, foreground, ReadPixel(frame, arrowLeft - 1, arrowTop));
        }

        try
        {
            window.MouseDown(ownerPoint, MouseButton.Left);
            window.MouseUp(ownerPoint, MouseButton.Left);
            Settle(window);
            menu.IsOpen.Should().BeTrue();
            MenuItem option = menu.Items.OfType<MenuItem>().First(item => item.IsEnabled && item.IsVisible);
            TopLevel popup = TopLevel.GetTopLevel(option) ?? throw new AssertionException("The original option is not in an actual popup.");
            window.MouseMove(new Point(window.ClientSize.Width - 10, window.ClientSize.Height - 10));
            popup.MouseMove(option.TranslatePoint(new Rect(option.Bounds.Size).Center, popup)!.Value);
            Settle(window);
            button.IsPointerOver.Should().BeFalse();
            option.IsPointerOver.Should().BeTrue();
            chrome.Background.Should().BeSameAs(background);
            chrome.BorderBrush.Should().BeSameAs(border);
            image.Bounds.Size.Should().Be(new Size(16, 16));
            image.TranslatePoint(default, button).Should().Be(imageOrigin);
            menu.Hide();
            Settle(window);
            chrome.Background.Should().NotBeSameAs(background);
        }
        finally
        {
            menu.Hide();
        }
    }

    internal static void AssertNativeArrow(WriteableBitmap bitmap, int left, int top, Color foreground, Color background)
    {
        for (int row = 0; row < 3; row++)
        {
            for (int column = -1; column <= 5; column++)
            {
                ReadPixel(bitmap, left + column, top + row).Should().Be(
                    column >= row && column < 5 - row ? foreground : background,
                    "the actual native Down arrow has full5/3/1 rows without antialias fringe");
            }
        }
    }

    [AvaloniaTest]
    public void Native_image_only_route_should_not_replace_standalone_or_text_bearing_dropdowns()
    {
        IconDropDownButton standalone = NewButton(false);
        standalone.Classes.Remove("gitextensions-toolbar-button");
        IconDropDownButton textBearing = NewButton(false);
        textBearing.Classes.Remove("gitextensions-icon-only");
        Window window = NewWindow(new StackPanel { Children = { standalone, textBearing } });
        try
        {
            window.Show();
            Settle(window);
            standalone.UseNativeToolStripLayout.Should().BeFalse();
            textBearing.UseNativeToolStripLayout.Should().BeFalse();
            standalone.GetVisualDescendants().OfType<PathIcon>().Should().ContainSingle();
            textBearing.GetVisualDescendants().OfType<PathIcon>().Should().ContainSingle();
            standalone.GetVisualDescendants().OfType<NativeToolStripDropDownButtonContent>().Should().BeEmpty();
            textBearing.GetVisualDescendants().OfType<NativeToolStripDropDownButtonContent>().Should().BeEmpty();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Source_ToolStripEx_dropdown_should_keep_preopening_hover_while_pointer_enters_menu_and_refresh_after_close(bool rightToLeft)
    {
        using NativeToolStrip strip = new();
        IconDropDownButton button = NewButton(rightToLeft);
        button.Margin = new Thickness(0, 1, 0, 2);
        MenuItem option = new() { Header = "Source option" };
        MenuFlyout menu = new() { Items = { option } };
        button.Flyout = menu;
        strip.Items.Add(button);
        Window window = NewWindow(strip);
        try
        {
            window.Show();
            Settle(window);
            NativeToolStrip.GetFreezeDropDownOwnerPaint(button).Should().BeTrue();
            Point point = button.TranslatePoint(new Rect(button.Bounds.Size).Center, window)!.Value;
            window.MouseMove(point);
            Settle(window);
            Border chrome = button.GetVisualDescendants().OfType<Border>().Single(part => part.Name == "RootBorder");
            IBrush? hoveredBackground = chrome.Background;
            IBrush? hoveredBorder = chrome.BorderBrush;
            Image image = button.GetVisualDescendants().OfType<Image>().Single();
            Rect imageBounds = image.Bounds;
            window.MouseDown(point, MouseButton.Left);
            Settle(window);
            menu.IsOpen.Should().BeTrue("ToolStripDropDownButton opens on MouseDown");
            window.MouseUp(point, MouseButton.Left);
            Settle(window);
            menu.IsOpen.Should().BeTrue("the opening release must not run a second toggle");
            window.MouseMove(new Point(170, 100));
            TopLevel popup = TopLevel.GetTopLevel(option) ?? throw new InvalidOperationException("The actual menu option is detached.");
            popup.MouseMove(option.TranslatePoint(new Rect(option.Bounds.Size).Center, popup)!.Value);
            Settle(window);
            button.IsPointerOver.Should().BeFalse();
            option.IsPointerOver.Should().BeTrue();
            chrome.Background.Should().BeSameAs(hoveredBackground);
            chrome.BorderBrush.Should().BeSameAs(hoveredBorder);
            image.Bounds.Should().Be(imageBounds);
            menu.Hide();
            Settle(window);
            chrome.Background.Should().NotBeSameAs(hoveredBackground);
            window.MouseMove(point);
            window.MouseDown(point, MouseButton.Left);
            window.MouseUp(point, MouseButton.Left);
            Settle(window);
            menu.IsOpen.Should().BeTrue();
            chrome.Background.Should().BeSameAs(hoveredBackground);
        }
        finally
        {
            menu.Hide();
            window.Close();
        }
    }

    private static IconDropDownButton NewButton(bool rightToLeft)
        => new()
        {
            Classes = { "gitextensions-toolbar-button", "gitextensions-icon-only" },
            Height = 22,
            MinHeight = 0,
            Margin = default,
            Content = "Settings",
            Foreground = Brushes.Black,
            FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            Icon = new DrawingImage
            {
                Drawing = new GeometryDrawing
                {
                    Brush = Brushes.Magenta,
                    Geometry = new RectangleGeometry(new Rect(0, 0, 16, 16)),
                },
            },
        };

    private static Window NewWindow(Control content)
        => new() { Width = 180, Height = 120, Content = new Canvas { Background = Brushes.White, Children = { content } } };

    private static void Settle(Window window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }

    private static WriteableBitmap Capture(Window window)
        => window.CaptureRenderedFrame() ?? throw new InvalidOperationException("The toolbar frame is unavailable.");

    private static Color ReadPixel(WriteableBitmap bitmap, int x, int y)
    {
        using ILockedFramebuffer buffer = bitmap.Lock();
        byte[] pixel = new byte[4];
        Marshal.Copy(IntPtr.Add(buffer.Address, (y * buffer.RowBytes) + (x * 4)), pixel, 0, pixel.Length);
        return buffer.Format == PixelFormat.Bgra8888
            ? Color.FromArgb(pixel[3], pixel[2], pixel[1], pixel[0])
            : Color.FromArgb(pixel[3], pixel[0], pixel[1], pixel[2]);
    }
}
