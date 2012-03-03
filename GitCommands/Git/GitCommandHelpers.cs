using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using System.Text;
using GitCommands.Config;

namespace GitCommands
{
    public enum UntrackedFilesMode
    {
        /// <summary>
        /// Default value.
        /// </summary>
        Default = 1,
        /// <summary>
        /// Show no untracked files.
        /// </summary>
        No = 2,
        /// <summary>
        /// Shows untracked files and directories.
        /// </summary>
        Normal = 3,
        /// <summary>
        /// Also shows individual files in untracked directories.
        /// </summary>
        All = 4
    }

    public enum IgnoreSubmodulesMode
    {
        Default = 1,
        None = 2,
        Untracked = 3,
        Dirty = 4,
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
        }

        public static string GetHomeDir()
        {
            return Environment.GetEnvironmentVariable("HOME");
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

        internal static int CreateAndStartProcess(string arguments, string cmd, out byte[] stdOutput, out byte[] stdError, string stdInput)
        {
            if (string.IsNullOrEmpty(cmd))
            {
                stdOutput = stdError = null;
                return -1;
            }

            Settings.GitLog.Log(cmd + " " + arguments);
            //process used to execute external commands

            var startInfo = CreateProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.FileName = cmd;
            startInfo.Arguments = arguments;
            startInfo.WorkingDirectory = Settings.WorkingDir;
            startInfo.LoadUserProfile = true;

            using (var process = Process.Start(startInfo))
            {
                if (!string.IsNullOrEmpty(stdInput))
                {
                    process.StandardInput.Write(stdInput);
                    process.StandardInput.Close();
                }

                stdOutput = ReadByte(process.StandardOutput.BaseStream);
                stdError = ReadByte(process.StandardError.BaseStream);
                process.WaitForExit();
                return process.ExitCode;
            }
        }

        private static byte[] ReadByte(Stream stream)
        {
            if (!stream.CanRead)
            {
                return null;
            }
            int commonLen = 0;
            List<byte[]> list = new List<byte[]>();
            byte[] buffer = new byte[4096];
            int len;
            while ((len = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                byte[] newbuff = new byte[len];
                Array.Copy(buffer, newbuff, len);
                commonLen += len;
                list.Add(newbuff);
            }
            buffer = new byte[commonLen];
            commonLen = 0;
            for (int i = 0; i < list.Count; i++)
            {
                Array.Copy(list[i], 0, buffer, commonLen, list[i].Length);
                commonLen += list[i].Length;
            }
            return buffer;
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
                options.Add("--depth " + depth.ToString());
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
            return PushCmd(path, null, branch, all, false, true, false);
        }

        public static string PushCmd(string path, string fromBranch, string toBranch, 
            bool all, bool force, bool track, bool recursiveSubmodulesCheck)
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

            var srecursiveSubmodulesCheck = "";
            if (recursiveSubmodulesCheck)
                srecursiveSubmodulesCheck = "--recurse-submodules=check ";

            if (all)
                return string.Format("push {0}{1}{2}--all \"{3}\"", sforce, strack, srecursiveSubmodulesCheck, path.Trim());

            if (!string.IsNullOrEmpty(toBranch) && !string.IsNullOrEmpty(fromBranch))
                return string.Format("push {0}{1}{2}\"{3}\" {4}:{5}", sforce, strack, srecursiveSubmodulesCheck, path.Trim(), fromBranch, toBranch);

            return string.Format("push {0}{1}{2}\"{3}\" {4}", sforce, strack, srecursiveSubmodulesCheck, path.Trim(), fromBranch);
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
            return new ConfigFile(GetHomeDir() + Settings.PathSeparator.ToString() + ".gitconfig");
        }

        public static string GetAllChangedFilesCmd(bool excludeIgnoredFiles, bool untrackedFiles)
        {
            return GetAllChangedFilesCmd( excludeIgnoredFiles, untrackedFiles ? UntrackedFilesMode.Default : UntrackedFilesMode.No );
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

            switch(untrackedFiles)
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
                    lastNewLinePos = trimmedStatus.IndexOfAny(nl, ind >= 0 ? ind: 0);
                    trimmedStatus = trimmedStatus.Substring(0, lastNewLinePos).Trim(nl);
                }
                else                                              //Warning at beginning
                    trimmedStatus = trimmedStatus.Substring(lastNewLinePos).Trim(nl);
            }

            // Doesn't work with removed submodules
            IList<string> Submodules = Settings.Module.GetSubmodulesNames();

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

                GitItemStatus gitItemStatus = null;

                if (x != '?' && x != '!')
                {
                    n = GitItemStatusFromStatusCharacter(fromDiff, files, n, status, fileName, x, out gitItemStatus);
                    if (gitItemStatus != null)
                    {
                        gitItemStatus.IsStaged = true;
                        if (Submodules.Contains(gitItemStatus.Name))
                            gitItemStatus.IsSubmodule = true;
                        diffFiles.Add(gitItemStatus);
                    }
                }

