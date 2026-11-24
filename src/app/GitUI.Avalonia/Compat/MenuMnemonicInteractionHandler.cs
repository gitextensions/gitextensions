using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace GitUI.Compat;

/// <summary>
///  Supplies ToolStrip character mnemonics while retaining native menu activation and navigation.
/// </summary>
internal sealed class MenuMnemonicInteractionHandler(bool isContextMenu)
    : DefaultMenuInteractionHandler(isContextMenu), IMenuInteractionHandler
{
    private MenuBase? _menu;
    private KeyModifiers _modifiers;
    private string? _handledText;

    void IMenuInteractionHandler.Attach(MenuBase menu)
    {
        _menu = menu;
        _modifiers = KeyModifiers.None;
        _handledText = null;
        menu.AddHandler(InputElement.KeyDownEvent, PreviewKeyDown, RoutingStrategies.Tunnel);
        menu.AddHandler(InputElement.KeyUpEvent, PreviewKeyUp, RoutingStrategies.Tunnel);
        menu.AddHandler(InputElement.TextInputEvent, PreviewTextInput, RoutingStrategies.Tunnel);
        Attach(menu);
    }

    void IMenuInteractionHandler.Detach(MenuBase menu)
    {
        menu.RemoveHandler(InputElement.KeyDownEvent, PreviewKeyDown);
        menu.RemoveHandler(InputElement.KeyUpEvent, PreviewKeyUp);
        menu.RemoveHandler(InputElement.TextInputEvent, PreviewTextInput);
        Detach(menu);
        _menu = null;
        _modifiers = KeyModifiers.None;
        _handledText = null;
    }

    private void PreviewKeyDown(object? sender, KeyEventArgs e)
    {
        _modifiers = e.KeyModifiers;
        _handledText = null;
        if (e.KeyModifiers.HasFlag(KeyModifiers.Alt)
            && e.KeySymbol is { Length: 1 } text && ProcessMnemonic(e.Source, text[0]))
        {
            _handledText = text;
            e.Handled = true;
        }
    }

    private void PreviewKeyUp(object? sender, KeyEventArgs e)
    {
        _modifiers = e.KeyModifiers;
        _handledText = null;
    }

    private void PreviewTextInput(object? sender, TextInputEventArgs e)
    {
        if (e.Text is not { Length: 1 } text)
        {
            return;
        }

        if (string.Equals(_handledText, text, StringComparison.Ordinal))
        {
            _handledText = null;
            e.Handled = true;
            return;
        }

        _handledText = null;
        e.Handled = ProcessMnemonic(e.Source, text[0]);
    }

    private bool ProcessMnemonic(object? source, char character)
    {
        bool isOpen = _menu is MenuFlyoutPresenter
            ? _menu.FindLogicalAncestorOfType<Popup>()?.IsOpen == true
            : _menu?.IsOpen == true;
        if (!isOpen || char.IsControl(character)
            || (_modifiers & (KeyModifiers.Control | KeyModifiers.Meta)) != 0
            || source is TextBox
            || (source is StyledElement element && element.GetLogicalAncestors().OfType<TextBox>().Any()))
        {
            return false;
        }

        MenuItem? sourceItem = source as MenuItem
            ?? (source as StyledElement)?.GetLogicalAncestors().OfType<MenuItem>().FirstOrDefault();
        SelectingItemsControl level = sourceItem is { IsSubMenuOpen: true }
            ? sourceItem
            : sourceItem?.Parent as SelectingItemsControl ?? _menu!;
        while (level.SelectedItem is MenuItem { IsSubMenuOpen: true } open)
        {
            level = open;
        }

        (MenuItem Item, string Text)[] candidates = level.Items.OfType<MenuItem>()
            .Where(item => item.IsEffectivelyVisible && item.IsEffectivelyEnabled
                && item.Header is string { Length: > 0 })
            .Select(item => (item, AvaloniaTranslationUtils.ToWinFormsMnemonics((string)item.Header!)))
            .ToArray();
        MenuItem[] matches = candidates.Where(candidate => ToolStripMnemonic.IsMnemonic(character, candidate.Text))
            .Select(candidate => candidate.Item).ToArray();
        if (matches.Length == 0)
        {
            matches = candidates.Where(candidate => ToolStripMnemonic.IsPseudoMnemonic(character, candidate.Text))
                .Select(candidate => candidate.Item).ToArray();
            if (matches.Length == 0)
            {
                return false;
            }
        }

        if (matches.Length == 1)
        {
            // The framework owns checking, Click, submenu focus and dismissal.
            AccessKeyPressed(_menu, new RoutedEventArgs { Source = matches[0] });
        }
        else
        {
            int current = Array.IndexOf(matches, level.SelectedItem as MenuItem);
            MenuItem next = matches[(current + 1) % matches.Length];
            level.SelectedItem = next;
            next.Focus(NavigationMethod.Tab);
        }

        return true;
    }
}
