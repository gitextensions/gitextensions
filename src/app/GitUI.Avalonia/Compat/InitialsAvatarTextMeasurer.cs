using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media;
using Size = Avalonia.Size;

namespace GitUI.Compat;

/// <summary>
///  Measures initials in the original provider's bitmap-backed GDI+ text context.
/// </summary>
internal static class InitialsAvatarTextMeasurer
{
    private const int NativeDpi = 96;
    private const int PointsPerInch = 72;
    private const int UnitPoint = 3;
    private const int PixelFormat32BppArgb = 0x26200A;
    private const int SmoothingModeHighSpeed = 1;
    private const int TextRenderingHintClearTypeGridFit = 5;

    /// <summary>
    ///  Gets the source bitmap measurement, or the actual resolved portable typeface substitute.
    /// </summary>
    public static Size Measure(string value, FontFamily family, float points, int imageSize)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(family);
        if (OperatingSystem.IsWindows() && TryMeasureNative(value, family.Name, points, imageSize, out Size measured))
        {
            return measured;
        }

        return MeasurePortable(value, family, points);
    }

    /// <summary>
    ///  Gets typographic metrics from the actual resolved platform typeface without native GDI+.
    /// </summary>
    internal static Size MeasurePortable(string value, FontFamily family, float points)
        => WinFormsGraphicsTextMeasurer.MeasurePortableSize(value, family, FontStyle.Normal, FontWeight.Normal,
            points * (double)NativeDpi / PointsPerInch);

    /// <summary>
    ///  Gets the original ClearTypeGridFit bitmap measurement when that native API is available.
    /// </summary>
    internal static bool TryMeasureNative(string value, string familyName, float points, int imageSize, out Size measured)
        => TryMeasureNativeCore(value, familyName, points, imageSize, false, out measured, out _);

    /// <summary>
    ///  Gets the default-format character origin and font ascent relative to typographic drawing.
    /// </summary>
    internal static bool TryGetDrawingMetrics(string value, string familyName, float points, int imageSize, out DrawingMetrics metrics)
        => TryMeasureNativeCore(value, familyName, points, imageSize, true, out _, out metrics);

    private static bool TryMeasureNativeCore(string value, string familyName, float points, int imageSize,
        bool measureDrawingMetrics, out Size measured, out DrawingMetrics drawingMetrics)
    {
        measured = default;
        drawingMetrics = default;
        if (!OperatingSystem.IsWindows())
        {
            return false;
        }

        nuint token = 0;
        nint image = 0;
        nint graphics = 0;
        nint family = 0;
        nint font = 0;
        nint defaultFormat = 0;
        nint typographicFormat = 0;
        nint defaultRegion = 0;
        nint typographicRegion = 0;
        try
        {
            StartupInput startup = new() { Version = 1 };
            if (GdiplusStartup(out token, ref startup, 0) != 0
                || GdipCreateBitmapFromScan0(imageSize, imageSize, 0, PixelFormat32BppArgb, 0, out image) != 0
                || GdipGetImageGraphicsContext(image, out graphics) != 0
                || GdipGetDpiX(graphics, out float dpiX) != 0 || dpiX != NativeDpi
                || GdipGetDpiY(graphics, out float dpiY) != 0 || dpiY != NativeDpi
                || GdipSetSmoothingMode(graphics, SmoothingModeHighSpeed) != 0
                || GdipSetTextRenderingHint(graphics, TextRenderingHintClearTypeGridFit) != 0
                || GdipCreateFontFamilyFromName(familyName, 0, out family) != 0
                || GdipCreateFont(family, points, 0, UnitPoint, out font) != 0)
            {
                return false;
            }

            // MeasureString uses a default format and empty layout rectangle. Its
            // ClearTypeGridFit metrics belong to this bitmap, not a screen HFONT/DC.
            NativeRectangle layout = default;
            if (GdipMeasureString(graphics, value, value.Length, font, ref layout, 0,
                    out NativeRectangle bounds, out _, out _) != 0
                || !float.IsFinite(bounds.Width) || !float.IsFinite(bounds.Height)
                || bounds.Width <= 0 || bounds.Height <= 0)
            {
                return false;
            }

            measured = new Size(bounds.Width, bounds.Height);
            if (measureDrawingMetrics)
            {
                if (GdipStringFormatGetGenericDefault(out defaultFormat) != 0
                    || GdipStringFormatGetGenericTypographic(out typographicFormat) != 0
                    || GdipCreateRegion(out defaultRegion) != 0 || GdipCreateRegion(out typographicRegion) != 0
                    || GdipGetEmHeight(family, 0, out ushort designUnitsPerEm) != 0 || designUnitsPerEm == 0
                    || GdipGetCellAscent(family, 0, out ushort cellAscent) != 0)
                {
                    return false;
                }

                NativeCharacterRange range = new() { Length = value.Length };
                layout.Width = bounds.Width * 2;
                layout.Height = bounds.Height * 2;
                if (GdipSetStringFormatMeasurableCharacterRanges(defaultFormat, 1, ref range) != 0
                    || GdipSetStringFormatMeasurableCharacterRanges(typographicFormat, 1, ref range) != 0
                    || GdipMeasureCharacterRanges(graphics, value, value.Length, font, ref layout, defaultFormat, 1, ref defaultRegion) != 0
                    || GdipMeasureCharacterRanges(graphics, value, value.Length, font, ref layout, typographicFormat, 1, ref typographicRegion) != 0
                    || GdipGetRegionBounds(defaultRegion, graphics, out NativeRectangle defaultBounds) != 0
                    || GdipGetRegionBounds(typographicRegion, graphics, out NativeRectangle typographicBounds) != 0)
                {
                    return false;
                }

                float fontSizeInPixels = points * NativeDpi / PointsPerInch;
                float ascent = fontSizeInPixels * cellAscent / designUnitsPerEm;
                if (!float.IsFinite(defaultBounds.X) || !float.IsFinite(defaultBounds.Y)
                    || !float.IsFinite(typographicBounds.X) || !float.IsFinite(typographicBounds.Y)
                    || !float.IsFinite(ascent) || ascent <= 0)
                {
                    return false;
                }

                drawingMetrics = new DrawingMetrics(
                    new Vector(defaultBounds.X - typographicBounds.X, defaultBounds.Y - typographicBounds.Y), ascent);
            }

            return true;
        }
        finally
        {
            if (defaultRegion != 0)
            {
                GdipDeleteRegion(defaultRegion);
            }

            if (typographicRegion != 0)
            {
                GdipDeleteRegion(typographicRegion);
            }

            if (defaultFormat != 0)
            {
                GdipDeleteStringFormat(defaultFormat);
            }

            if (typographicFormat != 0)
            {
                GdipDeleteStringFormat(typographicFormat);
            }

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

            if (image != 0)
            {
                GdipDisposeImage(image);
            }

            if (token != 0)
            {
                GdiplusShutdown(token);
            }
        }
    }

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern int GdiplusStartup(out nuint token, ref StartupInput input, nint output);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern void GdiplusShutdown(nuint token);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern int GdipCreateBitmapFromScan0(int width, int height, int stride, int format, nint scan0, out nint image);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern int GdipGetImageGraphicsContext(nint image, out nint graphics);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern int GdipGetDpiX(nint graphics, out float dpi);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern int GdipGetDpiY(nint graphics, out float dpi);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern int GdipSetSmoothingMode(nint graphics, int smoothingMode);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern int GdipSetTextRenderingHint(nint graphics, int textRenderingHint);

    [DllImport("gdiplus.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int GdipCreateFontFamilyFromName(string family, nint collection, out nint fontFamily);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern int GdipCreateFont(nint family, float size, int style, int unit, out nint font);

    [DllImport("gdiplus.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int GdipMeasureString(nint graphics, string text, int length, nint font,
        ref NativeRectangle layout, nint format, out NativeRectangle bounds, out int characters, out int lines);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern int GdipDeleteFont(nint font);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern int GdipDeleteFontFamily(nint family);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern int GdipDeleteGraphics(nint graphics);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern int GdipDisposeImage(nint image);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern int GdipStringFormatGetGenericDefault(out nint format);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern int GdipStringFormatGetGenericTypographic(out nint format);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern int GdipSetStringFormatMeasurableCharacterRanges(nint format, int count, ref NativeCharacterRange ranges);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern int GdipCreateRegion(out nint region);

    [DllImport("gdiplus.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int GdipMeasureCharacterRanges(nint graphics, string text, int length, nint font,
        ref NativeRectangle layout, nint format, int count, ref nint regions);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern int GdipGetRegionBounds(nint region, nint graphics, out NativeRectangle bounds);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern int GdipDeleteRegion(nint region);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern int GdipDeleteStringFormat(nint format);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern int GdipGetEmHeight(nint family, int style, out ushort height);

    [DllImport("gdiplus.dll", ExactSpelling = true)]
    private static extern int GdipGetCellAscent(nint family, int style, out ushort ascent);

    /// <summary>
    ///  Contains source character-layout leading and baseline metrics, not glyph ink bounds.
    /// </summary>
    /// <param name="Origin">The source default-format origin relative to typographic layout.</param>
    /// <param name="Ascent">The drawing font's baseline in bitmap pixels.</param>
    internal readonly record struct DrawingMetrics(Vector Origin, double Ascent);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeCharacterRange
    {
        public int First;
        public int Length;
    }

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
