namespace GitUI.Editor.Diff;

internal enum DiffMarkerKind
{
    Added,
    Removed,
    MovedAdded,
    MovedRemoved,
}

internal sealed record DiffTextMarker(
    int Offset,
    int Length,
    DiffMarkerKind Kind,
    Color? BackColor = null,
    Color? ForeColor = null)
{
    public int EndOffset => Offset + Length;
}

internal sealed record DiffInlineMarker(int Offset, int Length, bool IsRemoved, bool IsAnchor = false);
