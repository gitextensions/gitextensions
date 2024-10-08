using GitCommands.Config;
using GitCommands.Utils;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUIPluginInterfaces;

namespace GitCommands.Git
{
    public static partial class Commands
    {
        public static ArgumentString Abort()
        {
            return new GitArgumentBuilder("am")
            {
                "--3way",
                "--abort"
            };
        }

        public static ArgumentString AbortMerge()
        {
            return new GitArgumentBuilder("merge") { "--abort" };
        }

        public static ArgumentString AbortRebase()
        {
            return new GitArgumentBuilder("rebase") { "--abort" };
        }

        public static ArgumentString AddSubmodule(string remotePath, string localPath, string branch, bool force, IEnumerable<GitConfigItem> configs = null)
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

        public static ArgumentString ApplyDiffPatch(bool ignoreWhiteSpace, string patchFile, Func<string, string?> getPathForGitExecution)
        {
            return new GitArgumentBuilder("apply")
            {
                { ignoreWhiteSpace, "--ignore-whitespace" },
                getPathForGitExecution(patchFile).QuoteNE()
            };
        }

        public static ArgumentString ApplyMailboxPatch(bool signOff, bool ignoreWhiteSpace)
            => ApplyMailboxPatch(signOff, ignoreWhiteSpace, patchFile: null, path => path);

        public static ArgumentString ApplyMailboxPatch(bool signOff, bool ignoreWhiteSpace, string? patchFile, Func<string, string?> getPathForGitExecution)
        {
            return new GitArgumentBuilder("am")
            {
                "--3way",
                { signOff, "--signoff" },
                { ignoreWhiteSpace, "--ignore-whitespace" },
                getPathForGitExecution(patchFile).QuoteNE()
            };
        }

        public static ArgumentString Branch(string branchName, string revision, bool checkout)
        {
            return new GitArgumentBuilder(checkout ? "checkout" : "branch")
            {
                { checkout, "-b" },
                branchName.Trim().Quote(),
                revision?.Trim().QuoteNE()
            };
        }

        public static ArgumentString Checkout(string branchOrRevisionName, LocalChangesAction changesAction)
        {
            return new GitArgumentBuilder("checkout")
            {
                { changesAction == LocalChangesAction.Merge, "--merge" },
                { changesAction == LocalChangesAction.Reset, "--force" },
                branchOrRevisionName.Quote()
            };
        }

        public static ArgumentString CherryPick(ObjectId commitId, bool commit, string arguments)
        {
            return new GitArgumentBuilder("cherry-pick")
            {
                { !commit, "--no-commit" },
                arguments,
                commitId
            };
        }

        /// <summary>
        /// Arguments for git-clean.
        /// </summary>
        /// <param name="mode">The cleanup mode what to delete.</param>
        /// <param name="dryRun">Only show what would be deleted.</param>
        /// <param name="directories">Delete untracked directories too.</param>
        /// <param name="paths">Limit to specific paths.</param>
        /// <param name="excludes">Exclude certain files.</param>
        public static ArgumentString Clean(CleanMode mode, bool dryRun, bool directories, string? paths = null, string? excludes = null)
        {
            return new GitArgumentBuilder("clean")
            {
                mode,
                { directories, "-d" },
                { dryRun, "--dry-run", "-f" },
                paths,
                { !string.IsNullOrEmpty(excludes), excludes }
            };
        }

