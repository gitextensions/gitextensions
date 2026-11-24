using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using AwesomeAssertions;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;
using GitExtensions.ParityCapture;
using GitUI;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

/// <summary>
///  Observes the original filter dropdown algorithms without a repository or real settings.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class FilterToolBarMenuLayoutNativeTests
{
    private const string FilterTypeName = "GitUI.UserControls.FilterToolBar";
    private const string SettingsFileName = "GitExtensions.settings";
    private const string PortableSettingName = "IsPortable";
    private const int InputStageIntervalMilliseconds = 80;
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private static readonly string[] MenuOwnerNames =
    [
        "tsbtnAdvancedFilter", "tssbtnShowBranches", "tsddbtnBranchFilter", "tsddbtnRevisionFilter",
    ];

    [TestCase(9, false, false)]
    [TestCase(9, false, true)]
    [TestCase(9, true, false)]
    [TestCase(9, true, true)]
    [TestCase(11, false, false)]
    [TestCase(11, false, true)]
    [TestCase(11, true, false)]
    [TestCase(11, true, true)]
    [TestCase(14, false, false)]
    [TestCase(14, false, true)]
    [TestCase(14, true, false)]
    [TestCase(14, true, true)]
    public void FilterToolBar_menus_should_retain_original_preferred_size_padding_and_renderer_metrics(
        int points, bool rightToLeft, bool professional)
    {
        WithIsolatedSettings(directory =>
        {
            SystemColorMode originalMode = Application.ColorMode;
            Exception? threadException = null;
            ThreadExceptionEventHandler onThreadException = (_, args) => threadException ??= args.Exception;
            Application.ThreadException += onThreadException;
            try
            {
                Application.SetColorMode(SystemColorMode.Classic);
                using Font font = new("Segoe UI", points, FontStyle.Regular, GraphicsUnit.Point);
                AppSettings.Font = font;
                AppSettings.RevisionFilterDropdowns = [];
                using Form host = new()
                {
                    AutoScaleMode = AutoScaleMode.None,
                    ClientSize = new Size(1400, 600),
                    ShowInTaskbar = false,
                    TopMost = true,
                    StartPosition = FormStartPosition.CenterScreen,
                    Text = "Original filter dropdown layout probe",
                };
                using ToolStripEx strip = CreateOriginalFilter();
                strip.Font = font;
                strip.RightToLeft = rightToLeft ? RightToLeft.Yes : RightToLeft.No;
                if (professional)
                {
                    strip.RenderMode = ToolStripRenderMode.Professional;
                }

                host.Controls.Add(strip);
                host.Show();
                NativeTestWindowActivation.Activate(host);
                Application.DoEvents();
                host.DeviceDpi.Should().Be(96);
                strip.DeviceDpi.Should().Be(96);
                foreach (string name in MenuOwnerNames)
                {
                    ToolStripDropDownItem owner = strip.Items.Cast<ToolStripItem>().OfType<ToolStripDropDownItem>()
                        .Single(item => item.Name == name);
                    ToolStripDropDownMenu popup = owner.DropDown as ToolStripDropDownMenu
                        ?? throw new InvalidOperationException("The original filter item must retain its native menu.");
                    try
                    {
                        NativeTestWindowActivation.Activate(host);
                        owner.ShowDropDown();
                        Application.DoEvents();
                        NativeTestWindowActivation.AssertForeground(host);
                        popup.PerformLayout();
                        popup.Refresh();
                        popup.Update();
                        popup.Visible.Should().BeTrue();
                        popup.DeviceDpi.Should().Be(96);
                        popup.Font.Should().Be(font);
                        Size maximum = ReadProperty<Size>(popup, "MaxItemSize");
                        int trim = ReadProperty<int>(popup, "PaddingToTrim");
                        popup.Width.Should().Be(maximum.Width - trim + popup.Padding.Horizontal);
                        popup.Height.Should().Be(popup.Items.Cast<ToolStripItem>().Where(item => item.Available)
                            .Sum(item => item.GetPreferredSize(Size.Empty).Height + item.Margin.Vertical) + popup.Padding.Vertical);
                        popup.Items.OfType<ToolStripMenuItem>().Should().OnlyContain(item =>
                            item.Bounds.Size == item.GetPreferredSize(Size.Empty));

                        object metadata = new
                        {
                            scope = "actual original FilterToolBar; no Bind, Git, user settings, fabricated sizing or renderer-pixel inference",
                            hostTopMost = host.TopMost,
                            points,
                            rightToLeft,
                            professional,
                            owner = name,
                            nativeMode = Application.ColorMode.ToString(),
                            frameworkAssembly = typeof(ToolStrip).Assembly.FullName,
                            frameworkPath = typeof(ToolStrip).Assembly.Location,
                            renderer = popup.Renderer.GetType().FullName,
                            ownerRenderer = strip.Renderer.GetType().FullName,
                            popup.Bounds,
                            popup.ClientSize,
                            popup.DisplayRectangle,
                            popup.Padding,
                            popup.Margin,
                            popup.ShowCheckMargin,
                            popup.ShowImageMargin,
                            popup.ImageScalingSize,
                            font = DescribeFont(popup.Font),
                            maximumItemSize = maximum,
                            paddingToTrim = trim,
                            imageMargin = ReadProperty<Rectangle>(popup, "ImageMargin"),
                            checkRectangle = ReadProperty<Rectangle>(popup, "CheckRectangle"),
                            imageRectangle = ReadProperty<Rectangle>(popup, "ImageRectangle"),
                            textRectangle = ReadProperty<Rectangle>(popup, "TextRectangle"),
                            arrowRectangle = ReadProperty<Rectangle>(popup, "ArrowRectangle"),
                            measuredTab = TextRenderer.MeasureText("\t", popup.Font),
                            cachedTabWidth = ReadField<int>(popup, "_tabWidth"),
                            preferred = popup.GetPreferredSize(Size.Empty),
                            items = popup.Items.Cast<ToolStripItem>().Select(DescribeItem).ToArray(),
                        };
                        string prefix = Path.Combine(directory, name);
                        File.WriteAllText(prefix + ".json", JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }));
                        RecordWindowReceipts(host, popup, prefix + ".windows.json");
                        using CaptureImageResult capture = ImageCapture.Capture(host, [popup], []);
                        capture.Bitmap.Save(prefix + ".png", ImageFormat.Png);
                        File.WriteAllText(prefix + ".capture.json", JsonSerializer.Serialize(new
                        {
                            method = capture.Method.ToString(),
                            capture.ScreenBounds,
                            capture.PrimaryScreenBounds,
                            popupBounds = popup.Bounds,
                            dpiMode = "nativeMonitor",
                        }, new JsonSerializerOptions { WriteIndented = true }));
                        TestContext.Out.WriteLine($"original filter menu metrics and unscaled frame retained: {prefix}");
                        threadException.Should().BeNull("a native UI-thread failure invalidates these observations");
                    }
                    finally
                    {
                        popup.Close();
                        Application.DoEvents();
                    }
                }

                host.Close();
                Application.DoEvents();
            }
            finally
            {
                Application.ThreadException -= onThreadException;
                Application.SetColorMode(originalMode);
                Application.DoEvents();
            }
        });
    }

    [TestCase(false, false, false)]
    [TestCase(false, true, false)]
    [TestCase(true, false, false)]
    [TestCase(true, true, false)]
    [TestCase(false, false, true)]
    [TestCase(false, true, true)]
    [TestCase(true, false, true)]
    [TestCase(true, true, true)]
    public void FilterToolBar_should_hot_track_actual_disabled_rows_and_observe_queued_menu_keys(
        bool professional, bool checkedItem, bool rightToLeft)
    {
        WithIsolatedSettings(directory =>
        {
            SystemColorMode originalMode = Application.ColorMode;
            Point oldCursor = NativeMethods.GetCursorPosition();
            try
            {
                Application.SetColorMode(SystemColorMode.Classic);
                using Font font = new("Segoe UI", 9);
                AppSettings.Font = font;
                AppSettings.RevisionFilterDropdowns = [];
                using Form host = new()
                {
                    AutoScaleMode = AutoScaleMode.None,
                    ClientSize = new Size(1200, 400),
                    ShowInTaskbar = false,
                    TopMost = true,
                    StartPosition = FormStartPosition.CenterScreen,
                    RightToLeft = rightToLeft ? RightToLeft.Yes : RightToLeft.No,
                    Text = "Original disabled filter input probe",
                };
                using ToolStripEx strip = CreateOriginalFilter();
                strip.Font = font;
                if (professional)
                {
                    strip.RenderMode = ToolStripRenderMode.Professional;
                }

                host.Controls.Add(strip);
                ToolStripDropDownItem owner = strip.Items.OfType<ToolStripDropDownItem>().Single(item => item.Name == "tsddbtnBranchFilter");
                ToolStripDropDownMenu popup = (ToolStripDropDownMenu)owner.DropDown;
                ToolStripMenuItem disabled = popup.Items.OfType<ToolStripMenuItem>().First();
                disabled.Enabled = false;
                disabled.Checked = checkedItem;
                RunOriginalInputLoop(host, owner, popup, disabled, professional, checkedItem, rightToLeft, directory);
            }
            finally
            {
                Application.SetColorMode(originalMode);
                NativeMethods.SetCursorPosition(oldCursor);
                Application.DoEvents();
            }
        });
    }

    private static void RunOriginalInputLoop(Form host, ToolStripDropDownItem owner, ToolStripDropDownMenu popup,
        ToolStripMenuItem disabled, bool professional, bool checkedItem, bool rightToLeft, string directory)
    {
        using System.Windows.Forms.Timer timer = new() { Interval = InputStageIntervalMilliseconds };
        Stopwatch elapsed = Stopwatch.StartNew();
        ToolStripMenuItem enabled = popup.Items.OfType<ToolStripMenuItem>().Last();
        ToolStripMenuItem firstEnabled = popup.Items.OfType<ToolStripMenuItem>().First(item => item.Enabled);
        List<object> observations = [];
        bool enabledSelected = false;
        bool disabledBeforePointer = false;
        bool pointerSelected = false;
        nint cursorWindow = 0;
        nint popupWindow = 0;
        Rectangle popupBounds = Rectangle.Empty;
        Rectangle itemBounds = Rectangle.Empty;
        string? renderer = null;
        string? selectedAfterEnd = null;
        string? selectedAfterHome = null;
        string? selectedAfterDown = null;
        object? pointerCapture = null;
        Exception? failure = null;
        int stage = 0;
        host.Shown += (_, _) =>
        {
            try
            {
                NativeTestWindowActivation.Activate(host);
                owner.ShowDropDown();
                host.DeviceDpi.Should().Be(96);
                popup.DeviceDpi.Should().Be(96);
                popup.RightToLeft.Should().Be(rightToLeft ? RightToLeft.Yes : RightToLeft.No);
                popupWindow = popup.Handle;
                popupBounds = popup.Bounds;
                itemBounds = disabled.Bounds;
                renderer = popup.Renderer.GetType().FullName;
                observations.Add(ObserveInput(host, popup, "opened"));
                QueuePointer(host, popup, enabled.Bounds.Location + new Size(enabled.Width / 2, enabled.Height / 2));
                timer.Start();
            }
            catch (Exception exception)
            {
                failure = exception;
                host.Close();
            }
        };
        timer.Tick += (_, _) =>
        {
            try
            {
                if (elapsed.Elapsed > TimeSpan.FromSeconds(10))
                {
                    throw new TimeoutException("The original menu input stages exceeded their bounded owned message loop.");
                }

                switch (stage++)
                {
                    case 0:
                        observations.Add(ObserveInput(host, popup, "enabled-pointer"));
                        enabledSelected = enabled.Selected;
                        disabledBeforePointer = disabled.Selected;
                        QueuePointer(host, popup, disabled.Bounds.Location + new Size(disabled.Width / 2, disabled.Height / 2));
                        break;
                    case 1:
                        observations.Add(ObserveInput(host, popup, "disabled-pointer"));
                        pointerSelected = disabled.Selected;
                        cursorWindow = WindowFromPoint(NativeMethods.GetCursorPosition());
                        popupBounds = popup.Bounds;
                        itemBounds = disabled.Bounds;
                        popup.Refresh();
                        popup.Update();
                        RecordWindowReceipts(host, popup, Path.Combine(directory, "disabled-pointer.windows.json"));
                        using (CaptureImageResult pointerFrame = ImageCapture.Capture(host, [popup], []))
                        {
                            pointerFrame.Bitmap.Save(Path.Combine(directory, "disabled-pointer.png"), ImageFormat.Png);
                            pointerCapture = new { pointerFrame.ScreenBounds, pointerFrame.PrimaryScreenBounds, method = pointerFrame.Method.ToString() };
                        }

                        QueuePointer(host, popup, new Point(popup.Width / 2, popup.Height - 1));
                        break;
                    case 2:
                        observations.Add(ObserveInput(host, popup, "before-End"));
                        QueueKey(popup, Keys.End);
                        break;
                    case 3:
                        observations.Add(ObserveInput(host, popup, "after-End-before-Home"));
                        selectedAfterEnd = popup.Items.OfType<ToolStripMenuItem>().SingleOrDefault(item => item.Selected)?.Name;
                        QueueKey(popup, Keys.Home);
                        break;
                    case 4:
                        observations.Add(ObserveInput(host, popup, "after-Home-before-Down"));
                        selectedAfterHome = popup.Items.OfType<ToolStripMenuItem>().SingleOrDefault(item => item.Selected)?.Name;
                        QueueKey(popup, Keys.Down);
                        break;
                    default:
                        observations.Add(ObserveInput(host, popup, "after-Down"));
                        selectedAfterDown = popup.Items.OfType<ToolStripMenuItem>().SingleOrDefault(item => item.Selected)?.Name;
                        timer.Stop();
                        popup.Close();
                        host.Close();
                        break;
                }
            }
            catch (Exception exception)
            {
                failure = exception;
                timer.Stop();
                popup.Close();
                host.Close();
            }
        };
        try
        {
            Application.Run(host);
        }
        finally
        {
            timer.Stop();
            if (!popup.IsDisposed)
            {
                popup.Close();
            }

            File.WriteAllText(Path.Combine(directory, "disabled-input.json"), JsonSerializer.Serialize(new
            {
                scope = "actual original owned Application.Run loop, timer-separated real cursor/queued HWND moves and keys; no selection fallback",
                hostTopMost = host.TopMost,
                professional,
                checkedItem,
                rightToLeft,
                frameworkAssembly = typeof(ToolStrip).Assembly.FullName,
                frameworkVersion = FileVersionInfo.GetVersionInfo(typeof(ToolStrip).Assembly.Location).ProductVersion,
                renderer,
                popupBounds,
                itemBounds,
                pointerCapture,
                enabledSelected,
                disabledBeforePointer,
                pointerSelected,
                selectedAfterEnd,
                selectedAfterHome,
                selectedAfterDown,
                cursorWindow = cursorWindow.ToString(),
                popupWindow = popupWindow.ToString(),
                observations,
                failure = failure?.ToString(),
            }, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.Out.WriteLine($"original disabled filter input and frame retained: {directory}");
        }

        if (failure is not null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }

        enabledSelected.Should().BeTrue("the original enabled row must establish the actual pointer-selection baseline");
        disabledBeforePointer.Should().BeFalse();
        cursorWindow.Should().Be(popupWindow, "the real cursor must target the original owned popup HWND");
        pointerSelected.Should().BeTrue("the original disabled row must select through actual queued input without a Select fallback");
        selectedAfterEnd.Should().Be(enabled.Name, "ToolStripDropDownMenu ignores RTL-aware endpoint inversion");
        selectedAfterHome.Should().Be(firstEnabled.Name);
        selectedAfterDown.Should().Be(enabled.Name);
    }

    private static void RecordWindowReceipts(Form host, ToolStripDropDownMenu popup, string path)
    {
        NativeTestWindowActivation.AssertForeground(host);
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            scope = "actual native host and popup receipts immediately before guarded capture; foreground ownership strictly verified",
            foregroundVerifiedHwnd = host.Handle.ToString(),
            popupRightToLeft = popup.RightToLeft.ToString(),
            ownerRightToLeft = popup.OwnerItem?.RightToLeft.ToString(),
            popup.TabStop,
            orientation = popup.Orientation.ToString(),
            hostBounds = NativeMethods.GetWindowRectangle(host.Handle),
            popupBounds = NativeMethods.GetWindowRectangle(popup.Handle),
            hostWindows = NativeMethods.GetNativeWindowAndNeighborReceipts(host.Handle),
            popupWindows = NativeMethods.GetNativeWindowAndNeighborReceipts(popup.Handle),
        }, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static object ObserveInput(Form host, ToolStripDropDownMenu popup, string stage)
    {
        NativeTestWindowActivation.AssertForeground(host);
        return new
        {
            stage,
            messageLoop = Application.MessageLoop,
            popup.Visible,
            popupRightToLeft = popup.RightToLeft.ToString(),
            ownerRightToLeft = popup.OwnerItem?.RightToLeft.ToString(),
            popup.TabStop,
            orientation = popup.Orientation.ToString(),
            modifiers = Control.ModifierKeys.ToString(),
            cursor = NativeMethods.GetCursorPosition(),
            messagePosition = GetMessagePos(),
            items = popup.Items.OfType<ToolStripMenuItem>().Select(item => new { item.Name, item.Enabled, item.Selected }).ToArray(),
        };
    }

    private static void QueuePointer(Form host, ToolStripDropDownMenu popup, Point local)
    {
        NativeTestWindowActivation.AssertForeground(host);
        NativeMethods.SetCursorPosition(popup.PointToScreen(local));
        nint position = (nint)((local.X & 0xFFFF) | (local.Y << 16));
        PostMessage(popup.Handle, NativeMethods.WmMouseMove, 0, position).Should().BeTrue();
    }

    private static void QueueKey(ToolStripDropDownMenu popup, Keys key)
    {
        uint scanCode = MapVirtualKey((uint)key, 0);
        nint keyData = (nint)(1 | (int)(scanCode << 16) | (1 << 24));
        PostMessage(popup.Handle, 0x0100, (nint)key, keyData).Should().BeTrue();
        PostMessage(popup.Handle, 0x0101, (nint)key, unchecked((nint)((long)keyData | 0xC0000000L))).Should().BeTrue();
    }

    [DllImport("user32.dll")]
    private static extern uint GetMessagePos();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PostMessage(nint window, uint message, nint wordParameter, nint longParameter);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern uint MapVirtualKey(uint code, uint mapType);

    [DllImport("user32.dll")]
    private static extern nint WindowFromPoint(Point point);

    private static ToolStripEx CreateOriginalFilter()
    {
        Type type = typeof(ToolStripEx).Assembly.GetType(FilterTypeName, throwOnError: true)
            ?? throw new TypeLoadException(FilterTypeName);
        return Activator.CreateInstance(type) as ToolStripEx
            ?? throw new InvalidOperationException("The source filter must be constructed directly.");
    }

    private static object DescribeFont(Font font) => new
    {
        font.Name,
        font.Size,
        font.SizeInPoints,
        unit = font.Unit.ToString(),
        style = font.Style.ToString(),
        font.Height,
        font.GdiCharSet,
    };

    private static object DescribeItem(ToolStripItem item) => new
    {
        item.Name,
        type = item.GetType().FullName,
        item.Text,
        item.Available,
        item.Visible,
        item.Enabled,
        item.Bounds,
        item.Padding,
        item.Margin,
        item.Selected,
        item.Pressed,
        font = DescribeFont(item.Font),
        preferred = item.GetPreferredSize(Size.Empty),
        measuredCaption = TextRenderer.MeasureText(item.Text, item.Font),
        sourceCachedTextSize = typeof(ToolStripItem).GetMethod("GetTextSize", PrivateInstance)?.Invoke(item, null),
        showShortcutKeys = (item as ToolStripMenuItem)?.ShowShortcutKeys,
        shortcutKeys = (item as ToolStripMenuItem)?.ShortcutKeys.ToString(),
        shortcutText = (item as ToolStripMenuItem)?.ShortcutKeyDisplayString,
        sourceShortcutSize = item is ToolStripMenuItem menuItem
            ? typeof(ToolStripMenuItem).GetMethod("GetShortcutTextSize", PrivateInstance)?.Invoke(menuItem, null) : null,
        imageSize = item.Image?.Size,
    };

    private static T ReadProperty<T>(ToolStripDropDownMenu popup, string name)
        => typeof(ToolStripDropDownMenu).GetProperty(name, PrivateInstance)?.GetValue(popup) is T value
            ? value : throw new MissingMemberException(typeof(ToolStripDropDownMenu).FullName, name);

    private static T ReadField<T>(ToolStripDropDownMenu popup, string name)
        => typeof(ToolStripDropDownMenu).GetField(name, PrivateInstance)?.GetValue(popup) is T value
            ? value : throw new MissingFieldException(typeof(ToolStripDropDownMenu).FullName, name);

    private static void WithIsolatedSettings(Action<string> action)
    {
        string directory = Path.Combine(Path.GetTempPath(), "GitExtensions.FilterMenuLayoutProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string settingsPath = Path.Combine(directory, SettingsFileName);
        File.WriteAllText(settingsPath, "<?xml version=\"1.0\" encoding=\"utf-8\"?><dictionary />");
        InitializeAppSettingsWithoutRealConfiguration(directory);
        using GitExtSettingsCache cache = new(settingsPath, autoSave: false);
        DistributedSettings settings = new(lowerPriority: null, cache, SettingLevel.Unknown);
        TestContext.Out.WriteLine($"isolated original settings and menu probe retained: {directory}");
        AppSettings.UsingContainer(settings, () => action(directory));
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
            // AppSettings creates its first cache before its public container boundary;
            // keep that first initialization within the same disposable settings root.
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
}
