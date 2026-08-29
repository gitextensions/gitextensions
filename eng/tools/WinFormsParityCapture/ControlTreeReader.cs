using System.Collections;
using System.Drawing.Drawing2D;
using System.Reflection;
using GitExtensions.ParityCapture;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using GitUI.UserControls.RevisionGrid;

namespace WinFormsParityCapture;

internal sealed class ControlTreeReader
{
    private readonly Dictionary<object, List<string>> _fieldNames = new(ReferenceEqualityComparer.Instance);
    private readonly IReadOnlyDictionary<object, FontBaseline> _fontBaselines;
    private readonly List<ToolTip> _toolTips = [];
    private readonly decimal _dipFactor;
    private readonly Control _root;

    public ControlTreeReader(
        Control root,
        int dpi,
        IReadOnlyDictionary<object, FontBaseline>? fontBaselines = null)
    {
        _root = root;
        _dipFactor = 96m / dpi;
        _fontBaselines = fontBaselines
            ?? new Dictionary<object, FontBaseline>(ReferenceEqualityComparer.Instance);
        IndexFields(root);
    }

    internal static IReadOnlyDictionary<object, FontBaseline> CaptureFontBaselines(Control root)
    {
        Dictionary<object, FontBaseline> baselines = new(ReferenceEqualityComparer.Instance);

        void IndexToolStripItem(ToolStripItem item)
        {
            baselines[item] = FontBaseline.From(item.Font);
            if (item is ToolStripDropDownItem dropDownItem)
            {
                foreach (ToolStripItem child in dropDownItem.DropDownItems)
                {
                    IndexToolStripItem(child);
                }
            }
        }

        void IndexControl(Control control)
        {
            baselines[control] = FontBaseline.From(control.Font);
            if (control is ToolStrip toolStrip)
            {
                foreach (ToolStripItem item in toolStrip.Items)
                {
                    IndexToolStripItem(item);
                }
            }

            foreach (Control child in control.Controls)
            {
                IndexControl(child);
            }
        }

        IndexControl(root);
        return baselines;
    }

    public CaptureSurface ReadPrimary(Control root, Rectangle screenBounds)
    {
        CaptureNode rootNode = ReadControl(root, parentId: string.Empty, ordinal: 0);
        Rectangle clientScreenBounds = root.RectangleToScreen(root.ClientRectangle);
        Rectangle clientSurfaceBounds = new(
            clientScreenBounds.X - screenBounds.X,
            clientScreenBounds.Y - screenBounds.Y,
            clientScreenBounds.Width,
            clientScreenBounds.Height);

        // parity-scaffolding: PrintWindow keeps native non-client chrome in the bitmap, while
        // the root control tree describes the product client area. Store its surface-relative
        // inset so the cross-platform comparer can align client pixels without bitmap scaling.
        rootNode = rootNode with
        {
            BoundsPx = ToRectangle(clientSurfaceBounds),
            BoundsDip = new CaptureRectangleF
            {
                X = ToDip(clientSurfaceBounds.X),
                Y = ToDip(clientSurfaceBounds.Y),
                Width = ToDip(clientSurfaceBounds.Width),
                Height = ToDip(clientSurfaceBounds.Height)
            }
        };

        return new CaptureSurface
        {
            Role = "primary",
            ScreenBoundsPx = ToRectangle(screenBounds),
            Root = rootNode
        };
    }

    public CaptureSurface ReadPopup(ToolStripDropDown popup, int ordinal, Point? primaryScreenOrigin = null) =>
        new()
        {
            Role = $"popup:{ordinal}",
            ScreenBoundsPx = ToRectangle(popup.Bounds),
            Root = ReadToolStripItemCollection(popup, $"popup:{ordinal}", primaryScreenOrigin ?? Point.Empty)
        };

