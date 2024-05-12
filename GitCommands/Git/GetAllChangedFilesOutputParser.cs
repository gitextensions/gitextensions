using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;

namespace GitCommands.Git
{
    /// <summary>
    /// Provides a parser for output of <see cref="Commands.GetAllChangedFiles"/> command.
    /// </summary>
    public class GetAllChangedFilesOutputParser
    {
        private readonly Func<IGitModule> _getModule;

        public GetAllChangedFilesOutputParser(Func<IGitModule> getModule)
        {
            _getModule = getModule;
        }

        /// <summary>
        /// Parse the output from git-status --porcelain=2 -z
        /// Note that the caller should check for fatal errors in the Git output.
        /// </summary>
        /// <param name="getAllChangedFilesCommandOutput">An output of <see cref="Commands.GetAllChangedFiles"/> command.</param>
        /// <returns>list with the parsed GitItemStatus.</returns>
        /// <seealso href="https://git-scm.com/docs/git-status"/>
        public IReadOnlyList<GitItemStatus> Parse(string getAllChangedFilesCommandOutput)
        {
            return GetAllChangedFilesFromString_v2(getAllChangedFilesCommandOutput);
        }

        /// <summary>
        /// Parse git-status --porcelain=1 and git-diff --name-status
        /// Outputs are similar, except that git-status has status for both worktree and index.
        /// </summary>
        /// <param name="getAllChangedFilesCommandOutput">An output of <see cref="Commands.GetAllChangedFiles"/> command.</param>
        /// <param name="fromDiff">Parse git-diff.</param>
        /// <param name="staged">The staged status <see cref="GitItemStatus"/>, only relevant for git-diff (parsed for git-status).</param>
        /// <returns>list with the git items.</returns>
        internal List<GitItemStatus> GetAllChangedFilesFromString_v1(string getAllChangedFilesCommandOutput, bool fromDiff, StagedStatus staged)
        {
            List<GitItemStatus> diffFiles = [];

            if (string.IsNullOrEmpty(getAllChangedFilesCommandOutput))
            {
                return diffFiles;
            }

            string trimmedStatus = RemoveWarnings(getAllChangedFilesCommandOutput);

            // Doesn't work with removed submodules
            IReadOnlyList<string> submodules = GetModule().GetSubmodulesLocalPaths();

            // Split all files on '\0' (WE NEED ALL COMMANDS TO BE RUN WITH -z! THIS IS ALSO IMPORTANT FOR ENCODING ISSUES!)
            string[] files = trimmedStatus.Split(Delimiters.Null, StringSplitOptions.RemoveEmptyEntries);
            for (int n = 0; n < files.Length; n++)
            {
                if (string.IsNullOrEmpty(files[n]))
                {
                    continue;
                }

                int splitIndex;
                if (fromDiff)
                {
                    splitIndex = -1;
                }
                else
                {
                    // Note that this fails for files with spaces (git-status --porcelain=1 is deprecated)
                    splitIndex = files[n].IndexOfAny(Delimiters.TabAndSpace, 1);
                }

                string status;
                string fileName;

                if (splitIndex < 0)
                {
                    if (n >= files.Length - 1)
                    {
                        // Illegal, ignore
                        continue;
                    }

                    status = files[n];
                    fileName = files[n + 1];
                    n++;
                }
                else
                {
                    status = files[n][..splitIndex];
                    fileName = files[n][splitIndex..];
                }

                char x = status[0];
                char y = status.Length > 1 ? status[1] : GitItemStatusConverter.UnmodifiedStatus_v1;

                if (fromDiff && staged == StagedStatus.WorkTree && x == GitItemStatusConverter.UnmergedStatus)
                {
                    // git-diff has two lines to inform that a file is modified and has a merge conflict
                    continue;
                }

                // Skip unmerged where both are modified: Only worktree interesting.
                if ((x != GitItemStatusConverter.UntrackedStatus && x != GitItemStatusConverter.IgnoredStatus && x != GitItemStatusConverter.UnmodifiedStatus_v1)
                    || x != GitItemStatusConverter.UnmergedStatus
                    || y != GitItemStatusConverter.UnmergedStatus)
                {
                    GitItemStatus gitItemStatusX;
                    StagedStatus stagedX = fromDiff ? staged : StagedStatus.Index;
                    if (x == GitItemStatusConverter.RenamedStatus || x == GitItemStatusConverter.CopiedStatus)
                    {
                        // Find renamed files...
                        string nextFile = n + 1 < files.Length ? files[n + 1] : "";
                        gitItemStatusX = GitItemStatusFromCopyRename(stagedX, fromDiff, nextFile, fileName, x, status);
                        n++;
                    }
                    else
                    {
                        gitItemStatusX = GitItemStatusConverter.FromStatusCharacter(stagedX, fileName, x);
                    }

                    if (submodules.Contains(gitItemStatusX.Name))
                    {
                        gitItemStatusX.IsSubmodule = true;
                    }

                    diffFiles.Add(gitItemStatusX);
                }

                if (fromDiff || y == GitItemStatusConverter.UnmodifiedStatus_v1)
                {
                    continue;
                }

                GitItemStatus gitItemStatusY;
                StagedStatus stagedY = StagedStatus.WorkTree;
                if (y == GitItemStatusConverter.RenamedStatus || y == GitItemStatusConverter.CopiedStatus)
                {
                    // Find renamed files...
                    string nextFile = n + 1 < files.Length ? files[n + 1] : "";
                    gitItemStatusY = GitItemStatusFromCopyRename(stagedY, false, nextFile, fileName, y, status);
                    n++;
                }
                else
                {
                    gitItemStatusY = GitItemStatusConverter.FromStatusCharacter(stagedY, fileName, y);
                }

                if (submodules.Contains(gitItemStatusY.Name))
                {
                    gitItemStatusY.IsSubmodule = true;
                }

                diffFiles.Add(gitItemStatusY);
            }

            return diffFiles;
        }

