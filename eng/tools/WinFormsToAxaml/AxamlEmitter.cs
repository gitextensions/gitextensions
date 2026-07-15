using System.Text;

namespace WinFormsToAxaml;

/// <summary>
///  Emits a deterministic <c>.axaml</c> scaffold from a parsed Designer form: same input
///  produces byte-identical output, every named control keeps its field name as
///  <c>x:Name</c>, event hookups keep their handler names, and everything that has no
///  mechanical mapping is preserved as a <c>TODO:WinForms</c> comment.
/// </summary>
internal sealed class AxamlEmitter(DesignerForm form, string sourceDescription, bool asUserControl)
{
    private static readonly Dictionary<string, string> _controlMap = new()
    {
        ["Label"] = "TextBlock",
        ["Button"] = "Button",
        ["TextBox"] = "TextBox",
        ["MaskedTextBox"] = "TextBox",
        ["RichTextBox"] = "TextBox",
        ["CheckBox"] = "CheckBox",
        ["RadioButton"] = "RadioButton",
        ["ComboBox"] = "ComboBox",
        ["ListBox"] = "ListBox",
        ["ListView"] = "ListBox",
        ["TreeView"] = "TreeView",
        ["NumericUpDown"] = "NumericUpDown",
        ["LinkLabel"] = "HyperlinkButton",
        ["PictureBox"] = "Image",
        ["ProgressBar"] = "ProgressBar",
        ["GroupBox"] = "HeaderedContentControl",
        ["Panel"] = "StackPanel",
        ["TableLayoutPanel"] = "Grid",
        ["FlowLayoutPanel"] = "StackPanel",
        ["SplitContainer"] = "Grid",
        ["MenuStrip"] = "Menu",
        ["ContextMenuStrip"] = "ContextMenu",
        ["ToolStripMenuItem"] = "MenuItem",
        ["ToolStripSeparator"] = "Separator",
        ["ToolStrip"] = "StackPanel",
        ["StatusStrip"] = "StackPanel",
        ["ToolStripButton"] = "Button",
        ["ToolStripLabel"] = "TextBlock",
        ["TabControl"] = "TabControl",
        ["TabPage"] = "TabItem",
    };

    /// <summary>WinForms property → identical-meaning Avalonia property, applicable to any control.</summary>
    private static readonly Dictionary<string, string> _propertyMap = new()
    {
        ["Enabled"] = "IsEnabled",
        ["Visible"] = "IsVisible",
        ["ReadOnly"] = "IsReadOnly",
        ["Multiline"] = "AcceptsReturn",
        ["Checked"] = "IsChecked",
        ["ThreeState"] = "IsThreeState",
        ["Minimum"] = "Minimum",
        ["Maximum"] = "Maximum",
        ["Value"] = "Value",
        ["DecimalPlaces"] = "FormatString",
        ["MaxLength"] = "MaxLength",
        ["UseSystemPasswordChar"] = "RevealPassword",
    };

    /// <summary>Layout machinery and Windows-only appearance settings with no meaning in the scaffold.</summary>
    private static readonly HashSet<string> _droppedProperties =
    [
        "Name", "Location", "Size", "AutoSize", "AutoSizeMode", "TabStop", "UseVisualStyleBackColor",
        "Anchor", "CausesValidation", "ImeMode", "WrapContents", "ColumnCount", "RowCount",
        "AutoScaleDimensions", "AutoScaleMode", "SizeGripStyle", "ShowIcon", "ShowInTaskbar",
        "AutoValidate", "FlatStyle", "UseMnemonic", "UseCompatibleTextRendering", "DoubleBuffered",
        "BorderStyle", "FormattingEnabled", "ItemHeight", "FullRowSelect", "HideSelection",
        "AllowDrop", "AutoScroll", "Dock", "FlowDirection", "CheckState", "TabIndex",
        "CheckAlign", "ImageAlign", "TextImageRelation", "AutoEllipsis", "SizeMode",
    ];

    private static readonly Dictionary<string, string> _eventMap = new()
    {
        ["Click"] = "Click",
        ["CheckedChanged"] = "IsCheckedChanged",
        ["TextChanged"] = "TextChanged",
        ["SelectedIndexChanged"] = "SelectionChanged",
        ["SelectedValueChanged"] = "SelectionChanged",
        ["Leave"] = "LostFocus",
        ["Enter"] = "GotFocus",
        ["KeyDown"] = "KeyDown",
        ["KeyUp"] = "KeyUp",
        ["DoubleClick"] = "DoubleTapped",
        ["MouseDoubleClick"] = "DoubleTapped",
        ["ValueChanged"] = "ValueChanged",
    };

