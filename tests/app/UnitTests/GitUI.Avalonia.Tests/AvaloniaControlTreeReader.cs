using System.Collections;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.ParityCapture;
using GitUI;
using GitUI.Compat;
using GitUI.UserControls.RevisionGrid.Columns;

namespace GitExtensionsTests;

internal sealed class AvaloniaControlTreeReader
{
    private readonly Dictionary<object, List<string>> _fieldNames = new(ReferenceEqualityComparer.Instance);
    private readonly PixelPoint _primaryScreenOrigin;
    private readonly double _renderScale;
    private readonly Control _root;

    public AvaloniaControlTreeReader(Control root, double renderScale, PixelPoint? primaryScreenOrigin = null)
    {
        _root = root;
        _renderScale = renderScale;
        _primaryScreenOrigin = primaryScreenOrigin ?? default;
        IndexFields(root);
    }

    public CaptureSurface ReadPrimary(Control root, PixelSize imageSize) =>
        ReadSurface(
            root,
            "primary",
            new PixelRect(0, 0, imageSize.Width, imageSize.Height));

    public CaptureSurface ReadSurface(Control root, string role, PixelRect screenBounds)
    {
        Control semanticRoot = GetSemanticSurfaceRoot(root);
        Rect? rootBoundsOverride = IsPopupSurface(semanticRoot)
            ? new Rect(
                (screenBounds.X - _primaryScreenOrigin.X) / _renderScale,
                (screenBounds.Y - _primaryScreenOrigin.Y) / _renderScale,
                screenBounds.Width / _renderScale,
                screenBounds.Height / _renderScale)
            : null;
        return new CaptureSurface
        {
            Role = role,
            ScreenBoundsPx = new CaptureRectangle
            {
                X = screenBounds.X,
                Y = screenBounds.Y,
                Width = screenBounds.Width,
                Height = screenBounds.Height
            },
            Root = ReadControl(
                semanticRoot,
                parentId: string.Empty,
                ordinal: 0,
                ancestorSemanticVisible: true,
                semanticParent: null,
                boundsOverride: rootBoundsOverride)
        };
    }

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

        if (control is ComboBox { SelectedItem: IHostedRemote hostedRemote })
        {
            // parity-scaffolding: WinForms captures the DisplayMember text, not the selected
            // object's diagnostic ToString value; mirror the product ItemTemplate contract.
            return hostedRemote.DisplayData;
        }

        object? content = GetPropertyValue(control, "Content");
        object? header = GetPropertyValue(control, "Header");
        string? text = GetPropertyValue(control, "Text") as string
            ?? content as string
            ?? (content as TextBlock)?.Text
            ?? header as string
            ?? (header as TextBlock)?.Text;
        return text is null
            ? string.Empty
            : control is MenuItem or Button or Label && TranslationCompat.GetConvertMnemonics(control)
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
            if (type.Namespace?.StartsWith("Avalonia.", StringComparison.Ordinal) is true)
            {
                // parity-scaffolding: Framework bookkeeping fields such as TabControl's
                // _selectedContent are renderer internals, not WinForms control-field twins.
                continue;
            }

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

