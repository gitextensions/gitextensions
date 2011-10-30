using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Permissions;
using System.Text;
using GitCommands.Config;

namespace GitCommands
{
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
                    Environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.User));
            }

            //Default!
            Environment.SetEnvironmentVariable("HOME", GetDefaultHomeDir());
        }

        public static string GetDefaultHomeDir()
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.User)))
                return Environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.User);

            if (Settings.RunningOnWindows())
            {
                string homePath;
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HOMEDRIVE")))
                {
                    homePath = Environment.GetEnvironmentVariable("HOMEDRIVE");
                    homePath += Environment.GetEnvironmentVariable("HOMEPATH");
                }
                else
                {
                    homePath = Environment.GetEnvironmentVariable("USERPROFILE");
                }

                return homePath;
            }
            else
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            }
        }

        internal static string FixPath(string path)
        {
            path = path.Trim();
            return path.Replace('\\', '/');
        }

        internal static ProcessStartInfo CreateProcessStartInfo()
        {
            return new ProcessStartInfo
                       {
                           UseShellExecute = false,
                           ErrorDialog = false,
                           RedirectStandardOutput = true,
                           RedirectStandardInput = true,
                           RedirectStandardError = true,
                           StandardOutputEncoding = Settings.Encoding,
                           StandardErrorEncoding = Settings.Encoding
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

        internal static int CreateAndStartProcess(string arguments, string cmd, string workDir, out string stdOutput, out string stdError, string stdInput)
        {
            if (string.IsNullOrEmpty(cmd))
            {
                stdOutput = stdError = "";
                return -1;
            }

            Settings.GitLog.Log(cmd + " " + arguments);
            //process used to execute external commands

            var startInfo = CreateProcessStartInfo();
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

                stdOutput = process.StandardOutput.ReadToEnd();
                stdError = process.StandardError.ReadToEnd();
                process.WaitForExit();
                return process.ExitCode;
            }
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        private static string RunCmd(string cmd, string arguments)
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
        
        public static string DeleteBranchCmd(string branchName, bool force, bool remoteBranch)
        {
            StringBuilder cmd = new StringBuilder("branch");
            if (force)
                cmd.Append(" -D");
            else
                cmd.Append(" -d");

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

        public static string SubmoduleInitCmd(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "submodule update --init";

            return "submodule update --init \"" + name.Trim() + "\"";
        }

        public static string SubmoduleUpdateCmd(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "submodule update";

            return "submodule update \"" + name.Trim() + "\"";
        }

        public static string SubmoduleSyncCmd(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "submodule sync";

            return "submodule sync \"" + name.Trim() + "\"";
        }

        public static string AddSubmoduleCmd(string remotePath, string localPath, string branch)
        {
            remotePath = FixPath(remotePath);
            localPath = FixPath(localPath);

            if (!string.IsNullOrEmpty(branch))
                branch = " -b \"" + branch.Trim() + "\"";

            return "submodule add" + branch + " \"" + remotePath.Trim() + "\" \"" + localPath.Trim() + "\"";
        }

        public static GitSubmodule CreateGitSubmodule(string submodule)
        {
            var gitSubmodule =
                new GitSubmodule
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

        public static string RevertCmd(string commit, bool autoCommit)
        {
            if (autoCommit)
                return "revert " + commit;
            return "revert --no-commit " + commit;
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

        public static string CloneCmd(string fromPath, string toPath, bool central, string branch, int? depth)
        {
            var from = FixPath(fromPath);
            var to = FixPath(toPath);
            var options = new List<string> { "-v" };
            if (central)
                options.Add("--bare");
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

        public static string BranchCmd(string branchName, string revision, bool checkout)
        {
            if (checkout)
                return "checkout -b \"" + branchName.Trim() + "\" \"" + revision + "\"";
            return "branch \"" + branchName.Trim() + "\" \"" + revision + "\"";
        }

        public static void UnsetSsh()
        {
            Environment.SetEnvironmentVariable("GIT_SSH", "", EnvironmentVariableTarget.Process);
        }

        public static void SetSsh(string path)
        {
            if (!string.IsNullOrEmpty(path))
                Environment.SetEnvironmentVariable("GIT_SSH", path, EnvironmentVariableTarget.Process);
        }

        public static bool Plink()
        {
            var sshString = GetSsh();

            return sshString.EndsWith("plink.exe", StringComparison.CurrentCultureIgnoreCase);
        }

        public static string GetSsh()
        {
            var ssh = Environment.GetEnvironmentVariable("GIT_SSH", EnvironmentVariableTarget.Process);

            return ssh ?? "";
        }

        public static string PushCmd(string path, string branch, bool all)
        {
            return PushCmd(path, null, branch, all, false, true);
        }

        public static string PushCmd(string path, string fromBranch, string toBranch, bool all, bool force, bool track)
        {
            path = FixPath(path);

            if (string.IsNullOrEmpty(fromBranch) && !string.IsNullOrEmpty(toBranch))
                fromBranch = "HEAD";

            if (toBranch != null) toBranch = toBranch.Replace(" ", "");

            var sforce = "";
            if (force)
                sforce = "-f ";

            var strack = "";
            if (track)
                strack = "-u ";

            if (all)
                return string.Format("push {0}{1}--all \"{2}\"", sforce, strack, path.Trim());

            if (!string.IsNullOrEmpty(toBranch) && !string.IsNullOrEmpty(fromBranch))
                return string.Format("push {0}{1}\"{2}\" {3}:{4}", sforce, strack, path.Trim(), fromBranch, toBranch);

            return string.Format("push {0}{1}\"{2}\" {3}", sforce, strack, path.Trim(), fromBranch);
        }

        public static string PushMultipleCmd(string path, IEnumerable<GitPushAction> pushActions)
        {
            path = FixPath(path);

            string cmd = string.Format("push \"{0}\"", path.Trim());

            foreach (GitPushAction action in pushActions)
                cmd += " " + action.Format();

            return cmd;
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

            if (all)
                return "push " + sforce + "\"" + path.Trim() + "\" --tags";
            if (!string.IsNullOrEmpty(tag))
                return "push " + sforce + "\"" + path.Trim() + "\" tag " + tag;

            return "";
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

        public static string ContinueBisectCmd(bool good)
        {
            if (good)
                return "bisect good";
            return "bisect bad";
        }

        public static string MarkRevisionBisectCmd(bool good, string revision)
        {
            if (good)
                return "bisect good " + revision;
            return "bisect bad " + revision;
        }

        public static string StopBisectCmd()
        {
            return "bisect reset";
        }

        public static string RebaseCmd(string branch, bool interactive, bool autosquash)
        {
            StringBuilder sb = new StringBuilder("rebase ");

            if (interactive)
            {
                sb.Append(" -i ");
                sb.Append(autosquash ? "--autosquash " : "--no-autosquash ");
            }

            sb.Append('"');
            sb.Append(branch);
            sb.Append('"');
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
            return "am --3way --signoff \"" + FixPath(patchFile) + "\"";
        }

        public static string PatchCmdIgnoreWhitespace(string patchFile)
        {
            return "am --3way --signoff --ignore-whitespace \"" + FixPath(patchFile) + "\"";
        }

        public static string PatchDirCmd(string patchDir)
        {
            return "am --3way --signoff --directory=\"" + FixPath(patchDir) + "\"";
        }

        public static string PatchDirCmdIgnoreWhitespace(string patchDir)
        {
            return "am --3way --signoff --ignore-whitespace --directory=\"" + FixPath(patchDir) + "\"";
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
            return new ConfigFile(GetDefaultHomeDir() + Settings.PathSeparator + ".gitconfig");
        }
        
        public static string GetAllChangedFilesCmd(bool excludeIgnoredFiles, bool showUntrackedFiles)
        {
            if (!VersionInUse.SupportGitStatusPorcelain)
                throw new Exception("The version of git you are using is not supported for this action. Please upgrade to git 1.7.3 or newer.");

            StringBuilder stringBuilder = new StringBuilder("status --porcelain -z");

            if (!showUntrackedFiles)
                stringBuilder.Append(" --untracked-files=no");
            if (showUntrackedFiles)
                stringBuilder.Append(" --untracked-files");
            if (!excludeIgnoredFiles)
                stringBuilder.Append(" --ignored");

            return stringBuilder.ToString();
        }

        public static List<GitItemStatus> GetAllChangedFilesFromString(string statusString)
        {
            return GetAllChangedFilesFromString(statusString, false);
        }

        /*
               source: C:\Program Files\msysgit\doc\git\html\git-status.html
        */
        public static List<GitItemStatus> GetAllChangedFilesFromString(string statusString, bool fromDiff /*old name and new name are switched.. %^&#^% */)
        {
            var diffFiles = new List<GitItemStatus>();

            if (string.IsNullOrEmpty(statusString))
                return diffFiles;

            /*The status string can show warnings. This is a text blok at the start or at the beginning
              of the file status. Strip it. Example:
                warning: LF will be replaced by CRLF in CustomDictionary.xml.
                The file will have its original line endings in your working directory.
                warning: LF will be replaced by CRLF in FxCop.targets.
                The file will have its original line endings in your working directory.*/
            string trimmedStatus = statusString.Trim(new char[] { '\n', '\r' });
            int lastNewLinePos = trimmedStatus.LastIndexOfAny(new char[] { '\n', '\r' });
            if (lastNewLinePos > 0)
            {
                if (trimmedStatus.IndexOf('\0') < lastNewLinePos) //Warning at end
                    trimmedStatus = trimmedStatus.Substring(0, lastNewLinePos).Trim(new char[] { '\n', '\r' });
                else                                              //Warning at beginning
                    trimmedStatus = trimmedStatus.Substring(lastNewLinePos).Trim(new char[] { '\n', '\r' });
            }

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

                GitItemStatus gitItemStatus;

                if (x != '?' && x != '!')
                {
                    n = GitItemStatusFromStatusCharacter(fromDiff, files, n, status, fileName, x, out gitItemStatus);
                    if (gitItemStatus != null)
                    {
                        gitItemStatus.IsStaged = true;
                        diffFiles.Add(gitItemStatus);
                    }
                }

                if (!fromDiff)
                {
                    n = GitItemStatusFromStatusCharacter(fromDiff, files, n, status, fileName, y, out gitItemStatus);
                    if (gitItemStatus != null)
                    {
                        gitItemStatus.IsStaged = false;
                        diffFiles.Add(gitItemStatus);
                    }
                }
            }

            return diffFiles;
        }

        private static int GitItemStatusFromStatusCharacter(bool fromDiff, string[] files, int n, string status, string fileName, char x, out GitItemStatus gitItemStatus)
        {
            gitItemStatus = null;

            if (x == ' ')
                return n;

            gitItemStatus = new GitItemStatus();
            //Find renamed files...
            if (x == 'R')
            {
                if (fromDiff)
                {
                    gitItemStatus.OldName = fileName.Trim();
                    gitItemStatus.Name = files[n + 1].Trim();
                }
                else
                {
                    gitItemStatus.Name = fileName.Trim();
                    gitItemStatus.OldName = files[n + 1].Trim();
                }
                gitItemStatus.IsNew = false;
                gitItemStatus.IsChanged = false;
                gitItemStatus.IsDeleted = false;
                gitItemStatus.IsRenamed = true;
                gitItemStatus.IsTracked = true;
                if (status.Length > 2)
                    gitItemStatus.RenameCopyPercentage = status.Substring(1);
                n++;
            }
            else
                //Find copied files...
                if (x == 'C')
                {
                    if (fromDiff)
                    {
                        gitItemStatus.OldName = fileName.Trim();
                        gitItemStatus.Name = files[n + 1].Trim();
                    }
                    else
                    {
                        gitItemStatus.Name = fileName.Trim();
                        gitItemStatus.OldName = files[n + 1].Trim();
                    }
                    gitItemStatus.IsNew = false;
                    gitItemStatus.IsChanged = false;
                    gitItemStatus.IsDeleted = false;
                    gitItemStatus.IsCopied = true;
                    gitItemStatus.IsTracked = true;
                    if (status.Length > 2)
                        gitItemStatus.RenameCopyPercentage = status.Substring(1);
                    n++;
                }
                else
                {
                    gitItemStatus.Name = fileName.Trim();
                    gitItemStatus.IsNew = x == 'A' || x == '?' || x == '!';
                    gitItemStatus.IsChanged = x == 'M';
                    gitItemStatus.IsDeleted = x == 'D';
                    gitItemStatus.IsRenamed = false;
                    gitItemStatus.IsTracked = x != '?' && x != '!' && x != ' ' || !gitItemStatus.IsNew;
                    gitItemStatus.IsConflict = x == 'U';
                }
            return n;
        }
        
        public static string StageFiles(IList<GitItemStatus> files)
        {
            var gitCommand = new GitCommandsInstance();

            var output = "";

            Process process1 = null;
            foreach (var file in files)
            {
                if (file.IsDeleted)
                    continue;
                if (process1 == null)
                    process1 = gitCommand.CmdStartProcess(Settings.GitCommand, "update-index --add --stdin");

                process1.StandardInput.WriteLine("\"" + FixPath(file.Name) + "\"");
            }
            if (process1 != null)
            {
                process1.StandardInput.Close();
                process1.WaitForExit();

                if (gitCommand.Output != null)
                    output = gitCommand.Output.ToString().Trim();
            }

            Process process2 = null;
            foreach (var file in files)
            {
                if (!file.IsDeleted)
                    continue;
                if (process2 == null)
                    process2 = gitCommand.CmdStartProcess(Settings.GitCommand, "update-index --remove --stdin");
                process2.StandardInput.WriteLine("\"" + FixPath(file.Name) + "\"");
            }
            if (process2 != null)
            {
                process2.StandardInput.Close();
                process2.WaitForExit();

                if (gitCommand.Output != null)
                {
                    if (!string.IsNullOrEmpty(output))
                    {
                        output += Environment.NewLine;
                    }
                    output += gitCommand.Output.ToString().Trim();
                }
            }

            return output;
        }

        public static string UnstageFiles(List<GitItemStatus> files)
        {
            var gitCommand = new GitCommandsInstance();

            var output = "";

            Process process1 = null;
            foreach (var file in files)
            {
                if (file.IsNew)
                    continue;
                if (process1 == null)
                    process1 = gitCommand.CmdStartProcess(Settings.GitCommand, "update-index --info-only --index-info");

                process1.StandardInput.WriteLine("0 0000000000000000000000000000000000000000\t\"" + FixPath(file.Name) +
                                                 "\"");
            }
            if (process1 != null)
            {
                process1.StandardInput.Close();
                process1.WaitForExit();
            }

            if (gitCommand.Output != null)
                output = gitCommand.Output.ToString();

            Process process2 = null;
            foreach (var file in files)
            {
                if (!file.IsNew)
                    continue;
                if (process2 == null)
                    process2 = gitCommand.CmdStartProcess(Settings.GitCommand, "update-index --force-remove --stdin");
                process2.StandardInput.WriteLine("\"" + FixPath(file.Name) + "\"");
            }
            if (process2 != null)
            {
                process2.StandardInput.Close();
                process2.WaitForExit();
            }

            if (gitCommand.Output != null)
                output += gitCommand.Output.ToString();

            return output;
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
        
        /// <summary>
        /// Takes a date/time which and determines a friendly string for time from now to be displayed for the relative time from the date.
        /// It is important to note that times are compared using the current timezone, so the date that is passed in should be converted 
        /// to the local timezone before passing it in.
        /// </summary>
        /// <param name="theDate">The date to get relative time string for.</param>
        /// <returns>The human readable string for relative date.</returns>
        /// <see cref="http://stackoverflow.com/questions/11/how-do-i-calculate-relative-time"/>
        public static string GetRelativeDateString(DateTime originDate, DateTime previousDate)
        {
            var ts = new TimeSpan(originDate.Ticks - previousDate.Ticks);
            double delta = ts.TotalSeconds;

            if (delta < 60)
            {
                return ts.Seconds == 1 ? "1 second ago" : ts.Seconds + " seconds ago";
            }
            if (delta < 120)
            {
                return "1 minute ago";
            }
            if (delta < 2700) // 45 * 60
            {
                return ts.Minutes + " minutes ago";
            }
            if (delta < 5400) // 90 * 60
            {
                return "1 hour ago";
            }
            if (delta < 86400) // 24 * 60 * 60
            {
                return ts.Hours + " hours ago";
            }
            if (delta < 172800) // 48 * 60 * 60
            {
                return "1 day ago";
            }
            if (delta < 604800) // 7 * 24 * 60 * 60
            {
                return ts.Days + " days ago";
            }
            if (delta < 2592000) // 30 * 24 * 60 * 60
            {
                int weeks = Convert.ToInt32(Math.Floor((double)ts.Days / 7));
                return weeks <= 1 ? "1 week ago" : weeks + " weeks ago";
            }
            if (delta < 31104000) // 12 * 30 * 24 * 60 * 60
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "1 month ago" : months + " months ago";
            }
            int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
            return years <= 1 ? "1 year ago" : years + " years ago";
        }
    }
}