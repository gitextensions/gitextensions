using Avalonia;
using Avalonia.Controls;

namespace GitUI.Compat;

/// <summary>
///  Preserves the Professional renderer's connected area below an open menu title.
/// </summary>
public sealed class NativeToolStripMenuPopupBorder : Border
{
    /// <summary>
    ///  Identifies the width of the actual title connected to this popup.
    /// </summary>
    public static readonly StyledProperty<double> ConnectedTitleWidthProperty =
        AvaloniaProperty.Register<NativeToolStripMenuPopupBorder, double>(nameof(ConnectedTitleWidth));

    /// <summary>
    ///  Gets or sets the source title's arranged width, not a captured popup coordinate.
    /// </summary>
    public double ConnectedTitleWidth
    {
        get => GetValue(ConnectedTitleWidthProperty);
        set => SetValue(ConnectedTitleWidthProperty, value);
    }
}
