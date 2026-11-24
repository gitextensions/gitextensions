namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Measures rendered text size for the <see cref="TextRenderer"/> stand-in.
///  Implemented by the Avalonia application with real text shaping.
/// </summary>
public interface ITextMeasurer
{
    /// <summary>
    ///  Measures the size, in pixels, of the text when drawn with the font.
    /// </summary>
    Size MeasureText(string? text, Font? font);
}
