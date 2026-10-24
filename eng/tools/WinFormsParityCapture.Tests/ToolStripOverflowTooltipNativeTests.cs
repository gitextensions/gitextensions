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
///  Measures tooltip ownership when the same original ToolStripEx item genuinely moves into overflow.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class ToolStripOverflowTooltipNativeTests
{
    private const BindingFlags InstanceMembers = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    [TestCase(false, false, false)]
    [TestCase(false, true, false)]
    [TestCase(true, false, false)]
    [TestCase(true, true, false)]
    [TestCase(false, false, true)]
    [TestCase(false, true, true)]
    [TestCase(true, false, true)]
    [TestCase(true, true, true)]
    public void Source_item_should_record_tooltips_from_its_actual_main_or_overflow_parent(bool split, bool rightToLeft, bool pending)
    {
        Point previousCursor = NativeMethods.GetCursorPosition();
        using Form host = new()
        {
            AutoScaleMode = AutoScaleMode.None,
            StartPosition = FormStartPosition.Manual,
            Location = new Point(50, 50),
            ClientSize = new Size(850, 450),
            Text = "Actual source overflow tooltip ownership",
        };
        using ToolStripEx strip = new()
        {
            Dock = DockStyle.None,
            AutoSize = false,
            Size = new Size(450, 25),
            GripEnabled = false,
            RightToLeft = rightToLeft ? RightToLeft.Yes : RightToLeft.No,
        };
        strip.Items.Add(new ToolStripButton("Main command") { AutoSize = false, Size = new Size(140, 22) });
        using ToolStripDropDownItem owner = split ? new ToolStripSplitButton("Source selector") : new ToolStripDropDownButton("Source selector");
        owner.AutoSize = false;
        owner.Size = new Size(120, 22);
        owner.ToolTipText = "Same retained source owner hint";
        owner.DropDownItems.Add(new ToolStripMenuItem("Source option"));
        using ToolStripDropDown initialMenu = owner.DropDown;
        using ToolStripDropDownMenu replacementMenu = new();
        replacementMenu.Items.Add(new ToolStripMenuItem("Replacement source option"));
        strip.Items.Add(owner);
        host.Controls.Add(strip);
        ToolStripDropDown overflow = strip.OverflowButton.DropDown;
        string evidence = Path.Combine(TestContext.CurrentContext.TestDirectory, "ToolStripOverflowTooltipProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(evidence);
        List<object> observations = [];
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
                MoveOwner();
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
                if (elapsed.Elapsed > TimeSpan.FromSeconds(25))
                {
                    throw new TimeoutException($"The real source overflow tooltip probe did not finish stage {stage}.");
                }

                int delay = stage == 4 && pending ? 75 : 750;
                if (stageElapsed.ElapsedMilliseconds < delay)
                {
                    return;
                }

                owner.Owner.Should().BeSameAs(strip);
                switch (stage)
                {
                    case 0:
                        owner.GetCurrentParent().Should().BeSameAs(strip);
                        Record("main-hover");
                        TooltipVisible(strip).Should().BeTrue();
                        ClickOwner();
                        break;
                    case 1:
                        Record("main-menu");
                        owner.DropDown.Visible.Should().BeTrue();
                        IsWindowVisible(strip.Handle).Should().BeFalse();
                        TooltipVisible(strip).Should().BeFalse();
                        owner.DropDown.Close();
                        MoveAway();
                        break;
                    case 2:
                        strip.Width = 180;
                        strip.PerformLayout();
                        owner.Placement.Should().Be(ToolStripItemPlacement.Overflow);
                        ClickOverflow();
                        break;
                    case 3:
                        overflow.Visible.Should().BeTrue();
                        owner.GetCurrentParent().Should().BeSameAs(overflow);
                        Record("overflow-opened");
                        MoveOwner();
                        break;
                    case 4:
                        Record("overflow-owner-before-menu");
                        TooltipVisible(overflow).Should().Be(!pending);
                        ClickOwner();
                        break;
                    case 5:
                        Record("overflow-child-menu");
                        owner.DropDown.Visible.Should().BeTrue();
                        owner.GetCurrentParent().Should().BeSameAs(overflow);
                        IsWindowVisible(strip.Handle).Should().BeFalse();
                        IsWindowVisible(overflow.Handle).Should().BeTrue();
                        TooltipVisible(overflow).Should().BeTrue("the still-visible current overflow parent retains its pending or shown hint independently of the frozen logical main owner");
                        owner.DropDown.Close();
                        overflow.Close();
                        MoveAway();
                        break;
                    case 6:
                        ClickOverflow();
                        break;
                    case 7:
                        overflow.Visible.Should().BeTrue();
                        owner.GetCurrentParent().Should().BeSameAs(overflow);
                        MoveOwner();
                        break;
                    case 8:
                        Record("overflow-reopened-hover");
                        TooltipVisible(overflow).Should().BeTrue();
                        if (pending)
                        {
                            ReadToolTip(overflow).Show(owner.ToolTipText, overflow, new Point(owner.Bounds.Left, owner.Bounds.Bottom + 20), 100);
                        }
                        else
                        {
                            ReadToolTip(overflow).Hide(overflow);
                        }

                        break;
                    case 9:
                        Record("overflow-hint-closed-before-replacement");
                        TooltipVisible(overflow).Should().BeFalse();
                        ActiveHintIsOwner(overflow).Should().BeTrue("native auto-pop and explicit Hide retire the hint window, not the active hover item used by dropdown refresh");
                        owner.DropDown = replacementMenu;
                        ClickOwner();
                        break;
                    case 10:
                        Record("overflow-replacement-child-menu");
                        owner.DropDown.Should().BeSameAs(replacementMenu);
                        owner.DropDown.Visible.Should().BeTrue();
                        owner.GetCurrentParent().Should().BeSameAs(overflow);
                        IsWindowVisible(strip.Handle).Should().BeFalse();
                        IsWindowVisible(overflow.Handle).Should().BeTrue();
                        TooltipVisible(overflow).Should().BeTrue("the native parent refreshes its active hover item even after an explicit tooltip Hide request");
                        owner.DropDown.Close();
                        overflow.Close();
                        MoveAway();
                        break;
                    case 11:
                        strip.Width = 450;
                        strip.PerformLayout();
                        owner.Placement.Should().Be(ToolStripItemPlacement.Main);
                        owner.GetCurrentParent().Should().BeSameAs(strip);
                        MoveOwner();
                        break;
                    case 12:
                        Record("regrown-main-hover");
                        TooltipVisible(strip).Should().BeTrue();
                        ClickOwner();
                        break;
                    default:
                        Record("regrown-main-menu");
                        owner.DropDown.Visible.Should().BeTrue();
                        TooltipVisible(strip).Should().BeFalse();
                        break;
                }

                stage++;
                stageElapsed.Restart();
                if (stage == 14)
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
            overflow.Close();
            host.Close();
            NativeMethods.SetCursorPosition(previousCursor);
            File.WriteAllText(Path.Combine(evidence, "observations.json"), JsonSerializer.Serialize(new
            {
                split,
                rightToLeft,
                pending,
                completedStages = stage,
                failure = failure?.ToString(),
                observations,
                acquisition = "actual ToolStripEx resize and wrapped overflow; queued pointer activation verified against the native HWND under the cursor; actual parent-specific tooltip HWND and unscaled screen frames",
            }, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.Out.WriteLine($"toolStripOverflowTooltipEvidence={evidence}");
        }

        if (failure is not null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }

        stage.Should().Be(14);
        return;

        void MoveAway() => NativeMethods.SetCursorPosition(host.PointToScreen(new Point(host.ClientSize.Width - 20, host.ClientSize.Height - 20)));

        void MoveOwner()
        {
            ToolStrip current = owner.GetCurrentParent() ?? throw new InvalidOperationException("The source item must have its genuine current parent.");
            NativeMethods.SetCursorPosition(current.PointToScreen(new Point(owner.Bounds.Left + (owner.Width / 2), owner.Bounds.Top + (owner.Height / 2))));
        }

        void ClickOwner()
        {
            ToolStrip current = owner.GetCurrentParent() ?? throw new InvalidOperationException("The source item must have its genuine current parent.");
            Rectangle target = owner is ToolStripSplitButton splitOwner ? splitOwner.DropDownButtonBounds : new Rectangle(Point.Empty, owner.Size);
            QueueClick(current, new Point(owner.Bounds.Left + target.Left + (target.Width / 2), owner.Bounds.Top + target.Top + (target.Height / 2)));
        }

        void ClickOverflow() => QueueClick(strip, new Point(strip.OverflowButton.Bounds.Left + (strip.OverflowButton.Width / 2), strip.OverflowButton.Bounds.Top + (strip.OverflowButton.Height / 2)));

        void QueueClick(ToolStrip parent, Point target)
        {
            Point screen = parent.PointToScreen(target);
            NativeMethods.SetCursorPosition(screen);
            WindowFromPoint(screen).Should().Be(parent.Handle, "the queued pointer route must reach the genuine current-parent HWND");
            PostMessage(parent.Handle, NativeMethods.WmMouseMove, 0, Pack(target)).Should().BeTrue();
            PostMessage(parent.Handle, 0x0201, 1, Pack(target)).Should().BeTrue();
            PostMessage(parent.Handle, 0x0202, 0, Pack(target)).Should().BeTrue();
        }

        nint TooltipHandle(ToolStrip parent)
        {
            ToolTip tooltip = ReadToolTip(parent);
            return (nint)(typeof(ToolTip).GetProperty("Handle", InstanceMembers)?.GetValue(tooltip)
                ?? throw new MissingMemberException(typeof(ToolTip).FullName, "Handle"));
        }

        bool TooltipVisible(ToolStrip parent) => IsWindowVisible(TooltipHandle(parent));

        ToolTip ReadToolTip(ToolStrip parent)
            => typeof(ToolStrip).GetProperty("ToolTip", InstanceMembers)?.GetValue(parent) as ToolTip
                ?? throw new MissingMemberException(typeof(ToolStrip).FullName, "ToolTip");

        bool ActiveHintIsOwner(ToolStrip parent)
            => ReferenceEquals((typeof(ToolStrip).GetField("_currentlyActiveTooltipItem", InstanceMembers)
                ?? throw new MissingFieldException(typeof(ToolStrip).FullName, "_currentlyActiveTooltipItem")).GetValue(parent), owner);

        void Record(string state)
        {
            Rectangle bounds = NativeMethods.GetWindowRectangle(host.Handle);
            foreach (ToolStrip popup in new ToolStrip[] { overflow, owner.DropDown })
            {
                if (popup.Visible)
                {
                    bounds = Rectangle.Union(bounds, NativeMethods.GetWindowRectangle(popup.Handle));
                }
            }

            foreach (ToolStrip parent in new ToolStrip[] { strip, overflow })
            {
                if (TooltipVisible(parent))
                {
                    bounds = Rectangle.Union(bounds, NativeMethods.GetWindowRectangle(TooltipHandle(parent)));
                }
            }

            observations.Add(new
            {
                state,
                stage,
                elapsedMilliseconds = elapsed.ElapsedMilliseconds,
                placement = owner.Placement.ToString(),
                logicalOwnerIsOriginal = ReferenceEquals(owner.Owner, strip),
                currentParent = owner.GetCurrentParent()?.GetType().FullName,
                mainNativeVisible = IsWindowVisible(strip.Handle),
                overflowNativeVisible = IsWindowVisible(overflow.Handle),
                overflow.Visible,
                menuVisible = owner.DropDown.Visible,
                mainTooltipVisible = TooltipVisible(strip),
                overflowTooltipVisible = TooltipVisible(overflow),
                overflowActiveHintIsOwner = ActiveHintIsOwner(overflow),
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
            overflow.Close();
            host.Close();
            loop.ExitThread();
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Source_nested_overflow_menu_should_record_its_actual_owner_parent_and_outside_closure_routes(bool split)
    {
        Point previousCursor = NativeMethods.GetCursorPosition();
        using Form host = new()
        {
            AutoScaleMode = AutoScaleMode.None,
            StartPosition = FormStartPosition.Manual,
            Location = new Point(50, 50),
            ClientSize = new Size(850, 450),
            Text = "Actual source nested overflow closure routes",
        };
        using ToolStripEx strip = new()
        {
            Dock = DockStyle.None,
            AutoSize = false,
            Size = new Size(450, 25),
            GripEnabled = false,
        };
        using ToolStripButton main = new("Main command") { AutoSize = false, Size = new Size(140, 22) };
        using ToolStripDropDownItem owner = split ? new ToolStripSplitButton("Source selector") : new ToolStripDropDownButton("Source selector");
        owner.AutoSize = false;
        owner.Size = new Size(120, 22);
        owner.DropDownItems.Add(new ToolStripMenuItem("Source option"));
        using ToolStripButton sibling = new("Sibling command") { AutoSize = false, Size = new Size(120, 22) };
        using Button outside = new() { Location = new Point(600, 300), Size = new Size(100, 23), Text = "Outside command" };
        strip.Items.Add(main);
        strip.Items.Add(owner);
        strip.Items.Add(sibling);
        host.Controls.Add(strip);
        host.Controls.Add(outside);
        ToolStripDropDown overflow = strip.OverflowButton.DropDown;
        string evidence = Path.Combine(TestContext.CurrentContext.TestDirectory, "ToolStripOverflowTooltipProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(evidence);
        string[] routes = ["owner-reclick", "child-escape", "outside-button", "sibling-command", "blank-parent"];
        List<object> observations = [];
        List<object> events = [];
        int siblingClicks = 0;
        int outsideClicks = 0;
        int route = 0;
        int stage = 0;
        int completedStages = 0;
        sibling.Click += (_, _) => siblingClicks++;
        outside.Click += (_, _) => outsideClicks++;
        owner.DropDown.Closed += (_, args) => events.Add(new { route = route < routes.Length ? routes[route] : "completed", stage, popup = "child", reason = args.CloseReason.ToString() });
        overflow.Closed += (_, args) => events.Add(new { route = route < routes.Length ? routes[route] : "completed", stage, popup = "overflow", reason = args.CloseReason.ToString() });
        using ApplicationContext loop = new();
        using System.Windows.Forms.Timer timer = new() { Interval = 100 };
        Stopwatch elapsed = Stopwatch.StartNew();
        Exception? failure = null;
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
                strip.Width.Should().Be(450);
                strip.Width = 180;
                strip.PerformLayout();
                owner.Placement.Should().Be(ToolStripItemPlacement.Overflow);
                sibling.Placement.Should().Be(ToolStripItemPlacement.Overflow);
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
                if (elapsed.Elapsed > TimeSpan.FromSeconds(30))
                {
                    throw new TimeoutException($"The source closure probe exceeded its owned loop at route {route}, stage {stage}.");
                }

                switch (stage)
                {
                    case 0:
                        QueueClick(strip, new Point(strip.OverflowButton.Bounds.Left + (strip.OverflowButton.Width / 2), strip.OverflowButton.Bounds.Top + (strip.OverflowButton.Height / 2)));
                        break;
                    case 1:
                        overflow.Visible.Should().BeTrue();
                        owner.GetCurrentParent().Should().BeSameAs(overflow);
                        sibling.GetCurrentParent().Should().BeSameAs(overflow);
                        NativeMethods.SetCursorPosition(overflow.PointToScreen(new Point(owner.Bounds.Left + (owner.Width / 2), owner.Bounds.Top + (owner.Height / 2))));
                        break;
                    case 2:
                        ClickOwner();
                        break;
                    case 3:
                        owner.DropDown.Visible.Should().BeTrue();
                        overflow.Visible.Should().BeTrue();
                        Record("before");
                        MoveToRoute();
                        break;
                    case 4:
                        QueueRoute();
                        break;
                    default:
                        Record("after");
                        owner.DropDown.Close();
                        overflow.Close();
                        NativeMethods.SetCursorPosition(host.PointToScreen(new Point(host.ClientSize.Width - 20, host.ClientSize.Height - 20)));
                        route++;
                        if (route == routes.Length)
                        {
                            Finish();
                        }

                        break;
                }

                completedStages++;
                stage = (stage + 1) % 6;
            }
            catch (Exception exception)
            {
                failure = exception;
                if (route < routes.Length)
                {
                    Record("failed");
                }

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
            overflow.Close();
            host.Close();
            NativeMethods.SetCursorPosition(previousCursor);
            File.WriteAllText(Path.Combine(evidence, "closure-observations.json"), JsonSerializer.Serialize(new
            {
                split,
                completedStages,
                completedRoutes = route,
                failure = failure?.ToString(),
                siblingClicks,
                outsideClicks,
                observations,
                events,
                acquisition = "actual original ToolStripEx450x25 narrowed to180; fixed120x22 retained owner and sibling, queued owned-HWND mouse/keyboard routes, unscaled complete source screen frames; closure outcomes observed without speculative assertions",
            }, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.Out.WriteLine($"toolStripOverflowTooltipEvidence={evidence}");
        }

        if (failure is not null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }

        completedStages.Should().Be(30);
        route.Should().Be(routes.Length);
        observations.Count.Should().Be(routes.Length * 2);
        return;

        void ClickOwner()
        {
            owner.GetCurrentParent().Should().BeSameAs(overflow);
            Rectangle target = owner is ToolStripSplitButton splitOwner ? splitOwner.DropDownButtonBounds : new Rectangle(Point.Empty, owner.Size);
            QueueClick(overflow, new Point(owner.Bounds.Left + target.Left + (target.Width / 2), owner.Bounds.Top + target.Top + (target.Height / 2)));
        }

        void Finish()
        {
            if (finishing)
            {
                return;
            }

            finishing = true;
            timer.Stop();
            owner.DropDown.Close();
            overflow.Close();
            host.Close();
            loop.ExitThread();
        }

        void MoveToRoute()
        {
            Point screen = routes[route] switch
            {
                "outside-button" => outside.PointToScreen(new Point(outside.Width / 2, outside.Height / 2)),
                "sibling-command" => overflow.PointToScreen(new Point(sibling.Bounds.Left + (sibling.Width / 2), sibling.Bounds.Top + (sibling.Height / 2))),
                "blank-parent" => overflow.PointToScreen(Point.Empty),
                _ => NativeMethods.GetCursorPosition(),
            };
            NativeMethods.SetCursorPosition(screen);
        }

        void QueueClick(Control target, Point position)
        {
            Point screen = target.PointToScreen(position);
            NativeMethods.SetCursorPosition(screen);
            WindowFromPoint(screen).Should().Be(target.Handle, "the queued closure input must reach the actual owned HWND under the real cursor");
            GetWindowThreadProcessId(target.Handle, out uint processId).Should().NotBe(0);
            processId.Should().Be((uint)Environment.ProcessId);
            PostMessage(target.Handle, NativeMethods.WmMouseMove, 0, Pack(position)).Should().BeTrue();
            PostMessage(target.Handle, 0x0201, 1, Pack(position)).Should().BeTrue();
            PostMessage(target.Handle, 0x0202, 0, Pack(position)).Should().BeTrue();
        }

        void QueueRoute()
        {
            switch (routes[route])
            {
                case "owner-reclick":
                    ClickOwner();
                    break;
                case "child-escape":
                    owner.DropDown.Visible.Should().BeTrue();
                    GetWindowThreadProcessId(owner.DropDown.Handle, out uint processId).Should().NotBe(0);
                    processId.Should().Be((uint)Environment.ProcessId);
                    PostMessage(owner.DropDown.Handle, 0x0100, (nint)Keys.Escape, 1).Should().BeTrue();
                    PostMessage(owner.DropDown.Handle, 0x0101, (nint)Keys.Escape, unchecked((nint)0xC0000001)).Should().BeTrue();
                    break;
                case "outside-button":
                    QueueClick(outside, new Point(outside.Width / 2, outside.Height / 2));
                    break;
                case "sibling-command":
                    QueueClick(overflow, new Point(sibling.Bounds.Left + (sibling.Width / 2), sibling.Bounds.Top + (sibling.Height / 2)));
                    break;
                case "blank-parent":
                    overflow.ClientRectangle.Contains(Point.Empty).Should().BeTrue();
                    overflow.Items.Cast<ToolStripItem>().Any(item => item.Visible && item.Bounds.Contains(Point.Empty)).Should().BeFalse();
                    QueueClick(overflow, Point.Empty);
                    break;
            }
        }

        void Record(string moment)
        {
            Rectangle bounds = NativeMethods.GetWindowRectangle(host.Handle);
            foreach (ToolStrip popup in new ToolStrip[] { overflow, owner.DropDown })
            {
                if (popup.Visible)
                {
                    bounds = Rectangle.Union(bounds, NativeMethods.GetWindowRectangle(popup.Handle));
                }
            }

            observations.Add(new
            {
                route = routes[route],
                moment,
                stage,
                elapsedMilliseconds = elapsed.ElapsedMilliseconds,
                parentVisible = overflow.Visible,
                childVisible = owner.DropDown.Visible,
                mainNativeVisible = IsWindowVisible(strip.Handle),
                parentNativeVisible = IsWindowVisible(overflow.Handle),
                childNativeVisible = IsWindowVisible(owner.DropDown.Handle),
                owner.Bounds,
                siblingBounds = sibling.Bounds,
                siblingClicks,
                outsideClicks,
                cursor = NativeMethods.GetCursorPosition(),
                screenBounds = bounds,
            });
            NativeMethods.IsEntirelyOnScreen(bounds).Should().BeTrue();
            using Bitmap frame = new(bounds.Width, bounds.Height);
            using Graphics graphics = Graphics.FromImage(frame);
            graphics.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            frame.Save(Path.Combine(evidence, routes[route] + "-" + moment + ".png"), ImageFormat.Png);
        }
    }

    [TestCase(false, true)]
    [TestCase(true, true)]
    [TestCase(false, false)]
    [TestCase(true, false)]
    public void Source_shown_overflow_hint_should_record_the_native_sibling_hit_route(bool split, bool expectedCoveredState)
    {
        Point previousCursor = NativeMethods.GetCursorPosition();
        using Form host = new()
        {
            AutoScaleMode = AutoScaleMode.None,
            StartPosition = FormStartPosition.Manual,
            Location = new Point(50, 50),
            ClientSize = new Size(850, 450),
            Text = "Actual source passive overflow hint route",
        };
        using ToolStripEx strip = new()
        {
            Dock = DockStyle.None,
            AutoSize = false,
            Size = new Size(450, 25),
            GripEnabled = false,
        };
        strip.Items.Add(new ToolStripButton("Main command") { AutoSize = false, Size = new Size(140, 22) });
        using ToolStripDropDownItem owner = split ? new ToolStripSplitButton("Source selector") : new ToolStripDropDownButton("Source selector");
        owner.AutoSize = false;
        owner.Size = new Size(120, 22);
        owner.ToolTipText = "Same retained source owner hint";
        owner.DropDownItems.Add(new ToolStripMenuItem("Source option"));
        using ToolStripButton sibling = new("Sibling command") { AutoSize = false, Size = new Size(120, 22) };
        strip.Items.Add(owner);
        strip.Items.Add(sibling);
        host.Controls.Add(strip);
        ToolStripDropDown overflow = strip.OverflowButton.DropDown;
        string evidence = Path.Combine(TestContext.CurrentContext.TestDirectory, "ToolStripOverflowTooltipProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(evidence);
        List<object> observations = [];
        int siblingClicks = 0;
        int stage = 0;
        bool clickAttempted = false;
        Rectangle intersection = Rectangle.Empty;
        Point acquiredClickPosition = Point.Empty;
        bool hintVisibleAtAcquisition = false;
        bool hintVisibleBeforeInput = false;
        bool? actualCoveredBeforeInput = null;
        bool routeGeometryAvailable = false;
        bool actualSourceOutcomeProven = false;
        string? unsupportedGeometry = null;
        int expectedSiblingClicks = expectedCoveredState ? 0 : 1;
        nint hitBeforeNativeMove = 0;
        nint queuedHit = 0;
        nint inputForeground = 0;
        uint inputForegroundProcessId = 0;
        uint acceptedInputs = 0;
        int inputError = 0;
        nuint inputMarker = 0x47455454;
        List<object> clickObservations = [];
        sibling.Click += (_, _) =>
        {
            siblingClicks++;
            clickObservations.Add(new
            {
                expectedCoveredState,
                siblingClicks,
                cursor = NativeMethods.GetCursorPosition(),
                messagePosition = GetMessagePos(),
                messageTime = GetMessageTime(),
                messageExtraInfo = GetMessageExtraInfo().ToInt64(),
                hintVisibleDuringSiblingClick = IsWindowVisible(TooltipHandle()),
                parentVisible = overflow.Visible,
                childVisible = owner.DropDown.Visible,
            });
        };
        using ApplicationContext loop = new();
        using System.Windows.Forms.Timer timer = new() { Interval = 50 };
        Stopwatch elapsed = Stopwatch.StartNew();
        Stopwatch stageElapsed = new();
        Exception? failure = null;
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
                strip.Width = 180;
                strip.PerformLayout();
                owner.Placement.Should().Be(ToolStripItemPlacement.Overflow);
                sibling.Placement.Should().Be(ToolStripItemPlacement.Overflow);
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
                    throw new TimeoutException($"The source passive-hint probe did not finish stage {stage}.");
                }

                int delay = stage switch { < 2 => 100, 2 or 3 => 750, 4 => 0, _ => 100 };
                if (stageElapsed.ElapsedMilliseconds < delay)
                {
                    return;
                }

                switch (stage)
                {
                    case 0:
                        QueueClick(strip, new Point(strip.OverflowButton.Bounds.Left + (strip.OverflowButton.Width / 2), strip.OverflowButton.Bounds.Top + (strip.OverflowButton.Height / 2)));
                        break;
                    case 1:
                        overflow.Visible.Should().BeTrue();
                        owner.GetCurrentParent().Should().BeSameAs(overflow);
                        sibling.GetCurrentParent().Should().BeSameAs(overflow);
                        NativeMethods.SetCursorPosition(overflow.PointToScreen(new Point(owner.Bounds.Left + (owner.Width / 2), owner.Bounds.Top + (owner.Height / 2))));
                        break;
                    case 2:
                        IsWindowVisible(TooltipHandle()).Should().BeTrue();
                        Rectangle target = owner is ToolStripSplitButton splitOwner ? splitOwner.DropDownButtonBounds : new Rectangle(Point.Empty, owner.Size);
                        QueueClick(overflow, new Point(owner.Bounds.Left + target.Left + (target.Width / 2), owner.Bounds.Top + target.Top + (target.Height / 2)));
                        break;
                    case 3:
                        owner.DropDown.Visible.Should().BeTrue();
                        overflow.Visible.Should().BeTrue();
                        hintVisibleAtAcquisition = IsWindowVisible(TooltipHandle());
                        hintVisibleAtAcquisition.Should().BeTrue();
                        Rectangle tooltipBounds = NativeMethods.GetWindowRectangle(TooltipHandle());
                        Rectangle siblingScreenBounds = overflow.RectangleToScreen(sibling.Bounds);
                        intersection = Rectangle.Intersect(tooltipBounds, siblingScreenBounds);
                        acquiredClickPosition = expectedCoveredState
                            ? new Point(intersection.Left + (intersection.Width / 2), intersection.Top + (intersection.Height / 2))
                            : new Point(siblingScreenBounds.Left + (siblingScreenBounds.Width / 2), siblingScreenBounds.Top + (siblingScreenBounds.Height / 2));
                        routeGeometryAvailable = expectedCoveredState
                            ? intersection.Width > 2 && intersection.Height > 2
                            : siblingScreenBounds.Width > 2 && siblingScreenBounds.Height > 2 && !tooltipBounds.Contains(acquiredClickPosition);
                        if (!routeGeometryAvailable)
                        {
                            unsupportedGeometry = expectedCoveredState
                                ? "The automatically placed native hint did not intersect the sibling sufficiently; no covered-route claim or artificial repositioning."
                                : "The actual sibling center was not naturally outside the shown hint; no positive-control claim or artificial repositioning.";
                        }

                        Record("shown-hint-before-route");
                        if (routeGeometryAvailable)
                        {
                            NativeMethods.SetCursorPosition(acquiredClickPosition);
                            hitBeforeNativeMove = WindowFromPoint(acquiredClickPosition);
                            Record("native-move-queued");
                        }

                        // Native motion may relocate or retire the hint. Returning to the
                        // source message loop preserves that route without targeting an
                        // obsolete HWND or claiming the hint remains visible at dispatch.
                        break;
                    case 4:
                        if (routeGeometryAvailable)
                        {
                            Record("after-native-move-before-click");
                            NativeInput[] inputs =
                            [
                                new() { Type = NativeInput.Mouse, Data = new NativeInputUnion { Mouse = new NativeMouseInput { Flags = NativeMouseInput.LeftDown, ExtraInfo = inputMarker } } },
                                new() { Type = NativeInput.Mouse, Data = new NativeInputUnion { Mouse = new NativeMouseInput { Flags = NativeMouseInput.LeftUp, ExtraInfo = inputMarker } } },
                            ];
                            int inputSize = Marshal.SizeOf<NativeInput>();
                            inputSize.Should().Be(IntPtr.Size == 8 ? 40 : 28, "the native INPUT union must have the Windows ABI size on this architecture");
                            Point screen = NativeMethods.GetCursorPosition();
                            screen.Should().Be(acquiredClickPosition, "the one native click must use the exact saved source point without a second move");
                            if (expectedCoveredState)
                            {
                                intersection.Contains(screen).Should().BeTrue();
                            }

                            overflow.RectangleToScreen(sibling.Bounds).Contains(screen).Should().BeTrue();
                            overflow.GetItemAt(overflow.PointToClient(screen)).Should().BeSameAs(sibling);
                            hintVisibleBeforeInput = IsWindowVisible(TooltipHandle());
                            actualCoveredBeforeInput = hintVisibleBeforeInput && NativeMethods.GetWindowRectangle(TooltipHandle()).Contains(screen);
                            if (actualCoveredBeforeInput != expectedCoveredState)
                            {
                                routeGeometryAvailable = false;
                                unsupportedGeometry = "The native cursor move naturally changed the requested hint coverage before input; no click, replay, repositioning or route claim.";
                                Record("unsupported-coverage-before-input");
                                break;
                            }

                            queuedHit = WindowFromPoint(screen);
                            queuedHit.Should().NotBe(0);
                            GetWindowThreadProcessId(queuedHit, out uint processId).Should().NotBe(0);
                            processId.Should().Be((uint)Environment.ProcessId);
                            queuedHit.Should().Be(expectedCoveredState ? TooltipHandle() : overflow.Handle, "the covered route must really hit the shown hint and the positive control must really hit its source overflow parent");
                            inputForeground = GetForegroundWindow();
                            inputForeground.Should().NotBe(0);
                            GetWindowThreadProcessId(inputForeground, out inputForegroundProcessId).Should().NotBe(0);
                            inputForegroundProcessId.Should().Be((uint)Environment.ProcessId, "native input must not be injected into an unrelated foreground process");
                            clickAttempted = true;
                            acceptedInputs = SendInput((uint)inputs.Length, inputs, inputSize);
                            inputError = Marshal.GetLastWin32Error();
                            acceptedInputs.Should().Be((uint)inputs.Length, $"one native down/up batch must be accepted; Win32 error {inputError}");
                        }

                        break;
                    default:
                        Record("after-route");
                        if (clickAttempted)
                        {
                            siblingClicks.Should().Be(expectedSiblingClicks, "the measured source hint-covered click dismisses without invoking, while the unobstructed actual sibling center invokes once through the identical native actuator");
                            overflow.Visible.Should().BeFalse("both source routes close the parent menu");
                            owner.DropDown.Visible.Should().BeFalse("both source routes close the child menu");
                            IsWindowVisible(overflow.Handle).Should().BeFalse();
                            IsWindowVisible(owner.DropDown.Handle).Should().BeFalse();
                            actualSourceOutcomeProven = true;
                            Record("source-outcome-proven");
                        }

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
            overflow.Close();
            host.Close();
            NativeMethods.SetCursorPosition(previousCursor);
            File.WriteAllText(Path.Combine(evidence, "passive-hint-observations.json"), JsonSerializer.Serialize(new
            {
                split,
                expectedCoveredState,
                expectedSiblingClicks,
                completedStages = stage,
                failure = failure?.ToString(),
                intersection,
                clickAttempted,
                actualSourceOutcomeProven = failure is null && actualSourceOutcomeProven && clickAttempted && acceptedInputs == 2,
                acquiredClickPosition,
                hintVisibleAtAcquisition,
                hintVisibleBeforeInput,
                actualCoveredBeforeInput,
                hitBeforeNativeMove = hitBeforeNativeMove.ToInt64(),
                queuedHit = queuedHit.ToInt64(),
                inputForeground = inputForeground.ToInt64(),
                inputForegroundProcessId,
                acceptedInputs,
                inputError,
                inputMarker = (ulong)inputMarker,
                siblingClicks,
                clickObservations,
                observations,
                acquisition = "actual original ToolStripEx450x25 narrowed to180; automatically shown unmodified owner hint; covered route uses its actual sibling intersection, positive control uses the naturally uncovered actual sibling center; actual cursor move returned to the source message loop, then exact saved point/sibling, current hint coverage and process-owned WindowFromPoint/foreground were guarded before the identical one SendInput down/up batch; no waiting for hint closure, input replay, artificial repositioning or direct tooltip-forwarding claim",
                unsupportedGeometry,
            }, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.Out.WriteLine($"toolStripOverflowTooltipEvidence={evidence}");
        }

        if (failure is not null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }

        stage.Should().Be(6);
        if (unsupportedGeometry is not null)
        {
            Assert.Ignore(unsupportedGeometry);
        }

        return;

        nint TooltipHandle()
        {
            ToolTip tooltip = typeof(ToolStrip).GetProperty("ToolTip", InstanceMembers)?.GetValue(overflow) as ToolTip
                ?? throw new MissingMemberException(typeof(ToolStrip).FullName, "ToolTip");
            return (nint)(typeof(ToolTip).GetProperty("Handle", InstanceMembers)?.GetValue(tooltip)
                ?? throw new MissingMemberException(typeof(ToolTip).FullName, "Handle"));
        }

        void QueueClick(Control parent, Point target)
        {
            Point screen = parent.PointToScreen(target);
            NativeMethods.SetCursorPosition(screen);
            WindowFromPoint(screen).Should().Be(parent.Handle);
            GetWindowThreadProcessId(parent.Handle, out uint processId).Should().NotBe(0);
            processId.Should().Be((uint)Environment.ProcessId);
            PostMessage(parent.Handle, NativeMethods.WmMouseMove, 0, Pack(target)).Should().BeTrue();
            PostMessage(parent.Handle, 0x0201, 1, Pack(target)).Should().BeTrue();
            PostMessage(parent.Handle, 0x0202, 0, Pack(target)).Should().BeTrue();
        }

        void Record(string moment)
        {
            nint tooltip = TooltipHandle();
            bool hintVisible = IsWindowVisible(tooltip);
            Rectangle bounds = NativeMethods.GetWindowRectangle(host.Handle);
            foreach (nint window in new nint[] { overflow.Handle, owner.DropDown.Handle, tooltip })
            {
                if (IsWindowVisible(window))
                {
                    bounds = Rectangle.Union(bounds, NativeMethods.GetWindowRectangle(window));
                }
            }

            Point cursor = NativeMethods.GetCursorPosition();
            observations.Add(new
            {
                moment,
                stage,
                expectedCoveredState,
                expectedSiblingClicks,
                actualSourceOutcomeProven,
                elapsedMilliseconds = elapsed.ElapsedMilliseconds,
                hintVisible,
                tooltipHandle = tooltip.ToInt64(),
                tooltipBounds = NativeMethods.GetWindowRectangle(tooltip),
                siblingScreenBounds = overflow.RectangleToScreen(sibling.Bounds),
                intersection,
                cursor,
                actualHit = WindowFromPoint(cursor).ToInt64(),
                acquiredClickPosition,
                hintVisibleAtAcquisition,
                hintVisibleBeforeInput,
                actualCoveredBeforeInput,
                routeGeometryAvailable,
                unsupportedGeometry,
                hitBeforeNativeMove = hitBeforeNativeMove.ToInt64(),
                queuedHit = queuedHit.ToInt64(),
                inputForeground = inputForeground.ToInt64(),
                inputForegroundProcessId,
                acceptedInputs,
                parentVisible = overflow.Visible,
                childVisible = owner.DropDown.Visible,
                siblingClicks,
                screenBounds = bounds,
            });
            NativeMethods.IsEntirelyOnScreen(bounds).Should().BeTrue();
            using Bitmap frame = new(bounds.Width, bounds.Height);
            using Graphics graphics = Graphics.FromImage(frame);
            graphics.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            frame.Save(Path.Combine(evidence, moment + ".png"), ImageFormat.Png);
        }

        void Finish()
        {
            finishing = true;
            timer.Stop();
            owner.DropDown.Close();
            overflow.Close();
            host.Close();
            loop.ExitThread();
        }
    }

    private static nint Pack(Point point) => (nint)((point.X & 0xFFFF) | (point.Y << 16));

    [DllImport("user32.dll", EntryPoint = "PostMessageW")]
    private static extern bool PostMessage(nint window, uint message, nint parameter, nint data);

    [DllImport("user32.dll")]
    private static extern nint WindowFromPoint(Point point);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(nint window);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(nint window, out uint processId);

    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern nint GetMessageExtraInfo();

    [DllImport("user32.dll")]
    private static extern uint GetMessagePos();

    [DllImport("user32.dll")]
    private static extern int GetMessageTime();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint count, [In] NativeInput[] inputs, int inputSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeInput
    {
        public const uint Mouse = 0;

        public uint Type;
        public NativeInputUnion Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct NativeInputUnion
    {
        [FieldOffset(0)]
        public NativeMouseInput Mouse;

        [FieldOffset(0)]
        public NativeKeyboardInput Keyboard;

        [FieldOffset(0)]
        public NativeHardwareInput Hardware;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeMouseInput
    {
        public const uint LeftDown = 0x0002;
        public const uint LeftUp = 0x0004;

        public int X;
        public int Y;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public nuint ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeKeyboardInput
    {
        public ushort VirtualKey;
        public ushort ScanCode;
        public uint Flags;
        public uint Time;
        public nuint ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeHardwareInput
    {
        public uint Message;
        public ushort ParameterLow;
        public ushort ParameterHigh;
    }
}
