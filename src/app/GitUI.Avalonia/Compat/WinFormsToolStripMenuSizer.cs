using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace GitUI.Compat;

/// <summary>
/// Applies the WinForms <c>ToolStripDropDownMenu</c> 96-DPI width calculation to an Avalonia submenu.
/// </summary>
internal static class WinFormsToolStripMenuSizer
{
    private const double TextRendererOverhang = 7;
    private const double DefaultImageMarginWidth = 25;
    private const double TextPaddingLeft = 8;
    private const double TextPaddingRight = 9;
    private const double ArrowWidth = 10;
    private const double ArrowPaddingRight = 8;
    private static readonly ConditionalWeakTable<MenuItem, ShortcutDisplay> ShortcutDisplays = new();

    public static void SetShortcutDisplayString(MenuItem item, string? displayString)
    {
        ShortcutDisplays.Remove(item);
        if (!string.IsNullOrEmpty(displayString))
        {
            ShortcutDisplays.Add(item, new ShortcutDisplay(displayString));
        }
    }

    public static void Apply(MenuItem owner)
    {
        MenuItem[] menuItems = [.. owner.Items.OfType<MenuItem>()];
        if (menuItems.Length == 0)
        {
            return;
        }

        // ToolStripDropDownMenu.ShowImageMargin defaults to true. Its layout always reserves
        // one measured tab before the shortcut column, even when an item's shortcut is empty.
        double tabWidth = MeasureText(owner, "\t");
        double maximumTextAndShortcutWidth = menuItems.Max(item =>
            MeasureText(item, GetDisplayText(item.Header))
            + tabWidth
            + GetShortcutWidth(item));
        double itemWidth = Math.Ceiling(DefaultImageMarginWidth
            + TextPaddingLeft
            + maximumTextAndShortcutWidth
            + TextPaddingRight
            + ArrowWidth
            + ArrowPaddingRight);

        foreach (MenuItem item in menuItems)
        {
            item.Width = itemWidth;
            if (ShortcutDisplays.TryGetValue(item, out ShortcutDisplay? display))
            {
                item.GetVisualDescendants()
                    .OfType<TextBlock>()
                    .FirstOrDefault(textBlock => textBlock.Name == "PART_InputGestureText")
                    ?.SetCurrentValue(TextBlock.TextProperty, display.Value);
            }
        }
    }

    private static double MeasureText(MenuItem owner, string text)
        => WinFormsTextMeasurer.Measure(owner, text) + TextRendererOverhang;

    private static double GetShortcutWidth(MenuItem item)
    {
        string? shortcut = ShortcutDisplays.TryGetValue(item, out ShortcutDisplay? display)
            ? display.Value
            : item.InputGesture?.ToString();
        return string.IsNullOrEmpty(shortcut) ? 0 : MeasureText(item, shortcut);
    }

    private static string GetDisplayText(object? header)
    {
        string source = header?.ToString() ?? string.Empty;
        if (!source.Contains('_', StringComparison.Ordinal))
        {
            return source;
        }

        Span<char> buffer = source.Length <= 256 ? stackalloc char[source.Length] : new char[source.Length];
        int output = 0;
        bool mnemonicRemoved = false;
        for (int index = 0; index < source.Length; index++)
        {
            if (source[index] != '_')
            {
                buffer[output++] = source[index];
                continue;
            }

            if (index + 1 < source.Length && source[index + 1] == '_')
            {
                buffer[output++] = '_';
                index++;
            }
            else if (mnemonicRemoved)
            {
                buffer[output++] = '_';
            }
            else
            {
                mnemonicRemoved = true;
            }
        }

        return new string(buffer[..output]);
    }

    private sealed class ShortcutDisplay(string value)
    {
        public string Value { get; } = value;
    }
}
