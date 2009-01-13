using System;
using System.Collections.Generic;

using System.Text;
using System.Security.Permissions;
using System.IO;
using PatchApply;
using System.Diagnostics;
using System.Drawing;

namespace GitCommands
{
    public class GitCommands
    {
        public static string FindGitWorkingDir(string startDir)
        {
            if (string.IsNullOrEmpty(startDir))
                return "";

            string dir = startDir;
            if (!dir.EndsWith("\\") && !dir.EndsWith("/"))
                dir += "\\";

            while (dir.LastIndexOfAny(new char[]{'\\', '/'}) > 0)
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

        public static void RunRealCmd(string cmd, string arguments)
        {
            try
            {
                arguments = arguments.Replace('\\', '/');

                Settings.GitLog += cmd + " " + arguments + "\n";
                //process used to execute external commands

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.ErrorDialog = false;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.RedirectStandardInput = false;

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

        public static void RunRealCmdDetatched(string cmd, string arguments)
        {
            try
            {
                arguments = arguments.Replace('\\', '/');

                Settings.GitLog += cmd + " " + arguments + "\n";
                //process used to execute external commands

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.ErrorDialog = false;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.RedirectStandardInput = false;

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
                arguments = arguments.Replace('\\', '/');

                Settings.GitLog += cmd + " " + arguments + "\n";
                //process used to execute external commands

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.ErrorDialog = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardError = true;

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
            Kill();

            //process used to execute external commands
            Process = new System.Diagnostics.Process();
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.ErrorDialog = false;
            Process.StartInfo.RedirectStandardOutput = true;
            Process.StartInfo.RedirectStandardInput = true;
            Process.StartInfo.RedirectStandardError = true;

            Process.StartInfo.CreateNoWindow = true;
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
                Output.Append(e.Data + "\n");
            if (DataReceived != null)
                DataReceived(this, e);
        }

        void process_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if (CollectOutput)
                Output.Append( e.Data + "\n" );
            if (DataReceived != null)
                DataReceived(this, e);
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public static string RunCmd(string cmd, string arguments)
        {
            string output = "";
            try
            {
                arguments = arguments.Replace('\\', '/');

                Settings.GitLog += cmd + " " + arguments + "\n";
                //process used to execute external commands

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.ErrorDialog = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardError = true;

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
                    output += "\n" + error;
                }
            }
            catch
            {
            }
            return output;
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public static Process RunCmdAsync(string cmd, string arguments)
        {
            Process process = new System.Diagnostics.Process(); 
            try
            {
                arguments = arguments.Replace('\\', '/');

                Settings.GitLog += cmd + " " + arguments + "\n";
                //process used to execute external commands


                process.StartInfo.UseShellExecute = true;
                process.StartInfo.ErrorDialog = true;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.RedirectStandardInput = false;
                process.StartInfo.RedirectStandardError = false;

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
            //return RunCmd(Settings.GitDir + "git.cmd", "merge \"{95E16C63-E0D3-431f-9E87-F4B41F7EC30F}\"").Contains("fatal: You are in the middle of a conflicted merge.");
            return !string.IsNullOrEmpty(RunCmd(Settings.GitDir + "git.cmd", "ls-files --unmerged --exclude-standard"));
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
            Run("C:\\Windows\\System32\\cmd.exe", "/c \"\"" + Settings.GitDir + "gitk\" --all\"");
            //Run(Settings.GitDir + "gitk", "");
        }

        static public void RunGui()
        {
            Run("C:\\Windows\\System32\\cmd.exe", "/c \"\"" + Settings.GitDir + "git.cmd\" gui\"");
        }


        static public void RunBash()
        {
            RunRealCmdDetatched("C:\\Windows\\System32\\cmd.exe", "/c sh.exe --login -i");
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

        static public string UserCommitCount()
        {
            return RunCmd(Settings.GitDir + "git.cmd", "shortlog -s -n");
        }

        static public string DeleteBranch(string branchName, bool force)
        {
            if (force)
                return RunCmd(Settings.GitDir + "git.cmd", "branch -D \"" + branchName + "\"");
            else
                return RunCmd(Settings.GitDir + "git.cmd", "branch -d \"" + branchName + "\"");
        }

        static public string DeleteTag(string tagName)
        {
            return RunCmd(Settings.GitDir + "git.cmd", "tag -d \"" + tagName + "\"");
        }

        static public string GetCurrentCheckout()
        {
            return RunCmd(Settings.GitDir + "git.cmd", "log -g -1 HEAD --pretty=format:%H");
        }

        static public int CommitCount()
        {
            int count;
            if (int.TryParse(RunCmd(Settings.GitDir + "C:\\Windows\\System32\\cmd.exe", "/c \"\"" + Settings.GitDir + "git.cmd\" rev-list --all --abbrev-commit | wc -l\""), out count))
                return count;

            return 0;
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

        static public string ResetFile(string file)
        {
            return RunCmd(Settings.GitDir + "git.cmd", "checkout-index --index --force -- \"" + file + "\"");
        }


        public string FormatPatch(string from, string to, string output)
        {
            string result = RunCmd(Settings.GitDir + "git.cmd", "format-patch -M -C -B \"" + from + "\"..\"" + to + "\" -o \"" + output + "\"");

            return result;
        }

        static public string Tag(string tagName, string revision)
        {
            string result = GitCommands.RunCmd(Settings.GitDir + "git.cmd", "tag \"" + tagName.Trim() + "\" \"" + revision + "\"");

            return result;
        }

        static public string Branch(string branchName, string revision)
        {
            string result = GitCommands.RunCmd(Settings.GitDir + "git.cmd", "branch \"" + branchName.Trim() + "\" \"" + revision + "\"");

            return result;
        }

        static public string Push(string path)
        {
            string result = GitCommands.RunCmd(Settings.GitDir + "git.cmd", "push \"" + path.Trim() + "\"");

            return result;
        }

        static public Process PushAsync(string path, string branch, bool all)
        {
            branch = branch.Replace(" ", "");

            if (all)
                return RunCmdAsync("cmd.exe", " /k \"\"" + Settings.GitDir + "git.cmd\" push --all \"" + path.Trim() + "\"\"");
            else
                if (!string.IsNullOrEmpty(branch))
                    return RunCmdAsync("cmd.exe", " /k \"\"" + Settings.GitDir + "git.cmd\" push \"" + path.Trim() + "\" " + branch + "\"");


            return RunCmdAsync("cmd.exe", " /k \"\"" + Settings.GitDir + "git.cmd\" push \"" + path.Trim() + "\"\"");
        }

        static public string Fetch(string remote, string branch)
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);
            branch = branch.Replace(" ", "");

            string localbranch;

            if (string.IsNullOrEmpty(branch))
                localbranch = "";
            else
                localbranch = "+refs/heads/" + branch + "";

            string remotebranch;
            if (string.IsNullOrEmpty(GetSetting("remote." + remote + ".url")) || string.IsNullOrEmpty(branch))
                remotebranch = "";
            else
                remotebranch = ":" + "refs/remotes/" + remote.Trim() + "/" + branch + "";



            GitCommands.RunRealCmd("cmd.exe", " /k \"\"" + Settings.GitDir + "git.cmd\" fetch \"" + remote.Trim() + "\" " + localbranch + remotebranch + "\"");

            return "Done";
        }



        static public string Pull(string remote, string branch, bool rebase)
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);
            
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
            if (string.IsNullOrEmpty(GetSetting("remote." + remote + ".url")) || string.IsNullOrEmpty(branch))
                remotebranch = "";
            else
                remotebranch = ":" + "refs/remotes/" + remote.Trim() + "/" + branch + "";
            
                

            GitCommands.RunRealCmd("cmd.exe", " /k \"\"" + Settings.GitDir + "git.cmd\" pull " + rebaseOption + "\"" + remote.Trim() + "\" " + localbranch + remotebranch + "\"");

            return "Done";
        }

        static public string Resolved()
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            string result = GitCommands.RunCmd(Settings.GitDir + "git.cmd", "am --3way --resolved");

            return result;
        }

        static public string Skip()
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            string result = GitCommands.RunCmd(Settings.GitDir + "git.cmd", "am --3way --skip");

            return result;
        }

        static public string Abort()
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            string result = GitCommands.RunCmd(Settings.GitDir + "git.cmd", "am --3way --abort");

            return result;
        }