        /// <summary>
        /// Parse the output from git-status --porcelain=2.
        /// </summary>
        /// <param name="getAllChangedFilesCommandOutput">output from the git command.</param>
        /// <returns>list with the parsed GitItemStatus.</returns>
        private static IReadOnlyList<GitItemStatus> GetAllChangedFilesFromString_v2(string getAllChangedFilesCommandOutput)
        {
            List<GitItemStatus> diffFiles = [];

            if (string.IsNullOrEmpty(getAllChangedFilesCommandOutput))
            {
                return diffFiles;
            }

            string trimmedStatus = RemoveWarnings(getAllChangedFilesCommandOutput);

            // Split all files on '\0'
            string[] files = trimmedStatus.Split(Delimiters.Null, StringSplitOptions.RemoveEmptyEntries);
            for (int n = 0; n < files.Length; n++)
            {
                string line = files[n];
                if (line.Length <= 2 || line[1] != ' ')
                {
                    // Illegal info, like error output
                    continue;
                }

                char entryType = line[0];

                if (entryType == GitItemStatusConverter.UntrackedStatus || entryType == GitItemStatusConverter.IgnoredStatus)
                {
                    // Untracked and ignored with just the path following, supply dummy data for most info.
                    string otherFileName = line[2..];
                    const string NotSumoduleEntry = "N...";
                    UpdateItemStatus(entryType, false, NotSumoduleEntry, otherFileName, null, null);
                    continue;
                }

                const char OrdinaryEntry = '1';
                const char RenamedEntry = '2';
                const char UnmergedEntry = 'u';

                if ((entryType != OrdinaryEntry && entryType != RenamedEntry && entryType != UnmergedEntry) || line.Length <= 3)
                {
                    // Illegal entry type
                    continue;
                }

                // Parse from git-status documentation, assuming SHA-1 is used
                // Ignore octal and treeGuid
                // 1 XY subm <mH> <mI> <mW> <hH> <hI> <path>
                // renamed:
                // 2 XY subm <mH> <mI> <mW> <hH> <hI> <X><score> <path><sep><origPath>
                // worktree (merge conflicts)
                // u XY subm <m1> <m2> <m3> <mW> <h1> <h2> <h3> <path>

                char x = line[2];
                char y = line[3];
                string fileName;
                string? oldFileName = null;
                string? renamePercent = null;
                string subm = line.Substring(5, 4);

                if (entryType == OrdinaryEntry)
                {
                    DebugHelpers.Assert(line.Length > 113, "Cannot parse line:" + line);
                    fileName = line[113..];
                }
                else if (entryType == RenamedEntry)
                {
                    DebugHelpers.Assert(n + 1 < files.Length, "Cannot parse renamed:" + line);

                    // Find renamed files...
                    string[] renames = line[114..].Split(Delimiters.Space, 2);
                    renamePercent = renames[0];
                    fileName = renames[1];
                    oldFileName = files[++n];
                }
                else if (entryType == UnmergedEntry)
                {
                    DebugHelpers.Assert(line.Length > 161, "Cannot parse unmerged:" + line);
                    fileName = line[161..];
                }
                else
                {
                    // illegal, already checked
                    continue;
                }

                // Skip unmerged where both are modified: Only worktree interesting.
                if (entryType != UnmergedEntry || x != GitItemStatusConverter.UnmergedStatus || y != GitItemStatusConverter.UnmergedStatus)
                {
                    UpdateItemStatus(x, true, subm, fileName, oldFileName, renamePercent);
                }

                UpdateItemStatus(y, false, subm, fileName, oldFileName, renamePercent);
            }

            return diffFiles;

            void UpdateItemStatus(char x, bool isIndex, string subm, string fileName, string? oldFileName, string? renamePercent)
            {
                if (x == GitItemStatusConverter.UnmodifiedStatus_v2)
                {
                    return;
                }

                StagedStatus staged = isIndex ? StagedStatus.Index : StagedStatus.WorkTree;
                GitItemStatus gitItemStatus = GitItemStatusConverter.FromStatusCharacter(staged, fileName, x);
                if (oldFileName is not null)
                {
                    gitItemStatus.OldName = oldFileName;
                }

                if (renamePercent is not null)
                {
                    gitItemStatus.RenameCopyPercentage = renamePercent;
                }

                const char SubmoduleState = 'S';
                if (subm[0] == SubmoduleState)
                {
                    gitItemStatus.IsSubmodule = true;

                    if (!isIndex)
                    {
                        // Slight modification on how the following flags are used
                        // Changed commit
                        gitItemStatus.IsChanged = subm[1] == GitItemStatusConverter.CopiedStatus;
                        gitItemStatus.IsDirty = subm[2] == GitItemStatusConverter.ModifiedStatus || subm[3] == GitItemStatusConverter.UnmergedStatus;
                    }
                }

                diffFiles.Add(gitItemStatus);
            }
        }

