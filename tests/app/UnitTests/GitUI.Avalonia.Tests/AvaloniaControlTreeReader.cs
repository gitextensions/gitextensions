using System.Collections;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using GitExtensions.ParityCapture;

namespace GitExtensionsTests;

internal sealed class AvaloniaControlTreeReader
{
    private readonly Dictionary<object, List<string>> _fieldNames = new(ReferenceEqualityComparer.Instance);
    private readonly double _renderScale;
    private readonly Control _root;

    public AvaloniaControlTreeReader(Control root, double renderScale)
    {
        _root = root;
        _renderScale = renderScale;
        IndexFields(root);
    }

    public CaptureSurface ReadPrimary(Control root, PixelSize imageSize) =>
        new()
        {
            Role = "primary",
            ScreenBoundsPx = new CaptureRectangle { X = 0, Y = 0, Width = imageSize.Width, Height = imageSize.Height },
            Root = ReadControl(root, parentId: string.Empty, ordinal: 0)
        };

    private static string? BrushToArgb(object? value)
    {
        Color? color = value switch
        {
            Color directColor => directColor,
            ISolidColorBrush solidBrush => ApplyOpacity(solidBrush.Color, solidBrush.Opacity),
            _ => null
        };
        return color is { } resolved
            ? CaptureJson.FormatArgb(resolved.A, resolved.R, resolved.G, resolved.B)
            : null;

        static Color ApplyOpacity(Color color, double opacity)
        {
            byte alpha = (byte)Math.Clamp(
                (int)Math.Round(color.A * opacity, MidpointRounding.AwayFromZero),
                byte.MinValue,
                byte.MaxValue);
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }
    }

    private static CaptureColors EmptyColors() =>
        new()
        {
            Foreground = null,
            Background = null,
            Border = null,
            SelectionForeground = null,
            SelectionBackground = null,
            InactiveSelectionForeground = null,
            InactiveSelectionBackground = null,
            DisabledForeground = null,
            DisabledBackground = null,
            GridLine = null,
            Additional = new SortedDictionary<string, string>(StringComparer.Ordinal)
        };

    private static string GetControlKind(Control control) =>
        control switch
        {
            Window => "window",
            ToggleButton => "button",
            Button => "button",
            TextBox => "text",
            ComboBox => "comboBox",
            TreeView => "tree",
            ListBox => "list",
            Menu => "menu",
            MenuItem => "menuItem",
            TabControl => "tabs",
            GridSplitter => "split",
            _ when control.GetType().Name.Contains("DataGrid", StringComparison.Ordinal) => "dataGrid",
            _ => "control"
        };

