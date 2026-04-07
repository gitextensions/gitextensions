namespace GitUI.UserControls.RevisionGrid;

/// <summary>
/// Specifies the icon drawn inside a ref label in the revision grid.
/// </summary>
internal enum RefLabelIcon
{
    /// <summary>No icon is displayed.</summary>
    None,

    /// <summary>Solid filled arrow for the selected superproject ref.</summary>
    ArrowFilled,

    /// <summary>Hollow arrow outline for the superproject merge source ref.</summary>
    ArrowNotFilled,

    /// <summary>Target/crosshair icon for the checked-out branch.</summary>
    Head,

    /// <summary>Fork icon for local branches.</summary>
    Branch,

    /// <summary>Cloud icon for remote tracking branches.</summary>
    Remote,

    /// <summary>Tag-shaped background for tags, with the tag name written directly on the shape.</summary>
    Tag,

    /// <summary>Drawer/box icon for stashes.</summary>
    Stash
}
