namespace GitUI.AI;

/// <summary>
/// The category a file's diff was classified into by <see cref="IDiffNoiseClassifier"/>.
/// Every category other than <see cref="None"/> is considered "noise" that can be filtered away.
/// </summary>
public enum DiffNoiseCategory
{
    /// <summary>The file contains at least one substantive change and must not be hidden.</summary>
    None = 0,

    /// <summary>All changes are added/removed/reordered imports (e.g. C# <c>using</c> directives).</summary>
    Imports,

    /// <summary>All changes only update a renamed symbol at its call sites, not the declaration itself.</summary>
    CallerSiteRename,

    /// <summary>All changes only convert methods between synchronous and asynchronous form.</summary>
    SyncToAsync,

    /// <summary>All changes are style/formatting only (e.g. whitespace).</summary>
    StyleOnly,
}
