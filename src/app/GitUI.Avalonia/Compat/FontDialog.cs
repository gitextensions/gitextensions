using Avalonia.Media;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.Compat;

/// <summary>
///  Provides a reusable WinForms-shaped font-dialog boundary over a fresh Avalonia window.
/// </summary>
public sealed class FontDialog
{
    public WinFormsShims.Font Font { get; set; } = new(FontManager.Current.DefaultFontFamily.Name, 9);

    public bool FixedPitchOnly { get; set; }

    public string Text { get; set; } = "Fonts";

    public WinFormsShims.DialogResult ShowDialog(WinFormsShims.IWin32Window? owner)
    {
        using FontDialogWindow dialog = new(Font, FixedPitchOnly) { Text = Text };
        WinFormsShims.DialogResult result = dialog.ShowDialog(owner);
        if (result is WinFormsShims.DialogResult.OK or WinFormsShims.DialogResult.Yes)
        {
            Font = dialog.SelectedFont;
        }

        return result;
    }

    internal static IReadOnlyList<string> GetSystemFontNames(bool fixedPitchOnly)
        => FontManager.Current.SystemFonts
            .Where(fontFamily => !fixedPitchOnly || IsFixedPitch(fontFamily))
            .Select(fontFamily => fontFamily.Name)
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .Order(StringComparer.CurrentCultureIgnoreCase)
            .ToArray();

    internal static bool IsFixedPitch(FontFamily fontFamily)
    {
        if (!FontManager.Current.TryGetGlyphTypeface(new Typeface(fontFamily), out GlyphTypeface? glyphTypeface)
            || glyphTypeface is null
            || !glyphTypeface.CharacterToGlyphMap.TryGetGlyph('i', out ushort narrowGlyph)
            || !glyphTypeface.CharacterToGlyphMap.TryGetGlyph('W', out ushort wideGlyph)
            || !glyphTypeface.TryGetHorizontalGlyphAdvance(narrowGlyph, out ushort narrowAdvance)
            || !glyphTypeface.TryGetHorizontalGlyphAdvance(wideGlyph, out ushort wideAdvance))
        {
            return false;
        }

        return narrowAdvance == wideAdvance;
    }
}