                if (fromDiff)
                    continue;
                n = GitItemStatusFromStatusCharacter(false, files, n, status, fileName, y, out gitItemStatus);
                if (gitItemStatus == null)
                    continue;
                gitItemStatus.IsStaged = false;
                if (Submodules.Contains(gitItemStatus.Name))
                    gitItemStatus.IsSubmodule = true;
                diffFiles.Add(gitItemStatus);
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
            switch (x)
            {
                case 'R':
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
                    break;
                case 'C':
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
                    break;
                default:
                    gitItemStatus.Name = fileName.Trim();
                    gitItemStatus.IsNew = x == 'A' || x == '?' || x == '!';
                    gitItemStatus.IsChanged = x == 'M';
                    gitItemStatus.IsDeleted = x == 'D';
                    gitItemStatus.IsRenamed = false;
                    gitItemStatus.IsTracked = x != '?' && x != '!' && x != ' ' || !gitItemStatus.IsNew;
                    gitItemStatus.IsConflict = x == 'U';
                    break;
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

                //process1.StandardInput.WriteLine("\"" + FixPath(file.Name) + "\"");
                byte[] bytearr = EncodingHelper.ConvertTo(Settings.Encoding, "\"" + FixPath(file.Name) + "\"" + process1.StandardInput.NewLine);
                process1.StandardInput.BaseStream.Write(bytearr, 0, bytearr.Length);
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
                //process2.StandardInput.WriteLine("\"" + FixPath(file.Name) + "\"");
                byte[] bytearr = EncodingHelper.ConvertTo(Settings.Encoding, "\"" + FixPath(file.Name) + "\"" + process2.StandardInput.NewLine);
                process2.StandardInput.BaseStream.Write(bytearr, 0, bytearr.Length);
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
                //process2.StandardInput.WriteLine("\"" + FixPath(file.Name) + "\"");
                byte[] bytearr = EncodingHelper.ConvertTo(Settings.Encoding, "\"" + FixPath(file.Name) + "\"" + process2.StandardInput.NewLine);
                process2.StandardInput.BaseStream.Write(bytearr, 0, bytearr.Length);
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

        public static string ProcessSubmodulePatch(string text)
        {
            StringBuilder sb = new StringBuilder();
            using (StringReader reader = new StringReader(text))
            {
                string line;
                string module = "";
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("+++ "))
                    {
                        module = line.Substring("+++ ".Length);
                        var list = module.Split(new char[] { ' ' }, 2);
                        module = list.Length > 0 ? list[0] : "";
                        if (module.Length > 2 && module[1] == '/')
                        {
                            module = module.Substring(2);
                            break;
                        }
                    }
                }
                sb.AppendLine("Submodule " + module + " Change");
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("Subproject"))
                    {
                        sb.AppendLine();
                        char c = line[0];
                        const string commit = "commit ";
                        string hash = "";
                        int pos = line.IndexOf(commit);
                        if (pos >= 0)
                            hash = line.Substring(pos + commit.Length);
                        bool bdirty = hash.EndsWith("-dirty");
                        hash = hash.Replace("-dirty", "");
                        string dirty = !bdirty ? "" : " (dirty)";
                        if (c == '-')
                            sb.AppendLine("From:\t" + hash + dirty);
                        else if (c == '+')
                            sb.AppendLine("To:\t\t" + hash + dirty);

                        string path = Settings.Module.GetSubmoduleFullPath(module);
                        GitModule gitmodule = new GitModule(path);
                        if (gitmodule.ValidWorkingDir())
                        {
                            string error = "";
                            CommitData commitData = CommitData.GetCommitData(gitmodule, hash, ref error);
                            if (commitData != null)
                            {
                                sb.AppendLine("\t\t\t\t\t" + GitCommandHelpers.GetRelativeDateString(DateTime.UtcNow, commitData.CommitDate.UtcDateTime) + commitData.CommitDate.LocalDateTime.ToString(" (ddd MMM dd HH':'mm':'ss yyyy)"));
                                var delim = new char[] { '\n', '\r' };
                                var lines = commitData.Body.Trim(delim).Split(new string[] {"\r\n"}, 0);
                                foreach (var curline in lines)
                                    sb.AppendLine("\t\t" + curline);
                            }
                        }
                        else
                            sb.AppendLine();
                    }
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
                if (ts.Seconds == 1)
                    return String.Format(Strings.Get1SecondAgoText(), 1.ToString());
                return String.Format(Strings.GetNSecondsAgoText(), ts.Seconds.ToString());
            }
            if (delta < 120)
            {
                return String.Format(Strings.Get1MinuteAgoText(), 1.ToString());
            }
            if (delta < 2700) // 45 * 60
            {
                return String.Format(Strings.GetNMinutesAgoText(), ts.Minutes.ToString());
            }
            if (delta < 5400) // 90 * 60
            {
                return String.Format(Strings.Get1HourAgoText(), 1.ToString());
            }
            if (delta < 86400) // 24 * 60 * 60
            {
                return String.Format(Strings.GetNHoursAgoText(), ts.Hours.ToString());
            }
            if (delta < 172800) // 48 * 60 * 60
            {
                return String.Format(Strings.Get1DayAgoText(), 1.ToString());
            }
            if (delta < 604800) // 7 * 24 * 60 * 60
            {
                return String.Format(Strings.GetNDaysAgoText(), ts.Days.ToString());
            }
            if (delta < 2592000) // 30 * 24 * 60 * 60
            {
                int weeks = Convert.ToInt32(Math.Floor((double)ts.Days / 7));
                if (weeks <= 1)
                    return String.Format(Strings.Get1WeekAgoText(), 1.ToString());
                return String.Format(Strings.GetNWeeksAgoText(), weeks.ToString());
            }
            if (delta < 31104000) // 12 * 30 * 24 * 60 * 60
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                if (months <= 1)
                    return String.Format(Strings.Get1MonthAgoText(), 1.ToString());
                return String.Format(Strings.GetNMonthsAgoText(), months.ToString());
            }
            int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
            if (years <= 1)
                return String.Format(Strings.Get1YearAgoText(), 1.ToString());
            return String.Format(Strings.GetNYearsAgoText(), years.ToString());
        }


        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }


        public static string Join(this string left, string sep, string right)
        {
            if (left.IsNullOrEmpty())
                return right;
            else if (right.IsNullOrEmpty())
                return left;
            else
                return left + sep + right;

        }
    }
}