using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitUI.Compat;
using Size = Avalonia.Size;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class NativeToolStripSplitButtonTests
{
    [AvaloniaTest]
    [TestCase(32, false)]
    [TestCase(32, true)]
    [TestCase(83, false)]
    [TestCase(83, true)]
    public void Native_segments_should_be_actual_framework_part_bounds_not_independent_Fluent_minima(int width, bool rightToLeft)
    {
        NativeToolStripSplitButton button = NewButton(width, rightToLeft);
        Window window = NewWindow(button);
        try
        {
            window.Show();
            window.UpdateLayout();
            Button primary = Part(button, "PART_PrimaryButton");
            Button secondary = Part(button, "PART_SecondaryButton");
            Border splitter = button.GetVisualDescendants().OfType<Border>().Single(part => part.Name == "SeparatorBorder");
            Rect primaryBounds = new(rightToLeft ? 12 : 0, 0, width - 12, 22);
            Rect dropdownBounds = new(rightToLeft ? 0 : width - 11, 0, 11, 22);
            Rect splitterBounds = new(rightToLeft ? 11 : width - 12, 0, 1, 22);
            primary.Bounds.Should().Be(primaryBounds);
            secondary.Bounds.Should().Be(dropdownBounds);
            splitter.Bounds.Should().Be(splitterBounds);
            button.ButtonBounds.Should().Be(primaryBounds);
            button.DropDownButtonBounds.Should().Be(dropdownBounds);
            button.SplitterBounds.Should().Be(splitterBounds);
            primary.Content.Should().BeSameAs(button.Content);
            primary.MinWidth.Should().Be(0);
            secondary.MinWidth.Should().Be(0);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Native_icon_should_paint_at_source_two_by_three_origin_without_layout_border_inset(bool rightToLeft)
    {
        NativeToolStripSplitButton button = NewButton(32, rightToLeft);
        Window window = NewWindow(button);
        try
        {
            window.Show();
            window.UpdateLayout();
            Image image = button.GetVisualDescendants().OfType<Image>().Single();
            Point relative = image.TranslatePoint(default, button)!.Value;
            relative.Should().Be(new Point(rightToLeft ? 14 : 2, 3));
            image.Bounds.Size.Should().Be(new Size(16, 16));
            using WriteableBitmap frame = Capture(window);
            int first = rightToLeft ? 14 : 2;
            ReadPixel(frame, new PixelPoint(first, 3)).Should().Be(Colors.Magenta);
            ReadPixel(frame, new PixelPoint(first + 15, 18)).Should().Be(Colors.Magenta);
            ReadPixel(frame, new PixelPoint(first - 1, 3)).Should().Be(Colors.White);
            ReadPixel(frame, new PixelPoint(first, 2)).Should().Be(Colors.White);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Dropdown_should_open_on_down_survive_its_release_and_close_on_a_later_release(bool rightToLeft)
    {
        NativeToolStripSplitButton button = NewButton(32, rightToLeft);
        MenuFlyout flyout = (MenuFlyout)button.Flyout!;
        int commands = 0;
        button.Click += (_, _) => commands++;
        Window window = NewWindow(button);
        try
        {
            window.Show();
            window.UpdateLayout();
            Point point = Centre(button.DropDownButtonBounds);
            window.MouseMove(point);
            window.MouseDown(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            flyout.IsOpen.Should().BeTrue("the source opens during MouseDown, before its release-click");
            button.DropDownButtonPressed.Should().BeTrue();
            button.ButtonPressed.Should().BeFalse();
            button.ButtonSelected.Should().BeTrue();
            commands.Should().Be(0);
            window.MouseUp(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            flyout.IsOpen.Should().BeTrue("the release shares the opening mouse ID");
            commands.Should().Be(0);
            window.MouseDown(point, MouseButton.Left);
            flyout.IsOpen.Should().BeTrue("source closing occurs on the subsequent MouseUp, not MouseDown");
            window.MouseUp(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            flyout.IsOpen.Should().BeFalse();
            commands.Should().Be(0);
        }
        finally
        {
            flyout.Hide();
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false, MouseButton.Left)]
    [TestCase(true, MouseButton.Left)]
    [TestCase(false, MouseButton.Right)]
    [TestCase(true, MouseButton.Right)]
    [TestCase(false, MouseButton.Middle)]
    [TestCase(true, MouseButton.Middle)]
    public void Primary_should_push_for_each_source_button_kind_but_only_left_release_runs_the_command(bool rightToLeft, MouseButton mouseButton)
    {
        NativeToolStripSplitButton button = NewButton(32, rightToLeft);
        int commands = 0;
        button.Click += (_, _) => commands++;
        Window window = NewWindow(button);
        try
        {
            window.Show();
            window.UpdateLayout();
            Point point = Centre(button.ButtonBounds);
            window.MouseMove(point);
            window.MouseDown(point, mouseButton);
            button.ButtonPressed.Should().BeTrue();
            button.DropDownButtonPressed.Should().BeFalse();
            commands.Should().Be(0);
            window.MouseUp(point, mouseButton);
            button.ButtonPressed.Should().BeFalse();
            commands.Should().Be(mouseButton == MouseButton.Left ? 1 : 0);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Splitter_should_push_the_source_private_button_without_running_a_command(bool rightToLeft)
    {
        NativeToolStripSplitButton button = NewButton(32, rightToLeft);
        int commands = 0;
        button.Click += (_, _) => commands++;
        Window window = NewWindow(button);
        try
        {
            window.Show();
            window.UpdateLayout();
            Point point = Centre(button.SplitterBounds);
            window.MouseMove(point);
            window.MouseDown(point, MouseButton.Left);
            button.ButtonPressed.Should().BeTrue();
            button.Flyout!.IsOpen.Should().BeFalse();
            window.MouseUp(point, MouseButton.Left);
            commands.Should().Be(0);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Both_selected_flags_should_follow_the_source_owner_not_the_hovered_half()
    {
        NativeToolStripSplitButton button = NewButton();
        Window window = NewWindow(button);
        try
        {
            window.Show();
            window.UpdateLayout();
            foreach (Rect segment in new[] { button.ButtonBounds, button.DropDownButtonBounds })
            {
                window.MouseMove(Centre(segment));
                button.ButtonSelected.Should().BeTrue();
                button.DropDownButtonSelected.Should().BeTrue();
            }

            window.MouseMove(new Point(100, 50));
            button.ButtonSelected.Should().BeFalse();
            button.DropDownButtonSelected.Should().BeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Keyboard_selection_should_retain_the_framework_keyboard_routes_without_a_control_focus_rectangle()
    {
        NativeToolStripSplitButton button = NewButton();
        Window window = NewWindow(button);
        try
        {
            window.Show();
            window.UpdateLayout();
            button.Focus(NavigationMethod.Tab).Should().BeTrue();
            button.ButtonSelected.Should().BeTrue();
            button.DropDownButtonSelected.Should().BeTrue();
            button.FocusAdorner.Should().BeNull("a native ToolStrip item is selected/hot, not a separately dotted Button");
            button.Focusable.Should().BeTrue("ToolStrip menu/Alt keyboard selection is not equivalent to disabling focus");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(Key.Enter)]
    [TestCase(Key.Space)]
    public void Opt_in_should_preserve_the_framework_primary_keyboard_command_route(Key key)
    {
        NativeToolStripSplitButton button = NewButton();
        int commands = 0;
        button.Click += (_, _) => commands++;
        Window window = NewWindow(button);
        try
        {
            window.Show();
            window.UpdateLayout();
            button.Focus(NavigationMethod.Tab);
            button.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = key });
            commands.Should().Be(0);
            button.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyUpEvent, Key = key });
            commands.Should().Be(1);
            button.Flyout!.IsOpen.Should().BeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Native_layout_should_rearrange_live_RTL_and_width_changes_without_replacing_parts_or_content()
    {
        NativeToolStripSplitButton button = NewButton();
        Window window = NewWindow(button);
        try
        {
            window.Show();
            window.UpdateLayout();
            Button primary = Part(button, "PART_PrimaryButton");
            Button secondary = Part(button, "PART_SecondaryButton");
            object? content = button.Content;
            button.FlowDirection = FlowDirection.RightToLeft;
            button.Width = 83;
            window.UpdateLayout();
            Part(button, "PART_PrimaryButton").Should().BeSameAs(primary);
            Part(button, "PART_SecondaryButton").Should().BeSameAs(secondary);
            button.Content.Should().BeSameAs(content);
            primary.Bounds.Should().Be(new Rect(12, 0, 71, 22));
            secondary.Bounds.Should().Be(new Rect(0, 0, 11, 22));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Disabled_and_detached_owners_should_cancel_native_pressed_state_and_release_popup_pass_through_scope()
    {
        NativeToolStripSplitButton button = NewButton();
        MenuFlyout oldFlyout = (MenuFlyout)button.Flyout!;
        Window window = NewWindow(button);
        try
        {
            window.Show();
            window.UpdateLayout();
            oldFlyout.OverlayInputPassThroughElement.Should().BeSameAs(button);
            window.MouseDown(Centre(button.ButtonBounds), MouseButton.Right);
            button.ButtonPressed.Should().BeTrue();
            button.IsEnabled = false;
            button.ButtonPressed.Should().BeFalse();
            window.MouseUp(Centre(button.ButtonBounds), MouseButton.Right);
            MenuFlyout replacement = new() { Items = { new MenuItem { Header = "Replacement" } } };
            button.Flyout = replacement;
            oldFlyout.OverlayInputPassThroughElement.Should().BeNull();
            replacement.OverlayInputPassThroughElement.Should().BeSameAs(button);
            window.Content = null;
            replacement.OverlayInputPassThroughElement.Should().BeNull();
            button.ButtonPressed.Should().BeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Native_adapter_should_not_change_non_opted_in_framework_consumers_or_leave_part_widths_after_opt_out()
    {
        NativeToolStripSplitButton button = NewButton();
        Window window = NewWindow(button);
        try
        {
            window.Show();
            window.UpdateLayout();
            Button secondary = Part(button, "PART_SecondaryButton");
            secondary.Width.Should().Be(11);
            button.UseNativeToolStripLayout = false;
            window.UpdateLayout();
            secondary.Width.Should().Be(double.NaN);
            secondary.MinWidth.Should().Be(13);
            button.Classes.Should().NotContain("gitextensions-native-toolstrip-split-button");
            button.Classes.Should().NotContain("gitextensions-native-professional-toolstrip-split-button");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Professional_paint_should_keep_whole_owner_hover_and_source_primary_pressed_deflation(bool rightToLeft)
    {
        NativeToolStripSplitButton button = NewButton(32, rightToLeft);
        SetProfessionalBrushes(button);
        Window window = NewWindow(button);
        try
        {
            window.Show();
            window.UpdateLayout();
            window.MouseMove(Centre(button.ButtonBounds));
            using (WriteableBitmap selected = Capture(window))
            {
                ReadPixel(selected, new PixelPoint(5, 1)).Should().Be(Color.Parse("#2F4159"));
                ReadPixel(selected, new PixelPoint(23, 1)).Should().Be(Color.Parse("#2F4159"));
                ReadPixel(selected, new PixelPoint(rightToLeft ? 11 : 20, 10)).Should().Be(Color.Parse("#C0C0C0"));
                ReadPixel(selected, new PixelPoint(5, 0)).Should().Be(Color.Parse("#2864B4"));
            }

            window.MouseDown(Centre(button.ButtonBounds), MouseButton.Right);
            using WriteableBitmap pressed = Capture(window);
            int primaryX = rightToLeft ? 20 : 5;
            int dropdownX = rightToLeft ? 5 : 25;
            ReadPixel(pressed, new PixelPoint(primaryX, 1)).Should().Be(Color.Parse("#2D4B73"));
            ReadPixel(pressed, new PixelPoint(dropdownX, 1)).Should().Be(Color.Parse("#2F4159"));
            ReadPixel(pressed, new PixelPoint(primaryX, 0)).Should().Be(Color.Parse("#2864B4"));
            window.MouseUp(Centre(button.ButtonBounds), MouseButton.Right);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Professional_open_state_should_use_source_menu_title_gradient_and_border_not_primary_pressed_accent()
    {
        NativeToolStripSplitButton button = NewButton();
        SetProfessionalBrushes(button);
        button.ProfessionalOpenBrush = Brushes.Green;
        button.ProfessionalOpenBorderBrush = Brushes.Yellow;
        Window window = NewWindow(button);
        try
        {
            window.Show();
            window.UpdateLayout();
            button.Flyout!.ShowAt(button);
            Dispatcher.UIThread.RunJobs();
            button.DropDownButtonPressed.Should().BeTrue();
            using WriteableBitmap opened = Capture(window);
            ReadPixel(opened, new PixelPoint(5, 1)).Should().Be(Colors.Green);
            ReadPixel(opened, new PixelPoint(25, 1)).Should().Be(Colors.Green);
            ReadPixel(opened, new PixelPoint(20, 1)).Should().Be(Colors.Green);
            ReadPixel(opened, new PixelPoint(5, 0)).Should().Be(Colors.Yellow);
        }
        finally
        {
            button.Flyout!.Hide();
            window.Close();
        }
    }

    private static NativeToolStripSplitButton NewButton(int width = 32, bool rightToLeft = false)
    {
        DrawingImage icon = new()
        {
            Drawing = new GeometryDrawing
            {
                Brush = Brushes.Magenta,
                Geometry = new RectangleGeometry(new Rect(0, 0, 16, 16)),
            },
        };
        NativeToolStripSplitButton button = new()
        {
            UseNativeToolStripLayout = true,
            Width = width,
            Height = 22,
            MinHeight = 0,
            Margin = new Thickness(0),
            FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            Content = new Image { Width = 16, Height = 16, Source = icon },
            Flyout = new MenuFlyout { Items = { new MenuItem { Header = "Source item" } } },
        };
        button.Classes.Add("gitextensions-toolbar-button");
        return button;
    }

    private static Window NewWindow(Control button)
        => new() { Width = 180, Height = 90, Content = new Canvas { Background = Brushes.White, Children = { button } } };

    private static Button Part(NativeToolStripSplitButton button, string name)
        => button.GetVisualDescendants().OfType<Button>().Single(part => part.Name == name);

    private static Point Centre(Rect rect) => new(rect.X + (rect.Width / 2), rect.Y + (rect.Height / 2));

    private static void SetProfessionalBrushes(NativeToolStripSplitButton button)
    {
        button.UseSystemVisualStyle = false;
        button.ProfessionalSelectedBrush = Brush.Parse("#2F4159");
        button.ProfessionalPressedBrush = Brush.Parse("#2D4B73");
        button.ProfessionalBorderBrush = Brush.Parse("#2864B4");
        button.ProfessionalSplitterBrush = Brush.Parse("#C0C0C0");
    }

    private static WriteableBitmap Capture(Window window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        WriteableBitmap frame = window.CaptureRenderedFrame() ?? throw new InvalidOperationException("The split-button frame is unavailable.");
        string? evidenceRoot = Environment.GetEnvironmentVariable("GITEXT_SPLIT_STATE_EVIDENCE");
        if (!string.IsNullOrEmpty(evidenceRoot))
        {
            if (!Path.IsPathFullyQualified(evidenceRoot))
            {
                frame.Dispose();
                throw new InvalidOperationException("Split-button state evidence requires an explicit absolute directory.");
            }

            Directory.CreateDirectory(evidenceRoot);
            string caseName = string.Concat(TestContext.CurrentContext.Test.Name.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));
            using FileStream stream = File.Create(Path.Combine(evidenceRoot, $"{caseName}-{Guid.NewGuid():N}.png"));
            frame.Save(stream, PngBitmapEncoderOptions.Default);
        }

        return frame;
    }

    private static Color ReadPixel(WriteableBitmap frame, PixelPoint point)
    {
        using ILockedFramebuffer buffer = frame.Lock();
        byte[] pixel = new byte[4];
        Marshal.Copy(IntPtr.Add(buffer.Address, (point.Y * buffer.RowBytes) + (point.X * 4)), pixel, 0, pixel.Length);
        return buffer.Format == PixelFormat.Bgra8888
            ? Color.FromArgb(pixel[3], pixel[2], pixel[1], pixel[0])
            : Color.FromArgb(pixel[3], pixel[0], pixel[1], pixel[2]);
    }
}