    /// <summary>Avalonia elements on which a Click attribute is valid.</summary>
    private static readonly HashSet<string> _clickableElements = ["Button", "HyperlinkButton", "CheckBox", "RadioButton", "MenuItem"];

    private readonly StringBuilder _output = new();
    private readonly Dictionary<string, string> _xmlnsPrefixes = [];

    public string Emit()
    {
        CollectNamespaces();

        string rootElement = asUserControl ? "UserControl" : "Window";
        string indent = new(' ', rootElement.Length + 2);

        _output.Append($"<!--\n  Scaffolded by eng/tools/WinFormsToAxaml from {sourceDescription}.\n");
        _output.Append("  Hand-finish before use: resolve every TODO:WinForms comment, then review the layout\n");
        _output.Append("  against the running WinForms dialog.\n-->\n");
        _output.Append($"<{rootElement} xmlns=\"https://github.com/avaloniaui\"\n");
        _output.Append($"{indent}xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"\n");
        foreach ((string ns, string prefix) in _xmlnsPrefixes.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            _output.Append($"{indent}xmlns:{prefix}=\"clr-namespace:{ns}\"\n");
        }

        _output.Append($"{indent}x:Class=\"{form.Namespace}.{form.ClassName}\"");
        EmitRootAttributes(indent);
        _output.Append(">\n");

        // Form-level properties that had no attribute mapping (HelpButton, MaximizeBox,
        // custom base-dialog properties, …) are surfaced for the hand-finish.
        HashSet<string> consumedRootProperties =
            ["ClientSize", "MinimumSize", "StartPosition", "FormBorderStyle", "Text", "AcceptButton", "CancelButton"];
        foreach ((string property, string value) in form.Root.Properties)
        {
            if (!consumedRootProperties.Contains(property) && !_droppedProperties.Contains(property))
            {
                form.Root.Unmapped.Add($"{form.ClassName}.{property} = {value}");
            }
        }

        EmitComments(form.Root, "  ");

        // Non-visual components (ErrorProvider, Timer, dialogs, …) have no element in the
        // scaffold, but their configuration must not get lost: surface it for the code-behind.
        foreach (ControlNode component in form.AllControls.Where(control => DesignerForm.IsNonVisual(control) && !IsComponentContainer(control)))
        {
            foreach ((string eventName, string handler) in component.Events)
            {
                component.Unmapped.Add($"{component.Name}.{eventName} += {handler}");
            }

            foreach ((string property, string value) in component.Properties)
            {
                component.Unmapped.Add($"{component.Name}.{property} = {value}");
            }

            if (component.Unmapped.Count > 0)
            {
                _output.Append($"  <!-- TODO:WinForms non-visual component '{component.Name}' ({component.TypeName}); hand-wire in the code-behind: -->\n");
                EmitComments(component, "  ");
            }
        }

        IReadOnlyList<ControlNode> topLevel = form.TopLevelControls;
        if (topLevel.Count == 1)
        {
            EmitControl(topLevel[0], "  ", parentElement: rootElement);
        }
        else if (topLevel.Count > 1)
        {
            _output.Append("  <DockPanel>\n");
            foreach (ControlNode control in OrderForDockPanel(topLevel))
            {
                EmitControl(control, "    ", parentElement: "DockPanel");
            }

            _output.Append("  </DockPanel>\n");
        }

        _output.Append($"</{rootElement}>\n");
        return _output.ToString();
    }

    private void EmitRootAttributes(string indent)
    {
        Dictionary<string, string> properties = form.Root.Properties;

        if (!asUserControl)
        {
            if (properties.TryGetValue("ClientSize", out string? clientSize) && TryParsePair(clientSize, out string width, out string height))
            {
                _output.Append($"\n{indent}Width=\"{width}\" Height=\"{height}\"");
            }

            if (properties.TryGetValue("MinimumSize", out string? minimumSize) && TryParsePair(minimumSize, out string minWidth, out string minHeight))
            {
                _output.Append($"\n{indent}MinWidth=\"{minWidth}\" MinHeight=\"{minHeight}\"");
            }

            if (properties.TryGetValue("StartPosition", out string? startPosition))
            {
                string? location = startPosition switch
                {
                    "FormStartPosition.CenterParent" => "CenterOwner",
                    "FormStartPosition.CenterScreen" => "CenterScreen",
                    _ => null,
                };
                if (location is not null)
                {
                    _output.Append($"\n{indent}WindowStartupLocation=\"{location}\"");
                }
            }

            if (properties.TryGetValue("FormBorderStyle", out string? borderStyle)
                && borderStyle is "FormBorderStyle.FixedDialog" or "FormBorderStyle.FixedSingle" or "FormBorderStyle.Fixed3D")
            {
                _output.Append($"\n{indent}CanResize=\"False\"");
            }

            if (properties.TryGetValue("Text", out string? title))
            {
                _output.Append($"\n{indent}Title=\"{ConvertText(title, stripMnemonics: true)}\"");
            }
        }
    }

    private void CollectNamespaces()
    {
        foreach (ControlNode control in form.AllControls)
        {
            if (DesignerForm.IsNonVisual(control) || IsComponentContainer(control))
            {
                continue;
            }

            string typeName = QualifyCustomType(control.TypeName);
            if (_controlMap.ContainsKey(typeName) || !typeName.Contains('.'))
            {
                continue;
            }

            string ns = typeName[..typeName.LastIndexOf('.')];
            if (!_xmlnsPrefixes.ContainsKey(ns))
            {
                string prefix = ns[(ns.LastIndexOf('.') + 1)..].ToLowerInvariant();
                while (_xmlnsPrefixes.ContainsValue(prefix))
                {
                    prefix += "x";
                }

                _xmlnsPrefixes[ns] = prefix;
            }
        }
    }

    /// <summary>Custom control types keep their namespace; Designer files sometimes write it relative to the project root namespace.</summary>
    private string QualifyCustomType(string typeName)
    {
        if (_controlMap.ContainsKey(typeName) || typeName.Contains('.'))
        {
            return typeName;
        }

        // Unqualified and unknown: assume a sibling type in the form's own namespace.
        return form.Namespace.Length > 0 ? $"{form.Namespace}.{typeName}" : typeName;
    }

    private void EmitControl(ControlNode control, string indent, string parentElement)
    {
        if (DesignerForm.IsNonVisual(control) || IsComponentContainer(control))
        {
            return;
        }

        if (control.IsInherited && control.Name.Contains('.'))
        {
            // SplitContainer.Panel1/.Panel2 pseudo-containers: emit the children directly.
            foreach (ControlNode child in control.Children)
            {
                EmitControl(child, indent, parentElement);
            }

            return;
        }

        string element = MapElement(control);
        List<string> attributes = BuildAttributes(control, element, parentElement);

        EmitComments(control, indent);

        if (control.IsInherited)
        {
            _output.Append($"{indent}<!-- TODO:WinForms '{control.Name}' is a container inherited from the WinForms base form; map it to the twin's dialog chrome. -->\n");
            if (control.Name == "ControlsPanel" && parentElement == "DockPanel")
            {
                attributes.Add("DockPanel.Dock=\"Bottom\"");
                attributes.Add("Orientation=\"Horizontal\"");
                attributes.Add("HorizontalAlignment=\"Right\"");
                attributes.Add("Spacing=\"8\"");
                attributes.Add("Margin=\"8\"");
            }
        }

        List<ControlNode> children = OrderChildren(control, element);
        bool hasContent = children.Count > 0;

        _output.Append($"{indent}<{element}");
        foreach (string attribute in attributes)
        {
            _output.Append(' ').Append(attribute);
        }

        if (!hasContent)
        {
            _output.Append(" />\n");
            return;
        }

        _output.Append(">\n");

        string childIndent = indent + "  ";
        string childParent = element;

        if (element == "HeaderedContentControl" && children.Count > 1)
        {
            _output.Append($"{childIndent}<StackPanel>\n");
            childIndent += "  ";
            childParent = "StackPanel";
        }

        foreach (ControlNode child in children)
        {
            EmitControl(child, childIndent, childParent);
        }

        if (element == "HeaderedContentControl" && children.Count > 1)
        {
            childIndent = indent + "  ";
            _output.Append($"{childIndent}</StackPanel>\n");
        }

        _output.Append($"{indent}</{element}>\n");
    }

    private string MapElement(ControlNode control)
    {
        string typeName = QualifyCustomType(control.TypeName);

        if (_controlMap.TryGetValue(typeName, out string? mapped))
        {
            // Panels become DockPanels when their children use docking.
            if (typeName is "Panel" && control.Children.Any(UsesSideDock))
            {
                return "DockPanel";
            }

            // Vertical flow panels become vertical StackPanels (the default here is horizontal).
            return mapped;
        }

        if (!typeName.Contains('.'))
        {
            return typeName;
        }

        string ns = typeName[..typeName.LastIndexOf('.')];
        string name = typeName[(typeName.LastIndexOf('.') + 1)..];
        return $"{_xmlnsPrefixes[ns]}:{name}";
    }

    private List<string> BuildAttributes(ControlNode control, string element, string parentElement)
    {
        List<string> attributes = [$"x:Name=\"{control.Name}\""];

        if (parentElement == "Grid")
        {
            if (control.GridRow is int row and > 0)
            {
                attributes.Add($"Grid.Row=\"{row}\"");
            }

            if (control.GridColumn is int column and > 0)
            {
                attributes.Add($"Grid.Column=\"{column}\"");
            }

            if (control.RowSpan is int rowSpan and > 1)
            {
                attributes.Add($"Grid.RowSpan=\"{rowSpan}\"");
            }

            if (control.ColumnSpan is int columnSpan and > 1)
            {
                attributes.Add($"Grid.ColumnSpan=\"{columnSpan}\"");
            }
        }
        else if (parentElement == "DockPanel" && control.Properties.TryGetValue("Dock", out string? dock))
        {
            string? side = dock switch
            {
                "DockStyle.Top" => "Top",
                "DockStyle.Bottom" => "Bottom",
                "DockStyle.Left" => "Left",
                "DockStyle.Right" => "Right",
                _ => null, // Fill: the last DockPanel child fills by default.
            };
            if (side is not null)
            {
                attributes.Add($"DockPanel.Dock=\"{side}\"");
            }
        }

        if (element == "Grid")
        {
            EmitGridDefinitions(control, attributes);
        }
        else if (element == "StackPanel")
        {
            bool vertical = control.TypeName == "Panel"
                || (control.Properties.TryGetValue("FlowDirection", out string? direction) && direction == "FlowDirection.TopDown");
            if (!vertical && control.TypeName is "FlowLayoutPanel" or "ToolStrip" or "StatusStrip")
            {
                attributes.Add("Orientation=\"Horizontal\"");
            }
        }

        foreach ((string property, string value) in control.Properties)
        {
            AppendPropertyAttribute(control, element, property, value, attributes);
        }

        if (control.ToolTipText is not null)
        {
            attributes.Add($"ToolTip.Tip=\"{EscapeXml(control.ToolTipText)}\"");
        }

        // AcceptButton/CancelButton on the form become IsDefault/IsCancel on the button.
        if (element is "Button")
        {
            if (form.Root.Properties.TryGetValue("AcceptButton", out string? acceptButton) && acceptButton == control.Name)
            {
                attributes.Add("IsDefault=\"True\"");
            }

            if (form.Root.Properties.TryGetValue("CancelButton", out string? cancelButton) && cancelButton == control.Name)
            {
                attributes.Add("IsCancel=\"True\"");
            }
        }

        foreach ((string eventName, string handler) in control.Events)
        {
            if (_eventMap.TryGetValue(eventName, out string? avaloniaEvent)
                && (eventName != "Click" || _clickableElements.Contains(element)))
            {
                attributes.Add($"{avaloniaEvent}=\"{handler}\"");
            }
            else
            {
                control.Unmapped.Add($"{control.Name}.{eventName} += {handler}");
            }
        }

        return attributes;
    }

    private void EmitGridDefinitions(ControlNode control, List<string> attributes)
    {
        if (control.TypeName == "SplitContainer")
        {
            bool columns = !control.Properties.TryGetValue("Orientation", out string? orientation) || orientation != "Orientation.Horizontal";
            attributes.Add(columns ? "ColumnDefinitions=\"*,4,*\"" : "RowDefinitions=\"*,4,*\"");
            control.Unmapped.Add($"{control.Name} is a SplitContainer: place Panel1/Panel2 content in cells 0 and 2 and add a <GridSplitter /> in cell 1");
            return;
        }

        List<string> columnDefinitions = PadDefinitions(control.ColumnStyles, GetCount(control, "ColumnCount"));
        List<string> rowDefinitions = PadDefinitions(control.RowStyles, GetCount(control, "RowCount"));

        if (columnDefinitions.Count > 0)
        {
            attributes.Add($"ColumnDefinitions=\"{string.Join(',', columnDefinitions)}\"");
        }

        if (rowDefinitions.Count > 0)
        {
            attributes.Add($"RowDefinitions=\"{string.Join(',', rowDefinitions)}\"");
        }

        static int GetCount(ControlNode control, string property)
            => control.Properties.TryGetValue(property, out string? text) && int.TryParse(text, out int count) ? count : 0;

        static List<string> PadDefinitions(List<string> styles, int count)
        {
            List<string> definitions = [.. styles];
            while (definitions.Count < count)
            {
                definitions.Add("Auto");
            }

            return definitions;
        }
    }

    private void AppendPropertyAttribute(ControlNode control, string element, string property, string value, List<string> attributes)
    {
        switch (property)
        {
            case "Text":
                string text = ConvertText(value, stripMnemonics: false);
                string textAttribute = element switch
                {
                    "TextBlock" or "TextBox" => "Text",
                    "MenuItem" or "TabItem" or "HeaderedContentControl" => "Header",
                    "Window" => "Title",
                    _ => "Content",
                };
                attributes.Add($"{textAttribute}=\"{text}\"");
                return;

            case "Padding" when TryConvertThickness(value, out string padding):
                if (IsZeroThickness(padding))
                {
                    return;
                }

                if (element is "StackPanel" or "DockPanel" or "Grid" or "WrapPanel")
                {
                    // Avalonia panels have no Padding; the closest scaffold equivalent is
                    // a margin on the children — leave the decision to the hand-finish.
                    control.Unmapped.Add($"{control.Name}.Padding = {value} (panels have no Padding in Avalonia)");
                }
                else
                {
                    attributes.Add($"Padding=\"{padding}\"");
                }

                return;

            case "TextAlign" when element == "TextBlock":
                if (value.StartsWith("ContentAlignment.Middle"))
                {
                    attributes.Add("VerticalAlignment=\"Center\"");
                }

                if (value.EndsWith("Center"))
                {
                    attributes.Add("TextAlignment=\"Center\"");
                }
                else if (value.EndsWith("Right"))
                {
                    attributes.Add("TextAlignment=\"Right\"");
                }

                return;

            case "TextAlign":
                return;

            case "Margin" when TryConvertThickness(value, out string margin):
                if (!IsZeroThickness(margin))
                {
                    attributes.Add($"Margin=\"{margin}\"");
                }

                return;

            case "MinimumSize" when TryParsePair(value, out string minWidth, out string minHeight):
                attributes.Add($"MinWidth=\"{minWidth}\"");
                attributes.Add($"MinHeight=\"{minHeight}\"");
                return;

            case "MaximumSize" when TryParsePair(value, out string maxWidth, out string maxHeight):
                attributes.Add($"MaxWidth=\"{maxWidth}\"");
                attributes.Add($"MaxHeight=\"{maxHeight}\"");
                return;

            case "WordWrap":
                attributes.Add($"TextWrapping=\"{(value == "true" ? "Wrap" : "NoWrap")}\"");
                return;

            case "Checked" when control.TypeName is "CheckBox" or "RadioButton" or "ToolStripMenuItem":
                attributes.Add($"IsChecked=\"{(value == "true" ? "True" : "False")}\"");
                return;

            case "Style" when value == "ProgressBarStyle.Marquee":
                attributes.Add("IsIndeterminate=\"True\"");
                return;

            case "DialogResult":
                control.Unmapped.Add($"{control.Name}.DialogResult = {value} (WinForms auto-close: set the dialog result and close in the {control.Name} click handler)");
                return;

            case "Image":
                control.Unmapped.Add($"{control.Name}.Image = {value}");
                return;
        }

        if (_droppedProperties.Contains(property))
        {
            return;
        }

        if (_propertyMap.TryGetValue(property, out string? mapped))
        {
            string converted = value switch
            {
                "true" => "True",
                "false" => "False",
                _ => EscapeXml(TrimNumericSuffix(value)),
            };
            attributes.Add($"{mapped}=\"{converted}\"");
            return;
        }

        control.Unmapped.Add($"{control.Name}.{property} = {value}");
    }

    private void EmitComments(ControlNode control, string indent)
    {
        foreach (string unmapped in control.Unmapped)
        {
            _output.Append($"{indent}<!-- TODO:WinForms {EscapeComment(unmapped)} -->\n");
        }
    }

    /// <summary>Grid children ordered by cell for readable output; other containers keep source order.</summary>
    private static List<ControlNode> OrderChildren(ControlNode control, string element)
    {
        List<ControlNode> children = [.. control.Children];
        if (element == "Grid" && control.TypeName == "TableLayoutPanel")
        {
            return [.. children.OrderBy(child => child.GridRow ?? 0).ThenBy(child => child.GridColumn ?? 0)];
        }

        return children;
    }

    /// <summary>
    ///  DockPanel needs the filling child last; WinForms z-order puts it first. The button
    ///  row of the GitExtensions base dialog ("ControlsPanel") docks to the bottom edge and
    ///  "MainPanel" fills what remains.
    /// </summary>
    private static IEnumerable<ControlNode> OrderForDockPanel(IReadOnlyList<ControlNode> controls)
        => [.. controls.OrderBy(control => (control.Name, UsesSideDock(control)) switch
            {
                ("MainPanel", _) => 2,
                ("ControlsPanel", _) => 0,
                (_, true) => 0,
                (_, false) => 1,
            })];

    private static bool UsesSideDock(ControlNode control)
        => control.Properties.TryGetValue("Dock", out string? dock) && dock is not ("DockStyle.Fill" or "DockStyle.None");

    private static bool IsComponentContainer(ControlNode control)
        => control.TypeName is "Container" or "System.ComponentModel.Container";

    /// <summary>Unquotes a C# string literal, converts &amp;-mnemonics to Avalonia _-access keys, escapes XML.</summary>
    private static string ConvertText(string literal, bool stripMnemonics)
    {
        string text = literal.Length >= 2 && literal[0] == '"' && literal[^1] == '"'
            ? literal[1..^1].Replace("\\\"", "\"").Replace("\\\\", "\\").Replace("\\n", "\n").Replace("\\t", "\t").Replace("\\r", "\r")
            : literal;

        StringBuilder converted = new(text.Length);
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '&')
            {
                if (i + 1 < text.Length && text[i + 1] == '&')
                {
                    converted.Append('&');
                    i++;
                }
                else if (!stripMnemonics)
                {
                    converted.Append('_');
                }
            }
            else
            {
                converted.Append(text[i]);
            }
        }

