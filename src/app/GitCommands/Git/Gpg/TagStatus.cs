namespace GitCommands.Git.Gpg;

/// <summary>
/// Represents the status of a Git tag.
/// </summary>
public enum TagStatus
{
    /// <summary>
    /// No tag is present.
    /// </summary>
    NoTag = 0,

    /// <summary>
    /// One good tag is present.
    /// </summary>
    OneGood = 1,

    /// <summary>
    /// One bad tag is present.
    /// </summary>
    OneBad = 2,

    /// <summary>
    /// Multiple tags are present.
    /// </summary>
    Many = 3,

    /// <summary>
    /// No public key is available.
    /// </summary>
    NoPubKey = 4,

    /// <summary>
    /// The tag is not signed.
    /// </summary>
    TagNotSigned = 5
}
