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

    [DllImport("user32.dll", EntryPoint = "SendMessageW")]
    private static extern nint SendMessage(nint window, uint message, nint parameter, nint data);

    [DllImport("gdi32.dll")]
    private static extern nint CreateSolidBrush(uint color);

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
}
