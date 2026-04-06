using GitExtensions.Extensibility.Git;

namespace GitUI.UserControls.RevisionGrid.RefContextMenus;

/// <summary>
///  Provides context menu items for a specific kind of git ref or stash selector.
/// </summary>
internal interface IRefContextMenuProvider
{
    /// <summary>
    ///  Determines whether this provider can handle the given ref.
    /// </summary>
    bool Handles(IGitRef? gitRef, string? stashReflogSelector);

    /// <summary>
    ///  Populates <paramref name="menu"/> with items specific to the ref or stash selector.
    /// </summary>
    void Populate(ContextMenuStrip menu, IGitRef? gitRef, string? stashReflogSelector, RefContextMenuContext context);
}
