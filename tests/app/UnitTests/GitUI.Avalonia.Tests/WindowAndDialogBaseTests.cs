using System.Drawing;
using System.Text;
using System.Xml.Serialization;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Threading;
using GitUI;
using ResourceManager;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class WindowAndDialogBaseTests
{
    [TestCase(500, 500)]
    [TestCase(50, 50)]
    public void FitWindowOnScreen_should_keep_a_window_with_the_required_screen_presence(int width, int height)
    {
        Rectangle screen = new(0, 0, 1920, 1080);
        Rectangle window = new(0, 0, width, height);

        WindowPositionManager.FitWindowOnScreen(window, [screen]).Should().Be(window.Location);
    }

    [TestCase(-500, 100, 500, 500, 0, 0)]
    [TestCase(1728, 100, 500, 500, 0, 0)]
    [TestCase(-192, 100, 500, 500, -192, 100)]
    public void FitWindowOnScreen_should_fall_back_only_when_less_than_ten_percent_is_visible(
        int x,
        int y,
        int width,
        int height,
        int expectedX,
        int expectedY)
    {
        Rectangle[] screens =
        [
            Rectangle.Empty,
            new Rectangle(0, 0, 1920, 1080)
        ];

        WindowPositionManager.FitWindowOnScreen(new Rectangle(x, y, width, height), screens)
            .Should().Be(new Point(expectedX, expectedY));
    }

    [Test]
    public void Wayland_should_use_compositor_positioning_while_other_desktops_restore_coordinates()
    {
        WindowPositionManager.SupportsProgrammaticPositioningFor(isLinux: true, waylandDisplay: "wayland-0")
            .Should().BeFalse();
        WindowPositionManager.SupportsProgrammaticPositioningFor(isLinux: true, waylandDisplay: null)
            .Should().BeTrue();
        WindowPositionManager.SupportsProgrammaticPositioningFor(isLinux: false, waylandDisplay: "wayland-0")
            .Should().BeTrue();
    }

    [Test]
    public void Window_position_XML_should_round_trip_the_shared_property_names_and_state_values()
    {
        XmlSerializer serializer = new(typeof(TestWindowPositionList));
        TestWindowPositionList expected = new();
        expected.AddOrUpdate(new WindowPosition(
            new Rectangle(10, 20, 640, 480),
            144,
            WindowState.Maximized,
            "FormBrowse"));
        using MemoryStream stream = new();

        serializer.Serialize(stream, expected);
        string xml = Encoding.UTF8.GetString(stream.ToArray());
        stream.Position = 0;
        TestWindowPositionList actual = (TestWindowPositionList)serializer.Deserialize(stream)!;

        xml.Should().Contain("<DeviceDpi>144</DeviceDpi>");
        xml.Should().Contain("<State>Maximized</State>");
        actual.Get("FormBrowse")!.Rect.Should().Be(new Rectangle(10, 20, 640, 480));
        actual.Get("FormBrowse")!.DeviceDpi.Should().Be(144);
        actual.Get("FormBrowse")!.State.Should().Be(WindowState.Maximized);
    }

    [AvaloniaTest]
    public void Restored_sizable_window_should_restore_logical_size_and_visible_position()
    {
        WindowPosition position = new(new Rectangle(100, 120, 500, 400), 120, WindowState.Normal, "MockForm");
        RecordingPositionManager manager = new(position);
        MockForm form = new(enablePositionRestore: true)
        {
            Width = 300,
            Height = 200,
            CanResize = true
        };
        GitExtensionsForm.GitExtensionsFormTestAccessor accessor = form.GetGitExtensionsFormTestAccessor();
        accessor.WindowPositionManager = manager;
        accessor.GetScreensWorkingArea = () => [new Rectangle(0, 0, 1920, 1080)];
        accessor.SupportsProgrammaticPositioning = () => true;

        form.InvokeRestorePosition();

        form.Width.Should().Be(400);
        form.Height.Should().Be(320);
        form.WindowStartupLocation.Should().Be(WindowStartupLocation.Manual);
        form.Position.Should().Be(new Avalonia.PixelPoint(80, 96));
    }

    [AvaloniaTest]
    public void Restored_fixed_window_should_keep_designer_size()
    {
        WindowPosition position = new(new Rectangle(100, 120, 500, 400), 120, WindowState.Normal, "MockForm");
        RecordingPositionManager manager = new(position);
        MockForm form = new(enablePositionRestore: true)
        {
            Width = 300,
            Height = 200,
            CanResize = false
        };
        GitExtensionsForm.GitExtensionsFormTestAccessor accessor = form.GetGitExtensionsFormTestAccessor();
        accessor.WindowPositionManager = manager;
        accessor.GetScreensWorkingArea = () => [new Rectangle(0, 0, 1920, 1080)];

        form.InvokeRestorePosition();

        form.Width.Should().Be(300);
        form.Height.Should().Be(200);
    }

    [AvaloniaTest]
    public void Wayland_restore_should_keep_size_and_state_but_delegate_position_to_the_compositor()
    {
        WindowPosition position = new(new Rectangle(-5000, -5000, 600, 450), 144, WindowState.Normal, "MockForm");
        MockForm form = new(enablePositionRestore: true)
        {
            Width = 300,
            Height = 200,
            CanResize = true,
            WindowStartupLocation = WindowStartupLocation.Manual
        };
        GitExtensionsForm.GitExtensionsFormTestAccessor accessor = form.GetGitExtensionsFormTestAccessor();
        accessor.WindowPositionManager = new RecordingPositionManager(position);
        accessor.GetScreensWorkingArea = () => [new Rectangle(0, 0, 1920, 1080)];
        accessor.SupportsProgrammaticPositioning = () => false;

        form.InvokeRestorePosition();

        form.Width.Should().Be(400);
        form.Height.Should().Be(300);
        form.WindowStartupLocation.Should().Be(WindowStartupLocation.CenterScreen);
    }

    [AvaloniaTest]
    public void Closing_positioned_window_should_save_once()
    {
        RecordingPositionManager manager = new(position: null);
        MockForm form = new(enablePositionRestore: true);
        form.GetGitExtensionsFormTestAccessor().WindowPositionManager = manager;

        form.Show();
        form.Close();
        form.Close();

        manager.SaveCount.Should().Be(1);
    }

    [AvaloniaTest]
    public void Base_window_should_route_default_and_cancel_buttons_and_preserve_logical_size_across_DPI_changes()
    {
        MockForm form = new(enablePositionRestore: false)
        {
            Width = 500,
            Height = 300,
            Text = "Window title"
        };
        Button accept = new();
        Button cancel = new();
        int acceptClicks = 0;
        int cancelClicks = 0;
        accept.Click += (_, _) => acceptClicks++;
        cancel.Click += (_, _) => cancelClicks++;
        form.AcceptButton = accept;
        form.CancelButton = cancel;
        form.Show();
        try
        {
            accept.IsDefault.Should().BeTrue();
            form.Title.Should().Be("Window title");
            form.Icon.Should().NotBeNull();

            form.SetRenderScaling(2);
            form.Width.Should().Be(500);
            form.Height.Should().Be(300);

            form.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.None, keySymbol: null);
            form.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.None, keySymbol: null);
            acceptClicks.Should().Be(1);
            cancelClicks.Should().Be(1);
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void Modal_window_should_keep_owner_center_startup_and_return_dialog_result()
    {
        MockForm owner = new(enablePositionRestore: false);
        ModalForm dialog = new()
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        owner.Show();
        try
        {
            Dispatcher.UIThread.Post(() => dialog.DialogResult = DialogResult.OK);

            DialogResult result = dialog.ShowDialog(owner);

            result.Should().Be(DialogResult.OK);
            dialog.OwnerObservedOnOpen.Should().BeSameAs(owner);
            dialog.WindowStartupLocation.Should().Be(WindowStartupLocation.CenterOwner);
        }
        finally
        {
            owner.Close();
        }
    }

    [AvaloniaTest]
    public void Cancelled_close_should_leave_window_open_until_confirmation_allows_it()
    {
        ConfirmingForm form = new()
        {
            MinWidth = 320,
            MinHeight = 180
        };
        form.Show();

        form.Close();
        form.IsVisible.Should().BeTrue();
        form.MinWidth.Should().Be(320);
        form.MinHeight.Should().Be(180);

        form.AllowClose = true;
        form.Close();
        form.IsVisible.Should().BeFalse();
    }

    [AvaloniaTest]
    public void Busy_and_wait_cursor_scopes_should_restore_prior_window_state_when_nested()
    {
        MockForm first = new(enablePositionRestore: false);
        MockForm second = new(enablePositionRestore: false)
        {
            Cursor = new Cursor(StandardCursorType.Cross)
        };
        first.Show();
        second.Show();
        try
        {
            Cursor? firstCursor = first.Cursor;
            Cursor? secondCursor = second.Cursor;

            using (FormBusyScope.Enter(first))
            {
                first.IsEnabled.Should().BeFalse();
                first.Cursor.Should().NotBe(firstCursor);
                Cursor? busyCursor = first.Cursor;
                Cursor helpCursor = new(StandardCursorType.Help);

                using (FormBusyScope.Enter(first, helpCursor))
                {
                    first.Cursor.Should().BeSameAs(helpCursor);
                }

                first.Cursor.Should().BeSameAs(busyCursor);
            }

            first.IsEnabled.Should().BeTrue();
            first.Cursor.Should().BeSameAs(firstCursor);
            second.Cursor.Should().BeSameAs(secondCursor);
        }
        finally
        {
            first.Close();
            second.Close();
        }
    }

    private sealed class MockForm : GitExtensionsForm
    {
        public MockForm(bool enablePositionRestore)
            : base(enablePositionRestore)
        {
        }

        public void InvokeRestorePosition()
        {
            RestorePosition();
        }
    }

    private sealed class RecordingPositionManager(WindowPosition? position) : IWindowPositionManager
    {
        public int SaveCount { get; private set; }

        public WindowPosition? LoadPosition(Window form)
            => position;

        public void SavePosition(Window form)
            => SaveCount++;
    }

    private sealed class ModalForm : GitExtensionsFormBase
    {
        public WindowBase? OwnerObservedOnOpen { get; private set; }

        protected override void OnOpened(EventArgs e)
        {
            OwnerObservedOnOpen = Owner;
            base.OnOpened(e);
        }
    }

    private sealed class ConfirmingForm : GitExtensionsFormBase
    {
        public bool AllowClose { get; set; }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            e.Cancel = !AllowClose;
            base.OnClosing(e);
        }
    }

    public sealed class TestWindowPositionList : WindowPositionList
    {
        public TestWindowPositionList()
        {
        }
    }
}
