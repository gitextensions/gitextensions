using System;

namespace GitCommands.Git
{
    /// <summary>
    /// Options used by <see cref="GitBranchNameNormaliser"/> to ensures compliance with the GIT branch naming conventions.
    /// </summary>
    public sealed class GitBranchNameOptions
    {
        public GitBranchNameOptions(string replacementToken)
        {
            if (!string.IsNullOrEmpty(replacementToken))
            {
                if (replacementToken.Length > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(replacementToken), "Replacement token must be a single character");
                }

                if (!GitBranchNameNormaliser.IsValidChar(replacementToken[0]))
                {
                    throw new ArgumentOutOfRangeException(nameof(replacementToken), string.Format("Replacement token invalid: '{0}'", replacementToken));
                }
            }

            ReplacementToken = replacementToken ?? string.Empty;
        }

        /// <summary>
        /// Gets the character which will replace all invalid characters in git branch name.
        /// </summary>
        /// <seealso cref="GitBranchNameNormaliser"/>.
        public string ReplacementToken { get; }
    }
}