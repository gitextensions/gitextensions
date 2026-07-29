using System.Drawing.Imaging;
using GitExtensions.ParityCapture;

namespace WinFormsParityCapture;

internal static class ImageCapture
{
    public static CaptureImageResult Capture(Control root, IReadOnlyList<ToolStripDropDown> popups)
    {
        if (popups.Count > 0)
        {
            return CaptureScreen(root, popups);
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

        Bitmap bitmap = new(control.Width, control.Height, PixelFormat.Format32bppArgb);
        control.DrawToBitmap(bitmap, new Rectangle(Point.Empty, control.Size));
        EnsureRenderedContent(bitmap, "DrawToBitmap");
        Rectangle screenBounds = control.RectangleToScreen(control.ClientRectangle);
        return new CaptureImageResult(bitmap, CaptureMethod.DrawToBitmap, screenBounds);
    }

    private static CaptureImageResult CaptureScreen(Control root, IReadOnlyList<ToolStripDropDown> popups)
    {
        Rectangle bounds = NativeMethods.GetWindowRectangle(root.FindForm()?.Handle ?? root.Handle);
        foreach (ToolStripDropDown popup in popups)
        {
            bounds = Rectangle.Union(bounds, popup.Bounds);
        }

        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            throw new CaptureStateUnsupportedException("The visible surfaces have no screen area.");
        }

        Bitmap bitmap = new(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
        EnsureRenderedContent(bitmap, "screen capture");
        return new CaptureImageResult(bitmap, CaptureMethod.ScreenGrab, bounds);
    }

    private static CaptureImageResult CaptureWindow(Form form)
    {
        Rectangle bounds = NativeMethods.GetWindowRectangle(form.Handle);
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
            rendered = NativeMethods.PrintWindowContent(form.Handle, deviceContext);
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
        return new CaptureImageResult(bitmap, CaptureMethod.PrintWindow, bounds);
    }

    private static void EnsureRenderedContent(Bitmap bitmap, string method)
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
                    return;
                }
            }
        }

        bitmap.Dispose();
        throw new CaptureStateUnsupportedException($"{method} returned a blank image.");
    }
}

internal sealed record CaptureImageResult(Bitmap Bitmap, CaptureMethod Method, Rectangle ScreenBounds) : IDisposable
{
    public void Dispose()
    {
        Bitmap.Dispose();
    }
}
