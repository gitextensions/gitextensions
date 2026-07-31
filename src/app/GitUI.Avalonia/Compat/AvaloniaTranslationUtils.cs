using System.Runtime.CompilerServices;
using Avalonia;
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
            TextBlock? textBlock = item switch
            {
                TextBlock fieldTextBlock => fieldTextBlock,
                ContentControl { Content: TextBlock contentTextBlock } => contentTextBlock,
                _ => null,
            };
            if (textBlock is AccessText || textBlock?.Text is not string text)
            {
                continue;
            }

            RememberTextBlockSource(textBlock, text);
            textBlock.Text = RemoveAvaloniaMnemonics(text);
        }
    }

    public static void AddTranslationItemsFromFields(string category, object host, ITranslation translation)
    {
        if (host.GetType().IsDefined(typeof(UntranslatedAttribute), inherit: true))
        {
            return;
        }

        List<(string Name, object Item)> sharedItems = [];
        foreach ((string name, object item) in TranslationUtils.GetObjFields(host, "$this"))
        {
            if (name.StartsWith("_NO_TRANSLATE_", StringComparison.Ordinal))
            {
                continue;
            }

            if (name != "$this" && item is Window)
            {
                continue;
            }

            bool hostHasWinFormsText = name == "$this"
                && item is not Window
                && item.GetType().GetProperty("Text")?.PropertyType == typeof(string);
            string? text = null;
            bool convertMnemonics = false;
            bool isAvaloniaText = !hostHasWinFormsText
                && TryGetAvaloniaText(item, out text, out convertMnemonics);
            bool hasText = isAvaloniaText
                && (item is not Control textControl || TranslationCompat.GetTranslateText(textControl));
            bool suppressSharedText = item is Control sharedTextControl
                && !TranslationCompat.GetTranslateText(sharedTextControl);
            bool hasToolTip = item is Control control
                && TranslationCompat.GetTranslateToolTip(control)
                && ToolTip.GetTip(control) is string;
            if (item is TextBox { PlaceholderText: string placeholderText }
                && TranslationCompat.GetTranslateWatermark((TextBox)item)
                && placeholderText.Any(char.IsLetter))
            {
                translation.AddTranslationItem(category, name, "Watermark", placeholderText);
            }

            if (!isAvaloniaText && !hasToolTip && !suppressSharedText)
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
                translation.AddTranslationItem(category, name, GetToolTipPropertyName((Control)item), toolTip);
            }
        }

        TranslationUtils.AddTranslationItemsFromList(category, translation, sharedItems);
    }

    public static void TranslateItemsFromFields(string category, object host, ITranslation translation)
    {
        if (host.GetType().IsDefined(typeof(UntranslatedAttribute), inherit: true))
        {
            return;
        }

        List<(string Name, object Item)> sharedItems = [];
        foreach ((string name, object item) in TranslationUtils.GetObjFields(host, "$this"))
        {
            if (name.StartsWith("_NO_TRANSLATE_", StringComparison.Ordinal))
            {
                continue;
            }

            if (name != "$this" && item is Window)
            {
                continue;
            }

            bool hostHasWinFormsText = name == "$this"
                && item is not Window
                && item.GetType().GetProperty("Text")?.PropertyType == typeof(string);
            string? text = null;
            bool convertMnemonics = false;
            bool isAvaloniaText = !hostHasWinFormsText
                && TryGetAvaloniaText(item, out text, out convertMnemonics);
            bool hasText = isAvaloniaText
                && (item is not Control textControl || TranslationCompat.GetTranslateText(textControl));
            bool suppressSharedText = item is Control sharedTextControl
                && !TranslationCompat.GetTranslateText(sharedTextControl);
            bool hasToolTip = item is Control control
                && TranslationCompat.GetTranslateToolTip(control)
                && ToolTip.GetTip(control) is string;
            if (item is TextBox { PlaceholderText: string placeholderText })
            {
                if (TranslationCompat.GetTranslateWatermark((TextBox)item))
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
            }

            if (!isAvaloniaText && !hasToolTip && !suppressSharedText)
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
                else if (item is TextBlock or ContentControl { Content: TextBlock })
                {
                    // English XLF targets are intentionally empty, so the source AXAML is
                    // also the display fallback and still needs its access marker removed.
                    SetAvaloniaText(item, text);
                }
            }

            if (hasToolTip && ToolTip.GetTip((Control)item) is string toolTip)
            {
                string? translatedToolTip = translation.TranslateItem(
                    category,
                    name,
                    GetToolTipPropertyName((Control)item),
                    () => toolTip);
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
            case TabItem tabItem:
                text = tabItem.Header as string;
                convertMnemonics = false;
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
            case ContentControl { Content: TextBlock contentTextBlock }:
                text = GetTextBlockSource(contentTextBlock);
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
            case ContentControl { Content: AccessText accessText }:
                accessText.Text = text;
                break;
            case ContentControl { Content: TextBlock contentTextBlock }:
                contentTextBlock.Text = RemoveAvaloniaMnemonics(text);
                break;
            case ContentControl contentControl:
                contentControl.Content = text;
                break;
            case AccessText accessText:
                accessText.Text = text;
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
            .Replace("&", "&&", StringComparison.Ordinal)
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

    private static string GetToolTipPropertyName(Control control)
        => TranslationCompat.GetToolTipPropertyName(control)
            ?? (TranslationCompat.GetUseToolTipText(control) ? "ToolTipText" : "toolTip");
}

/// <summary>
/// Marks controls whose WinForms translation uses a ToolStripItem.ToolTipText key rather
/// than a ToolTip component's toolTip key.
/// </summary>
public sealed class TranslationCompat : AvaloniaObject
{
    public static readonly AttachedProperty<bool> TranslateTextProperty =
        AvaloniaProperty.RegisterAttached<TranslationCompat, Control, bool>("TranslateText", defaultValue: true);

    public static readonly AttachedProperty<bool> TranslateToolTipProperty =
        AvaloniaProperty.RegisterAttached<TranslationCompat, Control, bool>("TranslateToolTip", defaultValue: true);

    public static readonly AttachedProperty<bool> TranslateWatermarkProperty =
        AvaloniaProperty.RegisterAttached<TranslationCompat, TextBox, bool>("TranslateWatermark", defaultValue: true);

    public static readonly AttachedProperty<bool> UseToolTipTextProperty =
        AvaloniaProperty.RegisterAttached<TranslationCompat, Control, bool>("UseToolTipText");

    public static readonly AttachedProperty<string?> ToolTipPropertyNameProperty =
        AvaloniaProperty.RegisterAttached<TranslationCompat, Control, string?>("ToolTipPropertyName");

    public static bool GetTranslateText(Control control)
        => control.GetValue(TranslateTextProperty);

    public static void SetTranslateText(Control control, bool value)
        => control.SetValue(TranslateTextProperty, value);

    public static bool GetTranslateToolTip(Control control)
        => control.GetValue(TranslateToolTipProperty);

    public static void SetTranslateToolTip(Control control, bool value)
        => control.SetValue(TranslateToolTipProperty, value);

    public static bool GetTranslateWatermark(TextBox textBox)
        => textBox.GetValue(TranslateWatermarkProperty);

    public static void SetTranslateWatermark(TextBox textBox, bool value)
        => textBox.SetValue(TranslateWatermarkProperty, value);

    public static bool GetUseToolTipText(Control control)
        => control.GetValue(UseToolTipTextProperty);

    public static void SetUseToolTipText(Control control, bool value)
        => control.SetValue(UseToolTipTextProperty, value);

    public static string? GetToolTipPropertyName(Control control)
        => control.GetValue(ToolTipPropertyNameProperty);

    public static void SetToolTipPropertyName(Control control, string? value)
        => control.SetValue(ToolTipPropertyNameProperty, value);
}

/// <summary>Marks an Avalonia twin whose WinForms original does not participate in XLF translation.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
internal sealed class UntranslatedAttribute : Attribute;
