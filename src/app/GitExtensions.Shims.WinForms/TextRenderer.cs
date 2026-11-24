namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Windows.Forms.TextRenderer</c>: routes to the
///  <see cref="ITextMeasurer"/> service installed in <see cref="ShimHost"/>.
/// </summary>
/// <remarks>
///  Consumed by: <c>GitCommands/UserRepositoryHistory/RecentRepoInfo.cs</c> (caption shortening).
/// </remarks>
public static class TextRenderer
{
    /// <summary>
    ///  Measures the size, in pixels, of the text when drawn with the font.
    /// </summary>
    public static Size MeasureText(string? text, Font? font) => ShimHost.TextMeasurer.MeasureText(text, font);
}
