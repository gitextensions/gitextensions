namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Drawing.FontFamily</c>: carries the family name only; resolution
///  against installed fonts is owned by the real UI framework.
/// </summary>
/// <remarks>
///  Consumed by: <c>GitExtensions.Extensibility/FontParser.cs</c>.
/// </remarks>
public sealed class FontFamily(string name)
{
    /// <summary>
    ///  Gets the font family name.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    ///  Gets a generic monospace family. The name is the CSS-style alias <c>monospace</c>,
    ///  which the Avalonia font manager resolves to the platform's monospace font.
    /// </summary>
    public static FontFamily GenericMonospace { get; } = new("monospace");
}
