namespace GitUI.UserControls.RevisionGrid;

/// <summary>
///  Defines the shape of the left and right edges of a ref label capsule.
/// </summary>
internal enum RefLabelShape
{
    /// <summary>Concave '&gt;' notch on the left edge; rounded on the right.</summary>
    NotchLeft,

    /// <summary>Rounded on the left edge; concave '&lt;' notch on the right edge.</summary>
    NotchRight,

    /// <summary>Convex '&lt;' point on the left edge; rounded on the right.</summary>
    PointLeft,

    /// <summary>Rounded on the left edge; convex '&gt;' point on the right edge.</summary>
    PointRight,

    /// <summary>Rounded rectangle on both sides.</summary>
    Rect,
}
