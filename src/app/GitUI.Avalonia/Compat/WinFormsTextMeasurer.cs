using System.Globalization;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using AvaloniaSize = Avalonia.Size;

namespace GitUI.Compat;

/// <summary>
///  Measures text with the native WinForms GDI boundary on Windows and an Avalonia fallback elsewhere.
/// </summary>
internal static class WinFormsTextMeasurer
{
    private const uint DrawTextCalculateRectangle = 0x400;
    private const uint DrawTextSingleLine = 0x20;
    private const uint DrawTextNoPrefix = 0x800;

    public static double Measure(TemplatedControl owner, string value)
        => MeasureSize(owner.FontFamily, owner.FontStyle, owner.FontWeight, owner.FontSize, value).Width;

    public static double Measure(TextBlock owner, string value)
        => MeasureSize(owner, value).Width;

    public static AvaloniaSize MeasureSize(TextBlock owner, string value)
        => MeasureSize(owner.FontFamily, owner.FontStyle, owner.FontWeight, owner.FontSize, value);

    public static AvaloniaSize MeasureSize(TemplatedControl owner, string value)
        => MeasureSize(owner.FontFamily, owner.FontStyle, owner.FontWeight, owner.FontSize, value);

    private static AvaloniaSize MeasureSize(
        FontFamily fontFamily,
        FontStyle fontStyle,
        FontWeight fontWeight,
        double fontSize,
        string value)
    {
        if (OperatingSystem.IsWindows()
            && TryMeasureWithGdi(fontFamily, fontStyle, fontWeight, fontSize, value, out AvaloniaSize size))
        {
            return size;
        }

        Typeface typeface = new(fontFamily, fontStyle, fontWeight);
        FormattedText formattedText = new(
            value,
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            typeface,
            fontSize,
            foreground: null);
        return new AvaloniaSize(formattedText.WidthIncludingTrailingWhitespace, formattedText.Height);
    }

    private static bool TryMeasureWithGdi(
        FontFamily fontFamily,
        FontStyle fontStyle,
        FontWeight fontWeight,
        double fontSize,
        string value,
        out AvaloniaSize size)
    {
        const int defaultCharset = 1;
        const int outDefaultPrecision = 0;
        const int clipDefaultPrecision = 0;
        const int defaultQuality = 0;
        const int defaultPitchAndFamily = 0;

        nint deviceContext = GetDC(0);
        if (deviceContext == 0)
        {
            size = default;
            return false;
        }

        nint font = CreateFont(
            -(int)Math.Round(fontSize, MidpointRounding.AwayFromZero),
            0,
            0,
            0,
            (int)fontWeight,
            fontStyle == FontStyle.Italic,
            false,
            false,
            defaultCharset,
            outDefaultPrecision,
            clipDefaultPrecision,
            defaultQuality,
            defaultPitchAndFamily,
            fontFamily.Name);
        if (font == 0)
        {
            ReleaseDC(0, deviceContext);
            size = default;
            return false;
        }

        nint previousFont = SelectObject(deviceContext, font);
        NativeRectangle rectangle = default;
        int measuredHeight = DrawText(
            deviceContext,
            value,
            value.Length,
            ref rectangle,
            DrawTextCalculateRectangle | DrawTextSingleLine | DrawTextNoPrefix);
        SelectObject(deviceContext, previousFont);
        DeleteObject(font);
        ReleaseDC(0, deviceContext);
        size = new AvaloniaSize(rectangle.Right - rectangle.Left, rectangle.Bottom - rectangle.Top);
        return measuredHeight > 0;
    }

    [DllImport("user32.dll")]
    private static extern nint GetDC(nint window);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(nint window, nint deviceContext);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
    private static extern nint CreateFont(
        int height,
        int width,
        int escapement,
        int orientation,
        int weight,
        [MarshalAs(UnmanagedType.Bool)] bool italic,
        [MarshalAs(UnmanagedType.Bool)] bool underline,
        [MarshalAs(UnmanagedType.Bool)] bool strikeOut,
        int charSet,
        int outputPrecision,
        int clippingPrecision,
        int quality,
        int pitchAndFamily,
        string faceName);

    [DllImport("gdi32.dll")]
    private static extern nint SelectObject(nint deviceContext, nint value);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject(nint value);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int DrawText(
        nint deviceContext,
        string text,
        int textLength,
        ref NativeRectangle rectangle,
        uint format);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
