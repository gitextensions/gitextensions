using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using GitCommands.Config;
using GitUIPluginInterfaces;
using PatchApply;

namespace GitCommands
{
    public class GitCommands : IGitCommands
    {
        private static GitVersion _versionInUse;
        public bool CollectOutput = true;
        public bool StreamOutput;
        public Process Process { get; set; }
        public StringBuilder Output { get; set; }
        public StringBuilder ErrorOutput { get; set; }

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

        #region IGitCommands Members

        public IList<IGitSubmodule> GetSubmodules()
        {
            var submodules = RunCmd(Settings.GitCommand, "submodule status").Split('\n');

            IList<IGitSubmodule> submoduleList = new List<IGitSubmodule>();

            string lastLine = null;

            foreach (var submodule in submodules)
            {
                if (submodule.Length < 43)
                    continue;

                if (submodule.Equals(lastLine))
                    continue;

                lastLine = submodule;

                var gitSubmodule = CreateGitSubmodule(submodule);
                submoduleList.Add(gitSubmodule);
            }

            return submoduleList;
        }

        #endregion

        public static string FindGitWorkingDir(string startDir)
        {
            if (string.IsNullOrEmpty(startDir))
                return "";

            if (!startDir.EndsWith("\\") && !startDir.EndsWith("/"))
                startDir += "\\";

            var dir = startDir;

            while (dir.LastIndexOfAny(new[] {'\\', '/'}) > 0)
            {
                dir = dir.Substring(0, dir.LastIndexOfAny(new[] {'\\', '/'}));

                if (Settings.ValidWorkingDir(dir))
                    return dir + "\\";
            }
            return startDir;
        }

        public static Encoding EndcodingRouter(string arg)
        {
            var regcol = new StringCollection {"ls-files", "diff", "ls-tree"};
            var r = new Regex(ConvertRegCollectionToString(regcol));
            return
                r.IsMatch(arg)
                    ? Encoding.GetEncoding(Thread.CurrentThread.CurrentCulture.TextInfo.ANSICodePage)
                    : Encoding.UTF8;
        }

        public static string ConvertRegCollectionToString(StringCollection regcol)
        {
            var sb = new StringBuilder();
            foreach (var s in regcol)
            {
                sb.Append("|(" + s + ")");
            }
            return sb.ToString().Substring(1);
        }

        public static string RunCmd(string cmd)
        {
            return RunCmd(cmd, "");
        }

        public static void SetEnvironmentVariable()
        {
            SetEnvironmentVariable(false);
        }

        public static void SetEnvironmentVariable(bool reload)
        {
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
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HOME")))
                return;

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
            Environment.SetEnvironmentVariable("HOME", homePath);
        }