        private static GitItemStatus GitItemStatusFromCopyRename(StagedStatus staged, bool fromDiff, string nextFile, string fileName, char x, string status)
        {
            GitItemStatus gitItemStatus;

            // Find renamed files...
            if (fromDiff)
            {
                gitItemStatus = new GitItemStatus(name: nextFile);
                gitItemStatus.OldName = fileName;
            }
            else
            {
                gitItemStatus = new GitItemStatus(name: fileName);
                gitItemStatus.OldName = nextFile;
            }

            gitItemStatus.IsNew = false;
            gitItemStatus.IsChanged = false;
            gitItemStatus.IsDeleted = false;
            if (x == GitItemStatusConverter.RenamedStatus)
            {
                gitItemStatus.IsRenamed = true;
            }
            else
            {
                gitItemStatus.IsCopied = true;
            }

            gitItemStatus.IsTracked = true;
            if (status.Length > 2)
            {
                gitItemStatus.RenameCopyPercentage = status[1..];
            }

            gitItemStatus.Staged = staged;

            return gitItemStatus;
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

        private static string RemoveWarnings(string statusString)
        {
            // The status string from git-diff can show warnings. See tests
            string trimmedStatus = statusString;
            int lastNewLinePos = trimmedStatus.LastIndexOfAny(Delimiters.LineFeedAndCarriageReturn);
            while (lastNewLinePos >= 0)
            {
                if (lastNewLinePos == 0)
                {
                    trimmedStatus = trimmedStatus.Remove(0, 1);
                    break;
                }

                // Error always end with \n and start at previous index
                int ind = trimmedStatus.LastIndexOfAny(Delimiters.LineFeedCarriageReturnAndNull, lastNewLinePos - 1);

                trimmedStatus = trimmedStatus.Remove(ind + 1, lastNewLinePos - ind);
                lastNewLinePos = trimmedStatus.LastIndexOfAny(Delimiters.LineFeedAndCarriageReturn);
            }

            return trimmedStatus;
        }
    }
}
