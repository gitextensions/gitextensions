namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Windows.Forms.TextBox</c>: a headless, functional model of a text
///  input; the Avalonia settings renderer materializes a real control from it.
/// </summary>
/// <remarks>
///  Consumed by: <c>GitExtensions.Extensibility/Settings/PseudoSetting.cs</c>,
///  <c>GitExtensions.Extensibility/Settings/StringSetting.cs</c>,
///  <c>GitExtensions.Extensibility/Settings/PasswordSetting.cs</c>.
/// </remarks>
public class TextBox : Control
{
    /// <summary>
    ///  Gets or sets a value indicating whether the text can be edited by the user.
    /// </summary>
    public bool ReadOnly { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating whether the text box accepts multiple lines.
    /// </summary>
    public bool Multiline { get; set; }

    /// <summary>
    ///  Gets or sets the border style; stored only, rendering is owned by the real UI framework.
    /// </summary>
    public BorderStyle BorderStyle { get; set; }
}
