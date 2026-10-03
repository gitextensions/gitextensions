using System.Globalization;
using System.Runtime.InteropServices;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Size = Avalonia.Size;

namespace GitUI.Compat;

/// <summary>
///  Preserves Graphics.MeasureString's floating-point metrics rather than TextRenderer's HFONT metrics.
/// </summary>
internal static class WinFormsGraphicsTextMeasurer
{
    private const int MaximumCachedMeasurements = 256;
    private const int NativeDpi = 96;
    private const int PointsPerInch = 72;
    private const int UnitPoint = 3;
    private const int LogicalPixelsX = 88;
    private const int LogicalPixelsY = 90;
    private static readonly Lock CacheLock = new();
    private static readonly Dictionary<MeasurementKey, Size> Measurements = [];

    public static Size MeasureSize(TemplatedControl owner, string value)
        => MeasureSize(value, owner.FontFamily, owner.FontStyle, owner.FontWeight, owner.FontSize);

    public static Size MeasureSize(
        string value,
        FontFamily fontFamily,
        FontStyle fontStyle,
        FontWeight fontWeight,
        double fontSizeDip)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(fontFamily);
        if (!double.IsFinite(fontSizeDip) || fontSizeDip <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fontSizeDip));
        }

        if (value.Length == 0)
        {
            return default;
        }

        int style = (fontWeight >= FontWeight.Bold ? 1 : 0) | (fontStyle == FontStyle.Italic ? 2 : 0);
        float points = (float)(fontSizeDip * PointsPerInch / NativeDpi);
        MeasurementKey key = new(value, fontFamily.Name, points, style);
        if (OperatingSystem.IsWindows() && IsNativeDpiContext())
        {
            lock (CacheLock)
            {
                if (Measurements.TryGetValue(key, out Size cached))
                {
                    return cached;
                }
            }

            if (TryMeasureNativeSize(key, out Size measured))
            {
                lock (CacheLock)
                {
                    // Repository paths are not a bounded vocabulary. Keep caching useful
                    // for repeated row/font metrics without retaining every visited path.
                    if (Measurements.Count >= MaximumCachedMeasurements)
                    {
                        Measurements.Clear();
                    }

                    Measurements[key] = measured;
                }

                return measured;
            }
        }

        return MeasurePortableSize(value, fontFamily, fontStyle, fontWeight, fontSizeDip);
    }

    public static Size MeasurePortableSize(
        string value,
        FontFamily fontFamily,
        FontStyle fontStyle,
        FontWeight fontWeight,
        double fontSizeDip)
    {
        // GDI+ is Windows-only. The documented substitute uses the actual resolved
        // platform typeface/shaping, not Segoe-specific metrics or an assumed em padding.
        // This does not establish Windows glyph raster or MeasureString identity elsewhere.
        FormattedText text = new(value, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            new Typeface(fontFamily, fontStyle, fontWeight), fontSizeDip, foreground: null);
        return new Size(text.Width, text.Height);
    }

    public static void ClearCache()
    {
        lock (CacheLock)
        {
            Measurements.Clear();
        }
    }

    private static bool TryMeasureNativeSize(MeasurementKey key, out Size size)
    {
        size = default;
        nint deviceContext = GetDC(0);
        if (deviceContext == 0)
        {
            return false;
        }

        nuint token = 0;
        nint graphics = 0;
        nint family = 0;
        nint font = 0;
        try
        {
            StartupInput input = new() { Version = 1 };
            if (GdiplusStartup(out token, ref input, 0) != 0
                || GdipCreateFromHDC(deviceContext, out graphics) != 0
                || GdipGetDpiX(graphics, out float dpiX) != 0
                || GdipGetDpiY(graphics, out float dpiY) != 0
                || dpiX != NativeDpi || dpiY != NativeDpi
                || GdipCreateFontFamilyFromName(key.Family, 0, out family) != 0
                || GdipCreateFont(family, key.Points, key.Style, UnitPoint, out font) != 0)
            {
                return false;
            }

            // The original overload passes an empty layout rectangle and a null format
            // to GdipMeasureString. Keep its float result; the caller owns pixel rounding.
            NativeRectangle layout = default;
            if (GdipMeasureString(graphics, key.Value, key.Value.Length, font, ref layout, 0,
                    out NativeRectangle bounds, out _, out _) != 0
                || !float.IsFinite(bounds.Width) || !float.IsFinite(bounds.Height)
                || bounds.Width < 0 || bounds.Height <= 0)
            {
                return false;
            }

            size = new Size(bounds.Width, bounds.Height);
            return true;
        }
        finally
        {
            if (font != 0)
            {
                GdipDeleteFont(font);
            }

            if (family != 0)
            {
                GdipDeleteFontFamily(family);
            }

            if (graphics != 0)
            {
                GdipDeleteGraphics(graphics);
            }

            if (token != 0)
            {
                GdiplusShutdown(token);
            }

            ReleaseDC(0, deviceContext);
        }
    }

    private static bool IsNativeDpiContext()
    {
        nint deviceContext = GetDC(0);
        if (deviceContext == 0)
        {
            return false;
        }

        try
        {
            // Check the actual DC before cache lookup as well as before measurement.
            // A later non-96 context must not reuse a previous native-96 result.
            return GetDeviceCaps(deviceContext, LogicalPixelsX) == NativeDpi
                && GetDeviceCaps(deviceContext, LogicalPixelsY) == NativeDpi;
        }
        finally
        {
            ReleaseDC(0, deviceContext);
        }
    }

    [DllImport("gdiplus.dll")]
    private static extern int GdiplusStartup(out nuint token, ref StartupInput input, nint output);

    [DllImport("gdiplus.dll")]
    private static extern void GdiplusShutdown(nuint token);

    [DllImport("gdiplus.dll")]
    private static extern int GdipCreateFromHDC(nint deviceContext, out nint graphics);

    [DllImport("gdiplus.dll")]
    private static extern int GdipGetDpiX(nint graphics, out float dpi);

    [DllImport("gdiplus.dll")]
    private static extern int GdipGetDpiY(nint graphics, out float dpi);

    [DllImport("gdiplus.dll", CharSet = CharSet.Unicode)]
    private static extern int GdipCreateFontFamilyFromName(string name, nint collection, out nint family);

    [DllImport("gdiplus.dll")]
    private static extern int GdipCreateFont(nint family, float size, int style, int unit, out nint font);

    [DllImport("gdiplus.dll", CharSet = CharSet.Unicode)]
    private static extern int GdipMeasureString(nint graphics, string text, int length, nint font,
        ref NativeRectangle layout, nint format, out NativeRectangle bounds, out int characters, out int lines);

    [DllImport("gdiplus.dll")]
    private static extern int GdipDeleteFont(nint font);

    [DllImport("gdiplus.dll")]
    private static extern int GdipDeleteFontFamily(nint family);

    [DllImport("gdiplus.dll")]
    private static extern int GdipDeleteGraphics(nint graphics);

    [DllImport("user32.dll")]
    private static extern nint GetDC(nint window);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(nint window, nint deviceContext);

    [DllImport("gdi32.dll")]
    private static extern int GetDeviceCaps(nint deviceContext, int index);

    private readonly record struct MeasurementKey(string Value, string Family, float Points, int Style);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRectangle
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct StartupInput
    {
        public uint Version;
        public nint DebugEventCallback;
        [MarshalAs(UnmanagedType.Bool)]
        public bool SuppressBackgroundThread;
        [MarshalAs(UnmanagedType.Bool)]
        public bool SuppressExternalCodecs;
    }
}
