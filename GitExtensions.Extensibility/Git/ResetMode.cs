namespace GitExtensions.Extensibility.Git;

/// <summary>Arguments to 'git reset'.</summary>
public enum ResetMode
{
    /// <summary>(no option)</summary>
    ResetIndex = 0,

    /// <summary>--soft</summary>
    Soft,

    /// <summary>--mixed</summary>
    Mixed,

    /// <summary>--hard</summary>
    Hard,

    /// <summary>--merge</summary>
    Merge,

    /// <summary>--keep</summary>
    Keep

    // All options are not implemented, like --patch
}
