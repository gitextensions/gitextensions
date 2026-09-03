namespace GitExtensions.ParityCapture;

/// <summary>
///  Identifies how a capture reached its requested DPI.
/// </summary>
public enum CaptureDpiMode
{
    NativeMonitor,
    DpiChangeMessage,
    HeadlessRenderScale
}

/// <summary>
///  Identifies the API that produced a capture image.
/// </summary>
public enum CaptureMethod
{
    DrawToBitmap,
    PrintWindow,
    ScreenGrab,
    HeadlessSkia,
    HeadlessSkiaComposite,
    Unsupported
}

/// <summary>
///  Identifies whether a requested state was captured.
/// </summary>
public enum CaptureStateStatus
{
    Captured,
    Unsupported,
    Failed
}

/// <summary>
///  Describes one deterministic control-tree capture.
/// </summary>
public sealed record CaptureDocument
{
    public const int CurrentSchemaVersion = 1;

    public required int SchemaVersion { get; init; }

    public required CaptureComponent Component { get; init; }

    public required CaptureMetadata Capture { get; init; }

    public required CaptureImage Image { get; init; }

    public required IReadOnlyList<CaptureSurface> Surfaces { get; init; }
}

/// <summary>
///  Identifies the component represented by a capture.
/// </summary>
public sealed record CaptureComponent
{
    public required string TypeName { get; init; }

    public required string AssemblyName { get; init; }
}

/// <summary>
///  Describes the theme used by a capture.
/// </summary>
public sealed record CaptureTheme
{
    public required string Id { get; init; }

    public required string Kind { get; init; }

    public required string SourceSha256 { get; init; }
}

/// <summary>
///  Describes the state, theme, and scale of a capture.
/// </summary>
public sealed record CaptureMetadata
{
    public required string Framework { get; init; }

    public required CaptureTheme Theme { get; init; }

    public required int ScalePercent { get; init; }

    public required CaptureDpi Dpi { get; init; }

    public required CaptureDpiMode DpiMode { get; init; }

    public required string State { get; init; }

    public required CaptureStateStatus StateStatus { get; init; }

    public string? StateNote { get; init; }
}

/// <summary>
///  Holds horizontal and vertical DPI values.
/// </summary>
public sealed record CaptureDpi
{
    public required int X { get; init; }

    public required int Y { get; init; }
}

/// <summary>
///  Describes the image paired with a control tree.
/// </summary>
public sealed record CaptureImage
{
    public required int WidthPx { get; init; }

    public required int HeightPx { get; init; }

    public required CaptureMethod CaptureMethod { get; init; }
}

/// <summary>
///  Describes a top-level surface represented in one capture.
/// </summary>
public sealed record CaptureSurface
{
    public required string Role { get; init; }

    public required CaptureRectangle ScreenBoundsPx { get; init; }

    public required CaptureNode Root { get; init; }
}

/// <summary>
///  Describes one control or framework item in a capture tree.
/// </summary>
public sealed record CaptureNode
{
    public required string Id { get; init; }

    public string? FieldName { get; init; }

    public required IReadOnlyList<string> FieldAliases { get; init; }

    public string? Name { get; init; }

    public required string Type { get; init; }

    public required string ControlKind { get; init; }

    public required CaptureRectangle BoundsPx { get; init; }

    public required CaptureRectangleF BoundsDip { get; init; }

    public required CaptureSize ClientSizePx { get; init; }

    public required CaptureSizeF ClientSizeDip { get; init; }

    public decimal? ItemHeightDip { get; init; }

    public required CaptureThicknessPair Padding { get; init; }

    public required CaptureThicknessPair Margin { get; init; }

    public CaptureFont? Font { get; init; }

    public required CaptureColors Colors { get; init; }

    public string? BorderStyle { get; init; }

    public string? FlatStyle { get; init; }

    public decimal? BorderWidthDip { get; init; }

    public CaptureCornerRadius? CornerRadiusDip { get; init; }

    public required IReadOnlyList<string> Anchor { get; init; }

    public string? Dock { get; init; }

    public bool? AutoSize { get; init; }

    public string? Alignment { get; init; }

    public string? Text { get; init; }

    public string? ToolTip { get; init; }

