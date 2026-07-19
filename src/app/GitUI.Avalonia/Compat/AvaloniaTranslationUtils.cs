using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using GitExtensions.Extensibility.Translations;

namespace GitUI.Compat;

// Existing XLF files use WinForms .Text keys. Partition the host fields once so Avalonia
// Content/Header/TextBlock text and attached tooltips are mapped here, while every other
// field uses the shared walker.
internal static class AvaloniaTranslationUtils
{
    private static readonly ConditionalWeakTable<TextBlock, TextBlockSource> TextBlockSources = new();

    internal static void RemoveTextBlockMnemonicMarkers(object host)
    {
        foreach ((_, object item) in TranslationUtils.GetObjFields(host, "$this"))
        {
            if (item is not TextBlock textBlock || textBlock.Text is not string text)
            {
                continue;
            }

            RememberTextBlockSource(textBlock, text);
            textBlock.Text = RemoveAvaloniaMnemonics(text);
        }
    }

    public static void AddTranslationItemsFromFields(string category, object host, ITranslation translation)
    {
        List<(string Name, object Item)> sharedItems = [];
        foreach ((string name, object item) in TranslationUtils.GetObjFields(host, "$this"))
        {
            bool hasText = TryGetAvaloniaText(item, out string? text, out bool convertMnemonics);
            bool hasToolTip = item is Control control && ToolTip.GetTip(control) is string;
            if (item is TextBox { PlaceholderText: string placeholderText }
                && placeholderText.Any(char.IsLetter))
            {
                translation.AddTranslationItem(category, name, "Watermark", placeholderText);
            }

            if (!hasText && !hasToolTip)
            {
                sharedItems.Add((name, item));
                continue;
            }

            if (hasText && text?.Any(char.IsLetter) is true)
            {
                string neutralText = convertMnemonics ? ToWinFormsMnemonics(text) : text;
                translation.AddTranslationItem(category, name, "Text", neutralText);
            }

            if (hasToolTip
                && ToolTip.GetTip((Control)item) is string toolTip
                && toolTip.Any(char.IsLetter))
            {
                translation.AddTranslationItem(category, name, "toolTip", toolTip);
            }
        }

        TranslationUtils.AddTranslationItemsFromList(category, translation, sharedItems);
    }

    public static void TranslateItemsFromFields(string category, object host, ITranslation translation)
    {
        List<(string Name, object Item)> sharedItems = [];
        foreach ((string name, object item) in TranslationUtils.GetObjFields(host, "$this"))
        {
            bool hasText = TryGetAvaloniaText(item, out string? text, out bool convertMnemonics);
            bool hasToolTip = item is Control control && ToolTip.GetTip(control) is string;
            if (item is TextBox { PlaceholderText: string placeholderText })
            {
                string? translatedPlaceholder = translation.TranslateItem(
                    category,
                    name,
                    "Watermark",
                    () => placeholderText);
                if (!string.IsNullOrEmpty(translatedPlaceholder))
                {
                    ((TextBox)item).PlaceholderText = translatedPlaceholder;
                }
            }

            if (!hasText && !hasToolTip)
            {
                sharedItems.Add((name, item));
                continue;
            }

            if (hasText && text is not null)
            {
                string neutralText = convertMnemonics ? ToWinFormsMnemonics(text) : text;
                string? translatedText = translation.TranslateItem(category, name, "Text", () => neutralText);
                if (!string.IsNullOrEmpty(translatedText))
                {
                    SetAvaloniaText(item, convertMnemonics ? ToAvaloniaMnemonics(translatedText) : translatedText);
                }
                else if (item is TextBlock)
                {
                    // English XLF targets are intentionally empty, so the source AXAML is
                    // also the display fallback and still needs its access marker removed.
                    SetAvaloniaText(item, text);
                }
            }

            if (hasToolTip && ToolTip.GetTip((Control)item) is string toolTip)
            {
                string? translatedToolTip = translation.TranslateItem(category, name, "toolTip", () => toolTip);
                if (!string.IsNullOrEmpty(translatedToolTip))
                {
                    ToolTip.SetTip((Control)item, translatedToolTip);
                }
            }
        }

        TranslationUtils.TranslateItemsFromList(category, translation, sharedItems);
    }

    private static bool TryGetAvaloniaText(object item, out string? text, out bool convertMnemonics)
    {
        convertMnemonics = true;
        switch (item)
        {
            case Window window:
                text = window.Title;
                convertMnemonics = false;
                return true;
            case MenuItem menuItem:
                text = menuItem.Header as string;
                return true;
            case HeaderedContentControl headeredContentControl:
                text = headeredContentControl.Header as string;
                return true;
            case HeaderedSelectingItemsControl headeredSelectingItemsControl:
                text = headeredSelectingItemsControl.Header as string;
                return true;
            case HeaderedItemsControl headeredItemsControl:
                text = headeredItemsControl.Header as string;
                return true;
            case ContentControl contentControl:
                text = contentControl.Content as string;
                return true;
            case TextBlock textBlock:
                text = GetTextBlockSource(textBlock);
                return true;
            default:
                text = null;
                return false;
        }
    }

    private static void SetAvaloniaText(object item, string text)
    {
        switch (item)
        {
            case Window window:
                window.Title = text;
                break;
            case MenuItem menuItem:
                menuItem.Header = text;
                break;
            case HeaderedContentControl headeredContentControl:
                headeredContentControl.Header = text;
                break;
            case HeaderedSelectingItemsControl headeredSelectingItemsControl:
                headeredSelectingItemsControl.Header = text;
                break;
            case HeaderedItemsControl headeredItemsControl:
                headeredItemsControl.Header = text;
                break;
            case ContentControl contentControl:
                contentControl.Content = text;
                break;
            case TextBlock textBlock:
                // TextBlock has no access-key presenter. Keep the marker in AXAML so the
                // existing WinForms XLF key round-trips, but do not render it to the user.
                textBlock.Text = RemoveAvaloniaMnemonics(text);
                break;
        }
    }

    internal static string ToAvaloniaMnemonics(string text)
    {
        const string escapedAmpersand = "\u0001";
        return text
            .Replace("&&", escapedAmpersand, StringComparison.Ordinal)
            .Replace('&', '_')
            .Replace(escapedAmpersand, "&", StringComparison.Ordinal);
    }

    internal static string RemoveAvaloniaMnemonics(string text)
    {
        const string escapedUnderscore = "\u0001";
        return text
            .Replace("__", escapedUnderscore, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace(escapedUnderscore, "_", StringComparison.Ordinal);
    }

    private static string ToWinFormsMnemonics(string text)
    {
        const string escapedUnderscore = "\u0001";
        return text
            .Replace("__", escapedUnderscore, StringComparison.Ordinal)
            .Replace('_', '&')
            .Replace(escapedUnderscore, "_", StringComparison.Ordinal);
    }

    private static string? GetTextBlockSource(TextBlock textBlock)
    {
        if (TextBlockSources.TryGetValue(textBlock, out TextBlockSource? source))
        {
            return source.Text;
        }

        string? text = textBlock.Text;
        if (text is not null)
        {
            RememberTextBlockSource(textBlock, text);
        }

        return text;
    }

    private static void RememberTextBlockSource(TextBlock textBlock, string text)
        => TextBlockSources.GetValue(textBlock, _ => new TextBlockSource(text));

    private sealed record TextBlockSource(string Text);
}