    public CaptureSurface ReadComboBoxPopup(ComboBoxPopup popup, int ordinal, Point primaryScreenOrigin) =>
        new()
        {
            Role = $"popup:{ordinal}",
            ScreenBoundsPx = ToRectangle(popup.Bounds),
            Root = ReadComboBoxItemCollection(popup, $"popup:{ordinal}", primaryScreenOrigin)
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
        Color resolvedBackground = control.BackColor;
        SortedDictionary<string, string> additional = new(StringComparer.Ordinal);

        if (control is DataGridView grid)
        {
            bool isRevisionGrid = grid is RevisionDataGridView;
            resolvedBackground = isRevisionGrid
                ? AppColor.PanelBackground.GetThemeColor()
                : grid.BackgroundColor;
            selectionForeground = ColorToArgb(grid.DefaultCellStyle.SelectionForeColor);
            selectionBackground = ColorToArgb(grid.DefaultCellStyle.SelectionBackColor);
            gridLine = ColorToArgb(grid.GridColor);
            border = ColorToArgb(isRevisionGrid ? resolvedBackground : grid.RowHeadersDefaultCellStyle.BackColor);
        }
        else if (control is ListView or TreeView or ListBox)
        {
            selectionForeground = ColorToArgb(ResolveSystemColor(KnownColor.HighlightText));
            selectionBackground = ColorToArgb(ResolveSystemColor(KnownColor.Highlight));
            additional["hotTrack"] = ColorToArgb(ResolveSystemColor(KnownColor.HotTrack))!;
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
            Background = ColorToArgb(resolvedBackground),
            Border = border,
            SelectionForeground = selectionForeground,
            SelectionBackground = selectionBackground,
            InactiveSelectionForeground = selectionForeground,
            InactiveSelectionBackground = selectionBackground is null
                ? null
                : ColorToArgb(ResolveSystemColor(KnownColor.InactiveCaption)),
            DisabledForeground = ColorToArgb(ResolveSystemColor(KnownColor.GrayText)),
            DisabledBackground = ColorToArgb(resolvedBackground),
            GridLine = gridLine,
            Additional = additional
        };
    }

    private static void AddSemanticColorRoles(IDictionary<string, string> colors)
    {
        AddApp("semantic.app.panel.background", AppColor.PanelBackground);
        AddColor(
            "semantic.app.revision.alternating.background",
            ResolveAppColor(AppColor.PanelBackground).MakeDarkerBy(
                ThemeModule.Settings.Theme.SystemColorMode == SystemColorMode.Dark ? -0.018 : 0.025));
        AddApp("semantic.app.revision.authored.background", AppColor.AuthoredHighlight);
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
            AddColor(role, ResolveAppColor(name));
        }

        Color ResolveAppColor(AppColor name)
        {
            Color color = ThemeModule.Settings.Theme.GetColor(name);
            return color.IsEmpty ? ThemeModule.Settings.InvariantTheme.GetColor(name) : color;
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
        bool isDark = ThemeModule.Settings.Theme.SystemColorMode == SystemColorMode.Dark;
        Color color = ThemeModule.Settings.Theme.Id == ThemeId.DefaultLight
            ? Color.Empty
            : ThemeModule.Settings.Theme.GetColor(name);
        if (!color.IsEmpty)
        {
            return color;
        }

        if (isDark && TryGetDarkSystemColor(name, out color))
        {
            return color;
        }

        color = ThemeModule.Settings.InvariantTheme.GetColor(name);
        if (!color.IsEmpty)
        {
            return color;
        }

        // Keep the reference tree independent of the live Windows accent palette and use
        // the same invariant.css fallbacks published by the Avalonia theme boundary.
        return TryGetLightSystemColor(name, out color) ? color : Color.FromKnownColor(name);
    }

    private static bool TryGetLightSystemColor(KnownColor name, out Color color)
    {
        string? value = name switch
        {
            KnownColor.ActiveBorder => "#B4B4B4",
            KnownColor.ActiveCaption => "#99B4D1",
            KnownColor.ActiveCaptionText => "#000000",
            KnownColor.AppWorkspace => "#ABABAB",
            KnownColor.ButtonFace => "#F0F0F0",
            KnownColor.ButtonHighlight => "#FFFFFF",
            KnownColor.ButtonShadow => "#A0A0A0",
            KnownColor.Control => "#F0F0F0",
            KnownColor.ControlDark => "#A0A0A0",
            KnownColor.ControlDarkDark => "#696969",
            KnownColor.ControlLight => "#E3E3E3",
            KnownColor.ControlLightLight => "#FFFFFF",
            KnownColor.ControlText => "#000000",
            KnownColor.Desktop => "#000000",
            KnownColor.GradientActiveCaption => "#B9D1EA",
            KnownColor.GradientInactiveCaption => "#D7E4F2",
            KnownColor.GrayText => "#6D6D6D",
            KnownColor.Highlight => "#0078D7",
            KnownColor.HighlightText => "#FFFFFF",
            KnownColor.HotTrack => "#0066CC",
            KnownColor.InactiveBorder => "#F4F7FC",
            KnownColor.InactiveCaption => "#BFCDDB",
            KnownColor.InactiveCaptionText => "#000000",
            KnownColor.Info => "#FFFFE1",
            KnownColor.InfoText => "#000000",
            KnownColor.Menu => "#F0F0F0",
            KnownColor.MenuBar => "#F0F0F0",
            KnownColor.MenuHighlight => "#0078D7",
            KnownColor.MenuText => "#000000",
            KnownColor.ScrollBar => "#C8C8C8",
            KnownColor.Window => "#FFFFFF",
            KnownColor.WindowFrame => "#646464",
            KnownColor.WindowText => "#000000",
            _ => null,
        };

        color = value is null ? Color.Empty : ColorTranslator.FromHtml(value);
        return !color.IsEmpty;
    }

