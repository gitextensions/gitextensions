using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace GitUI.Compat;

/// <summary>
///  Adapts only the WorkingDir popup's activation context and source mnemonic search.
/// </summary>
internal sealed class WorkingDirectoryMenuInteractionHandler(
    TextBox filter,
    Func<MenuItem, string?> getSourceText,
    Action<KeyModifiers, Action> activate) : DefaultMenuInteractionHandler(true), IMenuInteractionHandler
{
    private MenuBase? _menu;
    private KeyModifiers _keyModifiers;
    private string? _handledKeyText;

    void IMenuInteractionHandler.Attach(MenuBase menu)
    {
        _menu = menu;
        _keyModifiers = KeyModifiers.None;
        _handledKeyText = null;
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
        _keyModifiers = KeyModifiers.None;
        _handledKeyText = null;
    }

    protected override void AccessKeyPressed(object? sender, RoutedEventArgs e)
    {
        // AccessText knows only its first marker and may register a literal '_'.
        // Owned source matching below supplies the actual target instead.
        e.Handled = true;
    }

    protected override void KeyDown(object? sender, KeyEventArgs e)
    {
        if (!IsFilterInput(e.Source) && e.Key == Key.Enter)
        {
            if (e.KeyModifiers != KeyModifiers.None && GetMenuItem(e.Source) is { HasSubMenu: false })
            {
                // ToolStripItem.ProcessDialogKey compares the complete keyData to
                // Enter; a modified leaf Enter is not a source Click activation.
                e.Handled = true;
                return;
            }

            // The framework still owns Enter's click/open and menu-close sequence.
            activate(e.KeyModifiers, () => base.KeyDown(sender, e));
            return;
        }

        base.KeyDown(sender, e);
    }

    protected override void PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        // ToolStrip reads ModifierKeys at Click, which follows release, not press.
        // The synchronous scope cannot retain a modifier on a different menu row.
        activate(e.KeyModifiers, () => base.PointerReleased(sender, e));
    }

    private void PreviewKeyDown(object? sender, KeyEventArgs e)
    {
        _keyModifiers = e.KeyModifiers;
        _handledKeyText = null;
        if (IsFilterInput(e.Source)
            || (e.KeyModifiers & (KeyModifiers.Control | KeyModifiers.Meta)) != 0
            || !e.KeyModifiers.HasFlag(KeyModifiers.Alt)
            || e.KeySymbol is not { Length: 1 } text
            || char.IsControl(text[0]))
        {
            return;
        }

        // MenuItem's internal access-key handler runs before the root bubble handler.
        // Intercept a real source match in the public tunnel, leaving other routes alone.
        if (ProcessMnemonic(e.Source, text[0], e.KeyModifiers))
        {
            _handledKeyText = text;
            e.Handled = true;
        }
    }

    private void PreviewKeyUp(object? sender, KeyEventArgs e)
    {
        _keyModifiers = e.KeyModifiers;
        _handledKeyText = null;
    }

    private void PreviewTextInput(object? sender, TextInputEventArgs e)
    {
        if (IsFilterInput(e.Source) || e.Text is not { Length: 1 } text || char.IsControl(text[0]))
        {
            return;
        }

        if (string.Equals(_handledKeyText, text, StringComparison.Ordinal))
        {
            _handledKeyText = null;
            e.Handled = true;
            return;
        }

        _handledKeyText = null;
        if ((_keyModifiers & (KeyModifiers.Control | KeyModifiers.Meta)) == 0)
        {
            e.Handled = ProcessMnemonic(e.Source, text[0], _keyModifiers);
        }
    }

    private bool IsFilterInput(object? source)
        => ReferenceEquals(source, filter)
           || (source is Visual visual && visual.GetVisualAncestors().Any(ancestor => ReferenceEquals(ancestor, filter)));

    private static MenuItem? GetMenuItem(object? source)
        => source is MenuItem item
            ? item
            : (source as StyledElement)?.GetLogicalAncestors().OfType<MenuItem>().FirstOrDefault();

    private bool ProcessMnemonic(object? source, char charCode, KeyModifiers modifiers)
    {
        MenuItem? sourceItem = GetMenuItem(source);
        SelectingItemsControl? level = sourceItem?.Parent as SelectingItemsControl ?? _menu;
        if (level is null)
        {
            return false;
        }

        MenuItem[] displayedItems = level.Items.OfType<MenuItem>()
            .Where(candidate => candidate.IsEffectivelyVisible).ToArray();
        MenuItem? startingItem = level.SelectedItem as MenuItem;
        int startIndex = startingItem is null ? 0 : Math.Max(0, Array.IndexOf(displayedItems, startingItem));
        MenuItem? firstMatch = null;
        bool foundMenuItem = false;
        int index = startIndex;

        // PASS1, iterate through the real mnemonics.
        for (int count = 0; count < displayedItems.Length; count++)
        {
            MenuItem currentItem = displayedItems[index];
            index = (index + 1) % displayedItems.Length;
            string? text = getSourceText(currentItem);
            if (string.IsNullOrEmpty(text) || !currentItem.IsEffectivelyEnabled)
            {
                continue;
            }

            foundMenuItem = true;
            if (IsMnemonic(charCode, text))
            {
                if (firstMatch is null)
                {
                    firstMatch = currentItem;
                }
                else
                {
                    SelectDuplicate(level, firstMatch == startingItem ? currentItem : firstMatch);
                    return true;
                }
            }
        }

        if (firstMatch is not null)
        {
            Activate(firstMatch, modifiers);
            return true;
        }

        if (!foundMenuItem)
        {
            return false;
        }

        index = startIndex;

        // PASS2, iterate through the pseudo mnemonics.
        for (int count = 0; count < displayedItems.Length; count++)
        {
            MenuItem currentItem = displayedItems[index];
            index = (index + 1) % displayedItems.Length;
            string? text = getSourceText(currentItem);
            if (string.IsNullOrEmpty(text) || !currentItem.IsEffectivelyEnabled)
            {
                continue;
            }

            if (IsPseudoMnemonic(charCode, text))
            {
                if (firstMatch is null)
                {
                    firstMatch = currentItem;
                }
                else
                {
                    SelectDuplicate(level, firstMatch == startingItem ? currentItem : firstMatch);
                    return true;
                }
            }
        }

        if (firstMatch is not null)
        {
            Activate(firstMatch, modifiers);
            return true;
        }

        return false;
    }

    private void Activate(MenuItem target, KeyModifiers modifiers)
        => activate(modifiers, () => base.AccessKeyPressed(_menu, new RoutedEventArgs { Source = target }));

    private static void SelectDuplicate(SelectingItemsControl level, MenuItem target)
    {
        level.SelectedItem = target;
        target.Focus(NavigationMethod.Tab);
    }

    private static bool IsMnemonic(char charCode, string text)
    {
        if (charCode == '&')
        {
            return false;
        }

        int position = -1;
        char upperCode = char.ToUpper(charCode, CultureInfo.CurrentCulture);
        while (position + 1 < text.Length)
        {
            position = text.IndexOf('&', position + 1) + 1;
            if (position <= 0 || position >= text.Length)
            {
                break;
            }

            char upperText = char.ToUpper(text[position], CultureInfo.CurrentCulture);
            if (upperText == upperCode
                || char.ToLower(upperText, CultureInfo.CurrentCulture) == char.ToLower(upperCode, CultureInfo.CurrentCulture))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsPseudoMnemonic(char charCode, string text)
    {
        // Preserve WinFormsUtils.ContainsMnemonic's actual legacy rule rather than
        // replacing it with a more conventional prefix parser.
        int firstAmpersand = text.IndexOf('&');
        bool containsMnemonic = firstAmpersand >= 0 && firstAmpersand <= text.Length - 2
            && text.IndexOf('&', firstAmpersand + 1) == -1;
        return !containsMnemonic
            && (char.ToUpper(text[0], CultureInfo.CurrentCulture) == char.ToUpper(charCode, CultureInfo.CurrentCulture)
                || char.ToLower(text[0], CultureInfo.CurrentCulture) == char.ToLower(charCode, CultureInfo.CurrentCulture));
    }
}
