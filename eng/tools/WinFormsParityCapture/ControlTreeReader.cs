using System.Collections;
using System.Drawing.Drawing2D;
using System.Reflection;
using GitExtensions.ParityCapture;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;

namespace WinFormsParityCapture;

internal sealed class ControlTreeReader
{
    private readonly Dictionary<object, List<string>> _fieldNames = new(ReferenceEqualityComparer.Instance);
    private readonly List<ToolTip> _toolTips = [];
    private readonly decimal _dipFactor;
    private readonly Control _root;

    public ControlTreeReader(Control root, int dpi)
    {
        _root = root;
        _dipFactor = 96m / dpi;
        IndexFields(root);
    }

    public CaptureSurface ReadPrimary(Control root, Rectangle screenBounds) =>
        new()
        {
            Role = "primary",
            ScreenBoundsPx = ToRectangle(screenBounds),
            Root = ReadControl(root, parentId: string.Empty, ordinal: 0)
        };

    public CaptureSurface ReadPopup(ToolStripDropDown popup, int ordinal) =>
        new()
        {
            Role = $"popup:{ordinal}",
            ScreenBoundsPx = ToRectangle(popup.Bounds),
            Root = ReadToolStripItemCollection(popup, $"popup:{ordinal}")
        };

    private static string? ColorToArgb(Color color) =>
        color.IsEmpty
            ? null
            : CaptureJson.FormatArgb(color.A, color.R, color.G, color.B);

    private static string GetControlKind(object value) =>
        value switch
        {
            Form => "window",
            ButtonBase => "button",
            TextBoxBase => "text",
            ComboBox => "comboBox",
            DataGridView => "dataGrid",
            ListView => "list",
            TreeView => "tree",
            MenuStrip => "menu",
            ToolStrip => "toolStrip",
            TabControl => "tabs",
            SplitContainer => "split",
            ToolStripItem => "menuItem",
            _ => "control"
        };

    private static IReadOnlyList<string> GetStyles(FontStyle style)
    {
        List<string> styles = [];
        foreach (FontStyle candidate in Enum.GetValues<FontStyle>())
        {
            if (candidate != FontStyle.Regular && style.HasFlag(candidate))
            {
                styles.Add(candidate.ToString());
            }
        }

        if (styles.Count == 0)
        {
            styles.Add(FontStyle.Regular.ToString());
        }

        return styles;
    }

    private static CaptureRectangle ToRectangle(Rectangle value) =>
        new() { X = value.X, Y = value.Y, Width = value.Width, Height = value.Height };

    private CaptureThicknessPair CreateThickness(Padding value) =>
        new()
        {
            Px = new CaptureThickness { Left = value.Left, Top = value.Top, Right = value.Right, Bottom = value.Bottom },
            Dip = new CaptureThicknessF
            {
                Left = ToDip(value.Left),
                Top = ToDip(value.Top),
                Right = ToDip(value.Right),
                Bottom = ToDip(value.Bottom)
            }
        };

    private CaptureColors GetColors(Control control)
    {
        string? selectionForeground = null;
        string? selectionBackground = null;
        string? gridLine = null;
        string? border = null;
        SortedDictionary<string, string> additional = new(StringComparer.Ordinal);

        if (control is DataGridView grid)
        {
            selectionForeground = ColorToArgb(grid.DefaultCellStyle.SelectionForeColor);
            selectionBackground = ColorToArgb(grid.DefaultCellStyle.SelectionBackColor);
            gridLine = ColorToArgb(grid.GridColor);
            border = ColorToArgb(grid.RowHeadersDefaultCellStyle.BackColor);
        }
        else if (control is ListView or TreeView or ListBox)
        {
            selectionForeground = ColorToArgb(SystemColors.HighlightText);
            selectionBackground = ColorToArgb(SystemColors.Highlight);
            additional["hotTrack"] = ColorToArgb(SystemColors.HotTrack)!;
        }
        else if (control is ButtonBase button && !button.FlatAppearance.BorderColor.IsEmpty)
        {
            border = ColorToArgb(button.FlatAppearance.BorderColor);
        }

        if (ReferenceEquals(control, _root))
        {
            AddSemanticColorRoles(additional);
        }

        return new CaptureColors
        {
            Foreground = ColorToArgb(control.ForeColor),
            Background = ColorToArgb(control.BackColor),
            Border = border,
            SelectionForeground = selectionForeground,
            SelectionBackground = selectionBackground,
            InactiveSelectionForeground = selectionForeground,
            InactiveSelectionBackground = selectionBackground is null ? null : ColorToArgb(SystemColors.InactiveCaption),
            DisabledForeground = ColorToArgb(SystemColors.GrayText),
            DisabledBackground = ColorToArgb(control.BackColor),
            GridLine = gridLine,
            Additional = additional
        };
    }