    private static bool TryGetDarkSystemColor(KnownColor name, out Color color)
    {
        string? value = name switch
        {
            KnownColor.ActiveBorder => "#464646",
            KnownColor.ActiveCaption => "#3C5F78",
            KnownColor.ActiveCaptionText => "#FFFFFF",
            KnownColor.AppWorkspace => "#3C3C3C",
            KnownColor.ButtonFace => "#202020",
            KnownColor.ButtonHighlight => "#101010",
            KnownColor.ButtonShadow => "#464646",
            KnownColor.Control => "#202020",
            KnownColor.ControlDark => "#4A4A4A",
            KnownColor.ControlDarkDark => "#5A5A5A",
            KnownColor.ControlLight => "#2E2E2E",
            KnownColor.ControlLightLight => "#1F1F1F",
            KnownColor.ControlText => "#FFFFFF",
            KnownColor.Desktop => "#101010",
            KnownColor.GradientActiveCaption => "#416482",
            KnownColor.GradientInactiveCaption => "#557396",
            KnownColor.GrayText => "#969696",
            KnownColor.Highlight => "#2864B4",
            KnownColor.HighlightText => "#000000",
            KnownColor.HotTrack => "#2D5FAF",
            KnownColor.InactiveBorder => "#3C3F41",
            KnownColor.InactiveCaption => "#374B5A",
            KnownColor.InactiveCaptionText => "#BEBEBE",
            KnownColor.Info => "#50503C",
            KnownColor.InfoText => "#BEBEBE",
            KnownColor.Menu => "#373737",
            KnownColor.MenuBar => "#373737",
            KnownColor.MenuHighlight => "#2A80D2",
            KnownColor.MenuText => "#F0F0F0",
            KnownColor.ScrollBar => "#505050",
            KnownColor.Window => "#323232",
            KnownColor.WindowFrame => "#282828",
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

            return TryGetLightSystemColor(name, out Color lightColor)
                ? lightColor
                : Color.FromKnownColor(name);
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
            InactiveSelectionBackground = ColorToArgb(ResolveSystemColor(KnownColor.InactiveCaption)),
            DisabledForeground = ColorToArgb(ResolveSystemColor(KnownColor.GrayText)),
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
            SelectionForeground = ColorToArgb(ResolveSystemColor(KnownColor.HighlightText)),
            SelectionBackground = ColorToArgb(ResolveSystemColor(KnownColor.Highlight)),
            InactiveSelectionForeground = ColorToArgb(ResolveSystemColor(KnownColor.MenuText)),
            InactiveSelectionBackground = ColorToArgb(ResolveSystemColor(KnownColor.Menu)),
            DisabledForeground = ColorToArgb(ResolveSystemColor(KnownColor.GrayText)),
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
        if (control is TabPage { ToolTipText.Length: > 0 } tabPage)
        {
            return tabPage.ToolTipText;
        }

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
            if (control is DataGridView && IsGeneratedDataGridViewChild(child))
            {
                continue;
            }

            if (control is NumericUpDown or ToolStrip)
            {
                // parity-scaffolding: NumericUpDown's native edit/spin controls and a
                // ToolStripControlHost's owned Control are WinForms renderer internals. The
                // product exposes the composite owner or semantic ToolStripItem, not both.
                continue;
            }

            if (control is SplitContainer && child is SplitterPanel)
            {
                // parity-scaffolding: SplitterPanel is WinForms layout infrastructure, not a
                // product field. Emit the controls it owns directly beneath the semantic split.
                foreach (Control panelChild in child.Controls)
                {
                    children.Add(ReadControl(panelChild, id, childOrdinal++));
                }

                continue;
            }

            if (control.GetType().FullName == "ICSharpCode.TextEditor.TextEditorControl")
            {
                // parity-scaffolding: the editor's scrollbars and text-area panels are private
                // implementation controls from the external library, not Git Extensions fields.
                continue;
            }

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
            ItemHeightDip = control is DataGridView grid
                ? ToDip(grid.Rows.Count > 0 ? grid.Rows[0].Height : grid.RowTemplate.Height)
                : null,
            Padding = CreateThickness(control.Padding),
            Margin = CreateThickness(control.Margin),
            Font = ReadFont(control, control.Font),
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
            Expanded = control is TreeView treeView
                ? treeView.Nodes.Cast<TreeNode>().Any(node => node.IsExpanded)
                : null,
            Columns = GetColumns(control),
            Children = children
        };
    }

