using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms.VisualStyles;

namespace WinFormsParityCapture;

// parity-scaffolding: Reads actual image-based Explorer selection paint, not inherited
// FillColor/BorderColor properties that describe a different part of the native theme.
internal sealed record NativeTreePalette(Color Background, Color Foreground, Color Border)
{
    internal static NativeTreePalette Read(TreeView tree, int state)
    {
        string className = Application.IsDarkModeEnabled ? "DarkMode_Explorer::TREEVIEW" : "Explorer::TREEVIEW";
        return Read(className, state, tree.BackColor);
    }

    internal static NativeTreePalette Read(string className, int state, Color backdrop)
    {
        const int treeItemPart = 1;
        VisualStyleElement element = VisualStyleElement.CreateElement(className, treeItemPart, state);
        if (!VisualStyleRenderer.IsElementDefined(element))
        {
            throw new CaptureStateUnsupportedException($"Native tree selection theme '{className}' state {state} is unavailable.");
        }

        VisualStyleRenderer renderer = new(element);
        Color foreground = renderer.GetColor(ColorProperty.TextColor);
        if (renderer.LastHResult != 0)
        {
            throw new CaptureStateUnsupportedException("Native tree selection text color did not resolve.");
        }

        using Bitmap bitmap = new(100, 18, PixelFormat.Format24bppRgb);
        using Graphics graphics = Graphics.FromImage(bitmap);
        nint deviceContext = graphics.GetHdc();
        nint brush = CreateSolidBrush(unchecked((uint)(backdrop.R | (backdrop.G << 8) | (backdrop.B << 16))));
        try
        {
            // GetHdc's temporary surface does not retain Graphics.Clear. Native translucent
            // theme images must be composited over the control's real resolved background.
            NativeRectangle bounds = new() { Right = bitmap.Width, Bottom = bitmap.Height };
            if (FillRect(deviceContext, ref bounds, brush) == 0
                || DrawThemeBackground(renderer.Handle, deviceContext, treeItemPart, state, ref bounds, 0) != 0)
            {
                throw new CaptureStateUnsupportedException("Native tree selection theme image did not render.");
            }
        }
        finally
        {
            DeleteObject(brush);
            graphics.ReleaseHdc(deviceContext);
        }

        return new NativeTreePalette(
            bitmap.GetPixel(bitmap.Width / 2, bitmap.Height / 2),
            foreground,
            bitmap.GetPixel(bitmap.Width / 2, 0));
    }

    [DllImport("gdi32.dll")]
    private static extern nint CreateSolidBrush(uint color);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject(nint handle);

    [DllImport("user32.dll")]
    private static extern int FillRect(nint deviceContext, ref NativeRectangle bounds, nint brush);

    [DllImport("uxtheme.dll")]
    private static extern int DrawThemeBackground(nint theme, nint deviceContext, int part, int state, ref NativeRectangle bounds, nint clippingBounds);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
