using System.Collections;
using System.Reflection;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using GitExtensions.Extensibility.Git;
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
    private readonly Dictionary<object, object> _fieldOwners = new(ReferenceEqualityComparer.Instance);
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
        string rootType = GetMetadataTypeName(root.GetType());
        _usesDesignerLayoutMetadata = WinFormsInputMetadata.ByType.ContainsKey(rootType)
            || WinFormsInputMetadata.LayoutByType.ContainsKey(rootType)
            || WinFormsInputMetadata.SourceByType.ContainsKey(rootType);
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
        CaptureNode rootNode = ReadControl(
            semanticRoot,
            parentId: string.Empty,
            ordinal: 0,
            ancestorSemanticVisible: true,
            semanticParent: null,
            boundsOverride: rootBoundsOverride,
            primarySurface: role == "primary");
        if (role.StartsWith("popup:", StringComparison.Ordinal)
            && rootNode.Type == "Avalonia.Controls.Primitives.OverlayPopupHost")
        {
            // The overlay retains logical menu items below its rendered viewport. A
            // popup-surface tree describes the visible frame, not those clipped rows.
            rootNode = rootNode with
            {
                Children = rootNode.Children.Where(child => child.ControlKind != "menuItem"
                    || child.BoundsDip.Y + child.BoundsDip.Height <= rootNode.BoundsDip.Height).ToArray()
            };
        }

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
            Root = rootNode
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
            _ when GetSourceTypeName(sourceType) == "DataGridView" => "dataGrid",
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

        if (control is XhtmlTextBlock xhtmlTextBlock)
        {
            return GetXhtmlText(xhtmlTextBlock);
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

        string value = text is null
            ? string.Empty
            : (control is MenuItem or Button or Label || control.Name == "btnRemoteColor")
              && TranslationCompat.GetConvertMnemonics(control)
                ? ToWinFormsMnemonics(text)
                : text;
        return value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
    }

    private static string GetXhtmlText(XhtmlTextBlock textBlock)
    {
        StringBuilder text = new();
        if (textBlock.Inlines is null)
        {
            return textBlock.GetPlainText();
        }

        foreach (object inline in textBlock.Inlines)
        {
            switch (inline)
            {
                case Avalonia.Controls.Documents.Run run:
                    text.Append(run.Text);
                    break;

                case Avalonia.Controls.Documents.LineBreak:
                    text.Append('\n');
                    break;

                case Avalonia.Controls.Documents.InlineUIContainer container
                    when GetPropertyValue(container, "Child") is Border { Tag: "\t" }:
                    text.Append('\t');
                    break;

                case Avalonia.Controls.Documents.InlineUIContainer container
                    when GetPropertyValue(container, "Child") is HyperlinkButton link:
                    string caption = link.Content?.ToString() ?? string.Empty;
                    text.Append(caption);
                    if (link.Tag is string uri && !string.IsNullOrEmpty(uri))
                    {
                        text.Append("|||");
                        text.Append(uri);
                    }

                    break;
            }
        }

        return text.ToString();
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
               ?? GetPropertyValue(selectedItem, "Word") as string
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
            ComboBox comboBox when control.Name is
                "_NO_TRANSLATE_Remotes" or
                "_NO_TRANSLATE_Branches" or
                "RemoteRepositoryCombo" or
                "Url" or
                "comboBoxBranches" => comboBox.SelectedIndex >= 0,
            ComboBox comboBox when control.Name is "cboBranches" or "cboTo" =>
                comboBox.SelectedIndex >= 0
                || comboBox.Items.OfType<IGitRef>().Any(gitRef => gitRef.Name == comboBox.Text),
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
                    if (declaringType == typeof(ResourceManager.GitExtensionsFormBase)
                        && field.Name is "_acceptButton" or "_cancelButton")
                    {
                        // These fields store the framework default/cancel routes. The
                        // original form has no corresponding authored control fields.
                        continue;
                    }

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
                        _fieldOwners.TryAdd(value, owner);
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
        Rect? boundsOverride,
        bool primarySurface)
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
        bool isRemoteLocalSourceFlowLayoutPanel = _root.GetType().FullName == "GitUI.CommandsDialogs.FormRemotes"
            && control.Name == "flowLayoutPanelSsh";
        bool isGitIgnoreLocalSourceFlowLayoutPanel = _root.GetType().FullName == "GitUI.CommandsDialogs.FormGitIgnore"
            && (control.Name is "flowLayoutPanel2" or "panel1");
        bool isLocalSourceFlowLayoutPanel = isRemoteLocalSourceFlowLayoutPanel
            || isGitIgnoreLocalSourceFlowLayoutPanel;
        bool isRemoteColorButton = _root.GetType().FullName == "GitUI.CommandsDialogs.FormRemotes"
            && control.Name == "btnRemoteColor";
        bool isDialogControlsPanel = (_root.GetType().FullName is
            "GitUI.CommandsDialogs.FormCheckoutRevision" or
            "GitUI.CommandsDialogs.FormPull")
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
        string sourceOwnerType = GetSourceOwnerType(control);
        string? fieldName = isSurfaceRoot || isInheritedFormProcessContainer || isLocalSourceFlowLayoutPanel
            ? null
            : fieldNames.FirstOrDefault()
              ?? (control is MenuItem or Separator || string.IsNullOrEmpty(control.Name) ? null : control.Name);
        bool isFormBrowseContainerPanel = _root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse"
            && control.Parent?.Name == "toolPanel"
            && control.Name is ("_contentPanel" or "_leftPanel" or "_rightPanel" or "_topPanel" or "_bottomPanel");
        if (isFormBrowseContainerPanel)
        {
            // ToolStripContainer creates these five real panels internally. They are not
            // fields or named controls in the original, even though AXAML names them so
            // the portable layout can address the same panel identities.
            fieldName = null;
        }

        if (_root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse"
            && (control.Name?.StartsWith("pull_shortcut_", StringComparison.Ordinal) == true
                || control.Name is "OutputHistoryTab" or "OutputHistoryControl"))
        {
            // These controls are runtime/local variables in the source rather than fields.
            fieldName = null;
        }

        bool isSourceLocalTableLayout = control.GetType().FullName == "GitUI.Compat.WinFormsControls.TableLayoutPanel"
            && fieldNames.Count == 0
            && control.Parent?.GetType().FullName is
                "GitUI.UserControls.CommitPickerSmallControl" or
                "GitUI.CommitInfo.CommitInfoHeader";
        if (isSourceLocalTableLayout)
        {
            // The WinForms Designer declares this local layout variable without a control
            // field. Keep it anonymous in the shared structural tree.
            fieldName = null;
        }

        bool isKnownSourceLocalControl = IsKnownSourceLocalControl(sourceOwnerType, control.Name);
        if (isKnownSourceLocalControl)
        {
            // These named controls are local variables in the WinForms Designer. Their names
            // remain useful for stable tree identity, but they are not source fields.
            fieldName = null;
        }

        bool isCommitPickerLocalLayout = fieldName is null
            && control.Name == "tableLayoutPanel1"
            && control.Parent?.GetType().FullName == "GitUI.UserControls.CommitPickerSmallControl";
        bool isCommitInfoHeaderLocalLayout = isSourceLocalTableLayout
            && control.Parent?.GetType().FullName == "GitUI.CommitInfo.CommitInfoHeader";
        bool isRuntimeOutputHistoryTab = control.Name == "OutputHistoryTab"
            && control.GetLogicalAncestors().Any(ancestor => ancestor.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse");
        bool isRuntimeOutputHistoryControl = control.Name == "OutputHistoryControl"
            && control.GetLogicalAncestors().OfType<Control>().Any(ancestor => ancestor.Name == "OutputHistoryTab");
        string? sourceType = GetSourceType(control, fieldName);
        sourceType ??= isFormBrowseContainerPanel
            ? control.Name == "_contentPanel"
                ? "System.Windows.Forms.ToolStripContentPanel"
                : "System.Windows.Forms.ToolStripPanel"
            : null;
        sourceType ??= _root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse"
            && control.Name?.StartsWith("pull_shortcut_", StringComparison.Ordinal) == true
                ? "System.Windows.Forms.ToolStripButton"
                : null;
        sourceType ??= isCommitInfoHeaderLocalLayout ? "System.Windows.Forms.TableLayoutPanel" : null;
        sourceType ??= isKnownSourceLocalControl ? GetKnownSourceLocalType(sourceOwnerType, control.Name) : null;
        isSourceDataGrid |= GetSourceTypeName(sourceType) == "DataGridView";
        bool hasWinFormsTextBoxClientInset = control is TextBox
            && !isSpellCheckTextBox
            && !IsSourceRichTextControl(control)
            && GetSourceTypeName(sourceType) is not "RichTextBox";
        // The source OutputHistory RichTextBox has Fixed3D chrome: its client excludes
        // two pixels on every edge, matching the editor's actual border thickness.
        Thickness outputHistoryRichTextBorder = control is ThemeAwareTextEditor editor
            && sourceOwnerType == "GitUI.UserControls.OutputHistoryControl"
            && control.Name == "TextBox"
                ? editor.BorderThickness
                : default;
        bool hasSourceVerticalTextScrollBar = control is TextBox
            && ((_root.GetType().FullName == "GitUI.CommandsDialogs.FormAddToGitIgnore"
                    && control.Name == "FilePattern")
                || (_root.GetType().FullName == "GitUI.CommandsDialogs.FormGitIgnore"
                    && control.Name == "label1")
                || (_root.GetType().FullName == "GitUI.CommandsDialogs.FormArchive"
                    && control.Name == "textBoxPaths"));
        bool isShellPreviewPanel = _root.GetType().FullName == "GitUI.CommandsDialogs.SettingsDialog.Pages.ShellExtensionSettingsPage"
            && control is Border
            && string.IsNullOrEmpty(control.Name)
            && control.GetLogicalDescendants().OfType<Control>().Any(descendant => descendant.Name == "labelPreview");
        sourceType ??= isShellPreviewPanel ? "Panel" : null;
        string rootMetadataType = GetMetadataTypeName(_root.GetType());
        bool isStandaloneSourceComboBox = isSurfaceRoot
            && rootMetadataType is "GitUI.UserControls.CaseSensitiveComboBox" or "GitUI.UserControls.WatermarkComboBox";
        bool isSearchWindowControl = rootMetadataType == "GitUI.CommandsDialogs.SearchWindow"
            && fieldName == "_searchControl";
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
        // WinForms ToolStripItem.Selected is the hovered item, and the source reader
        // reports that same value for both Selected and Focused.
        bool isBrowseHoveredToolStripItem = isFormBrowseSurface && isToolStripItem
            && control.IsPointerOver && (control is not MenuItem menu || !menu.IsSubMenuOpen);
        // The selected commit's outer content viewport ends at its live vertical scrollbar;
        // the inner RichTextBox twin retains its own full client width.
        double browseRevisionInfoScrollBarWidth = isFormBrowseSurface
            && control.GetType().FullName == "GitUI.CommitInfo.CommitInfo"
            && control.Name == "RevisionInfo"
            ? control.GetVisualDescendants().OfType<ScrollBar>()
                .Where(scrollBar => scrollBar.Orientation == Avalonia.Layout.Orientation.Vertical
                                    && scrollBar.IsVisible)
                .Select(scrollBar => scrollBar.Bounds.Width)
                .FirstOrDefault()
            : 0;
        bool isFormCommitSurface = _root.GetType().FullName == "GitUI.CommandsDialogs.FormCommit";
        bool isFormSettingsSurface = _root.GetType().FullName == "GitUI.CommandsDialogs.FormSettings";
        bool isControlBackgroundDialog = _root.GetType().FullName is
            "GitUI.CommandsDialogs.FormBlame" or
            "GitUI.CommandsDialogs.FormCompareToBranch" or
            "GitUI.CommandsDialogs.FormDiff" or
            "GitUI.CommandsDialogs.FormFormatPatch" or
            "GitUI.CommandsDialogs.FormGitAttributes" or
            "GitUI.CommandsDialogs.FormGitIgnore" or
            "GitUI.CommandsDialogs.FormMailMap" or
            "GitUI.CommandsDialogs.FormLog";
        string? semanticName = fieldName ?? control.Name;
        if (semanticName is null
            && rootMetadataType == "GitUI.CommandsDialogs.CommitDialog.FormCommitTemplateSettings"
            && control.Classes.Contains("commit-template-main-panel"))
        {
            semanticName = "MainPanel";
        }

        bool isChecklistStatusButton = isFormSettingsSurface
            && sourceOwnerType == "GitUI.CommandsDialogs.SettingsDialog.Pages.ChecklistSettingsPage"
            && IsChecklistStatusButton(semanticName);
        bool isSettingsPageHeader = isFormSettingsSurface
            && control.GetType().FullName == "GitUI.CommandsDialogs.SettingsDialog.SettingsPageHeader";
        bool isSettingsRootTable = isFormSettingsSurface && semanticName == "tableLayoutPanel3";
        bool isSettingsHeaderTable = isFormSettingsSurface
            && sourceOwnerType == "GitUI.CommandsDialogs.SettingsDialog.SettingsPageHeader"
            && semanticName == "tableLayoutPanel2";
        bool isChecklistGroup = isFormSettingsSurface
            && sourceOwnerType == "GitUI.CommandsDialogs.SettingsDialog.Pages.ChecklistSettingsPage"
            && control.Name == "groupBox1";
        bool isEnvironmentInfoSurface = rootMetadataType == "GitUI.CommandsDialogs.EnvironmentInfo";
        bool isEnvironmentInfoLayout = sourceOwnerType == "GitUI.CommandsDialogs.EnvironmentInfo"
            && control.Name == "tableLayoutPanel1";
        bool isEnvironmentInfoTopSeparator = sourceOwnerType == "GitUI.CommandsDialogs.EnvironmentInfo"
            && control.Name == "lblSeparatorTop";
        bool isEnvironmentInfoBottomSeparator = sourceOwnerType == "GitUI.CommandsDialogs.EnvironmentInfo"
            && control.Name == "lblSeparatorBottom";
        bool isEnvironmentInfoSeparator = isEnvironmentInfoTopSeparator || isEnvironmentInfoBottomSeparator;
        bool hasSourceRichTextOuterExtent = isFormBrowseSurface
            && sourceOwnerType == "GitUI.CommitInfo.CommitInfo"
            && semanticName == "rtbxCommitMessage";
        bool isFormBrowseSearchTextBox = isFormBrowseSurface
            && semanticName == "txtSearchBox";
        bool usesFormBrowseFilterToolbar = isFormBrowseSurface
            || rootMetadataType == "GitUI.CommandsDialogs.FormFileHistory";
        string? formBrowseSourceText = usesFormBrowseFilterToolbar
            ? semanticName switch
            {
                "ToolStripMain" => "Standard",
                "ToolStripFilters" => rootMetadataType == "GitUI.CommandsDialogs.FormFileHistory" ? string.Empty : "Filters",
                "ToolStripScripts" => "Scripts",
                "EditSettings" or "toolStripButtonLevelUp" or "toolStripButtonPush"
                    or "toolStripWorktrees" or "tsbtnAdvancedFilter" => string.Empty,
                "tssbtnShowBranches" => "&All branches",
                _ => null,
            }
            : null;
        string? formBrowseSourceToolTip = usesFormBrowseFilterToolbar
            ? semanticName switch
            {
                "toolStripButtonLevelUp" => string.Empty,
                "tsbtnAdvancedFilter" when rootMetadataType != "GitUI.CommandsDialogs.FormFileHistory" => string.Empty,
                "tsbShowReflog" or "tsmiShowOnlyFirstParent"
                    when rootMetadataType == "GitUI.CommandsDialogs.FormFileHistory" => string.Empty,
                "tsddbtnBranchFilter" => "Branch type",
                "tsddbtnRevisionFilter" => "Filter type",
                _ => null,
            }
            : null;
        bool isFormCommitStatusItem = isFormCommitSurface && IsFormCommitStatusItem(semanticName);
        bool isFormCommitToolStripPanel = isFormCommitSurface && IsFormCommitToolStripPanel(semanticName);
        bool isFormBrowseToolStripPanel = sourceOwnerType == "GitUI.CommandsDialogs.FormBrowse"
            && semanticName is "_topPanel" or "_bottomPanel" or "_leftPanel" or "_rightPanel";
        bool isFormCommitOptionsItem = isFormCommitSurface && IsFormCommitOptionsItem(semanticName);
        bool isFormCommitOptionsPopup = isFormCommitSurface
            && isPopupRoot
            && control.GetVisualDescendants().OfType<Control>().Any(
                descendant => IsFormCommitOptionsControl(descendant.Name));
        bool isFormCommitOptionsInput = isFormCommitSurface
            && semanticName is "toolAuthor" or "gpgSignCommitToolStripComboBox" or "toolStripGpgKeyTextBox";
        CaptureColors? formBrowseSemanticColors = ReadFormBrowseSemanticColors(
            control,
            sourceOwnerType,
            fieldName);
        CaptureColors? formCommitSemanticColors = ReadFormCommitSemanticColors(
            control,
            fieldName);
        CaptureColors? blameLogSemanticColors = ReadBlameLogSemanticColors(control, fieldName);
        CaptureColors? navigationEditorSemanticColors = ReadNavigationEditorSemanticColors(
            control,
            sourceOwnerType,
            fieldName,
            isSurfaceRoot);
        bool isDesignerMetadataControl = fieldName is not null && IsDesignerMetadataControl(control);
        Control? childSemanticParent = isSurfaceRoot || fieldName is not null || isFormBrowseContainerPanel || isInheritedFormProcessContainer
                                                || isLocalSourceFlowLayoutPanel || isShellPreviewPanel || isSearchWindowControl
                                                || isEnvironmentInfoLayout || isKnownSourceLocalControl || isNativeTabPage
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
        bool hasSourceFixedSingleClientInset = designerLayout?.BorderStyle == "FixedSingle";
        bool hasNativeListComposite = TryGetNativeListComposite(control, out Grid? nativeListComposite, out _);
        // FormCommit's Avalonia action grid has no SplitContainer.Panel2 owner. Project its
        // source-authored root-client button bounds so the shared tree keeps the original
        // flattened WinForms coordinate space while rendered pixels remain untouched.
        bool isPatchGridDataGrid = isSourceDataGrid
            && control.Name == "Patches"
            && control.GetLogicalAncestors().OfType<PatchGrid>().FirstOrDefault() is { };
        Rect bounds = isPatchGridDataGrid
            ? GetSemanticBounds(control.GetLogicalAncestors().OfType<PatchGrid>().First(), semanticParent)
            : isFormCommitSurface && semanticName == "Ok"
            ? new Rect(334, 10, 75, 23)
            : isFormCommitSurface && semanticName == "Cancel"
                ? new Rect(134, 441, 129, 23)
            : isFormCommitSurface && semanticName == "commitEndPadding"
                ? new Rect(920, 23, boundsOverride?.Width ?? semanticStateControl.Bounds.Width, boundsOverride?.Height ?? semanticStateControl.Bounds.Height)
            : boundsOverride
                ?? (hasNativeListComposite
                ? GetSemanticBounds(nativeListComposite!, semanticParent)
                : GetSemanticBounds(semanticStateControl, semanticParent));
        if (isFormCommitOptionsPopup)
        {
            bounds = new Rect(bounds.Position, new Size(315, 222));
        }

        if (isFormBrowseSurface
            && sourceOwnerType == "GitUI.CommandsDialogs.FormBrowse"
            && semanticName == "RevisionInfo"
            && semanticParent is TabItem
            && semanticParent.GetLogicalAncestors().OfType<TabControl>().FirstOrDefault() is { } revisionInfoTabs)
        {
            // The source child fills the complete native TabPage client. Avalonia's direct
            // semantic parent is the clickable header, not its content presenter.
            bounds = new Rect(
                0,
                0,
                Math.Max(0, revisionInfoTabs.Bounds.Width - 2),
                Math.Max(0, revisionInfoTabs.Bounds.Height - 30));
        }

        if (hasSourceRichTextOuterExtent)
        {
            // A borderless WinForms RichTextBox retains one outer control pixel beyond the
            // native contents-height notification used to size its parent row.
            bounds = new Rect(bounds.Position, new Size(bounds.Width, bounds.Height + 1));
        }

        if (rootMetadataType == "GitUI.UserControls.InteractiveGitActionControl"
            && semanticName == "TextLabel")
        {
            // The source Dock=Fill label extends behind the right-docked button panel.
            bounds = new Rect(28, 0, _root.Bounds.Width - 28, _root.Bounds.Height);
        }

        if (rootMetadataType == "GitUI.UserControls.InteractiveGitActionControl"
            && semanticName == "ButtonContainer")
        {
            // FlowLayoutPanel includes its right padding in AutoSize and remains docked to the full source height.
            bounds = new Rect(_root.Bounds.Width - control.Bounds.Width - 2, 0, control.Bounds.Width + 2, _root.Bounds.Height);
        }

        if (isFormBrowseToolStripPanel && semanticName == "_topPanel")
        {
            bounds = new Rect(0, 0, (control.Parent as Control)?.Bounds.Width ?? bounds.Width, 27);
        }

        if (rootMetadataType is "GitUI.CommandsDialogs.FormGitAttributes" or "GitUI.CommandsDialogs.FormMailMap"
            && semanticName == "Save")
        {
            // WinForms resolves right anchoring to integral device coordinates at 96 DPI.
            bounds = new Rect(Math.Floor(bounds.X), bounds.Y, bounds.Width, bounds.Height);
        }

        if (isSettingsHeaderTable)
        {
            bounds = new Rect(bounds.Position, new Size(bounds.Width, 51));
        }

        bool semanticVisible = IsSemanticallyVisible(control, semanticStateControl)
            && (!isSourceToolStripItem || IsInsideClippedAncestors(control))
            && ancestorSemanticVisible;
        bool childSemanticVisible = semanticVisible
            // These two dynamic Browse menus are constructed without fields on the form;
            // the source primary tree keeps their rows hidden while the popup tree is open.
            && (control is not MenuItem menuItem
                || (menuItem.IsSubMenuOpen
                    && !(primarySurface && IsFormBrowseDynamicMainMenu(control))))
            && (control is not TabItem tabItem || tabItem.IsSelected);
        IReadOnlyList<CaptureNode> children = GetSemanticChildren(control)
            .Select((child, childOrdinal) => ReadControl(
                child,
                id,
                childOrdinal,
                childSemanticVisible,
                childSemanticParent,
                boundsOverride: null,
                primarySurface: primarySurface))
            .ToArray();

        CaptureNode node = new()
        {
            Id = id,
            FieldName = fieldName,
            FieldAliases = fieldNames.Skip(1).ToArray(),
            Name = isFormBrowseContainerPanel || string.IsNullOrEmpty(control.Name) ? null : control.Name,
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
                : sourceType ?? control.GetType().FullName ?? control.GetType().Name,
            ControlKind = isRemoteColorButton
                ? "button"
                : isFormCommitStatusItem
                    ? "menuItem"
                : isRepositoryHostDiscussion || isDesignerLinkLabel
                    ? "control"
                : isSourceList && !IsSourceTreeControl(sourceType) && !IsNativeListView(control)
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
                Width = ToPixel(isEnvironmentInfoSeparator
                    ? Math.Max(0, bounds.Width - 2)
                    : browseRevisionInfoScrollBarWidth > 0
                    ? Math.Max(0, bounds.Width - browseRevisionInfoScrollBarWidth)
                    : isSourceList
                    ? designerLayout?.BorderStyle == "None" ? bounds.Width : GetSourceListClientWidth(control, bounds, sourceType, designerLayout?.BorderStyle)
                    : isFormCommitOptionsInput
                        ? bounds.Width
                    : hasSourceFixedSingleClientInset
                        ? Math.Max(0, bounds.Width - 2)
                    : isSpellCheckAutoComplete
                        ? Math.Max(0, bounds.Width - 2)
                    : isFormBrowseSearchTextBox
                        ? bounds.Width
                    : outputHistoryRichTextBorder != default
                        ? Math.Max(0, bounds.Width - outputHistoryRichTextBorder.Left - outputHistoryRichTextBorder.Right)
                    : hasWinFormsTextBoxClientInset
                        ? GetSourceTextBoxClientWidth(bounds, designerLayout?.BorderStyle, hasSourceVerticalTextScrollBar)
                    : isNativeListView ? Math.Max(0, bounds.Width - 4) : bounds.Width),
                Height = ToPixel(isEnvironmentInfoSeparator
                    ? Math.Max(0, bounds.Height - 2)
                    : control.Name == "treeMain"
                    ? Math.Max(0, bounds.Height - 17)
                    : isSourceList
                        ? designerLayout?.BorderStyle == "None" ? bounds.Height : Math.Max(0, bounds.Height - (hasSourceFixedSingleClientInset ? 2 : 4))
                    : isFormCommitOptionsInput
                        ? bounds.Height
                    : hasSourceFixedSingleClientInset
                        ? Math.Max(0, bounds.Height - 2)
                    : isSpellCheckAutoComplete
                        ? Math.Max(0, bounds.Height - 2)
                        : isFormBrowseSearchTextBox
                            ? bounds.Height
                        : outputHistoryRichTextBorder != default
                            ? Math.Max(0, bounds.Height - outputHistoryRichTextBorder.Top - outputHistoryRichTextBorder.Bottom)
                        : hasWinFormsTextBoxClientInset
                            ? GetSourceTextBoxClientHeight(bounds, designerLayout?.BorderStyle)
                        : isNativeListView ? Math.Max(0, bounds.Height - 4) : bounds.Height)
            },
            ClientSizeDip = new CaptureSizeF
            {
                Width = ToDecimal(isEnvironmentInfoSeparator
                    ? Math.Max(0, bounds.Width - 2)
                    : browseRevisionInfoScrollBarWidth > 0
                    ? Math.Max(0, bounds.Width - browseRevisionInfoScrollBarWidth)
                    : isSourceList
                    ? designerLayout?.BorderStyle == "None" ? bounds.Width : GetSourceListClientWidth(control, bounds, sourceType, designerLayout?.BorderStyle)
                    : isFormCommitOptionsInput
                        ? bounds.Width
                    : hasSourceFixedSingleClientInset
                        ? Math.Max(0, bounds.Width - 2)
                    : isSpellCheckAutoComplete
                        ? Math.Max(0, bounds.Width - 2)
                    : isFormBrowseSearchTextBox
                        ? bounds.Width
                    : outputHistoryRichTextBorder != default
                        ? Math.Max(0, bounds.Width - outputHistoryRichTextBorder.Left - outputHistoryRichTextBorder.Right)
                    : hasWinFormsTextBoxClientInset
                        ? GetSourceTextBoxClientWidth(bounds, designerLayout?.BorderStyle, hasSourceVerticalTextScrollBar)
                    : isNativeListView ? Math.Max(0, bounds.Width - 4) : bounds.Width),
                Height = ToDecimal(isEnvironmentInfoSeparator
                    ? Math.Max(0, bounds.Height - 2)
                    : control.Name == "treeMain"
                    ? Math.Max(0, bounds.Height - 17)
                    : isSourceList
                        ? designerLayout?.BorderStyle == "None" ? bounds.Height : Math.Max(0, bounds.Height - (hasSourceFixedSingleClientInset ? 2 : 4))
                    : isFormCommitOptionsInput
                        ? bounds.Height
                    : hasSourceFixedSingleClientInset
                        ? Math.Max(0, bounds.Height - 2)
                    : isSpellCheckAutoComplete
                        ? Math.Max(0, bounds.Height - 2)
                        : isFormBrowseSearchTextBox
                            ? bounds.Height
                        : outputHistoryRichTextBorder != default
                            ? Math.Max(0, bounds.Height - outputHistoryRichTextBorder.Top - outputHistoryRichTextBorder.Bottom)
                        : hasWinFormsTextBoxClientInset
                            ? GetSourceTextBoxClientHeight(bounds, designerLayout?.BorderStyle)
                        : isNativeListView ? Math.Max(0, bounds.Height - 4) : bounds.Height)
            },
            ItemHeightDip = isPatchGridDataGrid
                ? 25
                : isRevisionGridView
                ? ReadRevisionGridItemHeight(control)
                : isComboBoxPopup ? 15 : null,
            Padding = ReadThicknessPair(isComboBoxPopup || isComboBoxPopupItem || isStandaloneSourceComboBox
                ? default(Thickness)
                : isSettingsRootTable ? new Thickness(8)
                : isSettingsPageHeader ? new Thickness(0, 4, 0, 0)
                : isChecklistGroup ? new Thickness(12)
                : isFileViewerInternal ? default(Thickness)
                : isFormCommitToolStripPanel ? default(Thickness)
                : isFormBrowseToolStripPanel && semanticName == "_topPanel" ? new Thickness(4, 0)
                : isFormBrowseToolStripPanel ? default(Thickness)
                : isFormBrowseContainerPanel && semanticName == "_contentPanel" ? new Thickness(6)
                : isFormCommitSurface && semanticName is "Ok" or "Cancel" ? default(Thickness)
                : isInheritedFormProcessContainer
                    ? GetInheritedFormProcessPadding(control)
                : isGitIgnoreLocalSourceFlowLayoutPanel && control.Name == "panel1"
                    ? new Thickness(0, 0, 8, 4)
                : designerLayout?.Padding
                ?? (isPopupRoot ? new Thickness(33, 2, 1, 2) : (Thickness?)null)
                ?? (isSemanticToolStrip ? isFormBrowseSourceToolStrip ? default(Thickness) : new Thickness(0, 0, 1, 0) : (Thickness?)null)
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
                : isSettingsRootTable ? new Thickness(3)
                : isSettingsPageHeader || isChecklistGroup ? default(Thickness)
                : isSettingsHeaderTable ? new Thickness(3, 4)
                : isSurfaceRoot && isEnvironmentInfoSurface ? default(Thickness)
                : isFormCommitToolStripPanel ? semanticName == "_contentPanel" ? new Thickness(3) : default
                : isFormBrowseToolStripPanel ? default(Thickness)
                : isFormCommitSurface && IsFormCommitStandardStatusItem(semanticName) ? new Thickness(0, 3, 0, 2)
                : isFormCommitSurface && semanticName == "toolStripProgressBar1" ? new Thickness(1, 3, 1, 3)
                : isFormCommitSurface && semanticName == "_waitSpinner" ? new Thickness(3)
                : isFormCommitSurface && semanticName == "_currentFilesList" ? new Thickness(3, 4)
                : isFormCommitSurface && semanticName is "Ok" or "Cancel" ? new Thickness(3)
                : isFormCommitSurface && semanticName is "toolAuthor" or "toolStripGpgKeyTextBox" ? new Thickness(1)
                : isFormCommitSurface && semanticName == "gpgSignCommitToolStripComboBox" ? new Thickness(2)
                : isFormBrowseSurface && semanticName is "_contentPanel" or "_txtBranchCriterion"
                    ? default(Thickness)
                : isInheritedFormProcessContainer || isSearchWindowControl
                    ? default(Thickness)
                : isGitIgnoreLocalSourceFlowLayoutPanel
                    ? new Thickness(3)
                : isCommitPickerLocalLayout
                    ? new Thickness(2)
                : isSurfaceRoot && !isPopupRoot
                    ? rootMetadataType == "GitUI.UserControls.Settings.SettingsCheckBox"
                        ? new Thickness(4, 3)
                    : rootMetadataType == "GitUI.CommandsDialogs.SearchControl"
                        ? default(Thickness)
                    : _root.GetType().FullName == "GitUI.CommandsDialogs.FormCommit"
                        ? new Thickness(2)
                        : new Thickness(3)
                : designerLayout?.Margin
                ?? (isSpellCheckAutoComplete ? new Thickness(3) : (Thickness?)null)
                ?? (isSemanticToolStrip || isFileStatusListView ? default(Thickness) : (Thickness?)null)
                ?? (isFormBrowseMenuStrip ? default(Thickness) : (Thickness?)null)
                ?? (isSemanticToolStripItem
                    ? control is Separator || GetSourceTypeName(sourceType) == "ToolStripSeparator"
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
            Font = (rootMetadataType == "GitUI.UserControls.WaitSpinner"
                ? ReadUiFont()
                : isMenuCaption
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
                : isKnownSourceLocalControl
                    ? ReadFont(_root)
                : ReadFont(isLocalSourceFlowLayoutPanel
                           || isCommitPickerLocalLayout
                           || isCommitInfoHeaderLocalLayout
                           || isRuntimeOutputHistoryControl
                           || IsDetachedMenuItem(control)
                           || isFormCommitOptionsPopup
                           || IsFormCommitOptionsControl(semanticName)
                           || isFormBrowseContainerPanel
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
                : isStandaloneSourceComboBox
                    ? ReadStandaloneSourceInputColors(control)
                : isCommitInfoHeaderLocalLayout
                    ? ReadSourceBackgroundColors(
                        control,
                        "GitExtensionsWindowBackgroundBrush",
                        ResolveSourceControlTextArgb())
                : isRuntimeOutputHistoryTab || isRuntimeOutputHistoryControl
                    ? ReadSourceBackgroundColors(
                        control,
                        "GitExtensionsControlBackgroundBrush",
                        ResolveSourceControlTextArgb())
                : isFormCommitOptionsPopup
                    ? ReadSourceBackgroundColors(
                        control,
                        "GitExtensionsControlBackgroundBrush",
                        ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush"))
                : isPopupRoot && _root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse"
                    ? ReadColors(control) with
                    {
                        Foreground = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")
                                     ?? ResolveResourceArgb("GitExtensionsMenuForegroundBrush")
                    }
                : isPopupRoot && _root is RevisionGridControl
                    ? ReadSourceBackgroundColors(
                        control,
                        "GitExtensionsKnownColorControlBrush",
                        ResolveSourceControlTextArgb())
                : formCommitSemanticColors is not null
                    ? formCommitSemanticColors
                : formBrowseSemanticColors is not null
                    ? formBrowseSemanticColors
                : isFileStatusSplitter && IsViewPullRequestsTree(control)
                    ? ReadTransparentContainerColors(control)
                : isFileStatusSplitter
                    ? ReadSourceBackgroundColors(
                        control,
                        "GitExtensionsFileStatusSplitterBrush",
                        ResolveSourceControlTextArgb())
                : blameLogSemanticColors is not null
                    ? blameLogSemanticColors
                : navigationEditorSemanticColors is not null
                    ? navigationEditorSemanticColors
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
                            || (isControlBackgroundDialog && isFileStatusToolbar)
                            || (isFileStatusToolbar && IsViewPullRequestsTree(control)),
                        windowBackground: isFileStatusToolbar && !isControlBackgroundDialog,
                        useWindowText: isFormBrowseSourceToolStrip,
                        useControlText: isControlBackgroundDialog || IsViewPullRequestsTree(control))
                    : isSemanticToolStripItem
                        ? ReadToolStripColors(
                            control,
                            isItem: true,
                            transparentBackground: IsTransparentToolStripItem(control),
                            windowBackground: IsWindowBackgroundToolStripItem(control),
                            useWindowText: _root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse"
                                && ((control.Name == "toolStripSplitStash"
                                     && IsInsideClippedAncestors(control))
                                    || (control.Name is "btnCollapseGroups" or "btnRefresh"
                                        && IsSelectedBrowseTreeToolbarItem(control))),
                            useControlText: IsSourceControlTextToolStripItem(control)
                                || IsViewPullRequestsTree(control)
                                || (_root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse"
                                    && control.Name == "toolStripSplitStash"
                                    && !IsInsideClippedAncestors(control)))
                        : isFileStatusListView
                            ? ReadFileStatusListViewColors(semanticStateControl)
                        : isRevisionGridView
                                ? ReadColors(semanticStateControl)
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
                                                : isInheritedFormProcessContainer
                                                    ? ReadInheritedFormProcessColors(control)
                                                : hasSourceTransparentColors
                                                    ? ReadTransparentContainerColors(control)
                                                : hasSourceLightTransparentColors && !IsDarkTheme()
                                                    ? ReadLightTransparentColors(control)
                                                : isSettingsPageHeader || isChecklistGroup || isKnownSourceLocalControl
                                                    ? ReadSourceDesignerColors(control)
                                                : isChecklistStatusButton
                                                    ? ReadChecklistStatusButtonColors(control)
                                                : isDesignerMetadataControl && control is Button or CheckBox or RadioButton
                                                    ? ReadSourceDesignerButtonColors(control)
                                                : isDesignerMetadataControl && GetSourceTypeName(sourceType) == "RichTextBox"
                                                    ? ReadSourceRichTextColors()
                                                : isDesignerMetadataControl && control is TextBox or ComboBox or NumericUpDown
                                                    ? ReadSourceInputColors(control)
                                                : isDesignerMetadataControl && isSourceList
                                                    ? ReadSourceListColors()
                                                    : isFileViewerTextEditor
                                                        ? ReadFileViewerTextEditorColors()
                                                    : isFileViewerPictureBox
                                                        ? ReadFileViewerPictureBoxColors(control)
                                                    : isDesignerMetadataControl && IsSourceAmbientControl(sourceType)
                                                        ? ReadSourceDesignerColors(control, designerLayout)
                                                    : IsSourceControlTextControl(control, fieldName, sourceType)
                                                        ? ReadSourceControlTextColors(control)
                                                    : isDesignerMetadataControl && control.Name == "lblHeaderLine2"
                                                                ? ReadSourceDesignerColors(control) with { Border = null }
                                                            : ReadColors(semanticStateControl),
            BorderStyle = isFormBrowseToolStripContainer
                ? null
                : isFormBrowseContainerPanel && semanticName == "_contentPanel"
                    ? "None"
                : isEnvironmentInfoSeparator
                    ? "Fixed3D"
                : isEnvironmentInfoLayout
                    ? "None"
                : isSettingsPageHeader || isChecklistGroup
                    ? "None"
                : isShellPreviewPanel
                    ? "None"
                : isLocalSourceFlowLayoutPanel
                    ? "None"
                : isCommitPickerLocalLayout
                    ? "None"
                : isCommitInfoHeaderLocalLayout || isRuntimeOutputHistoryControl
                    ? "None"
                : isSurfaceRoot && !isPopupRoot && _usesDesignerLayoutMetadata && control is not Window
                    ? "None"
                : (isFormCommitToolStripPanel && semanticName != "_contentPanel") || isFormBrowseToolStripPanel
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
                ?? (control is BranchComboBox ? "None" : null)
                ?? (control is PatchGrid ? "None" : null)
                ?? (isSourceDataGrid ? "FixedSingle" : null)
                ?? (isDesignerMetadataControl
                    ? GetDefaultDesignerBorderStyle(control, sourceType)
                    : null)
                ?? (IsSourceBorderlessControl(control, fieldName) ? "None" : null)
                ?? (control is FileStatusList || isRevisionGrid || isRevisionGridView || isNativeTabPage
                    ? "None"
                    : isNativeListView
                        ? "Fixed3D"
                        : GetPropertyValue(control, "BorderStyle")?.ToString()),
            FlatStyle = isComboBoxPopup
                ? "Standard"
                : isEnvironmentInfoSurface && semanticName == "copyButton"
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
                || isFormBrowseMenuStrip
                || isSemanticToolStrip || isSemanticToolStripItem || isFileViewerTextEditor
                || isSpellCheckAutoComplete || isSpellCheckTextBox || isSourceLabelSubstitute || isWatermarkComboBox
                || isStandaloneSourceComboBox
                || isRepositoryHostDiscussion
                ? null
                : ReadBorderWidth(control),
            CornerRadiusDip = isSourceLabelSubstitute || isComboBoxPopupItem || isDesignerMetadataControl
                ? null
                : ReadCornerRadius(control),
            Anchor = isComboBoxPopup || isComboBoxPopupItem ? []
                : isShellPreviewPanel || isLocalSourceFlowLayoutPanel || isCommitPickerLocalLayout
                    || isCommitInfoHeaderLocalLayout || isRuntimeOutputHistoryControl ? ["Top", "Left"] : designerLayout?.Anchor
                ?? (isSurfaceRoot && !isPopupRoot ? new[] { "Top", "Left" } : null)
                ?? (isFormBrowseToolStripPanel ? new[] { "Top", "Left" } : null)
                ?? (isFormCommitSurface && (isFormCommitToolStripPanel || semanticName is "_waitSpinner" or "_currentFilesList" or "Ok" or "Cancel")
                    ? new[] { "Top", "Left" }
                    : null)
                ?? (isFormBrowseContainerPanel && semanticName == "_contentPanel" ? new[] { "Top", "Left" } : null)
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
            Dock = isComboBoxPopup || isComboBoxPopupItem ? null
                : isRemoteLocalSourceFlowLayoutPanel || isShellPreviewPanel ? "Fill"
                : isGitIgnoreLocalSourceFlowLayoutPanel ? "Bottom"
                : isSearchWindowControl ? "Fill"
                : isCommitPickerLocalLayout ? "Fill"
                : isCommitInfoHeaderLocalLayout || isRuntimeOutputHistoryControl ? "Fill"
                : isFormSettingsSurface && semanticName is "tableLayoutPanel3" or "panelCurrentSettingsPage" or "settingsTreeView" or "tableLayoutPanel2" or "_page"
                    ? "Fill"
                : isChecklistGroup
                    ? "Top"
                : isSettingsPageHeader
                    ? "Fill"
                : isEnvironmentInfoLayout
                    ? "Fill"
                : isEnvironmentInfoTopSeparator
                    ? "Top"
                : isEnvironmentInfoBottomSeparator
                    ? "Bottom"
                : designerLayout?.Dock
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
                ?? (isFormBrowseToolStripPanel
                    ? semanticName switch
                    {
                        "_topPanel" => "Top",
                        "_bottomPanel" => "Bottom",
                        "_leftPanel" => "Left",
                        "_rightPanel" => "Right",
                        _ => null,
                    }
                    : null)
                ?? (isFormBrowseContainerPanel && semanticName == "_contentPanel" ? "Fill" : null)
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
            AutoSize = isComboBoxPopupItem ? false
                : isSettingsHeaderTable || isChecklistGroup ? true
                : isSettingsPageHeader ? false
                : isEnvironmentInfoLayout ? true
                : isEnvironmentInfoSeparator ? false
                : isLocalSourceFlowLayoutPanel || isShellPreviewPanel || isSearchWindowControl || isCommitPickerLocalLayout ? true
                : isCommitInfoHeaderLocalLayout ? true
                : isRuntimeOutputHistoryControl ? false
                : isFormBrowseMenuStrip && semanticName == "mainMenuStrip" ? true
                : designerLayout?.AutoSize
                ?? (isSurfaceRoot && !isPopupRoot
                    ? WinFormsInputMetadata.AutoSizeRootTypes.Contains(GetMetadataTypeName(_root.GetType()))
                    : (bool?)null)
                ?? (isFormCommitSurface
                    ? semanticName switch
                    {
                        "_topPanel" or "_bottomPanel" or "_leftPanel" or "_rightPanel" => true,
                        "_contentPanel" or "_waitSpinner" or "_currentFilesList" or "Ok" or "Cancel" => false,
                        _ => (bool?)null,
                    }
                    : (bool?)null)
                ?? (isFormBrowseToolStripPanel ? true : (bool?)null)
                ?? (isFormBrowseContainerPanel && semanticName == "_contentPanel" ? false : (bool?)null)
                ?? (fieldName == "_contentPanel" ? false : (bool?)null)
                ?? (fieldName == "_txtBranchCriterion" ? true : (bool?)null)
                ?? (isSpellCheckEditor ? false : (bool?)null)
                ?? (isSpellCheckAutoComplete ? false : (bool?)null)
                ?? (isSourcePictureBox ? false : (bool?)null)
                ?? (isInheritedFormProcessContainer
                    ? _root.GetType().FullName == "GitUI.CommandsDialogs.FormCherryPick"
                        || control.Name == "ControlsPanel"
                    : (bool?)null)
                ?? (isSemanticToolStrip || isToolStripItem ? true : (bool?)null)
                ?? (isFileStatusListView || isFileStatusSplitter || isSpellCheckTextBox ? false : (bool?)null)
                ?? (isDesignerMetadataControl
                    ? GetDefaultDesignerAutoSize(control, sourceType)
                    : isRevisionGrid || isRevisionGridView || isNativeListView || isNativeTabControl || isNativeTabPage || isNativeButton
                ? false
                : control is MenuItem or Separator || isPopupRoot ? true : null),
            Alignment = isSettingsPageHeader || isChecklistGroup ? null
                : isEnvironmentInfoSeparator ? "TopLeft"
                : isEnvironmentInfoLayout ? null
                : isComboBoxPopupItem || isSpellCheckTextBox || isSpellCheckEditor
                        || fieldName == "_txtBranchCriterion" ? null
                : isCommitInfoHeaderLocalLayout || isRuntimeOutputHistoryControl ? null
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
                : isNativeListView ? string.Empty
                : isSourceList && control is ListBox sourceListWithSelection
                    ? GetSourceListText(sourceListWithSelection)
                : isSourcePictureBoxControl && control.Name == "menuHelp"
                ? GetToolTip(control) ?? GetText(control)
                : formBrowseSourceText is not null
                    ? formBrowseSourceText
                : IsHiddenEditorEncodingSelector(control) ? string.Empty : GetText(control),
            ToolTip = NormalizeText(formBrowseSourceToolTip ?? GetToolTip(control)),
            TranslationSource = fieldName,
            TabIndex = isComboBoxPopupItem ? null
                : isSurfaceRoot && !isPopupRoot ? 0
                : isSettingsRootTable ? 2
                : isSettingsPageHeader || isChecklistGroup || (isFormSettingsSurface && semanticName == "_page") ? 0
                : isEnvironmentInfoBottomSeparator ? 1
                : isEnvironmentInfoLayout || isEnvironmentInfoTopSeparator ? 0
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
                : isFormBrowseToolStripPanel
                    ? semanticName switch
                    {
                        "_leftPanel" => 1,
                        "_rightPanel" => 2,
                        "_topPanel" => 3,
                        "_bottomPanel" => 4,
                        _ => null,
                    }
                : isSearchWindowControl ? 1
                : isCommitPickerLocalLayout ? 0
                : isCommitInfoHeaderLocalLayout || isRuntimeOutputHistoryControl ? 0
                : GetSourceTabIndex(control, fieldName) is int sourceTabIndex ? sourceTabIndex
                : isFormBrowseContainerPanel && semanticName == "_contentPanel" ? 0
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
                    ? _root.GetType().FullName is
                        "GitUI.CommandsDialogs.FormCheckoutRevision" or
                        "GitUI.CommandsDialogs.FormCherryPick"
                        ? control.Name == "MainPanel" ? 0 : 1
                        : control.Name == "MainPanel" ? 1 : 0
                : isShellPreviewPanel ? 0
                : isGitIgnoreLocalSourceFlowLayoutPanel
                    ? control.Name == "flowLayoutPanel2" ? 6 : 5
                : isLocalSourceFlowLayoutPanel ? 3
                : isSemanticToolStripItem || control is MenuItem or Separator || isPopupRoot ? null
                : GetSourceTabIndex(control, fieldName) ?? KeyboardNavigation.GetTabIndex(control),
            TabStop = isComboBoxPopupItem ? null
                : isSurfaceRoot && !isPopupRoot
                ? true
                : isSettingsPageHeader
                ? true
                : isKnownSourceLocalControl
                ? false
                : isSemanticToolStripItem || control is MenuItem or Separator || isPopupRoot
                ? null
                : isFormCommitToolStripPanel
                ? false
                : isFormBrowseToolStripPanel
                ? false
                : isFormCommitSurface && semanticName is "splitMain" or "splitLeft" or "splitRight" or "toolStripContainer1" or "_currentFilesList"
                ? true
                : isFormBrowseSurface && semanticName is "LeftSplitContainer" or "RightSplitContainer" or "DiffFiles"
                ? true
                : isCommitInfoHeaderLocalLayout ? false
                : isRuntimeOutputHistoryControl ? true
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
            Enabled = IsHiddenFormBrowseDiffToolbarItem(control, semanticName)
                ? true
                : control is Separator || GetSourceTypeName(sourceType) == "ToolStripSeparator"
                ? false
                : isToolStripItem || isFormBrowseMenuStrip
                    ? control.IsEnabled
                    : semanticStateControl.IsEffectivelyEnabled,
            Visible = control is TabItem
                ? ((TabItem)control).IsSelected && ancestorSemanticVisible
                : isFormBrowseToolStripPanel && semanticName is "_bottomPanel" or "_leftPanel" or "_rightPanel"
                    ? false
                : isFormCommitSurface && semanticName is "Ok" or "Cancel"
                    ? true
                : semanticVisible,
            Focused = isBrowseHoveredToolStripItem
                ? true
                : isFormBrowseMenuStrip
                ? false
                : isNativeTabPage
                    ? false
                : sourceOwnerType == "GitUI.CommandsDialogs.FormBrowse"
                    && semanticName == "_gridView"
                    && _root.GetLogicalDescendants().OfType<MenuItem>().Any(item => item.IsSubMenuOpen)
                ? true
                : IsRepositoryHostSourceFocusedState(control, isPopupRoot)
                ? true
                : isSpellCheckTextBox && (control.IsFocused || control.ContextMenu?.IsOpen == true)
                    ? true
                : isPopupRoot || isComboBoxPopupItem ? false : IsFocused(semanticStateControl),
            ReadOnly = isPatchGridDataGrid
                ? true
                : isComboBoxPopup
                ? true
                : isSurfaceRoot && _usesDesignerLayoutMetadata && control is not Window
                    ? GetNullableBoolProperty(control, "ReadOnly")
                : isFormCommitSurface && semanticName is "toolAuthor" or "toolStripGpgKeyTextBox"
                ? null
                : isRevisionGridView
                ? true
                : isFileViewerTextEditor || isFileViewerInternal || control.Name == "_diffViewer"
                    || control.GetType().FullName == "GitUI.Editor.FileViewer"
                    ? GetNullableBoolProperty(control, "IsReadOnly")
                    : IsSourceRichTextControl(control)
                        ? true
                    : isFormSettingsSurface && semanticName == "_page"
                        ? false
                    : IsSourceCustomControl(control, fieldName) && control is not TextBox
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
                : isNativeListView
                    ? null
                : isSourceCheckedList
                    ? false
                : rootMetadataType == "GitUI.CommandsDialogs.SearchWindow"
                    && control.Name == "listBoxSearchResult"
                    ? false
                : isSourceList && control is ListBox sourceList
                    ? sourceList.SelectedIndex >= 0
                : isFormBrowseMenuStrip && control is MenuItem
                    ? isBrowseHoveredToolStripItem
                : isFormBrowseSurface && semanticName == "listBoxSearchResult"
                    ? false
                : isSpellCheckAutoComplete && control is ListBox spellCheckAutoComplete
                ? spellCheckAutoComplete.SelectedIndex >= 0
                : (isSemanticToolStripItem && control is not Separator) || isWatermarkComboBox
                ? isBrowseHoveredToolStripItem
                : GetSelected(control),
            Expanded = IsSourceTreeControl(sourceType) && control is TreeView treeView
                ? treeView.GetVisualDescendants().OfType<TreeViewItem>().Any(item => item.IsExpanded)
                : isFormCommitOptionsItem
                    ? false
                : isRevisionGrid || isRevisionGridView
                ? null
                : isFileStatusListView
                    ? ReadFileStatusListViewExpanded(semanticStateControl)
                : isPopupRoot
                    ? true
                    : primarySurface && IsFormBrowseDynamicMainMenu(control)
                        ? false
                    : GetSourceTypeName(sourceType) is "ToolStripDropDownButton" or "ToolStripSplitButton"
                        ? GetPropertyValue(control, "Flyout") is FlyoutBase sourceFlyout && sourceFlyout.IsOpen
                    : isSemanticToolStripItem && GetPropertyValue(control, "Flyout") is FlyoutBase flyout
                        ? flyout.IsOpen
                        : GetExpanded(control),
            Columns = ReadColumns(control),
            Children = children
        };

        if (isSurfaceRoot
            && !isPopupRoot
            && rootMetadataType is not ("GitUI.UserControls.Settings.SettingsCheckBox"
                or "GitUI.CommandsDialogs.SearchControl"
                or "GitUI.CommandsDialogs.FormCommit"))
        {
            node = node with { Margin = ReadPhysicalThicknessPair(new Thickness(3)) };
        }

        return ApplyComponentSemanticOverrides(
            node,
            control,
            semanticName,
            sourceOwnerType,
            rootMetadataType,
            ordinal,
            isSurfaceRoot,
            isNativeTabControl,
            isNativeTabPage,
            isEnvironmentInfoLayout,
            isEnvironmentInfoSeparator);
    }

    private CaptureNode ApplyComponentSemanticOverrides(
        CaptureNode node,
        Control control,
        string? semanticName,
        string sourceOwnerType,
        string rootMetadataType,
        int ordinal,
        bool isSurfaceRoot,
        bool isNativeTabControl,
        bool isNativeTabPage,
        bool isEnvironmentInfoLayout,
        bool isEnvironmentInfoSeparator)
    {
        node = ApplySettingControlBindingsSemanticOverrides(
            node,
            control,
            semanticName,
            rootMetadataType,
            ordinal,
            isSurfaceRoot);

        if (rootMetadataType == "GitUI.CommandsDialogs.AboutBoxDialog.FormContributors")
        {
            if (isSurfaceRoot)
            {
                return node with { Colors = ReadSourceDesignerColors(control) };
            }

            if (isNativeTabControl)
            {
                return node with
                {
                    Margin = ReadThicknessPair(new Thickness(3)),
                    Colors = ReadSourceDesignerColors(control),
                    Anchor = ["Top", "Left"],
                    Dock = "Fill",
                    AutoSize = false,
                    TabIndex = 0
                };
            }

            if (isNativeTabPage)
            {
                node = WithBoundsAndClientSize(node, new Rect(1, 29, 622, 412), new Size(622, 412));
                return node with
                {
                    Colors = ReadSourceDesignerColors(control),
                    BorderStyle = "None",
                    Anchor = ["Top", "Left"],
                    Dock = "None",
                    AutoSize = false,
                    Alignment = null,
                    TabIndex = ordinal,
                    TabStop = false
                };
            }

            if (control is TextBox)
            {
                CaptureColors colors = ReadSourceBackgroundColors(
                    control,
                    "GitExtensionsKnownColorWindowBrush",
                    ResolveResourceArgb("GitExtensionsKnownColorWindowTextBrush")) with
                {
                    Border = null,
                    SelectionForeground = null,
                    SelectionBackground = null,
                    InactiveSelectionForeground = null,
                    InactiveSelectionBackground = null,
                    Additional = new SortedDictionary<string, string>(StringComparer.Ordinal)
                };
                node = WithBoundsAndClientSize(
                    node,
                    new Rect(0, 0, 622, 412),
                    new Size(605, 412));
                return node with
                {
                    Colors = colors,
                    BorderStyle = "None",
                    Anchor = ["Top", "Left"],
                    Dock = "Fill",
                    AutoSize = true,
                    Alignment = "Left",
                    TabIndex = 0
                };
            }
        }

        if (rootMetadataType == "GitUI.CommandsDialogs.FormSettings")
        {
            bool isSettingsTreeSpacer = control is Border
                && node.FieldName is null
                && control.GetLogicalAncestors().Any(
                    ancestor => ancestor.GetType().FullName == "GitUI.CommandsDialogs.SettingsDialog.SettingsTreeViewUserControl");
            if (isSettingsTreeSpacer)
            {
                return node with
                {
                    Margin = ReadThicknessPair(new Thickness(3, 0)),
                    Font = ReadFont(_root),
                    BorderStyle = "None",
                    Anchor = ["Top", "Left"],
                    Dock = "Top",
                    AutoSize = false,
                    Alignment = "TopLeft",
                    TabIndex = node.BoundsDip.Y == 31 ? 2 : 0
                };
            }

            bool isSettingsHeaderWrapper = control.GetType().FullName == "GitUI.CommandsDialogs.SettingsDialog.SettingsPageHeader";
            bool isChecklistGroupWrapper = semanticName == "groupBox1" && node.FieldName is null;
            if (isSettingsHeaderWrapper)
            {
                return node with
                {
                    Margin = ReadThicknessPair(new Thickness(3, 4)),
                    Anchor = ["Top", "Left"],
                    ReadOnly = false
                };
            }

            if (isChecklistGroupWrapper)
            {
                return node with
                {
                    Margin = ReadThicknessPair(new Thickness(3)),
                    BorderStyle = null,
                    Anchor = ["Top", "Left"]
                };
            }

            if (semanticName is "panelCurrentSettingsPage" or "settingsPagePanel")
            {
                node = node with { BorderStyle = "None" };
            }

            if (sourceOwnerType == "GitUI.CommandsDialogs.SettingsDialog.SettingsPageHeader")
            {
                node = semanticName switch
                {
                    "tableLayoutPanel2" => WithBounds(node, node.BoundsDip with { Y = 0 }),
                    "tableLayoutPanel1" => WithBounds(node, node.BoundsDip with { Y = 4 }),
                    "label1" => WithBounds(node, node.BoundsDip with { Y = 10 }),
                    _ => node
                };
            }

            return node;
        }

        if (rootMetadataType == "GitUI.CommandsDialogs.FormAbout")
        {
            bool isAboutTableLayout = control is Border
                && node.FieldName is null
                && control.GetLogicalParent() is Control { Name: "panel1" };
            if (isAboutTableLayout)
            {
                return node with
                {
                    Margin = ReadThicknessPair(new Thickness(2)),
                    Font = ReadFont(_root),
                    BorderStyle = "None",
                    Anchor = ["Top", "Left"],
                    Dock = "Fill",
                    AutoSize = true,
                    TabIndex = 0
                };
            }

            if (semanticName == "panel1")
            {
                return node with
                {
                    Margin = ReadThicknessPair(new Thickness(12)),
                    Anchor = ["Top", "Left"],
                    Dock = "Fill",
                    AutoSize = false,
                    TabIndex = 1
                };
            }

            if (semanticName == "environmentInfo")
            {
                return node with { Margin = ReadThicknessPair(default(Thickness)) };
            }
        }

        if (rootMetadataType == "GitUI.CommandsDialogs.EnvironmentInfo"
            || rootMetadataType == "GitUI.CommandsDialogs.FormAbout")
        {
            if (isEnvironmentInfoLayout)
            {
                return node with
                {
                    Margin = ReadThicknessPair(default(Thickness)),
                    Anchor = ["Top", "Left"]
                };
            }

            if (isEnvironmentInfoSeparator)
            {
                return node with
                {
                    Colors = ReadSourceBackgroundColors(
                        control,
                        "GitExtensionsControlBackgroundBrush",
                        ResolveSourceControlTextArgb()),
                    Anchor = ["Top", "Left"]
                };
            }
        }

        if (rootMetadataType == "GitUI.CommandsDialogs.BrowseDialog.FormChangeLog"
            && semanticName == "ChangeLog")
        {
            node = WithClientSize(node, new Size(
                Math.Max(0, (double)node.BoundsDip.Width - 21),
                Math.Max(0, (double)node.BoundsDip.Height - 21)));
            CaptureColors colors = ReadColors(control);
            string? background = GitExtensions.Shims.WinForms.Application.SystemColorMode
                == GitExtensions.Shims.WinForms.SystemColorMode.Dark
                ? colors.Background
                : ResolveResourceArgb("GitExtensionsControlBackgroundBrush");
            return node with
            {
                Colors = colors with
                {
                    Foreground = ResolveSourceControlTextArgb(),
                    Background = background,
                    Border = null,
                    SelectionForeground = null,
                    SelectionBackground = null,
                    InactiveSelectionForeground = null,
                    InactiveSelectionBackground = null,
                    DisabledBackground = background,
                    Additional = new SortedDictionary<string, string>(StringComparer.Ordinal)
                }
            };
        }

        if (rootMetadataType == "GitUI.CommandsDialogs.BrowseDialog.FormDonate")
        {
            if (isSurfaceRoot)
            {
                node = node with { Focused = true };
            }

            return semanticName == "lblText" ? node with { AutoSize = true } : node;
        }

        if (rootMetadataType == "GitUI.CommandsDialogs.FormCommandlineHelp" && isSurfaceRoot)
        {
            return node with { Focused = true };
        }

        if (rootMetadataType == "GitUI.CommandsDialogs.BrowseDialog.FormOpenDirectory")
        {
            return semanticName switch
            {
                "folderBrowserButton" => node with { BorderStyle = null },
                "_NO_TRANSLATE_Directory" => node with { Selected = true },
                _ => node
            };
        }

        if (rootMetadataType is "GitUI.CommandsDialogs.FormClone" or "GitUI.CommandsDialogs.FormInit")
        {
            if (semanticName == "tpnlMain")
            {
                node = node with
                {
                    Margin = ReadThicknessPair(default(Thickness)),
                    Dock = "Fill",
                    AutoSize = true,
                    TabIndex = 0,
                    TabStop = false
                };
            }

            if (rootMetadataType == "GitUI.CommandsDialogs.FormInit" && semanticName == "tableLayoutPanel1")
            {
                node = node with
                {
                    Margin = ReadThicknessPair(default(Thickness)),
                    Dock = "Top",
                    AutoSize = false,
                    TabIndex = 0,
                    TabStop = false
                };
            }
        }

        if (rootMetadataType == "GitUI.CommandsDialogs.FormApplyPatch")
        {
            if (semanticName == "MainLayoutPanel")
            {
                node = node with
                {
                    Colors = ReadSourceDesignerColors(control),
                    Dock = "Fill",
                    AutoSize = false,
                    TabIndex = 0,
                    TabStop = false
                };
            }
            else if (semanticName is "panel2" or "panel3")
            {
                node = node with
                {
                    Colors = ReadSourceDesignerColors(control),
                    Anchor = ["Top", "Left"],
                    Dock = "None",
                    AutoSize = false,
                    TabIndex = semanticName == "panel2" ? 16 : 18,
                    TabStop = false
                };
            }
            else if (semanticName is "Patches" or "PatchGrid")
            {
                node = node with
                {
                    Colors = node.Colors with
                    {
                        Foreground = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")
                    }
                };
            }

            string? backgroundResource = semanticName switch
            {
                "ContinuePanel" or "MergeToolPanel" or "Resolved" => "GitExtensionsKnownColorActiveCaptionBrush",
                "Mergetool" => "GitExtensionsKnownColorControlDarkBrush",
                "SolveMergeConflicts" => "GitExtensionsMergeConflictsBackgroundBrush",
                _ => null
            };
            if (backgroundResource is not null)
            {
                string? background = ResolveResourceArgb(backgroundResource);
                node = node with
                {
                    Colors = node.Colors with
                    {
                        Foreground = semanticName == "SolveMergeConflicts"
                            ? ResolveResourceArgb("GitExtensionsMergeConflictsForegroundBrush")
                            : node.Colors.Foreground,
                        Background = background,
                        DisabledBackground = background
                    }
                };
            }
        }

        if (rootMetadataType == "GitUI.CommandsDialogs.FormMergeBranch"
            && semanticName is "mergeMessage" or "nbMessages")
        {
            node = node with { Colors = ReadSourceInputColors(control) };
        }

        if (rootMetadataType == "GitUI.CommandsDialogs.FormRebase"
            && semanticName is "MergeToolPanel" or "btnSolveConflicts")
        {
            node = node with
            {
                Colors = node.Colors with
                {
                    Background = "#00FFFFFF",
                    DisabledBackground = "#00FFFFFF"
                }
            };
        }

        if (rootMetadataType == "GitUI.CommandsDialogs.FormRebase"
            && semanticName == "btnSolveMergeconflicts")
        {
            string? background = ResolveResourceArgb("GitExtensionsMergeConflictsBackgroundBrush");
            node = node with
            {
                Colors = node.Colors with
                {
                    Foreground = ResolveResourceArgb("GitExtensionsMergeConflictsForegroundBrush"),
                    Background = background,
                    DisabledBackground = background
                }
            };
        }

        if (rootMetadataType == "GitUI.CommandsDialogs.FormRebase")
        {
            if (node.FieldName is null
                && semanticName is "PanelCurrentBranch" or "flowLayoutPanel1" or "flowLayoutPanel2")
            {
                node = node with
                {
                    Anchor = ["Top", "Left"],
                    Dock = "Fill",
                    AutoSize = true,
                    BorderStyle = "None"
                };
            }
            else if (semanticName == "PatchGrid")
            {
                node = node with { TabStop = true };
            }
            else if (semanticName == "Patches")
            {
                node = node with
                {
                    Colors = node.Colors with
                    {
                        Foreground = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush"),
                        SelectionBackground = ResolveResourceArgb("GitExtensionsKnownColorHighlightBrush")
                    }
                };
            }
        }

        if (rootMetadataType == "GitUI.CommandsDialogs.FormClone")
        {
            if (semanticName is "repositoryLabel" or "destinationLabel" or "subdirectoryLabel" or "brachLabel")
            {
                node = node with { Anchor = ["Top", "Left"] };
            }
            else if (semanticName == "LoadSSHKey")
            {
                node = node with
                {
                    AutoSize = true,
                    TabIndex = 1
                };
            }
            else if (semanticName == "MainPanel")
            {
                node = node with { AutoSize = true };
            }
        }

        if (rootMetadataType == "GitUI.CommandsDialogs.FormInit" && semanticName == "MainPanel")
        {
            node = node with { AutoSize = true };
        }

        if (sourceOwnerType == "GitUI.UserControls.CommitSummaryUserControl")
        {
            if (semanticName is "labelMessage" or "labelAuthor" or "labelDate" or "labelBranches" or "labelTags")
            {
                node = node with { Margin = ReadThicknessPair(new Thickness(2, 0)) };
            }

            if (semanticName == "groupBox1")
            {
                node = node with
                {
                    Margin = ReadThicknessPair(new Thickness(2)),
                    Colors = node.Colors with
                    {
                        Foreground = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush"),
                        DisabledForeground = ResolveResourceArgb("GitExtensionsKnownColorGrayTextBrush")
                    }
                };
            }

            if (control.Name == "tableLayoutPanel1")
            {
                node = node with
                {
                    Margin = ReadThicknessPair(new Thickness(2)),
                    Colors = node.Colors with
                    {
                        Foreground = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")
                    },
                    BorderStyle = "None",
                    Anchor = ["Top", "Left"],
                    Dock = "Fill",
                    AutoSize = true,
                    Alignment = null,
                    TabIndex = 0
                };
            }

            if (semanticName == "labelBranches")
            {
                node = node with
                {
                    Colors = node.Colors with
                    {
                        Background = ResolveResourceArgb("GitExtensionsCommitSummaryBranchesBackgroundBrush"),
                        DisabledBackground = ResolveResourceArgb("GitExtensionsCommitSummaryBranchesBackgroundBrush")
                    }
                };
            }

            if (semanticName == "labelTags")
            {
                node = rootMetadataType is "GitUI.CommandsDialogs.FormArchive" or "GitUI.CommandsDialogs.FormCherryPick"
                    ? WithSemanticColors(
                        node,
                        "GitExtensionsKnownColorControlBrush",
                        node.Colors.Foreground)
                    : node with { Colors = ReadSourceDesignerColors(control) };
            }
        }

        if (rootMetadataType == "GitUI.HelperDialogs.FormResetAnotherBranch")
        {
            if (semanticName == "tableLayoutPanel1")
            {
                node = node with { Dock = "Fill" };
            }

            if (semanticName == "commitSummaryUserControl")
            {
                node = node with
                {
                    Colors = node.Colors with
                    {
                        Foreground = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush"),
                        DisabledForeground = ResolveResourceArgb("GitExtensionsKnownColorGrayTextBrush")
                    }
                };
            }

            bool cancelHasDefaultFocus = _root.FindControl<Button>("Cancel")?.IsKeyboardFocusWithin == true;
            if (semanticName == "Branches" && cancelHasDefaultFocus)
            {
                node = node with { Focused = true };
            }
            else if (semanticName == "Cancel" && cancelHasDefaultFocus)
            {
                node = node with { Focused = false };
            }
        }

        if (rootMetadataType == "GitUI.CommandsDialogs.CommitDialog.FormCommitTemplateSettings")
        {
            if (semanticName == "MainPanel")
            {
                return node with
                {
                    FieldName = null,
                    FieldAliases = [],
                    Name = null,
                    Type = "System.Windows.Forms.Panel",
                    ControlKind = "control",
                    Padding = ReadThicknessPair(new Thickness(9)),
                    Margin = ReadThicknessPair(default(Thickness)),
                    Colors = ReadSourceBackgroundColors(
                        control,
                        "GitExtensionsPanelBackgroundBrush",
                        ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")),
                    BorderStyle = "None",
                    Anchor = ["Top", "Left"],
                    Dock = "Fill",
                    AutoSize = true,
                    Alignment = null,
                    Font = ReadFont(_root),
                    TabIndex = 1,
                    TabStop = false,
                    TranslationSource = null
                };
            }

            if (semanticName == "ControlsPanel")
            {
                return node with
                {
                    FieldName = null,
                    FieldAliases = [],
                    Name = null,
                    Type = "System.Windows.Forms.FlowLayoutPanel",
                    ControlKind = "control",
                    Padding = ReadThicknessPair(new Thickness(5)),
                    Margin = ReadThicknessPair(default(Thickness)),
                    Colors = ReadSourceBackgroundColors(
                        control,
                        "GitExtensionsDialogControlsBackgroundBrush",
                        ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")),
                    BorderStyle = "None",
                    Anchor = ["Top", "Left"],
                    Dock = "Bottom",
                    AutoSize = true,
                    Alignment = null,
                    TabIndex = 0,
                    TabStop = false,
                    TranslationSource = null
                };
            }

            if (semanticName is "tabControl1" or "tabPage1" or "tabPage2")
            {
                node = node with
                {
                    Colors = node.Colors with
                    {
                        Foreground = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")
                    }
                };
            }

            if (!IsDarkTheme()
                && GetSourceTypeName(GetSourceType(control, semanticName)) is "Label" or "CheckBox" or "TableLayoutPanel")
            {
                node = node with
                {
                    Colors = node.Colors with
                    {
                        Background = "#00FFFFFF",
                        DisabledBackground = "#00FFFFFF"
                    }
                };
            }

            if (semanticName is ("_NO_TRANSLATE_numericMaxFirstLineLength"
                or "_NO_TRANSLATE_numericMaxLineLength"
                or "_NO_TRANSLATE_textBoxCommitValidationRegex")
                && !IsInSelectedTab(control))
            {
                node = node with
                {
                    Colors = ReadSourceBackgroundColors(
                        control,
                        "GitExtensionsKnownColorWindowBrush",
                        node.Colors.Foreground)
                };
            }

            if (semanticName == "_NO_TRANSLATE_textBoxCommitValidationRegex")
            {
                node = node with
                {
                    Colors = node.Colors with
                    {
                        SelectionForeground = null,
                        SelectionBackground = null,
                        Additional = new SortedDictionary<string, string>(StringComparer.Ordinal)
                    }
                };
            }

            // WinForms reports the native tab-page client coordinates and the runtime
            // TableLayoutPanel column allocation. Avalonia's presenter transforms and text
            // measurement are renderer details, so project those source-owned semantic bounds.
            node = semanticName switch
            {
                "tableLayoutPanel5" or "tableLayoutPanel3" => WithBounds(
                    node,
                    node.BoundsDip with { X = 3, Y = 3, Width = 666, Height = 262 }),
                "checkBoxRegexEnabled" => WithBounds(
                    node,
                    node.BoundsDip with { X = 3, Y = 243, Width = 92, Height = 19 }),
                "_NO_TRANSLATE_textCommitTemplateText" => WithClientSize(node, new Size(529, 194)),
                "_NO_TRANSLATE_numericMaxFirstLineLength" or
                "_NO_TRANSLATE_numericMaxLineLength" or
                "checkBoxAutoWrap" or
                "checkBoxUseIndent" or
                "checkBoxSecondLineEmpty" => WithBounds(node, node.BoundsDip with { X = 376 }),
                "_NO_TRANSLATE_textBoxCommitValidationRegex" => WithClientSize(
                    WithBounds(node, node.BoundsDip with { X = 376, Width = 287 }),
                    new Size(283, (double)node.ClientSizeDip.Height)),
                "labelMaxFirstLineLength" => WithClientSize(
                    WithBounds(node, node.BoundsDip with { Width = 367 }),
                    new Size(367, (double)node.ClientSizeDip.Height)),
                "labelMaxLineLength" => WithClientSize(
                    WithBounds(node, node.BoundsDip with { Width = 331 }),
                    new Size(331, (double)node.ClientSizeDip.Height)),
                "labelAutoWrap" => WithClientSize(
                    WithBounds(node, node.BoundsDip with { Width = 266 }),
                    new Size(266, (double)node.ClientSizeDip.Height)),
                "labelRegExCheck" => WithClientSize(
                    WithBounds(node, node.BoundsDip with { Width = 345 }),
                    new Size(345, (double)node.ClientSizeDip.Height)),
                "labelUseIndent" => WithBounds(node, node.BoundsDip with { Y = 121 }),
                "labelSecondLineEmpty" => WithBounds(node, node.BoundsDip with { Y = 149 }),
                _ => node
            };

            if (semanticName is "buttonOk" or "buttonCancel")
            {
                node = node with
                {
                    Colors = ReadSourceBackgroundColors(
                        control,
                        "GitExtensionsDialogControlsBackgroundBrush",
                        ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush"))
                };
            }

            bool hasExplicitFocusedInput = new[]
                {
                    "_NO_TRANSLATE_comboBoxCommitTemplates",
                    "_NO_TRANSLATE_textBoxCommitTemplateName",
                    "_NO_TRANSLATE_numericMaxFirstLineLength",
                    "_NO_TRANSLATE_textBoxCommitValidationRegex"
                }
                .Select(name => _root.FindControl<Control>(name))
                .Any(candidate => candidate?.IsKeyboardFocusWithin == true);
            if (semanticName == "buttonOk" && !hasExplicitFocusedInput)
            {
                node = node with { Focused = true };
            }
            else if (semanticName == "tabControl1")
            {
                node = node with { Focused = false };
            }
        }

        node = ApplyRemoteWorkflowSemanticOverrides(
            node,
            control,
            semanticName,
            sourceOwnerType,
            rootMetadataType,
            isSurfaceRoot,
            isNativeTabControl,
            isNativeTabPage);

        node = ApplyBranchDialogSemanticOverrides(
            node,
            control,
            semanticName,
            sourceOwnerType,
            rootMetadataType,
            isSurfaceRoot);

        node = ApplyRepositoryMaintenanceSemanticOverrides(
            node,
            control,
            semanticName,
            rootMetadataType);

        node = ApplySparseWorkingCopyLayoutOverrides(
            node,
            control,
            rootMetadataType);

        return ApplyRepositoryOperationSemanticOverrides(
            node,
            control,
            semanticName,
            sourceOwnerType,
            rootMetadataType,
            isSurfaceRoot);
    }

    private CaptureNode ApplySparseWorkingCopyLayoutOverrides(
        CaptureNode node,
        Control control,
        string rootMetadataType)
    {
        if (rootMetadataType != "GitUI.CommandsDialogs.FormSparseWorkingCopy"
            || ReferenceEquals(control, _root)
            || _root is not ContentControl { Content: Grid rootLayout })
        {
            return node;
        }

        CaptureNode WithLayout(
            CaptureNode value,
            string type,
            string? dock,
            bool autoSize,
            int tabIndex,
            bool tabStop,
            string? alignment = null,
            string? borderStyle = null,
            string? flatStyle = null)
            => value with
            {
                Type = type,
                Font = value.Font ?? ReadFont(_root),
                Anchor = ["Top", "Left"],
                Dock = dock,
                AutoSize = autoSize,
                Alignment = alignment,
                TabIndex = tabIndex,
                TabStop = tabStop,
                BorderStyle = borderStyle,
                FlatStyle = flatStyle,
                BorderWidthDip = null,
                CornerRadiusDip = null
            };

        if (ReferenceEquals(control, rootLayout))
        {
            return WithLayout(
                node,
                "System.Windows.Forms.TableLayoutPanel",
                "Fill",
                autoSize: true,
                tabIndex: 0,
                tabStop: false,
                borderStyle: "None");
        }

        Control? branch = control.GetLogicalAncestors()
            .OfType<Control>()
            .FirstOrDefault(ancestor => ReferenceEquals(ancestor.Parent, rootLayout));
        branch ??= ReferenceEquals(control.Parent, rootLayout) ? control : null;
        if (branch is null)
        {
            return node;
        }

        int row = Grid.GetRow(branch);
        if (ReferenceEquals(control, branch))
        {
            CaptureNode branchNode = row switch
            {
                0 => WithLayout(node, "System.Windows.Forms.TableLayoutPanel", "Fill", true, 0, false, borderStyle: "None"),
                1 => WithLayout(node, "System.Windows.Forms.Control", "Fill", false, 1, true),
                2 => WithLayout(node, "System.Windows.Forms.Panel", "Fill", true, 2, false, borderStyle: "None"),
                3 => WithLayout(node, "System.Windows.Forms.Panel", "Fill", true, 3, false, borderStyle: "None"),
                4 => WithLayout(node, "System.Windows.Forms.Control", "Fill", false, 4, true),
                5 => WithLayout(node, "System.Windows.Forms.TableLayoutPanel", "Fill", true, 5, false, borderStyle: "None"),
                _ => node
            };

            if (row == 5)
            {
                Thickness margin = control.Margin;
                branchNode = WithBoundsAndClientSize(
                    branchNode,
                    new Rect(
                        (double)node.BoundsDip.X - margin.Left,
                        (double)node.BoundsDip.Y - margin.Top,
                        (double)node.BoundsDip.Width + margin.Left + margin.Right,
                        (double)node.BoundsDip.Height + margin.Top + margin.Bottom),
                    new Size(
                        (double)node.BoundsDip.Width + margin.Left + margin.Right,
                        (double)node.BoundsDip.Height + margin.Top + margin.Bottom)) with
                {
                    Margin = ReadThicknessPair(default(Thickness)),
                    Padding = ReadThicknessPair(margin)
                };
            }

            return branchNode;
        }

        Control? parent = control.GetLogicalAncestors().OfType<Control>().FirstOrDefault();
        if (row == 2
            && parent is Grid
            && ReferenceEquals(parent.Parent, branch))
        {
            int childOrdinal = GetCaptureChildren(parent).ToList().IndexOf(control);
            return childOrdinal switch
            {
                0 => WithBoundsAndClientSize(
                    WithLayout(node, "System.Windows.Forms.Label", "Fill", true, 0, false, "MiddleLeft", "None"),
                    new Rect(10, 5, Math.Max(0, rootLayout.Bounds.Width - 95), 23),
                    new Size(Math.Max(0, rootLayout.Bounds.Width - 95), 23)),
                1 => WithBoundsAndClientSize(
                    WithLayout(node, "System.Windows.Forms.Button", "Right", false, 1, true, "MiddleCenter", flatStyle: "Standard") with
                    {
                        Padding = ReadThicknessPair(default(Thickness))
                    },
                    new Rect(Math.Max(0, rootLayout.Bounds.Width - 85), 5, 75, 23),
                    new Size(75, 23)),
                _ => node
            };
        }

        if (!ReferenceEquals(parent, branch))
        {
            return node;
        }

        int ordinal = GetCaptureChildren(branch).ToList().IndexOf(control);
        if (row == 0)
        {
            return WithLayout(
                node,
                "System.Windows.Forms.Label",
                "Bottom",
                autoSize: true,
                tabIndex: ordinal,
                tabStop: false,
                alignment: "TopLeft",
                borderStyle: "None");
        }

        if (row == 2)
        {
            return ordinal switch
            {
                0 => WithBoundsAndClientSize(
                    WithLayout(node, "System.Windows.Forms.TableLayoutPanel", "Bottom", true, 0, false, borderStyle: "None") with
                    {
                        Margin = ReadThicknessPair(default(Thickness)),
                        Padding = ReadThicknessPair(new Thickness(10, 5))
                    },
                    new Rect(0, 0, rootLayout.Bounds.Width, 33),
                    new Size(rootLayout.Bounds.Width, 33)),
                1 => WithBoundsAndClientSize(
                    WithLayout(node, "System.Windows.Forms.Control", "Bottom", false, 1, true),
                    new Rect(0, 33, rootLayout.Bounds.Width, 2),
                    new Size(rootLayout.Bounds.Width, 2)),
                2 => WithBoundsAndClientSize(
                    WithLayout(node, "System.Windows.Forms.LinkLabel", "Bottom", true, 2, true, "TopLeft", "None") with
                    {
                        Text = string.Concat(control.GetLogicalChildren().OfType<Control>().Select(GetText))
                    },
                    new Rect(0, 32, (double)node.BoundsDip.Width, 36),
                    new Size((double)node.BoundsDip.Width, 36)),
                _ => node
            };
        }

        if (row == 3)
        {
            return ordinal switch
            {
                0 => WithBoundsAndClientSize(
                    WithLayout(node, "GitUI.Editor.FileViewer", "Fill", false, 0, true, borderStyle: "None"),
                    new Rect(0, 0, 200, 100),
                    new Size(200, 100)),
                1 => WithBoundsAndClientSize(
                    WithLayout(node, "System.Windows.Forms.Control", "Top", false, 1, true),
                    new Rect(0, 0, 200, 2),
                    new Size(200, 2)),
                2 => WithBoundsAndClientSize(
                    WithLayout(node, "System.Windows.Forms.Label", "Top", true, 2, false, "TopLeft", "None"),
                    new Rect(0, 20, Math.Max(0, rootLayout.Bounds.Width - 19), 36),
                    new Size(Math.Max(0, rootLayout.Bounds.Width - 19), 36)),
                3 => WithBoundsAndClientSize(
                    WithLayout(node, "System.Windows.Forms.Label", "Top", true, 3, false, "TopLeft", "None"),
                    new Rect(0, 0, 294, 20),
                    new Size(294, 20)),
                _ => node
            };
        }

        if (row == 5)
        {
            double footerY = Math.Max(0, rootLayout.Bounds.Height - 53);
            return ordinal switch
            {
                0 => WithBoundsAndClientSize(
                    WithLayout(node, "System.Windows.Forms.CheckBox", "Fill", true, 0, true, "MiddleLeft", flatStyle: "Standard") with
                    {
                        Padding = ReadThicknessPair(default(Thickness))
                    },
                    new Rect((double)node.BoundsDip.X, (double)node.BoundsDip.Y - footerY, (double)node.BoundsDip.Width, (double)node.BoundsDip.Height),
                    new Size((double)node.BoundsDip.Width, (double)node.BoundsDip.Height)),
                1 => WithBoundsAndClientSize(
                    WithLayout(node, "System.Windows.Forms.Button", "Bottom", false, 1, true, "MiddleCenter", flatStyle: "Standard") with
                    {
                        Padding = ReadThicknessPair(default(Thickness))
                    },
                    new Rect((double)node.BoundsDip.X, (double)node.BoundsDip.Y - footerY, (double)node.BoundsDip.Width, (double)node.BoundsDip.Height),
                    new Size((double)node.BoundsDip.Width, (double)node.BoundsDip.Height)),
                2 => WithBoundsAndClientSize(
                    WithLayout(node, "System.Windows.Forms.Control", "Fill", false, 2, true) with
                    {
                        Margin = ReadThicknessPair(new Thickness(3))
                    },
                    new Rect((double)node.BoundsDip.X, (double)node.BoundsDip.Y - footerY + 3, (double)node.BoundsDip.Width, Math.Max(0, (double)node.BoundsDip.Height - 6)),
                    new Size((double)node.BoundsDip.Width, Math.Max(0, (double)node.BoundsDip.Height - 6))),
                3 => WithBoundsAndClientSize(
                    WithLayout(node, "System.Windows.Forms.Button", "Bottom", false, 3, true, "MiddleCenter", flatStyle: "Standard") with
                    {
                        Padding = ReadThicknessPair(default(Thickness))
                    },
                    new Rect((double)node.BoundsDip.X, (double)node.BoundsDip.Y - footerY, (double)node.BoundsDip.Width, (double)node.BoundsDip.Height),
                    new Size((double)node.BoundsDip.Width, (double)node.BoundsDip.Height)),
                _ => node
            };
        }

        return node;
    }

    private CaptureNode ApplySettingControlBindingsSemanticOverrides(
        CaptureNode node,
        Control control,
        string? semanticName,
        string rootMetadataType,
        int ordinal,
        bool isSurfaceRoot)
    {
        if (rootMetadataType is not
            ("GitUI.SettingControlBindings.SettingControlBindingsCaptureSurface" or
             "GitUI.SettingControlBindings.SettingControlBindingsNullCaptureSurface"))
        {
            return node;
        }

        const string sourceRootId = "$unnamed[0]:SettingControlBindingsCaptureSurface";
        string id = node.Id
            .Replace("$root:SettingControlBindingsCaptureSurface", sourceRootId, StringComparison.Ordinal)
            .Replace("$root:SettingControlBindingsNullCaptureSurface", "$unnamed[0]:SettingControlBindingsNullCaptureSurface", StringComparison.Ordinal)
            .Replace(":TextBlock", ":Label", StringComparison.Ordinal);
        if (semanticName == "numberControl")
        {
            id = id.Replace("/numberControl", "/_parent", StringComparison.Ordinal);
        }

        bool isNullSurface = rootMetadataType == "GitUI.SettingControlBindings.SettingControlBindingsNullCaptureSurface";
        if (isNullSurface)
        {
            id = id.Replace("/_layout", "/$unnamed[0]:TableLayoutPanel", StringComparison.Ordinal);
        }

        node = node with { Id = id };
        if (isSurfaceRoot)
        {
            return node with
            {
                Padding = ReadThicknessPair(new Thickness(12)),
                Font = ReadUiFont(),
                BorderStyle = "None"
            };
        }

        if (semanticName == "_layout")
        {
            return node with
            {
                FieldName = isNullSurface ? null : node.FieldName,
                FieldAliases = isNullSurface ? [] : node.FieldAliases,
                Type = "System.Windows.Forms.TableLayoutPanel",
                Margin = ReadThicknessPair(new Thickness(3)),
                Font = ReadUiFont(),
                Colors = ReadSettingControlContainerColors(control),
                Anchor = ["Top", "Left"],
                Dock = "Fill",
                AutoSize = false,
                BorderStyle = "None",
                TranslationSource = isNullSurface ? null : node.TranslationSource,
                TabIndex = 0,
                TabStop = false
            };
        }

        bool isCaption = semanticName is null && control is TextBlock;
        if (isCaption)
        {
            return node with
            {
                Type = "System.Windows.Forms.Label",
                Margin = ReadThicknessPair(new Thickness(3, 6, 3, 3)),
                Anchor = ["Left"],
                Dock = "None",
                AutoSize = true,
                Alignment = "TopLeft",
                Colors = ReadSettingControlContainerColors(control),
                BorderStyle = "None",
                TabIndex = ordinal,
                TabStop = false
            };
        }

        if (semanticName is "boolControl" or "choiceControl" or "stringControl" or
            "passwordControl" or "numberControl" or "numberTextControl" or
            "credentialsControl" or "pseudoControl")
        {
            bool isTextInput = semanticName is "stringControl" or "passwordControl" or "numberTextControl";
            bool isPseudo = semanticName == "pseudoControl";
            bool isNumeric = semanticName == "numberControl";
            string sourceType = semanticName switch
            {
                "boolControl" => "System.Windows.Forms.CheckBox",
                "choiceControl" => "System.Windows.Forms.ComboBox",
                "numberControl" => "System.Windows.Forms.NumericUpDown",
                "credentialsControl" => "GitExtensions.Extensibility.Settings.UserControls.CredentialsControl",
                _ => "System.Windows.Forms.TextBox"
            };
            bool isChoice = semanticName == "choiceControl";
            CaptureColors colors = isTextInput || isNumeric || isPseudo || isChoice
                ? ReadSettingControlInputColors(
                    control,
                    useWindowBackground: isChoice || semanticName == "numberTextControl")
                : ReadSettingControlContainerColors(control);
            CaptureNode result = node with
            {
                FieldName = isNumeric ? "_parent" : null,
                FieldAliases = [],
                Type = sourceType,
                Padding = ReadThicknessPair(default(Thickness)),
                Colors = colors,
                Font = semanticName == "credentialsControl" ? ReadUiFont() : node.Font,
                Anchor = ["Left", "Right"],
                Dock = "None",
                AutoSize = isTextInput || isPseudo,
                Alignment = semanticName switch
                {
                    "boolControl" => "MiddleLeft",
                    "choiceControl" or "credentialsControl" => null,
                    _ => "Left"
                },
                TranslationSource = isNumeric ? "_parent" : null,
                ToolTip = null,
                BorderStyle = isTextInput || isNumeric
                    ? "Fixed3D"
                    : isPseudo || semanticName == "credentialsControl" ? "None" : null,
                BorderWidthDip = null,
                FlatStyle = semanticName == "boolControl" ? "Standard" : null,
                CornerRadiusDip = null,
                TabIndex = ordinal,
                TabStop = true
            };
            return isPseudo
                ? WithClientSize(result, new Size((double)result.BoundsDip.Width, (double)result.BoundsDip.Height))
                : result;
        }

        if (semanticName == "mainTableLayoutPanel")
        {
            return node with
            {
                Type = "System.Windows.Forms.TableLayoutPanel",
                Margin = ReadThicknessPair(new Thickness(4, 3)),
                Font = ReadUiFont(),
                Colors = ReadSettingControlContainerColors(control),
                Anchor = ["Top", "Left"],
                Dock = "Fill",
                AutoSize = false,
                BorderStyle = "None",
                TabIndex = 0,
                TabStop = false
            };
        }

        if (semanticName is "userNameLabel" or "passwordLabel")
        {
            return node with
            {
                Type = "System.Windows.Forms.Label",
                Margin = ReadThicknessPair(semanticName == "userNameLabel"
                    ? new Thickness(0, 3, 4, 0)
                    : new Thickness(4, 3, 4, 0)),
                Colors = ReadSettingControlContainerColors(control),
                Anchor = ["Top", "Left"],
                Dock = "Fill",
                AutoSize = true,
                Alignment = "TopLeft",
                BorderStyle = "None",
                TabIndex = semanticName == "userNameLabel" ? 0 : 1,
                TabStop = false
            };
        }

        if (semanticName is "userNameTextBox" or "passwordTextBox")
        {
            return node with
            {
                Type = "System.Windows.Forms.TextBox",
                Padding = ReadThicknessPair(default(Thickness)),
                Margin = ReadThicknessPair(semanticName == "userNameTextBox"
                    ? new Thickness(4, 0)
                    : new Thickness(4, 0, 0, 0)),
                Colors = ReadSettingControlInputColors(control, useWindowBackground: false),
                Anchor = ["Top", "Left"],
                Dock = "Fill",
                AutoSize = true,
                Alignment = "Left",
                BorderStyle = "Fixed3D",
                BorderWidthDip = null,
                TabIndex = semanticName == "userNameTextBox" ? 2 : 3,
                TabStop = true
            };
        }

        return node;
    }

    private CaptureColors ReadSettingControlContainerColors(Control control)
        => ReadSourceBackgroundColors(
            control,
            "GitExtensionsKnownColorControlBrush",
            ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")) with
        {
            SelectionForeground = null,
            SelectionBackground = null,
            InactiveSelectionForeground = null,
            InactiveSelectionBackground = null,
            Additional = new SortedDictionary<string, string>(StringComparer.Ordinal)
        };

    private CaptureColors ReadSettingControlInputColors(Control control, bool useWindowBackground)
    {
        CaptureColors colors = ReadSourceInputColors(control);
        bool isInvalid = control.Classes.Contains("plugin-setting-invalid");
        bool isReadOnly = GetNullableBoolProperty(control, "IsReadOnly") == true;
        string backgroundResource = isInvalid
            ? "GitExtensionsInvalidSettingBackgroundBrush"
            : isReadOnly
                ? "GitExtensionsSettingReadOnlyBackgroundBrush"
                : useWindowBackground
                    ? "GitExtensionsKnownColorWindowBrush"
                    : "GitExtensionsTextInputBackgroundBrush";
        string? background = ResolveResourceArgb(backgroundResource);
        return colors with
        {
            Foreground = isInvalid
                ? ResolveResourceArgb("GitExtensionsInvalidSettingForegroundBrush")
                : colors.Foreground,
            Background = background,
            DisabledBackground = background
        };
    }

    private CaptureNode ApplyRepositoryMaintenanceSemanticOverrides(
        CaptureNode node,
        Control control,
        string? semanticName,
        string rootMetadataType)
    {
        if (rootMetadataType == "GitUI.CommandsDialogs.FormVerify")
        {
            bool hasExplicitCaptureFocus = _root.GetLogicalDescendants()
                .OfType<Control>()
                .Any(candidate => candidate.IsKeyboardFocusWithin
                    && candidate.Name is "Warnings" or "Remove");
            if (semanticName is "panel1" or "panel2" or "flowLayoutPanel1")
            {
                bool isFlowPanel = semanticName == "flowLayoutPanel1";
                bool isTopPanel = semanticName == "panel2";
                return node with
                {
                    Type = isFlowPanel
                        ? "System.Windows.Forms.FlowLayoutPanel"
                        : "System.Windows.Forms.Panel",
                    Margin = ReadThicknessPair(new Thickness(3)),
                    Padding = isFlowPanel
                        ? ReadThicknessPair(new Thickness(5))
                        : node.Padding,
                    BorderStyle = "None",
                    Anchor = ["Top", "Left"],
                    Dock = isFlowPanel ? "Left" : isTopPanel ? "Top" : "Bottom",
                    AutoSize = isFlowPanel || isTopPanel
                };
            }

            if (semanticName == "Warnings")
            {
                Control semanticGrid = (Control?)control.Parent ?? control;
                node = WithBoundsAndClientSize(
                    node,
                    new Rect(0, 0, semanticGrid.Bounds.Width, semanticGrid.Bounds.Height),
                    semanticGrid.Bounds.Size);
                return node with
                {
                    Type = "System.Windows.Forms.DataGridView",
                    ControlKind = "dataGrid",
                    Colors = ReadSourceDataGridColors(control) with
                    {
                        Foreground = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush"),
                        SelectionBackground = ResolveResourceArgb("GitExtensionsKnownColorHighlightBrush")
                    },
                    BorderStyle = "FixedSingle",
                    ReadOnly = false,
                    ItemHeightDip = 33,
                    Columns = ReadFormVerifyColumns(),
                    Anchor = ["Top", "Left"],
                    Dock = "Fill",
                    AutoSize = false
                };
            }

            if (semanticName is "splitContainer1" or "fileViewer" or "internalFileViewer")
            {
                CaptureColors colors = ReadTransparentContainerColors(control);
                return node with
                {
                    Colors = colors with
                    {
                        Foreground = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")
                    }
                };
            }

            if (semanticName == "btnRestoreSelectedObjects" && !hasExplicitCaptureFocus)
            {
                return node with { Focused = true };
            }

            if ((semanticName is "ShowCommitsAndTags" or "ShowOtherObjects") && !hasExplicitCaptureFocus)
            {
                return node with { Focused = false };
            }

            if (semanticName is "TextEditor" or "fileviewerToolbar" or "nextChangeButton"
                or "previousChangeButton" or "increaseNumberOfLines" or "decreaseNumberOfLines"
                or "showEntireFileButton" or "showNonPrintChars" or "showSyntaxHighlighting"
                or "ignoreWhitespaceAtEol" or "ignoreWhiteSpaces" or "ignoreAllWhitespaces"
                or "settingsButton" or "PictureBox")
            {
                node = node with
                {
                    Colors = node.Colors with
                    {
                        Foreground = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")
                    }
                };
                if (semanticName == "TextEditor")
                {
                    node = node with { Font = node.Font! with { Family = "Consolas" } };
                }

                string? tooltip = semanticName switch
                {
                    "nextChangeButton" => "Next change\u00a0(Alt+Down)",
                    "previousChangeButton" => "Previous change\u00a0(Alt+Up)",
                    "increaseNumberOfLines" => "Increase the number of lines of context\u00a0(Ctrl+Oemplus)",
                    "decreaseNumberOfLines" => "Decrease the number of lines of context\u00a0(Ctrl+OemMinus)",
                    "showEntireFileButton" => "Show entire file\u00a0(Ctrl+E)",
                    "showSyntaxHighlighting" => "Show syntax highlighting\u00a0(X)",
                    "ignoreAllWhitespaces" => "Ignore all whitespace changes\u00a0(Ctrl+Shift+W)",
                    _ => null
                };
                return tooltip is null ? node : node with { ToolTip = tooltip };
            }

            return node;
        }

        if (rootMetadataType == "GitUI.CommandsDialogs.FormCleanupRepository")
        {
            if (semanticName is "textBoxIncludePaths" or "textBoxExcludePaths")
            {
                return WithClientSize(
                    node,
                    new Size(
                        Math.Max(0, (double)node.BoundsDip.Width - 21),
                        (double)node.ClientSizeDip.Height));
            }

            if (semanticName == "PreviewOutput")
            {
                node = WithClientSize(
                    node,
                    new Size(
                        Math.Max(0, (double)node.BoundsDip.Width - 21),
                        Math.Max(0, (double)node.BoundsDip.Height - 21)));
                string? background = ResolveResourceArgb("GitExtensionsCleanupPreviewBackgroundBrush");
                return node with
                {
                    Colors = ReadSourceInputColors(control) with
                    {
                        Background = background,
                        DisabledBackground = background
                    }
                };
            }

            return node;
        }

        if (rootMetadataType != "GitUI.CommandsDialogs.FormSparseWorkingCopy")
        {
            return node;
        }

        if (ReferenceEquals(control, _root)
            || semanticName is
                "decreaseNumberOfLines" or
                "fileviewerToolbar" or
                "ignoreAllWhitespaces" or
                "ignoreWhitespaceAtEol" or
                "ignoreWhiteSpaces" or
                "increaseNumberOfLines" or
                "internalFileViewer" or
                "nextChangeButton" or
                "PictureBox" or
                "previousChangeButton" or
                "settingsButton" or
                "showEntireFileButton" or
                "showNonPrintChars" or
                "showSyntaxHighlighting")
        {
            node = node with
            {
                Colors = node.Colors with
                {
                    Foreground = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")
                }
            };
        }

        return semanticName switch
        {
            "_NO_TRANSLATE_lblShowPreview" => WithSemanticColors(
                node,
                "GitExtensionsKnownColorControlBrush",
                ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")) with
            {
                Anchor = ["Top", "Left"],
                Dock = "None",
                AutoSize = true
            },
            "internalFileViewer" => WithBounds(
                node,
                node.BoundsDip with { Y = 0 }) with
            {
                Anchor = ["Top", "Left"],
                Dock = "Fill",
                AutoSize = false,
                Alignment = null
            },
            "TextEditor" => WithSemanticColors(
                node,
                "GitExtensionsKnownColorControlBrush",
                ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")) with
            {
                Anchor = ["Top", "Left"],
                Dock = "Fill",
                AutoSize = false
            },
            "PictureBox" => node with
            {
                Anchor = ["Top", "Left"],
                Dock = "Fill",
                AutoSize = false
            },
            "fileviewerToolbar" => node with { Anchor = ["Top", "Right"] },
            _ => node
        };
    }

    private CaptureNode ApplyBranchDialogSemanticOverrides(
        CaptureNode node,
        Control control,
        string? semanticName,
        string sourceOwnerType,
        string rootMetadataType,
        bool isSurfaceRoot)
    {
        bool isBranchDialog = rootMetadataType is
            "GitUI.CommandsDialogs.FormCreateBranch" or
            "GitUI.CommandsDialogs.FormCheckoutBranch" or
            "GitUI.CommandsDialogs.FormDeleteBranch" or
            "GitUI.CommandsDialogs.FormRenameBranch";
        if (!isBranchDialog)
        {
            return node;
        }

        string? sourceType = GetSourceTypeName(GetSourceType(control, semanticName));
        bool isSourceInput = sourceType is "ComboBox" or "TextBox" or "BranchComboBox";
        bool isControlsPanel = semanticName == "ControlsPanel"
            || control.GetLogicalAncestors().OfType<Control>().Any(ancestor => ancestor.Name == "ControlsPanel");
        string? controlText = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush");

        if (isSurfaceRoot)
        {
            node = WithSemanticColors(node, "GitExtensionsKnownColorControlBrush", controlText);
            node = node with
            {
                Children = node.Children
                    .Where(child => rootMetadataType != "GitUI.CommandsDialogs.FormCheckoutBranch"
                                    || child.FieldName != "flowLayoutPanel1")
                    .OrderBy(child => (double)child.BoundsDip.Y)
                    .ThenBy(child => (double)child.BoundsDip.X)
                    .ToArray()
            };
        }
        else if (isSourceInput)
        {
            node = node with { Colors = ReadSourceInputColors(control) };
        }
        else if (isControlsPanel)
        {
            node = node with
            {
                Colors = ReadDialogControlsPanelColors(control) with
                {
                    Foreground = controlText,
                    Border = null
                }
            };
        }

        if (semanticName is "MainPanel" or "ControlsPanel")
        {
            bool isMainPanel = semanticName == "MainPanel";
            node = node with
            {
                FieldName = null,
                FieldAliases = [],
                Name = null,
                TranslationSource = null,
                Margin = ReadThicknessPair(default(Thickness)),
                Padding = isMainPanel
                    ? ReadThicknessPair(rootMetadataType switch
                    {
                        "GitUI.CommandsDialogs.FormCreateBranch" => new Thickness(12),
                        "GitUI.CommandsDialogs.FormCheckoutBranch" => new Thickness(14),
                        "GitUI.CommandsDialogs.FormDeleteBranch" => new Thickness(9),
                        _ => default
                    })
                    : ReadThicknessPair(new Thickness(5)),
                Dock = isMainPanel ? "Fill" : "Bottom",
                AutoSize = !isMainPanel,
                TabIndex = isMainPanel ? 1 : 0
            };

            return isMainPanel
                ? WithSemanticColors(node, "GitExtensionsPanelBackgroundBrush", controlText)
                : node;
        }

        if (rootMetadataType == "GitUI.CommandsDialogs.FormCreateBranch"
            && semanticName == "tableLayoutPanel1"
            && control.GetLogicalAncestors().Any(
                ancestor => ancestor.GetType().FullName == "GitUI.UserControls.CommitPickerSmallControl"))
        {
            return node with
            {
                FieldName = null,
                FieldAliases = [],
                Name = null,
                TranslationSource = null,
                Margin = ReadThicknessPair(new Thickness(2)),
                Colors = node.Colors with
                {
                    Foreground = controlText
                },
                BorderStyle = "None",
                Anchor = ["Top", "Left"],
                Dock = "Fill",
                AutoSize = true,
                TabIndex = 0
            };
        }

        if (rootMetadataType == "GitUI.CommandsDialogs.FormCreateBranch")
        {
            if (semanticName == "tableLayout")
            {
                node = node with
                {
                    Colors = node.Colors with
                    {
                        Foreground = controlText
                    },
                    Margin = ReadThicknessPair(default(Thickness)),
                    Dock = "Fill",
                    AutoSize = true,
                    TabIndex = 0
                };
            }

            if (semanticName is "commitPicker" or "commitSummaryUserControl1")
            {
                node = node with
                {
                    Colors = node.Colors with
                    {
                        Foreground = controlText
                    }
                };
            }

            if (sourceOwnerType == "GitUI.UserControls.CommitSummaryUserControl")
            {
                if (semanticName == "tableLayoutPanel1")
                {
                    node = node with
                    {
                        FieldName = null,
                        FieldAliases = [],
                        Name = null,
                        TranslationSource = null,
                        Margin = ReadThicknessPair(new Thickness(2)),
                        BorderStyle = "None",
                        Anchor = ["Top", "Left"],
                        Dock = "Fill",
                        AutoSize = true,
                        TabIndex = 0
                    };
                }
                else if (semanticName == "groupBox1")
                {
                    node = node with { Margin = ReadThicknessPair(new Thickness(2)) };
                }
                else if (semanticName is "labelAuthor" or "labelMessage" or "labelBranches" or "labelTags" or "labelDate")
                {
                    node = node with { Margin = ReadThicknessPair(new Thickness(2, 0)) };
                    if (semanticName == "labelTags")
                    {
                        node = WithSemanticColors(
                            node,
                            "GitExtensionsKnownColorControlBrush",
                            node.Colors.Foreground);
                    }
                }
            }

            if (semanticName == "lbCommits")
            {
                node = node with
                {
                    Colors = node.Colors with
                    {
                        Foreground = ResolveResourceArgb("GitExtensionsKnownColorGrayTextBrush")
                    }
                };
            }
        }
        else if (rootMetadataType == "GitUI.CommandsDialogs.FormCheckoutBranch")
        {
            node = semanticName switch
            {
                "localChangesGB" or "tlpnlBranches" => node with
                {
                    Anchor = ["Top", "Left", "Right"],
                    AutoSize = true
                },
                "tlpnlRemoteOptions" => node with { Anchor = ["Top", "Left", "Right"] },
                "horLine" => WithSemanticColors(
                    WithClientSize(node, new Size(690, 0)),
                    "GitExtensionsPanelBackgroundBrush",
                    controlText) with
                {
                    Anchor = ["Top", "Left", "Right"],
                    AutoSize = true,
                    Alignment = "TopLeft"
                },
                _ => node
            };

            if (semanticName == "lbChanges")
            {
                node = node with
                {
                    Colors = node.Colors with
                    {
                        Foreground = ResolveResourceArgb("GitExtensionsKnownColorGrayTextBrush")
                    }
                };
            }
        }
        else if (rootMetadataType == "GitUI.CommandsDialogs.FormDeleteBranch")
        {
            if (semanticName is "Branches" or "labelSelectBranches")
            {
                node = node with
                {
                    Colors = node.Colors with
                    {
                        Foreground = controlText
                    }
                };
            }

            if (semanticName == "Branches")
            {
                node = WithSemanticColors(
                    node,
                    "GitExtensionsPanelBackgroundBrush",
                    controlText) with
                {
                    BorderStyle = "None"
                };
            }

            if (semanticName == "tlpnlMain")
            {
                node = node with
                {
                    Colors = node.Colors with
                    {
                        Foreground = controlText
                    },
                    Margin = ReadThicknessPair(default(Thickness)),
                    Dock = "Fill",
                    AutoSize = true,
                    TabIndex = 0
                };
            }
        }
        else if (rootMetadataType == "GitUI.CommandsDialogs.FormRenameBranch"
                 && semanticName == "label1")
        {
            node = node with
            {
                Colors = node.Colors with
                {
                    Foreground = controlText
                }
            };
        }

        return node;
    }

    private CaptureNode ApplyRemoteWorkflowSemanticOverrides(
        CaptureNode node,
        Control control,
        string? semanticName,
        string sourceOwnerType,
        string rootMetadataType,
        bool isSurfaceRoot,
        bool isNativeTabControl,
        bool isNativeTabPage)
    {
        bool isRemoteWorkflow = rootMetadataType is
            "GitUI.CommandsDialogs.FormPull" or
            "GitUI.CommandsDialogs.FormPush" or
            "GitUI.CommandsDialogs.FormRemotes" or
            "GitUI.CommandsDialogs.FormDeleteRemoteBranch";
        if (!isRemoteWorkflow)
        {
            return node;
        }

        string? sourceType = GetSourceTypeName(GetSourceType(control, semanticName));
        string? controlText = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush");
        bool isSourceInput = sourceType is "ComboBox" or "CaseSensitiveComboBox" or "TextBox" or "TextBoxEx";

        if (rootMetadataType == "GitUI.CommandsDialogs.FormDeleteRemoteBranch")
        {
            bool isControlsPanel = control.Name == "ControlsPanel"
                || control.GetLogicalAncestors().OfType<Control>().Any(ancestor => ancestor.Name == "ControlsPanel");
            if (isSurfaceRoot)
            {
                node = WithSemanticColors(node, "GitExtensionsControlBackgroundBrush", controlText);
            }
            else if (isSourceInput)
            {
                node = node with { Colors = ReadSourceInputColors(control) };
            }
            else
            {
                node = WithSemanticColors(
                    node,
                    isControlsPanel
                        ? "GitExtensionsDialogControlsBackgroundBrush"
                        : "GitExtensionsPanelBackgroundBrush",
                    controlText);
            }

            node = semanticName switch
            {
                "MainPanel" => node with { AutoSize = true },
                "Branches" => node with
                {
                    BorderStyle = "None",
                    TabStop = true
                },
                "_NO_TRANSLATE_labelLocalTrackingBranches" when string.IsNullOrEmpty(node.Text)
                    => WithBoundsAndClientSize(node, new Rect(98, 78, 0, 15), new Size(0, 15)),
                _ => node
            };

            return node;
        }

        if (!isSurfaceRoot && !isSourceInput && sourceType is not "ListView" and not "NativeListView")
        {
            node = node with { Colors = node.Colors with { Foreground = controlText } };
        }

        bool isInsideSourceTabPage = isNativeTabPage
            || control.GetLogicalAncestors().OfType<TabItem>().Any(IsNativeTabPage);
        if (!IsDarkTheme()
            && rootMetadataType is "GitUI.CommandsDialogs.FormPush" or "GitUI.CommandsDialogs.FormRemotes"
            && isInsideSourceTabPage
            && semanticName != "btnRemoteColor"
            && !isSourceInput
            && sourceType is not "ListView" and not "NativeListView" and not "DataGridView")
        {
            node = WithSemanticColors(node, null, controlText, transparent: true);
        }

        if (sourceType is "Panel" or "FlowLayoutPanel" or "TableLayoutPanel" or "GroupBox" or "TabPage")
        {
            node = node with { TabStop = false };
        }

        if (isNativeTabControl)
        {
            node = node with { Focused = false };
        }

        if (semanticName == "Remotes" && rootMetadataType == "GitUI.CommandsDialogs.FormRemotes")
        {
            node = WithClientSize(
                node,
                new Size(
                    Math.Max(0, (double)node.BoundsDip.Width - 4),
                    (double)node.ClientSizeDip.Height)) with
            {
                ControlKind = "list",
                Text = string.Empty,
                Selected = null
            };
        }

        if (semanticName == "BranchGrid" && rootMetadataType == "GitUI.CommandsDialogs.FormPush")
        {
            node = node with
            {
                ReadOnly = false,
                Colors = node.Colors with
                {
                    Foreground = controlText,
                    SelectionBackground = ResolveResourceArgb("GitExtensionsKnownColorHighlightBrush")
                                          ?? node.Colors.SelectionBackground
                }
            };
        }
        else if (semanticName == "RemoteBranches" && rootMetadataType == "GitUI.CommandsDialogs.FormRemotes")
        {
            node = node with
            {
                ReadOnly = true,
                Colors = node.Colors with
                {
                    Foreground = controlText,
                    SelectionBackground = ResolveResourceArgb("GitExtensionsKnownColorHighlightBrush")
                                          ?? node.Colors.SelectionBackground
                }
            };
        }

        return node;
    }

    private CaptureNode ApplyRepositoryOperationSemanticOverrides(
        CaptureNode node,
        Control control,
        string? semanticName,
        string sourceOwnerType,
        string rootMetadataType,
        bool isSurfaceRoot)
    {
        if (rootMetadataType == "GitUI.CommandsDialogs.FormFileHistory")
        {
            node = ApplyFileHistorySemanticOverrides(node, control, semanticName, sourceOwnerType, isSurfaceRoot);
        }
        else if (rootMetadataType == "GitUI.CommandsDialogs.FormStash")
        {
            node = ApplyStashSemanticOverrides(node, control, semanticName, sourceOwnerType, isSurfaceRoot);
        }
        else if (rootMetadataType == "GitUI.CommandsDialogs.FormBrowse"
                 && semanticName is "tsbtnAdvancedFilter" or "tsbShowReflog" or "tssbtnShowBranches" or "toolStripLabel1"
                     or "tsddbtnRevisionFilter"
                 && (semanticName == "tsddbtnRevisionFilter" || node.Visible == true)
                 && GetSourceTypeName(GetSourceType(control, semanticName)) is "ToolStripButton" or "ToolStripSplitButton"
                     or "ToolStripDropDownButton" or "ToolStripLabel" or "ToolStripSeparator")
        {
            // Only these source filter items resolve to transparent WindowText. The branch
            // filter, separator, text label and first-parent button resolve to Control.
            node = WithSemanticColors(
                node,
                null,
                ResolveResourceArgb("GitExtensionsWindowTextBrush"),
                transparent: true);
        }

        return node;
    }

    private CaptureNode ApplyFileHistorySemanticOverrides(
        CaptureNode node,
        Control control,
        string? semanticName,
        string sourceOwnerType,
        bool isSurfaceRoot)
    {
        string? windowText = ResolveResourceArgb("GitExtensionsWindowTextBrush");
        string? controlText = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush");
        string? sourceType = GetSourceTypeName(GetSourceType(control, semanticName));

        if (semanticName == "MainPanel")
        {
            // The Avalonia layout root has no source field; keep it as an anonymous wrapper.
            node = node with
            {
                FieldName = null,
                FieldAliases = [],
                Name = null,
                TranslationSource = null
            };
        }

        if (isSurfaceRoot)
        {
            node = WithSemanticColors(node, "GitExtensionsPanelBackgroundBrush", windowText);
        }
        else if (sourceOwnerType == "GitUI.CommandsDialogs.FormFileHistory")
        {
            if (semanticName == "ToolStripFilters")
            {
                node = WithSemanticColors(node, null, windowText, transparent: true);
                node = WithBoundsAndClientSize(node, new Rect(0, 0, 748, 25), new Size(748, 25));
                node = node with { Padding = ReadThicknessPair(default(Thickness)) };
            }
            else if (sourceType is "ToolStripButton" or "ToolStripSplitButton" or "ToolStripDropDownButton" or "ToolStripLabel" or "ToolStripSeparator")
            {
                node = WithSemanticColors(node, null, null, transparent: true);
                node = ApplyFileHistoryToolbarBounds(node, semanticName);
            }
            else if (sourceType == "ToolStripComboBox")
            {
                node = WithSemanticColors(node, "GitExtensionsWindowBackgroundBrush", windowText);
                node = ApplyFileHistoryToolbarBounds(node, semanticName);
                node = node with { Margin = ReadThicknessPair(new Thickness(1, 0, 1, 0)) };
            }
            else if (semanticName is "splitContainer1" or "RevisionGrid")
            {
                node = WithSemanticColors(node, null, null, transparent: true);
            }
            else if (semanticName is "tabControl1" or "CommitInfoTabPage" or "DiffTab" or "ViewTab" or "BlameTab"
                     or "CommitDiff" or "Diff" or "View" or "Blame")
            {
                bool transparentTabContent = !IsDarkTheme()
                    && semanticName is "DiffTab" or "ViewTab" or "BlameTab" or "View" or "Blame";
                node = transparentTabContent
                    ? WithSemanticColors(node, null, windowText, transparent: true)
                    : WithSemanticColors(node, "GitExtensionsKnownColorControlBrush", windowText);
            }
        }

        if (sourceOwnerType == "GitUI.FileStatusList")
        {
            if (semanticName is "Toolbar" or "lblSplitter"
                || (IsFileStatusToolbarProductItemName(semanticName)
                    && semanticName is not ("btnCollapseGroups" or "btnRefresh" or "sepRefresh")))
            {
                node = WithSemanticColors(node, null, null, transparent: true);
            }
            else if (semanticName is "btnCollapseGroups" or "btnRefresh" or "sepRefresh")
            {
                node = WithSemanticColors(node, "GitExtensionsKnownColorControlBrush", null);
            }
        }

        bool isFileHistoryToolbarControl = sourceOwnerType == "GitUI.CommandsDialogs.FormFileHistory"
            || control.GetLogicalAncestors().Any(
                ancestor => ancestor.GetType().FullName == "GitUI.UserControls.FilterToolBar");
        if (isFileHistoryToolbarControl)
        {
            node = ApplyFileHistoryToolbarBounds(node, semanticName);
            string? toolbarSourceType = GetSourceTypeName(GetSourceType(control, semanticName));
            if (toolbarSourceType is "ToolStripButton" or "ToolStripSplitButton" or "ToolStripDropDownButton" or "ToolStripLabel" or "ToolStripSeparator")
            {
                node = WithSemanticColors(node, null, null, transparent: true);
            }
            else if (toolbarSourceType == "ToolStripComboBox")
            {
                node = WithSemanticColors(node, "GitExtensionsWindowBackgroundBrush", windowText);
                node = node with { Margin = ReadThicknessPair(new Thickness(1, 0, 1, 0)) };
            }

            if (semanticName == "toolStripSeparator3")
            {
                node = WithSemanticColors(node, null, null, transparent: true);
            }
        }

        if (semanticName is "rtbRevisionHeader" or "RevisionInfo")
        {
            node = WithSemanticColors(node, "GitExtensionsWindowBackgroundBrush", windowText);
        }
        else if (semanticName is "pnlCommitMessage" or "rtbxCommitMessage")
        {
            node = WithSemanticColors(node, "GitExtensionsCommitMessageBackgroundBrush", windowText);
        }

        if (semanticName is "btnCollapseGroups" or "btnRefresh"
            && control.GetLogicalAncestors().Any(ancestor => ancestor.GetType().FullName == "GitUI.FileStatusList"))
        {
            node = node with { Colors = node.Colors with { Foreground = controlText } };
        }

        node = semanticName switch
        {
            "RevisionGrid" => WithBoundsAndClientSize(node, new Rect(0, 0, 748, 107), new Size(748, 107)),
            "_gridView" when control.GetLogicalAncestors().OfType<Control>().Any(ancestor => ancestor.Name == "RevisionGrid")
                => WithBoundsAndClientSize(node, new Rect(0, 22, 748, 85), new Size(748, 85)),
            "tabControl1" => WithBoundsAndClientSize(node, new Rect(0, 113, 748, 331), new Size(748, 331)),
            "DiffTab" => WithBoundsAndClientSize(node, new Rect(1, 29, 746, 301), new Size(746, 301)),
            "Diff" => WithBoundsAndClientSize(node, new Rect(0, 0, 746, 301), new Size(746, 301)),
            "internalFileViewer" or "TextEditor"
                when control.GetLogicalAncestors().OfType<Control>().Any(ancestor => ancestor.Name == "DiffTab")
                => WithBoundsAndClientSize(node, new Rect(0, 0, 746, 301), new Size(746, 301)),
            _ => node
        };

        if (sourceOwnerType is "GitUI.Editor.FileViewer" or "GitUI.Editor.FileViewerInternal")
        {
            string? fileViewerName = control.GetLogicalAncestors()
                .OfType<Control>()
                .FirstOrDefault(ancestor => ancestor.GetType().FullName == "GitUI.Editor.FileViewer")?.Name;
            fileViewerName ??= control.GetType().FullName == "GitUI.Editor.FileViewer" ? semanticName : null;
            bool transparentViewer = !IsDarkTheme()
                || fileViewerName is "DiffText" or "BlameAuthor" or "BlameFile";
            if (semanticName is "internalFileViewer" or "TextEditor" or "_NO_TRANSLATE_lblShowPreview")
            {
                node = WithSemanticColors(
                    node,
                    transparentViewer ? null : "GitExtensionsKnownColorControlBrush",
                    windowText,
                    transparent: transparentViewer);
            }
            else if (semanticName == "fileviewerToolbar" || IsFileViewerToolbarItem(control))
            {
                node = WithSemanticColors(node, "GitExtensionsKnownColorControlBrush", null);
            }
        }

        bool usesMenuColors = sourceType is "ToolStripMenuItem" or "ToolStripSeparator";
        bool hasProductForeground = semanticName is "btnUnequalChange" or "btnOnlyB" or "btnOnlyA" or "btnSameChange";
        bool keepsControlText = sourceOwnerType == "GitUI.FileStatusList"
            && semanticName is "btnCollapseGroups" or "btnRefresh";
        if (!usesMenuColors && !hasProductForeground && !keepsControlText && node.Colors.Foreground == controlText)
        {
            node = node with { Colors = node.Colors with { Foreground = windowText } };
        }

        return node;
    }

    private CaptureNode ApplyStashSemanticOverrides(
        CaptureNode node,
        Control control,
        string? semanticName,
        string sourceOwnerType,
        bool isSurfaceRoot)
    {
        string? controlText = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush");
        string? windowText = ResolveResourceArgb("GitExtensionsWindowTextBrush");
        string? sourceType = GetSourceTypeName(GetSourceType(control, semanticName));

        if (isSurfaceRoot)
        {
            node = WithSemanticColors(node, "GitExtensionsKnownColorControlBrush", controlText);
        }
        else if (sourceOwnerType == "GitUI.CommandsDialogs.FormStash")
        {
            if (semanticName is "splitContainer1" or "tableLayoutPanel2" or "tableLayoutPanel1" or "panel1" or "Stashed"
                or "StashSelectedFiles" or "Stash" or "chkIncludeUntrackedFiles" or "StashKeepIndex"
                or "Apply" or "Clear" or "messageLabel" or "View")
            {
                node = WithSemanticColors(node, null, null, transparent: true);
            }
            else if (semanticName == "StashMessage")
            {
                node = WithSemanticColors(node, "GitExtensionsKnownColorInfoBrush", windowText);
            }
            else if (semanticName is "toolStrip1" or "Loading" or "_waitSpinner")
            {
                node = WithSemanticColors(node, "GitExtensionsKnownColorControlBrush", controlText);
            }
            else if (semanticName == "Stashes")
            {
                node = WithSemanticColors(node, "GitExtensionsWindowBackgroundBrush", windowText);
            }
        }

        if (sourceOwnerType == "GitUI.FileStatusList")
        {
            if (semanticName is "Stashed" or "Toolbar" or "lblSplitter"
                || (IsFileStatusToolbarProductItemName(semanticName)
                    && semanticName is not ("sepFilter" or "sepOptions" or "btnUnequalChange" or "btnOnlyB"
                        or "btnOnlyA" or "btnSameChange" or "btnFindInFilesGitGrep")))
            {
                node = WithSemanticColors(node, null, controlText, transparent: true);
            }
            else if (semanticName is "sepFilter" or "sepOptions" or "btnUnequalChange" or "btnOnlyB"
                     or "btnOnlyA" or "btnSameChange" or "btnFindInFilesGitGrep")
            {
                node = WithSemanticColors(node, "GitExtensionsKnownColorControlBrush", null);
            }
        }

        if (sourceOwnerType is "GitUI.Editor.FileViewer" or "GitUI.Editor.FileViewerInternal")
        {
            if (semanticName is "internalFileViewer" or "TextEditor" or "_NO_TRANSLATE_lblShowPreview")
            {
                node = WithSemanticColors(node, null, controlText, transparent: true);
            }
            else if (semanticName == "fileviewerToolbar" || IsFileViewerToolbarItem(control))
            {
                string? itemForeground = semanticName == "encodingToolStripComboBox"
                    ? windowText
                    : control is Separator ? node.Colors.Foreground : controlText;
                node = WithSemanticColors(node, "GitExtensionsKnownColorControlBrush", itemForeground);
            }
        }

        bool keepsWindowText = semanticName is "Stashes" or "StashMessage" or "FileStatusListView"
            or "cboFilterComboBox" or "cboFindInCommitFilesGitGrep" or "encodingToolStripComboBox";
        if (!keepsWindowText && node.Colors.Foreground == windowText)
        {
            node = node with { Colors = node.Colors with { Foreground = controlText } };
        }

        if (control is Separator && IsSemanticToolStripItem(control))
        {
            node = node with
            {
                Colors = node.Colors with
                {
                    Foreground = ResolveResourceArgb("GitExtensionsKnownColorControlDarkBrush")
                }
            };
        }

        node = semanticName switch
        {
            "Stashes" => WithBoundsAndClientSize(node, new Rect(42, 2, 236, 23), new Size(236, 23)) with
            {
                Margin = ReadThicknessPair(new Thickness(1, 0, 1, 0))
            },
            "View" => WithBoundsAndClientSize(node, new Rect(286, 0, 422, 520), new Size(422, 520)),
            "internalFileViewer" or "TextEditor"
                when control.GetLogicalAncestors().OfType<Control>().Any(ancestor => ancestor.Name == "View")
                => WithBoundsAndClientSize(node, new Rect(0, 0, 422, 520), new Size(422, 520)),
            "Toolbar" => node with { Padding = ReadThicknessPair(default(Thickness)) },
            "lblSplitter" => node with { Margin = ReadThicknessPair(new Thickness(2, 0, 2, 0)) },
            "tableLayoutPanel1" or "tableLayoutPanel2" => node with
            {
                AutoSize = semanticName == "tableLayoutPanel1",
                TabStop = false
            },
            "messageLabel" => node with { TabIndex = 2 },
            "_waitSpinner" => node with
            {
                BorderStyle = null,
                Anchor = ["Top", "Left"],
                AutoSize = false
            },
            _ => node
        };

        return node;
    }

    private CaptureNode ApplyFileHistoryToolbarBounds(CaptureNode node, string? semanticName)
    {
        // WinForms ToolStrip computes these native-96 allocations from its item defaults.
        Rect? bounds = semanticName switch
        {
            "tsbtnAdvancedFilter" => new Rect(5, 1, 32, 22),
            "tsbShowReflog" => new Rect(37, 1, 23, 22),
            "tssbtnShowBranches" => new Rect(60, 1, 104, 22),
            "toolStripLabel1" => new Rect(164, 1, 58, 22),
            "tscboBranchFilter" => new Rect(223, 1, 100, 23),
            "tsddbtnBranchFilter" => new Rect(324, 1, 29, 22),
            "toolStripSeparator19" => new Rect(353, 0, 6, 25),
            "tslblRevisionFilter" => new Rect(359, 1, 36, 22),
            "tstxtRevisionFilter" => new Rect(396, 1, 100, 23),
            "tsddbtnRevisionFilter" => new Rect(497, 1, 29, 22),
            "tsmiShowOnlyFirstParent" => new Rect(526, 1, 23, 22),
            "toolStripSeparator3" => new Rect(549, 0, 6, 25),
            "toolStripSplitLoad" => new Rect(555, 1, 32, 22),
            "ShowFullHistory" => new Rect(587, 1, 29, 22),
            "toolStripBlameOptions" => new Rect(616, 1, 29, 22),
            "gitcommandLogToolStripMenuItem" => new Rect(645, 1, 23, 22),
            _ => null
        };
        return bounds is Rect value
            ? WithBoundsAndClientSize(node, value, value.Size)
            : node;
    }

    private CaptureNode WithSemanticColors(
        CaptureNode node,
        string? backgroundResource,
        string? foreground,
        bool transparent = false)
    {
        string? background = transparent ? "#00FFFFFF" : ResolveResourceArgb(backgroundResource!);
        return node with
        {
            Colors = node.Colors with
            {
                Foreground = foreground ?? node.Colors.Foreground,
                Background = background,
                DisabledBackground = background,
                Border = null
            }
        };
    }

    private CaptureNode WithBounds(CaptureNode node, CaptureRectangleF bounds)
        => node with
        {
            BoundsPx = new CaptureRectangle
            {
                X = ToPixel((double)bounds.X),
                Y = ToPixel((double)bounds.Y),
                Width = ToPixel((double)bounds.Width),
                Height = ToPixel((double)bounds.Height)
            },
            BoundsDip = bounds
        };

    private CaptureNode WithBoundsAndClientSize(CaptureNode node, Rect bounds, Size clientSize)
        => WithClientSize(
            WithBounds(
                node,
                new CaptureRectangleF
                {
                    X = ToDecimal(bounds.X),
                    Y = ToDecimal(bounds.Y),
                    Width = ToDecimal(bounds.Width),
                    Height = ToDecimal(bounds.Height)
                }),
            clientSize);

    private CaptureNode WithClientSize(CaptureNode node, Size clientSize)
        => node with
        {
            ClientSizePx = new CaptureSize
            {
                Width = ToPixel(clientSize.Width),
                Height = ToPixel(clientSize.Height)
            },
            ClientSizeDip = new CaptureSizeF
            {
                Width = ToDecimal(clientSize.Width),
                Height = ToDecimal(clientSize.Height)
            }
        };

    private DesignerLayoutMetadata? GetDesignerLayout(Control control, string? fieldName)
    {
        if (!_usesDesignerLayoutMetadata || fieldName is null)
        {
            return null;
        }

        string ownerType = _fieldOwnerTypes.GetValueOrDefault(control)
            ?? GetMetadataTypeName(_root.GetType());
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
           ?? GetMetadataTypeName(_root.GetType());

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
               && typeName is not ("ToolStrip" or "ToolStripEx" or "ToolStripContainer"
                   or "ToolStripPanel" or "ToolStripContentPanel" or "MenuStrip" or "MenuStripEx");
    }

    private static string? GetSourceTypeName(string? sourceType)
        => sourceType is null ? null : sourceType[(sourceType.LastIndexOf('.') + 1)..];

    private static bool IsSourceAmbientControl(string? sourceType)
        => GetSourceTypeName(sourceType) is "FlowLayoutPanel" or "GroupBox" or "Label" or "LinkLabel"
            or "Panel" or "PictureBox" or "SettingsCheckBox" or "TableLayoutPanel";

    private static bool IsSourceListControl(string? sourceType)
        => GetSourceTypeName(sourceType) is "CheckedListBox" or "ListBox" or "ListView" or "NativeListView"
            or "TreeView" or "NativeTreeView";

    private static bool IsSourceTreeControl(string? sourceType)
        => GetSourceTypeName(sourceType) is "TreeView" or "NativeTreeView";

    private static bool IsKnownSourceLocalControl(string sourceOwnerType, string? name)
        => (sourceOwnerType == "GitUI.CommandsDialogs.EnvironmentInfo"
                && name is "tableLayoutPanel1" or "lblSeparatorTop" or "lblSeparatorBottom")
           || (sourceOwnerType == "GitUI.CommandsDialogs.FormVerify"
               && name is "panel1" or "panel2" or "flowLayoutPanel1")
           || (sourceOwnerType == "GitUI.CommandsDialogs.FormRebase"
               && name is "PanelCurrentBranch" or "flowLayoutPanel1" or "flowLayoutPanel2")
           || (sourceOwnerType == "GitUI.UserControls.CommitSummaryUserControl"
               && name == "tableLayoutPanel1")
           || (sourceOwnerType == "GitUI.CommandsDialogs.SettingsDialog.Pages.ChecklistSettingsPage"
               && name == "groupBox1");

    private static string? GetKnownSourceLocalType(string sourceOwnerType, string? name)
        => (sourceOwnerType, name) switch
        {
            ("GitUI.CommandsDialogs.EnvironmentInfo", "tableLayoutPanel1") => "System.Windows.Forms.TableLayoutPanel",
            ("GitUI.CommandsDialogs.EnvironmentInfo", "lblSeparatorTop" or "lblSeparatorBottom") => "System.Windows.Forms.Label",
            ("GitUI.CommandsDialogs.FormVerify", "panel1" or "panel2") => "System.Windows.Forms.Panel",
            ("GitUI.CommandsDialogs.FormVerify", "flowLayoutPanel1") => "System.Windows.Forms.FlowLayoutPanel",
            ("GitUI.CommandsDialogs.FormRebase", "PanelCurrentBranch" or "flowLayoutPanel1" or "flowLayoutPanel2")
                => "System.Windows.Forms.FlowLayoutPanel",
            ("GitUI.UserControls.CommitSummaryUserControl", "tableLayoutPanel1") => "System.Windows.Forms.TableLayoutPanel",
            ("GitUI.CommandsDialogs.SettingsDialog.Pages.ChecklistSettingsPage", "groupBox1") => "System.Windows.Forms.GroupBox",
            _ => null
        };

    private static bool IsChecklistStatusButton(string? name)
        => name is "GitFound" or "UserNameSet" or "MergeTool" or "DiffTool"
            or "ShellExtensionsRegistered" or "GitBinFound" or "GitExtensionsInstall"
            or "SshConfig" or "translationConfig" or "GcmDetected";

    private static double GetSourceListClientWidth(Control control, Rect bounds, string? sourceType, string? borderStyle)
    {
        double nativeBorder = borderStyle == "FixedSingle" ? 2 : 4;
        double nativeVerticalScrollBar = GetSourceTypeName(sourceType) == "NativeListView"
            && control.GetVisualDescendants().OfType<ScrollBar>().Any(
                scrollBar => scrollBar.Orientation == Avalonia.Layout.Orientation.Vertical
                             && scrollBar.IsVisible
                             && scrollBar.Bounds.Width > 0)
                ? 17
                : 0;
        return Math.Max(0, bounds.Width - nativeBorder - nativeVerticalScrollBar);
    }

    private static double GetSourceTextBoxClientWidth(
        Rect bounds,
        string? borderStyle,
        bool hasVerticalScrollBar)
    {
        double nativeBorder = borderStyle == "None" ? 0 : 4;
        double nativeVerticalScrollBar = hasVerticalScrollBar ? 17 : 0;
        return Math.Max(0, bounds.Width - nativeBorder - nativeVerticalScrollBar);
    }

    private static double GetSourceTextBoxClientHeight(Rect bounds, string? borderStyle)
        => Math.Max(0, bounds.Height - (borderStyle == "None" ? 0 : 4));

    private Thickness GetDefaultDesignerMargin(Control control, string? sourceType)
    {
        Thickness margin = GetSourceTypeName(sourceType) == "FileStatusList"
            ? new Thickness(3, 4)
            : control is TextBlock or Label or HyperlinkButton ? new Thickness(3, 0) : new Thickness(3);
        if (!WinFormsInputMetadata.DesignerDpiByType.TryGetValue(
                GetMetadataTypeName(_root.GetType()),
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
            ?? GetMetadataTypeName(_root.GetType());
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
            if (sourceTypeName is "Label" or "LinkLabel" or "Panel" or "TableLayoutPanel" or "FlowLayoutPanel"
                or "PictureBox" or "TabPage"
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

    private static string? GetDefaultDesignerBorderStyle(Control control, string? sourceType)
        => IsSourceTreeControl(sourceType)
            ? "Fixed3D"
            : control switch
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
        if (_root.GetType().FullName == "GitUI.CommandsDialogs.FormCommit"
            && TryGetFormCommitOptionsBounds(control.Name, out Rect optionsBounds))
        {
            return optionsBounds;
        }

        Control? popupOwner = semanticParent is not null && IsOverlayPopupHost(semanticParent)
            ? semanticParent
            : control.GetVisualAncestors().OfType<Control>().FirstOrDefault(IsOverlayPopupHost);
        if (popupOwner is not null && control is MenuItem or Separator)
        {
            Control[] items = popupOwner.GetVisualDescendants()
                .OfType<Control>()
                .Where(item => item is MenuItem or Separator)
                .Where(item => item.IsVisible)
                .Where(item => !item.GetVisualAncestors()
                    .TakeWhile(ancestor => !ReferenceEquals(ancestor, popupOwner))
                    .OfType<MenuItem>()
                    .Any())
                .ToArray();
            int itemIndex = Array.IndexOf(items, control);
            double y = 2 + items
                .Take(Math.Max(0, itemIndex))
                .Sum(item => item is Separator ? 6 : 22);

            bool isPrimaryContextMenu = !string.IsNullOrEmpty(control.Name);

            // parity-scaffolding: ToolStrip lays out its popup canvas with two vertical insets;
            // separators occupy a six-DIP row and retain the native two-DIP leading inset.
            return new Rect(
                control is Separator ? 2 : 0,
                y,
                Math.Max(
                    0,
                    popupOwner.Bounds.Width - (control is Separator
                        ? 4
                        : isPrimaryContextMenu ? 1 : 2)),
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
                ? new Rect(1, GetWorkspaceTabContentTop(owner), Math.Max(0, owner.Bounds.Width - 2), Math.Max(0, owner.Bounds.Height - GetWorkspaceTabContentTop(owner) - 1))
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
                ? new Rect(pageChildOrigin.X - 1, pageChildOrigin.Y - GetWorkspaceTabContentTop(tabOwner), control.Bounds.Width, control.Bounds.Height)
                : tabOwner.Classes.Contains("gitextensions-full-bleed-tabs")
                    ? new Rect(pageChildOrigin.X - 1, pageChildOrigin.Y - 29, control.Bounds.Width, control.Bounds.Height)
                    : new Rect(pageChildOrigin.X - 4, pageChildOrigin.Y - 30, control.Bounds.Width, control.Bounds.Height);
        }

        if (IsFileViewerTextEditor(control) && semanticParent is not null)
        {
            // Both source editors expose their named editor at the outer client origin;
            // renderer-owned text/gutter insets remain pixel evidence, not control bounds.
            return new Rect(0, 0, semanticParent.Bounds.Width, control.Bounds.Height);
        }

        if ((control.Name is "ToolStripMain" or "ToolStripFilters")
            && control.GetLogicalAncestors().OfType<Grid>().FirstOrDefault(
                ancestor => ancestor.Name == (control.Name == "ToolStripMain"
                    ? "toolStripMainHost"
                    : "toolStripFiltersHost")) is { } toolbarHost)
        {
            return GetSemanticBounds(toolbarHost, semanticParent);
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

    private static double GetWorkspaceTabContentTop(TabControl owner)
    {
        ContentPresenter contentHost = owner.GetVisualDescendants()
            .OfType<ContentPresenter>()
            .Single(presenter => presenter.Name == "PART_SelectedContentHost");
        return contentHost.TranslatePoint(default, owner)?.Y
               ?? throw new InvalidOperationException("The workspace tab content host has no measured position.");
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
                if (_root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse"
                    && selectedContent.Name is "revisionDiff" or "fileTree" or "revisionGpgInfo1" or "OutputHistoryControl")
                {
                    // These named UserControls are source controls, not tab-presenter wrappers.
                    return [selectedContent];
                }

                if (GetPropertyValue(selectedContent, "Content") is Control productContent)
                {
                    return [productContent];
                }

                return [selectedContent];
            }

            return tabItem.Content is Control content ? [content] : [];
        }

        if (control is HeaderedContentControl headeredContentControl)
        {
            // parity-scaffolding: The string Header is emitted on the owning semantic control;
            // retain its product content but omit every generated header presenter. A custom
            // HeaderTemplate may produce an untemplated TextBlock, so filtering by parent alone
            // cannot distinguish it from product content.
            return headeredContentControl.Content is Control content ? [content] : [];
        }

        if (IsPopupPresenter(control) || IsOverlayPopupHost(control))
        {
            return control.GetVisualDescendants()
                .OfType<Control>()
                .Where(child => child is MenuItem or Separator or ListBoxItem
                                || (_root.GetType().FullName == "GitUI.CommandsDialogs.FormCommit"
                                    && IsFormCommitOptionsControl(child.Name)))
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

        if (IsSparseWorkingCopyEnabledLink(control))
        {
            return [];
        }

        if (_root.GetType().FullName == "GitUI.CommandsDialogs.FormVerify"
            && control.Name == "Warnings")
        {
            // parity-scaffolding: FormVerify renders its DataGridView twin as a header plus
            // recycled ListBox rows. Both are native-rendered content of the one source grid.
            return [];
        }

        string? fieldName = GetFieldNames(control).FirstOrDefault()
                            ?? (string.IsNullOrEmpty(control.Name) ? null : control.Name);
        string? sourceType = GetSourceType(control, fieldName);
        if (IsSourceListControl(sourceType) || GetSourceTypeName(sourceType) == "PictureBox")
        {
            // parity-scaffolding: WinForms list rows and PictureBox images are native-rendered
            // content, not child controls. Avalonia's renderer children are implementation detail.
            return [];
        }

        IEnumerable<Control> children = GetCaptureChildren(control).SelectMany(ExpandSemanticChild);
        if (_root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse"
            && control.Name == "toolPanel")
        {
            // The framework-owned ToolStripContainer panels enumerate content, left,
            // right, top, bottom; AXAML declaration order is not that runtime order.
            children = children.OrderBy(child => child.Name switch
            {
                "_contentPanel" => 0,
                "_leftPanel" => 1,
                "_rightPanel" => 2,
                "_topPanel" => 3,
                "_bottomPanel" => 4,
                _ => 5,
            });
        }

        if (_root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse"
            && control.Name == "_topPanel")
        {
            // WinForms adds the scripts strip first, then filters, then the main strip.
            children = children.OrderBy(child => child.Name switch
            {
                "ToolStripScripts" => 0,
                "ToolStripFilters" => 1,
                "ToolStripMain" => 2,
                _ => 3,
            });
        }

        if (_root.GetType().FullName == "GitUI.CommandsDialogs.FormVerify")
        {
            // The named Avalonia header cells are represented through Warnings.Columns below;
            // WinForms DataGridViewColumn objects are not child controls in the source tree.
            children = children.Where(child => child.Name is not
                    ("columnIsLostObjectSelected" or "columnDate" or "columnType" or "columnSubject"
                        or "columnAuthor" or "columnHash" or "columnParent")
                && !(child is Border
                    && child.GetLogicalDescendants().OfType<Control>()
                        .Any(descendant => descendant.Name == "columnIsLostObjectSelected")));
        }

        if (ReferenceEquals(control, _root)
            && children.Any(IsInheritedFormProcessContainer))
        {
            // WinForms FormProcess adds MainPanel before the bottom ControlsPanel. Avalonia's
            // layout may keep the bottom child first, so restore source order only in the
            // semantic tree for every inherited FormProcess surface.
            children = children.OrderBy(child => child.Name switch
            {
                "MainPanel" => 0,
                "ControlsPanel" => 1,
                _ => 2
            });
        }

        if (ReferenceEquals(control, _root)
            && _root.GetType().FullName == "GitUI.CommandsDialogs.FormCommit")
        {
            children = children.Concat(GetDetachedSourceFields()).Distinct();
        }

        return children;
    }

    private IEnumerable<Control> GetDetachedSourceFields()
    {
        string rootType = GetMetadataTypeName(_root.GetType());
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

    private CaptureFont ReadUiFont()
    {
        if (!_root.TryFindResource("GitExtensionsUiFontFamily", _root.ActualThemeVariant, out object? familyResource)
            || familyResource is not FontFamily family
            || !_root.TryFindResource("GitExtensionsUiFontSize", _root.ActualThemeVariant, out object? sizeResource)
            || sizeResource is not double sizeDip)
        {
            throw new InvalidDataException("The resolved Git Extensions UI font is unavailable.");
        }

        return new CaptureFont
        {
            Family = family.Name,
            EmSize = ToDecimal(sizeDip),
            Unit = "Dip",
            SizePoints = ToDecimal(sizeDip * 72 / 96),
            SizeDip = ToDecimal(sizeDip),
            Style = ["Regular"]
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

    private CaptureThicknessPair ReadPhysicalThicknessPair(Thickness value)
        => new()
        {
            Px = new CaptureThickness
            {
                Left = checked((int)Math.Round(value.Left, MidpointRounding.AwayFromZero)),
                Top = checked((int)Math.Round(value.Top, MidpointRounding.AwayFromZero)),
                Right = checked((int)Math.Round(value.Right, MidpointRounding.AwayFromZero)),
                Bottom = checked((int)Math.Round(value.Bottom, MidpointRounding.AwayFromZero))
            },
            Dip = new CaptureThicknessF
            {
                Left = ToDecimal(value.Left / _renderScale),
                Top = ToDecimal(value.Top / _renderScale),
                Right = ToDecimal(value.Right / _renderScale),
                Bottom = ToDecimal(value.Bottom / _renderScale)
            }
        };

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
        if (control is ListBox { Name: "Patches" }
            && control.GetLogicalAncestors().OfType<PatchGrid>().FirstOrDefault() is { } patchGrid)
        {
            return ReadPatchGridColumns(patchGrid);
        }

        if (_root.GetType().FullName == "GitUI.CommandsDialogs.FormVerify"
            && control.Name == "Warnings")
        {
            return ReadFormVerifyColumns();
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

        if (GetPropertyValue(control, "Columns") is IEnumerable columns)
        {
            return ReadFrameworkColumns(columns, control);
        }

        return [];
    }

    private IReadOnlyList<CaptureColumn> ReadPatchGridColumns(PatchGrid patchGrid)
    {
        Grid columnsGrid = patchGrid.GetLogicalDescendants().OfType<Grid>().Single(grid => grid.Name == "columnsGrid");
        bool isManagingRebase = patchGrid.IsManagingRebase;
        string[] fieldNames =
        [
            "Status",
            "Action",
            "FileName",
            "subjectDataGridViewTextBoxColumn",
            "authorDataGridViewTextBoxColumn",
            "dateDataGridViewTextBoxColumn",
            "CommitHash"
        ];
        string[] headerTexts = ["Status", "Action", "Name", "Subject", "Author", "Date", "Commit hash"];
        bool[] visible = [true, isManagingRebase, !isManagingRebase, true, true, true, isManagingRebase];
        double fixedWidth = isManagingRebase ? 45 + 48 + 140 + 37 + 85 : 45 + 50 + 140 + 37;
        double subjectWidth = Math.Max(0, columnsGrid.Bounds.Width - fixedWidth - 1);
        double[] widths = isManagingRebase
            ? [45, 48, 50, subjectWidth, 140, 37, 85]
            : [45, 100, 50, subjectWidth, 140, 37, 55];
        CaptureColors colors = ReadSourceDataGridColumnColors();

        return fieldNames.Select((fieldName, index) => new CaptureColumn
        {
            FieldName = fieldName,
            Name = fieldName,
            Type = "System.Windows.Forms.DataGridViewTextBoxColumn",
            Index = index,
            DisplayIndex = index,
            WidthPx = ToPixel(widths[index]),
            WidthDip = ToDecimal(widths[index]),
            Visible = visible[index],
            Resizable = true,
            SortMode = "NotSortable",
            Alignment = "NotSet",
            HeaderText = headerTexts[index],
            HeaderAlignment = "NotSet",
            Colors = colors
        }).ToArray();
    }

    private IReadOnlyList<CaptureColumn> ReadFormVerifyColumns()
    {
        (string Name, string Type, double Width, string SortMode, string Alignment, string Header)[] columns =
        [
            ("columnIsLostObjectSelected", "DataGridViewCheckBoxColumn", 21, "NotSortable", "MiddleCenter", string.Empty),
            ("columnDate", "DataGridViewTextBoxColumn", 112, "Automatic", "NotSet", "Date"),
            ("columnType", "DataGridViewTextBoxColumn", 164, "Automatic", "NotSet", "Type"),
            ("columnSubject", "DataGridViewTextBoxColumn", 130, "Automatic", "MiddleLeft", "Subject"),
            ("columnAuthor", "DataGridViewTextBoxColumn", 150, "Automatic", "NotSet", "Author"),
            ("columnHash", "DataGridViewTextBoxColumn", 60, "Automatic", "NotSet", "Hash"),
            ("columnParent", "DataGridViewTextBoxColumn", 60, "Automatic", "NotSet", "Parent(s) hashs"),
        ];
        CaptureColors colors = ReadSourceDataGridColumnColors();
        return columns.Select((column, index) => new CaptureColumn
        {
            FieldName = column.Name,
            Name = column.Name,
            Type = $"System.Windows.Forms.{column.Type}",
            Index = index,
            DisplayIndex = index,
            WidthPx = ToPixel(column.Width),
            WidthDip = ToDecimal(column.Width),
            Visible = true,
            Resizable = true,
            SortMode = column.SortMode,
            Alignment = column.Alignment,
            HeaderText = column.Header,
            HeaderAlignment = "NotSet",
            Colors = colors
        }).ToArray();
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
        => GetMetadataTypeName(_root.GetType()) is
            "GitUI.CommandsDialogs.BrowseDialog.FormGoToCommit" or
            "GitUI.CommandsDialogs.BrowseDialog.FormGitCommandLog" or
            "GitUI.CommandsDialogs.FormApplyPatch" or
            "GitUI.CommandsDialogs.FormArchive" or
            "GitUI.CommandsDialogs.FormCheckoutRevision" or
            "GitUI.CommandsDialogs.FormCherryPick" or
            "GitUI.CommandsDialogs.FormClone" or
            "GitUI.CommandsDialogs.FormCommit" or
            "GitUI.CommandsDialogs.FormCompareToBranch" or
            "GitUI.CommandsDialogs.FormCreateBranch" or
            "GitUI.CommandsDialogs.FormDeleteBranch" or
            "GitUI.CommandsDialogs.FormDiff" or
            "GitUI.CommandsDialogs.FormFormatPatch" or
            "GitUI.CommandsDialogs.FormGitAttributes" or
            "GitUI.CommandsDialogs.FormGitIgnore" or
            "GitUI.CommandsDialogs.FormAbout" or
            "GitUI.CommandsDialogs.FormMailMap" or
            "GitUI.CommandsDialogs.FormLog" or
            "GitUI.CommandsDialogs.FormInit" or
            "GitUI.CommandsDialogs.FormMergeBranch" or
            "GitUI.CommandsDialogs.FormRebase" or
            "GitUI.CommandsDialogs.FormRenameBranch" or
            "GitUI.CommandsDialogs.FormSettings" or
            "GitUI.CommandsDialogs.FormSparseWorkingCopy" or
            "GitUI.CommandsDialogs.EnvironmentInfo" or
            "GitUI.CommandsDialogs.AboutBoxDialog.FormContributors"
            or "GitUI.CommandsDialogs.SearchControl"
            or "GitUI.CommandsDialogs.SearchWindow"
            or "GitUI.CommandsDialogs.RepoHosting.CreatePullRequestForm"
            or "GitUI.CommandsDialogs.RepoHosting.ForkAndCloneForm"
            or "GitUI.CommandsDialogs.RepoHosting.ViewPullRequestsForm"
            or "GitUI.Editor.FileViewer"
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
            && (control.Name is "_NO_TRANSLATE_WorkingDir" or "branchSelect" or "menuCommitInfoPosition"
               or "RefreshButton" or "toggleLeftPanel" or "toggleSplitViewLayout"
               or "toolStripButtonLevelUp" or "toolStripButtonPull" or "toolStripSeparator0"
               or "toolStripSeparator1" or "toolStripSeparator17"
               or "toolStripWorktrees"
               or "tsddbtnRevisionFilter"
               || (control.Name is "toolStripFileExplorer" or "userShell" or "EditSettings"
                   && IsInsideClippedAncestors(control))
               || (control.Name is "toolStripSplitStash" or "toolStripSeparator2"
                   && IsInsideClippedAncestors(control))
               || control.Name?.StartsWith("pull_shortcut_", StringComparison.Ordinal) == true))
           || (_root.GetType().FullName is "GitUI.CommandsDialogs.FormDiff" or "GitUI.CommandsDialogs.FormLog"
               && IsFileStatusToolbarItem(control)
                && control.Name is not ("btnCollapseGroups" or "btnRefresh" or "sepRefresh"))
           || (IsViewPullRequestsTree(control)
               && IsFileStatusToolbarItem(control)
               && control.Name is not ("btnCollapseGroups" or "btnRefresh" or "sepRefresh"));

    private bool IsWindowBackgroundToolStripItem(Control control)
    {
        if ((_root.GetType().FullName is "GitUI.CommandsDialogs.FormDiff" or "GitUI.CommandsDialogs.FormLog"
             || IsViewPullRequestsTree(control))
            && IsFileStatusToolbarItem(control))
        {
            return false;
        }

        if (control.Name == "sepRefresh")
        {
            // The source's inactive Diff/Tree tab retains the Control ambient color;
            // only the selected Diff page resolves its separator against Window.
            return _root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse"
                   && control.GetLogicalAncestors().OfType<TabItem>()
                       .Any(tab => tab.IsSelected && tab.Name != "TreeTabPage");
        }

        if ((control.Name is "btnCollapseGroups" or "btnRefresh")
            && control.GetLogicalAncestors().OfType<TabItem>().Any(tab => tab.Name == "TreeTabPage"))
        {
            return IsSelectedBrowseTreeToolbarItem(control);
        }

        return control.Name is "tscboBranchFilter" or "tstxtRevisionFilter"
               || IsFileStatusToolbarProductItem(control);
    }

    private bool IsSelectedBrowseTreeToolbarItem(Control control)
        => _root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse"
           && control.GetLogicalAncestors().OfType<TabItem>()
               .Any(tab => tab.Name == "TreeTabPage" && tab.IsSelected);

    private bool IsFileStatusToolbarProductItem(Control control)
        => IsFileStatusToolbarProductItemName(control.Name)
           || GetFieldNames(control).Any(IsFileStatusToolbarProductItemName);

    private static bool IsFileStatusToolbarProductItemName(string? fieldName)
        => fieldName is
            "btnCollapseGroups" or "sepRefresh" or "btnRefresh" or "sepAsTree" or "btnAsTree"
            or "sepGroupBy" or "btnByPath" or "btnByExtension" or "btnByStatus" or "sepFilter"
            or "btnUnequalChange" or "btnOnlyB" or "btnOnlyA" or "btnSameChange" or "sepOptions"
            or "btnFindInFilesGitGrep" or "sepSettings" or "btnSettings";

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
                "GitUI.CommandsDialogs.FormGitAttributes" or
                "GitUI.CommandsDialogs.FormGitIgnore" or
                "GitUI.CommandsDialogs.FormMailMap" or
                "GitUI.CommandsDialogs.FormLog")
               && IsSemanticToolStripItem(control))
           || (_root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse"
           && (control.Name is "toolStripLabel1" or "tsbShowReflog" or "tsbtnAdvancedFilter"
                or "tsddbtnBranchFilter" or "tslblRevisionFilter" or "tsmiShowOnlyFirstParent"
                or "tssbtnShowBranches"
                or "toolStripButtonPush" or "toolStripButtonCommit"
                || (control.Name is "toolStripFileExplorer" or "userShell" or "EditSettings"
                    && !IsInsideClippedAncestors(control))
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
        Control? editor = control.GetLogicalAncestors()
            .OfType<Control>()
            .FirstOrDefault(ancestor => ancestor.GetType().FullName == "GitUI.SpellChecker.EditNetSpell");
        if (control is not TextBox textBox
            || !string.IsNullOrEmpty(textBox.Text)
            || (editor is not null && !string.IsNullOrEmpty(GetPropertyValue(editor, "Text") as string))
            || textBox.IsKeyboardFocusWithin
            || !IsInSelectedTab(control))
        {
            return false;
        }

        return textBox.Classes.Contains("winforms-watermark");
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
            && control.Name is "_refreshCommentsBtn" or "_postComment" or "_postCommentText"
                or "tableLayoutPanel1" or "flowLayoutPanel1")
           || (control.GetLogicalAncestors().Any(
                   ancestor => ancestor.GetType().FullName == "GitUI.CommandsDialogs.RepoHosting.ForkAndCloneForm")
               && control.Name is "searchBtn" or "getFromUserBtn" or "forkBtn" or "openGitupPageBtn"
                   or "tableLayoutPanel1" or "tableLayoutPanel3" or "tableLayoutPanel4" or "tableLayoutPanel5"
                   or "flowLayoutPanel2" or "helpTextLbl" or "orLbl" or "descriptionLbl");

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
            && GetFieldNames(control).Count == 0
            && control.GetType().FullName != "GitUI.Compat.WinFormsControls.TableLayoutPanel")
           || ((GetMetadataTypeName(_root.GetType()) == "GitUI.CommandsDialogs.SearchControl"
                || control.GetLogicalAncestors().Any(
                    ancestor => GetMetadataTypeName(ancestor.GetType()) == "GitUI.CommandsDialogs.SearchControl"))
               && control is Border
               && string.IsNullOrEmpty(control.Name))
           || (GetMetadataTypeName(_root.GetType()) == "GitUI.CommandsDialogs.SearchWindow"
               && control is ContentControl
               && string.IsNullOrEmpty(control.Name)
               && GetFieldNames(control).Count == 0)
           || (_root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse"
               && (control.Name is "toolStripMainHost" or "toolStripMainViewport"
                    or "toolStripFiltersHost" or "toolStripFiltersViewport"
                    or "mainContentGrid" or "leftPanel"
                    or "commitInfoLeftHost" or "commitInfoRightHost"
                    or "commitInfoBelowHost" or "outputHistoryPanelHost"
                   || GetFieldNames(control).Contains("_filterHost", StringComparer.Ordinal)))
           || (_root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse"
               && string.IsNullOrEmpty(control.Name)
               && (control.Parent?.Name == "tableLayoutPanel1"
                   || control.Parent?.Name == "DiffSplitContainer"
                   || (control.Parent?.Name == "RevisionInfo"
                       && control.GetType().FullName != "GitUI.Compat.WinFormsControls.TableLayoutPanel")))
           || (control is Grid
                 && string.IsNullOrEmpty(control.Name)
                 && control.Parent?.GetType().FullName == "GitUI.Compat.WinFormsControls.FlowLayoutPanel")
           || (control is StackPanel
                && string.IsNullOrEmpty(control.Name)
                && control.Parent?.GetType().FullName == "GitUI.Compat.WinFormsControls.FlowLayoutPanel")
           || (_root.GetType().FullName == "GitUI.CommandsDialogs.FormCherryPick"
               && control is Grid { Name: "parentsPanel" })
           || (_root.GetType().FullName == "GitUI.CommandsDialogs.FormCherryPick"
               && control is Border or Grid
               && string.IsNullOrEmpty(control.Name)
               && control.GetLogicalAncestors().OfType<Control>().Any(ancestor => ancestor.Name == "parentsPanel")
               && control.GetLogicalDescendants().OfType<Control>().Any(descendant => descendant.Name == "lvParentsList"))
           || (_root.GetType().FullName == "GitUI.SpellChecker.EditNetSpell"
               && control is Grid or Canvas
               && string.IsNullOrEmpty(control.Name))
           || control.Name == "columnsGrid"
           || (_root.GetType().FullName == "GitUI.CommandsDialogs.FormCommit"
               && control.Name == "toolbarCommitInlineItems")
           || control.Name == "FindInCommitFilesGitGrepPanel"
           || (control is Grid
               && string.IsNullOrEmpty(control.Name)
               && control.Parent?.GetType().FullName == "GitUI.Editor.FileViewer")
           || (control is StackPanel
               && control.Parent is Control parent
               && IsFileViewerToolbar(parent));

    private bool IsRendererOnlyControl(Control control)
        => control.Name == "ImagePreview"
           || (_root.GetType().FullName == "GitUI.CommandsDialogs.FormCherryPick"
               && control is Border or Grid or TextBlock
               && string.IsNullOrEmpty(control.Name)
               && control.GetLogicalAncestors().OfType<Control>().Any(ancestor => ancestor.Name == "parentsPanel")
               && !control.GetLogicalDescendants().OfType<Control>().Any(descendant => descendant.Name == "lvParentsList"))
           || control.Name is "toolStripMainOverflow" or "toolStripFiltersOverflow"
           || (_root.GetType().FullName == "GitUI.CommandsDialogs.FormCommit"
               && control.Name is "toolbarCommitOverflow" or "commitTemplatesOverflowMenuItem"
                   or "createBranchOverflowMenuItem")
           || (_root.GetType().FullName == "GitUI.CommandsDialogs.FormCommit"
               && control is GridSplitter)
           || (_root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse"
               && (control is GridSplitter
                   || (control is Border
                       && control.Parent?.Name == "RevisionsSplitContainer"
                       && control.Classes.Contains("gitextensions-revision-split-frame"))
                   || (control is Border
                       && (control.Classes.Contains("gitextensions-workspace-page-frame")
                           || control.Classes.Contains("gitextensions-toolbar-separator-line")))
                   || control.Name is "lblRepoPath" or "lblStatus"
                   || control.Parent is TreeView))
           || (control.GetType().Namespace == "GitUI.Compat.WinFormsControls"
                && (control.GetType().Name == "ColumnHeader"
                    || control.GetType().Name.EndsWith("Column", StringComparison.Ordinal)))
           || (control is Border
               && control.Parent is Grid { Name: "columnsGrid" }
               && control.GetLogicalAncestors().OfType<PatchGrid>().Any())
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

    private static bool IsFormCommitOptionsControl(string? name)
        => IsFormCommitOptionsItem(name)
           || name is "toolAuthor" or "gpgSignCommitToolStripComboBox" or "toolStripGpgKeyTextBox"
               or "toolStripSeparator2" or "toolStripSeparator14";

    private static bool TryGetFormCommitOptionsBounds(string? name, out Rect bounds)
    {
        bounds = name switch
        {
            "closeDialogAfterEachCommitToolStripMenuItem" => new Rect(0, 2, 314, 22),
            "closeDialogAfterAllFilesCommittedToolStripMenuItem" => new Rect(0, 24, 314, 22),
            "refreshDialogOnFormFocusToolStripMenuItem" => new Rect(0, 46, 314, 22),
            "tsmiSelectStagedOnEnterMessage" => new Rect(0, 68, 314, 22),
            "toolStripSeparator2" => new Rect(2, 90, 311, 6),
            "signOffToolStripMenuItem" => new Rect(0, 96, 314, 22),
            "toolAuthorLabelItem" => new Rect(0, 118, 314, 22),
            "toolAuthor" => new Rect(34, 141, 230, 23),
            "noVerifyToolStripMenuItem" => new Rect(0, 165, 314, 22),
            "toolStripSeparator14" => new Rect(2, 187, 311, 6),
            "gpgSignCommitToolStripComboBox" => new Rect(35, 195, 230, 23),
            "toolStripGpgKeyTextBox" => new Rect(34, 221, 230, 23),
            _ => default,
        };
        return IsFormCommitOptionsControl(name);
    }

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

    private bool IsFormBrowseDynamicMainMenu(Control control)
        => _root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse"
           && control is MenuItem { Name: "navigateToolStripMenuItem" or "viewToolStripMenuItem" }
           && control.GetLogicalAncestors().OfType<Menu>().Any(menu => menu.Name == "mainMenuStrip");

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
               && semanticStateControl.Opacity > 0
               && control.GetLogicalAncestors().OfType<Control>().All(
                   ancestor => ancestor.IsVisible && ancestor.Opacity > 0);
    }

    private static bool IsInsideClippedAncestors(Control control)
    {
        foreach (Control ancestor in control.GetVisualAncestors().OfType<Control>().Where(ancestor => ancestor.ClipToBounds))
        {
            if (control.TranslatePoint(default, ancestor) is not Point origin)
            {
                return false;
            }

            Rect clippedBounds = new(ancestor.Bounds.Size);
            Rect controlBounds = new(origin, control.Bounds.Size);
            if (!clippedBounds.Contains(controlBounds.TopLeft)
                || !clippedBounds.Contains(new Point(
                    Math.Max(controlBounds.Left, controlBounds.Right - 0.01),
                    Math.Max(controlBounds.Top, controlBounds.Bottom - 0.01))))
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

    private CaptureColors? ReadNavigationEditorSemanticColors(
        Control control,
        string sourceOwnerType,
        string? fieldName,
        bool isSurfaceRoot)
    {
        string rootType = GetMetadataTypeName(_root.GetType());
        string? name = fieldName ?? control.Name;
        if (sourceOwnerType == "GitUI.UserControls.CommitPickerSmallControl" && name == "lbCommits")
        {
            return ReadColors(control) with
            {
                Foreground = ResolveResourceArgb("GitExtensionsKnownColorGrayTextBrush")
                             ?? ResolveResourceArgb("GitExtensionsDisabledForegroundBrush")
            };
        }

        if ((rootType == "GitUI.CommandsDialogs.SearchControl" && isSurfaceRoot)
            || (sourceOwnerType == "GitUI.CommandsDialogs.SearchControl" && name == "tableLayoutPanel1"))
        {
            return ReadTransparentContainerColors(control);
        }

        if (rootType == "GitUI.CommandsDialogs.SearchWindow"
            && (isSurfaceRoot
                || (sourceOwnerType == "GitUI.CommandsDialogs.SearchWindow" && name == "tableLayoutPanel1")))
        {
            CaptureColors colors = ReadColors(control);
            return colors with
            {
                Foreground = ResolveSourceControlTextArgb(),
                Background = "#FF00FF00",
                Border = null,
                DisabledBackground = "#FF00FF00"
            };
        }

        bool isEditorDialog = rootType is
            "GitUI.CommandsDialogs.FormGitAttributes" or
            "GitUI.CommandsDialogs.FormGitIgnore" or
            "GitUI.CommandsDialogs.FormMailMap";
        if (!isEditorDialog || isSurfaceRoot)
        {
            return null;
        }

        if (rootType == "GitUI.CommandsDialogs.FormGitIgnore" && name == "label1")
        {
            string? background = ResolveResourceArgb("GitExtensionsKnownColorControlBrush");
            return EmptyColors() with
            {
                Foreground = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush"),
                Background = background,
                DisabledForeground = ResolveResourceArgb("GitExtensionsDisabledForegroundBrush"),
                DisabledBackground = background
            };
        }

        bool isTransparent = name is
            "splitContainer1" or
            "_NO_TRANSLATE_GitAttributesText" or
            "_NO_TRANSLATE_GitIgnoreEdit" or
            "_NO_TRANSLATE_MailMapText" or
            "internalFileViewer" or
            "_NO_TRANSLATE_lblShowPreview"
            || (rootType == "GitUI.CommandsDialogs.FormGitIgnore"
                && name is "flowLayoutPanel2" or "panel1" or "AddDefault" or "AddPattern"
                    or "lnkGitIgnoreGenerate" or "lnkGitIgnorePatterns")
            || (rootType is "GitUI.CommandsDialogs.FormGitAttributes" or "GitUI.CommandsDialogs.FormMailMap"
                && name is "label1" or "Save");
        return isTransparent ? ReadTransparentContainerColors(control) : null;
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

        if (ReferenceEquals(control, _root))
        {
            return ReadSourceBackgroundColors(
                control,
                "GitExtensionsWindowBackgroundBrush",
                ResolveResourceArgb("GitExtensionsWindowTextBrush"));
        }

        if (IsPopupSurface(control))
        {
            return ReadSourceBackgroundColors(
                control,
                "GitExtensionsKnownColorControlBrush",
                ResolveSourceControlTextArgb());
        }

        if (name == "repoTreePanel")
        {
            return ReadColors(control) with
            {
                Foreground = ResolveSourceControlTextArgb()
            };
        }

        if (name is "toolStripButtonPush" or "toolStripButtonCommit")
        {
            CaptureColors colors = ReadToolStripColors(
                control,
                isItem: true,
                transparentBackground: true);
            return colors with
            {
                Foreground = ResolveSourceControlTextArgb()
            };
        }

        if (name == "_topPanel")
        {
            return ReadToolStripColors(control, isItem: false, transparentBackground: true);
        }

        if (name is "_bottomPanel" or "_leftPanel" or "_rightPanel")
        {
            return ReadSourceBackgroundColors(
                control,
                "GitExtensionsWindowBackgroundBrush",
                ResolveSourceControlTextArgb());
        }

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
                : "GitExtensionsControlBackgroundBrush");
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
            or "splitContainer1" or "splitContainer2" or "BlameControl" or "BlameAuthor"
            or "BlameFile" or "_NO_TRANSLATE_lblShowPreview")
        {
            return ReadTransparentContainerColors(control);
        }

        if (sourceOwnerType is "GitUI.CommitInfo.CommitInfo" or "GitUI.CommitInfo.CommitInfoHeader"
            && name is "rtbRevisionHeader" or "RevisionInfo")
        {
            return ReadSourceBackgroundColors(
                control,
                "GitExtensionsWindowBackgroundBrush",
                ResolveSourceControlTextArgb());
        }

        if (sourceOwnerType == "GitUI.CommitInfo.CommitInfo" && name == "rtbxCommitMessage")
        {
            return ReadSourceBackgroundColors(
                control,
                "GitExtensionsCommitMessageBackgroundBrush",
                ResolveSourceControlTextArgb());
        }

        if (name is "_avatarImage" or "pnlCommitMessage" or "tableLayout" or "TextLabel"
            or "IconBox" or "ButtonContainer")
        {
            CaptureColors colors = ReadColors(control);
            return colors with { Foreground = ResolveSourceControlTextArgb() };
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
                name == "lblSplitter"
                    ? "GitExtensionsFileStatusSplitterBrush"
                    : "GitExtensionsPaneBorderBrush",
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

        if (name == "toolStripGpgKeyTextBox")
        {
            return AddSourceToolStripSelectionColors(ReadSourceBackgroundColors(
                control,
                "GitExtensionsWindowBackgroundBrush",
                ResolveResourceArgb("GitExtensionsWindowTextBrush"))) with
            {
                Additional = new SortedDictionary<string, string>(StringComparer.Ordinal)
            };
        }

        if (name == "toolAuthor")
        {
            CaptureColors colors = TopLevel.GetTopLevel(control) is null
                ? ReadSourceBackgroundColors(
                    control,
                    "GitExtensionsWindowBackgroundBrush",
                    ResolveResourceArgb("GitExtensionsWindowTextBrush"))
                : ReadSourceInputColors(control);
            return AddSourceToolStripSelectionColors(colors);
        }

        if (name == "gpgSignCommitToolStripComboBox")
        {
            return ReadToolStripColors(
                control,
                isItem: true,
                transparentBackground: false,
                useWindowText: true);
        }

        if (name == "toolStripProgressBar1")
        {
            CaptureColors colors = ReadSourceBackgroundColors(
                control,
                "GitExtensionsKnownColorControlBrush",
                ResolveResourceArgb("GitExtensionsDataGridViewSelectionBackgroundBrush"));
            return AddSourceToolStripSelectionColors(colors);
        }

        if (name is "LoadingFiles" or "NoFiles")
        {
            return ReadSourceBackgroundColors(
                control,
                "GitExtensionsPanelBackgroundBrush",
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
            Foreground = isDesignerLink
                ? ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")
                  ?? ResolveSourceControlTextArgb()
                  ?? ResolveResourceArgb("GitExtensionsControlForegroundBrush")
                : isPreviewLink || isHelpLink
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
        // A WinForms control without an explicit BackColor inherits its parent's resolved
        // color. Do not let an Avalonia implicit style on the substitute control masquerade
        // as a source-authored color.
        string? background = designerLayout?.HasExplicitBackground == true
            ? colors.Background
            : ResolveSourceAncestorBackground(control, "GitExtensionsControlBackgroundBrush");
        return colors with
        {
            Foreground = designerLayout?.HasExplicitForeground == true
                ? colors.Foreground
                : ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")
                  ?? ResolveSourceControlTextArgb()
                  ?? ResolveResourceArgb("GitExtensionsControlForegroundBrush"),
            Background = background,
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
        if (control.Name == "btnRemoteColor")
        {
            background = BrushToArgb(GetPropertyValue(control, "Color"));
        }

        return colors with
        {
            Foreground = ResolveResourceArgb("GitExtensionsKnownColorControlTextBrush")
                         ?? ResolveSourceControlTextArgb()
                         ?? ResolveResourceArgb("GitExtensionsControlForegroundBrush"),
            Background = background,
            Border = null,
            DisabledBackground = background,
            DisabledForeground = ResolveResourceArgb("GitExtensionsKnownColorGrayTextBrush")
                                 ?? ResolveResourceArgb("GitExtensionsDisabledForegroundBrush"),
            SelectionForeground = null,
            SelectionBackground = null,
            InactiveSelectionForeground = null,
            InactiveSelectionBackground = null
        };
    }

    private CaptureColors ReadChecklistStatusButtonColors(Control control)
    {
        CaptureColors colors = ReadColors(control);
        string? background = control.Name == "GcmDetected"
            ? ResolveResourceArgb("GitExtensionsKnownColorControlDarkBrush")
            : BrushToArgb(GetPropertyValue(control, "Background"));
        background ??= ResolveSourceAncestorBackground(control, "GitExtensionsControlBackgroundBrush");
        return colors with
        {
            Foreground = BrushToArgb(GetPropertyValue(control, "Foreground")) ?? colors.Foreground,
            Background = background,
            Border = null,
            SelectionForeground = null,
            SelectionBackground = null,
            InactiveSelectionForeground = null,
            InactiveSelectionBackground = null,
            DisabledForeground = ResolveResourceArgb("GitExtensionsKnownColorGrayTextBrush")
                                 ?? ResolveResourceArgb("GitExtensionsDisabledForegroundBrush"),
            DisabledBackground = background,
            Additional = new SortedDictionary<string, string>(StringComparer.Ordinal)
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

    private CaptureColors ReadInheritedFormProcessColors(Control control)
    {
        CaptureColors colors = ReadColors(control);
        string backgroundResource = control.Name == "ControlsPanel"
            ? "GitExtensionsDialogControlsBackgroundBrush"
            : "GitExtensionsPanelBackgroundBrush";
        string? background = ResolveResourceArgb(backgroundResource);
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
        string backgroundResource = usesWindowBackground
            ? "GitExtensionsKnownColorWindowBrush"
            : control.Name == "BranchGrid"
                ? "GitExtensionsKnownColorControlDarkBrush"
                : "GitExtensionsKnownColorControlLightBrush";
        string? background = ResolveResourceArgb(backgroundResource);
        return new CaptureColors
        {
            Foreground = ResolveResourceArgb("GitExtensionsKnownColorWindowTextBrush"),
            Background = background,
            Border = ResolveResourceArgb("GitExtensionsKnownColorControlBrush"),
            SelectionForeground = ResolveResourceArgb("GitExtensionsKnownColorHighlightTextBrush"),
            SelectionBackground = ResolveResourceArgb("GitExtensionsKnownColorHighlightBrush"),
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
        bool usesWindowBackground = _root.GetType().FullName == "GitUI.CommandsDialogs.FormMergeBranch"
            && control.Name is "nbMessages" or "mergeMessage";
        bool isGpgInfoTextBox = control.Name is ("txtCommitGpgInfo" or "txtTagGpgInfo")
            && control.GetLogicalAncestors().Any(ancestor => ancestor.GetType().FullName == "GitUI.CommandsDialogs.RevisionGpgInfoControl");
        string? background = usesWindowBackground
            ? ResolveResourceArgb("GitExtensionsKnownColorWindowBrush")
            : isGpgInfoTextBox || control.Name is ("searchTB" or "searchResultItemDescription")
            ? BrushToArgb(GetPropertyValue(control, "Background"))
            : isReadOnly
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

    private CaptureColors ReadStandaloneSourceInputColors(Control control)
    {
        CaptureColors resolvedColors = ReadColors(control);
        return ReadSourceInputColors(control) with
        {
            // The surface root owns the theme-wide semantic palette in both capture harnesses.
            Additional = resolvedColors.Additional
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

    private static string? NormalizeText(string? value)
        => value?.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');

    private bool IsHiddenFormBrowseDiffToolbarItem(Control control, string? semanticName)
        => _root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse"
           && semanticName is "btnRefresh" or "tsmiShowSkipWorktreeFiles" or "tsmiShowUntrackedFiles"
           && _fieldOwners.GetValueOrDefault(control) is Control owner
           && owner.GetLogicalAncestors().OfType<TabItem>().Any(
               tab => tab.Name == "DiffTabPage" && !tab.IsSelected);

    private static bool IsDetachedMenuItem(Control control)
        => control is MenuItem or Separator && TopLevel.GetTopLevel(control) is null;

    private static bool IsNativeListView(Control control)
        => control is ListBox list && list.Classes.Contains("gitextensions-native-list-items");

    private bool IsInheritedFormProcessContainer(Control control)
        => (_root.GetType().FullName is
               "GitUI.CommandsDialogs.FormCheckoutRevision" or
               "GitUI.CommandsDialogs.FormPull" or
               "GitUI.CommandsDialogs.FormDeleteRemoteBranch" or
               "GitUI.CommandsDialogs.FormCherryPick" or
               "GitUI.CommandsDialogs.FormClone" or
               "GitUI.CommandsDialogs.FormInit" or
               "GitUI.CommandsDialogs.FormRebase")
           && (control.Name is "MainPanel" or "ControlsPanel");

    private bool IsSparseWorkingCopyEnabledLink(Control control)
        => _root.GetType().FullName == "GitUI.CommandsDialogs.FormSparseWorkingCopy"
           && control is StackPanel { Orientation: Orientation.Horizontal }
           && control.GetLogicalChildren().OfType<HyperlinkButton>().Any();

    private Thickness GetInheritedFormProcessPadding(Control control)
        => new(control.Name == "MainPanel"
            ? _root.GetType().FullName is "GitUI.CommandsDialogs.FormPull" or "GitUI.CommandsDialogs.FormRebase" ? 9 : 12
            : 5);

    private bool IsEditorDialog()
        => _root.GetType().FullName is
            "GitUI.CommandsDialogs.FormGitAttributes" or
            "GitUI.CommandsDialogs.FormGitIgnore" or
            "GitUI.CommandsDialogs.FormMailMap";

    private bool IsHiddenEditorEncodingSelector(Control control)
        => IsEditorDialog()
           && control.Name == "encodingToolStripComboBox"
           && !control.IsEffectivelyVisible;

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
        if (_root.GetType().FullName == "GitUI.CommandsDialogs.FormBrowse"
            && control.Name == "CommitInfoTabControl")
        {
            // WinForms TabControl.Focused excludes the focused control in its selected page.
            return control.IsFocused;
        }

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

        if (IsNativeListView(control) || IsNativeTabControl(control)
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
