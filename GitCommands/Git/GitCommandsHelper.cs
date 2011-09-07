﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using GitCommands.Config;
using PatchApply;

namespace GitCommands
{
    public static class GitCommandHelpers
    {
        private static GitVersion _versionInUse;

        /// <summary>
        /// This is a faster function to get the names of all submodules then the 
        /// GetSubmodules() function. The command @git submodule is very slow.
        /// </summary>
        public static IList<string> GetSubmodulesNames()
        {
            IList<string> submodulesNames = new List<string>();
            var configFile = new ConfigFile(Settings.WorkingDir + ".gitmodules");
            foreach (ConfigSection configSection in configFile.GetConfigSections())
            {
                submodulesNames.Add(configSection.SubSection);
            }

            return submodulesNames;
        }

        public static string GetGlobalSetting(string setting)
        {
            var configFile = GetGlobalConfig();
            return configFile.GetValue(setting);
        }

        public static void SetGlobalSetting(string setting, string value)
        {
            var configFile = GetGlobalConfig();
            configFile.SetValue(setting, value);
            configFile.Save();
        }

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

                if (Settings.ValidWorkingDir(dir))
                    return dir + Settings.PathSeparator;
            }
            return startDir;
        }

        public static Encoding GetLogoutputEncoding()
        {
            string encodingString = GetLocalConfig().GetValue("i18n.logoutputencoding");
            if (string.IsNullOrEmpty(encodingString))
                encodingString = GetGlobalConfig().GetValue("i18n.logoutputencoding");
            if (string.IsNullOrEmpty(encodingString))
                encodingString = GetLocalConfig().GetValue("i18n.commitEncoding");
            if (string.IsNullOrEmpty(encodingString))
                encodingString = GetGlobalConfig().GetValue("i18n.commitEncoding");
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
            return path.Replace('\\', '/');
        }

        public static void RunRealCmdDetached(string cmd, string arguments)
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

        private static void CreateAndStartCommand(string cmd, string arguments, bool waitForExit)
        {
            SetEnvironmentVariable();

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
                               WorkingDirectory = Settings.WorkingDir,
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

        public static void StartExternalCommand(string cmd, string arguments)
        {
            try
            {
                SetEnvironmentVariable();

                var processInfo = new ProcessStartInfo
                                      {
                                          UseShellExecute = false,
                                          RedirectStandardOutput = false,
                                          FileName = cmd,
                                          WorkingDirectory = Settings.WorkingDir,
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
            return RunCmd(cmd, arguments, null);
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public static string RunCmd(string cmd, string arguments, string stdInput)
        {
            int exitCode;
            return RunCmd(cmd, arguments, out exitCode, stdInput);
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public static string RunCmd(string cmd, string arguments, out int exitCode)
        {
            return RunCmd(cmd, arguments, out exitCode, null);
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public static string RunCmd(string cmd, string arguments, out int exitCode, string stdInput)
        {
            try
            {
                SetEnvironmentVariable();

                arguments = arguments.Replace("$QUOTE$", "\\\"");

                string output, error;
                exitCode = CreateAndStartProcess(arguments, cmd, out output, out error, stdInput);

                if (!string.IsNullOrEmpty(error))
                {
                    output += Environment.NewLine + error;
                }
                return output;
            }
            catch (Win32Exception)
            {
                exitCode = 1;
                return string.Empty;
            }
        }

        private static int CreateAndStartProcess(string arguments, string cmd, out string stdOutput, out string stdError, string stdInput)
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
            startInfo.WorkingDirectory = Settings.WorkingDir;
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
        public static void RunCmdAsync(string cmd, string arguments)
        {
            SetEnvironmentVariable();

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
                               WorkingDirectory = Settings.WorkingDir,
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

        public static void EditNotes(string revision)
        {
            if (GitCommandHelpers.GetGlobalConfig().GetValue("core.editor").ToLower().Contains("gitextensions") ||
                GitCommandHelpers.GetLocalConfig().GetValue("core.editor").ToLower().Contains("gitextensions") ||
                GitCommandHelpers.GetGlobalConfig().GetValue("core.editor").ToLower().Contains("notepad") ||
                GitCommandHelpers.GetLocalConfig().GetValue("core.editor").ToLower().Contains("notepad"))
            {
                RunCmd(Settings.GitCommand, "notes edit " + revision);
            }
            else
            {
                RunRealCmd(Settings.GitCommand, "notes edit " + revision);
            }
        }

        public static bool InTheMiddleOfConflictedMerge()
        {
            return !string.IsNullOrEmpty(RunCmd(Settings.GitCommand, "ls-files -z --unmerged"));
        }

        public static List<GitItem> GetConflictedFiles()
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

        private static IEnumerable<string> GetUnmergedFileListing()
        {
            return RunCmd(Settings.GitCommand, "ls-files -z --unmerged").Split(new[] { '\0', '\n' }, StringSplitOptions.RemoveEmptyEntries);
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
            Directory.SetCurrentDirectory(Settings.WorkingDir);

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
                Directory.SetCurrentDirectory(Settings.WorkingDir);
                SaveBlobAs(saveAs, fileline[1]);
                return true;
            }
            return false;
        }

        public static void SaveBlobAs(string saveAs, string blob)
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
                    ConfigFile globalConfig = GetGlobalConfig();
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

        public static string[] GetConflictedFiles(string filename)
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

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

            if (!File.Exists(fileNames[0])) fileNames[0] = null;
            if (!File.Exists(fileNames[1])) fileNames[1] = null;
            if (!File.Exists(fileNames[2])) fileNames[2] = null;

            return fileNames;
        }

        public static string[] GetConflictedFileNames(string filename)
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

        public static string GetMergeMessage()
        {
            var file = Settings.WorkingDir + ".git" + Settings.PathSeparator + "MERGE_MSG";

            return
                File.Exists(file)
                    ? File.ReadAllText(file)
                    : "";
        }

        public static void RunGitK()
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

        public static void RunGui()
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

        public static void RunBash()
        {
            if (Settings.RunningOnUnix())
            {
                RunRealCmdDetached("bash", "--login -i");
            }
            else
            {
                if (File.Exists(Settings.GitBinDir + "bash.exe"))
                    RunRealCmdDetached("cmd.exe", "/c \"\"" + Settings.GitBinDir + "bash\" --login -i\"");
                else
                    RunRealCmdDetached("cmd.exe", "/c \"\"" + Settings.GitBinDir + "sh\" --login -i\"");
            }
        }

        public static string Init(bool bare, bool shared)
        {
            if (bare && shared)
                return RunCmd(Settings.GitCommand, "init --bare --shared=all");
            if (bare)
                return RunCmd(Settings.GitCommand, "init --bare");
            return RunCmd(Settings.GitCommand, "init");
        }

        public static bool IsMerge(string commit)
        {
            string output = RunCmd(Settings.GitCommand, "log -n 1 --format=format:%P \"" + commit + "\"");
            string[] parents = output.Split(' ');
            if (parents.Length > 1) return true;
            return false;
        }

        public static GitRevision[] GetParents(string commit)
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

        public static string CherryPickCmd(string cherry, bool commit, string arguments)
        {
            string CherryPickCmd = commit ? "cherry-pick" : "cherry-pick --no-commit";
            return CherryPickCmd + " " + arguments + " \"" + cherry + "\"";
        }


        public static string CherryPick(string cherry, bool commit, string arguments)
        {
            return RunCmd(Settings.GitCommand, CherryPickCmd(cherry, commit, arguments));
        }

        public static string ShowSha1(string sha1)
        {
            return RunCachableCmd(Settings.GitCommand, "show --encoding=" + Settings.Encoding.HeaderName + " " + sha1);
        }

        public static string UserCommitCount()
        {
            return RunCmd(Settings.GitCommand, "shortlog -s -n");
        }

        public static string DeleteBranch(string branchName, bool force, bool remoteBranch)
        {
            return RunCmd(Settings.GitCommand, DeleteBranchCmd(branchName, force, remoteBranch));
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
            var configFile = new ConfigFile(Settings.WorkingDir + Settings.PathSeparator + ".gitmodules");
            return configFile.GetValue("submodule." + name.Trim() + ".url").Trim();
        }

        public static string GetSubmoduleLocalPath(string name)
        {
            var configFile = new ConfigFile(Settings.WorkingDir + Settings.PathSeparator + ".gitmodules");
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
                branch = " -b \"" + branch.Trim() + "\"";

            return "submodule add" + branch + " \"" + remotePath.Trim() + "\" \"" + localPath.Trim() + "\"";
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
                gitSubmodule.Name = name.Substring(0, name.IndexOf("("));
                gitSubmodule.Branch = name.Substring(name.IndexOf("(")).Trim(new[] { '(', ')', ' ' });
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

        public static string ResetFile(string file)
        {
            file = FixPath(file);
            return RunCmd(Settings.GitCommand, "checkout-index --index --force -- \"" + file + "\"");
        }


        public static string FormatPatch(string from, string to, string output, int start)
        {
            output = FixPath(output);

            var result = RunCmd(Settings.GitCommand,
                                "format-patch -M -C -B --start-number " + start + " \"" + from + "\"..\"" + to +
                                "\" -o \"" + output + "\"");

            return result;
        }

        public static string FormatPatch(string from, string to, string output)
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

        public static string Push(string path)
        {
            return RunCmd(Settings.GitCommand, "push \"" + FixPath(path).Trim() + "\"");
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
            StartExternalCommand(Settings.Pageant, "\"" + sshKeyFile + "\"");
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

        public static string Fetch(string remote, string branch)
        {
            remote = FixPath(remote);

            Directory.SetCurrentDirectory(Settings.WorkingDir);

            RunRealCmd("cmd.exe", " /k \"\"" + Settings.GitCommand + "\" " + FetchCmd(remote, null, branch) + "\"");

            return "Done";
        }

        public static bool PathIsUrl(string path)
        {
            return path.Contains(Settings.PathSeparator.ToString()) || path.Contains(Settings.PathSeparatorWrong.ToString());
        }

        public static string FetchCmd(string remote, string remoteBranch, string localBranch)
        {
            if (string.IsNullOrEmpty(remote) && string.IsNullOrEmpty(remoteBranch) && string.IsNullOrEmpty(localBranch))
                return "fetch";

            return "fetch " + GetFetchArgs(remote, remoteBranch, localBranch);
        }

        public static string Pull(string remote, string remoteBranch, string localBranch, bool rebase)
        {
            remote = FixPath(remote);

            Directory.SetCurrentDirectory(Settings.WorkingDir);

            RunRealCmd("cmd.exe", " /k \"\"" + Settings.GitCommand + "\" " + PullCmd(remote, localBranch, remoteBranch, rebase) + "\"");

            return "Done";
        }

        public static string PullCmd(string remote, string remoteBranch, string localBranch, bool rebase)
        {
            if (rebase && !string.IsNullOrEmpty(remoteBranch))
                return "pull --rebase " + remote + " refs/heads/" + remoteBranch;

            if (rebase)
                return "pull --rebase " + remote;

            return "pull " + GetFetchArgs(remote, remoteBranch, localBranch);
        }

        private static string GetFetchArgs(string remote, string remoteBranch, string localBranch)
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
            if (VersionInUse.FetchCanAskForProgress)
                progressOption = "--progress ";

            return progressOption + "\"" + remote.Trim() + "\" " + remoteBranchArguments + localBranchArguments;
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
            if (Directory.Exists(Settings.WorkingDir + ".git" + Settings.PathSeparator + "rebase-merge" + Settings.PathSeparator))
                return Settings.WorkingDir + ".git" + Settings.PathSeparator + "rebase-merge" + Settings.PathSeparator;
            if (Directory.Exists(Settings.WorkingDir + ".git" + Settings.PathSeparator + "rebase-apply" + Settings.PathSeparator))
                return Settings.WorkingDir + ".git" + Settings.PathSeparator + "rebase-apply" + Settings.PathSeparator;
            if (Directory.Exists(Settings.WorkingDir + ".git" + Settings.PathSeparator + "rebase" + Settings.PathSeparator))
                return Settings.WorkingDir + ".git" + Settings.PathSeparator + "rebase" + Settings.PathSeparator;

            return "";
        }

        public static bool InTheMiddleOfBisect()
        {
            return File.Exists(Settings.WorkingDirGitDir() + Settings.PathSeparator + "BISECT_START");
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


        public static string Rebase(string branch)
        {
            Directory.SetCurrentDirectory(Settings.WorkingDir);

            return RunCmd(Settings.GitCommand, RebaseCmd(branch, false, false));
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
            return Commit(amend, "");
        }

        public static string Commit(bool amend, string author)
        {
            return RunCmd(Settings.GitCommand, CommitCmd(amend, author));
        }

        public static string CommitCmd(bool amend)
        {
            return CommitCmd(amend, "");
        }

        public static string CommitCmd(bool amend, string author)
        {
            string command = "commit";
            if (amend)
                command += " --amend";

            if (!string.IsNullOrEmpty(author))
                command += " --author=\"" + author + "\"";

            var path = Settings.WorkingDirGitDir() + Settings.PathSeparator + "COMMITMESSAGE\"";
            command += " -F \"" + path;

            return command;
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

        public static string UpdateRemotes()
        {
            return RunCmd(Settings.GitCommand, "remote update");
        }

        public static string RemoveRemote(string name)
        {
            return RunCmd(Settings.GitCommand, "remote rm \"" + name + "\"");
        }

        public static string RenameRemote(string name, string newName)
        {
            return RunCmd(Settings.GitCommand, "remote rename \"" + name + "\" \"" + newName + "\"");
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
            return new ConfigFile(GitCommandHelpers.GetDefaultHomeDir() + Settings.PathSeparator + ".gitconfig");
        }

        public static ConfigFile GetLocalConfig()
        {
            return new ConfigFile(Settings.WorkingDirGitDir() + Settings.PathSeparator + "config");
        }

        public static string GetSetting(string setting)
        {
            var configFile = GetLocalConfig();
            return configFile.GetValue(setting);
        }

        public static string GetEffectiveSetting(string setting)
        {
            var localConfig = GetLocalConfig();
            if (localConfig.HasValue(setting))
                return localConfig.GetValue(setting);

            return GetGlobalConfig().GetValue(setting);
        }

        public static void UnsetSetting(string setting)
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

            return patchManager.Patches;
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

        public static Patch GetSingleDiff(string from, string to, string fileName, string oldFileName, string extraDiffArguments)
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
            patchManager.LoadPatch(RunCachableCmd(Settings.GitCommand, arguments), false);

            return patchManager.Patches.Count > 0 ? patchManager.Patches[0] : null;
        }


        public static Patch GetSingleDiff(string from, string to, string fileName, string extraDiffArguments)
        {
            return GetSingleDiff(from, to, fileName, null, extraDiffArguments);
        }

        public static List<Patch> GetDiff(string from, string to, string extraDiffArguments)
        {
            if (Settings.UsePatienceDiffAlgorithm)
                extraDiffArguments = string.Concat(extraDiffArguments, " --patience");

            var patchManager = new PatchManager();
            var arguments = string.Format("diff{0} \"{1}\" \"{2}\"", extraDiffArguments, from, to);
            patchManager.LoadPatch(RunCachableCmd(Settings.GitCommand, arguments), false);

            return patchManager.Patches;
        }

        public static List<GitItemStatus> GetDiffFiles(string from, string to)
        {
            return GetDiffFiles(from, to, false);
        }

        public static List<GitItemStatus> GetDiffFiles(string from, string to, bool noCache)
        {
            string result;
            string cmd = "diff -M -C -z --name-status \"" + to + "\" \"" + from + "\"";
            if (noCache)
            {
                result = RunCmd(Settings.GitCommand, cmd);
            }
            else
            {
                result = RunCachableCmd(Settings.GitCommand, cmd);

            }
            return GetAllChangedFilesFromString(result, true);
        }

        public static List<GitItemStatus> GetUntrackedFiles()
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

        public static List<GitItemStatus> GetAllChangedFiles()
        {
            var status = RunCmd(Settings.GitCommand, GetAllChangedFilesCmd(true, true));

            return GetAllChangedFilesFromString(status);
        }

        public static List<GitItemStatus> GetTrackedChangedFiles()
        {
            var status = RunCmd(Settings.GitCommand, GetAllChangedFilesCmd(true, false));

            return GetAllChangedFilesFromString(status);
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

        public static List<GitItemStatus> GetDeletedFiles()
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

        public static bool FileIsStaged(string filename)
        {
            var status = RunCmd(Settings.GitCommand, "diff -z --cached --numstat -- \"" + filename + "\"");
            return !string.IsNullOrEmpty(status);
        }

        public static List<GitItemStatus> GetStagedFiles()
        {
            string status = RunCmd(Settings.GitCommand, "diff -M -C -z --cached --name-status");

            if (true && status.Length < 50 && status.Contains("fatal: No HEAD commit to compare"))
            {
                //This command is a little more expensive because it will return both staged and unstaged files
                status = RunCmd(Settings.GitCommand, "status --porcelain --untracked-files=no -z");
                List<GitItemStatus> stagedFiles = GetAllChangedFilesFromString(status, false);
                return stagedFiles.Where(f => f.IsStaged).ToList<GitItemStatus>();
            }

            return GetAllChangedFilesFromString(status, true);
        }

        public static List<GitItemStatus> GitStatus()
        {
            return GitStatus(true);
        }

        public static List<GitItemStatus> GitStatus(bool untracked)
        {
            if (!VersionInUse.SupportGitStatusPorcelain)
                throw new Exception("The version of git you are using is not supported for this action. Please upgrade to git 1.7.3 or newer.");

            string status = RunCmd(Settings.GitCommand, "status --porcelain --untracked-files -z");
            return GetAllChangedFilesFromString(status);
        }

        public static string GetCurrentChanges(string fileName, string oldFileName, bool staged, string extraDiffArguments)
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
            return RunCmd(Settings.GitCommand, "reset HEAD -- \"" + FixPath(file) + "\"");
        }

        public static string GetSelectedBranch(string repositoryPath)
        {
            string head;
            string headFileName = Path.Combine(GetGitDirectory(repositoryPath), "HEAD");
            if (File.Exists(headFileName))
            {
                head = File.ReadAllText(headFileName);
                if (!head.Contains("ref:"))
                    head = "(no branch)";
            }
            else
            {
                int exitcode;
                head = RunCmd(Settings.GitCommand, "symbolic-ref HEAD", out exitcode);
                if (exitcode == 1)
                    return "(no branch)";
            }

            if (!string.IsNullOrEmpty(head))
            {
                return head.Replace("ref:", "").Replace("refs/heads/", string.Empty).Trim();
            }

            return string.Empty;
        }

        public static string GetSelectedBranch()
        {
            return GetSelectedBranch(Settings.WorkingDir);
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

            var remotes = GetRemotes();

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

        public static IList<string> GetFiles(string filePattern)
        {
            // filter duplicates out of the result because options -c and -m may return 
            // same files at times
            return RunCmd(Settings.GitCommand, "ls-files -z -o -m -c \"" + filePattern + "\"")
                .Split(new[] {'\0', '\n'}, StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToList();
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

        static public string[] GetFullTree(string id)
        {
            string tree = RunCachableCmd(Settings.GitCommand, string.Format("ls-tree -z -r --name-only {0}", id));
            return tree.Split(new char[] { '\0', '\n' });
        }
        public static List<IGitItem> GetTree(string id)
        {
            var tree = RunCachableCmd(Settings.GitCommand, "ls-tree -z \"" + id + "\"");

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

        public static GitBlame Blame(string filename, string from)
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
                        blameLine.LineText = line.Substring(1).Trim(new char[] { '\r' });//trim first tab
                    else
                        if (line.StartsWith("author-mail"))
                            blameHeader.AuthorMail = line.Substring("author-mail".Length).Trim();
                        else
                            if (line.StartsWith("author-time"))
                                blameHeader.AuthorTime = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddSeconds(int.Parse(line.Substring("author-time".Length).Trim()));
                            else
                                if (line.StartsWith("author-tz"))
                                    blameHeader.AuthorTimeZone = line.Substring("author-tz".Length).Trim();
                                else
                                    if (line.StartsWith("author"))
                                    {
                                        blameHeader = new GitBlameHeader();
                                        blameHeader.CommitGuid = blameLine.CommitGuid;
                                        blameHeader.Author = line.Substring("author".Length).Trim();
                                        blame.Headers.Add(blameHeader);
                                    }
                                    else
                                        if (line.StartsWith("committer-mail"))
                                            blameHeader.CommitterMail = line.Substring("committer-mail".Length).Trim();
                                        else
                                            if (line.StartsWith("committer-time"))
                                                blameHeader.CommitterTime = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddSeconds(int.Parse(line.Substring("committer-time".Length).Trim()));
                                            else
                                                if (line.StartsWith("committer-tz"))
                                                    blameHeader.CommitterTimeZone = line.Substring("committer-tz".Length).Trim();
                                                else
                                                    if (line.StartsWith("committer"))
                                                        blameHeader.Committer = line.Substring("committer".Length).Trim();
                                                    else
                                                        if (line.StartsWith("summary"))
                                                            blameHeader.Summary = line.Substring("summary".Length).Trim();
                                                        else
                                                            if (line.StartsWith("filename"))
                                                                blameHeader.FileName = line.Substring("filename".Length).Trim();
                                                            else
                                                                if (line.IndexOf(' ') == 40) //SHA1, create new line!
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

        public static string GetFileRevisionText(string file, string revision)
        {
            return
                RunCachableCmd(
                    Settings.GitCommand,
                    string.Format("show --encoding=" + Settings.Encoding.HeaderName + " {0}:\"{1}\"", revision, file.Replace('\\', '/')));
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

        public static Stream GetFileStream(string blob)
        {
            try
            {
                var newStream = new MemoryStream();

                SetEnvironmentVariable();

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
                    WorkingDirectory = Settings.WorkingDir,
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

        public static string RecodeString(string s) {

            Encoding logoutputEncoding = GitCommandHelpers.GetLogoutputEncoding();
            if (logoutputEncoding != Settings.Encoding)
                s = logoutputEncoding.GetString(Settings.Encoding.GetBytes(s));

            return s;
        }

        public static string GetPreviousCommitMessage(int numberBack)
        {
            //+"--encoding=" + Settings.Encoding.HeaderName doesn't work
            return RecodeString(RunCmd(Settings.GitCommand, "log -n 1 HEAD~" + numberBack + " --pretty=format:%s%n%n%b "));
        }

        public static string MergeBranch(string branch)
        {
            return RunCmd(Settings.GitCommand, MergeBranchCmd(branch, true, false, false, null));
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

        public static string RevParse(string revisionExpression)
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

        public static string GetGitDirectory(string repositoryPath)
        {
            var candidatePath = Path.Combine(repositoryPath, ".git");
            return Directory.Exists(candidatePath) ? candidatePath : repositoryPath;
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