using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using AwesomeAssertions;
using GitExtensions.Extensibility;
using GitUI;
using GitUI.CommandsDialogs;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

/// <summary>
///  Observes source filter-menu activation without changing its owners, rows or command handlers.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class FilterDropDownActivationNativeTests
{
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private const uint MouseMoveMessage = 0x200;
    private const uint LeftButtonDownMessage = 0x201;
    private const uint LeftButtonUpMessage = 0x202;
    private const uint KeyDownMessage = 0x100;
    private const uint KeyUpMessage = 0x101;
    private const uint SystemKeyDownMessage = 0x104;
    private const uint SystemKeyUpMessage = 0x105;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly string[] OwnerNames =
    [
        "tsbtnAdvancedFilter", "tssbtnShowBranches", "tsddbtnBranchFilter", "tsddbtnRevisionFilter",
    ];

    private static IEnumerable<TestCaseData> ActivationCases
    {
        get
        {
            foreach (string owner in OwnerNames)
            {
                foreach (string route in new[] { "programmatic", "pointer-arrow", "Down", "Up", "F4", "Alt+Down" })
                {
                    yield return new TestCaseData(owner, route);
                }

                // Advanced's plain Enter/Space and primary pointer can open its actual
                // revision-filter dialog. This menu-only probe does not cover that modal action.
                if (owner != "tsbtnAdvancedFilter")
                {
                    yield return new TestCaseData(owner, "Enter");
                    yield return new TestCaseData(owner, "Space");
                }
            }

            yield return new TestCaseData("tssbtnShowBranches", "pointer-primary");
        }
    }

    [TestCaseSource(nameof(ActivationCases))]
    public void Actual_source_filter_menu_should_distinguish_pointer_programmatic_and_keyboard_activation(
        string ownerName, string route)
    {
        using OriginalBrowseNativeProbe probe = OriginalBrowseNativeProbe.Create("FilterDropDownActivationProbe");
        Point originalCursor = NativeMethods.GetCursorPosition();
        try
        {
            probe.WithOriginalBrowse(new BrowseArguments(), (form, getThreadException, closeAndDrain) =>
            {
                ToolStrip strip = ReadField<ToolStrip>(form, "ToolStripFilters");
                ToolStripDropDownItem owner = strip.Items.OfType<ToolStripDropDownItem>().Single(item => item.Name == ownerName);
                ToolStripDropDown popup = owner.DropDown;
                List<object> observations = [];
                List<object> events = [];
                Exception? failure = null;
                using ApplicationContext loop = new();
                using System.Windows.Forms.Timer timer = new() { Interval = 80 };
                using KeyboardStateScope keyboard = new();
                Stopwatch elapsed = Stopwatch.StartNew();
                int step = 0;
                int cycle = 0;
                bool finishing = false;

                // The actual FormBrowse queued WM_SYSKEY Alt+Down route does not
                // reach ToolStripDropDownItem's direct dialog-key opener.
                bool opens = route is not ("F4" or "Alt+Down");
                bool selectsFirst = route is "Down" or "Up"
                    || (owner is not ToolStripSplitButton && (route is "Enter" or "Space"));
                owner.DropDownOpened += OnOpened;
                owner.DropDownClosed += OnClosed;
                EventHandler onIdle = null!;
                onIdle = (_, _) =>
                {
                    Application.Idle -= onIdle;
                    try
                    {
                        form.Show();
                        NativeTestWindowActivation.Activate(form, waitForPresentation: false);
                        timer.Start();
                    }
                    catch (Exception exception)
                    {
                        failure = exception;
                        Finish();
                    }
                };
                timer.Tick += (_, _) =>
                {
                    try
                    {
                        if (getThreadException() is Exception exception)
                        {
                            ExceptionDispatchInfo.Capture(exception).Throw();
                        }

                        if (elapsed.Elapsed > TimeSpan.FromSeconds(30))
                        {
                            throw new TimeoutException("The original filter activation exceeded its bounded owned message loop.");
                        }

                        if (!IsLoadingComplete(form.RevisionGridControl))
                        {
                            return;
                        }

                        switch (step++)
                        {
                            case 0:
                                form.DeviceDpi.Should().Be(96);
                                if (!owner.Available || !owner.Enabled)
                                {
                                    observations.Add(Observe("owner-not-ready"));
                                    WriteEvidence("owner-not-ready");
                                }

                                owner.Available.Should().BeTrue();
                                owner.Enabled.Should().BeTrue();
                                keyboard.Set(Keys.None);
                                NativeMethods.SetCursorPosition(form.PointToScreen(new Point(form.ClientSize.Width - 20, form.ClientSize.Height - 20)));
                                observations.Add(Observe("before-activation"));
                                Activate();
                                break;
                            case 1:
                                observations.Add(Observe("after-activation"));
                                WriteEvidence("opened");
                                popup.Visible.Should().Be(opens, "only original source routes determine whether this menu opens");

                                // Form.Load and DropDownOpening update the source reset commands.
                                // Their constructor-time Enabled values are not activation targets.
                                ToolStripItem firstEnabled = popup.Items.Cast<ToolStripItem>()
                                    .First(item => item.Available && item.Enabled && item.CanSelect);
                                popup.Items.Cast<ToolStripItem>().Where(item => item.Selected).Select(item => item.Name)
                                    .Should().Equal(selectsFirst ? new[] { firstEnabled.Name } : Array.Empty<string>(),
                                        "ToolStripDropDownItem selects the first enabled item only on its keyboard-open route");
                                keyboard.Set(Keys.None);
                                owner.HideDropDown();
                                break;
                            default:
                                observations.Add(Observe("after-close"));
                                popup.Visible.Should().BeFalse();
                                if (++cycle < 2)
                                {
                                    step = 0;
                                }
                                else
                                {
                                    Finish();
                                }

                                break;
                        }
                    }
                    catch (Exception exception)
                    {
                        failure = exception;
                        Finish();
                    }
                };
                Application.Idle += onIdle;
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
                    Application.Idle -= onIdle;
                    owner.DropDownOpened -= OnOpened;
                    owner.DropDownClosed -= OnClosed;
                    WriteReport();
                }

                if (failure is not null)
                {
                    ExceptionDispatchInfo.Capture(failure).Throw();
                }

                cycle.Should().Be(2);
                return;

                void Activate()
                {
                    NativeTestWindowActivation.AssertForeground(form);
                    if (route == "programmatic")
                    {
                        owner.ShowDropDown();
                    }
                    else if (route.StartsWith("pointer-", StringComparison.Ordinal))
                    {
                        Rectangle target = owner is ToolStripSplitButton split
                            ? route == "pointer-primary" ? split.ButtonBounds : split.DropDownButtonBounds
                            : new Rectangle(Point.Empty, owner.Size);
                        Point local = owner.Bounds.Location + new Size(target.X + (target.Width / 2), target.Y + (target.Height / 2));
                        Point screen = strip.PointToScreen(local);
                        NativeMethods.SetCursorPosition(screen);
                        WindowFromPoint(screen).Should().Be(strip.Handle, "real pointer activation must reach the owned source HWND");
                        nint position = (nint)((local.X & 0xFFFF) | (local.Y << 16));
                        PostMessage(strip.Handle, MouseMoveMessage, 0, position).Should().BeTrue();
                        PostMessage(strip.Handle, LeftButtonDownMessage, 1, position).Should().BeTrue();
                        PostMessage(strip.Handle, LeftButtonUpMessage, 0, position).Should().BeTrue();
                    }
                    else
                    {
                        strip.Focus().Should().BeTrue("the queued key must enter the real selected source toolbar");
                        owner.Select();
                        owner.Selected.Should().BeTrue();
                        Keys modifiers = route == "Alt+Down" ? Keys.Alt : Keys.None;
                        keyboard.Set(modifiers);
                        Keys key = route == "Alt+Down" ? Keys.Down : Enum.Parse<Keys>(route);
                        uint scanCode = MapVirtualKey((uint)key, 0);
                        long bits = 1 | (long)(scanCode << 16) | (key is Keys.Up or Keys.Down ? 1L << 24 : 0)
                            | (modifiers == Keys.Alt ? 1L << 29 : 0);
                        PostMessage(strip.Handle, modifiers == Keys.Alt ? SystemKeyDownMessage : KeyDownMessage, (nint)key, (nint)bits).Should().BeTrue();
                        PostMessage(strip.Handle, modifiers == Keys.Alt ? SystemKeyUpMessage : KeyUpMessage, (nint)key,
                            unchecked((nint)(bits | 0xC0000000L))).Should().BeTrue();
                    }
                }

                object Observe(string stage)
                    => new
                    {
                        stage,
                        cycle,
                        Application.MessageLoop,
                        foreground = GetForegroundWindow().ToString(),
                        focus = GetFocus().ToString(),
                        cursor = NativeMethods.GetCursorPosition(),
                        messagePosition = GetMessagePos(),
                        modifiers = Control.ModifierKeys.ToString(),
                        strip.Focused,
                        strip.ContainsFocus,
                        stripRightToLeft = strip.RightToLeft.ToString(),
                        popup.Visible,
                        popupRightToLeft = popup.RightToLeft.ToString(),
                        renderer = popup.Renderer.GetType().FullName,
                        owner = Describe(owner),
                        items = popup.Items.Cast<ToolStripItem>().Select(Describe).ToArray(),
                    };

                void WriteEvidence(string stage)
                {
                    NativeTestWindowActivation.AssertForeground(form);
                    ToolStripDropDown[] popups = popup.Visible ? [popup] : [];
                    File.WriteAllText(Path.Combine(probe.EvidenceDirectory, $"{cycle}-{stage}.windows.json"), JsonSerializer.Serialize(new
                    {
                        host = NativeMethods.GetNativeWindowAndNeighborReceipts(form.Handle),
                        popup = popup.Visible ? NativeMethods.GetNativeWindowAndNeighborReceipts(popup.Handle) : null,
                    }, JsonOptions));
                    using CaptureImageResult frame = ImageCapture.Capture(form, popups, []);
                    frame.Bitmap.Save(Path.Combine(probe.EvidenceDirectory, $"{cycle}-{stage}.png"), ImageFormat.Png);
                    ControlTreeReader reader = new(form, form.DeviceDpi);
                    File.WriteAllText(Path.Combine(probe.EvidenceDirectory, $"{cycle}-{stage}.tree.json"), JsonSerializer.Serialize(new
                    {
                        method = frame.Method.ToString(),
                        frame.ScreenBounds,
                        frame.PrimaryScreenBounds,
                        frame.Acquisitions,
                        primary = reader.ReadPrimary(form, frame.ScreenBounds),
                        popup = popup.Visible ? reader.ReadPopup(popup, 0, frame.ScreenBounds.Location) : null,
                    }, JsonOptions));
                }

                void WriteReport()
                    => File.WriteAllText(Path.Combine(probe.EvidenceDirectory, "activation.json"), JsonSerializer.Serialize(new
                    {
                        ownerName,
                        route,
                        scope = "actual original FormBrowse, existing source handlers, owned Application.Run, real cursor/queued HWND input; owner.Select only establishes keyboard entry",
                        notCovered = "Advanced Filter primary pointer/Enter/Space modal dialog, physical keyboard device input and other themes",
                        form.TopMost,
                        sourceAssembly = typeof(FormBrowse).Assembly.FullName,
                        frameworkAssembly = typeof(ToolStrip).Assembly.FullName,
                        frameworkVersion = FileVersionInfo.GetVersionInfo(typeof(ToolStrip).Assembly.Location).ProductVersion,
                        observations,
                        events,
                        failure = failure?.ToString(),
                    }, JsonOptions));

                void OnOpened(object? sender, EventArgs args) => events.Add(Observe("DropDownOpened"));

                void OnClosed(object? sender, EventArgs args) => events.Add(Observe("DropDownClosed"));

                void Finish()
                {
                    if (finishing)
                    {
                        return;
                    }

                    finishing = true;
                    timer.Stop();
                    keyboard.Set(Keys.None);
                    owner.HideDropDown();
                    closeAndDrain();
                    loop.ExitThread();
                }
            });
        }
        finally
        {
            NativeMethods.SetCursorPosition(originalCursor);
        }
    }

    private static object Describe(ToolStripItem item)
        => new
        {
            item.Name,
            type = item.GetType().FullName,
            item.Text,
            item.Available,
            item.Visible,
            item.Enabled,
            item.CanSelect,
            item.Selected,
            item.Pressed,
            item.Bounds,
            foreground = $"#{item.ForeColor.ToArgb():X8}",
            background = $"#{item.BackColor.ToArgb():X8}",
        };

    private static T ReadField<T>(object owner, string name)
        where T : class
        => owner.GetType().GetField(name, PrivateInstance)?.GetValue(owner) as T
            ?? throw new MissingFieldException(owner.GetType().FullName, name);

    private static bool IsLoadingComplete(RevisionGridControl grid)
    {
        object accessor = typeof(RevisionGridControl).GetMethod("GetTestAccessor", PrivateInstance)?.Invoke(grid, null)
            ?? throw new MissingMethodException(typeof(RevisionGridControl).FullName, "GetTestAccessor");
        return (bool)(accessor.GetType().GetProperty("IsDataLoadComplete", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)?.GetValue(accessor)
            ?? throw new MissingMemberException(accessor.GetType().FullName, "IsDataLoadComplete"));
    }

    private sealed class KeyboardStateScope : IDisposable
    {
        private readonly byte[] _original = new byte[256];

        public KeyboardStateScope() => GetKeyboardState(_original).Should().BeTrue();

        public void Set(Keys modifiers)
        {
            byte[] current = (byte[])_original.Clone();
            foreach (int key in new[] { 0x01, 0x10, 0x11, 0x12, 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5 })
            {
                current[key] &= 0x7F;
            }

            if (modifiers == Keys.Alt)
            {
                current[0x12] |= 0x80;
                current[0xA4] |= 0x80;
            }

            SetKeyboardState(current).Should().BeTrue();
            Control.ModifierKeys.Should().Be(modifiers);
        }

        public void Dispose() => SetKeyboardState(_original).Should().BeTrue();
    }

    [DllImport("user32.dll", EntryPoint = "PostMessageW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PostMessage(nint window, uint message, nint parameter, nint bits);

    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint code, uint mapType);

    [DllImport("user32.dll")]
    private static extern nint WindowFromPoint(Point point);

    [DllImport("user32.dll")]
    private static extern uint GetMessagePos();

    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern nint GetFocus();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetKeyboardState([Out] byte[] state);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetKeyboardState(byte[] state);
}
