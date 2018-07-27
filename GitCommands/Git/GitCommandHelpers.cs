using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using GitCommands.Git;
using GitCommands.Git.Extensions;
using GitCommands.Logging;
using GitCommands.Patches;
using GitCommands.Utils;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands
{
    /// <summary>Specifies whether to check untracked files/directories (e.g. via 'git status')</summary>
    public enum UntrackedFilesMode
    {
        /// <summary>Default is <see cref="All"/>; when <see cref="UntrackedFilesMode"/> is NOT used, 'git status' uses <see cref="Normal"/>.</summary>
        Default = 1,

        /// <summary>Show no untracked files.</summary>
        No = 2,

        /// <summary>Shows untracked files and directories.</summary>
        Normal = 3,

        /// <summary>Shows untracked files and directories, and individual files in untracked directories.</summary>
        All = 4
    }

    /// <summary>Specifies whether to ignore changes to submodules when looking for changes (e.g. via 'git status').</summary>
    public enum IgnoreSubmodulesMode
    {
        /// <summary>Default is <see cref="All"/> (hides all changes to submodules).</summary>
        Default = 1,

        /// <summary>Consider a submodule modified when it either:
        ///  contains untracked or modified files,
        ///  or its HEAD differs from the commit recorded in the superproject.</summary>
        None = 2,

        /// <summary>Submodules NOT considered dirty when they only contain <i>untracked</i> content
        ///  (but they are still scanned for modified content).</summary>
        Untracked = 3,

        /// <summary>Ignores all changes to the work tree of submodules,
        ///  only changes to the <i>commits</i> stored in the superproject are shown.</summary>
        Dirty = 4,

        /// <summary>Hides all changes to submodules
        ///  (and suppresses the output of submodule summaries when the config option status.submodulesummary is set).</summary>
        All = 5
    }

    public static class GitCommandHelpers
    {
        private static readonly ISshPathLocator SshPathLocatorInstance = new SshPathLocator();

        public static ProcessStartInfo CreateProcessStartInfo(string fileName, string arguments, string workingDirectory, Encoding outputEncoding)
        {
            return new ProcessStartInfo
            {
                UseShellExecute = false,
                ErrorDialog = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = outputEncoding,
                StandardErrorEncoding = outputEncoding,
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory
            };
        }

        [NotNull]
        internal static Process StartProcess(string fileName, string arguments, string workingDirectory, Encoding outputEncoding)
        {
            EnvironmentConfiguration.SetEnvironmentVariables();

            var operation = CommandLog.LogProcessStart(fileName, arguments);

            var startInfo = CreateProcessStartInfo(fileName, arguments, workingDirectory, outputEncoding);
            var process = Process.Start(startInfo);
            process.EnableRaisingEvents = true;
            operation.SetProcessId(process.Id);

            void ProcessExited(object sender, EventArgs args)
            {
                process.Exited -= ProcessExited;
                operation.LogProcessEnd();
            }

            process.Exited += ProcessExited;

            return process;
        }

        public static bool UseSsh(string arguments)
        {
            var x = !Plink() && GetArgumentsRequiresSsh(arguments);
            return x || arguments.Contains("plink");
        }

        private static bool GetArgumentsRequiresSsh(string arguments)
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

        /// <summary>
        /// Transforms the given input Url to make it compatible with Plink, if necessary
        /// </summary>
        public static string GetPlinkCompatibleUrl(string inputUrl)
        {
            // We don't need putty for http:// links and git@... urls are already usable.
            // But ssh:// urls can cause problems
            if (!inputUrl.StartsWith("ssh") || !Uri.IsWellFormedUriString(inputUrl, UriKind.Absolute))
            {
                return "\"" + inputUrl + "\"";
            }

            // Turn ssh://user@host/path into user@host:path, which works better
            var uri = new Uri(inputUrl, UriKind.Absolute);
            string fixedUrl = "";
            if (!uri.IsDefaultPort)
            {
                fixedUrl += "-P " + uri.Port + " ";
            }

            fixedUrl += "\"";

            if (!string.IsNullOrEmpty(uri.UserInfo))
            {
                fixedUrl += uri.UserInfo + "@";
            }

            fixedUrl += uri.Host;
            fixedUrl += ":" + uri.LocalPath.Substring(1) + "\"";

            return fixedUrl;
        }

        private static IEnumerable<string> StartProcessAndReadLines(string arguments, string cmd, string workDir, string stdInput)
        {
            if (string.IsNullOrEmpty(cmd))
            {
                yield break;
            }

            // process used to execute external commands
            using (var process = StartProcess(cmd, arguments, workDir, GitModule.SystemEncoding))
            {
                if (!string.IsNullOrEmpty(stdInput))
                {
                    process.StandardInput.Write(stdInput);
                    process.StandardInput.Close();
                }

                string line;
                do
                {
                    line = process.StandardOutput.ReadLine();
                    if (line != null)
                    {
                        yield return line;
                    }
                }
                while (line != null);

                do
                {
                    line = process.StandardError.ReadLine();
                    if (line != null)
                    {
                        yield return line;
                    }
                }
                while (line != null);

                process.WaitForExit();
            }
        }

        /// <summary>
        /// Run command, console window is hidden, wait for exit, redirect output
        /// </summary>
        public static IEnumerable<string> ReadCmdOutputLines(string cmd, string arguments, string workDir, string stdInput)
        {
            EnvironmentConfiguration.SetEnvironmentVariables();
            arguments = arguments.Replace("$QUOTE$", "\\\"");
            return StartProcessAndReadLines(arguments, cmd, workDir, stdInput);
        }

        [CanBeNull]
        private static Process StartProcessAndReadAllText(string arguments, string cmd, string workDir, out string stdOutput, out string stdError, string stdInput)
        {
            if (string.IsNullOrEmpty(cmd))
            {
                stdOutput = stdError = "";
                return null;
            }

            // process used to execute external commands
            var process = StartProcess(cmd, arguments, workDir, GitModule.SystemEncoding);
            if (!string.IsNullOrEmpty(stdInput))
            {
                process.StandardInput.Write(stdInput);
                process.StandardInput.Close();
            }

            SynchronizedProcessReader.Read(process, out stdOutput, out stdError);
            return process;
        }

        /// <summary>
        /// Run command, console window is hidden, wait for exit, redirect output
        /// </summary>
        public static string RunCmd(string cmd, string arguments)
        {
            try
            {
                EnvironmentConfiguration.SetEnvironmentVariables();

                arguments = arguments.Replace("$QUOTE$", "\\\"");

                string output, error;
                using (var process = StartProcessAndReadAllText(arguments, cmd, "", out output, out error, null))
                {
                    process.WaitForExit();
                }

                if (!string.IsNullOrEmpty(error))
                {
                    output += Environment.NewLine + error;
                }

                return output;
            }
            catch (Win32Exception)
            {
                return string.Empty;
            }
        }

        [CanBeNull]
        private static Process StartProcessAndReadAllBytes(string arguments, string cmd, string workDir, out byte[] stdOutput, out byte[] stdError, byte[] stdInput)
        {
            if (string.IsNullOrEmpty(cmd))
            {
                stdOutput = stdError = null;
                return null;
            }

            // process used to execute external commands
            var process = StartProcess(cmd, arguments, workDir, Encoding.Default);
            if (stdInput != null && stdInput.Length > 0)
            {
                process.StandardInput.BaseStream.Write(stdInput, 0, stdInput.Length);
                process.StandardInput.Close();
            }

            SynchronizedProcessReader.ReadBytes(process, out stdOutput, out stdError);

            return process;
        }

        /// <summary>
        /// Run command, console window is hidden, wait for exit, redirect output
        /// </summary>
        public static int RunCmdByte(string cmd, string arguments, string workingDir, byte[] stdInput, out byte[] output, out byte[] error)
        {
            try
            {
                arguments = arguments.Replace("$QUOTE$", "\\\"");
                using (var process = StartProcessAndReadAllBytes(arguments, cmd, workingDir, out output, out error, stdInput))
                {
                    process.WaitForExit();
                    return process.ExitCode;
                }
            }
            catch (Win32Exception)
            {
                output = error = null;
                return 1;
            }
        }

        private static GitVersion _versionInUse;

        public static GitVersion VersionInUse
        {
            get
            {
                if (_versionInUse == null || _versionInUse.IsUnknown)
                {
                    var result = RunCmd(AppSettings.GitCommand, "--version");
                    _versionInUse = new GitVersion(result);
                }

                return _versionInUse;
            }
        }

        public static string CherryPickCmd(string cherry, bool commit, string arguments)
        {
            var args = new ArgumentBuilder
            {
                "cherry-pick",
                { !commit, "--no-commit" },
                arguments,
                cherry.Quote()
            };

            return args.ToString();
        }

        public static string DeleteTagCmd(string tagName)
        {
            return "tag -d \"" + tagName + "\"";
        }

        public static string SubmoduleUpdateCmd(string name)
        {
            name = name ?? "";
            return SubmoduleUpdateCommand(name.Trim().QuoteNE());
        }

        public static string SubmoduleUpdateCmd(IEnumerable<string> submodules)
        {
            string submodulesQuoted = string.Join(" ", submodules.Select(s => s.Trim().QuoteNE()));
            return SubmoduleUpdateCommand(submodulesQuoted);
        }

        private static string SubmoduleUpdateCommand(string name)
        {
            return "submodule update --init --recursive " + name;
        }

        public static string SubmoduleSyncCmd(string name)
        {
            var args = new ArgumentBuilder
            {
                "submodule sync",
                name?.Trim().QuoteNE()
            };

            return args.ToString();
        }

        public static string AddSubmoduleCmd(string remotePath, string localPath, string branch, bool force)
        {
            var args = new ArgumentBuilder
            {
                "submodule add",
                { force, "-f" },
                { !string.IsNullOrEmpty(branch), $"-b \"{branch?.Trim()}\"" },
                remotePath.ToPosixPath().Quote(),
                localPath.ToPosixPath().Quote()
            };

            return args.ToString();
        }

        public static string RevertCmd(string commit, bool autoCommit, int parentIndex)
        {
            var args = new ArgumentBuilder
            {
                "revert",
                { !autoCommit, "--no-commit" },
                { parentIndex > 0, $"-m {parentIndex}" },
                commit
            };

            return args.ToString();
        }

        public static string ResetSoftCmd(string commit)
        {
            return "reset --soft \"" + commit + "\"";
        }

        public static string ResetMixedCmd(string commit)
        {
            return "reset --mixed \"" + commit + "\"";
        }

        public static string ResetHardCmd(string commit)
        {
            return "reset --hard \"" + commit + "\"";
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
        public static string CloneCmd(string fromPath, string toPath, bool central = false, bool initSubmodules = false, [CanBeNull] string branch = "", int? depth = null, bool? isSingleBranch = null, bool lfs = false)
        {
            var from = PathUtil.IsLocalFile(fromPath) ? fromPath.ToPosixPath() : fromPath;

            var args = new ArgumentBuilder
            {
                { lfs, "lfs" },
                "clone",
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

            return args.ToString();
        }

        public static string CheckoutCmd(string branchOrRevisionName, LocalChangesAction changesAction)
        {
            var args = new ArgumentBuilder
            {
                "checkout",
                { changesAction == LocalChangesAction.Merge, "--merge" },
                { changesAction == LocalChangesAction.Reset, "--force" },
                branchOrRevisionName.Quote()
            };

            return args.ToString();
        }

        /// <summary>Create a new orphan branch from <paramref name="startPoint"/> and switch to it.</summary>
        public static string CreateOrphanCmd(string newBranchName, ObjectId startPoint = null)
        {
            return $"checkout --orphan {newBranchName} {startPoint}";
        }

        /// <summary>Remove files from the working tree and from the index. <remarks>git rm</remarks></summary>
        /// <param name="force">Override the up-to-date check.</param>
        /// <param name="isRecursive">Allow recursive removal when a leading directory name is given.</param>
        /// <param name="files">Files to remove. File globs can be given to remove matching files.</param>
        public static string RemoveCmd(bool force = true, bool isRecursive = true, params string[] files)
        {
            var args = new ArgumentBuilder
            {
                "rm",
                { force, "--force" },
                { isRecursive, "-r" },
                { files.Length == 0, "." },
                files
            };

            return args.ToString();
        }

        public static string BranchCmd(string branchName, string revision, bool checkout)
        {
            var args = new ArgumentBuilder
            {
                { checkout, "checkout -b", "branch" },
                branchName.Trim().Quote(),
                revision?.Trim().QuoteNE()
            };

            return args.ToString();
        }

        public static string MergedBranches(bool includeRemote = false)
        {
            return includeRemote
                ? "branch -a --merged"
                : "branch --merged";
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
            var sshString = SshPathLocatorInstance.Find(AppSettings.GitBinDir);

            return sshString.EndsWith("plink.exe", StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>Pushes multiple sets of local branches to remote branches.</summary>
        public static string PushMultipleCmd(string remote, IEnumerable<GitPushAction> pushActions)
        {
            remote = remote.ToPosixPath();
            return new GitPush(remote, pushActions)
            {
                ReportProgress = VersionInUse.PushCanAskForProgress
            }.ToString();
        }

        public static string PushTagCmd(string path, string tag, bool all, ForcePushOptions force = ForcePushOptions.DoNotForce)
        {
            if (!all && string.IsNullOrWhiteSpace(tag))
            {
                // TODO this is probably an error
                return "";
            }

            var args = new ArgumentBuilder
            {
                "push",
                force,
                { VersionInUse.PushCanAskForProgress, "--progress" },
                path.ToPosixPath().Trim().Quote(),
                { all, "--tags" },
                { !all, $"tag {tag.Replace(" ", "")}" }
            };

            return args.ToString();
        }

        public static string StashSaveCmd(bool untracked, bool keepIndex, string message, IReadOnlyList<string> selectedFiles)
        {
            var isPartialStash = selectedFiles != null && selectedFiles.Any();

            var args = new ArgumentBuilder
            {
                "stash",
                { isPartialStash, "push", "save" },
                { untracked && VersionInUse.StashUntrackedFilesSupported, "-u" },
                { keepIndex, "--keep-index" },
                message.QuoteNE(),
                { isPartialStash, "--" },
                { isPartialStash, selectedFiles }
            };

            return args.ToString();
        }

        public static string ContinueRebaseCmd()
        {
            return "rebase --continue";
        }

        public static string SkipRebaseCmd()
        {
            return "rebase --skip";
        }

        public static string StartBisectCmd()
        {
            return "bisect start";
        }

        public static string ContinueBisectCmd(GitBisectOption bisectOption, params string[] revisions)
        {
            var args = new ArgumentBuilder
            {
                "bisect",
                bisectOption,
                revisions
            };

            return args.ToString();
        }

        public static string StopBisectCmd()
        {
            return "bisect reset";
        }

        public static string RebaseCmd(string branch, bool interactive, bool preserveMerges, bool autosquash, bool autoStash, string from = null, string onto = null)
        {
            if (from == null ^ onto == null)
            {
                throw new ArgumentException($"For arguments \"{nameof(from)}\" and \"{nameof(onto)}\", either both must have values, or neither may.");
            }

            var args = new ArgumentBuilder
            {
                "rebase",
                { interactive, "-i" },
                { interactive && autosquash, "--autosquash" },
                { interactive && !autosquash, "--no-autosquash" },
                { preserveMerges, "--preserve-merges" },
                { autoStash, "--autostash" },
                from.QuoteNE(),
                branch.Quote(),
                { onto != null, $"--onto {onto}" }
            };

            return args.ToString();
        }

        public static string AbortRebaseCmd()
        {
            return "rebase --abort";
        }

        public static string ResolvedCmd()
        {
            return "am --3way --resolved";
        }

        public static string SkipCmd()
        {
            return "am --3way --skip";
        }

        public static string AbortCmd()
        {
            return "am --3way --abort";
        }

        [NotNull]
        public static string ApplyMailboxPatchCmd(bool ignoreWhiteSpace, string patchFile = null)
        {
            var args = new ArgumentBuilder
            {
                "am",
                "--3way",
                "--signoff",
                { ignoreWhiteSpace, "--ignore-whitespace" },
                patchFile?.ToPosixPath().Quote()
            };

            return args.ToString();
        }

        [NotNull]
        public static string ApplyDiffPatchCmd(bool ignoreWhiteSpace, [NotNull] string patchFile)
        {
            var args = new ArgumentBuilder
            {
                "apply",
                { ignoreWhiteSpace, "--ignore-whitespace" },
                patchFile.ToPosixPath().Quote()
            };

            return args.ToString();
        }

        public static string CleanUpCmd(bool dryRun, bool directories, bool nonIgnored, bool ignored, string paths = null)
        {
            var args = new ArgumentBuilder
            {
                "clean",
                { directories, "-d" },
                { !nonIgnored && !ignored, "-x" },
                { ignored, "-X" },
                { dryRun, "--dry-run" },
                { !dryRun, "-f" },
                paths
            };

            return args.ToString();
        }

        public static string GetAllChangedFilesCmd(bool excludeIgnoredFiles, UntrackedFilesMode untrackedFiles, IgnoreSubmodulesMode ignoreSubmodules = IgnoreSubmodulesMode.None, bool noLocks = false)
        {
            var args = new ArgumentBuilder
            {
                { noLocks && VersionInUse.SupportNoOptionalLocks, "--no-optional-locks" },
                $"status --porcelain={(VersionInUse.SupportStatusPorcelainV2 ? 2 : 1)} -z",
                untrackedFiles,
                ignoreSubmodules,
                { !excludeIgnoredFiles, "--ignored" }
            };

            return args.ToString();
        }

        [CanBeNull]
        public static GitSubmoduleStatus GetCurrentSubmoduleChanges(GitModule module, string fileName, string oldFileName, bool staged)
        {
            Patch patch = module.GetCurrentChanges(fileName, oldFileName, staged, "", module.FilesEncoding);
            string text = patch != null ? patch.Text : "";
            return ParseSubmoduleStatus(text, module, fileName);
        }

        [CanBeNull]
        public static GitSubmoduleStatus GetCurrentSubmoduleChanges(GitModule module, string submodule)
        {
            return GetCurrentSubmoduleChanges(module, submodule, submodule, false);
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
                var submodule = module.GetSubmodule(fileName);
                addedCommits = submodule.GetCommitCount(commitId.ToString(), oldCommitId.ToString());
                removedCommits = submodule.GetCommitCount(oldCommitId.ToString(), commitId.ToString());
            }

            return new GitSubmoduleStatus(name, oldName, isDirty, commitId, oldCommitId, addedCommits, removedCommits);
        }

        /// <summary>
        /// Parse the output from git-diff --name-status
        /// </summary>
        /// <param name="module">The Git module</param>
        /// <param name="statusString">output from the git command</param>
        /// <param name="firstRevision">from revision string</param>
        /// <param name="secondRevision">to revision</param>
        /// <param name="parentToSecond">The parent for the second revision</param>
        /// <returns>list with the parsed GitItemStatus</returns>
        /// <seealso href="https://git-scm.com/docs/git-diff"/>
        /// <remarks>Git revisions are required to determine if the <see cref="GitItemStatus"/> are WorkTree or Index.</remarks>
        public static IReadOnlyList<GitItemStatus> GetDiffChangedFilesFromString(IGitModule module, string statusString, [CanBeNull] string firstRevision, [CanBeNull] string secondRevision, [CanBeNull] string parentToSecond)
        {
            StagedStatus staged;
            if (firstRevision == GitRevision.IndexGuid && secondRevision == GitRevision.WorkTreeGuid)
            {
                staged = StagedStatus.WorkTree;
            }
            else if (firstRevision == parentToSecond && secondRevision == GitRevision.IndexGuid)
            {
                staged = StagedStatus.Index;
            }
            else if ((firstRevision.IsNotNullOrWhitespace() && !firstRevision.IsArtificial()) ||
                (secondRevision.IsNotNullOrWhitespace() && !secondRevision.IsArtificial()) ||
                parentToSecond.IsNotNullOrWhitespace())
            {
                // This cannot be a worktree/index file
                staged = StagedStatus.None;
            }
            else
            {
                staged = StagedStatus.Unknown;
            }

            return GetAllChangedFilesFromString_v1(module, statusString, true, staged);
        }

        /// <summary>
        /// Parse the output from git-status --porcelain -z
        /// </summary>
        /// <param name="module">The Git module</param>
        /// <param name="statusString">output from the git command</param>
        /// <returns>list with the parsed GitItemStatus</returns>
        /// <seealso href="https://git-scm.com/docs/git-status"/>
        public static IReadOnlyList<GitItemStatus> GetStatusChangedFilesFromString(IGitModule module, string statusString)
        {
            if (VersionInUse.SupportStatusPorcelainV2)
            {
                return GetAllChangedFilesFromString_v2(statusString);
            }
            else
            {
                return GetAllChangedFilesFromString_v1(module, statusString, false, StagedStatus.Index);
            }
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

            // Split all files on '\0'
            var files = statusString.Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
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
                        string[] renames = line.Substring(114).Split(new char[] { ' ' }, 2);
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
                        // suppress warning for variable not assigned
                        fileName = null;
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

                        // Is dirty
                        gitItemStatus.IsTracked = subm[2] != 'M' && subm[3] != 'U';
                    }
                }

                diffFiles.Add(gitItemStatus);
            }
        }

        /// <summary>
        /// Parse git-status --porcelain=1 and git-diff --name-status
        /// Outputs are similar, except that git-status has status for both worktree and index
        /// </summary>
        private static IReadOnlyList<GitItemStatus> GetAllChangedFilesFromString_v1(IGitModule module, string statusString, bool fromDiff, StagedStatus staged)
        {
            var diffFiles = new List<GitItemStatus>();

            if (string.IsNullOrEmpty(statusString))
            {
                return diffFiles;
            }

            // The status string from git-diff can show warnings. See tests
            var nl = new[] { '\n', '\r' };
            string trimmedStatus = statusString.Trim(nl);
            int lastNewLinePos = trimmedStatus.LastIndexOfAny(nl);
            if (lastNewLinePos > 0)
            {
                int ind = trimmedStatus.LastIndexOf('\0');
                if (ind < lastNewLinePos)
                {
                    // Warning at end
                    lastNewLinePos = trimmedStatus.IndexOfAny(nl, ind >= 0 ? ind : 0);
                    trimmedStatus = trimmedStatus.Substring(0, lastNewLinePos).Trim(nl);
                }
                else
                {
                    // Warning at beginning
                    trimmedStatus = trimmedStatus.Substring(lastNewLinePos).Trim(nl);
                }
            }

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

                int splitIndex = files[n].IndexOfAny(new[] { '\0', '\t', ' ' }, 1);

                string status;
                string fileName;

                if (splitIndex < 0)
                {
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
                GitItemStatus gitItemStatus = GitItemStatusFromStatusCharacter(StagedStatus.Unknown, fileName, statusCharacter);
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
                GitItemStatus gitItemStatus = GitItemStatusFromStatusCharacter(StagedStatus.Unknown, fileName, statusCharacter);
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
                gitItemStatus.OldName = fileName.Trim();
                gitItemStatus.Name = nextFile.Trim();
            }
            else
            {
                gitItemStatus.Name = fileName.Trim();
                gitItemStatus.OldName = nextFile.Trim();
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
                Name = fileName.Trim(),
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

        public static string MergeBranchCmd(string branch, bool allowFastForward, bool squash, bool noCommit, string strategy, bool allowUnrelatedHistories, string message, int? log)
        {
            // TODO Quote should escape any " characters, at least for usages like the below

            var args = new ArgumentBuilder
            {
                "merge",
                { !allowFastForward, "--no-ff" },
                { !string.IsNullOrEmpty(strategy), $"--strategy={strategy}" },
                { squash, "--squash" },
                { noCommit, "--no-commit" },
                { allowUnrelatedHistories, "--allow-unrelated-histories" },
                { !string.IsNullOrEmpty(message), $"-m {message.Quote()}" },
                { log != null, $"--log={log}" },
                branch
            };

            return args.ToString();
        }

        // returns " --find-renames=..." according to app settings
        public static string FindRenamesOpt()
        {
            string result = " --find-renames";
            if (AppSettings.FollowRenamesInFileHistoryExactOnly)
            {
                result += "=\"100%\"";
            }

            return result;
        }

        // returns " --find-renames=... --find-copies=..." according to app settings
        public static string FindRenamesAndCopiesOpts()
        {
            string findCopies = " --find-copies";
            if (AppSettings.FollowRenamesInFileHistoryExactOnly)
            {
                findCopies += "=\"100%\"";
            }

            return FindRenamesOpt() + findCopies;
        }

        private static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            public static extern bool SetConsoleCtrlHandler(IntPtr HandlerRoutine, bool Add);

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
                NativeMethods.SetConsoleCtrlHandler(IntPtr.Zero, true);
                NativeMethods.GenerateConsoleCtrlEvent(0, 0);
                if (!process.HasExited)
                {
                    System.Threading.Thread.Sleep(500);
                }
            }

            if (!process.HasExited)
            {
                process.Kill();
            }
        }
    }
}