        return EscapeXml(converted.ToString());
    }

    private static bool TryParsePair(string value, out string first, out string second)
    {
        // "new Size(570, 386)" → ("570", "386").
        first = second = "";
        int open = value.IndexOf('(');
        int close = value.LastIndexOf(')');
        if (open < 0 || close <= open)
        {
            return false;
        }

        string[] parts = value[(open + 1)..close].Split(',', StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        first = TrimNumericSuffix(parts[0]);
        second = TrimNumericSuffix(parts[1]);
        return true;
    }

    private static bool TryConvertThickness(string value, out string thickness)
    {
        // "new Padding(3)" → "3"; "new Padding(8, 0, 0, 0)" → "8,0,0,0" (same edge order).
        thickness = "";
        int open = value.IndexOf('(');
        int close = value.LastIndexOf(')');
        if (open < 0 || close <= open)
        {
            return false;
        }

        string[] parts = value[(open + 1)..close].Split(',', StringSplitOptions.TrimEntries);
        if (parts.Length is not (1 or 4) || !parts.All(part => int.TryParse(part, out _)))
        {
            return false;
        }

        thickness = string.Join(',', parts);
        return true;
    }

    private static bool IsZeroThickness(string thickness)
        => thickness.Split(',').All(part => part == "0");

    private static string TrimNumericSuffix(string value)
        => value.EndsWith('F') || value.EndsWith('f') || value.EndsWith('D') || value.EndsWith('d')
            ? value[..^1]
            : value;

    private static string EscapeXml(string text)
        => text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("\n", "&#x0a;");

    private static string EscapeComment(string text)
        => text.Replace("--", "- -");
}
