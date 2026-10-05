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
using GitUI.Compat;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class NativeToolStripGripTests
{
    [AvaloniaTest]
    [TestCase(false, false, 25)]
    [TestCase(true, false, 25)]
    [TestCase(false, true, 29)]
    [TestCase(true, true, 29)]
    public void System_grip_should_tile_actual_native_REBAR_composite_with_source_integer_height(bool rightToLeft, bool white, int height)
    {
        Color backdrop = white ? Colors.White : Colors.Black;
        using NativeToolStrip strip = new()
        {
            Width = 40,
            Height = height,
            UseSystemVisualStyle = true,
            Background = new SolidColorBrush(backdrop),
            FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
        };
        Window window = new() { Width = 60, Height = 50, Content = new Canvas { Children = { strip } } };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            using WriteableBitmap frame = window.CaptureRenderedFrame() ?? throw new AssertionException("The actual grip frame is unavailable.");
            int renderedHeight = ((height - 2) / 4) * 4;
            int top = Math.Max(0, (height - renderedHeight - 2) / 2);
            int left = rightToLeft ? 35 : 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    Color expected = backdrop;
                    if (y >= top && y < top + renderedHeight)
                    {
                        int row = (y - top) % 4;
                        if ((row == 0 && (x == 1 || x == 2)) || (row == 1 && x == 1))
                        {
                            expected = white ? Colors.White : Color.FromRgb(178, 178, 178);
                        }
                        else if (row == 1 && x == 2)
                        {
                            byte channel = white ? (byte)228 : (byte)170;
                            expected = Color.FromRgb(channel, channel, channel);
                        }
                        else if (row == 1 && x == 3)
                        {
                            expected = white ? Color.FromRgb(195, 195, 195) : Colors.Black;
                        }
                    }

                    ReadPixel(frame, left + x, y).Should().Be(expected);
                }
            }

            strip.GetVisualChildren().OfType<Control>().Should().ContainSingle("the disabled grip is owner paint, not a draggable input child");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public void Professional_grip_should_use_source_ControlText_and_GripLight_roles_with_source_RTL_dot_order(bool rightToLeft, bool dark)
    {
        Color darkRole = dark ? Colors.White : Colors.Black;
        Color lightRole = dark ? Color.FromRgb(50, 50, 50) : Colors.White;
        using NativeToolStrip strip = new()
        {
            Width = 40,
            Height = 25,
            Background = Brushes.Magenta,
            UseSystemVisualStyle = false,
            GripDarkBrush = new SolidColorBrush(darkRole),
            GripLightBrush = new SolidColorBrush(lightRole),
            FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
        };
        Button item = new() { Width = 23, Height = 22, Margin = default };
        strip.Items.Add(item);
        Window window = new() { Width = 60, Height = 50, Content = new Canvas { Children = { strip } } };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            using WriteableBitmap frame = window.CaptureRenderedFrame() ?? throw new AssertionException("The actual grip frame is unavailable.");
            int left = rightToLeft ? 35 : 0;
            int lightX = rightToLeft ? 1 : 2;
            int darkX = rightToLeft ? 2 : 1;
            for (int y = 0; y < 25; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    Color expected = Colors.Magenta;
                    for (int dot = 0; dot < (25 - 8) / 4; dot++)
                    {
                        int lightY = 5 + (dot * 4);
                        if (x >= lightX && x < lightX + 2 && y >= lightY && y < lightY + 2)
                        {
                            expected = lightRole;
                        }

                        if (x >= darkX && x < darkX + 2 && y >= lightY - 1 && y < lightY + 1)
                        {
                            expected = darkRole;
                        }
                    }

                    ReadPixel(frame, left + x, y).Should().Be(expected);
                }
            }

            item.Bounds.X.Should().Be(rightToLeft ? 12 : 5, "painting the grip must not change its existing five-pixel reserve");
        }
        finally
        {
            window.Close();
        }
    }

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
