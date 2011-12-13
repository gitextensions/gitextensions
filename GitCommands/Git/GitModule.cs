using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using GitCommands.Config;
using PatchApply;

namespace GitCommands
{
    /// <summary>
    /// Class provide non-static methods for manipulation with git module.
    /// You can create several instances for submodules.
    /// </summary>
    public class GitModule
    {
        public GitModule()
        {            
        }

        public GitModule(string workingdir)
        {
            WorkingDir = workingdir;
        }

        private string _workingdir;
        private GitModule _superprojectModule;
        private string _submoduleName;

        public string WorkingDir
        {
            get
            {
                return _workingdir;
            }
            set
            {
                _workingdir = FindGitWorkingDir(value.Trim());
                string superprojectDir = FindGitSuperprojectPath(out _submoduleName);
                if (superprojectDir == null)
                    _superprojectModule = null;
                else
                    _superprojectModule = new GitModule(superprojectDir);
            }
        }

        public string SubmoduleName
        {
            get
            {
                return _submoduleName;
            }
        }

        public GitModule SuperprojectModule
        {
            get
            {
                return _superprojectModule;
            }
        }

        private static string FixPath(string path)
        {
            return GitCommandHelpers.FixPath(path);
        }

        public bool ValidWorkingDir()
        {
            return ValidWorkingDir(_workingdir);
        }

        public static bool ValidWorkingDir(string dir)
        {
            if (string.IsNullOrEmpty(dir))
                return false;

            if (Directory.Exists(dir + Settings.PathSeparator + ".git") || File.Exists(dir + Settings.PathSeparator + ".git"))
                return true;

            return !dir.Contains(".git") &&
                   Directory.Exists(dir + Settings.PathSeparator + "info") &&
                   Directory.Exists(dir + Settings.PathSeparator + "objects") &&
                   Directory.Exists(dir + Settings.PathSeparator + "refs");
        }

        public string GetGitDirectory()
        {
            return GetGitDirectory(_workingdir);
        }

        public bool IsBareRepository()
        {
            return IsBareRepository(_workingdir);
        }

        public string WorkingDirGitDir()
        {
            return WorkingDirGitDir(_workingdir);
        }

        /// <summary>
        /// This is a faster function to get the names of all submodules then the 
        /// GetSubmodules() function. The command @git submodule is very slow.
        /// </summary>
        public IList<string> GetSubmodulesNames()
        {
            IList<string> submodulesNames = new List<string>();
            var configFile = new ConfigFile(_workingdir + ".gitmodules");
            foreach (ConfigSection configSection in configFile.GetConfigSections())
            {
                submodulesNames.Add(configSection.SubSection);
            }

            return submodulesNames;
        }

        public string GetGlobalSetting(string setting)
        {
            var configFile = GitCommandHelpers.GetGlobalConfig();
            return configFile.GetValue(setting);
        }

        public void SetGlobalSetting(string setting, string value)
        {
            var configFile = GitCommandHelpers.GetGlobalConfig();
            configFile.SetValue(setting, value);
            configFile.Save();
        }
        
        public static string FindGitWorkingDir(string startDir)
        {
            if (string.IsNullOrEmpty(startDir))
                return "";

            if (!startDir.EndsWith(Settings.PathSeparator.ToString()) && !startDir.EndsWith(Settings.PathSeparatorWrong.ToString()))
                startDir += Settings.PathSeparator;

            var dir = startDir;

            while (dir.LastIndexOfAny(new[] { Settings.PathSeparator, Settings.PathSeparatorWrong }) > 0)
            {
                dir = dir.Substring(0, dir.LastIndexOfAny(new[] { Settings.PathSeparator, Settings.PathSeparatorWrong }));

                if (ValidWorkingDir(dir))
                    return dir + Settings.PathSeparator;
            }
            return startDir;
        }

        public Encoding GetLogoutputEncoding()
        {
            string encodingString = GetLocalConfig().GetValue("i18n.logoutputencoding");
            if (string.IsNullOrEmpty(encodingString))
                encodingString = GitCommandHelpers.GetGlobalConfig().GetValue("i18n.logoutputencoding");
            if (string.IsNullOrEmpty(encodingString))
                encodingString = GetLocalConfig().GetValue("i18n.commitEncoding");
            if (string.IsNullOrEmpty(encodingString))
                encodingString = GitCommandHelpers.GetGlobalConfig().GetValue("i18n.commitEncoding");
            if (!string.IsNullOrEmpty(encodingString))
            {
                try
                {
                    return Encoding.GetEncoding(encodingString);
                }
                catch (ArgumentException ex)
                {
                    throw new Exception(ex.Message + Environment.NewLine + "Unsupported encoding set in git config file: " + encodingString + Environment.NewLine + "Please check the setting i18n.commitencoding in your local and/or global config files. Command aborted.", ex);
                }
            }

            return Encoding.UTF8;
        }

        public string RunCmd(string cmd)
        {
            return RunCmd(cmd, "");
        }
        
        public void RunRealCmd(string cmd, string arguments)
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

        public void RunGitRealCmd(string arguments)
        {
            RunRealCmd(Settings.GitCommand, arguments);
        }

        public void RunRealCmdDetached(string cmd, string arguments)
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

        private void CreateAndStartCommand(string cmd, string arguments, bool waitForExit)
        {
            GitCommandHelpers.SetEnvironmentVariable();

            Settings.GitLog.Log(cmd + " " + arguments);
            //process used to execute external commands

            var info = new ProcessStartInfo
                           {
                               UseShellExecute = true,
                               ErrorDialog = false,
                               RedirectStandardOutput = false,
                               RedirectStandardInput = false,
                               CreateNoWindow = false,
                               FileName = cmd,
                               Arguments = arguments,
                               WorkingDirectory = _workingdir,
                               WindowStyle = ProcessWindowStyle.Normal,
                               LoadUserProfile = true
                           };

            if (waitForExit)
            {
                using (var process = Process.Start(info))
                {
                    process.WaitForExit();
                }
            }
            else
            {
                Process.Start(info);
            }
        }

