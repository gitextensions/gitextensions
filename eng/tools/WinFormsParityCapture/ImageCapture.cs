using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using GitExtensions.ParityCapture;

namespace WinFormsParityCapture;

internal static class ImageCapture
{
    public static CaptureImageResult Capture(
        Control root,
        IReadOnlyList<ToolStripDropDown> popups,
        IReadOnlyList<ComboBoxPopup> comboBoxPopups)
    {
        bool frozenToolbarOverlap = FrozenNativeToolbarOverlapsPopup(root, popups, comboBoxPopups);
        if (RequiresScreenGrab(root) || frozenToolbarOverlap)
        {
            CaptureImageResult screen = CaptureScreen(root, popups, comboBoxPopups, frozenToolbarOverlap);
            return frozenToolbarOverlap
                ? screen with
                {
                    ScreenAcquisitionNote = "Whole owned window and popup surfaces acquired unscaled from the visible screen because a requested popup overlaps a redraw-disabled native ToolStrip; the image includes that real overlap, not an unobscured owner beneath the popup."
                }
                : screen;
        }

        if (popups.Count > 0 || comboBoxPopups.Count > 0)
        {
            return CaptureWindows(root, popups, comboBoxPopups);
        }

        return CapturePrimary(root, [], []);
    }

    private static bool FrozenNativeToolbarOverlapsPopup(
        Control root,
        IReadOnlyList<ToolStripDropDown> popups,
        IReadOnlyList<ComboBoxPopup> comboBoxPopups)
    {
        IntPtr[] popupHandles = popups.Select(popup => popup.Handle)
            .Concat(comboBoxPopups.Select(popup => NativeMethods.GetComboBoxListHandle(popup.Owner.Handle))).ToArray();
        Rectangle[] popupBounds = popupHandles.Select(NativeMethods.GetWindowRectangle)
            .Concat(NativeMethods.GetAssociatedPopupShadows(popupHandles).Select(shadow => shadow.Bounds))
            .ToArray();
        return EnumerateSelfAndDescendants(root).OfType<ToolStrip>().Any(strip =>
            strip is not ToolStripDropDown && strip.IsHandleCreated && strip.Visible && NativeMethods.IsRedrawDisabled(strip.Handle)
                && popupBounds.Any(bounds => bounds.IntersectsWith(NativeMethods.GetWindowRectangle(strip.Handle))));
    }