        /// <summary>
        /// Arguments for cleaning submodules.
        /// </summary>
        /// <param name="mode">The cleanup mode what to delete.</param>
        /// <param name="dryRun">Only show what would be deleted.</param>
        /// <param name="directories">Delete untracked directories too.</param>
        /// <param name="paths">Limit to specific paths.</param>
        public static ArgumentString CleanSubmodules(CleanMode mode, bool dryRun, bool directories, string? paths = null)
        {
            return new GitArgumentBuilder("submodule")
            {
                "foreach --recursive git clean",
                mode,
                { directories, "-d" },
                { dryRun, "--dry-run", "-f" },
                paths
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
        public static ArgumentString Clone(string fromPath, string toPath, Func<string, string?> getPathForGitExecution, bool central = false, bool initSubmodules = false, string? branch = "", int? depth = null, bool? isSingleBranch = null)
        {
            fromPath = fromPath.Trim();
            if (PathUtil.IsLocalFile(fromPath))
            {
                fromPath = getPathForGitExecution(fromPath);
            }

            toPath = getPathForGitExecution(toPath.Trim());

            DebugHelpers.Assert(!EnvUtils.RunningOnWindows() || fromPath.IndexOf(PathUtil.NativeDirectorySeparatorChar) < 0,
               $"'CloneCmd' must be called with 'fromPath' in Posix format");
            DebugHelpers.Assert(!EnvUtils.RunningOnWindows() || toPath.IndexOf(PathUtil.NativeDirectorySeparatorChar) < 0,
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
                fromPath.Quote(),
                toPath.Quote()
            };
        }

        public static ArgumentString Commit(bool amend, bool signOff, string author, bool useExplicitCommitMessage, string? commitMessageFile, Func<string, string?> getPathForGitExecution, bool noVerify = false, bool? gpgSign = null, string gpgKeyId = "", bool allowEmpty = false, bool resetAuthor = false)
        {
            if (useExplicitCommitMessage && string.IsNullOrEmpty(commitMessageFile))
            {
                throw new ArgumentException("Required", nameof(commitMessageFile));
            }

            return new GitArgumentBuilder("commit")
            {
                { amend, "--amend" },
                { noVerify, "--no-verify" },
                { signOff, "--signoff" },
                { !string.IsNullOrEmpty(author), $"--author=\"{author?.Trim().Trim('"')}\"" },
                { gpgSign is false, "--no-gpg-sign" },
                { gpgSign is true && string.IsNullOrWhiteSpace(gpgKeyId), "--gpg-sign" },
                { gpgSign is true && !string.IsNullOrWhiteSpace(gpgKeyId), $"--gpg-sign={gpgKeyId}" },
                { useExplicitCommitMessage, $"-F {getPathForGitExecution(commitMessageFile).Quote()}" },
                { allowEmpty, "--allow-empty" },
                { resetAuthor && amend, "--reset-author" }
            };
        }

        public static ArgumentString ContinueBisect(GitBisectOption bisectOption, params ObjectId[] revisions)
        {
            return new GitArgumentBuilder("bisect")
            {
                bisectOption,
                revisions
            };
        }

        public static ArgumentString ContinueMerge()
        {
            return new GitArgumentBuilder("merge") { "--continue" };
        }

        public static ArgumentString ContinueRebase()
        {
            return new GitArgumentBuilder("rebase") { "--continue" };
        }

        /// <summary>Create a new orphan branch from <paramref name="startPoint"/> and switch to it.</summary>
        public static ArgumentString CreateOrphan(string newBranchName, ObjectId? startPoint = null)
        {
            return new GitArgumentBuilder("checkout")
            {
                "--orphan",
                newBranchName,
                startPoint
            };
        }

        public static ArgumentString EditTodoRebase()
        {
            return new GitArgumentBuilder("rebase") { "--edit-todo" };
        }

        public static ArgumentString GetAllChangedFiles(bool excludeIgnoredFiles, UntrackedFilesMode untrackedFiles, IgnoreSubmodulesMode ignoreSubmodules = IgnoreSubmodulesMode.None, bool noLocks = false)
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

        /// <summary>
        /// Gets <see cref="GitConfigItem"/> that sets 'protocol.file.allow' to always.
        /// </summary>
        /// <remarks>IEnumerable to allow future concat with other needed configs.</remarks>
        /// <returns>Config with 'protocol.file.allow set to always</returns>
        public static IEnumerable<GitConfigItem> GetAllowFileConfig()
        {
            yield return new GitConfigItem(SettingKeyString.AllowFileProtocol, "always");
        }

        public static ArgumentString GetCurrentChanges(string? fileName, string? oldFileName, bool staged, string extraDiffArguments, bool noLocks)
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

        public static ArgumentString GetRefs(RefsFilter getRef, bool noLocks, GitRefsSortBy sortBy, GitRefsSortOrder sortOrder, int count = 0)
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

                // special handling for GitRefsSortBy.versionRefname as enum string is not the same as git sort key
                // also, colon cannot be included in the deref sort key at all
                const string gitRefsSortByVersion = "version:refname";
                string sortKey = sortBy == GitRefsSortBy.versionRefname ? gitRefsSortByVersion : sortBy.ToString();
                string order = sortOrder == GitRefsSortOrder.Ascending ? string.Empty : "-";
                if (!needTags)
                {
                    return $@"--sort=""{order}{sortKey}""";
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
                string derefSortKey = (sortBy == GitRefsSortBy.versionRefname ? GitRefsSortBy.refname : sortBy).ToString();
                return $@"--sort=""{order}*{derefSortKey}"" --sort=""{order}{sortKey}""";
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

                ArgumentBuilder builder = new()
                {
                    { (option & RefsFilter.Heads) != 0, "refs/heads/" },
                    { (option & RefsFilter.Remotes) != 0, "refs/remotes/" },
                    { (option & RefsFilter.Tags) != 0, "refs/tags/" }
                };

                return builder;
            }
        }

        public static ArgumentString MergeBranch(string branch, bool allowFastForward, bool squash, bool noCommit, string strategy, bool allowUnrelatedHistories, string? mergeCommitFilePath, Func<string, string?> getPathForGitExecution, int? log)
        {
            return new GitArgumentBuilder("merge")
            {
                { !allowFastForward, "--no-ff" },
                { !string.IsNullOrEmpty(strategy), $"--strategy={strategy}" },
                { squash, "--squash" },
                { noCommit, "--no-commit" },
                { allowUnrelatedHistories, "--allow-unrelated-histories" },

                 // let git fail, if the file doesn't exist
                { !string.IsNullOrWhiteSpace(mergeCommitFilePath), $"-F {getPathForGitExecution(mergeCommitFilePath).Quote()}" },
                { log is not null && log.Value > 0, $"--log={log}" },
                branch
            };
        }

        public static ArgumentString MergedBranches(bool includeRemote = false, bool fullRefname = false, string? commit = null)
        {
            return new GitArgumentBuilder("branch")
            {
                { fullRefname, @"--format=""%(refname)""" },
                { includeRemote, "-a" },
                "--merged",
                commit.QuoteNE()
            };
        }

        /// <summary>
        ///  Creates a <c>git push</c> command using the specified parameters.</summary>
        /// <param name="remote">The remote repository that is the destination of the push operation.</param>
        /// <param name="fromBranch">
        ///  The fully-qualified name of the branch to push (i.e., the name must start with <c>refs/heads/</c>).
        /// </param>
        /// <param name="toBranch">Name of the ref on the remote side to update with the push.</param>
        /// <param name="force">If a remote ref is not an ancestor of the local ref, overwrite it.</param>
        /// <param name="track">For every branch that is up to date or successfully pushed, add upstream (tracking) reference.</param>
        /// <param name="recursiveSubmodules">
        ///  If '1', check whether all submodule commits used by the revisions to be pushed are available on a remote
        ///  tracking branch; otherwise, the push will be aborted.
        /// </param>
        /// <returns>'git push' command with the specified parameters.</returns>
        public static ArgumentString Push(string remote, string fromBranch, string? toBranch, ForcePushOptions force, bool track, int recursiveSubmodules)
        {
            ArgumentNullException.ThrowIfNull(remote);
            ArgumentNullException.ThrowIfNull(fromBranch);

            toBranch = GitRefName.GetFullBranchName(toBranch);

            if (string.IsNullOrEmpty(fromBranch) && !string.IsNullOrEmpty(toBranch))
            {
                fromBranch = "HEAD";
            }

            // TODO make an enum for RecursiveSubmodulesOption and add to ArgumentBuilderExtensions
            return new GitArgumentBuilder("push")
            {
                force,
                { track, "-u" },
                { recursiveSubmodules == 1, "--recurse-submodules=check" },
                { recursiveSubmodules == 2, "--recurse-submodules=on-demand" },
                "--progress",
                remote.ToPosixPath().Trim().Quote(),
                { string.IsNullOrEmpty(toBranch), fromBranch },
                { !string.IsNullOrEmpty(toBranch), $"{fromBranch}:{toBranch}" }
            };
        }

        /// <summary>
        ///  Creates a 'git push' command using the specified parameters.
        /// </summary>
        /// <param name="remote">The remote repository that is the destination of the push operation.</param>
        /// <param name="force">If a remote ref is not an ancestor of the local ref, overwrite it.</param>
        /// <param name="track">For every branch that is up to date or successfully pushed, add upstream (tracking) reference.</param>
        /// <param name="recursiveSubmodules">
        ///  If '1', check whether all submodule commits used by the revisions to be pushed are available on a remote
        ///  tracking branch; otherwise, the push will be aborted.
        /// </param>
        /// <returns>'git push' command with the specified parameters.</returns>
        public static ArgumentString PushAll(string remote, ForcePushOptions force, bool track, int recursiveSubmodules)
        {
            // TODO make an enum for RecursiveSubmodulesOption and add to ArgumentBuilderExtensions
            return new GitArgumentBuilder("push")
            {
                force,
                { track, "-u" },
                { recursiveSubmodules == 1, "--recurse-submodules=check" },
                { recursiveSubmodules == 2, "--recurse-submodules=on-demand" },
                "--progress",
                "--all",
                remote.ToPosixPath().Trim().Quote()
            };
        }

        /// <summary>
        /// Push a local reference to a new commit
        /// This is similar to "git branch --force "branch" "commit", except that you get a warning if commits are lost.
        /// </summary>
        /// <param name="gitRef">The branch to move.</param>
        /// <param name="targetId">The commit to move to.</param>
        /// <param name="repoDir">Directory to the current repo.</param>
        /// <param name="force">Push the reference also if commits are lost.</param>
        /// <returns>The Git command to execute.</returns>
        public static ArgumentString PushLocal(string gitRef, ObjectId targetId, string repoDir, Func<string, string?> getPathForGitExecution, bool force = false)
        {
            return new GitArgumentBuilder("push")
            {
                $@"""file://{getPathForGitExecution(repoDir)}""",
                $"{targetId}:{gitRef}".QuoteNE(),
                { force, "--force" }
            };
        }

        /// <summary>Pushes multiple sets of local branches to remote branches.</summary>
        public static ArgumentString PushMultiple(string remote, IEnumerable<GitPushAction> pushActions)
        {
            return new GitPush(remote.ToPosixPath(), pushActions)
            {
                ReportProgress = true
            }.ToString();
        }

        public static ArgumentString PushTag(string path, string tag, bool all, ForcePushOptions force = ForcePushOptions.DoNotForce)
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

        public static ArgumentString Rebase(in RebaseOptions rebaseOptions)
        {
            // TODO-NULLABLE does it make sense for 'branch' to be null here?

            if (rebaseOptions.From is null ^ rebaseOptions.OnTo is null)
            {
                throw new ArgumentException($"For arguments \"{nameof(rebaseOptions.From)}\" and \"{nameof(rebaseOptions.OnTo)}\", either both must have values, or neither may.");
            }

            GitArgumentBuilder builder = new("rebase");
            if (rebaseOptions.IgnoreDate)
            {
                builder.Add("--ignore-date");
            }
            else if (rebaseOptions.CommitterDateIsAuthorDate)
            {
                builder.Add("--committer-date-is-author-date");
            }
            else
            {
                if (rebaseOptions.Interactive)
                {
                    builder.Add("-i");
                    builder.Add(rebaseOptions.AutoSquash ? "--autosquash" : "--no-autosquash");
                }

                if (rebaseOptions.PreserveMerges)
                {
                    builder.Add(rebaseOptions.SupportRebaseMerges ? "--rebase-merges" : "--preserve-merges");
                }
            }

            if (rebaseOptions.UpdateRefs.HasValue)
            {
                builder.Add(rebaseOptions.UpdateRefs.Value ? "--update-refs" : "--no-update-refs");
            }

            builder.Add(rebaseOptions.AutoStash, "--autostash");
            builder.Add(rebaseOptions.OnTo is not null, $"--onto {rebaseOptions.OnTo}");
            builder.Add(rebaseOptions.From.QuoteNE());
            builder.Add(rebaseOptions.BranchName.Quote());

            return builder;
        }

        /// <summary>Remove files from the working tree and from the index. <remarks>git rm</remarks></summary>
        /// <param name="force">Override the up-to-date check.</param>
        /// <param name="isRecursive">Allow recursive removal when a leading directory name is given.</param>
        /// <param name="files">Files to remove. File globs can be given to remove matching files.</param>
        public static ArgumentString Remove(bool force = true, bool isRecursive = true, params string[] files)
        {
            return new GitArgumentBuilder("rm")
            {
                { force, "--force" },
                { isRecursive, "-r" },
                { files.Length == 0, "." },
                files
            };
        }

        /// <summary>
        /// The Git command line for reset.
        /// </summary>
        /// <param name="mode">Reset mode.</param>
        /// <param name="commit">Optional commit-ish (for reset-index this is tree-ish and mandatory).</param>
        /// <param name="file">Optional file to reset.</param>
        /// <returns>Argument string.</returns>
        public static ArgumentString Reset(ResetMode mode, string? commit = null, string? file = null)
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

        public static ArgumentString Resolved()
        {
            return new GitArgumentBuilder("am")
            {
                "--3way",
                "--resolved"
            };
        }

        public static ArgumentString Revert(ObjectId commitId, bool autoCommit, int parentIndex)
        {
            return new GitArgumentBuilder("revert")
            {
                { !autoCommit, "--no-commit" },
                { parentIndex > 0, $"-m {parentIndex}" },
                commitId
            };
        }

        public static ArgumentString Skip()
        {
            return new GitArgumentBuilder("am")
            {
                "--3way",
                "--skip"
            };
        }

        public static ArgumentString SkipRebase()
        {
            return new GitArgumentBuilder("rebase") { "--skip" };
        }

        public static ArgumentString StartBisect()
        {
            return new GitArgumentBuilder("bisect") { "start" };
        }

        public static ArgumentString StashSave(bool untracked, bool keepIndex, string message, IReadOnlyList<string>? selectedFiles)
        {
            selectedFiles ??= Array.Empty<string>();

            bool isPartialStash = selectedFiles.Any();

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

        public static ArgumentString StopBisect()
        {
            return new GitArgumentBuilder("bisect") { "reset" };
        }

        public static ArgumentString SubmoduleSync(string? name)
        {
            return new GitArgumentBuilder("submodule")
            {
                "sync",
                name?.Trim().QuoteNE()
            };
        }

        public static ArgumentString SubmoduleUpdate(IEnumerable<string> submodules, IEnumerable<GitConfigItem>? configs = null)
        {
            string submodulesQuoted = string.Join(" ", submodules.Select(s => s.Trim().QuoteNE()));
            return SubmoduleUpdateCommand(submodulesQuoted, configs);
        }

        public static ArgumentString SubmoduleUpdate(string? name, IEnumerable<GitConfigItem>? configs = null)
        {
            return SubmoduleUpdateCommand((name ?? "").Trim().QuoteNE(), configs);
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
    }
}
