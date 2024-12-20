namespace GitCommands.Git.Gpg;

/// <summary>
/// Represents the status of a commit's GPG signature.
/// </summary>
public enum CommitStatus
{
    /// <summary>
    /// No signature is present on the commit.
    /// </summary>
    NoSignature = 0,

    /// <summary>
    /// The commit has a valid GPG signature.
    /// </summary>
    GoodSignature = 1,

    /// <summary>
    /// There is an error with the commit's GPG signature.
    /// </summary>
    SignatureError = 2,

    /// <summary>
    /// The public key for the commit's GPG signature is missing.
    /// </summary>
    MissingPublicKey = 3,
}
