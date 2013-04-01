using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using GitCommands.Config;
using GitCommands.Git;
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
        public static void SetEnvironmentVariable()
        {
            SetEnvironmentVariable(false);
        }

        public static void SetEnvironmentVariable(bool reload)
        {
            string path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            if (!string.IsNullOrEmpty(Settings.GitBinDir) && !path.Contains(Settings.GitBinDir))
                Environment.SetEnvironmentVariable("PATH", string.Concat(path, ";", Settings.GitBinDir), EnvironmentVariableTarget.Process);

            if (!string.IsNullOrEmpty(Settings.CustomHomeDir))
            {
                Environment.SetEnvironmentVariable(
                    "HOME",
                    Settings.CustomHomeDir);
                return;
            }

            if (Settings.UserProfileHomeDir)
            {
                Environment.SetEnvironmentVariable(
                    "HOME",
                    Environment.GetEnvironmentVariable("USERPROFILE"));
                return;
            }

            if (reload)
            {
                Environment.SetEnvironmentVariable(
                    "HOME",
                    UserHomeDir);
            }

            //Default!
            Environment.SetEnvironmentVariable("HOME", GetDefaultHomeDir());
            // To prevent leaking processes and suppress ANSI sequences, set TERM to "dumb".
            // Don't use "msys" because that still allows ANSI sequences.
            // See issues #1092 and #1313.
            Environment.SetEnvironmentVariable("TERM", "dumb");
        }

        public static string GetHomeDir()
        {
            return Environment.GetEnvironmentVariable("HOME") ?? "";
        }

        public static string GetDefaultHomeDir()
        {
            if (!string.IsNullOrEmpty(UserHomeDir))
                return UserHomeDir;

            if (Settings.RunningOnWindows())
            {
                return WindowsDefaultHomeDir;
            }
            return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }

        private static string WindowsDefaultHomeDir
        {
            get
            {
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HOMEDRIVE")))
                {
                    string homePath = Environment.GetEnvironmentVariable("HOMEDRIVE");
                    homePath += Environment.GetEnvironmentVariable("HOMEPATH");
                    return homePath;
                }
                return Environment.GetEnvironmentVariable("USERPROFILE");
            }
        }

        /// <summary>Trims whitespace and replaces '\' with '/'.</summary>
        public static string FixPath(string path)
        {
            path = path.Trim();
            return path.Replace('\\', '/');
        }

        internal static ProcessStartInfo CreateProcessStartInfo([CanBeNull] Encoding outputEncoding)
        {
            if (outputEncoding == null)
                outputEncoding = GitModule.SystemEncoding;

            return new ProcessStartInfo
                       {
                           UseShellExecute = false,
                           ErrorDialog = false,
                           RedirectStandardOutput = true,
                           RedirectStandardInput = true,
                           RedirectStandardError = true,
                           StandardOutputEncoding = outputEncoding,
                           StandardErrorEncoding = outputEncoding
                       };
        }

        internal static bool UseSsh(string arguments)
        {
            var x = !Plink() && GetArgumentsRequiresSsh(arguments);
            return x || arguments.Contains("plink");
        }

        private static bool GetArgumentsRequiresSsh(string arguments)
        {
            return (arguments.Contains("@") && arguments.Contains("://")) ||
                   (arguments.Contains("@") && arguments.Contains(":")) ||
                   (arguments.Contains("ssh://")) ||
                   (arguments.Contains("http://")) ||
                   (arguments.Contains("git://")) ||
                   (arguments.Contains("push")) ||
                   (arguments.Contains("remote")) ||
                   (arguments.Contains("fetch")) ||
                   (arguments.Contains("pull"));
        }

        internal static int CreateAndStartProcess(string arguments, string cmd, string workDir, out byte[] stdOutput, out byte[] stdError, byte[] stdInput)
        {
            if (string.IsNullOrEmpty(cmd))
            {
                stdOutput = stdError = null;
                return -1;
            }

            string quotedCmd = cmd;
            if (quotedCmd.IndexOf(' ') != -1)
                quotedCmd = quotedCmd.Quote();
            Settings.GitLog.Log(quotedCmd + " " + arguments);
            //process used to execute external commands

            //data is read from base stream, so encoding doesn't matter
            var startInfo = CreateProcessStartInfo(Encoding.Default);
            startInfo.CreateNoWindow = true;
            startInfo.FileName = cmd;
            startInfo.Arguments = arguments;
            startInfo.WorkingDirectory = workDir;
            startInfo.LoadUserProfile = true;

            using (var process = Process.Start(startInfo))
            {
                if (stdInput != null && stdInput.Length > 0)
                {
                    process.StandardInput.BaseStream.Write(stdInput, 0, stdInput.Length);
                    process.StandardInput.Close();
                }

                SynchronizedProcessReader.ReadBytes(process, out stdOutput, out stdError);

                process.WaitForExit();

                startInfo = null;
                return process.ExitCode;
            }
        }

        internal static int CreateAndStartProcess(string arguments, string cmd, string workDir, out string stdOutput, out string stdError, string stdInput)
        {
            if (string.IsNullOrEmpty(cmd))
            {
                stdOutput = stdError = "";
                return -1;
            }

            string quotedCmd = cmd;
            if (quotedCmd.IndexOf(' ') != -1)
                quotedCmd = quotedCmd.Quote();
            Settings.GitLog.Log(quotedCmd + " " + arguments);
            //process used to execute external commands

            var startInfo = CreateProcessStartInfo(null);
            startInfo.CreateNoWindow = true;
            startInfo.FileName = cmd;
            startInfo.Arguments = arguments;
            startInfo.WorkingDirectory = workDir;
            startInfo.LoadUserProfile = true;

            using (var process = Process.Start(startInfo))
            {
                if (!string.IsNullOrEmpty(stdInput))
                {
                    process.StandardInput.Write(stdInput);
                    process.StandardInput.Close();
                }

                SynchronizedProcessReader.Read(process, out stdOutput, out stdError);

                process.WaitForExit();
                return process.ExitCode;
            }
        }

        internal static IEnumerable<string> CreateAndStartProcessAsync(string arguments, string cmd, string workDir, string stdInput, Encoding encoding)
        {
            if (string.IsNullOrEmpty(cmd))
                yield break;

            string quotedCmd = cmd;
            if (quotedCmd.IndexOf(' ') != -1)
                quotedCmd = quotedCmd.Quote();
            Settings.GitLog.Log(quotedCmd + " " + arguments);

            //process used to execute external commands
            using (var process = new Process { StartInfo = CreateProcessStartInfo(null) })
            {
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.FileName = cmd;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.WorkingDirectory = workDir;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                process.StartInfo.LoadUserProfile = true;
                process.StartInfo.StandardOutputEncoding = encoding;
                process.StartInfo.StandardErrorEncoding = encoding;
                process.Start();

                string line;
                do
                {
                    line = process.StandardOutput.ReadLine();
                    if (line != null)
                        yield return line;
                } while (line != null);

                do
                {
                    line = process.StandardError.ReadLine();
                    if (line != null)
                        yield return line;
                } while (line != null);

                process.WaitForExit();
            }
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public static string RunCmd(string cmd, string arguments)
        {
            try
            {
                SetEnvironmentVariable();

                arguments = arguments.Replace("$QUOTE$", "\\\"");

                string output, error;
                CreateAndStartProcess(arguments, cmd, "", out output, out error, null);

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

        private static GitVersion _versionInUse;
        private static readonly string UserHomeDir = Environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.User);

        public static GitVersion VersionInUse
        {
            get
            {
                if (_versionInUse == null || _versionInUse.IsUnknown)
                {
                    var result = RunCmd(Settings.GitCommand, "--version");
                    _versionInUse = new GitVersion(result);
                }

                return _versionInUse;
            }
        }

        public static string CherryPickCmd(string cherry, bool commit, string arguments)
        {
            string CherryPickCmd = commit ? "cherry-pick" : "cherry-pick --no-commit";
            return CherryPickCmd + " " + arguments + " \"" + cherry + "\"";
        }

        public static string GetFullBranchName(string branch)
        {
            if (branch.StartsWith("refs/"))
                return branch;
            return "refs/heads/" + branch;
        }

        public static string DeleteBranchCmd(string branchName, bool force, bool remoteBranch)
        {
            StringBuilder cmd = new StringBuilder("branch");
            cmd.Append(force ? " -D" : " -d");

            if (remoteBranch)
                cmd.Append(" -r");

            cmd.Append(" \"");
            cmd.Append(branchName);
            cmd.Append("\"");

            return cmd.ToString();
        }

        public static string DeleteTagCmd(string tagName)
        {
            return "tag -d \"" + tagName + "\"";
        }

        public static string SubmoduleUpdateCmd(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "submodule update --init --recursive";

            return "submodule update --init --recursive \"" + name.Trim() + "\"";
        }

        public static string SubmoduleSyncCmd(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "submodule sync";

            return "submodule sync \"" + name.Trim() + "\"";
        }

        public static string AddSubmoduleCmd(string remotePath, string localPath, string branch, bool force)
        {
            remotePath = FixPath(remotePath);
            localPath = FixPath(localPath);

            if (!string.IsNullOrEmpty(branch))
                branch = " -b \"" + branch.Trim() + "\"";

            var forceCmd = force ? " -f" : string.Empty;

            return "submodule add" + forceCmd + branch + " \"" + remotePath.Trim() + "\" \"" + localPath.Trim() + "\"";
        }

        public static GitSubmodule CreateGitSubmodule(GitModule aModule, string submodule)
        {
            var gitSubmodule =
                new GitSubmodule(aModule)
                    {
                        Initialized = submodule[0] != '-',
                        UpToDate = submodule[0] != '+',
                        CurrentCommitGuid = submodule.Substring(1, 40).Trim()
                    };

            var name = submodule.Substring(42).Trim();
            if (name.Contains("("))
            {
                gitSubmodule.Name = name.Substring(0, name.IndexOf("("));
                gitSubmodule.Branch = name.Substring(name.IndexOf("(")).Trim(new[] { '(', ')', ' ' });
            }
            else
                gitSubmodule.Name = name;
            return gitSubmodule;
        }

        public static string RevertCmd(string commit, bool autoCommit, int parentIndex)
        {
            var cmd = new StringBuilder("revert ");
            if (!autoCommit)
            {
                cmd.Append("--no-commit ");
            }

            if (parentIndex > 0)
            {
                cmd.AppendFormat("-m {0} ", parentIndex);
            }

            cmd.Append(commit);

            return cmd.ToString();
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

        public static string CloneCmd(string fromPath, string toPath)
        {
            return CloneCmd(fromPath, toPath, false, false, string.Empty, null);
        }

        public static string CloneCmd(string fromPath, string toPath, bool central, bool initSubmodules, string branch, int? depth)
        {
            var from = FixPath(fromPath);
            var to = FixPath(toPath);
            var options = new List<string> { "-v" };
            if (central)
                options.Add("--bare");
            if (initSubmodules)
                options.Add("--recurse-submodules");
            if (depth.HasValue)
                options.Add("--depth " + depth);
            if (VersionInUse.CloneCanAskForProgress)
                options.Add("--progress");
            if (!string.IsNullOrEmpty(branch))
                options.Add("--branch " + branch);
            options.Add(string.Format("\"{0}\"", from.Trim()));
            options.Add(string.Format("\"{0}\"", to.Trim()));

            return "clone " + string.Join(" ", options.ToArray());
        }

        public static string CheckoutCmd(string branchOrRevisionName, LocalChangesAction changesAction)
        {
            string args = "";
            switch (changesAction)
            {
                case LocalChangesAction.Merge:
                    args = " --merge";
                    break;
                case LocalChangesAction.Reset:
                    args = " --force";
                    break;
            }
            return string.Format("checkout{0} \"{1}\"", args, branchOrRevisionName);
        }

        /// <summary>Create a new orphan branch from <paramref name="startPoint"/> and switch to it.</summary>
        public static string CreateOrphanCmd(string newBranchName, string startPoint = null)
        {
            return string.Format("checkout --orphan {0} {1}", newBranchName, startPoint);
        }

        /// <summary>Remove files from the working tree and from the index. <remarks>git rm</remarks></summary>
        /// <param name="force">Override the up-to-date check.</param>
        /// <param name="isRecursive">Allow recursive removal when a leading directory name is given.</param>
        /// <param name="files">Files to remove. Fileglobs can be given to remove matching files.</param>
        public static string RemoveCmd(bool force = true, bool isRecursive = true, params string[] files)
        {
            string file = files.Any()
                              ? string.Join(" ", files)
                              : ".";

            return string.Format("rm {0} {1} {2}",
                force ? "--force" : string.Empty,
                isRecursive ? "-r" : string.Empty,
                file
            );
        }

        public static string BranchCmd(string branchName, string revision, bool checkout)
        {
            return string.Format(
                checkout
                ? "checkout -b \"{0}\" \"{1}\""
                : "branch \"{0}\" \"{1}\"", branchName.Trim(), revision);
        }

        public static string MergedBranches()
        {
            return "branch --merged";
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
                Environment.SetEnvironmentVariable("GIT_SSH", path, EnvironmentVariableTarget.Process);
        }

        /// <summary>Indicates whether the git SSH command uses Plink.</summary>
        public static bool Plink()
        {
            var sshString = GetSsh();

            return sshString.EndsWith("plink.exe", StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>Gets the git SSH command; or "" if the environment variable is NOT set.</summary>
        public static string GetSsh()
        {
            return Environment.GetEnvironmentVariable("GIT_SSH", EnvironmentVariableTarget.Process)
                ?? "";
        }

        /// <summary>Creates a 'git push' command using the specified parameters, pushing from HEAD.</summary>
        /// <param name="remote">Remote repository that is the destination of the push operation.</param>
        /// <param name="toBranch">Name of the ref on the remote side to update with the push.</param>
        /// <param name="all">All refs under 'refs/heads/' will be pushed.</param>
        /// <returns>'git push' command with the specified parameters.</returns>
        public static string PushCmd(string remote, string toBranch, bool all)
        {
            return PushCmd(remote, null, toBranch, all, false, true, 0);
        }

        /// <summary>Creates a 'git push' command using the specified parameters.</summary>
        /// <param name="remote">Remote repository that is the destination of the push operation.</param>
        /// <param name="fromBranch">Name of the branch to push.</param>
        /// <param name="toBranch">Name of the ref on the remote side to update with the push.</param>
        /// <param name="force">If a remote ref is not an ancestor of the local ref, overwrite it. 
        /// <remarks>This can cause the remote repository to lose commits; use it with care.</remarks></param>
        /// <returns>'git push' command with the specified parameters.</returns>
        public static string PushCmd(string remote, string fromBranch, string toBranch, bool force = false)
        {
            return PushCmd(remote, fromBranch, toBranch, false, force, false, 0);
        }

        /// <summary>Creates a 'git push' command using the specified parameters.</summary>
        /// <param name="remote">Remote repository that is the destination of the push operation.</param>
        /// <param name="fromBranch">Name of the branch to push.</param>
        /// <param name="toBranch">Name of the ref on the remote side to update with the push.</param>
        /// <param name="all">All refs under 'refs/heads/' will be pushed.</param>
        /// <param name="force">If a remote ref is not an ancestor of the local ref, overwrite it. 
        /// <remarks>This can cause the remote repository to lose commits; use it with care.</remarks></param>
        /// <param name="track">For every branch that is up to date or successfully pushed, add upstream (tracking) reference.</param>
        /// <param name="recursiveSubmodules">If '1', check whether all submodule commits used by the revisions to be pushed are available on a remote tracking branch; otherwise, the push will be aborted.</param>
        /// <returns>'git push' command with the specified parameters.</returns>
        public static string PushCmd(string remote, string fromBranch, string toBranch,
            bool all, bool force, bool track, int recursiveSubmodules)
        {
            remote = FixPath(remote);

            // This method is for pushing to remote branches, so fully qualify the
            // remote branch name with refs/heads/.
            fromBranch = GetFullBranchName(fromBranch);
            toBranch = GetFullBranchName(toBranch);

            if (string.IsNullOrEmpty(fromBranch) && !string.IsNullOrEmpty(toBranch))
                fromBranch = "HEAD";

            if (toBranch != null) toBranch = toBranch.Replace(" ", "");

            var sforce = "";
            if (force)
                sforce = "-f ";

            var strack = "";
            if (track)
                strack = "-u ";

            var srecursiveSubmodules = "";
            if (recursiveSubmodules == 1)
                srecursiveSubmodules = "--recurse-submodules=check ";
            if (recursiveSubmodules == 2)
                srecursiveSubmodules = "--recurse-submodules=on-demand ";

            var sprogressOption = "";
            if (VersionInUse.PushCanAskForProgress)
                sprogressOption = "--progress ";

            var options = String.Concat(sforce, strack, srecursiveSubmodules, sprogressOption);
            if (all)
                return string.Format("push {0}--all \"{1}\"", options, remote);

            if (!string.IsNullOrEmpty(toBranch) && !string.IsNullOrEmpty(fromBranch))
                return string.Format("push {0}\"{1}\" {2}:{3}", options, remote, fromBranch, toBranch);

            return string.Format("push {0}\"{1}\" {2}", options, remote, fromBranch);
        }

        /// <summary>Pushes multiple sets of local branches to remote branches.</summary>
        public static string PushMultipleCmd(string remote, IEnumerable<GitPushAction> pushActions)
        {
            remote = FixPath(remote);
            return new GitPush(remote, pushActions)
            {
                ReportProgress = VersionInUse.PushCanAskForProgress
            }.ToString();
        }

        public static string PushTagCmd(string path, string tag, bool all)
        {
            return PushTagCmd(path, tag, all, false);
        }

        public static string PushTagCmd(string path, string tag, bool all, bool force)
        {
            path = FixPath(path);

            tag = tag.Replace(" ", "");

            var sforce = "";
            if (force)
                sforce = "-f ";

            var sprogressOption = "";
            if (VersionInUse.PushCanAskForProgress)
                sprogressOption = "--progress ";

            var options = String.Concat(sforce, sprogressOption);

            if (all)
                return "push " + options + "\"" + path.Trim() + "\" --tags";
            if (!string.IsNullOrEmpty(tag))
                return "push " + options + "\"" + path.Trim() + "\" tag " + tag;

            return "";
        }

        public static string StashSaveCmd(bool untracked)
        {
            var cmd = "stash save";
            if (untracked && VersionInUse.StashUntrackedFilesSupported)
                cmd += " -u";
            return cmd;
        }

        public static bool PathIsUrl(string path)
        {
            return path.Contains(Settings.PathSeparator.ToString()) || path.Contains(Settings.PathSeparatorWrong.ToString());
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
            var bisectCommand = GetBisectCommand(bisectOption);
            return (revisions.Length == 0)
                ? bisectCommand
                : string.Format("{0} {1}", bisectCommand, string.Join(" ", revisions));
        }

        private static string GetBisectCommand(GitBisectOption bisectOption)
        {
            switch (bisectOption)
            {
                case GitBisectOption.Good:
                    return "bisect good";
                case GitBisectOption.Bad:
                    return "bisect bad";
                case GitBisectOption.Skip:
                    return "bisect skip";
                default:
                    throw new NotSupportedException(string.Format("Bisect option {0} is not supported", bisectOption));
            }
        }

        public static string StopBisectCmd()
        {
            return "bisect reset";
        }

        public static string RebaseCmd(string branch, bool interactive, bool preserveMerges, bool autosquash)
        {
            StringBuilder sb = new StringBuilder("rebase ");

            if (interactive)
            {
                sb.Append(" -i ");
                sb.Append(autosquash ? "--autosquash " : "--no-autosquash ");
            }

            if (preserveMerges)
            {
                sb.Append("--preserve-merges ");
            }


            sb.Append('"');
            sb.Append(branch);
            sb.Append('"');


            return sb.ToString();
        }


        public static string RebaseRangeCmd(string from, string branch, string onto, bool interactive, bool preserveMerges, bool autosquash)
        {
            StringBuilder sb = new StringBuilder("rebase ");

            if (interactive)
            {
                sb.Append(" -i ");
                sb.Append(autosquash ? "--autosquash " : "--no-autosquash ");
            }

            if (preserveMerges)
            {
                sb.Append("--preserve-merges ");
            }

            sb.Append('"')
              .Append(from)
              .Append("\" ");


            sb.Append('"')
              .Append(branch)
              .Append("\"");


            sb.Append(" --onto ")
              .Append(onto);

            return sb.ToString();
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

        public static string PatchCmd(string patchFile)
        {
            return IsDiffFile(patchFile)
                       ? "apply \"" + FixPath(patchFile) + "\""
                       : "am --3way --signoff \"" + FixPath(patchFile) + "\"";
        }

        public static string PatchCmdIgnoreWhitespace(string patchFile)
        {
            return IsDiffFile(patchFile)
                       ? "apply --ignore-whitespace \"" + FixPath(patchFile) + "\""
                       : "am --3way --signoff --ignore-whitespace \"" + FixPath(patchFile) + "\"";
        }

        public static string PatchDirCmd()
        {
            return "am --3way --signoff";
        }

        public static string PatchDirCmdIgnoreWhitespace()
        {
            return PatchDirCmd() + " --ignore-whitespace";
        }

        public static string CleanUpCmd(bool dryrun, bool directories, bool nonignored, bool ignored)
        {
            var stringBuilder = new StringBuilder("clean");

            if (directories)
                stringBuilder.Append(" -d");
            if (!nonignored && !ignored)
                stringBuilder.Append(" -x");
            if (ignored)
                stringBuilder.Append(" -X");
            if (dryrun)
                stringBuilder.Append(" --dry-run");
            if (!dryrun)
                stringBuilder.Append(" -f");

            return stringBuilder.ToString();
        }

        public static ConfigFile GetGlobalConfig()
        {
            string configPath = Path.Combine(GetHomeDir(), ".config", "git", "config");
            return File.Exists(configPath)
                ? new ConfigFile(configPath, false)
                : new ConfigFile(Path.Combine(GetHomeDir(), ".gitconfig"), false);
        }

        public static string GetAllChangedFilesCmd(bool excludeIgnoredFiles, bool untrackedFiles)
        {
            return GetAllChangedFilesCmd(excludeIgnoredFiles, untrackedFiles ? UntrackedFilesMode.Default : UntrackedFilesMode.No);
        }

        public static string GetAllChangedFilesCmd(bool excludeIgnoredFiles, UntrackedFilesMode untrackedFiles)
        {
            return GetAllChangedFilesCmd(excludeIgnoredFiles, untrackedFiles, 0);
        }

        public static string GetAllChangedFilesCmd(bool excludeIgnoredFiles, UntrackedFilesMode untrackedFiles, IgnoreSubmodulesMode ignoreSubmodules)
        {
            if (!VersionInUse.SupportGitStatusPorcelain)
                throw new Exception("The version of git you are using is not supported for this action. Please upgrade to git 1.7.3 or newer.");

            StringBuilder stringBuilder = new StringBuilder("status --porcelain -z");

            switch (untrackedFiles)
            {
                case UntrackedFilesMode.Default:
                    stringBuilder.Append(" --untracked-files");
                    break;
                case UntrackedFilesMode.No:
                    stringBuilder.Append(" --untracked-files=no");
                    break;
                case UntrackedFilesMode.Normal:
                    stringBuilder.Append(" --untracked-files=normal");
                    break;
                case UntrackedFilesMode.All:
                    stringBuilder.Append(" --untracked-files=all");
                    break;
            }
            switch (ignoreSubmodules)
            {
                case IgnoreSubmodulesMode.Default:
                    stringBuilder.Append(" --ignore-submodules");
                    break;
                case IgnoreSubmodulesMode.None:
                    stringBuilder.Append(" --ignore-submodules=none");
                    break;
                case IgnoreSubmodulesMode.Untracked:
                    stringBuilder.Append(" --ignore-submodules=untracked");
                    break;
                case IgnoreSubmodulesMode.Dirty:
                    stringBuilder.Append(" --ignore-submodules=dirty");
                    break;
                case IgnoreSubmodulesMode.All:
                    stringBuilder.Append(" --ignore-submodules=all");
                    break;
            }
            if (!excludeIgnoredFiles)
                stringBuilder.Append(" --ignored");

            return stringBuilder.ToString();
        }

        public static GitSubmoduleStatus GetCurrentSubmoduleChanges(GitModule module, string fileName, string oldFileName, bool staged)
        {
            PatchApply.Patch patch = module.GetCurrentChanges(fileName, oldFileName, staged, "", module.FilesEncoding);
            string text = patch != null ? patch.Text : "";
            return GetSubmoduleStatus(text);
        }

        public static GitSubmoduleStatus GetCurrentSubmoduleChanges(GitModule module, string submodule)
        {
            return GetCurrentSubmoduleChanges(module, submodule, submodule, false);
        }

        public static GitSubmoduleStatus GetSubmoduleStatus(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            var status = new GitSubmoduleStatus();
            using (StringReader reader = new StringReader(text))
            {
                string line = reader.ReadLine();

                if (line != null)
                {
                    var match = Regex.Match(line, @"diff --git a/(\S+) b/(\S+)");
                    if (match != null && match.Groups.Count > 0)
                    {
                        status.Name = match.Groups[1].Value;
                        status.OldName = match.Groups[2].Value;
                    }
                }

                while ((line = reader.ReadLine()) != null)
                {
                    if (!line.Contains("Subproject"))
                        continue;

                    char c = line[0];
                    const string commit = "commit ";
                    string hash = "";
                    int pos = line.IndexOf(commit);
                    if (pos >= 0)
                        hash = line.Substring(pos + commit.Length);
                    bool bdirty = hash.EndsWith("-dirty");
                    hash = hash.Replace("-dirty", "");
                    if (c == '-')
                    {
                        status.OldCommit = hash;
                    }
                    else if (c == '+')
                    {
                        status.Commit = hash;
                        status.IsDirty = bdirty;
                    }
                }
            }
            return status;
        }

        public static List<GitItemStatus> GetAllChangedFilesFromString(GitModule module, string statusString)
        {
            return GetAllChangedFilesFromString(module, statusString, false);
        }

        /*
               source: C:\Program Files\msysgit\doc\git\html\git-status.html
        */
        public static List<GitItemStatus> GetAllChangedFilesFromString(GitModule module, string statusString, bool fromDiff /*old name and new name are switched.. %^&#^% */)
        {
            var diffFiles = new List<GitItemStatus>();

            if (string.IsNullOrEmpty(statusString))
                return diffFiles;

            /*The status string can show warnings. This is a text block at the start or at the beginning
              of the file status. Strip it. Example:
                warning: LF will be replaced by CRLF in CustomDictionary.xml.
                The file will have its original line endings in your working directory.
                warning: LF will be replaced by CRLF in FxCop.targets.
                The file will have its original line endings in your working directory.*/
            var nl = new char[] { '\n', '\r' };
            string trimmedStatus = statusString.Trim(nl);
            int lastNewLinePos = trimmedStatus.LastIndexOfAny(nl);
            if (lastNewLinePos > 0)
            {
                int ind = trimmedStatus.LastIndexOf('\0');
                if (ind < lastNewLinePos) //Warning at end
                {
                    lastNewLinePos = trimmedStatus.IndexOfAny(nl, ind >= 0 ? ind : 0);
                    trimmedStatus = trimmedStatus.Substring(0, lastNewLinePos).Trim(nl);
                }
                else                                              //Warning at beginning
                    trimmedStatus = trimmedStatus.Substring(lastNewLinePos).Trim(nl);
            }

            // Doesn't work with removed submodules
            IList<string> Submodules = module.GetSubmodulesLocalPathes();

            //Split all files on '\0' (WE NEED ALL COMMANDS TO BE RUN WITH -z! THIS IS ALSO IMPORTANT FOR ENCODING ISSUES!)
            var files = trimmedStatus.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            for (int n = 0; n < files.Length; n++)
            {
                if (string.IsNullOrEmpty(files[n]))
                    continue;

                int splitIndex = files[n].IndexOfAny(new char[] { '\0', '\t', ' ' }, 1);

                string status = string.Empty;
                string fileName = string.Empty;

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

                if (x != '?' && x != '!' && x != ' ')
                {
                    GitItemStatus gitItemStatusX;
                    if (x == 'R' || x == 'C') // Find renamed files...
                    {
                        string nextfile = n + 1 < files.Length ? files[n + 1] : "";
                        gitItemStatusX = GitItemStatusFromCopyRename(fromDiff, nextfile, fileName, x, status);
                        n++;
                    }
                    else
                        gitItemStatusX = GitItemStatusFromStatusCharacter(fileName, x);

                    gitItemStatusX.IsStaged = true;
                    if (Submodules.Contains(gitItemStatusX.Name))
                        gitItemStatusX.IsSubmodule = true;
                    diffFiles.Add(gitItemStatusX);
                }

                if (fromDiff || y == ' ')
                    continue;
                GitItemStatus gitItemStatusY;
                if (y == 'R' || y == 'C') // Find renamed files...
                {
                    string nextfile = n + 1 < files.Length ? files[n + 1] : "";
                    gitItemStatusY = GitItemStatusFromCopyRename(false, nextfile, fileName, y, status);
                    n++;
                }
                else
                    gitItemStatusY = GitItemStatusFromStatusCharacter(fileName, y);
                gitItemStatusY.IsStaged = false;
                if (Submodules.Contains(gitItemStatusY.Name))
                    gitItemStatusY.IsSubmodule = true;
                diffFiles.Add(gitItemStatusY);
            }

            return diffFiles;
        }

        private static GitItemStatus GitItemStatusFromCopyRename(bool fromDiff, string nextfile, string fileName, char x, string status)
        {
            var gitItemStatus = new GitItemStatus();
            //Find renamed files...
            if (fromDiff)
            {
                gitItemStatus.OldName = fileName.Trim();
                gitItemStatus.Name = nextfile.Trim();
            }
            else
            {
                gitItemStatus.Name = fileName.Trim();
                gitItemStatus.OldName = nextfile.Trim();
            }
            gitItemStatus.IsNew = false;
            gitItemStatus.IsChanged = false;
            gitItemStatus.IsDeleted = false;
            if (x == 'R')
                gitItemStatus.IsRenamed = true;
            else
                gitItemStatus.IsCopied = true;
            gitItemStatus.IsTracked = true;
            if (status.Length > 2)
                gitItemStatus.RenameCopyPercentage = status.Substring(1);
            return gitItemStatus;
        }

        private static GitItemStatus GitItemStatusFromStatusCharacter(string fileName, char x)
        {
            var gitItemStatus = new GitItemStatus();
            gitItemStatus.Name = fileName.Trim();
            gitItemStatus.IsNew = x == 'A' || x == '?' || x == '!';
            gitItemStatus.IsChanged = x == 'M';
            gitItemStatus.IsDeleted = x == 'D';
            gitItemStatus.IsRenamed = false;
            gitItemStatus.IsTracked = x != '?' && x != '!' && x != ' ' || !gitItemStatus.IsNew;
            gitItemStatus.IsConflict = x == 'U';
            return gitItemStatus;
        }

        public static string GetSubmoduleText(GitModule superproject, string name, string hash)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Submodule " + name);
            sb.AppendLine();
            GitModule module = superproject.GetSubmodule(name);
            if (module.IsValidGitWorkingDir())
            {
                string error = "";
                CommitData data = CommitData.GetCommitData(module, hash, ref error);
                if (data == null)
                {
                    sb.AppendLine("Commit hash:\t" + hash);
                    return sb.ToString();
                }

                string header = data.GetHeaderPlain();
                string body = "\n" + data.Body.Trim();
                sb.AppendLine(header);
                sb.Append(body);
            }
            else
                sb.AppendLine("Commit hash:\t" + hash);
            return sb.ToString();
        }

        public static string ProcessSubmodulePatch(GitModule module, PatchApply.Patch patch)
        {
            string text = patch != null ? patch.Text : null;
            var status = GetSubmoduleStatus(text);
            return ProcessSubmoduleStatus(module, status);
        }

        public static string ProcessSubmoduleStatus([NotNull] GitModule module, [NotNull] GitSubmoduleStatus status)
        {
            if (module == null)
                throw new ArgumentNullException("module");
            if (status == null)
                throw new ArgumentNullException("status");
            GitModule gitmodule = module.GetSubmodule(status.Name);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Submodule " + status.Name + " Change");

            sb.AppendLine();
            sb.AppendLine("From:\t" + (status.OldCommit ?? "null"));
            CommitData oldCommitData = null;
            if (gitmodule.IsValidGitWorkingDir())
            {
                string error = "";
                if (status.OldCommit != null)
                    oldCommitData = CommitData.GetCommitData(gitmodule, status.OldCommit, ref error);
                if (oldCommitData != null)
                {
                    sb.AppendLine("\t\t\t\t\t" + GetRelativeDateString(DateTime.UtcNow, oldCommitData.CommitDate.UtcDateTime) + oldCommitData.CommitDate.LocalDateTime.ToString(" (ddd MMM dd HH':'mm':'ss yyyy)"));
                    var delim = new char[] { '\n', '\r' };
                    var lines = oldCommitData.Body.Trim(delim).Split(new string[] { "\r\n" }, 0);
                    foreach (var curline in lines)
                        sb.AppendLine("\t\t" + curline);
                }
            }
            else
                sb.AppendLine();

            sb.AppendLine();
            string dirty = !status.IsDirty ? "" : " (dirty)";
            sb.AppendLine("To:\t\t" + (status.Commit ?? "null") + dirty);
            CommitData commitData = null;
            if (gitmodule.IsValidGitWorkingDir())
            {
                string error = "";
                if (status.Commit != null)
                    commitData = CommitData.GetCommitData(gitmodule, status.Commit, ref error);
                if (commitData != null)
                {
                    sb.AppendLine("\t\t\t\t\t" + GetRelativeDateString(DateTime.UtcNow, commitData.CommitDate.UtcDateTime) + commitData.CommitDate.LocalDateTime.ToString(" (ddd MMM dd HH':'mm':'ss yyyy)"));
                    var delim = new char[] { '\n', '\r' };
                    var lines = commitData.Body.Trim(delim).Split(new string[] { "\r\n" }, 0);
                    foreach (var curline in lines)
                        sb.AppendLine("\t\t" + curline);
                }
            }
            else
                sb.AppendLine();

            sb.AppendLine();
            var submoduleStatus = gitmodule.CheckSubmoduleStatus(status.Commit, status.OldCommit, commitData, oldCommitData);
            sb.Append("Type: ");
            switch (submoduleStatus)
            {
                case SubmoduleStatus.NewSubmodule:
                    sb.AppendLine("New submodule");
                    break;
                case SubmoduleStatus.FastForward:
                    sb.AppendLine("Fast Forward");
                    break;
                case SubmoduleStatus.Rewind:
                    sb.AppendLine("Rewind");
                    break;
                case SubmoduleStatus.NewerTime:
                    sb.AppendLine("Newer commit time");
                    break;
                case SubmoduleStatus.OlderTime:
                    sb.AppendLine("Older commit time");
                    break;
                case SubmoduleStatus.SameTime:
                    sb.AppendLine("Same commit time");
                    break;
                default:
                    sb.AppendLine("Unknown");
                    break;
            }

            if (status.Commit != null && status.OldCommit != null)
            {
                if (status.IsDirty)
                {
                    string statusText = gitmodule.GetStatusText(false);
                    if (!String.IsNullOrEmpty(statusText))
                    {
                        sb.AppendLine("\nStatus:");
                        sb.Append(statusText);
                    }
                }

                string diffs = gitmodule.GetDiffFilesText(status.OldCommit, status.Commit);
                if (!String.IsNullOrEmpty(diffs))
                {
                    sb.AppendLine("\nDifferences:");
                    sb.Append(diffs);
                }
            }

            return sb.ToString();
        }

        public static string GetRemoteName(string completeName, IEnumerable<string> remotes)
        {
            string trimmedName = completeName.StartsWith("refs/remotes/") ? completeName.Substring(13) : completeName;

            foreach (string remote in remotes)
            {
                if (trimmedName.StartsWith(string.Concat(remote, "/")))
                    return remote;
            }

            return string.Empty;
        }

        public static string MergeBranchCmd(string branch, bool allowFastForward, bool squash, bool noCommit, string strategy)
        {
            StringBuilder command = new StringBuilder("merge");

            if (!allowFastForward)
                command.Append(" --no-ff");
            if (!string.IsNullOrEmpty(strategy))
            {
                command.Append(" --strategy=");
                command.Append(strategy);
            }
            if (squash)
                command.Append(" --squash");
            if (noCommit)
                command.Append(" --no-commit");

            command.Append(" ");
            command.Append(branch);
            return command.ToString();
        }

        public static string GetFileExtension(string fileName)
        {
            if (fileName.Contains(".") && fileName.LastIndexOf(".") < fileName.Length)
                return fileName.Substring(fileName.LastIndexOf('.') + 1);

            return null;
        }

        private static DateTime RoundDateTime(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
        }

        /// <summary>
        /// Takes a date/time which and determines a friendly string for time from now to be displayed for the relative time from the date.
        /// It is important to note that times are compared using the current timezone, so the date that is passed in should be converted
        /// to the local timezone before passing it in.
        /// </summary>
        /// <param name="originDate">Current date.</param>
        /// <param name="previousDate">The date to get relative time string for.</param>
        /// <param name="displayWeeks">Indicates whether to display weeks.</param>
        /// <returns>The human readable string for relative date.</returns>
        /// <see cref="http://stackoverflow.com/questions/11/how-do-i-calculate-relative-time"/>
        public static string GetRelativeDateString(DateTime originDate, DateTime previousDate, bool displayWeeks)
        {
            var ts = new TimeSpan(RoundDateTime(originDate).Ticks - RoundDateTime(previousDate).Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 60)
            {
                return Strings.GetNSecondsAgoText(ts.Seconds);
            }
            if (delta < 45 * 60)
            {
                return Strings.GetNMinutesAgoText(ts.Minutes);
            }
            if (delta < 24 * 60 * 60)
            {
                int hours = delta < 60 * 60 ? Math.Sign(ts.Minutes) * 1 : ts.Hours;
                return Strings.GetNHoursAgoText(hours);
            }
            // 30.417 = 365 days / 12 months - note that the if statement only bothers with 30 days for "1 month ago" because ts.Days is int
            if (delta < (displayWeeks ? 7 : 30) * 24 * 60 * 60)
            {
                return Strings.GetNDaysAgoText(ts.Days);
            }
            if (displayWeeks && delta < 30 * 24 * 60 * 60)
            {
                int weeks = Convert.ToInt32(ts.Days / 7.0);
                return Strings.GetNWeeksAgoText(weeks);
            }
            if (delta < 365 * 24 * 60 * 60)
            {
                int months = Convert.ToInt32(ts.Days / 30.0);
                return Strings.GetNMonthsAgoText(months);
            }
            int years = Convert.ToInt32(ts.Days / 365.0);
            return Strings.GetNYearsAgoText(years);
        }

        public static string GetRelativeDateString(DateTime originDate, DateTime previousDate)
        {
            return GetRelativeDateString(originDate, previousDate, true);
        }

        // look into patch file and try to figure out if it's a raw diff (i.e from git diff -p)
        // only looks at start, as all we want is to tell from automail format
        // returns false on any problem, never throws
        private static bool IsDiffFile(string path)
        {
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    return sr.ReadLine().StartsWith("diff ");
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

#if !MONO
        static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            public static extern bool SetConsoleCtrlHandler(IntPtr HandlerRoutine,
               bool Add);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool AttachConsole(int dwProcessId);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent,
               int dwProcessGroupId);
        }
#endif

        public static void TerminateTree(this Process process)
        {
#if !MONO
            if (Settings.RunningOnWindows())
            {
                // Send Ctrl+C
                NativeMethods.AttachConsole(process.Id);
                NativeMethods.SetConsoleCtrlHandler(IntPtr.Zero, true);
                NativeMethods.GenerateConsoleCtrlEvent(0, 0);
                if (!process.HasExited)
                    System.Threading.Thread.Sleep(500);
                if (!process.HasExited)
                    process.Kill();
            }
#else
            process.Kill();
#endif
        }
    }
}