        static public string Commit(string message, bool amend)
        {
            return GitCommands.RunCmd(Settings.GitDir + "git.cmd", CommitCmd(message, amend));
        }

        static public string CommitCmd(string message, bool amend)
        {
            if (amend)
                return "commit --amend -m \"" + message + "\"";
            else
                return "commit -m \"" + message + "\"";
        }

        static public string Patch(string patchFile)
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            string result = GitCommands.RunCmd(Settings.GitDir + "git.cmd", "am --3way --signoff \"" + patchFile + "\"");

            return result;
        }

        public static Process UpdateRemotes()
        {
            return RunCmdAsync("cmd.exe", " /k \"\"" + Settings.GitDir + "git.cmd\" remote update\"");
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
            GitCommands.RunCmd(Settings.GitDir + "git.cmd", "config --global --unset-all \"" + setting + "\"").Trim();

            if (!string.IsNullOrEmpty(value))
                GitCommands.RunCmd(Settings.GitDir + "git.cmd", "config --global \"" + setting + "\" \"" + value.Trim() + "\"").Trim();
        }

        static public string GetSetting(string setting)
        {
            return GitCommands.RunCmd(Settings.GitDir + "git.cmd", "config --get \"" + setting + "\"").Trim();
        }

        static public void UnSetSetting(string setting)
        {
            GitCommands.RunCmd(Settings.GitDir + "git.cmd", "config --unset-all \"" + setting + "\"").Trim();
        }

