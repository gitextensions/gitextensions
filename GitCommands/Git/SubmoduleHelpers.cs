using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using GitExtensions.Extensibility.Git;
using Microsoft;

namespace GitCommands.Git
{
    public static partial class SubmoduleHelpers
    {
        [GeneratedRegex(@"diff --git [^/\s]+/(?<filenamea>.+)\s[^/\s]+/(?<filenameb>.+)", RegexOptions.ExplicitCapture)]
        private static partial Regex DiffCommandRegex();
        [GeneratedRegex(@"diff --cc (?<filenamea>.+)", RegexOptions.ExplicitCapture)]
        private static partial Regex CombinedDiffCommandRegex();

        public static async Task<GitSubmoduleStatus?> GetCurrentSubmoduleChangesAsync(IGitModule module, string? fileName, string? oldFileName, ObjectId? firstId, ObjectId? secondId, CancellationToken cancellationToken)
        {
            (Patch? patch, string? errorMessage) = await module.GetSingleDiffAsync(firstId, secondId, fileName, oldFileName, "", GitModule.SystemEncoding, cacheResult: true, isTracked: true, useGitColoring: false, commandConfiguration: null, cancellationToken: cancellationToken).ConfigureAwait(false);
            return patch is null
                ? new GitSubmoduleStatus(errorMessage ?? "", null, false, null, null, null, null)
                : ParseSubmodulePatchStatus(patch, module, fileName);
        }

        public static async Task<GitSubmoduleStatus?> GetCurrentSubmoduleChangesAsync(IGitModule module, string? fileName, string? oldFileName, bool staged, bool noLocks = false)
        {
            Patch? patch = await module.GetCurrentChangesAsync(fileName, oldFileName, staged, "", noLocks: noLocks).ConfigureAwait(false);
            return ParseSubmodulePatchStatus(patch, module, fileName);
        }

        public static Task<GitSubmoduleStatus?> GetCurrentSubmoduleChangesAsync(IGitModule module, string submodule, bool noLocks = false)
        {
            return GetCurrentSubmoduleChangesAsync(module, submodule, submodule, false, noLocks: noLocks);
        }

        private static GitSubmoduleStatus? ParseSubmodulePatchStatus(Patch? patch, IGitModule module, string? fileName)
        {
            GitSubmoduleStatus? submoduleStatus = ParseSubmoduleStatus(patch?.Text, module, fileName);
            if (submoduleStatus is not null && submoduleStatus.Commit != submoduleStatus.OldCommit)
            {
                IGitModule submodule = submoduleStatus.GetSubmodule(module);
                submoduleStatus.CheckSubmoduleStatus(submodule);
            }

            return submoduleStatus;
        }

        [return: NotNullIfNotNull("text")]
        public static GitSubmoduleStatus? ParseSubmoduleStatus(string? text, IGitModule module, string? fileName)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

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
                    Match match = DiffCommandRegex().Match(line);
                    if (match.Groups.Count > 1)
                    {
                        name = match.Groups["filenamea"].Value;
                        oldName = match.Groups["filenameb"].Value;
                    }
                    else
                    {
                        match = CombinedDiffCommandRegex().Match(line);
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

            if (oldCommitId is not null && commitId is not null)
            {
                if (oldCommitId == commitId)
                {
                    addedCommits = 0;
                    removedCommits = 0;
                }
                else
                {
                    IGitModule submodule = module.GetSubmodule(fileName);
                    if (submodule.IsValidGitWorkingDir())
                    {
                        addedCommits = submodule.GetCommitCount(commitId.ToString(), oldCommitId.ToString(), cache: true, throwOnErrorExit: false);
                        removedCommits = submodule.GetCommitCount(oldCommitId.ToString(), commitId.ToString(), cache: true, throwOnErrorExit: false);
                    }
                }
            }

            Validates.NotNull(name);

            return new GitSubmoduleStatus(name, oldName, isDirty, commitId, oldCommitId, addedCommits, removedCommits);
        }
    }
}
