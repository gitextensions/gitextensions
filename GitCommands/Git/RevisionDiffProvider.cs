using System;
using System.Diagnostics;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands.Git
{
    public interface IRevisionDiffProvider
    {
        /// <summary>
        /// options to git-diff from GE arguments, including artificial commits
        /// </summary>
        /// <param name="firstRevision">The first revision, "A"</param>
        /// <param name="secondRevision">The second "current" revision, "B"</param>
        ArgumentString Get(string firstRevision, string secondRevision);

        /// <summary>
        /// options to git-diff from GE arguments, including artificial commits
        /// </summary>
        /// <param name="firstRevision">The first revision, "A"</param>
        /// <param name="secondRevision">The second "current" revision, "B"</param>
        /// <param name="fileName">The file to compare</param>
        /// <param name="oldFileName">The old name of the file</param>
        /// <param name="isTracked">The file is tracked</param>
        ArgumentString Get(string firstRevision, string secondRevision, string fileName, string oldFileName, bool isTracked);
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
        public ArgumentString Get(string firstRevision, string secondRevision)
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
        public ArgumentString Get(string firstRevision, string secondRevision, string fileName, string oldFileName, bool isTracked)
        {
            return GetInternal(firstRevision, secondRevision, fileName, oldFileName, isTracked);
        }

        private ArgumentString GetInternal([CanBeNull] string firstRevision, [CanBeNull] string secondRevision, string fileName = null, string oldFileName = null, bool isTracked = true)
        {
            // Combined Diff artificial commit should not be included in diffs
            if (firstRevision == GitRevision.CombinedDiffGuid || secondRevision == GitRevision.CombinedDiffGuid)
            {
                throw new ArgumentOutOfRangeException(nameof(firstRevision), firstRevision,
                    "CombinedDiff artificial commit cannot be explicitly compared: " +
                    firstRevision + ", " + secondRevision);
            }

            var extra = new ArgumentBuilder();
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
                extra.Add("-R");
            }

            // Special case: Remove options comparing worktree-index
            if ((firstRevision.IsNullOrEmpty() && secondRevision == StagedOpt) ||
                (firstRevision == StagedOpt && secondRevision.IsNullOrEmpty()))
            {
                firstRevision = secondRevision = "";
            }

            // Reorder options - not strictly required
            if (secondRevision == StagedOpt)
            {
                extra.Add(StagedOpt);
                secondRevision = "";
            }

            if (fileName.IsNullOrWhiteSpace())
            {
                extra.Add(firstRevision);
                extra.Add(secondRevision);
            }
            else
            {
                // Untracked files can only be compared to /dev/null
                // The UI should normally only allow this for worktree to index, but it can be included in multi selections
                if (!isTracked)
                {
                    extra.Add("--no-index");
                    oldFileName = fileName;
                    fileName = "/dev/null";
                }
                else
                {
                    extra.Add(firstRevision);
                    extra.Add(secondRevision);
                }

                extra.Add("--");
                extra.Add(fileName.QuoteNE());
                extra.Add(oldFileName.QuoteNE());
            }

            return extra;
        }

        /// <summary>
        /// Translate the revision string to an option string
        /// Artificial "commits" are options, handle aliases too
        /// (order and handling of empty arguments is not handled here)
        /// </summary>
        [CanBeNull]
        private static string ArtificialToDiffOptions([CanBeNull] string rev)
        {
            switch (rev)
            {
                case GitRevision.WorkTreeGuid:
                case "":
                case null:
                    return "";
                case "^":
                case GitRevision.WorkTreeGuid + "^":
                case GitRevision.IndexGuid:
                    return StagedOpt;
                case "^^":
                case GitRevision.WorkTreeGuid + "^^":
                case GitRevision.IndexGuid + "^":
                    return "\"HEAD\"";
                case "^^^":
                case GitRevision.WorkTreeGuid + "^^^":
                case GitRevision.IndexGuid + "^^":
                    return "HEAD^";
                default:
                    return rev.QuoteNE();
            }
        }
    }
}
