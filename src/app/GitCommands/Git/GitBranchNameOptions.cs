namespace GitCommands.Git;

/// <summary>
///  Options used by <see cref="GitBranchNameNormaliser"/> to ensures compliance with the GIT branch naming conventions.
/// </summary>
public sealed class GitBranchNameOptions
{
    /// <summary>
    ///  Initializes a new instance of the <see cref="GitBranchNameOptions"/> class.
    /// </summary>
    /// <param name="replacementToken">The character which will replace all invalid characters in git branch name.</param>
    /// <param name="allowTrailingSlash">Indicates whether a trailing slash is allowed in the branch name, e.g. in case of a branch prefix.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the replacement token is invalid.</exception>
    public GitBranchNameOptions(string? replacementToken, bool allowTrailingSlash = false)
    {
        AllowTrailingSlash = allowTrailingSlash;

        if (!string.IsNullOrEmpty(replacementToken))
        {
            if (replacementToken.Length > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(replacementToken), "Replacement token must be a single character");
            }

            if (!PathUtil.IsValidPathChar(replacementToken[0]))
            {
                throw new ArgumentOutOfRangeException(nameof(replacementToken), string.Format("Replacement token invalid: '{0}'", replacementToken));
            }
        }

        ReplacementToken = replacementToken ?? string.Empty;
    }

    /// <summary>
    ///  Indicates whether a trailing slash is allowed in the branch name, e.g. in case of a branch prefix.
    /// </summary>
    public bool AllowTrailingSlash { get; }

    /// <summary>
    ///  Gets the character which will replace all invalid characters in git branch name.
    /// </summary>
    /// <seealso cref="GitBranchNameNormaliser"/>.
    public string ReplacementToken { get; }
}
