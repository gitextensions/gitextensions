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
        if (OperatingSystem.IsWindows() && TryMeasureLineHeight(owner, out int height))
        {
            return height;
        }

        // If native conversion is unavailable, retain the existing platform-font fallback.
        // This is not evidence that it matches Windows RichEdit glyphs or mixed RTF fonts.
        return Math.Ceiling(WinFormsTextMeasurer.MeasureSize(owner, "Mg").Height);
    }

    private static bool TryMeasureLineHeight(TextBlock owner, out int height)
    {
        height = 0;
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

            int style = (owner.FontWeight >= FontWeight.Bold ? 1 : 0)
                | (owner.FontStyle == FontStyle.Italic ? 2 : 0);

            // Control.FontHandleWrapper calls Font.ToHfont, whose GDI+ LOGFONT conversion
            // quantizes the native-control font differently from TextRenderer's cache.
            // The owner size is already in DIPs, so UnitPixel preserves the 96-DPI size.
            const int unitPixel = 2;
            if (GdipCreateFont(family, (float)owner.FontSize, style, unitPixel, out font) != 0
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