    private static void AddSemanticColorRoles(IDictionary<string, string> colors)
    {
        AddApp("semantic.app.panel.background", AppColor.PanelBackground);
        AddApp("semantic.app.selection.background", AppColor.Selection);
        AddSystem("semantic.system.control.background", KnownColor.Control);
        AddSystem("semantic.system.control.foreground", KnownColor.ControlText);
        AddSystem("semantic.system.control.disabledForeground", KnownColor.GrayText);
        AddSystem("semantic.system.highlight.background", KnownColor.Highlight);
        AddSystem("semantic.system.highlight.foreground", KnownColor.HighlightText);
        AddSystem("semantic.system.inactiveSelection.background", KnownColor.InactiveCaption);
        AddSystem("semantic.system.inactiveSelection.foreground", KnownColor.InactiveCaptionText);
        AddSystem("semantic.system.tooltip.background", KnownColor.Info);
        AddSystem("semantic.system.tooltip.foreground", KnownColor.InfoText);
        AddSystem("semantic.system.window.background", KnownColor.Window);
        AddSystem("semantic.system.window.foreground", KnownColor.WindowText);
        AddSystem("semantic.system.control.border", KnownColor.ControlDark);
        AddColor("semantic.app.pane.border", OtherColors.PanelBorderColor);
        AddColor("semantic.app.reset.soft.background", OtherColors.BrightGreen);
        AddColor("semantic.app.reset.mixed.background", OtherColors.BrightYellow);
        AddColor("semantic.app.reset.hard.background", OtherColors.BrightRed);

        void AddApp(string role, AppColor name)
        {
            Color color = ThemeModule.Settings.Theme.GetColor(name);
            AddColor(role, color.IsEmpty ? ThemeModule.Settings.InvariantTheme.GetColor(name) : color);
        }

        void AddSystem(string role, KnownColor name)
        {
            AddColor(role, ResolveSystemColor(name));
        }

        void AddColor(string role, Color color)
        {
            colors[role] = ColorToArgb(color)
                ?? throw new InvalidDataException($"Semantic color role '{role}' did not resolve to ARGB.");
        }
    }

    private static Color ResolveSystemColor(KnownColor name)
    {
        Color color = ThemeModule.Settings.Theme.GetColor(name);
        if (!color.IsEmpty)
        {
            return color;
        }

        if (ThemeModule.Settings.Theme.SystemColorMode == SystemColorMode.Dark
            && TryGetDarkSystemColor(name, out color))
        {
            return color;
        }

        color = ThemeModule.Settings.InvariantTheme.GetColor(name);
        return color.IsEmpty ? Color.FromKnownColor(name) : color;
    }

    private static bool TryGetDarkSystemColor(KnownColor name, out Color color)
    {
        string? value = name switch
        {
            KnownColor.Control => "#202020",
            KnownColor.ControlDark => "#4A4A4A",
            KnownColor.ControlText => "#FFFFFF",
            KnownColor.GrayText => "#969696",
            KnownColor.Highlight => "#2864B4",
            KnownColor.HighlightText => "#000000",
            KnownColor.InactiveCaption => "#374B5A",
            KnownColor.InactiveCaptionText => "#BEBEBE",
            KnownColor.Info => "#50503C",
            KnownColor.InfoText => "#BEBEBE",
            KnownColor.Window => "#323232",
            KnownColor.WindowText => "#F0F0F0",
            _ => null,
        };

        color = value is null ? Color.Empty : ColorTranslator.FromHtml(value);
        return !color.IsEmpty;
    }

    // parity-scaffolding: Exposes deterministic semantic-color resolution to the capture-tool tests.
    internal readonly struct TestAccessor
    {
        internal static Color ResolveSystemColor(KnownColor name, bool isDark)
        {
            if (isDark && TryGetDarkSystemColor(name, out Color color))
            {
                return color;
            }

            return Color.FromKnownColor(name);
        }
    }