        public static void RunRealCmd(string cmd, string arguments)
        {
            try
            {
                CreateAndStartCommand(cmd, arguments, true);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private static string FixPath(string path)
        {
            path = path.Trim();

            return path.StartsWith("\\\\") ? path : path.Replace('\\', '/');
        }

        public static void RunRealCmdDetatched(string cmd, string arguments)
        {
            try
            {
                CreateAndStartCommand(cmd, arguments, false);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private static void CreateAndStartCommand(string cmd, string arguments, bool waitAndExit)
        {
            SetEnvironmentVariable();

            Settings.GitLog.Log(cmd + " " + arguments);
            //process used to execute external commands

            var process =
                new Process
                    {
                        StartInfo =
                            {
                                UseShellExecute = true,
                                ErrorDialog = false,
                                RedirectStandardOutput = false,
                                RedirectStandardInput = false,
                                CreateNoWindow = false,
                                FileName = string.Format("\"{0}\"", cmd),
                                Arguments = arguments,
                                WorkingDirectory = Settings.WorkingDir,
                                WindowStyle = ProcessWindowStyle.Normal,
                                LoadUserProfile = true
                            }
                    };


            process.Start();
            if (!waitAndExit)
                return;
            process.WaitForExit();
            process.Close();
        }

        public static void Run(string cmd, string arguments)
        {
            try
            {
                SetEnvironmentVariable();
                CreateAndStartProcess(arguments, cmd);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public Process CmdStartProcess(string cmd, string arguments)
        {
            SetEnvironmentVariable();

            var ssh = UseSsh(arguments);

            Kill();

            Settings.GitLog.Log(cmd + " " + arguments);

            //process used to execute external commands
            Process = new Process();
            SetCommonProcessAttributes(Process, arguments);
            Process.StartInfo.CreateNoWindow = (!ssh && !Settings.ShowGitCommandLine);
            Process.StartInfo.FileName = "\"" + cmd + "\"";
            Process.StartInfo.Arguments = arguments;
            Process.StartInfo.WorkingDirectory = Settings.WorkingDir;
            Process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            Process.StartInfo.LoadUserProfile = true;
            Process.EnableRaisingEvents = true;

            if (!StreamOutput)
            {
                Process.OutputDataReceived += ProcessOutputDataReceived;
                Process.ErrorDataReceived += ProcessErrorDataReceived;
            }
            Output = new StringBuilder();
            ErrorOutput = new StringBuilder();

            Process.Exited += ProcessExited;
            Process.Start();

            if (!StreamOutput)
            {
                Process.BeginErrorReadLine();
                Process.BeginOutputReadLine();
            }

            return Process;
        }

        private static void SetCommonProcessAttributes(Process process, string arguments)
        {
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.ErrorDialog = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.StandardErrorEncoding = EndcodingRouter(arguments);
            process.StartInfo.StandardOutputEncoding = process.StartInfo.StandardErrorEncoding;
        }

        private static bool UseSsh(string arguments)
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
                   (arguments.Contains("pull"));
        }

        public void Kill()
        {
            //If there was another process running, kill it
            if (Process == null)
                return;
            try
            {
                if (!Process.HasExited)
                {
                    Process.Kill();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            Process.Close();
        }

        public event DataReceivedEventHandler DataReceived;
        public event EventHandler Exited;

        private void ProcessExited(object sender, EventArgs e)
        {
            if (Exited != null)
                Exited(this, e);
        }

        private void ProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (CollectOutput)
                Output.Append(e.Data + Environment.NewLine);
            if (DataReceived != null)
                DataReceived(this, e);
        }

        private void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (CollectOutput)
                Output.Append(e.Data + Environment.NewLine);
            if (DataReceived != null)
                DataReceived(this, e);
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public static string RunCachableCmd(string cmd, string arguments)
        {
            string output;
            if (GitCommandCache.TryGet(arguments, out output))
                return output;

            output = RunCmd(cmd, arguments);

            GitCommandCache.Add(arguments, output);

            return output;
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public static string RunCmd(string cmd, string arguments)
        {
            string output;
            try
            {
                SetEnvironmentVariable();

                arguments = arguments.Replace("$QUOTE$", "\\\"");

                var process = CreateAndStartProcess(arguments, cmd);

                output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();

                process.WaitForExit();
                process.Close();
                // Read the output stream first and then wait. 

                if (!string.IsNullOrEmpty(error))
                {
                    output += Environment.NewLine + error;
                }
            }
            catch
            {
                return "";
            }
            return output;
        }

        private static Process CreateAndStartProcess(string arguments, string cmd)
        {
            Settings.GitLog.Log(cmd + " " + arguments);
            //process used to execute external commands

            var process = new Process();
            SetCommonProcessAttributes(process, arguments);

            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = string.Format("\"{0}\"", cmd);
            process.StartInfo.Arguments = arguments;
            process.StartInfo.WorkingDirectory = Settings.WorkingDir;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.StartInfo.LoadUserProfile = true;

            process.Start();
            return process;
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public static Process RunCmdAsync(string cmd, string arguments)
        {
            var process = new Process();
            try
            {
                SetEnvironmentVariable();

                Settings.GitLog.Log(cmd + " " + arguments);
                //process used to execute external commands

                process.StartInfo.UseShellExecute = true;
                process.StartInfo.ErrorDialog = true;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.RedirectStandardInput = false;
                process.StartInfo.RedirectStandardError = false;


                process.StartInfo.LoadUserProfile = true;
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.FileName = string.Format("\"{0}\"", cmd);
                process.StartInfo.Arguments = arguments;
                process.StartInfo.WorkingDirectory = Settings.WorkingDir;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.LoadUserProfile = true;

                process.Start();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

            return process;
        }

        public static bool InTheMiddleOfConflictedMerge()
        {
            return !string.IsNullOrEmpty(RunCmd(Settings.GitCommand, "ls-files --unmerged"));
        }

        public static List<GitItem> GetConflictedFiles()
        {
            var unmergedFiles = new List<GitItem>();

            var fileName = "";
            foreach (var file in GetUnmergedFileListing())
            {
                if (file.LastIndexOfAny(new[] {' ', '\t'}) <= 0)
                    continue;
                if (file.Substring(file.LastIndexOfAny(new[] {' ', '\t'}) + 1) == fileName)
                    continue;
                fileName = file.Substring(file.LastIndexOfAny(new[] {' ', '\t'}) + 1);
                unmergedFiles.Add(new GitItem {FileName = fileName});
            }

            return unmergedFiles;
        }

        private static IEnumerable<string> GetUnmergedFileListing()
        {
            return RunCmd(Settings.GitCommand, "ls-files --unmerged").Split('\n');
        }

        public static bool HandleConflictSelectBase(string fileName)
        {
            if (!HandleConflictsSaveSide(fileName, fileName, "1"))
                return false;

            RunCmd(Settings.GitCommand, "add -- \"" + fileName + "\"");
            return true;
        }

        public static bool HandleConflictSelectLocal(string fileName)
        {
            if (!HandleConflictsSaveSide(fileName, fileName, "2"))
                return false;

            RunCmd(Settings.GitCommand, "add -- \"" + fileName + "\"");
            return true;
        }

        public static bool HandleConflictSelectRemote(string fileName)
        {
            if (!HandleConflictsSaveSide(fileName, fileName, "3"))
                return false;

            RunCmd(Settings.GitCommand, "add -- \"" + fileName + "\"");
            return true;
        }

        public static bool HandleConflictsSaveSide(string fileName, string saveAs, string side)
        {
            side = GetSide(side);

            fileName = FixPath(fileName);
            var unmerged = RunCmd(Settings.GitCommand, "ls-files --unmerged \"" + fileName + "\"").Split('\n');

            foreach (var file in unmerged)
            {
                var fileline = file.Split(new[] {' ', '\t'});
                if (fileline.Length < 3)
                    continue;
                if (fileline[2].Trim() != side)
                    continue;
                RunCmd(Settings.GitCommand, "cat-file blob \"" + fileline[1] + "\" > \"" + saveAs + "\"");
                return true;
            }
            return false;
        }

        private static string GetSide(string side)
        {
            if (side.Equals("REMOTE", StringComparison.CurrentCultureIgnoreCase))
                side = "3";
            if (side.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
                side = "2";
            if (side.Equals("BASE", StringComparison.CurrentCultureIgnoreCase))
                side = "1";
            return side;
        }

        public static string[] GetConflictedFiles(string filename)
        {
            filename = FixPath(filename);

            string[] fileNames =
                {
                    filename + ".BASE",
                    filename + ".LOCAL",
                    filename + ".REMOTE"
                };

            var unmerged = RunCmd(Settings.GitCommand, "ls-files --unmerged \"" + filename + "\"").Split('\n');

            foreach (var file in unmerged)
            {
                var fileline = file.Split(new[] {' ', '\t'});
                if (fileline.Length < 3)
                    continue;

                int stage;
                Int32.TryParse(fileline[2].Trim(), out stage);

                var tempFile = RunCmd(Settings.GitCommand, "checkout-index --temp --stage=" + stage + " -- " + filename);
                tempFile = tempFile.Split('\t')[0];
                tempFile = Path.Combine(Settings.WorkingDir, tempFile);

                var newFileName = Path.Combine(Settings.WorkingDir, fileNames[stage - 1]);
                try
                {
                    fileNames[stage - 1] = newFileName;
                    var index = 1;
                    while (File.Exists(fileNames[stage - 1]) && index < 50)
                    {
                        fileNames[stage - 1] = newFileName + index;
                        index++;
                    }
                    File.Move(tempFile, fileNames[stage - 1]);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }

            return fileNames;
        }

        public static string GetMergeMessage()
        {
            var file = Settings.WorkingDir + ".git\\MERGE_MSG";

            return
                File.Exists(file)
                    ? File.ReadAllText(file)
                    : "";
        }

        public static void RunGitK()
        {
            Run("cmd.exe", "/c \"\"" + Settings.GitCommand.Replace("git.cmd", "gitk.cmd") + "\" --all\"");
        }

        public static void RunGui()
        {
            Run("cmd.exe", "/c \"\"" + Settings.GitCommand + "\" gui\"");
        }

        public static void RunBash()
        {
            RunRealCmdDetatched("cmd.exe", "/c \"\"" + Settings.GitBinDir + "sh\" --login -i\"");
        }

        public static string Init(bool bare, bool shared)
        {
            if (bare && shared)
                return RunCmd(Settings.GitCommand, "init --bare --shared=all");
            if (bare)
                return RunCmd(Settings.GitCommand, "init --bare");
            return RunCmd(Settings.GitCommand, "init");
        }

        public static string CherryPickCmd(string cherry, bool commit)
        {
            if (commit)
                return "cherry-pick \"" + cherry + "\"";
            return "cherry-pick --no-commit \"" + cherry + "\"";
        }


        public static string CherryPick(string cherry, bool commit)
        {
            if (commit)
                return RunCmd(Settings.GitCommand, "cherry-pick \"" + cherry + "\"");
            return RunCmd(Settings.GitCommand, "cherry-pick --no-commit \"" + cherry + "\"");
        }

        public static string ShowSha1(string sha1)
        {
            return RunCachableCmd(Settings.GitCommand, "show " + sha1);
        }

        public static string UserCommitCount()
        {
            return RunCmd(Settings.GitCommand, "shortlog -s -n");
        }

        public static string DeleteBranch(string branchName, bool force)
        {
            return RunCmd(Settings.GitCommand, DeleteBranchCmd(branchName, force));
        }

        public static string DeleteBranchCmd(string branchName, bool force)
        {
            if (force)
                return "branch -D \"" + branchName + "\"";
            return "branch -d \"" + branchName + "\"";
        }

        public static string DeleteTag(string tagName)
        {
            return RunCmd(Settings.GitCommand, DeleteTagCmd(tagName));
        }

        public static string DeleteTagCmd(string tagName)
        {
            return "tag -d \"" + tagName + "\"";
        }

        public static string GetCurrentCheckout()
        {
            return RunCmd(Settings.GitCommand, "log -g -1 HEAD --pretty=format:%H");
        }

        public static int CommitCount()
        {
            int count;
            var arguments = "/c \"\"" + Settings.GitCommand + "\" rev-list --all --abbrev-commit | wc -l\"";
            return
                int.TryParse(RunCmd("cmd.exe", arguments), out count)
                    ? count
                    : 0;
        }

        public static string GetSubmoduleRemotePath(string name)
        {
            var configFile = new ConfigFile(Settings.WorkingDir + "\\.gitmodules");
            return configFile.GetValue("submodule." + name.Trim() + ".url").Trim();
        }

        public static string GetSubmoduleLocalPath(string name)
        {
            var configFile = new ConfigFile(Settings.WorkingDir + "\\.gitmodules");
            return configFile.GetValue("submodule." + name.Trim() + ".path").Trim();
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
                branch = " \"" + branch.Trim() + "\"";

            return "submodule add \"" + remotePath.Trim() + "\" \"" + localPath.Trim() + "\"" + branch;
        }

        private static GitSubmodule CreateGitSubmodule(string submodule)
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
                gitSubmodule.Branch = name.Substring(name.IndexOf("(")).Trim(new[] {'(', ')', ' '});
            }
            else
                gitSubmodule.Name = name;
            return gitSubmodule;
        }

        public static string Stash()
        {
            return RunCmd(Settings.GitCommand, "stash save");
        }

        public static string StashApply()
        {
            return RunCmd(Settings.GitCommand, "stash apply");
        }

        public static string StashClear()
        {
            return RunCmd(Settings.GitCommand, "stash clear");
        }

        public static string RevertCmd(string commit, bool autoCommit)
        {
            if (autoCommit)
                return "revert " + commit;
            return "revert --no-commit " + commit;
        }


        public static string ResetSoft(string commit)
        {
            return ResetSoft(commit, "");
        }

        public static string ResetMixed(string commit)
        {
            return ResetMixed(commit, "");
        }

        public static string ResetHard(string commit)
        {
            return ResetHard(commit, "");
        }

        public static string ResetSoft(string commit, string file)
        {
            var args = "reset --soft";

            if (!string.IsNullOrEmpty(commit))
                args += " \"" + commit + "\"";

            if (!string.IsNullOrEmpty(file))
                args += " -- \"" + file + "\"";

            return RunCmd(Settings.GitCommand, args);
        }

        public static string ResetMixed(string commit, string file)
        {
            var args = "reset --mixed";

            if (!string.IsNullOrEmpty(commit))
                args += " \"" + commit + "\"";

            if (!string.IsNullOrEmpty(file))
                args += " -- \"" + file + "\"";

            return RunCmd(Settings.GitCommand, args);
        }

        public static string ResetHard(string commit, string file)
        {
            var args = "reset --hard";

            if (!string.IsNullOrEmpty(commit))
                args += " \"" + commit + "\"";

            if (!string.IsNullOrEmpty(file))
                args += " -- \"" + file + "\"";

            return RunCmd(Settings.GitCommand, args);
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

        public static string CloneCmd(string fromPath, string toPath, bool central, int? depth)
        {
            var from = FixPath(fromPath);
            var to = FixPath(toPath);
            var options = new List<string> {"-v"};
            if (central)
                options.Add("--bare");
            if (depth.HasValue)
                options.Add("--depth " + depth);
            if (VersionInUse.CloneCanAskForProgress)
                options.Add("--progress");
            options.Add(string.Format("\"{0}\"", from.Trim()));
            options.Add(string.Format("\"{0}\"", to.Trim()));

            return "clone " + string.Join(" ", options.ToArray());
        }

        public static string ResetFile(string file)
        {
            file = FixPath(file);
            return RunCmd(Settings.GitCommand, "checkout-index --index --force -- \"" + file + "\"");
        }


        public string FormatPatch(string from, string to, string output, int start)
        {
            output = FixPath(output);

            var result = RunCmd(Settings.GitCommand,
                                "format-patch -M -C -B --start-number " + start + " \"" + from + "\"..\"" + to +
                                "\" -o \"" + output + "\"");

            return result;
        }

        public string FormatPatch(string from, string to, string output)
        {
            output = FixPath(output);

            var result = RunCmd(Settings.GitCommand,
                                "format-patch -M -C -B \"" + from + "\"..\"" + to + "\" -o \"" + output + "\"");

            return result;
        }


        public static string Tag(string tagName, string revision, bool annotation)
        {
            string result;

            if (annotation)
                result = RunCmd(Settings.GitCommand,
                                "tag \"" + tagName.Trim() + "\" -a -F \"" + Settings.WorkingDirGitDir() +
                                "\\TAGMESSAGE\" -- \"" + revision + "\"");
            else
                result = RunCmd(Settings.GitCommand, "tag \"" + tagName.Trim() + "\" \"" + revision + "\"");

            return result;
        }

        public static string Branch(string branchName, string revision, bool checkout)
        {
            var result = RunCmd(Settings.GitCommand, BranchCmd(branchName, revision, checkout));

            return result;
        }

        public static string BranchCmd(string branchName, string revision, bool checkout)
        {
            if (checkout)
                return "checkout -b \"" + branchName.Trim() + "\" \"" + revision + "\"";
            return "branch \"" + branchName.Trim() + "\" \"" + revision + "\"";
        }

        public static void UnSetSsh()
        {
            Environment.SetEnvironmentVariable("GIT_SSH", "", EnvironmentVariableTarget.Process);
        }

        public static void SetSsh(string path)
        {
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

        public static string Push(string path)
        {
            return RunCmd(Settings.GitCommand, "push \"" + FixPath(path).Trim() + "\"");
        }

        public static Process PushAsync(string path, string branch, bool all)
        {
            var arguments =
                string.Format(" /k \"\"{0}\" {1}\"",
                              Settings.GitCommand,
                              PushCmd(FixPath(path),
                                      branch,
                                      all));
            return RunCmdAsync("cmd.exe", arguments);
        }


        public static bool StartPageantForRemote(string remote)
        {
            var sshKeyFile = GetPuttyKeyFileForRemote(remote);
            if (string.IsNullOrEmpty(sshKeyFile))
                return false;

            StartPageantWithKey(sshKeyFile);
            return true;
        }

        public static void StartPageantWithKey(string sshKeyFile)
        {
            Run(Settings.Pageant, "\"" + sshKeyFile + "\"");
        }

        public static string GetPuttyKeyFileForRemote(string remote)
        {
            if (string.IsNullOrEmpty(remote) ||
                string.IsNullOrEmpty(Settings.Pageant) ||
                !Settings.AutoStartPageant ||
                !Plink())
                return "";

            return GetSetting("remote." + remote + ".puttykeyfile");
        }

        public static string PushCmd(string path, string branch, bool all)
        {
            return PushCmd(path, null, branch, all, false);
        }

        public static string PushCmd(string path, string fromBranch, string toBranch, bool all, bool force)
        {
            path = FixPath(path);

            if (string.IsNullOrEmpty(fromBranch) && !string.IsNullOrEmpty(toBranch))
                fromBranch = "HEAD";

            toBranch = toBranch.Replace(" ", "");

            var sforce = "";
            if (force)
                sforce = "-f ";

            if (all)
                return string.Format("push {0}--all \"{1}\"", sforce, path.Trim());

            if (!string.IsNullOrEmpty(toBranch) && !string.IsNullOrEmpty(fromBranch))
                return string.Format("push {0}\"{1}\" {2}:{3}", sforce, path.Trim(), fromBranch, toBranch);

            return string.Format("push {0}\"{1}\" {2}", sforce, path.Trim(), fromBranch);
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

        public static string Fetch(string remote, string branch)
        {
            remote = FixPath(remote);

            Directory.SetCurrentDirectory(Settings.WorkingDir);
            branch = FetchCmd(remote, branch);

            RunRealCmd("cmd.exe", " /k \"\"" + Settings.GitCommand + "\" " + FetchCmd(remote, branch) + "\"");

            return "Done";
        }

        public static bool PathIsUrl(string path)
        {
            return path.Contains("\\") || path.Contains("/");
        }

        public static string FetchCmd(string remote, string branch)
        {
            return "fetch " + GetFetchArgs(remote, branch);
        }

        public static string Pull(string remote, string branch, bool rebase)
        {
            remote = FixPath(remote);

            Directory.SetCurrentDirectory(Settings.WorkingDir);

            RunRealCmd("cmd.exe", " /k \"\"" + Settings.GitCommand + "\" " + PullCmd(remote, branch, rebase) + "\"");

            return "Done";
        }

        public static string PullCmd(string remote, string branch, bool rebase)
        {
            var rebaseOption = "";
            if (rebase)
                rebaseOption = "--rebase ";

            return "pull " + rebaseOption + GetFetchArgs(remote, branch);
        }

        private static string GetFetchArgs(string remote, string branch)
        {
            remote = FixPath(remote);

            branch = branch.Replace(" ", "");

            string localbranch;

            if (string.IsNullOrEmpty(branch))
                localbranch = "";
            else
                localbranch = "+refs/heads/" + branch + "";

            string remotebranch;
            var remoteUrl = GetSetting("remote." + remote + ".url");

            if (PathIsUrl(remote) && !string.IsNullOrEmpty(branch) && string.IsNullOrEmpty(remoteUrl))
                remotebranch = ":refs/heads/" + branch + "";
            else if (string.IsNullOrEmpty(branch) || PathIsUrl(remote) || string.IsNullOrEmpty(remoteUrl))
                remotebranch = "";
            else
                remotebranch = ":" + "refs/remotes/" + remote.Trim() + "/" + branch + "";

            var progressOption = "";
            if (VersionInUse.FetchCanAskForProgress)
                progressOption = "--progress ";

            return progressOption + "\"" + remote.Trim() + "\" " + localbranch + remotebranch;
        }

        public static string ContinueRebase()
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            var result = RunCmd(Settings.GitCommand, ContinueRebaseCmd());

            return result;
        }

        public static string ContinueRebaseCmd()
        {
            return "rebase --continue";
        }

        public static string SkipRebase()
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            var result = RunCmd(Settings.GitCommand, SkipRebaseCmd());

            return result;
        }

        public static string SkipRebaseCmd()
        {
            return "rebase --skip";
        }

        public static string GetRebaseDir()
        {
            if (Directory.Exists(Settings.WorkingDir + ".git\\rebase-apply\\"))
                return Settings.WorkingDir + ".git\\rebase-apply\\";
            if (Directory.Exists(Settings.WorkingDir + ".git\\rebase\\"))
                return Settings.WorkingDir + ".git\\rebase\\";

            return "";
        }

        public static bool InTheMiddleOfRebase()
        {
            return !File.Exists(GetRebaseDir() + "applying") &&
                   Directory.Exists(GetRebaseDir());
        }

        public static bool InTheMiddleOfPatch()
        {
            return !File.Exists(GetRebaseDir() + "rebasing") &&
                   Directory.Exists(GetRebaseDir());
        }


        public static string GetNextRebasePatch()
        {
            var file = GetRebaseDir() + "next";
            return File.Exists(file) ? File.ReadAllText(file).Trim() : "";
        }

        public static List<PatchFile> GetRebasePatchFiles()
        {
            var patchFiles = new List<PatchFile>();

            var nextFile = GetNextRebasePatch();

            int next;
            int.TryParse(nextFile, out next);


            var files = new string[0];
            if (Directory.Exists(GetRebaseDir()))
                files = Directory.GetFiles(GetRebaseDir());

            foreach (var fullFileName in files)
            {
                int n;
                var file = fullFileName.Substring(fullFileName.LastIndexOf('\\') + 1);
                if (!int.TryParse(file, out n))
                    continue;

                var patchFile =
                    new PatchFile
                        {
                            Name = file,
                            FullName = fullFileName,
                            IsNext = n == next,
                            IsSkipped = n < next
                        };

                if (File.Exists(GetRebaseDir() + file))
                {
                    foreach (var line in File.ReadAllLines(GetRebaseDir() + file))
                    {
                        if (line.StartsWith("From: "))
                            if (line.IndexOf('<') > 0 && line.IndexOf('<') < line.Length)
                                patchFile.Author = line.Substring(6, line.IndexOf('<') - 6).Trim();
                            else
                                patchFile.Author = line.Substring(6).Trim();

                        if (line.StartsWith("Date: "))
                            if (line.IndexOf('+') > 0 && line.IndexOf('<') < line.Length)
                                patchFile.Date = line.Substring(6, line.IndexOf('+') - 6).Trim();
                            else
                                patchFile.Date = line.Substring(6).Trim();


                        if (line.StartsWith("Subject: ")) patchFile.Subject = line.Substring(9).Trim();

                        if (!string.IsNullOrEmpty(patchFile.Author) &&
                            !string.IsNullOrEmpty(patchFile.Date) &&
                            !string.IsNullOrEmpty(patchFile.Subject))
                            break;
                    }
                }

                patchFiles.Add(patchFile);
            }

            return patchFiles;
        }

        public static string Rebase(string branch)
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            return RunCmd(Settings.GitCommand, RebaseCmd(branch));
        }

        public static string RebaseCmd(string branch)
        {
            return "rebase \"" + branch + "\"";
        }


        public static string AbortRebase()
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            return RunCmd(Settings.GitCommand, AbortRebaseCmd());
        }

        public static string AbortRebaseCmd()
        {
            return "rebase --abort";
        }

        public static string Resolved()
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            return RunCmd(Settings.GitCommand, ResolvedCmd());
        }

        public static string ResolvedCmd()
        {
            return "am --3way --resolved";
        }

        public static string Skip()
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            return RunCmd(Settings.GitCommand, SkipCmd());
        }

        public static string SkipCmd()
        {
            return "am --3way --skip";
        }

        public static string Abort()
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            return RunCmd(Settings.GitCommand, AbortCmd());
        }

        public static string AbortCmd()
        {
            return "am --3way --abort";
        }

        public static string Commit(bool amend)
        {
            return RunCmd(Settings.GitCommand, CommitCmd(amend));
        }

        public static string CommitCmd(bool amend)
        {
            var path = Settings.WorkingDirGitDir() + "\\COMMITMESSAGE\"";
            if (amend)
                return "commit --amend -F \"" + path;
            return "commit  -F \"" + path;
        }

        public static string Patch(string patchFile)
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            return RunCmd(Settings.GitCommand, PatchCmd(FixPath(patchFile)));
        }

        public static string PatchCmd(string patchFile)
        {
            return "am --3way --signoff \"" + FixPath(patchFile) + "\"";
        }

        public static string PatchDirCmd(string patchDir)
        {
            return "am --3way --signoff --directory=\"" + FixPath(patchDir) + "\"";
        }

        public static string UpdateRemotes()
        {
            return RunCmd(Settings.GitCommand, "remote update");
        }

        public static string RemoveRemote(string name)
        {
            return RunCmd(Settings.GitCommand, "remote rm \"" + name + "\"");
        }

        public static string RenameRemote(string name, string newname)
        {
            return RunCmd(Settings.GitCommand, "remote rename \"" + name + "\" \"" + newname + "\"");
        }

        public static string AddRemote(string name, string path)
        {
            var location = FixPath(path);

            if (string.IsNullOrEmpty(name))
                return "Please enter a name.";

            return
                string.IsNullOrEmpty(location)
                    ? RunCmd(Settings.GitCommand, string.Format("remote add \"{0}\" \"\"", name))
                    : RunCmd(Settings.GitCommand, string.Format("remote add \"{0}\" \"{1}\"", name, location));
        }

        public static string[] GetRemotes()
        {
            return RunCmd(Settings.GitCommand, "remote show").Split('\n');
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
            return new ConfigFile(Environment.GetEnvironmentVariable("HOME") + "\\.gitconfig");
        }

        public string GetGlobalSetting(string setting)
        {
            var configFile = GetGlobalConfig();
            return configFile.GetValue(setting);
        }

        public void SetGlobalSetting(string setting, string value)
        {
            var configFile = GetGlobalConfig();
            configFile.SetValue(setting, value);
            configFile.Save();
        }

        public static ConfigFile GetLocalConfig()
        {
            return new ConfigFile(Settings.WorkingDirGitDir() + "\\config");
        }

        public static string GetSetting(string setting)
        {
            var configFile = GetLocalConfig();
            return configFile.GetValue(setting);
        }

        public static void UnSetSetting(string setting)
        {
            var configFile = GetLocalConfig();
            configFile.RemoveSetting(setting);
            configFile.Save();
        }

        public static void SetSetting(string setting, string value)
        {
            var configFile = GetLocalConfig();
            configFile.SetValue(setting, value);
            configFile.Save();
        }

        public static List<Patch> GetStashedItems(string stashName)
        {
            var patchManager = new PatchManager();
            patchManager.LoadPatch(RunCmd(Settings.GitCommand, "stash show -p " + stashName), false);

            return patchManager.patches;
        }

        public static List<GitStash> GetStashes()
        {
            var list = RunCmd(Settings.GitCommand, "stash list").Split('\n');

            var stashes = new List<GitStash>();
            foreach (var stashString in list)
            {
                if (stashString.IndexOf(':') <= 0)
                    continue;

                var stash =
                    new GitStash
                        {
                            Name = stashString.Substring(0, stashString.IndexOf(':')).Trim()
                        };

                if (stashString.IndexOf(':') + 1 < stashString.Length)
                    stash.Message = stashString.Substring(stashString.IndexOf(':') + 1).Trim();

                stashes.Add(stash);
            }

            return stashes;
        }

        public static Patch GetSingleDiff(string from, string to, string filter, string extraDiffArguments)
        {
            filter = FixPath(filter);
            from = FixPath(from);
            to = FixPath(to);

            var patchManager = new PatchManager();
            var arguments = string.Format("diff{0} \"{1}\" \"{2}\" -- \"{3}\"", extraDiffArguments, to, from, filter);
            patchManager.LoadPatch(RunCachableCmd(Settings.GitCommand, arguments), false);

            return patchManager.patches.Count > 0 ? patchManager.patches[0] : null;
        }

        public static List<Patch> GetDiff(string from, string to, string extraDiffArguments)
        {
            var patchManager = new PatchManager();
            var arguments = string.Format("diff{0} \"{1}\" \"{2}\"", extraDiffArguments, from, to);
            patchManager.LoadPatch(RunCachableCmd(Settings.GitCommand, arguments), false);

            return patchManager.patches;
        }

        public static List<string> GetDiffFiles(string from)
        {
            var result = RunCmd(Settings.GitCommand, "diff --name-only \"" + from + "\"");

            var files = result.Split('\n');

            var diffFiles = new List<string>();
            foreach (var s in files)
            {
                if (!string.IsNullOrEmpty(s))
                    diffFiles.Add(s);
            }

            return diffFiles;
        }

        public static List<GitItemStatus> GetDiffFiles(string from, string to)
        {
            var result = RunCachableCmd(Settings.GitCommand, "diff --name-status \"" + to + "\" \"" + from + "\"");

            var files = result.Split('\n');

            var diffFiles = new List<GitItemStatus>();
            foreach (var s in files)
            {
                if (string.IsNullOrEmpty(s))
                    continue;

                diffFiles.Add(
                    new GitItemStatus
                        {
                            Name = s.Substring(1).Trim(),
                            IsNew = s[0] == 'A',
                            IsChanged = s[0] == 'M',
                            IsDeleted = s[0] == 'D',
                            IsTracked = true
                        });
            }

            return diffFiles;
        }

        public static List<GitItemStatus> GetUntrackedFiles()
        {
            var status = RunCmd(Settings.GitCommand,
                                "ls-files --others --directory --no-empty-directory --exclude-standard");

            var statusStrings = status.Split('\n');

            var gitItemStatusList = new List<GitItemStatus>();

            foreach (var statusString in statusStrings)
            {
                if (string.IsNullOrEmpty(statusString.Trim()))
                    continue;
                gitItemStatusList.Add(
                    new GitItemStatus
                        {
                            IsNew = true,
                            IsChanged = false,
                            IsDeleted = false,
                            IsTracked = false,
                            Name = statusString.Trim()
                        });
            }

            return gitItemStatusList;
        }

        public static List<GitItemStatus> GetModifiedFiles()
        {
            var status = RunCmd(Settings.GitCommand, "ls-files --modified --exclude-standard");

            var statusStrings = status.Split('\n');

            var gitItemStatusList = new List<GitItemStatus>();

            foreach (var statusString in statusStrings)
            {
                if (string.IsNullOrEmpty(statusString.Trim()))
                    continue;
                gitItemStatusList.Add(
                    new GitItemStatus
                        {
                            IsNew = false,
                            IsChanged = true,
                            IsDeleted = false,
                            IsTracked = true,
                            Name = statusString.Trim()
                        });
            }

            return gitItemStatusList;
        }

        public static string GetAllChangedFilesCmd(bool excludeIgnoredFiles, bool showUntrackedFiles)
        {
            var stringBuilder = new StringBuilder("ls-files --deleted --modified --no-empty-directory -t");

            if (showUntrackedFiles)
                stringBuilder.Append(" --others");
            if (excludeIgnoredFiles)
                stringBuilder.Append(" --exclude-standard");

            return stringBuilder.ToString();
        }


        public static List<GitItemStatus> GetAllChangedFiles()
        {
            var status = RunCmd(Settings.GitCommand, GetAllChangedFilesCmd(true, true));

            return GetAllChangedFilesFromString(status);
        }

        public static List<GitItemStatus> GetAllChangedFilesFromString(string status)
        {
            var statusStrings = status.Split('\n');

            var gitItemStatusList = new List<GitItemStatus>();

            GitItemStatus itemStatus = null;
            foreach (var statusString in statusStrings)
            {
                if (string.IsNullOrEmpty(statusString.Trim()) || statusString.Length < 2)
                    continue;

                if (!(itemStatus != null && itemStatus.Name == statusString.Substring(1).Trim()))
                {
                    itemStatus = new GitItemStatus {Name = statusString.Substring(1).Trim()};
                    gitItemStatusList.Add(itemStatus);
                }

                itemStatus.IsNew = itemStatus.IsNew || statusString[0] == '?';
                itemStatus.IsChanged = itemStatus.IsChanged || statusString[0] == 'C';
                itemStatus.IsDeleted = itemStatus.IsDeleted || statusString[0] == 'R';
                itemStatus.IsTracked = itemStatus.IsTracked || statusString[0] != '?';
            }

            return gitItemStatusList;
        }

        public static List<GitItemStatus> GetDeletedFiles()
        {
            var status = RunCmd(Settings.GitCommand, "ls-files --deleted --exclude-standard");

            var statusStrings = status.Split('\n');

            var gitItemStatusList = new List<GitItemStatus>();

            foreach (var statusString in statusStrings)
            {
                if (string.IsNullOrEmpty(statusString.Trim()))
                    continue;
                gitItemStatusList.Add(
                    new GitItemStatus
                        {
                            IsNew = false,
                            IsChanged = false,
                            IsDeleted = true,
                            IsTracked = true,
                            Name = statusString.Trim()
                        });
            }

            return gitItemStatusList;
        }

        public static bool FileIsStaged(string filename)
        {
            var status = RunCmd(Settings.GitCommand, "diff --cached --numstat -- \"" + filename + "\"");
            return !string.IsNullOrEmpty(status);
        }

        public static List<GitItemStatus> GetStagedFiles()
        {
            var status = RunCmd(Settings.GitCommand, "diff --cached --name-status");

            var gitItemStatusList = new List<GitItemStatus>();

            if (status.Length < 50 && status.Contains("fatal: No HEAD commit to compare"))
            {
                status = RunCmd(Settings.GitCommand, "status --untracked-files=no");

                var statusStrings = status.Split('\n');

                foreach (var statusString in statusStrings)
                {
                    if (statusString.StartsWith("#\tnew file:"))
                    {
                        ProcessStatusNewFile(statusString, gitItemStatusList);
                    }
                }
            }
            else
            {
                var statusStrings = status.Split('\n');

                foreach (var statusString in statusStrings)
                {
                    if (string.IsNullOrEmpty(statusString.Trim()))
                        continue;
                    var itemStatus =
                        new GitItemStatus
                            {
                                IsTracked = true,
                                Name = statusString.Substring(1).Trim()
                            };

                    itemStatus.IsNew = itemStatus.IsNew || statusString[0] == 'A';
                    itemStatus.IsChanged = itemStatus.IsChanged || statusString[0] == 'M';
                    itemStatus.IsDeleted = itemStatus.IsDeleted || statusString[0] == 'D';
                    itemStatus.IsTracked = itemStatus.IsTracked || statusString[0] != '?';

                    gitItemStatusList.Add(itemStatus);
                }
            }

            return gitItemStatusList;
        }

        public static List<GitItemStatus> GitStatus()
        {
            return GitStatus(true);
        }

        public static List<GitItemStatus> GitStatus(bool untracked)
        {
            var status = RunCmd(Settings.GitCommand, untracked ? "status --untracked=all" : "status");
            var statusStrings = status.Split('\n');

            var gitItemStatusList = new List<GitItemStatus>();

            foreach (var statusString in statusStrings)
            {
                if (statusString.StartsWith("#\tnew file:"))
                {
                    ProcessStatusNewFile(statusString, gitItemStatusList);
                }
                else if (statusString.StartsWith("#\tdeleted:"))
                {
                    gitItemStatusList.Add(
                        new GitItemStatus
                            {
                                IsDeleted = true,
                                IsTracked = true,
                                Name = statusString.Substring(statusString.LastIndexOf(':') + 1).Trim()
                            });
                }
                else if (statusString.StartsWith("#\tmodified:"))
                {
                    gitItemStatusList.Add(
                        new GitItemStatus
                            {
                                IsChanged = true,
                                IsTracked = true,
                                Name = statusString.Substring(statusString.LastIndexOf(':') + 1).Trim()
                            });
                }
                else if (statusString.StartsWith("#\t"))
                {
                    gitItemStatusList.Add(
                        new GitItemStatus
                            {
                                IsNew = true,
                                Name = statusString.Substring(2).Trim()
                            });
                }
            }

            return gitItemStatusList;
        }

        private static void ProcessStatusNewFile(string statusString, ICollection<GitItemStatus> gitItemStatusList)
        {
            gitItemStatusList.Add(
                new GitItemStatus
                    {
                        IsNew = true,
                        IsTracked = true,
                        Name = statusString.Substring(statusString.LastIndexOf(':') + 1).Trim()
                    });
        }

        public static string GetCurrentChanges(string name, bool staged, string extraDiffArguments)
        {
            name = FixPath(name);
            var args = "diff" + extraDiffArguments + " -- \"" + name + "\"";
            if (staged)
                args = "diff --cached" + extraDiffArguments + " -- \"" + name + "\"";

            return RunCmd(Settings.GitCommand, args);
        }

        public static string StageFiles(IList<GitItemStatus> files)
        {
            var gitCommand = new GitCommands();

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
            var gitCommand = new GitCommands();

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

        public static string StageFile(string file)
        {
            return RunCmd(Settings.GitCommand, "update-index --add" + " \"" + FixPath(file) + "\"");
        }

        public static string StageFileToRemove(string file)
        {
            return RunCmd(Settings.GitCommand, "update-index --remove" + " \"" + FixPath(file) + "\"");
        }


        public static string UnstageFile(string file)
        {
            return RunCmd(Settings.GitCommand, "rm" + " --cached \"" + FixPath(file) + "\"");
        }

        public static string UnstageFileToRemove(string file)
        {
            return RunCmd(Settings.GitCommand, "reset HEAD --" + " \"" + FixPath(file) + "\"");
        }

        public static string GetSelectedBranch()
        {
            var branches = RunCmd(Settings.GitCommand, "branch");
            var branchStrings = branches.Split('\n');
            foreach (var branch in branchStrings)
            {
                if (branch.IndexOf('*') > -1)
                    return branch.Trim('*', ' ');
            }
            return "";
        }

        public static List<GitHead> GetRemoteHeads(string remote, bool tags, bool branches)
        {
            remote = FixPath(remote);

            var tree = GetTreeFromRemoteHeands(remote, tags, branches);
            return GetHeads(tree);
        }

        private static string GetTreeFromRemoteHeands(string remote, bool tags, bool branches)
        {
            if (tags && branches)
                return RunCmd(Settings.GitCommand, "ls-remote --heads --tags \"" + remote + "\"");
            if (tags)
                return RunCmd(Settings.GitCommand, "ls-remote --tags \"" + remote + "\"");
            if (branches)
                return RunCmd(Settings.GitCommand, "ls-remote --heads \"" + remote + "\"");
            return "";
        }

        public static List<GitHead> GetHeads()
        {
            return GetHeads(true);
        }

        public static List<GitHead> GetHeads(bool tags)
        {
            return GetHeads(tags, true);
        }

        public static List<GitHead> GetHeads(bool tags, bool branches)
        {
            var tree = GetTree(tags, branches);
            return GetHeads(tree);
        }

        private static string GetTree(bool tags, bool branches)
        {
            if (tags && branches)
                return RunCmd(Settings.GitCommand, "show-ref --dereference");

            if (tags)
                return RunCmd(Settings.GitCommand, "show-ref --tags");

            if (branches)
                return RunCmd(Settings.GitCommand, "show-ref --dereference --heads");
            return "";
        }

        private static List<GitHead> GetHeads(string tree)
        {
            var itemsStrings = tree.Split('\n');

            var heads = new List<GitHead>();

            foreach (var itemsString in itemsStrings)
            {
                if (itemsString.Length <= 42) continue;

                var guid = itemsString.Substring(0, 40);
                var completeName = itemsString.Substring(41).Trim();
                heads.Add(new GitHead(guid, completeName));
            }

            return heads;
        }


        public static List<string> GetBranches(bool remotes, string filterRemote)
        {
            var tree =
                RunCmd(Settings.GitCommand, remotes
                                                ? "branch --verbose --no-abbrev -a"
                                                : "branch --verbose --no-abbrev");

            var heads = new List<string>();
            var itemsStrings = tree.Split('\n');
            foreach (var itemsString in itemsStrings)
            {
                if (itemsString.Length == 0)
                    continue;

                try
                {
                    var column = 0;
                    var itemColumns = itemsString.Split(" \t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (itemColumns[column] == "*")
                    {
                        column++;
                    }

                    var head = itemColumns[column];
                    column++;
                    if (head.Length == 0)
                        continue;

                    var guid = itemColumns[column];

                    if (!IsValidGuid(guid))
                        continue;

                    var remote = "";
                    if (head.LastIndexOf("/") > 1)
                    {
                        var isRemote = head.Contains("remotes/");

                        if (isRemote)
                        {
                            head = head.Substring(head.LastIndexOf("remotes/") + 8);
                        }
                        var index = head.LastIndexOf("/");
                        remote = head.Substring(0, index);
                        head = head.Substring(index + 1);
                    }

                    if (!string.IsNullOrEmpty(filterRemote) && remote != filterRemote)
                        continue;

                    heads.Add(head);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }

            return heads;
        }

        private static bool IsValidGuid(string guid)
        {
            return guid.Trim("1234567890ABCDEFabcdef".ToCharArray()).Length == 0;
        }

        public static string[] GetFiles(string filePattern)
        {
            return RunCmd(Settings.GitCommand, "ls-files -o -m -c \"" + filePattern + "\"")
                .Split('\n');
        }

        public static List<GitItem> GetFileChanges(string file)
        {
            file = FixPath(file);
            var tree = RunCmd(Settings.GitCommand, "whatchanged --all -- \"" + file + "\"");

            var itemsStrings = tree.Split('\n');

            var items = new List<GitItem>();

            GitItem item = null;
            foreach (var itemsString in itemsStrings)
            {
                if (itemsString.StartsWith("commit "))
                {
                    item = new GitItem {CommitGuid = itemsString.Substring(7).Trim()};

                    items.Add(item);
                }
                else if (item == null)
                {
                    continue;
                }
                else if (itemsString.StartsWith("Author: "))
                {
                    item.Author = itemsString.Substring(7).Trim();
                }
                else if (itemsString.StartsWith("Date:   "))
                {
                    item.Date = itemsString.Substring(7).Trim();
                }
                else if (!itemsString.StartsWith(":") && !string.IsNullOrEmpty(itemsString))
                {
                    item.Name += itemsString.Trim() + Environment.NewLine;
                }
                else
                {
                    if (itemsString.Length > 32)
                        item.Guid = itemsString.Substring(26, 7);
                }
            }

            return items;
        }

        public static List<IGitItem> GetTree(string id)
        {
            var tree = RunCachableCmd(Settings.GitCommand, "ls-tree \"" + id + "\"");

            var itemsStrings = tree.Split('\n');

            var items = new List<IGitItem>();

            foreach (var itemsString in itemsStrings)
            {
                if (itemsString.Length <= 53)
                    continue;

                var item = new GitItem {Mode = itemsString.Substring(0, 6)};
                var guidStart = itemsString.IndexOf(' ', 7);
                item.ItemType = itemsString.Substring(7, guidStart - 7);
                item.Guid = itemsString.Substring(guidStart + 1, 40);
                item.Name = itemsString.Substring(guidStart + 42).Trim();
                item.FileName = item.Name;

                items.Add(item);
            }

            return items;
        }

        public static List<GitBlame> Blame(string filename, string from)
        {
            from = FixPath(from);
            filename = FixPath(filename);
            var itemsStrings =
                RunCmd(
                    Settings.GitCommand,
                    string.Format("blame -M -w -l \"{0}\" -- \"{1}\"", from, filename))
                    .Split('\n');

            var items = new List<GitBlame>();

            GitBlame item;
            var lastCommitGuid = "";

            var color1 = Color.Azure;
            var color2 = Color.Ivory;
            var currentColor = color1;

            foreach (var itemsString in itemsStrings)
            {
                if (itemsString.Length <= 50)
                    continue;

                var commitGuid = itemsString.Substring(0, 40).Trim();

                if (lastCommitGuid != commitGuid)
                    currentColor = currentColor == color1 ? color2 : color1;

                item = new GitBlame {color = currentColor, CommitGuid = commitGuid};
                items.Add(item);

                var codeIndex = itemsString.IndexOf(')', 41) + 1;
                if (codeIndex > 41)
                {
                    if (lastCommitGuid != commitGuid)
                        item.Author = itemsString.Substring(41, codeIndex - 41).Trim();

                    if (!string.IsNullOrEmpty(item.Text))
                        item.Text += Environment.NewLine;
                    item.Text += itemsString.Substring(codeIndex).Trim(new[] {'\r'});
                }

                lastCommitGuid = commitGuid;
            }


            return items;
        }

        public static string GetFileRevisionText(string file, string revision)
        {
            return
                RunCachableCmd(
                    Settings.GitCommand,
                    string.Format("show {0}:\"{1}\"", revision, file.Replace('\\', '/')));
        }

        public static string GetFileText(string id)
        {
            return RunCachableCmd(Settings.GitCommand, "cat-file blob \"" + id + "\"");
        }

        public static void StreamCopy(Stream input, Stream output)
        {
            int read;
            var buffer = new byte[2048];
            do
            {
                read = input.Read(buffer, 0, buffer.Length);
                output.Write(buffer, 0, read);
            } while (read > 0);
        }


        public static Stream GetFileStream(string id)
        {
            try
            {
                var newStream = new MemoryStream();

                SetEnvironmentVariable();

                Settings.GitLog.Log(Settings.GitCommand + " " + "cat-file blob \"" + id + "\"");
                //process used to execute external commands

                var process =
                    new Process
                        {
                            StartInfo =
                                {
                                    UseShellExecute = false,
                                    ErrorDialog = false,
                                    RedirectStandardOutput = true,
                                    RedirectStandardInput = false,
                                    RedirectStandardError = false,
                                    CreateNoWindow = true,
                                    FileName = "\"" + Settings.GitCommand + "\"",
                                    Arguments = "cat-file blob \"" + id + "\"",
                                    WorkingDirectory = Settings.WorkingDir,
                                    WindowStyle = ProcessWindowStyle.Normal,
                                    LoadUserProfile = true
                                }
                        };

                process.Start();

                StreamCopy(process.StandardOutput.BaseStream, newStream);
                newStream.Position = 0;

                process.WaitForExit();

                return newStream;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

            return null;
        }

        public static string GetPreviousCommitMessage(int numberBack)
        {
            return RunCmd(Settings.GitCommand, "log -n 1 HEAD~" + numberBack + " --pretty=format:%s%n%n%b");
        }

        public static string MergeBranch(string branch)
        {
            return RunCmd(Settings.GitCommand, MergeBranchCmd(branch, true));
        }

        public static string OpenWithDifftool(string filename)
        {
            var output = "";
            if (VersionInUse.GuiDiffToolExist)
                RunCmdAsync(Settings.GitCommand, "difftool --gui --no-prompt \"" + filename + "\"");
            else
                output = RunCmd(Settings.GitCommand, "difftool --no-prompt \"" + filename + "\"");
            return output;
        }

        public static string OpenWithDifftool(string filename, string revision1, string revision2)
        {
            var output = "";
            if (VersionInUse.GuiDiffToolExist)
                RunCmdAsync(Settings.GitCommand,
                            "difftool --gui --no-prompt " + revision2 + " " + revision1 + " -- \"" + filename + "\"");
            else
                output = RunCmd(Settings.GitCommand,
                                "difftool --no-prompt " + revision2 + " " + revision1 + " -- \"" + filename + "\"");
            return output;
        }

        public static string MergeBranchCmd(string branch, bool allowFastForward)
        {
            if (!allowFastForward)
                return "merge --no-ff \"" + branch + "\"";
            return "merge \"" + branch + "\"";
        }

        public static string GetFileExtension(string fileName)
        {
            if (fileName.Contains(".") && fileName.LastIndexOf(".") < fileName.Length)
                return fileName.Substring(fileName.LastIndexOf('.') + 1);

            return null;
        }
    }
}