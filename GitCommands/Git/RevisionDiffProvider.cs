using System;
using System.Diagnostics;

namespace GitCommands
{
    /// <summary>
    /// Translate GitRevision including artificial commits to diff options
    /// Closely related to GitRevision.cs 
    /// </summary>
    public class RevisionDiffProvider
    {
        /// <summary>
        /// options to git-diff from GE arguments, including artificial commits
        /// </summary>
        /// <param name="from">The first revision</param>
        /// <param name="to">The second "current" revision</param>
        /// <returns></returns>
        public static string Get(string from, string to)
        {
            string extra = string.Empty;
            from = ArtificialToDiffOptions(from);
            to = ArtificialToDiffOptions(to);

            //Note: As artificial are options, diff unstage..unstage and 
            // stage..stage will show output, different from e.g. HEAD..HEAD
            //Diff-to-itself is not always disabled why this is not handled as error in release builds
            Debug.Assert(!(from == to && (from.IsNullOrEmpty() || from == StagedOpt)),
                "Unexpectedly two identical artificial revisions to diff: " + from +
                ". This will be displayed as diff to HEAD, not an identical diff.");

            //As empty (unstaged) and --cached (staged) are options (not revisions),
            // order must be preserved with -R
            if (from != to && (from.IsNullOrEmpty() ||
                from == StagedOpt && !to.IsNullOrEmpty()))
            {
                extra = "-R";
            }

            //Special case: Remove options comparing unstaged-staged
            if (from.IsNullOrEmpty() && to == StagedOpt ||
                from == StagedOpt && to.IsNullOrEmpty())
            {
                from = to = string.Empty;
            }

            //Reorder options - not strictly required
            if (to == StagedOpt)
            {
                extra += " " + StagedOpt;
                to = String.Empty;
            }

            return string.Join(" ", extra, from, to).Trim();
        }

        /// <summary>
        /// Translate the revision string to an option string
        /// Artificial "commits" are options, handle aliases too
        /// (order and handling of empty arguments is not handled here)
        /// </summary>
        /// <param name="rev"></param>
        /// <returns></returns>
        private static string ArtificialToDiffOptions(string rev)
        {
            if (rev.IsNullOrEmpty() || rev == GitRevision.UnstagedGuid) { rev = string.Empty; }
            else if (rev == "^" || rev == GitRevision.UnstagedGuid + "^" || rev == GitRevision.IndexGuid) { rev = StagedOpt; }
            else
            {
                //Normal commit
                if (rev == "^^" || rev == GitRevision.UnstagedGuid + "^^" || rev == GitRevision.IndexGuid + "^") { rev = "HEAD"; }
                else if (rev == "^^^" || rev == GitRevision.UnstagedGuid + "^^^" || rev == GitRevision.IndexGuid + "^^") { rev = "HEAD^"; }
                rev = rev.QuoteNE();
            }

            return rev;
        }

        private const string StagedOpt = "--cached";
    }
}
