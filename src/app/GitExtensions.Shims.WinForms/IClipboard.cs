namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Provides clipboard access for the <see cref="Clipboard"/> stand-in.
///  Implemented by the Avalonia application over its window clipboard API.
/// </summary>
public interface IClipboard
{
    /// <summary>
    ///  Places text on the clipboard.
    /// </summary>
    void SetText(string text);

    /// <summary>
    ///  Returns the clipboard text, or an empty string when the clipboard holds no text.
    /// </summary>
    string GetText();

    /// <summary>
    ///  Returns whether the clipboard currently holds text.
    /// </summary>
    bool ContainsText();
}
