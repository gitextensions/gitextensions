using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using AwesomeAssertions;
using GitExtensions.Extensibility;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.UserControls;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

/// <summary>
///  Separates native cached label clipping from the enclosing tree viewport.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class NativeTreeLabelOverflowNativeTests
{
    private const string Caption = "repository (Avalonia_Port)";
    private const uint GetItemRectangleMessage = 0x1104;

    [Test]
    public void Actual_source_Browse_should_record_styled_node_fonts_and_cached_label_extents()
    {
        using OriginalBrowseNativeProbe probe = OriginalBrowseNativeProbe.Create("NativeTreeLabelOverflowProbe");
        probe.WithOriginalBrowse(new BrowseArguments(), (form, getThreadException, closeAndDrain) =>
        {
            NativeTreeView tree = DescendantControls(form).OfType<NativeTreeView>().Single(control => control.Name == "treeMain");
            using ApplicationContext loop = new();
            using System.Windows.Forms.Timer timer = new() { Interval = 80 };
            Stopwatch elapsed = Stopwatch.StartNew();
            Exception? failure = null;
            bool finished = false;
            string? previousLabels = null;
            int stableObservations = 0;
            form.Shown += (_, _) => timer.Start();
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                try
                {
                    if (getThreadException() is Exception exception)
                    {
                        ExceptionDispatchInfo.Capture(exception).Throw();
                    }

                    if (elapsed.Elapsed > TimeSpan.FromSeconds(30))
                    {
                        throw new TimeoutException("The original Browse tree font probe did not settle within its owned message loop.");
                    }

                    TreeNode[] nodes = [.. DescendantNodes(tree.Nodes).Where(node => node.IsVisible)];
                    if (!nodes.Any(node => node.NodeFont is not null))
                    {
                        return;
                    }

                    string labels = string.Join('|', nodes.Select(node => node.Text));
                    stableObservations = labels == previousLabels ? stableObservations + 1 : 0;
                    previousLabels = labels;
                    if (stableObservations < 3)
                    {
                        return;
                    }

                    form.DeviceDpi.Should().Be(96);
                    object[] snapshots = [.. nodes.Select(node => new
                    {
                        node.Text,
                        node.Level,
                        font = new { tree.Font.Name, tree.Font.SizeInPoints, style = tree.Font.Style.ToString() },
                        nodeFont = node.NodeFont is Font font ? new { font.Name, font.SizeInPoints, style = font.Style.ToString() } : null,
                        labelBounds = GetItemRectangle(tree, node),
                        ambientNativeAdvance = GetNativeAdvance(tree, node.Text),
                        styledNativeAdvance = GetNativeAdvance(tree, node.Text, node.NodeFont ?? tree.Font),
                    })];
                    File.WriteAllText(Path.Combine(probe.EvidenceDirectory, "actual-browse-labels.json"),
                        JsonSerializer.Serialize(new { tree.ClientSize, tree.ItemHeight, tree.Indent, snapshots }, new JsonSerializerOptions { WriteIndented = true }));
                    using CaptureImageResult frame = ImageCapture.Capture(form, [], []);
                    frame.Bitmap.Save(Path.Combine(probe.EvidenceDirectory, "actual-browse.png"), ImageFormat.Png);
                    Finish();
                }
                catch (Exception exception)
                {
                    failure = exception;
                    Finish();
                }
                finally
                {
                    if (!finished)
                    {
                        timer.Start();
                    }
                }
            };
            Application.Idle += OnIdle;
            try
            {
                form.StartPosition = FormStartPosition.Manual;
                form.Location = new Point(30, 30);
                form.ClientSize = new Size(1800, 950);
                Application.Run(loop);
            }
            finally
            {
                timer.Stop();
                Application.Idle -= OnIdle;
            }

            if (failure is not null)
            {
                ExceptionDispatchInfo.Capture(failure).Throw();
            }

            void OnIdle(object? sender, EventArgs args)
            {
                Application.Idle -= OnIdle;
                try
                {
                    form.Show();
                    NativeTestWindowActivation.Activate(form, waitForPresentation: false);
                }
                catch (Exception exception)
                {
                    failure = exception;
                    Finish();
                }
            }

            void Finish()
            {
                if (finished)
                {
                    return;
                }

                finished = true;
                timer.Stop();
                try
                {
                    closeAndDrain();
                }
                catch (Exception exception)
                {
                    failure ??= exception;
                }
                finally
                {
                    loop.ExitThread();
                }
            }
        });
    }

    [TestCase(FontStyle.Bold, false)]
    [TestCase(FontStyle.Bold | FontStyle.Italic, false)]
    [TestCase(FontStyle.Bold, true)]
    [TestCase(FontStyle.Bold | FontStyle.Italic, true)]
    public void Source_styled_label_should_preserve_cached_item_and_viewport_clipping(FontStyle style, bool populateAfterShow)
    {
        SystemColorMode originalMode = Application.ColorMode;
        Point originalCursor = NativeMethods.GetCursorPosition();
        string directory = Path.Combine(TestContext.CurrentContext.TestDirectory, "NativeTreeLabelOverflowProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        TestContext.Out.WriteLine($"nativeTreeLabelOverflowEvidence={directory}");
        try
        {
            Application.SetColorMode(SystemColorMode.Classic);
            using Font ambientFont = new("Segoe UI", 9);
            using Font nodeFont = new(ambientFont, style);
            using Bitmap image = new(16, 18);
            using ImageList images = new() { ImageSize = image.Size, ColorDepth = ColorDepth.Depth32Bit };
            images.Images.Add(image);
            using Form window = new()
            {
                AutoScaleMode = AutoScaleMode.None,
                ClientSize = new Size(400, 180),
                BackColor = Color.Magenta,
                ShowInTaskbar = false,
            };
            using NativeTreeView tree = new()
            {
                Bounds = new Rectangle(8, 8, 340, 120),
                BorderStyle = BorderStyle.None,
                Font = ambientFont,
                ForeColor = Color.Black,
                BackColor = Color.White,
                ImageList = images,
                FullRowSelect = true,
                HideSelection = true,
            };
            using Button other = new() { Bounds = new Rectangle(8, 140, 100, 25), Text = "Other" };
            TreeNode node = new(Caption) { NodeFont = nodeFont };
            if (!populateAfterShow)
            {
                tree.Nodes.Add(node);
            }

            window.Controls.Add(tree);
            window.Controls.Add(other);
            window.Show();
            NativeTestWindowActivation.Activate(window);
            if (populateAfterShow)
            {
                // Nodes.FillTreeViewNode adds an empty native item, then applies its
                // text and styled NodeFont while the live source tree is updating.
                tree.BeginUpdate();
                node = tree.Nodes.Add(string.Empty);
                node.Text = Caption;
                node.NodeFont = nodeFont;
                tree.EndUpdate();
            }

            other.Focus().Should().BeTrue();
            NativeMethods.SetCursorPosition(window.PointToScreen(new Point(390, 170)));
            Settle(window, tree);
            window.DeviceDpi.Should().Be(96);
            tree.DeviceDpi.Should().Be(96);
            tree.Focused.Should().BeFalse("the source's hidden inactive selection must not alter the node text paint");
            Rectangle wideLabel = GetItemRectangle(tree, node);
            int wideInkRight;
            using (CaptureImageResult frame = ImageCapture.Capture(window, [], []))
            {
                frame.Bitmap.Save(Path.Combine(directory, "wide.png"), ImageFormat.Png);
                wideInkRight = GetInkRight(frame, tree, wideLabel);
                File.WriteAllText(Path.Combine(directory, "wide.json"), JsonSerializer.Serialize(new
                {
                    acquisition = frame.Method.ToString(),
                    style = style.ToString(),
                    populateAfterShow,
                    ambientFont = new { ambientFont.Name, ambientFont.SizeInPoints },
                    nodeFont = new { nodeFont.Name, nodeFont.SizeInPoints },
                    caption = Caption,
                    tree.ClientSize,
                    labelBounds = wideLabel,
                    rightmostPaintedTextPixel = wideInkRight,
                    textPaintBeyondCachedSlot = wideInkRight >= wideLabel.Right,
                }, new JsonSerializerOptions { WriteIndented = true }));
            }

            wideInkRight.Should().BeGreaterThanOrEqualTo(wideLabel.Left);
            wideInkRight.Should().BeLessThan(wideLabel.Right,
                "the actual source TREEVIEW clips styled text at its cached native item rectangle");
            tree.Width = wideLabel.Left + (wideLabel.Width / 2);
            Settle(window, tree);
            Rectangle narrowLabel = GetItemRectangle(tree, node);
            narrowLabel.Should().Be(wideLabel, "resizing the viewport must not replace the node's cached label extent");
            wideInkRight.Should().BeGreaterThanOrEqualTo(tree.ClientSize.Width,
                "the narrowed viewport must actually cut through the original text paint");
            using (CaptureImageResult frame = ImageCapture.Capture(window, [], []))
            {
                frame.Bitmap.Save(Path.Combine(directory, "narrow.png"), ImageFormat.Png);
                Point origin = tree.PointToScreen(Point.Empty) - new Size(frame.ScreenBounds.Location);
                for (int y = wideLabel.Top; y < wideLabel.Bottom; y++)
                {
                    for (int x = tree.ClientSize.Width; x <= wideInkRight; x++)
                    {
                        frame.Bitmap.GetPixel(origin.X + x, origin.Y + y).ToArgb().Should().Be(window.BackColor.ToArgb(),
                            "native text must not paint beyond its actual HWND viewport at {0},{1}", x, y);
                    }
                }

                File.WriteAllText(Path.Combine(directory, "narrow.json"), JsonSerializer.Serialize(new
                {
                    acquisition = frame.Method.ToString(),
                    tree.ClientSize,
                    labelBounds = narrowLabel,
                    rightmostPaintedTextPixel = GetInkRight(frame, tree, narrowLabel),
                    outsideViewportBackground = $"#{window.BackColor.ToArgb():X8}",
                }, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        finally
        {
            NativeMethods.SetCursorPosition(originalCursor);
            Application.SetColorMode(originalMode);
        }
    }

    private static int GetInkRight(CaptureImageResult frame, NativeTreeView tree, Rectangle label)
    {
        Point origin = tree.PointToScreen(Point.Empty) - new Size(frame.ScreenBounds.Location);
        int rightmost = -1;
        for (int y = label.Top; y < label.Bottom; y++)
        {
            for (int x = label.Left; x < tree.ClientSize.Width; x++)
            {
                if (frame.Bitmap.GetPixel(origin.X + x, origin.Y + y).ToArgb() != tree.BackColor.ToArgb())
                {
                    rightmost = Math.Max(rightmost, x);
                }
            }
        }

        return rightmost;
    }

    private static Rectangle GetItemRectangle(NativeTreeView tree, TreeNode node)
    {
        nint pointer = Marshal.AllocHGlobal(Marshal.SizeOf<NativeRectangle>());
        try
        {
            // TVM_GETITEMRECT consumes the complete pointer-sized HTREEITEM in RECT.
            Marshal.WriteIntPtr(pointer, node.Handle);
            SendMessage(tree.Handle, GetItemRectangleMessage, 1, pointer).Should().NotBe(0);
            NativeRectangle bounds = Marshal.PtrToStructure<NativeRectangle>(pointer);
            return Rectangle.FromLTRB(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    private static void Settle(Form window, NativeTreeView tree)
    {
        long deadline = Environment.TickCount64 + 100;
        while (Environment.TickCount64 < deadline)
        {
            Application.DoEvents();
            Thread.Sleep(1);
        }

        window.Refresh();
        tree.Refresh();
        window.Update();
        tree.Update();
    }

    private static IEnumerable<Control> DescendantControls(Control parent)
        => parent.Controls.Cast<Control>().SelectMany(control => DescendantControls(control).Prepend(control));

    private static IEnumerable<TreeNode> DescendantNodes(TreeNodeCollection nodes)
        => nodes.Cast<TreeNode>().SelectMany(node => DescendantNodes(node.Nodes).Prepend(node));

    private static int GetNativeAdvance(NativeTreeView tree, string caption, Font? paintFont = null)
    {
        nint deviceContext = GetDC(tree.Handle);
        deviceContext.Should().NotBe(0);
        nint nativeFont = paintFont?.ToHfont() ?? SendMessage(tree.Handle, 0x31, 0, 0);
        nint previousFont = SelectObject(deviceContext, nativeFont);
        try
        {
            GetTextExtentPoint32(deviceContext, caption, caption.Length, out NativeSize size).Should().BeTrue();
            return size.Width;
        }
        finally
        {
            SelectObject(deviceContext, previousFont);
            ReleaseDC(tree.Handle, deviceContext);
            if (paintFont is not null)
            {
                DeleteObject(nativeFont);
            }
        }
    }

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

    [DllImport("user32.dll", EntryPoint = "SendMessageW")]
    private static extern nint SendMessage(nint window, uint message, nint wordParameter, nint bounds);

    [DllImport("user32.dll")]
    private static extern nint GetDC(nint window);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(nint window, nint deviceContext);

    [DllImport("gdi32.dll")]
    private static extern nint SelectObject(nint deviceContext, nint value);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(nint value);

    [DllImport("gdi32.dll", EntryPoint = "GetTextExtentPoint32W", CharSet = CharSet.Unicode)]
    private static extern bool GetTextExtentPoint32(nint deviceContext, string text, int length, out NativeSize size);
}