    private CaptureColors GetColors(DataGridViewCellStyle style) =>
        new()
        {
            Foreground = ColorToArgb(style.ForeColor),
            Background = ColorToArgb(style.BackColor),
            Border = null,
            SelectionForeground = ColorToArgb(style.SelectionForeColor),
            SelectionBackground = ColorToArgb(style.SelectionBackColor),
            InactiveSelectionForeground = ColorToArgb(style.SelectionForeColor),
            InactiveSelectionBackground = ColorToArgb(SystemColors.InactiveCaption),
            DisabledForeground = ColorToArgb(SystemColors.GrayText),
            DisabledBackground = ColorToArgb(style.BackColor),
            GridLine = null,
            Additional = new SortedDictionary<string, string>(StringComparer.Ordinal)
        };

    private CaptureColors GetColors(ToolStripItem item) =>
        new()
        {
            Foreground = ColorToArgb(item.ForeColor),
            Background = ColorToArgb(item.BackColor),
            Border = null,
            SelectionForeground = ColorToArgb(SystemColors.HighlightText),
            SelectionBackground = ColorToArgb(SystemColors.Highlight),
            InactiveSelectionForeground = ColorToArgb(SystemColors.MenuText),
            InactiveSelectionBackground = ColorToArgb(SystemColors.Menu),
            DisabledForeground = ColorToArgb(SystemColors.GrayText),
            DisabledBackground = ColorToArgb(item.BackColor),
            GridLine = null,
            Additional = new SortedDictionary<string, string>(StringComparer.Ordinal)
        };

    private IReadOnlyList<CaptureColumn> GetColumns(Control control)
    {
        if (control is DataGridView grid)
        {
            return grid.Columns.Cast<DataGridViewColumn>()
                .Select(column => new CaptureColumn
                {
                    FieldName = GetFieldNames(column).FirstOrDefault(),
                    Name = column.Name,
                    Type = column.GetType().FullName ?? column.GetType().Name,
                    Index = column.Index,
                    DisplayIndex = column.DisplayIndex,
                    WidthPx = column.Width,
                    WidthDip = ToDip(column.Width),
                    Visible = column.Visible,
                    Resizable = column.Resizable == DataGridViewTriState.True,
                    SortMode = column.SortMode.ToString(),
                    Alignment = column.DefaultCellStyle.Alignment.ToString(),
                    HeaderText = column.HeaderText,
                    HeaderAlignment = column.HeaderCell.Style.Alignment.ToString(),
                    Colors = GetColors(column.DefaultCellStyle)
                })
                .ToArray();
        }

        if (control is ListView list)
        {
            return list.Columns.Cast<ColumnHeader>()
                .Select((column, index) => new CaptureColumn
                {
                    FieldName = GetFieldNames(column).FirstOrDefault(),
                    Name = column.Name,
                    Type = column.GetType().FullName ?? column.GetType().Name,
                    Index = index,
                    DisplayIndex = column.DisplayIndex,
                    WidthPx = column.Width,
                    WidthDip = ToDip(column.Width),
                    Visible = column.Width > 0,
                    Resizable = true,
                    SortMode = null,
                    Alignment = column.TextAlign.ToString(),
                    HeaderText = column.Text,
                    HeaderAlignment = column.TextAlign.ToString(),
                    Colors = GetColors(list)
                })
                .ToArray();
        }

        return [];
    }

    private IReadOnlyList<string> GetFieldNames(object value) =>
        _fieldNames.TryGetValue(value, out List<string>? names)
            ? names
            : [];

    private string? GetToolTip(Control control)
    {
        foreach (ToolTip toolTip in _toolTips)
        {
            string? text = toolTip.GetToolTip(control);
            if (!string.IsNullOrEmpty(text))
            {
                return text;
            }
        }

        return null;
    }

