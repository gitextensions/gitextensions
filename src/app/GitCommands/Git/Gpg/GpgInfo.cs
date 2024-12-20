namespace GitCommands.Git.Gpg;

/// <summary>
/// Represents information about GPG verification for commits and tags.
/// </summary>
/// <param name="CommitStatus">The status of the commit verification.</param>
/// <param name="CommitVerificationMessage">The message associated with the commit verification.</param>
/// <param name="TagStatus">The status of the tag verification.</param>
/// <param name="TagVerificationMessage">The message associated with the tag verification.</param>
public record GpgInfo(CommitStatus CommitStatus, string CommitVerificationMessage, TagStatus TagStatus, string? TagVerificationMessage);