    // parity-scaffolding: DataGridView owns framework scrollbars and an editing placeholder;
    // the Avalonia reader likewise excludes generated ListBox template visuals from the semantic tree.
    private static bool IsGeneratedDataGridViewChild(Control child) =>
        string.IsNullOrEmpty(child.Name) && child is HScrollBar or VScrollBar or Label;

    private CaptureFont ReadFont(object owner, Font font)
    {
        decimal sizePoints = (decimal)(font.Unit == GraphicsUnit.Point
            ? font.Size
            : font.SizeInPoints);
        decimal normalizationFactor = IsDpiScaledFont(owner, font, sizePoints)
            ? _dipFactor
            : 1m;
        return new CaptureFont
        {
            Family = font.FontFamily.Name,
            EmSize = decimal.Round((decimal)font.Size * normalizationFactor, 4),
            Unit = font.Unit.ToString(),
            SizePoints = decimal.Round(sizePoints * normalizationFactor, 4),
            SizeDip = decimal.Round(sizePoints * normalizationFactor * 96m / 72m, 4),
            Style = GetStyles(font.Style)
        };
    }

    private bool IsDpiScaledFont(object owner, Font font, decimal sizePoints)
    {
        if (_dipFactor == 1m)
        {
            return false;
        }

        if (!_fontBaselines.TryGetValue(owner, out FontBaseline baseline))
        {
            // Some native composites create inherited children only after their handle exists.
            // Their effective Font is the root's scaled Font, but they could not be indexed
            // before WM_DPICHANGED. Reuse the root baseline only for that exact inherited font.
            if (!font.Equals(_root.Font) || !_fontBaselines.TryGetValue(_root, out baseline))
            {
                return false;
            }
        }

        decimal scaleFactor = 1m / _dipFactor;
        return Math.Abs((decimal)font.Size - (baseline.EmSize * scaleFactor)) <= 0.01m
            && Math.Abs(sizePoints - (baseline.SizePoints * scaleFactor)) <= 0.01m;
    }

