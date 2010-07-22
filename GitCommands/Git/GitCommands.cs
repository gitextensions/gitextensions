using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Permissions;
using System.IO;
using PatchApply;
using System.Diagnostics;
using System.Drawing;
using GitUIPluginInterfaces;
using System.ComponentModel;
using ResourceManager.Translation;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Threading;

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
        public static Encoding EndcodingRouter(string arg)
        {
            Regex r = default(Regex);
            StringCollection regcol = new StringCollection();
            regcol.Add("ls-files");
            regcol.Add("diff");
            regcol.Add("ls-tree");
            r = new Regex(ConvertRegCollectionToString(regcol));
            if (r.IsMatch(arg))
            {
                return Encoding.GetEncoding(Thread.CurrentThread.CurrentCulture.TextInfo.ANSICodePage);
            }
            else
            {
                return Encoding.UTF8;
            }
        }
        public static string ConvertRegCollectionToString(StringCollection regcol)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in regcol)
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
                Environment.SetEnvironmentVariable("HOME", Settings.CustomHomeDir);
                return;
            }

            if (Settings.UserProfileHomeDir)
            {
                Environment.SetEnvironmentVariable("HOME", Environment.GetEnvironmentVariable("USERPROFILE"));
                return;
            }

            if (reload)
            {
                Environment.SetEnvironmentVariable("HOME", Environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.User));
            }

            //Default!
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HOME")))
            {
                string homePath = "";
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
        }

        public static void RunRealCmd(string cmd, string arguments)
        {
            try
            {
                createAndStartCommand(cmd, arguments, true);
                

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
                createAndStartCommand(cmd, arguments, false);
            }
            catch (Exception)
            {

            }


        }

        private static void createAndStartCommand(string cmd, string arguments, bool waitAndExit)
        {                    
            SetEnvironmentVariable();

            Settings.GitLog.Log(cmd + " " + arguments);
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
            if (waitAndExit)
            {
                process.WaitForExit();
                process.Close();
            }
        }

        public static void Run(string cmd, string arguments)
        {
            try
            {
                SetEnvironmentVariable();
                Process process = CreateAndStartProcess(arguments, cmd);                
                //process.WaitForExit();
            }
            catch
            {
            }

        }

        public System.Diagnostics.Process Process { get; set; }

        public bool CollectOutput = true;
        public bool StreamOutput;

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public Process CmdStartProcess(string cmd, string arguments)
        {
            SetEnvironmentVariable();

            bool ssh = UseSSH(arguments);

            Kill();

            Settings.GitLog.Log(cmd + " " + arguments);

            //process used to execute external commands
            Process = new System.Diagnostics.Process();
            setCommonProcessAttributes(Process, arguments);
            Process.StartInfo.CreateNoWindow = (!ssh && !Settings.ShowGitCommandLine);
            Process.StartInfo.FileName = "\"" + cmd + "\"";
            Process.StartInfo.Arguments = arguments;
            Process.StartInfo.WorkingDirectory = Settings.WorkingDir;
            Process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            Process.StartInfo.LoadUserProfile = true;
            Process.EnableRaisingEvents = true;

            if (!StreamOutput)
            {
                Process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(process_OutputDataReceived);
                Process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(process_ErrorDataReceived);
            }
            Output = new StringBuilder();
            ErrorOutput = new StringBuilder();

            Process.Exited += new EventHandler(process_Exited);
            Process.Start();

            if (!StreamOutput)
            {
                Process.BeginErrorReadLine();
                Process.BeginOutputReadLine();
            }

            return Process;
        }

        private static void setCommonProcessAttributes(Process Process, string arguments)
        {
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.ErrorDialog = false;
            Process.StartInfo.RedirectStandardOutput = true;
            Process.StartInfo.RedirectStandardInput = true;
            Process.StartInfo.RedirectStandardError = true;
            Process.StartInfo.StandardErrorEncoding = EndcodingRouter(arguments);
            Process.StartInfo.StandardOutputEncoding = Process.StartInfo.StandardErrorEncoding;
        }

        private bool UseSSH(string arguments)
        {
            var x = !Plink() && GetArgumentsRequiresSsh(arguments);
            return x || arguments.Contains("plink");
        }

        private bool GetArgumentsRequiresSsh(string arguments)
        {
            return ((arguments.Contains("@") && arguments.Contains("://")) ||
                       (arguments.Contains("@") && arguments.Contains(":")) ||
                       (arguments.Contains("ssh://")) ||
                       (arguments.Contains("http://")) ||
                       (arguments.Contains("git://")) ||
                       (arguments.Contains("push")) ||
                       (arguments.Contains("remote")) ||
                       (arguments.Contains("pull")));
        }

        public void Kill()
        {
            //If there was another process running, kill it
            if (Process != null)
            {
                try
                {
                    if (!Process.HasExited)
                    {
                        Process.Kill();
                    }
                }
                catch
                {
                }
                Process.Close();
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
            string output = "";
            try
            {
                SetEnvironmentVariable();

                arguments = arguments.Replace("$QUOTE$", "\\\"");

                Process process = CreateAndStartProcess(arguments, cmd);
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

        private static Process CreateAndStartProcess(string arguments, string cmd)
        {                       
            Settings.GitLog.Log(cmd + " " + arguments);
            //process used to execute external commands

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            setCommonProcessAttributes(process, arguments);
                        
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = "\"" + cmd + "\"";
            process.StartInfo.Arguments = arguments;
            process.StartInfo.WorkingDirectory = Settings.WorkingDir;
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            process.StartInfo.LoadUserProfile = true;

            process.Start();
            return process;
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


                process.StartInfo.LoadUserProfile = true;
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.FileName = "\"" + cmd + "\"";
                process.StartInfo.Arguments = arguments;
                process.StartInfo.WorkingDirectory = Settings.WorkingDir;
                process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
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
            return !string.IsNullOrEmpty(RunCmd(Settings.GitCommand, "ls-files --unmerged"));
            //else
            //    return RunCmd(Settings.GitCommand, "merge \"{95E16C63-E0D3-431f-9E87-F4B41F7EC30F}\"").Contains("fatal: You are in the middle of a conflicted merge.");
        }

        static public List<GitItem> GetConflictedFiles()
        {
            List<GitItem> unmergedFiles = new List<GitItem>();


            string fileName = "";
            foreach (string file in GetUnmergedFileListing())
            {
                if (file.LastIndexOfAny(new char[] {' ', '\t'}) <= 0)
                    continue;
                if (file.Substring(file.LastIndexOfAny(new char[] {' ', '\t'}) + 1) == fileName)
                    continue;
                fileName = file.Substring(file.LastIndexOfAny(new char[] { ' ', '\t' }) + 1);
                unmergedFiles.Add(new GitItem {FileName = fileName});
            }

            return unmergedFiles;
        }

        private static string[] GetUnmergedFileListing()
        {
            return RunCmd(Settings.GitCommand, "ls-files --unmerged").Split('\n');
        }

        static public bool HandleConflict_SelectBase(string fileName)
        {
            if (HandleConflicts_SaveSide(fileName, fileName, "1"))
            {
                GitCommands.RunCmd(Settings.GitCommand, "add -- \"" + fileName + "\"");
                return true;
            }
            return false;
        }

        static public bool HandleConflict_SelectLocal(string fileName)
        {
            if (HandleConflicts_SaveSide(fileName, fileName, "2"))
            {
                GitCommands.RunCmd(Settings.GitCommand, "add -- \"" + fileName + "\"");
                return true;
            }
            return false;
        }

        static public bool HandleConflict_SelectRemote(string fileName)
        {
            if (HandleConflicts_SaveSide(fileName, fileName, "3"))
            {
                GitCommands.RunCmd(Settings.GitCommand, "add -- \"" + fileName + "\"");
                return true;
            }
            return false;
        }

        public static bool HandleConflicts_SaveSide(string fileName, string saveAs, string side)
        {
            side = GetSide(side);

            fileName = FixPath(fileName);
            string[] unmerged = RunCmd(Settings.GitCommand, "ls-files --unmerged \"" + fileName + "\"").Split('\n');

            foreach (string file in unmerged)
            {
                string[] fileline = file.Split(new char[] { ' ', '\t' });
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

        static public string[] GetConflictedFiles(string filename)
        {
            filename = FixPath(filename);

            string[] fileNames = 
            {
                filename + ".BASE",
                filename + ".LOCAL",
                filename + ".REMOTE"
            };

            string[] unmerged = RunCmd(Settings.GitCommand, "ls-files --unmerged \"" + filename + "\"").Split('\n');

            foreach (string file in unmerged)
            {
                string[] fileline = file.Split(new char[] { ' ', '\t' });
                if (fileline.Length < 3)
                    continue;

                int stage;
                Int32.TryParse(fileline[2].Trim(), out stage);

                string tempFile = RunCmd(Settings.GitCommand, "checkout-index --temp --stage=" + stage + " -- " + filename);
                tempFile = tempFile.Split('\t')[0];
                tempFile = Path.Combine(Settings.WorkingDir, tempFile);

                string newFileName = Path.Combine(Settings.WorkingDir, fileNames[stage - 1]);
                try
                {
                    fileNames[stage - 1] = newFileName;
                    int index = 1;
                    while (File.Exists(fileNames[stage - 1]) && index < 50)
                    {
                        fileNames[stage - 1] = newFileName + index.ToString();
                        index++;
                    }
                    File.Move(tempFile, fileNames[stage - 1]);
                }
                catch { }
            }

            return fileNames;
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
            Run("cmd.exe", "/c \"\"" + Settings.GitCommand.Replace("git.cmd", "gitk.cmd") + "\" --all\"");
            //Run(Settings.GitDir + "gitk", "");
        }

        static public void RunGui()
        {
            Run("cmd.exe", "/c \"\"" + Settings.GitCommand + "\" gui\"");
        }


        static public void RunBash()
        {
            RunRealCmdDetatched("cmd.exe", "/c \"\"" + Settings.GitBinDir + "sh\" --login -i\"");
        }

        static public string Init(bool bare, bool shared)
        {
            if (bare && shared)
                return RunCmd(Settings.GitCommand, "init --bare --shared=all");
            else
                if (bare)
                    return RunCmd(Settings.GitCommand, "init --bare");
                else
                    return RunCmd(Settings.GitCommand, "init");
        }

        static public string CherryPickCmd(string cherry, bool commit)
        {
            if (commit)
                return "cherry-pick \"" + cherry + "\"";
            else
                return "cherry-pick --no-commit \"" + cherry + "\"";
        }


        static public string CherryPick(string cherry, bool commit)
        {
            if (commit)
                return RunCmd(Settings.GitCommand, "cherry-pick \"" + cherry + "\"");
            else
                return RunCmd(Settings.GitCommand, "cherry-pick --no-commit \"" + cherry + "\"");
        }

        static public string ShowSha1(string sha1)
        {
            return RunCachableCmd(Settings.GitCommand, "show " + sha1);
        }

        static public string UserCommitCount()
        {
            return RunCmd(Settings.GitCommand, "shortlog -s -n");
        }

        static public string DeleteBranch(string branchName, bool force)
        {
            return RunCmd(Settings.GitCommand, DeleteBranchCmd(branchName, force));
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
            return RunCmd(Settings.GitCommand, DeleteTagCmd(tagName));
        }

        public static string DeleteTagCmd(string tagName)
        {
            return "tag -d \"" + tagName + "\"";
        }

        static public string GetCurrentCheckout()
        {
            return RunCmd(Settings.GitCommand, "log -g -1 HEAD --pretty=format:%H");
        }

        static public int CommitCount()
        {
            int count;
            if (int.TryParse(RunCmd("cmd.exe", "/c \"\"" + Settings.GitCommand + "\" rev-list --all --abbrev-commit | wc -l\""), out count))
                return count;

            return 0;
        }

        static public string GetSubmoduleRemotePath(string name)
        {
            ConfigFile configFile = new ConfigFile(Settings.WorkingDir + "\\.gitmodules");
            return configFile.GetValue("submodule." + name.Trim() + ".url").Trim();

            //return RunCmd(Settings.GitCommand, "config -f .gitmodules --get submodule." + name.Trim() + ".url");
        }

        static public string GetSubmoduleLocalPath(string name)
        {
            ConfigFile configFile = new ConfigFile(Settings.WorkingDir + "\\.gitmodules");
            return configFile.GetValue("submodule." + name.Trim() + ".path").Trim();

            //return RunCmd(Settings.GitCommand, "config -f .gitmodules --get submodule." + name.Trim() + ".path").Trim();
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
            string[] submodules = RunCmd(Settings.GitCommand, "submodule status").Split('\n');

            IList<IGitSubmodule> submoduleList = new List<IGitSubmodule>();

            string lastLine = null;

            foreach (string submodule in submodules)
            {
                if (submodule.Length < 43)
                    continue;

                if (submodule.Equals(lastLine))
                    continue;

                lastLine = submodule;

                GitSubmodule gitSubmodule = CreateGitSubmodule(submodule);
                submoduleList.Add(gitSubmodule);
            }

            return submoduleList;
        }

        private GitSubmodule CreateGitSubmodule(string submodule)
        {
            GitSubmodule gitSubmodule = new GitSubmodule
                                            {
                                                Initialized = submodule[0] != '-',
                                                UpToDate = submodule[0] != '+',
                                                CurrentCommitGuid = submodule.Substring(1, 40).Trim()
                                            };
            string name = submodule.Substring(42).Trim();
            if (name.Contains("("))
            {
                gitSubmodule.Name = name.Substring(0, name.IndexOf("("));
                gitSubmodule.Branch = name.Substring(name.IndexOf("(")).Trim(new char[] { '(', ')', ' ' });
            }
            else
                gitSubmodule.Name = name;
            return gitSubmodule;
        }

        static public string Stash()
        {
            return RunCmd(Settings.GitCommand, "stash save");
        }

        static public string StashApply()
        {
            return RunCmd(Settings.GitCommand, "stash apply");
        }

        static public string StashClear()
        {
            return RunCmd(Settings.GitCommand, "stash clear");
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

            return RunCmd(Settings.GitCommand, args);
        }

        static public string ResetMixed(string commit, string file)
        {
            string args = "reset --mixed";

            if (!string.IsNullOrEmpty(commit))
                args += " \"" + commit + "\"";

            if (!string.IsNullOrEmpty(file))
                args += " -- \"" + file + "\"";

            return RunCmd(Settings.GitCommand, args);
        }

        static public string ResetHard(string commit, string file)
        {
            string args = "reset --hard";

            if (!string.IsNullOrEmpty(commit))
                args += " \"" + commit + "\"";

            if (!string.IsNullOrEmpty(file))
                args += " -- \"" + file + "\"";

            return RunCmd(Settings.GitCommand, args);
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

        private static GitVersion versionInUse;

        public static GitVersion VersionInUse
        {
            get
            {
                if (versionInUse == null || versionInUse.IsUnknown)
                {
                    string result = RunCmd(Settings.GitCommand, "--version");
                    versionInUse = new GitVersion(result);
                }

                return versionInUse;
            }
        }

        static public string CloneCmd(string from, string to, bool central, int? depth)
        {
            from = FixPath(from);
            to = FixPath(to);
            List<string> options = new List<string> { "-v" };
            if (central)
                options.Add("--bare");
            if (depth.HasValue)
                options.Add("--depth " + depth);
            if (VersionInUse.CloneCanAskForProgress)
                options.Add("--progress");
            options.Add("\"" + from.Trim() + "\"");
            options.Add("\"" + to.Trim() + "\"");

            return "clone " + string.Join(" ", options.ToArray());
        }

        static public string ResetFile(string file)
        {
            file = FixPath(file);
            return RunCmd(Settings.GitCommand, "checkout-index --index --force -- \"" + file + "\"");
        }


        public string FormatPatch(string from, string to, string output, int start)
        {
            output = FixPath(output);

            string result = RunCmd(Settings.GitCommand, "format-patch -M -C -B --start-number " + start + " \"" + from + "\"..\"" + to + "\" -o \"" + output + "\"");

            return result;
        }

        public string FormatPatch(string from, string to, string output)
        {
            output = FixPath(output);

            string result = RunCmd(Settings.GitCommand, "format-patch -M -C -B \"" + from + "\"..\"" + to + "\" -o \"" + output + "\"");

            return result;
        }


        static public string Tag(string tagName, string revision, bool annotation)
        {
            string result;

            if (annotation)
                result = GitCommands.RunCmd(Settings.GitCommand, "tag \"" + tagName.Trim() + "\" -a -F \"" + Settings.WorkingDirGitDir() + "\\TAGMESSAGE\" -- \"" + revision + "\"");
            else
                result = GitCommands.RunCmd(Settings.GitCommand, "tag \"" + tagName.Trim() + "\" \"" + revision + "\"");

            return result;
        }

        static public string Branch(string branchName, string revision, bool checkout)
        {
            string result = GitCommands.RunCmd(Settings.GitCommand, BranchCmd(branchName, revision, checkout));

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

            if (sshString.EndsWith("plink.exe", StringComparison.CurrentCultureIgnoreCase))
                return true;

            return false;
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

            string result = GitCommands.RunCmd(Settings.GitCommand, "push \"" + path.Trim() + "\"");

            return result;
        }

        static public Process PushAsync(string path, string branch, bool all)
        {
            path = FixPath(path);

            return RunCmdAsync("cmd.exe", " /k \"\"" + Settings.GitCommand + "\" " + PushCmd(path, branch, all) + "\"");
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
            return PushCmd(path, null, branch, all, false);
        }

        public static string PushCmd(string path, string fromBranch, string toBranch, bool all, bool force)
        {
            path = FixPath(path);

            if (string.IsNullOrEmpty(fromBranch) && !string.IsNullOrEmpty(toBranch))
                fromBranch = "HEAD";

            toBranch = toBranch.Replace(" ", "");

            string sforce = "";
            if (force)
                sforce = "-f ";

            if (all)
                return "push " + sforce + "--all \"" + path.Trim() + "\"";
            else if (!string.IsNullOrEmpty(toBranch) && !string.IsNullOrEmpty(fromBranch))
                return "push " + sforce + "\"" + path.Trim() + "\" " + fromBranch + ":" + toBranch;

            return "push " + sforce + "\"" + path.Trim() + "\" " + fromBranch;
        }

        public static string PushTagCmd(string path, string tag, bool all)
        {
            return PushTagCmd(path, tag, all, false);
        }

        public static string PushTagCmd(string path, string tag, bool all, bool force)
        {
            path = FixPath(path);

            tag = tag.Replace(" ", "");

            string sforce = "";
            if (force)
                sforce = "-f ";

            if (all)
                return "push " + sforce + "\"" + path.Trim() + "\" --tags";
            else
                if (!string.IsNullOrEmpty(tag))
                    return "push " + sforce + "\"" + path.Trim() + "\" tag " + tag;

            return "";
        }

        static public string Fetch(string remote, string branch)
        {
            remote = FixPath(remote);

            Directory.SetCurrentDirectory(Settings.WorkingDir);
            branch = FetchCmd(remote, branch);

            GitCommands.RunRealCmd("cmd.exe", " /k \"\"" + Settings.GitCommand + "\" " + FetchCmd(remote, branch) + "\"");

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
            return "fetch " + GetFetchArgs(remote, branch);
        }

        static public string Pull(string remote, string branch, bool rebase)
        {
            remote = FixPath(remote);

            Directory.SetCurrentDirectory(Settings.WorkingDir);

            GitCommands.RunRealCmd("cmd.exe", " /k \"\"" + Settings.GitCommand + "\" " + PullCmd(remote, branch, rebase) + "\"");

            return "Done";
        }

        public static string PullCmd(string remote, string branch, bool rebase)
        {
            string rebaseOption = "";
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
            string remoteUrl = GetSetting("remote." + remote + ".url");

            if (PathIsUrl(remote) && !string.IsNullOrEmpty(branch) && string.IsNullOrEmpty(remoteUrl))
                remotebranch = ":refs/heads/" + branch + "";
            else
                if (string.IsNullOrEmpty(branch) || PathIsUrl(remote) || string.IsNullOrEmpty(remoteUrl))
                    remotebranch = "";
                else
                    remotebranch = ":" + "refs/remotes/" + remote.Trim() + "/" + branch + "";

            string progressOption = "";
            if (VersionInUse.FetchCanAskForProgress)
                progressOption = "--progress ";

            return progressOption + "\"" + remote.Trim() + "\" " + localbranch + remotebranch;
        }

        static public string ContinueRebase()
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            string result = GitCommands.RunCmd(Settings.GitCommand, ContinueRebaseCmd());

            return result;
        }

        public static string ContinueRebaseCmd()
        {
            return "rebase --continue";
        }

        static public string SkipRebase()
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            string result = GitCommands.RunCmd(Settings.GitCommand, SkipRebaseCmd());

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

            string result = GitCommands.RunCmd(Settings.GitCommand, RebaseCmd(branch));

            return result;
        }

        public static string RebaseCmd(string branch)
        {
            return "rebase \"" + branch + "\"";
        }


        static public string AbortRebase()
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            string result = GitCommands.RunCmd(Settings.GitCommand, AbortRebaseCmd());

            return result;
        }

        public static string AbortRebaseCmd()
        {
            return "rebase --abort";
        }

        static public string Resolved()
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            string result = GitCommands.RunCmd(Settings.GitCommand, ResolvedCmd());

            return result;
        }

        public static string ResolvedCmd()
        {
            return "am --3way --resolved";
        }

        static public string Skip()
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            string result = GitCommands.RunCmd(Settings.GitCommand, SkipCmd());

            return result;
        }

        public static string SkipCmd()
        {
            return "am --3way --skip";
        }

        static public string Abort()
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            string result = GitCommands.RunCmd(Settings.GitCommand, AbortCmd());

            return result;
        }

        public static string AbortCmd()
        {
            return "am --3way --abort";
        }

        static public string Commit(bool amend)
        {
            return GitCommands.RunCmd(Settings.GitCommand, CommitCmd(amend));
        }

        static public string CommitCmd(bool amend)
        {
            //message = message.Replace('\"', '\'');

            var path = Settings.WorkingDirGitDir() + "\\COMMITMESSAGE\"";
            if (amend)
                return "commit --amend -F \"" + path;
            return "commit  -F \"" + path;
        }

        static public string Patch(string patchFile)
        {
            patchFile = FixPath(patchFile);

            Directory.SetCurrentDirectory(Settings.WorkingDir);

            string result = GitCommands.RunCmd(Settings.GitCommand, PatchCmd(patchFile));

            return result;
        }

        public static string PatchCmd(string patchFile)
        {
            patchFile = FixPath(patchFile);

            return "am --3way --signoff \"" + patchFile + "\"";
        }

        public static string PatchDirCmd(string patchDir)
        {
            patchDir = FixPath(patchDir);

            return "am --3way --signoff --directory=\"" + patchDir + "\"";
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

        public static string AddRemote(string name, string location)
        {
            location = FixPath(location);

            if (string.IsNullOrEmpty(name))
                return "Please enter a name.";

            if (string.IsNullOrEmpty(location))
                return RunCmd(Settings.GitCommand, "remote add \"" + name + "\" \"\"");
            return RunCmd(Settings.GitCommand, "remote add \"" + name + "\" \"" + location + "\"");
        }

        public static string[] GetRemotes()
        {
            return RunCmd(Settings.GitCommand, "remote show").Split('\n');
        }

        public static string CleanUpCmd(bool dryrun, bool directories, bool nonignored, bool ignored)
        {
            StringBuilder stringBuilder = new StringBuilder("clean");

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

        private static string FixPathAndEscapeQuotes(string path)
        {
            path = path.Replace("\"", "$QUOTE$");
            path = FixPath(path);
            path = path.Replace("$QUOTE$", "\\\"");
            return path;
        }

        static public ConfigFile GetGlobalConfig()
        {
            return new ConfigFile(Environment.GetEnvironmentVariable("HOME") + "\\.gitconfig");
        }

        public string GetGlobalSetting(string setting)
        {
            ConfigFile configFile = GetGlobalConfig();
            return configFile.GetValue(setting);
        }

        public void SetGlobalSetting(string setting, string value)
        {
            ConfigFile configFile = GetGlobalConfig();
            configFile.SetValue(setting, value);
            configFile.Save();
        }

        static public ConfigFile GetLocalConfig()
        {
            return new ConfigFile(Settings.WorkingDirGitDir() + "\\config");
        }

        static public string GetSetting(string setting)
        {
            ConfigFile configFile = GetLocalConfig();
            return configFile.GetValue(setting);
        }

        static public void UnSetSetting(string setting)
        {
            ConfigFile configFile = GetLocalConfig();
            configFile.RemoveSetting(setting);
            configFile.Save();

        }

        static public void SetSetting(string setting, string value)
        {
            ConfigFile configFile = GetLocalConfig();
            configFile.SetValue(setting, value);
            configFile.Save();
        }

        static public List<Patch> GetStashedItems(string stashName)
        {
            PatchManager patchManager = new PatchManager();
            patchManager.LoadPatch(GitCommands.RunCmd(Settings.GitCommand, "stash show -p " + stashName), false);

            return patchManager.patches;
        }

        static public List<GitStash> GetStashes()
        {
            string[] list = GitCommands.RunCmd(Settings.GitCommand, "stash list").Split('\n');


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

        static public Patch GetSingleDiff(string from, string to, string filter, string extraDiffArguments)
        {
            filter = FixPath(filter);
            from = FixPath(from);
            to = FixPath(to);

            PatchManager patchManager = new PatchManager();
            patchManager.LoadPatch(GitCommands.RunCachableCmd(Settings.GitCommand, "diff" + extraDiffArguments + " \"" + to + "\" \"" + from + "\" -- \"" + filter + "\""), false);

            if (patchManager.patches.Count > 0)
                return patchManager.patches[0];

            return null;
        }

        static public List<Patch> GetDiff(string from, string to, string extraDiffArguments)
        {
            PatchManager patchManager = new PatchManager();
            patchManager.LoadPatch(GitCommands.RunCachableCmd(Settings.GitCommand, "diff" + extraDiffArguments + " \"" + from + "\" \"" + to + "\""), false);

            return patchManager.patches;
        }

        static public List<string> GetDiffFiles(string from)
        {
            string result = RunCmd(Settings.GitCommand, "diff --name-only \"" + from + "\"");

            string[] files = result.Split('\n');

            List<string> retVal = new List<string>();
            foreach (string s in files)
            {
                if (!string.IsNullOrEmpty(s))
                    retVal.Add(s);
            }

            return retVal;
        }

        static public List<GitItemStatus> GetDiffFiles(string from, string to)
        {
            string result = RunCachableCmd(Settings.GitCommand, "diff --name-status \"" + to + "\" \"" + from + "\"");

            string[] files = result.Split('\n');

            List<GitItemStatus> retVal = new List<GitItemStatus>();
            foreach (string s in files)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    GitItemStatus gitItemStatus = new GitItemStatus();
                    gitItemStatus.Name = s.Substring(1).Trim();

                    gitItemStatus.IsNew = s[0] == 'A';
                    gitItemStatus.IsChanged = s[0] == 'M';
                    gitItemStatus.IsDeleted = s[0] == 'D';
                    gitItemStatus.IsTracked = true;
                    retVal.Add(gitItemStatus);
                }
            }

            return retVal;
        }

        static public List<GitItemStatus> GetUntrackedFiles()
        {
            string status = RunCmd(Settings.GitCommand, "ls-files --others --directory --no-empty-directory --exclude-standard");

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
            string status = RunCmd(Settings.GitCommand, "ls-files --modified --exclude-standard");

            string[] statusStrings = status.Split('\n');

            List<GitItemStatus> gitItemStatusList = new List<GitItemStatus>();

            foreach (string statusString in statusStrings)
            {
                if (string.IsNullOrEmpty(statusString.Trim()))
                    continue;
                GitItemStatus itemStatus = new GitItemStatus
                                               {
                                                   IsNew = false,
                                                   IsChanged = true,
                                                   IsDeleted = false,
                                                   IsTracked = true,
                                                   Name = statusString.Trim()
                                               };
                gitItemStatusList.Add(itemStatus);
            }

            return gitItemStatusList;
        }

        public static string GetAllChangedFilesCmd(bool excludeIgnoredFiles, bool showUntrackedFiles)
        {
            StringBuilder stringBuilder = new StringBuilder("ls-files --deleted --modified --no-empty-directory -t");

            if (showUntrackedFiles)
                stringBuilder.Append(" --others");
            if (excludeIgnoredFiles)
                stringBuilder.Append(" --exclude-standard");

            return stringBuilder.ToString();
        }



        static public List<GitItemStatus> GetAllChangedFiles()
        {
            string status = RunCmd(Settings.GitCommand, GetAllChangedFilesCmd(true, true));

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
            string status = RunCmd(Settings.GitCommand, "ls-files --deleted --exclude-standard");

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
            string status = RunCmd(Settings.GitCommand, "diff --cached --numstat -- \"" + filename + "\"");
            if (string.IsNullOrEmpty(status))
                return false;
            return
                true;
        }

        static public List<GitItemStatus> GetStagedFiles()
        {
            string status = RunCmd(Settings.GitCommand, "diff --cached --name-status");

            List<GitItemStatus> gitItemStatusList = new List<GitItemStatus>();

            if (status.Length < 50 && status.Contains("fatal: No HEAD commit to compare"))
            {
                status = RunCmd(Settings.GitCommand, "status --untracked-files=no");

                string[] statusStrings = status.Split('\n');

                foreach (string statusString in statusStrings)
                {
                    if (statusString.StartsWith("#\tnew file:"))
                    {
                        processStatusNewFile(statusString, gitItemStatusList);
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
                status = RunCmd(Settings.GitCommand, "status --untracked=all");
            else
                status = RunCmd(Settings.GitCommand, "status");

            string[] statusStrings = status.Split('\n');

            List<GitItemStatus> gitItemStatusList = new List<GitItemStatus>();

            foreach (string statusString in statusStrings)
            {
                if (statusString.StartsWith("#\tnew file:"))
                {
                    processStatusNewFile(statusString, gitItemStatusList);
                }
                else
                    if (statusString.StartsWith("#\tdeleted:"))
                    {
                        GitItemStatus itemStatus = new GitItemStatus();
                        itemStatus.IsDeleted = true;
                        itemStatus.IsTracked = true;
                        itemStatus.Name = statusString.Substring(statusString.LastIndexOf(':') + 1).Trim();
                        gitItemStatusList.Add(itemStatus);
                    }
                    else
                        if (statusString.StartsWith("#\tmodified:"))
                        {
                            GitItemStatus itemStatus = new GitItemStatus();
                            itemStatus.IsChanged = true;
                            itemStatus.IsTracked = true;
                            itemStatus.Name = statusString.Substring(statusString.LastIndexOf(':') + 1).Trim();
                            gitItemStatusList.Add(itemStatus);
                        }
                        else
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

        private static void processStatusNewFile(string statusString, List<GitItemStatus> gitItemStatusList)
        {
            GitItemStatus itemStatus = new GitItemStatus
                                           {
                                               IsNew = true,
                                               IsTracked = true,
                                               Name = statusString.Substring(statusString.LastIndexOf(':') + 1).Trim()
                                           };
            gitItemStatusList.Add(itemStatus);
        }

        static public string GetCurrentChanges(string name, bool staged, string extraDiffArguments)
        {
            name = FixPath(name);
            var args = "diff" + extraDiffArguments + " -- \"" + name + "\"";
            if (staged)
                args = "diff --cached" + extraDiffArguments + " -- \"" + name + "\"";

            return RunCmd(Settings.GitCommand, args);
        }

        static public string StageFiles(IList<GitItemStatus> files)
        {
            GitCommands gitCommand = new GitCommands();

            string output = "";

            Process process1 = null;
            foreach (GitItemStatus file in files)
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
            foreach (GitItemStatus file in files)
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

        static public string UnstageFiles(List<GitItemStatus> files)
        {
            GitCommands gitCommand = new GitCommands();

            string output = "";

            Process process1 = null;
            foreach (GitItemStatus file in files)
            {
                if (file.IsNew)
                    continue;
                if (process1 == null)
                    process1 = gitCommand.CmdStartProcess(Settings.GitCommand, "update-index --info-only --index-info");

                process1.StandardInput.WriteLine("0 0000000000000000000000000000000000000000\t\"" + FixPath(file.Name) + "\"");
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

        static public string StageFile(string file)
        {
            file = FixPath(file);
            return GitCommands.RunCmd(Settings.GitCommand, "update-index --add" + " \"" + file + "\"");
        }

        static public string StageFileToRemove(string file)
        {
            file = FixPath(file);
            return GitCommands.RunCmd(Settings.GitCommand, "update-index --remove" + " \"" + file + "\"");
        }


        static public string UnstageFile(string file)
        {
            file = FixPath(file);
            return GitCommands.RunCmd(Settings.GitCommand, "rm" + " --cached \"" + file + "\"");
        }

        static public string UnstageFileToRemove(string file)
        {
            file = FixPath(file);
            return GitCommands.RunCmd(Settings.GitCommand, "reset HEAD --" + " \"" + file + "\"");
        }

        static public string GetSelectedBranch()
        {
            string branches = RunCmd(Settings.GitCommand, "branch");
            string[] branchStrings = branches.Split('\n');
            foreach (string branch in branchStrings)
            {
                if (branch.IndexOf('*') > -1)
                    return branch.Trim('*', ' ');
            }
            return "";
        }

        static public List<GitHead> GetRemoteHeads(string remote, bool tags, bool branches)
        {
            remote = FixPath(remote);

            string tree = GetTreeFromRemoteHeands(remote, tags, branches);
            return getHeads(tree);
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

        static public List<GitHead> GetHeads()
        {
            return GetHeads(true);
        }

        static public List<GitHead> GetHeads(bool tags)
        {
            return GetHeads(tags, true);
        }

        static public List<GitHead> GetHeads(bool tags, bool branches)
        {
            string tree = GetTree(tags, branches);
            return getHeads(tree);
        }

        private static string GetTree(bool tags, bool branches)
        {
            if (tags && branches)
                return RunCmd(Settings.GitCommand, "show-ref --dereference");                

            if (tags)
                return RunCmd(Settings.GitCommand, "show-ref --dereference --tags");

            if (branches)
                return RunCmd(Settings.GitCommand, "show-ref --dereference --heads");
            return "";
        }
      
        private static List<GitHead> getHeads(string tree)
        {
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

        static public List<string> GetBranches(bool remotes, string filterRemote)
        {
            string tree = "";
            if (remotes)
                tree = RunCmd(Settings.GitCommand, "branch --verbose --no-abbrev -a");
            else
                tree = RunCmd(Settings.GitCommand, "branch --verbose --no-abbrev");

            List<string> heads = new List<string>();
            string[] itemsStrings = tree.Split('\n');
            foreach (string itemsString in itemsStrings)
            {
                if (itemsString.Length == 0)
                {
                    continue;
                }
                try
                {
                    string head = "";

                    int column = 0;
                    string[] itemColumns = itemsString.Split(" \t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (itemColumns[column] == "*")
                    {
                        column++;
                    }

                    head = itemColumns[column];
                    column++;
                    if (head.Length == 0)
                    {
                        continue;
                    }
                    string guid = itemColumns[column];
                    column++;

                    // If not a valid GUID, skip.
                    if (guid.Trim("1234567890ABCDEFabcdef".ToCharArray()).Length != 0)
                    {
                        continue;
                    }

                    string remote = "";
                    if (head.LastIndexOf("/") > 1)
                    {
                        bool isRemote = head.Contains("remotes/"); ;
                        if (isRemote)
                        {
                            head = head.Substring(head.LastIndexOf("remotes/") + 8);
                        }
                        int index = head.LastIndexOf("/");
                        remote = head.Substring(0, index);
                        head = head.Substring(index + 1);
                    }

                    if (!string.IsNullOrEmpty(filterRemote) && remote != filterRemote)
                    {
                        continue;
                    }

                    heads.Add(head);
                }
                catch { }
            }

            return heads;
        }

        static public string[] GetFiles(string filePattern)
        {
            string tree = RunCmd(Settings.GitCommand, "ls-files -o -m -c \"" + filePattern + "\"");

            return tree.Split('\n');
        }

        static public List<GitItem> GetFileChanges(string file)
        {
            file = FixPath(file);
            string tree = RunCmd(Settings.GitCommand, "whatchanged --all -- \"" + file + "\"");

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
            string tree = RunCachableCmd(Settings.GitCommand, "ls-tree \"" + id + "\"");

            string[] itemsStrings = tree.Split('\n');

            List<IGitItem> items = new List<IGitItem>();

            foreach (string itemsString in itemsStrings)
            {
                GitItem item = new GitItem();

                if (itemsString.Length > 53)
                {

                    item.Mode = itemsString.Substring(0, 6);
                    int guidStart = itemsString.IndexOf(' ', 7);
                    item.ItemType = itemsString.Substring(7, guidStart - 7);
                    item.Guid = itemsString.Substring(guidStart + 1, 40);
                    item.Name = itemsString.Substring(guidStart + 42).Trim();
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
            string[] itemsStrings = RunCmd(Settings.GitCommand, "blame -M -w -l \"" + from + "\" -- \"" + filename + "\"").Split('\n');

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

                    int codeIndex = itemsString.IndexOf(')', 41) + 1;
                    if (codeIndex > 41)
                    {
                        if (lastCommitGuid != commitGuid)
                            item.Author = itemsString.Substring(41, codeIndex - 41).Trim();

                        if (!string.IsNullOrEmpty(item.Text))
                            item.Text += Environment.NewLine;
                        item.Text += itemsString.Substring(codeIndex).Trim(new char[] { '\r' });
                    }

                    lastCommitGuid = commitGuid;
                }
            }


            return items;

        }

        public static string GetFileRevisionText(string file, string revision)
        {
            return RunCachableCmd(Settings.GitCommand, "show " + revision + ":\"" + file.Replace('\\', '/') + "\"");
        }

        public static string GetFileText(string id)
        {
            return RunCachableCmd(Settings.GitCommand, "cat-file blob \"" + id + "\"");
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

                Settings.GitLog.Log(Settings.GitCommand + " " + "cat-file blob \"" + id + "\"");
                //process used to execute external commands

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.ErrorDialog = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardInput = false;
                process.StartInfo.RedirectStandardError = false;

                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.FileName = "\"" + Settings.GitCommand + "\"";
                process.StartInfo.Arguments = "cat-file blob \"" + id + "\"";
                process.StartInfo.WorkingDirectory = Settings.WorkingDir;
                process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                process.StartInfo.LoadUserProfile = true;

                process.Start();

                StreamCopy(process.StandardOutput.BaseStream, newStream);
                newStream.Position = 0;

                process.WaitForExit();

                return newStream;
            }
            catch
            {
            }

            return null;
        }

        public static string GetPreviousCommitMessage(int numberBack)
        {
            return RunCmd(Settings.GitCommand, "log -n 1 HEAD~" + numberBack.ToString() + " --pretty=format:%s%n%n%b");
        }

        public static string MergeBranch(string branch)
        {
            return RunCmd(Settings.GitCommand, MergeBranchCmd(branch, true));
        }

        public static string OpenWithDifftool(string filename)
        {
            string output = "";
            if (GitCommands.VersionInUse.GuiDiffToolExist)
                RunCmdAsync(Settings.GitCommand, "difftool --gui --no-prompt \"" + filename + "\"");
            else
                output = RunCmd(Settings.GitCommand, "difftool --no-prompt \"" + filename + "\"");
            return output;
        }

        public static string OpenWithDifftool(string filename, string revision1, string revision2)
        {
            string output = "";
            if (GitCommands.VersionInUse.GuiDiffToolExist)
                RunCmdAsync(Settings.GitCommand, "difftool --gui --no-prompt " + revision2 + " " + revision1 + " -- \"" + filename + "\"");
            else
                output = RunCmd(Settings.GitCommand, "difftool --no-prompt " + revision2 + " " + revision1 + " -- \"" + filename + "\"");
            return output;
        }

        public static string MergeBranchCmd(string branch, bool allowFastForward)
        {
            if (!allowFastForward)
                return "merge --no-ff \"" + branch + "\"";
            else
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
