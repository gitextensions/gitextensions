using System;
using System.Diagnostics;

namespace GitCommands.Git
{
    public interface IRevisionDiffProvider
    {
        /// <summary>
        /// options to git-diff from GE arguments, including artificial commits
        /// </summary>
        /// <param name="firstRevision">The first revision, "A"</param>
        /// <param name="secondRevision">The second "current" revision, "B"</param>
        string Get(string firstRevision, string secondRevision);

        /// <summary>
        /// options to git-diff from GE arguments, including artificial commits
        /// </summary>
        /// <param name="firstRevision">The first revision, "A"</param>
        /// <param name="secondRevision">The second "current" revision, "B"</param>
        /// <param name="fileName">The file to compare</param>
        /// <param name="oldFileName">The old name of the file</param>
        /// <param name="isTracked">The file is tracked</param>
        string Get(string firstRevision, string secondRevision, string fileName, string oldFileName, bool isTracked);
    }

    /// <summary>
    /// Translate GitRevision including artificial commits to diff options
    /// Closely related to GitRevision.cs
    /// </summary>
    public sealed class RevisionDiffProvider : IRevisionDiffProvider
    {
        // This is an instance class to not have static dependencies in GitModule
        private const string StagedOpt = "--cached";

        /// <summary>
        /// options to git-diff from GE arguments, including artificial commits
        /// </summary>
        /// <param name="firstRevision">The first revision</param>
        /// <param name="secondRevision">The second "current" revision</param>
        public string Get(string firstRevision, string secondRevision)
        {
            return GetInternal(firstRevision, secondRevision);
        }

        /// <summary>
        /// options to git-diff from GE arguments, including artificial commits
        /// </summary>
        /// <param name="firstRevision">The first revision, "A"</param>
        /// <param name="secondRevision">The second "current" revision, "B"</param>
        /// <param name="fileName">The file to compare</param>
        /// <param name="oldFileName">The old name of the file</param>
        /// <param name="isTracked">The file is tracked</param>
        public string Get(string firstRevision, string secondRevision, string fileName, string oldFileName, bool isTracked)
        {
            return GetInternal(firstRevision, secondRevision, fileName, oldFileName, isTracked);
        }

        private string GetInternal(string firstRevision, string secondRevision, string fileName = null, string oldFileName = null, bool isTracked = true)
        {
            string extra = string.Empty;
            firstRevision = ArtificialToDiffOptions(firstRevision);
            secondRevision = ArtificialToDiffOptions(secondRevision);

            // Note: As artificial are options, diff unstage..unstage and
            // stage..stage will show output, different from e.g. HEAD..HEAD
            // Diff-to-itself is not always disabled or is transient why this is not handled as error in release builds
            Debug.Assert(!(firstRevision == secondRevision && (firstRevision.IsNullOrEmpty() || firstRevision == StagedOpt)),
                "Unexpectedly two identical artificial revisions to diff: " + firstRevision +
                ". This will be displayed as diff to HEAD, not an identical diff.");

            // As empty (unstaged) and --cached (staged) are options (not revisions),
            // order must be preserved with -R
            if (firstRevision != secondRevision && (firstRevision.IsNullOrEmpty() ||
                               (firstRevision == StagedOpt && !secondRevision.IsNullOrEmpty())))
            {
                extra = "-R";
            }

            // Special case: Remove options comparing unstaged-staged
            if ((firstRevision.IsNullOrEmpty() && secondRevision == StagedOpt) ||
                (firstRevision == StagedOpt && secondRevision.IsNullOrEmpty()))
            {
                firstRevision = secondRevision = string.Empty;
            }

            // Reorder options - not strictly required
            if (secondRevision == StagedOpt)
            {
                extra += " " + StagedOpt;
                secondRevision = string.Empty;
            }

            if (fileName.IsNullOrWhiteSpace())
            {
                extra = string.Join(" ", extra, firstRevision, secondRevision);
            }
            else
            {
                // Untracked files can only be compared to /dev/null
                // The UI should normall only allow this for unstaged to staged, but it can be included in multi selections
                if (!isTracked)
                {
                    extra += " --no-index";
                    oldFileName = fileName;
                    fileName = "/dev/null";
                }
                else
                {
                    extra += " " + firstRevision + " " + secondRevision;
                }

                extra += " -- " + fileName.QuoteNE() + " " + oldFileName.QuoteNE();
            }

            return extra.Trim();
        }

        /// <summary>
        /// Translate the revision string to an option string
        /// Artificial "commits" are options, handle aliases too
        /// (order and handling of empty arguments is not handled here)
        /// </summary>
        private static string ArtificialToDiffOptions(string rev)
        {
            if (rev.IsNullOrEmpty() || rev == GitRevision.UnstagedGuid)
            {
                rev = string.Empty;
            }
            else if (rev == "^" || rev == GitRevision.UnstagedGuid + "^" || rev == GitRevision.IndexGuid)
            {
                rev = StagedOpt;
            }
            else
            {
                // Normal commit
                if (rev == "^^" || rev == GitRevision.UnstagedGuid + "^^" || rev == GitRevision.IndexGuid + "^")
                {
                    rev = "HEAD";
                }
                else if (rev == "^^^" || rev == GitRevision.UnstagedGuid + "^^^" || rev == GitRevision.IndexGuid + "^^")
                {
                    rev = "HEAD^";
                }

                rev = rev.QuoteNE();
            }

            return rev;
        }
    }
}