    private static CaptureImageResult CapturePrimary(
        Control root,
        IReadOnlyList<ToolStripDropDown> popups,
        IReadOnlyList<ComboBoxPopup> comboBoxPopups)
    {
        ToolStrip[] frozenStrips = EnumerateSelfAndDescendants(root)
            .OfType<ToolStrip>()
            .Where(strip => strip is not ToolStripDropDown && strip.IsHandleCreated
                && strip.Visible && NativeMethods.IsRedrawDisabled(strip.Handle))
            .ToArray();
        if (frozenStrips.Length == 0)
        {
            return root is Form ordinaryForm ? CaptureWindow(ordinaryForm) : CaptureControl(root);
        }

        Form host = root.FindForm() ?? throw new CaptureStateUnsupportedException("A frozen native ToolStrip requires an owning window.");
        HashSet<IntPtr> capturedWindows = [host.Handle, .. popups.Select(popup => popup.Handle),
            .. comboBoxPopups.Select(popup => NativeMethods.GetComboBoxListHandle(popup.Owner.Handle))];
        IReadOnlyList<NativePopupShadow> popupShadows = NativeMethods.GetAssociatedPopupShadows(capturedWindows);
        Rectangle hostBounds = NativeMethods.GetWindowRectangle(host.Handle);
        Rectangle primaryBounds = GetPrimaryScreenBounds(root);
        List<(IntPtr Handle, Rectangle Bounds, Bitmap Pixels)> frozenImages = [];
        CaptureImageResult? primary = null;
        try
        {
            foreach (ToolStrip strip in frozenStrips)
            {
                IntPtr handle = strip.Handle;
                Rectangle stripBounds = NativeMethods.GetWindowRectangle(handle);
                EnsureFrozenSurfaceAcquirable(handle, stripBounds);
                Bitmap pixels = new(stripBounds.Width, stripBounds.Height, PixelFormat.Format32bppArgb);
                frozenImages.Add((handle, stripBounds, pixels));
                using Graphics screenGraphics = Graphics.FromImage(pixels);

                // PrintWindow's parent paint can replace the live cached pixels of
                // a WM_SETREDRAW-disabled child. Acquire the whole verified HWND
                // before that API, without resuming or repainting the source strip.
                screenGraphics.CopyFromScreen(stripBounds.Location, Point.Empty, stripBounds.Size, CopyPixelOperation.SourceCopy);
                EnsureFrozenSurfaceAcquirable(handle, stripBounds);
            }

            primary = root is Form form
                ? CaptureWindow(form, allowFrozenNativeContent: true)
                : CaptureControl(root, allowFrozenNativeContent: true);
            if (primary.Method != CaptureMethod.PrintWindow || primary.ScreenBounds != primaryBounds)
            {
                throw new CaptureStateUnsupportedException("A redraw-disabled native ToolStrip requires a PrintWindow primary image.");
            }

            List<CaptureImageAcquisition> acquisitions = [];
            using Graphics graphics = Graphics.FromImage(primary.Bitmap);
            graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            foreach ((IntPtr handle, Rectangle stripBounds, Bitmap pixels) in frozenImages)
            {
                EnsureFrozenSurfaceAcquirable(handle, stripBounds);
                Point destination = new(stripBounds.X - primaryBounds.X, stripBounds.Y - primaryBounds.Y);
                graphics.DrawImageUnscaled(pixels, destination);
                EnsureFrozenSurfaceAcquirable(handle, stripBounds);
                acquisitions.Add(new CaptureImageAcquisition
                {
                    SurfaceRole = "primary",
                    RegionPx = new CaptureRectangle
                    {
                        X = destination.X,
                        Y = destination.Y,
                        Width = stripBounds.Width,
                        Height = stripBounds.Height
                    },
                    CaptureMethod = CaptureMethod.ScreenGrab,
                    Reason = "redrawDisabledNativeSurface"
                });
            }

            EnsureRenderedContent(primary.Bitmap, "PrintWindow with native screen acquisitions");
            return primary with
            {
                Method = CaptureMethod.PrintWindowScreenGrabComposite,
                Acquisitions = acquisitions,
                NativePopupShadows = popupShadows
            };

            void EnsureFrozenSurfaceAcquirable(IntPtr handle, Rectangle stripBounds)
            {
                if (!NativeMethods.IsWindowShown(host.Handle)
                    || !NativeMethods.IsRedrawDisabled(handle)
                    || NativeMethods.GetWindowRectangle(handle) != stripBounds
                    || NativeMethods.GetWindowRectangle(host.Handle) != hostBounds
                    || GetPrimaryScreenBounds(root) != primaryBounds
                    || !NativeMethods.GetAssociatedPopupShadows(capturedWindows).SequenceEqual(popupShadows))
                {
                    throw new CaptureStateUnsupportedException("The redraw-disabled native ToolStrip or its owning window changed during acquisition.");
                }

                if (!primaryBounds.Contains(stripBounds) || !hostBounds.Contains(stripBounds)
                    || !NativeMethods.IsEntirelyOnScreen(stripBounds))
                {
                    throw new CaptureStateUnsupportedException("The redraw-disabled native ToolStrip is not wholly inside the primary image and visible screen area.");
                }

                if (capturedWindows.Where(popupHandle => popupHandle != host.Handle).Any(popupHandle =>
                    NativeMethods.GetWindowRectangle(popupHandle).IntersectsWith(stripBounds))
                    || popupShadows.Any(shadow => shadow.Bounds.IntersectsWith(stripBounds)))
                {
                    throw new CaptureStateUnsupportedException("An owned popup overlaps the redraw-disabled native ToolStrip screen-acquisition region.");
                }

                if (!NativeMethods.IsUnoccluded(host.Handle, stripBounds, capturedWindows))
                {
                    throw new CaptureStateUnsupportedException("An unrelated window occludes the redraw-disabled native ToolStrip screen-acquisition region.");
                }

                if (!NativeMethods.IsOwnedNativeSurfaceUnoccluded(handle, host.Handle, stripBounds, capturedWindows))
                {
                    throw new CaptureStateUnsupportedException("The redraw-disabled native ToolStrip's native owner hierarchy is hidden, clipped, or occluded.");
                }
            }
        }
        catch (Exception exception) when (exception is ExternalException)
        {
            primary?.Dispose();
            throw new CaptureStateUnsupportedException($"The redraw-disabled native ToolStrip screen acquisition failed: {exception.Message}");
        }
        catch
        {
            primary?.Dispose();
            throw;
        }
        finally
        {
            foreach ((IntPtr _, Rectangle _, Bitmap pixels) in frozenImages)
            {
                pixels.Dispose();
            }
        }
    }

