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
        if (RequiresScreenGrab(root))
        {
            return CaptureScreen(root, popups, comboBoxPopups);
        }

        if (popups.Count > 0 || comboBoxPopups.Count > 0)
        {
            return CaptureWindows(root, popups, comboBoxPopups);
        }

        if (root is Form form)
        {
            return CaptureWindow(form);
        }

        return CaptureControl(root);
    }

    private static CaptureImageResult CaptureControl(Control control)
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
                    if (HasRenderedContent(hostedControl))
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
        using CaptureImageResult primary = root is Form form ? CaptureWindow(form) : CaptureControl(root);
        if (primary.Method != CaptureMethod.PrintWindow)
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

            return new CaptureImageResult(bitmap, CaptureMethod.PrintWindow, bounds, primary.PrimaryScreenBounds);
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
        IReadOnlyList<ComboBoxPopup> comboBoxPopups)
    {
        Rectangle primaryBounds = GetPrimaryScreenBounds(root);
        Rectangle bounds = primaryBounds;
        foreach (ToolStripDropDown popup in popups)
        {
            bounds = Rectangle.Union(bounds, popup.Bounds);
        }

        foreach (ComboBoxPopup popup in comboBoxPopups)
        {
            bounds = Rectangle.Union(bounds, popup.Bounds);
        }

        int maximumExpectedWidth = primaryBounds.Width
            + popups.Sum(popup => popup.Width)
            + comboBoxPopups.Sum(popup => popup.Bounds.Width);
        int maximumExpectedHeight = primaryBounds.Height
            + popups.Sum(popup => popup.Height)
            + comboBoxPopups.Sum(popup => popup.Bounds.Height);
        if (bounds.Width > maximumExpectedWidth || bounds.Height > maximumExpectedHeight)
        {
            throw new CaptureStateUnsupportedException(
                "The popup screen bounds could not be reconciled with the owning window; refusing a partial desktop capture.");
        }

        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            throw new CaptureStateUnsupportedException("The visible surfaces have no screen area.");
        }

        Form host = root.FindForm() ?? throw new CaptureStateUnsupportedException("A screen capture requires an owning window.");
        HashSet<IntPtr> capturedWindows = [host.Handle, .. popups.Select(popup => popup.Handle),
            .. comboBoxPopups.Select(popup => NativeMethods.GetComboBoxListHandle(popup.Owner.Handle))];
        EnsureUnoccluded();
        Bitmap bitmap = new(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
        using Graphics graphics = Graphics.FromImage(bitmap);
        try
        {
            // Only copy owned surface rectangles, never unrelated desktop pixels in the union's gaps.
            foreach (Rectangle surfaceBounds in new[] { primaryBounds }
                         .Concat(popups.Select(popup => popup.Bounds))
                         .Concat(comboBoxPopups.Select(popup => popup.Bounds)))
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
        return new CaptureImageResult(bitmap, CaptureMethod.ScreenGrab, bounds, primaryBounds);

        void EnsureUnoccluded()
        {
            if (!NativeMethods.IsUnoccluded(host.Handle, primaryBounds, capturedWindows)
                || capturedWindows.Where(handle => handle != host.Handle).Any(handle =>
                    !NativeMethods.IsUnoccluded(handle, NativeMethods.GetWindowRectangle(handle), capturedWindows)))
            {
                throw new CaptureStateUnsupportedException("An unrelated window occludes a requested screen-capture surface.");
            }
        }
    }

    private static CaptureImageResult CaptureWindow(Form form)
        => CaptureWindow(form.Handle);

    private static CaptureImageResult CaptureWindow(IntPtr handle)
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

        EnsureRenderedContent(bitmap, "PrintWindow(PW_RENDERFULLCONTENT)");
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
    public void Dispose()
    {
        Bitmap.Dispose();
    }
}
