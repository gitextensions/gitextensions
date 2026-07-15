using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using GitExtensions.Extensibility.Translations;

namespace GitUI.Compat;

// Existing XLF files use WinForms .Text keys. Partition the host fields once so Avalonia
// Content/Header properties are mapped here and every other field uses the shared walker.
internal static class AvaloniaTranslationUtils
{
    public static void AddTranslationItemsFromFields(string category, object host, ITranslation translation)
    {
        List<(string Name, object Item)> sharedItems = [];
        foreach ((string name, object item) in TranslationUtils.GetObjFields(host, "$this"))
        {
            if (!TryGetAvaloniaText(item, out string? text, out bool convertMnemonics))
            {
                sharedItems.Add((name, item));
                continue;
            }

            if (text?.Any(char.IsLetter) is true)
            {
                string neutralText = convertMnemonics ? ToWinFormsMnemonics(text) : text;
                translation.AddTranslationItem(category, name, "Text", neutralText);
            }
        }

        TranslationUtils.AddTranslationItemsFromList(category, translation, sharedItems);
    }

    public static void TranslateItemsFromFields(string category, object host, ITranslation translation)
    {
        List<(string Name, object Item)> sharedItems = [];
        foreach ((string name, object item) in TranslationUtils.GetObjFields(host, "$this"))
        {
            if (!TryGetAvaloniaText(item, out string? text, out bool convertMnemonics))
            {
                sharedItems.Add((name, item));
                continue;
            }

            if (text is null)
            {
                continue;
            }

            string neutralText = convertMnemonics ? ToWinFormsMnemonics(text) : text;
            string? translatedText = translation.TranslateItem(category, name, "Text", () => neutralText);
            if (!string.IsNullOrEmpty(translatedText))
            {
                SetAvaloniaText(item, convertMnemonics ? ToAvaloniaMnemonics(translatedText) : translatedText);
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
        }
    }

    private static string ToAvaloniaMnemonics(string text)
    {
        const string escapedAmpersand = "\u0001";
        return text
            .Replace("&&", escapedAmpersand, StringComparison.Ordinal)
            .Replace('&', '_')
            .Replace(escapedAmpersand, "&", StringComparison.Ordinal);
    }

    private static string ToWinFormsMnemonics(string text)
    {
        const string escapedUnderscore = "\u0001";
        return text
            .Replace("__", escapedUnderscore, StringComparison.Ordinal)
            .Replace('_', '&')
            .Replace(escapedUnderscore, "_", StringComparison.Ordinal);
    }
}
