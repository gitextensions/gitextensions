using System.Drawing.Imaging;
using System.Text.Json;
using AwesomeAssertions;
using GitExtensions.ParityCapture;
using GitUI;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class ImageCaptureTests
{
    [Test]
    public void Popup_capture_should_render_owned_windows_instead_of_an_occluding_desktop_window()
    {
        using Form form = new() { ClientSize = new Size(240, 180), BackColor = Color.CornflowerBlue };
        using ContextMenuStrip menu = new() { AutoClose = false };
        menu.Items.Add("Captured action");
        form.Show();
        Application.DoEvents();
        form.Refresh();
        menu.Show(form, new Point(180, 120));

        // Show/focus queues native client and popup painting. Settle those real
        // messages before another window covers the owned surfaces; PrintWindow
        // must render them independently of the later occluding desktop pixels.
        long paintingStartedAt = Environment.TickCount64;
        long paintingSettledAt = paintingStartedAt + 100;
        while (Environment.TickCount64 < paintingSettledAt)
        {
            Application.DoEvents();
        }

        form.Refresh();
        menu.Refresh();
        form.Update();
        menu.Update();
        long paintingSettlementMilliseconds = Environment.TickCount64 - paintingStartedAt;
        using Form occluder = CreateOccluder(form);
        occluder.Show();
        Application.DoEvents();
        menu.Visible.Should().BeTrue();
        form.DeviceDpi.Should().Be(96);

        using CaptureImageResult result = ImageCapture.Capture(form, [menu], []);
        Point sample = form.PointToScreen(new Point(20, 80));
        Point popupSample = new(menu.Left + 8, menu.Top + 8);
        Color client = result.Bitmap.GetPixel(sample.X - result.ScreenBounds.X, sample.Y - result.ScreenBounds.Y);
        Color popup = result.Bitmap.GetPixel(popupSample.X - result.ScreenBounds.X, popupSample.Y - result.ScreenBounds.Y);
        string directory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "ImageCaptureProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        result.Bitmap.Save(Path.Combine(directory, "occluded-owned-popup.png"), ImageFormat.Png);
        File.WriteAllText(Path.Combine(directory, "metadata.json"), JsonSerializer.Serialize(new
        {
            inputMode = "ownedPopupOccludedByUnrelatedWindow",
            dpiMode = "nativeMonitor",
            deviceDpi = form.DeviceDpi,
            captureMethod = result.Method.ToString(),
            paintingSettlementMilliseconds,
            formVisible = form.Visible,
            popupVisible = menu.Visible,
            occluderVisible = occluder.Visible,
            occluder.TopMost,
            client = $"#{client.ToArgb():X8}",
            expectedClient = $"#{Color.CornflowerBlue.ToArgb():X8}",
            popup = $"#{popup.ToArgb():X8}",
            occluder = $"#{occluder.BackColor.ToArgb():X8}",
            result.ScreenBounds,
            popupBounds = menu.Bounds,
            occluderBounds = occluder.Bounds
        }, new JsonSerializerOptions { WriteIndented = true }));
        TestContext.Out.WriteLine($"diagnostics={directory} captureMethod={result.Method} deviceDpi={form.DeviceDpi} paintingSettlementMilliseconds={paintingSettlementMilliseconds} client=#{client.ToArgb():X8} popup=#{popup.ToArgb():X8} occluder=#{occluder.BackColor.ToArgb():X8} popupVisible={menu.Visible} occluderVisible={occluder.Visible}");
        result.Method.Should().Be(CaptureMethod.PrintWindow);
        client.ToArgb().Should().Be(Color.CornflowerBlue.ToArgb());
        popup.ToArgb().Should().NotBe(Color.Magenta.ToArgb());
        result.ScreenBounds.Contains(menu.Bounds).Should().BeTrue();
    }

    [Test]
    public void Native_browser_capture_should_reject_an_occluded_screen_surface()
    {
        using Form form = new() { ClientSize = new Size(240, 180) };
        using WebBrowser browser = new() { Dock = DockStyle.Fill };
        form.Controls.Add(browser);
        form.Show();
        using Form occluder = CreateOccluder(form);
        occluder.Show();
        Application.DoEvents();

        ImageCapture.RequiresScreenGrab(form).Should().BeTrue();
        Action capture = () => ImageCapture.Capture(form, [], []);
        capture.Should().Throw<CaptureStateUnsupportedException>().WithMessage("*unrelated window occludes*");
    }

    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    public void Frozen_native_toolbar_capture_should_acquire_its_actual_unscaled_pixels_without_changing_state(bool includePopup, bool captureStrip)
    {
        using Form form = CreateFrozenStripHost(out ToolStripEx strip, out ToolStripDropDownButton button, syntheticNonOverlappingPopup: true);
        ToolStripDropDown popup = button.DropDown;
        Rectangle stripBounds = NativeMethods.GetWindowRectangle(strip.Handle);
        string directory = RetainFrozenSurfaceDiagnostic(form, strip, popup, "synthetic-nonoverlapping-before-acquisition");
        NativeMethods.IsRedrawDisabled(strip.Handle).Should().BeTrue();
        popup.Visible.Should().BeTrue();
        popup.Bounds.IntersectsWith(stripBounds).Should().BeFalse();
        form.DeviceDpi.Should().Be(96);
        using Bitmap actualScreen = new(stripBounds.Width, stripBounds.Height, PixelFormat.Format32bppArgb);
        using (Graphics graphics = Graphics.FromImage(actualScreen))
        {
            graphics.CopyFromScreen(stripBounds.Location, Point.Empty, stripBounds.Size, CopyPixelOperation.SourceCopy);
        }

        using CaptureImageResult result = ImageCapture.Capture(captureStrip ? strip : form, includePopup ? [popup] : [], []);
        result.Bitmap.Save(Path.Combine(directory, "frozen-toolbar-owned.png"), ImageFormat.Png);
        actualScreen.Save(Path.Combine(directory, "native-toolbar-screen.png"), ImageFormat.Png);
        using Bitmap afterScreen = new(stripBounds.Width, stripBounds.Height, PixelFormat.Format32bppArgb);
        using (Graphics graphics = Graphics.FromImage(afterScreen))
        {
            graphics.CopyFromScreen(stripBounds.Location, Point.Empty, stripBounds.Size, CopyPixelOperation.SourceCopy);
        }

        afterScreen.Save(Path.Combine(directory, "native-toolbar-screen-after-printwindow.png"), ImageFormat.Png);
        File.WriteAllText(Path.Combine(directory, "metadata.json"), JsonSerializer.Serialize(new
        {
            inputMode = "originalToolStripExDropDownOpen; fixture explicitly moves the popup outside the frozen owner to exercise the nonoverlapping composite path",
            includePopup,
            captureStrip,
            captureMethod = result.Method.ToString(),
            result.Acquisitions,
            result.AcquisitionNote,
            result.NativePopupShadows,
            stripBounds,
            primaryBounds = result.PrimaryScreenBounds,
            popupBounds = popup.Bounds,
            redrawDisabledAfterCapture = NativeMethods.IsRedrawDisabled(strip.Handle),
            popupVisibleAfterCapture = popup.Visible,
            fixtureSurface = "Controlled borderless opaque host; no claim about real DWM rounded/non-client chrome raster stability.",
            beforePrintWindow = $"#{actualScreen.GetPixel(actualScreen.Width - 1, actualScreen.Height / 2).ToArgb():X8}",
            afterPrintWindow = $"#{afterScreen.GetPixel(afterScreen.Width - 1, afterScreen.Height / 2).ToArgb():X8}",
        }, new JsonSerializerOptions { WriteIndented = true }));
        TestContext.Out.WriteLine($"diagnostics={directory} captureMethod={result.Method} includePopup={includePopup} captureStrip={captureStrip}");
        result.Method.Should().Be(CaptureMethod.PrintWindowScreenGrabComposite);
        result.AcquisitionNote.Should().NotBeNullOrWhiteSpace();
        CaptureImageAcquisition acquisition = result.Acquisitions?.Single()
            ?? throw new InvalidOperationException("The frozen native strip must record one screen acquisition.");
        acquisition.SurfaceRole.Should().Be("primary");
        acquisition.CaptureMethod.Should().Be(CaptureMethod.ScreenGrab);
        acquisition.Reason.Should().Be("redrawDisabledNativeSurface");
        acquisition.RegionPx.Should().BeEquivalentTo(new CaptureRectangle
        {
            X = stripBounds.X - result.PrimaryScreenBounds.X,
            Y = stripBounds.Y - result.PrimaryScreenBounds.Y,
            Width = stripBounds.Width,
            Height = stripBounds.Height
        });
        Point offset = new(stripBounds.X - result.ScreenBounds.X, stripBounds.Y - result.ScreenBounds.Y);
        int changedPixels = 0;
        for (int y = 0; y < actualScreen.Height; y++)
        {
            for (int x = 0; x < actualScreen.Width; x++)
            {
                if (actualScreen.GetPixel(x, y).ToArgb() != result.Bitmap.GetPixel(offset.X + x, offset.Y + y).ToArgb())
                {
                    changedPixels++;
                }
            }
        }

        changedPixels.Should().Be(0, "every acquired pixel must come from the whole actual native HWND without scaling or repainting");
        NativeMethods.IsRedrawDisabled(strip.Handle).Should().BeTrue();
        popup.Visible.Should().BeTrue();
        popup.Close();
        NativeMethods.IsRedrawDisabled(strip.Handle).Should().BeFalse("only the original DropDownClosed handler resumes its owner");
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Frozen_native_toolbar_capture_should_acquire_the_whole_requested_natural_overlap_from_screen(bool captureStrip)
    {
        using Form form = CreateFrozenStripHost(out ToolStripEx strip, out ToolStripDropDownButton button);
        ToolStripDropDown popup = button.DropDown;
        Rectangle stripBounds = NativeMethods.GetWindowRectangle(strip.Handle);
        string directory = RetainFrozenSurfaceDiagnostic(form, strip, popup, "natural-overlap-before-acquisition");
        Rectangle popupBounds = NativeMethods.GetWindowRectangle(popup.Handle);
        popupBounds.IntersectsWith(stripBounds).Should().BeTrue("the actual source popup opens across its frozen owner's lower edge");
        Control root = captureStrip ? strip : form;
        Rectangle primaryBounds = ImageCapture.GetPrimaryScreenBounds(root);
        Rectangle bounds = Rectangle.Union(primaryBounds, popupBounds);
        using Bitmap actualScreen = CopyDiagnosticScreenFootprints(primaryBounds, popupBounds);

        using CaptureImageResult capture = ImageCapture.Capture(root, [popup], []);

        capture.Bitmap.Save(Path.Combine(directory, "natural-overlap-owned.png"), ImageFormat.Png);
        actualScreen.Save(Path.Combine(directory, "natural-overlap-screen.png"), ImageFormat.Png);
        File.WriteAllText(Path.Combine(directory, "metadata.json"), JsonSerializer.Serialize(new
        {
            captureStrip,
            captureMethod = capture.Method.ToString(),
            capture.AcquisitionNote,
            capture.NativePopupShadows,
            capture.ScreenBounds,
            capture.PrimaryScreenBounds,
            stripBounds,
            popupBounds,
            note = "Actual whole requested screen footprints, including the known popup overlap; not an unobscured toolbar or a PrintWindow composite.",
        }, new JsonSerializerOptions { WriteIndented = true }));
        capture.Method.Should().Be(CaptureMethod.ScreenGrab);
        capture.Acquisitions.Should().BeNull("the entire requested surfaces use the screen API, not supplemental PrintWindow regions");
        capture.AcquisitionNote.Should().NotBeNullOrWhiteSpace();
        NativePopupShadow shadow = capture.NativePopupShadows?.Single()
            ?? throw new InvalidOperationException("The actual native popup must retain its associated system shadow receipt.");
        shadow.PopupHandle.Should().Be(popup.Handle.ToString());
        shadow.PopupNextWindow.Should().Be(shadow.Handle);
        shadow.ShadowPreviousWindow.Should().Be(shadow.PopupHandle);
        shadow.ProcessId.Should().Be((uint)Environment.ProcessId);
        shadow.ThreadId.Should().BeGreaterThan(0);
        shadow.ShadowProcessId.Should().Be(shadow.ProcessId);
        shadow.ShadowThreadId.Should().Be(shadow.ThreadId);
        (shadow.PopupClassStyle & 0x00020000).Should().Be(0x00020000);
        shadow.Bounds.Location.Should().Be(popupBounds.Location);
        shadow.Bounds.Contains(popupBounds).Should().BeTrue();
        capture.ScreenBounds.Should().Be(bounds);
        capture.PrimaryScreenBounds.Should().Be(primaryBounds);
        int changedPixels = 0;
        for (int y = 0; y < actualScreen.Height; y++)
        {
            for (int x = 0; x < actualScreen.Width; x++)
            {
                if (actualScreen.GetPixel(x, y).ToArgb() != capture.Bitmap.GetPixel(x, y).ToArgb())
                {
                    changedPixels++;
                }
            }
        }

        changedPixels.Should().Be(0, "every requested surface pixel, including the real overlap, is acquired unscaled from the screen");
        NativeMethods.IsRedrawDisabled(strip.Handle).Should().BeTrue();
        popup.Visible.Should().BeTrue();
    }

    [Test]
    public void Frozen_native_toolbar_capture_should_reject_an_overlapping_unrequested_popup()
    {
        using Form form = CreateFrozenStripHost(out ToolStripEx strip, out ToolStripDropDownButton button);
        Rectangle stripBounds = NativeMethods.GetWindowRectangle(strip.Handle);
        RetainFrozenSurfaceDiagnostic(form, strip, button.DropDown, "unrequested-overlap-before-acquisition");
        NativeMethods.GetWindowRectangle(button.DropDown.Handle).IntersectsWith(stripBounds).Should().BeTrue();

        Action capture = () => ImageCapture.Capture(form, [], []);

        capture.Should().Throw<CaptureStateUnsupportedException>().WithMessage("*unrelated window occludes*");
        NativeMethods.IsRedrawDisabled(strip.Handle).Should().BeTrue();
        button.DropDown.Visible.Should().BeTrue();
    }

    [Test]
    public void Frozen_native_toolbar_capture_should_reject_an_unrelated_occluding_window()
    {
        using Form form = CreateFrozenStripHost(out ToolStripEx strip, out ToolStripDropDownButton button);
        using Form occluder = CreateOccluder(form);
        occluder.Show();
        Application.DoEvents();
        button.DropDown.Visible.Should().BeTrue();
        RetainFrozenSurfaceDiagnostic(form, strip, button.DropDown, "unrelated-occluder-before-acquisition");

        Action capture = () => ImageCapture.Capture(form, [button.DropDown], []);

        capture.Should().Throw<CaptureStateUnsupportedException>().WithMessage("*unrelated window occludes*");
        NativeMethods.IsRedrawDisabled(strip.Handle).Should().BeTrue();
    }

    [Test]
    public void Frozen_native_toolbar_capture_should_reject_a_different_same_process_popup_and_its_shadow()
    {
        // This deliberately occluded scene stays non-topmost: native menu shadow
        // decoration below an opaque topmost host is not a visible occluder. This
        // rejection probe permits other desktop blockers, never accepted pixels.
        using Form form = CreateFrozenStripHost(out ToolStripEx strip, out ToolStripDropDownButton button, topMost: false);
        using ShadowedContextMenuStrip unrelatedPopup = new() { AutoClose = false, DropShadowEnabled = true };
        unrelatedPopup.Items.Add("Unrequested same-process popup");
        unrelatedPopup.Show(form, new Point(80, 80));
        Application.DoEvents();
        unrelatedPopup.Refresh();
        unrelatedPopup.Update();
        SettleDesktopFrame();
        button.DropDown.Visible.Should().BeTrue();
        NativeMethods.IsRedrawDisabled(strip.Handle).Should().BeTrue();
        string directory = RetainFrozenSurfaceDiagnostic(form, strip, button.DropDown, "different-same-process-shadow-before-acquisition");
        IReadOnlyList<NativeWindowOccluder> windowAndNeighbors = NativeMethods.GetNativeWindowAndNeighborReceipts(unrelatedPopup.Handle);
        NativeWindowOccluder otherPopup = windowAndNeighbors.Single(window => window.Handle == unrelatedPopup.Handle.ToString());
        HashSet<IntPtr> requestedWindows = [form.Handle, button.DropDown.Handle];
        IReadOnlyList<NativeWindowOccluder> blockers = NativeMethods.GetOccludingNativeWindows(
            form.Handle, ImageCapture.GetPrimaryScreenBounds(form), requestedWindows);
        File.WriteAllText(Path.Combine(directory, "unrequested-popup-native-receipt.json"), JsonSerializer.Serialize(new
        {
            unrelatedPopup.DropShadowEnabled,
            unrelatedPopup.Visible,
            windowAndNeighbors,
            rawBlockers = blockers,
            associatedShadows = NativeMethods.GetAssociatedPopupShadows([unrelatedPopup.Handle]),
        }, new JsonSerializerOptions { WriteIndented = true }));
        otherPopup.ProcessId.Should().Be((uint)Environment.ProcessId);
        otherPopup.ThreadId.Should().BeGreaterThan(0);
        (otherPopup.ClassStyle & 0x00020000).Should().Be(0x00020000);

        // A real second popup's shadow need not be adjacent to its HWND. Locate
        // the unknown native surface from its raw receipt, not the strict known-
        // popup association API; neither its process nor its class exempts it.
        NativeWindowOccluder otherShadow = blockers.Single(blocker => blocker.ClassName == "SysShadow"
            && blocker.ProcessId == otherPopup.ProcessId && blocker.ThreadId == otherPopup.ThreadId
            && blocker.Bounds.Location == otherPopup.Bounds.Location && blocker.Bounds.Contains(otherPopup.Bounds));
        NativeMethods.GetAssociatedPopupShadows(requestedWindows).Should().NotContain(shadow => shadow.Handle == otherShadow.Handle);
        blockers.Should().Contain(blocker => blocker.Handle == otherPopup.Handle);
        blockers.Should().Contain(blocker => blocker.Handle == otherShadow.Handle && blocker.ClassName == "SysShadow"
            && blocker.ProcessId == (uint)Environment.ProcessId);

        Action capture = () => ImageCapture.Capture(form, [button.DropDown], []);

        capture.Should().Throw<CaptureStateUnsupportedException>().WithMessage("*unrelated window occludes*");
        NativeMethods.IsRedrawDisabled(strip.Handle).Should().BeTrue();
        button.DropDown.Visible.Should().BeTrue();
        unrelatedPopup.Visible.Should().BeTrue();
    }

    [Test]
    public void Frozen_native_toolbar_capture_should_reject_a_surface_outside_the_visible_screen_area()
    {
        using Form form = CreateFrozenStripHost(out ToolStripEx strip, out ToolStripDropDownButton button,
            SystemInformation.VirtualScreen.Width + 200);
        Rectangle stripBounds = NativeMethods.GetWindowRectangle(strip.Handle);
        NativeMethods.IsEntirelyOnScreen(stripBounds).Should().BeFalse();

        Action capture = () => ImageCapture.Capture(form, [button.DropDown], []);

        capture.Should().Throw<CaptureStateUnsupportedException>().WithMessage("*visible screen area*");
        NativeMethods.IsRedrawDisabled(strip.Handle).Should().BeTrue();
    }

    [Test]
    public void IsEntirelyOnScreen_should_accept_full_monitor_bounds_and_reject_desktop_gaps_or_empty_rectangles()
    {
        foreach (Screen screen in Screen.AllScreens)
        {
            NativeMethods.IsEntirelyOnScreen(screen.Bounds).Should().BeTrue();
        }

        Rectangle desktop = SystemInformation.VirtualScreen;
        NativeMethods.IsEntirelyOnScreen(new Rectangle(desktop.Right, desktop.Top, 1, 1)).Should().BeFalse();
        NativeMethods.IsEntirelyOnScreen(Rectangle.Empty).Should().BeFalse();
    }

    [Test]
    public void Frozen_native_toolbar_acquisition_guard_should_reject_a_covering_sibling_of_its_ancestor()
    {
        using Form form = CreateFrozenStripHost(out ToolStripEx strip, out ToolStripDropDownButton button, nestedOwner: true);
        Control parent = strip.Parent ?? throw new InvalidOperationException("The fixture requires a nested native owner.");
        Rectangle stripBounds = NativeMethods.GetWindowRectangle(strip.Handle);
        HashSet<IntPtr> capturedWindows = [form.Handle, button.DropDown.Handle];
        RetainFrozenSurfaceDiagnostic(form, strip, button.DropDown, "ancestor-before-cover");
        NativeMethods.IsOwnedNativeSurfaceUnoccluded(strip.Handle, form.Handle, stripBounds, capturedWindows).Should().BeTrue();
        using Panel coveringSibling = new() { Bounds = parent.Bounds, BackColor = Color.Magenta };
        form.Controls.Add(coveringSibling);
        coveringSibling.BringToFront();
        coveringSibling.Refresh();
        Application.DoEvents();
        RetainFrozenSurfaceDiagnostic(form, strip, button.DropDown, "ancestor-covered");

        NativeMethods.IsRedrawDisabled(strip.Handle).Should().BeTrue();
        NativeMethods.IsUnoccluded(strip.Handle, stripBounds, capturedWindows).Should().BeTrue(
            "the direct child-sibling scan alone cannot see a covering ancestor sibling");
        NativeMethods.IsOwnedNativeSurfaceUnoccluded(strip.Handle, form.Handle, stripBounds, capturedWindows).Should().BeFalse();
    }

    [Test]
    public void Frozen_native_toolbar_acquisition_guard_should_reject_an_ancestor_client_clip()
    {
        using Form form = CreateFrozenStripHost(out ToolStripEx strip, out ToolStripDropDownButton button, nestedOwner: true);
        Control parent = strip.Parent ?? throw new InvalidOperationException("The fixture requires a nested native owner.");
        Rectangle stripBounds = NativeMethods.GetWindowRectangle(strip.Handle);
        HashSet<IntPtr> capturedWindows = [form.Handle, button.DropDown.Handle];
        RetainFrozenSurfaceDiagnostic(form, strip, button.DropDown, "ancestor-before-clip");
        NativeMethods.IsOwnedNativeSurfaceUnoccluded(strip.Handle, form.Handle, stripBounds, capturedWindows).Should().BeTrue();
        parent.Height = Math.Max(1, strip.Height / 2);
        Application.DoEvents();
        stripBounds = NativeMethods.GetWindowRectangle(strip.Handle);
        RetainFrozenSurfaceDiagnostic(form, strip, button.DropDown, "ancestor-clipped");

        NativeMethods.IsRedrawDisabled(strip.Handle).Should().BeTrue();
        NativeMethods.TryGetNativeClientRectangle(parent.Handle, out Rectangle clientBounds).Should().BeTrue();
        clientBounds.Contains(stripBounds).Should().BeFalse();
        NativeMethods.IsOwnedNativeSurfaceUnoccluded(strip.Handle, form.Handle, stripBounds, capturedWindows).Should().BeFalse();
    }

    private static string RetainFrozenSurfaceDiagnostic(Form form, ToolStrip strip, ToolStripDropDown popup, string stage)
    {
        string directory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "ImageCaptureProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        Rectangle primaryBounds = ImageCapture.GetPrimaryScreenBounds(form);
        Rectangle stripBounds = NativeMethods.GetWindowRectangle(strip.Handle);
        Rectangle popupBounds = NativeMethods.GetWindowRectangle(popup.Handle);
        HashSet<IntPtr> capturedWindows = [form.Handle, popup.Handle];
        List<object> ancestors = [];
        HashSet<IntPtr> visited = [];
        for (IntPtr ancestor = NativeMethods.GetNativeParentWindow(strip.Handle);
             ancestor != IntPtr.Zero && visited.Add(ancestor);
             ancestor = NativeMethods.GetNativeParentWindow(ancestor))
        {
            bool hasClientBounds = NativeMethods.TryGetNativeClientRectangle(ancestor, out Rectangle clientBounds);
            ancestors.Add(new
            {
                hwnd = ancestor.ToString(),
                type = Control.FromHandle(ancestor)?.GetType().FullName,
                outerBounds = NativeMethods.GetWindowRectangle(ancestor),
                hasClientBounds,
                clientBounds,
                shown = NativeMethods.IsWindowShown(ancestor),
                redrawDisabled = NativeMethods.IsRedrawDisabled(ancestor),
                blockers = NativeMethods.GetOccludingNativeWindows(ancestor, stripBounds, capturedWindows),
            });
            if (ancestor == form.Handle)
            {
                break;
            }
        }

        bool fullyOnScreen = NativeMethods.IsEntirelyOnScreen(primaryBounds) && NativeMethods.IsEntirelyOnScreen(popupBounds);
        bool unoccluded = NativeMethods.IsUnoccluded(form.Handle, primaryBounds, capturedWindows)
            && NativeMethods.IsUnoccluded(popup.Handle, popupBounds, capturedWindows);
        File.WriteAllText(Path.Combine(directory, "before-acquisition.json"), JsonSerializer.Serialize(new
        {
            stage,
            diagnosticOnly = true,
            acquisition = "diagnostic requested-window screen footprints, possibly including occluder pixels; never an accepted capture or unobscured primary",
            fullyOnScreen,
            unoccluded,
            primaryHwnd = form.Handle.ToString(),
            stripHwnd = strip.Handle.ToString(),
            popupHwnd = popup.Handle.ToString(),
            associatedPopupShadows = NativeMethods.GetAssociatedPopupShadows([popup.Handle]),
            primaryBounds,
            stripBounds,
            popupBounds,
            nativePopupIntersectsStrip = popupBounds.IntersectsWith(stripBounds),
            managedPopupIntersectsStrip = popup.Bounds.IntersectsWith(stripBounds),
            stripRedrawDisabled = NativeMethods.IsRedrawDisabled(strip.Handle),
            hostBlockers = NativeMethods.GetOccludingNativeWindows(form.Handle, primaryBounds, capturedWindows),
            stripBlockers = NativeMethods.GetOccludingNativeWindows(strip.Handle, stripBounds, capturedWindows),
            popupBlockers = NativeMethods.GetOccludingNativeWindows(popup.Handle, popupBounds, capturedWindows),
            popup.Visible,
            ancestors,
        }, new JsonSerializerOptions { WriteIndented = true }));
        if (fullyOnScreen)
        {
            using Bitmap bitmap = CopyDiagnosticScreenFootprints(primaryBounds, popupBounds);
            bitmap.Save(Path.Combine(directory, "diagnostic-owned-visible-frame.png"), ImageFormat.Png);
        }

        TestContext.Out.WriteLine($"preAcquisitionDiagnostics={directory} stage={stage} stripBounds={stripBounds} popupBounds={popupBounds} intersects={popupBounds.IntersectsWith(stripBounds)}");
        return directory;
    }

    private static Bitmap CopyDiagnosticScreenFootprints(Rectangle primaryBounds, Rectangle popupBounds)
    {
        Rectangle bounds = Rectangle.Union(primaryBounds, popupBounds);
        Bitmap bitmap = new(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
        try
        {
            using Graphics graphics = Graphics.FromImage(bitmap);
            foreach (Rectangle surface in new[] { primaryBounds, popupBounds })
            {
                graphics.CopyFromScreen(surface.Location, new Point(surface.X - bounds.X, surface.Y - bounds.Y),
                    surface.Size, CopyPixelOperation.SourceCopy);
            }

            return bitmap;
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }
    }

    private static Form CreateFrozenStripHost(out ToolStripEx strip, out ToolStripDropDownButton button,
        int width = 360, bool nestedOwner = false, bool syntheticNonOverlappingPopup = false, bool topMost = true)
    {
        Rectangle monitor = Screen.PrimaryScreen?.WorkingArea
            ?? throw new InvalidOperationException("The native pixel-acquisition fixture requires a desktop monitor.");
        Form form = new()
        {
            StartPosition = FormStartPosition.Manual,
            Location = new Point(monitor.Left + 16, monitor.Top + 16),
            ClientSize = new Size(width, 180),

            // Exact API pixels use an opaque controlled surface, not DWM-rounded
            // non-client chrome which exposes changing desktop pixels behind it.
            FormBorderStyle = FormBorderStyle.None,
            BackColor = Color.CornflowerBlue,
            TopMost = topMost,
            ShowInTaskbar = false
        };
        strip = new ToolStripEx { Dock = DockStyle.Top, BackColor = Color.LimeGreen, GripStyle = ToolStripGripStyle.Hidden };
        button = syntheticNonOverlappingPopup
            ? new SyntheticDropDownButton("Frozen source action")
            : new ToolStripDropDownButton("Frozen source action");
        button.DropDownItems.Add("Captured action");
        button.DropDown.AutoClose = false;
        strip.Items.Add(button);
        if (nestedOwner)
        {
            Panel parent = new() { Location = new Point(8, 8), Size = new Size(width - 16, 100), BorderStyle = BorderStyle.FixedSingle };
            parent.Controls.Add(strip);
            form.Controls.Add(parent);
        }
        else
        {
            form.Controls.Add(strip);
        }

        form.Show();
        Application.DoEvents();
        form.Refresh();
        form.Update();
        button.ShowDropDown();
        SettleDesktopFrame();

        if (syntheticNonOverlappingPopup)
        {
            // This deliberate fixture-only scene is not the source's natural popup
            // placement. Capture never relocates an opened popup or changes redraw state.
            Rectangle stripBounds = NativeMethods.GetWindowRectangle(strip.Handle);
            SyntheticDropDownButton syntheticOwner = (SyntheticDropDownButton)button;
            syntheticOwner.SyntheticLocation = new Point(button.DropDown.Left, stripBounds.Bottom + 1);
            button.DropDown.Location = syntheticOwner.SyntheticLocation.Value;
            Application.DoEvents();
            button.DropDown.Refresh();
            button.DropDown.Update();
            SettleDesktopFrame();
        }

        return form;
    }

    private sealed class SyntheticDropDownButton(string text) : ToolStripDropDownButton(text)
    {
        public Point? SyntheticLocation { get; set; }

        // Native popup SetBoundsCore resolves its owner location again; overriding
        // that fixture input preserves the real owner and original redraw handlers.
        protected override Point DropDownLocation => SyntheticLocation ?? base.DropDownLocation;
    }

    private sealed class ShadowedContextMenuStrip : ContextMenuStrip
    {
        // This guard fixture explicitly requests a real system shadow; it does not
        // substitute a painted fake or infer a shadow from the managed control type.
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams parameters = base.CreateParams;
                parameters.ClassStyle |= 0x00020000;
                return parameters;
            }
        }
    }

    private static void SettleDesktopFrame()
    {
        long settledAt = Environment.TickCount64 + 500;
        while (Environment.TickCount64 < settledAt)
        {
            Application.DoEvents();
            Thread.Sleep(1);
        }
    }

    private static Form CreateOccluder(Form form) => new()
    {
        StartPosition = FormStartPosition.Manual,
        Location = form.Location,
        Size = new Size(form.Width + 300, form.Height + 300),
        FormBorderStyle = FormBorderStyle.None,
        BackColor = Color.Magenta,
        TopMost = true,
        ShowInTaskbar = false
    };
}
