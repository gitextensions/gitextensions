using System.Globalization;
using System.Text;

namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Windows.Forms.KeysConverter</c>: converts <see cref="Keys"/> values
///  to display strings with the same shape WinForms produces (for example <c>Ctrl+None</c> for
///  a bare modifier, <c>S</c> for a plain key). Strings are invariant; WinForms' localized
///  modifier names are not reproduced.
/// </summary>
/// <remarks>
///  Consumed by: <c>ResourceManager/Hotkey/KeysExtensions.cs</c>.
/// </remarks>
public class KeysConverter
{
    /// <summary>
    ///  Converts a <see cref="Keys"/> value to its display string.
    /// </summary>
    /// <param name="context">Unused; present for WinForms signature compatibility.</param>
    /// <param name="culture">Unused; strings are invariant.</param>
    /// <param name="value">The <see cref="Keys"/> value to convert.</param>
    public string? ConvertToString(object? context, CultureInfo? culture, object? value)
    {
        if (value is not Keys key)
        {
            return value?.ToString();
        }

        StringBuilder text = new();

        if (key.HasFlag(Keys.Control))
        {
            text.Append("Ctrl+");
        }

        if (key.HasFlag(Keys.Shift))
        {
            text.Append("Shift+");
        }

        if (key.HasFlag(Keys.Alt))
        {
            text.Append("Alt+");
        }

        text.Append(key & Keys.KeyCode);
        return text.ToString();
    }
}
