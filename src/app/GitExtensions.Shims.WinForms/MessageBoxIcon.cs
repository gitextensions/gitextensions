namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Windows.Forms.MessageBoxIcon</c>; values match WinForms
///  (several names intentionally share a value, as in WinForms).
/// </summary>
public enum MessageBoxIcon
{
    None = 0,
    Hand = 0x10,
    Stop = 0x10,
    Error = 0x10,
    Question = 0x20,
    Exclamation = 0x30,
    Warning = 0x30,
    Asterisk = 0x40,
    Information = 0x40,
}
