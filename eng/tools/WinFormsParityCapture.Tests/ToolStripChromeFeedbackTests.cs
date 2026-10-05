using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Forms.VisualStyles;
using AwesomeAssertions;
using GitExtensions.ParityCapture;
using GitUI;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class ToolStripChromeFeedbackTests
{
    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void Source_disabled_visible_grip_should_retain_actual_renderer_raster_and_metadata(bool professional, bool dark)
    {
        SystemColorMode oldMode = Application.ColorMode;
        try
        {
            Application.SetColorMode(dark ? SystemColorMode.Dark : SystemColorMode.Classic);
            using Form host = NewHost();
            using ToolStripEx strip = NewStrip(host, professional);
            ToolStripGripRenderEventArgs? grip = null;
            strip.Renderer.RenderGrip += (_, e) => grip = e;
            strip.Items.Add(new ToolStripButton("Refresh"));
            host.Show();
            Application.DoEvents();
            strip.DeviceDpi.Should().Be(96);
            strip.GripEnabled.Should().BeFalse();
            using Bitmap actual = new(strip.Width, strip.Height);
            strip.DrawToBitmap(actual, strip.ClientRectangle);
            grip.Should().NotBeNull("the disabled source grip remains visible and is rendered");
            ToolStripGripRenderEventArgs sourceGrip = grip ?? throw new InvalidOperationException("The source grip was not painted.");
            using Bitmap black = RenderGrip(strip, sourceGrip, Color.Black);
            using Bitmap white = RenderGrip(strip, sourceGrip, Color.White);
            object? theme = null;
            if (!professional)
            {
                VisualStyleRenderer renderer = new(VisualStyleElement.Rebar.Gripper.Normal);
                using Graphics graphics = strip.CreateGraphics();
                theme = new
                {
                    renderer.Class,
                    renderer.Part,
                    renderer.State,
                    sizingType = renderer.GetEnumValue(EnumProperty.SizingType).ToString(),
                    sizingMargins = renderer.GetMargins(graphics, MarginProperty.SizingMargins),
                    contentMargins = renderer.GetMargins(graphics, MarginProperty.ContentMargins),
                    trueSize = renderer.GetPartSize(graphics, ThemeSizeType.True),
                    minimumSize = renderer.GetPartSize(graphics, ThemeSizeType.Minimum),
                    bitmap = ReadThemeBitmap(renderer.Handle, renderer.Part, renderer.State),
                };
            }

            string directory = EvidenceDirectory();
            actual.Save(Path.Combine(directory, "actual-owner.png"), ImageFormat.Png);
            black.Save(Path.Combine(directory, "grip-over-black.png"), ImageFormat.Png);
            white.Save(Path.Combine(directory, "grip-over-white.png"), ImageFormat.Png);
            Record(directory, new
            {
                acquisition = "actualToolStripExDrawToBitmapAndRendererOnGdiInitializedHdc",
                professional,
                dark,
                deviceDpi = strip.DeviceDpi,
                renderer = strip.Renderer.GetType().FullName,
                strip.GripRectangle,
                sourceGrip.GripBounds,
                gripDisplayStyle = sourceGrip.GripDisplayStyle.ToString(),
                gripEnabled = strip.GripEnabled,
                darkGrip = strip.Renderer is ToolStripProfessionalRenderer table ? Argb(table.ColorTable.GripDark) : null,
                lightGrip = strip.Renderer is ToolStripProfessionalRenderer light ? Argb(light.ColorTable.GripLight) : null,
                theme,
                overBlack = Pixels(black),
                overWhite = Pixels(white),
            });
        }
        finally
        {
            Application.SetColorMode(oldMode);
            Application.DoEvents();
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Source_ToolStripEx_should_keep_actual_preopening_owner_pixels_when_pointer_enters_dropdown(bool split)
    {
        Point oldCursor = NativeMethods.GetCursorPosition();
        using Form host = NewHost();
        using ToolStripEx strip = NewStrip(host, professional: false);
        using ToolStripDropDownItem item = split ? new ToolStripSplitButton("Source item") : new ToolStripDropDownButton("Source item");
        item.DropDownItems.Add(new ToolStripMenuItem("Source option"));
        strip.Items.Add(item);
        try
        {
            host.Show();
            host.Activate();
            Application.DoEvents();
            strip.DeviceDpi.Should().Be(96);
            Point point = split && item is ToolStripSplitButton splitItem
                ? new Point(item.Bounds.X + splitItem.DropDownButtonBounds.X + (splitItem.DropDownButtonBounds.Width / 2), item.Bounds.Y + (item.Height / 2))
                : new Point(item.Bounds.X + (item.Width / 2), item.Bounds.Y + (item.Height / 2));
            NativeMethods.SetCursorPosition(strip.PointToScreen(point));
            NativeMethods.SendMouseMessage(strip.Handle, NativeMethods.WmMouseMove, point.X, point.Y);
            Application.DoEvents();
            if (!item.Selected)
            {
                // Match the capture driver's documented fallback when its queued
                // native hover is cleared by modal menu filtering after the pump.
                item.Select();
                strip.Refresh();
            }

            item.Selected.Should().BeTrue();
            using Bitmap before = ScreenOwner(strip, item);
            NativeMethods.SendMouseMessage(strip.Handle, 0x0201, point.X, point.Y);
            Application.DoEvents();
            item.DropDown.Visible.Should().BeTrue();
            NativeMethods.SendMouseMessage(strip.Handle, 0x0202, point.X, point.Y);
            Application.DoEvents();
            using Bitmap opened = ScreenOwner(strip, item);
            Rectangle ownerBounds = strip.RectangleToScreen(item.Bounds);
            Rectangle popupBounds = NativeMethods.GetWindowRectangle(item.DropDown.Handle);
            IReadOnlyList<NativePopupShadow> shadows = NativeMethods.GetAssociatedPopupShadows([item.DropDown.Handle]);
            Rectangle[] occludingBounds = [popupBounds, .. shadows.Select(shadow => shadow.Bounds)];
            Rectangle popupFootprint = occludingBounds.Aggregate(Rectangle.Union);
            using Bitmap openedPopup = ScreenRectangle(popupFootprint);
            ToolStripItem option = item.DropDownItems[0];
            Point optionPoint = new(option.Bounds.X + (option.Width / 2), option.Bounds.Y + (option.Height / 2));
            NativeMethods.SetCursorPosition(item.DropDown.PointToScreen(optionPoint));
            NativeMethods.SendMouseMessage(item.DropDown.Handle, NativeMethods.WmMouseMove, optionPoint.X, optionPoint.Y);
            Application.DoEvents();
            if (!option.Selected)
            {
                option.Select();
                item.DropDown.Refresh();
            }

            option.Selected.Should().BeTrue();
            using Bitmap hovered = ScreenOwner(strip, item);
            using Bitmap hoveredPopup = ScreenRectangle(popupFootprint);
            OwnerComparison openedComparison = CompareOwner(before, opened, openedPopup, ownerBounds, popupFootprint, occludingBounds);
            OwnerComparison hoveredComparison = CompareOwner(before, hovered, hoveredPopup, ownerBounds, popupFootprint, occludingBounds);
            string directory = EvidenceDirectory();
            before.Save(Path.Combine(directory, "preopening-owner-screen.png"), ImageFormat.Png);
            opened.Save(Path.Combine(directory, "opened-owner-screen.png"), ImageFormat.Png);
            hovered.Save(Path.Combine(directory, "option-hover-owner-screen.png"), ImageFormat.Png);
            openedPopup.Save(Path.Combine(directory, "opened-popup-and-qualified-shadow-screen.png"), ImageFormat.Png);
            hoveredPopup.Save(Path.Combine(directory, "option-hover-popup-and-qualified-shadow-screen.png"), ImageFormat.Png);
            Record(directory, new
            {
                acquisition = "ownedHwndPointerMessagesWithSourceSelectFallbackAndDwmFlushBeforeUnscaledActualOwnerScreenPixels",
                split,
                item.Selected,
                item.Pressed,
                buttonSelected = item is ToolStripSplitButton primary && primary.ButtonSelected,
                buttonPressed = item is ToolStripSplitButton pressed && pressed.ButtonPressed,
                dropDownSelected = item is ToolStripSplitButton dropdown && dropdown.DropDownButtonSelected,
                dropDownPressed = item is ToolStripSplitButton open && open.DropDownButtonPressed,
                sourceRedrawSuspension = "ToolStripEx.DropDownOpening sends WM_SETREDRAW(FALSE) to the owner",
                actualScreenBounds = ownerBounds,
                actualPopupBounds = popupBounds,
                actualAssociatedShadows = shadows,
                ownerOcclusionRegions = occludingBounds.Select(bounds => Rectangle.Intersect(ownerBounds, bounds)).Where(bounds => !bounds.IsEmpty).ToArray(),
                comparisonScope = "all owner pixels outside actual popup/qualified adjacent SysShadow rectangles; occluded pixels compared with independently acquired actual popup footprint at the same state",
                openedComparison,
                hoveredComparison,
                preopening = Pixels(before),
                opened = Pixels(opened),
                optionHover = Pixels(hovered),
            });
            openedComparison.UnoccludedPixels.Should().BeGreaterThan(0);
            hoveredComparison.UnoccludedPixels.Should().Be(openedComparison.UnoccludedPixels);
            openedComparison.UnoccludedDifferences.Should().Be(0, "WM_SETREDRAW preserves every unoccluded source owner pixel");
            hoveredComparison.UnoccludedDifferences.Should().Be(0, "entering the actual option cannot repaint the frozen source owner");
            openedComparison.PopupFootprintDifferences.Should().Be(0, "occluded owner pixels come from the actual opened popup/shadow screen acquisition");
            hoveredComparison.PopupFootprintDifferences.Should().Be(0, "occluded owner pixels come from the actual hovered popup/shadow screen acquisition");
        }
        finally
        {
            item.DropDown.Close();
            host.Close();
            NativeMethods.SetCursorPosition(oldCursor);
        }
    }

    [TestCase(false, false, 22)]
    [TestCase(false, false, 24)]
    [TestCase(true, false, 22)]
    [TestCase(true, true, 24)]
    public void Source_down_arrow_should_retain_integer_center_and_actual_filled_renderer_pixels(bool professional, bool dark, int height)
    {
        SystemColorMode oldMode = Application.ColorMode;
        try
        {
            Application.SetColorMode(dark ? SystemColorMode.Dark : SystemColorMode.Classic);
            using Form host = NewHost();
            using ToolStripEx strip = NewStrip(host, professional);
            using ToolStripSplitButton item = new() { Text = "Source item" };
            strip.Items.Add(item);
            host.Show();
            Application.DoEvents();
            strip.DeviceDpi.Should().Be(96);
            using Bitmap frame = new(11, height, PixelFormat.Format24bppRgb);
            using (Graphics graphics = Graphics.FromImage(frame))
            {
                nint hdc = graphics.GetHdc();
                nint brush = CreateSolidBrush(0x00FF00FF);
                try
                {
                    NativeRectangle bounds = new() { Right = frame.Width, Bottom = frame.Height };
                    FillRect(hdc, ref bounds, brush).Should().NotBe(0);
                    using Graphics native = Graphics.FromHdc(hdc);
                    strip.Renderer.DrawArrow(new ToolStripArrowRenderEventArgs(native, item,
                        new Rectangle(Point.Empty, frame.Size), SystemColors.ControlText, ArrowDirection.Down));
                }
                finally
                {
                    DeleteObject(brush);
                    graphics.ReleaseHdc(hdc);
                }
            }

            string directory = EvidenceDirectory();
            frame.Save(Path.Combine(directory, "source-down-arrow.png"), ImageFormat.Png);
            Record(directory, new
            {
                acquisition = "actualToolStripRendererDrawArrowOnGdiInitializedHdc",
                professional,
                dark,
                arrowRectangle = new Rectangle(Point.Empty, frame.Size),
                direction = ArrowDirection.Down.ToString(),
                color = Argb(SystemColors.ControlText),
                deviceDpi = strip.DeviceDpi,
                renderer = strip.Renderer.GetType().FullName,
                pixels = Pixels(frame),
            });
        }
        finally
        {
            Application.SetColorMode(oldMode);
            Application.DoEvents();
        }
    }

    private static Form NewHost()
        => new() { AutoScaleMode = AutoScaleMode.None, ClientSize = new Size(260, 100), ShowInTaskbar = false, TopMost = true };

    private static ToolStripEx NewStrip(Form host, bool professional)
    {
        ToolStripEx strip = new()
        {
            Dock = DockStyle.None,
            AutoSize = false,
            Size = new Size(240, 25),
            GripMargin = Padding.Empty,
            GripEnabled = false,
            Padding = Padding.Empty,
        };
        if (professional)
        {
            strip.RenderMode = ToolStripRenderMode.Professional;
        }

        host.Controls.Add(strip);
        return strip;
    }

    private static Bitmap ScreenOwner(ToolStrip strip, ToolStripItem item)
        => ScreenRectangle(strip.RectangleToScreen(item.Bounds));

    private static Bitmap ScreenRectangle(Rectangle bounds)
    {
        NativeMethods.IsEntirelyOnScreen(bounds).Should().BeTrue();

        // Pumping WinForms messages does not wait for the native surface's presentation.
        // Flush this application's queued DWM updates before independent screen reads.
        DwmFlush().Should().Be(0, "the actual native surface must be presented before acquisition");
        Bitmap frame = new(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb);
        using Graphics graphics = Graphics.FromImage(frame);
        graphics.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
        return frame;
    }

    private static OwnerComparison CompareOwner(Bitmap before, Bitmap owner, Bitmap popup,
        Rectangle ownerBounds, Rectangle popupFootprint, IReadOnlyList<Rectangle> occludingBounds)
    {
        int unoccluded = 0;
        int differences = 0;
        int occluded = 0;
        int popupDifferences = 0;
        for (int y = 0; y < owner.Height; y++)
        {
            for (int x = 0; x < owner.Width; x++)
            {
                Point screen = new(ownerBounds.X + x, ownerBounds.Y + y);
                if (occludingBounds.Any(bounds => bounds.Contains(screen)))
                {
                    occluded++;
                    if (owner.GetPixel(x, y).ToArgb() != popup.GetPixel(screen.X - popupFootprint.X, screen.Y - popupFootprint.Y).ToArgb())
                    {
                        popupDifferences++;
                    }
                }
                else
                {
                    unoccluded++;
                    if (owner.GetPixel(x, y).ToArgb() != before.GetPixel(x, y).ToArgb())
                    {
                        differences++;
                    }
                }
            }
        }

        return new OwnerComparison(unoccluded, differences, occluded, popupDifferences);
    }

    private sealed record OwnerComparison(int UnoccludedPixels, int UnoccludedDifferences, int OccludedPixels, int PopupFootprintDifferences);

    private static Bitmap RenderGrip(ToolStrip strip, ToolStripGripRenderEventArgs source, Color backdrop)
    {
        Bitmap frame = new(source.GripBounds.Width, source.GripBounds.Height, PixelFormat.Format24bppRgb);
        using Graphics graphics = Graphics.FromImage(frame);
        nint hdc = graphics.GetHdc();
        nint brush = CreateSolidBrush(unchecked((uint)(backdrop.R | (backdrop.G << 8) | (backdrop.B << 16))));
        try
        {
            NativeRectangle bounds = new() { Right = frame.Width, Bottom = frame.Height };
            FillRect(hdc, ref bounds, brush).Should().NotBe(0);
            using Graphics native = Graphics.FromHdc(hdc);
            strip.Renderer.DrawGrip(new ToolStripGripRenderEventArgs(native, strip));
        }
        finally
        {
            DeleteObject(brush);
            graphics.ReleaseHdc(hdc);
        }

        return frame;
    }

    private static object ReadThemeBitmap(nint theme, int part, int state)
    {
        GetThemeBitmap(theme, part, state, 0, 2, out nint bitmap).Should().Be(0);
        try
        {
            int objectBytes = GetObject(bitmap, Marshal.SizeOf<NativeDibSection>(), out NativeDibSection section);
            if (objectBytes != Marshal.SizeOf<NativeDibSection>() || section.Bitmap.Bits == 0)
            {
                // GBF_COPY may return a device-dependent bitmap rather than a DIB.
                // Keep RGB conversion distinct from raw alpha; black/white native
                // renderer composites independently retain the actual part paint.
                using Bitmap converted = Image.FromHbitmap(bitmap);
                return new
                {
                    method = "GetThemeBitmapGBF_COPYDeviceDependentBitmapRgbConversion",
                    objectBytes,
                    converted.Width,
                    converted.Height,
                    alphaStatus = "unverified; converted RGB is not native alpha evidence",
                    rgb = Pixels(converted),
                };
            }

            section.Bitmap.BitsPerPixel.Should().Be(32);
            GdiFlush().Should().BeTrue();
            int stride = Math.Abs(section.Bitmap.WidthBytes);
            byte[] bytes = new byte[stride * section.Bitmap.Height];
            Marshal.Copy(section.Bitmap.Bits, bytes, 0, bytes.Length);
            return new
            {
                method = "GetThemeBitmapGBF_COPYGetObjectDibSectionGdiFlushRawBgra",
                section.Bitmap.Width,
                section.Bitmap.Height,
                nativeHeightOrientation = section.Header.Height,
                rawRows = Enumerable.Range(0, section.Bitmap.Height).Select(y => Convert.ToHexString(bytes.AsSpan(y * stride, stride))).ToArray(),
            };
        }
        finally
        {
            DeleteObject(bitmap);
        }
    }

    private static string[][] Pixels(Bitmap frame)
        => Enumerable.Range(0, frame.Height).Select(y => Enumerable.Range(0, frame.Width).Select(x => Argb(frame.GetPixel(x, y))).ToArray()).ToArray();

    private static string Argb(Color color) => $"#{color.ToArgb():X8}";

    private static string EvidenceDirectory()
    {
        string directory = Path.Combine(TestContext.CurrentContext.TestDirectory, "ToolStripChromeFeedbackProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static void Record(string directory, object metadata)
    {
        File.WriteAllText(Path.Combine(directory, "probe.json"), JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);
        TestContext.Out.WriteLine($"toolbarChromeFeedbackEvidence={directory}");
        TestContext.Out.WriteLine(JsonSerializer.Serialize(metadata));
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmFlush();

    [DllImport("gdi32.dll")]
    private static extern nint CreateSolidBrush(uint color);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(nint handle);

    [DllImport("gdi32.dll")]
    private static extern bool GdiFlush();

    [DllImport("gdi32.dll", EntryPoint = "GetObjectW")]
    private static extern int GetObject(nint handle, int size, out NativeDibSection section);

    [DllImport("user32.dll")]
    private static extern int FillRect(nint hdc, ref NativeRectangle bounds, nint brush);

    [DllImport("uxtheme.dll")]
    private static extern int GetThemeBitmap(nint theme, int part, int state, int property, uint flags, out nint bitmap);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeBitmap
    {
        public int Type;
        public int Width;
        public int Height;
        public int WidthBytes;
        public ushort Planes;
        public ushort BitsPerPixel;
        public nint Bits;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeBitmapHeader
    {
        public uint Size;
        public int Width;
        public int Height;
        public ushort Planes;
        public ushort BitsPerPixel;
        public uint Compression;
        public uint ImageBytes;
        public int XPixelsPerMeter;
        public int YPixelsPerMeter;
        public uint ColorsUsed;
        public uint ColorsImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeDibSection
    {
        public NativeBitmap Bitmap;
        public NativeBitmapHeader Header;
        public uint RedMask;
        public uint GreenMask;
        public uint BlueMask;
        public nint Section;
        public uint Offset;
    }
}