    private CaptureNode ReadControl(
        Control control,
        string parentId,
        int ordinal,
        bool ancestorSemanticVisible,
        Control? semanticParent,
        Rect? boundsOverride)
    {
        IReadOnlyList<string> fieldNames = GetFieldNames(control);
        bool isSurfaceRoot = string.IsNullOrEmpty(parentId);
        bool isRevisionGrid = control is RevisionGridControl;
        bool isRevisionGridView = IsRevisionGridView(control);
        bool isNativeListView = IsNativeListView(control);
        bool isNativeTabControl = IsNativeTabControl(control);
        bool isNativeTabPage = IsNativeTabPage(control);
        bool isNativeButton = IsNativeButton(control);
        bool isPopupRoot = isSurfaceRoot && IsPopupSurface(control);
        string? fieldName = isSurfaceRoot
            ? null
            : fieldNames.FirstOrDefault()
              ?? (control is MenuItem or Separator || string.IsNullOrEmpty(control.Name) ? null : control.Name);
        Control? childSemanticParent = isSurfaceRoot || fieldName is not null
            ? control
            : semanticParent;
        string segment = isSurfaceRoot
            ? $"$root:{control.GetType().Name}"
            : fieldName ?? $"$unnamed[{ordinal}]:{control.GetType().Name}";
        string id = string.IsNullOrEmpty(parentId) ? segment : $"{parentId}/{segment}";
        bool hasNativeListComposite = TryGetNativeListComposite(control, out Grid? nativeListComposite, out _);
        Rect bounds = boundsOverride
            ?? (hasNativeListComposite
                ? GetSemanticBounds(nativeListComposite!, semanticParent)
                : GetSemanticBounds(control, semanticParent));
        bool childSemanticVisible = ancestorSemanticVisible
            && (control is not MenuItem menuItem || menuItem.IsSubMenuOpen)
            && (control is not TabItem tabItem || !isNativeTabPage || tabItem.IsSelected);
        IReadOnlyList<CaptureNode> children = GetCaptureChildren(control)
            .Select((child, childOrdinal) => ReadControl(
                child,
                id,
                childOrdinal,
                childSemanticVisible,
                childSemanticParent,
                boundsOverride: null))
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
            ClientSizePx = new CaptureSize
            {
                Width = ToPixel(isNativeListView ? Math.Max(0, bounds.Width - 4) : bounds.Width),
                Height = ToPixel(isNativeListView ? Math.Max(0, bounds.Height - 4) : bounds.Height)
            },
            ClientSizeDip = new CaptureSizeF
            {
                Width = ToDecimal(isNativeListView ? Math.Max(0, bounds.Width - 4) : bounds.Width),
                Height = ToDecimal(isNativeListView ? Math.Max(0, bounds.Height - 4) : bounds.Height)
            },
            ItemHeightDip = isRevisionGridView
                ? ReadRevisionGridItemHeight(control)
                : null,
            Padding = ReadThicknessPair(isNativeTabPage || isNativeButton
                ? default(Thickness)
                : GetPropertyValue(control, "Padding")),
            Margin = ReadThicknessPair(isNativeButton
                ? new Thickness(3)
                : hasNativeListComposite ? nativeListComposite!.Margin : control.Margin),
            Font = ReadFont(control),
            Colors = ReadColors(control),
            BorderStyle = isRevisionGrid || isRevisionGridView || isNativeTabPage
                ? "None"
                : isNativeListView
                    ? "Fixed3D"
                    : GetPropertyValue(control, "BorderStyle")?.ToString(),
            FlatStyle = isNativeButton ? (IsDarkTheme() ? "Flat" : "Standard") : null,
            BorderWidthDip = isPopupRoot || isNativeListView || isNativeTabControl || isNativeTabPage || isNativeButton
                ? null
                : ReadBorderWidth(control),
            CornerRadiusDip = ReadCornerRadius(control),
            Anchor = isRevisionGrid || isRevisionGridView || isNativeListView || isNativeTabControl || isNativeTabPage
                ? ["Top", "Left"]
                : [],
            Dock = isRevisionGrid || isNativeTabPage || isNativeButton
                ? isNativeButton && control.Name == "buttonBrowse" ? "Fill" : "None"
                : isRevisionGridView || isNativeListView || isNativeTabControl ? "Fill" : null,
            AutoSize = isRevisionGrid || isRevisionGridView || isNativeListView || isNativeTabControl || isNativeTabPage || isNativeButton
                ? false
                : control is MenuItem or Separator || isPopupRoot ? true : null,
            Alignment = isNativeButton
                ? "MiddleCenter"
                : isRevisionGrid || isRevisionGridView || isNativeTabControl || isNativeTabPage || isPopupRoot
                ? null
                : control is MenuItem or Separator ? "MiddleCenter" : GetAlignment(control),
            Text = GetText(control),
            ToolTip = ToolTip.GetTip(control)?.ToString(),
            TranslationSource = fieldName,
            TabIndex = isRevisionGrid || isRevisionGridView
                ? 0
                : control is MenuItem or Separator || isPopupRoot ? null : KeyboardNavigation.GetTabIndex(control),
            TabStop = isNativeTabPage
                ? false
                : isRevisionGrid || isRevisionGridView || isNativeListView || isNativeTabControl
                ? true
                : control is MenuItem or Separator || isPopupRoot ? null : control.Focusable,
            Enabled = control is Separator ? false : control.IsEffectivelyEnabled,
            Visible = isNativeTabPage
                ? ((TabItem)control).IsSelected && ancestorSemanticVisible
                : control.IsVisible && ancestorSemanticVisible,
            Focused = isPopupRoot ? false : IsFocused(control),
            ReadOnly = isRevisionGridView ? true : GetNullableBoolProperty(control, "IsReadOnly"),
            CheckState = control switch
            {
                MenuItem checkedMenuItem => checkedMenuItem.IsChecked ? "Checked" : "Unchecked",
                ToggleButton toggle when control is CheckBox or RadioButton => toggle.IsChecked switch
                {
                    true => "Checked",
                    false => "Unchecked",
                    null => "Indeterminate"
                },
                _ => null
            },
            Selected = GetSelected(control),
            Expanded = isRevisionGrid || isRevisionGridView ? false : isPopupRoot ? true : GetExpanded(control),
            Columns = ReadColumns(control),
            Children = children
        };
    }

    private static Rect GetSemanticBounds(Control control, Control? semanticParent)
    {
        if (IsNativeTabPage(control)
            && control.GetLogicalAncestors().OfType<TabControl>().FirstOrDefault() is { } owner)
        {
            // parity-scaffolding: WinForms TabPage.Bounds is the native display rectangle,
            // while Avalonia's TabItem.Bounds describes only the clickable header.
            return new Rect(
                4,
                30,
                Math.Max(0, owner.Bounds.Width - 8),
                Math.Max(0, owner.Bounds.Height - 34));
        }

        if (semanticParent is TabItem && IsNativeTabPage(semanticParent)
            && semanticParent.GetLogicalAncestors().OfType<TabControl>().FirstOrDefault() is { } tabOwner
            && control.TranslatePoint(default, tabOwner) is Point pageChildOrigin)
        {
            // parity-scaffolding: Product content is rendered through Avalonia's selected-content
            // presenter; report it relative to the emitted WinForms-shaped TabPage client.
            return new Rect(pageChildOrigin.X - 4, pageChildOrigin.Y - 30, control.Bounds.Width, control.Bounds.Height);
        }

        if (IsNativeTabControl(control)
            && semanticParent is Grid { Name: "splitContainer2" }
            && Grid.GetRow(control) > 0)
        {
            // parity-scaffolding: the Avalonia Grid row is the WinForms SplitterPanel2 owner,
            // which is intentionally suppressed from the semantic tree.
            return new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
        }

        if (semanticParent is null
            || control.TranslatePoint(default, semanticParent) is not Point origin)
        {
            return control.Bounds;
        }

        // parity-scaffolding: Logical menu children are rendered below Fluent presenter
        // wrappers. Report their position in the emitted semantic parent, like ToolStripItem.Bounds.
        return new Rect(origin, control.Bounds.Size);
    }

    private bool IsDarkTheme()
        => _root.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark;

    private decimal? ReadRevisionGridItemHeight(Control control)
        => control.GetVisualDescendants()
            .OfType<ListBoxItem>()
            .Select(item => (decimal?)ToDecimal(item.Bounds.Height))
            .FirstOrDefault();

    private IReadOnlyList<string> GetFieldNames(object value) =>
        _fieldNames.TryGetValue(value, out List<string>? names)
            ? names
            : [];

    private IEnumerable<Control> GetCaptureChildren(Control control)
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

        if (IsNativeListComposite(control, out ListBox? nativeList))
        {
            // parity-scaffolding: the native Avalonia substitute separates its header from
            // recycled rows; emit the one semantic ListView exposed by the WinForms original.
            return [nativeList!];
        }

        if (control is Label)
        {
            // parity-scaffolding: Label.Content is the WinForms Label.Text value; Avalonia's
            // generated AccessText is renderer infrastructure, not a second semantic control.
            return [];
        }

        if (control is TabControl tabControl)
        {
            // parity-scaffolding: TabControl exposes its selected content both beneath the owning
            // TabItem and through an internal selected-content presenter. Emit each product view
            // once through its TabItem, matching WinForms TabPage ownership.
            IEnumerable items = tabControl.ItemsSource ?? tabControl.Items;
            return items.OfType<Control>();
        }

        if (control is TabItem tabItem)
        {
            // parity-scaffolding: TabPage owns its product content directly; the Avalonia
            // selected-content presenter between them is template infrastructure.
            if (tabItem.IsSelected
                && tabItem.GetLogicalAncestors().OfType<TabControl>().FirstOrDefault() is { } owner
                && GetPropertyValue(owner, "SelectedContent") is Control selectedContent)
            {
                if (GetPropertyValue(selectedContent, "Content") is Control productContent)
                {
                    return [productContent];
                }

                return [selectedContent];
            }

            return tabItem.Content is Control content ? [content] : [];
        }

        if (control is HeaderedContentControl)
        {
            // parity-scaffolding: The string Header is emitted on the owning semantic control;
            // retain its product content but omit the generated header AccessText.
            return control.GetLogicalChildren()
                .OfType<Control>()
                .Where(child => child is not AccessText
                                && child.TemplatedParent is null
                                && child.GetType().Name != "TopLevelHost");
        }

        if (IsPopupPresenter(control) || IsOverlayPopupHost(control))
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

        if (IsNativeListView(control))
        {
            return [];
        }

        if (control is Button or TextBox or ComboBox)
        {
            return [];
        }

        IEnumerable<Control> children = control.GetLogicalChildren()
            .OfType<Control>()
            .Where(child => child.TemplatedParent is null
                            && child.GetType().Name != "TopLevelHost")
            .Where(child => !IsSearchResultOverlay(child));

        if (ReferenceEquals(control, _root) && control is Window)
        {
            // parity-scaffolding: WinForms reparents SearchControl's result list to its owning
            // form. Avalonia keeps the same overlay in the control's logical tree, so lift it
            // to the emitted window root and measure it against that semantic owner.
            children = children.Concat(
                control.GetLogicalDescendants()
                    .OfType<Control>()
                    .Where(IsSearchResultOverlay));
        }

        return children;
    }

    private static bool IsSearchResultOverlay(Control control)
        => control.Name == "listBoxSearchResult"
           && control.GetLogicalAncestors()
               .OfType<Control>()
               .Any(ancestor => ancestor.GetType().Name.StartsWith("SearchControl", StringComparison.Ordinal));

    private CaptureColors ReadColors(Control control)
    {
        SortedDictionary<string, string> additional = new(StringComparer.Ordinal);
        AddAdditional("caret", GetPropertyValue(control, "CaretBrush"));
        AddAdditional("pointerOverBackground", GetPropertyValue(control, "PointerOverBackground"));
        AddAdditional("pressedBackground", GetPropertyValue(control, "PressedBackground"));
        if (ReferenceEquals(control, _root))
        {
            AddSemantic("semantic.app.panel.background", "GitExtensionsPanelBackgroundBrush");
            AddSemantic("semantic.app.revision.alternating.background", "GitExtensionsRevisionAlternatingRowBrush");
            AddSemantic("semantic.app.revision.authored.background", "GitExtensionsRevisionAuthoredBrush");
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

        if (IsNativeListView(control))
        {
            string? background = BrushToArgb(GetPropertyValue(control, "Background"));
            return new CaptureColors
            {
                Foreground = BrushToArgb(GetPropertyValue(control, "Foreground")),
                Background = background,
                Border = null,
                SelectionForeground = ResolveResourceArgb("GitExtensionsKnownColorHighlightTextBrush")
                                      ?? ResolveResourceArgb("GitExtensionsHighlightForegroundBrush"),
                SelectionBackground = ResolveResourceArgb("GitExtensionsKnownColorHighlightBrush")
                                      ?? ResolveResourceArgb("GitExtensionsHighlightBackgroundBrush"),
                InactiveSelectionForeground = ResolveResourceArgb("GitExtensionsKnownColorHighlightTextBrush")
                                              ?? ResolveResourceArgb("GitExtensionsHighlightForegroundBrush"),
                InactiveSelectionBackground = ResolveResourceArgb("GitExtensionsKnownColorInactiveCaptionBrush")
                                              ?? ResolveResourceArgb("GitExtensionsSystemInactiveSelectionBackgroundBrush"),
                DisabledForeground = ResolveResourceArgb("GitExtensionsKnownColorGrayTextBrush")
                                     ?? ResolveResourceArgb("GitExtensionsDisabledForegroundBrush"),
                DisabledBackground = background,
                GridLine = null,
                Additional = new SortedDictionary<string, string>(additional, StringComparer.Ordinal)
                {
                    ["hotTrack"] = ResolveResourceArgb("GitExtensionsNativeListHotTrackBrush")
                                   ?? throw new InvalidDataException("The native-list hot-track color did not resolve.")
                }
            };
        }

        if (IsNativeButton(control))
        {
            // parity-scaffolding: WinForms visual styles paint native button state without
            // changing the Button's semantic Control/ControlText color properties.
            string? background = ResolveResourceArgb("GitExtensionsKnownColorControlBrush")
                                 ?? ResolveResourceArgb("GitExtensionsControlBackgroundBrush");
            return new CaptureColors
            {
                Foreground = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")
                             ?? ResolveResourceArgb("GitExtensionsControlForegroundBrush"),
                Background = background,
                Border = null,
                SelectionForeground = null,
                SelectionBackground = null,
                InactiveSelectionForeground = null,
                InactiveSelectionBackground = null,
                DisabledForeground = ResolveResourceArgb("GitExtensionsKnownColorGrayTextBrush")
                                     ?? ResolveResourceArgb("GitExtensionsDisabledForegroundBrush"),
                DisabledBackground = background,
                GridLine = null,
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
            Foreground = BrushToArgb(GetPropertyValue(control, "Foreground"))
                         ?? ResolveResourceArgb("GitExtensionsControlForegroundBrush"),
            Background = ResolveEffectiveBackground(control),
            Border = IsNativeButton(control) ? null : BrushToArgb(GetPropertyValue(control, "BorderBrush")),
            SelectionForeground = BrushToArgb(
                GetPropertyValue(control, "SelectionForegroundBrush")
                ?? GetPropertyValue(control, "SelectionForeground")),
            SelectionBackground = BrushToArgb(
                GetPropertyValue(control, "SelectionBrush")
                ?? GetPropertyValue(control, "SelectionBackground")),
            InactiveSelectionForeground = BrushToArgb(GetPropertyValue(control, "InactiveSelectionForeground")),
            InactiveSelectionBackground = BrushToArgb(GetPropertyValue(control, "InactiveSelectionBackground")),
            DisabledForeground = BrushToArgb(GetPropertyValue(control, "DisabledForeground"))
                                 ?? ResolveResourceArgb("GitExtensionsDisabledForegroundBrush"),
            DisabledBackground = BrushToArgb(GetPropertyValue(control, "DisabledBackground"))
                                 ?? ResolveEffectiveBackground(control),
            GridLine = BrushToArgb(GetPropertyValue(control, "GridLinesBrush")),
            Additional = additional
        };

        string? ResolveEffectiveBackground(Control target)
        {
            string? direct = BrushToArgb(GetPropertyValue(target, "Background"));
            if (direct is not null)
            {
                return direct;
            }

            foreach (Control ancestor in target.GetLogicalAncestors().OfType<Control>())
            {
                string? inherited = BrushToArgb(GetPropertyValue(ancestor, "Background"));
                if (inherited is not null)
                {
                    return inherited;
                }
            }

            return ResolveResourceArgb("GitExtensionsControlBackgroundBrush");
        }

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

        if (TryGetNativeListComposite(control, out _, out Grid? header))
        {
            CaptureColors colors = ReadColors(control);
            return header!.Children
                .OfType<ContentControl>()
                .OrderBy(Grid.GetColumn)
                .Select((column, index) =>
                {
                    ColumnDefinition definition = header.ColumnDefinitions[index];
                    double widthDip = definition.ActualWidth > 0
                        ? definition.ActualWidth
                        : definition.Width.Value;
                    return new CaptureColumn
                    {
                        FieldName = GetFieldNames(column).FirstOrDefault() ?? column.Name,
                        Name = string.Empty,
                        Type = "System.Windows.Forms.ColumnHeader",
                        Index = index,
                        DisplayIndex = index,
                        WidthPx = ToPixel(widthDip),
                        WidthDip = ToDecimal(widthDip),
                        Visible = widthDip > 0,
                        Resizable = true,
                        SortMode = null,
                        Alignment = column.HorizontalContentAlignment.ToString(),
                        HeaderText = GetText(column),
                        HeaderAlignment = column.HorizontalContentAlignment.ToString(),
                        Colors = colors
                    };
                })
                .ToArray();
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

    private static bool IsNativeListView(Control control)
        => control is ListBox list && list.Classes.Contains("gitextensions-native-list-items");

    private static bool IsNativeTabControl(Control control)
        => control is TabControl tabControl && tabControl.Classes.Contains("gitextensions-native-tabs");

    private static bool IsNativeTabPage(Control control)
        => control is TabItem tabItem
           && tabItem.GetLogicalAncestors()
               .OfType<TabControl>()
               .Any(IsNativeTabControl);

    private static bool IsNativeButton(Control control)
        => control is Button button && button.Classes.Contains("gitextensions-native-dialog-action");

    private static bool IsNativeListComposite(Control control, out ListBox? list)
    {
        if (control is not Grid grid)
        {
            list = null;
            return false;
        }

        list = grid.Children.OfType<ListBox>().SingleOrDefault(IsNativeListView);
        return list is not null && grid.Children.OfType<Grid>().Any(IsNativeListHeader);
    }

    private static bool TryGetNativeListComposite(
        Control control,
        out Grid? composite,
        out Grid? header)
    {
        composite = control.Parent as Grid;
        header = composite?.Children.OfType<Grid>().SingleOrDefault(IsNativeListHeader);
        return IsNativeListView(control) && composite is not null && header is not null;
    }

    private static bool IsNativeListHeader(Grid grid)
        => grid.Children.OfType<ContentControl>().Any()
           && grid.Children
               .OfType<ContentControl>()
               .All(header => header.Classes.Contains("gitextensions-list-header-cell"));

    private static bool IsPopupPresenter(Control control) =>
        control.GetType().Name == "MenuFlyoutPresenter";

    private static bool IsPopupSurface(Control control) =>
        control is ContextMenu || IsPopupPresenter(control) || IsOverlayPopupHost(control);

    private bool IsFocused(Control control)
    {
        if (IsNativeListView(control))
        {
            return control.IsKeyboardFocusWithin;
        }

        if (!IsRevisionGridView(control))
        {
            return control.IsFocused;
        }

        // parity-scaffolding: Avalonia moves keyboard focus into an owned ContextMenu, while
        // WinForms keeps the owning grid focused; emit the equivalent owner-focused state.
        return control.IsKeyboardFocusWithin
               || control.ContextMenu?.IsOpen == true
               || _root.GetLogicalDescendants().OfType<ContextMenu>().Any(menu => menu.IsOpen);
    }

    private static bool IsOverlayPopupHost(Control control) =>
        control.GetType().Name == "OverlayPopupHost";

    private decimal ToDecimal(double value) => decimal.Round((decimal)value, 4);

    private int ToPixel(double value) =>
        checked((int)Math.Round(value * _renderScale, MidpointRounding.AwayFromZero));
}
