using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Media;

namespace GitUI.Compat;

/// <summary>
///  Measures the native-control font used by RichEdit rather than TextRenderer's font cache.
/// </summary>
internal static class WinFormsRichEditTextMeasurer
{
    public static double GetLineHeight(TextBlock owner)
    {
        if (OperatingSystem.IsWindows() && TryMeasureNativeFont(owner, null, owner.FontSize, out int height, out _, out _))
        {
            return height;
        }

        // If native conversion is unavailable, retain the existing platform-font fallback.
        // This is not evidence that it matches Windows RichEdit glyphs or mixed RTF fonts.
        return Math.Ceiling(WinFormsTextMeasurer.MeasureSize(owner, "Mg").Height);
    }

    public static bool TryGetCharacterAdvances(TextBlock owner, string text, out int[] advances, double? fontSize = null)
    {
        advances = [];

        // Keep the framework's font fallback and complex-script shaping. The native
        // advance boundary below is for directly represented, left-to-right characters.
        return OperatingSystem.IsWindows()
            && text.All(character => character is >= ' ' and <= '~')
            && TryMeasureNativeFont(owner, text, fontSize ?? owner.FontSize, out _, out advances, out _);
    }

    public static bool TryGetTextWidth(TextBlock owner, string text, out double width)
    {
        width = 0;
        if (!OperatingSystem.IsWindows()
            || !TryMeasureNativeFont(owner, text, owner.FontSize, out _, out int[] advances, out _))
        {
            return false;
        }

        width = advances.Length == 0 ? 0 : advances[^1];
        return true;
    }

    public static double GetFontSize(TextBlock owner, bool rtfRoundTrip = false)
    {
        if (!OperatingSystem.IsWindows()
            || !TryMeasureNativeFont(owner, null, owner.FontSize, out _, out _, out int nativeEmSize))
        {
            return owner.FontSize;
        }

        // RichEdit's default CHARFORMAT reflects the control HFONT in twips. SelectedRtf
        // serializes \fs in integral half-points; reinserting an anchor reads that value
        // back, while unmodified text retains the default font's original twip height.
        const double pixelsPerPoint = 96d / 72;
        return rtfRoundTrip
            ? Math.Round(nativeEmSize / pixelsPerPoint * 2, MidpointRounding.AwayFromZero) / 2 * pixelsPerPoint
            : nativeEmSize;
    }

    private static bool TryMeasureNativeFont(TextBlock owner, string? text, double fontSize, out int height, out int[] advances, out int nativeEmSize)
    {
        height = 0;
        advances = [];
        nativeEmSize = 0;
        nint deviceContext = GetDC(0);
        if (deviceContext == 0)
        {
            return false;
        }

        nuint token = 0;
        nint graphics = 0;
        nint family = 0;
        nint font = 0;
        nint nativeFont = 0;
        nint previousFont = 0;
        try
        {
            StartupInput input = new() { Version = 1 };
            if (GdiplusStartup(out token, ref input, 0) != 0
                || GdipCreateFromHDC(deviceContext, out graphics) != 0
                || GdipCreateFontFamilyFromName(owner.FontFamily.Name, 0, out family) != 0)
            {
                return false;
            }

            // These native advances describe 96-DPI DIPs. An HDC at another system DPI
            // would turn point-font pixels into larger logical widths a second time when
            // Avalonia scales them. Use its normal scalable renderer in that context.
            if (GdipGetDpiX(graphics, out float dpiX) != 0 || GdipGetDpiY(graphics, out float dpiY) != 0
                || dpiX != 96 || dpiY != 96)
            {
                return false;
            }

            int style = (owner.FontWeight >= FontWeight.Bold ? 1 : 0)
                | (owner.FontStyle == FontStyle.Italic ? 2 : 0);

            // Control.FontHandleWrapper calls Font.ToHfont, whose GDI+ LOGFONT conversion
            // quantizes the native-control font differently from TextRenderer's cache.
            // The source Font is in points. Convert the owner's 96-DPI DIPs back to that
            // unit before the same GDI+ conversion; UnitPixel can choose different metrics.
            const int unitPoint = 3;
            if (GdipCreateFont(family, (float)(fontSize * 72 / 96), style, unitPoint, out font) != 0
                || GdipGetLogFontW(font, graphics, out NativeLogFont logFont) != 0)
            {
                return false;
            }

            nativeFont = CreateFontIndirect(ref logFont);
            if (nativeFont == 0)
            {
                return false;
            }

            previousFont = SelectObject(deviceContext, nativeFont);
            if (previousFont == 0 || previousFont == -1)
            {
                return false;
            }

            if (!GetTextMetrics(deviceContext, out TextMetric metrics))
            {
                return false;
            }

            height = metrics.Height;
            if (text is { Length: > 0 })
            {
                int[] nativeAdvances = new int[text.Length];
                if (!GetTextExtentExPoint(deviceContext, text, text.Length, int.MaxValue,
                    out int fittedCharacters, nativeAdvances, out _) || fittedCharacters != text.Length)
                {
                    return false;
                }

                advances = nativeAdvances;
            }

            nativeEmSize = Math.Abs(logFont.Height);

            return height > 0;
        }
        finally
        {
            if (previousFont != 0 && previousFont != -1)
            {
                SelectObject(deviceContext, previousFont);
            }

            if (nativeFont != 0)
            {
                DeleteObject(nativeFont);
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

            if (token != 0)
            {
                GdiplusShutdown(token);
            }

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
    private static extern int GdipGetLogFontW(nint font, nint graphics, out NativeLogFont logFont);

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

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
    private static extern nint CreateFontIndirect(ref NativeLogFont logFont);

    [DllImport("gdi32.dll")]
    private static extern nint SelectObject(nint deviceContext, nint value);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(nint value);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
    private static extern bool GetTextMetrics(nint deviceContext, out TextMetric metric);

    [DllImport("gdi32.dll", EntryPoint = "GetTextExtentExPointW", CharSet = CharSet.Unicode)]
    private static extern bool GetTextExtentExPoint(nint deviceContext, string text, int length,
        int maximumExtent, out int fittedCharacters, [Out] int[] advances, out NativeSize size);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeSize
    {
        public int Width;
        public int Height;
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

    [StructLayout(LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private struct NativeLogFont
    {
        public int Height;
        public int Width;
        public int Escapement;
        public int Orientation;
        public int Weight;
        public byte Italic;
        public byte Underline;
        public byte StrikeOut;
        public byte CharSet;
        public byte OutputPrecision;
        public byte ClipPrecision;
        public byte Quality;
        public byte PitchAndFamily;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string FaceName;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private struct TextMetric
    {
        public int Height;
        public int Ascent;
        public int Descent;
        public int InternalLeading;
        public int ExternalLeading;
        public int AverageCharWidth;
        public int MaximumCharWidth;
        public int Weight;
        public int Overhang;
        public int DigitizedAspectX;
        public int DigitizedAspectY;
        public char FirstChar;
        public char LastChar;
        public char DefaultChar;
        public char BreakChar;
        public byte Italic;
        public byte Underlined;
        public byte StruckOut;
        public byte PitchAndFamily;
        public byte CharSet;
    }
}
