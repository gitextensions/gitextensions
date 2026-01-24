using System.Text.RegularExpressions;
using GitExtensions.Extensibility.Git;
using Microsoft;

namespace GitCommands.Git;

public static partial class SubmoduleHelpers
{
    [GeneratedRegex(@"diff --git\s+[ab]/(?<filenamea>.+)\s+[ba]/(?<filenameb>.+)", RegexOptions.ExplicitCapture)]
    private static partial Regex DiffCommandRegex { get; }
    [GeneratedRegex(@"diff --cc (?<filenamea>.+)", RegexOptions.ExplicitCapture)]
    private static partial Regex CombinedDiffCommandRegex { get; }

    public static async Task<GitSubmoduleStatus?> GetSubmoduleDiffChangesAsync(IGitModule module, string? fileName, string? oldFileName, ObjectId? firstId, ObjectId? secondId, CancellationToken cancellationToken)
    {
        (Patch? patch, string? errorMessage) = await module.GetSingleDiffAsync(firstId, secondId, fileName, oldFileName, "", GitModule.SystemEncoding, cacheResult: true, isTracked: true, useGitColoring: false, commandConfiguration: null, cancellationToken: cancellationToken).ConfigureAwait(false);
        return GetSubmoduleChanges(patch, errorMessage, module, fileName);
    }

    public static async Task<GitSubmoduleStatus?> GetSubmoduleCurrentChangesAsync(IGitModule module, string? fileName, string? oldFileName, bool staged, bool noLocks = false)
    {
        Patch? patch = await module.GetCurrentChangesAsync(fileName, oldFileName, staged, extraDiffArguments: "", noLocks: noLocks).ConfigureAwait(false);
        return GetSubmoduleChanges(patch, "", module, fileName);
    }

    private static GitSubmoduleStatus GetSubmoduleChanges(Patch? patch, string? errorMessage, IGitModule module, string? fileName)
    {
        if (!string.IsNullOrEmpty(errorMessage))
        {
            // (some) Git errors, propagate
            return new GitSubmoduleStatus(errorMessage, null, false, null, null, null, null, null, GetSubmoduleStatus);
        }

        if (string.IsNullOrEmpty(patch?.Text))
        {
            // Note that empty diff will give null Patch too, not necessarily an error
            return null;
        }

        IGitModule submodule = module.GetSubmodule(fileName);
        CommitDataManager commitDataManager = new(() => submodule);
        return ParseSubmoduleStatus(patch.Text, submodule, commitId => commitDataManager.GetCommitData(commitId));
    }

    private static GitSubmoduleStatus ParseSubmoduleStatus(string text, IGitModule submodule, Func<string, CommitData?> getCommitData)
    {
        string? name = null;
        string? oldName = null;
        bool isDirty = false;
        ObjectId? commitId = null;
        ObjectId? oldCommitId = null;
        int? addedCommits = null;
        int? removedCommits = null;

        using (StringReader reader = new(text))
        {
            string? line = reader.ReadLine();

            if (line is not null)
            {
                Match match = DiffCommandRegex.Match(line);
                if (match.Groups.Count > 1)
                {
                    name = match.Groups["filenamea"].Value;
                    oldName = match.Groups["filenameb"].Value;
                }
                else
                {
                    match = CombinedDiffCommandRegex.Match(line);
                    if (match.Groups.Count > 1)
                    {
                        name = match.Groups["filenamea"].Value;
                        oldName = name;
                    }
                }
            }

            while ((line = reader.ReadLine()) is not null)
            {
                // We are looking for lines resembling:
                //
                // -Subproject commit bfef4454fc51e345051ee5bf66686dc28deed627
                // +Subproject commit 8b20498b954609770205c2cc794b868b4ac3ee69-dirty

                if (!line.Contains("Subproject"))
                {
                    continue;
                }

                char c = line[0];
                const string commitStr = "commit ";
                string hash = "";
                int pos = line.IndexOf(commitStr);
                if (pos >= 0)
                {
                    hash = line[(pos + commitStr.Length)..];
                }

                bool endsWithDirty = hash.EndsWith("-dirty");
                hash = hash.Replace("-dirty", "");
                if (c == '-')
                {
                    oldCommitId = ObjectId.Parse(hash);
                }
                else if (c == '+')
                {
                    commitId = ObjectId.Parse(hash);
                    isDirty = endsWithDirty;
                }

                // TODO: Support combined merge
            }
        }

        Validates.NotNull(name);

        if (!submodule.IsValidGitWorkingDir())
        {
            // cannot calculate the data
            getCommitData = null;
        }

        // Force calculation of caches, could be separate
        if (oldCommitId is not null && commitId is not null)
        {
            if (oldCommitId == commitId)
            {
                addedCommits = 0;
                removedCommits = 0;
            }
            else if (submodule.IsValidGitWorkingDir())
            {
                (addedCommits, removedCommits) = submodule.GetCommitRangeDiffCount(commitId, oldCommitId);
            }
        }

        GitSubmoduleStatus status = new(name, oldName, isDirty, commitId, oldCommitId, addedCommits, removedCommits, getCommitData, GetSubmoduleStatus);

        // Force calculation of caches, could be separate
        _ = status.Status;
        _ = status.CommitData;
        _ = status.OldCommitData;

        return status;
    }

    private static SubmoduleStatus GetSubmoduleStatus(GitSubmoduleStatus submoduleStatus)
    {
        if (submoduleStatus.OldCommit is null)
        {
            return SubmoduleStatus.NewSubmodule;
        }

        if (submoduleStatus.Commit is null)
        {
            return SubmoduleStatus.RemovedSubmodule;
        }

        if (submoduleStatus.Commit == submoduleStatus.OldCommit)
        {
            return SubmoduleStatus.SameCommit;
        }

        // From this on, the status is by default Modified

        if (submoduleStatus.AddedCommits is null || submoduleStatus.RemovedCommits is null)
        {
            return SubmoduleStatus.Modified;
        }

        if (submoduleStatus.AddedCommits > 0 && submoduleStatus.RemovedCommits == 0)
        {
            return SubmoduleStatus.FastForward;
        }

        if (submoduleStatus.AddedCommits == 0 && submoduleStatus.RemovedCommits > 0)
        {
            return SubmoduleStatus.Rewind;
        }

        if (submoduleStatus.CommitData is null || submoduleStatus.OldCommitData is null)
        {
            return SubmoduleStatus.Modified;
        }

        if (submoduleStatus.CommitData.CommitDate > submoduleStatus.OldCommitData.CommitDate)
        {
            return SubmoduleStatus.NewerTime;
        }

        if (submoduleStatus.CommitData.CommitDate < submoduleStatus.OldCommitData.CommitDate)
        {
            return SubmoduleStatus.OlderTime;
        }

        return SubmoduleStatus.Modified;
    }

    internal readonly struct TestAccessor
    {
        internal static GitSubmoduleStatus ParseSubmoduleStatus(string text, IGitModule submodule, Func<string, CommitData?> getCommitData)
            => SubmoduleHelpers.ParseSubmoduleStatus(text, submodule, getCommitData);
    }
}
