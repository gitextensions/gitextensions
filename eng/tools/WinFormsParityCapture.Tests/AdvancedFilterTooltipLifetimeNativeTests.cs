using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using AwesomeAssertions;
using GitCommands;
using GitUI;
using GitUI.UserControls.RevisionGrid;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

/// <summary>
///  Observes the actual advanced-filter summary handler while its original menu remains open.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class AdvancedFilterTooltipLifetimeNativeTests
{
    private const BindingFlags InstanceMembers = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    [TestCase(false)]
    [TestCase(true)]
    public void Actual_advanced_filter_should_record_summary_updates_and_tooltip_requests_with_and_without_its_menu(bool openMenu)
    {
        using OriginalBrowseNativeProbe probe = OriginalBrowseNativeProbe.Create("AdvancedFilterTooltipLifetimeProbe");
        AppSettings.BranchFilterEnabled.Value = false;
        AppSettings.ShowCurrentBranchOnly.Value = false;
        AppSettings.ShowOnlyFirstParent = false;
        AppSettings.ShowReflogReferences.Value = false;
        AppSettings.ShowSimplifyByDecoration = false;
        Type type = typeof(ToolStripEx).Assembly.GetType("GitUI.UserControls.FilterToolBar", throwOnError: true)
            ?? throw new TypeLoadException("GitUI.UserControls.FilterToolBar");
        using ToolStripEx strip = Activator.CreateInstance(type) as ToolStripEx
            ?? throw new InvalidOperationException("The actual source filter toolbar must be constructed directly.");
        ToolStripSplitButton owner = strip.Items.OfType<ToolStripSplitButton>().Single(item => item.Name == "tsbtnAdvancedFilter");
        MethodInfo onFilterChanged = type.GetMethod("revisionGridFilter_FilterChanged", InstanceMembers)
            ?? throw new MissingMethodException(type.FullName, "revisionGridFilter_FilterChanged");
        ToolTip tooltip = typeof(ToolStrip).GetProperty("ToolTip", InstanceMembers)?.GetValue(strip) as ToolTip
            ?? throw new MissingMemberException(typeof(ToolStrip).FullName, "ToolTip");
        FilterChangedEventArgs empty = new(new FilterInfo());
        FilterChangedEventArgs applied = new(new FilterInfo { ByDateFrom = true, DateFrom = new DateTime(2026, 1, 2) });
        empty.FilterSummary.Should().BeEmpty();
        applied.FilterSummary.Should().NotBeEmpty();
        using Form host = new()
        {
            StartPosition = FormStartPosition.Manual,
            Location = new Point(50, 50),
            ClientSize = new Size(1400, 600),
            Text = "Actual advanced filter tooltip summary",
        };
        host.Controls.Add(strip);
        List<object> observations = [];
        Point previousCursor = NativeMethods.GetCursorPosition();
        using ApplicationContext loop = new();
        using System.Windows.Forms.Timer timer = new() { Interval = 50 };
        Stopwatch elapsed = Stopwatch.StartNew();
        Stopwatch stageElapsed = new();
        Exception? failure = null;
        int stage = 0;
        bool finishing = false;
        EventHandler showOnIdle = null!;
        showOnIdle = (_, _) =>
        {
            Application.Idle -= showOnIdle;
            try
            {
                host.Show();
                NativeTestWindowActivation.Activate(host, waitForPresentation: false);
                host.DeviceDpi.Should().Be(96);
                Apply(empty);
                MoveToOwner();
                if (openMenu)
                {
                    owner.ShowDropDown();
                }

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
                if (elapsed.Elapsed > TimeSpan.FromSeconds(20))
                {
                    throw new TimeoutException($"The actual advanced-filter tooltip probe did not finish stage {stage}.");
                }

                if (stageElapsed.ElapsedMilliseconds < tooltip.InitialDelay + 250)
                {
                    return;
                }

                owner.DropDown.Visible.Should().Be(stage < 5 && openMenu);
                switch (stage)
                {
                    case 0:
                        Record("empty-summary");
                        owner.ToolTipText.Should().BeEmpty();
                        Apply(applied);
                        MoveAway();
                        break;
                    case 1:
                        MoveToOwner();
                        break;
                    case 2:
                        Record("updated-summary-natural-hover");
                        owner.ToolTipText.Should().Be(applied.FilterSummary);
                        IsTooltipVisible().Should().Be(!openMenu, "a real away/back hover reaches the visible source owner only while its HWND is not redraw-suspended");
                        tooltip.Show(owner.ToolTipText, strip, new Point(owner.Bounds.Left, owner.Bounds.Bottom + 20), tooltip.AutoPopDelay);
                        break;
                    case 3:
                        Record("updated-summary-explicit-tooltip-request");
                        owner.ToolTipText.Should().Be(applied.FilterSummary);
                        IsTooltipVisible().Should().BeTrue("the original explicit ToolTip.Show request remains supported even with its menu open");
                        Apply(empty);
                        break;
                    case 4:
                        Record("cleared-summary");
                        owner.ToolTipText.Should().BeEmpty();
                        owner.DropDown.Close();
                        MoveAway();
                        break;
                    case 5:
                        Apply(applied);
                        MoveToOwner();
                        break;
                    case 6:
                        Record("summary-hover-after-close");
                        owner.ToolTipText.Should().Be(applied.FilterSummary);
                        IsTooltipVisible().Should().BeTrue("the actual summary tooltip must remain available after the menu closes");
                        Apply(empty);
                        MoveAway();
                        break;
                    default:
                        Record("empty-summary-after-pointer-leaves");
                        owner.ToolTipText.Should().BeEmpty();
                        IsTooltipVisible().Should().BeFalse();
                        break;
                }

                stage++;
                stageElapsed.Restart();
                if (stage == 8)
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
            File.WriteAllText(Path.Combine(probe.EvidenceDirectory, "observations.json"), JsonSerializer.Serialize(new
            {
                openMenu,
                completedStages = stage,
                applied.FilterSummary,
                tooltip.InitialDelay,
                tooltip.AutoPopDelay,
                failure = failure?.ToString(),
                observations,
                acquisition = "actual original FilterToolBar revisionGridFilter_FilterChanged handler and ToolStrip tooltip HWND; real pointer residence plus explicit native ToolTip.Show; unscaled screen frames",
            }, new JsonSerializerOptions { WriteIndented = true }));
        }

        if (failure is not null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }

        stage.Should().Be(8);
        return;

        void Apply(FilterChangedEventArgs args) => onFilterChanged.Invoke(strip, [strip, args]);

        void MoveToOwner() => NativeMethods.SetCursorPosition(strip.PointToScreen(new Point(owner.Bounds.Left + (owner.Width / 2), owner.Bounds.Top + (owner.Height / 2))));

        void MoveAway() => NativeMethods.SetCursorPosition(host.PointToScreen(new Point(host.ClientSize.Width - 20, host.ClientSize.Height - 20)));

        nint TooltipHandle()
            => (nint)(typeof(ToolTip).GetProperty("Handle", InstanceMembers)?.GetValue(tooltip)
                ?? throw new MissingMemberException(typeof(ToolTip).FullName, "Handle"));

        bool IsTooltipVisible() => IsWindowVisible(TooltipHandle());

        void Record(string state)
        {
            bool visible = IsTooltipVisible();
            Rectangle bounds = NativeMethods.GetWindowRectangle(host.Handle);
            if (owner.DropDown.Visible)
            {
                bounds = Rectangle.Union(bounds, NativeMethods.GetWindowRectangle(owner.DropDown.Handle));
            }

            if (visible)
            {
                bounds = Rectangle.Union(bounds, NativeMethods.GetWindowRectangle(TooltipHandle()));
            }

            observations.Add(new
            {
                state,
                stage,
                elapsedMilliseconds = elapsed.ElapsedMilliseconds,
                tooltipVisible = visible,
                nativeOwnerVisible = IsWindowVisible(strip.Handle),
                menuVisible = owner.DropDown.Visible,
                owner.ToolTipText,
                owner.AutoToolTip,
                cursor = NativeMethods.GetCursorPosition(),
                screenBounds = bounds,
            });
            NativeMethods.IsEntirelyOnScreen(bounds).Should().BeTrue();
            using Bitmap image = new(bounds.Width, bounds.Height);
            using Graphics graphics = Graphics.FromImage(image);
            graphics.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            image.Save(Path.Combine(probe.EvidenceDirectory, state + ".png"), ImageFormat.Png);
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

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(nint window);
}
