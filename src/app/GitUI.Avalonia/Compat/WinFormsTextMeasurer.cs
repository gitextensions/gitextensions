using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
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
    private const uint DrawTextBottom = 0x8;
    private const uint DrawTextSingleLine = 0x20;
    private const uint DrawTextNoPrefix = 0x800;

    /// <summary>
    ///  Gets the source text renderer's glyph-overhang insets from the current font metrics.
    /// </summary>
    public static Avalonia.Thickness GetTextRendererPadding(TextBlock owner)
    {
        // TextRenderer's glyph overhang margins use the current font height, not a
        // screenshot offset. Reuse the same GDI metric as native measurement on Windows.
        if (OperatingSystem.IsWindows()
            && TryMeasureWithGdi(owner.FontFamily, owner.FontStyle, owner.FontWeight,
                owner.FontSize, "0", true, true, out _, out Avalonia.Thickness padding, out _))
        {
            return padding;
        }

        double overhangPadding = MeasureSize(owner, "0").Height / 6;
        return new Avalonia.Thickness(Math.Ceiling(overhangPadding), 0, Math.Ceiling(overhangPadding * 1.5), 0);
    }

    public static double Measure(TemplatedControl owner, string value)
        => MeasureSize(owner.FontFamily, owner.FontStyle, owner.FontWeight, owner.FontSize, value).Width;

    public static double Measure(TextBlock owner, string value)
        => MeasureSize(owner, value).Width;

    public static AvaloniaSize MeasureSize(TextBlock owner, string value, bool singleLine = true)
        => MeasureSize(owner.FontFamily, owner.FontStyle, owner.FontWeight, owner.FontSize, value, singleLine);

    public static AvaloniaSize MeasureTextRenderer(TextBlock owner, string value)
        => MeasureSize(
            owner.FontFamily,
            owner.FontStyle,
            owner.FontWeight,
            owner.FontSize,
            value,
            singleLine: true,
            useTextRendererPadding: true);

    public static AvaloniaSize MeasureTextRenderer(TemplatedControl owner, string value)
        => MeasureSize(
            owner.FontFamily,
            owner.FontStyle,
            owner.FontWeight,
            owner.FontSize,
            value,
            singleLine: true,
            useTextRendererPadding: true);

    public static AvaloniaSize MeasureSize(TemplatedControl owner, string value)
        => MeasureSize(owner.FontFamily, owner.FontStyle, owner.FontWeight, owner.FontSize, value, singleLine: true);

    /// <summary>
    ///  Measures the source renderer's NoPadding-only text without changing its literal paint caption.
    /// </summary>
    public static AvaloniaSize MeasureTextRendererNoPadding(TemplatedControl owner, string value)
    {
        if (value.Length == 0)
        {
            return default;
        }

        // Ref labels measure with NoPadding alone but paint with NoPrefix. Keep this
        // separate from the literal single-line route used by other controls.
        if (OperatingSystem.IsWindows()
            && TryMeasureWithGdi(owner.FontFamily, owner.FontStyle, owner.FontWeight,
                owner.FontSize, value, false, false, out AvaloniaSize size, out _, out _, parsePrefixes: true))
        {
            return size;
        }

        string measuredValue = RemoveMeasurePrefixes(value);
        FormattedText text = new(measuredValue.Length == 0 ? " " : measuredValue,
            CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            new Typeface(owner.FontFamily, owner.FontStyle, owner.FontWeight), owner.FontSize, foreground: null);
        return new AvaloniaSize(measuredValue.Length == 0 ? 0 : text.WidthIncludingTrailingWhitespace, text.Height);
    }

    private static string RemoveMeasurePrefixes(string value)
    {
        StringBuilder text = new(value.Length);
        for (int index = 0; index < value.Length; index++)
        {
            char character = value[index];
            if (character != '&')
            {
                text.Append(character);
            }
            else if (index + 1 < value.Length && value[index + 1] == '&')
            {
                text.Append('&');
                index++;
            }
        }

        return text.ToString();
    }

    /// <summary>
    ///  Gets the common control's default image-free tab minimum, excluding its outer padding.
    /// </summary>
    public static double GetTabCaptionMinimumWidth(TextBlock owner)
    {
        // The native tab reserves six average-width characters even for an empty caption.
        if (OperatingSystem.IsWindows()
            && TryMeasureWithGdi(owner.FontFamily, owner.FontStyle, owner.FontWeight,
                owner.FontSize, "0", true, false, out _, out _, out double averageWidth)
            && averageWidth > 0)
        {
            return 6 * averageWidth;
        }

        return 6 * Math.Ceiling(MeasureSize(owner, "0").Width);
    }

    /// <summary>
    ///  Measures a literal caption using the current font and source text-renderer flags.
    /// </summary>
    public static AvaloniaSize MeasureSize(
        FontFamily fontFamily,
        FontStyle fontStyle,
        FontWeight fontWeight,
        double fontSize,
        string value,
        bool singleLine = true,
        bool useTextRendererPadding = false)
    {
        string measuredValue = singleLine ? value : value.TrimEnd('\r', '\n');
        if (measuredValue.Length == 0)
        {
            return default;
        }

        if (OperatingSystem.IsWindows()
            && TryMeasureWithGdi(
                fontFamily,
                fontStyle,
                fontWeight,
                fontSize,
                measuredValue,
                singleLine,
                useTextRendererPadding,
                out AvaloniaSize size,
                out _,
                out _))
        {
            return size;
        }

        Typeface typeface = new(fontFamily, fontStyle, fontWeight);
        FormattedText formattedText = new(
            measuredValue,
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            typeface,
            fontSize,
            foreground: null);
        double width = formattedText.WidthIncludingTrailingWhitespace;
        if (useTextRendererPadding)
        {
            double overhangPadding = formattedText.Height / 6;
            width += Math.Ceiling(overhangPadding) + Math.Ceiling(overhangPadding * 1.5);
        }

        return new AvaloniaSize(width, formattedText.Height);
    }

    private static bool TryMeasureWithGdi(
        FontFamily fontFamily,
        FontStyle fontStyle,
        FontWeight fontWeight,
        double fontSize,
        string value,
        bool singleLine,
        bool useTextRendererPadding,
        out AvaloniaSize size,
        out Avalonia.Thickness textPadding,
        out double averageWidth,
        bool parsePrefixes = false)
    {
        textPadding = default;
        averageWidth = 0;
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
            -(int)Math.Ceiling(fontSize),
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
        if (GetTextMetrics(deviceContext, out TextMetric fontMetrics))
        {
            averageWidth = fontMetrics.AverageCharacterWidth;
        }

        NativeRectangle rectangle = default;
        int measuredHeight;
        if (useTextRendererPadding && GetTextMetrics(deviceContext, out TextMetric metric))
        {
            rectangle.Right = int.MaxValue;
            rectangle.Bottom = int.MaxValue;
            float overhangPadding = metric.Height / 6f;
            textPadding = new Avalonia.Thickness(Math.Ceiling(overhangPadding), 0, Math.Ceiling(overhangPadding * 1.5f), 0);
            DrawTextParameters parameters = new()
            {
                Size = (uint)Marshal.SizeOf<DrawTextParameters>(),
                LeftMargin = (int)Math.Ceiling(overhangPadding),
                RightMargin = (int)Math.Ceiling(overhangPadding * 1.5f),
            };
            measuredHeight = DrawTextEx(
                deviceContext,
                value,
                value.Length,
                ref rectangle,
                DrawTextCalculateRectangle | DrawTextBottom,
                ref parameters);
        }
        else
        {
            if (parsePrefixes)
            {
                // TextRenderer's Size.Empty proposed bounds become unconstrained.
                rectangle.Right = int.MaxValue;
                rectangle.Bottom = int.MaxValue;
            }

            measuredHeight = DrawText(
                deviceContext,
                value,
                value.Length,
                ref rectangle,
                DrawTextCalculateRectangle | (parsePrefixes ? 0 : DrawTextNoPrefix) | (singleLine ? DrawTextSingleLine : 0));
        }

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

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int DrawTextEx(
        nint deviceContext,
        string text,
        int textLength,
        ref NativeRectangle rectangle,
        uint format,
        ref DrawTextParameters parameters);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetTextMetrics(nint deviceContext, out TextMetric metric);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DrawTextParameters
    {
        public uint Size;
        public int TabLength;
        public int LeftMargin;
        public int RightMargin;
        public uint LengthDrawn;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct TextMetric
    {
        public int Height;
        public int Ascent;
        public int Descent;
        public int InternalLeading;
        public int ExternalLeading;
        public int AverageCharacterWidth;
        public int MaximumCharacterWidth;
        public int Weight;
        public int Overhang;
        public int DigitizedAspectX;
        public int DigitizedAspectY;
        public char FirstCharacter;
        public char LastCharacter;
        public char DefaultCharacter;
        public char BreakCharacter;
        public byte Italic;
        public byte Underlined;
        public byte StruckOut;
        public byte PitchAndFamily;
        public byte CharacterSet;
    }
}
