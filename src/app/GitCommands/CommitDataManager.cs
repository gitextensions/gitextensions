using System.Diagnostics.CodeAnalysis;
using GitCommands.Git.Extensions;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUIPluginInterfaces;

namespace GitCommands;

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
    /// <param name="includeNotes">Include Notes with the commit info. This also means that the Git command is not cached.
    /// Note that Notes are only needed if the full Body with Notes is to be used, regardless of Settings.</param>
    CommitData? GetCommitData(string commitId, bool includeNotes = false);

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
        const string BodyAndNotesFormat = $"%B{RevisionReader.NotesFormat}";
        const string NotesFormat = "%N";

        if (!TryGetCommitLog(commitData.ObjectId.ToString(), appendNotesOnly ? NotesFormat : BodyAndNotesFormat, out error, out string? data, cache: false))
        {
            return;
        }

        // Commit message is not re-encoded by Git when format is given
        data = GetModule().ReEncodeCommitMessage(data.Replace('\v', '\n'));

        if (appendNotesOnly)
        {
            commitData.Notes = data;
            return;
        }

        int splitPos = data.LastIndexOf(RevisionReader.NotesMarkerWithoutTrailingLF);
        commitData.Body = data[0..splitPos].TrimEnd();
        splitPos += RevisionReader.NotesMarkerWithoutTrailingLF.Length + /*LF*/ 1;
        commitData.Notes = splitPos >= data.Length ? "" : data[splitPos..];
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
        ArgumentNullException.ThrowIfNull(revision);

        if (revision.ObjectId is null)
        {
            throw new ArgumentException($"Cannot have a null {nameof(GitRevision.ObjectId)}.", nameof(revision));
        }

        return new CommitData(revision.ObjectId, revision.ParentIds,
            FormatUser(revision.Author, revision.AuthorEmail), revision.AuthorDate,
            FormatUser(revision.Committer, revision.CommitterEmail), revision.CommitDate,
            revision.Body ?? revision.Subject)
        { ChildIds = children, Notes = revision.Notes };

        static string FormatUser(string user, string email) => string.IsNullOrWhiteSpace(email) ? user : $"{user} <{email}>";
    }

    private IGitModule GetModule()
        => _getModule() ?? throw new ArgumentException($"Require a valid instance of {nameof(IGitModule)}");

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
}
