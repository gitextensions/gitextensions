using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace GitUI.Compat;

/// <summary>
///  Preserves TextRenderer's single-line CR/LF contract without changing the literal caption.
/// </summary>
internal class WinFormsSingleLineTextBlock : TextBlock
{
    protected override Type StyleKeyOverride => typeof(TextBlock);

    protected override TextLayout CreateTextLayout(string? text)
    {
        bool hasLineBreaks = _textRuns is null
            ? text.AsSpan().IndexOfAny('\r', '\n') >= 0
            : _textRuns.Any(run => run is TextEndOfLine
                || (run is TextCharacters characters && characters.Text.Span.IndexOfAny('\r', '\n') >= 0));
        if (!hasLineBreaks)
        {
            return base.CreateTextLayout(text);
        }

        GenericTextRunProperties properties = new(new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
            FontSize, TextDecorations, Foreground, fontFeatures: FontFeatures);
        GenericTextParagraphProperties paragraph = new(FlowDirection,
            IsMeasureValid ? TextAlignment : TextAlignment.Left, true, false, properties,
            TextWrapping.NoWrap, LineHeight, 0, LetterSpacing);
        IReadOnlyList<TextRun> originalRuns = _textRuns ?? [new TextCharacters(text ?? string.Empty, properties)];
        List<TextRun> runs = [];
        foreach (TextRun run in originalRuns)
        {
            switch (run)
            {
                case TextCharacters characters:
                    AddSingleLineRuns(runs, characters.Text, characters.Properties);
                    break;
                case TextEndOfLine:
                    AddSingleLineRuns(runs, new string('\n', run.Length).AsMemory(), run.Properties ?? properties);
                    break;
                default:
                    runs.Add(run);
                    break;
            }
        }

        Avalonia.Size maximumSize = GetMaxSizeFromConstraint();

        // NoWrap still splits TextCharacters at required breaks. Only pre-shape the
        // nonprinting controls, leaving ordinary text to the framework's complete-line
        // bidi processing, font fallback and complex-script shaping.
        return new TextLayout(new InlinesTextSource(runs), paragraph, TextTrimming,
            maximumSize.Width, maximumSize.Height);
    }

    private void AddSingleLineRuns(List<TextRun> runs, ReadOnlyMemory<char> text, TextRunProperties properties)
    {
        if (text.IsEmpty)
        {
            return;
        }

        if (text.Span.IndexOfAny('\r', '\n') < 0)
        {
            runs.Add(new TextCharacters(text, properties));
            return;
        }

        GlyphTypeface glyphTypeface = properties.Typeface.GlyphTypeface;
        double carriageReturnAdvance = text.Span.Contains('\r') ? GetCarriageReturnAdvance(properties, glyphTypeface) : 0;
        glyphTypeface.CharacterToGlyphMap.TryGetGlyph(' ', out ushort blankGlyph);
        GenericTextRunProperties controlProperties = new(properties.Typeface,
            properties.FontRenderingEmSize, baselineAlignment: properties.BaselineAlignment,
            cultureInfo: properties.CultureInfo, fontFeatures: properties.FontFeatures);
        while (!text.IsEmpty)
        {
            int controlIndex = text.Span.IndexOfAny('\r', '\n');
            if (controlIndex < 0)
            {
                runs.Add(new TextCharacters(text, properties));
                return;
            }

            if (controlIndex > 0)
            {
                runs.Add(new TextCharacters(text[..controlIndex], properties));
            }

            ReadOnlyMemory<char> controlText = text.Slice(controlIndex, 1);
            double advance = controlText.Span[0] == '\r' ? carriageReturnAdvance : 0;

            // A source carriage return can advance without painting ink; LF has no
            // advance even when the font would substitute a visible missing glyph.
            // Keep the raw source character and its cluster for trimming/hit testing.
            ShapedBuffer buffer = new(controlText, 1, glyphTypeface, properties.FontRenderingEmSize, (sbyte)FlowDirection);
            buffer[0] = new GlyphInfo(blankGlyph, 0, advance);
            runs.Add(new ShapedTextRun(buffer, controlProperties));
            text = text[(controlIndex + 1)..];
        }
    }

    private static double GetCarriageReturnAdvance(TextRunProperties properties, GlyphTypeface glyphTypeface)
    {
        Typeface typeface = properties.Typeface;
        if (OperatingSystem.IsWindows())
        {
            return WinFormsTextMeasurer.MeasureSize(typeface.FontFamily, typeface.Style,
                typeface.Weight, properties.FontRenderingEmSize, "\r").Width;
        }

        // The return glyph is font-dependent (including zero-width faces), not an
        // authored one-pixel spacer. Portable platforms retain their resolved font's
        // actual metric; native glyph rasterization/rounding remains separate debt.
        return glyphTypeface.CharacterToGlyphMap.TryGetGlyph('\r', out ushort glyph)
            && glyphTypeface.TryGetHorizontalGlyphAdvance(glyph, out ushort advance)
                ? advance * properties.FontRenderingEmSize / glyphTypeface.Metrics.DesignEmHeight
                : 0;
    }
}
