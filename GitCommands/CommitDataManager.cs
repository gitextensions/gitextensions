using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using GitCommands.Git.Extensions;
using GitExtUtils;
using GitUIPluginInterfaces;
using Microsoft;

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
        CommitData CreateFromRevision(GitRevision revision, IReadOnlyList<ObjectId>? children);

        /// <summary>
        /// Gets <see cref="CommitData"/> for the specified <paramref name="commitId"/>.
        /// </summary>
        /// <param name="commitId">The sha or Git reference.</param>
        /// <param name="error">error message for the Git command</param>
        /// <param name="cache">Allow caching of the Git command, should only be used if commitId is a sha and Notes are not used.</param>
        CommitData? GetCommitData(string commitId, out string? error, bool cache = false);

        /// <summary>
        /// Updates the <see cref="CommitData.Body"/> (commit message) property of <paramref name="commitData"/>.
        /// </summary>
        void UpdateBody(CommitData commitData, out string? error);
    }

    public sealed class CommitDataManager : ICommitDataManager
    {
        private const string CommitDataFormat = "%H%n%T%n%P%n%aN <%aE>%n%at%n%cN <%cE>%n%ct%n%e%n%B";
        private const string CommitDataWithNotesFormat = "%H%n%T%n%P%n%aN <%aE>%n%at%n%cN <%cE>%n%ct%n%e%n%B%nNotes:%n%-N";
        private const string BodyAndNotesFormat = "%H%n%e%n%B%nNotes:%n%-N";

        private readonly Func<IGitModule> _getModule;

        public CommitDataManager(Func<IGitModule> getModule)
        {
            _getModule = getModule;
        }

        /// <inheritdoc />
        public void UpdateBody(CommitData commitData, out string? error)
        {
            if (!TryGetCommitLog(commitData.ObjectId.ToString(), BodyAndNotesFormat, out error, out var data, cache: false))
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

            var lines = data.Split(Delimiters.LineFeed);

            var guid = lines[0];
            var commitEncoding = lines[1];
            var message = ProcessDiffNotes(startIndex: 2, lines);

            Debug.Assert(commitData.ObjectId.ToString() == guid, "Guid in response doesn't match that of request");

            // Commit message is not re-encoded by git when format is given
            commitData.Body = GetModule().ReEncodeCommitMessage(message, commitEncoding);
        }

        /// <inheritdoc />
        public CommitData? GetCommitData(string commitId, out string? error, bool cache = false)
        {
            return TryGetCommitLog(commitId, cache ? CommitDataFormat : CommitDataWithNotesFormat, out error, out var info, cache)
                ? CreateFromFormattedData(info)
                : null;
        }

        /// <summary>
        /// Parses <paramref name="data"/> into a <see cref="CommitData"/> object.
        /// </summary>
        /// <param name="data">Data produced by a <c>git log</c> or <c>git show</c> command where <c>--format</c>
        /// was provided the string <see cref="CommitDataFormat"/>.</param>
        /// <returns>CommitData object populated with parsed info from git string.</returns>
        internal CommitData CreateFromFormattedData(string data)
        {
            if (data is null)
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

            var lines = data.Split(Delimiters.LineFeed);

            var guid = ObjectId.Parse(lines[0]);

            // TODO: we can use this to add more relationship info like gitk does if wanted
            var treeGuid = ObjectId.Parse(lines[1]);

            // TODO: we can use this to add more relationship info like gitk does if wanted
            var parentIds = lines[2].LazySplit(' ').Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => ObjectId.Parse(id)).ToList();
            var author = module.ReEncodeStringFromLossless(lines[3]);
            var authorDate = DateTimeUtils.ParseUnixTime(lines[4]);
            var committer = module.ReEncodeStringFromLossless(lines[5]);
            var commitDate = DateTimeUtils.ParseUnixTime(lines[6]);
            var commitEncoding = lines[7];
            var message = ProcessDiffNotes(startIndex: 8, lines);

            // commit message is not re-encoded by git when format is given
            var body = module.ReEncodeCommitMessage(message, commitEncoding);

            Validates.NotNull(author);
            Validates.NotNull(committer);

            return new CommitData(guid, treeGuid, parentIds, author, authorDate, committer, commitDate, body);
        }

        /// <inheritdoc />
        public CommitData CreateFromRevision(GitRevision revision, IReadOnlyList<ObjectId>? children)
        {
            if (revision is null)
            {
                throw new ArgumentNullException(nameof(revision));
            }

            if (revision.ObjectId is null)
            {
                throw new ArgumentException($"Cannot have a null {nameof(GitRevision.ObjectId)}.", nameof(revision));
            }

            if (revision.ParentIds is null)
            {
                throw new ArgumentException($"Cannot have null {nameof(GitRevision.ParentIds)}.", nameof(revision));
            }

            return new CommitData(revision.ObjectId, revision.TreeGuid, revision.ParentIds,
                string.Format("{0} <{1}>", revision.Author, revision.AuthorEmail), revision.AuthorDate,
                string.Format("{0} <{1}>", revision.Committer, revision.CommitterEmail), revision.CommitDate,
                revision.Body ?? revision.Subject)
            { ChildIds = children };
        }

        private IGitModule GetModule()
        {
            var module = _getModule();

            if (module is null)
            {
                throw new ArgumentException($"Require a valid instance of {nameof(IGitModule)}");
            }

            return module;
        }

        private bool TryGetCommitLog(string commitId, string format, [NotNullWhen(returnValue: false)] out string? error, [NotNullWhen(returnValue: true)] out string? data, bool cache)
        {
            if (commitId.IsArtificial())
            {
                data = null;
                error = "No log information for artificial commits";
                return false;
            }

            GitArgumentBuilder arguments = new("log")
            {
                "-1",
                $"--pretty=\"format:{format}\"",
                commitId
            };

            // This command can be cached if commitId is a git sha and Notes are ignored
            Debug.Assert(!cache || ObjectId.TryParse(commitId, out _), $"git-log cache should be used only for sha ({commitId})");

            ExecutionResult exec = GetModule().GitExecutable.Execute(arguments,
                outputEncoding: GitModule.LosslessEncoding,
                cache: cache ? GitModule.GitCommandCache : null);

            if (!exec.ExitedSuccessfully)
            {
                data = null;
                error = "Cannot find commit " + commitId;
                return false;
            }

            data = exec.StandardOutput;
            error = null;
            return true;
        }

        private static string ProcessDiffNotes(int startIndex, string[] lines)
        {
            int endIndex = lines.Length - 1;
            if (lines[endIndex] == "Notes:")
            {
                endIndex--;
            }

            StringBuilder message = new();
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
