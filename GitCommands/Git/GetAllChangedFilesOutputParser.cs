using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GitCommands.Git.Commands;
using GitUIPluginInterfaces;

namespace GitCommands.Git
{
    /// <summary>
    /// Provides a parser for output of <see cref="GitCommandHelpers.GetAllChangedFilesCmd"/> command.
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
        /// <param name="getAllChangedFilesCommandOutput">An output of <see cref="GitCommandHelpers.GetAllChangedFilesCmd"/> command.</param>
        /// <returns>list with the parsed GitItemStatus.</returns>
        /// <seealso href="https://git-scm.com/docs/git-status"/>
        public IReadOnlyList<GitItemStatus> Parse(string getAllChangedFilesCommandOutput)
        {
            if (GitVersion.Current.SupportStatusPorcelainV2)
            {
                return GetAllChangedFilesFromString_v2(getAllChangedFilesCommandOutput);
            }
            else
            {
                return GetAllChangedFilesFromString_v1(getAllChangedFilesCommandOutput, false, StagedStatus.Unset);
            }
        }

        /// <summary>
        /// Parse git-status --porcelain=1 and git-diff --name-status
        /// Outputs are similar, except that git-status has status for both worktree and index.
        /// </summary>
        /// <param name="getAllChangedFilesCommandOutput">An output of <see cref="GitCommandHelpers.GetAllChangedFilesCmd"/> command.</param>
        /// <param name="fromDiff">Parse git-diff.</param>
        /// <param name="staged">The staged status <see cref="GitItemStatus"/>, only relevant for git-diff (parsed for git-status).</param>
        /// <returns>list with the git items.</returns>
        internal List<GitItemStatus> GetAllChangedFilesFromString_v1(string getAllChangedFilesCommandOutput, bool fromDiff, StagedStatus staged)
        {
            List<GitItemStatus> diffFiles = new();

            if (string.IsNullOrEmpty(getAllChangedFilesCommandOutput))
            {
                return diffFiles;
            }

            string trimmedStatus = RemoveWarnings(getAllChangedFilesCommandOutput);

            // Doesn't work with removed submodules
            var submodules = GetModule().GetSubmodulesLocalPaths();

            // Split all files on '\0' (WE NEED ALL COMMANDS TO BE RUN WITH -z! THIS IS ALSO IMPORTANT FOR ENCODING ISSUES!)
            var files = trimmedStatus.Split(Delimiters.Null, StringSplitOptions.RemoveEmptyEntries);
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
                    status = files[n].Substring(0, splitIndex);
                    fileName = files[n].Substring(splitIndex);
                }

                char x = status[0];
                char y = status.Length > 1 ? status[1] : ' ';

                if (fromDiff && staged == StagedStatus.WorkTree && x == 'U')
                {
                    // git-diff has two lines to inform that a file is modified and has a merge conflict
                    continue;
                }

                if (x != '?' && x != '!' && x != ' ')
                {
                    GitItemStatus gitItemStatusX;
                    var stagedX = fromDiff ? staged : StagedStatus.Index;
                    if (x == 'R' || x == 'C')
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

                if (fromDiff || y == ' ')
                {
                    continue;
                }

                GitItemStatus gitItemStatusY;
                var stagedY = StagedStatus.WorkTree;
                if (y == 'R' || y == 'C')
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
            List<GitItemStatus> diffFiles = new();

            if (string.IsNullOrEmpty(getAllChangedFilesCommandOutput))
            {
                return diffFiles;
            }

            string trimmedStatus = RemoveWarnings(getAllChangedFilesCommandOutput);

            // Split all files on '\0'
            var files = trimmedStatus.Split(Delimiters.Null, StringSplitOptions.RemoveEmptyEntries);
            for (int n = 0; n < files.Length; n++)
            {
                string line = files[n];
                char entryType = line[0];

                if (entryType == '?' || entryType == '!')
                {
                    Debug.Assert(line.Length > 2 && line[1] == ' ', "Cannot parse for untracked:" + line);
                    string fileName = line.Substring(2);
                    UpdateItemStatus(entryType, false, "N...", fileName, null, null);
                }
                else if (entryType == '1' || entryType == '2' || entryType == 'u')
                {
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

                    if (entryType == '1')
                    {
                        Debug.Assert(line.Length > 113 && line[1] == ' ', "Cannot parse line:" + line);
                        fileName = line.Substring(113);
                    }
                    else if (entryType == '2')
                    {
                        Debug.Assert(line.Length > 2 && n + 1 < files.Length, "Cannot parse renamed:" + line);

                        // Find renamed files...
                        string[] renames = line.Substring(114).Split(Delimiters.Space, 2);
                        renamePercent = renames[0];
                        fileName = renames[1];
                        oldFileName = files[++n];
                    }
                    else if (entryType == 'u')
                    {
                        Debug.Assert(line.Length > 161, "Cannot parse unmerged:" + line);
                        fileName = line.Substring(161);
                    }
                    else
                    {
                        // illegal
                        continue;
                    }

                    UpdateItemStatus(x, true, subm, fileName, oldFileName, renamePercent);
                    UpdateItemStatus(y, false, subm, fileName, oldFileName, renamePercent);
                }
            }

            return diffFiles;

            void UpdateItemStatus(char x, bool isIndex, string subm, string fileName, string? oldFileName, string? renamePercent)
            {
                if (x == '.')
                {
                    return;
                }

                var staged = isIndex ? StagedStatus.Index : StagedStatus.WorkTree;
                GitItemStatus gitItemStatus = GitItemStatusConverter.FromStatusCharacter(staged, fileName, x);
                if (oldFileName is not null)
                {
                    gitItemStatus.OldName = oldFileName;
                }

                if (renamePercent is not null)
                {
                    gitItemStatus.RenameCopyPercentage = renamePercent;
                }

                if (subm[0] == 'S')
                {
                    gitItemStatus.IsSubmodule = true;

                    if (!isIndex)
                    {
                        // Slight modification on how the following flags are used
                        // Changed commit
                        gitItemStatus.IsChanged = subm[1] == 'C';
                        gitItemStatus.IsDirty = subm[2] == 'M' || subm[3] == 'U';
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
            if (x == 'R')
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
                gitItemStatus.RenameCopyPercentage = status.Substring(1);
            }

            gitItemStatus.Staged = staged;

            return gitItemStatus;
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

        private static string RemoveWarnings(string statusString)
        {
            // The status string from git-diff can show warnings. See tests
            var nl = new[] { '\n', '\r' };
            string trimmedStatus = statusString;
            int lastNewLinePos = trimmedStatus.LastIndexOfAny(nl);
            while (lastNewLinePos >= 0)
            {
                if (lastNewLinePos == 0)
                {
                    trimmedStatus = trimmedStatus.Remove(0, 1);
                    break;
                }

                // Error always end with \n and start at previous index
                int ind = trimmedStatus.LastIndexOfAny(new[] { '\n', '\r', '\0' }, lastNewLinePos - 1);

                trimmedStatus = trimmedStatus.Remove(ind + 1, lastNewLinePos - ind);
                lastNewLinePos = trimmedStatus.LastIndexOfAny(nl);
            }

            return trimmedStatus;
        }
    }
}
