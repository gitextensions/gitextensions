using GitExtensions.Extensibility.Git;

namespace GitUI.UserControls.RevisionGrid.Columns;

// TODO: When C# unions are available, model the value as a union of IGitRef and string (stash selector).
/// <summary>
///  Represents a painted ref label's bounds in the revision grid's client coordinate space,
///  enabling hit-testing for mouse hover and context menu interactions.
///  Exactly one of <see cref="GitRef"/> or <see cref="StashReflogSelector"/> will be non-null.
/// </summary>
/// <param name="Bounds">The label's bounding rectangle in DataGridView client coordinates.</param>
/// <param name="Shape">
///  The drawn capsule shape. For point/notch shapes the interlocking diagonal edge - not the
///  bounding rectangle - determines ownership.
/// </param>
/// <param name="PointWidth">The depth of the point/notch in pixels; unused for <see cref="RefLabelShape.Rect"/>.</param>
/// <param name="GitRef">The ref this label represents.</param>
/// <param name="StashReflogSelector">The stash reflog selector.</param>
internal readonly record struct RefLabelHitInfo(Rectangle Bounds, RefLabelShape Shape, int PointWidth, IGitRef? GitRef, string? StashReflogSelector)
{
    /// <summary>
    ///  Tests whether <paramref name="point"/> falls inside the label, honouring the slanted
    ///  point/notch edge so adjacent nestled capsules split along their shared diagonal.
    /// </summary>
    public bool Contains(Point point)
    {
        if (point.X < Bounds.Left || point.X >= Bounds.Right || point.Y < Bounds.Top || point.Y >= Bounds.Bottom)
        {
            return false;
        }

        int halfHeight = Bounds.Height / 2;
        if (PointWidth <= 0 || Shape == RefLabelShape.Rect || halfHeight <= 0)
        {
            return true;
        }

        // Distance from the vertical centre, where the point tip / notch tip sits.
        int dy = Math.Min(Math.Abs(point.Y - (Bounds.Top + halfHeight)), halfHeight);

        // Horizontal extent of the slant at this row: 0 at the tip row, PointWidth at the top/bottom rows.
        int slant = PointWidth * dy / halfHeight;

        return Shape switch
        {
            // Convex tip at Right (mid row); top/bottom corners step back by PointWidth.
            RefLabelShape.PointRight => point.X <= Bounds.Right - slant,

            // Concave tip indented to Right - PointWidth (mid row); opens out to Right at top/bottom.
            RefLabelShape.NotchRight => point.X <= Bounds.Right - PointWidth + slant,

            // Convex tip at Left (mid row); top/bottom corners step in by PointWidth.
            RefLabelShape.PointLeft => point.X >= Bounds.Left + slant,

            // Concave tip indented to Left + PointWidth (mid row); opens out to Left at top/bottom.
            RefLabelShape.NotchLeft => point.X >= Bounds.Left + PointWidth - slant,

            _ => true
        };
    }
}
