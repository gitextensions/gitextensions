using Avalonia.Input;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.Compat;

/// <summary>Maps Avalonia key events to the persisted WinForms-compatible hotkey values.</summary>
internal static class KeysMapper
{
    public static WinFormsShims.Keys ToKeys(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        return ToKeys(e.Key, e.KeyModifiers);
    }

    public static WinFormsShims.Keys ToKeys(Key key, KeyModifiers modifiers)
    {
        WinFormsShims.Keys mappedKey = key switch
        {
            >= Key.A and <= Key.Z => WinFormsShims.Keys.A + (key - Key.A),
            >= Key.D0 and <= Key.D9 => WinFormsShims.Keys.D0 + (key - Key.D0),
            >= Key.NumPad0 and <= Key.NumPad9 => WinFormsShims.Keys.NumPad0 + (key - Key.NumPad0),
            >= Key.F1 and <= Key.F24 => WinFormsShims.Keys.F1 + (key - Key.F1),
            Key.Back => WinFormsShims.Keys.Back,
            Key.Tab => WinFormsShims.Keys.Tab,
            Key.Enter => WinFormsShims.Keys.Enter,
            Key.Escape => WinFormsShims.Keys.Escape,
            Key.Space => WinFormsShims.Keys.Space,
            Key.PageUp => WinFormsShims.Keys.PageUp,
            Key.PageDown => WinFormsShims.Keys.PageDown,
            Key.End => WinFormsShims.Keys.End,
            Key.Home => WinFormsShims.Keys.Home,
            Key.Left => WinFormsShims.Keys.Left,
            Key.Up => WinFormsShims.Keys.Up,
            Key.Right => WinFormsShims.Keys.Right,
            Key.Down => WinFormsShims.Keys.Down,
            Key.Insert => WinFormsShims.Keys.Insert,
            Key.Delete => WinFormsShims.Keys.Delete,
            Key.Multiply => WinFormsShims.Keys.Multiply,
            Key.Add => WinFormsShims.Keys.Add,
            Key.Subtract => WinFormsShims.Keys.Subtract,
            Key.Decimal => WinFormsShims.Keys.Decimal,
            Key.Divide => WinFormsShims.Keys.Divide,
            Key.OemSemicolon => WinFormsShims.Keys.OemSemicolon,
            Key.OemPlus => WinFormsShims.Keys.Oemplus,
            Key.OemComma => WinFormsShims.Keys.Oemcomma,
            Key.OemMinus => WinFormsShims.Keys.OemMinus,
            Key.OemPeriod => WinFormsShims.Keys.OemPeriod,
            Key.OemQuestion => WinFormsShims.Keys.OemQuestion,
            Key.OemTilde => WinFormsShims.Keys.Oemtilde,
            Key.OemOpenBrackets => WinFormsShims.Keys.OemOpenBrackets,
            Key.OemPipe => WinFormsShims.Keys.OemPipe,
            Key.OemCloseBrackets => WinFormsShims.Keys.OemCloseBrackets,
            Key.OemQuotes => WinFormsShims.Keys.OemQuotes,
            Key.OemBackslash => WinFormsShims.Keys.OemBackslash,
            Key.BrowserBack => WinFormsShims.Keys.BrowserBack,
            Key.BrowserForward => WinFormsShims.Keys.BrowserForward,
            _ => WinFormsShims.Keys.None,
        };

        if (mappedKey == WinFormsShims.Keys.None)
        {
            return mappedKey;
        }

        if (modifiers.HasFlag(KeyModifiers.Shift))
        {
            mappedKey |= WinFormsShims.Keys.Shift;
        }

        if (modifiers.HasFlag(KeyModifiers.Control) || modifiers.HasFlag(KeyModifiers.Meta))
        {
            // Persisted Git Extensions Ctrl shortcuts map to Command on macOS.
            mappedKey |= WinFormsShims.Keys.Control;
        }

        if (modifiers.HasFlag(KeyModifiers.Alt))
        {
            mappedKey |= WinFormsShims.Keys.Alt;
        }

        return mappedKey;
    }
}
