using System.Diagnostics.CodeAnalysis;
using System.Text;
using GitCommands.Git.Extensions;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUIPluginInterfaces;

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
        /// <param name="cache">Allow caching of the Git command, should only be used if commitId is a sha and Notes are not used.</param>
        CommitData? GetCommitData(string commitId, bool cache = false);

        /// <summary>
        /// Updates the <see cref="CommitData.Body"/> (commit message) property of <paramref name="commitData"/>.
        /// </summary>
        void UpdateBody(CommitData commitData, bool appendNotesOnly, out string? error);
    }

    public sealed class CommitDataManager : ICommitDataManager
    {
        private readonly Func<IGitModule> _getModule;

        public CommitDataManager(Func<IGitModule> getModule)
        {
            _getModule = getModule;
        }

        /// <inheritdoc />
        public void UpdateBody(CommitData commitData, bool appendNotesOnly, out string? error)
        {
            const string BodyAndNotesFormat = "%B%nNotes:%n%-N";
            const string NotesFormat = "%-N";

            if (!TryGetCommitLog(commitData.ObjectId.ToString(), appendNotesOnly ? NotesFormat : BodyAndNotesFormat, out error, out string? data, cache: false))
            {
                return;
            }

            if (appendNotesOnly)
            {
                if (!string.IsNullOrWhiteSpace(data))
                {
                    commitData.Body += $"\nNotes:\n    {GetModule().ReEncodeCommitMessage(data.Replace('\v', '\n'))}";
                }
            }
            else
            {
                string[] lines = data.Split(Delimiters.LineAndVerticalFeed);

                // Commit message is not re-encoded by Git when format is given
                commitData.Body = GetModule().ReEncodeCommitMessage(ProcessDiffNotes(startIndex: 0, lines));
            }
        }

        /// <inheritdoc />
        public CommitData? GetCommitData(string commitId, bool includeNotes = false)
        {
            GitRevision? revision = new RevisionReader(GetModule(), allBodies: true).GetRevision(commitId, hasNotes: includeNotes, throwOnError: false, cancellationToken: default);
            return revision is not null
                ? CreateFromRevision(revision, null)
                : null;
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

            return new CommitData(revision.ObjectId, revision.ParentIds,
                string.Format("{0} <{1}>", revision.Author, revision.AuthorEmail), revision.AuthorDate,
                string.Format("{0} <{1}>", revision.Committer, revision.CommitterEmail), revision.CommitDate,
                revision.Body ?? revision.Subject)
            { ChildIds = children };
        }

        private IGitModule GetModule()
        {
            IGitModule module = _getModule();

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
                commitId.Quote()
            };

            // This command can be cached if commitId is a git sha and Notes are ignored
            DebugHelpers.Assert(!cache || ObjectId.TryParse(commitId, out _), $"git-log cache should be used only for sha ({commitId})");

            ExecutionResult exec = GetModule().GitExecutable.Execute(arguments,
                outputEncoding: GitModule.LosslessEncoding,
                cache: cache ? GitModule.GitCommandCache : null, throwOnErrorExit: false);

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
                // No Notes, ignore
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