    private static object? GetPropertyValue(object value, string name)
    {
        try
        {
            return value.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public)?.GetValue(value);
        }
        catch (TargetInvocationException)
        {
            return null;
        }
    }

    private static bool? GetNullableBoolProperty(object value, string name)
        => GetPropertyValue(value, name) is bool result ? result : null;

    private static string? GetText(Control control)
    {
        object? value = GetPropertyValue(control, "Text")
            ?? GetPropertyValue(control, "Content")
            ?? GetPropertyValue(control, "Header");
        return value as string;
    }

    private static string? GetAlignment(Control control) =>
        GetPropertyValue(control, "TextAlignment")?.ToString()
        ?? GetPropertyValue(control, "HorizontalContentAlignment")?.ToString();

    private static bool? GetSelected(Control control) =>
        control switch
        {
            ListBoxItem listItem => listItem.IsSelected,
            TreeViewItem treeItem => treeItem.IsSelected,
            SelectingItemsControl selectingItems => selectingItems.SelectedItem is not null,
            _ => null
        };

    private static bool? GetExpanded(Control control) =>
        control switch
        {
            TreeViewItem treeItem => treeItem.IsExpanded,
            Expander expander => expander.IsExpanded,
            MenuItem menuItem => menuItem.IsSubMenuOpen,
            _ => null
        };

    private void IndexFields(Control root)
    {
        foreach (Control owner in EnumerateLogicalControls(root))
        {
            Type type = owner.GetType();
            foreach (FieldInfo field in type.GetFields(
                         BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (field.Name.StartsWith('<'))
                {
                    continue;
                }

                object? value;
                try
                {
                    value = field.GetValue(owner);
                }
                catch (TargetInvocationException)
                {
                    continue;
                }

                if (value is not null
                    && !ReferenceEquals(value, owner)
                    && (value is Control || value.GetType().Name.Contains("Column", StringComparison.Ordinal)))
                {
                    if (!_fieldNames.TryGetValue(value, out List<string>? names))
                    {
                        names = [];
                        _fieldNames.Add(value, names);
                    }

                    if (!names.Contains(field.Name, StringComparer.Ordinal))
                    {
                        names.Add(field.Name);
                    }
                }
            }
        }
    }

    private static IEnumerable<Control> EnumerateLogicalControls(Control root)
    {
        yield return root;
        foreach (Control child in GetCaptureChildren(root))
        {
            foreach (Control descendant in EnumerateLogicalControls(child))
            {
                yield return descendant;
            }
        }
    }

    private CaptureNode ReadControl(Control control, string parentId, int ordinal)
    {
        IReadOnlyList<string> fieldNames = GetFieldNames(control);
        string? fieldName = ReferenceEquals(control, _root)
            ? null
            : fieldNames.FirstOrDefault()
              ?? (string.IsNullOrEmpty(control.Name) ? null : control.Name);
        string segment = ReferenceEquals(control, _root)
            ? $"$root:{control.GetType().Name}"
            : fieldName ?? $"$unnamed[{ordinal}]:{control.GetType().Name}";
        string id = string.IsNullOrEmpty(parentId) ? segment : $"{parentId}/{segment}";
        Rect bounds = control.Bounds;
        IReadOnlyList<CaptureNode> children = GetCaptureChildren(control)
            .Select((child, childOrdinal) => ReadControl(child, id, childOrdinal))
            .ToArray();

        return new CaptureNode
        {
            Id = id,
            FieldName = fieldName,
            FieldAliases = fieldNames.Skip(1).ToArray(),
            Name = string.IsNullOrEmpty(control.Name) ? null : control.Name,
            Type = control.GetType().FullName ?? control.GetType().Name,
            ControlKind = GetControlKind(control),
            BoundsPx = new CaptureRectangle
            {
                X = ToPixel(bounds.X),
                Y = ToPixel(bounds.Y),
                Width = ToPixel(bounds.Width),
                Height = ToPixel(bounds.Height)
            },
            BoundsDip = new CaptureRectangleF
            {
                X = ToDecimal(bounds.X),
                Y = ToDecimal(bounds.Y),
                Width = ToDecimal(bounds.Width),
                Height = ToDecimal(bounds.Height)
            },
            ClientSizePx = new CaptureSize { Width = ToPixel(bounds.Width), Height = ToPixel(bounds.Height) },
            ClientSizeDip = new CaptureSizeF { Width = ToDecimal(bounds.Width), Height = ToDecimal(bounds.Height) },
            Padding = ReadThicknessPair(GetPropertyValue(control, "Padding")),
            Margin = ReadThicknessPair(control.Margin),
            Font = ReadFont(control),
            Colors = ReadColors(control),
            BorderStyle = GetPropertyValue(control, "BorderStyle")?.ToString(),
            FlatStyle = null,
            BorderWidthDip = ReadBorderWidth(control),
            CornerRadiusDip = ReadCornerRadius(control),
            Anchor = [],
            Dock = null,
            AutoSize = null,
            Alignment = GetAlignment(control),
            Text = GetText(control),
            ToolTip = ToolTip.GetTip(control)?.ToString(),
            TranslationSource = fieldName,
            TabIndex = KeyboardNavigation.GetTabIndex(control),
            TabStop = control.Focusable,
            Enabled = control.IsEnabled,
            Visible = control.IsVisible,
            Focused = control.IsFocused,
            ReadOnly = GetNullableBoolProperty(control, "IsReadOnly"),
            CheckState = control is ToggleButton toggle
                ? toggle.IsChecked switch
                {
                    true => "Checked",
                    false => "Unchecked",
                    null => "Indeterminate"
                }
                : null,
            Selected = GetSelected(control),
            Expanded = GetExpanded(control),
            Columns = ReadColumns(control),
            Children = children
        };
    }

    private IReadOnlyList<string> GetFieldNames(object value) =>
        _fieldNames.TryGetValue(value, out List<string>? names)
            ? names
            : [];

    private static IEnumerable<Control> GetCaptureChildren(Control control)
    {
        if (control is Button or TextBox or ComboBox)
        {
            return [];
        }

        return control.GetLogicalChildren()
            .OfType<Control>()
            .Where(child => child.TemplatedParent is null
                            && child.GetType().Name != "TopLevelHost");
    }

    private CaptureColors ReadColors(Control control)
    {
        SortedDictionary<string, string> additional = new(StringComparer.Ordinal);
        AddAdditional("caret", GetPropertyValue(control, "CaretBrush"));
        AddAdditional("pointerOverBackground", GetPropertyValue(control, "PointerOverBackground"));
        AddAdditional("pressedBackground", GetPropertyValue(control, "PressedBackground"));
        if (ReferenceEquals(control, _root))
        {
            AddSemantic("semantic.app.panel.background", "GitExtensionsPanelBackgroundBrush");
            AddSemantic("semantic.app.selection.background", "GitExtensionsSelectionBackgroundBrush");
            AddSemantic("semantic.system.control.background", "GitExtensionsControlBackgroundBrush");
            AddSemantic("semantic.system.control.foreground", "GitExtensionsControlForegroundBrush");
            AddSemantic("semantic.system.control.disabledForeground", "GitExtensionsDisabledForegroundBrush");
            AddSemantic("semantic.system.highlight.background", "GitExtensionsHighlightBackgroundBrush");
            AddSemantic("semantic.system.highlight.foreground", "GitExtensionsHighlightForegroundBrush");
            AddSemantic("semantic.system.inactiveSelection.background", "GitExtensionsSystemInactiveSelectionBackgroundBrush");
            AddSemantic("semantic.system.inactiveSelection.foreground", "GitExtensionsInactiveSelectionForegroundBrush");
            AddSemantic("semantic.system.tooltip.background", "GitExtensionsToolTipBackgroundBrush");
            AddSemantic("semantic.system.tooltip.foreground", "GitExtensionsToolTipForegroundBrush");
            AddSemantic("semantic.system.window.background", "GitExtensionsWindowBackgroundBrush");
            AddSemantic("semantic.system.window.foreground", "GitExtensionsWindowTextBrush");
            AddSemantic("semantic.system.control.border", "GitExtensionsControlBorderBrush");
            AddSemantic("semantic.app.pane.border", "GitExtensionsPaneBorderBrush");
            AddSemantic("semantic.app.reset.soft.background", "GitExtensionsResetSoftBackgroundBrush");
            AddSemantic("semantic.app.reset.mixed.background", "GitExtensionsResetMixedBackgroundBrush");
            AddSemantic("semantic.app.reset.hard.background", "GitExtensionsResetHardBackgroundBrush");
        }

        return new CaptureColors
        {
            Foreground = BrushToArgb(GetPropertyValue(control, "Foreground")),
            Background = BrushToArgb(GetPropertyValue(control, "Background")),
            Border = BrushToArgb(GetPropertyValue(control, "BorderBrush")),
            SelectionForeground = BrushToArgb(
                GetPropertyValue(control, "SelectionForegroundBrush")
                ?? GetPropertyValue(control, "SelectionForeground")),
            SelectionBackground = BrushToArgb(
                GetPropertyValue(control, "SelectionBrush")
                ?? GetPropertyValue(control, "SelectionBackground")),
            InactiveSelectionForeground = BrushToArgb(GetPropertyValue(control, "InactiveSelectionForeground")),
            InactiveSelectionBackground = BrushToArgb(GetPropertyValue(control, "InactiveSelectionBackground")),
            DisabledForeground = BrushToArgb(GetPropertyValue(control, "DisabledForeground")),
            DisabledBackground = BrushToArgb(GetPropertyValue(control, "DisabledBackground")),
            GridLine = BrushToArgb(GetPropertyValue(control, "GridLinesBrush")),
            Additional = additional
        };

        void AddAdditional(string name, object? brush)
        {
            if (BrushToArgb(brush) is { } color)
            {
                additional[name] = color;
            }
        }

        void AddSemantic(string role, string resourceKey)
        {
            if (!_root.TryFindResource(resourceKey, _root.ActualThemeVariant, out object? resource)
                || BrushToArgb(resource) is not { } color)
            {
                throw new InvalidDataException($"Semantic color role '{role}' did not resolve from '{resourceKey}'.");
            }

            additional[role] = color;
        }
    }

    private CaptureFont? ReadFont(Control control)
    {
        if (GetPropertyValue(control, "FontFamily") is not FontFamily family
            || GetPropertyValue(control, "FontSize") is not double sizeDip)
        {
            return null;
        }

        List<string> styles = [];
        string fontStyle = GetPropertyValue(control, "FontStyle")?.ToString() ?? "Normal";
        string fontWeight = GetPropertyValue(control, "FontWeight")?.ToString() ?? "Normal";
        if (!fontStyle.Equals("Normal", StringComparison.Ordinal))
        {
            styles.Add(fontStyle);
        }

        if (!fontWeight.Equals("Normal", StringComparison.Ordinal))
        {
            styles.Add(fontWeight);
        }

        if (styles.Count == 0)
        {
            styles.Add("Regular");
        }

        return new CaptureFont
        {
            Family = family.Name,
            EmSize = ToDecimal(sizeDip),
            Unit = "Dip",
            SizePoints = ToDecimal(sizeDip * 72 / 96),
            SizeDip = ToDecimal(sizeDip),
            Style = styles
        };
    }

    private CaptureThicknessPair ReadThicknessPair(object? value)
    {
        Thickness thickness = value is Thickness actual ? actual : default;
        return new CaptureThicknessPair
        {
            Px = new CaptureThickness
            {
                Left = ToPixel(thickness.Left),
                Top = ToPixel(thickness.Top),
                Right = ToPixel(thickness.Right),
                Bottom = ToPixel(thickness.Bottom)
            },
            Dip = new CaptureThicknessF
            {
                Left = ToDecimal(thickness.Left),
                Top = ToDecimal(thickness.Top),
                Right = ToDecimal(thickness.Right),
                Bottom = ToDecimal(thickness.Bottom)
            }
        };
    }

    private decimal? ReadBorderWidth(Control control)
    {
        if (GetPropertyValue(control, "BorderThickness") is not Thickness thickness)
        {
            return null;
        }

        return ToDecimal(Math.Max(Math.Max(thickness.Left, thickness.Top), Math.Max(thickness.Right, thickness.Bottom)));
    }

    private CaptureCornerRadius? ReadCornerRadius(Control control)
    {
        if (GetPropertyValue(control, "CornerRadius") is not CornerRadius radius)
        {
            return null;
        }

        return new CaptureCornerRadius
        {
            TopLeft = ToDecimal(radius.TopLeft),
            TopRight = ToDecimal(radius.TopRight),
            BottomRight = ToDecimal(radius.BottomRight),
            BottomLeft = ToDecimal(radius.BottomLeft)
        };
    }

    private IReadOnlyList<CaptureColumn> ReadColumns(Control control)
    {
        if (GetPropertyValue(control, "Columns") is not IEnumerable columns)
        {
            return [];
        }

        return columns.Cast<object>()
            .Select((column, index) =>
            {
                double widthDip = GetPropertyValue(column, "ActualWidth") as double?
                    ?? GetPropertyValue(GetPropertyValue(column, "Width")!, "Value") as double?
                    ?? 0;
                return new CaptureColumn
                {
                    FieldName = GetFieldNames(column).FirstOrDefault(),
                    Name = GetPropertyValue(column, "Name") as string,
                    Type = column.GetType().FullName ?? column.GetType().Name,
                    Index = index,
                    DisplayIndex = GetPropertyValue(column, "DisplayIndex") as int? ?? index,
                    WidthPx = ToPixel(widthDip),
                    WidthDip = ToDecimal(widthDip),
                    Visible = GetNullableBoolProperty(column, "IsVisible") ?? widthDip > 0,
                    Resizable = GetNullableBoolProperty(column, "CanUserResize"),
                    SortMode = GetPropertyValue(column, "SortMemberPath")?.ToString(),
                    Alignment = GetPropertyValue(column, "HorizontalContentAlignment")?.ToString(),
                    HeaderText = GetPropertyValue(column, "Header")?.ToString(),
                    HeaderAlignment = GetPropertyValue(column, "HorizontalHeaderContentAlignment")?.ToString(),
                    Colors = EmptyColors()
                };
            })
            .ToArray();
    }

    private decimal ToDecimal(double value) => decimal.Round((decimal)value, 4);

    private int ToPixel(double value) =>
        checked((int)Math.Round(value * _renderScale, MidpointRounding.AwayFromZero));
}