    private void IndexFields(object owner)
    {
        HashSet<object> visited = new(ReferenceEqualityComparer.Instance);
        Queue<object> queue = new();
        queue.Enqueue(owner);

        while (queue.Count > 0)
        {
            object current = queue.Dequeue();
            if (!visited.Add(current))
            {
                continue;
            }

            Type type = current.GetType();
            foreach (FieldInfo field in type.GetFields(
                         BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                object? value;
                try
                {
                    value = field.GetValue(current);
                }
                catch (TargetInvocationException)
                {
                    continue;
                }

                if (value is null || value is string || value.GetType().IsValueType || ReferenceEquals(value, current))
                {
                    continue;
                }

                if (value is ToolTip toolTip && !_toolTips.Contains(toolTip))
                {
                    _toolTips.Add(toolTip);
                }

                if (value is Control or ToolStripItem or DataGridViewColumn or ColumnHeader)
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

            if (current is Control control)
            {
                foreach (Control child in control.Controls)
                {
                    queue.Enqueue(child);
                }

                if (control is ToolStrip toolStrip)
                {
                    foreach (ToolStripItem item in toolStrip.Items)
                    {
                        queue.Enqueue(item);
                    }
                }
            }
            else if (current is ToolStripDropDownItem dropDownItem)
            {
                foreach (ToolStripItem item in dropDownItem.DropDownItems)
                {
                    queue.Enqueue(item);
                }
            }
        }
    }

    private CaptureNode ReadControl(Control control, string parentId, int ordinal)
    {
        IReadOnlyList<string> names = GetFieldNames(control);
        string segment = names.FirstOrDefault()
            ?? (!string.IsNullOrEmpty(control.Name) ? control.Name : $"$unnamed[{ordinal}]:{control.GetType().Name}");
        string id = string.IsNullOrEmpty(parentId) ? segment : $"{parentId}/{segment}";

        List<CaptureNode> children = [];
        int childOrdinal = 0;
        foreach (Control child in control.Controls)
        {
            children.Add(ReadControl(child, id, childOrdinal++));
        }

        if (control is ToolStrip toolStrip)
        {
            foreach (ToolStripItem item in toolStrip.Items)
            {
                children.Add(ReadToolStripItem(item, id, childOrdinal++));
            }
        }

        Rectangle bounds = control.Bounds;
        Size clientSize = control.ClientSize;
        return new CaptureNode
        {
            Id = id,
            FieldName = names.FirstOrDefault(),
            FieldAliases = names.Skip(1).ToArray(),
            Name = string.IsNullOrEmpty(control.Name) ? null : control.Name,
            Type = control.GetType().FullName ?? control.GetType().Name,
            ControlKind = GetControlKind(control),
            BoundsPx = ToRectangle(bounds),
            BoundsDip = new CaptureRectangleF
            {
                X = ToDip(bounds.X),
                Y = ToDip(bounds.Y),
                Width = ToDip(bounds.Width),
                Height = ToDip(bounds.Height)
            },
            ClientSizePx = new CaptureSize { Width = clientSize.Width, Height = clientSize.Height },
            ClientSizeDip = new CaptureSizeF { Width = ToDip(clientSize.Width), Height = ToDip(clientSize.Height) },
            Padding = CreateThickness(control.Padding),
            Margin = CreateThickness(control.Margin),
            Font = ReadFont(control.Font),
            Colors = GetColors(control),
            BorderStyle = GetPropertyValue(control, "BorderStyle"),
            FlatStyle = control is ButtonBase button ? button.FlatStyle.ToString() : null,
            BorderWidthDip = null,
            CornerRadiusDip = null,
            Anchor = control.Anchor.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            Dock = control.Dock.ToString(),
            AutoSize = control.AutoSize,
            Alignment = GetPropertyValue(control, "TextAlign") ?? GetPropertyValue(control, "ContentAlignment"),
            Text = control.Text,
            ToolTip = GetToolTip(control),
            TranslationSource = names.FirstOrDefault(),
            TabIndex = control.TabIndex,
            TabStop = control.TabStop,
            Enabled = control.Enabled,
            Visible = control.Visible,
            Focused = control.Focused,
            ReadOnly = GetNullableBoolProperty(control, "ReadOnly"),
            CheckState = control is CheckBox checkBox ? checkBox.CheckState.ToString() : null,
            Selected = control is ListControl listControl ? listControl.SelectedValue is not null : null,
            Expanded = control is TreeView treeView && treeView.Nodes.Cast<TreeNode>().Any(node => node.IsExpanded),
            Columns = GetColumns(control),
            Children = children
        };
    }

    private CaptureFont ReadFont(Font font)
    {
        float sizePoints = font.Unit == GraphicsUnit.Point
            ? font.Size
            : font.SizeInPoints;
        return new CaptureFont
        {
            Family = font.FontFamily.Name,
            EmSize = decimal.Round((decimal)font.Size, 4),
            Unit = font.Unit.ToString(),
            SizePoints = decimal.Round((decimal)sizePoints, 4),
            SizeDip = decimal.Round((decimal)sizePoints * 96m / 72m, 4),
            Style = GetStyles(font.Style)
        };
    }

    private CaptureNode ReadToolStripItem(ToolStripItem item, string parentId, int ordinal)
    {
        IReadOnlyList<string> names = GetFieldNames(item);
        string segment = names.FirstOrDefault()
            ?? (!string.IsNullOrEmpty(item.Name) ? item.Name : $"$unnamed[{ordinal}]:{item.GetType().Name}");
        string id = $"{parentId}/{segment}";
        IReadOnlyList<CaptureNode> children = item is ToolStripDropDownItem dropDownItem
            ? dropDownItem.DropDownItems.Cast<ToolStripItem>().Select((child, index) => ReadToolStripItem(child, id, index)).ToArray()
            : [];
        Rectangle bounds = item.Bounds;
        return new CaptureNode
        {
            Id = id,
            FieldName = names.FirstOrDefault(),
            FieldAliases = names.Skip(1).ToArray(),
            Name = string.IsNullOrEmpty(item.Name) ? null : item.Name,
            Type = item.GetType().FullName ?? item.GetType().Name,
            ControlKind = GetControlKind(item),
            BoundsPx = ToRectangle(bounds),
            BoundsDip = new CaptureRectangleF { X = ToDip(bounds.X), Y = ToDip(bounds.Y), Width = ToDip(bounds.Width), Height = ToDip(bounds.Height) },
            ClientSizePx = new CaptureSize { Width = bounds.Width, Height = bounds.Height },
            ClientSizeDip = new CaptureSizeF { Width = ToDip(bounds.Width), Height = ToDip(bounds.Height) },
            Padding = CreateThickness(item.Padding),
            Margin = CreateThickness(item.Margin),
            Font = ReadFont(item.Font),
            Colors = GetColors(item),
            BorderStyle = null,
            FlatStyle = null,
            BorderWidthDip = null,
            CornerRadiusDip = null,
            Anchor = [],
            Dock = null,
            AutoSize = item.AutoSize,
            Alignment = item.TextAlign.ToString(),
            Text = item.Text,
            ToolTip = item.ToolTipText,
            TranslationSource = names.FirstOrDefault(),
            TabIndex = null,
            TabStop = null,
            Enabled = item.Enabled,
            Visible = item.Visible,
            Focused = item.Selected,
            ReadOnly = null,
            CheckState = item is ToolStripMenuItem menuItem ? menuItem.CheckState.ToString() : null,
            Selected = item.Selected,
            Expanded = item is ToolStripDropDownItem { DropDown.Visible: true },
            Columns = [],
            Children = children
        };
    }

    private CaptureNode ReadToolStripItemCollection(ToolStrip popup, string id)
    {
        Rectangle bounds = popup.Bounds;
        return new CaptureNode
        {
            Id = id,
            FieldName = null,
            FieldAliases = [],
            Name = popup.Name,
            Type = popup.GetType().FullName ?? popup.GetType().Name,
            ControlKind = "popup",
            BoundsPx = ToRectangle(bounds),
            BoundsDip = new CaptureRectangleF { X = ToDip(bounds.X), Y = ToDip(bounds.Y), Width = ToDip(bounds.Width), Height = ToDip(bounds.Height) },
            ClientSizePx = new CaptureSize { Width = bounds.Width, Height = bounds.Height },
            ClientSizeDip = new CaptureSizeF { Width = ToDip(bounds.Width), Height = ToDip(bounds.Height) },
            Padding = CreateThickness(popup.Padding),
            Margin = CreateThickness(popup.Margin),
            Font = ReadFont(popup.Font),
            Colors = GetColors(popup),
            BorderStyle = null,
            FlatStyle = null,
            BorderWidthDip = null,
            CornerRadiusDip = null,
            Anchor = [],
            Dock = null,
            AutoSize = popup.AutoSize,
            Alignment = null,
            Text = null,
            ToolTip = null,
            TranslationSource = null,
            TabIndex = null,
            TabStop = null,
            Enabled = popup.Enabled,
            Visible = popup.Visible,
            Focused = popup.Focused,
            ReadOnly = null,
            CheckState = null,
            Selected = null,
            Expanded = true,
            Columns = [],
            Children = popup.Items.Cast<ToolStripItem>().Select((item, ordinal) => ReadToolStripItem(item, id, ordinal)).ToArray()
        };
    }

    private decimal ToDip(int value) => decimal.Round(value * _dipFactor, 4);

    private static bool? GetNullableBoolProperty(object value, string name)
    {
        PropertyInfo? property = value.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
        return property?.PropertyType == typeof(bool) ? (bool?)property.GetValue(value) : null;
    }

    private static string? GetPropertyValue(object value, string name)
    {
        PropertyInfo? property = value.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
        object? propertyValue = property?.GetValue(value);
        return propertyValue?.ToString();
    }
}