    private CaptureNode ReadToolStripItem(ToolStripItem item, string parentId, int ordinal)
    {
        bool isSeparator = item is ToolStripSeparator;
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
            Font = ReadFont(item, item.Font),
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
            Enabled = isSeparator ? false : item.Enabled,
            Visible = item.Visible,
            Focused = isSeparator ? false : item.Selected,
            ReadOnly = null,
            CheckState = item is ToolStripMenuItem menuItem
                ? menuItem.CheckState.ToString()
                : null,
            Selected = isSeparator ? null : item.Selected,
            Expanded = item is ToolStripDropDownItem { DropDown.Visible: true }
                ? true
                : item is ToolStripDropDownItem
                    ? false
                    : null,
            Columns = [],
            Children = children
        };
    }

    private CaptureNode ReadToolStripItemCollection(ToolStrip popup, string id, Point primaryScreenOrigin)
    {
        Rectangle bounds = popup.Bounds;
        Rectangle semanticBounds = new(
            bounds.X - primaryScreenOrigin.X,
            bounds.Y - primaryScreenOrigin.Y,
            bounds.Width,
            bounds.Height);
        return new CaptureNode
        {
            Id = id,
            FieldName = null,
            FieldAliases = [],
            Name = popup.Name,
            Type = popup.GetType().FullName ?? popup.GetType().Name,
            ControlKind = "popup",
            BoundsPx = ToRectangle(semanticBounds),
            BoundsDip = new CaptureRectangleF { X = ToDip(semanticBounds.X), Y = ToDip(semanticBounds.Y), Width = ToDip(bounds.Width), Height = ToDip(bounds.Height) },
            ClientSizePx = new CaptureSize { Width = bounds.Width, Height = bounds.Height },
            ClientSizeDip = new CaptureSizeF { Width = ToDip(bounds.Width), Height = ToDip(bounds.Height) },
            Padding = CreateThickness(popup.Padding),
            Margin = CreateThickness(popup.Margin),
            Font = ReadFont(
                popup is ToolStripDropDown { OwnerItem: not null } dropDown ? dropDown.OwnerItem : popup,
                popup.Font),
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

    private CaptureNode ReadComboBoxItemCollection(ComboBoxPopup popup, string id, Point primaryScreenOrigin)
    {
        ComboBox comboBox = popup.Owner;
        Rectangle bounds = popup.Bounds;
        Rectangle semanticBounds = new(
            bounds.X - primaryScreenOrigin.X,
            bounds.Y - primaryScreenOrigin.Y,
            bounds.Width,
            bounds.Height);
        CaptureColors colors = GetColors(comboBox);
        CaptureFont font = ReadFont(comboBox, comboBox.Font);
        CaptureThicknessPair emptyThickness = CreateThickness(Padding.Empty);
        int itemHeight = Math.Max(1, comboBox.ItemHeight);
        CaptureNode[] children = comboBox.Items.Cast<object>()
            .Select((item, ordinal) => new CaptureNode
            {
                Id = $"{id}/item[{ordinal}]",
                FieldName = null,
                FieldAliases = [],
                Name = null,
                Type = item.GetType().FullName ?? item.GetType().Name,
                ControlKind = "listItem",
                BoundsPx = ToRectangle(new Rectangle(0, ordinal * itemHeight, bounds.Width, itemHeight)),
                BoundsDip = new CaptureRectangleF
                {
                    X = 0,
                    Y = ToDip(ordinal * itemHeight),
                    Width = ToDip(bounds.Width),
                    Height = ToDip(itemHeight)
                },
                ClientSizePx = new CaptureSize { Width = bounds.Width, Height = itemHeight },
                ClientSizeDip = new CaptureSizeF { Width = ToDip(bounds.Width), Height = ToDip(itemHeight) },
                ItemHeightDip = null,
                Padding = emptyThickness,
                Margin = emptyThickness,
                Font = font,
                Colors = colors,
                BorderStyle = null,
                FlatStyle = null,
                BorderWidthDip = null,
                CornerRadiusDip = null,
                Anchor = [],
                Dock = null,
                AutoSize = false,
                Alignment = null,
                Text = comboBox.GetItemText(item),
                ToolTip = null,
                TranslationSource = null,
                TabIndex = null,
                TabStop = null,
                Enabled = comboBox.Enabled,
                Visible = true,
                Focused = false,
                ReadOnly = null,
                CheckState = null,
                Selected = ordinal == comboBox.SelectedIndex,
                Expanded = null,
                Columns = [],
                Children = []
            })
            .ToArray();
        return new CaptureNode
        {
            Id = id,
            FieldName = null,
            FieldAliases = [],
            Name = null,
            Type = "System.Windows.Forms.ComboLBox",
            ControlKind = "popup",
            BoundsPx = ToRectangle(semanticBounds),
            BoundsDip = new CaptureRectangleF
            {
                X = ToDip(semanticBounds.X),
                Y = ToDip(semanticBounds.Y),
                Width = ToDip(bounds.Width),
                Height = ToDip(bounds.Height)
            },
            ClientSizePx = new CaptureSize { Width = bounds.Width, Height = bounds.Height },
            ClientSizeDip = new CaptureSizeF { Width = ToDip(bounds.Width), Height = ToDip(bounds.Height) },
            ItemHeightDip = ToDip(itemHeight),
            Padding = emptyThickness,
            Margin = emptyThickness,
            Font = font,
            Colors = colors,
            BorderStyle = "FixedSingle",
            FlatStyle = comboBox.FlatStyle.ToString(),
            BorderWidthDip = ToDip(1),
            CornerRadiusDip = null,
            Anchor = [],
            Dock = null,
            AutoSize = false,
            Alignment = null,
            Text = null,
            ToolTip = null,
            TranslationSource = null,
            TabIndex = null,
            TabStop = null,
            Enabled = comboBox.Enabled,
            Visible = comboBox.DroppedDown,
            Focused = comboBox.Focused,
            ReadOnly = comboBox.DropDownStyle == ComboBoxStyle.DropDownList,
            CheckState = null,
            Selected = null,
            Expanded = true,
            Columns = [],
            Children = children
        };
    }

    private decimal ToDip(int value) => decimal.Round(value * _dipFactor, 4);

    internal readonly record struct FontBaseline(decimal EmSize, decimal SizePoints)
    {
        public static FontBaseline From(Font font) =>
            new(
                (decimal)font.Size,
                (decimal)(font.Unit == GraphicsUnit.Point ? font.Size : font.SizeInPoints));
    }

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
