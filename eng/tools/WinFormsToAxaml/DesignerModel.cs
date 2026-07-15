namespace WinFormsToAxaml;

/// <summary>
///  One control from <c>InitializeComponent</c>: its Windows Forms type, the property
///  assignments, event subscriptions, and child controls in source order.
/// </summary>
internal sealed class ControlNode(string name, string typeName)
{
    /// <summary>The field name (identical in the twin; translation keys depend on it).</summary>
    public string Name { get; } = name;

    /// <summary>The Windows Forms type as written in the Designer file, e.g. "Label" or "GitUI.UserControls.CommitPickerSmallControl".</summary>
    public string TypeName { get; set; } = typeName;

    public ControlNode? Parent { get; set; }

    public List<ControlNode> Children { get; } = [];

    /// <summary>Property name → raw source text of the assigned value, in source order.</summary>
    public Dictionary<string, string> Properties { get; } = [];

    /// <summary>Event name → handler method name, in source order.</summary>
    public List<(string Event, string Handler)> Events { get; } = [];

    /// <summary>Constructs the Designer file could not map; emitted as TODO comments.</summary>
    public List<string> Unmapped { get; } = [];

    /// <summary>Grid cell when added to a TableLayoutPanel with Controls.Add(child, column, row).</summary>
    public int? GridColumn { get; set; }

    public int? GridRow { get; set; }

    public int? ColumnSpan { get; set; }

    public int? RowSpan { get; set; }

    /// <summary>Tooltip text set through ToolTip.SetToolTip(control, text).</summary>
    public string? ToolTipText { get; set; }

    /// <summary>TableLayoutPanel only: column definitions already converted to Avalonia syntax ("Auto", "2*", "28").</summary>
    public List<string> ColumnStyles { get; } = [];

    public List<string> RowStyles { get; } = [];

    /// <summary>Set when the control is used but never created here (a container inherited from the base form).</summary>
    public bool IsInherited { get; set; }
}

/// <summary>
///  The parse result: the form itself (namespace, class name, form-level properties) and
///  all controls, keyed by field name, in source order.
/// </summary>
internal sealed class DesignerForm
{
    public required string Namespace { get; init; }

    public required string ClassName { get; init; }

    /// <summary>The form node ("this"): AcceptButton, ClientSize, Text, … live here.</summary>
    public required ControlNode Root { get; init; }

    public required IReadOnlyList<ControlNode> AllControls { get; init; }

    /// <summary>Controls contained by the form itself (or by nothing): the visual roots, in source order.</summary>
    public IReadOnlyList<ControlNode> TopLevelControls
        => [.. AllControls.Where(control => (control.Parent is null || control.Parent == Root) && !IsNonVisual(control))];

    /// <summary>Component-tray types that have no visual twin (their data is folded into other controls or hand-wired).</summary>
    public static bool IsNonVisual(ControlNode control)
    {
        string typeName = control.TypeName[(control.TypeName.LastIndexOf('.') + 1)..];
        return typeName is "ToolTip" or "IContainer" or "Container" or "ErrorProvider" or "Timer"
            or "ImageList" or "BindingSource" or "HelpProvider" or "NotifyIcon"
            or "OpenFileDialog" or "SaveFileDialog" or "FolderBrowserDialog" or "ColorDialog" or "FontDialog";
    }
}
