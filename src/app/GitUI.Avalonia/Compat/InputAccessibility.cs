using System.Runtime.CompilerServices;
using System.Text;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using GitExtensions.Extensibility.Translations;

namespace GitUI.Compat;

internal readonly record struct InputControlMetadata(
    string FieldName,
    int? TabIndex,
    bool? IsTabStop,
    string? AccessibleName);

internal static class InputAccessibility
{
    private static readonly object InitializedMarker = new();
    private static readonly ConditionalWeakTable<Control, object> InitializedHosts = new();

    internal static void Apply(Control host)
    {
        ArgumentNullException.ThrowIfNull(host);
        if (!InitializedHosts.TryAdd(host, InitializedMarker))
        {
            return;
        }

        Dictionary<string, Control> fields = TranslationUtils.GetObjFields(host, "$this")
            .Where(field => field.Item is Control)
            .GroupBy(field => field.Name, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => (Control)group.First().Item,
                StringComparer.Ordinal);
        if (WinFormsInputMetadata.ByType.TryGetValue(host.GetType().FullName ?? host.GetType().Name, out IReadOnlyList<InputControlMetadata>? metadata))
        {
            foreach (InputControlMetadata item in metadata)
            {
                Control? control = fields.GetValueOrDefault(item.FieldName)
                    ?? host.FindControl<Control>(item.FieldName);
                if (control is null)
                {
                    continue;
                }

                if (item.TabIndex is int tabIndex)
                {
                    KeyboardNavigation.SetTabIndex(control, tabIndex);
                }

                if (item.IsTabStop is bool isTabStop)
                {
                    KeyboardNavigation.SetIsTabStop(control, isTabStop);
                }

                if (!string.IsNullOrWhiteSpace(item.AccessibleName))
                {
                    AutomationProperties.SetName(control, item.AccessibleName);
                }
            }
        }

        foreach (Control control in EnumerateControls(host, fields))
        {
            ApplyAutomationProperties(control);
        }

        host.AddHandler(InputElement.KeyDownEvent, HandleContextMenuKey, RoutingStrategies.Bubble);
    }

    internal static bool IsActionable(Control control)
        => control is Button
            or MenuItem
            or TextBox
            or (SelectingItemsControl and not MenuBase)
            or NumericUpDown
            or Slider
            or SplitButton
            || control.Focusable;

    internal static IReadOnlyCollection<Control> EnumerateControls(Control host)
    {
        Dictionary<string, Control> fields = TranslationUtils.GetObjFields(host, "$this")
            .Where(field => field.Item is Control)
            .GroupBy(field => field.Name, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => (Control)group.First().Item,
                StringComparer.Ordinal);
        return EnumerateControls(host, fields);
    }

    private static IReadOnlyCollection<Control> EnumerateControls(
        Control host,
        IReadOnlyDictionary<string, Control> fields)
    {
        HashSet<Control> controls = [host];
        controls.UnionWith(host.GetLogicalDescendants().OfType<Control>());
        controls.UnionWith(fields.Values);
        return controls;
    }

    private static void ApplyAutomationProperties(Control control)
    {
        if (!string.IsNullOrWhiteSpace(control.Name))
        {
            AutomationProperties.SetAutomationId(control, control.Name);
        }

        string? text = GetText(control);
        if (string.IsNullOrWhiteSpace(AutomationProperties.GetName(control)) && IsActionable(control))
        {
            string name = !string.IsNullOrWhiteSpace(text)
                ? AvaloniaTranslationUtils.RemoveAvaloniaMnemonics(text)
                : GetToolTip(control) ?? HumanizeName(control.Name ?? control.GetType().Name);
            AutomationProperties.SetName(control, name);
        }

        if (string.IsNullOrWhiteSpace(AutomationProperties.GetHelpText(control))
            && GetToolTip(control) is string helpText)
        {
            AutomationProperties.SetHelpText(control, helpText);
        }

        if (string.IsNullOrWhiteSpace(AutomationProperties.GetAccessKey(control))
            && GetMnemonic(text) is char mnemonic)
        {
            AutomationProperties.SetAccessKey(control, $"Alt+{char.ToUpperInvariant(mnemonic)}");
        }
    }

    private static void HandleContextMenuKey(object? sender, KeyEventArgs e)
    {
        bool isContextMenuKey = e.Key == Key.Apps
            || (e.Key == Key.F10 && e.KeyModifiers == KeyModifiers.Shift);
        if (!isContextMenuKey || sender is not Control host)
        {
            return;
        }

        Control? focused = TopLevel.GetTopLevel(host)?.FocusManager?.GetFocusedElement() as Control;
        for (Control? control = focused; control is not null; control = control.GetLogicalParent() as Control)
        {
            if (control.ContextMenu is not ContextMenu contextMenu)
            {
                continue;
            }

            contextMenu.Open(control);
            e.Handled = true;
            return;
        }
    }

    private static string? GetText(Control control)
        => control switch
        {
            MenuItem menuItem => GetContentText(menuItem.Header),
            HeaderedContentControl headered => GetContentText(headered.Header),
            HeaderedSelectingItemsControl headered => GetContentText(headered.Header),
            HeaderedItemsControl headered => GetContentText(headered.Header),
            ContentControl contentControl => GetContentText(contentControl.Content),
            TextBox textBox when !string.IsNullOrWhiteSpace(textBox.PlaceholderText) => textBox.PlaceholderText,
            _ => null,
        };

    private static string? GetContentText(object? content)
        => content switch
        {
            string text => text,
            TextBlock textBlock => textBlock.Text,
            _ => null,
        };

    private static string? GetToolTip(Control control)
        => ToolTip.GetTip(control) switch
        {
            string text when !string.IsNullOrWhiteSpace(text) => text,
            TextBlock { Text: string text } when !string.IsNullOrWhiteSpace(text) => text,
            _ => null,
        };

    private static char? GetMnemonic(string? text)
    {
        if (text is null)
        {
            return null;
        }

        for (int i = 0; i < text.Length - 1; i++)
        {
            if (text[i] != '_')
            {
                continue;
            }

            if (text[i + 1] == '_')
            {
                i++;
                continue;
            }

            return text[i + 1];
        }

        return null;
    }

    private static string HumanizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Control";
        }

        string value = name.StartsWith("_NO_TRANSLATE_", StringComparison.Ordinal)
            ? name[14..]
            : name.TrimStart('_');
        string[] prefixes = ["toolStrip", "tsmi", "tsbtn", "tssbtn", "tsddbtn", "btn", "lbl", "txt", "cbx", "chk", "rb", "gbx", "lnk", "lst", "tv"];
        foreach (string prefix in prefixes)
        {
            if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && value.Length > prefix.Length)
            {
                value = value[prefix.Length..];
                break;
            }
        }

        StringBuilder result = new();
        foreach (char character in value)
        {
            if (result.Length > 0 && char.IsUpper(character) && !char.IsUpper(result[^1]))
            {
                result.Append(' ');
            }

            result.Append(character is '_' ? ' ' : character);
        }

        return result.ToString().Trim();
    }
}
