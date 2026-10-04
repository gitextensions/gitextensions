using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Text.Json;
using AwesomeAssertions;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;
using GitExtUtils;
using GitUI;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

// These are original ToolStripEx consumer routes, not physical-pointer or full
// FormBrowse menu-navigation evidence. The owner dispatches each item-relative event.
[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class ToolStripOverflowInputTests
{
    private const string PortableSettingName = "IsPortable";
    private const string DialogKeyMethodName = "ProcessDialogKey";
    private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    [TestCase(false, MouseButtons.Left)]
    [TestCase(true, MouseButtons.Left)]
    [TestCase(false, MouseButtons.Right)]
    [TestCase(true, MouseButtons.Right)]
    [TestCase(false, MouseButtons.Middle)]
    [TestCase(true, MouseButtons.Middle)]
    public void Source_overflow_mouse_dispatch_should_open_on_left_down_keep_first_release_and_close_the_next_down(
        bool rightToLeft, MouseButtons button)
    {
        WithIsolatedSettings(() =>
        {
            using Form host = CreateHost();
            using ProbeToolStrip strip = CreateStrip(host, rightToLeft);
            ToolStripDropDown popup = strip.OverflowButton.DropDown;
            using NativePopupLifetime lifetime = new(host, popup);
            ToolStripItem[] sourceItems = strip.Items.Cast<ToolStripItem>().ToArray();
            int commands = 0;
            sourceItems[0].Click += (_, _) => commands++;
            List<object> transitions = [];
            List<string> closeReasons = [];
            popup.Closed += (_, args) => closeReasons.Add(args.CloseReason.ToString());
            ShowAndSettle(host, strip);
            ToolStripItem[] overflowItems = sourceItems.Where(item => item.Placement == ToolStripItemPlacement.Overflow).ToArray();
            overflowItems.Should().NotBeEmpty();
            strip.OverflowButton.Visible.Should().BeTrue();
            Point position = Center(strip.OverflowButton.Bounds);
            strip.MovePointer(position);
            strip.Press(position, button);
            transitions.Add(Snapshot(strip, popup, "firstDown"));
            bool expectedOpen = button == MouseButtons.Left;
            popup.Visible.Should().Be(expectedOpen,
                "ToolStripDropDownButton handles its left mouse-down before the owner can dispatch the first release");
            if (expectedOpen)
            {
                VerifyOwnedPopup(strip, popup, sourceItems, overflowItems);
                RetainFrame(host, popup, "first-down", new { rightToLeft, button = button.ToString(), inputMode = "sourceProtectedOwnerMouseDispatch" });
            }

            strip.Release(position, button);
            transitions.Add(Snapshot(strip, popup, "firstUp"));
            popup.Visible.Should().Be(expectedOpen,
                "the first left release shares the native owner mouse ID which opened the overflow");
            commands.Should().Be(0);
            if (expectedOpen)
            {
                strip.Press(position, button);
                transitions.Add(Snapshot(strip, popup, "secondDown"));
                popup.Visible.Should().BeFalse("the second left mouse-down closes the already-open native dropdown");
                strip.Release(position, button);
                transitions.Add(Snapshot(strip, popup, "secondUp"));
                popup.Visible.Should().BeFalse();
                closeReasons.Should().Contain(ToolStripDropDownCloseReason.AppClicked.ToString());
            }

            commands.Should().Be(0, "the overflow button is not an authored command item");
            strip.Items.Cast<ToolStripItem>().Should().Equal(sourceItems);
            sourceItems.Should().OnlyContain(item => ReferenceEquals(item.Owner, strip));
            RetainFrame(host, popup, "completed", new
            {
                rightToLeft,
                button = button.ToString(),
                inputMode = "sourceProtectedOwnerMouseDispatch",
                transitions,
                closeReasons,
                commands,
            });
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Source_overflow_Escape_should_close_the_real_popup_without_replacing_its_hosted_combo(bool rightToLeft)
    {
        WithIsolatedSettings(() =>
        {
            using Form host = CreateHost();
            using ProbeToolStrip strip = CreateStrip(host, rightToLeft);
            ToolStripDropDown popup = strip.OverflowButton.DropDown;
            using NativePopupLifetime lifetime = new(host, popup);
            ToolStripItem[] sourceItems = strip.Items.Cast<ToolStripItem>().ToArray();
            ToolStripComboBox combo = sourceItems.OfType<ToolStripComboBox>().Single();
            ComboBox actualCombo = combo.ComboBox;
            List<string> closeReasons = [];
            popup.Closed += (_, args) => closeReasons.Add(args.CloseReason.ToString());
            ShowAndSettle(host, strip);
            Point position = Center(strip.OverflowButton.Bounds);
            strip.MovePointer(position);
            strip.Press(position, MouseButtons.Left);
            strip.Release(position, MouseButtons.Left);
            popup.Visible.Should().BeTrue();
            ToolStripItem[] overflowItems = sourceItems.Where(item => item.Placement == ToolStripItemPlacement.Overflow).ToArray();
            VerifyOwnedPopup(strip, popup, sourceItems, overflowItems);
            combo.GetCurrentParent().Should().BeSameAs(popup);
            combo.ComboBox.Should().BeSameAs(actualCombo);
            actualCombo.Parent.Should().BeSameAs(popup);
            actualCombo.Text.Should().Be("retained filter");
            Rectangle comboBounds = actualCombo.Bounds;
            comboBounds.Width.Should().BeGreaterThan(0);
            comboBounds.Height.Should().BeGreaterThan(0);
            popup.ClientRectangle.Contains(comboBounds).Should().BeTrue();
            RetainFrame(host, popup, "before-escape", new { rightToLeft, comboBounds, inputMode = "sourceProtectedOwnerMouseDispatch" });
            MethodInfo processDialogKey = popup.GetType().GetMethod(DialogKeyMethodName, PrivateInstance)
                ?? throw new MissingMethodException(popup.GetType().FullName, DialogKeyMethodName);
            object? handled = processDialogKey.Invoke(popup, [Keys.Escape]);
            handled.Should().Be(true, "the real ToolStripDropDown.ProcessDialogKey consumes Escape");
            popup.Visible.Should().BeFalse();
            closeReasons.Should().Contain(ToolStripDropDownCloseReason.Keyboard.ToString());
            combo.ComboBox.Should().BeSameAs(actualCombo);
            combo.Owner.Should().BeSameAs(strip);
            actualCombo.Text.Should().Be("retained filter");
            RetainFrame(host, popup, "after-escape", new
            {
                rightToLeft,
                inputMode = "sourceProtectedDropDownDialogKey",
                handled,
                closeReasons,
                comboBounds,
                currentParent = combo.GetCurrentParent()?.GetType().FullName,
                controlParent = actualCombo.Parent?.GetType().FullName,
                snapshot = Snapshot(strip, popup, "afterEscape"),
                scope = "Escape closure and same hosted-control identity; no general menu traversal or editing-key claim",
            });
        });
    }

    private static Form CreateHost() => new()
    {
        AutoScaleMode = AutoScaleMode.Dpi,
        ClientSize = new Size(400, 180),
        StartPosition = FormStartPosition.CenterScreen,
        ShowInTaskbar = false,
        Text = "Original overflow input consumer",
    };

    private static ProbeToolStrip CreateStrip(Form host, bool rightToLeft)
    {
        ProbeToolStrip strip = new()
        {
            AutoSize = false,
            Dock = DockStyle.None,
            GripStyle = ToolStripGripStyle.Visible,
            GripEnabled = false,
            GripMargin = Padding.Empty,
            Size = new Size(180, 25),
            RightToLeft = rightToLeft ? RightToLeft.Yes : RightToLeft.No,
        };
        strip.Items.Add(new ToolStripButton("main command")
        {
            Name = "btnMainCommand",
            AutoSize = false,
            Size = new Size(140, 22),
        });
        strip.Items.Add(new ToolStripComboBox
        {
            Name = "cbxRetainedFilter",
            AutoSize = false,
            Size = new Size(100, 25),
            Text = "retained filter",
        });
        strip.Items.Add(new ToolStripButton("overflow command")
        {
            Name = "btnOverflowCommand",
            AutoSize = false,
            Size = new Size(90, 22),
        });
        host.Controls.Add(strip);
        return strip;
    }

    private static void ShowAndSettle(Form host, ProbeToolStrip strip)
    {
        host.Show();
        host.Activate();
        host.Focus();
        Stopwatch activation = Stopwatch.StartNew();
        do
        {
            Application.DoEvents();
            Thread.Sleep(10);
        }
        while (activation.ElapsedMilliseconds < 100);

        host.DeviceDpi.Should().Be(96);
        strip.DeviceDpi.Should().Be(96);
        Form.ActiveForm.Should().BeSameAs(host);
        host.ContainsFocus.Should().BeTrue();
        strip.PerformLayout();
    }

    private static void VerifyOwnedPopup(ToolStrip strip, ToolStripDropDown popup, ToolStripItem[] sourceItems, ToolStripItem[] overflowItems)
    {
        popup.Visible.Should().BeTrue();
        popup.OwnerItem.Should().BeSameAs(strip.OverflowButton);
        FlowLayoutSettings flow = popup.LayoutSettings as FlowLayoutSettings
            ?? throw new InvalidOperationException("The actual overflow must retain the native flow layout.");
        flow.FlowDirection.Should().Be(FlowDirection.LeftToRight);
        flow.WrapContents.Should().BeTrue();
        strip.Items.Cast<ToolStripItem>().Should().Equal(sourceItems);
        foreach (ToolStripItem item in overflowItems)
        {
            item.Owner.Should().BeSameAs(strip);
            item.GetCurrentParent().Should().BeSameAs(popup);
            item.Bounds.Width.Should().BeGreaterThan(0);
            item.Bounds.Height.Should().BeGreaterThan(0);
        }
    }

    private static Point Center(Rectangle bounds) => new(bounds.X + (bounds.Width / 2), bounds.Y + (bounds.Height / 2));

    private static object Snapshot(ToolStrip strip, ToolStripDropDown popup, string stage) => new
    {
        stage,
        popup.Visible,
        popup.Bounds,
        popup.ClientSize,
        strip.ContainsFocus,
        strip.OverflowButton.Selected,
        strip.OverflowButton.Pressed,
        strip.OverflowButton.HasDropDownItems,
        overflowBounds = strip.OverflowButton.Bounds,
        items = strip.Items.Cast<ToolStripItem>().Select(item => new
        {
            item.Name,
            placement = item.Placement.ToString(),
            item.Bounds,
            owner = item.Owner?.GetType().FullName,
            currentParent = item.GetCurrentParent()?.GetType().FullName,
            controlParent = (item as ToolStripControlHost)?.Control.Parent?.GetType().FullName,
        }).ToArray(),
    };

    private static void RetainFrame(Form host, ToolStripDropDown popup, string stage, object metadata)
    {
        string directory = Path.Combine(TestContext.CurrentContext.TestDirectory, "OverflowInputProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        using CaptureImageResult frame = ImageCapture.Capture(host, popup.Visible ? [popup] : [], []);
        frame.Bitmap.Save(Path.Combine(directory, "actual-owner-popup.png"), ImageFormat.Png);
        string json = JsonSerializer.Serialize(new
        {
            stage,
            metadata,
            captureMethod = frame.Method.ToString(),
            frame.ScreenBounds,
            hostDpi = host.DeviceDpi,
            popupDpi = popup.DeviceDpi,
            dpiMode = "nativeMonitor",
            popup.Visible,
            popup.Bounds,
            note = "Unmodified real native window capture; mouse/dialog-key input is framework dispatch, not physical interaction.",
        }, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(directory, "probe.json"), json);
        TestContext.Out.WriteLine($"overflowInputEvidence={directory}");
        TestContext.Out.WriteLine(json);
        frame.Method.Should().Be(GitExtensions.ParityCapture.CaptureMethod.PrintWindow);
    }

    private static void WithIsolatedSettings(Action action)
    {
        string directory = Path.Combine(Path.GetTempPath(), "GitExtensions.OverflowInputProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string settingsPath = Path.Combine(directory, "GitExtensions.settings");
        File.WriteAllText(settingsPath, "<?xml version=\"1.0\" encoding=\"utf-8\"?><dictionary />");
        InitializeAppSettingsWithoutRealConfiguration(directory);
        using GitExtSettingsCache cache = new(settingsPath, autoSave: false);
        DistributedSettings settings = new(lowerPriority: null, cache, SettingLevel.Unknown);
        AppSettings.UsingContainer(settings, action);

        // The first initialization may retain a watcher on its initial cache. Keep
        // the exact temporary root until the coordinating cleanup disposes the host.
        TestContext.Out.WriteLine($"isolated original settings retained: {directory}");
    }

    private static void InitializeAppSettingsWithoutRealConfiguration(string directory)
    {
        Type settingsType = typeof(AppSettings).Assembly.GetType("GitCommands.Properties.Settings", throwOnError: true)
            ?? throw new TypeLoadException("GitCommands.Properties.Settings");
        ApplicationSettingsBase configuration = settingsType.GetProperty("Default", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as ApplicationSettingsBase
            ?? throw new MissingMemberException(settingsType.FullName, "Default");
        _ = configuration[PortableSettingName];
        SettingsPropertyValue portable = configuration.PropertyValues[PortableSettingName]
            ?? throw new MissingMemberException(settingsType.FullName, PortableSettingName);
        object originalPortableValue = portable.PropertyValue;
        bool originalDirty = portable.IsDirty;
        FieldInfo frameworkPath = typeof(Application).GetField("s_executablePath", PrivateStatic)
            ?? throw new MissingFieldException(typeof(Application).FullName, "s_executablePath");
        object? originalFrameworkPath = frameworkPath.GetValue(null);
        string actualExecutablePath = Application.ExecutablePath;
        string isolatedExecutablePath = Path.Combine(directory, "GitExtensions.exe");
        try
        {
            // AppSettings chooses its first cache in its static constructor, before
            // its public isolation scope exists. Alias only that first path safely.
            portable.PropertyValue = true;
            frameworkPath.SetValue(null, isolatedExecutablePath);
            _ = AppSettings.SettingsContainer;
            FieldInfo sourcePath = typeof(AppSettings).GetField("_applicationExecutablePath", PrivateStatic)
                ?? throw new MissingFieldException(typeof(AppSettings).FullName, "_applicationExecutablePath");
            if (Equals(sourcePath.GetValue(null), isolatedExecutablePath))
            {
                sourcePath.SetValue(null, actualExecutablePath);
            }
        }
        finally
        {
            frameworkPath.SetValue(null, originalFrameworkPath);
            portable.PropertyValue = originalPortableValue;
            portable.IsDirty = originalDirty;
        }
    }

    private sealed class ProbeToolStrip : ToolStripEx
    {
        public void MovePointer(Point position) => OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, position.X, position.Y, 0));

        public void Press(Point position, MouseButtons button) => OnMouseDown(new MouseEventArgs(button, 1, position.X, position.Y, 0));

        public void Release(Point position, MouseButtons button) => OnMouseUp(new MouseEventArgs(button, 1, position.X, position.Y, 0));
    }

    private sealed class NativePopupLifetime(Form host, ToolStripDropDown popup) : IDisposable
    {
        public void Dispose()
        {
            if (!popup.IsDisposed)
            {
                // Parameterless Close also exits the native global menu-mode queue.
                popup.Close();
                Application.DoEvents();
            }

            host.Close();
            Application.DoEvents();
        }
    }
}
