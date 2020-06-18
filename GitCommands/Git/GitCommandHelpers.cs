using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitCommands.Git;
using GitCommands.Patches;
using GitCommands.Utils;
using GitExtUtils;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands
{
    /// <summary>Specifies whether to check untracked files/directories (e.g. via 'git status')</summary>
    public enum UntrackedFilesMode
    {
        /// <summary>Default is <see cref="All"/>; when <see cref="UntrackedFilesMode"/> is NOT used, 'git status' uses <see cref="Normal"/>.</summary>
        Default = 0,

        /// <summary>Show no untracked files.</summary>
        No,

        /// <summary>Shows untracked files and directories.</summary>
        Normal,

        /// <summary>Shows untracked files and directories, and individual files in untracked directories.</summary>
        All
    }

    /// <summary>Specifies whether to ignore changes to submodules when looking for changes (e.g. via 'git status').</summary>
    public enum IgnoreSubmodulesMode
    {
        /// <summary>Default is <see cref="All"/> (hides all changes to submodules).</summary>
        Default = 0,

        /// <summary>Consider a submodule modified when it either:
        ///  contains untracked or modified files,
        ///  or its HEAD differs from the commit recorded in the superproject.</summary>
        None,

        /// <summary>Submodules NOT considered dirty when they only contain <i>untracked</i> content
        ///  (but they are still scanned for modified content).</summary>
        Untracked,

        /// <summary>Ignores all changes to the work tree of submodules,
        ///  only changes to the <i>commits</i> stored in the superproject are shown.</summary>
        Dirty,

        /// <summary>Hides all changes to submodules
        ///  (and suppresses the output of submodule summaries when the config option status.submodulesummary is set).</summary>
        All
    }

    /// <summary>Mode for 'git clean'</summary>
    public enum CleanMode
    {
        /// <summary>Only untracked files not in .gitignore, the default. Git clean without either -x or -X option.</summary>
        OnlyNonIgnored = 0,

        /// <summary>Only files included in any ignore list (.gitignore, $GIT_DIR/info/exclude). Git clean with -X option.</summary>
        OnlyIgnored,

        /// <summary>All files not tracked by Git. Git clean with  -x option.</summary>
        All
    }

    /// <summary>Arguments to 'git reset'.</summary>
    public enum ResetMode
    {
        /// <summary>(no option)</summary>
        ResetIndex = 0,

        /// <summary>--soft</summary>
        Soft,

        /// <summary>--mixed</summary>
        Mixed,

        /// <summary>--hard</summary>
        Hard,

        /// <summary>--merge</summary>
        Merge,

        /// <summary>--keep</summary>
        Keep

        // All options are not implemented, like --patch
    }

    public static class GitCommandHelpers
    {
        #region SSH / Plink

        private static readonly ISshPathLocator _sshPathLocatorInstance = new SshPathLocator();

        public static bool UseSsh(string arguments)
        {
            var x = !Plink() && DoArgumentsRequireSsh();
            return x || arguments.Contains("plink");

            bool DoArgumentsRequireSsh()
            {
                return (arguments.Contains("@") && arguments.Contains("://")) ||
                       (arguments.Contains("@") && arguments.Contains(":")) ||
                       arguments.Contains("ssh://") ||
                       arguments.Contains("http://") ||
                       arguments.Contains("git://") ||
                       arguments.Contains("push") ||
                       arguments.Contains("remote") ||
                       arguments.Contains("fetch") ||
                       arguments.Contains("pull");
            }
        }

        /// <summary>Un-sets the git SSH command path.</summary>
        public static void UnsetSsh()
        {
            Environment.SetEnvironmentVariable("GIT_SSH", "", EnvironmentVariableTarget.Process);
        }

        /// <summary>Sets the git SSH command path.</summary>
        public static void SetSsh(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                Environment.SetEnvironmentVariable("GIT_SSH", path, EnvironmentVariableTarget.Process);
            }
        }

        public static bool Plink()
        {
            var sshString = _sshPathLocatorInstance.Find(AppSettings.GitBinDir);

            return sshString.EndsWith("plink.exe", StringComparison.CurrentCultureIgnoreCase);
        }

        #endregion

        public static ArgumentString CherryPickCmd(ObjectId commitId, bool commit, string arguments)
        {
            return new GitArgumentBuilder("cherry-pick")
            {
                { !commit, "--no-commit" },
                arguments,
                commitId
            };
        }

        public static ArgumentString SubmoduleUpdateCmd([CanBeNull] string name)
        {
            return SubmoduleUpdateCommand((name ?? "").Trim().QuoteNE());
        }

        public static ArgumentString SubmoduleUpdateCmd(IEnumerable<string> submodules)
        {
            string submodulesQuoted = string.Join(" ", submodules.Select(s => s.Trim().QuoteNE()));
            return SubmoduleUpdateCommand(submodulesQuoted);
        }

        private static ArgumentString SubmoduleUpdateCommand(string name)
        {
            return new GitArgumentBuilder("submodule")
            {
                "update",
                "--init",
                "--recursive",
                name
            };
        }

        public static ArgumentString SubmoduleSyncCmd([CanBeNull] string name)
        {
            return new GitArgumentBuilder("submodule")
            {
                "sync",
                name?.Trim().QuoteNE()
            };
        }

        public static ArgumentString AddSubmoduleCmd(string remotePath, string localPath, string branch, bool force)
        {
            return new GitArgumentBuilder("submodule")
            {
                "add",
                { force, "-f" },
                { !string.IsNullOrEmpty(branch), $"-b \"{branch?.Trim()}\"" },
                remotePath.ToPosixPath().Quote(),
                localPath.ToPosixPath().Quote()
            };
        }

        public static ArgumentString RevertCmd(ObjectId commitId, bool autoCommit, int parentIndex)
        {
            return new GitArgumentBuilder("revert")
            {
                { !autoCommit, "--no-commit" },
                { parentIndex > 0, $"-m {parentIndex}" },
                commitId
            };
        }

        /// <summary>
        /// The Git command line for reset
        /// </summary>
        /// <param name="mode">Reset mode</param>
        /// <param name="commit">Optional commit-ish (for reset-index this is tree-ish and mandatory)</param>
        /// <param name="file">Optional file to reset</param>
        /// <returns>Argument string</returns>
        public static ArgumentString ResetCmd(ResetMode mode, string commit = null, string file = null)
        {
            if (mode == ResetMode.ResetIndex && string.IsNullOrWhiteSpace(commit))
            {
                throw new ArgumentException("reset to index requires a tree-ish parameter");
            }

            return new GitArgumentBuilder("reset")
            {
                mode,
                commit.QuoteNE(),
                "--",
                file?.ToPosixPath().QuoteNE()
            };
        }

        /// <summary>
        /// Push a local reference to a new commit
        /// This is similar to "git branch --force "branch" "commit", except that you get a warning if commits are lost
        /// </summary>
        /// <param name="repoPath">Full path to the repo</param>
        /// <param name="gitRef">The branch to move</param>
        /// <param name="targetId">The commit to move to</param>
        /// <param name="force">Push the reference also if commits are lost</param>
        /// <returns>The Git command to execute</returns>
        public static ArgumentString PushLocalCmd(string repoPath, string gitRef, ObjectId targetId, bool force = false)
        {
            return new GitArgumentBuilder("push")
            {
                $"file://{repoPath}".RemoveTrailingPathSeparator().QuoteNE(),
                $"{targetId}:{gitRef}".QuoteNE(),
                { force, "--force" }
            };
        }

        /// <summary>
        /// Git Clone.
        /// </summary>
        /// <param name="central">Makes a bare repo.</param>
        /// <param name="branch">
        /// <para><c>NULL</c>: do not checkout working copy (--no-checkout).</para>
        /// <para><c>""</c> (empty string): checkout remote HEAD (branch param omitted, default behavior for clone).</para>
        /// <para>(a non-empty string): checkout the given branch (--branch some_branch).</para>
        /// </param>
        /// <param name="depth">An int value for --depth param, or <c>NULL</c> to omit the param.</param>
        /// <param name="isSingleBranch">
        /// <para><c>True</c>: --single-branch.</para>
        /// <para><c>False</c>: --no-single-branch.</para>
        /// <para><c>NULL</c>: don't pass any such param to git.</para>
        /// </param>
        /// <param name="lfs">True to use the <c>git lfs clone</c> command instead of <c>git clone</c>.</param>
        public static ArgumentString CloneCmd(string fromPath, string toPath, bool central = false, bool initSubmodules = false, [CanBeNull] string branch = "", int? depth = null, bool? isSingleBranch = null, bool lfs = false)
        {
            var from = PathUtil.IsLocalFile(fromPath) ? fromPath.ToPosixPath() : fromPath;

            return new GitArgumentBuilder(lfs ? "lfs" : "clone")
            {
                { lfs, "clone" },
                "-v",
                { central, "--bare" },
                { initSubmodules, "--recurse-submodules" },
                { depth != null, $"--depth {depth}" },
                { isSingleBranch == true, "--single-branch" },
                { isSingleBranch == false, "--no-single-branch" },
                "--progress",
                { branch == null, "--no-checkout" },
                { !string.IsNullOrEmpty(branch), $"--branch {branch}" },
                from.Trim().Quote(),
                toPath.ToPosixPath().Trim().Quote()
            };
        }

        public static ArgumentString CheckoutCmd(string branchOrRevisionName, LocalChangesAction changesAction)
        {
            return new GitArgumentBuilder("checkout")
            {
                { changesAction == LocalChangesAction.Merge, "--merge" },
                { changesAction == LocalChangesAction.Reset, "--force" },
                branchOrRevisionName.Quote()
            };
        }

        /// <summary>Create a new orphan branch from <paramref name="startPoint"/> and switch to it.</summary>
        public static ArgumentString CreateOrphanCmd(string newBranchName, ObjectId startPoint = null)
        {
            return new GitArgumentBuilder("checkout")
            {
                "--orphan",
                newBranchName,
                startPoint
            };
        }

        /// <summary>Remove files from the working tree and from the index. <remarks>git rm</remarks></summary>
        /// <param name="force">Override the up-to-date check.</param>
        /// <param name="isRecursive">Allow recursive removal when a leading directory name is given.</param>
        /// <param name="files">Files to remove. File globs can be given to remove matching files.</param>
        public static ArgumentString RemoveCmd(bool force = true, bool isRecursive = true, params string[] files)
        {
            return new GitArgumentBuilder("rm")
            {
                { force, "--force" },
                { isRecursive, "-r" },
                { files.Length == 0, "." },
                files
            };
        }

        public static ArgumentString BranchCmd(string branchName, string revision, bool checkout)
        {
            return new GitArgumentBuilder(checkout ? "checkout" : "branch")
            {
                { checkout, "-b" },
                branchName.Trim().Quote(),
                revision?.Trim().QuoteNE()
            };
        }

        public static ArgumentString MergedBranchesCmd(bool includeRemote = false, bool fullRefname = false, [CanBeNull] string commit = null)
        {
            return new GitArgumentBuilder("branch")
            {
                { fullRefname, "--format=%(refname)" },
                { includeRemote, "-a" },
                "--merged",
                commit
            };
        }

        /// <summary>Pushes multiple sets of local branches to remote branches.</summary>
        public static ArgumentString PushMultipleCmd(string remote, IEnumerable<GitPushAction> pushActions)
        {
            return new GitPush(remote.ToPosixPath(), pushActions)
            {
                ReportProgress = GitVersion.Current.PushCanAskForProgress
            }.ToString();
        }

        public static ArgumentString PushTagCmd(string path, string tag, bool all, ForcePushOptions force = ForcePushOptions.DoNotForce)
        {
            if (!all && string.IsNullOrWhiteSpace(tag))
            {
                // TODO this is probably an error
                return "";
            }

            return new GitArgumentBuilder("push")
            {
                force,
                { GitVersion.Current.PushCanAskForProgress, "--progress" },
                path.ToPosixPath().Trim().Quote(),
                { all, "--tags" },
                { !all, $"tag {tag.Replace(" ", "")}" }
            };
        }

        public static ArgumentString StashSaveCmd(bool untracked, bool keepIndex, string message, IReadOnlyList<string> selectedFiles)
        {
            if (selectedFiles == null)
            {
                selectedFiles = Array.Empty<string>();
            }

            var isPartialStash = selectedFiles.Any();

            return new GitArgumentBuilder("stash")
            {
                { isPartialStash, "push", "save" },
                { untracked && GitVersion.Current.StashUntrackedFilesSupported, "-u" },
                { keepIndex, "--keep-index" },
                { isPartialStash && !string.IsNullOrWhiteSpace(message), "-m" },
                { !string.IsNullOrWhiteSpace(message), message.Quote() },
                { isPartialStash, "--" },
                { isPartialStash, string.Join(" ", selectedFiles.Where(path => !string.IsNullOrWhiteSpace(path)).Select(path => path.QuoteNE())) }
            };
        }

        public static ArgumentString ContinueRebaseCmd()
        {
            return new GitArgumentBuilder("rebase") { "--continue" };
        }

        public static ArgumentString SkipRebaseCmd()
        {
            return new GitArgumentBuilder("rebase") { "--skip" };
        }

        public static ArgumentString ContinueMergeCmd()
        {
            return new GitArgumentBuilder("merge") { "--continue" };
        }

        public static ArgumentString AbortMergeCmd()
        {
            return new GitArgumentBuilder("merge") { "--abort" };
        }

        public static ArgumentString StartBisectCmd()
        {
            return new GitArgumentBuilder("bisect") { "start" };
        }

        public static ArgumentString ContinueBisectCmd(GitBisectOption bisectOption, params ObjectId[] revisions)
        {
            return new GitArgumentBuilder("bisect")
            {
                bisectOption,
                revisions
            };
        }

        public static ArgumentString StopBisectCmd()
        {
            return new GitArgumentBuilder("bisect") { "reset" };
        }

        public static ArgumentString RebaseCmd(string branch, bool interactive, bool preserveMerges, bool autosquash, bool autoStash, string from = null, string onto = null)
        {
            if (from == null ^ onto == null)
            {
                throw new ArgumentException($"For arguments \"{nameof(from)}\" and \"{nameof(onto)}\", either both must have values, or neither may.");
            }

            return new GitArgumentBuilder("rebase")
            {
                { interactive, "-i" },
                { interactive && autosquash, "--autosquash" },
                { interactive && !autosquash, "--no-autosquash" },
                { preserveMerges, GitVersion.Current.SupportRebaseMerges ? "--rebase-merges" : "--preserve-merges" },
                { autoStash, "--autostash" },
                from.QuoteNE(),
                branch.Quote(),
                { onto != null, $"--onto {onto}" }
            };
        }

        public static ArgumentString AbortRebaseCmd()
        {
            return new GitArgumentBuilder("rebase") { "--abort" };
        }

        public static ArgumentString ResolvedCmd()
        {
            return new GitArgumentBuilder("am")
            {
                "--3way",
                "--resolved"
            };
        }

        public static ArgumentString SkipCmd()
        {
            return new GitArgumentBuilder("am")
            {
                "--3way",
                "--skip"
            };
        }

        public static ArgumentString AbortCmd()
        {
            return new GitArgumentBuilder("am")
            {
                "--3way",
                "--abort"
            };
        }

        public static ArgumentString ApplyMailboxPatchCmd(bool ignoreWhiteSpace, string patchFile = null)
        {
            return new GitArgumentBuilder("am")
            {
                "--3way",
                "--signoff",
                { ignoreWhiteSpace, "--ignore-whitespace" },
                patchFile?.ToPosixPath().Quote()
            };
        }

        public static ArgumentString ApplyDiffPatchCmd(bool ignoreWhiteSpace, [NotNull] string patchFile)
        {
            return new GitArgumentBuilder("apply")
            {
                { ignoreWhiteSpace, "--ignore-whitespace" },
                patchFile.ToPosixPath().Quote()
            };
        }

        /// <summary>
        /// Arguments for git-clean
        /// </summary>
        /// <param name="mode">The cleanup mode what to delete</param>
        /// <param name="dryRun">Only show what would be deleted</param>
        /// <param name="directories">Delete untracked directories too</param>
        /// <param name="paths">Limit to specific paths</param>
        public static ArgumentString CleanCmd(CleanMode mode, bool dryRun, bool directories, string paths = null)
        {
            return new GitArgumentBuilder("clean")
            {
                mode,
                { directories, "-d" },
                { dryRun, "--dry-run", "-f" },
                paths
            };
        }

        public static ArgumentString GetAllChangedFilesCmd(bool excludeIgnoredFiles, UntrackedFilesMode untrackedFiles, IgnoreSubmodulesMode ignoreSubmodules = IgnoreSubmodulesMode.None, bool noLocks = false)
        {
            var args = new GitArgumentBuilder("status", gitOptions:
                noLocks && GitVersion.Current.SupportNoOptionalLocks
                    ? (ArgumentString)"--no-optional-locks"
                    : default)
            {
                $"--porcelain{(GitVersion.Current.SupportStatusPorcelainV2 ? "=2" : "")} -z",
                untrackedFiles,
                { !excludeIgnoredFiles, "--ignored" }
            };

            // git-config is set to None, to allow overrides for specific submodules (in .gitconfig or .gitmodules)
            if (ignoreSubmodules != IgnoreSubmodulesMode.None)
            {
                args.Add(ignoreSubmodules);
            }

            return args;
        }

        [CanBeNull]
        public static async Task<GitSubmoduleStatus> GetCurrentSubmoduleChangesAsync(GitModule module, string fileName, string oldFileName, bool staged, bool noLocks = false)
        {
            Patch patch = await module.GetCurrentChangesAsync(fileName, oldFileName, staged, "", noLocks: noLocks).ConfigureAwait(false);
            string text = patch != null ? patch.Text : "";
            return ParseSubmoduleStatus(text, module, fileName);
        }

        [CanBeNull]
        public static Task<GitSubmoduleStatus> GetCurrentSubmoduleChangesAsync(GitModule module, string submodule, bool noLocks = false)
        {
            return GetCurrentSubmoduleChangesAsync(module, submodule, submodule, false, noLocks: noLocks);
        }

        [CanBeNull]
        public static GitSubmoduleStatus ParseSubmoduleStatus(string text, GitModule module, string fileName)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            string name = null;
            string oldName = null;
            bool isDirty = false;
            ObjectId commitId = null;
            ObjectId oldCommitId = null;
            int? addedCommits = null;
            int? removedCommits = null;

            using (var reader = new StringReader(text))
            {
                string line = reader.ReadLine();

                if (line != null)
                {
                    var match = Regex.Match(line, @"diff --git [abic]/(.+)\s[abwi]/(.+)");
                    if (match.Groups.Count > 1)
                    {
                        name = match.Groups[1].Value;
                        oldName = match.Groups[2].Value;
                    }
                    else
                    {
                        match = Regex.Match(line, @"diff --cc (.+)");
                        if (match.Groups.Count > 1)
                        {
                            name = match.Groups[1].Value;
                            oldName = match.Groups[1].Value;
                        }
                    }
                }

                while ((line = reader.ReadLine()) != null)
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
                        hash = line.Substring(pos + commitStr.Length);
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

            if (oldCommitId != null && commitId != null)
            {
                if (oldCommitId == commitId)
                {
                    addedCommits = 0;
                    removedCommits = 0;
                }
                else
                {
                    var submodule = module.GetSubmodule(fileName);
                    addedCommits = submodule.GetCommitCount(commitId.ToString(), oldCommitId.ToString());
                    removedCommits = submodule.GetCommitCount(oldCommitId.ToString(), commitId.ToString());
                }
            }

            return new GitSubmoduleStatus(name, oldName, isDirty, commitId, oldCommitId, addedCommits, removedCommits);
        }

        /// <summary>
        /// Parse the output from git-diff --name-status
        /// </summary>
        /// <param name="module">The Git module</param>
        /// <param name="statusString">output from the git command</param>
        /// <param name="staged">required to determine if <see cref="StagedStatus"/> allows stage/unstage.</param>
        /// <returns>list with the parsed GitItemStatus</returns>
        /// <seealso href="https://git-scm.com/docs/git-diff"/>
        public static IReadOnlyList<GitItemStatus> GetDiffChangedFilesFromString(IGitModule module, string statusString, StagedStatus staged)
        {
            return GetAllChangedFilesFromString_v1(module, statusString, true, staged);
        }

        /// <summary>
        /// If possible, find if files in a diff are index or worktree
        /// </summary>
        /// <param name="firstId">from revision string</param>
        /// <param name="secondId">to revision</param>
        /// <param name="parentToSecond">The parent for the second revision</param>
        /// <remarks>Git revisions are required to determine if <see cref="StagedStatus"/> allows stage/unstage.</remarks>
        public static StagedStatus GetStagedStatus([CanBeNull] ObjectId firstId, [CanBeNull] ObjectId secondId, [CanBeNull] ObjectId parentToSecond)
        {
            StagedStatus staged;
            if (firstId == ObjectId.IndexId && secondId == ObjectId.WorkTreeId)
            {
                staged = StagedStatus.WorkTree;
            }
            else if (firstId == parentToSecond && secondId == ObjectId.IndexId)
            {
                staged = StagedStatus.Index;
            }
            else if (firstId != null && !firstId.IsArtificial &&
                     secondId != null && !secondId.IsArtificial)
            {
                // This cannot be a worktree/index file
                staged = StagedStatus.None;
            }
            else
            {
                staged = StagedStatus.Unknown;
            }

            return staged;
        }

        /// <summary>
        /// Parse the output from git-status --porcelain=2 -z
        /// Note that the caller should check for fatal errors in the Git output
        /// </summary>
        /// <param name="module">The Git module</param>
        /// <param name="statusString">output from the git command</param>
        /// <returns>list with the parsed GitItemStatus</returns>
        /// <seealso href="https://git-scm.com/docs/git-status"/>
        public static IReadOnlyList<GitItemStatus> GetStatusChangedFilesFromString(IGitModule module, string statusString)
        {
            if (GitVersion.Current.SupportStatusPorcelainV2)
            {
                return GetAllChangedFilesFromString_v2(statusString);
            }
            else
            {
                return GetAllChangedFilesFromString_v1(module, statusString, false, StagedStatus.Unset);
            }
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

        /// <summary>
        /// Parse the output from git-status --porcelain=2
        /// </summary>
        /// <param name="statusString">output from the git command</param>
        /// <returns>list with the parsed GitItemStatus</returns>
        private static IReadOnlyList<GitItemStatus> GetAllChangedFilesFromString_v2(string statusString)
        {
            var diffFiles = new List<GitItemStatus>();

            if (string.IsNullOrEmpty(statusString))
            {
                return diffFiles;
            }

            string trimmedStatus = RemoveWarnings(statusString);

            // Split all files on '\0'
            var files = trimmedStatus.Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
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
                    string oldFileName = null;
                    string renamePercent = null;
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
                        string[] renames = line.Substring(114).Split(new[] { ' ' }, 2);
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

            void UpdateItemStatus(char x, bool isIndex, string subm, string fileName, string oldFileName, string renamePercent)
            {
                if (x == '.')
                {
                    return;
                }

                var staged = isIndex ? StagedStatus.Index : StagedStatus.WorkTree;
                GitItemStatus gitItemStatus = GitItemStatusFromStatusCharacter(staged, fileName, x);
                if (oldFileName != null)
                {
                    gitItemStatus.OldName = oldFileName;
                }

                if (renamePercent != null)
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

        /// <summary>
        /// Parse git-status --porcelain=1 and git-diff --name-status
        /// Outputs are similar, except that git-status has status for both worktree and index
        /// </summary>
        /// <param name="module">The GitModule</param>
        /// <param name="statusString">Output from Git command</param>
        /// <param name="fromDiff">Parse git-diff</param>
        /// <param name="staged">The staged status <see cref="GitItemStatus"/>, only relevant for git-diff (parsed for git-status)</param>
        /// <returns>list with the git items</returns>
        private static IReadOnlyList<GitItemStatus> GetAllChangedFilesFromString_v1(IGitModule module, string statusString, bool fromDiff, StagedStatus staged)
        {
            var diffFiles = new List<GitItemStatus>();

            if (string.IsNullOrEmpty(statusString))
            {
                return diffFiles;
            }

            string trimmedStatus = RemoveWarnings(statusString);

            // Doesn't work with removed submodules
            var submodules = module.GetSubmodulesLocalPaths();

            // Split all files on '\0' (WE NEED ALL COMMANDS TO BE RUN WITH -z! THIS IS ALSO IMPORTANT FOR ENCODING ISSUES!)
            var files = trimmedStatus.Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
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
                    var splitChars = new[] { '\t', ' ' };
                    splitIndex = files[n].IndexOfAny(splitChars, 1);
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
                        gitItemStatusX = GitItemStatusFromStatusCharacter(stagedX, fileName, x);
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
                    gitItemStatusY = GitItemStatusFromStatusCharacter(stagedY, fileName, y);
                }

                if (submodules.Contains(gitItemStatusY.Name))
                {
                    gitItemStatusY.IsSubmodule = true;
                }

                diffFiles.Add(gitItemStatusY);
            }

            return diffFiles;
        }

        public static IReadOnlyList<GitItemStatus> GetAssumeUnchangedFilesFromString(string lsString)
        {
            var result = new List<GitItemStatus>();
            string[] lines = lsString.SplitLines();
            foreach (string line in lines)
            {
                char statusCharacter = line[0];
                if (char.IsUpper(statusCharacter))
                {
                    continue;
                }

                string fileName = line.SubstringAfter(' ');
                GitItemStatus gitItemStatus = GitItemStatusFromStatusCharacter(StagedStatus.WorkTree, fileName, statusCharacter);
                gitItemStatus.IsAssumeUnchanged = true;
                result.Add(gitItemStatus);
            }

            return result;
        }

        public static IReadOnlyList<GitItemStatus> GetSkipWorktreeFilesFromString(string lsString)
        {
            var result = new List<GitItemStatus>();
            string[] lines = lsString.SplitLines();
            foreach (string line in lines)
            {
                char statusCharacter = line[0];

                string fileName = line.SubstringAfter(' ');
                GitItemStatus gitItemStatus = GitItemStatusFromStatusCharacter(StagedStatus.WorkTree, fileName, statusCharacter);
                if (gitItemStatus.IsSkipWorktree)
                {
                    result.Add(gitItemStatus);
                }
            }

            return result;
        }

        private static GitItemStatus GitItemStatusFromCopyRename(StagedStatus staged, bool fromDiff, string nextFile, string fileName, char x, string status)
        {
            var gitItemStatus = new GitItemStatus();

            // Find renamed files...
            if (fromDiff)
            {
                gitItemStatus.OldName = fileName;
                gitItemStatus.Name = nextFile;
            }
            else
            {
                gitItemStatus.Name = fileName;
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

        private static GitItemStatus GitItemStatusFromStatusCharacter(StagedStatus staged, string fileName, char x)
        {
            var isNew = x == 'A' || x == '?' || x == '!';

            return new GitItemStatus
            {
                Name = fileName,
                IsNew = isNew,
                IsChanged = x == 'M',
                IsDeleted = x == 'D',
                IsSkipWorktree = x == 'S',
                IsRenamed = x == 'R',
                IsCopied = x == 'C',
                IsTracked = (x != '?' && x != '!' && x != ' ') || !isNew,
                IsIgnored = x == '!',
                IsConflict = x == 'U',
                Staged = staged
            };
        }

        public static ArgumentString MergeBranchCmd(string branch, bool allowFastForward, bool squash, bool noCommit, string strategy, bool allowUnrelatedHistories, string mergeCommitFilePath, int? log)
        {
            return new GitArgumentBuilder("merge")
            {
                { !allowFastForward, "--no-ff" },
                { !string.IsNullOrEmpty(strategy), $"--strategy={strategy}" },
                { squash, "--squash" },
                { noCommit, "--no-commit" },
                { allowUnrelatedHistories, "--allow-unrelated-histories" },
                { !string.IsNullOrWhiteSpace(mergeCommitFilePath), $"-F \"{mergeCommitFilePath}\"" }, // let git fail, if the file doesn't exist
                { log != null && log.Value > 0, $"--log={log}" },
                branch
            };
        }

        // returns " --find-renames=..." according to app settings
        public static ArgumentString FindRenamesOpt()
        {
            return AppSettings.FollowRenamesInFileHistoryExactOnly
                ? " --find-renames=\"100%\""
                : " --find-renames";
        }

        // returns " --find-renames=... --find-copies=..." according to app settings
        public static ArgumentString FindRenamesAndCopiesOpts()
        {
            var findCopies = AppSettings.FollowRenamesInFileHistoryExactOnly
                ? " --find-copies=\"100%\""
                : " --find-copies";
            return FindRenamesOpt() + findCopies;
        }

        private static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            public static extern bool SetConsoleCtrlHandler(IntPtr handlerRoutine, bool add);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool AttachConsole(int dwProcessId);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, int dwProcessGroupId);
        }

        public static void TerminateTree(this Process process)
        {
            if (EnvUtils.RunningOnWindows())
            {
                // Send Ctrl+C
                NativeMethods.AttachConsole(process.Id);
                NativeMethods.SetConsoleCtrlHandler(IntPtr.Zero, add: true);
                NativeMethods.GenerateConsoleCtrlEvent(0, 0);

                if (!process.HasExited)
                {
                    process.WaitForExit(500);
                }
            }

            if (!process.HasExited)
            {
                process.Kill();
            }
        }
    }
}
