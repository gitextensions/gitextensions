using System.Diagnostics.Contracts;

namespace GitUI;

/// <summary>
/// Extension methods for <see cref="Rectangle"/>.
/// </summary>
public static class RectangleExtensions
{
    /// <summary>
    /// Returns a new rectangle with its left edge moved right by <paramref name="offset"/>,
    /// reducing the width accordingly.
    /// </summary>
    [Pure]
    public static Rectangle ReduceLeft(this Rectangle rect, int offset)
    {
        return new Rectangle(
            rect.Left + offset,
            rect.Top,
            rect.Width - offset,
            rect.Height);
    }

    /// <summary>
    /// Returns a new rectangle with its width reduced from the right by <paramref name="offset"/>.
    /// </summary>
    [Pure]
    public static Rectangle ReduceRight(this Rectangle rect, int offset)
    {
        return new Rectangle(
            rect.Left,
            rect.Top,
            rect.Width - offset,
            rect.Height);
    }
}