    public string? TranslationSource { get; init; }

    public int? TabIndex { get; init; }

    public bool? TabStop { get; init; }

    public bool? Enabled { get; init; }

    public bool? Visible { get; init; }

    public bool? Focused { get; init; }

    public bool? ReadOnly { get; init; }

    public string? CheckState { get; init; }

    public bool? Selected { get; init; }

    public bool? Expanded { get; init; }

    public required IReadOnlyList<CaptureColumn> Columns { get; init; }

    public required IReadOnlyList<CaptureNode> Children { get; init; }
}

/// <summary>
///  Describes a grid or list column.
/// </summary>
public sealed record CaptureColumn
{
    public string? FieldName { get; init; }

    public string? Name { get; init; }

    public required string Type { get; init; }

    public required int Index { get; init; }

    public required int DisplayIndex { get; init; }

    public required int WidthPx { get; init; }

    public required decimal WidthDip { get; init; }

    public required bool Visible { get; init; }

    public bool? Resizable { get; init; }

    public string? SortMode { get; init; }

    public string? Alignment { get; init; }

    public string? HeaderText { get; init; }

    public string? HeaderAlignment { get; init; }

    public required CaptureColors Colors { get; init; }
}

/// <summary>
///  Stores resolved colors only, never symbolic color names.
/// </summary>
public sealed record CaptureColors
{
    public string? Foreground { get; init; }

    public string? Background { get; init; }

    public string? Border { get; init; }

    public string? SelectionForeground { get; init; }

    public string? SelectionBackground { get; init; }

    public string? InactiveSelectionForeground { get; init; }

    public string? InactiveSelectionBackground { get; init; }

    public string? DisabledForeground { get; init; }

    public string? DisabledBackground { get; init; }

    public string? GridLine { get; init; }

    public required IReadOnlyDictionary<string, string> Additional { get; init; }
}

/// <summary>
///  Describes a font using its native unit and normalized sizes.
/// </summary>
public sealed record CaptureFont
{
    public required string Family { get; init; }

    public required decimal EmSize { get; init; }

    public required string Unit { get; init; }

    public required decimal SizePoints { get; init; }

    public required decimal SizeDip { get; init; }

    public required IReadOnlyList<string> Style { get; init; }
}

/// <summary>
///  Holds integer rectangle coordinates.
/// </summary>
public sealed record CaptureRectangle
{
    public required int X { get; init; }

    public required int Y { get; init; }

    public required int Width { get; init; }

    public required int Height { get; init; }
}

/// <summary>
///  Holds normalized rectangle coordinates.
/// </summary>
public sealed record CaptureRectangleF
{
    public required decimal X { get; init; }

    public required decimal Y { get; init; }

    public required decimal Width { get; init; }

    public required decimal Height { get; init; }
}

/// <summary>
///  Holds integer dimensions.
/// </summary>
public sealed record CaptureSize
{
    public required int Width { get; init; }

    public required int Height { get; init; }
}

/// <summary>
///  Holds normalized dimensions.
/// </summary>
public sealed record CaptureSizeF
{
    public required decimal Width { get; init; }

    public required decimal Height { get; init; }
}

/// <summary>
///  Holds integer edge thicknesses.
/// </summary>
public sealed record CaptureThickness
{
    public required int Left { get; init; }

    public required int Top { get; init; }

    public required int Right { get; init; }

    public required int Bottom { get; init; }
}

/// <summary>
///  Holds normalized edge thicknesses.
/// </summary>
public sealed record CaptureThicknessF
{
    public required decimal Left { get; init; }

    public required decimal Top { get; init; }

    public required decimal Right { get; init; }

    public required decimal Bottom { get; init; }
}

/// <summary>
///  Holds the native and normalized forms of an edge thickness.
/// </summary>
public sealed record CaptureThicknessPair
{
    public required CaptureThickness Px { get; init; }

    public required CaptureThicknessF Dip { get; init; }
}

/// <summary>
///  Holds normalized corner radii.
/// </summary>
public sealed record CaptureCornerRadius
{
    public required decimal TopLeft { get; init; }

    public required decimal TopRight { get; init; }

    public required decimal BottomRight { get; init; }

    public required decimal BottomLeft { get; init; }
}
