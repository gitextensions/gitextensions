using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Forms.VisualStyles;
using AwesomeAssertions;
using GitExtensions.ParityCapture;
using GitUI.UserControls;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class NativeTreeGlyphPaintTests
{
    private const int GlyphPart = 2;

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void Source_tree_glyph_should_record_its_native_theme_raster_and_button_hit_area(bool dark, bool expanded)
    {
        string directory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "TreeGlyphProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        SystemColorMode originalMode = Application.ColorMode;
        try
        {
            Application.SetColorMode(dark ? SystemColorMode.Dark : SystemColorMode.Classic);
            Application.DoEvents();
            using Font font = new("Segoe UI", 9);
            using Bitmap image = new(16, 18);
            using ImageList images = new() { ImageSize = image.Size, ColorDepth = ColorDepth.Depth32Bit };
            images.Images.Add(image);
            using Form window = new() { AutoScaleMode = AutoScaleMode.None, ClientSize = new Size(300, 150), ShowInTaskbar = false };
            using NativeTreeView tree = new()
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Font = font,
                ImageList = images,
                FullRowSelect = true,
            };
            TreeNode root = tree.Nodes.Add("Branches");
            TreeNode nested = root.Nodes.Add("feature");
            nested.Nodes.Add("nested-leaf");
            root.Nodes.Add("main");
            tree.Nodes.Add("Tags");
            if (expanded)
            {
                root.Expand();
                nested.Expand();
            }

            window.Controls.Add(tree);
            window.Show();
            window.Activate();
            tree.Focus().Should().BeTrue();
            tree.SelectedNode = null;

            // Settle actual queued native painting, without capturing and retrying
            // based on pixel values or assuming the desktop pointer is over this HWND.
            long settledAt = Environment.TickCount64 + 100;
            while (Environment.TickCount64 < settledAt)
            {
                Application.DoEvents();
            }

            window.Refresh();
            tree.Refresh();
            window.Update();
            tree.Update();
            tree.DeviceDpi.Should().Be(96);
            window.DeviceDpi.Should().Be(96);
            tree.Focused.Should().BeTrue();
            GetFocus().Should().Be(tree.Handle);
            Application.IsDarkModeEnabled.Should().Be(dark);
            root.IsExpanded.Should().Be(expanded);
            nint fontHandle = SendMessage(tree.Handle, 0x31, 0, 0);
            fontHandle.Should().NotBe(0);
            nint cachedWindowTheme = GetWindowTheme(tree.Handle);
            nint deviceContext = GetDC(tree.Handle);
            deviceContext.Should().NotBe(0);
            int state = expanded ? 2 : 1;
            string themeClass = dark ? "DarkMode_Explorer::TREEVIEW" : "Explorer::TREEVIEW";
            VisualStyleElement element = VisualStyleElement.CreateElement(themeClass, GlyphPart, state);
            VisualStyleRenderer.IsElementDefined(element).Should().BeTrue();

            // GetWindowTheme reports the HWND's most recently opened cached theme,
            // not the class of that theme. WinForms can cache a different class, or
            // no handle in dark mode. Open the source Explorer TREEVIEW explicitly;
            // actual PrintWindow pixels must still corroborate every raster pixel.
            VisualStyleRenderer renderer = new(element);
            nint theme = renderer.Handle;
            try
            {
                NativeRectangle sourceBounds = new() { Right = tree.ClientSize.Width, Bottom = tree.ItemHeight };
                GetThemePartSize(theme, deviceContext, GlyphPart, state, ref sourceBounds, 1, out NativeSize intrinsic).Should().Be(0);
                intrinsic.Width.Should().BeGreaterThan(0);
                intrinsic.Height.Should().BeGreaterThan(0);
                Dictionary<string, object> properties = QueryTheme(theme, deviceContext, state, ref sourceBounds);
                object imageDescriptor = ReadNativeThemeBitmap(theme, state);
                uint lineColor = unchecked((uint)SendMessage(tree.Handle, 0x1129, 0, 0));
                using Bitmap actualPart = RenderPart(theme, state, new Size(intrinsic.Width, intrinsic.Height), tree.BackColor);
                using Bitmap blackPart = RenderPart(theme, state, actualPart.Size, Color.Black);
                using Bitmap whitePart = RenderPart(theme, state, actualPart.Size, Color.White);
                actualPart.Save(Path.Combine(directory, "theme-part-over-backdrop.png"), ImageFormat.Png);
                blackPart.Save(Path.Combine(directory, "theme-part-over-black.png"), ImageFormat.Png);
                whitePart.Save(Path.Combine(directory, "theme-part-over-white.png"), ImageFormat.Png);
                using CaptureImageResult capture = ImageCapture.Capture(window, [], []);
                capture.Bitmap.Save(Path.Combine(directory, "source-tree.png"), ImageFormat.Png);
                capture.Method.Should().Be(CaptureMethod.PrintWindow);
                Point origin = tree.PointToScreen(Point.Empty) - new Size(capture.ScreenBounds.Location);
                Rectangle labelBeforeBorderQuery = root.Bounds;
                List<Point> buttonPixels = ReadButtonHitPixels(tree, root);
                Rectangle row = new(0, root.Bounds.Top, tree.ClientSize.Width, tree.ItemHeight);
                buttonPixels.Should().NotBeEmpty("the original native node has children and its own expander hit zone");
                Rectangle button = Rectangle.FromLTRB(buttonPixels.Min(point => point.X), buttonPixels.Min(point => point.Y),
                    buttonPixels.Max(point => point.X) + 1, buttonPixels.Max(point => point.Y) + 1);
                NativeRectangle nativeButton = new() { Left = button.Left, Top = button.Top, Right = button.Right, Bottom = button.Bottom };
                properties["NativeButtonAllocation"] = QueryTheme(theme, deviceContext, state, ref nativeButton);

                // TVM_SETBORDER returns the previous item spacing. Neither action
                // flag is set: this diagnostic must leave geometry and pixels alone.
                // The API is documented as internal; no shipping code relies on it.
                uint border = unchecked((uint)SendMessage(tree.Handle, 0x1123, 0, 0));
                uint borderAfter = unchecked((uint)SendMessage(tree.Handle, 0x1123, 0, 0));
                borderAfter.Should().Be(border);
                root.Bounds.Should().Be(labelBeforeBorderQuery);
                ReadButtonHitPixels(tree, root).Should().Equal(buttonPixels);
                using CaptureImageResult afterBorderQuery = ImageCapture.Capture(window, [], []);
                afterBorderQuery.Bitmap.Save(Path.Combine(directory, "source-tree-after-border-query.png"), ImageFormat.Png);
                afterBorderQuery.Method.Should().Be(CaptureMethod.PrintWindow);
                afterBorderQuery.ScreenBounds.Should().Be(capture.ScreenBounds);
                Rectangle client = new(origin, tree.ClientSize);
                ReadPixels(afterBorderQuery.Bitmap, client).SelectMany(pixels => pixels)
                    .Should().Equal(ReadPixels(capture.Bitmap, client).SelectMany(pixels => pixels));
                List<Point> exactRasterMatches = [];

                // Native hit testing bounds the clickable ink, not the full
                // image part's transparent padding. Keep the entire queried
                // raster and search the actual hierarchy area, without cropping
                // to observed ink or requiring padding inside the hit rectangle.
                for (int y = row.Top; y <= row.Bottom - actualPart.Height; y++)
                {
                    for (int x = row.Left; x <= root.Bounds.Left - actualPart.Width; x++)
                    {
                        if (Matches(capture.Bitmap, origin + new Size(x, y), actualPart))
                        {
                            exactRasterMatches.Add(new Point(x, y));
                        }
                    }
                }

                Point cursorInTree = tree.PointToClient(Cursor.Position);
                TreeViewHitTestInfo pointerHit = tree.HitTest(cursorInTree);
                object report = new
                {
                    inputMode = "sourceNodeExpandCollapseWithoutSyntheticHover",
                    captureMethod = capture.Method.ToString(),
                    dpiMode = "nativeMonitor",
                    deviceDpi = tree.DeviceDpi,
                    actualApplicationMode = Application.ColorMode.ToString(),
                    actualDarkModeEnabled = Application.IsDarkModeEnabled,
                    requestedDark = dark,
                    actualNativeThemeHandle = theme.ToInt64(),
                    cachedWindowThemeHandle = cachedWindowTheme.ToInt64(),
                    queriedThemeBoundary = "explicitSourceExplorerTreeViewClassIndependentlyCorroboratedByNativeWindowPixels",
                    themeClass,
                    themePart = GlyphPart,
                    state,
                    sourceFont = new { family = tree.Font.FontFamily.Name, points = tree.Font.SizeInPoints, style = tree.Font.Style.ToString(), nativeHandle = fontHandle.ToInt64() },
                    tree.Indent,
                    tree.ItemHeight,
                    tree.ShowLines,
                    tree.ShowRootLines,
                    tree.ShowPlusMinus,
                    lineColor = new { rawColorRef = $"0x{lineColor:X8}", defaultSentinel = lineColor == 0xFF000000, resolvedArgb = lineColor == 0xFF000000 ? null : Argb(Color.FromArgb((int)(lineColor & 255), (int)((lineColor >> 8) & 255), (int)((lineColor >> 16) & 255))) },
                    backdrop = Argb(tree.BackColor),
                    nativeLabelBounds = root.Bounds,
                    nativeButtonHitZone = button,
                    nativeItemBorderQuery = new
                    {
                        method = "TVM_SETBORDER(zeroActionFlags,zeroSizes)PreviousSpacingOnly",
                        actionFlags = 0,
                        packedPrevious = $"0x{border:X8}",
                        horizontalSpacing = unchecked((short)border),
                        verticalSpacing = unchecked((short)(border >> 16)),
                        geometryHitTestingAndFullClientPixelsUnchanged = true,
                        shippingDependency = false,
                    },
                    intrinsicGlyphSize = new { width = intrinsic.Width, height = intrinsic.Height },
                    properties,
                    imageDescriptor,
                    nativeCursorPoint = cursorInTree,
                    nativeCursorHwnd = WindowFromPoint(Cursor.Position).ToInt64(),
                    nativeTreeHwnd = tree.Handle.ToInt64(),
                    pointerHitNode = pointerHit.Node?.FullPath,
                    pointerHitLocation = pointerHit.Location.ToString(),
                    rowPixels = ReadPixels(capture.Bitmap, new Rectangle(origin + new Size(row.Location), row.Size)),
                    buttonPixels = ReadPixels(capture.Bitmap, new Rectangle(origin + new Size(button.Location), button.Size)),
                    themePartPixels = ReadPixels(actualPart, new Rectangle(Point.Empty, actualPart.Size)),
                    themePartOverBlack = ReadPixels(blackPart, new Rectangle(Point.Empty, blackPart.Size)),
                    themePartOverWhite = ReadPixels(whitePart, new Rectangle(Point.Empty, whitePart.Size)),
                    exactIntrinsicRasterMatchOrigins = exactRasterMatches,
                    matchingContract = "completeIntrinsicThemeRasterSearchWithinNativeHierarchyEveryPaintedPixelIndependentlyButtonHitTested",
                    physicalHoverClaim = false,
                };
                string path = Path.Combine(directory, "glyph.json");
                File.WriteAllText(path, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
                TestContext.Out.WriteLine($"dpiMode=nativeMonitor actualTheme={themeClass} part={GlyphPart} state={state} nativeButtonHitZone={button} queriedIntrinsic={intrinsic.Width},{intrinsic.Height} exactMatches={string.Join(';', exactRasterMatches)} output={path}");
                exactRasterMatches.Should().ContainSingle("the actual source TREEVIEW glyph must corroborate the independently queried intrinsic theme raster before it becomes a portable descriptor");
                Point glyphOrigin = exactRasterMatches[0];
                for (int y = 0; y < actualPart.Height; y++)
                {
                    for (int x = 0; x < actualPart.Width; x++)
                    {
                        if (actualPart.GetPixel(x, y).ToArgb() != tree.BackColor.ToArgb())
                        {
                            TreeViewHitTestInfo paintedHit = tree.HitTest(glyphOrigin.X + x, glyphOrigin.Y + y);
                            paintedHit.Node.Should().BeSameAs(root);
                            paintedHit.Location.HasFlag(TreeViewHitTestLocations.PlusMinus).Should().BeTrue();
                        }
                    }
                }
            }
            finally
            {
                ReleaseDC(tree.Handle, deviceContext);
            }
        }
        finally
        {
            Application.SetColorMode(originalMode);
            Application.DoEvents();
        }
    }

    private static Dictionary<string, object> QueryTheme(nint theme, nint deviceContext, int state, ref NativeRectangle bounds)
    {
        Dictionary<string, object> properties = [];
        foreach (EnumProperty property in new[]
        {
            EnumProperty.BackgroundType, EnumProperty.SizingType, EnumProperty.ImageLayout,
            EnumProperty.HorizontalAlignment, EnumProperty.VerticalAlignment, EnumProperty.ContentAlignment,
            EnumProperty.OffsetType, EnumProperty.TrueSizeScalingType,
        })
        {
            int result = GetThemeEnumValue(theme, GlyphPart, state, (int)property, out int value);
            properties[property.ToString()] = new { hresult = result, value };
        }

        foreach (IntegerProperty property in new[] { IntegerProperty.ImageCount, IntegerProperty.BorderSize })
        {
            int result = GetThemeInt(theme, GlyphPart, state, (int)property, out int value);
            properties[property.ToString()] = new { hresult = result, value };
        }

        foreach (MarginProperty property in new[] { MarginProperty.SizingMargins, MarginProperty.ContentMargins })
        {
            int result = GetThemeMargins(theme, deviceContext, GlyphPart, state, (int)property, ref bounds, out NativeMargins margins);
            properties[property.ToString()] = new { hresult = result, left = margins.Left, top = margins.Top, right = margins.Right, bottom = margins.Bottom };
        }

        for (int sizing = 0; sizing <= 2; sizing++)
        {
            int result = GetThemePartSize(theme, deviceContext, GlyphPart, state, ref bounds, sizing, out NativeSize size);
            properties[$"PartSize{(ThemeSizeType)sizing}"] = new { hresult = result, width = size.Width, height = size.Height };
        }

        int contentResult = GetThemeBackgroundContentRect(theme, deviceContext, GlyphPart, state, ref bounds, out NativeRectangle content);
        properties["BackgroundContentRectangle"] = new { hresult = contentResult, left = content.Left, top = content.Top, right = content.Right, bottom = content.Bottom };
        if (contentResult >= 0)
        {
            int extentResult = GetThemeBackgroundExtent(theme, deviceContext, GlyphPart, state, ref content, out NativeRectangle extent);
            properties["BackgroundExtentFromContentRectangle"] = new { hresult = extentResult, left = extent.Left, top = extent.Top, right = extent.Right, bottom = extent.Bottom };
        }

        return properties;
    }

    private static List<Point> ReadButtonHitPixels(NativeTreeView tree, TreeNode root)
    {
        List<Point> pixels = [];
        for (int y = root.Bounds.Top; y < root.Bounds.Top + tree.ItemHeight; y++)
        {
            for (int x = 0; x < root.Bounds.Left; x++)
            {
                TreeViewHitTestInfo hit = tree.HitTest(x, y);
                if (ReferenceEquals(hit.Node, root) && hit.Location.HasFlag(TreeViewHitTestLocations.PlusMinus))
                {
                    pixels.Add(new Point(x, y));
                }
            }
        }

        return pixels;
    }

    private static Bitmap RenderPart(nint theme, int state, Size size, Color backdrop)
    {
        Bitmap bitmap = new(size.Width, size.Height, PixelFormat.Format24bppRgb);
        using Graphics graphics = Graphics.FromImage(bitmap);
        nint deviceContext = graphics.GetHdc();
        nint brush = CreateSolidBrush(unchecked((uint)(backdrop.R | (backdrop.G << 8) | (backdrop.B << 16))));
        try
        {
            NativeRectangle bounds = new() { Right = size.Width, Bottom = size.Height };
            FillRect(deviceContext, ref bounds, brush).Should().NotBe(0);
            DrawThemeBackground(theme, deviceContext, GlyphPart, state, ref bounds, 0).Should().Be(0);
        }
        finally
        {
            DeleteObject(brush);
            graphics.ReleaseHdc(deviceContext);
        }

        return bitmap;
    }

    private static object ReadNativeThemeBitmap(nint theme, int state)
    {
        int result = GetThemeBitmap(theme, GlyphPart, state, 0, 2, out nint bitmap);
        if (result < 0 || bitmap == 0)
        {
            return new { status = "unsupported", reason = "Native theme bitmap descriptor unavailable.", hresult = result };
        }

        try
        {
            int bytes = GetObject(bitmap, Marshal.SizeOf<NativeDibSection>(), out NativeDibSection section);
            if (bytes != Marshal.SizeOf<NativeDibSection>() || section.Bitmap.Bits == 0)
            {
                return new { status = "unsupported", reason = "Device-dependent theme bitmap exposes no raw alpha storage; converted GetDIBits RGB is not native-alpha evidence.", objectBytes = bytes };
            }

            section.Bitmap.BitsPerPixel.Should().Be(32);
            GdiFlush().Should().BeTrue();
            int stride = Math.Abs(section.Bitmap.WidthBytes);
            byte[] raw = new byte[checked(stride * section.Bitmap.Height)];
            Marshal.Copy(section.Bitmap.Bits, raw, 0, raw.Length);
            string[] rawRows = Enumerable.Range(0, section.Bitmap.Height).Select(y => Convert.ToHexString(raw.AsSpan(y * stride, stride))).ToArray();
            return new
            {
                status = "supported",
                method = "GetThemeBitmap(property0,GBF_COPY)+GetObject(DIBSECTION)+GdiFlush+rawMemory",
                width = section.Bitmap.Width,
                height = section.Bitmap.Height,
                nativeHeightOrientation = section.Header.Height,
                bitsPerPixel = section.Bitmap.BitsPerPixel,
                rowStride = stride,
                pixelEncoding = "actualRawDibStorageBytesBgraWithoutAlphaInterpretation",
                rawRows,
            };
        }
        finally
        {
            DeleteObject(bitmap);
        }
    }

    private static bool Matches(Bitmap capture, Point origin, Bitmap part)
    {
        for (int y = 0; y < part.Height; y++)
        {
            for (int x = 0; x < part.Width; x++)
            {
                if (capture.GetPixel(origin.X + x, origin.Y + y).ToArgb() != part.GetPixel(x, y).ToArgb())
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static string[][] ReadPixels(Bitmap bitmap, Rectangle bounds)
        => Enumerable.Range(bounds.Top, bounds.Height).Select(y => Enumerable.Range(bounds.Left, bounds.Width)
            .Select(x => Argb(bitmap.GetPixel(x, y))).ToArray()).ToArray();

    private static string Argb(Color color) => $"#{color.ToArgb():X8}";

    [DllImport("user32.dll")]
    private static extern nint GetFocus();

    [DllImport("user32.dll")]
    private static extern nint GetDC(nint window);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(nint window, nint deviceContext);

    [DllImport("user32.dll")]
    private static extern nint WindowFromPoint(Point point);

    [DllImport("user32.dll", EntryPoint = "SendMessageW")]
    private static extern nint SendMessage(nint window, uint message, nint messageParameter, nint locationParameter);

    [DllImport("uxtheme.dll")]
    private static extern nint GetWindowTheme(nint window);

    [DllImport("uxtheme.dll")]
    private static extern int GetThemePartSize(nint theme, nint deviceContext, int part, int state, ref NativeRectangle bounds, int sizing, out NativeSize size);

    [DllImport("uxtheme.dll")]
    private static extern int GetThemeBackgroundContentRect(nint theme, nint deviceContext, int part, int state, ref NativeRectangle bounds, out NativeRectangle content);

    [DllImport("uxtheme.dll")]
    private static extern int GetThemeBackgroundExtent(nint theme, nint deviceContext, int part, int state, ref NativeRectangle content, out NativeRectangle extent);

    [DllImport("uxtheme.dll")]
    private static extern int GetThemeMargins(nint theme, nint deviceContext, int part, int state, int property, ref NativeRectangle bounds, out NativeMargins margins);

    [DllImport("uxtheme.dll")]
    private static extern int GetThemeEnumValue(nint theme, int part, int state, int property, out int value);

    [DllImport("uxtheme.dll")]
    private static extern int GetThemeInt(nint theme, int part, int state, int property, out int value);

    [DllImport("uxtheme.dll")]
    private static extern int GetThemeBitmap(nint theme, int part, int state, int property, uint flags, out nint bitmap);

    [DllImport("uxtheme.dll")]
    private static extern int DrawThemeBackground(nint theme, nint deviceContext, int part, int state, ref NativeRectangle bounds, nint clip);

    [DllImport("user32.dll")]
    private static extern int FillRect(nint deviceContext, ref NativeRectangle bounds, nint brush);

    [DllImport("gdi32.dll")]
    private static extern nint CreateSolidBrush(uint color);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject(nint value);

    [DllImport("gdi32.dll", EntryPoint = "GetObjectW")]
    private static extern int GetObject(nint value, int size, out NativeDibSection section);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GdiFlush();

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeSize
    {
        public int Width;
        public int Height;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeMargins
    {
        public int Left;
        public int Right;
        public int Top;
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
