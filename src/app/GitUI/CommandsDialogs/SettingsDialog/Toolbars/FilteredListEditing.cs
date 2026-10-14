namespace GitUI.CommandsDialogs.SettingsDialog.Toolbars;

// Edits a list through a filtered view of it. The settings page shows a toolbar's items through a
// search filter, and the user points at rows of that view, but the edit belongs to the whole list:
// applied to the view, it would lose every item the filter hides.
internal static class FilteredListEditing
{
    /// <summary>
    /// Moves the item shown at <paramref name="fromIndex"/> of <paramref name="visible"/> to the
    /// place of the one shown at <paramref name="toIndex"/>, within <paramref name="items"/>.
    /// </summary>
    /// <remarks>
    /// The items the filter hides keep their places relative to each other, and the view afterwards
    /// reads as if the row had been moved within it: moving up lands just before the target, moving
    /// down just after it.
    /// </remarks>
    /// <param name="items">The whole list, which receives the move.</param>
    /// <param name="visible">The items <paramref name="items"/> currently shows, in its order.</param>
    /// <param name="fromIndex">Position in <paramref name="visible"/> of the item to move.</param>
    /// <param name="toIndex">Position in <paramref name="visible"/> it is moved to.</param>
    /// <exception cref="ArgumentOutOfRangeException">Either index is outside <paramref name="visible"/>.</exception>
    public static void Move<T>(List<T> items, IReadOnlyList<T> visible, int fromIndex, int toIndex)
        where T : class
    {
        ArgumentOutOfRangeException.ThrowIfNegative(fromIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(fromIndex, visible.Count);
        ArgumentOutOfRangeException.ThrowIfNegative(toIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(toIndex, visible.Count);

        if (fromIndex == toIndex)
        {
            return;
        }

        T moved = visible[fromIndex];
        T target = visible[toIndex];

        // By reference: two separator rows are equal in every respect but their place.
        items.RemoveAt(IndexOf(items, moved));
        int targetPosition = IndexOf(items, target);
        items.Insert(toIndex > fromIndex ? targetPosition + 1 : targetPosition, moved);
    }

    private static int IndexOf<T>(List<T> items, T item)
        where T : class
    {
        int index = items.FindIndex(candidate => ReferenceEquals(candidate, item));
        if (index < 0)
        {
            throw new ArgumentException("The view shows an item the list does not contain.", nameof(items));
        }

        return index;
    }
}
