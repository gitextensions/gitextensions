namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Windows.Forms.CheckBox</c>: a headless model of a boolean setting
///  control; the Avalonia settings renderer materializes a real control from it.
/// </summary>
/// <remarks>
///  Consumed by: <c>GitExtensions.Extensibility/Settings/BoolSetting.cs</c>.
/// </remarks>
public class CheckBox : Control
{
    /// <summary>
    ///  Gets or sets a value indicating whether the check box is checked; derived from
    ///  <see cref="CheckState"/> like in WinForms.
    /// </summary>
    public bool Checked
    {
        get => CheckState == CheckState.Checked;
        set => CheckState = value ? CheckState.Checked : CheckState.Unchecked;
    }

    /// <summary>
    ///  Gets or sets the three-state check value.
    /// </summary>
    public CheckState CheckState { get; set; }
}
