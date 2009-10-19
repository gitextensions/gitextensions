using System;
using System.Collections.Generic;

using System.Text;
using System.Security.Permissions;
using System.IO;
using PatchApply;
using System.Diagnostics;
using System.Drawing;
using GitUIPluginInterfaces;

namespace GitCommands
{
    public class GitCommands : IGitCommands
    {
        public static string FindGitWorkingDir(string startDir)
        {
            if (string.IsNullOrEmpty(startDir))
                return "";

            if (!startDir.EndsWith("\\") && !startDir.EndsWith("/"))
                startDir += "\\";

            string dir = startDir;

            while (dir.LastIndexOfAny(new char[] { '\\', '/' }) > 0)
            {
                dir = dir.Substring(0, dir.LastIndexOfAny(new char[] { '\\', '/' }));

                if (Settings.ValidWorkingDir(dir))
                    return dir + "\\";
            }
            return startDir;
        }

        public static string RunCmd(string cmd)
        {
            return RunCmd(cmd, "");
        }

        public static void SetEnvironmentVariable()
        {
        }

        public static void RunRealCmd(string cmd, string arguments)
        {
            try
            {
                SetEnvironmentVariable();

                Settings.GitLog.Log(cmd + " " + arguments);
                //process used to execute external commands

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.ErrorDialog = false;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.RedirectStandardInput = false;
				process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
				process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.FileName = "\"" + cmd + "\"";
                process.StartInfo.Arguments = arguments;
                process.StartInfo.WorkingDirectory = Settings.WorkingDir;
                process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                process.StartInfo.LoadUserProfile = true;

                process.Start();
                process.WaitForExit();
                process.Close();
            }
            catch
            {
            }
        }

        private static string FixPath(string path)
        {
            path = path.Trim();

            if (path.StartsWith("\\\\"))
                return path;

            return path.Replace('\\', '/');
        }

        public static void RunRealCmdDetatched(string cmd, string arguments)
        {
            try
            {
                SetEnvironmentVariable();

                Settings.GitLog.Log(cmd + " " + arguments);
                //process used to execute external commands

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.ErrorDialog = false;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.RedirectStandardInput = false;
				process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
				process.StartInfo.StandardOutputEncoding = Encoding.UTF8;


                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.FileName = "\"" + cmd + "\"";
                process.StartInfo.Arguments = arguments;
                process.StartInfo.WorkingDirectory = Settings.WorkingDir;
                process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                process.StartInfo.LoadUserProfile = true;

                process.Start();
            }
            catch
            {
            }


        }

        public static void Run(string cmd, string arguments)
        {
            try
            {
                SetEnvironmentVariable();

                Settings.GitLog.Log(cmd + " " + arguments);
                //process used to execute external commands

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.ErrorDialog = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardError = true;
				process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
				process.StartInfo.StandardOutputEncoding = Encoding.UTF8;


                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.FileName = "\"" + cmd + "\"";
                process.StartInfo.Arguments = arguments;
                process.StartInfo.WorkingDirectory = Settings.WorkingDir;
                process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                process.StartInfo.LoadUserProfile = true;

                process.Start();
                //process.WaitForExit();
            }
            catch
            {
            }

        }

        public System.Diagnostics.Process Process { get; set; }

        public bool CollectOutput = true;

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public Process CmdStartProcess(string cmd, string arguments)
        {
            SetEnvironmentVariable();
            
            bool ssh = false;
            if (
                    (!Plink() &&
                        (
                        (arguments.Contains("@") && arguments.Contains("://")) ||
                        (arguments.Contains("@") && arguments.Contains(":")) ||
                        (arguments.Contains("ssh://")) ||
                        (arguments.Contains("http://")) ||
                        (arguments.Contains("git://")) ||
                        (arguments.Contains("push")) ||
                        (arguments.Contains("remote")) ||
                        (arguments.Contains("pull"))
                        )
                    ) ||
                    arguments.Contains("plink")
                )
                ssh = true;


            Kill();

            Settings.GitLog.Log(cmd + " " + arguments);

            //process used to execute external commands
            Process = new System.Diagnostics.Process();
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.ErrorDialog = false;
            Process.StartInfo.RedirectStandardOutput = true;
            Process.StartInfo.RedirectStandardInput = true;
            Process.StartInfo.RedirectStandardError = true;
			Process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
			Process.StartInfo.StandardOutputEncoding = Encoding.UTF8;

            Process.StartInfo.CreateNoWindow = (!ssh && !Settings.ShowGitCommandLine);
            Process.StartInfo.FileName = "\"" + cmd + "\"";
            Process.StartInfo.Arguments = arguments;
            Process.StartInfo.WorkingDirectory = Settings.WorkingDir;
            Process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            Process.StartInfo.LoadUserProfile = true;
            Process.EnableRaisingEvents = true;

            Process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(process_OutputDataReceived);
            Process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(process_ErrorDataReceived);
            Output = new StringBuilder();
            ErrorOutput = new StringBuilder();

            Process.Exited += new EventHandler(process_Exited);
            Process.Start();
            

            Process.BeginErrorReadLine();
            Process.BeginOutputReadLine();

            return Process;
        }

        public void Kill()
        {
            try
            {
                //If there was another process running, kill it
                if (Process != null && !Process.HasExited)
                {
                    try
                    {
                        Process.Kill();
                    }
                    catch
                    {
                    }
                    Process.Close();
                }
            }
            catch
            {
            }
        }

        public StringBuilder Output { get; set; }
        public StringBuilder ErrorOutput { get; set; }

        public event DataReceivedEventHandler DataReceived;
        public event EventHandler Exited;

        void process_Exited(object sender, EventArgs e)
        {
            if (Exited != null)
                Exited(this, e);
        }

        void process_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            //ErrorOutput.Append(e.Data + "\n");
            if (CollectOutput)
                Output.Append(e.Data + Environment.NewLine);
            if (DataReceived != null)
                DataReceived(this, e);
        }

