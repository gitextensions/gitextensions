using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using AwesomeAssertions;
using GitUI;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

/// <summary>
///  Observes the original toolbar's real tooltip HWND across pointer and programmatic menu opening.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class ToolStripTooltipLifetimeNativeTests
{
    [TestCase(false, false, false)]
    [TestCase(false, true, false)]
    [TestCase(true, false, false)]
    [TestCase(true, true, false)]
    [TestCase(false, false, true)]
    [TestCase(false, true, true)]
    [TestCase(true, false, true)]
    [TestCase(true, true, true)]
    public void Source_dropdown_should_record_actual_tooltip_lifetime_across_menu_activation_and_pointer_residence(bool split, bool pointer, bool pending)
    {
        Point previousCursor = NativeMethods.GetCursorPosition();
        using Form host = new()
        {
            StartPosition = FormStartPosition.Manual,
            Location = new Point(60, 60),
            ClientSize = new Size(700, 400),
            Text = "Source tooltip lifetime",
        };
        using ToolStripEx strip = new() { Dock = DockStyle.Top, GripEnabled = false };
        using ToolStripDropDownItem owner = split ? new ToolStripSplitButton("Source selector") : new ToolStripDropDownButton("Source selector");
        owner.ToolTipText = "Original toolbar selector tooltip";
        owner.DropDownItems.Add(new ToolStripMenuItem("Source option"));
        strip.Items.Add(owner);
        host.Controls.Add(strip);
        ToolTip tooltip = typeof(ToolStrip).GetProperty("ToolTip", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(strip) as ToolTip
            ?? throw new MissingMemberException(typeof(ToolStrip).FullName, "ToolTip");
        string evidence = Path.Combine(TestContext.CurrentContext.TestDirectory, "ToolStripTooltipLifetimeProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(evidence);
        List<object> observations = [];
        using ApplicationContext loop = new();
        using System.Windows.Forms.Timer timer = new() { Interval = 50 };
        Stopwatch elapsed = Stopwatch.StartNew();
        Stopwatch stageElapsed = new();
        Exception? failure = null;
        int stage = 0;
        bool finishing = false;
        Point target = default;
        EventHandler showOnIdle = null!;
        showOnIdle = (_, _) =>
        {
            Application.Idle -= showOnIdle;
            try
            {
                host.Show();
            }
            catch (Exception exception)
            {
                failure = exception;
                Finish();
            }
        };
        host.Shown += (_, _) =>
        {
            try
            {
                NativeTestWindowActivation.Activate(host, waitForPresentation: false);
                host.DeviceDpi.Should().Be(96);
                target = split && owner is ToolStripSplitButton splitOwner
                    ? new Point(owner.Bounds.X + splitOwner.DropDownButtonBounds.X + (splitOwner.DropDownButtonBounds.Width / 2), owner.Bounds.Y + (owner.Height / 2))
                    : new Point(owner.Bounds.X + (owner.Width / 2), owner.Bounds.Y + (owner.Height / 2));
                Move(target);
                stageElapsed.Restart();
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
            timer.Stop();
            try
            {
                if (elapsed.Elapsed > TimeSpan.FromSeconds(15))
                {
                    throw new TimeoutException($"The source tooltip lifetime probe did not finish stage {stage}.");
                }

                int delay = stage == 0 && pending ? 50 : stage is 0 or 2 or 5 ? tooltip.InitialDelay + 250 : 150;
                if (stageElapsed.ElapsedMilliseconds < delay)
                {
                    return;
                }

                switch (stage)
                {
                    case 0:
                        Record("hover-before-opening");
                        IsTooltipVisible().Should().Be(!pending,
                            "the native input route must start with the declared visible or pending tooltip");
                        if (pointer)
                        {
                            nint coordinates = Pack(target);
                            PostMessage(strip.Handle, 0x0201, 1, coordinates).Should().BeTrue();
                            PostMessage(strip.Handle, 0x0202, 0, coordinates).Should().BeTrue();
                        }
                        else
                        {
                            owner.ShowDropDown();
                        }

                        break;
                    case 1:
                        Record("opened");
                        owner.DropDown.Visible.Should().BeTrue();
                        break;
                    case 2:
                        Record("owner-dwell-while-open");
                        owner.DropDown.Visible.Should().BeTrue();
                        IsWindowVisible(strip.Handle).Should().BeFalse("ToolStripEx suspends its owner HWND with WM_SETREDRAW while the menu is open");
                        IsTooltipVisible().Should().BeFalse("after the native fade completes, the redraw-suspended owner's tooltip is closed rather than retained for its 5000ms AutoPopDelay");
                        ToolStripItem option = owner.DropDownItems[0];
                        Point optionPoint = new(option.Bounds.X + (option.Width / 2), option.Bounds.Y + (option.Height / 2));
                        NativeMethods.SetCursorPosition(owner.DropDown.PointToScreen(optionPoint));
                        PostMessage(owner.DropDown.Handle, NativeMethods.WmMouseMove, 0, Pack(optionPoint)).Should().BeTrue();
                        break;
                    case 3:
                        Record("option-hover");
                        owner.DropDown.Visible.Should().BeTrue();
                        IsTooltipVisible().Should().BeFalse();
                        owner.DropDown.Close();
                        Move(new Point(strip.ClientSize.Width - 10, strip.ClientSize.Height / 2));
                        break;
                    case 4:
                        owner.DropDown.Visible.Should().BeFalse();
                        Move(target);
                        break;
                    default:
                        Record("hover-after-close");
                        IsWindowVisible(strip.Handle).Should().BeTrue();
                        IsTooltipVisible().Should().BeTrue("closing the menu must not permanently remove the original tooltip");
                        break;
                }

                stage++;
                stageElapsed.Restart();
                if (stage == 6)
                {
                    Finish();
                }
            }
            catch (Exception exception)
            {
                failure = exception;
                Record("failed");
                Finish();
            }
            finally
            {
                if (!finishing)
                {
                    timer.Start();
                }
            }
        };
        try
        {
            Application.Idle += showOnIdle;
            Application.Run(loop);
        }
        finally
        {
            Application.Idle -= showOnIdle;
            timer.Stop();
            owner.DropDown.Close();
            host.Close();
            NativeMethods.SetCursorPosition(previousCursor);
            File.WriteAllText(Path.Combine(evidence, "observations.json"), JsonSerializer.Serialize(new
            {
                split,
                pointer,
                pending,
                initialDelay = tooltip.InitialDelay,
                autoPopDelay = tooltip.AutoPopDelay,
                completedStages = stage,
                failure = failure?.ToString(),
                observations,
                acquisition = "actual original ToolStripEx and native pointer messages in an owned message loop; native tooltip HWND visibility and unscaled screen frames",
            }, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.Out.WriteLine($"toolStripTooltipLifetimeEvidence={evidence}");
        }

        if (failure is not null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }

        stage.Should().Be(6);
        return;

        void Move(Point point)
        {
            NativeMethods.SetCursorPosition(strip.PointToScreen(point));
            NativeMethods.SendMouseMessage(strip.Handle, NativeMethods.WmMouseMove, point.X, point.Y);
        }

        nint TooltipHandle()
            => (nint)(typeof(ToolTip).GetProperty("Handle", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(tooltip)
                ?? throw new MissingMemberException(typeof(ToolTip).FullName, "Handle"));

        bool IsTooltipVisible() => IsWindowVisible(TooltipHandle());

        void Record(string state)
        {
            nint handle = TooltipHandle();
            bool visible = IsWindowVisible(handle);
            Rectangle bounds = NativeMethods.GetWindowRectangle(host.Handle);
            if (owner.DropDown.Visible)
            {
                bounds = Rectangle.Union(bounds, NativeMethods.GetWindowRectangle(owner.DropDown.Handle));
            }

            if (visible)
            {
                bounds = Rectangle.Union(bounds, NativeMethods.GetWindowRectangle(handle));
            }

            observations.Add(new
            {
                state,
                stage,
                elapsedMilliseconds = elapsed.ElapsedMilliseconds,
                stageElapsedMilliseconds = stageElapsed.ElapsedMilliseconds,
                tooltipHandle = handle.ToString(),
                tooltipVisible = visible,
                menuVisible = owner.DropDown.Visible,
                nativeOwnerVisible = IsWindowVisible(strip.Handle),
                owner.Selected,
                owner.Pressed,
                strip.Focused,
                strip.ContainsFocus,
                cursor = NativeMethods.GetCursorPosition(),
                screenBounds = bounds,
            });
            NativeMethods.IsEntirelyOnScreen(bounds).Should().BeTrue();
            using Bitmap image = new(bounds.Width, bounds.Height);
            using Graphics graphics = Graphics.FromImage(image);
            graphics.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            image.Save(Path.Combine(evidence, state + ".png"), ImageFormat.Png);
        }

        void Finish()
        {
            finishing = true;
            timer.Stop();
            owner.DropDown.Close();
            host.Close();
            loop.ExitThread();
        }
    }

    private static nint Pack(Point point) => (nint)((point.X & 0xFFFF) | (point.Y << 16));

    [DllImport("user32.dll", EntryPoint = "PostMessageW")]
    private static extern bool PostMessage(nint window, uint message, nint parameter, nint data);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(nint window);
}
