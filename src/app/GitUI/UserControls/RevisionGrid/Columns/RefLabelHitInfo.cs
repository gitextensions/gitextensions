using GitExtensions.Extensibility.Git;

namespace GitUI.UserControls.RevisionGrid.Columns;

// TODO: When C# unions are available, model the value as a union of IGitRef and string (stash selector).
/// <summary>
///  Represents a painted ref label's bounds in the revision grid's client coordinate space,
///  enabling hit-testing for mouse hover and context menu interactions.
///  Exactly one of <see cref="GitRef"/> or <see cref="StashReflogSelector"/> will be non-null.
/// </summary>
internal readonly record struct RefLabelHitInfo(Rectangle Bounds, IGitRef? GitRef, string? StashReflogSelector);
