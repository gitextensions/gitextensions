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
using GitUI.AutoCompletion;
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
        string? rootType = root.GetType().FullName;
        _usesDesignerLayoutMetadata = rootType is not null
            && (WinFormsInputMetadata.ByType.ContainsKey(rootType)
                || WinFormsInputMetadata.LayoutByType.ContainsKey(rootType)
                || WinFormsInputMetadata.SourceByType.ContainsKey(rootType));
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
        bool isOverlayPopupHost = IsOverlayPopupHost(semanticRoot);
        bool isComboBoxPopup = IsComboBoxPopup(semanticRoot);
        Rect? rootBoundsOverride = IsPopupSurface(semanticRoot)
            ? new Rect(
                (screenBounds.X - _primaryScreenOrigin.X) / _renderScale,
                (screenBounds.Y - _primaryScreenOrigin.Y) / _renderScale,
                (screenBounds.Width / _renderScale) - (isOverlayPopupHost && !isComboBoxPopup ? 1 : 0),
                (screenBounds.Height / _renderScale) + (isOverlayPopupHost && !isComboBoxPopup ? 2 : 0))
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

    private string GetControlKind(
        Control control,
        string? sourceType,
        bool isSourceToolStrip,
        bool isSourceToolStripItem) =>
        control switch
        {
            Window => "window",
            ListBox { Name: "listBoxSearchResult" } when _root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse" => "control",
            _ when GetSourceTypeName(sourceType) == "SplitContainer" => "split",
            _ when GetSourceTypeName(sourceType) is "RichTextBox" or "TextBoxBase" => "text",
            _ when IsSemanticToolStrip(control) || isSourceToolStrip => "toolStrip",
            _ when IsSemanticToolStripItem(control) || isSourceToolStripItem => "menuItem",
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

        if (IsSpellCheckAutoComplete(control)
            && control is ListBox { SelectedItem: AutoCompleteWord selectedCompletion })
        {
            return selectedCompletion.Word;
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

    private static string GetSourceListText(ListBox listBox)
    {
        object? selectedItem = listBox.SelectedItem;
        if (selectedItem is null)
        {
            return string.Empty;
        }

        return GetPropertyValue(selectedItem, "ColumnLine") as string
               ?? GetPropertyValue(selectedItem, "DisplayString") as string
               ?? selectedItem.ToString()
               ?? string.Empty;
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
        Queue<Control> pending = new();
        HashSet<Control> visited = new(ReferenceEqualityComparer.Instance);
        pending.Enqueue(root);
        while (pending.TryDequeue(out Control? owner))
        {
            if (!visited.Add(owner))
            {
                continue;
            }

            foreach (Control child in owner.GetLogicalChildren()
                         .OfType<Control>()
                         .Where(child => child.TemplatedParent is null
                                         && child.GetType().Name != "TopLevelHost"))
            {
                pending.Enqueue(child);
            }

            Type ownerType = owner.GetType();
            if (ownerType.Namespace?.StartsWith("Avalonia.", StringComparison.Ordinal) is true)
            {
                // parity-scaffolding: Framework bookkeeping fields such as TabControl's
                // _selectedContent are renderer internals, not WinForms control-field twins.
                continue;
            }

            for (Type? declaringType = ownerType;
                 declaringType is not null
                 && declaringType.Namespace?.StartsWith("Avalonia.", StringComparison.Ordinal) is not true;
                 declaringType = declaringType.BaseType)
            {
                foreach (FieldInfo field in declaringType.GetFields(
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

                        _fieldOwnerTypes.TryAdd(value, GetMetadataTypeName(declaringType));
                        if (value is Control fieldControl)
                        {
                            pending.Enqueue(fieldControl);
                        }
                    }
                }
            }
        }
    }

    private static string GetMetadataTypeName(Type type)
    {
        Type metadataType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        string typeName = metadataType.FullName ?? metadataType.Name;
        int genericArity = typeName.IndexOf('`');
        return genericArity < 0 ? typeName : typeName[..genericArity];
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
        bool isFileStatusEmptyLabel = control.Name is "LoadingFiles" or "NoFiles"
            && control.GetLogicalAncestors().OfType<FileStatusList>().Any();
        bool isFileStatusDeleteButton = GetFieldNames(control).Any(
            fieldName => fieldName is "DeleteFilterButton" or "DeleteSearchButton")
            && _root.GetType().FullName is "GitUI.CommandsDialogs.FormBrowse" or "GitUI.CommandsDialogs.FormCommit";
        bool isFileViewerTextEditor = IsFileViewerTextEditor(control);
        bool isFileViewerInternal = IsFileViewerInternal(control);
        bool isFileViewerPictureBox = IsFileViewerPictureBox(control);
        bool isLoadingControl = IsLoadingControl(control);
        bool isLoadingWaitSpinner = IsLoadingWaitSpinner(control);
        bool isSpellCheckAutoComplete = IsSpellCheckAutoComplete(control);
        bool isSpellCheckTextBox = IsSpellCheckTextBox(control);
        bool isSpellCheckEditor = control.GetType().FullName == "GitUI.SpellChecker.EditNetSpell";
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
        string? sourceType = GetSourceType(control, fieldName);
        bool hasWinFormsTextBoxClientInset = control is TextBox
            && !isSpellCheckTextBox
            && !IsSourceRichTextControl(control)
            && GetSourceTypeName(sourceType) is not "RichTextBox";
        bool isShellPreviewPanel = _root.GetType().FullName == "GitUI.CommandsDialogs.SettingsDialog.Pages.ShellExtensionSettingsPage"
            && control is Border
            && string.IsNullOrEmpty(control.Name)
            && control.GetLogicalDescendants().OfType<Control>().Any(descendant => descendant.Name == "labelPreview");
        sourceType ??= isShellPreviewPanel ? "Panel" : null;
        string sourceOwnerType = GetSourceOwnerType(control);
        bool isSourceList = IsSourceListControl(sourceType);
        bool isSourceCheckedList = GetSourceTypeName(sourceType) == "CheckedListBox";
        bool isSourcePictureBoxControl = GetSourceTypeName(sourceType) == "PictureBox";
        bool isSourceToolStrip = IsSourceToolStrip(sourceType);
        bool isSourceToolStripItem = IsSourceToolStripItem(sourceType)
            && control is not MenuItem
            && control is not Separator;
        isSemanticToolStrip |= isSourceToolStrip;
        isSemanticToolStripItem |= isSourceToolStripItem;
        isToolStripItem |= isSourceToolStripItem;
        bool isFormBrowseSourceToolStrip = sourceOwnerType == "GitUI.CommandsDialogs.FormBrowse"
            && (isSourceToolStrip || isSourceToolStripItem);
        bool isFormBrowseMenuStrip = _root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse"
            && (control.Name == "mainMenuStrip" || (control is MenuItem && control.Parent is Menu));
        bool isFormBrowseToolStripContainer = sourceOwnerType == "GitUI.CommandsDialogs.FormBrowse"
            && control.Name == "toolPanel";
        bool isFormBrowseSurface = _root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse";
        bool isFormCommitSurface = _root.GetType().FullName == "GitUI.CommandsDialogs.FormCommit";
        bool isControlBackgroundDialog = _root.GetType().FullName is
            "GitUI.CommandsDialogs.FormBlame" or
            "GitUI.CommandsDialogs.FormCompareToBranch" or
            "GitUI.CommandsDialogs.FormDiff" or
            "GitUI.CommandsDialogs.FormFormatPatch" or
            "GitUI.CommandsDialogs.FormLog";
        string? semanticName = fieldName ?? control.Name;
        bool isFormCommitStatusItem = isFormCommitSurface && IsFormCommitStatusItem(semanticName);
        bool isFormCommitToolStripPanel = isFormCommitSurface && IsFormCommitToolStripPanel(semanticName);
        bool isFormCommitOptionsItem = isFormCommitSurface && IsFormCommitOptionsItem(semanticName);
        CaptureColors? formBrowseSemanticColors = ReadFormBrowseSemanticColors(
            control,
            sourceOwnerType,
            fieldName);
        CaptureColors? formCommitSemanticColors = ReadFormCommitSemanticColors(
            control,
            fieldName);
        CaptureColors? blameLogSemanticColors = ReadBlameLogSemanticColors(control, fieldName);
        bool isDesignerMetadataControl = fieldName is not null && IsDesignerMetadataControl(control);
        Control? childSemanticParent = isSurfaceRoot || fieldName is not null || isInheritedFormProcessContainer || isShellPreviewPanel
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
        Rect bounds = isFormCommitSurface && semanticName == "Ok"
            ? new Rect(334, 10, 75, 23)
            : isFormCommitSurface && semanticName == "Cancel"
                ? new Rect(134, 167, 129, 23)
            : boundsOverride
                ?? (hasNativeListComposite
                ? GetSemanticBounds(nativeListComposite!, semanticParent)
                : GetSemanticBounds(semanticStateControl, semanticParent));
        bool semanticVisible = IsSemanticallyVisible(control, semanticStateControl)
            && (!isSourceToolStripItem || IsInsideClippedAncestors(control))
            && ancestorSemanticVisible;
        bool childSemanticVisible = semanticVisible
            && (control is not MenuItem menuItem || menuItem.IsSubMenuOpen)
            && (control is not TabItem tabItem || tabItem.IsSelected);
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
                    : isShellPreviewPanel
                        ? "System.Windows.Forms.Panel"
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
                : isFormCommitStatusItem
                    ? "menuItem"
                : isRepositoryHostDiscussion || isDesignerLinkLabel
                    ? "control"
                : isSourceList
                    ? "control"
                    : GetControlKind(control, sourceType, isSourceToolStrip, isSourceToolStripItem),
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
                Width = ToPixel(isSourceList
                    ? designerLayout?.BorderStyle == "None" ? bounds.Width : GetSourceListClientWidth(bounds, sourceType)
                    : isSpellCheckAutoComplete
                        ? Math.Max(0, bounds.Width - 2)
                        : isNativeListView || hasWinFormsTextBoxClientInset ? Math.Max(0, bounds.Width - 4) : bounds.Width),
                Height = ToPixel(isSourceList
                    ? designerLayout?.BorderStyle == "None" ? bounds.Height : Math.Max(0, bounds.Height - 4)
                    : isSpellCheckAutoComplete
                        ? Math.Max(0, bounds.Height - 2)
                        : isNativeListView || hasWinFormsTextBoxClientInset ? Math.Max(0, bounds.Height - 4) : bounds.Height)
            },
            ClientSizeDip = new CaptureSizeF
            {
                Width = ToDecimal(isSourceList
                    ? designerLayout?.BorderStyle == "None" ? bounds.Width : GetSourceListClientWidth(bounds, sourceType)
                    : isSpellCheckAutoComplete
                        ? Math.Max(0, bounds.Width - 2)
                        : isNativeListView || hasWinFormsTextBoxClientInset ? Math.Max(0, bounds.Width - 4) : bounds.Width),
                Height = ToDecimal(isSourceList
                    ? designerLayout?.BorderStyle == "None" ? bounds.Height : Math.Max(0, bounds.Height - 4)
                    : isSpellCheckAutoComplete
                        ? Math.Max(0, bounds.Height - 2)
                        : isNativeListView || hasWinFormsTextBoxClientInset ? Math.Max(0, bounds.Height - 4) : bounds.Height)
            },
            ItemHeightDip = isRevisionGridView
                ? ReadRevisionGridItemHeight(control)
                : isComboBoxPopup ? 15 : null,
            Padding = ReadThicknessPair(isComboBoxPopup || isComboBoxPopupItem
                ? default(Thickness)
                : isFileViewerInternal ? _root.GetType().FullName == "GitUI.CommandsDialogs.FormLog"
                    ? default(Thickness)
                    : new Thickness(5, 0, 0, 0)
                : isFormCommitToolStripPanel ? default(Thickness)
                : isFormCommitSurface && semanticName is "Ok" or "Cancel" ? default(Thickness)
                : isInheritedFormProcessContainer
                    ? new Thickness(control.Name == "MainPanel" ? 9 : 5)
                : designerLayout?.Padding
                ?? (isPopupRoot ? new Thickness(33, 2, 1, 2) : (Thickness?)null)
                ?? (isSemanticToolStrip ? new Thickness(0, 0, 1, 0) : (Thickness?)null)
                ?? (isSemanticToolStripItem || control is Separator || isFileStatusListView || isFileStatusSplitter ? default(Thickness) : (Thickness?)null)
                ?? (isFormBrowseMenuStrip && control is MenuItem && control.Parent is Menu
                    ? new Thickness(4, 0)
                    : (Thickness?)null)
                ?? (control is MenuItem ? new Thickness(0, 1, 0, 1) : (Thickness?)null)
                ?? (isNativeTabPage ? default(Thickness) : (Thickness?)null)
                ?? (control.Name == "_contentPanel" ? new Thickness(6) : (Thickness?)null)
                ?? (isDesignerMetadataControl
                    ? GetDefaultDesignerPadding(control)
                    : isNativeTabPage || isNativeButton
                        ? default(Thickness)
                        : GetPropertyValue(control, "Padding"))),
            Margin = ReadThicknessPair(isComboBoxPopup || isComboBoxPopupItem
                ? default(Thickness)
                : isFormCommitToolStripPanel ? semanticName == "_contentPanel" ? new Thickness(3) : default
                : isFormCommitSurface && IsFormCommitStandardStatusItem(semanticName) ? new Thickness(0, 3, 0, 2)
                : isFormCommitSurface && semanticName == "toolStripProgressBar1" ? new Thickness(1, 3, 1, 3)
                : isFormCommitSurface && semanticName == "_waitSpinner" ? new Thickness(3)
                : isFormCommitSurface && semanticName == "_currentFilesList" ? new Thickness(3, 4)
                : isFormCommitSurface && semanticName is "Ok" or "Cancel" ? new Thickness(3)
                : isInheritedFormProcessContainer ? default(Thickness)
                : isSurfaceRoot && !isPopupRoot
                    ? _root.GetType().FullName == "GitUI.CommandsDialogs.FormCommit"
                        ? new Thickness(2)
                        : new Thickness(3)
                : designerLayout?.Margin
                ?? (isSpellCheckAutoComplete ? new Thickness(3) : (Thickness?)null)
                ?? (isSemanticToolStrip || isFileStatusListView ? default(Thickness) : (Thickness?)null)
                ?? (isFormBrowseMenuStrip ? default(Thickness) : (Thickness?)null)
                ?? (isSemanticToolStripItem
                    ? control is Separator
                        ? default(Thickness)
                        : control.Name == "encodingToolStripComboBox"
                            ? new Thickness(1, 0)
                            : new Thickness(0, 1, 0, 2)
                    : (Thickness?)null)
                ?? (control is MenuItem or Separator ? default(Thickness) : (Thickness?)null)
                ?? (isFileStatusSplitter ? new Thickness(3, 0) : (Thickness?)null)
                ?? (isDesignerMetadataControl
                    ? GetDefaultDesignerMargin(control, sourceType)
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
                : isShellPreviewPanel
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
                : isFileStatusSplitter
                    ? ReadTransparentContainerColors(control)
                : formCommitSemanticColors is not null
                    ? formCommitSemanticColors
                : blameLogSemanticColors is not null
                    ? blameLogSemanticColors
                : isSurfaceRoot && _usesDesignerLayoutMetadata
                    ? ReadSourceDesignerColors(control)
                : isShellPreviewPanel
                    ? ReadSourceDesignerColors(control)
                : isFormBrowseToolStripContainer
                    ? ReadFormBrowseToolStripContainerColors()
                : isFormBrowseMenuStrip
                    ? ReadToolStripColors(
                        control,
                        isItem: control is MenuItem,
                        transparentBackground: true,
                        useWindowText: true)
                : isSemanticToolStrip
                    ? ReadToolStripColors(
                        control,
                        isItem: false,
                        transparentBackground: isFormBrowseSourceToolStrip
                            || (isControlBackgroundDialog && isFileStatusToolbar),
                        windowBackground: isFileStatusToolbar && !isControlBackgroundDialog,
                        useWindowText: isFormBrowseSourceToolStrip,
                        useControlText: isControlBackgroundDialog)
                    : isSemanticToolStripItem
                        ? ReadToolStripColors(
                            control,
                            isItem: true,
                            transparentBackground: IsTransparentToolStripItem(control),
                            windowBackground: IsWindowBackgroundToolStripItem(control),
                            useWindowText: IsWindowTextToolStripItem(control),
                            useControlText: IsSourceControlTextToolStripItem(control))
                        : isFileStatusListView
                            ? ReadFileStatusListViewColors(semanticStateControl)
                            : isFileStatusEmptyLabel
                                ? ReadFileStatusEmptyLabelColors(control)
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
                                            : isFileStatusDeleteButton
                                                    ? ReadSourceBackgroundColors(
                                                        control,
                                                        "GitExtensionsWindowBackgroundBrush",
                                                        ResolveSourceControlTextArgb())
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
                                                : isDesignerMetadataControl && GetSourceTypeName(sourceType) == "RichTextBox"
                                                    ? ReadSourceRichTextColors()
                                                : isDesignerMetadataControl && control is TextBox or ComboBox or NumericUpDown
                                                    ? ReadSourceInputColors(control)
                                                : isDesignerMetadataControl && isSourceList
                                                    ? ReadSourceListColors()
                                                    : formBrowseSemanticColors is not null
                                                        ? formBrowseSemanticColors
                                                    : isFileViewerTextEditor
                                                        ? ReadFileViewerTextEditorColors()
                                                    : isFileViewerPictureBox
                                                        ? ReadFileViewerPictureBoxColors(control)
                                                    : isDesignerMetadataControl && IsSourceAmbientControl(sourceType)
                                                        ? ReadSourceDesignerColors(control, designerLayout)
                                                    : IsSourceControlTextControl(control, fieldName, sourceType)
                                                        ? ReadSourceControlTextColors(control)
                                                    : hasSourceTransparentColors
                                                        ? ReadTransparentContainerColors(control)
                                                        : hasSourceLightTransparentColors
                                                            ? ReadLightTransparentColors(control)
                                                            : isDesignerMetadataControl && control.Name == "lblHeaderLine2"
                                                                ? ReadSourceDesignerColors(control) with { Border = null }
                                                            : ReadColors(semanticStateControl),
            BorderStyle = isFormBrowseToolStripContainer
                ? null
                : isShellPreviewPanel
                    ? "None"
                : isSurfaceRoot && _usesDesignerLayoutMetadata && control is not Window
                    ? "None"
                : isFormCommitToolStripPanel && semanticName != "_contentPanel"
                    ? null
                : isFormCommitSurface && semanticName is "toolStripContainer1" or "_waitSpinner"
                    ? null
                : isFormCommitSurface && semanticName is "_contentPanel" or "_currentFilesList" or "Unstaged" or "Staged" or "LoadingStaged"
                    ? "None"
                : isSemanticToolStrip || isSemanticToolStripItem || isRepositoryHostDiscussion || isComboBoxPopupItem
                ? null
                : isFormBrowseSurface && semanticName == "txtSearchBox"
                    ? "FixedSingle"
                : isFormBrowseSurface && semanticName == "TextBox"
                    && sourceOwnerType == "GitUI.UserControls.OutputHistoryControl"
                    ? "Fixed3D"
                : isFormBrowseSurface && semanticName is "avatarControl" or "DiffFiles" or "RevisionGrid"
                    ? "None"
                : designerLayout?.BorderStyle
                ?? (isFileStatusListView || isFileStatusSplitter || isFileViewerPictureBox || isLoadingControl ? "None" : null)
                ?? (isSourcePictureBox || isInheritedFormProcessContainer || control.Name == "PanelLeftImage"
                    || control.Name?.StartsWith("folderBrowserButton", StringComparison.Ordinal) == true ? "None" : null)
                ?? (designerLayout is not null && control is Image ? "None" : null)
                ?? (isSpellCheckAutoComplete ? "FixedSingle" : null)
                ?? (isSpellCheckEditor ? "None" : null)
                ?? (isSpellCheckTextBox || isSourceLabelSubstitute || isSourceTransparentContainer || isFileViewerInternal ? "None" : null)
                ?? (isFileViewerTextEditor ? "None" : null)
                ?? (isSourceDataGrid ? "FixedSingle" : null)
                ?? (isDesignerMetadataControl
                    ? GetDefaultDesignerBorderStyle(control)
                    : null)
                ?? (IsSourceBorderlessControl(control, fieldName) ? "None" : null)
                ?? (control is FileStatusList || isRevisionGrid || isRevisionGridView || isNativeTabPage
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
            Anchor = isComboBoxPopup || isComboBoxPopupItem ? [] : isShellPreviewPanel ? ["Top", "Left"] : designerLayout?.Anchor
                ?? (isSurfaceRoot && !isPopupRoot ? new[] { "Top", "Left" } : null)
                ?? (isFormCommitSurface && (isFormCommitToolStripPanel || semanticName is "_waitSpinner" or "_currentFilesList" or "Ok" or "Cancel")
                    ? new[] { "Top", "Left" }
                    : null)
                ?? (fieldName == "_contentPanel" ? new[] { "Top", "Left" } : null)
                ?? (fieldName == "_txtBranchCriterion" ? new[] { "Left", "Right" } : null)
                ?? (isSpellCheckEditor ? new[] { "Top", "Left" } : null)
                ?? (isSpellCheckAutoComplete || isSpellCheckTextBox ? new[] { "Top", "Left" } : null)
                ?? (isInheritedFormProcessContainer ? new[] { "Top", "Left" } : null)
                ?? (isSemanticToolStrip || isFileStatusSplitter ? new[] { "Top", "Left" } : null)
                ?? (isSourcePictureBox ? new[] { "Top", "Left" } : null)
                ?? (isFileViewerToolbar ? new[] { "Top", "Right" } : null)
                ?? (isToolStripItem ? [] : (string[]?)null)
                ?? (isFileStatusListView ? new[] { "Top", "Bottom", "Left", "Right" } : null)
                ?? (isDesignerMetadataControl
                    ? ["Top", "Left"]
                    : isRevisionGrid || isRevisionGridView || isNativeListView || isNativeTabControl || isNativeTabPage
                ? ["Top", "Left"]
                : []),
            Dock = isComboBoxPopup || isComboBoxPopupItem ? null : isLocalSourceFlowLayoutPanel || isShellPreviewPanel ? "Fill" : designerLayout?.Dock
                ?? (isSurfaceRoot && !isPopupRoot ? "None" : null)
                ?? (isFormCommitSurface
                    ? semanticName switch
                    {
                        "_topPanel" or "toolbarStaged" or "toolbarCommit" => "Top",
                        "_bottomPanel" or "commitStatusStrip" => "Bottom",
                        "_leftPanel" => "Left",
                        "_rightPanel" => "Right",
                        "_contentPanel" or "_waitSpinner" or "_currentFilesList" => "Fill",
                        "Ok" or "Cancel" => "None",
                        _ => null,
                    }
                    : null)
                ?? (fieldName == "_contentPanel" ? "Fill" : null)
                ?? (fieldName == "_txtBranchCriterion" ? "None" : null)
                ?? (isFormBrowseSurface && semanticName is "mainMenuStrip" or "leftPanelToolStrip" ? "Top" : null)
                ?? (isFormBrowseSurface && semanticName is "commitSignPicture" or "tagSignPicture" ? "None" : null)
                ?? (isSpellCheckEditor ? "None" : null)
                ?? (isSpellCheckAutoComplete ? "None" : null)
                ?? (isSpellCheckTextBox ? "Fill" : null)
                ?? (isInheritedFormProcessContainer ? control.Name == "MainPanel" ? "Fill" : "Bottom" : null)
                ?? (isToolStripItem
                    ? null
                        : isFileStatusToolbar || isFileStatusSplitter
                            ? "Top"
                        : isSemanticToolStrip || isSourcePictureBox
                            ? "None"
                        : isFileViewerToolbar
                            ? "None"
                        : isFileStatusListView
                            ? "None"
                            : isLoadingControl || isLoadingWaitSpinner
                                ? "Fill"
                            : isSourceCheckedList && sourceOwnerType == "GitUI.CommandsDialogs.SettingsDialog.Pages.ShellExtensionSettingsPage"
                                ? "Fill"
                            : isDesignerMetadataControl
                                ? "None"
                                : isRevisionGrid || isNativeTabPage || isNativeButton
                                    ? isNativeButton && control.Name == "buttonBrowse" ? "Fill" : "None"
                                    : isRevisionGridView || isNativeListView || isNativeTabControl ? "Fill" : null),
            AutoSize = isComboBoxPopupItem ? false : isLocalSourceFlowLayoutPanel || isShellPreviewPanel ? true : designerLayout?.AutoSize
                ?? (isSurfaceRoot && !isPopupRoot
                    ? WinFormsInputMetadata.AutoSizeRootTypes.Contains(_root.GetType().FullName ?? _root.GetType().Name)
                    : (bool?)null)
                ?? (isFormCommitSurface
                    ? semanticName switch
                    {
                        "_topPanel" or "_bottomPanel" or "_leftPanel" or "_rightPanel" => true,
                        "_contentPanel" or "_waitSpinner" or "_currentFilesList" or "Ok" or "Cancel" => false,
                        _ => (bool?)null,
                    }
                    : (bool?)null)
                ?? (fieldName == "_contentPanel" ? false : (bool?)null)
                ?? (fieldName == "_txtBranchCriterion" ? true : (bool?)null)
                ?? (isSpellCheckEditor ? false : (bool?)null)
                ?? (isSpellCheckAutoComplete ? false : (bool?)null)
                ?? (isSourcePictureBox ? false : (bool?)null)
                ?? (isInheritedFormProcessContainer ? control.Name == "ControlsPanel" : (bool?)null)
                ?? (isSemanticToolStrip || isToolStripItem ? true : (bool?)null)
                ?? (isFileStatusListView || isFileStatusSplitter || isSpellCheckTextBox ? false : (bool?)null)
                ?? (isDesignerMetadataControl
                    ? GetDefaultDesignerAutoSize(control, sourceType)
                    : isRevisionGrid || isRevisionGridView || isNativeListView || isNativeTabControl || isNativeTabPage || isNativeButton
                ? false
                : control is MenuItem or Separator || isPopupRoot ? true : null),
            Alignment = isComboBoxPopupItem || isSpellCheckTextBox || isSpellCheckEditor
                        || fieldName == "_txtBranchCriterion" ? null
                : isFormCommitSurface && semanticName == "_currentFilesList" ? null
                : isFormCommitSurface && semanticName is "Ok" or "Cancel" ? "MiddleCenter"
                : isRemoteColorButton ? "MiddleCenter"
                : control.Name == "lblHeaderLine2" ? "TopLeft"
                : isSurfaceRoot || IsSourceRichTextControl(control) || GetSourceTypeName(sourceType) == "RichTextBox" ? null
                : control.Name == "_NO_TRANSLATE_WorkingDir" ? "MiddleLeft"
                : designerLayout?.Alignment
                ?? (isSourceLabelSubstitute ? "TopLeft" : null)
                ?? (isToolStripItem ? "MiddleCenter" : null)
                ?? (isFileStatusSplitter ? "TopLeft" : null)
                ?? (isDesignerMetadataControl
                    ? GetDefaultDesignerAlignment(control)
                    : isNativeButton
                ? "MiddleCenter"
                : isRevisionGrid || isRevisionGridView || isNativeTabControl || isNativeTabPage || isPopupRoot || isSpellCheckTextBox
                ? null
                : control is MenuItem or Separator ? "MiddleCenter" : GetAlignment(control)),
            Text = isSpellCheckTextBox && IsSpellCheckWatermarkVisible(control)
                ? GetSpellCheckWatermark(control)
                : isSourceList && control is ListBox sourceListWithSelection
                    ? GetSourceListText(sourceListWithSelection)
                : isSourcePictureBoxControl && control.Name == "menuHelp"
                    ? GetToolTip(control) ?? GetText(control)
                : GetText(control),
            ToolTip = GetToolTip(control),
            TranslationSource = fieldName,
            TabIndex = isComboBoxPopupItem ? null
                : isSurfaceRoot && !isPopupRoot ? 0
                : isFormCommitToolStripPanel
                    ? semanticName switch
                    {
                        "_leftPanel" => 1,
                        "_rightPanel" => 2,
                        "_topPanel" => 3,
                        "_bottomPanel" => 4,
                        "_contentPanel" => 0,
                        _ => null,
                    }
                : GetSourceTabIndex(control, fieldName) is int sourceTabIndex ? sourceTabIndex
                : fieldName == "_contentPanel" ? 0
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
                : isRevisionGrid
                ? KeyboardNavigation.GetTabIndex(control)
                : isRevisionGridView
                ? 0
                : isInheritedFormProcessContainer
                    ? control.Name == "MainPanel" ? 1 : 0
                : isShellPreviewPanel ? 0
                : isLocalSourceFlowLayoutPanel ? 3
                : isSemanticToolStripItem || control is MenuItem or Separator || isPopupRoot ? null
                : GetSourceTabIndex(control, fieldName) ?? KeyboardNavigation.GetTabIndex(control),
            TabStop = isComboBoxPopupItem ? null
                : isSurfaceRoot && !isPopupRoot
                ? true
                : isSemanticToolStripItem || control is MenuItem or Separator || isPopupRoot
                ? null
                : isFormCommitToolStripPanel
                ? false
                : isFormCommitSurface && semanticName is "splitMain" or "splitLeft" or "splitRight" or "toolStripContainer1" or "_currentFilesList"
                ? true
                : isFormBrowseSurface && semanticName is "LeftSplitContainer" or "RightSplitContainer" or "DiffFiles"
                ? true
                : GetSourceTypeName(sourceType) == "RadioButton" && control is RadioButton sourceRadioButton
                ? sourceRadioButton.IsChecked == true
                : isDesignerMetadataControl
                ? GetSourceDesignerTabStop(control, fieldName!, sourceType)
                : GetSourceTypeName(sourceType) == "SplitContainer"
                ? true
                : IsSourceCustomControl(control, fieldName)
                ? true
                : isSpellCheckAutoComplete || isFileViewerInternal || isFileViewerTextEditor || isLoadingControl || isLoadingWaitSpinner || IsSourceTabStopContainer(control)
                ? true
                : isSemanticToolStrip || isFileStatusSplitter
                ? false
                : isNativeTabPage
                ? false
                : isFileStatusListView || isRevisionGrid || isRevisionGridView || isNativeListView || isSourceDataGrid || isNativeTabControl
                ? true
                : control.Focusable && KeyboardNavigation.GetIsTabStop(control),
            Enabled = control is Separator || GetSourceTypeName(sourceType) == "ToolStripSeparator"
                ? false
                : isToolStripItem || isFormBrowseMenuStrip
                    ? control.IsEnabled
                    : semanticStateControl.IsEffectivelyEnabled,
            Visible = control is TabItem
                ? ((TabItem)control).IsSelected && ancestorSemanticVisible
                : isFormCommitSurface && semanticName is "Ok" or "Cancel"
                    ? true
                : semanticVisible,
            Focused = IsRepositoryHostSourceFocusedState(control, isPopupRoot)
                ? true
                : isSpellCheckTextBox && (control.IsFocused || control.ContextMenu?.IsOpen == true)
                    ? true
                : isPopupRoot || isComboBoxPopupItem ? false : IsFocused(semanticStateControl),
            ReadOnly = isComboBoxPopup
                ? true
                : isSurfaceRoot && _usesDesignerLayoutMetadata && control is not Window
                    ? GetNullableBoolProperty(control, "ReadOnly")
                : isFormCommitSurface && semanticName is "toolAuthor" or "toolStripGpgKeyTextBox"
                ? null
                : isRevisionGridView
                ? true
                : isFileViewerTextEditor || isFileViewerInternal || control.Name == "_diffViewer"
                    ? null
                    : IsSourceRichTextControl(control)
                        ? GetNullableBoolProperty(control, "IsReadOnly")
                    : IsSourceCustomControl(control, fieldName)
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
            Selected = control is Separator || GetSourceTypeName(sourceType) == "ToolStripSeparator"
                ? null
                : isSourceCheckedList
                    ? false
                : isSourceList && control is ListBox sourceList
                    ? sourceList.SelectedIndex >= 0
                : isFormBrowseMenuStrip && control is MenuItem
                    ? false
                : isFormBrowseSurface && semanticName == "listBoxSearchResult"
                    ? false
                : isSpellCheckAutoComplete && control is ListBox spellCheckAutoComplete
                ? spellCheckAutoComplete.SelectedIndex >= 0
                : (isSemanticToolStripItem && control is not Separator) || isWatermarkComboBox
                ? false
                : GetSelected(control),
            Expanded = control.Name == "treeMain" && control is TreeView treeView
                ? treeView.GetVisualDescendants().OfType<TreeViewItem>().Any(item => item.IsExpanded)
                : isFormCommitOptionsItem
                    ? false
                : isRevisionGrid || isRevisionGridView
                ? null
                : isFileStatusListView
                    ? ReadFileStatusListViewExpanded(semanticStateControl)
                : isPopupRoot
                    ? true
                    : isSemanticToolStripItem && GetPropertyValue(control, "Flyout") is FlyoutBase flyout
                        ? flyout.IsOpen
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

    private string? GetSourceType(Control control, string? fieldName)
    {
        if (!_usesDesignerLayoutMetadata || fieldName is null)
        {
            return null;
        }

        string ownerType = GetSourceOwnerType(control);
        return WinFormsInputMetadata.SourceByType.TryGetValue(ownerType, out IReadOnlyList<SourceControlMetadata>? controls)
            ? controls.FirstOrDefault(item => item.FieldName == fieldName).SourceType
            : null;
    }

    private string GetSourceOwnerType(Control control)
        => _fieldOwnerTypes.GetValueOrDefault(control)
           ?? _root.GetType().FullName
           ?? _root.GetType().Name;

    private bool IsDesignerMetadataControl(Control control)
        => _usesDesignerLayoutMetadata
           && _fieldOwnerTypes.TryGetValue(control, out string? ownerType)
           && (WinFormsInputMetadata.ByType.ContainsKey(ownerType)
               || WinFormsInputMetadata.LayoutByType.ContainsKey(ownerType)
               || WinFormsInputMetadata.SourceByType.ContainsKey(ownerType));

    private static bool IsSourceToolStrip(string? sourceType)
        => GetSourceTypeName(sourceType) is "ToolStrip" or "ToolStripEx" or "FilterToolBar" or "StatusStrip";

    private static bool IsSourceToolStripItem(string? sourceType)
    {
        string? typeName = GetSourceTypeName(sourceType);
        return typeName?.Contains("ToolStrip", StringComparison.Ordinal) == true
               && typeName is not ("ToolStrip" or "ToolStripEx" or "ToolStripContainer" or "MenuStrip" or "MenuStripEx");
    }

    private static string? GetSourceTypeName(string? sourceType)
        => sourceType is null ? null : sourceType[(sourceType.LastIndexOf('.') + 1)..];

    private static bool IsSourceAmbientControl(string? sourceType)
        => GetSourceTypeName(sourceType) is "FlowLayoutPanel" or "GroupBox" or "Label" or "LinkLabel"
            or "Panel" or "PictureBox" or "SettingsCheckBox" or "TableLayoutPanel";

    private static bool IsSourceListControl(string? sourceType)
        => GetSourceTypeName(sourceType) is "CheckedListBox" or "ListBox" or "ListView" or "NativeListView";

    private static double GetSourceListClientWidth(Rect bounds, string? sourceType)
    {
        double nativeBorder = 4;
        double nativeVerticalScrollBar = GetSourceTypeName(sourceType) == "NativeListView" ? 17 : 0;
        return Math.Max(0, bounds.Width - nativeBorder - nativeVerticalScrollBar);
    }

    private Thickness GetDefaultDesignerMargin(Control control, string? sourceType)
    {
        Thickness margin = GetSourceTypeName(sourceType) == "FileStatusList"
            ? new Thickness(3, 4)
            : control is TextBlock or Label or HyperlinkButton ? new Thickness(3, 0) : new Thickness(3);
        if (!WinFormsInputMetadata.DesignerDpiByType.TryGetValue(
                _root.GetType().FullName ?? _root.GetType().Name,
                out DesignerDpiMetadata designerDpi))
        {
            return margin;
        }

        return new Thickness(
            NormalizeDesignerDefault(margin.Left, designerDpi.Horizontal),
            NormalizeDesignerDefault(margin.Top, designerDpi.Vertical),
            NormalizeDesignerDefault(margin.Right, designerDpi.Horizontal),
            NormalizeDesignerDefault(margin.Bottom, designerDpi.Vertical));
    }

    private static double NormalizeDesignerDefault(double value, decimal sourceDpi)
        => decimal.ToDouble(decimal.Round((decimal)value * 96m / sourceDpi, 0, MidpointRounding.AwayFromZero));

    private static Thickness GetDefaultDesignerPadding(Control control)
        => control is HeaderedContentControl ? new Thickness(3) : default;

    private static bool GetDefaultDesignerAutoSize(Control control, string? sourceType)
        => control is TextBox && GetSourceTypeName(sourceType) is not "RichTextBox" and not "TextBoxBase";

    private bool GetSourceDesignerTabStop(Control control, string fieldName, string? sourceType)
    {
        string ownerType = _fieldOwnerTypes.GetValueOrDefault(control)
            ?? _root.GetType().FullName
            ?? _root.GetType().Name;
        bool? explicitTabStop = WinFormsInputMetadata.ByType.TryGetValue(
            ownerType,
            out IReadOnlyList<InputControlMetadata>? controls)
            ? controls.FirstOrDefault(item => item.FieldName == fieldName).IsTabStop
            : null;
        if (explicitTabStop is not null)
        {
            return explicitTabStop.Value;
        }

        string? sourceTypeName = GetSourceTypeName(sourceType);
        if (sourceTypeName is not null)
        {
            if (sourceTypeName is "Label" or "LinkLabel" or "Panel" or "PictureBox" or "TabPage"
                or "ToolStrip" or "ToolStripEx" or "FilterToolBar" or "MenuStrip" or "MenuStripEx")
            {
                return false;
            }

            if (sourceTypeName is "Button" or "CheckBox" or "RadioButton" or "TextBox" or "TextBoxEx"
                or "ComboBox" or "ListBox" or "TreeView" or "NumericUpDown" or "SplitContainer"
                or "TabControl" or "FullBleedTabControl"
                || sourceType?.Contains('.', StringComparison.Ordinal) == true)
            {
                return true;
            }
        }

        return control switch
        {
            RadioButton radioButton => radioButton.IsChecked == true,
            Button or HyperlinkButton or CheckBox or TextBox or ComboBox or ListBox or NumericUpDown or TabControl => true,
            TreeView => true,
            _ when control.GetType().Namespace?.StartsWith("GitUI.", StringComparison.Ordinal) == true => true,
            _ when control.Name is "PanelLeftImage" or "folderBrowserButton1" or "folderBrowserButtonUrl"
                or "folderBrowserButtonPushUrl" or "btnRemoteColor" => true,
            _ => false
        };
    }

    private int? GetSourceTabIndex(Control control, string? fieldName)
    {
        if (fieldName is null)
        {
            return null;
        }

        string ownerType = GetSourceOwnerType(control);
        return WinFormsInputMetadata.ByType.TryGetValue(
            ownerType,
            out IReadOnlyList<InputControlMetadata>? controls)
            ? controls.FirstOrDefault(item => item.FieldName == fieldName).TabIndex
            : null;
    }

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

    private static bool IsSourceCustomControl(Control control, string? fieldName)
        => fieldName is not null
           && control.GetType().Namespace?.StartsWith("GitUI.", StringComparison.Ordinal) == true;

    private static bool IsSourceBorderlessControl(Control control, string? fieldName)
        => fieldName is not null
           && (control is Image
               || control.Name == "_contentPanel"
               || control.Name == "toolPanel"
               || (IsSourceCustomControl(control, fieldName)
                   && control.GetType().Namespace != "GitUI.Compat.WinFormsControls"
                   && control is not Button
                   && control is not ToggleButton
                   && control is not ComboBox
                   && control is not MenuItem
                   && control is not Separator
                   && control is not TabControl));

    private static bool IsSourceRichTextControl(Control control)
        => control.GetType().FullName is "GitUI.Compat.XhtmlTextBlock" or "GitUI.Compat.ThemeAwareTextEditor";

    private static bool IsSourceControlTextControl(Control control, string? fieldName, string? sourceType)
    {
        if (fieldName is null
            || control is MenuItem or Separator or TextBox or ComboBox or ListBox or TreeView
            || control.Name is "LoadingFiles" or "NoFiles")
        {
            return false;
        }

        return GetSourceTypeName(sourceType) is not ("TextBox" or "RichTextBox" or "ComboBox"
            or "ListBox" or "ListView" or "TreeView" or "DataGridView" or "MenuStrip"
            or "ToolStrip" or "ToolStripEx" or "FilterToolBar");
    }

    private Rect GetSemanticBounds(Control control, Control? semanticParent)
    {
        Control? popupOwner = semanticParent is not null && IsOverlayPopupHost(semanticParent)
            ? semanticParent
            : control.GetVisualAncestors().OfType<Control>().FirstOrDefault(IsOverlayPopupHost);
        if (popupOwner is not null && control is MenuItem or Separator)
        {
            Control[] items = popupOwner.GetVisualDescendants()
                .OfType<Control>()
                .Where(item => item is MenuItem or Separator)
                .Where(item => !item.GetVisualAncestors()
                    .TakeWhile(ancestor => !ReferenceEquals(ancestor, popupOwner))
                    .OfType<MenuItem>()
                    .Any())
                .ToArray();
            int itemIndex = Array.IndexOf(items, control);
            double y = 2 + items
                .Take(Math.Max(0, itemIndex))
                .Sum(item => item is Separator ? 6 : 22);

            // parity-scaffolding: ToolStrip lays out its popup canvas with two vertical insets;
            // separators occupy a six-DIP row and retain the native two-DIP leading inset.
            return new Rect(
                control is Separator ? 2 : 0,
                y,
                Math.Max(0, popupOwner.Bounds.Width - (control is Separator ? 5 : 2)),
                control is Separator ? 6 : 22);
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
            return owner.Classes.Contains("gitextensions-workspace-tabs")
                ? new Rect(1, 29, Math.Max(0, owner.Bounds.Width - 2), Math.Max(0, owner.Bounds.Height - 30))
                : owner.Classes.Contains("gitextensions-full-bleed-tabs")
                    ? new Rect(1, 29, Math.Max(0, owner.Bounds.Width - 2), Math.Max(0, owner.Bounds.Height - 30))
                    : new Rect(4, 30, Math.Max(0, owner.Bounds.Width - 8), Math.Max(0, owner.Bounds.Height - 34));
        }

        if (semanticParent is TabItem && IsNativeTabPage(semanticParent)
            && semanticParent.GetLogicalAncestors().OfType<TabControl>().FirstOrDefault() is { } tabOwner
            && control.TranslatePoint(default, tabOwner) is Point pageChildOrigin)
        {
            // parity-scaffolding: Product content is rendered through Avalonia's selected-content
            // presenter; report it relative to the emitted WinForms-shaped TabPage client.
            return tabOwner.Classes.Contains("gitextensions-workspace-tabs")
                ? new Rect(pageChildOrigin.X - 1, pageChildOrigin.Y - 29, control.Bounds.Width, control.Bounds.Height)
                : tabOwner.Classes.Contains("gitextensions-full-bleed-tabs")
                    ? new Rect(pageChildOrigin.X - 1, pageChildOrigin.Y - 29, control.Bounds.Width, control.Bounds.Height)
                    : new Rect(pageChildOrigin.X - 4, pageChildOrigin.Y - 30, control.Bounds.Width, control.Bounds.Height);
        }

        if (control.Name == "RightSplitContainer" && semanticParent?.Name == "MainSplitContainer")
        {
            // parity-scaffolding: WinForms reports SplitterPanel2 children relative to that
            // implicit panel; the panel is flattened from the shared capture schema.
            return new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
        }

        if (semanticParent is Grid
            && GetSourceTypeName(GetSourceType(
                semanticParent,
                GetFieldNames(semanticParent).FirstOrDefault() ?? semanticParent.Name)) == "SplitContainer"
            && control is not GridSplitter
            && control.TranslatePoint(default, semanticParent) is Point splitPanelOrigin)
        {
            // WinForms omits SplitterPanel from the shared tree but reports each child relative
            // to its panel. Avalonia uses one Grid, so project second-panel children to its origin.
            return new Rect(
                Grid.GetColumn(control) > 0 ? 0 : splitPanelOrigin.X,
                Grid.GetRow(control) > 0 ? 0 : splitPanelOrigin.Y,
                control.Bounds.Width,
                control.Bounds.Height);
        }

        if (IsFileViewerTextEditor(control) && semanticParent is not null)
        {
            // The native text editor exposes a five-DIP left client inset even though its
            // outer Designer bounds begin at zero. Preserve that semantic client geometry.
            return _root.GetType().FullName == "GitUI.CommandsDialogs.FormLog"
                ? new Rect(0, 0, semanticParent.Bounds.Width, control.Bounds.Height)
                : new Rect(5, 0, Math.Max(0, semanticParent.Bounds.Width - 5), control.Bounds.Height);
        }

        if (control.GetLogicalAncestors().Any(
                ancestor => ancestor.GetType().FullName == "GitUI.CommandsDialogs.FormCommit"))
        {
            if (control.Name == "splitRight" && semanticParent?.Name == "splitMain")
            {
                // parity-scaffolding: the Grid column is WinForms SplitterPanel2, which is
                // omitted from the shared schema. Its child begins at that panel's origin.
                return new Rect(0, control.Bounds.Y, control.Bounds.Width, control.Bounds.Height);
            }

            if (control.Name == "tableLayoutPanel1" && semanticParent?.Name == "splitRight")
            {
                // parity-scaffolding: report the panel-two child relative to SplitterPanel2,
                // retaining the source panel's one-DIP padding.
                return new Rect(control.Margin.Left, control.Margin.Top, control.Bounds.Width, control.Bounds.Height);
            }

            if (semanticParent?.Name == "splitLeft"
                && control.Name is "toolbarStaged" or "Staged" or "LoadingStaged"
                && control.GetLogicalAncestors().OfType<Grid>().FirstOrDefault(
                    ancestor => string.IsNullOrEmpty(ancestor.Name) && Grid.GetRow(ancestor) == 2) is { } panelTwo
                && control.TranslatePoint(default, panelTwo) is Point panelOrigin)
            {
                // parity-scaffolding: WinForms reports these controls in SplitterPanel2
                // coordinates; Avalonia's unnamed row owner represents that implicit panel.
                return new Rect(
                    panelOrigin.X + panelTwo.Margin.Left,
                    panelOrigin.Y + panelTwo.Margin.Top,
                    control.Bounds.Width,
                    control.Bounds.Height);
            }
        }

        if ((control.Name is "ToolStripMain" or "ToolStripFilters")
            && control.GetLogicalAncestors().OfType<Grid>().FirstOrDefault(
                ancestor => ancestor.Name == (control.Name == "ToolStripMain"
                    ? "toolStripMainHost"
                    : "toolStripFiltersHost")) is { } toolbarHost)
        {
            return GetSemanticBounds(toolbarHost, semanticParent);
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
    {
        if (IsSpellCheckAutoComplete(control))
        {
            return [];
        }

        string? fieldName = GetFieldNames(control).FirstOrDefault()
                            ?? (string.IsNullOrEmpty(control.Name) ? null : control.Name);
        if (IsSourceListControl(GetSourceType(control, fieldName)))
        {
            // parity-scaffolding: WinForms list rows are native-rendered items, not child
            // controls. Avalonia's recycled ListBoxItem containers are renderer internals.
            return [];
        }

        IEnumerable<Control> children = GetCaptureChildren(control).SelectMany(ExpandSemanticChild);
        if (ReferenceEquals(control, _root)
            && _root.GetType().FullName == "GitUI.CommandsDialogs.FormCommit")
        {
            children = children.Concat(GetDetachedSourceFields()).Distinct();
        }

        return children;
    }

    private IEnumerable<Control> GetDetachedSourceFields()
    {
        string rootType = _root.GetType().FullName ?? _root.GetType().Name;
        if (!WinFormsInputMetadata.SourceByType.TryGetValue(
                rootType,
                out IReadOnlyList<SourceControlMetadata>? sourceControls))
        {
            return [];
        }

        HashSet<Control> reachable = [];
        Queue<Control> pending = new();
        pending.Enqueue(_root);
        while (pending.TryDequeue(out Control? owner))
        {
            foreach (Control child in GetCaptureChildren(owner))
            {
                if (reachable.Add(child))
                {
                    pending.Enqueue(child);
                }
            }
        }

        List<Control> detached = [];
        foreach (SourceControlMetadata metadata in sourceControls)
        {
            Control? field = _fieldNames
                .Where(pair => pair.Value.Contains(metadata.FieldName, StringComparer.Ordinal))
                .Select(pair => pair.Key)
                .OfType<Control>()
                .FirstOrDefault(candidate => GetSourceOwnerType(candidate) == rootType);
            if (field is not null && !reachable.Contains(field) && !detached.Contains(field))
            {
                detached.Add(field);
            }
        }

        return detached;
    }

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
                Foreground = ResolveSourceControlTextArgb()
                             ?? BrushToArgb(GetPropertyValue(control, "Foreground")),
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
                Foreground = ResolveSourceControlTextArgb()
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
        if (control is ListBox { Name: "_gridView" }
            && control.GetLogicalAncestors().OfType<RevisionGridControl>().FirstOrDefault() is { } revisionGrid)
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

    private string? ResolveSourceControlTextArgb()
        => _root.GetType().FullName is
            "GitUI.CommandsDialogs.BrowseDialog.FormGitCommandLog" or
            "GitUI.CommandsDialogs.FormCommit" or
            "GitUI.CommandsDialogs.FormCompareToBranch" or
            "GitUI.CommandsDialogs.FormDiff" or
            "GitUI.CommandsDialogs.FormFormatPatch" or
            "GitUI.CommandsDialogs.FormLog"
            ? ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")
            : ResolveResourceArgb("GitExtensionsSourceControlTextBrush");

    private bool IsRevisionGridView(Control control) =>
        control is ListBox { Name: "_gridView" }
        && (GetSourceOwnerType(control) == "GitUI.RevisionGridControl"
            || control.GetLogicalAncestors().OfType<RevisionGridControl>().Any());

    private bool IsFileStatusToolbar(Control control)
        => control is StackPanel { Name: "Toolbar" }
           && (GetSourceOwnerType(control) == "GitUI.FileStatusList"
               || control.GetLogicalAncestors().OfType<FileStatusList>().Any());

    private bool IsFileStatusToolbarItem(Control control)
        => control.Parent is StackPanel toolbar && IsFileStatusToolbar(toolbar);

    private bool IsTransparentToolStripItem(Control control)
        => (_root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse"
            && control.Name is "_NO_TRANSLATE_WorkingDir" or "branchSelect" or "menuCommitInfoPosition"
               or "RefreshButton" or "toggleLeftPanel" or "toggleSplitViewLayout"
               or "toolStripButtonLevelUp" or "toolStripButtonPull" or "toolStripSeparator0"
               or "toolStripSeparator1" or "toolStripSeparator17" or "toolStripWorktrees"
               or "tsddbtnRevisionFilter")
           || (_root.GetType().FullName is "GitUI.CommandsDialogs.FormDiff" or "GitUI.CommandsDialogs.FormLog"
               && IsFileStatusToolbarItem(control)
                && control.Name is not ("btnCollapseGroups" or "btnRefresh" or "sepRefresh"));

    private bool IsWindowBackgroundToolStripItem(Control control)
    {
        if (_root.GetType().FullName is "GitUI.CommandsDialogs.FormDiff" or "GitUI.CommandsDialogs.FormLog"
            && IsFileStatusToolbarItem(control))
        {
            return false;
        }

        if (control.Name is "sepRefresh"
            || (control.Name is "btnCollapseGroups" or "btnRefresh"
                && control.GetLogicalAncestors().OfType<TabItem>().Any(tab => tab.Name == "TreeTabPage")))
        {
            return false;
        }

        return control.Name is "tscboBranchFilter" or "tstxtRevisionFilter"
               || IsFileStatusToolbarProductItem(control);
    }

    private bool IsFileStatusToolbarProductItem(Control control)
        => IsFileStatusToolbarProductItemName(control.Name)
           || GetFieldNames(control).Any(IsFileStatusToolbarProductItemName);

    private static bool IsFileStatusToolbarProductItemName(string? fieldName)
        => fieldName is
            "btnCollapseGroups" or "sepRefresh" or "btnRefresh" or "sepAsTree" or "btnAsTree"
            or "sepGroupBy" or "btnByPath" or "btnByExtension" or "btnByStatus" or "sepFilter"
            or "btnUnequalChange" or "btnOnlyB" or "btnOnlyA" or "btnSameChange" or "sepOptions"
            or "btnFindInFilesGitGrep" or "sepSettings" or "btnSettings";

    private static bool IsWindowTextToolStripItem(Control control) => false;

    private bool IsSourceControlTextToolStripItem(Control control)
        => (_root.GetType().FullName == "GitUI.CommandsDialogs.FormCommit"
            && control.GetLogicalAncestors().OfType<Control>().Any(
                ancestor => ancestor.Name is "toolbarSelectionFilter" or "toolbarStaged"
                    or "toolbarCommit" or "commitStatusStrip" or "fileviewerToolbar"))
           || ((_root.GetType().FullName is
                "GitUI.CommandsDialogs.FormBlame" or
                    "GitUI.CommandsDialogs.FormCompareToBranch" or
                    "GitUI.CommandsDialogs.FormDiff" or
                    "GitUI.CommandsDialogs.FormFormatPatch" or
                    "GitUI.CommandsDialogs.FormLog")
               && IsSemanticToolStripItem(control))
           || (_root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse"
           && (control.Name is "toolStripLabel1" or "tsbShowReflog" or "tsbtnAdvancedFilter"
                or "tsddbtnBranchFilter" or "tslblRevisionFilter" or "tsmiShowOnlyFirstParent"
                or "tssbtnShowBranches" or "toolStripSplitStash" or "toolStripFileExplorer"
                or "toolStripButtonPush" or "toolStripButtonCommit" or "EditSettings" or "userShell"
                || (control.Name is "btnRefresh" or "btnCollapseGroups"
                    && control.GetLogicalAncestors().OfType<TabItem>().Any(tab => tab.Name == "TreeTabPage"))));

    private bool IsFileViewerToolbar(Control control)
        => control is Border { Name: "fileviewerToolbar" }
           && (GetSourceOwnerType(control) == "GitUI.Editor.FileViewer"
               || control.GetLogicalAncestors().Any(ancestor => ancestor.GetType().FullName == "GitUI.Editor.FileViewer"));

    private bool IsFileViewerToolbarItem(Control control)
        => control.Parent is StackPanel stackPanel
           && stackPanel.Parent is Control toolbar
           && IsFileViewerToolbar(toolbar);

    private bool IsSemanticToolStrip(Control control)
        => IsFileStatusToolbar(control) || IsFileViewerToolbar(control);

    private bool IsSemanticToolStripItem(Control control)
        => IsFileStatusToolbarItem(control) || IsFileViewerToolbarItem(control);

    private bool IsFileViewerTextEditor(Control control)
        => control.Name == "TextEditor"
           && (GetSourceOwnerType(control) == "GitUI.Editor.FileViewerInternal"
               || control.GetLogicalAncestors().Any(ancestor => ancestor.GetType().FullName == "GitUI.Editor.FileViewerInternal"));

    private static bool IsFileViewerInternal(Control control)
        => control.Name == "internalFileViewer"
           && control.GetType().FullName == "GitUI.Editor.FileViewerInternal";

    private bool IsFileViewerPictureBox(Control control)
        => control.Name == "PictureBox"
           && (GetSourceOwnerType(control) == "GitUI.Editor.FileViewer"
               || control.GetLogicalAncestors().Any(ancestor => ancestor.GetType().FullName == "GitUI.Editor.FileViewer"));

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
    {
        if (control is not TextBox textBox
            || !string.IsNullOrEmpty(textBox.Text)
            || textBox.IsKeyboardFocusWithin
            || !IsInSelectedTab(control))
        {
            return false;
        }

        return !string.IsNullOrEmpty(GetSpellCheckWatermark(control));
    }

    private static string? GetSpellCheckWatermark(Control control)
        => control.GetLogicalAncestors()
            .OfType<Control>()
            .Where(ancestor => ancestor.GetType().FullName == "GitUI.SpellChecker.EditNetSpell")
            .Select(editor => GetPropertyValue(editor, "WatermarkText") as string)
            .FirstOrDefault();

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
            && control is Panel or ScrollViewer
            && string.IsNullOrEmpty(control.Name)
            && GetFieldNames(control).Count == 0)
           || (_root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse"
                && control.Name is "toolStripMainHost" or "toolStripMainViewport"
                    or "toolStripFiltersHost" or "toolStripFiltersViewport"
                    or "mainContentGrid" or "leftPanel"
                    or "commitInfoLeftHost" or "commitInfoRightHost"
                    or "commitInfoBelowHost" or "outputHistoryPanelHost" or "_filterHost")
           || (_root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse"
               && string.IsNullOrEmpty(control.Name)
               && (control.Parent?.Name == "tableLayoutPanel1"
                   || control.Parent?.Name == "DiffSplitContainer"
                   || control.Parent?.Name == "RevisionInfo"))
           || (control is Grid
                 && string.IsNullOrEmpty(control.Name)
                 && control.Parent?.GetType().FullName == "GitUI.Compat.WinFormsControls.FlowLayoutPanel")
           || (control is StackPanel
                && string.IsNullOrEmpty(control.Name)
                && control.Parent?.GetType().FullName == "GitUI.Compat.WinFormsControls.FlowLayoutPanel")
           || (_root.GetType().FullName == "GitUI.SpellChecker.EditNetSpell"
               && control is Grid or Canvas
               && string.IsNullOrEmpty(control.Name))
           || control.Name == "columnsGrid"
           || (_root.GetType().FullName == "GitUI.CommandsDialogs.FormCommit"
               && control.Name == "toolbarCommitInlineItems")
           || control.Name == "FindInCommitFilesGitGrepPanel"
           || (control is StackPanel
               && control.Parent is Control parent
               && IsFileViewerToolbar(parent));

    private bool IsRendererOnlyControl(Control control)
        => control.Name == "ImagePreview"
           || control.Name is "toolStripMainOverflow" or "toolStripFiltersOverflow"
           || (_root.GetType().FullName == "GitUI.CommandsDialogs.FormCommit"
               && control.Name is "toolbarCommitOverflow" or "commitTemplatesOverflowMenuItem"
                   or "createBranchOverflowMenuItem")
           || (_root.GetType().FullName == "GitUI.CommandsDialogs.FormCommit"
               && control is GridSplitter)
           || (_root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse"
               && (control is GridSplitter
                   || control.Name is "lblRepoPath" or "lblStatus"
                   || control.Parent is TreeView))
           || (control.GetType().Namespace == "GitUI.Compat.WinFormsControls"
                && (control.GetType().Name == "ColumnHeader"
                    || control.GetType().Name.EndsWith("Column", StringComparison.Ordinal)))
           || (control is Image
               && control.Parent?.GetType().FullName == "GitUI.Compat.WinFormsControls.PictureBox")
           || control.GetType().FullName == "GitUI.SpellChecker.SpellCheckAdorner"
            || (control is GridSplitter
                && control.Parent is Control parent
                && (IsRepositoryHostSplit(parent)
                    || GetSourceTypeName(GetSourceType(
                        parent,
                        GetFieldNames(parent).FirstOrDefault() ?? parent.Name)) == "SplitContainer"));

    private static bool IsFormCommitToolStripPanel(string? name)
        => name is "_topPanel" or "_bottomPanel" or "_leftPanel" or "_rightPanel" or "_contentPanel";

    private static bool IsFormCommitStatusItem(string? name)
        => name is "commitAuthorStatus" or "toolStripProgressBar1" or "toolStripStatusBranchIcon"
            or "branchNameLabel" or "remoteNameLabel" or "commitStagedCountLabel" or "commitStagedCount"
            or "commitCursorLineLabel" or "commitCursorLine" or "commitCursorColumnLabel"
            or "commitCursorColumn" or "commitEndPadding";

    private static bool IsFormCommitStandardStatusItem(string? name)
        => name is "commitAuthorStatus" or "toolStripStatusBranchIcon" or "commitStagedCountLabel"
            or "commitStagedCount" or "commitCursorLineLabel" or "commitCursorLine"
            or "commitCursorColumnLabel" or "commitCursorColumn" or "commitEndPadding";

    private static bool IsFormCommitOptionsItem(string? name)
        => name is "closeDialogAfterEachCommitToolStripMenuItem"
            or "closeDialogAfterAllFilesCommittedToolStripMenuItem"
            or "refreshDialogOnFormFocusToolStripMenuItem"
            or "tsmiSelectStagedOnEnterMessage"
            or "signOffToolStripMenuItem"
            or "toolAuthorLabelItem"
            or "noVerifyToolStripMenuItem";

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
        if (TopLevel.GetTopLevel(control) is null
            && control.GetLogicalAncestors().All(ancestor => ancestor is not Window))
        {
            // parity-scaffolding: controls owned by a closed Flyout retain their local
            // IsVisible value in Avalonia, while WinForms reports the detached drop-down
            // subtree as not visible until its native popup is opened.
            return false;
        }

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

    private static bool IsInsideClippedAncestors(Control control)
    {
        foreach (Control ancestor in control.GetVisualAncestors().OfType<Control>().Where(ancestor => ancestor.ClipToBounds))
        {
            if (control.TranslatePoint(default, ancestor) is not Point origin
                || !new Rect(origin, control.Bounds.Size).Intersects(new Rect(ancestor.Bounds.Size)))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsComboBoxPopup(Control control)
        => control.GetVisualDescendants().OfType<ListBoxItem>().Any();

    private static bool IsComboBoxPopupItem(Control control)
        => control is ListBoxItem
           && control.GetVisualAncestors().OfType<Control>().Any(
               ancestor => IsPopupPresenter(ancestor) || IsOverlayPopupHost(ancestor));

    private CaptureColors ReadToolStripColors(
        Control control,
        bool isItem,
        bool transparentBackground,
        bool windowBackground = false,
        bool useWindowText = false,
        bool useControlText = false)
    {
        bool isSeparator = control is Separator
            || GetSourceTypeName(GetSourceType(control, GetFieldNames(control).FirstOrDefault())) == "ToolStripSeparator";
        bool isFormCommitFileStatus = _root.GetType().FullName == "GitUI.CommandsDialogs.FormCommit"
            && (IsFileStatusToolbar(control)
                || IsFileStatusToolbarItem(control)
                || IsFileStatusToolbarProductItem(control));
        string? background = transparentBackground
            ? "#00FFFFFF"
            : isFormCommitFileStatus
                ? control.Name == "btnRefresh"
                    && control.GetLogicalAncestors().OfType<FileStatusList>().Any(list => list.Name == "Unstaged")
                    ? ResolveResourceArgb("GitExtensionsPaneBorderBrush")
                    : control.Name is "btnCollapseGroups" or "btnRefresh" or "sepRefresh"
                    ? ResolveResourceArgb("GitExtensionsKnownColorControlBrush")
                    : ResolveResourceArgb("GitExtensionsPaneBorderBrush")
            : windowBackground
                ? ResolveResourceArgb("GitExtensionsWindowBackgroundBrush")
                : ResolveResourceArgb("GitExtensionsKnownColorControlBrush")
                  ?? ResolveResourceArgb("GitExtensionsControlBackgroundBrush");
        string? foreground = isSeparator
            ? ResolveResourceArgb("GitExtensionsKnownColorControlDarkBrush")
              ?? ResolveResourceArgb("GitExtensionsControlBorderBrush")
            : control.Name is "btnUnequalChange" or "btnOnlyB" or "btnOnlyA" or "btnSameChange"
                ? BrushToArgb(GetPropertyValue(control, "Foreground"))
            : control.Name == "encodingToolStripComboBox"
                ? ResolveResourceArgb("GitExtensionsMenuForegroundBrush")
                  ?? ResolveResourceArgb("GitExtensionsWindowTextBrush")
                : useWindowText
                    ? ResolveResourceArgb("GitExtensionsWindowTextBrush")
                : isFormCommitFileStatus || useControlText
                    ? ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")
                      ?? ResolveResourceArgb("GitExtensionsControlForegroundBrush")
                : isItem
                    ? ResolveResourceArgb("GitExtensionsMenuForegroundBrush")
                    : ResolveSourceControlTextArgb()
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

    private CaptureColors ReadFormBrowseToolStripContainerColors()
    {
        string? background = ResolveResourceArgb("GitExtensionsWindowBackgroundBrush");
        return new CaptureColors
        {
            Foreground = ResolveResourceArgb("GitExtensionsWindowTextBrush"),
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

    private CaptureColors ReadSourceControlTextColors(Control control)
    {
        CaptureColors colors = ReadColors(control);
        return colors with
        {
            Foreground = ResolveSourceControlTextArgb()
                         ?? ResolveResourceArgb("GitExtensionsControlForegroundBrush")
        };
    }

    private CaptureColors? ReadFormBrowseSemanticColors(
        Control control,
        string sourceOwnerType,
        string? fieldName)
    {
        if (_root.GetType().FullName != "GitUI.CommandsDialogs.FormBrowse")
        {
            return null;
        }

        string? name = fieldName ?? control.Name;

        if (name == "treeMain")
        {
            return ReadNativeSelectionColors(control, "GitExtensionsPanelBackgroundBrush") with
            {
                Border = null
            };
        }

        if (name == "listBoxSearchResult")
        {
            return ReadNativeSelectionColors(control, "GitExtensionsWindowBackgroundBrush");
        }

        if (name is "LoadingFiles" or "NoFiles")
        {
            CaptureColors colors = ReadColors(control);
            string? background = ResolveResourceArgb("GitExtensionsPanelBackgroundBrush");
            return colors with
            {
                Foreground = ResolveResourceArgb("GitExtensionsKnownColorGrayTextBrush"),
                Background = background,
                Border = null,
                DisabledBackground = background
            };
        }

        if (sourceOwnerType == "GitUI.UserControls.OutputHistoryControl" && name == "TextBox")
        {
            CaptureColors colors = ReadColors(control);
            string? background = ResolveResourceArgb(IsDarkTheme()
                ? "GitExtensionsKnownColorControlLightBrush"
                : "GitExtensionsKnownColorControlBrush");
            return colors with
            {
                Foreground = ResolveSourceControlTextArgb(),
                Background = background,
                Border = null,
                DisabledBackground = background
            };
        }

        if (name is "RevisionGrid" or "RevisionGridContainer")
        {
            return ReadSourceBackgroundColors(
                control,
                "GitExtensionsSectionBorderBrush",
                ResolveSourceControlTextArgb());
        }

        if (name is "DiffSplitContainer" or "DiffText" or "internalFileViewer"
            or "splitContainer1" or "BlameControl" or "BlameFile" or "_NO_TRANSLATE_lblShowPreview")
        {
            return ReadTransparentContainerColors(control);
        }

        if (name is "DiffTabPage" or "TreeTabPage" or "GpgInfoTabPage" or "revisionDiff"
            or "fileTree" or "revisionGpgInfo1"
            || (sourceOwnerType == "GitUI.CommandsDialogs.RevisionGpgInfoControl" && name == "tableLayoutPanel1"))
        {
            return ReadSourceBackgroundColors(
                control,
                "GitExtensionsNativeTabPageBackgroundBrush",
                ResolveSourceControlTextArgb());
        }

        if (name is "commitSignPicture" or "tagSignPicture")
        {
            return ReadSourceBackgroundColors(
                control,
                "GitExtensionsNativeTabPageBackgroundBrush",
                ResolveSourceControlTextArgb());
        }

        if (name is "branchSearchPanel" or "btnSearch" or "CommitInfoTabControl")
        {
            return ReadSourceBackgroundColors(
                control,
                "GitExtensionsKnownColorControlBrush",
                ResolveSourceControlTextArgb());
        }

        if (name is "_contentPanel" or "CommitInfo" or "DiffFiles" or "lblSplitter")
        {
            return ReadSourceBackgroundColors(
                control,
                "GitExtensionsWindowBackgroundBrush",
                ResolveSourceControlTextArgb());
        }

        if (name == "RevisionInfo")
        {
            return ReadSourceBackgroundColors(
                control,
                sourceOwnerType == "GitUI.CommandsDialogs.FormBrowse"
                    ? "GitExtensionsPanelBackgroundBrush"
                    : "GitExtensionsWindowBackgroundBrush",
                ResolveSourceControlTextArgb());
        }

        if (name == "CommitInfoTabPage")
        {
            return ReadSourceBackgroundColors(
                control,
                "GitExtensionsPanelBackgroundBrush",
                ResolveSourceControlTextArgb());
        }

        if (name == "_txtBranchCriterion"
            || (sourceOwnerType == "GitUI.CommandsDialogs.SearchControl" && name == "tableLayoutPanel1"))
        {
            return ReadTransparentContainerColors(control);
        }

        return null;
    }

    private CaptureColors? ReadBlameLogSemanticColors(Control control, string? fieldName)
    {
        string? rootType = _root.GetType().FullName;
        string? name = fieldName ?? control.Name;
        bool isTransparentSourceContainer = rootType switch
        {
            "GitUI.CommandsDialogs.FormBlame" => name is "splitContainer1" or "splitContainer2"
                or "BlameAuthor" or "BlameFile" or "internalFileViewer",
            "GitUI.CommandsDialogs.FormLog" => name is "splitContainer1" or "splitContainer3"
                or "RevisionGrid" or "DiffFiles" or "diffViewer" or "internalFileViewer",
            "GitUI.CommandsDialogs.BrowseDialog.FormGitCommandLog" => name is "splitContainer1" or "splitContainer2",
            _ => false,
        };

        if (isTransparentSourceContainer)
        {
            CaptureColors colors = ReadTransparentContainerColors(control);
            return rootType == "GitUI.CommandsDialogs.FormBlame"
                ? colors with { Foreground = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush") }
                : colors;
        }

        if (rootType == "GitUI.CommandsDialogs.FormBlame"
            && name is "_NO_TRANSLATE_lblShowPreview" or "avatarControl" or "blameControl1" or "CommitInfo"
                or "commitInfoHeader" or "PictureBox")
        {
            return ReadColors(control) with
            {
                Foreground = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")
            };
        }

        return null;
    }

    private CaptureColors? ReadFormCommitSemanticColors(Control control, string? fieldName)
    {
        if (_root.GetType().FullName != "GitUI.CommandsDialogs.FormCommit")
        {
            return null;
        }

        string? name = fieldName ?? control.Name;
        string? controlName = control.Name;
        string? controlText = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush");
        if (ReferenceEquals(control, _root))
        {
            return ReadSourceBackgroundColors(control, "GitExtensionsOtherBackgroundBrush", controlText);
        }

        if (name is "splitMain" or "splitLeft" or "splitRight")
        {
            return ReadTransparentContainerColors(control) with
            {
                Foreground = controlText
            };
        }

        if (IsFormCommitToolStripPanel(name))
        {
            return ReadSourceBackgroundColors(
                control,
                "GitExtensionsPaneBorderBrush",
                ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush"));
        }

        if (name is "toolStripContainer1" or "Unstaged" or "Staged" or "SelectedDiff" or "Toolbar"
            || controlName is "Unstaged" or "Staged")
        {
            return ReadSourceBackgroundColors(
                control,
                "GitExtensionsPaneBorderBrush",
                controlText);
        }

        if (name is "toolbarSelectionFilter" or "lblSplitter")
        {
            return ReadSourceBackgroundColors(
                control,
                "GitExtensionsPaneBorderBrush",
                controlText);
        }

        if (name == "toolStripLabel1")
        {
            CaptureColors colors = ReadSourceBackgroundColors(
                control,
                "GitExtensionsPaneBorderBrush",
                controlText);
            return AddSourceToolStripSelectionColors(colors);
        }

        if (IsFormCommitOptionsItem(name))
        {
            return ReadToolStripColors(
                control,
                isItem: true,
                transparentBackground: false,
                useControlText: true);
        }

        if (name is "toolbarStaged" or "tableLayoutPanel1" or "toolbarCommit" or "commitStatusStrip" or "Message"
            or "Loading" or "_waitSpinner" or "flowCommitButtons" or "AmendPanel"
            or "Commit" or "CommitAndPush" or "StageInSuperproject" or "Amend" or "ResetAuthor"
            or "ResetSoft" or "StashStaged" or "btnResetAllChanges" or "btnResetUnstagedChanges")
        {
            return ReadSourceBackgroundColors(
                control,
                "GitExtensionsKnownColorControlBrush",
                controlText);
        }

        if (name == "LoadingStaged")
        {
            return ReadSourceBackgroundColors(
                control,
                "GitExtensionsKnownColorAppWorkspaceBrush",
                controlText);
        }

        if (name == "selectionFilter")
        {
            CaptureColors colors = ReadSourceInputColors(control);
            return colors with
            {
                SelectionForeground = ResolveResourceArgb("GitExtensionsKnownColorHighlightTextBrush"),
                SelectionBackground = ResolveResourceArgb("GitExtensionsKnownColorHighlightBrush"),
                InactiveSelectionForeground = ResolveResourceArgb("GitExtensionsKnownColorMenuTextBrush")
                                              ?? ResolveResourceArgb("GitExtensionsMenuForegroundBrush"),
                InactiveSelectionBackground = ResolveResourceArgb("GitExtensionsKnownColorMenuBrush")
                                              ?? ResolveResourceArgb("GitExtensionsMenuBackgroundBrush")
            };
        }

        if (name is "DeleteFilterButton" or "DeleteSearchButton")
        {
            return ReadSourceBackgroundColors(
                control,
                "GitExtensionsWindowBackgroundBrush",
                controlText);
        }

        if (name is "toolAuthor" or "toolStripGpgKeyTextBox")
        {
            CaptureColors colors = TopLevel.GetTopLevel(control) is null
                ? ReadSourceBackgroundColors(
                    control,
                    "GitExtensionsWindowBackgroundBrush",
                    ResolveResourceArgb("GitExtensionsWindowTextBrush"))
                : ReadSourceInputColors(control);
            return AddSourceToolStripSelectionColors(colors);
        }

        if (name == "toolStripProgressBar1")
        {
            CaptureColors colors = ReadSourceBackgroundColors(
                control,
                "GitExtensionsKnownColorControlBrush",
                ResolveResourceArgb("GitExtensionsKnownColorHighlightBrush"));
            return AddSourceToolStripSelectionColors(colors);
        }

        if (name is "LoadingFiles" or "NoFiles")
        {
            return ReadSourceBackgroundColors(
                control,
                "GitExtensionsWindowBackgroundBrush",
                ResolveResourceArgb("GitExtensionsKnownColorGrayTextBrush"));
        }

        if (name == "modifyCommitMessageButton")
        {
            return ReadSourceBackgroundColors(
                control,
                "GitExtensionsKnownColorControlBrush",
                ResolveResourceArgb(IsDarkTheme()
                    ? "GitExtensionsKnownColorControlTextBrush"
                    : "GitExtensionsKnownColorHotTrackBrush"));
        }

        if (name == "SolveMergeconflicts")
        {
            return ReadSourceBackgroundColors(
                control,
                "GitExtensionsMergeConflictsBackgroundBrush",
                controlText);
        }

        if (name is "internalFileViewer" or "TextEditor" or "_NO_TRANSLATE_lblShowPreview")
        {
            return ReadSourceBackgroundColors(
                control,
                "GitExtensionsPaneBorderBrush",
                controlText);
        }

        if (name == "Cancel")
        {
            return ReadSourceBackgroundColors(control, "GitExtensionsPaneBorderBrush", controlText);
        }

        if (name == "Ok")
        {
            return ReadTransparentContainerColors(control) with
            {
                Foreground = controlText
            };
        }

        return null;
    }

    private CaptureColors AddSourceToolStripSelectionColors(CaptureColors colors)
        => colors with
        {
            SelectionForeground = ResolveResourceArgb("GitExtensionsKnownColorHighlightTextBrush"),
            SelectionBackground = ResolveResourceArgb("GitExtensionsKnownColorHighlightBrush"),
            InactiveSelectionForeground = ResolveResourceArgb("GitExtensionsKnownColorMenuTextBrush")
                                          ?? ResolveResourceArgb("GitExtensionsMenuForegroundBrush"),
            InactiveSelectionBackground = ResolveResourceArgb("GitExtensionsKnownColorMenuBrush")
                                          ?? ResolveResourceArgb("GitExtensionsMenuBackgroundBrush")
        };

    private CaptureColors ReadSourceBackgroundColors(
        Control control,
        string backgroundResource,
        string? foreground)
    {
        CaptureColors colors = ReadColors(control);
        string? background = ResolveResourceArgb(backgroundResource);
        return colors with
        {
            Foreground = foreground ?? colors.Foreground,
            Background = background,
            Border = null,
            DisabledBackground = background
        };
    }

    private CaptureColors ReadFileStatusListViewColors(Control control)
        => ReadNativeSelectionColors(control, "GitExtensionsPanelBackgroundBrush");

    private CaptureColors ReadFileStatusEmptyLabelColors(Control control)
    {
        CaptureColors colors = ReadColors(control);
        string? background = ResolveResourceArgb("GitExtensionsPanelBackgroundBrush");
        return colors with
        {
            Foreground = ResolveResourceArgb("GitExtensionsKnownColorGrayTextBrush")
                         ?? ResolveResourceArgb("GitExtensionsDisabledForegroundBrush"),
            Background = background,
            Border = null,
            DisabledBackground = background
        };
    }

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
        string? background = ResolveSourceAmbientBackground(control, "GitExtensionsWindowBackgroundBrush");
        return colors with
        {
            Foreground = isPreviewLink || isHelpLink || isDesignerLink
                ? ResolveSourceControlTextArgb()
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
            DisabledBackground = isPreviewLink
                ? "#00FFFFFF"
                : isHelpLink || isDesignerLink ? background : colors.DisabledBackground
        };
    }

    private CaptureColors ReadSourceDesignerColors(
        Control control,
        DesignerLayoutMetadata? designerLayout = null)
    {
        CaptureColors colors = ReadColors(control);
        string? background = ResolveSourceAmbientBackground(control, "GitExtensionsControlBackgroundBrush");
        return colors with
        {
            Foreground = designerLayout?.HasExplicitForeground == true
                ? colors.Foreground
                : ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")
                  ?? ResolveSourceControlTextArgb()
                  ?? ResolveResourceArgb("GitExtensionsControlForegroundBrush"),
            Background = designerLayout?.HasExplicitBackground == true ? colors.Background : background,
            Border = null,
            SelectionForeground = null,
            SelectionBackground = null,
            InactiveSelectionForeground = null,
            InactiveSelectionBackground = null,
            DisabledBackground = designerLayout?.HasExplicitBackground == true ? colors.DisabledBackground : background,
            DisabledForeground = ResolveResourceArgb("GitExtensionsKnownColorGrayTextBrush")
                                 ?? ResolveResourceArgb("GitExtensionsDisabledForegroundBrush")
        };
    }

    private static IEnumerable<Control> EnumerateSelfAndLogicalAncestors(Control control)
    {
        yield return control;
        foreach (Control ancestor in control.GetLogicalAncestors().OfType<Control>())
        {
            yield return ancestor;
        }
    }

    private static bool IsVisibleBackground(string? color)
        => color is not null && !color.StartsWith("#00", StringComparison.Ordinal);

    private string? ResolveSourceAmbientBackground(Control control, string fallbackResource)
        => EnumerateSelfAndLogicalAncestors(control)
               .Select(candidate => BrushToArgb(GetPropertyValue(candidate, "Background")))
               .FirstOrDefault(IsVisibleBackground)
           ?? ResolveResourceArgb(fallbackResource);

    private string? ResolveSourceAncestorBackground(Control control, string fallbackResource)
        => control.GetLogicalAncestors()
               .OfType<Control>()
               .Select(ancestor => BrushToArgb(GetPropertyValue(ancestor, "Background")))
               .FirstOrDefault(IsVisibleBackground)
           ?? ResolveResourceArgb(fallbackResource);

    private CaptureColors ReadSourceDesignerButtonColors(Control control)
    {
        CaptureColors colors = ReadColors(control);
        string? background = ResolveSourceAncestorBackground(control, "GitExtensionsControlBackgroundBrush");
        bool isTransparentSourceButton = control.Name == "btnRemoteColor";
        return colors with
        {
            Foreground = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")
                         ?? ResolveSourceControlTextArgb()
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
            Foreground = ResolveSourceControlTextArgb() ?? colors.Foreground,
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
            Foreground = ResolveSourceControlTextArgb(),
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
            : control is TextBox or NumericUpDown
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

    private CaptureColors ReadSourceRichTextColors()
    {
        string? background = ResolveResourceArgb("GitExtensionsKnownColorControlLightBrush");
        return new CaptureColors
        {
            Foreground = ResolveResourceArgb("GitExtensionsSourceControlTextBrush"),
            Background = background,
            Border = null,
            SelectionForeground = null,
            SelectionBackground = null,
            InactiveSelectionForeground = null,
            InactiveSelectionBackground = null,
            DisabledForeground = ResolveResourceArgb("GitExtensionsDisabledForegroundBrush"),
            DisabledBackground = background,
            GridLine = null,
            Additional = new SortedDictionary<string, string>(StringComparer.Ordinal)
        };
    }

    private CaptureColors ReadSourceListColors()
    {
        string? background = ResolveResourceArgb("GitExtensionsKnownColorWindowBrush")
                             ?? ResolveResourceArgb("GitExtensionsWindowBackgroundBrush");
        string? highlightText = ResolveResourceArgb("GitExtensionsKnownColorHighlightTextBrush")
                                ?? ResolveResourceArgb("GitExtensionsHighlightForegroundBrush");
        return new CaptureColors
        {
            Foreground = ResolveResourceArgb("GitExtensionsKnownColorWindowTextBrush")
                         ?? ResolveResourceArgb("GitExtensionsWindowTextBrush"),
            Background = background,
            Border = null,
            SelectionForeground = highlightText,
            SelectionBackground = ResolveResourceArgb("GitExtensionsKnownColorHighlightBrush")
                                  ?? ResolveResourceArgb("GitExtensionsHighlightBackgroundBrush"),
            InactiveSelectionForeground = highlightText,
            InactiveSelectionBackground = ResolveResourceArgb("GitExtensionsKnownColorInactiveCaptionBrush")
                                          ?? ResolveResourceArgb("GitExtensionsSystemInactiveSelectionBackgroundBrush"),
            DisabledForeground = ResolveResourceArgb("GitExtensionsKnownColorGrayTextBrush")
                                 ?? ResolveResourceArgb("GitExtensionsDisabledForegroundBrush"),
            DisabledBackground = background,
            GridLine = null,
            Additional = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["hotTrack"] = ResolveResourceArgb("GitExtensionsKnownColorHotTrackBrush")
                               ?? ResolveResourceArgb("GitExtensionsNativeListHotTrackBrush")
                               ?? throw new InvalidDataException("The source list hot-track color did not resolve.")
            }
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
            Foreground = ResolveSourceControlTextArgb()
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
            Foreground = _root.GetType().FullName == "GitUI.CommandsDialogs.FormBlame"
                ? ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")
                : ResolveSourceControlTextArgb(),
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
            Foreground = ResolveSourceControlTextArgb() ?? colors.Foreground,
            Background = background,
            Border = null,
            DisabledBackground = background
        };
    }

    private static string? GetToolTip(Control control)
    {
        string? text = ToolTip.GetTip(control) switch
        {
            TextBlock textBlock => textBlock.Text,
            string value => value,
            object value => value.ToString(),
            null => null
        };
        return string.IsNullOrEmpty(text) ? null : text;
    }

    private static bool IsDetachedMenuItem(Control control)
        => control is MenuItem or Separator && TopLevel.GetTopLevel(control) is null;

    private static bool IsNativeListView(Control control)
        => control is ListBox list && list.Classes.Contains("gitextensions-native-list-items");

    private bool IsInheritedFormProcessContainer(Control control)
        => _root.GetType().FullName == "GitUI.CommandsDialogs.FormPull"
           && control.Name is "MainPanel" or "ControlsPanel";

    private static bool IsNativeTabControl(Control control)
        => control is TabControl tabControl
           && (tabControl.Classes.Contains("gitextensions-native-tabs")
               || tabControl.Classes.Contains("gitextensions-workspace-tabs"));

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

        if (control is FileStatusList)
        {
            return control.IsKeyboardFocusWithin;
        }

        if (IsNativeListView(control)
            || control is ComboBox or ListBox or TreeView or NumericUpDown)
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
