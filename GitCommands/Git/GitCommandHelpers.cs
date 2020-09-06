using System;
using System.Collections.Generic;
using System.Linq;
using GitCommands.Git;
using GitExtUtils;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands
{
    public static class GitCommandHelpers
    {
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

        public static ArgumentString GetCurrentChangesCmd(string fileName, [CanBeNull] string oldFileName, bool staged,
            string extraDiffArguments, bool noLocks)
        {
            return new GitArgumentBuilder("diff", gitOptions:
                    noLocks && GitVersion.Current.SupportNoOptionalLocks
                        ? (ArgumentString)"--no-optional-locks"
                        : default)
                {
                    "--no-color",
                    { staged, "-M -C --cached" },
                    extraDiffArguments,
                    { AppSettings.UseHistogramDiffAlgorithm, "--histogram" },
                    "--",
                    fileName.ToPosixPath().Quote(),
                    { staged, oldFileName?.ToPosixPath().Quote() }
                };
        }

        public static ArgumentString GetRefsCmd(bool tags, bool branches, bool noLocks)
        {
            GitArgumentBuilder cmd;

            if (tags)
            {
                cmd = new GitArgumentBuilder("show-ref", gitOptions:
                    noLocks && GitVersion.Current.SupportNoOptionalLocks
                        ? (ArgumentString)"--no-optional-locks"
                        : default)
                {
                    { branches, "--dereference", "--tags" },
                };
            }
            else if (branches)
            {
                // branches only
                cmd = new GitArgumentBuilder("for-each-ref", gitOptions:
                    noLocks && GitVersion.Current.SupportNoOptionalLocks
                        ? (ArgumentString)"--no-optional-locks"
                        : default)
                {
                    "--sort=-committerdate",
                    @"refs/heads/",
                    @"--format=""%(objectname) %(refname)"""
                };
            }
            else
            {
                throw new ArgumentException("GetRefs: Neither branches nor tags requested");
            }

            return cmd;
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

        public static ArgumentString GetSortedRefsCommand(bool noLocks = false)
        {
            if (AppSettings.ShowSuperprojectRemoteBranches)
            {
                return new GitArgumentBuilder("for-each-ref", gitOptions:
                    noLocks && GitVersion.Current.SupportNoOptionalLocks
                        ? (ArgumentString)"--no-optional-locks"
                        : default)
                {
                    "--sort=-committerdate",
                    "--format=\"%(objectname) %(refname)\"",
                    "refs/"
                };
            }

            if (AppSettings.ShowSuperprojectBranches || AppSettings.ShowSuperprojectTags)
            {
                return new GitArgumentBuilder("for-each-ref", gitOptions:
                    noLocks && GitVersion.Current.SupportNoOptionalLocks
                        ? (ArgumentString)"--no-optional-locks"
                        : default)
                {
                    "--sort=-committerdate",
                    "--format=\"%(objectname) %(refname)\"",
                    { AppSettings.ShowSuperprojectBranches, "refs/heads/" },
                    { AppSettings.ShowSuperprojectTags, " refs/tags/" }
                };
            }

            return "";
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

        public static ArgumentString RebaseCmd(
            string branch, bool interactive, bool preserveMerges, bool autosquash, bool autoStash, bool ignoreDate, bool committerDateIsAuthorDate, string from = null, string onto = null)
        {
            if (from == null ^ onto == null)
            {
                throw new ArgumentException($"For arguments \"{nameof(from)}\" and \"{nameof(onto)}\", either both must have values, or neither may.");
            }

            var builder = new GitArgumentBuilder("rebase");
            if (ignoreDate)
            {
                builder.Add("--ignore-date");
            }
            else if (committerDateIsAuthorDate)
            {
                builder.Add("--committer-date-is-author-date");
            }
            else
            {
                if (interactive)
                {
                    builder.Add("-i");
                    builder.Add(autosquash ? "--autosquash" : "--no-autosquash");
                }

                if (preserveMerges)
                {
                    builder.Add(GitVersion.Current.SupportRebaseMerges ? "--rebase-merges" : "--preserve-merges");
                }
            }

            builder.Add(autoStash, "--autostash");
            builder.Add(from.QuoteNE());
            builder.Add(branch.Quote());
            builder.Add(onto != null, $"--onto {onto}");

            return builder;
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

        public static ArgumentString ApplyMailboxPatchCmd(bool signOff, bool ignoreWhiteSpace, string patchFile = null)
        {
            return new GitArgumentBuilder("am")
            {
                "--3way",
                { signOff, "--signoff" },
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
            return new GetAllChangedFilesOutputParser(() => module).GetAllChangedFilesFromString_v1(statusString, true, staged);
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
                GitItemStatus gitItemStatus = GitItemStatusConverter.FromStatusCharacter(StagedStatus.WorkTree, fileName, statusCharacter);
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
                GitItemStatus gitItemStatus = GitItemStatusConverter.FromStatusCharacter(StagedStatus.WorkTree, fileName, statusCharacter);
                if (gitItemStatus.IsSkipWorktree)
                {
                    result.Add(gitItemStatus);
                }
            }

            return result;
        }

        /// <summary>
        /// Parse the output from 'git difftool --tool-help'
        /// </summary>
        /// <param name="output">The output string</param>
        /// <returns>list with tool names</returns>
        public static List<string> ParseCustomDiffMergeTool(string output)
        {
            var tools = new List<string>();

            // Simple parsing of the textual output opposite to porcelain format
            // https://github.com/git/git/blob/main/git-mergetool--lib.sh#L298
            // An alternative is to parse "git config --get-regexp difftool'\..*\.cmd'" and see show_tool_names()

            // The sections to parse in the text has a 'header', then break parsing at first non match

            foreach (var l in output.Split('\n'))
            {
                if (l == "The following tools are valid, but not currently available:")
                {
                    // No more usable tools
                    break;
                }

                if (!l.StartsWith("\t\t"))
                {
                    continue;
                }

                // two tabs, then toolname, cmd (if split in 3) in second
                // cmd is unreliable for diff and not needed but could be used for mergetool special handling
                string[] delimit = { " ", ".cmd" };
                var tool = l.Substring(2).Split(delimit, 2, StringSplitOptions.None);
                if (tool.Length == 0)
                {
                    continue;
                }

                // Ignore tools that must run in a terminal
                string[] ignoredTools = { "vimdiff", "vimdiff2", "vimdiff3" };
                var toolName = tool[0];
                if (!string.IsNullOrWhiteSpace(toolName) && !tools.Contains(toolName) && !ignoredTools.Contains(toolName))
                {
                    tools.Add(toolName);
                }
            }

            return tools.OrderBy(i => i).ToList();
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
    }
}
