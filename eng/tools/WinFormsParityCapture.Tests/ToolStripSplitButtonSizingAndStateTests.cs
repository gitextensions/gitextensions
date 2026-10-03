using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.Json;
using AwesomeAssertions;
using GitUI;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class ToolStripSplitButtonSizingAndStateTests
{
    [Test]
    [TestCase(32, false, false)]
    [TestCase(32, true, false)]
    [TestCase(83, false, true)]
    [TestCase(83, true, true)]
    public void Source_split_button_should_keep_native_segment_and_image_bounds(int width, bool rightToLeft, bool text)
    {
        using Form host = CreateHost();
        using ProbeToolStrip strip = CreateStrip(host);
        using Bitmap image = new(16, 16);
        using (Graphics imageGraphics = Graphics.FromImage(image))
        {
            imageGraphics.Clear(Color.Magenta);
        }

        using ToolStripSplitButton item = CreateItem(strip, image, width, rightToLeft);
        item.DisplayStyle = text ? ToolStripItemDisplayStyle.ImageAndText : ToolStripItemDisplayStyle.Image;
        item.Text = text ? "Branch" : string.Empty;
        Rectangle imageRectangle = Rectangle.Empty;
        strip.Renderer.RenderItemImage += (_, e) =>
        {
            if (ReferenceEquals(e.Item, item))
            {
                imageRectangle = e.ImageRectangle;
            }
        };
        using Bitmap frame = Paint(strip);
        item.ButtonBounds.Should().Be(new Rectangle(rightToLeft ? 12 : 0, 0, width - 12, 22));
        item.SplitterBounds.Should().Be(new Rectangle(rightToLeft ? 11 : width - 12, 0, 1, 22));
        item.DropDownButtonBounds.Should().Be(new Rectangle(rightToLeft ? 0 : width - 11, 0, 11, 22));
        item.DropDownButtonWidth.Should().Be(11);
        imageRectangle.Size.Should().Be(new Size(16, 16));
        item.ButtonBounds.Contains(imageRectangle).Should().BeTrue();
        if (!text)
        {
            imageRectangle.Should().Be(new Rectangle(rightToLeft ? 14 : 2, 3, 16, 16));
        }

        Record(frame, new { inputMode = "sourceLayoutAndRenderer", width, rightToLeft, text, item.ButtonBounds, item.SplitterBounds, item.DropDownButtonBounds, imageRectangle });
    }

    [Test]
    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public void Source_dropdown_should_open_on_down_and_survive_the_same_mouse_up(bool rightToLeft, bool professional)
    {
        using Form host = CreateHost();
        using ProbeToolStrip strip = CreateStrip(host);
        if (professional)
        {
            strip.RenderMode = ToolStripRenderMode.Professional;
        }

        using Bitmap image = new(16, 16);
        using ToolStripSplitButton item = CreateItem(strip, image, 32, rightToLeft);
        int commands = 0;
        item.ButtonClick += (_, _) => commands++;
        Point position = strip.ItemPoint(item, item.DropDownButtonBounds);
        try
        {
            host.Show();
            host.Activate();
            Application.DoEvents();
            item.Select();
            item.ButtonSelected.Should().BeTrue("the private button Selected property delegates to the owner, not a separate hover half");
            item.DropDownButtonSelected.Should().BeTrue();
            strip.Press(position, MouseButtons.Left);
            Application.DoEvents();
            item.DropDown.Visible.Should().BeTrue("native ShowDropDown(mousePush:true) occurs during OnMouseDown");
            item.DropDownButtonPressed.Should().BeTrue();
            item.ButtonPressed.Should().BeFalse();
            commands.Should().Be(0);
            using Bitmap opened = Paint(strip);
            strip.Release(position, MouseButtons.Left);
            Application.DoEvents();
            item.DropDown.Visible.Should().BeTrue("the first release shares the native opening mouse ID");
            commands.Should().Be(0);
            strip.Press(position, MouseButtons.Left);
            strip.Release(position, MouseButtons.Left);
            Application.DoEvents();
            item.DropDown.Visible.Should().BeFalse("a later mouse ID closes the already-open popup");
            commands.Should().Be(0);
            Record(opened, new { inputMode = "frameworkMouseDispatch", rightToLeft, professional, item.ButtonBounds, item.DropDownButtonBounds, commands });
        }
        finally
        {
            item.HideDropDown();
        }
    }

    [Test]
    [TestCase(false, MouseButtons.Left)]
    [TestCase(true, MouseButtons.Left)]
    [TestCase(false, MouseButtons.Right)]
    [TestCase(true, MouseButtons.Right)]
    [TestCase(false, MouseButtons.Middle)]
    [TestCase(true, MouseButtons.Middle)]
    public void Source_primary_should_press_without_opening_and_only_left_release_runs_its_command(bool rightToLeft, MouseButtons button)
    {
        using Form host = CreateHost();
        using ProbeToolStrip strip = CreateStrip(host);
        using Bitmap image = new(16, 16);
        using ToolStripSplitButton item = CreateItem(strip, image, 32, rightToLeft);
        int commands = 0;
        item.ButtonClick += (_, _) => commands++;
        item.Select();
        Point position = strip.ItemPoint(item, item.ButtonBounds);
        strip.Press(position, button);
        item.ButtonPressed.Should().BeTrue("the source primary Push(true) is independent of mouse-button kind");
        item.DropDownButtonPressed.Should().BeFalse();
        commands.Should().Be(0);
        using Bitmap pressed = Paint(strip);
        strip.Release(position, button);
        item.ButtonPressed.Should().BeFalse();
        commands.Should().Be(button == MouseButtons.Left ? 1 : 0);
        Record(pressed, new { inputMode = "frameworkMouseDispatch", rightToLeft, button = button.ToString(), commands, item.ButtonBounds, item.DropDownButtonBounds });
    }

    [Test]
    [TestCase(false, false, "selected")]
    [TestCase(false, false, "primaryPressed")]
    [TestCase(false, false, "dropdownOpen")]
    [TestCase(false, true, "selected")]
    [TestCase(false, true, "primaryPressed")]
    [TestCase(false, true, "dropdownOpen")]
    [TestCase(true, false, "selected")]
    [TestCase(true, false, "primaryPressed")]
    [TestCase(true, false, "dropdownOpen")]
    [TestCase(true, true, "selected")]
    [TestCase(true, true, "primaryPressed")]
    [TestCase(true, true, "dropdownOpen")]
    public void Source_renderer_should_record_actual_selected_pressed_and_open_paint(bool dark, bool professional, string state)
    {
        SystemColorMode originalMode = Application.ColorMode;
        try
        {
            Application.SetColorMode(dark ? SystemColorMode.Dark : SystemColorMode.Classic);
            Application.DoEvents();
            using Form host = CreateHost();
            using ProbeToolStrip strip = CreateStrip(host);
            strip.BackColor = SystemColors.Control;
            if (professional)
            {
                strip.RenderMode = ToolStripRenderMode.Professional;
            }

            using Bitmap image = new(16, 16);
            using ToolStripSplitButton item = CreateItem(strip, image, 32, rightToLeft: false);
            host.Show();
            host.Activate();
            Application.DoEvents();
            item.Select();
            if (state == "primaryPressed")
            {
                strip.Press(strip.ItemPoint(item, item.ButtonBounds), MouseButtons.Left);
            }
            else if (state == "dropdownOpen")
            {
                // These independent paint probes establish the actual source API
                // state, not another physical/native-activation timing assertion.
                // Mouse-down timing is independently covered above in both renderers.
                item.ShowDropDown();
            }

            try
            {
                item.ButtonSelected.Should().BeTrue();
                item.DropDownButtonSelected.Should().BeTrue();
                item.ButtonPressed.Should().Be(state == "primaryPressed");
                item.DropDownButtonPressed.Should().Be(state == "dropdownOpen");

                // Source DropDownOpening disables the owner's WM_SETREDRAW. Calling
                // its renderer directly measures real state paint without mistaking
                // a suspended DrawToBitmap client for a blank native button.
                using Bitmap frame = PaintBackground(strip, item);
                object? colors = strip.Renderer is ToolStripProfessionalRenderer renderer
                    ? new
                    {
                        renderer.ColorTable.UseSystemColors,
                        selectedBegin = Argb(renderer.ColorTable.ButtonSelectedGradientBegin),
                        selectedMiddle = Argb(renderer.ColorTable.ButtonSelectedGradientMiddle),
                        selectedEnd = Argb(renderer.ColorTable.ButtonSelectedGradientEnd),
                        pressedBegin = Argb(renderer.ColorTable.ButtonPressedGradientBegin),
                        pressedMiddle = Argb(renderer.ColorTable.ButtonPressedGradientMiddle),
                        pressedEnd = Argb(renderer.ColorTable.ButtonPressedGradientEnd),
                        selectedBorder = Argb(renderer.ColorTable.ButtonSelectedBorder),
                        pressedHighlight = Argb(renderer.ColorTable.ButtonPressedHighlight),
                        openBegin = Argb(renderer.ColorTable.MenuItemPressedGradientBegin),
                        openEnd = Argb(renderer.ColorTable.MenuItemPressedGradientEnd),
                        openBorder = Argb(renderer.ColorTable.MenuBorder),
                    }
                    : null;
                Record(frame, new
                {
                    inputMode = state == "dropdownOpen" ? "sourceShowDropDownAndRenderer" : "frameworkMouseDispatchAndRenderer",
                    captureMethod = "rendererClientBitmap",
                    dpiMode = "nativeMonitor",
                    deviceDpi = strip.DeviceDpi,
                    renderer = strip.Renderer.GetType().FullName,
                    dark,
                    professional,
                    state,
                    backdrop = Argb(strip.BackColor),
                    item.ButtonSelected,
                    item.DropDownButtonSelected,
                    item.ButtonPressed,
                    item.DropDownButtonPressed,
                    colors,
                    pixels = Enumerable.Range(0, frame.Height).Select(y => Enumerable.Range(0, frame.Width).Select(x => Argb(frame.GetPixel(x, y))).ToArray()).ToArray(),
                });
            }
            finally
            {
                item.HideDropDown();
                if (state == "primaryPressed")
                {
                    strip.Release(strip.ItemPoint(item, item.ButtonBounds), MouseButtons.Left);
                }
            }
        }
        finally
        {
            Application.SetColorMode(originalMode);
            Application.DoEvents();
        }
    }

    private static Form CreateHost()
        => new() { AutoScaleMode = AutoScaleMode.None, ClientSize = new Size(260, 80), ShowInTaskbar = false };

    private static ProbeToolStrip CreateStrip(Form host)
    {
        ProbeToolStrip strip = new()
        {
            Dock = DockStyle.None,
            AutoSize = false,
            Size = new Size(240, 25),
            GripStyle = ToolStripGripStyle.Hidden,
            Padding = Padding.Empty,
            BackColor = Color.White,
        };
        host.Controls.Add(strip);
        host.CreateControl();
        strip.CreateControl();
        _ = host.Handle;
        _ = strip.Handle;
        strip.DeviceDpi.Should().Be(96);
        return strip;
    }

    private static ToolStripSplitButton CreateItem(ProbeToolStrip strip, Image image, int width, bool rightToLeft)
    {
        ToolStripSplitButton item = new()
        {
            AutoSize = false,
            Size = new Size(width, 22),
            Margin = Padding.Empty,
            Image = image,
            DisplayStyle = ToolStripItemDisplayStyle.Image,
            RightToLeft = rightToLeft ? RightToLeft.Yes : RightToLeft.No,
        };
        item.DropDownItems.Add(new ToolStripMenuItem("Source item"));
        strip.Items.Add(item);
        strip.PerformLayout();
        item.Size = new Size(width, 22);
        return item;
    }

    private static Bitmap Paint(ToolStrip strip)
    {
        Bitmap bitmap = new(strip.Width, strip.Height, PixelFormat.Format32bppArgb);
        strip.DrawToBitmap(bitmap, strip.ClientRectangle);
        return bitmap;
    }

    private static Bitmap PaintBackground(ToolStrip strip, ToolStripSplitButton item)
    {
        Bitmap bitmap = new(item.Width, item.Height, PixelFormat.Format24bppRgb);
        bitmap.SetResolution(96, 96);
        using Graphics graphics = Graphics.FromImage(bitmap);
        nint deviceContext = graphics.GetHdc();
        Color backdrop = strip.BackColor;
        nint brush = CreateSolidBrush(unchecked((uint)(backdrop.R | (backdrop.G << 8) | (backdrop.B << 16))));
        brush.Should().NotBe(0);
        try
        {
            // UxTheme composites onto the acquired GDI surface, not Graphics.Clear.
            NativeRectangle bounds = new() { Right = item.Width, Bottom = item.Height };
            FillRect(deviceContext, ref bounds, brush).Should().NotBe(0);
            using Graphics rendererGraphics = Graphics.FromHdc(deviceContext);
            strip.Renderer.DrawSplitButton(new ToolStripItemRenderEventArgs(rendererGraphics, item));
        }
        finally
        {
            DeleteObject(brush);
            graphics.ReleaseHdc(deviceContext);
        }

        return bitmap;
    }

    private static string Argb(Color color) => $"#{color.ToArgb():X8}";

    private static void Record(Bitmap frame, object metadata)
    {
        string directory = Path.Combine(TestContext.CurrentContext.TestDirectory, "SplitButtonProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        frame.Save(Path.Combine(directory, "actual.png"), ImageFormat.Png);
        File.WriteAllText(Path.Combine(directory, "probe.json"), JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);
        TestContext.Out.WriteLine($"splitButtonProbeEvidence={directory}");
        TestContext.Out.WriteLine(JsonSerializer.Serialize(metadata));
    }

    private sealed class ProbeToolStrip : ToolStripEx
    {
        public Point ItemPoint(ToolStripItem item, Rectangle region)
            => new(item.Bounds.X + region.X + (region.Width / 2), item.Bounds.Y + region.Y + (region.Height / 2));

        public void Press(Point position, MouseButtons button)
            => OnMouseDown(new MouseEventArgs(button, 1, position.X, position.Y, 0));

        public void Release(Point position, MouseButtons button)
            => OnMouseUp(new MouseEventArgs(button, 1, position.X, position.Y, 0));
    }

    [DllImport("gdi32.dll")]
    private static extern nint CreateSolidBrush(uint color);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(nint handle);

    [DllImport("user32.dll")]
    private static extern int FillRect(nint deviceContext, ref NativeRectangle bounds, nint brush);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
