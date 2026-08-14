using System.Collections;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using GitExtensions.ParityCapture;
using GitUI;
using GitUI.UserControls.RevisionGrid.Columns;

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
        ReadSurface(
            root,
            "primary",
            new PixelRect(0, 0, imageSize.Width, imageSize.Height));

    public CaptureSurface ReadSurface(Control root, string role, PixelRect screenBounds) =>
        new()
        {
            Role = role,
            ScreenBoundsPx = new CaptureRectangle
            {
                X = screenBounds.X,
                Y = screenBounds.Y,
                Width = screenBounds.Width,
                Height = screenBounds.Height
            },
            Root = ReadControl(GetSemanticSurfaceRoot(root), parentId: string.Empty, ordinal: 0)
        };

    private Control GetSemanticSurfaceRoot(Control root)
    {
        if (root.GetType().Name != "OverlayPopupHost")
        {
            return root;
        }

        // parity-scaffolding: Avalonia's in-frame popup host is rendering infrastructure;
        // emit the product ContextMenu as the same semantic surface root as ContextMenuStrip.
        return root.GetLogicalDescendants().OfType<ContextMenu>().SingleOrDefault()
               ?? root.GetVisualDescendants().OfType<Control>().FirstOrDefault(IsPopupPresenter)
               ?? root;
    }

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
            ListBox { Name: "_gridView" } when control.GetLogicalAncestors().OfType<RevisionGridControl>().Any() => "dataGrid",
            ListBox => "list",
            ContextMenu => "popup",
            _ when IsPopupPresenter(control) => "popup",
            Menu => "menu",
            MenuItem => "menuItem",
            Separator => "menuItem",
            TabControl => "tabs",
            GridSplitter => "split",
            _ when control.GetType().Name == "OverlayPopupHost" => "popup",
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
        if (IsOverlayPopupHost(control) || control is ContextMenu || IsPopupPresenter(control))
        {
            return null;
        }

        object? value = GetPropertyValue(control, "Text")
            ?? GetPropertyValue(control, "Content")
            ?? GetPropertyValue(control, "Header");
        return value is not string text
            ? string.Empty
            : control is MenuItem or Button or Label
                ? ToWinFormsMnemonics(text)
                : text;
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

    private static string? GetAlignment(Control control) =>
        GetPropertyValue(control, "TextAlignment")?.ToString()
        ?? GetPropertyValue(control, "HorizontalContentAlignment")?.ToString();

    private static bool? GetSelected(Control control) =>
        control switch
        {
            ListBoxItem listItem => listItem.IsSelected,
            TreeViewItem treeItem => treeItem.IsSelected,
            MenuItem menuItem => menuItem.IsSelected,
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
        foreach (Control child in root.GetLogicalChildren()
                     .OfType<Control>()
                     .Where(child => child.TemplatedParent is null
                                     && child.GetType().Name != "TopLevelHost"))
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
        bool isSurfaceRoot = string.IsNullOrEmpty(parentId);
        string? fieldName = isSurfaceRoot
            ? null
            : fieldNames.FirstOrDefault()
              ?? (control is MenuItem or Separator || string.IsNullOrEmpty(control.Name) ? null : control.Name);
        string segment = isSurfaceRoot
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
            AutoSize = control is MenuItem or Separator ? true : null,
            Alignment = control is MenuItem or Separator ? "MiddleCenter" : GetAlignment(control),
            Text = GetText(control),
            ToolTip = ToolTip.GetTip(control)?.ToString(),
            TranslationSource = fieldName,
            TabIndex = control is MenuItem or Separator ? null : KeyboardNavigation.GetTabIndex(control),
            TabStop = control is MenuItem or Separator ? null : control.Focusable,
            Enabled = control is Separator ? false : control.IsEnabled,
            Visible = IsMenuItemVisible(control),
            Focused = IsRevisionGridView(control) ? control.IsKeyboardFocusWithin : control.IsFocused,
            ReadOnly = IsRevisionGridView(control) ? true : GetNullableBoolProperty(control, "IsReadOnly"),
            CheckState = control is ToggleButton toggle
                         && (control is CheckBox || control is RadioButton || control is MenuItem)
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

    private static bool IsMenuItemVisible(Control control) =>
        control.IsVisible
        && (control is not MenuItem
            || control.GetLogicalAncestors().OfType<MenuItem>().All(parent => parent.IsSubMenuOpen));

    private IReadOnlyList<string> GetFieldNames(object value) =>
        _fieldNames.TryGetValue(value, out List<string>? names)
            ? names
            : [];

    private static IEnumerable<Control> GetCaptureChildren(Control control)
    {
        if (control is RevisionGridControl)
        {
            // parity-scaffolding: The Avalonia twin uses layout/recycling controls around its
            // native ListBox; the WinForms tree exposes only the semantic grid at this level.
            return control.GetLogicalDescendants().OfType<ListBox>().Where(IsRevisionGridView).Take(1);
        }

        if (control is MenuItem menuItem)
        {
            // parity-scaffolding: Items are the semantic ToolStripDropDownItems; AccessText,
            // icon presenters, and other template children are renderer implementation details.
            return menuItem.Items.OfType<Control>();
        }

        if (control is Separator)
        {
            return [];
        }

        if (IsPopupPresenter(control))
        {
            return control.GetVisualDescendants()
                .OfType<Control>()
                .Where(child => child is MenuItem or Separator)
                .Where(child => !child.GetVisualAncestors().TakeWhile(ancestor => !ReferenceEquals(ancestor, control)).OfType<MenuItem>().Any());
        }

        if (IsRevisionGridView(control))
        {
            return [];
        }

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

        if (control is MenuItem or Separator)
        {
            string? foreground = control is Separator
                ? BrushToArgb(GetPropertyValue(control, "Foreground"))
                : ResolveResourceArgb("GitExtensionsControlForegroundBrush")
                  ?? BrushToArgb(GetPropertyValue(control, "Foreground"));
            string? background = ResolveResourceArgb("GitExtensionsControlBackgroundBrush")
                                 ?? BrushToArgb(GetPropertyValue(control, "Background"));
            return new CaptureColors
            {
                Foreground = foreground,
                Background = background,
                Border = BrushToArgb(GetPropertyValue(control, "BorderBrush")),
                SelectionForeground = ResolveResourceArgb("GitExtensionsKnownColorHighlightTextBrush")
                                      ?? ResolveResourceArgb("GitExtensionsHighlightForegroundBrush"),
                SelectionBackground = ResolveResourceArgb("GitExtensionsKnownColorHighlightBrush")
                                      ?? ResolveResourceArgb("GitExtensionsHighlightBackgroundBrush"),
                InactiveSelectionForeground = ResolveResourceArgb("GitExtensionsMenuForegroundBrush")
                                              ?? ResolveResourceArgb("GitExtensionsWindowTextBrush"),
                InactiveSelectionBackground = ResolveResourceArgb("GitExtensionsMenuBackgroundBrush")
                                              ?? ResolveResourceArgb("GitExtensionsControlBackgroundBrush"),
                DisabledForeground = ResolveResourceArgb("GitExtensionsKnownColorGrayTextBrush")
                                     ?? ResolveResourceArgb("GitExtensionsDisabledForegroundBrush"),
                DisabledBackground = background,
                GridLine = null,
                Additional = additional
            };
        }

        if (IsRevisionGridView(control))
        {
            string? background = BrushToArgb(GetPropertyValue(control, "Background"));
            return new CaptureColors
            {
                Foreground = BrushToArgb(GetPropertyValue(control, "Foreground")),
                Background = background,
                Border = background,
                SelectionForeground = ResolveResourceArgb("GitExtensionsKnownColorHighlightTextBrush")
                                      ?? ResolveResourceArgb("GitExtensionsHighlightForegroundBrush"),
                SelectionBackground = ResolveResourceArgb("GitExtensionsKnownColorGradientActiveCaptionBrush")
                                      ?? ResolveResourceArgb("GitExtensionsHighlightBackgroundBrush"),
                InactiveSelectionForeground = ResolveResourceArgb("GitExtensionsKnownColorHighlightTextBrush")
                                              ?? ResolveResourceArgb("GitExtensionsHighlightForegroundBrush"),
                InactiveSelectionBackground = ResolveResourceArgb("GitExtensionsKnownColorInactiveCaptionBrush")
                                              ?? ResolveResourceArgb("GitExtensionsSystemInactiveSelectionBackgroundBrush"),
                DisabledForeground = ResolveResourceArgb("GitExtensionsKnownColorGrayTextBrush")
                                     ?? ResolveResourceArgb("GitExtensionsDisabledForegroundBrush"),
                DisabledBackground = background,
                GridLine = ResolveResourceArgb("GitExtensionsKnownColorWindowBrush")
                           ?? ResolveResourceArgb("GitExtensionsWindowBackgroundBrush"),
                Additional = additional
            };
        }

        if (control is ContextMenu || IsPopupPresenter(control) || IsOverlayPopupHost(control))
        {
            string? background = BrushToArgb(GetPropertyValue(control, "Background"))
                                 ?? ResolveResourceArgb("GitExtensionsControlBackgroundBrush");
            return new CaptureColors
            {
                Foreground = BrushToArgb(GetPropertyValue(control, "Foreground"))
                             ?? ResolveResourceArgb("GitExtensionsControlForegroundBrush"),
                Background = background,
                Border = null,
                SelectionForeground = null,
                SelectionBackground = null,
                InactiveSelectionForeground = null,
                InactiveSelectionBackground = null,
                DisabledForeground = ResolveResourceArgb("GitExtensionsDisabledForegroundBrush"),
                DisabledBackground = background,
                GridLine = null,
                Additional = additional
            };
        }

        if (ReferenceEquals(control, _root) && control is RevisionGridControl)
        {
            string? background = BrushToArgb(GetPropertyValue(control, "Background"));
            return new CaptureColors
            {
                Foreground = BrushToArgb(GetPropertyValue(control, "Foreground")),
                Background = background,
                Border = null,
                SelectionForeground = null,
                SelectionBackground = null,
                InactiveSelectionForeground = null,
                InactiveSelectionBackground = null,
                DisabledForeground = ResolveResourceArgb("GitExtensionsDisabledForegroundBrush"),
                DisabledBackground = background,
                GridLine = null,
                Additional = additional
            };
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
        if (IsOverlayPopupHost(control)
            && _root.TryFindResource("GitExtensionsUiFontFamily", _root.ActualThemeVariant, out object? familyResource)
            && familyResource is FontFamily menuFamily
            && _root.TryFindResource("GitExtensionsUiFontSize", _root.ActualThemeVariant, out object? sizeResource)
            && sizeResource is double menuSizeDip)
        {
            // parity-scaffolding: The private host inherits Fluent's application font, while
            // the menu it visibly renders uses Git Extensions' resolved UI font and size.
            return new CaptureFont
            {
                Family = menuFamily.Name,
                EmSize = ToDecimal(menuSizeDip),
                Unit = "Dip",
                SizePoints = ToDecimal(menuSizeDip * 72 / 96),
                SizeDip = ToDecimal(menuSizeDip),
                Style = ["Regular"]
            };
        }

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

        double width = Math.Max(Math.Max(thickness.Left, thickness.Top), Math.Max(thickness.Right, thickness.Bottom));
        return width == 0 ? null : ToDecimal(width);
    }

    private CaptureCornerRadius? ReadCornerRadius(Control control)
    {
        if (GetPropertyValue(control, "CornerRadius") is not CornerRadius radius)
        {
            return null;
        }

        if (radius.TopLeft == 0 && radius.TopRight == 0 && radius.BottomRight == 0 && radius.BottomLeft == 0)
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
        if (GetPropertyValue(control, "Columns") is IEnumerable columns)
        {
            return ReadFrameworkColumns(columns);
        }

        // parity-scaffolding: RevisionGrid uses native recycled ListBox rows, so expose its real
        // provider layout through the same grid-column schema that represents DataGridView.
        if (_root is RevisionGridControl revisionGrid
            && control is ListBox { Name: "_gridView" })
        {
            return ReadRevisionGridColumns(revisionGrid);
        }

        return [];
    }

    private IReadOnlyList<CaptureColumn> ReadFrameworkColumns(IEnumerable columns)
        => columns.Cast<object>()
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

    private IReadOnlyList<CaptureColumn> ReadRevisionGridColumns(RevisionGridControl revisionGrid)
    {
        Grid? realizedRow = revisionGrid.GetLogicalDescendants()
            .OfType<Grid>()
            .FirstOrDefault(row => row.Classes.Contains("revision-row"));
        CaptureColors colors = ReadRevisionGridColumnColors();

        return revisionGrid.ColumnProviders
            .Select(provider =>
            {
                RevisionGridColumn column = provider.Column;
                bool visible = column.IsVisible && column.IsAvailable;
                double widthDip = column.Width.IsStar && realizedRow is not null
                    ? realizedRow.ColumnDefinitions[provider.Index].ActualWidth
                    : column.Width.Value;
                return new CaptureColumn
                {
                    FieldName = GetFieldNames(column).FirstOrDefault(),
                    Name = null,
                    Type = column.GetType().FullName ?? column.GetType().Name,
                    Index = provider.Index,
                    DisplayIndex = provider.Index,
                    WidthPx = ToPixel(widthDip),
                    WidthDip = ToDecimal(widthDip),
                    Visible = visible,
                    Resizable = column.Resizable,
                    SortMode = "NotSortable",
                    Alignment = "NotSet",
                    HeaderText = column.HeaderText,
                    HeaderAlignment = "NotSet",
                    Colors = colors
                };
            })
            .ToArray();
    }

    private CaptureColors ReadRevisionGridColumnColors()
    {
        string? inactiveSelectionBackground = ResolveResourceArgb("GitExtensionsSystemInactiveSelectionBackgroundBrush");
        string? disabledForeground = ResolveResourceArgb("GitExtensionsDisabledForegroundBrush");
        return EmptyColors() with
        {
            InactiveSelectionBackground = inactiveSelectionBackground,
            DisabledForeground = disabledForeground
        };
    }

    private string? ResolveResourceArgb(string key)
        => _root.TryFindResource(key, _root.ActualThemeVariant, out object? resource)
            ? BrushToArgb(resource)
            : null;

    private static bool IsRevisionGridView(Control control) =>
        control is ListBox { Name: "_gridView" }
        && control.GetLogicalAncestors().OfType<RevisionGridControl>().Any();

    private static bool IsPopupPresenter(Control control) =>
        control.GetType().Name == "MenuFlyoutPresenter";

    private static bool IsOverlayPopupHost(Control control) =>
        control.GetType().Name == "OverlayPopupHost";

    private decimal ToDecimal(double value) => decimal.Round((decimal)value, 4);

    private int ToPixel(double value) =>
        checked((int)Math.Round(value * _renderScale, MidpointRounding.AwayFromZero));
}
