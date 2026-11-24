using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms.VisualStyles;
using AwesomeAssertions;
using GitUI.UserControls;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class NativeTreeThemeTests
{
    private const int TreeItemPart = 1;
    private const uint GetItemRectangleMessage = 0x1104;

    [Test]
    [TestCase("Explorer::TREEVIEW", 3)]
    [TestCase("Explorer::TREEVIEW", 5)]
    [TestCase("DarkMode_Explorer::TREEVIEW", 3)]
    [TestCase("DarkMode_Explorer::TREEVIEW", 5)]
    public void Native_tree_selection_corners_should_preserve_image_mask_composition(string className, int state)
    {
        using Bitmap black = RenderTreePart(className, state, Color.Black);
        using Bitmap white = RenderTreePart(className, state, Color.White);
        foreach (Color backdrop in new[] { Color.FromArgb(43, 45, 58), Color.FromArgb(170, 210, 130), Color.FromArgb(127, 128, 129) })
        {
            using Bitmap actual = RenderTreePart(className, state, backdrop);
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    Color value = actual.GetPixel(x, y);
                    value.Should().Be(CompositeSamples(backdrop, black.GetPixel(x, y), white.GetPixel(x, y)));
                    value.Should().Be(actual.GetPixel(actual.Width - x - 1, y));
                    value.Should().Be(actual.GetPixel(x, actual.Height - y - 1));
                }
            }
        }

        foreach (Point point in new[] { new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1), new Point(2, 0), new Point(2, 2) })
        {
            TestContext.Progress.WriteLine($"class={className} state={state} point={point} black=#{black.GetPixel(point.X, point.Y).ToArgb():X8} white=#{white.GetPixel(point.X, point.Y).ToArgb():X8}");
        }

        white.GetPixel(0, 0).Should().NotBe(white.GetPixel(2, 0), "the native image corner has different backdrop composition from its edge");
    }

    [Test]
    public void Native_tree_image_list_should_preserve_the_source_padding_alpha_pipeline()
    {
        using Bitmap source = (Bitmap)GitUI.Properties.Images.BranchLocalRoot.Clone();
        using Bitmap padded = new(source.Width, source.Height + 2, PixelFormat.Format32bppArgb);
        using (Graphics graphics = Graphics.FromImage(padded))
        {
            graphics.DrawImageUnscaled(source, 0, 1);
        }

        using ImageList images = new() { ImageSize = padded.Size, ColorDepth = ColorDepth.Depth32Bit };
        images.Images.Add(padded);
        using Bitmap retained = new(images.Images[0]);
        using Bitmap drawn = new(padded.Width, padded.Height, PixelFormat.Format24bppRgb);
        using (Graphics graphics = Graphics.FromImage(drawn))
        {
            nint deviceContext = graphics.GetHdc();
            nint brush = CreateSolidBrush(0xFFFFFF);
            try
            {
                NativeRectangle bounds = new() { Right = drawn.Width, Bottom = drawn.Height };
                FillRect(deviceContext, ref bounds, brush).Should().NotBe(0);
                ImageListDraw(images.Handle, 0, deviceContext, 0, 0, 1).Should().BeTrue();
            }
            finally
            {
                DeleteObject(brush);
                graphics.ReleaseHdc(deviceContext);
            }
        }

        foreach (Point point in new[] { new Point(0, 1), new Point(0, 9), new Point(2, 9) })
        {
            Color original = source.GetPixel(point.X, point.Y - 1);
            Color intermediate = padded.GetPixel(point.X, point.Y);
            Color retainedColor = retained.GetPixel(point.X, point.Y);
            Color actual = drawn.GetPixel(point.X, point.Y);
            TestContext.Progress.WriteLine($"point={point} source=#{original.ToArgb():X8} padded=#{intermediate.ToArgb():X8} retained=#{retainedColor.ToArgb():X8} drawn=#{actual.ToArgb():X8}");
            intermediate.A.Should().Be(original.A);
            actual.A.Should().Be(byte.MaxValue);
        }
    }

    [Test]
    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public void Native_tree_message_driven_selection_should_separate_hit_testing_and_focus_cues(bool overLabel, bool showFocusCues)
    {
        using Font font = new("Segoe UI", 9);
        using Bitmap image = new(16, 18);
        using ImageList images = new() { ImageSize = image.Size, ColorDepth = ColorDepth.Depth32Bit };
        images.Images.Add(image);
        using Form window = new() { ClientSize = new Size(300, 180), ShowInTaskbar = false, AutoScaleMode = AutoScaleMode.None };
        using NativeTreeView tree = new() { Font = font, ImageList = images, BorderStyle = BorderStyle.None, FullRowSelect = true, Bounds = new Rectangle(0, 0, 240, 140), BackColor = Color.White };
        TreeNode node = tree.Nodes.Add("Branches");
        window.Controls.Add(tree);
        window.Show();
        window.Activate();
        tree.SelectedNode = node;
        tree.Focus().Should().BeTrue();
        Rectangle text = GetItemRectangle(tree, node, textOnly: true);
        Point position = new(overLabel ? text.Left + 10 : tree.Width - 10, text.Top + (text.Height / 2));
        TreeViewHitTestInfo hit = tree.HitTest(position);
        hit.Location.Should().Be(overLabel ? TreeViewHitTestLocations.Label : TreeViewHitTestLocations.RightOfLabel);
        hit.Node.Should().BeSameAs(node);

        // Drive the real native WndProc without claiming physical desktop hover.
        // Focus cues are a separate native UI state and can overpaint the theme edge.
        SendMessage(window.Handle, 0x128, showFocusCues ? 0x10002 : 0x10001, 0);
        SendMessage(tree.Handle, 0x200, 0, (position.Y << 16) | position.X);
        Application.DoEvents();
        nint uiState = SendMessage(tree.Handle, 0x129, 0, 0);
        ((uiState & 1) == 0).Should().Be(showFocusCues);
        tree.Focused.Should().BeTrue();
        tree.SelectedNode.Should().BeSameAs(node);
        tree.HotTracking.Should().BeFalse();
        window.Refresh();
        tree.Refresh();
        long settledAt = Environment.TickCount64 + 100;
        while (Environment.TickCount64 < settledAt)
        {
            Application.DoEvents();
        }

        tree.DeviceDpi.Should().Be(96);
        window.DeviceDpi.Should().Be(96);
        using CaptureImageResult capture = ImageCapture.Capture(window, [], []);
        Point origin = tree.PointToScreen(Point.Empty) - new Size(capture.ScreenBounds.Location);
        TestContext.Progress.WriteLine($"windowVisible={window.Visible} nativeWindowVisible={IsWindowVisible(window.Handle)} treeVisible={tree.Visible} nativeTreeVisible={IsWindowVisible(tree.Handle)} windowDpi={window.DeviceDpi} treeDpi={tree.DeviceDpi} treeClient={tree.ClientRectangle} text={text} captureBounds={capture.ScreenBounds} treeOrigin={origin} captureMethod={capture.Method}");
        Color fill = capture.Bitmap.GetPixel(origin.X + text.Left - 3, origin.Y + text.Top + 2);
        NativeTreePalette palette = NativeTreePalette.Read(tree, 3);
        Color[] adjacentEdge = Enumerable.Range(text.Left + 8, 4)
            .Select(x => capture.Bitmap.GetPixel(origin.X + x, origin.Y + text.Top)).ToArray();
        TestContext.Progress.WriteLine($"inputMode=windowMessage overLabel={overLabel} showFocusCues={showFocusCues} uiState={uiState} messagePosition={position} hit={hit.Location} hitNode={hit.Node?.Text} fill=#{fill.ToArgb():X8} adjacentEdge={string.Join(',', adjacentEdge.Select(color => $"#{color.ToArgb():X8}"))} hotTracking={tree.HotTracking}");
        if (!showFocusCues)
        {
            adjacentEdge.Should().OnlyContain(color => color == palette.Border, "native selected part3 is not a keyboard focus rectangle");
        }
        else
        {
            adjacentEdge.Should().OnlyContain(color => color == palette.Border || color.ToArgb() == Color.Black.ToArgb(),
                "visible keyboard focus cues may overpaint the selected theme edge, independently of HotTracking");
        }

        fill.Should().Be(palette.Background);
    }

    [Test]
    [TestCase("Explorer::TREEVIEW", 3)]
    [TestCase("Explorer::TREEVIEW", 5)]
    [TestCase("Explorer::TREEVIEW", 6)]
    [TestCase("DarkMode_Explorer::TREEVIEW", 3)]
    [TestCase("DarkMode_Explorer::TREEVIEW", 5)]
    [TestCase("DarkMode_Explorer::TREEVIEW", 6)]
    public void Native_tree_image_parts_should_resolve_against_the_control_backdrop(string className, int state)
    {
        NativeTreePalette black = NativeTreePalette.Read(className, state, Color.Black);
        NativeTreePalette white = NativeTreePalette.Read(className, state, Color.White);
        foreach (Color backdrop in new[] { Color.FromArgb(43, 45, 58), Color.FromArgb(170, 210, 130), Color.FromArgb(100, 100, 100), Color.FromArgb(1, 2, 3), Color.FromArgb(127, 128, 129) })
        {
            NativeTreePalette actual = NativeTreePalette.Read(className, state, backdrop);
            actual.Foreground.Should().Be(white.Foreground);
            actual.Background.A.Should().Be(byte.MaxValue);
            actual.Background.Should().Be(CompositeSamples(backdrop, black.Background, white.Background));
            actual.Border.Should().Be(CompositeSamples(backdrop, black.Border, white.Border));
            TestContext.Progress.WriteLine($"class={className} state={state} backdrop=#{backdrop.ToArgb():X8} black=#{black.Background.ToArgb():X8} white=#{white.Background.ToArgb():X8} actual=#{actual.Background.ToArgb():X8} blackEdge=#{black.Border.ToArgb():X8} whiteEdge=#{white.Border.ToArgb():X8} actualEdge=#{actual.Border.ToArgb():X8}");
            if (state != 5)
            {
                actual.Border.Should().Be(white.Border, "the focused selected/hot-selected edge is opaque");
                if (className.StartsWith("DarkMode", StringComparison.Ordinal))
                {
                    actual.Background.Should().Be(white.Background, "the dark focused selected/hot-selected image part is opaque");
                }
            }
        }
    }

    [Test]
    [TestCase(9, "Branches", FontStyle.Regular)]
    [TestCase(9, "main", FontStyle.Bold)]
    [TestCase(9, "Wörk & branches", FontStyle.Italic)]
    [TestCase(11, "Branches", FontStyle.Regular)]
    [TestCase(11, "main", FontStyle.Bold)]
    [TestCase(11, "Wörk & branches", FontStyle.Italic)]
    public void Native_tree_label_extents_should_follow_the_node_font(float points, string caption, FontStyle style)
    {
        using Font font = new("Segoe UI", points);
        using Font nodeFont = new(font, style);
        using Bitmap image = new(16, 18);
        using ImageList images = new() { ImageSize = image.Size, ColorDepth = ColorDepth.Depth32Bit };
        images.Images.Add(image);
        using Form window = new() { ClientSize = new Size(320, 180), ShowInTaskbar = false, AutoScaleMode = AutoScaleMode.None };
        using NativeTreeView tree = new() { Font = font, ImageList = images, BorderStyle = BorderStyle.None, Dock = DockStyle.Fill };
        TreeNode node = tree.Nodes.Add(caption);
        node.NodeFont = nodeFont;
        window.Controls.Add(tree);
        window.Show();
        tree.Refresh();
        Application.DoEvents();
        Rectangle text = GetItemRectangle(tree, node, textOnly: true);
        Size gdi = TextRenderer.MeasureText(caption, nodeFont, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
        Size ambient = TextRenderer.MeasureText(caption, font, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
        nint fontHandle = SendMessage(tree.Handle, 0x31, 0, 0);
        nint deviceContext = GetDC(tree.Handle);
        nint previousFont = SelectObject(deviceContext, fontHandle);
        NativeSize advance;
        try
        {
            GetTextExtentPoint32(deviceContext, caption, caption.Length, out advance).Should().BeTrue();
        }
        finally
        {
            SelectObject(deviceContext, previousFont);
            ReleaseDC(tree.Handle, deviceContext);
        }

        text.Width.Should().Be(advance.Width + 4, "native item extents use the ambient control HFONT plus text-slot padding, not NodeFont");
        TestContext.Progress.WriteLine($"points={points} caption={caption} style={style} text={text} gdi={gdi} ambient={ambient} nativeAdvance={advance.Width},{advance.Height} nativeExtra={text.Width - gdi.Width} advanceExtra={text.Width - advance.Width}");
    }

    [Test]
    [TestCase("Explorer::TREEVIEW", 3)]
    [TestCase("Explorer::TREEVIEW", 5)]
    [TestCase("Explorer::TREEVIEW", 6)]
    [TestCase("DarkMode_Explorer::TREEVIEW", 3)]
    [TestCase("DarkMode_Explorer::TREEVIEW", 5)]
    [TestCase("DarkMode_Explorer::TREEVIEW", 6)]
    public void Native_selected_tree_roles_should_come_from_the_tree_theme_part(string className, int state)
    {
        // NativeTreeView selects Explorer, or WinForms' DarkMode_Explorer, for native
        // item painting. Those theme-part colors need not equal SystemColors.Highlight.
        VisualStyleElement element = VisualStyleElement.CreateElement(className, TreeItemPart, state);
        VisualStyleRenderer.IsElementDefined(element).Should().BeTrue();
        VisualStyleRenderer renderer = new(element);
        Color text = renderer.GetColor(ColorProperty.TextColor);
        renderer.LastHResult.Should().Be(0);
        TestContext.Progress.WriteLine($"class={className} part={TreeItemPart} state={state} text=#{text.ToArgb():X8} highContrast={SystemInformation.HighContrast}");

        foreach (ColorProperty property in new[] { ColorProperty.BorderColor, ColorProperty.FillColor, ColorProperty.GradientColor1, ColorProperty.GradientColor2 })
        {
            Color value = renderer.GetColor(property);
            TestContext.Progress.WriteLine($"{property}=#{value.ToArgb():X8} hr={renderer.LastHResult:X8}");
        }

        foreach (IntegerProperty property in new[] { IntegerProperty.BorderSize, IntegerProperty.RoundCornerWidth, IntegerProperty.RoundCornerHeight })
        {
            int value = renderer.GetInteger(property);
            TestContext.Progress.WriteLine($"{property}={value} hr={renderer.LastHResult:X8}");
        }

        using Bitmap bitmap = new(100, 18, PixelFormat.Format24bppRgb);
        using Graphics graphics = Graphics.FromImage(bitmap);
        Color backdrop = className.StartsWith("DarkMode", StringComparison.Ordinal) ? Color.FromArgb(50, 50, 50) : Color.White;
        Padding contentMargins = renderer.GetMargins(graphics, MarginProperty.ContentMargins);
        TestContext.Progress.WriteLine($"ContentMargins={contentMargins} hr={renderer.LastHResult:X8}");
        nint deviceContext = graphics.GetHdc();
        nint brush = CreateSolidBrush(unchecked((uint)(backdrop.R | (backdrop.G << 8) | (backdrop.B << 16))));
        try
        {
            // Graphics.GetHdc uses a temporary opaque surface. Clear that actual GDI
            // surface, otherwise translucent Explorer images blend against black.
            NativeRectangle bounds = new() { Right = bitmap.Width, Bottom = bitmap.Height };
            FillRect(deviceContext, ref bounds, brush).Should().NotBe(0);
            DrawThemeBackground(renderer.Handle, deviceContext, TreeItemPart, state, ref bounds, 0).Should().Be(0);
        }
        finally
        {
            DeleteObject(brush);
            graphics.ReleaseHdc(deviceContext);
        }

        Color fill = bitmap.GetPixel(bitmap.Width / 2, bitmap.Height / 2);
        Color edge = bitmap.GetPixel(bitmap.Width / 2, 0);
        fill.A.Should().Be(byte.MaxValue);
        TestContext.Progress.WriteLine($"drawnFill=#{fill.ToArgb():X8} drawnEdge=#{edge.ToArgb():X8}");
        (int fillRgb, int edgeRgb) = (className.StartsWith("DarkMode", StringComparison.Ordinal), state) switch
        {
            (false, 3) => (0xCCE8FF, 0x0078D4),
            (false, 5) => (0xD9D9D9, 0x949494),
            (false, 6) => (0xCCE8FF, 0x000000),
            (true, 3) => (0x626262, 0x60CDFF),
            (true, 5) => (0x333333, 0x3D3D3D),
            (true, 6) => (0x505050, 0xC3C3C3),
            _ => throw new InvalidOperationException("Unexpected native tree probe case.")
        };
        fill.ToArgb().Should().Be(unchecked((int)0xFF000000) | fillRgb);
        edge.ToArgb().Should().Be(unchecked((int)0xFF000000) | edgeRgb);
        text.ToArgb().Should().Be(className.StartsWith("DarkMode", StringComparison.Ordinal) ? Color.White.ToArgb() : Color.Black.ToArgb());
        NativeTreePalette palette = NativeTreePalette.Read(className, state, backdrop);
        palette.Background.Should().Be(fill);
        palette.Foreground.Should().Be(text);
        palette.Border.Should().Be(edge);
    }

    [Test]
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void Native_tree_item_rectangles_should_preserve_source_image_padding_and_focus(bool hideSelection, bool focused)
    {
        using Font font = new("Segoe UI", 9);
        using Bitmap image = new(16, 18);
        using ImageList images = new() { ImageSize = image.Size, ColorDepth = ColorDepth.Depth32Bit };
        images.Images.Add(image);
        using Form window = new() { ClientSize = new Size(300, 180), ShowInTaskbar = false, AutoScaleMode = AutoScaleMode.None };
        using NativeTreeView tree = new()
        {
            Name = "tree",
            Font = font,
            BorderStyle = BorderStyle.None,
            Bounds = new Rectangle(0, 0, 240, 140),
            ImageList = images,
            FullRowSelect = true,
            HideSelection = hideSelection,
            BackColor = Color.White,
            ForeColor = Color.Black
        };
        using Button other = new() { Name = "other", Bounds = new Rectangle(0, 145, 100, 25), Text = "Other" };
        TreeNode root = tree.Nodes.Add("Branches");
        root.Nodes.Add("main");
        window.Controls.Add(tree);
        window.Controls.Add(other);
        window.Show();
        window.Activate();
        root.Expand();
        tree.SelectedNode = root;
        (focused ? (Control)tree : other).Focus().Should().BeTrue();

        long settledAt = Environment.TickCount64 + 100;
        while (Environment.TickCount64 < settledAt)
        {
            Application.DoEvents();
        }

        tree.DeviceDpi.Should().Be(96);
        tree.Focused.Should().Be(focused);
        tree.SelectedNode.Should().BeSameAs(root);
        tree.Indent.Should().Be(19);
        tree.ItemHeight.Should().Be(images.ImageSize.Height);
        Rectangle textRectangle = GetItemRectangle(tree, root, textOnly: true);
        Rectangle itemRectangle = GetItemRectangle(tree, root, textOnly: false);
        Rectangle childTextRectangle = GetItemRectangle(tree, root.Nodes[0], textOnly: true);
        itemRectangle.Left.Should().BeLessThan(textRectangle.Left);
        itemRectangle.Height.Should().Be(tree.ItemHeight);
        childTextRectangle.Left.Should().Be(textRectangle.Left + tree.Indent);
        textRectangle.Should().Be(root.Bounds);

        tree.Refresh();
        using CaptureImageResult capture = ImageCapture.Capture(window, [], []);
        Point origin = tree.PointToScreen(Point.Empty) - new Size(capture.ScreenBounds.Location);
        Point labelPadding = new(origin.X + textRectangle.Left - images.ImageSize.Width - 4, origin.Y + itemRectangle.Top + 1);
        Color background = capture.Bitmap.GetPixel(labelPadding.X, labelPadding.Y);
        background.ToArgb().Should().Be((focused ? Color.FromArgb(204, 232, 255) : hideSelection ? Color.White : Color.FromArgb(217, 217, 217)).ToArgb());
        TestContext.Progress.WriteLine($"focused={focused} hideSelection={hideSelection} item={itemRectangle} text={textRectangle} childText={childTextRectangle} imageSize={images.ImageSize} itemHeight={tree.ItemHeight} padding=#{background.ToArgb():X8}");
    }

    private static Rectangle GetItemRectangle(TreeView tree, TreeNode node, bool textOnly)
    {
        // TVM_GETITEMRECT takes the HTREEITEM in the first pointer-sized bytes of RECT.
        // Query both native roles rather than inferring item extents from a bitmap.
        nint pointer = Marshal.AllocHGlobal(Marshal.SizeOf<NativeRectangle>());
        try
        {
            Marshal.WriteIntPtr(pointer, node.Handle);
            SendMessage(tree.Handle, GetItemRectangleMessage, textOnly ? 1 : 0, pointer).Should().NotBe(0);
            NativeRectangle rectangle = Marshal.PtrToStructure<NativeRectangle>(pointer);
            return Rectangle.FromLTRB(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    private static Bitmap RenderTreePart(string className, int state, Color backdrop)
    {
        VisualStyleElement element = VisualStyleElement.CreateElement(className, TreeItemPart, state);
        VisualStyleRenderer.IsElementDefined(element).Should().BeTrue();
        VisualStyleRenderer renderer = new(element);
        Bitmap bitmap = new(75, 18, PixelFormat.Format24bppRgb);
        try
        {
            using Graphics graphics = Graphics.FromImage(bitmap);
            nint deviceContext = graphics.GetHdc();
            nint brush = CreateSolidBrush(unchecked((uint)(backdrop.R | (backdrop.G << 8) | (backdrop.B << 16))));
            try
            {
                NativeRectangle bounds = new() { Right = bitmap.Width, Bottom = bitmap.Height };
                FillRect(deviceContext, ref bounds, brush).Should().NotBe(0);
                DrawThemeBackground(renderer.Handle, deviceContext, TreeItemPart, state, ref bounds, 0).Should().Be(0);
            }
            finally
            {
                DeleteObject(brush);
                graphics.ReleaseHdc(deviceContext);
            }

            return bitmap;
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }
    }

    [DllImport("user32.dll", EntryPoint = "SendMessageW")]
    private static extern nint SendMessage(nint window, uint message, nint parameter, nint data);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(nint window);

    [DllImport("gdi32.dll")]
    private static extern nint CreateSolidBrush(uint color);

    [DllImport("user32.dll")]
    private static extern nint GetDC(nint window);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(nint window, nint deviceContext);

    [DllImport("gdi32.dll")]
    private static extern nint SelectObject(nint deviceContext, nint handle);

    [DllImport("gdi32.dll", EntryPoint = "GetTextExtentPoint32W", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetTextExtentPoint32(nint deviceContext, string text, int count, out NativeSize size);

    [DllImport("comctl32.dll", EntryPoint = "ImageList_Draw")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ImageListDraw(nint imageList, int index, nint deviceContext, int x, int y, uint style);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject(nint handle);

    [DllImport("user32.dll")]
    private static extern int FillRect(nint deviceContext, ref NativeRectangle bounds, nint brush);

    [DllImport("uxtheme.dll")]
    private static extern int DrawThemeBackground(nint theme, nint deviceContext, int part, int state, ref NativeRectangle bounds, nint clippingBounds);

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

    private static Color CompositeSamples(Color backdrop, Color black, Color white)
        => Color.FromArgb(
            CompositeChannel(backdrop.R, black.R, white.R),
            CompositeChannel(backdrop.G, black.G, white.G),
            CompositeChannel(backdrop.B, black.B, white.B));

    private static int CompositeChannel(byte backdrop, byte black, byte white)
        => black + ((((white - black) * backdrop) + 127) / 255);
}
