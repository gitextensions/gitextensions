using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using GitCommands.Git.Extensions;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands
{
    public interface ICommitDataManager
    {
        /// <summary>
        /// Converts a <see cref="GitRevision"/> object into a <see cref="CommitData"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="GitRevision"/> object contains all required fields, so no additional
        /// data lookup is required to populate the returned <see cref="CommitData"/> object.
        /// </remarks>
        /// <param name="revision">The <see cref="GitRevision"/> to convert from.</param>
        /// <param name="children">The list of children to add to the returned object.</param>
        [NotNull]
        CommitData CreateFromRevision([NotNull] GitRevision revision, IReadOnlyList<ObjectId> children);

        /// <summary>
        /// Gets <see cref="CommitData"/> for the specified <paramref name="sha1"/>.
        /// </summary>
        [ContractAnnotation("=>null,error:notnull")]
        [ContractAnnotation("=>notnull,error:null")]
        CommitData GetCommitData([NotNull] string sha1, out string error);

        /// <summary>
        /// Updates the <see cref="CommitData.Body"/> (commit message) property of <paramref name="commitData"/>.
        /// </summary>
        void UpdateBody([NotNull] CommitData commitData, [CanBeNull] out string error);
    }

    public sealed class CommitDataManager : ICommitDataManager
    {
        private const string CommitDataFormat = "%H%n%T%n%P%n%aN <%aE>%n%at%n%cN <%cE>%n%ct%n%e%n%B%nNotes:%n%-N";
        private const string BodyAndNotesFormat = "%H%n%e%n%B%nNotes:%n%-N";

        private readonly Func<IGitModule> _getModule;

        public CommitDataManager(Func<IGitModule> getModule)
        {
            _getModule = getModule;
        }

        /// <inheritdoc />
        public void UpdateBody(CommitData commitData, out string error)
        {
            if (!TryGetCommitLog(commitData.ObjectId.ToString(), BodyAndNotesFormat, out error, out var data))
            {
                return;
            }

            // $ git log --pretty="format:%H%n%e%n%B%nNotes:%n%-N" -1
            // 8c601c9bb040e575af75c9eee6e14441e2a1b207
            //
            // Remove redundant parameter
            //
            // The sha1 parameter must match CommitData.Guid.
            // There's no point passing it. It only creates opportunity for bugs.
            //
            // Notes:

            // commit id
            // encoding
            // commit message
            // ...

            var lines = data.Split('\n');

            var guid = lines[0];
            var commitEncoding = lines[1];
            var message = ProcessDiffNotes(startIndex: 2, lines);

            Debug.Assert(commitData.ObjectId.ToString() == guid, "Guid in response doesn't match that of request");

            // Commit message is not re-encoded by git when format is given
            commitData.Body = GetModule().ReEncodeCommitMessage(message, commitEncoding);
        }

        /// <inheritdoc />
        public CommitData GetCommitData(string sha1, out string error)
        {
            return TryGetCommitLog(sha1, CommitDataFormat, out error, out var info)
                ? CreateFromFormattedData(info)
                : null;
        }

        /// <summary>
        /// Parses <paramref name="data"/> into a <see cref="CommitData"/> object.
        /// </summary>
        /// <param name="data">Data produced by a <c>git log</c> or <c>git show</c> command where <c>--format</c>
        /// was provided the string <see cref="CommitDataFormat"/>.</param>
        /// <returns>CommitData object populated with parsed info from git string.</returns>
        [NotNull]
        internal CommitData CreateFromFormattedData([NotNull] string data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var module = GetModule();

            // $ git log --pretty="format:%H%n%T%n%P%n%aN <%aE>%n%at%n%cN <%cE>%n%ct%n%e%n%B%nNotes:%n%-N" -1
            // 4bc1049fc3b9191dbd390e1ae6885aedd1a4e34b
            // a59c21f0b2e6f43ae89b76a216f9f6124fc359f8
            // 8e3873685d89f8cb543657d1b9e66e516cae7e1d dfd353d3b02d24a0d98855f6a1848c51d9ba4d6b
            // RussKie <RussKie@users.noreply.github.com>
            // 1521115435
            // GitHub <noreply@github.com>
            // 1521115435
            //
            // Merge pull request #4615 from drewnoakes/modernise-3
            //
            // New language features
            // Notes:

            // commit id
            // tree id
            // parent ids (separated by spaces)
            // author
            // authored date (unix time)
            // committer
            // committed date (unix time)
            // encoding (may be blank)
            // diff notes
            // ...

            var lines = data.Split('\n');

            var guid = ObjectId.Parse(lines[0]);

            // TODO: we can use this to add more relationship info like gitk does if wanted
            var treeGuid = ObjectId.Parse(lines[1]);

            // TODO: we can use this to add more relationship info like gitk does if wanted
            var parentGuids = lines[2].Split(' ').Where(id => id.IsNotNullOrWhitespace()).Select(id => ObjectId.Parse(id)).ToList();
            var author = module.ReEncodeStringFromLossless(lines[3]);
            var authorDate = DateTimeUtils.ParseUnixTime(lines[4]);
            var committer = module.ReEncodeStringFromLossless(lines[5]);
            var commitDate = DateTimeUtils.ParseUnixTime(lines[6]);
            var commitEncoding = lines[7];
            var message = ProcessDiffNotes(startIndex: 8, lines);

            // commit message is not re-encoded by git when format is given
            var body = module.ReEncodeCommitMessage(message, commitEncoding);

            return new CommitData(guid, treeGuid, parentGuids, author, authorDate, committer, commitDate, body);
        }

        /// <inheritdoc />
        public CommitData CreateFromRevision(GitRevision revision, IReadOnlyList<ObjectId> children)
        {
            if (revision == null)
            {
                throw new ArgumentNullException(nameof(revision));
            }

            if (revision.ObjectId == null)
            {
                throw new ArgumentException($"Cannot have a null {nameof(GitRevision.ObjectId)}.", nameof(revision));
            }

            return new CommitData(revision.ObjectId, revision.TreeGuid, revision.ParentIds,
                string.Format("{0} <{1}>", revision.Author, revision.AuthorEmail), revision.AuthorDate,
                string.Format("{0} <{1}>", revision.Committer, revision.CommitterEmail), revision.CommitDate,
                revision.Body ?? revision.Subject)
            { ChildIds = children };
        }

        [NotNull]
        private IGitModule GetModule()
        {
            var module = _getModule();

            if (module == null)
            {
                throw new ArgumentException($"Require a valid instance of {nameof(IGitModule)}");
            }

            return module;
        }

        [ContractAnnotation("=>false,error:notnull,data:null")]
        [ContractAnnotation("=>true,error:null,data:notnull")]
        private bool TryGetCommitLog([NotNull] string commitId, [NotNull] string format, out string error, out string data)
        {
            if (commitId.IsArtificial())
            {
                data = null;
                error = "No log information for artificial commits";
                return false;
            }

            var arguments = new GitArgumentBuilder("log")
            {
                "-1",
                $"--pretty=\"format:{format}\"",
                commitId
            };

            // Do not cache this command, since notes can be added
            data = GetModule().GitExecutable.GetOutput(arguments, outputEncoding: GitModule.LosslessEncoding);

            if (GitModule.IsGitErrorMessage(data))
            {
                error = "Cannot find commit " + commitId;
                return false;
            }

            error = null;
            return true;
        }

        [NotNull]
        private static string ProcessDiffNotes(int startIndex, [NotNull, ItemNotNull] string[] lines)
        {
            int endIndex = lines.Length - 1;
            if (lines[endIndex] == "Notes:")
            {
                endIndex--;
            }

            var message = new StringBuilder();
            bool notesStart = false;

            for (int i = startIndex; i <= endIndex; i++)
            {
                string line = lines[i];

                if (notesStart)
                {
                    message.Append("    ");
                }

                message.AppendLine(line);

                if (line == "Notes:")
                {
                    notesStart = true;
                }
            }

            return message.ToString();
        }
    }
}