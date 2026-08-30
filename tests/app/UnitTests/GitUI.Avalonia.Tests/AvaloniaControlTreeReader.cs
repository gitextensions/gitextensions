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
    private readonly Dictionary<object, string> _fieldOwnerTypes = new(ReferenceEqualityComparer.Instance);
    private readonly PixelPoint _primaryScreenOrigin;
    private readonly bool _usesDesignerLayoutMetadata;
    private readonly double _renderScale;
    private readonly Control _root;

    public AvaloniaControlTreeReader(Control root, double renderScale, PixelPoint? primaryScreenOrigin = null)
    {
        _root = root;
        _renderScale = renderScale;
        _primaryScreenOrigin = primaryScreenOrigin ?? default;
        _usesDesignerLayoutMetadata = root.GetType().FullName is
            "GitUI.CommandsDialogs.RepoHosting.CreatePullRequestForm"
            or "GitUI.CommandsDialogs.RepoHosting.ForkAndCloneForm"
            or "GitUI.CommandsDialogs.RepoHosting.ViewPullRequestsForm"
            or "GitUI.CommandsDialogs.FormPull"
            or "GitUI.CommandsDialogs.FormPush"
            or "GitUI.CommandsDialogs.FormRemotes";
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
                (screenBounds.Width / _renderScale) - (IsOverlayPopupHost(semanticRoot) ? 1 : 0),
                (screenBounds.Height / _renderScale) + (IsOverlayPopupHost(semanticRoot) ? 2 : 0))
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
            _ when IsSemanticToolStrip(control) => "toolStrip",
            _ when IsSemanticToolStripItem(control) => "menuItem",
            _ when IsFileStatusListView(control) => "tree",
            _ when IsRepositoryHostSplit(control) => "split",
            _ when IsSourceLabelSubstitute(control) || IsSpellCheckAutoComplete(control) => "control",
            ToggleButton => "button",
            Button => "button",
            TextBox => "text",
            ComboBox => "comboBox",
            TreeView => "tree",
            ListBox { Name: "_gridView" } when control.GetLogicalAncestors().OfType<RevisionGridControl>().Any() => "dataGrid",
            _ when control.GetType().FullName == "GitUI.Compat.WinFormsControls.DataGridView" => "dataGrid",
            ListBox => "list",
            ContextMenu => "popup",
            _ when IsPopupPresenter(control) => "popup",
            ListBoxItem when IsComboBoxPopupItem(control) => "listItem",
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

        if (control is ListBoxItem { Content: IHostedRemote popupRemote })
        {
            // parity-scaffolding: the open ComboBox surface exposes the same display member
            // text as WinForms' native list, rather than the model's diagnostic ToString().
            return popupRemote.DisplayData;
        }

        if (control is ListBoxItem { Content: { } popupItem } && IsComboBoxPopupItem(control))
        {
            return popupItem.ToString();
        }

        object? content = GetPropertyValue(control, "Content");
        object? header = GetPropertyValue(control, "Header");
        string? text = control.Name == "btnRemoteColor"
            ? content as string
              ?? (content as TextBlock)?.Text
              ?? (content is null ? null : GetPropertyValue(content, "Text") as string)
            : GetPropertyValue(control, "Text") as string
            ?? content as string
            ?? (content as TextBlock)?.Text
            ?? header as string
            ?? (header as TextBlock)?.Text;
        if (control is ComboBox comboBox && string.IsNullOrEmpty(text))
        {
            text = comboBox.SelectedItem as string
                ?? (comboBox.SelectedItem as ComboBoxItem)?.Content as string;
        }

        return text is null
            ? string.Empty
            : (control is MenuItem or Button or Label || control.Name == "btnRemoteColor")
              && TranslationCompat.GetConvertMnemonics(control)
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
            // WinForms ListControl.SelectedValue stays null for item-backed ComboBoxes but is
            // populated for the data-bound protocol selector used by ForkAndCloneForm.
            ComboBox comboBox when control.Name == "ProtocolDropdownList" => comboBox.SelectedIndex >= 0,
            ComboBox comboBox when control.Name is "_NO_TRANSLATE_Remotes" or "RemoteRepositoryCombo" or "Url" => comboBox.SelectedIndex >= 0,
            ComboBox => false,
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

                    _fieldOwnerTypes.TryAdd(value, type.FullName ?? type.Name);
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
        bool isFileStatusToolbar = IsFileStatusToolbar(control);
        bool isFileViewerToolbar = IsFileViewerToolbar(control);
        bool isSemanticToolStrip = isFileStatusToolbar || isFileViewerToolbar;
        bool isSemanticToolStripItem = IsSemanticToolStripItem(control);
        bool isFileStatusListView = IsFileStatusListView(control);
        bool isFileStatusSplitter = IsFileStatusSplitter(control);
        bool isFileViewerTextEditor = IsFileViewerTextEditor(control);
        bool isFileViewerInternal = IsFileViewerInternal(control);
        bool isFileViewerPictureBox = IsFileViewerPictureBox(control);
        bool isLoadingControl = IsLoadingControl(control);
        bool isLoadingWaitSpinner = IsLoadingWaitSpinner(control);
        bool isSpellCheckAutoComplete = IsSpellCheckAutoComplete(control);
        bool isSpellCheckTextBox = IsSpellCheckTextBox(control);
        bool isDesignerLinkLabel = control is HyperlinkButton && IsDesignerMetadataControl(control);
        bool isSourceLabelSubstitute = IsSourceLabelSubstitute(control) || isDesignerLinkLabel;
        bool isWatermarkComboBox = IsFileStatusWatermarkComboBox(control);
        bool isSourceDataGrid = control.GetType().FullName == "GitUI.Compat.WinFormsControls.DataGridView";
        bool isSourcePictureBox = control.GetType().FullName == "GitUI.Compat.WinFormsControls.PictureBox";
        bool isLocalSourceFlowLayoutPanel = _root.GetType().FullName == "GitUI.CommandsDialogs.FormRemotes"
            && control.Name == "flowLayoutPanelSsh";
        bool isRemoteColorButton = _root.GetType().FullName == "GitUI.CommandsDialogs.FormRemotes"
            && control.Name == "btnRemoteColor";
        bool isDialogControlsPanel = _root.GetType().FullName == "GitUI.CommandsDialogs.FormPull"
            && control.Name == "ControlsPanel";
        bool isInheritedFormProcessContainer = IsInheritedFormProcessContainer(control);
        bool isSourceTransparentContainer = IsSourceTransparentContainer(control);
        bool hasSourceTransparentColors = HasSourceTransparentColors(control);
        bool hasSourceLightTransparentColors = HasSourceLightTransparentColors(control);
        bool isRepositoryHostDiscussion = IsRepositoryHostDiscussion(control);
        bool isMenuCaption = control.Classes.Contains("gitextensions-menu-caption");
        bool hasWinFormsTextBoxClientInset = control is TextBox && !isSpellCheckTextBox;
        bool isToolStripItem = isSemanticToolStripItem || control is MenuItem or Separator;
        Control semanticStateControl = IsFileStatusListView(control)
            ? GetActiveFileStatusListView(control) ?? control
            : control;
        bool isPopupRoot = isSurfaceRoot && IsPopupSurface(control);
        bool isComboBoxPopup = isPopupRoot && IsComboBoxPopup(control);
        bool isComboBoxPopupItem = IsComboBoxPopupItem(control);
        string? fieldName = isSurfaceRoot || isInheritedFormProcessContainer || isLocalSourceFlowLayoutPanel
            ? null
            : fieldNames.FirstOrDefault()
              ?? (control is MenuItem or Separator || string.IsNullOrEmpty(control.Name) ? null : control.Name);
        bool isDesignerMetadataControl = fieldName is not null && IsDesignerMetadataControl(control);
        Control? childSemanticParent = isSurfaceRoot || fieldName is not null || isInheritedFormProcessContainer
            ? control
            : semanticParent;
        string segment = isSurfaceRoot
            ? $"$root:{control.GetType().Name}"
            : isComboBoxPopupItem
                ? $"item[{ordinal}]"
            : fieldName ?? $"$unnamed[{ordinal}]:{control.GetType().Name}";
        string id = string.IsNullOrEmpty(parentId) ? segment : $"{parentId}/{segment}";
        DesignerLayoutMetadata? designerLayout = GetDesignerLayout(
            control,
            fieldName ?? (isLocalSourceFlowLayoutPanel ? control.Name : null));
        bool hasNativeListComposite = TryGetNativeListComposite(control, out Grid? nativeListComposite, out _);
        Rect bounds = boundsOverride
            ?? (hasNativeListComposite
                ? GetSemanticBounds(nativeListComposite!, semanticParent)
                : GetSemanticBounds(semanticStateControl, semanticParent));
        bool semanticVisible = IsSemanticallyVisible(control, semanticStateControl) && ancestorSemanticVisible;
        bool childSemanticVisible = semanticVisible
            && (control is not MenuItem menuItem || menuItem.IsSubMenuOpen)
            && (control is not TabItem tabItem || !isNativeTabPage || tabItem.IsSelected);
        IReadOnlyList<CaptureNode> children = GetSemanticChildren(control)
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
            Type = isComboBoxPopupItem && control is ListBoxItem { Content: { } popupItem }
                ? popupItem.GetType().FullName ?? popupItem.GetType().Name
                : isSourceDataGrid
                    ? "System.Windows.Forms.DataGridView"
                    : isSourcePictureBox
                        ? "System.Windows.Forms.PictureBox"
                        : isLocalSourceFlowLayoutPanel
                            ? "System.Windows.Forms.FlowLayoutPanel"
                        : isRemoteColorButton
                            ? "System.Windows.Forms.Button"
                            : isInheritedFormProcessContainer
                                ? control.Name == "MainPanel"
                                    ? "System.Windows.Forms.Panel"
                                    : "System.Windows.Forms.FlowLayoutPanel"
                : control.GetType().FullName ?? control.GetType().Name,
            ControlKind = isRemoteColorButton
                ? "button"
                : isRepositoryHostDiscussion || isDesignerLinkLabel ? "control" : GetControlKind(control),
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
                Width = ToPixel(isNativeListView || hasWinFormsTextBoxClientInset ? Math.Max(0, bounds.Width - 4) : bounds.Width),
                Height = ToPixel(isNativeListView || hasWinFormsTextBoxClientInset ? Math.Max(0, bounds.Height - 4) : bounds.Height)
            },
            ClientSizeDip = new CaptureSizeF
            {
                Width = ToDecimal(isNativeListView || hasWinFormsTextBoxClientInset ? Math.Max(0, bounds.Width - 4) : bounds.Width),
                Height = ToDecimal(isNativeListView || hasWinFormsTextBoxClientInset ? Math.Max(0, bounds.Height - 4) : bounds.Height)
            },
            ItemHeightDip = isRevisionGridView
                ? ReadRevisionGridItemHeight(control)
                : isComboBoxPopup ? 15 : null,
            Padding = ReadThicknessPair(isComboBoxPopup || isComboBoxPopupItem
                ? default(Thickness)
                : isInheritedFormProcessContainer
                    ? new Thickness(control.Name == "MainPanel" ? 9 : 5)
                : designerLayout?.Padding
                ?? (isPopupRoot ? new Thickness(33, 2, 1, 2) : (Thickness?)null)
                ?? (isSemanticToolStrip ? new Thickness(0, 0, 1, 0) : (Thickness?)null)
                ?? (isSemanticToolStripItem || control is Separator || isFileStatusListView || isFileStatusSplitter ? default(Thickness) : (Thickness?)null)
                ?? (control is MenuItem ? new Thickness(0, 1, 0, 1) : (Thickness?)null)
                ?? (_usesDesignerLayoutMetadata
                    ? GetDefaultDesignerPadding(control)
                    : isNativeTabPage || isNativeButton
                        ? default(Thickness)
                        : GetPropertyValue(control, "Padding"))),
            Margin = ReadThicknessPair(isComboBoxPopup || isComboBoxPopupItem
                ? default(Thickness)
                : isInheritedFormProcessContainer ? default(Thickness)
                : designerLayout?.Margin
                ?? (isSemanticToolStrip || isFileStatusListView ? default(Thickness) : (Thickness?)null)
                ?? (isSemanticToolStripItem
                    ? control is Separator
                        ? default(Thickness)
                        : control.Name == "encodingToolStripComboBox"
                            ? new Thickness(1, 0)
                            : new Thickness(0, 1, 0, 2)
                    : (Thickness?)null)
                ?? (control is MenuItem or Separator ? default(Thickness) : (Thickness?)null)
                ?? (isFileStatusSplitter ? new Thickness(3, 0) : (Thickness?)null)
                ?? (_usesDesignerLayoutMetadata
                    ? GetDefaultDesignerMargin(control)
                    : isNativeButton
                        ? new Thickness(3)
                        : isRevisionGrid || isRevisionGridView
                            ? new Thickness(3)
                        : hasNativeListComposite ? nativeListComposite!.Margin : control.Margin)),
            Font = (isMenuCaption
                ? ReadFont(control) is { } menuCaptionFont
                    ? menuCaptionFont with { Style = ["Italic"] }
                    : null
                : isSpellCheckTextBox && IsSpellCheckWatermarkVisible(control)
                ? ReadFont(control) is { } spellCheckWatermarkFont
                    ? spellCheckWatermarkFont with { Style = ["Italic"] }
                    : null
                : IsFileStatusWatermarkVisible(control)
                    ? ReadFont(control) is { } fileStatusWatermarkFont
                        ? fileStatusWatermarkFont with { Style = ["Italic"] }
                        : null
                : isInheritedFormProcessContainer
                    ? ReadFont(_root)
                : ReadFont(isLocalSourceFlowLayoutPanel
                           || IsDetachedMenuItem(control)
                           || isSemanticToolStrip
                           || isSemanticToolStripItem
                           || isSourceTransparentContainer
                    ? _root
                    : control))
                // WinForms controls inherit a concrete Font even when the Avalonia layout
                // counterpart is a non-templated Panel without font properties of its own.
                ?? (fieldName is not null ? ReadFont(_root) : null),
            Colors = isComboBoxPopup || isComboBoxPopupItem
                ? ReadComboBoxPopupColors()
                : isSemanticToolStrip
                    ? ReadToolStripColors(control, isItem: false, transparentBackground: isFileStatusToolbar)
                    : isSemanticToolStripItem
                        ? ReadToolStripColors(control, isItem: true, transparentBackground: IsTransparentFileStatusToolbarItem(control))
                        : isFileStatusListView
                            ? ReadFileStatusListViewColors(semanticStateControl)
                            : isWatermarkComboBox
                                ? IsFileStatusWatermarkVisible(control)
                                    ? ReadFileStatusWatermarkColors(control)
                                    : ReadSourceInputColors(control)
                                : isRepositoryHostDiscussion
                                    ? ReadRepositoryHostDiscussionColors(control)
                                    : isSpellCheckAutoComplete
                                        ? ReadNativeSelectionColors(control, "GitExtensionsWindowBackgroundBrush")
                                        : isSpellCheckTextBox
                                            ? ReadSpellCheckTextBoxColors(control)
                                            : isSourceLabelSubstitute
                                                ? ReadSourceLabelSubstituteColors(control)
                                                : isRemoteColorButton
                                                    ? ReadSourceDesignerButtonColors(control)
                                                 : isSourceDataGrid
                                                     ? ReadSourceDataGridColors(control)
                                                 : isDialogControlsPanel
                                                     ? ReadDialogControlsPanelColors(control)
                                                : isDesignerMetadataControl && control is Button or CheckBox or RadioButton
                                                    ? ReadSourceDesignerButtonColors(control)
                                                : isDesignerMetadataControl && control is TextBox or ComboBox or NumericUpDown
                                                    ? ReadSourceInputColors(control)
                                                    : hasSourceTransparentColors
                                                        ? ReadTransparentContainerColors(control)
                                                        : hasSourceLightTransparentColors
                                                            ? ReadLightTransparentColors(control)
                                                            : isDesignerMetadataControl && control.Name == "lblHeaderLine2"
                                                                ? ReadSourceDesignerColors(control) with { Border = null }
                                                            : isDesignerMetadataControl && control is TextBlock or Label or TabItem
                                                                ? ReadSourceDesignerColors(control)
                                                                : isFileViewerTextEditor
                                                                    ? ReadFileViewerTextEditorColors()
                                                                    : isFileViewerPictureBox
                                                                        ? ReadFileViewerPictureBoxColors(control)
                                                                        : ReadColors(semanticStateControl),
            BorderStyle = isSemanticToolStrip || isSemanticToolStripItem || isRepositoryHostDiscussion || isComboBoxPopupItem
                ? null
                : designerLayout?.BorderStyle
                ?? (isFileStatusListView || isFileStatusSplitter || isFileViewerPictureBox || isLoadingControl ? "None" : null)
                ?? (isSourcePictureBox || isInheritedFormProcessContainer || control.Name == "PanelLeftImage"
                    || control.Name?.StartsWith("folderBrowserButton", StringComparison.Ordinal) == true ? "None" : null)
                ?? (_usesDesignerLayoutMetadata && control is Image ? "None" : null)
                ?? (isSpellCheckAutoComplete ? "FixedSingle" : null)
                ?? (isSpellCheckTextBox || isSourceLabelSubstitute || isSourceTransparentContainer || isFileViewerInternal ? "None" : null)
                ?? (isFileViewerTextEditor ? "None" : null)
                ?? (isSourceDataGrid ? "FixedSingle" : null)
                ?? (_usesDesignerLayoutMetadata
                    ? GetDefaultDesignerBorderStyle(control)
                    : isRevisionGrid || isRevisionGridView || isNativeTabPage
                ? "None"
                : isNativeListView
                    ? "Fixed3D"
                    : GetPropertyValue(control, "BorderStyle")?.ToString()),
            FlatStyle = isComboBoxPopup
                ? "Standard"
                : isWatermarkComboBox || isSemanticToolStripItem || isSourceLabelSubstitute
                ? null
                : isRemoteColorButton
                    ? (IsDarkTheme() ? "Flat" : "Standard")
                : designerLayout?.FlatStyle
                  ?? (isDesignerMetadataControl
                      ? GetDefaultDesignerFlatStyle(control)
                      : isNativeButton ? (IsDarkTheme() ? "Flat" : "Standard") : null),
            // WinForms exposes the native BorderStyle but not a numeric border width for
            // Designer controls. Pixel evidence still retains the rendered border itself.
            BorderWidthDip = isComboBoxPopup
                ? 1
                : isDesignerMetadataControl || isPopupRoot || isComboBoxPopupItem || isNativeListView || isNativeTabControl || isNativeTabPage || isNativeButton
                || isSemanticToolStrip || isSemanticToolStripItem || isFileViewerTextEditor
                || isSpellCheckAutoComplete || isSpellCheckTextBox || isSourceLabelSubstitute || isWatermarkComboBox
                || isRepositoryHostDiscussion
                ? null
                : ReadBorderWidth(control),
            CornerRadiusDip = isSourceLabelSubstitute || isComboBoxPopupItem || isDesignerMetadataControl
                ? null
                : ReadCornerRadius(control),
            Anchor = isComboBoxPopup || isComboBoxPopupItem ? [] : designerLayout?.Anchor
                ?? (isInheritedFormProcessContainer ? new[] { "Top", "Left" } : null)
                ?? (isFileStatusToolbar || isFileStatusSplitter ? new[] { "Top", "Left" } : null)
                ?? (isFileViewerToolbar ? new[] { "Top", "Right" } : null)
                ?? (isToolStripItem ? [] : (string[]?)null)
                ?? (isFileStatusListView ? new[] { "Top", "Bottom", "Left", "Right" } : null)
                ?? (_usesDesignerLayoutMetadata
                    ? ["Top", "Left"]
                    : isRevisionGrid || isRevisionGridView || isNativeListView || isNativeTabControl || isNativeTabPage
                ? ["Top", "Left"]
                : []),
            Dock = isComboBoxPopup || isComboBoxPopupItem ? null : isLocalSourceFlowLayoutPanel ? "Fill" : designerLayout?.Dock
                ?? (isInheritedFormProcessContainer ? control.Name == "MainPanel" ? "Fill" : "Bottom" : null)
                ?? (isToolStripItem
                    ? null
                    : isFileStatusToolbar || isFileStatusSplitter
                        ? "Top"
                        : isFileViewerToolbar
                            ? "None"
                        : isFileStatusListView
                            ? "None"
                            : isLoadingControl || isLoadingWaitSpinner
                                ? "Fill"
                            : _usesDesignerLayoutMetadata
                                ? "None"
                                : isRevisionGrid || isNativeTabPage || isNativeButton
                                    ? isNativeButton && control.Name == "buttonBrowse" ? "Fill" : "None"
                                    : isRevisionGridView || isNativeListView || isNativeTabControl ? "Fill" : null),
            AutoSize = isComboBoxPopupItem ? false : isLocalSourceFlowLayoutPanel ? true : designerLayout?.AutoSize
                ?? (isSourcePictureBox ? true : (bool?)null)
                ?? (isInheritedFormProcessContainer ? control.Name == "ControlsPanel" : (bool?)null)
                ?? (isSemanticToolStrip || isToolStripItem ? true : (bool?)null)
                ?? (isFileStatusListView || isFileStatusSplitter || isSpellCheckTextBox ? false : (bool?)null)
                ?? (_usesDesignerLayoutMetadata
                    ? GetDefaultDesignerAutoSize(control)
                    : isRevisionGrid || isRevisionGridView || isNativeListView || isNativeTabControl || isNativeTabPage || isNativeButton
                ? false
                : control is MenuItem or Separator || isPopupRoot ? true : null),
            Alignment = isComboBoxPopupItem || isSpellCheckTextBox ? null
                : isRemoteColorButton ? "MiddleCenter"
                : control.Name == "lblHeaderLine2" ? "TopLeft"
                : designerLayout?.Alignment
                ?? (isSourceLabelSubstitute ? "TopLeft" : null)
                ?? (isToolStripItem ? "MiddleCenter" : null)
                ?? (isFileStatusSplitter ? "TopLeft" : null)
                ?? (_usesDesignerLayoutMetadata
                    ? GetDefaultDesignerAlignment(control)
                    : isNativeButton
                ? "MiddleCenter"
                : isRevisionGrid || isRevisionGridView || isNativeTabControl || isNativeTabPage || isPopupRoot || isSpellCheckTextBox
                ? null
                : control is MenuItem or Separator ? "MiddleCenter" : GetAlignment(control)),
            Text = GetText(control),
            ToolTip = GetToolTip(control),
            TranslationSource = fieldName,
            TabIndex = isComboBoxPopupItem ? null
                : isSurfaceRoot && !isPopupRoot ? 0
                : isSemanticToolStrip ? 0
                : isFileStatusListView ? 9
                : isLoadingControl
                    ? _root.GetType().FullName switch
                    {
                        "GitUI.CommandsDialogs.RepoHosting.CreatePullRequestForm" => 9,
                        "GitUI.CommandsDialogs.RepoHosting.ViewPullRequestsForm" => 7,
                        _ => KeyboardNavigation.GetTabIndex(control),
                    }
                : isLoadingWaitSpinner ? 0
                : isFileStatusSplitter ? 8
                : isRevisionGrid || isRevisionGridView
                ? 0
                : isInheritedFormProcessContainer
                    ? control.Name == "MainPanel" ? 1 : 0
                : isLocalSourceFlowLayoutPanel ? 3
                : isSemanticToolStripItem || control is MenuItem or Separator || isPopupRoot ? null : KeyboardNavigation.GetTabIndex(control),
            TabStop = isComboBoxPopupItem ? null
                : isSurfaceRoot && !isPopupRoot
                ? true
                : isDesignerMetadataControl
                ? GetSourceDesignerTabStop(control)
                : isSpellCheckAutoComplete || isFileViewerInternal || isFileViewerTextEditor || isLoadingControl || isLoadingWaitSpinner || IsSourceTabStopContainer(control)
                ? true
                : isSemanticToolStrip || isFileStatusSplitter
                ? false
                : isNativeTabPage
                ? false
                : isFileStatusListView || isRevisionGrid || isRevisionGridView || isNativeListView || isSourceDataGrid || isNativeTabControl
                ? true
                : isSemanticToolStripItem || control is MenuItem or Separator || isPopupRoot
                    ? null
                    : control.Focusable && KeyboardNavigation.GetIsTabStop(control),
            Enabled = control is Separator ? false : semanticStateControl.IsEffectivelyEnabled,
            Visible = isNativeTabPage
                ? ((TabItem)control).IsSelected && ancestorSemanticVisible
                : semanticVisible,
            Focused = IsRepositoryHostSourceFocusedState(control, isPopupRoot)
                ? true
                : isPopupRoot || isComboBoxPopupItem ? false : IsFocused(semanticStateControl),
            ReadOnly = isComboBoxPopup
                ? true
                : isRevisionGridView
                ? true
                : isFileViewerTextEditor || control.Name == "_diffViewer"
                    ? null
                    : GetNullableBoolProperty(control, "IsReadOnly"),
            CheckState = control switch
            {
                MenuItem checkedMenuItem => checkedMenuItem.IsChecked ? "Checked" : "Unchecked",
                CheckBox checkBox => checkBox.IsChecked switch
                {
                    true => "Checked",
                    false => "Unchecked",
                    null => "Indeterminate"
                },
                _ => null
            },
            Selected = (isSemanticToolStripItem && control is not Separator) || isSpellCheckAutoComplete || isWatermarkComboBox
                ? false
                : GetSelected(control),
            Expanded = isRevisionGrid || isRevisionGridView
                ? null
                : isFileStatusListView
                    ? ReadFileStatusListViewExpanded(semanticStateControl)
                : isPopupRoot
                    ? true
                    : isSemanticToolStripItem && GetPropertyValue(control, "Flyout") is not null
                        ? false
                        : GetExpanded(control),
            Columns = ReadColumns(control),
            Children = children
        };
    }

    private DesignerLayoutMetadata? GetDesignerLayout(Control control, string? fieldName)
    {
        if (!_usesDesignerLayoutMetadata || fieldName is null)
        {
            return null;
        }

        string ownerType = _fieldOwnerTypes.GetValueOrDefault(control)
            ?? _root.GetType().FullName
            ?? _root.GetType().Name;
        return WinFormsInputMetadata.LayoutByType.TryGetValue(ownerType, out IReadOnlyList<DesignerLayoutMetadata>? controls)
            ? controls.FirstOrDefault(item => item.FieldName == fieldName) is { FieldName: not null } metadata
                ? metadata
                : null
            : null;
    }

    private bool IsDesignerMetadataControl(Control control)
        => _usesDesignerLayoutMetadata
           && _fieldOwnerTypes.TryGetValue(control, out string? ownerType)
           && (WinFormsInputMetadata.ByType.ContainsKey(ownerType)
               || WinFormsInputMetadata.LayoutByType.ContainsKey(ownerType));

    private static Thickness GetDefaultDesignerMargin(Control control)
        => control is TextBlock or Label or HyperlinkButton ? new Thickness(3, 0) : new Thickness(3);

    private static Thickness GetDefaultDesignerPadding(Control control)
        => control is HeaderedContentControl ? new Thickness(3) : default;

    private static bool GetDefaultDesignerAutoSize(Control control)
        => control is TextBox;

    private static bool GetSourceDesignerTabStop(Control control)
        => control switch
        {
            RadioButton radioButton => radioButton.IsChecked == true,
            Button or HyperlinkButton or CheckBox or TextBox or ComboBox or ListBox or NumericUpDown or TabControl => true,
            _ when control.Name is "PanelLeftImage" or "folderBrowserButton1" or "folderBrowserButtonUrl"
                or "folderBrowserButtonPushUrl" or "btnRemoteColor" => true,
            _ => false
        };

    private static string? GetDefaultDesignerAlignment(Control control)
        => control switch
        {
            CheckBox or RadioButton => "MiddleLeft",
            HyperlinkButton => "TopLeft",
            Button => "MiddleCenter",
            TextBlock or Label => "TopLeft",
            TextBox or NumericUpDown => "Left",
            _ => null
        };

    private string? GetDefaultDesignerFlatStyle(Control control)
        => control switch
        {
            CheckBox or RadioButton => "Standard",
            Button => IsDarkTheme() ? "Flat" : "Standard",
            _ => null
        };

    private static string? GetDefaultDesignerBorderStyle(Control control)
        => control switch
        {
            TextBox or NumericUpDown => "Fixed3D",
            ListBox => "Fixed3D",
            Panel or Decorator or TabItem or Image or TextBlock or Label or HyperlinkButton => "None",
            _ when control.Name == "browseForCloneToDirbtn" => "None",
            _ when control.GetType().FullName == "GitUI.SpellChecker.EditNetSpell" => "None",
            _ => null
        };

    private static Rect GetSemanticBounds(Control control, Control? semanticParent)
    {
        if (semanticParent is not null
            && IsOverlayPopupHost(semanticParent)
            && control is MenuItem or Separator
            && control.TranslatePoint(default, semanticParent) is Point popupItemOrigin)
        {
            // parity-scaffolding: Fluent reserves its submenu border outside the item canvas;
            // ToolStrip overlays the leading border and retains two vertical canvas insets.
            return new Rect(
                popupItemOrigin.X - 1,
                popupItemOrigin.Y + 1,
                control.Bounds.Width,
                control.Bounds.Height);
        }

        if (IsComboBoxPopupItem(control)
            && semanticParent is not null
            && control.TranslatePoint(default, semanticParent) is Point itemOrigin
            && semanticParent.GetVisualDescendants().OfType<ListBoxItem>().FirstOrDefault() is { } firstItem
            && firstItem.TranslatePoint(default, semanticParent) is Point firstItemOrigin)
        {
            // parity-scaffolding: ComboLBox item bounds span its complete one-pixel-bordered
            // popup client. Avalonia positions the container inside renderer padding.
            return new Rect(0, itemOrigin.Y - firstItemOrigin.Y, semanticParent.Bounds.Width, control.Bounds.Height);
        }

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

        if (semanticParent is Grid { Name: "splitContainer3" }
            && Grid.GetRow(control) > 0)
        {
            // parity-scaffolding: SplitterPanel2 is flattened on the WinForms side; report its
            // child relative to that semantic panel rather than Avalonia's second Grid row.
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
        if (IsRepositoryHostDiscussion(control))
        {
            // parity-scaffolding: WinForms exposes the read-only WebBrowser as one semantic
            // control; Avalonia's native row renderer is internal presentation, not product UI.
            return [];
        }

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

        if (GetPropertyValue(control, "Flyout") is MenuFlyout menuFlyout)
        {
            // parity-scaffolding: ToolStrip drop-down items remain children of their owning
            // item while closed; Avalonia stores the equivalent controls in a detached Flyout.
            return menuFlyout.Items.OfType<Control>();
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
                .Where(child => child is MenuItem or Separator or ListBoxItem)
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

        if (control is Button or TextBox or ComboBox or NumericUpDown or ListBoxItem)
        {
            return [];
        }

        IEnumerable<Control> children = control.GetLogicalChildren()
            .OfType<Control>()
            .Where(child => child.TemplatedParent is null
                            && child.GetType().Name != "TopLevelHost")
            .Where(child => !IsSearchResultOverlay(child))
            .Where(child => !IsFileStatusAlternateView(child))
            .Where(child => !IsLoadingControl(child));

        if (ReferenceEquals(control, _root) && control is Window)
        {
            // parity-scaffolding: WinForms reparents SearchControl's result list to its owning
            // form. Avalonia keeps the same overlay in the control's logical tree, so lift it
            // to the emitted window root and measure it against that semantic owner.
            children = control.GetLogicalDescendants()
                .OfType<Control>()
                .Where(IsLoadingControl)
                .Concat(children)
                .Concat(
                control.GetLogicalDescendants()
                    .OfType<Control>()
                    .Where(IsSearchResultOverlay));
        }

        return children;
    }

    private IEnumerable<Control> GetSemanticChildren(Control control)
        => GetCaptureChildren(control).SelectMany(ExpandSemanticChild);

    private IEnumerable<Control> ExpandSemanticChild(Control child)
    {
        if (IsSemanticLayoutWrapper(child))
        {
            // parity-scaffolding: Avalonia needs layout owners around source controls that
            // WinForms positions directly. Flatten those owners without hiding their content.
            return GetCaptureChildren(child).SelectMany(ExpandSemanticChild);
        }

        if (IsRendererOnlyControl(child))
        {
            // parity-scaffolding: these controls paint a source control or implement framework
            // layout; they are not independently named controls in the WinForms product tree.
            return [];
        }

        return [child];
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
                ? ResolveResourceArgb("GitExtensionsKnownColorControlDarkBrush")
                  ?? ResolveResourceArgb("GitExtensionsControlBorderBrush")
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
            return ReadFrameworkColumns(columns, control);
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
                    double widthDip = definition.Width.IsAbsolute
                        ? definition.Width.Value
                        : definition.ActualWidth > 0
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

    private IReadOnlyList<CaptureColumn> ReadFrameworkColumns(IEnumerable columns, Control owner)
        => columns.Cast<object>()
            .Select((column, index) =>
            {
                double widthDip = GetPropertyValue(column, "SourceWidth") as double?
                    ?? GetPropertyValue(column, "ActualWidth") as double?
                    ?? GetPropertyValue(GetPropertyValue(column, "Width")!, "Value") as double?
                    ?? 0;
                bool isSourceColumn = column.GetType().Namespace == "GitUI.Compat.WinFormsControls";
                bool isListColumn = isSourceColumn && column.GetType().Name == "ColumnHeader";
                return new CaptureColumn
                {
                    FieldName = GetFieldNames(column).FirstOrDefault(),
                    Name = GetPropertyValue(column, "Name") as string,
                    Type = isSourceColumn
                        ? $"System.Windows.Forms.{column.GetType().Name}"
                        : column.GetType().FullName ?? column.GetType().Name,
                    Index = index,
                    DisplayIndex = GetPropertyValue(column, "DisplayIndex") as int? ?? index,
                    WidthPx = ToPixel(widthDip),
                    WidthDip = ToDecimal(widthDip),
                    Visible = isSourceColumn || (GetNullableBoolProperty(column, "IsVisible") ?? widthDip > 0),
                    Resizable = GetNullableBoolProperty(column, "CanUserResize"),
                    SortMode = isListColumn
                        ? null
                        : GetPropertyValue(column, "SortMode")?.ToString()
                               ?? GetPropertyValue(column, "SortMemberPath")?.ToString(),
                    Alignment = isListColumn
                        ? "Left"
                        : GetPropertyValue(column, "CellAlignment")?.ToString()
                                ?? GetPropertyValue(column, "HorizontalContentAlignment")?.ToString(),
                    HeaderText = GetPropertyValue(column, "Header")?.ToString()
                        ?? (isSourceColumn ? string.Empty : null),
                    HeaderAlignment = isListColumn
                        ? "Left"
                        : isSourceColumn ? "NotSet"
                        : GetPropertyValue(column, "HorizontalHeaderContentAlignment")?.ToString(),
                    Colors = isListColumn
                        ? ReadColors(owner)
                        : isSourceColumn ? ReadSourceDataGridColumnColors() : EmptyColors()
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

    private static bool IsFileStatusToolbar(Control control)
        => control is StackPanel { Name: "Toolbar" }
           && control.GetLogicalAncestors().OfType<FileStatusList>().Any();

    private static bool IsFileStatusToolbarItem(Control control)
        => control.Parent is StackPanel toolbar && IsFileStatusToolbar(toolbar);

    private static bool IsTransparentFileStatusToolbarItem(Control control)
        => IsFileStatusToolbarItem(control)
           && control.Name is not ("btnCollapseGroups" or "sepRefresh" or "btnRefresh");

    private static bool IsFileViewerToolbar(Control control)
        => control is Border { Name: "fileviewerToolbar" }
           && control.GetLogicalAncestors().Any(ancestor => ancestor.GetType().FullName == "GitUI.Editor.FileViewer");

    private static bool IsFileViewerToolbarItem(Control control)
        => control.Parent is StackPanel stackPanel
           && stackPanel.Parent is Control toolbar
           && IsFileViewerToolbar(toolbar);

    private static bool IsSemanticToolStrip(Control control)
        => IsFileStatusToolbar(control) || IsFileViewerToolbar(control);

    private static bool IsSemanticToolStripItem(Control control)
        => IsFileStatusToolbarItem(control) || IsFileViewerToolbarItem(control);

    private static bool IsFileViewerTextEditor(Control control)
        => control.Name == "TextEditor"
           && control.GetLogicalAncestors().Any(ancestor => ancestor.GetType().FullName == "GitUI.Editor.FileViewerInternal");

    private static bool IsFileViewerInternal(Control control)
        => control.Name == "internalFileViewer"
           && control.GetType().FullName == "GitUI.Editor.FileViewerInternal";

    private static bool IsFileViewerPictureBox(Control control)
        => control.Name == "PictureBox"
           && control.GetLogicalAncestors().Any(ancestor => ancestor.GetType().FullName == "GitUI.Editor.FileViewer");

    private static bool IsLoadingControl(Control control)
        => control.GetType().FullName == "GitUI.UserControls.RevisionGrid.LoadingControl";

    private bool IsLoadingWaitSpinner(Control control)
        => GetFieldNames(control).Contains("_waitSpinner", StringComparer.Ordinal)
           && control.GetLogicalAncestors().OfType<Control>().Any(IsLoadingControl);

    private static bool IsSpellCheckAutoComplete(Control control)
        => control is ListBox { Name: "AutoComplete" }
           && control.GetLogicalAncestors().Any(ancestor => ancestor.GetType().FullName == "GitUI.SpellChecker.EditNetSpell");

    private static bool IsSpellCheckTextBox(Control control)
        => control is TextBox { Name: "TextBox" }
           && control.GetLogicalAncestors().Any(ancestor => ancestor.GetType().FullName == "GitUI.SpellChecker.EditNetSpell");

    private static bool IsSpellCheckWatermarkVisible(Control control)
        => control is TextBox textBox
           && string.IsNullOrEmpty(textBox.Text)
           && !textBox.IsKeyboardFocusWithin
           && control.GetLogicalAncestors()
               .OfType<Control>()
               .Any(ancestor => ancestor.Name == "_postCommentText")
           && IsInSelectedTab(control);

    private static bool IsFileStatusWatermarkVisible(Control control)
        => control.Name == "cboFilterComboBox"
           && string.IsNullOrEmpty(GetText(control))
           && control.Bounds.Width > 0;

    private bool IsRepositoryHostSourceFocusedState(Control control, bool isPopupRoot)
    {
        ComboBox[] openComboBoxes = _root.GetLogicalDescendants()
            .OfType<ComboBox>()
            .Where(comboBox => comboBox.IsDropDownOpen)
            .ToArray();
        string? rootType = _root.GetType().FullName;
        if (rootType == "GitUI.CommandsDialogs.RepoHosting.CreatePullRequestForm")
        {
            bool sourceBranchOpen = openComboBoxes.Any(comboBox => comboBox.Name == "_yourBranchesCB");
            bool targetBranchOpen = openComboBoxes.Any(comboBox => comboBox.Name == "_pullReqTargetsCB");
            return (control.Name == "_yourBranchesCB" && (sourceBranchOpen || targetBranchOpen))
                   || (isPopupRoot && sourceBranchOpen);
        }

        if (rootType == "GitUI.CommandsDialogs.RepoHosting.ForkAndCloneForm")
        {
            return control.Name == "myReposLV"
                   && openComboBoxes.Any(comboBox => comboBox.Name == "ProtocolDropdownList");
        }

        return rootType == "GitUI.CommandsDialogs.RepoHosting.ViewPullRequestsForm"
               && control.Name == "_fetchBtn"
               && openComboBoxes.Any(comboBox => comboBox.Name == "_selectHostedRepoCB");
    }

    private static bool IsInSelectedTab(Control control)
        => control.GetLogicalAncestors().OfType<TabItem>().All(tabItem => tabItem.IsSelected);

    private static bool IsSourceLabelSubstitute(Control control)
        => control.Name is "DeleteFilterButton" or "DeleteSearchButton" or "_NO_TRANSLATE_lblShowPreview";

    private static bool IsFileStatusWatermarkComboBox(Control control)
        => (control.Name is "cboFilterComboBox" or "cboFindInCommitFilesGitGrep")
           && control.GetLogicalAncestors().Any(ancestor => ancestor.GetType().FullName == "GitUI.FileStatusList");

    private bool IsSourceTransparentContainer(Control control)
        => IsViewPullRequestsTree(control)
           && control.Name is "tableLayoutPanel2" or "tableLayoutPanel3"
               or "flowLayoutPanel2" or "flowLayoutPanel3"
               or "splitContainer2" or "splitContainer3" or "_fileStatusList" or "_diffViewer"
               or "lblSplitter";

    private bool HasSourceTransparentColors(Control control)
        => IsSourceTransparentContainer(control)
           || (IsViewPullRequestsTree(control)
               && control.Name is "_fetchBtn" or "_addAndFetchBtn" or "_closePullRequestBtn"
                   or "_chooseRepo" or "internalFileViewer");

    private bool IsViewPullRequestsTree(Control control)
        => _root.GetType().FullName == "GitUI.CommandsDialogs.RepoHosting.ViewPullRequestsForm"
           || control.GetLogicalAncestors().Any(
               ancestor => ancestor.GetType().FullName == "GitUI.CommandsDialogs.RepoHosting.ViewPullRequestsForm");

    private static bool HasSourceLightTransparentColors(Control control)
        => (control.GetLogicalAncestors().Any(
                ancestor => ancestor.GetType().FullName == "GitUI.CommandsDialogs.RepoHosting.ViewPullRequestsForm")
            && control.Name is "_refreshCommentsBtn" or "_postComment" or "_postCommentText")
           || (control.GetLogicalAncestors().Any(
                   ancestor => ancestor.GetType().FullName == "GitUI.CommandsDialogs.RepoHosting.ForkAndCloneForm")
               && control.Name is "searchBtn" or "getFromUserBtn" or "forkBtn" or "openGitupPageBtn");

    private bool IsSourceTabStopContainer(Control control)
        => (IsSourceTransparentContainer(control)
            && (control.Name is "splitContainer2" or "splitContainer3" or "_fileStatusList" or "_diffViewer"))
           // Composite Avalonia controls delegate focus to their inner editor/button. Preserve
           // the source UserControl/ComboBox/NumericUpDown tab contract at the semantic owner.
           || control.Name is "_bodyTB" or "_postCommentText" or "addUpstreamRemoteAsCB"
               or "browseForCloneToDirbtn" or "depthUpDown" or "cboFilterComboBox"
               or "cboFindInCommitFilesGitGrep";

    private static bool IsRepositoryHostSplit(Control control)
        => control is Grid { Name: "splitContainer2" or "splitContainer3" }
           && (control.GetType().FullName == "GitUI.CommandsDialogs.RepoHosting.ViewPullRequestsForm"
               || control.GetLogicalAncestors().Any(
                   ancestor => ancestor.GetType().FullName == "GitUI.CommandsDialogs.RepoHosting.ViewPullRequestsForm"));

    private bool IsSemanticLayoutWrapper(Control control)
        => (_usesDesignerLayoutMetadata
            && control is Panel
            && string.IsNullOrEmpty(control.Name)
            && GetFieldNames(control).Count == 0)
           || (control is Grid
                 && string.IsNullOrEmpty(control.Name)
                 && control.Parent?.GetType().FullName == "GitUI.Compat.WinFormsControls.FlowLayoutPanel")
           || (control is StackPanel
                && string.IsNullOrEmpty(control.Name)
                && control.Parent?.GetType().FullName == "GitUI.Compat.WinFormsControls.FlowLayoutPanel")
           || control.Name == "columnsGrid"
           || control.Name == "FindInCommitFilesGitGrepPanel"
           || (control is StackPanel
               && control.Parent is Control parent
               && IsFileViewerToolbar(parent));

    private static bool IsRendererOnlyControl(Control control)
        => control.Name == "ImagePreview"
           || (control.GetType().Namespace == "GitUI.Compat.WinFormsControls"
                && (control.GetType().Name == "ColumnHeader"
                    || control.GetType().Name.EndsWith("Column", StringComparison.Ordinal)))
           || (control is Image
               && control.Parent?.GetType().FullName == "GitUI.Compat.WinFormsControls.PictureBox")
           || control.GetType().FullName == "GitUI.SpellChecker.SpellCheckAdorner"
           || (control is GridSplitter
               && control.Parent is Control parent
               && IsRepositoryHostSplit(parent));

    private static bool IsFileStatusListView(Control control)
        => control.Name == "FileStatusListView"
           && control.GetLogicalAncestors().OfType<FileStatusList>().Any();

    private static bool IsRepositoryHostDiscussion(Control control)
        => control is ListBox { Name: "_discussionWB" }
           && control.GetLogicalAncestors().Any(
               ancestor => ancestor.GetType().FullName == "GitUI.CommandsDialogs.RepoHosting.ViewPullRequestsForm");

    private static bool IsFileStatusSplitter(Control control)
        => control is TextBlock { Name: "lblSplitter" }
           && control.GetLogicalAncestors().OfType<FileStatusList>().Any();

    private static bool IsFileStatusAlternateView(Control control)
        => control.Name is "lstFiles" or "tvDiffFiles" or "tvFiles"
           && control.GetLogicalAncestors().OfType<FileStatusList>().Any();

    private static Control? GetActiveFileStatusListView(Control control)
        => control.GetLogicalAncestors()
            .OfType<FileStatusList>()
            .FirstOrDefault()?
            .GetLogicalDescendants()
            .OfType<Control>()
            .FirstOrDefault(candidate => IsFileStatusAlternateView(candidate) && candidate.IsVisible);

    private static bool IsSemanticallyVisible(Control control, Control semanticStateControl)
    {
        if (control is MenuItem or Separator && TopLevel.GetTopLevel(control) is null)
        {
            return false;
        }

        // parity-scaffolding: flattened Avalonia layout owners still participate in source
        // visibility. A hidden WinForms control cannot become visible merely because its
        // unnamed/Grid wrapper was omitted from the semantic tree.
        return semanticStateControl.IsVisible
               && control.GetLogicalAncestors().OfType<Control>().All(ancestor => ancestor.IsVisible);
    }

    private static bool IsComboBoxPopup(Control control)
        => control.GetVisualDescendants().OfType<ListBoxItem>().Any();

    private static bool IsComboBoxPopupItem(Control control)
        => control is ListBoxItem
           && control.GetVisualAncestors().OfType<Control>().Any(
               ancestor => IsPopupPresenter(ancestor) || IsOverlayPopupHost(ancestor));

    private CaptureColors ReadToolStripColors(Control control, bool isItem, bool transparentBackground)
    {
        string? background = transparentBackground
            ? "#00FFFFFF"
            : ResolveResourceArgb("GitExtensionsKnownColorControlBrush")
              ?? ResolveResourceArgb("GitExtensionsControlBackgroundBrush");
        string? foreground = control is Separator
            ? ResolveResourceArgb("GitExtensionsKnownColorControlDarkBrush")
              ?? ResolveResourceArgb("GitExtensionsControlBorderBrush")
            : control.Name is "btnUnequalChange" or "btnOnlyB" or "btnOnlyA" or "btnSameChange"
                ? BrushToArgb(GetPropertyValue(control, "Foreground"))
            : control.Name == "encodingToolStripComboBox"
                ? ResolveResourceArgb("GitExtensionsMenuForegroundBrush")
                  ?? ResolveResourceArgb("GitExtensionsWindowTextBrush")
                : ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")
                  ?? ResolveResourceArgb("GitExtensionsControlForegroundBrush");
        return new CaptureColors
        {
            Foreground = foreground,
            Background = background,
            Border = null,
            SelectionForeground = isItem ? ResolveResourceArgb("GitExtensionsKnownColorHighlightTextBrush") : null,
            SelectionBackground = isItem ? ResolveResourceArgb("GitExtensionsKnownColorHighlightBrush") : null,
            InactiveSelectionForeground = isItem
                ? ResolveResourceArgb("GitExtensionsMenuForegroundBrush")
                  ?? ResolveResourceArgb("GitExtensionsWindowTextBrush")
                : null,
            InactiveSelectionBackground = isItem
                ? ResolveResourceArgb("GitExtensionsMenuBackgroundBrush")
                  ?? ResolveResourceArgb("GitExtensionsControlBackgroundBrush")
                : null,
            DisabledForeground = ResolveResourceArgb("GitExtensionsKnownColorGrayTextBrush")
                                 ?? ResolveResourceArgb("GitExtensionsDisabledForegroundBrush"),
            DisabledBackground = background,
            GridLine = null,
            Additional = new SortedDictionary<string, string>(StringComparer.Ordinal)
        };
    }

    private CaptureColors ReadFileStatusListViewColors(Control control)
        => ReadNativeSelectionColors(control, "GitExtensionsPanelBackgroundBrush");

    private CaptureColors ReadFileStatusWatermarkColors(Control control)
    {
        CaptureColors colors = ReadColors(control);
        return colors with
        {
            Foreground = colors.DisabledForeground,
            Border = null
        };
    }

    private CaptureColors ReadNativeSelectionColors(Control control, string backgroundResource)
    {
        string? background = ResolveResourceArgb(backgroundResource)
                             ?? BrushToArgb(GetPropertyValue(control, "Background"));
        return new CaptureColors
        {
            Foreground = BrushToArgb(GetPropertyValue(control, "Foreground"))
                         ?? ResolveResourceArgb("GitExtensionsWindowTextBrush"),
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
            Additional = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["hotTrack"] = ResolveResourceArgb("GitExtensionsNativeListHotTrackBrush")
                               ?? throw new InvalidDataException("The native-list hot-track color did not resolve.")
            }
        };
    }

    private CaptureColors ReadSpellCheckTextBoxColors(Control control)
    {
        CaptureColors colors = ReadColors(control);
        bool isHiddenRepositoryComment = control.GetLogicalAncestors()
            .OfType<Control>()
            .Any(ancestor => ancestor.Name == "_postCommentText")
            && !IsInSelectedTab(control);
        string? background = isHiddenRepositoryComment
            ? ResolveResourceArgb("GitExtensionsWindowBackgroundBrush")
            : colors.Background;
        return colors with
        {
            Foreground = IsSpellCheckWatermarkVisible(control)
                ? ResolveResourceArgb("GitExtensionsKnownColorGrayTextBrush")
                  ?? ResolveResourceArgb("GitExtensionsDisabledForegroundBrush")
                : colors.Foreground,
            Background = background,
            DisabledBackground = background,
            Border = null,
            SelectionForeground = null,
            SelectionBackground = null,
            Additional = new SortedDictionary<string, string>(StringComparer.Ordinal)
        };
    }

    private CaptureColors ReadSourceLabelSubstituteColors(Control control)
    {
        CaptureColors colors = ReadColors(control);
        bool isPreviewLink = control.Name == "_NO_TRANSLATE_lblShowPreview";
        bool isHelpLink = control.Name is "linkLabelShowHelp" or "linkLabelHide";
        bool isDesignerLink = control is HyperlinkButton && IsDesignerMetadataControl(control);
        string? background = control.GetLogicalAncestors()
            .OfType<Control>()
            .Select(ancestor => BrushToArgb(GetPropertyValue(ancestor, "Background")))
            .FirstOrDefault(color => color is not null)
            ?? ResolveResourceArgb("GitExtensionsWindowBackgroundBrush");
        return colors with
        {
            Foreground = isPreviewLink || isHelpLink || isDesignerLink
                ? ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")
                  ?? ResolveResourceArgb("GitExtensionsControlForegroundBrush")
                : colors.Foreground,
            Background = isPreviewLink
                ? "#00FFFFFF"
                : background,
            Border = null,
            SelectionForeground = null,
            SelectionBackground = null,
            InactiveSelectionForeground = null,
            InactiveSelectionBackground = null,
            DisabledBackground = isHelpLink || isDesignerLink ? background : colors.DisabledBackground
        };
    }

    private CaptureColors ReadSourceDesignerColors(Control control)
    {
        CaptureColors colors = ReadColors(control);
        return colors with
        {
            Foreground = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")
                         ?? ResolveResourceArgb("GitExtensionsControlForegroundBrush"),
            DisabledForeground = ResolveResourceArgb("GitExtensionsKnownColorGrayTextBrush")
                                 ?? ResolveResourceArgb("GitExtensionsDisabledForegroundBrush")
        };
    }

    private CaptureColors ReadSourceDesignerButtonColors(Control control)
    {
        CaptureColors colors = ReadColors(control);
        string? background = control.GetLogicalAncestors()
            .OfType<Control>()
            .Select(ancestor => BrushToArgb(GetPropertyValue(ancestor, "Background")))
            .FirstOrDefault(color => color is not null)
            ?? ResolveResourceArgb("GitExtensionsControlBackgroundBrush");
        bool isTransparentSourceButton = control.Name == "btnRemoteColor";
        return colors with
        {
            Foreground = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")
                         ?? ResolveResourceArgb("GitExtensionsControlForegroundBrush"),
            Background = isTransparentSourceButton ? "#00FFFFFF" : background,
            Border = null,
            DisabledBackground = isTransparentSourceButton ? "#00FFFFFF" : background,
            DisabledForeground = ResolveResourceArgb("GitExtensionsKnownColorGrayTextBrush")
                                 ?? ResolveResourceArgb("GitExtensionsDisabledForegroundBrush"),
            SelectionForeground = null,
            SelectionBackground = null,
            InactiveSelectionForeground = null,
            InactiveSelectionBackground = null
        };
    }

    private CaptureColors ReadDialogControlsPanelColors(Control control)
    {
        CaptureColors colors = ReadColors(control);
        string? background = ResolveResourceArgb("GitExtensionsDialogControlsBackgroundBrush");
        return colors with
        {
            Background = background,
            DisabledBackground = background
        };
    }

    private CaptureColors ReadSourceDataGridColors(Control control)
    {
        bool usesWindowBackground = control.Name == "RemoteBranches";
        string? background = ResolveResourceArgb(usesWindowBackground
            ? "GitExtensionsKnownColorWindowBrush"
            : "GitExtensionsKnownColorControlDarkBrush");
        return new CaptureColors
        {
            Foreground = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush"),
            Background = background,
            Border = ResolveResourceArgb("GitExtensionsKnownColorControlBrush"),
            SelectionForeground = ResolveResourceArgb("GitExtensionsKnownColorHighlightTextBrush"),
            SelectionBackground = ResolveResourceArgb("GitExtensionsDataGridViewSelectionBackgroundBrush"),
            InactiveSelectionForeground = ResolveResourceArgb("GitExtensionsKnownColorHighlightTextBrush")
                                          ?? ResolveResourceArgb("GitExtensionsHighlightForegroundBrush"),
            InactiveSelectionBackground = ResolveResourceArgb("GitExtensionsKnownColorInactiveCaptionBrush"),
            DisabledForeground = ResolveResourceArgb("GitExtensionsKnownColorGrayTextBrush"),
            DisabledBackground = background,
            GridLine = usesWindowBackground
                ? ResolveResourceArgb("GitExtensionsKnownColorControlLightBrush")
                  ?? ResolveResourceArgb("GitExtensionsControlPointerOverBackgroundBrush")
                : ResolveResourceArgb("GitExtensionsDataGridViewGridLineBrush"),
            Additional = new SortedDictionary<string, string>(StringComparer.Ordinal)
        };
    }

    private CaptureColors ReadSourceDataGridColumnColors()
        => EmptyColors() with
        {
            InactiveSelectionBackground = ResolveResourceArgb("GitExtensionsKnownColorInactiveCaptionBrush"),
            DisabledForeground = ResolveResourceArgb("GitExtensionsKnownColorGrayTextBrush")
        };

    private CaptureColors ReadRepositoryHostDiscussionColors(Control control)
        => ReadColors(control) with { Border = null };

    private CaptureColors ReadSourceInputColors(Control control)
    {
        CaptureColors colors = ReadColors(control);
        bool isReadOnly = GetNullableBoolProperty(control, "IsReadOnly") == true;
        string? background = isReadOnly
            ? ResolveResourceArgb("GitExtensionsReadOnlyTextInputBackgroundBrush")
            : control is TextBox
                ? ResolveResourceArgb("GitExtensionsTextInputBackgroundBrush")
                : ResolveResourceArgb("GitExtensionsKnownColorWindowBrush")
                  ?? ResolveResourceArgb("GitExtensionsWindowBackgroundBrush");
        return colors with
        {
            Foreground = ResolveResourceArgb("GitExtensionsKnownColorWindowTextBrush")
                         ?? ResolveResourceArgb("GitExtensionsWindowTextBrush"),
            Background = background,
            DisabledBackground = background,
            Border = null,
            SelectionForeground = null,
            SelectionBackground = null,
            InactiveSelectionForeground = null,
            InactiveSelectionBackground = null,
            Additional = new SortedDictionary<string, string>(StringComparer.Ordinal)
        };
    }

    private CaptureColors ReadComboBoxPopupColors()
    {
        string? background = ResolveResourceArgb("GitExtensionsKnownColorWindowBrush")
                             ?? ResolveResourceArgb("GitExtensionsWindowBackgroundBrush");
        return new CaptureColors
        {
            Foreground = ResolveResourceArgb("GitExtensionsKnownColorWindowTextBrush")
                         ?? ResolveResourceArgb("GitExtensionsWindowTextBrush"),
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
            Additional = new SortedDictionary<string, string>(StringComparer.Ordinal)
        };
    }

    private CaptureColors ReadTransparentContainerColors(Control control)
    {
        CaptureColors colors = ReadColors(control);
        return colors with
        {
            Foreground = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")
                         ?? ResolveResourceArgb("GitExtensionsControlForegroundBrush"),
            Background = "#00FFFFFF",
            Border = null,
            DisabledBackground = "#00FFFFFF"
        };
    }

    private CaptureColors ReadLightTransparentColors(Control control)
    {
        CaptureColors colors = ReadColors(control);
        return IsDarkTheme()
            ? colors
            : colors with
            {
                Background = "#00FFFFFF",
                DisabledBackground = "#00FFFFFF"
            };
    }

    private static bool ReadFileStatusListViewExpanded(Control control)
        => control is TreeView
           && control.GetVisualDescendants().OfType<TreeViewItem>().Any(item => item.IsExpanded);

    private CaptureColors ReadFileViewerTextEditorColors()
        => new()
        {
            Foreground = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush"),
            Background = "#00FFFFFF",
            Border = null,
            SelectionForeground = null,
            SelectionBackground = null,
            InactiveSelectionForeground = null,
            InactiveSelectionBackground = null,
            DisabledForeground = ResolveResourceArgb("GitExtensionsKnownColorGrayTextBrush"),
            DisabledBackground = "#00FFFFFF",
            GridLine = null,
            Additional = new SortedDictionary<string, string>(StringComparer.Ordinal)
        };

    private CaptureColors ReadFileViewerPictureBoxColors(Control control)
    {
        CaptureColors colors = ReadColors(control);
        string? background = ResolveResourceArgb("GitExtensionsKnownColorControlLightLightBrush");
        return colors with
        {
            Background = background,
            Border = null,
            DisabledBackground = background
        };
    }

    private static string? GetToolTip(Control control)
        => ToolTip.GetTip(control) switch
        {
            TextBlock textBlock => textBlock.Text,
            string text => text,
            object value => value.ToString(),
            null => null
        };

    private static bool IsDetachedMenuItem(Control control)
        => control is MenuItem or Separator && TopLevel.GetTopLevel(control) is null;

    private static bool IsNativeListView(Control control)
        => control is ListBox list && list.Classes.Contains("gitextensions-native-list-items");

    private bool IsInheritedFormProcessContainer(Control control)
        => _root.GetType().FullName == "GitUI.CommandsDialogs.FormPull"
           && control.Name is "MainPanel" or "ControlsPanel";

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
        if (IsRevisionGridView(control))
        {
            // parity-scaffolding: Avalonia moves keyboard focus into an owned ContextMenu, while
            // WinForms keeps the owning grid focused; emit the equivalent owner-focused state.
            return control.IsKeyboardFocusWithin
                   || control.ContextMenu?.IsOpen == true
                   || _root.GetLogicalDescendants().OfType<ContextMenu>().Any(menu => menu.IsOpen);
        }

        if (IsNativeListView(control)
            || control is ComboBox or ListBox or TreeView)
        {
            return control.IsKeyboardFocusWithin;
        }

        return control.IsFocused;
    }

    private static bool IsOverlayPopupHost(Control control) =>
        control.GetType().Name == "OverlayPopupHost";

    private decimal ToDecimal(double value) => decimal.Round((decimal)value, 4);

    private int ToPixel(double value) =>
        checked((int)Math.Round(value * _renderScale, MidpointRounding.AwayFromZero));
}