    private static CaptureImageResult CaptureControl(Control control, bool allowFrozenNativeContent = false)
    {
        if (control.Width <= 0 || control.Height <= 0)
        {
            throw new CaptureStateUnsupportedException("The control has no drawable area.");
        }

        Rectangle screenBounds = control.RectangleToScreen(control.ClientRectangle);
        Form? host = control.FindForm();
        if (host is not null)
        {
            Rectangle hostBounds = NativeMethods.GetWindowRectangle(host.Handle);
            using Bitmap hostBitmap = new(hostBounds.Width, hostBounds.Height, PixelFormat.Format32bppArgb);
            using (Graphics hostGraphics = Graphics.FromImage(hostBitmap))
            {
                IntPtr hostDeviceContext = hostGraphics.GetHdc();
                bool hostRendered;
                try
                {
                    hostRendered = NativeMethods.PrintWindowContent(host.Handle, hostDeviceContext);
                }
                finally
                {
                    hostGraphics.ReleaseHdc(hostDeviceContext);
                }

                Rectangle crop = new(
                    screenBounds.X - hostBounds.X,
                    screenBounds.Y - hostBounds.Y,
                    screenBounds.Width,
                    screenBounds.Height);
                if (hostRendered
                    && crop.X >= 0
                    && crop.Y >= 0
                    && crop.Right <= hostBitmap.Width
                    && crop.Bottom <= hostBitmap.Height)
                {
                    Bitmap hostedControl = hostBitmap.Clone(crop, PixelFormat.Format32bppArgb);

                    // A frozen-only crop can be blank until CapturePrimary acquires
                    // the verified native HWND pixels; it must not use DrawToBitmap.
                    if (HasRenderedContent(hostedControl) || allowFrozenNativeContent)
                    {
                        return new CaptureImageResult(hostedControl, CaptureMethod.PrintWindow, screenBounds, screenBounds);
                    }

                    hostedControl.Dispose();
                }
            }
        }

        Bitmap drawToBitmap = new(screenBounds.Width, screenBounds.Height, PixelFormat.Format32bppArgb);
        try
        {
            control.DrawToBitmap(drawToBitmap, control.ClientRectangle);
        }
        catch (Exception exception) when (exception is ArgumentException or ExternalException or InvalidOperationException)
        {
            drawToBitmap.Dispose();
            throw new CaptureStateUnsupportedException($"DrawToBitmap does not support this control: {exception.Message}");
        }

        if (HasRenderedContent(drawToBitmap))
        {
            return new CaptureImageResult(drawToBitmap, CaptureMethod.DrawToBitmap, screenBounds, screenBounds);
        }

        drawToBitmap.Dispose();
        throw new CaptureStateUnsupportedException(
            "The owning window and DrawToBitmap both returned blank client content.");
    }