        void process_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if (CollectOutput)
                Output.Append(e.Data + Environment.NewLine);
            if (DataReceived != null)
                DataReceived(this, e);
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public static string RunCmd(string cmd, string arguments)
        {
            string output = "";
            try
            {
                SetEnvironmentVariable();
                
                arguments = arguments.Replace("$QUOTE$", "\\\"");

                Settings.GitLog.Log(cmd + " " + arguments);
                //process used to execute external commands

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.ErrorDialog = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardError = true;
				process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
				process.StartInfo.StandardOutputEncoding = Encoding.UTF8;

                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.FileName = "\"" + cmd + "\"";
                process.StartInfo.Arguments = arguments;
                process.StartInfo.WorkingDirectory = Settings.WorkingDir;
                process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                process.StartInfo.LoadUserProfile = true;

                process.Start();

                
                string error;

                output = process.StandardOutput.ReadToEnd();
                error = process.StandardError.ReadToEnd();

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

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public static Process RunCmdAsync(string cmd, string arguments)
        {
            Process process = new System.Diagnostics.Process(); 
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
				process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
				process.StartInfo.StandardOutputEncoding = Encoding.UTF8;


                process.StartInfo.LoadUserProfile = true;
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.FileName = "\"" + cmd + "\"";
                process.StartInfo.Arguments = arguments;
                process.StartInfo.WorkingDirectory = Settings.WorkingDir;
                process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                process.StartInfo.LoadUserProfile = true;

                process.Start();
            }
            catch
            {
            }

            return process;
        }

        static public bool InTheMiddleOfConflictedMerge()
        {
            //if (Settings.UseFastChecks)
                return !string.IsNullOrEmpty(RunCmd(Settings.GitDir + "git.cmd", "ls-files --unmerged"));
            //else
            //    return RunCmd(Settings.GitDir + "git.cmd", "merge \"{95E16C63-E0D3-431f-9E87-F4B41F7EC30F}\"").Contains("fatal: You are in the middle of a conflicted merge.");
        }

        static public List<GitItem> GetConflictedFiles()
        {
            string[] unmerged = RunCmd(Settings.GitDir + "git.cmd", "ls-files --unmerged").Split('\n');

            List<GitItem> unmergedFiles = new List<GitItem>();

            
            string fileName = "";
            foreach (string file in unmerged)
            {
                if (file.LastIndexOfAny(new char[] { ' ', '\t' })> 0)
                {
                    if (file.Substring(file.LastIndexOfAny(new char[] { ' ', '\t' }) + 1) != fileName)
                    {
                        fileName = file.Substring(file.LastIndexOfAny(new char[] { ' ', '\t' }) + 1);
                        GitItem gitFile = new GitItem();
                        gitFile.FileName = fileName;


                        unmergedFiles.Add(gitFile);
                    }
                }
            }

            return unmergedFiles;
        }

        static public string GetConflictedFiles(string filename)
        {
            filename = FixPath(filename);
            string[] unmerged = RunCmd(Settings.GitDir + "git.cmd", "ls-files --unmerged \"" + filename + "\"").Split('\n');

            foreach (string file in unmerged)
            {
                string[] fileline = file.Split(new char[] { ' ', '\t' });
                if (fileline.Length < 3)
                    continue;
                if (fileline[2].Trim() == "1")
                {
                    string newFileName = filename + ".BASE";
                    RunCmd(Settings.GitDir + "git.cmd", "cat-file blob \"" + fileline[1] + "\" > \"" + newFileName + "\"");
                }
                if (fileline[2].Trim() == "2")
                {
                    string newFileName = filename + ".LOCAL";
                    RunCmd(Settings.GitDir + "git.cmd", "cat-file blob \"" + fileline[1] + "\" > \"" + newFileName + "\"");
                }
                if (fileline[2].Trim() == "3")
                {
                    string newFileName = filename + ".REMOTE";
                    RunCmd(Settings.GitDir + "git.cmd", "cat-file blob \"" + fileline[1] + "\" > \"" + newFileName + "\"");
                }
            }

            return filename;
        }

        static public string GetMergeMessage()
        {
            string file = Settings.WorkingDir + ".git\\MERGE_MSG";

            if (File.Exists(file))
                return File.ReadAllText(file);

            return "";
        }

        static public void RunGitK()
        {
            Run("cmd.exe", "/c \"\"" + Settings.GitDir + "gitk\" --all\"");
            //Run(Settings.GitDir + "gitk", "");
        }

        static public void RunGui()
        {
            Run("cmd.exe", "/c \"\"" + Settings.GitDir + "git.cmd\" gui\"");
        }


        static public void RunBash()
        {
            RunRealCmdDetatched("cmd.exe", "/c \"\"" + Settings.GitBinDir + "sh.exe\" --login -i\"");
        }

        static public string Init(bool bare, bool shared)
        {
            if (bare && shared)
                return RunCmd(Settings.GitDir + "git.cmd", "init --bare --shared=all");
            else 
            if (bare)
                return RunCmd(Settings.GitDir + "git.cmd", "init --bare");
            else
                return RunCmd(Settings.GitDir + "git.cmd", "init");
        }

        static public string CherryPick(string cherry, bool commit)
        {
            if (commit)
                return RunCmd(Settings.GitDir + "git.cmd", "cherry-pick \"" + cherry + "\"");
            else
                return RunCmd(Settings.GitDir + "git.cmd", "cherry-pick --no-commit \"" + cherry + "\"");
        }

        static public string ShowSha1(string sha1)
        {
            return RunCmd(Settings.GitDir + "git.cmd", "show " + sha1);
        }

        static public string GetCommitInfo(string sha1)
        {
            return RunCmd(Settings.GitDir + "git.cmd", "show -s --pretty=format:\"Author:\t\t%aN%nDate:\t\t%cr (%cd)%nCommit hash:\t%H%n%n%s%n%b\" " + sha1);
        }

        static public string UserCommitCount()
        {
            return RunCmd(Settings.GitDir + "git.cmd", "shortlog -s -n --email ");
        }

        static public string DeleteBranch(string branchName, bool force)
        {
            return RunCmd(Settings.GitDir + "git.cmd", DeleteBranchCmd(branchName, force));
        }

        public static string DeleteBranchCmd(string branchName, bool force)
        {
            if (force)
                return "branch -D \"" + branchName + "\"";
            else
                return "branch -d \"" + branchName + "\"";
        }

        static public string DeleteTag(string tagName)
        {
            return RunCmd(Settings.GitDir + "git.cmd", DeleteTagCmd(tagName));
        }

        public static string DeleteTagCmd(string tagName)
        {
            return "tag -d \"" + tagName + "\"";
        }

        static public string GetCurrentCheckout()
        {
            return RunCmd(Settings.GitDir + "git.cmd", "log -g -1 HEAD --pretty=format:%H");
        }

        static public int CommitCount()
        {
            int count;
            if (int.TryParse(RunCmd(Settings.GitDir + "cmd.exe", "/c \"\"" + Settings.GitDir + "git.cmd\" rev-list --all --abbrev-commit | wc -l\""), out count))
                return count;

            return 0;
        }

        static public string GetSubmoduleRemotePath(string name)
        {
            return RunCmd(Settings.GitDir + "git.cmd", "config -f .gitmodules --get submodule." + name.Trim() + ".url");
        }

        static public string GetSubmoduleLocalPath(string name)
        {
            return RunCmd(Settings.GitDir + "git.cmd", "config -f .gitmodules --get submodule." + name.Trim() + ".path").Trim();
        }

        static public string SubmoduleInitCmd(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "submodule update --init";

            return "submodule update --init \"" + name.Trim() + "\"";
        }

        static public string SubmoduleUpdateCmd(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "submodule update";

            return "submodule update \"" + name.Trim() + "\"";
        }

        static public string SubmoduleSyncCmd(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "submodule sync";

            return "submodule sync \"" + name.Trim() + "\"";
        }

        static public string AddSubmoduleCmd(string remotePath, string localPath, string branch)
        {
            remotePath = FixPath(remotePath);
            localPath = FixPath(localPath);

            if (!string.IsNullOrEmpty(branch))
                branch = " \"" + branch.Trim() + "\"";

            return "submodule add \"" + remotePath.Trim() + "\" \"" + localPath.Trim() + "\"" + branch;
        }

        public IList<IGitSubmodule> GetSubmodules()
        {
            string[] submodules = RunCmd(Settings.GitDir + "git.cmd", "submodule status").Split('\n');

            IList<IGitSubmodule> submoduleList = new List<IGitSubmodule>();

            string lastLine = null;

            foreach (string submodule in submodules)
            {
                if (submodule.Length < 43)
                    continue;

                if (submodule.Equals(lastLine))
                    continue;

                lastLine = submodule;

                GitSubmodule gitSubmodule = new GitSubmodule();
                gitSubmodule.Initialized = submodule[0] != '-';
                gitSubmodule.UpToDate = submodule[0] != '+';
                gitSubmodule.CurrentCommitGuid = submodule.Substring(1, 40).Trim();
                string name = submodule.Substring(42).Trim();
                if (name.Contains("("))
                {
                    gitSubmodule.Name = name.Substring(0, name.IndexOf("("));
                    gitSubmodule.Branch = name.Substring(name.IndexOf("(")).Trim(new char[] { '(', ')', ' ' });
                }
                else
                    gitSubmodule.Name = name;
                submoduleList.Add(gitSubmodule);
            }

            return submoduleList;
        }

        static public string Stash()
        {
            return RunCmd(Settings.GitDir + "git.cmd", "stash save");
        }

        static public string StashApply()
        {
            return RunCmd(Settings.GitDir + "git.cmd", "stash apply");
        }

        static public string StashClear()
        {
            return RunCmd(Settings.GitDir + "git.cmd", "stash clear");
        }

        static public string RevertCmd(string commit, bool autoCommit)
        {
            if (autoCommit)
                return "revert " + commit;
            else
                return "revert --no-commit " + commit;
        }


        static public string ResetSoft(string commit)
        {
            return ResetSoft(commit, "");
        }

        static public string ResetMixed(string commit)
        {
            return ResetMixed(commit, "");
        }

        static public string ResetHard(string commit)
        {
            return ResetHard(commit, "");
        }

        static public string ResetSoft(string commit, string file)
        {
            string args = "reset --soft";

            if (!string.IsNullOrEmpty(commit))
                args += " \"" + commit + "\"";

            if (!string.IsNullOrEmpty(file))
                args += " -- \"" + file + "\"";

            return RunCmd(Settings.GitDir + "git.cmd", args);
        }

        static public string ResetMixed(string commit, string file)
        {
            string args = "reset --mixed";

            if (!string.IsNullOrEmpty(commit))
                args += " \"" + commit + "\"";

            if (!string.IsNullOrEmpty(file))
                args += " -- \"" + file + "\"";

            return RunCmd(Settings.GitDir + "git.cmd", args);
        }

        static public string ResetHard(string commit, string file)
        {
            string args = "reset --hard";
            
            if (!string.IsNullOrEmpty(commit))
                args +=  " \"" + commit + "\"";

            if (!string.IsNullOrEmpty(file))
                args += " -- \"" + file + "\"";

            return RunCmd(Settings.GitDir + "git.cmd", args);
        }

        static public string ResetSoftCmd(string commit)
        {
            return "reset --soft \"" + commit + "\"";
        }

        static public string ResetMixedCmd(string commit)
        {
            return "reset --mixed \"" + commit + "\"";
        }

        static public string ResetHardCmd(string commit)
        {
            return "reset --hard \"" + commit + "\"";
        }

        static public string CloneCmd(string from, string to, bool central)
        {
            from = FixPath(from);
            to = FixPath(to);

            if (central)
                return "clone -v --bare \"" + from.Trim() + "\" \"" + to.Trim() + "\"";
            else
                return "clone -v \"" + from.Trim() + "\" \"" + to.Trim() + "\"";

        }

        static public string ResetFile(string file)
        {
            file = FixPath(file);
            return RunCmd(Settings.GitDir + "git.cmd", "checkout-index --index --force -- \"" + file + "\"");
        }


        public string FormatPatch(string from, string to, string output, int start)
        {
            output = FixPath(output);

            string result = RunCmd(Settings.GitDir + "git.cmd", "format-patch -M -C -B --start-number " + start + " \"" + from + "\"..\"" + to + "\" -o \"" + output + "\"");

            return result;
        }

        public string FormatPatch(string from, string to, string output)
        {
            output = FixPath(output);

            string result = RunCmd(Settings.GitDir + "git.cmd", "format-patch -M -C -B \"" + from + "\"..\"" + to + "\" -o \"" + output + "\"");

            return result;
        }


        static public string Tag(string tagName, string revision)
        {
            string result = GitCommands.RunCmd(Settings.GitDir + "git.cmd", "tag \"" + tagName.Trim() + "\" \"" + revision + "\"");

            return result;
        }

        static public string Branch(string branchName, string revision, bool checkout)
        {
            string result = GitCommands.RunCmd(Settings.GitDir + "git.cmd", BranchCmd(branchName, revision, checkout));

            return result;
        }

        public static string BranchCmd(string branchName, string revision, bool checkout)
        {
            if (checkout)
                return "checkout -b \"" + branchName.Trim() + "\" \"" + revision + "\"";
            else
                return "branch \"" + branchName.Trim() + "\" \"" + revision + "\"";
        }

        static public void UnSetSsh()
        {
            Environment.SetEnvironmentVariable("GIT_SSH", "", EnvironmentVariableTarget.Process);
        }

        static public void SetSsh(string path)
        {
            Environment.SetEnvironmentVariable("GIT_SSH", path, EnvironmentVariableTarget.Process);
        }

        static public bool Plink()
        {
            string sshString = GetSsh();
            return string.Compare(sshString, sshString.LastIndexOfAny(new char[] { '\\', '/' }) + 1, "plink.exe", 0, 9, true) == 0;
            //return GetSsh().Contains("plink");
        }

        static public string GetSsh()
        {
            string ssh = Environment.GetEnvironmentVariable("GIT_SSH", EnvironmentVariableTarget.Process);
            
            if (ssh == null)
                return "";
            
            return ssh;
        }

        static public string Push(string path)
        {
            path = FixPath(path);

            string result = GitCommands.RunCmd(Settings.GitDir + "git.cmd", "push \"" + path.Trim() + "\"");

            return result;
        }

        static public Process PushAsync(string path, string branch, bool all)
        {
            path = FixPath(path);

            return RunCmdAsync("cmd.exe", " /k \"\"" + Settings.GitDir + "git.cmd\" " + PushCmd(path, branch, all) + "\"");
        }


        static public bool StartPageantForRemote(string remote)
        {
            string sshKeyFile = GetPuttyKeyFileForRemote(remote);
            if (!string.IsNullOrEmpty(sshKeyFile))
            {
                StartPageantWithKey(sshKeyFile);
                return true;
            }
            return false;
        }

        public static void StartPageantWithKey(string sshKeyFile)
        {
            GitCommands.Run(Settings.Pageant, "\"" + sshKeyFile + "\"");
        }

        public static string GetPuttyKeyFileForRemote(string remote)
        {
            if (!string.IsNullOrEmpty(remote) && 
                !string.IsNullOrEmpty(Settings.Pageant) && 
                Settings.AutoStartPageant &&
                Plink())
            {
                return GetSetting("remote." + remote + ".puttykeyfile");
            }
            return "";
        }

        public static string PushCmd(string path, string branch, bool all)
        {
            path = FixPath(path);

            branch = branch.Replace(" ", "");

            if (all)
                return "push --all \"" + path.Trim() + "\"";
            else
                if (!string.IsNullOrEmpty(branch))
                    return "push \"" + path.Trim() + "\" " + branch;


            return "push \"" + path.Trim() + "\"";
        }

        public static string PushTagCmd(string path, string tag, bool all)
        {
            path = FixPath(path);

            tag = tag.Replace(" ", "");

            if (all)
                return "push \"" + path.Trim() + "\" --tags";
            else
                if (!string.IsNullOrEmpty(tag))
                    return "push \"" + path.Trim() + "\" tag " + tag;

            return "";
        }

        static public string Fetch(string remote, string branch)
        {
            remote = FixPath(remote);

            Directory.SetCurrentDirectory(Settings.WorkingDir);
            branch = FetchCmd(remote, branch);

            GitCommands.RunRealCmd("cmd.exe", " /k \"\"" + Settings.GitDir + "git.cmd\" " + FetchCmd(remote, branch) + "\"");

            return "Done";
        }

        public static bool PathIsUrl(string path)
        {
            if (path.Contains("\\") || path.Contains("/"))
                return true;

            return false;
        }

        public static string FetchCmd(string remote, string branch)
        {
            remote = FixPath(remote);

            branch = branch.Replace(" ", "");

            string localbranch;

            if (string.IsNullOrEmpty(branch))
                localbranch = "";
            else
                localbranch = "+refs/heads/" + branch + "";

            string remotebranch;
            string remoteUrl = GetSetting("remote." + remote + ".url");

            if (PathIsUrl(remote) && !string.IsNullOrEmpty(branch) && string.IsNullOrEmpty(remoteUrl))
                remotebranch = ":refs/heads/" + branch + "";
            else
            if (PathIsUrl(remote) || string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(remoteUrl))
                remotebranch = "";
            else
                remotebranch = ":" + "refs/remotes/" + remote.Trim() + "/" + branch + "";


            return "fetch \"" + remote.Trim() + "\" " + localbranch + remotebranch;
        }



        static public string Pull(string remote, string branch, bool rebase)
        {
            remote = FixPath(remote);

            Directory.SetCurrentDirectory(Settings.WorkingDir);

            GitCommands.RunRealCmd("cmd.exe", " /k \"\"" + Settings.GitDir + "git.cmd\" " + PullCmd(remote, branch, rebase) + "\"");

            return "Done";
        }

        public static string PullCmd(string remote, string branch, bool rebase)
        {
            remote = FixPath(remote);

            branch = branch.Replace(" ", "");

            string rebaseOption = "";
            if (rebase)
                rebaseOption = "--rebase ";

            string localbranch;

            if (string.IsNullOrEmpty(branch))
                localbranch = "";
            else
                localbranch = "+refs/heads/" + branch + "";

            string remotebranch;
            string remoteUrl = GetSetting("remote." + remote + ".url");

            if (PathIsUrl(remote) && !string.IsNullOrEmpty(branch) && string.IsNullOrEmpty(remoteUrl))
                remotebranch = ":refs/heads/" + branch + "";
            else
            if (string.IsNullOrEmpty(branch)  || PathIsUrl(remote) || string.IsNullOrEmpty(remoteUrl))
                remotebranch = "";
            else
                remotebranch = ":" + "refs/remotes/" + remote.Trim() + "/" + branch + "";


            return "pull " + rebaseOption + "\"" + remote.Trim() + "\" " + localbranch + remotebranch;
        }

        static public string ContinueRebase()
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            string result = GitCommands.RunCmd(Settings.GitDir + "git.cmd", ContinueRebaseCmd());

            return result;
        }

