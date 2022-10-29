using System.Diagnostics;
using GitCommands.Config;
using GitCommands.Utils;
using GitExtUtils;
using GitUIPluginInterfaces;

namespace GitCommands.Git.Commands
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

        public static ArgumentString SubmoduleUpdateCmd(string? name, IEnumerable<GitConfigItem>? configs = null)
        {
            return SubmoduleUpdateCommand((name ?? "").Trim().QuoteNE(), configs);
        }

        public static ArgumentString SubmoduleUpdateCmd(IEnumerable<string> submodules, IEnumerable<GitConfigItem>? configs = null)
        {
            string submodulesQuoted = string.Join(" ", submodules.Select(s => s.Trim().QuoteNE()));
            return SubmoduleUpdateCommand(submodulesQuoted, configs);
        }

        private static ArgumentString SubmoduleUpdateCommand(string name, IEnumerable<GitConfigItem>? configs)
        {
            GitArgumentBuilder args = new("submodule")
            {
                "update",
                "--init",
                "--recursive",
                name
            };

            if (configs is not null)
            {
                foreach (GitConfigItem cfg in configs)
                {
                    args.Add(cfg);
                }
            }

            return args;
        }

        public static ArgumentString SubmoduleSyncCmd(string? name)
        {
            return new GitArgumentBuilder("submodule")
            {
                "sync",
                name?.Trim().QuoteNE()
            };
        }

        public static ArgumentString AddSubmoduleCmd(string remotePath, string localPath, string branch, bool force, IEnumerable<GitConfigItem> configs = null)
        {
            GitArgumentBuilder argsBuilder = new("submodule")
            {
                "add",
                { force, "-f" },
                { !string.IsNullOrEmpty(branch), $"-b \"{branch?.Trim()}\"" },
                remotePath.ToPosixPath().Quote(),
                localPath.ToPosixPath().Quote()
            };

            if (configs is not null)
            {
                foreach (GitConfigItem cfg in configs)
                {
                    argsBuilder.Add(cfg);
                }
            }

            return argsBuilder;
        }

        /// <summary>
        /// Gets <see cref="GitConfigItem"/> that sets 'protocol.file.allow' to always.
        /// </summary>
        /// <remarks>IEnumerable to allow future concat with other needed configs.</remarks>
        /// <returns>Config with 'protocol.file.allow set to always</returns>
        public static IEnumerable<GitConfigItem> GetAllowFileConfig()
        {
            yield return new GitConfigItem(SettingKeyString.AllowFileProtocol, "always");
        }

        public static ArgumentString GetCurrentChangesCmd(string? fileName, string? oldFileName, bool staged,
            string extraDiffArguments, bool noLocks)
        {
            return new GitArgumentBuilder("diff", gitOptions: noLocks ? (ArgumentString)"--no-optional-locks" : default)
                {
                    "--find-renames",
                    "--find-copies",
                    { AppSettings.UseHistogramDiffAlgorithm, "--histogram" },
                    extraDiffArguments,
                    { staged, "--cached" },
                    "--",
                    fileName.ToPosixPath().Quote(),
                    { staged, oldFileName?.ToPosixPath().Quote() }
                };
        }

        public static ArgumentString GetRefsCmd(RefsFilter getRef, bool noLocks, GitRefsSortBy sortBy, GitRefsSortOrder sortOrder, int count = 0)
        {
            bool hasTags = (getRef == RefsFilter.NoFilter) || (getRef & RefsFilter.Tags) != 0;

            GitArgumentBuilder cmd = new("for-each-ref",
                gitOptions: noLocks ? (ArgumentString)"--no-optional-locks" : default)
            {
                SortCriteria(hasTags, sortBy, sortOrder),
                GitRefsFormat(hasTags),
                { count > 0, $"--count={count}" },
                GitRefsPattern(getRef)
            };

            return cmd;

            static ArgumentString SortCriteria(bool needTags, GitRefsSortBy sortBy, GitRefsSortOrder sortOrder)
            {
                if (sortBy == GitRefsSortBy.Default)
                {
                    return string.Empty;
                }

                string order = sortOrder == GitRefsSortOrder.Ascending ? string.Empty : "-";
                if (!needTags)
                {
                    return $@"--sort=""{order}{sortBy}""";
                }

                // Sort by dereferenced data
                //
                // NOTE: This will cause tags to be sorted in a strange way, all tags will come first
                // then all dereferences even though then may be pointing to commits that are younger
                // then those of pointed by actual tags.
                // If we swap the sort order, i.e. do "ref, deref" then we will be breaking the normal
                // ref ordering (i.e. local and remote branches), and this is generally a significantly
                // greater of two evils.
                // Refer to https://github.com/gitextensions/gitextensions/issues/8621 for more info.
                return $@"--sort=""{order}*{sortBy}"" --sort=""{order}{sortBy}""";
            }

            static ArgumentString GitRefsFormat(bool needTags)
            {
                if (!needTags)
                {
                    // If we don't need tags, it is easy.
                    return @"--format=""%(objectname) %(refname)""";
                }

                // ...however, if we're interested in tags, tags may be simple (in which case they are point to commits directly),
                // or "dereferences" (i.e. commits that contain metadata and point to other commits, "^{}").
                // Dereference commits do not contain date information, so we need to find information from the referenced commits (those with '*').
                // So the following format is as follows:
                //      If (there is a 'authordate' information, then this is a simple tag/direct commit)
                //      Then
                //          format = %(objectname) %(refname)
                //      Else
                //          format = %(*objectname) %(*refname) // i.e. info from a referenced commit
                //      End
                return @"--format=""%(if)%(authordate)%(then)%(objectname) %(refname)%(else)%(*objectname) %(*refname)%(end)""";
            }

            static ArgumentString GitRefsPattern(RefsFilter option)
            {
                if (option == RefsFilter.NoFilter)
                {
                    // Include all refs/
                    return string.Empty;
                }

                ArgumentBuilder builder = new();
                builder.Add((option & RefsFilter.Heads) != 0, "refs/heads/");
                builder.Add((option & RefsFilter.Remotes) != 0, "refs/remotes/");
                builder.Add((option & RefsFilter.Tags) != 0, "refs/tags/");

                return builder;
            }
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
        /// The Git command line for reset.
        /// </summary>
        /// <param name="mode">Reset mode.</param>
        /// <param name="commit">Optional commit-ish (for reset-index this is tree-ish and mandatory).</param>
        /// <param name="file">Optional file to reset.</param>
        /// <returns>Argument string.</returns>
        public static ArgumentString ResetCmd(ResetMode mode, string? commit = null, string? file = null)
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
        /// This is similar to "git branch --force "branch" "commit", except that you get a warning if commits are lost.
        /// </summary>
        /// <param name="gitRef">The branch to move.</param>
        /// <param name="targetId">The commit to move to.</param>
        /// <param name="repoDir">Directory to the current repo in Posix format.</param>
        /// <param name="force">Push the reference also if commits are lost.</param>
        /// <returns>The Git command to execute.</returns>
        public static ArgumentString PushLocalCmd(string gitRef, ObjectId targetId, string repoDir, bool force = false)
        {
            Debug.Assert(!EnvUtils.RunningOnWindows() || repoDir.IndexOf(PathUtil.NativeDirectorySeparatorChar) < 0,
                $"'PushLocalCmd' must be called with 'repoDir' in Posix format");

            return new GitArgumentBuilder("push")
            {
                $@"""file://{repoDir}""",
                $"{targetId}:{gitRef}".QuoteNE(),
                { force, "--force" }
            };
        }

        /// <summary>
        /// Git Clone.
        /// </summary>
        /// <param name="fromPath">URL or file system path in Posix format.</param>
        /// <param name="toPath">Directory to the destination path in Posix format.</param>
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
        public static ArgumentString CloneCmd(string fromPath, string toPath, bool central = false, bool initSubmodules = false, string? branch = "", int? depth = null, bool? isSingleBranch = null)
        {
            Debug.Assert(!EnvUtils.RunningOnWindows() || fromPath.IndexOf(PathUtil.NativeDirectorySeparatorChar) < 0,
               $"'CloneCmd' must be called with 'fromPath' in Posix format");
            Debug.Assert(!EnvUtils.RunningOnWindows() || toPath.IndexOf(PathUtil.NativeDirectorySeparatorChar) < 0,
               $"'CloneCmd' must be called with 'toPath' in Posix format");

            return new GitArgumentBuilder("clone")
            {
                "-v",
                { central, "--bare" },
                { initSubmodules, "--recurse-submodules" },
                { depth is not null, $"--depth {depth}" },
                { isSingleBranch == true, "--single-branch" },
                { isSingleBranch == false, "--no-single-branch" },
                "--progress",
                { branch is null, "--no-checkout" },
                { !string.IsNullOrEmpty(branch), $"--branch {branch}" },
                fromPath.Trim().Quote(),
                toPath.Trim().Quote()
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
        public static ArgumentString CreateOrphanCmd(string newBranchName, ObjectId? startPoint = null)
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

        public static ArgumentString MergedBranchesCmd(bool includeRemote = false, bool fullRefname = false, string? commit = null)
        {
            return new GitArgumentBuilder("branch")
            {
                { fullRefname, @"--format=""%(refname)""" },
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
                ReportProgress = true
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
                "--progress",
                path.ToPosixPath().Trim().Quote(),
                { all, "--tags" },
                { !all, $"tag {tag.Replace(" ", "")}" }
            };
        }

        public static ArgumentString StashSaveCmd(bool untracked, bool keepIndex, string message, IReadOnlyList<string>? selectedFiles)
        {
            selectedFiles ??= Array.Empty<string>();

            var isPartialStash = selectedFiles.Any();

            return new GitArgumentBuilder("stash")
            {
                { isPartialStash, "push", "save" },
                { untracked, "-u" },
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
            string? branch, bool interactive, bool preserveMerges, bool autosquash, bool autoStash, bool ignoreDate, bool committerDateIsAuthorDate, string? from = null, string? onto = null, bool supportRebaseMerges = true)
        {
            // TODO-NULLABLE does it make sense for 'branch' to be null here?

            if (from is null ^ onto is null)
            {
                throw new ArgumentException($"For arguments \"{nameof(from)}\" and \"{nameof(onto)}\", either both must have values, or neither may.");
            }

            GitArgumentBuilder builder = new("rebase");
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
                    builder.Add(supportRebaseMerges ? "--rebase-merges" : "--preserve-merges");
                }
            }

            builder.Add(autoStash, "--autostash");
            builder.Add(from.QuoteNE());
            builder.Add(branch.Quote());
            builder.Add(onto is not null, $"--onto {onto}");

            return builder;
        }

        public static ArgumentString AbortRebaseCmd()
        {
            return new GitArgumentBuilder("rebase") { "--abort" };
        }

        public static ArgumentString EditTodoRebaseCmd()
        {
            return new GitArgumentBuilder("rebase") { "--edit-todo" };
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

        public static ArgumentString ApplyMailboxPatchCmd(bool signOff, bool ignoreWhiteSpace, string? patchFile = null)
        {
            return new GitArgumentBuilder("am")
            {
                "--3way",
                { signOff, "--signoff" },
                { ignoreWhiteSpace, "--ignore-whitespace" },
                patchFile?.ToPosixPath().Quote()
            };
        }

        public static ArgumentString ApplyDiffPatchCmd(bool ignoreWhiteSpace, string patchFile)
        {
            return new GitArgumentBuilder("apply")
            {
                { ignoreWhiteSpace, "--ignore-whitespace" },
                patchFile.ToPosixPath().Quote()
            };
        }

        /// <summary>
        /// Arguments for git-clean.
        /// </summary>
        /// <param name="mode">The cleanup mode what to delete.</param>
        /// <param name="dryRun">Only show what would be deleted.</param>
        /// <param name="directories">Delete untracked directories too.</param>
        /// <param name="paths">Limit to specific paths.</param>
        public static ArgumentString CleanCmd(CleanMode mode, bool dryRun, bool directories, string? paths = null)
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
            GitArgumentBuilder args = new("status", gitOptions: noLocks ? (ArgumentString)"--no-optional-locks" : default)
            {
                $"--porcelain=2 -z",
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

        public static ArgumentString MergeBranchCmd(string branch, bool allowFastForward, bool squash, bool noCommit, string strategy, bool allowUnrelatedHistories, string? mergeCommitFilePath, int? log)
        {
            return new GitArgumentBuilder("merge")
            {
                { !allowFastForward, "--no-ff" },
                { !string.IsNullOrEmpty(strategy), $"--strategy={strategy}" },
                { squash, "--squash" },
                { noCommit, "--no-commit" },
                { allowUnrelatedHistories, "--allow-unrelated-histories" },

                 // let git fail, if the file doesn't exist
                { !string.IsNullOrWhiteSpace(mergeCommitFilePath), $"-F \"{mergeCommitFilePath}\"" },
                { log is not null && log.Value > 0, $"--log={log}" },
                branch
            };
        }
    }
}
