using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.Json;
using AwesomeAssertions;
using GitExtensions.ParityCapture;
using GitUI.UserControls;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class NativeTreeSearchPaintTests
{
    [Test]
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public void Native_search_node_colors_should_preserve_native_label_extent_and_selection_precedence(bool dark, bool focused)
    {
        SystemColorMode originalMode = Application.ColorMode;
        try
        {
            Application.SetColorMode(dark ? SystemColorMode.Dark : SystemColorMode.Classic);
            Application.ColorMode.Should().Be(dark ? SystemColorMode.Dark : SystemColorMode.Classic);
            Application.IsDarkModeEnabled.Should().Be(dark);
            using Font font = new("Segoe UI", 9);
            using Bitmap image = new(16, 18);
            using ImageList images = new() { ImageSize = image.Size, ColorDepth = ColorDepth.Depth32Bit };
            images.Images.Add(image);
            using Form window = new() { AutoScaleMode = AutoScaleMode.None, ClientSize = new Size(300, 180), ShowInTaskbar = false };
            using NativeTreeView tree = new()
            {
                Font = font,
                BorderStyle = BorderStyle.None,
                Bounds = new Rectangle(0, 0, 260, 140),
                ImageList = images,
                FullRowSelect = true,
                HideSelection = true,
            };
            using Button other = new() { Bounds = new Rectangle(0, 145, 100, 25), Text = "Other" };
            TreeNode node = tree.Nodes.Add("Branches");
            node.Nodes.Add("main");
            node.BackColor = SystemColors.Info;
            node.ForeColor = SystemColors.InfoText;
            window.Controls.Add(tree);
            window.Controls.Add(other);
            window.Show();
            window.Activate();
            node.Expand();
            tree.SelectedNode = node;
            (focused ? (Control)tree : other).Focus().Should().BeTrue();

            // Native activation/focus changes queue client painting after Show.
            // Settle those real messages before PrintWindow, as the EDIT chrome
            // fixture does; a title-only result is not a verified tree capture.
            long paintingStartedAt = Environment.TickCount64;
            long paintingSettledAt = paintingStartedAt + 100;
            while (Environment.TickCount64 < paintingSettledAt)
            {
                Application.DoEvents();
            }

            window.Refresh();
            tree.Refresh();
            window.Update();
            tree.Update();
            long paintingSettlementMilliseconds = Environment.TickCount64 - paintingStartedAt;
            tree.Focused.Should().Be(focused);
            tree.DeviceDpi.Should().Be(96);
            window.DeviceDpi.Should().Be(96);

            string directory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "TreeSearchProbe", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            using CaptureImageResult capture = ImageCapture.Capture(window, [], []);
            capture.Method.Should().Be(CaptureMethod.PrintWindow, "this native client/chrome assertion requires the full-content window path");
            capture.Bitmap.Save(Path.Combine(directory, "search.png"), ImageFormat.Png);
            Point origin = tree.PointToScreen(Point.Empty) - new Size(capture.ScreenBounds.Location);
            Rectangle text = node.Bounds;
            Color label = Read(text.Right - 2, text.Top + 2);
            Color icon = Read(text.Left - 8, text.Top + 2);
            Color right = Read(text.Right + 10, text.Top + 2);
            Color hierarchy = Read(0, text.Top + 2);
            File.WriteAllText(Path.Combine(directory, "metadata.json"), JsonSerializer.Serialize(new
            {
                inputMode = "programmaticNativeSelection",
                dpiMode = "nativeMonitor",
                captureMethod = capture.Method,
                dark,
                focused,
                colorMode = Application.ColorMode.ToString(),
                nativeDarkModeEnabled = Application.IsDarkModeEnabled,
                deviceDpi = tree.DeviceDpi,
                windowDeviceDpi = window.DeviceDpi,
                paintingSettlementMilliseconds,
                nativeWindowVisible = IsWindowVisible(window.Handle),
                nativeTreeVisible = IsWindowVisible(tree.Handle),
                nativeFocusHandle = GetFocus().ToInt64(),
                expectedFocusHandle = (focused ? tree.Handle : other.Handle).ToInt64(),
                textBounds = new { text.X, text.Y, text.Width, text.Height },
                window = Argb(tree.BackColor),
                info = Argb(SystemColors.Info),
                infoText = Argb(SystemColors.InfoText),
                label = Argb(label),
                icon = Argb(icon),
                right = Argb(right),
                hierarchy = Argb(hierarchy),
            }, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.Out.WriteLine($"diagnostics={directory} dark={dark} focused={focused} text={text} window={Argb(tree.BackColor)} info={Argb(SystemColors.Info)} infoText={Argb(SystemColors.InfoText)} label={Argb(label)} icon={Argb(icon)} right={Argb(right)} hierarchy={Argb(hierarchy)} paintingSettlementMilliseconds={paintingSettlementMilliseconds} nativeWindowVisible={IsWindowVisible(window.Handle)} nativeTreeVisible={IsWindowVisible(tree.Handle)} nativeFocusHandle={GetFocus()} expectedFocusHandle={(focused ? tree.Handle : other.Handle)} captureMethod={capture.Method}");
            GetFocus().Should().Be(focused ? tree.Handle : other.Handle);
            node.BackColor.Should().Be(SystemColors.Info);
            node.ForeColor.Should().Be(SystemColors.InfoText);
            right.ToArgb().Should().Be(tree.BackColor.ToArgb());
            hierarchy.ToArgb().Should().Be(tree.BackColor.ToArgb());
            if (!focused)
            {
                label.ToArgb().Should().Be(SystemColors.Info.ToArgb());
                icon.ToArgb().Should().Be(SystemColors.Info.ToArgb(), "the real native node background includes its image slot, not the hierarchy or the remainder of its row");
            }
            else
            {
                NativeTreePalette selected = NativeTreePalette.Read(tree, 3);
                label.Should().Be(selected.Background);
                icon.Should().Be(selected.Background);
            }

            return;

            Color Read(int x, int y) => capture.Bitmap.GetPixel(origin.X + x, origin.Y + y);
        }
        finally
        {
            Application.SetColorMode(originalMode);
        }
    }

    private static string Argb(Color color) => $"#{color.ToArgb():X8}";

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(nint window);

    [DllImport("user32.dll")]
    private static extern nint GetFocus();
}
