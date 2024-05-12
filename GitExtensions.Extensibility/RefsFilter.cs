namespace GitExtensions.Extensibility;

/// <summary>
/// Enums requestable in GetRefs() (multiple names can be appended)
/// Compare to <see ref="GitRefType"/> for actual values of parsed GitRefs
/// </summary>
[Flags]
public enum RefsFilter
{
    // No filter, include those below but also (at least) stash, notes and bisect
    // Note that if NoFilter is combined with a filter, the filter takes precedence
    NoFilter = 0,

    // Local Branches
    Heads = 1 << 0,

    // Remote branches
    Remotes = 1 << 1,

    // Tags
    Tags = 1 << 2
}