        public static string ContinueRebaseCmd()
        {
            return "rebase --continue";
        }

        static public string SkipRebase()
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            string result = GitCommands.RunCmd(Settings.GitDir + "git.cmd", SkipRebaseCmd());

            return result;
        }

        public static string SkipRebaseCmd()
        {
            return "rebase --skip";
        }

        static public string GetRebaseDir()
        {
            if (Directory.Exists(Settings.WorkingDir + ".git\\rebase-apply\\")) return Settings.WorkingDir + ".git\\rebase-apply\\";
            if (Directory.Exists(Settings.WorkingDir + ".git\\rebase\\")) return Settings.WorkingDir + ".git\\rebase\\";
            return "";

        }

        static public bool InTheMiddleOfRebase()
        {
            if (!File.Exists(GetRebaseDir() + "applying") &&
                 Directory.Exists(GetRebaseDir())) return true;

            return false;
        }

        static public bool InTheMiddleOfPatch()
        {
            if (!File.Exists(GetRebaseDir() + "rebasing") &&
                 Directory.Exists(GetRebaseDir())) return true;

            return false;
        }


        static public string GetNextRebasePatch()
        {
            string file = GetRebaseDir() + "next";
            if (File.Exists(file))
                return File.ReadAllText(file).Trim();

            return "";
        }

        static public List<PatchFile> GetRebasePatchFiles()
        {
            List<PatchFile> patchFiles = new List<PatchFile>();

            string nextFile = GetNextRebasePatch();

            int next = 0;
            int.TryParse(nextFile, out next);


            string[] files = new string[0];
            if (Directory.Exists(GetRebaseDir()))
                files = Directory.GetFiles(GetRebaseDir());

            foreach (string fullFileName in files)
            {
                int n = 0;
                string file = fullFileName.Substring(fullFileName.LastIndexOf('\\') + 1);
                if (int.TryParse(file, out n))
                {
                    PatchFile patchFile = new PatchFile();
                    patchFile.Name = file;
                    patchFile.FullName = fullFileName;
                    patchFile.IsNext = n == next;
                    patchFile.IsSkipped = n < next;

                    if (File.Exists(GetRebaseDir() + file))
                    {
                        foreach (string line in File.ReadAllLines(GetRebaseDir() + file))
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
            }

            return patchFiles;
        }

        static public string Rebase(string branch)
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            string result = GitCommands.RunCmd(Settings.GitDir + "git.cmd", RebaseCmd(branch));

            return result;
        }

        public static string RebaseCmd(string branch)
        {
            return "rebase \"" + branch + "\"";
        }


        static public string AbortRebase()
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            string result = GitCommands.RunCmd(Settings.GitDir + "git.cmd", AbortRebaseCmd());

            return result;
        }

        public static string AbortRebaseCmd()
        {
            return "rebase --abort";
        }

        static public string Resolved()
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            string result = GitCommands.RunCmd(Settings.GitDir + "git.cmd", ResolvedCmd());

            return result;
        }

        public static string ResolvedCmd()
        {
            return "am --3way --resolved";
        }

        static public string Skip()
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            string result = GitCommands.RunCmd(Settings.GitDir + "git.cmd", SkipCmd());

            return result;
        }

        public static string SkipCmd()
        {
            return "am --3way --skip";
        }

        static public string Abort()
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            string result = GitCommands.RunCmd(Settings.GitDir + "git.cmd", AbortCmd());

            return result;
        }

        public static string AbortCmd()
        {
            return "am --3way --abort";
        }

        static public string Commit(bool amend)
        {
            return GitCommands.RunCmd(Settings.GitDir + "git.cmd", CommitCmd(amend));
        }

        static public string CommitCmd(bool amend)
        {
            //message = message.Replace('\"', '\'');

            if (amend)
                return "commit --amend -F \"" + Settings.WorkingDirGitDir() + "\\COMMITMESSAGE\"";
            else
                return "commit  -F \"" + Settings.WorkingDirGitDir() + "\\COMMITMESSAGE\"";
        }

        static public string Patch(string patchFile)
        {
            patchFile = FixPath(patchFile);

            Directory.SetCurrentDirectory(Settings.WorkingDir);

            string result = GitCommands.RunCmd(Settings.GitDir + "git.cmd", PatchCmd(patchFile));

            return result;
        }

        public static string PatchCmd(string patchFile)
        {
            patchFile = FixPath(patchFile);

            return "am --3way --signoff \"" + patchFile + "\"";
        }

        public static string UpdateRemotes()
        {
            return RunCmd(Settings.GitDir + "git.cmd", "remote update");
        }


        public static string RemoveRemote(string name)
        {
            return RunCmd(Settings.GitDir + "git.cmd", "remote rm \"" + name + "\"");
        }

        public static string RenameRemote(string name, string newname)
        {
            return RunCmd(Settings.GitDir + "git.cmd", "remote rename \"" + name + "\" \"" + newname + "\"");
        }

        public static string AddRemote(string name, string location)
        {
            location = FixPath(location);

            if (string.IsNullOrEmpty(name))
                return "Please enter a name.";

            if (string.IsNullOrEmpty(location))
                return RunCmd(Settings.GitDir + "git.cmd", "remote add \"" + name + "\" \"\"");
            else
                return RunCmd(Settings.GitDir + "git.cmd", "remote add \"" + name + "\" \"" + location + "\"");
        }

        public static string[] GetRemotes()
        {
            return RunCmd(Settings.GitDir + "git.cmd", "remote show").Split('\n');
        }

        public string GetGlobalSetting(string setting)
        {
            return RunCmd(Settings.GitDir + "git.cmd", "config --global --get \"" + setting + "\"").Trim();
        }

        public void SetGlobalSetting(string setting, string value)
        {
            if (!string.IsNullOrEmpty(value) && !value.Contains("git.exe' is not"))
            {

                value = value.Replace("\"", "$QUOTE$");
                value = FixPath(value);
                value = value.Replace("$QUOTE$", "\\\"");

                GitCommands.RunCmd(Settings.GitDir + "git.cmd", "config --global --replace-all \"" + setting + "\" \"" + value.Trim() + "\"").Trim();
            }
            else
            {
                GitCommands.RunCmd(Settings.GitDir + "git.cmd", "config --global --unset-all \"" + setting + "\"").Trim();
            }
        }

        static public string GetSetting(string setting)
        {
            string configFileName = Settings.WorkingDirGitDir() + "\\config";
            if (!File.Exists(configFileName))
                return "";

            return GitCommands.RunCmd(Settings.GitDir + "git.cmd", "config --file \"" + configFileName + "\" --get \"" + setting + "\"").Trim();
        }

        static public void UnSetSetting(string setting)
        {
            string configFileName = Settings.WorkingDirGitDir() + "\\config";
            GitCommands.RunCmd(Settings.GitDir + "git.cmd", "config --file \"" + configFileName + "\" --unset-all \"" + setting + "\"").Trim();
        }

        static public void SetSetting(string setting, string value)
        {
            string configFileName = Settings.WorkingDirGitDir() + "\\config";
            if (!File.Exists(configFileName))
                return;

            if (!string.IsNullOrEmpty(value) && !value.Contains("git.exe' is not"))
            {
                value = value.Replace("\"", "$QUOTE$");
                value = FixPath(value);
                value = value.Replace("$QUOTE$", "\"");

                GitCommands.RunCmd(Settings.GitDir + "git.cmd", "config --file \"" + configFileName + "\" --replace-all  \"" + setting + "\" \"" + value.Trim() + "\"").Trim();
            }
            else
            {
                UnSetSetting(setting);
            }
        }

        static public List<Patch> GetStashedItems(string stashName)
        {
            PatchManager patchManager = new PatchManager();
            patchManager.LoadPatch(GitCommands.RunCmd(Settings.GitDir + "git.cmd", "stash show -p " + stashName), false);

            return patchManager.patches;
        }

        static public List<GitStash> GetStashes()
        {
            string [] list = GitCommands.RunCmd(Settings.GitDir + "git.cmd", "stash list").Split('\n');


            List<GitStash> stashes = new List<GitStash>();
            foreach (string stashString in list)
            {
                if (stashString.IndexOf(':') > 0)
                {
                    GitStash stash = new GitStash();
                    stash.Name = stashString.Substring(0, stashString.IndexOf(':')).Trim();

                    if (stashString.IndexOf(':') + 1 < stashString.Length)
                        stash.Message = stashString.Substring(stashString.IndexOf(':') + 1).Trim();

                    stashes.Add(stash);
                }
            }

            return stashes;
        }

        static public Patch GetSingleDiff(string from, string to, string filter)
        {
            filter = FixPath(filter);
            from = FixPath(from);
            to = FixPath(to);

            PatchManager patchManager = new PatchManager();
            patchManager.LoadPatch(GitCommands.RunCmd(Settings.GitDir + "git.cmd", "diff --ignore-submodules \"" + to + "\" \"" + from + "\" -- \"" + filter + "\""), false);

            if (patchManager.patches.Count > 0)
                return patchManager.patches[0];

            return null;
        }

        static public List<Patch> GetDiff(string from, string to)
        {
            PatchManager patchManager = new PatchManager();
            patchManager.LoadPatch(GitCommands.RunCmd(Settings.GitDir + "git.cmd", "diff \"" + from + "\" \"" + to + "\""), false);

            return patchManager.patches;
        }

        static public List<string> GetDiffFiles(string from)
        {
            string result = RunCmd(Settings.GitDir + "git.cmd", "diff --name-only \"" + from + "\"");

            string[] files = result.Split('\n');

            List<string> retVal = new List<string>();
            foreach (string s in files)
            {
                if (!string.IsNullOrEmpty(s))
                    retVal.Add(s);
            }

            return retVal;
        }

        static public List<string> GetDiffFiles(string from, string to)
        {
            string result = RunCmd(Settings.GitDir + "git.cmd", "diff --name-only \"" + from + "\" \"" + to + "\"");

            string[] files = result.Split('\n');

            List<string> retVal = new List<string>();
            foreach (string s in files)
            {
                if (!string.IsNullOrEmpty(s))
                    retVal.Add(s);
            }

            return retVal;
        }

        static public List<GitItemStatus> GetUntrackedFiles()
        {
            string status = RunCmd(Settings.GitDir + "git.cmd", "ls-files --others --directory --no-empty-directory --exclude-standard");

            string[] statusStrings = status.Split('\n');

            List<GitItemStatus> gitItemStatusList = new List<GitItemStatus>();

            foreach (string statusString in statusStrings)
            {
                if (string.IsNullOrEmpty(statusString.Trim()))
                    continue;
                GitItemStatus itemStatus = new GitItemStatus();
                itemStatus.IsNew = true;
                itemStatus.IsChanged = false;
                itemStatus.IsDeleted = false;
                itemStatus.IsTracked = false;
                itemStatus.Name = statusString.Trim();
                gitItemStatusList.Add(itemStatus);
            }

            return gitItemStatusList;
        }

        static public List<GitItemStatus> GetModifiedFiles()
        {
            string status = RunCmd(Settings.GitDir + "git.cmd", "ls-files --modified --exclude-standard");

            string[] statusStrings = status.Split('\n');

            List<GitItemStatus> gitItemStatusList = new List<GitItemStatus>();

            foreach (string statusString in statusStrings)
            {
                if (string.IsNullOrEmpty(statusString.Trim()))
                    continue;
                GitItemStatus itemStatus = new GitItemStatus();
                itemStatus.IsNew = false;
                itemStatus.IsChanged = true;
                itemStatus.IsDeleted = false;
                itemStatus.IsTracked = true;
                itemStatus.Name = statusString.Trim();
                gitItemStatusList.Add(itemStatus);
            }

            return gitItemStatusList;
        }

        public const string GetAllChangedFilesCmd = "ls-files --deleted --modified --others --no-empty-directory --exclude-standard -t";

        static public List<GitItemStatus> GetAllChangedFiles()
        {
            string status = RunCmd(Settings.GitDir + "git.cmd", GetAllChangedFilesCmd);

            return GetAllChangedFilesFromString(status);
        }

        public static List<GitItemStatus> GetAllChangedFilesFromString(string status)
        {
            string[] statusStrings = status.Split('\n');

            List<GitItemStatus> gitItemStatusList = new List<GitItemStatus>();

            GitItemStatus itemStatus = null;
            foreach (string statusString in statusStrings)
            {
                if (string.IsNullOrEmpty(statusString.Trim()) || statusString.Length < 2)
                    continue;

                if (!(itemStatus != null && itemStatus.Name == statusString.Substring(1).Trim()))
                {
                    itemStatus = new GitItemStatus();
                    itemStatus.Name = statusString.Substring(1).Trim();
                    gitItemStatusList.Add(itemStatus);
                }

                itemStatus.IsNew = itemStatus.IsNew || statusString[0] == '?';
                itemStatus.IsChanged = itemStatus.IsChanged || statusString[0] == 'C';
                itemStatus.IsDeleted = itemStatus.IsDeleted || statusString[0] == 'R';
                itemStatus.IsTracked = itemStatus.IsTracked || statusString[0] != '?';
            }

            return gitItemStatusList;
        }

        static public List<GitItemStatus> GetDeletedFiles()
        {
            string status = RunCmd(Settings.GitDir + "git.cmd", "ls-files --deleted --exclude-standard");

            string[] statusStrings = status.Split('\n');

            List<GitItemStatus> gitItemStatusList = new List<GitItemStatus>();

            foreach (string statusString in statusStrings)
            {
                if (string.IsNullOrEmpty(statusString.Trim()))
                    continue; 
                GitItemStatus itemStatus = new GitItemStatus();
                itemStatus.IsNew = false;
                itemStatus.IsChanged = false;
                itemStatus.IsDeleted = true;
                itemStatus.IsTracked = true;
                itemStatus.Name = statusString.Trim();
                gitItemStatusList.Add(itemStatus);
            }

            return gitItemStatusList;
        }

        static public bool FileIsStaged(string filename)
        {
            string status = RunCmd(Settings.GitDir + "git.cmd", "diff --cached --numstat -- \"" + filename + "\"");
            if (string.IsNullOrEmpty(status))
                return false;
            return 
                true;
        }

        static public List<GitItemStatus> GetStagedFiles()
        {
            string status = RunCmd(Settings.GitDir + "git.cmd", "diff --cached --name-status");

            List<GitItemStatus> gitItemStatusList = new List<GitItemStatus>();

            if (status.Length < 50 && status.Contains("fatal: No HEAD commit to compare"))
            {
                status = RunCmd(Settings.GitDir + "git.cmd", "status --untracked-files=no");
                    
                string[] statusStrings = status.Split('\n');

                foreach (string statusString in statusStrings)
                {
                    if (statusString.StartsWith("#\tnew file:"))
                    {
                        GitItemStatus itemStatus = new GitItemStatus();
                        itemStatus.IsNew = true;
                        itemStatus.IsTracked = true;
                        itemStatus.Name = statusString.Substring(statusString.LastIndexOf(':') + 1).Trim();
                        gitItemStatusList.Add(itemStatus);
                    }
                }
            }
            else
            {
                string[] statusStrings = status.Split('\n');

                foreach (string statusString in statusStrings)
                {
                    if (string.IsNullOrEmpty(statusString.Trim()))
                        continue;
                    GitItemStatus itemStatus = new GitItemStatus();
                    itemStatus.IsTracked = true;
                    itemStatus.Name = statusString.Substring(1).Trim();

                    itemStatus.IsNew = itemStatus.IsNew || statusString[0] == 'A';
                    itemStatus.IsChanged = itemStatus.IsChanged || statusString[0] == 'M';
                    itemStatus.IsDeleted = itemStatus.IsDeleted || statusString[0] == 'D';
                    itemStatus.IsTracked = itemStatus.IsTracked || statusString[0] != '?';

                    gitItemStatusList.Add(itemStatus);
                }
            }

            return gitItemStatusList;
        }

        static public List<GitItemStatus> GitStatus()
        {
            return GitStatus(true);
        }

        static public List<GitItemStatus> GitStatus(bool untracked)
        {
            string status;
            if (untracked)
                status = RunCmd(Settings.GitDir + "git.cmd", "status --untracked=all");
            else
                status = RunCmd(Settings.GitDir + "git.cmd", "status");

            string[] statusStrings = status.Split('\n');

            List<GitItemStatus> gitItemStatusList = new List<GitItemStatus>();

            foreach (string statusString in statusStrings)
            {
                if (statusString.StartsWith("#\tnew file:"))
                {
                    GitItemStatus itemStatus = new GitItemStatus();
                    itemStatus.IsNew = true;
                    itemStatus.IsTracked = true;
                    itemStatus.Name = statusString.Substring(statusString.LastIndexOf(':') + 1).Trim();
                    gitItemStatusList.Add(itemStatus);
                } else
                if (statusString.StartsWith("#\tdeleted:"))
                {
                    GitItemStatus itemStatus = new GitItemStatus();
                    itemStatus.IsDeleted = true;
                    itemStatus.IsTracked = true;
                    itemStatus.Name = statusString.Substring(statusString.LastIndexOf(':')+1).Trim();
                    gitItemStatusList.Add(itemStatus);
                } else
                    if (statusString.StartsWith("#\tmodified:"))
                {
                    GitItemStatus itemStatus = new GitItemStatus();
                    itemStatus.IsChanged = true;
                    itemStatus.IsTracked = true;
                    itemStatus.Name = statusString.Substring(statusString.LastIndexOf(':') + 1).Trim();
                    gitItemStatusList.Add(itemStatus);
                } else
                if (statusString.StartsWith("#\t"))
                {
                    GitItemStatus itemStatus = new GitItemStatus();
                    itemStatus.IsNew = true;
                    itemStatus.Name = statusString.Substring(2).Trim();
                    gitItemStatusList.Add(itemStatus);
                }
            }

            return gitItemStatusList;
        }

        static public string GetCurrentChanges(string name, bool staged)
        {
            name = FixPath(name);
            if (staged)
                return RunCmd(Settings.GitDir + "git.cmd", "diff --cached -- \"" + name + "\"");
            else
                return RunCmd(Settings.GitDir + "git.cmd", "diff -- " + name);
        }

        static public List<GitRevision> GitRevisionGraph()
        {
            return GetRevisionGraph(RunCmd(Settings.GitDir + "git.cmd", "log -" + Settings.MaxCommits.ToString() + " --graph --all --pretty=format:\"Commit %H %nTree:   %T%nAuthor: %aN %nDate:   %cd%nParents:%P %n%s\""));
        }

        static public List<GitRevision> GetRevisionGraph(string tree)
        {
            List<GitHead> heads = GetHeads(true);

            string[] itemsStrings = tree.Split('\n');

            List<GitRevision> revisions = new List<GitRevision>();

            char[] graphChars = new char[] { '*', '|', '*', '\\', '/' };

            for (int n = 0; n < itemsStrings.Length; )
            {
                GitRevision revision = new GitRevision();

                string line;

                line = itemsStrings[n];
                int graphIndex = 0;
                if (line.IndexOf("Commit ") > 0)
                {
                    graphIndex = line.IndexOf("Commit ");
                    if (line.LastIndexOfAny(graphChars) >= 0)
                        revision.GraphLines.Add(line.Substring(0, graphIndex));
                    revision.Name = revision.Guid = line.Substring(line.LastIndexOf("Commit ") + 7).Trim();
                    n++;
                    if (itemsStrings.Length == n) break;
                }
                line = itemsStrings[n];

                if (line.IndexOf("Tree:   ") > 0)
                {
                    revision.TreeGuid = line.Substring(line.LastIndexOf("Tree:   ") + 8).Trim();
                    if (line.LastIndexOfAny(graphChars) >= 0)
                        revision.GraphLines.Add(line.Substring(0, graphIndex));
                    n++;
                }
                line = itemsStrings[n];

                if (line.IndexOf("Merge: ") > 0)
                {
                    //ignore
                    if (line.LastIndexOfAny(graphChars) >= 0)
                        revision.GraphLines.Add(line.Substring(0, graphIndex));
                    n++;
                }
                line = itemsStrings[n];

                if (line.IndexOf("Author: ") > 0)
                {
                    revision.Author = line.Substring(line.LastIndexOf("Author: ") + 8).Trim();
                    if (line.LastIndexOfAny(graphChars) >= 0)
                        revision.GraphLines.Add(line.Substring(0, graphIndex));
                    n++;
                    if (itemsStrings.Length == n) break;
                }
                line = itemsStrings[n];

                if (line.IndexOf("Date:   ") > 0)
                {
                    revision.Date = line.Substring(line.LastIndexOf("Date:   ") + 8).Trim();
                    if (line.LastIndexOfAny(graphChars) >= 0)
                        revision.GraphLines.Add(line.Substring(0, graphIndex));
                    n++;
                    if (itemsStrings.Length == n) break;
                }
                line = itemsStrings[n];

                if (line.IndexOf("Parents:") > 0)
                {
                    List<string> parentGuids = new List<string>();
                    foreach (string s in line.Substring(line.LastIndexOf("Parents:") + 8).Split(' '))
                    {
                        parentGuids.Add(s.Trim());
                    }

                    revision.ParentGuids = parentGuids;
                    if (line.LastIndexOfAny(graphChars) >= 0)
                        revision.GraphLines.Add(line.Substring(0, graphIndex));
                    n++;
                    if (itemsStrings.Length == n) break;
                }
                line = itemsStrings[n];

                List<GitHead> foundHeads = new List<GitHead>();

                foreach (GitHead h in heads)
                {
                    if (h.Guid == revision.Guid)
                    {
                        foundHeads.Add(h);
                    }
                }

                foreach (var head in foundHeads)
                {
                    revision.Heads.Add(head);
                }

                while (!(line.Length == line.LastIndexOf("Commit ") + 7 + 40) || (line.LastIndexOf("Commit ") < 0))
                {
                    if (line.Length > graphIndex)
                        revision.Message += line.Substring(graphIndex).Trim() + Environment.NewLine;
                    if (line.LastIndexOfAny(graphChars) >= 0)
                        revision.GraphLines.Add(line.Substring(0, graphIndex));
                    n++;
                    if (itemsStrings.Length == n)
                    {
                        break;
                    }
                    line = itemsStrings[n];
                }
                if (itemsStrings.Length == n) break;
                //n--;

                revisions.Add(revision);
            }

            return revisions;
        }


        static public List<GitRevision> GitRevisions()
        {
            return GitRevisions("");
        }

        static public List<GitRevision> GitRevisions(string filter)
        {
            filter = FixPath(filter);
            string tree;
            if (string.IsNullOrEmpty(filter))
                tree = RunCmd(Settings.GitDir + "git.cmd", "rev-list --all --header --date-order");
            else
                tree = RunCmd(Settings.GitDir + "git.cmd", "rev-list --header --topo-order \"" + filter + "\"");
            
            string[] itemsStrings = tree.Split('\n');

            List<GitRevision> revisions = new List<GitRevision>();

            for (int n = 0; n < itemsStrings.Length-6;)
            {
                GitRevision revision = new GitRevision();
                revision.Guid = itemsStrings[n++].Trim('\0');
                revision.Name = revision.TreeGuid = itemsStrings[n++].Substring(4).Trim();
                while (itemsStrings[n].Contains("parent"))
                {
                    //Add parent
                    revision.ParentGuids.Add( itemsStrings[n++].Substring(6).Trim());
                }
                if (revision.ParentGuids.Count == 0)
                {
                    revision.ParentGuids.Add("0000000000000000000000000000000000000000");
                }
                revision.Author = itemsStrings[n++].Substring(6).Trim();
                revision.Committer = itemsStrings[n++].Substring(9).Trim();
                n++;

                while (itemsStrings.Length > n + 1 &&
                    itemsStrings[n].Length > 0 &&
                    itemsStrings[n][0] == ' ')
                {
                    revision.Message += itemsStrings[n++].Trim() + Environment.NewLine;
                }

                revisions.Add(revision);
            }

            return revisions;
        }

        static public List<GitHead> GetHeads()
        {
            return GetHeads(true);
        }

        static public string StageFiles(List<GitItemStatus> files)
        {
            GitCommands gitCommand = new GitCommands();

            string output = "";

            Process process1 = null;
            foreach (GitItemStatus file in files)
            {
                if (!file.IsDeleted)
                {
                    if (process1 == null)
                        process1 = gitCommand.CmdStartProcess(Settings.GitDir + "git.cmd", "update-index --add --stdin");

                    process1.StandardInput.WriteLine("\"" + FixPath(file.Name) + "\"");
                }
            }
            if (process1 != null)
            {
                process1.StandardInput.Close();
                process1.WaitForExit();
            }

            if (gitCommand.Output != null)
                output = gitCommand.Output.ToString();

            Process process2 = null;
            foreach (GitItemStatus file in files)
            {
                if (file.IsDeleted)
                {
                    if (process2 == null)
                        process2 = gitCommand.CmdStartProcess(Settings.GitDir + "git.cmd", "update-index --remove --stdin");
                    process2.StandardInput.WriteLine("\"" + FixPath(file.Name) + "\"");
                }
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

        static public string UnstageFiles(List<GitItemStatus> files)
        {
            GitCommands gitCommand = new GitCommands();

            string output = "";

            Process process1 = null;
            foreach (GitItemStatus file in files)
            {
                if (!file.IsNew)
                {
                    if (process1 == null)
                        process1 = gitCommand.CmdStartProcess(Settings.GitDir + "git.cmd", "update-index --info-only --index-info");

                    process1.StandardInput.WriteLine("0 0000000000000000000000000000000000000000\t\"" + FixPath(file.Name) + "\"");
                }
            }
            if (process1 != null)
            {
                process1.StandardInput.Close();
                process1.WaitForExit();
            }

            if (gitCommand.Output != null)
                output = gitCommand.Output.ToString();

            Process process2 = null;
            foreach (GitItemStatus file in files)
            {
                if (file.IsNew)
                {
                    if (process2 == null)
                        process2 = gitCommand.CmdStartProcess(Settings.GitDir + "git.cmd", "update-index --force-remove --stdin");
                    process2.StandardInput.WriteLine("\"" + FixPath(file.Name) + "\"");
                }
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

        static public string StageFile(string file)
        {
            file = FixPath(file);
            return GitCommands.RunCmd(Settings.GitDir + "git.cmd", "update-index --add" + " \"" + file + "\"");
        }

        static public string StageFileToRemove(string file)
        {
            file = FixPath(file);
            return GitCommands.RunCmd(Settings.GitDir + "git.cmd", "update-index --remove" + " \"" + file + "\"");
        }


        static public string UnstageFile(string file)
        {
            file = FixPath(file);
            return GitCommands.RunCmd(Settings.GitDir + "git.cmd", "rm" + " --cached \"" + file + "\"");
        }

        static public string UnstageFileToRemove(string file)
        {
            file = FixPath(file);
            return GitCommands.RunCmd(Settings.GitDir + "git.cmd", "reset HEAD --" + " \"" + file + "\"");
        }

        static public string GetSelectedBranch()
        {
            string branches = RunCmd(Settings.GitDir + "git.cmd", "branch");
            string[] branchStrings = branches.Split('\n');
            foreach (string branch in branchStrings)
            {
                if (branch.IndexOf('*') > -1)
                    return branch.Trim('*', ' ');
            }
            return "";
        }

        static public List<GitHead> GetHeads(bool tags)
        {
            return GetHeads(tags, true);
        }

        static public List<GitHead> GetRemoteHeads(string remote, bool tags, bool branches)
        {
            string tree = "";
            if (tags && branches)
                tree = RunCmd(Settings.GitDir + "git.cmd", "ls-remote --heads --tags \"" + remote + "\"");
            else
                if (tags)
                    tree = RunCmd(Settings.GitDir + "git.cmd", "ls-remote --tags \"" + remote + "\"");
                else
                    if (branches)
                        tree = RunCmd(Settings.GitDir + "git.cmd", "ls-remote --heads \"" + remote + "\"");

            string[] itemsStrings = tree.Split('\n');

            List<GitHead> heads = new List<GitHead>();

            foreach (string itemsString in itemsStrings)
            {
                if (itemsString.Length > 42)
                {
                    GitHead head = new GitHead();
                    head.Guid = itemsString.Substring(0, 40);
                    head.Name = itemsString.Substring(41).Trim();
                    if (head.Name.Length > 0 && head.Name.LastIndexOf("/") > 1)
                    {
                        if (head.Name.Contains("refs/tags/"))
                        {
                            //we need the one containing ^{}, because it contains the reference
                            if (head.Name.Contains("^{}"))
                            {
                                head.Name = head.Name.Substring(0, head.Name.Length - 3);
                            }
                            head.Name = head.Name.Substring(head.Name.LastIndexOf("/") + 1);
                            head.IsHead = false;
                            head.IsTag = true;
                        }
                        else
                        {
                            head.IsHead = head.Name.Contains("refs/heads/");
                            head.IsRemote = head.Name.Contains("refs/remotes/");
                            head.IsTag = false;
                            head.IsOther = !head.IsHead && !head.IsRemote && !head.IsTag;
                            if (head.IsHead)
                                head.Name = head.Name.Substring(head.Name.LastIndexOf("heads/") + 6);
                            else
                                if (head.IsRemote)
                                    head.Name = head.Name.Substring(head.Name.LastIndexOf("remotes/") + 8);
                                else
                                    head.Name = head.Name.Substring(head.Name.LastIndexOf("/") + 1);
                        }
                    }

                    heads.Add(head);
                }
            }

            return heads;
        }


        static public List<GitHead> GetHeads(bool tags, bool branches)
        {
            string tree = "" ;
            if (tags && branches)
                tree = RunCmd(Settings.GitDir + "git.cmd", "show-ref --dereference");
            else
            if (tags)
                tree = RunCmd(Settings.GitDir + "git.cmd", "show-ref --dereference --tags");
            else
            if (branches)
                tree = RunCmd(Settings.GitDir + "git.cmd", "show-ref --dereference --heads");

            string[] itemsStrings = tree.Split('\n');

            List<GitHead> heads = new List<GitHead>();

            foreach (string itemsString in itemsStrings)
            {
                if (itemsString.Length > 42)
                {
                    GitHead head = new GitHead();
                    head.Guid = itemsString.Substring(0, 40);
                    head.Name = itemsString.Substring(41).Trim();
                    if (head.Name.Length > 0 && head.Name.LastIndexOf("/") > 1)
                    {
                        if (head.Name.Contains("refs/tags/"))
                        {
                            //we need the one containing ^{}, because it contains the reference
                            if (head.Name.Contains("^{}"))
                            {
                                head.Name = head.Name.Substring(0, head.Name.Length - 3);
                            }
                            head.Name = head.Name.Substring(head.Name.LastIndexOf("/") + 1);
                            head.IsHead = false;
                            head.IsTag = true;
                        }
                        else
                        {
                            head.IsHead = head.Name.Contains("refs/heads/");
                            head.IsRemote = head.Name.Contains("refs/remotes/");
                            head.IsTag = false;
                            head.IsOther = !head.IsHead && !head.IsRemote && !head.IsTag;
                            if (head.IsHead)
                                head.Name = head.Name.Substring(head.Name.LastIndexOf("heads/") + 6);
                            else
                            if (head.IsRemote)
                                head.Name = head.Name.Substring(head.Name.LastIndexOf("remotes/") + 8);
                            else
                                head.Name = head.Name.Substring(head.Name.LastIndexOf("/") + 1);
                        }
                    }

                    heads.Add(head);
                }
            }

            return heads;
        }

        static public List<GitItem> GetFileChanges(string file)
        {
            file = FixPath(file);
            string tree = RunCmd(Settings.GitDir + "git.cmd", "whatchanged --all -- \"" + file + "\"");

            string[] itemsStrings = tree.Split('\n');

            List<GitItem> items = new List<GitItem>();

            GitItem item = null;
            foreach (string itemsString in itemsStrings)
            {
                if (itemsString.StartsWith("commit "))
                {
                    item = new GitItem();

                    item.CommitGuid = itemsString.Substring(7).Trim();
                    items.Add(item);
                }
                else
                if (item == null)
                {
                    continue;
                }
                else
                if (itemsString.StartsWith("Author: "))
                {
                    item.Author = itemsString.Substring(7).Trim();
                }
                else
                if (itemsString.StartsWith("Date:   "))
                {
                    item.Date = itemsString.Substring(7).Trim();
                }
                else
                if (!itemsString.StartsWith(":") && !string.IsNullOrEmpty(itemsString))
                {
                    item.Name += itemsString.Trim() + Environment.NewLine;
                }
                else
                {
                    if (item != null && itemsString.Length > 32)
                        item.Guid = itemsString.Substring(26, 7);
                }
            }

            return items;
        }


        static public List<IGitItem> GetTree(string id)
        {
            string tree = RunCmd(Settings.GitDir + "git.cmd", "ls-tree \"" + id + "\"");

            string [] itemsStrings = tree.Split('\n');

            List<IGitItem> items = new List<IGitItem>();

            foreach (string itemsString in itemsStrings)
            {
                GitItem item = new GitItem();

                if (itemsString.Length > 53)
                {

                    item.Mode = itemsString.Substring(0, 6);
                    int guidStart = itemsString.IndexOf(' ', 7);
                    item.ItemType = itemsString.Substring(7, guidStart - 7);
                    item.Guid = itemsString.Substring(guidStart+1, 40);
                    item.Name = itemsString.Substring(guidStart+42).Trim();
                    item.FileName = item.Name;

                    //if (item.ItemType == "tree")
                    //    item.SubItems = GetTree(item.Guid);

                    items.Add(item);
                }
            }

            return items;
        }

        public static List<GitBlame> Blame(string filename, string from)
        {
            from = FixPath(from);
            filename = FixPath(filename);
            string[] itemsStrings = RunCmd(Settings.GitDir + "git.cmd", "blame -M -w -l \"" + from + "\" -- \"" + filename + "\"").Split('\n');

            List<GitBlame> items = new List<GitBlame>();

            GitBlame item = null;
            string lastCommitGuid = "";

            Color color1 = Color.Azure;
            Color color2 = Color.Ivory;
            Color currentColor = color1;

            foreach (string itemsString in itemsStrings)
            {
                if (itemsString.Length > 50)
                {
                    string commitGuid = itemsString.Substring(0, 40).Trim();

                    if (lastCommitGuid != commitGuid)
                    {
                        if (currentColor == color1)
                            currentColor = color2;
                        else
                            currentColor = color1;
                    }

                    {
                        item = new GitBlame();
                        item.color = currentColor;
                        item.CommitGuid = commitGuid;
                        items.Add(item);
                    }

                    int codeIndex = itemsString.IndexOf(')', 41)+1;
                    if (codeIndex > 41)
                    {
                        if (lastCommitGuid != commitGuid)
                            item.Author = itemsString.Substring(41, codeIndex - 41).Trim();

                        if (!string.IsNullOrEmpty(item.Text))
                            item.Text += Environment.NewLine;
                        item.Text += itemsString.Substring(codeIndex);
                    }
                    
                    lastCommitGuid = commitGuid;
                }
            }


            return items;

        }

        public static string GetFileText(string id)
        {
            return RunCmd(Settings.GitDir + "git.cmd", "cat-file blob \"" + id + "\"");
        }

        public static void StreamCopy(Stream input, Stream output)
        {
            int read = 0;
            byte[] buffer = new byte[2048];
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
                MemoryStream newStream = new MemoryStream();

                SetEnvironmentVariable();

                Settings.GitLog.Log(Settings.GitDir + "git.cmd" + " " + "cat-file blob \"" + id + "\"");
                //process used to execute external commands

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.ErrorDialog = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardInput = false;
                process.StartInfo.RedirectStandardError = false;

                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.FileName = "\"" + Settings.GitDir + "git.cmd" + "\"";
                process.StartInfo.Arguments = "cat-file blob \"" + id + "\"";
                process.StartInfo.WorkingDirectory = Settings.WorkingDir;
                process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                process.StartInfo.LoadUserProfile = true;

                process.Start();

                StreamCopy(process.StandardOutput.BaseStream, newStream);
                
                process.WaitForExit();

                return newStream;
            }
            catch
            {
            }

            return null;
        }

        public static string MergeBranch(string branch)
        {
            return RunCmd(Settings.GitDir + "git.cmd", MergeBranchCmd(branch));
        }

        public static string MergeBranchCmd(string branch)
        {
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
