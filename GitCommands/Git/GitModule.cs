using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitCommands.Config;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using PatchApply;

namespace GitCommands
{
    public delegate void GitModuleChangedEventHandler(GitModule module);

    /// <summary>
    /// Class provide non-static methods for manipulation with git module.
    /// You can create several instances for submodules.
    /// </summary>
    [DebuggerDisplay("GitModule ( {_workingdir} )")]
    public sealed class GitModule : IGitModule
    {
        private static readonly Regex DefaultHeadPattern = new Regex("refs/remotes/[^/]+/HEAD", RegexOptions.Compiled);

        public GitModule(string workingdir)
        {
            WorkingDir = workingdir;
        }

        private string _workingdir;

        public string WorkingDir
        {
            get
            {
                return _workingdir;
            }
            private set
            {
                _superprojectInit = false;
                _workingdir = FindGitWorkingDir(value);
            }
        }

        private bool _superprojectInit;
        private GitModule _superprojectModule;
        private string _submoduleName;

        public string SubmoduleName
        {
            get
            {
                if (!_superprojectInit)
                {
                    string superprojectDir = FindGitSuperprojectPath(out _submoduleName);
                    _superprojectModule = superprojectDir == null ? null : new GitModule(superprojectDir);
                    _superprojectInit = true;
                }
                return _submoduleName;
            }
        }

        public GitModule SuperprojectModule
        {
            get
            {
                if (!_superprojectInit)
                {
                    string superprojectDir = FindGitSuperprojectPath(out _submoduleName);
                    _superprojectModule = superprojectDir == null ? null : new GitModule(superprojectDir);
                    _superprojectInit = true;
                }
                return _superprojectModule;
            }
        }

        public GitModule FindTopProjectModule()
        {
            GitModule module = SuperprojectModule;
            if (module == null)
                return null;
            do
            {
                if (module.SuperprojectModule == null)
                    return module;
                module = module.SuperprojectModule;
            } while (module != null);
            return module;
        }

        //encoding for files paths
        private static Encoding _SystemEncoding = null;
        public static Encoding SystemEncoding
        {
            get
            {
                if (_SystemEncoding == null)
                {
                    //check whether GitExtensions works with standard msysgit or msysgit-unicode

                    // invoke a git command that returns an invalid argument in its response, and
                    // check if a unicode-only character is reported back. If so assume msysgit-unicode

                    // git config --get with a malformed key (no section) returns:
                    // "error: key does not contain a section: <key>"
                    const string controlStr = "ą"; // "a caudata"
                    string arguments = string.Format("config --get {0}", controlStr);

                    int exitCode;

                    String s = new GitModule("").RunGitCmd(arguments, out exitCode, null, Encoding.UTF8);
                    if (s != null && s.IndexOf(controlStr) != -1)
                        _SystemEncoding = new UTF8Encoding(false);
                    else
                        _SystemEncoding = Encoding.Default;

                    Debug.WriteLine("System encoding: " + _SystemEncoding.EncodingName);
                }

                return _SystemEncoding;
            }
        }

        private Encoding GetEncoding(bool local, string settingName)
        {
            string lname = local ? "_local" + '_' + WorkingDir : "_global";
            lname = settingName + lname;
            Encoding result;
            if (Settings.GetEncoding(lname, out result))
                return result;

            string encodingName;
            ConfigFile cfg;
            if (local)
                cfg = GetLocalConfig();
            else
                cfg = GitCommandHelpers.GetGlobalConfig();

            encodingName = cfg.GetValue(settingName);

            if (string.IsNullOrEmpty(encodingName))
                result = null;
            else if (!Settings.availableEncodings.TryGetValue(encodingName, out result))
            {
                try
                {
                    result = Encoding.GetEncoding(encodingName);
                }
                catch (ArgumentException)
                {
                    Debug.WriteLine(string.Format("Unsupported encoding set in git config file: {0}\nPlease check the setting {1} in your {2} config file.", encodingName, settingName, (local ? "local" : "global")));
                    result = null;
                }
            }

            Settings.SetEncoding(lname, result);

            return result;
        }

        private void SetEncoding(bool local, string settingName, Encoding encoding)
        {
            string lname = local ? "_local" + '_' + WorkingDir : "_global";
            lname = settingName + lname;
            Settings.SetEncoding(lname, encoding);
            //storing to config file is handled by FormSettings
        }


        //Encoding that let us read all bytes without replacing any char
        //It is using to read output of commands, which may consist of:
        //1) commit header (message, author, ...) encoded in CommitEncoding, recoded to LogOutputEncoding or not dependent of 
        //   pretty parameter (pretty=raw - recoded, pretty=format - not recoded)
        //2) file content encoded in its original encoding
        //3) file path (file name is encoded in system default encoding),
        //   when core.quotepath is on, every non ASCII character is escaped 
        //   with \ followed by its code as a three digit octal number
        //4) branch, tag name, errors, warnings, hints encoded in system default encoding
        public static readonly Encoding LosslessEncoding = Encoding.GetEncoding("ISO-8859-1");//is any better?

        public Encoding GetFilesEncoding(bool local)
        {
            return GetEncoding(local, "i18n.filesEncoding");
        }
        public void SetFilesEncoding(bool local, Encoding encoding)
        {
            SetEncoding(local, "i18n.filesEncoding", encoding);
        }
        public Encoding FilesEncoding
        {
            get
            {
                Encoding result = GetFilesEncoding(true);
                if (result == null)
                    result = GetFilesEncoding(false);
                if (result == null)
                    result = new UTF8Encoding(false);
                return result;
            }
        }

        public Encoding GetCommitEncoding(bool local)
        {
            return GetEncoding(local, "i18n.commitEncoding");
        }
        public Encoding CommitEncoding
        {
            get
            {
                Encoding result = GetCommitEncoding(true);
                if (result == null)
                    result = GetCommitEncoding(false);
                if (result == null)
                    result = new UTF8Encoding(false);
                return result;
            }
        }


        public Encoding GetLogOutputEncoding(bool local)
        {
            return GetEncoding(local, "i18n.logoutputencoding");
        }
        /// <summary>
        /// Encoding for commit header (message, notes, author, commiter, emails)
        /// </summary>
        public Encoding LogOutputEncoding
        {
            get
            {
                Encoding result = GetLogOutputEncoding(true);
                if (result == null)
                    result = GetLogOutputEncoding(false);
                if (result == null)
                    result = CommitEncoding;
                return result;
            }
        }

        public Settings.PullAction LastPullAction
        {
            get { return Settings.GetEnum<Settings.PullAction>("LastPullAction_" + WorkingDir, Settings.PullAction.None); }
            set { Settings.SetEnum<Settings.PullAction>("LastPullAction_" + WorkingDir, value); }
        }