    private static CaptureImageResult CaptureWindows(
        Control root,
        IReadOnlyList<ToolStripDropDown> popups,
        IReadOnlyList<ComboBoxPopup> comboBoxPopups)
    {
        using CaptureImageResult primary = CapturePrimary(root, popups, comboBoxPopups);
        if (primary.Method is not (CaptureMethod.PrintWindow or CaptureMethod.PrintWindowScreenGrabComposite))
        {
            throw new CaptureStateUnsupportedException("The primary surface does not support PrintWindow popup composition.");
        }

        List<CaptureImageResult> popupImages = [];
        try
        {
            foreach (ToolStripDropDown popup in popups)
            {
                popupImages.Add(CaptureWindow(popup.Handle));
            }

            foreach (ComboBoxPopup popup in comboBoxPopups)
            {
                popupImages.Add(CaptureWindow(NativeMethods.GetComboBoxListHandle(popup.Owner.Handle)));
            }

            Rectangle bounds = primary.ScreenBounds;
            foreach (CaptureImageResult popup in popupImages)
            {
                bounds = Rectangle.Union(bounds, popup.ScreenBounds);
            }

            Bitmap bitmap = new(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            graphics.DrawImageUnscaled(primary.Bitmap, primary.ScreenBounds.X - bounds.X, primary.ScreenBounds.Y - bounds.Y);
            foreach (CaptureImageResult popup in popupImages)
            {
                graphics.DrawImageUnscaled(popup.Bitmap, popup.ScreenBounds.X - bounds.X, popup.ScreenBounds.Y - bounds.Y);
            }

            return new CaptureImageResult(bitmap, primary.Method, bounds, primary.PrimaryScreenBounds)
            {
                Acquisitions = primary.Acquisitions,
                NativePopupShadows = primary.NativePopupShadows
            };
        }
        finally
        {
            foreach (CaptureImageResult popup in popupImages)
            {
                popup.Dispose();
            }
        }
    }

    private static CaptureImageResult CaptureScreen(
        Control root,
        IReadOnlyList<ToolStripDropDown> popups,
        IReadOnlyList<ComboBoxPopup> comboBoxPopups,
        bool requireFrozenToolbarOverlap = false)
    {
        Form host = root.FindForm() ?? throw new CaptureStateUnsupportedException("A screen capture requires an owning window.");
        Rectangle hostBounds = NativeMethods.GetWindowRectangle(host.Handle);
        Rectangle rootWindowBounds = NativeMethods.GetWindowRectangle(root.Handle);
        Rectangle primaryBounds = GetPrimaryScreenBounds(root);
        List<(IntPtr Handle, Rectangle Bounds)> surfaces = [(root.Handle, primaryBounds)];
        surfaces.AddRange(popups.Select(popup => (popup.Handle, NativeMethods.GetWindowRectangle(popup.Handle))));
        surfaces.AddRange(comboBoxPopups.Select(popup =>
        {
            IntPtr handle = NativeMethods.GetComboBoxListHandle(popup.Owner.Handle);
            return (handle, NativeMethods.GetWindowRectangle(handle));
        }));
        Rectangle bounds = surfaces.Select(surface => surface.Bounds).Aggregate(Rectangle.Union);

        int maximumExpectedWidth = primaryBounds.Width
            + surfaces.Skip(1).Sum(surface => surface.Bounds.Width);
        int maximumExpectedHeight = primaryBounds.Height
            + surfaces.Skip(1).Sum(surface => surface.Bounds.Height);
        if (bounds.Width > maximumExpectedWidth || bounds.Height > maximumExpectedHeight)
        {
            throw new CaptureStateUnsupportedException(
                "The popup screen bounds could not be reconciled with the owning window; refusing a partial desktop capture.");
        }

        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            throw new CaptureStateUnsupportedException("The visible surfaces have no screen area.");
        }

        HashSet<IntPtr> capturedWindows = [host.Handle, .. surfaces.Select(surface => surface.Handle)];
        IReadOnlyList<NativePopupShadow> popupShadows = NativeMethods.GetAssociatedPopupShadows(capturedWindows);
        EnsureUnoccluded();
        Bitmap bitmap = new(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
        using Graphics graphics = Graphics.FromImage(bitmap);
        try
        {
            // Only copy owned surface rectangles, never unrelated desktop pixels in the union's gaps.
            foreach ((IntPtr _, Rectangle surfaceBounds) in surfaces)
            {
                graphics.CopyFromScreen(surfaceBounds.Location,
                    new Point(surfaceBounds.X - bounds.X, surfaceBounds.Y - bounds.Y), surfaceBounds.Size, CopyPixelOperation.SourceCopy);
            }

            EnsureUnoccluded();
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }

        EnsureRenderedContent(bitmap, "screen capture");
        return new CaptureImageResult(bitmap, CaptureMethod.ScreenGrab, bounds, primaryBounds)
        {
            NativePopupShadows = popupShadows
        };

        void EnsureUnoccluded()
        {
            if (!root.Visible || !NativeMethods.IsWindowShown(host.Handle)
                || NativeMethods.GetWindowRectangle(host.Handle) != hostBounds
                || NativeMethods.GetWindowRectangle(root.Handle) != rootWindowBounds
                || GetPrimaryScreenBounds(root) != primaryBounds
                || !NativeMethods.GetAssociatedPopupShadows(capturedWindows).SequenceEqual(popupShadows)
                || surfaces.Any(surface =>
                    (surface.Handle != root.Handle && NativeMethods.GetWindowRectangle(surface.Handle) != surface.Bounds)
                    || (!NativeMethods.IsWindowShown(surface.Handle) && !(surface.Handle == root.Handle
                        && root is ToolStrip && NativeMethods.IsRedrawDisabled(surface.Handle)))))
            {
                throw new CaptureStateUnsupportedException("A requested screen-capture surface is hidden, minimized, or changed during acquisition.");
            }

            if (requireFrozenToolbarOverlap && !FrozenNativeToolbarOverlapsPopup(root, popups, comboBoxPopups))
            {
                throw new CaptureStateUnsupportedException("The requested popup overlap with a redraw-disabled native ToolStrip changed during acquisition.");
            }

            if (surfaces.Any(surface => !NativeMethods.IsEntirelyOnScreen(surface.Bounds)))
            {
                throw new CaptureStateUnsupportedException("A requested screen-capture surface is not wholly inside the visible screen area.");
            }

            if (!NativeMethods.IsUnoccluded(host.Handle, primaryBounds, capturedWindows)
                || surfaces.Any(surface => !NativeMethods.IsUnoccluded(surface.Handle, surface.Bounds, capturedWindows)))
            {
                NativeWindowOccluder[] blockers = NativeMethods.GetOccludingNativeWindows(host.Handle, primaryBounds, capturedWindows)
                    .Concat(surfaces.SelectMany(surface =>
                        NativeMethods.GetOccludingNativeWindows(surface.Handle, surface.Bounds, capturedWindows)))
                    .Distinct().ToArray();
                throw new CaptureStateUnsupportedException("An unrelated window occludes a requested screen-capture surface. "
                    + string.Join("; ", blockers.Select(blocker => blocker.ToString())));
            }

            if (root != host && !NativeMethods.IsOwnedNativeSurfaceUnoccluded(root.Handle, host.Handle, primaryBounds, capturedWindows))
            {
                throw new CaptureStateUnsupportedException("The requested primary screen-capture surface's native owner hierarchy is hidden, clipped, or occluded.");
            }
        }
    }

    private static CaptureImageResult CaptureWindow(Form form, bool allowFrozenNativeContent = false)
        => CaptureWindow(form.Handle, allowFrozenNativeContent);

    private static CaptureImageResult CaptureWindow(IntPtr handle, bool allowFrozenNativeContent = false)
    {
        Rectangle bounds = NativeMethods.GetWindowRectangle(handle);
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            throw new CaptureStateUnsupportedException("The window has no drawable area.");
        }

        Bitmap bitmap = new(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
        using Graphics graphics = Graphics.FromImage(bitmap);
        IntPtr deviceContext = graphics.GetHdc();
        bool rendered;
        try
        {
            rendered = NativeMethods.PrintWindowContent(handle, deviceContext);
        }
        finally
        {
            graphics.ReleaseHdc(deviceContext);
        }

        if (!rendered)
        {
            bitmap.Dispose();
            throw new CaptureStateUnsupportedException("PrintWindow(PW_RENDERFULLCONTENT) did not render the window.");
        }

        if (!allowFrozenNativeContent)
        {
            EnsureRenderedContent(bitmap, "PrintWindow(PW_RENDERFULLCONTENT)");
        }

        return new CaptureImageResult(bitmap, CaptureMethod.PrintWindow, bounds, bounds);
    }

    internal static Rectangle GetPrimaryScreenBounds(Control root) =>
        root is Form form
            ? NativeMethods.GetWindowRectangle(form.Handle)
            : root.RectangleToScreen(root.ClientRectangle);

    // Native WebBrowser content is rendered by its own child window and PrintWindow can return
    // the containing form with that region silently blank. Capture the real visible desktop pixels.
    internal static bool RequiresScreenGrab(Control root)
        => EnumerateSelfAndDescendants(root).Any(control => control is WebBrowser && control.Visible);

    private static IEnumerable<Control> EnumerateSelfAndDescendants(Control control)
    {
        yield return control;
        foreach (Control child in control.Controls)
        {
            foreach (Control descendant in EnumerateSelfAndDescendants(child))
            {
                yield return descendant;
            }
        }
    }

    private static void EnsureRenderedContent(Bitmap bitmap, string method)
    {
        if (HasRenderedContent(bitmap))
        {
            return;
        }

        bitmap.Dispose();
        throw new CaptureStateUnsupportedException($"{method} returned a blank image.");
    }

    private static bool HasRenderedContent(Bitmap bitmap)
    {
        int stepX = Math.Max(1, bitmap.Width / 64);
        int stepY = Math.Max(1, bitmap.Height / 64);
        int firstPixel = bitmap.GetPixel(0, 0).ToArgb();
        for (int y = 0; y < bitmap.Height; y += stepY)
        {
            for (int x = 0; x < bitmap.Width; x += stepX)
            {
                if (bitmap.GetPixel(x, y).ToArgb() != firstPixel)
                {
                    return true;
                }
            }
        }

        return false;
    }
}

internal sealed record CaptureImageResult(
    Bitmap Bitmap,
    CaptureMethod Method,
    Rectangle ScreenBounds,
    Rectangle PrimaryScreenBounds) : IDisposable
{
    public IReadOnlyList<CaptureImageAcquisition>? Acquisitions { get; init; }

    public string? ScreenAcquisitionNote { get; init; }

    public IReadOnlyList<NativePopupShadow>? NativePopupShadows { get; init; }

    public string? AcquisitionNote
    {
        get
        {
            string? note = ScreenAcquisitionNote ?? (Acquisitions is { Count: > 0 }
                ? "PrintWindow supplemented with unscaled screen pixels from redraw-disabled owned native ToolStrip windows; acquired regions are recorded in the tree."
                : null);
            return NativePopupShadows is { Count: > 0 }
                ? (note is null ? string.Empty : note + " ")
                    + $"Validated {NativePopupShadows.Count} adjacent native SysShadow decoration(s) belonging to requested popup HWNDs; only requested window footprints were acquired."
                : note;
        }
    }

    public void Dispose()
    {
        Bitmap.Dispose();
    }
}
