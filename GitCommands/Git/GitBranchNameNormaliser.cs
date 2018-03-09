using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GitCommands.Git
{
    /// <summary>
    /// Provides ability to ensure compliance with the GIT branch naming conventions.
    /// </summary>
    public interface IGitBranchNameNormaliser
    {
        /// <summary>
        /// Ensures that the branch name meets the GIT branch naming conventions.
        /// For more details refer to <see href="https://www.git-scm.com/docs/git-check-ref-format/1.8.2"/>.
        /// </summary>
        /// <param name="branchName">Name of the branch.</param>
        /// <param name="options">The options.</param>
        /// <returns>Normalised branch name.</returns>
        string Normalise(string branchName, GitBranchNameOptions options);
    }

    /*

        DESCRIPTION
        Checks if a given refname is acceptable, and exits with a non-zero status if it is not.

        A reference is used in Git to specify branches and tags. A branch head is stored in the refs/heads hierarchy, while a tag is stored in the refs/tags hierarchy of the ref namespace (typically in $GIT_DIR/refs/heads and $GIT_DIR/refs/tags directories or, as entries in file $GIT_DIR/packed-refs if refs are packed by git gc).

        Git imposes the following rules on how references are named:

        1. They can include slash '/' for hierarchical (directory) grouping, but no slash-separated component can begin with a dot '.' or end with the sequence '.lock'.

        2. They must contain at least one '/'. This enforces the presence of a category like 'heads/', 'tags/' etc. but the actual names are not restricted. If the --allow-onelevel option is used, this rule is waived.

        3. They cannot have two consecutive dots '..' anywhere.

        4. They cannot have ASCII control characters (i.e. bytes whose values are lower than \040, or \177 DEL), space, tilde '~', caret '^', or colon ':' anywhere.

        5. They cannot have question-mark '?', asterisk '*', or open bracket '[' anywhere. See the --refspec-pattern option below for an exception to this rule.

        6. They cannot begin or end with a slash '/' or contain multiple consecutive slashes (see the --normalize option below for an exception to this rule)

        7. They cannot end with a dot '.'.

        8. They cannot contain a sequence '@{'.

        9. They cannot contain a '\'.

        These rules make it easy for shell script based tools to parse reference names, pathname expansion by the shell when a reference name is used unquoted (by mistake), and also avoids ambiguities in certain reference name expressions (see gitrevisions(7)):

        1. A double-dot '..' is often used as in 'ref1..ref2', and in some contexts this notation means '^ref1 ref2' (i.e. not in 'ref1' and in 'ref2').

        2. A tilde '~' and caret '^' are used to introduce the postfix nth parent and peel onion operation.

        3. A colon ':' is used as in 'srcref:dstref' to mean "use srcref\s value and store it in dstref" in fetch and push operations. It may also be used to select a specific object such as with git cat-file': "git cat-file blob v1.3.3:refs.c".

        4. at-open-brace '@{' is used as a notation to access a reflog entry.

        With the --branch option, it expands the “previous branch syntax” @{-n}. For example, @{-1} is a way to refer the last branch you were on. This option should be used by porcelains to accept this syntax anywhere a branch name is expected, so they can act as if you typed the branch name.

    */

    /// <summary>
    /// Ensures compliance with the GIT branch naming conventions.
    /// </summary>
    public sealed class GitBranchNameNormaliser : IGitBranchNameNormaliser
    {
        /// <summary>
        /// Ensures that the branch name meets the GIT branch naming conventions.
        /// For more details refer to <see href="https://www.git-scm.com/docs/git-check-ref-format/1.8.2"/>.
        /// </summary>
        /// <param name="branchName">Name of the branch.</param>
        /// <param name="options">The options.</param>
        /// <returns>Normalised branch name.</returns>
        public string Normalise(string branchName, GitBranchNameOptions options)
        {
            if (string.IsNullOrWhiteSpace(branchName))
            {
                return string.Empty;
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // run rules in reverse order
            branchName = Rule09(branchName, options);
            branchName = Rule08(branchName, options);
            branchName = Rule07(branchName, options);
            branchName = Rule05(branchName, options);
            branchName = Rule04(branchName, options);
            branchName = Rule03(branchName, options);

            // rule #2 is not applicable
            // rule #6 runs as second last to ensure no consecutive '/' are left after previous normalisations
            branchName = Rule06(branchName);
            branchName = Rule01(branchName, options);

            return branchName;
        }

        /// <summary>
        /// Indicates whether the given character can be used in a branch name.
        /// </summary>
        public static bool IsValidChar(char c)
        {
            return (c >= 32 && c < 127) &&
                    c != ' ' && c != '~' && c != '^' && c != ':' &&
                    Array.IndexOf(Path.GetInvalidPathChars(), c) < 0;
        }

        /// <summary>
        /// Branch name can include slash '/' for hierarchical (directory) grouping,
        /// but no slash-separated component can begin with a dot '.' or end with the sequence '.lock'.
        /// </summary>
        /// <returns>Normalised branch name.</returns>
        internal string Rule01(string branchName, GitBranchNameOptions options)
        {
            var tokens = branchName.Split('/').ToList();
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].StartsWith("."))
                {
                    tokens[i] = Regex.Replace(tokens[i], "^(\\.)*", options.ReplacementToken);
                }

                if (tokens[i].EndsWith(".lock", StringComparison.OrdinalIgnoreCase))
                {
                    tokens[i] = Regex.Replace(tokens[i], "(\\.lock)$", options.ReplacementToken + "lock");
                }
            }

            return tokens.Join("/");
        }

        /// <summary>
        /// Branch name cannot have two consecutive dots '..' anywhere.
        /// </summary>
        /// <param name="branchName">Name of the branch.</param>
        /// <param name="options">The options.</param>
        /// <returns>Normalised branch name.</returns>
        internal string Rule03(string branchName, GitBranchNameOptions options)
        {
            return Regex.Replace(branchName, "\\.{2,}", options.ReplacementToken);
        }

        /// <summary>
        /// Branch name cannot have ASCII control characters (i.e. bytes whose values are lower than \040, or \127 'DEL'),
        /// space, tilde '~', caret '^', or colon ':' anywhere.
        /// Also allow any valid Unicode letters.
        /// </summary>
        /// <param name="branchName">Name of the branch.</param>
        /// <param name="options">The options.</param>
        /// <returns>Normalised branch name.</returns>
        internal string Rule04(string branchName, GitBranchNameOptions options)
        {
            var result = new StringBuilder(branchName.Length);
            foreach (char t in branchName)
            {
                if (IsValidChar(t) || char.IsLetterOrDigit(t))
                {
                    result.Append(t);
                }
                else
                {
                    result.Append(options.ReplacementToken);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Branch name cannot have question-mark '?', asterisk '*', or open bracket '[' anywhere.
        /// </summary>
        /// <param name="branchName">Name of the branch.</param>
        /// <param name="options">The options.</param>
        /// <returns>Normalised branch name.</returns>
        internal string Rule05(string branchName, GitBranchNameOptions options)
        {
            return Regex.Replace(branchName, "(\\?|\\*|\\[)", options.ReplacementToken);
        }

        /// <summary>
        /// Branch name begin or end with a slash '/' or contain multiple consecutive slashes.
        /// </summary>
        /// <param name="branchName">Name of the branch.</param>
        /// <returns>Normalised branch name.</returns>
        internal string Rule06(string branchName)
        {
            branchName = Regex.Replace(branchName, @"(\/{2,})", "/");
            if (branchName.StartsWith("/"))
            {
                branchName = branchName.Substring(1);
            }

            if (branchName.EndsWith("/"))
            {
                branchName = branchName.Substring(0, branchName.Length - 1);
            }

            return branchName;
        }

        /// <summary>
        /// Branch name end with a dot '.'.
        /// </summary>
        /// <param name="branchName">Name of the branch.</param>
        /// <param name="options">The options.</param>
        /// <returns>Normalised branch name.</returns>
        internal string Rule07(string branchName, GitBranchNameOptions options)
        {
            return Regex.Replace(branchName, "(\\.{1,})$", options.ReplacementToken);
        }

        /// <summary>
        /// Branch name cannot contain a sequence '@{'.
        /// </summary>
        /// <param name="branchName">Name of the branch.</param>
        /// <param name="options">The options.</param>
        /// <returns>Normalised branch name.</returns>
        internal string Rule08(string branchName, GitBranchNameOptions options)
        {
            return Regex.Replace(branchName, "(@\\{)", options.ReplacementToken);
        }

        /// <summary>
        /// Branch name cannot contain a '\'.
        /// </summary>
        /// <param name="branchName">Name of the branch.</param>
        /// <param name="options">The options.</param>
        /// <returns>Normalised branch name.</returns>
        internal string Rule09(string branchName, GitBranchNameOptions options)
        {
            return Regex.Replace(branchName, @"(\\{1,})", options.ReplacementToken);
        }
    }
}