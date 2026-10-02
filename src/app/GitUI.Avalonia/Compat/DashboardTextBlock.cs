using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Point = Avalonia.Point;

namespace GitUI.Compat;

/// <summary>
///  Paints Dashboard labels and link captions with the source TextRenderer font quantization and baseline.
/// </summary>
public sealed class DashboardTextBlock : WinFormsControls.Label
{
    private TextLayout? _paintLayout;
    private double _paintBaseline;

    internal double PaintEmSize => Math.Ceiling(FontSize);

    internal double PaintBaseline
    {
        get
        {
            EnsurePaintLayout();
            return _paintBaseline;
        }
    }

    protected override void RenderTextLayout(DrawingContext context, Point origin)
    {
        EnsurePaintLayout();
        if (_paintLayout is { TextLines.Count: > 0 } layout)
        {
            layout.Draw(context, origin + new Vector(0, _paintBaseline - layout.TextLines[0].Baseline));
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == FontFamilyProperty
            || change.Property == FontSizeProperty
            || change.Property == FontStyleProperty
            || change.Property == FontWeightProperty
            || change.Property == ForegroundProperty
            || change.Property == TextProperty
            || change.Property == TextAlignmentProperty
            || change.Property == FlowDirectionProperty)
        {
            _paintLayout?.Dispose();
            _paintLayout = null;
            InvalidateVisual();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _paintLayout?.Dispose();
        _paintLayout = null;
        base.OnDetachedFromVisualTree(e);
    }

    private void EnsurePaintLayout()
    {
        if (_paintLayout is not null)
        {
            return;
        }

        // TextRenderer's font cache rounds the em height up, unlike an ordinary Skia layout
        // at the requested fractional DIP size. Preserve that public source font size.
        _paintLayout = new TextLayout(Text ?? string.Empty,
            new Typeface(FontFamily, FontStyle, FontWeight), PaintEmSize, Foreground,
            textAlignment: TextAlignment, flowDirection: FlowDirection);
        _paintBaseline = _paintLayout.TextLines.Count == 0 ? 0 : _paintLayout.TextLines[0].Baseline;
        if (OperatingSystem.IsWindows() && TryGetNativeAscent(out int ascent))
        {
            _paintBaseline = ascent;
        }

        // The portable fallback uses its actual typeface baseline, not a captured offset.
        // Native glyph rasterization remains different from Skia on every platform.
    }

    private bool TryGetNativeAscent(out int ascent)
    {
        ascent = 0;
        nint deviceContext = GetDC(0);
        if (deviceContext == 0)
        {
            return false;
        }

        nint font = CreateFont(-(int)PaintEmSize, 0, 0, 0, (int)FontWeight,
            FontStyle == FontStyle.Italic, false, false, 1, 0, 0, 0, 0, FontFamily.Name);
        nint previous = font == 0 ? 0 : SelectObject(deviceContext, font);
        try
        {
            if (previous != 0 && previous != -1 && GetTextMetrics(deviceContext, out TextMetric metrics))
            {
                ascent = metrics.Ascent;
                return ascent > 0;
            }

            return false;
        }
        finally
        {
            if (previous != 0 && previous != -1)
            {
                SelectObject(deviceContext, previous);
            }

            if (font != 0)
            {
                DeleteObject(font);
            }

            ReleaseDC(0, deviceContext);
        }
    }

    [DllImport("user32.dll")]
    private static extern nint GetDC(nint window);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(nint window, nint deviceContext);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
    private static extern nint CreateFont(int height, int width, int escapement, int orientation, int weight,
        bool italic, bool underline, bool strikeOut, int charSet, int outPrecision, int clipPrecision, int quality, int pitchAndFamily, string name);

    [DllImport("gdi32.dll")]
    private static extern nint SelectObject(nint deviceContext, nint value);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(nint value);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
    private static extern bool GetTextMetrics(nint deviceContext, out TextMetric metrics);

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
