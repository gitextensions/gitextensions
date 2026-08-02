using Avalonia.Input;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.Compat;

/// <summary>Maps Avalonia key events to the persisted WinForms-compatible hotkey values.</summary>
internal static class KeysMapper
{
    public static KeyGesture? ToKeyGesture(WinFormsShims.Keys? keyData)
    {
        if (keyData is null)
        {
            return null;
        }

        WinFormsShims.Keys keyCode = keyData.Value & WinFormsShims.Keys.KeyCode;
        Key key = keyCode switch
        {
            >= WinFormsShims.Keys.A and <= WinFormsShims.Keys.Z => Key.A + (keyCode - WinFormsShims.Keys.A),
            >= WinFormsShims.Keys.D0 and <= WinFormsShims.Keys.D9 => Key.D0 + (keyCode - WinFormsShims.Keys.D0),
            >= WinFormsShims.Keys.NumPad0 and <= WinFormsShims.Keys.NumPad9 => Key.NumPad0 + (keyCode - WinFormsShims.Keys.NumPad0),
            >= WinFormsShims.Keys.F1 and <= WinFormsShims.Keys.F24 => Key.F1 + (keyCode - WinFormsShims.Keys.F1),
            WinFormsShims.Keys.Back => Key.Back,
            WinFormsShims.Keys.Tab => Key.Tab,
            WinFormsShims.Keys.Enter => Key.Enter,
            WinFormsShims.Keys.Escape => Key.Escape,
            WinFormsShims.Keys.Space => Key.Space,
            WinFormsShims.Keys.PageUp => Key.PageUp,
            WinFormsShims.Keys.PageDown => Key.PageDown,
            WinFormsShims.Keys.End => Key.End,
            WinFormsShims.Keys.Home => Key.Home,
            WinFormsShims.Keys.Left => Key.Left,
            WinFormsShims.Keys.Up => Key.Up,
            WinFormsShims.Keys.Right => Key.Right,
            WinFormsShims.Keys.Down => Key.Down,
            WinFormsShims.Keys.Insert => Key.Insert,
            WinFormsShims.Keys.Delete => Key.Delete,
            WinFormsShims.Keys.Multiply => Key.Multiply,
            WinFormsShims.Keys.Add => Key.Add,
            WinFormsShims.Keys.Subtract => Key.Subtract,
            WinFormsShims.Keys.Decimal => Key.Decimal,
            WinFormsShims.Keys.Divide => Key.Divide,
            WinFormsShims.Keys.OemSemicolon => Key.OemSemicolon,
            WinFormsShims.Keys.Oemplus => Key.OemPlus,
            WinFormsShims.Keys.Oemcomma => Key.OemComma,
            WinFormsShims.Keys.OemMinus => Key.OemMinus,
            WinFormsShims.Keys.OemPeriod => Key.OemPeriod,
            WinFormsShims.Keys.OemQuestion => Key.OemQuestion,
            WinFormsShims.Keys.Oemtilde => Key.OemTilde,
            WinFormsShims.Keys.OemOpenBrackets => Key.OemOpenBrackets,
            WinFormsShims.Keys.OemPipe => Key.OemPipe,
            WinFormsShims.Keys.OemCloseBrackets => Key.OemCloseBrackets,
            WinFormsShims.Keys.OemQuotes => Key.OemQuotes,
            WinFormsShims.Keys.OemBackslash => Key.OemBackslash,
            WinFormsShims.Keys.BrowserBack => Key.BrowserBack,
            WinFormsShims.Keys.BrowserForward => Key.BrowserForward,
            _ => Key.None,
        };
        if (key == Key.None)
        {
            return null;
        }

        KeyModifiers modifiers = KeyModifiers.None;
        if (keyData.Value.HasFlag(WinFormsShims.Keys.Shift))
        {
            modifiers |= KeyModifiers.Shift;
        }

        if (keyData.Value.HasFlag(WinFormsShims.Keys.Control))
        {
            modifiers |= OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;
        }

        if (keyData.Value.HasFlag(WinFormsShims.Keys.Alt))
        {
            modifiers |= KeyModifiers.Alt;
        }

        return new KeyGesture(key, modifiers);
    }

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