        static public void SetSetting(string setting, string value)
        {
            UnSetSetting(setting);

            if (!string.IsNullOrEmpty(value))
                GitCommands.RunCmd(Settings.GitDir + "git.cmd", "config \"" + setting + "\" \"" + value.Trim() + "\"").Trim();
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
                        revision.Message += line.Substring(graphIndex).Trim() + "\n";
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
                    revision.Message += itemsStrings[n++].Trim() + "\n";
                }

                revisions.Add(revision);
            }

            return revisions;
        }

        static public List<GitHead> GetHeads()
        {
            return GetHeads(true);
        }

        static public string StageFiles(List<string> files)
        {
            string fileslist = "";
            foreach (string file in files)
            {
                fileslist += " \"" + file + "\"";
            }

            return GitCommands.RunCmd(Settings.GitDir + "git.cmd", "update-index --add" + fileslist);
        }

        static public string UnstageFiles(List<string> files)
        {
            if (files.Count == 0) return "No staged files selected to unstage";

            string fileslist = "";
            foreach (string file in files)
            {
                fileslist += " \"" + file + "\"";
            }

            return GitCommands.RunCmd(Settings.GitDir + "git.cmd", "reset HEAD" + fileslist);
        }

        static public string StageFile(string file)
        {
            return GitCommands.RunCmd(Settings.GitDir + "git.cmd", "update-index --add" + " \"" + file + "\"");
        }

        static public string StageFileToRemove(string file)
        {
            return GitCommands.RunCmd(Settings.GitDir + "git.cmd", "update-index --remove" + " \"" + file + "\"");
        }


        static public string UnstageFile(string file)
        {
            return GitCommands.RunCmd(Settings.GitDir + "git.cmd", "rm" + " --cached \"" + file + "\"");
        }

        static public string UnstageFileToRemove(string file)
        {
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
            string tree = RunCmd(Settings.GitDir + "git.cmd", "whatchanged --all \"" + file + "\"");

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
                    item.Name += itemsString.Trim() + "\n";
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
                    item.ItemType = itemsString.Substring(7, 4);
                    item.Guid = itemsString.Substring(12, 40);
                    item.Name = itemsString.Substring(53).Trim();
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
            string[] itemsStrings = RunCmd(Settings.GitDir + "git.cmd", "blame -l \"" + from + "\" \"" + filename + "\"").Split('\n');

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
                            item.Text += "\n";
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

        public static string MergeBranch(string branch)
        {
            return RunCmd(Settings.GitDir + "git.cmd", "merge \"" + branch + "\"");
        }
    }
}
