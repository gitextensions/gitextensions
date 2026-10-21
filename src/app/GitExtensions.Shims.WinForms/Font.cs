namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Drawing.Font</c>: an immutable family/size/style record.
///  Unlike <c>System.Drawing.Common</c>, it is fully functional on non-Windows platforms;
///  resolution to a renderable font is owned by the real UI framework.
/// </summary>
/// <remarks>
///  Consumed by: <c>GitExtensions.Extensibility/FontParser.cs</c>,
///  <c>GitExtensions.Extensibility/Settings/SettingsSource.cs</c> (font settings),
///  <c>GitCommands/Settings/AppSettings.cs</c> (font getters).
/// </remarks>
public sealed class Font : IDisposable
{
    public Font(string familyName, float size)
        : this(familyName, size, FontStyle.Regular)
    {
    }

    public Font(string familyName, float size, FontStyle style)
        : this(new FontFamily(familyName), size, style)
    {
    }

    public Font(FontFamily family, float size)
        : this(family, size, FontStyle.Regular)
    {
    }

    public Font(FontFamily family, float size, FontStyle style)
    {
        FontFamily = family;
        Size = size;
        Style = style;
    }

    /// <summary>
    ///  Gets the font family.
    /// </summary>
    public FontFamily FontFamily { get; }

    /// <summary>
    ///  Gets the font family name.
    /// </summary>
    public string Name => FontFamily.Name;

    /// <summary>
    ///  Gets the em-size of the font in points.
    /// </summary>
    public float Size { get; }

    /// <summary>
    ///  Gets the style flags.
    /// </summary>
    public FontStyle Style { get; }

    /// <summary>
    ///  Gets a value indicating whether the font is bold.
    /// </summary>
    public bool Bold => Style.HasFlag(FontStyle.Bold);

    /// <summary>
    ///  Gets a value indicating whether the font is italic.
    /// </summary>
    public bool Italic => Style.HasFlag(FontStyle.Italic);

    public void Dispose()
    {
        // Immutable data holder; nothing to release. Present because callers dispose fonts.
    }

    public override string ToString()
        => $"[{nameof(Font)}: Name={Name}, Size={Size}, Style={Style}]";
}