        public void StartExternalCommand(string cmd, string arguments)
        {
            try
            {
                GitCommandHelpers.SetEnvironmentVariable();

                var processInfo = new ProcessStartInfo
                                      {
                                          UseShellExecute = false,
                                          RedirectStandardOutput = false,
                                          FileName = cmd,
                                          WorkingDirectory = _workingdir,
                                          Arguments = arguments,
                                          CreateNoWindow = true
                                      };

                using (var process = new Process { StartInfo = processInfo })
                {
                    process.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public string RunCachableCmd(string cmd, string arguments, Encoding encoding)
        {
            if (encoding == null) 
                encoding = Settings.Encoding;

            string output;
            if (GitCommandCache.TryGet(arguments, encoding, out output))
                return output;

            byte[] cmdout, cmderr;
            RunCmdByte(cmd, arguments, out cmdout, out cmderr);

            GitCommandCache.Add(arguments, cmdout, cmderr);

            return EncodingHelper.GetString(cmdout, cmderr, encoding);
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public string RunCachableCmd(string cmd, string arguments)
        {
            return RunCachableCmd(cmd, arguments, Settings.Encoding);
        }


        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public string RunCmd(string cmd, string arguments)
        {
            return RunCmd(cmd, arguments, null);
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public string RunCmd(string cmd, string arguments, string stdInput)
        {
            int exitCode;
            return RunCmd(cmd, arguments, out exitCode, stdInput);
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public string RunCmd(string cmd, string arguments, out int exitCode)
        {
            return RunCmd(cmd, arguments, out exitCode, null);
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public string RunCmd(string cmd, string arguments, out int exitCode, string stdInput)
        {
            byte[] output, error;
            exitCode = RunCmdByte(cmd, arguments, stdInput, out output, out error);
            return EncodingHelper.GetString(output, error, Settings.Encoding);
        }

        private  int RunCmdByte(string cmd, string arguments, out byte[] output, out byte[] error)
        {
            return RunCmdByte(cmd, arguments, null, out output, out error);
        }
        private int RunCmdByte(string cmd, string arguments, string stdInput, out byte[] output, out byte[] error)
        {
            try
            {
                GitCommandHelpers.SetEnvironmentVariable();
                arguments = arguments.Replace("$QUOTE$", "\\\"");
                int exitCode = GitCommandHelpers.CreateAndStartProcess(arguments, cmd, out output, out error, stdInput);
                return exitCode;
            }
            catch (Win32Exception)
            {
                output = error = null;
                return 1;
            }

        }

        public string RunGitCmd(string arguments, string stdInput)
        {
            return RunCmd(Settings.GitCommand, arguments, stdInput);
        }

        public string RunGitCmd(string arguments)
        {
            return RunCmd(Settings.GitCommand, arguments, null);
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public void RunCmdAsync(string cmd, string arguments)
        {
            GitCommandHelpers.SetEnvironmentVariable();

            Settings.GitLog.Log(cmd + " " + arguments);
            //process used to execute external commands

            var info = new ProcessStartInfo
                           {
                               UseShellExecute = true,
                               ErrorDialog = true,
                               RedirectStandardOutput = false,
                               RedirectStandardInput = false,
                               RedirectStandardError = false,

                               LoadUserProfile = true,
                               CreateNoWindow = false,
                               FileName = cmd,
                               Arguments = arguments,
                               WorkingDirectory = _workingdir,
                               WindowStyle = ProcessWindowStyle.Hidden
                           };

            try
            {
                Process.Start(info);
            }
            catch (Win32Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public void EditNotes(string revision)
        {
            if (GitCommandHelpers.GetGlobalConfig().GetValue("core.editor").ToLower().Contains("gitextensions") ||
                GetLocalConfig().GetValue("core.editor").ToLower().Contains("gitextensions") ||
                GitCommandHelpers.GetGlobalConfig().GetValue("core.editor").ToLower().Contains("notepad") ||
                GetLocalConfig().GetValue("core.editor").ToLower().Contains("notepad") ||
                GitCommandHelpers.GetGlobalConfig().GetValue("core.editor").ToLower().Contains("notepad++") ||
                GetLocalConfig().GetValue("core.editor").ToLower().Contains("notepad++"))
            {
                RunCmd(Settings.GitCommand, "notes edit " + revision);
            }
            else
            {
                RunRealCmd(Settings.GitCommand, "notes edit " + revision);
            }
        }

        public bool InTheMiddleOfConflictedMerge()
        {
            return !string.IsNullOrEmpty(RunCmd(Settings.GitCommand, "ls-files -z --unmerged"));
        }

        public List<GitItem> GetConflictedFiles()
        {
            var unmergedFiles = new List<GitItem>();

            var fileName = "";
            foreach (var file in GetUnmergedFileListing())
            {
                if (file.IndexOf('\t') <= 0)
                    continue;
                if (file.Substring(file.IndexOf('\t') + 1) == fileName)
                    continue;
                fileName = file.Substring(file.IndexOf('\t') + 1);
                unmergedFiles.Add(new GitItem { FileName = fileName });
            }

            return unmergedFiles;
        }

        private IEnumerable<string> GetUnmergedFileListing()
        {
            return RunCmd(Settings.GitCommand, "ls-files -z --unmerged").Split(new[] { '\0', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public bool HandleConflictSelectBase(string fileName)
        {
            if (!HandleConflictsSaveSide(fileName, fileName, "1"))
                return false;

            RunCmd(Settings.GitCommand, "add -- \"" + fileName + "\"");
            return true;
        }

        public bool HandleConflictSelectLocal(string fileName)
        {
            if (!HandleConflictsSaveSide(fileName, fileName, "2"))
                return false;

            RunCmd(Settings.GitCommand, "add -- \"" + fileName + "\"");
            return true;
        }

        public bool HandleConflictSelectRemote(string fileName)
        {
            if (!HandleConflictsSaveSide(fileName, fileName, "3"))
                return false;

            RunCmd(Settings.GitCommand, "add -- \"" + fileName + "\"");
            return true;
        }

        public bool HandleConflictsSaveSide(string fileName, string saveAs, string side)
        {
            Directory.SetCurrentDirectory(_workingdir);

            side = GetSide(side);

            fileName = FixPath(fileName);
            var unmerged = RunCmd(Settings.GitCommand, "ls-files -z --unmerged \"" + fileName + "\"").Split(new char[] { '\0', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var file in unmerged)
            {
                string fileStage = null;
                int findSecondWhitespace = file.IndexOfAny(new[] { ' ', '\t' });
                if (findSecondWhitespace >= 0) fileStage = file.Substring(findSecondWhitespace).Trim();
                findSecondWhitespace = fileStage.IndexOfAny(new[] { ' ', '\t' });
                if (findSecondWhitespace >= 0) fileStage = fileStage.Substring(findSecondWhitespace).Trim();
                if (string.IsNullOrEmpty(fileStage))
                    continue;
                if (fileStage.Trim()[0] != side[0])
                    continue;


                var fileline = file.Split(new[] { ' ', '\t' });
                if (fileline.Length < 3)
                    continue;
                Directory.SetCurrentDirectory(_workingdir);
                SaveBlobAs(saveAs, fileline[1]);
                return true;
            }
            return false;
        }

        public void SaveBlobAs(string saveAs, string blob)
        {
            using (var ms = (MemoryStream)GetFileStream(blob)) //Ugly, has implementation info.
            {
                byte[] buf;
                ConfigFile localConfig = GetLocalConfig();
                bool convertcrlf = false;
                if (localConfig.HasValue("core.autocrlf"))
                {
                    convertcrlf = localConfig.GetValue("core.autocrlf").Equals("true", StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    ConfigFile globalConfig = GitCommandHelpers.GetGlobalConfig();
                    convertcrlf = globalConfig.GetValue("core.autocrlf").Equals("true", StringComparison.OrdinalIgnoreCase);
                }

                buf = ms.ToArray();
                if (convertcrlf)
                {
                    if (!FileHelper.IsBinaryFile(saveAs) &&
                        !FileHelper.IsBinaryFileAccordingToContent(buf))
                    {
                        buf = null;
                        StreamReader reader = new StreamReader(ms, Settings.Encoding);
                        String sfileout = reader.ReadToEnd();
                        sfileout = sfileout.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
                        buf = Settings.Encoding.GetBytes(sfileout);
                    }
                }

                using (FileStream fileOut = File.Create(saveAs))
                {
                    fileOut.Write(buf, 0, buf.Length);
                }
            }
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

        public string[] GetConflictedFiles(string filename)
        {
            Directory.SetCurrentDirectory(_workingdir);

            filename = FixPath(filename);

            string[] fileNames =
                {
                    filename + ".BASE",
                    filename + ".LOCAL",
                    filename + ".REMOTE"
                };

            var unmerged = RunCmd(Settings.GitCommand, "ls-files -z --unmerged \"" + filename + "\"").Split(new char[] { '\0', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var file in unmerged)
            {
                string fileStage = null;
                int findSecondWhitespace = file.IndexOfAny(new[] { ' ', '\t' });
                if (findSecondWhitespace >= 0) fileStage = file.Substring(findSecondWhitespace).Trim();
                findSecondWhitespace = fileStage.IndexOfAny(new[] { ' ', '\t' });
                if (findSecondWhitespace >= 0) fileStage = fileStage.Substring(findSecondWhitespace).Trim();
                if (string.IsNullOrEmpty(fileStage))
                    continue;

                int stage;
                if (!Int32.TryParse(fileStage.Trim()[0].ToString(), out stage))
                    continue;

                var tempFile = RunCmd(Settings.GitCommand, "checkout-index --temp --stage=" + stage + " -- " + "\"" + filename + "\"");
                tempFile = tempFile.Split('\t')[0];
                tempFile = Path.Combine(_workingdir, tempFile);

                var newFileName = Path.Combine(_workingdir, fileNames[stage - 1]);
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

            if (!File.Exists(fileNames[0])) fileNames[0] = null;
            if (!File.Exists(fileNames[1])) fileNames[1] = null;
            if (!File.Exists(fileNames[2])) fileNames[2] = null;

            return fileNames;
        }

        public string[] GetConflictedFileNames(string filename)
        {
            filename = FixPath(filename);

            var fileNames = new string[3];

            var unmerged = RunCmd(Settings.GitCommand, "ls-files -z --unmerged \"" + filename + "\"").Split(new[] { '\0', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var file in unmerged)
            {
                int findSecondWhitespace = file.IndexOfAny(new[] { ' ', '\t' });
                string fileStage = findSecondWhitespace >= 0 ? file.Substring(findSecondWhitespace).Trim() : "";

                findSecondWhitespace = fileStage.IndexOfAny(new[] { ' ', '\t' });

                fileStage = findSecondWhitespace >= 0 ? fileStage.Substring(findSecondWhitespace).Trim() : "";

                int stage;
                if (Int32.TryParse(fileStage.Trim()[0].ToString(), out stage) && stage >= 1 && stage <= 3 && fileStage.Length > 2)
                {
                    fileNames[stage - 1] = fileStage.Substring(2);
                }
            }

            return fileNames;
        }

        public static string GetGitDirectory(string repositoryPath)
        {
            if (File.Exists(repositoryPath + ".git"))
            {
                var lines = File.ReadAllLines(repositoryPath + ".git");
                foreach(string line in lines)
                {
                    if (line.StartsWith("gitdir:"))
                    {
                        string path = line.Substring(7).Trim().Replace('/', '\\');
                        return path + Settings.PathSeparator;
                    }
                }
            }
            return repositoryPath + ".git" + Settings.PathSeparator;
        }

        public string GetMergeMessage()
        {
            var file = GetGitDirectory() + "MERGE_MSG";

            return
                File.Exists(file)
                    ? File.ReadAllText(file)
                    : "";
        }

        public void RunGitK()
        {
            if (Settings.RunningOnUnix())
            {
                RunRealCmdDetached("gitk", "");
            }
            else
            {
                StartExternalCommand("cmd.exe", "/c \"\"" + Settings.GitCommand.Replace("git.cmd", "gitk.cmd")
                                                              .Replace("bin\\git.exe", "cmd\\gitk.cmd")
                                                              .Replace("bin/git.exe", "cmd/gitk.cmd") + "\" --branches --tags --remotes\"");
            }
        }

        public void RunGui()
        {
            if (Settings.RunningOnUnix())
            {
                RunRealCmdDetached("git", "gui");
            }
            else
            {
                StartExternalCommand("cmd.exe", "/c \"\"" + Settings.GitCommand + "\" gui\"");
            }
        }

        public void RunBash()
        {
            if (Settings.RunningOnUnix())
            {
                string[] termEmuCmds =
                {
                    "gnome-terminal",
                    "konsole",
                    "Terminal",
                    "xterm"
                };
                
                string args = "";
                string cmd = null;
                
                foreach (var termEmuCmd in termEmuCmds)
                {
                    if (!string.IsNullOrEmpty(RunCmd("which", termEmuCmd)))
                    {
                        cmd = termEmuCmd;
                        break;
                    }
                }
                
                if (string.IsNullOrEmpty(cmd))
                {
                    cmd = "bash";
                    args = "--login -i";
                }

                RunRealCmdDetached(cmd, args);
            }
            else
            {
                if (File.Exists(Settings.GitBinDir + "bash.exe"))
                    RunRealCmdDetached("cmd.exe", "/c \"\"" + Settings.GitBinDir + "bash\" --login -i\"");
                else
                    RunRealCmdDetached("cmd.exe", "/c \"\"" + Settings.GitBinDir + "sh\" --login -i\"");
            }
        }

        public string Init(bool bare, bool shared)
        {
            if (bare && shared)
                return RunCmd(Settings.GitCommand, "init --bare --shared=all");
            if (bare)
                return RunCmd(Settings.GitCommand, "init --bare");
            return RunCmd(Settings.GitCommand, "init");
        }

        public bool IsMerge(string commit)
        {
            string output = RunCmd(Settings.GitCommand, "log -n 1 --format=format:%P \"" + commit + "\"");
            string[] parents = output.Split(' ');
            if (parents.Length > 1) return true;
            return false;
        }

        public GitRevision[] GetParents(string commit)
        {
            string output = RunCmd(Settings.GitCommand, "log -n 1 --format=format:%P \"" + commit + "\"");
            string[] Parents = output.Split(' ');
            var ParentsRevisions = new GitRevision[Parents.Length];
            for (int i = 0; i < Parents.Length; i++)
            {
                const string formatString =
                    /* Tree           */ "%T%n" +
                    /* Author Name    */ "%aN%n" +
                    /* Author Date    */ "%ai%n" +
                    /* Committer Name */ "%cN%n" +
                    /* Committer Date */ "%ci%n" +
                    /* Commit Message */ "%s";
                string cmd = "log -n 1 --format=format:" + formatString + " " + Parents[i];
                var RevInfo = RunCmd(Settings.GitCommand, cmd);
                string[] Infos = RevInfo.Split('\n');
                var Revision = new GitRevision(Parents[i])
                {
                    TreeGuid = Infos[0],
                    Author = Infos[1],
                    Committer = Infos[3],
                    Message = Infos[5]
                };
                DateTime Date;
                DateTime.TryParse(Infos[2], out Date);
                Revision.AuthorDate = Date;
                DateTime.TryParse(Infos[4], out Date);
                Revision.CommitDate = Date;
                ParentsRevisions[i] = Revision;
            }
            return ParentsRevisions;
        }

        public string CherryPick(string cherry, bool commit, string arguments)
        {
            return RunCmd(Settings.GitCommand, GitCommandHelpers.CherryPickCmd(cherry, commit, arguments));
        }

        public string ShowSha1(string sha1)
        {
            return this.RunCachableCmd(Settings.GitCommand, "show --encoding=" + Settings.Encoding.HeaderName + " " + sha1);
        }

        public string UserCommitCount()
        {
            return RunCmd(Settings.GitCommand, "shortlog -s -n");
        }

        public string DeleteBranch(string branchName, bool force, bool remoteBranch)
        {
            return RunCmd(Settings.GitCommand, GitCommandHelpers.DeleteBranchCmd(branchName, force, remoteBranch));
        }

        public string DeleteTag(string tagName)
        {
            return RunCmd(Settings.GitCommand, GitCommandHelpers.DeleteTagCmd(tagName));
        }

        public string GetCurrentCheckout()
        {
            return RunCmd(Settings.GitCommand, "log -g -1 HEAD --pretty=format:%H");
        }

        public string GetSuperprojectCurrentCheckout()
        {
            if (_superprojectModule == null)
                return "";

            var lines = _superprojectModule.RunGitCmd("submodule status --cached " + _submoduleName).Split('\n');

            if (lines.Length == 0)
                return "";

            string submodule = lines[0];
            if (submodule.Length < 43)
                return "";

            var currentCommitGuid = submodule.Substring(1, 40).Trim();
            return currentCommitGuid;
        }

        public int CommitCount()
        {
            int count;
            var arguments = "/c \"\"" + Settings.GitCommand + "\" rev-list --all --abbrev-commit | wc -l\"";
            return
                int.TryParse(RunCmd("cmd.exe", arguments), out count)
                    ? count
                    : 0;
        }

        public bool IsMergeCommit(string commitId)
        {
            return ExistsMergeCommit(commitId+"~1", commitId);
        }

        public bool ExistsMergeCommit(string startRev, string endRev)
        {
            string revisions = RunCmd(Settings.GitCommand, "rev-list --parents --no-walk " + startRev + ".." + endRev);
            string[] revisionsTab = revisions.Split('\n');
            foreach (string parents in revisionsTab)
            {
                string[] parentsTab = parents.Split(' ');
                if (parentsTab.Length > 2)
                    return true;
            }
            return false;
        }
        
        public string GetSubmoduleRemotePath(string name)
        {
            var configFile = new ConfigFile(_workingdir + ".gitmodules");
            return configFile.GetValue("submodule." + name.Trim() + ".url").Trim();
        }

        public string GetSubmoduleLocalPath(string name)
        {
            var configFile = new ConfigFile(_workingdir + ".gitmodules");
            return configFile.GetValue("submodule." + name.Trim() + ".path").Trim();
        }

        public string GetSubmoduleFullPath(string name)
        {
            return _workingdir + FixPath(GetSubmoduleLocalPath(name)) + Settings.PathSeparator;
        }

        public string FindGitSuperprojectPath(out string submoduleName)
        {
            submoduleName = null;
            if (String.IsNullOrEmpty(_workingdir))
                return null;

            string superprojectPath = null;
            if (File.Exists(_workingdir + ".git"))
            {
                var lines = File.ReadAllLines(_workingdir + ".git");
                foreach (string line in lines)
                {
                    if (line.StartsWith("gitdir:"))
                    {
                        string gitpath = line.Substring(7).Trim();
                        int pos = gitpath.IndexOf("/.git/");
                        if (pos != -1)
                        {
                            gitpath = gitpath.Substring(0, pos + 1).Replace('/', '\\');
                            if (File.Exists(gitpath + ".gitmodules") && ValidWorkingDir(gitpath))
                                superprojectPath = gitpath;
                        }
                    }
                }
            }

            string currentPath = Path.GetDirectoryName(_workingdir); // remove last slash
            if (!string.IsNullOrEmpty(currentPath) &&
                superprojectPath == null)
            {
                string path = Path.GetDirectoryName(currentPath);
                if (!string.IsNullOrEmpty(path) &&
                    (!File.Exists(path + Settings.PathSeparator + ".gitmodules") || !ValidWorkingDir(path + Settings.PathSeparator)))
                {
                    // Check upper directory
                    path = Path.GetDirectoryName(path);
                    if (!File.Exists(path + Settings.PathSeparator + ".gitmodules") || !ValidWorkingDir(path + Settings.PathSeparator))
                        return null;
                }
                superprojectPath = path + Settings.PathSeparator;
            }

            if (!string.IsNullOrEmpty(superprojectPath))
            {
                var localPath = currentPath.Substring(superprojectPath.Length);
                var configFile = new ConfigFile(superprojectPath + ".gitmodules");
                foreach (ConfigSection configSection in configFile.GetConfigSections())
                {
                    if (configSection.GetValue("path") == localPath)
                    {
                        submoduleName = configSection.SubSection;
                        return superprojectPath;
                    }
                }
            }

            return null;
        }
        
        internal static GitSubmodule CreateGitSubmodule(string submodule)
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
                gitSubmodule.Name = name.Substring(0, name.IndexOf("(")).TrimEnd();
                gitSubmodule.Branch = name.Substring(name.IndexOf("(")).Trim(new[] { '(', ')', ' ' });
            }
            else
                gitSubmodule.Name = name;
            return gitSubmodule;
        }

        public string GetSubmoduleSummary(string submodule)
        {
            var arguments = string.Format("submodule summary {0}", submodule);
            return RunCmd(Settings.GitCommand, arguments);
        }

        public string Stash()
        {
            return RunCmd(Settings.GitCommand, "stash save");
        }

        public string StashApply()
        {
            return RunCmd(Settings.GitCommand, "stash apply");
        }

        public string StashClear()
        {
            return RunCmd(Settings.GitCommand, "stash clear");
        }

        public string ResetSoft(string commit)
        {
            return ResetSoft(commit, "");
        }

        public string ResetMixed(string commit)
        {
            return ResetMixed(commit, "");
        }

        public string ResetHard(string commit)
        {
            return ResetHard(commit, "");
        }

        public string ResetSoft(string commit, string file)
        {
            var args = "reset --soft";

            if (!string.IsNullOrEmpty(commit))
                args += " \"" + commit + "\"";

            if (!string.IsNullOrEmpty(file))
                args += " -- \"" + file + "\"";

            return RunCmd(Settings.GitCommand, args);
        }

        public string ResetMixed(string commit, string file)
        {
            var args = "reset --mixed";

            if (!string.IsNullOrEmpty(commit))
                args += " \"" + commit + "\"";

            if (!string.IsNullOrEmpty(file))
                args += " -- \"" + file + "\"";

            return RunCmd(Settings.GitCommand, args);
        }

        public string ResetHard(string commit, string file)
        {
            var args = "reset --hard";

            if (!string.IsNullOrEmpty(commit))
                args += " \"" + commit + "\"";

            if (!string.IsNullOrEmpty(file))
                args += " -- \"" + file + "\"";

            return RunCmd(Settings.GitCommand, args);
        }

        public string ResetFile(string file)
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


        public string Tag(string tagName, string revision, bool annotation)
        {
            string result;

            if (annotation)
                result = RunCmd(Settings.GitCommand,
                                "tag \"" + tagName.Trim() + "\" -a -F \"" + WorkingDirGitDir() +
                                "\\TAGMESSAGE\" -- \"" + revision + "\"");
            else
                result = RunCmd(Settings.GitCommand, "tag \"" + tagName.Trim() + "\" \"" + revision + "\"");

            return result;
        }

        public string Branch(string branchName, string revision, bool checkout)
        {
            var result = RunCmd(Settings.GitCommand, GitCommandHelpers.BranchCmd(branchName, revision, checkout));

            return result;
        }

        public string Push(string path)
        {
            return RunCmd(Settings.GitCommand, "push \"" + FixPath(path).Trim() + "\"");
        }

        public bool StartPageantForRemote(string remote)
        {
            var sshKeyFile = GetPuttyKeyFileForRemote(remote);
            if (string.IsNullOrEmpty(sshKeyFile))
                return false;

            StartPageantWithKey(sshKeyFile);
            return true;
        }

        public void StartPageantWithKey(string sshKeyFile)
        {
            StartExternalCommand(Settings.Pageant, "\"" + sshKeyFile + "\"");
        }

        public string GetPuttyKeyFileForRemote(string remote)
        {
            if (string.IsNullOrEmpty(remote) ||
                string.IsNullOrEmpty(Settings.Pageant) ||
                !Settings.AutoStartPageant ||
                !GitCommandHelpers.Plink())
                return "";

            return GetSetting("remote." + remote + ".puttykeyfile");
        }

        public string Fetch(string remote, string branch)
        {
            remote = FixPath(remote);

            Directory.SetCurrentDirectory(_workingdir);

            RunRealCmd("cmd.exe", " /k \"\"" + Settings.GitCommand + "\" " + FetchCmd(remote, null, branch) + "\"");

            return "Done";
        }

        public static bool PathIsUrl(string path)
        {
            return path.Contains(Settings.PathSeparator.ToString()) || path.Contains(Settings.PathSeparatorWrong.ToString());
        }

        public string FetchCmd(string remote, string remoteBranch, string localBranch)
        {
            if (string.IsNullOrEmpty(remote) && string.IsNullOrEmpty(remoteBranch) && string.IsNullOrEmpty(localBranch))
                return "fetch";

            return "fetch " + GetFetchArgs(remote, remoteBranch, localBranch);
        }

        public string Pull(string remote, string remoteBranch, string localBranch, bool rebase)
        {
            remote = FixPath(remote);

            Directory.SetCurrentDirectory(_workingdir);

            RunRealCmd("cmd.exe", " /k \"\"" + Settings.GitCommand + "\" " + PullCmd(remote, localBranch, remoteBranch, rebase) + "\"");

            return "Done";
        }

        public string PullCmd(string remote, string remoteBranch, string localBranch, bool rebase)
        {
            if (rebase && !string.IsNullOrEmpty(remoteBranch))
                return "pull --rebase " + remote + " refs/heads/" + remoteBranch;

            if (rebase)
                return "pull --rebase " + remote;

            return "pull " + GetFetchArgs(remote, remoteBranch, localBranch);
        }

        private string GetFetchArgs(string remote, string remoteBranch, string localBranch)
        {
            remote = FixPath(remote);

            //Remove spaces... 
            if (remoteBranch != null)
                remoteBranch = remoteBranch.Replace(" ", "");
            if (localBranch != null)
                localBranch = localBranch.Replace(" ", "");

            string remoteBranchArguments;

            if (string.IsNullOrEmpty(remoteBranch))
                remoteBranchArguments = "";
            else
                remoteBranchArguments = "+refs/heads/" + remoteBranch + "";

            string localBranchArguments;
            var remoteUrl = GetSetting("remote." + remote + ".url");

            if (PathIsUrl(remote) && !string.IsNullOrEmpty(localBranch) && string.IsNullOrEmpty(remoteUrl))
                localBranchArguments = ":refs/heads/" + localBranch + "";
            else if (string.IsNullOrEmpty(localBranch) || PathIsUrl(remote) || string.IsNullOrEmpty(remoteUrl))
                localBranchArguments = "";
            else
                localBranchArguments = ":" + "refs/remotes/" + remote.Trim() + "/" + localBranch + "";

            var progressOption = "";
            if (GitCommandHelpers.VersionInUse.FetchCanAskForProgress)
                progressOption = "--progress ";

            return progressOption + "\"" + remote.Trim() + "\" " + remoteBranchArguments + localBranchArguments;
        }

        public string ContinueRebase()
        {
            Directory.SetCurrentDirectory(_workingdir);

            var result = RunCmd(Settings.GitCommand, GitCommandHelpers.ContinueRebaseCmd());

            return result;
        }

        public string SkipRebase()
        {
            Directory.SetCurrentDirectory(_workingdir);

            var result = RunCmd(Settings.GitCommand, GitCommandHelpers.SkipRebaseCmd());

            return result;
        }

        public string GetRebaseDir()
        {
            string gitDirectory = GetGitDirectory();
            if (Directory.Exists(gitDirectory + "rebase-merge" + Settings.PathSeparator))
                return gitDirectory + "rebase-merge" + Settings.PathSeparator;
            if (Directory.Exists(gitDirectory + "rebase-apply" + Settings.PathSeparator))
                return gitDirectory + "rebase-apply" + Settings.PathSeparator;
            if (Directory.Exists(gitDirectory + "rebase" + Settings.PathSeparator))
                return gitDirectory + "rebase" + Settings.PathSeparator;

            return "";
        }

        public bool InTheMiddleOfBisect()
        {
            return File.Exists(WorkingDirGitDir() + Settings.PathSeparator + "BISECT_START");
        }


        public bool InTheMiddleOfRebase()
        {
            return !File.Exists(GetRebaseDir() + "applying") &&
                   Directory.Exists(GetRebaseDir());
        }

        public bool InTheMiddleOfPatch()
        {
            return !File.Exists(GetRebaseDir() + "rebasing") &&
                   Directory.Exists(GetRebaseDir());
        }


        public string GetNextRebasePatch()
        {
            var file = GetRebaseDir() + "next";
            return File.Exists(file) ? File.ReadAllText(file).Trim() : "";
        }

        public List<PatchFile> GetRebasePatchFiles()
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
                var file = fullFileName.Substring(fullFileName.LastIndexOf(Settings.PathSeparator) + 1);
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

        public string Rebase(string branch)
        {
            Directory.SetCurrentDirectory(_workingdir);

            return RunCmd(Settings.GitCommand, GitCommandHelpers.RebaseCmd(branch, false, false, false));
        }

        public string AbortRebase()
        {
            Directory.SetCurrentDirectory(_workingdir);

            return RunCmd(Settings.GitCommand, GitCommandHelpers.AbortRebaseCmd());
        }

        public string Resolved()
        {
            Directory.SetCurrentDirectory(_workingdir);

            return RunCmd(Settings.GitCommand, GitCommandHelpers.ResolvedCmd());
        }

        public string Skip()
        {
            Directory.SetCurrentDirectory(_workingdir);

            return RunCmd(Settings.GitCommand, GitCommandHelpers.SkipCmd());
        }

        public string Abort()
        {
            Directory.SetCurrentDirectory(_workingdir);

            return RunCmd(Settings.GitCommand, GitCommandHelpers.AbortCmd());
        }

        public string Commit(bool amend)
        {
            return Commit(amend, "");
        }

        public string Commit(bool amend, string author)
        {
            return RunCmd(Settings.GitCommand, CommitCmd(amend, author));
        }

        public string CommitCmd(bool amend)
        {
            return CommitCmd(amend, "");
        }

        public string CommitCmd(bool amend, string author)
        {
            string command = "commit";
            if (amend)
                command += " --amend";

            if (!string.IsNullOrEmpty(author))
                command += " --author=\"" + author + "\"";

            var path = WorkingDirGitDir() + Settings.PathSeparator + "COMMITMESSAGE\"";
            command += " -F \"" + path;

            return command;
        }

        public string Patch(string patchFile)
        {
            Directory.SetCurrentDirectory(_workingdir);

            return RunCmd(Settings.GitCommand, GitCommandHelpers.PatchCmd(FixPath(patchFile)));
        }

        public string UpdateRemotes()
        {
            return RunCmd(Settings.GitCommand, "remote update");
        }

        public string RemoveRemote(string name)
        {
            return RunCmd(Settings.GitCommand, "remote rm \"" + name + "\"");
        }

        public string RenameRemote(string name, string newName)
        {
            return RunCmd(Settings.GitCommand, "remote rename \"" + name + "\" \"" + newName + "\"");
        }

        public string Rename(string name, string newName)
        {
            return RunCmd(Settings.GitCommand, "branch -m \"" + name + "\" \"" + newName + "\"");
        }

        public string AddRemote(string name, string path)
        {
            var location = FixPath(path);

            if (string.IsNullOrEmpty(name))
                return "Please enter a name.";

            return
                string.IsNullOrEmpty(location)
                    ? RunCmd(Settings.GitCommand, string.Format("remote add \"{0}\" \"\"", name))
                    : RunCmd(Settings.GitCommand, string.Format("remote add \"{0}\" \"{1}\"", name, location));
        }

        public string[] GetRemotes()
        {
            return GetRemotes(true);
        }

        public string[] GetRemotes(bool allowEmpty)
        {
            string remotes = RunCmd(Settings.GitCommand, "remote show");
            if (allowEmpty)
                return remotes.Split('\n');
            else
                return remotes.Split(new char[]{'\n'}, StringSplitOptions.RemoveEmptyEntries);
            
        }

        public ConfigFile GetLocalConfig()
        {
            return new ConfigFile(WorkingDirGitDir() + Settings.PathSeparator + "config");
        }

        public string GetSetting(string setting)
        {
            var configFile = GetLocalConfig();
            return configFile.GetValue(setting);
        }

        public string GetEffectiveSetting(string setting)
        {
            var localConfig = GetLocalConfig();
            if (localConfig.HasValue(setting))
                return localConfig.GetValue(setting);

            return GitCommandHelpers.GetGlobalConfig().GetValue(setting);
        }

        public void UnsetSetting(string setting)
        {
            var configFile = GetLocalConfig();
            configFile.RemoveSetting(setting);
            configFile.Save();
        }

        public void SetSetting(string setting, string value)
        {
            var configFile = GetLocalConfig();
            configFile.SetValue(setting, value);
            configFile.Save();
        }

        public List<Patch> GetStashedItems(string stashName)
        {
            var patchManager = new PatchManager();
            patchManager.LoadPatch(RunCmd(Settings.GitCommand, "stash show -p " + stashName), false);

            return patchManager.Patches;
        }

        public List<GitStash> GetStashes()
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

        public Patch GetSingleDiff(string @from, string to, string fileName, string oldFileName, string extraDiffArguments, Encoding encoding)
        {
            if (!string.IsNullOrEmpty(fileName))
                fileName = string.Concat("\"", FixPath(fileName), "\"");

            if (!string.IsNullOrEmpty(oldFileName))
                oldFileName = string.Concat("\"", FixPath(oldFileName), "\"");

            from = FixPath(from);
            to = FixPath(to);

            if (Settings.UsePatienceDiffAlgorithm)
                extraDiffArguments = string.Concat(extraDiffArguments, " --patience");

            var patchManager = new PatchManager();
            var arguments = string.Format("diff{0} -M -C \"{1}\" \"{2}\" -- {3} {4}", extraDiffArguments, to, from, fileName, oldFileName);
            patchManager.LoadPatch(this.RunCachableCmd(Settings.GitCommand, arguments, encoding), false);

            return patchManager.Patches.Count > 0 ? patchManager.Patches[0] : null;
        }

        public Patch GetSingleDiff(string @from, string to, string fileName, string extraDiffArguments, Encoding encoding)
        {
            return this.GetSingleDiff(from, to, fileName, null, extraDiffArguments, encoding);
        }

        public List<Patch> GetDiff(string from, string to, string extraDiffArguments)
        {
            if (Settings.UsePatienceDiffAlgorithm)
                extraDiffArguments = string.Concat(extraDiffArguments, " --patience");

            var patchManager = new PatchManager();
            var arguments = string.Format("diff{0} \"{1}\" \"{2}\"", extraDiffArguments, from, to);
            patchManager.LoadPatch(this.RunCachableCmd(Settings.GitCommand, arguments), false);

            return patchManager.Patches;
        }

        public List<GitItemStatus> GetDiffFiles(string from, string to)
        {
            return GetDiffFiles(from, to, false);
        }

        public List<GitItemStatus> GetDiffFiles(string from, string to, bool noCache)
        {
            string result;
            string cmd = "diff -M -C -z --name-status \"" + to + "\" \"" + from + "\"";
            if (noCache)
            {
                result = RunCmd(Settings.GitCommand, cmd);
            }
            else
            {
                result = this.RunCachableCmd(Settings.GitCommand, cmd);
            }
            return GitCommandHelpers.GetAllChangedFilesFromString(result, true);
        }

        public List<GitItemStatus> GetUntrackedFiles()
        {
            var status = RunCmd(Settings.GitCommand,
                                "ls-files -z --others --directory --no-empty-directory --exclude-standard");

            var statusStrings = status.Split(new char[] { '\0', '\n' }, StringSplitOptions.RemoveEmptyEntries);

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

        public List<GitItemStatus> GetAllChangedFiles()
        {
            var status = RunCmd(Settings.GitCommand, GitCommandHelpers.GetAllChangedFilesCmd(true, true));

            return GitCommandHelpers.GetAllChangedFilesFromString(status);
        }

        public List<GitItemStatus> GetTrackedChangedFiles()
        {
            var status = RunCmd(Settings.GitCommand, GitCommandHelpers.GetAllChangedFilesCmd(true, false));

            return GitCommandHelpers.GetAllChangedFilesFromString(status);
        }

        public List<GitItemStatus> GetDeletedFiles()
        {
            var status = RunCmd(Settings.GitCommand, "ls-files -z --deleted --exclude-standard");

            var statusStrings = status.Split(new char[] { '\0', '\n' }, StringSplitOptions.RemoveEmptyEntries);

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

        public bool FileIsStaged(string filename)
        {
            var status = RunCmd(Settings.GitCommand, "diff -z --cached --numstat -- \"" + filename + "\"");
            return !string.IsNullOrEmpty(status);
        }

        public List<GitItemStatus> GetStagedFiles()
        {
            string status = RunCmd(Settings.GitCommand, "diff -M -C -z --cached --name-status");

            if (true && status.Length < 50 && status.Contains("fatal: No HEAD commit to compare"))
            {
                //This command is a little more expensive because it will return both staged and unstaged files
                string command = GitCommandHelpers.GetAllChangedFilesCmd(true, false);
                status = RunCmd(Settings.GitCommand, command);
                List<GitItemStatus> stagedFiles = GitCommandHelpers.GetAllChangedFilesFromString(status, false);
                return stagedFiles.Where(f => f.IsStaged).ToList<GitItemStatus>();
            }

            return GitCommandHelpers.GetAllChangedFilesFromString(status, true);
        }

        public List<GitItemStatus> GitStatus()
        {
            return GitStatus(UntrackedFilesMode.Default, 0);
        }

        public List<GitItemStatus> GitStatus(UntrackedFilesMode untrackedFilesMode, IgnoreSubmodulesMode ignoreSubmodulesMode)
        {
            if (!GitCommandHelpers.VersionInUse.SupportGitStatusPorcelain)
                throw new Exception("The version of git you are using is not supported for this action. Please upgrade to git 1.7.3 or newer.");

            string command = GitCommandHelpers.GetAllChangedFilesCmd(true, untrackedFilesMode, ignoreSubmodulesMode);
            string status = RunCmd(Settings.GitCommand, command);
            return GitCommandHelpers.GetAllChangedFilesFromString(status);
        }

        public string GetCurrentChanges(string fileName, string oldFileName, bool staged, string extraDiffArguments)
        {
            fileName = string.Concat("\"", FixPath(fileName), "\"");
            if (!string.IsNullOrEmpty(oldFileName))
                oldFileName = string.Concat("\"", FixPath(oldFileName), "\"");

            if (Settings.UsePatienceDiffAlgorithm)
                extraDiffArguments = string.Concat(extraDiffArguments, " --patience");

            var args = string.Concat("diff ", extraDiffArguments, " -- ", fileName);
            if (staged)
                args = string.Concat("diff -M -C --cached", extraDiffArguments, " -- ", fileName, " ", oldFileName);

            return RunCmd(Settings.GitCommand, args);
        }

        public string StageFile(string file)
        {
            return RunCmd(Settings.GitCommand, "update-index --add" + " \"" + FixPath(file) + "\"");
        }

        public string StageFileToRemove(string file)
        {
            return RunCmd(Settings.GitCommand, "update-index --remove" + " \"" + FixPath(file) + "\"");
        }


        public string UnstageFile(string file)
        {
            return RunCmd(Settings.GitCommand, "rm" + " --cached \"" + FixPath(file) + "\"");
        }

        public string UnstageFileToRemove(string file)
        {
            return RunCmd(Settings.GitCommand, "reset HEAD -- \"" + FixPath(file) + "\"");
        }


        public static bool IsBareRepository(string repositoryPath)
        {
            return !Directory.Exists(GetGitDirectory(repositoryPath));
        }


        /// <summary>
        /// Dirty but fast. This sometimes fails.
        /// </summary>
        public static string GetSelectedBranchFast(string repositoryPath)
        {
            if (string.IsNullOrEmpty(repositoryPath))
                return string.Empty;

            string head;
            string headFileName = Path.Combine(WorkingDirGitDir(repositoryPath), "HEAD");
            if (File.Exists(headFileName))
            {
                head = File.ReadAllText(headFileName);
                if (!head.Contains("ref:"))
                    return "(no branch)";
            }
            else
            {
                return string.Empty;
            }
            
            if (!string.IsNullOrEmpty(head))
            {
                return head.Replace("ref:", "").Replace("refs/heads/", string.Empty).Trim();
            }

            return string.Empty;
        }

        public string GetSelectedBranch(string repositoryPath)
        {
            string head = GetSelectedBranchFast(repositoryPath);

            if (string.IsNullOrEmpty(head))
            {
                int exitcode;
                head = RunCmd(Settings.GitCommand, "symbolic-ref HEAD", out exitcode);
                if (exitcode == 1)
                    return "(no branch)";
            }

            return head;
        }

        public string GetSelectedBranch()
        {
            return GetSelectedBranch(_workingdir);
        }

        public string GetRemoteBranch(string branch)
        {
            string remote = GetSetting(string.Format("branch.{0}.remote", branch));
            string merge = GetSetting(string.Format("branch.{0}.merge", branch));
            if (String.IsNullOrEmpty(remote) || String.IsNullOrEmpty(merge))
                return "";
            return remote + "/" + (merge.StartsWith("refs/heads/") ? merge.Substring(11) : merge);
        }

        public List<GitHead> GetRemoteHeads(string remote, bool tags, bool branches)
        {
            remote = FixPath(remote);

            var tree = GetTreeFromRemoteHeands(remote, tags, branches);
            return GetHeads(tree);
        }

        private string GetTreeFromRemoteHeands(string remote, bool tags, bool branches)
        {
            if (tags && branches)
                return RunCmd(Settings.GitCommand, "ls-remote --heads --tags \"" + remote + "\"");
            if (tags)
                return RunCmd(Settings.GitCommand, "ls-remote --tags \"" + remote + "\"");
            if (branches)
                return RunCmd(Settings.GitCommand, "ls-remote --heads \"" + remote + "\"");
            return "";
        }

        public List<GitHead> GetHeads()
        {
            return GetHeads(true);
        }

        public List<GitHead> GetHeads(bool tags)
        {
            return GetHeads(tags, true);
        }

        public List<GitHead> GetHeads(bool tags, bool branches)
        {
            var tree = GetTree(tags, branches);
            return GetHeads(tree);
        }

        private string GetTree(bool tags, bool branches)
        {
            if (tags && branches)
                return RunCmd(Settings.GitCommand, "show-ref --dereference");

            if (tags)
                return RunCmd(Settings.GitCommand, "show-ref --tags");

            if (branches)
                return RunCmd(Settings.GitCommand, "show-ref --dereference --heads");
            return "";
        }

        private List<GitHead> GetHeads(string tree)
        {
            var itemsStrings = tree.Split('\n');

            var heads = new List<GitHead>();

            var remotes = GetRemotes(false);

            foreach (var itemsString in itemsStrings)
            {
                if (itemsString == null || itemsString.Length <= 42) continue;

                var guid = itemsString.Substring(0, 40);
                var completeName = itemsString.Substring(41).Trim();
                heads.Add(new GitHead(guid, completeName, GetRemoteName(completeName, remotes)));
            }

            return heads;
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

        public IList<string> GetFiles(string filePattern)
        {
            // filter duplicates out of the result because options -c and -m may return 
            // same files at times
            return RunCmd(Settings.GitCommand, "ls-files -z -o -m -c \"" + filePattern + "\"")
                .Split(new[] { '\0', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToList();
        }

        public List<GitItem> GetFileChanges(string file)
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
                    item = new GitItem { CommitGuid = itemsString.Substring(7).Trim() };

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

        public string[] GetFullTree(string id)
        {
            string tree = this.RunCachableCmd(Settings.GitCommand, string.Format("ls-tree -z -r --name-only {0}", id));
            return tree.Split(new char[] { '\0', '\n' });
        }

        public List<IGitItem> GetTree(string id)
        {
            var tree = this.RunCachableCmd(Settings.GitCommand, "ls-tree -z \"" + id + "\"");

            var itemsStrings = tree.Split(new char[] { '\0', '\n' });

            var items = new List<IGitItem>();

            foreach (var itemsString in itemsStrings)
            {
                if (itemsString.Length <= 53)
                    continue;

                var item = new GitItem { Mode = itemsString.Substring(0, 6) };
                var guidStart = itemsString.IndexOf(' ', 7);
                item.ItemType = itemsString.Substring(7, guidStart - 7);
                item.Guid = itemsString.Substring(guidStart + 1, 40);
                item.Name = itemsString.Substring(guidStart + 42).Trim();
                item.FileName = item.Name;

                items.Add(item);
            }

            return items;
        }

        public GitBlame Blame(string filename, string from)
        {
            from = FixPath(from);
            filename = FixPath(filename);
            string blameCommand = string.Format("blame --porcelain -M -w -l \"{0}\" -- \"{1}\"", from, filename);
            var itemsStrings =
                RunCmd(
                    Settings.GitCommand,
                    blameCommand
                    )
                    .Split('\n');

            GitBlame blame = new GitBlame();

            GitBlameHeader blameHeader = null;
            GitBlameLine blameLine = null;

            for (int i = 0; i < itemsStrings.GetLength(0); i++)
            {
                try
                {
                    string line = itemsStrings[i];

                    //The contents of the actual line is output after the above header, prefixed by a TAB. This is to allow adding more header elements later.
                    if (line.StartsWith("\t"))
                        blameLine.LineText = line.Substring(1) //trim ONLY first tab
                                                 .Trim(new char[] { '\r' }); //trim \r, this is a workaround for a \r\n bug
                    else if (line.StartsWith("author-mail"))
                        blameHeader.AuthorMail = line.Substring("author-mail".Length).Trim();
                    else if (line.StartsWith("author-time"))
                        blameHeader.AuthorTime = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddSeconds(int.Parse(line.Substring("author-time".Length).Trim()));
                    else if (line.StartsWith("author-tz"))
                        blameHeader.AuthorTimeZone = line.Substring("author-tz".Length).Trim();
                    else if (line.StartsWith("author"))
                    {
                        blameHeader = new GitBlameHeader();
                        blameHeader.CommitGuid = blameLine.CommitGuid;
                        blameHeader.Author = line.Substring("author".Length).Trim();
                        blame.Headers.Add(blameHeader);
                    }
                    else if (line.StartsWith("committer-mail"))
                        blameHeader.CommitterMail = line.Substring("committer-mail".Length).Trim();
                    else if (line.StartsWith("committer-time"))
                        blameHeader.CommitterTime = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddSeconds(int.Parse(line.Substring("committer-time".Length).Trim()));
                    else if (line.StartsWith("committer-tz"))
                        blameHeader.CommitterTimeZone = line.Substring("committer-tz".Length).Trim();
                    else if (line.StartsWith("committer"))
                        blameHeader.Committer = line.Substring("committer".Length).Trim();
                    else if (line.StartsWith("summary"))
                        blameHeader.Summary = line.Substring("summary".Length).Trim();
                    else if (line.StartsWith("filename"))
                        blameHeader.FileName = line.Substring("filename".Length).Trim();
                    else if (line.IndexOf(' ') == 40) //SHA1, create new line!
                    {
                        blameLine = new GitBlameLine();
                        blameLine.CommitGuid = line.Substring(0, 40);
                        blame.Lines.Add(blameLine);
                    }
                }
                catch
                {
                    //Catch all parser errors, and ignore them all!
                    //We should never get here...
                    Settings.GitLog.Log("Error parsing output from command: " + blameCommand + "\n\nPlease report a bug!");
                }
            }


            return blame;
        }

        public string GetFileRevisionText(string file, string revision)
        {
            return
                this.RunCachableCmd(
                    Settings.GitCommand,
                    string.Format("show --encoding=" + Settings.Encoding.HeaderName + " {0}:\"{1}\"", revision, file.Replace('\\', '/')));
        }

        public string GetFileText(string id)
        {
            return this.RunCachableCmd(Settings.GitCommand, "cat-file blob \"" + id + "\"");
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

        public Stream GetFileStream(string blob)
        {
            try
            {
                var newStream = new MemoryStream();

                GitCommandHelpers.SetEnvironmentVariable();

                Settings.GitLog.Log(Settings.GitCommand + " " + "cat-file blob " + blob);
                //process used to execute external commands

                var info = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    ErrorDialog = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = true,
                    FileName = "\"" + Settings.GitCommand + "\"",
                    Arguments = "cat-file blob " + blob,
                    WorkingDirectory = _workingdir,
                    WindowStyle = ProcessWindowStyle.Normal,
                    LoadUserProfile = true
                };

                using (var process = Process.Start(info))
                {
                    StreamCopy(process.StandardOutput.BaseStream, newStream);
                    newStream.Position = 0;

                    process.WaitForExit();
                    return newStream;
                }
            }
            catch (Win32Exception ex)
            {
                Trace.WriteLine(ex);
            }

            return null;
        }

        public string RecodeString(string s) {

            Encoding logoutputEncoding = GetLogoutputEncoding();
            if (logoutputEncoding != Settings.Encoding)
                s = logoutputEncoding.GetString(Settings.Encoding.GetBytes(s));

            return s;
        }

        public string GetPreviousCommitMessage(int numberBack)
        {
            //+"--encoding=" + Settings.Encoding.HeaderName doesn't work
            return RecodeString(RunCmd(Settings.GitCommand, "log -n 1 HEAD~" + numberBack + " --pretty=format:%s%n%n%b "));
        }

        public string MergeBranch(string branch)
        {
            return RunCmd(Settings.GitCommand, GitCommandHelpers.MergeBranchCmd(branch, true, false, false, null));
        }

        public string OpenWithDifftool(string filename)
        {
            return OpenWithDifftool(filename, null, null);
        }

        public string OpenWithDifftool(string filename, string revision1)
        {
            return OpenWithDifftool(filename, revision1, null);
        }

        public string OpenWithDifftool(string filename, string revision1, string revision2)
        {
            var output = "";
            string args = revision2.Join(" ", revision1).Join(" ", "-- \"" + filename + "\"");
            if (GitCommandHelpers.VersionInUse.GuiDiffToolExist)
                RunCmdAsync(Settings.GitCommand,
                            "difftool --gui --no-prompt " + args);
            else
                output = RunCmd(Settings.GitCommand,
                                "difftool --no-prompt " + args);
            return output;
        }

        public string RevParse(string revisionExpression)
        {
            string revparseCommand = string.Format("rev-parse \"{0}\"", revisionExpression);
            int exitCode = 0;
            string[] resultStrings =
                RunCmd(
                    Settings.GitCommand,
                    revparseCommand,
                    out exitCode, ""
                    )
                    .Split('\n');
            if (exitCode == 0)
            {
                return resultStrings[0];
            }
            else
            {
                return "";
            }
        }

        public static string WorkingDirGitDir(string repositoryPath)
        {
            if (string.IsNullOrEmpty(repositoryPath))
                return repositoryPath;
            var candidatePath = GetGitDirectory(repositoryPath);
            return Directory.Exists(candidatePath) ? candidatePath : repositoryPath;
        }
    }
}