        public void LastPullActionToPullMerge()
        {
            if (LastPullAction == Settings.PullAction.FetchAll)
                Settings.PullMerge = Settings.PullAction.Fetch;
            else if (LastPullAction != Settings.PullAction.None)
                Settings.PullMerge = LastPullAction;
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

            if (Directory.Exists(dir + Settings.PathSeparator.ToString() + ".git") || File.Exists(dir + Settings.PathSeparator.ToString() + ".git"))
                return true;

            return Directory.Exists(dir + Settings.PathSeparator.ToString() + "info") &&
                   Directory.Exists(dir + Settings.PathSeparator.ToString() + "objects") &&
                   Directory.Exists(dir + Settings.PathSeparator.ToString() + "refs");
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
        public IList<string> GetSubmodulesLocalPathes(bool recursive)
        {
            var configFile = GetSubmoduleConfigFile();
            var submodules = configFile.GetConfigSections().Select(configSection => configSection.GetPathValue("path").Trim()).ToList();
            if (recursive)
            {
                for (int i = 0; i < submodules.Count; i++)
                {
                    var submodule = GetSubmodule(submodules[i]);
                    var submoduleConfigFile = submodule.GetSubmoduleConfigFile();
                    var subsubmodules = submoduleConfigFile.GetConfigSections().Select(configSection => configSection.GetPathValue("path").Trim()).ToList();
                    for (int j = 0; j < subsubmodules.Count; j++)
                        subsubmodules[j] = submodules[i] + '/' + subsubmodules[j];
                    submodules.InsertRange(i + 1, subsubmodules);
                    i += subsubmodules.Count;
                }
            }
            return submodules;
        }

        public IList<string> GetSubmodulesLocalPathes()
        {
            return GetSubmodulesLocalPathes(true);
        }

        public string GetGlobalSetting(string setting)
        {
            var configFile = GitCommandHelpers.GetGlobalConfig();
            return configFile.GetValue(setting);
        }

        public string GetGlobalPathSetting(string setting)
        {
            var configFile = GitCommandHelpers.GetGlobalConfig();
            return configFile.GetPathValue(setting);
        }

        public void SetGlobalSetting(string setting, string value)
        {
            var configFile = GitCommandHelpers.GetGlobalConfig();
            configFile.SetValue(setting, value);
            configFile.Save();
        }

        public void SetGlobalPathSetting(string setting, string value)
        {
            var configFile = GitCommandHelpers.GetGlobalConfig();
            configFile.SetPathValue(setting, value);
            configFile.Save();
        }

        public static string FindGitWorkingDir(string startDir)
        {
            if (string.IsNullOrEmpty(startDir))
                return "";

            startDir = startDir.Trim();

            var pathSeparators = new[] { Settings.PathSeparator, Settings.PathSeparatorWrong };
            var len = startDir.Length;

            while (len > 0 && pathSeparators.Any(s => s == startDir[len - 1]))
                len--;

            startDir = startDir.Substring(0, len) + Settings.PathSeparator.ToString();

            var dir = startDir;

            while (dir.LastIndexOfAny(pathSeparators) > 0)
            {
                dir = dir.Substring(0, dir.LastIndexOfAny(pathSeparators));

                if (ValidWorkingDir(dir))
                    return dir + Settings.PathSeparator.ToString();
            }
            return startDir;
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

        public Process RunRealCmdDetached(string cmd, string arguments)
        {
            try
            {
                return CreateAndStartCommand(cmd, arguments, false);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            return null;
        }

        private Process CreateAndStartCommand(string cmd, string arguments, bool waitForExit)
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

                return null;
            }
            else
            {
                return Process.Start(info);
            }
        }

        public void StartExternalCommand(string cmd, string arguments)
        {
            StartExternalCommand(_workingdir, cmd, arguments);
        }

        public static void StartExternalCommand(string workingdir, string cmd, string arguments)
        {
            try
            {
                GitCommandHelpers.SetEnvironmentVariable();

                var processInfo = new ProcessStartInfo
                                      {
                                          UseShellExecute = false,
                                          RedirectStandardOutput = false,
                                          FileName = cmd,
                                          WorkingDirectory = workingdir,
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
                encoding = GitModule.SystemEncoding;

            byte[] cmdout, cmderr;
            if (GitCommandCache.TryGet(arguments, out cmdout, out cmderr))
                return EncodingHelper.DecodeString(cmdout, cmderr, ref encoding);

            RunCmdByte(cmd, arguments, out cmdout, out cmderr);

            GitCommandCache.Add(arguments, cmdout, cmderr);

            return EncodingHelper.DecodeString(cmdout, cmderr, ref encoding);
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public string RunCachableCmd(string cmd, string arguments)
        {
            return RunCachableCmd(cmd, arguments, GitModule.SystemEncoding);
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public string RunCmd(string cmd, string arguments)
        {
            return RunCmd(cmd, arguments, (byte[])null);
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public string RunCmd(string cmd, string arguments, Encoding encoding)
        {
            return RunCmd(cmd, arguments, null, encoding);
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public string RunCmd(string cmd, string arguments, byte[] stdInput, Encoding encoding)
        {
            int exitCode;
            return RunCmd(cmd, arguments, out exitCode, stdInput, encoding);
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public string RunCmd(string cmd, string arguments, byte[] stdInput)
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
        public string RunCmd(string cmd, string arguments, out int exitCode, byte[] stdInput)
        {
            return RunCmd(cmd, arguments, out exitCode, stdInput, GitModule.SystemEncoding);
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public string RunCmd(string cmd, string arguments, out int exitCode, byte[] stdInput, Encoding encoding)
        {
            byte[] output, error;
            exitCode = RunCmdByte(cmd, arguments, stdInput, out output, out error);
            return EncodingHelper.GetString(output, error, encoding);
        }

        private int RunCmdByte(string cmd, string arguments, out byte[] output, out byte[] error)
        {
            return RunCmdByte(cmd, arguments, null, out output, out error);
        }

        private int RunCmdByte(string cmd, string arguments, byte[] stdInput, out byte[] output, out byte[] error)
        {
            try
            {
                GitCommandHelpers.SetEnvironmentVariable();
                arguments = arguments.Replace("$QUOTE$", "\\\"");
                int exitCode = GitCommandHelpers.CreateAndStartProcess(arguments, cmd, _workingdir, out output, out error, stdInput);
                return exitCode;
            }
            catch (Win32Exception)
            {
                output = error = null;
                return 1;
            }
        }

        public string RunGitCmd(string arguments, out int exitCode, byte[] stdInput)
        {
            return RunGitCmd(arguments, out exitCode, stdInput, SystemEncoding);
        }

        public string RunGitCmd(string arguments, out int exitCode, byte[] stdInput, Encoding encoding)
        {
            return RunCmd(Settings.GitCommand, arguments, out exitCode, stdInput, encoding);
        }

        public string RunGitCmd(string arguments, out int exitCode)
        {
            return RunGitCmd(arguments, out exitCode, null);
        }

        public string RunGitCmd(string arguments, byte[] stdInput)
        {
            int exitCode;
            return RunGitCmd(arguments, out exitCode, stdInput);
        }

        public string RunGitCmd(string arguments, byte[] stdInput, Encoding encoding)
        {
            int exitCode;
            return RunGitCmd(arguments, out exitCode, stdInput, encoding);
        }

        public string RunGitCmd(string arguments)
        {
            return RunGitCmd(arguments, (byte[])null);
        }

        public string RunGitCmd(string arguments, Encoding encoding)
        {
            return RunGitCmd(arguments, null, encoding);
        }

        public string RunGit(string arguments)
        {
            return RunGitCmd(arguments);
        }

        public string RunGit(string arguments, out int exitCode)
        {
            return RunGitCmd(arguments, out exitCode);
        }


        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        private IEnumerable<string> RunCmdAsync(string cmd, string arguments, string stdInput, Encoding encoding)
        {
            GitCommandHelpers.SetEnvironmentVariable();
            arguments = arguments.Replace("$QUOTE$", "\\\"");
            return GitCommandHelpers.CreateAndStartProcessAsync(arguments, cmd, _workingdir, stdInput, encoding);
        }

        public IEnumerable<string> RunGitCmdAsync(string arguments)
        {
            return RunCmdAsync(Settings.GitCommand, arguments, null, GitModule.SystemEncoding);
        }

        public string RunBatchFile(string batchFile)
        {
            string tempFileName = Path.ChangeExtension(Path.GetTempFileName(), ".cmd");
            using (var writer = new StreamWriter(tempFileName))
            {
                writer.WriteLine("@prompt $G");
                writer.Write(batchFile);
            }
            string result = RunCmd("cmd.exe", "/C \"" + tempFileName + "\"");
            File.Delete(tempFileName);
            return result;
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
            string editor = GetEffectivePathSetting("core.editor").ToLower();
            if (editor.Contains("gitextensions") || editor.Contains("notepad") ||
                editor.Contains("notepad++"))
            {
                RunGitCmd("notes edit " + revision);
            }
            else
            {
                RunRealCmd(Settings.GitCommand, "notes edit " + revision);
            }
        }

        public bool InTheMiddleOfConflictedMerge()
        {
            return !string.IsNullOrEmpty(RunGitCmd("ls-files -z --unmerged"));
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
                unmergedFiles.Add(new GitItem(this) { FileName = fileName });
            }

            return unmergedFiles;
        }

        private IEnumerable<string> GetUnmergedFileListing()
        {
            return RunGitCmd("ls-files -z --unmerged").Split(new[] { '\0', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public bool HandleConflictSelectBase(string fileName)
        {
            if (!HandleConflictsSaveSide(fileName, fileName, "1"))
                return false;

            RunGitCmd("add -- \"" + fileName + "\"");
            return true;
        }

        public bool HandleConflictSelectLocal(string fileName)
        {
            if (!HandleConflictsSaveSide(fileName, fileName, "2"))
                return false;

            RunGitCmd("add -- \"" + fileName + "\"");
            return true;
        }

        public bool HandleConflictSelectRemote(string fileName)
        {
            if (!HandleConflictsSaveSide(fileName, fileName, "3"))
                return false;

            RunGitCmd("add -- \"" + fileName + "\"");
            return true;
        }

        public bool HandleConflictsSaveSide(string fileName, string saveAs, string side)
        {
            Directory.SetCurrentDirectory(_workingdir);

            side = GetSide(side);

            fileName = FixPath(fileName);
            var unmerged = RunGitCmd("ls-files -z --unmerged \"" + fileName + "\"").Split(new char[] { '\0', '\n' }, StringSplitOptions.RemoveEmptyEntries);

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
                string autocrlf = GetEffectiveSetting("core.autocrlf").ToLower();
                bool convertcrlf = autocrlf == "true";

                byte[] buf = ms.ToArray();
                if (convertcrlf)
                {
                    if (!FileHelper.IsBinaryFile(this, saveAs) && !FileHelper.IsBinaryFileAccordingToContent(buf))
                    {
                        StreamReader reader = new StreamReader(ms, FilesEncoding);
                        String sfileout = reader.ReadToEnd();
                        sfileout = sfileout.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
                        buf = FilesEncoding.GetBytes(sfileout);
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

            var unmerged = RunGitCmd("ls-files -z --unmerged \"" + filename + "\"").Split(new char[] { '\0', '\n' }, StringSplitOptions.RemoveEmptyEntries);

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

                var tempFile = RunGitCmd("checkout-index --temp --stage=" + stage + " -- " + "\"" + filename + "\"");
                tempFile = tempFile.Split('\t')[0];
                tempFile = Path.Combine(_workingdir, tempFile);

                var newFileName = Path.Combine(_workingdir, fileNames[stage - 1]);
                try
                {
                    fileNames[stage - 1] = newFileName;
                    var index = 1;
                    while (File.Exists(fileNames[stage - 1]) && index < 50)
                    {
                        fileNames[stage - 1] = newFileName + index.ToString();
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

            var unmerged = RunGitCmd("ls-files -z --unmerged \"" + filename + "\"").Split(new[] { '\0', '\n' }, StringSplitOptions.RemoveEmptyEntries);

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
                foreach (string line in lines)
                {
                    if (line.StartsWith("gitdir:"))
                    {
                        string path = line.Substring(7).Trim().Replace('/', '\\');
                        if (Path.IsPathRooted(path))
                            return path + Settings.PathSeparator.ToString();
                        else
                            return Path.GetFullPath(Path.Combine(repositoryPath, path + Settings.PathSeparator.ToString()));
                    }
                }
            }
            return repositoryPath + ".git" + Settings.PathSeparator.ToString();
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

        public Process RunBash(string bashCommand = null)
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
                string cmd = termEmuCmds.FirstOrDefault(termEmuCmd => !string.IsNullOrEmpty(RunCmd("which", termEmuCmd)));

                if (string.IsNullOrEmpty(cmd))
                {
                    cmd = "bash";
                    args = "--login -i";
                }

                return RunRealCmdDetached(cmd, args);
            }
            else
            {
                string args;
                if (string.IsNullOrWhiteSpace(bashCommand))
                {
                    args = " --login -i\"";
                }
                else
                {
                    args = " --login -i -c \"" + bashCommand.Replace("\"", "\\\"") + "\"";
                }

                if (File.Exists(Settings.GitBinDir + "bash.exe"))
                    return RunRealCmdDetached("cmd.exe", "/c \"\"" + Settings.GitBinDir + "bash\"" + args);
                else
                    return RunRealCmdDetached("cmd.exe", "/c \"\"" + Settings.GitBinDir + "sh\"" + args);
            }
        }

        public string Init(bool bare, bool shared)
        {
            if (bare && shared)
                return RunGitCmd("init --bare --shared=all");
            if (bare)
                return RunGitCmd("init --bare");
            return RunGitCmd("init");
        }

        public bool IsMerge(string commit)
        {
            string output = RunGitCmd("log -n 1 --format=format:%P \"" + commit + "\"");
            string[] parents = output.Split(' ');
            if (parents.Length > 1) return true;
            return false;
        }

        public GitRevision[] GetParents(string commit)
        {
            string output = RunGitCmd("log -n 1 --format=format:%P \"" + commit + "\"");
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
                var RevInfo = RunGitCmd(cmd);
                string[] Infos = RevInfo.Split('\n');
                var Revision = new GitRevision(this, Parents[i])
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
            return RunGitCmd(GitCommandHelpers.CherryPickCmd(cherry, commit, arguments));
        }

        public string ShowSha1(string sha1)
        {
            return ReEncodeShowString(RunCachableCmd(Settings.GitCommand, "show " + sha1, GitModule.LosslessEncoding));
        }

        public string UserCommitCount()
        {
            return RunGitCmd("shortlog -s -n");
        }

        public string DeleteBranch(string branchName, bool force, bool remoteBranch)
        {
            return RunGitCmd(GitCommandHelpers.DeleteBranchCmd(branchName, force, remoteBranch));
        }

        public string DeleteTag(string tagName)
        {
            return RunGitCmd(GitCommandHelpers.DeleteTagCmd(tagName));
        }

        public string GetCurrentCheckout()
        {
            return RunGitCmd("log -g -1 HEAD --pretty=format:%H");
        }

        public string GetSuperprojectCurrentCheckout()
        {
            if (SuperprojectModule == null)
                return "";

            var lines = SuperprojectModule.RunGitCmd("submodule status --cached " + _submoduleName).Split('\n');

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
            return ExistsMergeCommit(commitId + "~1", commitId);
        }

        public bool ExistsMergeCommit(string startRev, string endRev)
        {
            if (startRev.IsNullOrEmpty() || endRev.IsNullOrEmpty())
                return false;

            string revisions = RunGitCmd("rev-list --parents --no-walk " + startRev + ".." + endRev);
            string[] revisionsTab = revisions.Split('\n');
            Func<string, bool> ex = (string parents) =>
                {
                    string[] tab = parents.Split(' ');
                    return tab.Length > 2 && tab.All(parent => GitRevision.Sha1HashRegex.IsMatch(parent));
                };
            return revisionsTab.Any(ex);
        }

        public ConfigFile GetSubmoduleConfigFile()
        {
            return new ConfigFile(_workingdir + ".gitmodules", true);
        }

        public string GetCurrentSubmoduleLocalPath()
        {
            if (SuperprojectModule == null)
                return null;
            string submodulePath = WorkingDir.Substring(SuperprojectModule.WorkingDir.Length);
            submodulePath = submodulePath.Replace(Settings.PathSeparator, Settings.PathSeparatorWrong).TrimEnd(
                    Settings.PathSeparatorWrong);
            return submodulePath;
        }

        public string GetSubmoduleNameByPath(string localPath)
        {
            var configFile = GetSubmoduleConfigFile();
            var submodule = configFile.GetConfigSections().FirstOrDefault(configSection => configSection.GetPathValue("path").Trim() == localPath);
            if (submodule != null)
                return submodule.SubSection.Trim();
            return null;
        }

        public string GetSubmoduleLocalPath(string name)
        {
            var configFile = GetSubmoduleConfigFile();
            return configFile.GetPathValue(string.Format("submodule.{0}.path", name)).Trim();
        }

        public string GetSubmoduleRemotePath(string name)
        {
            var configFile = GetSubmoduleConfigFile();
            return configFile.GetPathValue(string.Format("submodule.{0}.url", name)).Trim();
        }

        public string GetSubmoduleFullPath(string localPath)
        {
            string dir = _workingdir + localPath + Settings.PathSeparator.ToString();
            return Path.GetFullPath(dir); // fix slashes
        }

        public GitModule GetSubmodule(string localPath)
        {
            return new GitModule(GetSubmoduleFullPath(localPath));
        }

        public IGitModule GetISubmodule(string submoduleName)
        {
            return GetSubmodule(submoduleName);
        }

        public IEnumerable<IGitSubmodule> GetSubmodules()
        {
            var submodules = RunGitCmdAsync("submodule status");

            string lastLine = null;

            foreach (var submodule in submodules)
            {
                if (submodule.Length < 43)
                    continue;

                if (submodule.Equals(lastLine))
                    continue;

                lastLine = submodule;

                yield return CreateGitSubmodule(this, submodule);
            }
        }

        public string FindGitSuperprojectPath(out string submoduleName)
        {
            submoduleName = null;
            if (String.IsNullOrEmpty(_workingdir))
                return null;

            string superprojectPath = null;

            string currentPath = Path.GetDirectoryName(_workingdir); // remove last slash
            if (!string.IsNullOrEmpty(currentPath))
            {
                string path = Path.GetDirectoryName(currentPath);
                if (!string.IsNullOrEmpty(path))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (File.Exists(path + Settings.PathSeparator.ToString() + ".gitmodules") &&
                            ValidWorkingDir(path + Settings.PathSeparator.ToString()))
                        {
                            superprojectPath = path + Settings.PathSeparator.ToString();
                            break;
                        }
                        // Check upper directory
                        path = Path.GetDirectoryName(path);
                    }
                }
            }

            if (File.Exists(_workingdir + ".git") &&
                superprojectPath == null)
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
                            gitpath = Path.GetFullPath(_workingdir + gitpath);
                            if (File.Exists(gitpath + ".gitmodules") && ValidWorkingDir(gitpath))
                                superprojectPath = gitpath;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(superprojectPath))
            {
                var localPath = currentPath.Substring(superprojectPath.Length);
                var configFile = new ConfigFile(superprojectPath + ".gitmodules", true);
                foreach (ConfigSection configSection in configFile.GetConfigSections())
                {
                    if (configSection.GetPathValue("path") == FixPath(localPath))
                    {
                        submoduleName = configSection.SubSection;
                        return superprojectPath;
                    }
                }
            }

            return null;
        }

        internal static GitSubmodule CreateGitSubmodule(GitModule aModule, string submodule)
        {
            var gitSubmodule =
                new GitSubmodule(aModule)
                    {
                        Initialized = submodule[0] != '-',
                        UpToDate = submodule[0] != '+',
                        CurrentCommitGuid = submodule.Substring(1, 40).Trim()
                    };

            var localPath = submodule.Substring(42).Trim();
            if (localPath.Contains("("))
            {
                gitSubmodule.LocalPath = localPath.Substring(0, localPath.IndexOf("(")).TrimEnd();
                gitSubmodule.Branch = localPath.Substring(localPath.IndexOf("(")).Trim(new[] { '(', ')', ' ' });
            }
            else
                gitSubmodule.LocalPath = localPath;
            return gitSubmodule;
        }

        public string GetSubmoduleSummary(string submodule)
        {
            var arguments = string.Format("submodule summary {0}", submodule);
            return RunGitCmd(arguments);
        }

        public string Stash()
        {
            var arguments = GitCommandHelpers.StashSaveCmd(Settings.IncludeUntrackedFilesInAutoStash);
            return RunGitCmd(arguments);
        }

        public string StashApply()
        {
            return RunGitCmd("stash apply");
        }

        public string StashClear()
        {
            return RunGitCmd("stash clear");
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

            return RunGitCmd(args);
        }

        public string ResetMixed(string commit, string file)
        {
            var args = "reset --mixed";

            if (!string.IsNullOrEmpty(commit))
                args += " \"" + commit + "\"";

            if (!string.IsNullOrEmpty(file))
                args += " -- \"" + file + "\"";

            return RunGitCmd(args);
        }

        public string ResetHard(string commit, string file)
        {
            var args = "reset --hard";

            if (!string.IsNullOrEmpty(commit))
                args += " \"" + commit + "\"";

            if (!string.IsNullOrEmpty(file))
                args += " -- \"" + file + "\"";

            return RunGitCmd(args);
        }

        public string ResetFile(string file)
        {
            file = FixPath(file);
            return RunGitCmd("checkout-index --index --force -- \"" + file + "\"");
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


        public string Tag(string tagName, string revision, bool annotation, bool force)
        {
            return annotation
                ? RunCmd(Settings.GitCommand,
                                "tag \"" + tagName.Trim() + "\" -a " + (force ? "-f" : "") + " -F \"" + WorkingDirGitDir() +
                                "\\TAGMESSAGE\" -- \"" + revision + "\"")
                : RunGitCmd("tag " + (force ? "-f" : "") + " \"" + tagName.Trim() + "\" \"" + revision + "\"");
        }

        public string Branch(string branchName, string revision, bool checkout)
        {
            return RunGitCmd(GitCommandHelpers.BranchCmd(branchName, revision, checkout));
        }

        public string CheckoutFiles(IEnumerable<string> fileList, string revision, bool force)
        {
            string files = fileList.Select(s => s.Quote()).Join(" ");
            return RunGitCmd("checkout " + force.AsForce() + revision.Quote() + " -- " + files);
        }


        public string Push(string path)
        {
            return RunGitCmd("push \"" + FixPath(path).Trim() + "\"");
        }

        public bool StartPageantForRemote(string remote)
        {
            var sshKeyFile = GetPuttyKeyFileForRemote(remote);
            if (string.IsNullOrEmpty(sshKeyFile))
                return false;

            StartPageantWithKey(sshKeyFile);
            return true;
        }

        public static void StartPageantWithKey(string sshKeyFile)
        {
            StartExternalCommand(string.Empty, Settings.Pageant, "\"" + sshKeyFile + "\"");
        }

        public string GetPuttyKeyFileForRemote(string remote)
        {
            if (string.IsNullOrEmpty(remote) ||
                string.IsNullOrEmpty(Settings.Pageant) ||
                !Settings.AutoStartPageant ||
                !GitCommandHelpers.Plink())
                return "";

            return GetPathSetting(string.Format("remote.{0}.puttykeyfile", remote));
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

        public string FetchCmd(string remote, string remoteBranch, string localBranch, bool noTags)
        {
            var progressOption = "";
            if (GitCommandHelpers.VersionInUse.FetchCanAskForProgress)
                progressOption = "--progress ";

            if (string.IsNullOrEmpty(remote) && string.IsNullOrEmpty(remoteBranch) && string.IsNullOrEmpty(localBranch))
                return "fetch " + progressOption;

            return "fetch " + progressOption + GetFetchArgs(remote, remoteBranch, localBranch, noTags);
        }

        public string FetchCmd(string remote, string remoteBranch, string localBranch)
        {
            return FetchCmd(remote, remoteBranch, localBranch, false);
        }

        public string Pull(string remote, string remoteBranch, string localBranch, bool rebase)
        {
            remote = FixPath(remote);

            Directory.SetCurrentDirectory(_workingdir);

            RunRealCmd("cmd.exe", " /k \"\"" + Settings.GitCommand + "\" " + PullCmd(remote, localBranch, remoteBranch, rebase) + "\"");

            return "Done";
        }

        public string PullCmd(string remote, string remoteBranch, string localBranch, bool rebase, bool noTags)
        {
            var progressOption = "";
            if (GitCommandHelpers.VersionInUse.FetchCanAskForProgress)
                progressOption = "--progress ";

            if (rebase && !string.IsNullOrEmpty(remoteBranch))
                return "pull --rebase " + progressOption + remote + " refs/heads/" + remoteBranch;

            if (rebase)
                return "pull --rebase " + progressOption + remote;

            return "pull " + progressOption + GetFetchArgs(remote, remoteBranch, localBranch, noTags);
        }

        public string PullCmd(string remote, string remoteBranch, string localBranch, bool rebase)
        {
            return PullCmd(remote, remoteBranch, localBranch, rebase, false);
        }

        private string GetFetchArgs(string remote, string remoteBranch, string localBranch, bool noTags)
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
            var remoteUrl = GetPathSetting(string.Format(SettingKeyString.RemoteUrl, remote));

            if (PathIsUrl(remote) && !string.IsNullOrEmpty(localBranch) && string.IsNullOrEmpty(remoteUrl))
                localBranchArguments = ":refs/heads/" + localBranch + "";
            else if (string.IsNullOrEmpty(localBranch) || PathIsUrl(remote) || string.IsNullOrEmpty(remoteUrl))
                localBranchArguments = "";
            else
                localBranchArguments = ":" + "refs/remotes/" + remote.Trim() + "/" + localBranch + "";

            string arguments = noTags ? " --no-tags" : "";

            return "\"" + remote.Trim() + "\" " + remoteBranchArguments + localBranchArguments + arguments;
        }

        public string ContinueRebase()
        {
            Directory.SetCurrentDirectory(_workingdir);

            var result = RunGitCmd(GitCommandHelpers.ContinueRebaseCmd());

            return result;
        }

        public string SkipRebase()
        {
            Directory.SetCurrentDirectory(_workingdir);

            var result = RunGitCmd(GitCommandHelpers.SkipRebaseCmd());

            return result;
        }

        public string GetRebaseDir()
        {
            string gitDirectory = GetGitDirectory();
            if (Directory.Exists(gitDirectory + "rebase-merge" + Settings.PathSeparator.ToString()))
                return gitDirectory + "rebase-merge" + Settings.PathSeparator.ToString();
            if (Directory.Exists(gitDirectory + "rebase-apply" + Settings.PathSeparator.ToString()))
                return gitDirectory + "rebase-apply" + Settings.PathSeparator.ToString();
            if (Directory.Exists(gitDirectory + "rebase" + Settings.PathSeparator.ToString()))
                return gitDirectory + "rebase" + Settings.PathSeparator.ToString();

            return "";
        }

        public string ApplyPatch(string dir, string amCommand)
        {
            var output = string.Empty;

            using (var gitCommand = new GitCommandsInstance(this))
            {

                var files = Directory.GetFiles(dir);

                if (files.Length > 0)
                    using (Process process1 = gitCommand.CmdStartProcess(Settings.GitCommand, amCommand))
                    {
                        foreach (var file in files)
                        {
                            using (FileStream fs = new FileStream(file, FileMode.Open))
                            {
                                fs.CopyTo(process1.StandardInput.BaseStream);
                            }
                        }
                        process1.StandardInput.Close();
                        process1.WaitForExit();

                        if (gitCommand.Output != null)
                            output = gitCommand.Output.ToString().Trim();
                    }
            }

            return output;
        }

        public string StageFiles(IList<GitItemStatus> files)
        {
            var gitCommand = new GitCommandsInstance(this);

            var output = "";

            Process process1 = null;
            foreach (var file in files)
            {
                if (file.IsDeleted)
                    continue;
                if (process1 == null)
                    process1 = gitCommand.CmdStartProcess(Settings.GitCommand, "update-index --add --stdin");

                //process1.StandardInput.WriteLine("\"" + FixPath(file.Name) + "\"");
                byte[] bytearr = EncodingHelper.ConvertTo(GitModule.SystemEncoding, "\"" + FixPath(file.Name) + "\"" + process1.StandardInput.NewLine);
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
                byte[] bytearr = EncodingHelper.ConvertTo(GitModule.SystemEncoding, "\"" + FixPath(file.Name) + "\"" + process2.StandardInput.NewLine);
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

        public string UnstageFiles(List<GitItemStatus> files)
        {
            var gitCommand = new GitCommandsInstance(this);

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
                byte[] bytearr = EncodingHelper.ConvertTo(GitModule.SystemEncoding, "\"" + FixPath(file.Name) + "\"" + process2.StandardInput.NewLine);
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
      
        public bool InTheMiddleOfBisect()
        {
            return File.Exists(WorkingDirGitDir() + Settings.PathSeparator.ToString() + "BISECT_START");
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
                var file = fullFileName.Substring(fullFileName.LastIndexOf(Settings.PathSeparator.ToString()) + 1);
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

            return RunGitCmd(GitCommandHelpers.RebaseCmd(branch, false, false, false));
        }

        public string AbortRebase()
        {
            Directory.SetCurrentDirectory(_workingdir);

            return RunGitCmd(GitCommandHelpers.AbortRebaseCmd());
        }

        public string Resolved()
        {
            Directory.SetCurrentDirectory(_workingdir);

            return RunGitCmd(GitCommandHelpers.ResolvedCmd());
        }

        public string Skip()
        {
            Directory.SetCurrentDirectory(_workingdir);

            return RunGitCmd(GitCommandHelpers.SkipCmd());
        }

        public string Abort()
        {
            Directory.SetCurrentDirectory(_workingdir);

            return RunGitCmd(GitCommandHelpers.AbortCmd());
        }

        public string Commit(bool amend)
        {
            return Commit(amend, "");
        }

        public string Commit(bool amend, string author)
        {
            return RunGitCmd(CommitCmd(amend, author));
        }

        public string CommitCmd(bool amend)
        {
            return CommitCmd(amend, false, "", true);
        }

        public string CommitCmd(bool amend, string author)
        {
            return CommitCmd(amend, false, author, true);
        }

        public string CommitCmd(bool amend, bool signOff, string author, bool useExplicitCommitMessage)
        {
            string command = "commit";
            if (amend)
                command += " --amend";

            if (signOff)
                command += " --signoff";

            if (!string.IsNullOrEmpty(author))
                command += " --author=\"" + author + "\"";

            if (useExplicitCommitMessage)
            {
                var path = WorkingDirGitDir() + Settings.PathSeparator.ToString() + "COMMITMESSAGE\"";
                command += " -F \"" + path;
            }

            return command;
        }

        public string Patch(string patchFile)
        {
            Directory.SetCurrentDirectory(_workingdir);

            return RunGitCmd(GitCommandHelpers.PatchCmd(FixPath(patchFile)));
        }

        public string UpdateRemotes()
        {
            return RunGitCmd("remote update");
        }

        public string RemoveRemote(string name)
        {
            return RunGitCmd("remote rm \"" + name + "\"");
        }

        public string RenameRemote(string name, string newName)
        {
            return RunGitCmd("remote rename \"" + name + "\" \"" + newName + "\"");
        }

        public string Rename(string name, string newName)
        {
            return RunGitCmd("branch -m \"" + name + "\" \"" + newName + "\"");
        }

        public string AddRemote(string name, string path)
        {
            var location = FixPath(path);

            if (string.IsNullOrEmpty(name))
                return "Please enter a name.";

            return
                string.IsNullOrEmpty(location)
                    ? RunGitCmd(string.Format("remote add \"{0}\" \"\"", name))
                    : RunGitCmd(string.Format("remote add \"{0}\" \"{1}\"", name, location));
        }

        public string[] GetRemotes()
        {
            return GetRemotes(true);
        }

        public string[] GetRemotes(bool allowEmpty)
        {
            string remotes = RunGitCmd("remote show");
            return allowEmpty ? remotes.Split('\n') : remotes.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public ConfigFile GetLocalConfig()
        {
            return new ConfigFile(WorkingDirGitDir() + Settings.PathSeparator.ToString() + "config", true);
        }

        public string GetSetting(string setting)
        {
            var configFile = GetLocalConfig();
            return configFile.GetValue(setting);
        }

        public string GetISetting(string setting)
        {
            return GetSetting(setting);
        }

        public string GetPathSetting(string setting)
        {
            var configFile = GetLocalConfig();
            return configFile.GetPathValue(setting);
        }

        public string GetEffectiveSetting(string setting)
        {
            var localConfig = GetLocalConfig();
            if (localConfig.HasValue(setting))
                return localConfig.GetValue(setting);

            return GitCommandHelpers.GetGlobalConfig().GetValue(setting);
        }

        public string GetEffectivePathSetting(string setting)
        {
            var localConfig = GetLocalConfig();
            if (localConfig.HasValue(setting))
                return localConfig.GetPathValue(setting);

            return GitCommandHelpers.GetGlobalConfig().GetPathValue(setting);
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

        public void SetPathSetting(string setting, string value)
        {
            var configFile = GetLocalConfig();
            configFile.SetPathValue(setting, value);
            configFile.Save();
        }

        public List<Patch> GetStashedItems(string stashName)
        {
            var patchManager = new PatchManager();
            patchManager.LoadPatch(RunGitCmd("stash show -p " + stashName, GitModule.LosslessEncoding), false, FilesEncoding);

            return patchManager.Patches;
        }

        public List<GitStash> GetStashes()
        {
            var list = RunGitCmd("stash list").Split('\n');

            var stashes = new List<GitStash>();
            foreach (var stashString in list)
            {
                if (stashString.IndexOf(':') <= 0)
                    continue;

                var stash = new GitStash
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
            string fileA = null;
            string fileB = null;

            if (!string.IsNullOrEmpty(fileName))
            {
                fileB = fileName;
                fileName = string.Concat("\"", FixPath(fileName), "\"");
            }

            if (!string.IsNullOrEmpty(oldFileName))
            {
                fileA = oldFileName;
                oldFileName = string.Concat("\"", FixPath(oldFileName), "\"");
            }

            if (fileA.IsNullOrEmpty())
                fileA = fileB;
            else if (fileB.IsNullOrEmpty())
                fileB = fileA;

            from = FixPath(from);
            to = FixPath(to);
            string commitRange = string.Empty;
            if (!to.IsNullOrEmpty())
                commitRange = "\"" + to + "\"";
            if (!from.IsNullOrEmpty())
                commitRange = string.Join(" ", commitRange, "\"" + from + "\"");

            if (Settings.UsePatienceDiffAlgorithm)
                extraDiffArguments = string.Concat(extraDiffArguments, " --patience");

            var patchManager = new PatchManager();
            var arguments = string.Format("diff {0} -M -C {1} -- {2} {3}", extraDiffArguments, commitRange, fileName, oldFileName);
            patchManager.LoadPatch(this.RunCachableCmd(Settings.GitCommand, arguments, GitModule.LosslessEncoding), false, encoding);

            foreach (Patch p in patchManager.Patches)
                if (p.FileNameA.Equals(fileA) && p.FileNameB.Equals(fileB) ||
                    p.FileNameA.Equals(fileB) && p.FileNameB.Equals(fileA))
                    return p;

            return patchManager.Patches.Count > 0 ? patchManager.Patches[patchManager.Patches.Count - 1] : null;
        }

        public Patch GetSingleDiff(string @from, string to, string fileName, string extraDiffArguments, Encoding encoding)
        {
            return this.GetSingleDiff(from, to, fileName, null, extraDiffArguments, encoding);
        }

        public string GetStatusText(bool untracked)
        {
            string cmd = "status -s";
            if (untracked)
                cmd = cmd + " -u";
            return RunGitCmd(cmd);
        }

        public string GetDiffFilesText(string from, string to)
        {
            return GetDiffFilesText(from, to, false);
        }

        public string GetDiffFilesText(string from, string to, bool noCache)
        {
            string cmd = "diff -M -C --name-status \"" + to + "\" \"" + from + "\"";
            return noCache ? RunGitCmd(cmd) : this.RunCachableCmd(Settings.GitCommand, cmd, GitModule.SystemEncoding);
        }

        public List<GitItemStatus> GetDiffFiles(string from, string to)
        {
            return GetDiffFiles(from, to, false);
        }

        public List<GitItemStatus> GetDiffFiles(string from, string to, bool noCache)
        {
            string cmd = "diff -M -C -z --name-status \"" + to + "\" \"" + from + "\"";
            string result = noCache ? RunGitCmd(cmd) : this.RunCachableCmd(Settings.GitCommand, cmd, GitModule.SystemEncoding);
            return GitCommandHelpers.GetAllChangedFilesFromString(this, result, true);
        }

        public List<GitItemStatus> GetStashDiffFiles(string stashName)
        {
            bool gitShowsUntrackedFiles = false;

            var list = GetDiffFiles(stashName, stashName + "^", true);
            if (!gitShowsUntrackedFiles)
            {
                string untrackedTreeHash = RunGitCmd("log " + stashName + "^3 --pretty=format:\"%T\" --max-count=1");
                if (GitRevision.Sha1HashRegex.IsMatch(untrackedTreeHash))
                    list.AddRange(GetTreeFiles(untrackedTreeHash, true));
            }

            return list;
        }

        public List<GitItemStatus> GetUntrackedFiles()
        {
            var status = RunCmd(Settings.GitCommand,
                                "ls-files -z --others --directory --no-empty-directory --exclude-standard");

            return status.Split(new char[] { '\0', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(statusString => statusString.Trim())
                .Where(statusString => !string.IsNullOrEmpty(statusString))
                .Select(statusString => new GitItemStatus
            {
                IsNew = true,
                IsChanged = false,
                IsDeleted = false,
                IsTracked = false,
                Name = statusString
            })
                .ToList();

        }

        public List<GitItemStatus> GetTreeFiles(string treeGuid, bool full)
        {
            var tree = GetTree(treeGuid, full);

            var list = tree
                .Select(file => new GitItemStatus
                {
                    IsNew = true,
                    IsChanged = false,
                    IsDeleted = false,
                    IsStaged = false,
                    Name = file.Name,
                    TreeGuid = file.Guid
                })
                .ToList();

            // Doesn't work with removed submodules
            IList<string> Submodules = GetSubmodulesLocalPathes();
            foreach (var item in list)
            {
                if (Submodules.Contains(item.Name))
                    item.IsSubmodule = true;
            }
            return list;
        }


        public List<GitItemStatus> GetAllChangedFiles()
        {
            return GetAllChangedFiles(true, true);
        }

        public List<GitItemStatus> GetAllChangedFiles(bool excludeIgnoredFiles, bool untrackedFiles)
        {
            var status = RunGitCmd(GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles, untrackedFiles));

            return GitCommandHelpers.GetAllChangedFilesFromString(this, status);
        }

        public List<GitItemStatus> GetAllChangedFilesWithSubmodulesStatus(bool excludeIgnoredFiles, bool untrackedFiles)
        {
            var status = GetAllChangedFiles(excludeIgnoredFiles, untrackedFiles);

            foreach(var item in status)
                if (item.IsSubmodule)
                {
                    item.SubmoduleStatus = GitCommandHelpers.GetSubmoduleChanges(this, item.Name, item.OldName, item.IsStaged);
                    if (item.SubmoduleStatus.Commit != item.SubmoduleStatus.OldCommit)
                    {
                        var submodule = item.SubmoduleStatus.GetSubmodule(this);
                        item.SubmoduleStatus.CheckIsCommitNewer(submodule);
                    }
                }
            return status;
        }

        public List<GitItemStatus> GetTrackedChangedFiles()
        {
            var status = RunGitCmd(GitCommandHelpers.GetAllChangedFilesCmd(true, false));

            return GitCommandHelpers.GetAllChangedFilesFromString(this, status);
        }

        public List<GitItemStatus> GetDeletedFiles()
        {
            var status = RunGitCmd("ls-files -z --deleted --exclude-standard");

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
            var status = RunGitCmd("diff -z --cached --numstat -- \"" + filename + "\"");
            return !string.IsNullOrEmpty(status);
        }

        public IList<GitItemStatus> GetStagedFiles()
        {
            string status = RunGitCmd("diff -M -C -z --cached --name-status", GitModule.SystemEncoding);

            if (status.Length < 50 && status.Contains("fatal: No HEAD commit to compare"))
            {
                //This command is a little more expensive because it will return both staged and unstaged files
                string command = GitCommandHelpers.GetAllChangedFilesCmd(true, false);
                status = RunGitCmd(command, GitModule.SystemEncoding);
                List<GitItemStatus> stagedFiles = GitCommandHelpers.GetAllChangedFilesFromString(this, status, false);
                return stagedFiles.Where(f => f.IsStaged).ToList<GitItemStatus>();
            }

            return GitCommandHelpers.GetAllChangedFilesFromString(this, status, true);
        }

        public IList<GitItemStatus> GetUnstagedFiles()
        {
            return GetAllChangedFiles().Where(x => !x.IsStaged).ToArray();
        }

        public IList<GitItemStatus> GitStatus()
        {
            return GitStatus(UntrackedFilesMode.Default, 0);
        }

        public IList<GitItemStatus> GitStatus(UntrackedFilesMode untrackedFilesMode, IgnoreSubmodulesMode ignoreSubmodulesMode)
        {
            if (!GitCommandHelpers.VersionInUse.SupportGitStatusPorcelain)
                throw new Exception("The version of git you are using is not supported for this action. Please upgrade to git 1.7.3 or newer.");

            string command = GitCommandHelpers.GetAllChangedFilesCmd(true, untrackedFilesMode, ignoreSubmodulesMode);
            string status = RunGitCmd(command);
            return GitCommandHelpers.GetAllChangedFilesFromString(this, status);
        }

        public bool IsDirtyDir()
        {
            return GitStatus(UntrackedFilesMode.All, IgnoreSubmodulesMode.Default).Count > 0;
        }

        public string GetCurrentChanges(string fileName, string oldFileName, bool staged, string extraDiffArguments, Encoding encoding)
        {
            fileName = string.Concat("\"", FixPath(fileName), "\"");
            if (!string.IsNullOrEmpty(oldFileName))
                oldFileName = string.Concat("\"", FixPath(oldFileName), "\"");

            if (Settings.UsePatienceDiffAlgorithm)
                extraDiffArguments = string.Concat(extraDiffArguments, " --patience");

            var args = string.Concat("diff ", extraDiffArguments, " -- ", fileName);
            if (staged)
                args = string.Concat("diff -M -C --cached", extraDiffArguments, " -- ", fileName, " ", oldFileName);

            String result = RunGitCmd(args, GitModule.LosslessEncoding);
            var patchManager = new PatchManager();
            patchManager.LoadPatch(result, false, encoding);

            return patchManager.Patches.Count > 0 ? patchManager.Patches[patchManager.Patches.Count - 1].Text : string.Empty;
        }

        public string StageFile(string file)
        {
            return RunGitCmd("update-index --add" + " \"" + FixPath(file) + "\"");
        }

        public string StageFileToRemove(string file)
        {
            return RunGitCmd("update-index --remove" + " \"" + FixPath(file) + "\"");
        }


        public string UnstageFile(string file)
        {
            return RunGitCmd("rm" + " --cached \"" + FixPath(file) + "\"");
        }

        public string UnstageFileToRemove(string file)
        {
            return RunGitCmd("reset HEAD -- \"" + FixPath(file) + "\"");
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
                head = File.ReadAllText(headFileName, SystemEncoding);
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
                head = RunGitCmd("symbolic-ref HEAD", out exitcode);
                if (exitcode == 1)
                    return "(no branch)";
            }

            return head;
        }

        public string GetSelectedBranch()
        {
            return GetSelectedBranch(_workingdir);
        }

        public string GetCurrentRemote()
        {
            string remote = GetSetting(string.Format("branch.{0}.remote", GetSelectedBranch()));
            if (String.IsNullOrEmpty(remote))
                return "origin";
            return remote;
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

            var tree = GetTreeFromRemoteHeads(remote, tags, branches);
            return GetHeads(tree);
        }

        private string GetTreeFromRemoteHeads(string remote, bool tags, bool branches)
        {
            if (tags && branches)
                return RunGitCmd("ls-remote --heads --tags \"" + remote + "\"");
            if (tags)
                return RunGitCmd("ls-remote --tags \"" + remote + "\"");
            if (branches)
                return RunGitCmd("ls-remote --heads \"" + remote + "\"");
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

        public ICollection<string> GetMergedBranches()
        {
            return RunGitCmd(GitCommandHelpers.MergedBranches()).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private string GetTree(bool tags, bool branches)
        {
            if (tags && branches)
                return RunGitCmd("show-ref --dereference", GitModule.SystemEncoding);

            if (tags)
                return RunGitCmd("show-ref --tags", GitModule.SystemEncoding);

            if (branches)
                return RunGitCmd("show-ref --dereference --heads", GitModule.SystemEncoding);
            return "";
        }

        private List<GitHead> GetHeads(string tree)
        {
            var itemsStrings = tree.Split('\n');

            var heads = new List<GitHead>();
            var defaultHeads = new Dictionary<string, GitHead>(); // remote -> HEAD
            var remotes = GetRemotes(false);

            foreach (var itemsString in itemsStrings)
            {
                if (itemsString == null || itemsString.Length <= 42)
                    continue;

                var completeName = itemsString.Substring(41).Trim();
                var guid = itemsString.Substring(0, 40);
                var remoteName = GetRemoteName(completeName, remotes);
                var head = new GitHead(this, guid, completeName, remoteName);
                if (DefaultHeadPattern.IsMatch(completeName))
                    defaultHeads[remoteName] = head;
                else
                    heads.Add(head);
            }

            // do not show default head if remote has a branch on the same commit
            GitHead defaultHead;
            foreach (var head in heads.Where(head => defaultHeads.TryGetValue(head.Remote, out defaultHead) && head.Guid == defaultHead.Guid))
            {
                defaultHeads.Remove(head.Remote);
            }

            heads.AddRange(defaultHeads.Values);

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

        public IList<string> GetFiles(IEnumerable<string> filePatterns)
        {
            var quotedPatterns = filePatterns
                .Where(pattern => !pattern.Contains("\""))
                .Select(pattern => pattern.Quote())
                .Join(" ");
            // filter duplicates out of the result because options -c and -m may return 
            // same files at times
            return RunGitCmd("ls-files -z -o -m -c " + quotedPatterns)
                .Split(new[] { '\0', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToList();
        }

        public List<GitItem> GetFileChanges(string file)
        {
            file = FixPath(file);
            var tree = RunGitCmd("whatchanged --all -- \"" + file + "\"");

            var itemsStrings = tree.Split('\n');

            var items = new List<GitItem>();

            GitItem item = null;
            foreach (var itemsString in itemsStrings)
            {
                if (itemsString.StartsWith("commit "))
                {
                    item = new GitItem(this) { CommitGuid = itemsString.Substring(7).Trim() };

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
            string tree = this.RunCachableCmd(Settings.GitCommand, string.Format("ls-tree -z -r --name-only {0}", id), GitModule.SystemEncoding);
            return tree.Split(new char[] { '\0', '\n' });
        }

        public List<IGitItem> GetTree(string id, bool full)
        {
            string args = "-z";
            if (full)
                args += " -r";
            var tree = this.RunCachableCmd(Settings.GitCommand, "ls-tree " + args + " \"" + id + "\"", GitModule.SystemEncoding);

            return GitItem.CreateIGitItemsFromString(this, tree);
        }

        public GitBlame Blame(string filename, string from, Encoding encoding)
        {
            return Blame(filename, from, null, encoding);
        }

        public GitBlame Blame(string filename, string from, string lines, Encoding encoding)
        {
            from = FixPath(from);
            filename = FixPath(filename);
            string blameCommand = string.Format("blame --porcelain -M -w -l{0} \"{1}\" -- \"{2}\"", lines != null ? " -L " + lines : "", from, filename);
            var itemsStrings =
                RunCachableCmd(
                    Settings.GitCommand,
                    blameCommand,
                    GitModule.LosslessEncoding
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
                    {
                        blameLine.LineText = line.Substring(1) //trim ONLY first tab
                                                 .Trim(new char[] { '\r' }); //trim \r, this is a workaround for a \r\n bug
                        blameLine.LineText = ReEncodeStringFromLossless(blameLine.LineText, encoding);
                    }
                    else if (line.StartsWith("author-mail"))
                        blameHeader.AuthorMail = ReEncodeStringFromLossless(line.Substring("author-mail".Length).Trim());
                    else if (line.StartsWith("author-time"))
                        blameHeader.AuthorTime = DateTimeUtils.ParseUnixTime(line.Substring("author-time".Length).Trim());
                    else if (line.StartsWith("author-tz"))
                        blameHeader.AuthorTimeZone = line.Substring("author-tz".Length).Trim();
                    else if (line.StartsWith("author"))
                    {
                        blameHeader = new GitBlameHeader();
                        blameHeader.CommitGuid = blameLine.CommitGuid;
                        blameHeader.Author = ReEncodeStringFromLossless(line.Substring("author".Length).Trim());
                        blame.Headers.Add(blameHeader);
                    }
                    else if (line.StartsWith("committer-mail"))
                        blameHeader.CommitterMail = line.Substring("committer-mail".Length).Trim();
                    else if (line.StartsWith("committer-time"))
                        blameHeader.CommitterTime = DateTimeUtils.ParseUnixTime(line.Substring("committer-time".Length).Trim());
                    else if (line.StartsWith("committer-tz"))
                        blameHeader.CommitterTimeZone = line.Substring("committer-tz".Length).Trim();
                    else if (line.StartsWith("committer"))
                        blameHeader.Committer = ReEncodeStringFromLossless(line.Substring("committer".Length).Trim());
                    else if (line.StartsWith("summary"))
                        blameHeader.Summary = ReEncodeStringFromLossless(line.Substring("summary".Length).Trim());
                    else if (line.StartsWith("filename"))
                        blameHeader.FileName = ReEncodeFileNameFromLossless(line.Substring("filename".Length).Trim());
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

        public string GetFileRevisionText(string file, string revision, Encoding encoding)
        {
            return
                this.RunCachableCmd(
                    Settings.GitCommand,
                    string.Format("show {0}:\"{1}\"", revision, file.Replace('\\', '/')), encoding);
        }

        public string GetFileText(string id, Encoding encoding)
        {
            return RunCachableCmd(Settings.GitCommand, "cat-file blob \"" + id + "\"", encoding);
        }

        public string GetFileBlobHash(string fileName, string revision)
        {
            if (revision == GitRevision.UncommittedWorkingDirGuid) //working dir changes
            {
                return null;
            }
            else if (revision == GitRevision.IndexGuid) //index
            {
                string blob = RunGitCmd(string.Format("ls-files -s \"{0}\"", fileName));
                string[] s = blob.Split(new char[] { ' ', '\t' });
                if (s.Length >= 2)
                    return s[1];
                else
                    return string.Empty;

            }
            else
            {
                string blob = RunGitCmd(string.Format("ls-tree -r {0} \"{1}\"", revision, fileName));
                string[] s = blob.Split(new char[] { ' ', '\t' });
                if (s.Length >= 3)
                    return s[2];
                else
                    return string.Empty;
            }


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

        public string GetPreviousCommitMessage(int numberBack)
        {
            return GetPreviousCommitMessage("HEAD", numberBack);
        }

        public string GetPreviousCommitMessage(string revision, int numberBack)
        {
            string msg = RunCmd(Settings.GitCommand, "log -n 1 " + revision + "~" + numberBack + " --pretty=format:%e%n%s%n%n%b ", GitModule.LosslessEncoding);
            int idx = msg.IndexOf("\n");
            string encodingName = msg.Substring(0, idx);
            msg = msg.Substring(idx + 1, msg.Length - idx - 1);
            msg = ReEncodeCommitMessage(msg, encodingName);
            return msg;
        }


        public string MergeBranch(string branch)
        {
            return RunGitCmd(GitCommandHelpers.MergeBranchCmd(branch, true, false, false, null));
        }

        public string OpenWithDifftool(string filename)
        {
            return OpenWithDifftool(filename, string.Empty, null, null);
        }

        public string OpenWithDifftool(string filename, string revision1)
        {
            return OpenWithDifftool(filename, string.Empty, revision1, null);
        }

        public string OpenWithDifftool(string filename, string oldFileName, string revision1, string revision2)
        {
            return OpenWithDifftool(filename, oldFileName, revision1, revision2, string.Empty);
        }

        public string OpenWithDifftool(string filename, string oldFileName, string revision1, string revision2, string extraDiffArguments)
        {
            var output = "";
            if (!filename.IsNullOrEmpty())
                filename = filename.Quote();
            if (!oldFileName.IsNullOrEmpty())
                oldFileName = oldFileName.Quote();

            string args = string.Join(" ", extraDiffArguments, revision2.QuoteNE(), revision1.QuoteNE(), "--", filename, oldFileName);
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
            string revparseCommand = string.Format("rev-parse \"{0}~0\"", revisionExpression);
            int exitCode = 0;
            string[] resultStrings = RunCmd(Settings.GitCommand, revparseCommand, out exitCode).Split('\n');
            return exitCode == 0 ? resultStrings[0] : "";
        }

        public string GetMergeBase(string a, string b)
        {
            return RunGitCmd("merge-base " + a + " " + b).TrimEnd();
        }

        public static string WorkingDirGitDir(string repositoryPath)
        {
            if (string.IsNullOrEmpty(repositoryPath))
                return repositoryPath;
            var candidatePath = GetGitDirectory(repositoryPath);
            return Directory.Exists(candidatePath) ? candidatePath : repositoryPath;
        }

        /// <summary>
        /// Uses check-ref-format to ensure that a branch name is well formed.
        /// </summary>
        /// <param name="branchName">Branch name to test.</param>
        /// <returns>true if <see cref="branchName"/> is valid reference name, otherwise false.</returns>
        public bool CheckBranchFormat([NotNull] string branchName)
        {
            if (branchName == null)
                throw new ArgumentNullException("branchName");

            if (branchName.IsNullOrWhiteSpace())
                return false;

            branchName = branchName.Replace("\"", "\\\"");

            int exitCode;
            RunCmd(Settings.GitCommand, string.Format("check-ref-format --branch \"{0}\"", branchName), out exitCode);
            return exitCode == 0;
        }

        public bool IsLockedIndex()
        {
            return IsLockedIndex(_workingdir);
        }

        public static bool IsLockedIndex(string repositoryPath)
        {
            var gitDir = WorkingDirGitDir(repositoryPath);
            var indexLockFile = Path.Combine(gitDir, "index.lock");

            if (File.Exists(indexLockFile))
            {
                return true;
            }

            return false;
        }

        public bool IsRunningGitProcess()
        {
            if (IsLockedIndex())
            {
                return true;
            }

            // Get processes by "ps" command.
            var cmd = Path.Combine(Settings.GitBinDir, "ps");
            var arguments = "x";
            if (Settings.RunningOnWindows())
            {
                // "x" option is unimplemented by msysgit and cygwin.
                arguments = "";
            }

            var output = RunCmd(cmd, arguments);
            var lines = output.Split('\n');
            var headers = lines[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var commandIndex = Array.IndexOf(headers, "COMMAND");
            for (int i = 1; i < lines.Count(); i++)
            {
                var columns = lines[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (commandIndex < columns.Count())
                {
                    var command = columns[commandIndex];
                    if (command.EndsWith("/git"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static string ReEncodeFileName(string diffStr, int headerLines)
        {
            StringReader r = new StringReader(diffStr);
            StringWriter w = new StringWriter();
            string line;
            while (headerLines > 0 && (line = r.ReadLine()) != null)
            {
                headerLines--;
                line = ReEncodeFileNameFromLossless(line);
                w.WriteLine(line);
            }
            w.Write(r.ReadToEnd());

            return w.ToString();
        }

        public static string UnquoteFileName(string fileName)
        {
            char[] chars = fileName.ToCharArray();
            List<byte> blist = new List<byte>();
            int i = 0;
            StringBuilder sb = new StringBuilder();
            while (i < chars.Length)
            {
                char c = chars[i];
                if (c == '\\')
                {
                    //there should be 3 digits
                    if (chars.Length >= i + 3)
                    {
                        string octNumber = "" + chars[i + 1] + chars[i + 2] + chars[i + 3];

                        try
                        {
                            int code = System.Convert.ToInt32(octNumber, 8);
                            blist.Add((byte)code);
                            i += 4;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                else
                {
                    if (blist.Count > 0)
                    {
                        sb.Append(GitModule.SystemEncoding.GetString(blist.ToArray()));
                        blist.Clear();
                    }

                    sb.Append(c);
                    i++;
                }
            }
            if (blist.Count > 0)
            {
                sb.Append(GitModule.SystemEncoding.GetString(blist.ToArray()));
                blist.Clear();
            }
            return sb.ToString();
        }

        public static string ReEncodeFileNameFromLossless(string fileName)
        {
            fileName = ReEncodeStringFromLossless(fileName, GitModule.SystemEncoding);
            return UnquoteFileName(fileName);
        }

        public static string ReEncodeString(string s, Encoding fromEncoding, Encoding toEncoding)
        {
            if (s == null || fromEncoding.HeaderName.Equals(toEncoding.HeaderName))
                return s;
            else
            {
                byte[] bytes = fromEncoding.GetBytes(s);
                s = toEncoding.GetString(bytes);
                return s;
            }
        }

        /// <summary>
        /// reencodes string from GitCommandHelpers.LosslessEncoding to toEncoding
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ReEncodeStringFromLossless(string s, Encoding toEncoding)
        {
            if (toEncoding == null)
                return s;
            else
                return ReEncodeString(s, LosslessEncoding, toEncoding);

        }

        public string ReEncodeStringFromLossless(string s)
        {
            return ReEncodeStringFromLossless(s, LogOutputEncoding);
        }

        //there is a bug: git does not recode commit message when format is given
        //Lossless encoding is used, because LogOutputEncoding might not be lossless and not recoded
        //characters could be replaced by replacement character while reencoding to LogOutputEncoding
        public string ReEncodeCommitMessage(string s, string toEncodingName)
        {

            bool isABug = true;

            Encoding encoding;
            try
            {
                if (isABug)
                {
                    if (toEncodingName.IsNullOrEmpty())
                        encoding = Encoding.UTF8;
                    else if (toEncodingName.Equals(LosslessEncoding.HeaderName, StringComparison.InvariantCultureIgnoreCase))
                        encoding = null; //no recoding is needed
                    else
                        encoding = Encoding.GetEncoding(toEncodingName);
                }
                else//if bug will be fixed, git should recode commit message to LogOutputEncoding
                    encoding = LogOutputEncoding;

            }
            catch (Exception)
            {
                return "! Unsupported commit message encoding: " + toEncodingName + " !\n\n" + s;
            }
            return ReEncodeStringFromLossless(s, encoding);
        }

        /// <summary>
        /// header part of show result is encoded in logoutputencoding (including reencoded commit message)
        /// diff part is raw data in file's original encoding
        /// s should be encoded in LosslessEncoding
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public string ReEncodeShowString(string s)
        {
            if (s.IsNullOrEmpty())
                return s;

            int p = s.IndexOf("diff --git");
            string header;
            string diffHeader;
            string diffContent;
            string diff;
            if (p > 0)
            {
                header = s.Substring(0, p);
                diff = s.Substring(p);
            }
            else
            {
                header = string.Empty;
                diff = s;
            }

            p = diff.IndexOf("@@");
            if (p > 0)
            {
                diffHeader = diff.Substring(0, p);
                diffContent = diff.Substring(p);
            }
            else
            {
                diffHeader = string.Empty;
                diffContent = diff;
            }

            header = ReEncodeString(header, GitModule.LosslessEncoding, LogOutputEncoding);
            diffHeader = ReEncodeFileNameFromLossless(diffHeader);
            diffContent = ReEncodeString(diffContent, GitModule.LosslessEncoding, FilesEncoding);
            return header + diffHeader + diffContent;
        }

        #region IGitCommands

        public string GitWorkingDir
        {
            get
            {
                return WorkingDir;
            }
        }

        public bool IsValidGitWorkingDir(string workingDir)
        {
            return ValidWorkingDir(workingDir);
        }

        public string GitCommand
        {
            get
            {
                return Settings.GitCommand;
            }
        }

        public string GitVersion
        {
            get
            {
                return Settings.GitExtensionsVersionInt.ToString();
            }
        }

        public string GravatarCacheDir
        {
            get
            {
                return Settings.GravatarCachePath;
            }
        }

        #endregion
    }
}
