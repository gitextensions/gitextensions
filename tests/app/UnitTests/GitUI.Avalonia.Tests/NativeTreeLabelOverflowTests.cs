using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using GitUI.Compat;
using GitUI.LeftPanel;
using GitUI.Properties;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class NativeTreeLabelOverflowTests
{
    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    public void Live_repository_headers_should_use_the_source_styled_slot_without_changing_regular_font_inheritance(bool bold, bool italic)
    {
        const string caption = "repository (Avalonia_Port)";
        Control header = RepoObjectsTree.CreateHeader(caption, Images.FolderSubmodule, bold, italic);
        NativeTreeTextBlock text = ((StackPanel)header).Children.OfType<NativeTreeTextBlock>().Single();
        TreeView tree = new()
        {
            FontFamily = text.FontFamily,
            FontSize = text.FontSize,
            Items = { new TreeViewItem { Header = header } },
        };
        Window window = new() { Width = 360, Height = 160, Content = tree };
        try
        {
            window.Show();
            window.UpdateLayout();
            text.UsesNodeFontForMeasurement.Should().Be(bold || italic);
            text.UsesAmbientFont.Should().Be(!bold && !italic);
            text.DesiredSize.Width.Should().Be(GetNativeLabelWidth(bold || italic ? text : CreateAmbientFont(tree), caption));
            text.ClipToBounds.Should().BeTrue();
            text.Padding.Should().Be(new Thickness(2, 0));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public void Styled_node_ink_should_preserve_cached_item_and_tree_viewport_clipping(bool italic, bool liveNodeUpdate)
    {
        const string caption = "repository (Avalonia_Port)";
        NativeTreeTextBlock text = new()
        {
            Text = caption,
            UsesNodeFontForMeasurement = liveNodeUpdate,
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 12,
            FontWeight = FontWeight.Bold,
            FontStyle = italic ? FontStyle.Italic : FontStyle.Normal,
            Foreground = Brushes.Black,
        };
        TreeView tree = new()
        {
            Width = 340,
            Height = 120,
            BorderThickness = new Thickness(0),
            Background = Brushes.White,
            FontFamily = text.FontFamily,
            FontSize = text.FontSize,
            FontWeight = FontWeight.Normal,
            FontStyle = FontStyle.Normal,
            Items = { new TreeViewItem { Header = text } },
            Classes = { "gitextensions-native-tree", "gitextensions-explorer-tree", "gitextensions-hide-tree-selection" },
        };
        ScrollViewer.SetHorizontalScrollBarVisibility(tree, Avalonia.Controls.Primitives.ScrollBarVisibility.Auto);
        ScrollViewer.SetVerticalScrollBarVisibility(tree, Avalonia.Controls.Primitives.ScrollBarVisibility.Auto);
        ScrollViewer.SetAllowAutoHide(tree, false);
        Canvas.SetLeft(tree, 8);
        Canvas.SetTop(tree, 8);
        Button other = new() { Content = "Other", Width = 100, Height = 25 };
        Canvas.SetLeft(other, 8);
        Canvas.SetTop(other, 140);
        Window window = new()
        {
            Width = 400,
            Height = 180,
            RequestedThemeVariant = ThemeVariant.Light,
            Content = new Canvas { Background = Brushes.Magenta, Children = { tree, other } },
        };
        try
        {
            window.Show();
            other.Focus().Should().BeTrue();
            window.UpdateLayout();
            Size wideSlot = text.Bounds.Size;
            double expectedSlotWidth = GetNativeLabelWidth(liveNodeUpdate ? text : CreateAmbientFont(tree), caption);
            text.DesiredSize.Width.Should().Be(expectedSlotWidth);
            if (OperatingSystem.IsWindows() && WinFormsRichEditTextMeasurer.TryGetTextWidth(text, caption, out _))
            {
                // Independent native96 oracle: pre-show cache141, live Bold148,
                // live Bold|Italic152 for this exact Segoe UI9pt source caption.
                text.DesiredSize.Width.Should().Be(liveNodeUpdate ? italic ? 152 : 148 : 141);
            }

            text.ClipToBounds.Should().BeTrue("the actual source TREEVIEW clips styled text at its cached item rectangle");
            text.FontWeight.Should().Be(FontWeight.Bold);
            text.FontStyle.Should().Be(italic ? FontStyle.Italic : FontStyle.Normal);
            Point labelOrigin = text.TranslatePoint(default, window)!.Value;
            Point treeOrigin = tree.TranslatePoint(default, window)!.Value;
            int labelRight = (int)Math.Ceiling(labelOrigin.X + wideSlot.Width);
            int wideInkRight;
            using (WriteableBitmap frame = Capture(window))
            {
                wideInkRight = GetInkRight(frame, labelOrigin, wideSlot.Height, (int)(treeOrigin.X + tree.Bounds.Width));
            }

            wideInkRight.Should().BeGreaterThanOrEqualTo((int)Math.Ceiling(labelOrigin.X));
            wideInkRight.Should().BeLessThan(labelRight,
                "styled glyphs must respect the cached source label slot, as the source TREEVIEW does");
            tree.Width = labelOrigin.X - treeOrigin.X + (wideSlot.Width / 2);
            window.UpdateLayout();
            text.Bounds.Size.Should().Be(wideSlot, "narrowing the viewport must not replace the node's cached ambient-font extent");
            text.DesiredSize.Width.Should().Be(expectedSlotWidth);
            using WriteableBitmap narrow = Capture(window);
            int viewportRight = (int)Math.Ceiling(treeOrigin.X + tree.Bounds.Width);
            wideInkRight.Should().BeGreaterThanOrEqualTo(viewportRight,
                "the narrowed viewport must actually cut through the original styled text paint");
            for (int y = (int)Math.Ceiling(labelOrigin.Y); y < (int)(labelOrigin.Y + wideSlot.Height); y++)
            {
                for (int x = viewportRight; x <= wideInkRight; x++)
                {
                    ReadFramePixel(narrow, new PixelPoint(x, y)).Should().Be(Colors.Magenta,
                        "styled node text must not escape its TreeView viewport at {0},{1}", x, y);
                }
            }
        }
        finally
        {
            window.Close();
        }
    }

    private static TextBlock CreateAmbientFont(TreeView tree)
        => new()
        {
            FontFamily = tree.FontFamily,
            FontSize = tree.FontSize,
            FontWeight = tree.FontWeight,
            FontStyle = tree.FontStyle,
        };

    private static double GetNativeLabelWidth(TextBlock font, string caption)
        => (WinFormsRichEditTextMeasurer.TryGetTextWidth(font, caption, out double width)
            ? width
            : WinFormsTextMeasurer.MeasureSize(font, caption).Width) + 4;

    private static WriteableBitmap Capture(Window window)
        => window.CaptureRenderedFrame() ?? throw new InvalidOperationException("The tree label overflow frame is unavailable.");

    private static int GetInkRight(WriteableBitmap frame, Point origin, double height, int right)
    {
        int rightmost = -1;
        for (int y = (int)Math.Ceiling(origin.Y); y < (int)(origin.Y + height); y++)
        {
            for (int x = (int)Math.Ceiling(origin.X); x < right; x++)
            {
                if (ReadFramePixel(frame, new PixelPoint(x, y)) != Colors.White)
                {
                    rightmost = Math.Max(rightmost, x);
                }
            }
        }

        return rightmost;
    }

    private static Color ReadFramePixel(WriteableBitmap frame, PixelPoint point)
    {
        using ILockedFramebuffer buffer = frame.Lock();
        buffer.Format.BitsPerPixel.Should().Be(32);
        byte[] pixel = new byte[4];
        Marshal.Copy(IntPtr.Add(buffer.Address, (point.Y * buffer.RowBytes) + (point.X * 4)), pixel, 0, pixel.Length);
        return buffer.Format == PixelFormat.Bgra8888
            ? Color.FromArgb(pixel[3], pixel[2], pixel[1], pixel[0])
            : Color.FromArgb(pixel[3], pixel[0], pixel[1], pixel[2]);
    }
}
