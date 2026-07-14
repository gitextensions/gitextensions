namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Drawing.Graphics</c>: measurement-only; there is no drawing surface.
///  Measurement routes to the <see cref="ITextMeasurer"/> service installed in <see cref="ShimHost"/>.
/// </summary>
/// <remarks>
///  Consumed by: <c>GitExtensions.Extensibility/Extensions/UIExtensions.cs</c> (<c>IsFixedWidth</c>),
///  <c>ResourceManager/CommitDataRenders/*</c> (<c>GetFont(Graphics)</c> signatures).
/// </remarks>
public class Graphics
{
    /// <summary>
    ///  Measures the size, in pixels, of the text when drawn with the font.
    /// </summary>
    public SizeF MeasureString(string text, Font font) => ShimHost.TextMeasurer.MeasureText(text, font);
}
