using Avalonia.Controls;
using Avalonia.Input;

namespace GitUI.Compat;

/// <summary>
///  Adapts ToolStrip-owned captions and current activation input to the retained native popup.
/// </summary>
internal static class NativeRepositoryMenu
{
    internal static KeyModifiers? ActivationModifiers { get; private set; }

    internal static void Activate(KeyModifiers modifiers, Action activate)
    {
        KeyModifiers? previous = ActivationModifiers;
        ActivationModifiers = modifiers;
        try
        {
            activate();
        }
        finally
        {
            ActivationModifiers = previous;
        }
    }

    internal static NativeToolStripDropDownMenuItem Copy(MenuItem source)
    {
        NativeToolStripDropDownMenuItem item = new()
        {
            UseSourceMnemonicRouting = true,
            Header = source.Header,
            Icon = source.Icon is Image image ? new Image { Width = 16, Height = 16, Source = image.Source } : null,
            InputGesture = source.InputGesture,
        };
        item.Classes.Add("gitextensions-working-directory-entry");
        WinFormsToolStripMenuSizer.SetShortcutDisplayString(item, WinFormsToolStripMenuSizer.GetShortcutDisplayString(source));
        return item;
    }

    internal static string? GetSourceItemText(MenuItem item)
    {
        if (item.Header is not string text)
        {
            return null;
        }

        // Header's encoding is reversible; the owned presenter normalizes only its
        // painted Content, so fixed/category captions retain every native marker.
        System.Text.StringBuilder source = new(text.Length);
        for (int index = 0; index < text.Length; index++)
        {
            char character = text[index];
            if (character == '_')
            {
                if (index + 1 < text.Length && text[index + 1] == '_')
                {
                    source.Append('_');
                    index++;
                }
                else
                {
                    source.Append('&');
                }
            }
            else
            {
                source.Append(character);
                if (character == '&')
                {
                    source.Append('&');
                }
            }
        }

        return source.ToString();
    }
}
