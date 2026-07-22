namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Windows.Forms.ToolTip</c>: a functional per-control tooltip-text
///  store (no rendering) so the translation walker can read and write tooltips.
/// </summary>
/// <remarks>
///  Consumed by: <c>GitExtensions.Extensibility/Translations/Xliff/TranslationUtil.cs</c>.
/// </remarks>
public class ToolTip
{
    private readonly Dictionary<Control, string> _texts = [];

    /// <summary>
    ///  Gets or sets the tooltip window title.
    /// </summary>
    public string ToolTipTitle { get; set; } = string.Empty;

    /// <summary>
    ///  Gets the tooltip text registered for the control, or an empty string.
    /// </summary>
    public string GetToolTip(Control control)
        => _texts.TryGetValue(control, out string? text) ? text : string.Empty;

    /// <summary>
    ///  Registers the tooltip text for the control.
    /// </summary>
    public void SetToolTip(Control control, string? caption)
        => _texts[control] = caption ?? string.Empty;
}
