using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.Settings;
using GitCommands.Utils;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using PatchApply;
using SmartFormat;

namespace GitCommands
{
    public delegate void GitModuleChangedEventHandler(GitModule module);

    public enum SubmoduleStatus
    {
        Unknown,
        NewSubmodule,
        FastForward,
        Rewind,
        NewerTime,
        OlderTime,
        SameTime
    }

    /// <summary>Provides manipulation with git module. 
    /// <remarks>Several instances may be created for submodules.</remarks></summary>
    [DebuggerDisplay("GitModule ( {_workingdir} )")]
    public sealed class GitModule : IGitModule
    {
        private static readonly Regex DefaultHeadPattern = new Regex("refs/remotes/[^/]+/HEAD", RegexOptions.Compiled);
        private readonly object _lock = new object();

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
                _workingdir = PathUtil.EnsureTrailingPathSeparator(value);
            }
        }

        public string FullPath(string localPath)
        {
            return Path.Combine(WorkingDir, localPath);
        }

        private bool _superprojectInit;
        private GitModule _superprojectModule;
        private string _submoduleName;
        private string _submodulePath;

        public string SubmoduleName
        {
            get
            {
                InitSuperproject();
                return _submoduleName;
            }
        }

        public string SubmodulePath
        {
            get
            {
                InitSuperproject();
                return _submodulePath;
            }
        }

        public GitModule SuperprojectModule
        {
            get
            {
                InitSuperproject();
                return _superprojectModule;
            }
        }

        private void InitSuperproject()
        {
            if (!_superprojectInit)
            {
                string superprojectDir = FindGitSuperprojectPath(out _submoduleName, out _submodulePath);
                _superprojectModule = superprojectDir == null ? null : new GitModule(superprojectDir);
                _superprojectInit = true;
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

        private RepoDistSettings _Settings;
        public RepoDistSettings Settings
        {
            get
            {
                lock (_lock)
                {
                    if (_Settings == null)
                        _Settings = RepoDistSettings.CreateEffective(this);
                }

                return _Settings;
            }
        }

        private ConfigFileSettings _EffectiveConfigFile;
        public ConfigFileSettings EffectiveConfigFile
        {
            get
            {
                lock (_lock)
                {
                    if (_EffectiveConfigFile == null)
                        _EffectiveConfigFile = ConfigFileSettings.CreateEffective(this);
                }

                return _EffectiveConfigFile;
            }
        }

        public ConfigFileSettings GlobalConfigFile
        {
            get
            {
                return EffectiveConfigFile.LowerPriority;
            }
        }

        public ConfigFileSettings LocalConfigFile
        {
            get
            {
                return new ConfigFileSettings(null, EffectiveConfigFile.SettingsCache);
            }
        }

        //encoding for files paths
        private static Encoding _systemEncoding;
        public static Encoding SystemEncoding
        {
            get
            {
                if (_systemEncoding == null)
                {
                    //check whether GitExtensions works with standard msysgit or msysgit-unicode

                    // invoke a git command that returns an invalid argument in its response, and
                    // check if a unicode-only character is reported back. If so assume msysgit-unicode

                    // git config --get with a malformed key (no section) returns:
                    // "error: key does not contain a section: <key>"
                    const string controlStr = "ą"; // "a caudata"
                    string arguments = string.Format("config --get {0}", controlStr);

                    int exitCode;

                    String s = new GitModule("").RunGitCmd(arguments, out exitCode, Encoding.UTF8);
                    if (s != null && s.IndexOf(controlStr) != -1)
                        _systemEncoding = new UTF8Encoding(false);
                    else
                        _systemEncoding = Encoding.Default;

                    Debug.WriteLine("System encoding: " + _systemEncoding.EncodingName);
                }

                return _systemEncoding;
            }
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

        public Encoding FilesEncoding
        {
            get
            {
                Encoding result = EffectiveConfigFile.FilesEncoding;
                if (result == null)
                    result = new UTF8Encoding(false);
                return result;
            }
        }

        public Encoding CommitEncoding
        {
            get
            {
                Encoding result = EffectiveConfigFile.CommitEncoding;
                if (result == null)
                    result = new UTF8Encoding(false);
                return result;
            }
        }

        /// <summary>
        /// Encoding for commit header (message, notes, author, commiter, emails)
        /// </summary>
        public Encoding LogOutputEncoding
        {
            get
            {
                Encoding result = EffectiveConfigFile.LogOutputEncoding;
                if (result == null)
                    result = CommitEncoding;
                return result;
            }
        }

        /// <summary>"(no branch)"</summary>
        public static readonly string DetachedBranch = "(no branch)";

        private static readonly string[] DetachedPrefixes = { "(no branch", "(detached from " };

        public AppSettings.PullAction LastPullAction
        {
            get { return AppSettings.GetEnum("LastPullAction_" + WorkingDir, AppSettings.PullAction.None); }
            set { AppSettings.SetEnum("LastPullAction_" + WorkingDir, value); }
        }

        public void LastPullActionToFormPullAction()
        {
            if (LastPullAction == AppSettings.PullAction.FetchAll)
                AppSettings.FormPullAction = AppSettings.PullAction.Fetch;
            else if (LastPullAction != AppSettings.PullAction.None)
                AppSettings.FormPullAction = LastPullAction;
        }

        /// <summary>Trims whitespace and replaces '\' with '/'.</summary>
        static string FixPath(string path)
        {
            return GitCommandHelpers.FixPath(path);
        }

        /// <summary>Indicates whether the <see cref="WorkingDir"/> contains a git repository.</summary>
        public bool IsValidGitWorkingDir()
        {
            return IsValidGitWorkingDir(_workingdir);
        }

        /// <summary>Indicates whether the specified directory contains a git repository.</summary>
        public static bool IsValidGitWorkingDir(string dir)
        {
            if (string.IsNullOrEmpty(dir))
                return false;

            string dirPath = dir + AppSettings.PathSeparator;
            string path = dirPath + ".git";

            if (Directory.Exists(path) || File.Exists(path))
                return true;

            return Directory.Exists(dirPath + "info") &&
                   Directory.Exists(dirPath + "objects") &&
                   Directory.Exists(dirPath + "refs");
        }

        /// <summary>Gets the ".git" directory path.</summary>
        public string GetGitDirectory()
        {
            return GetGitDirectory(_workingdir);
        }

        public static string GetGitDirectory(string repositoryPath)
        {
            if (File.Exists(repositoryPath + ".git"))
            {
                var lines = File.ReadLines(repositoryPath + ".git");
                foreach (string line in lines)
                {
                    if (line.StartsWith("gitdir:"))
                    {
                        string path = line.Substring(7).Trim().Replace('/', '\\');
                        if (Path.IsPathRooted(path))
                            return path + AppSettings.PathSeparator.ToString();
                        else
                            return Path.GetFullPath(Path.Combine(repositoryPath, path + AppSettings.PathSeparator.ToString()));
                    }
                }
            }
            return repositoryPath + ".git" + AppSettings.PathSeparator.ToString();
        }

        public bool IsBareRepository()
        {
            return IsBareRepository(_workingdir);
        }

        public static bool IsBareRepository(string repositoryPath)
        {
            return !Directory.Exists(GetGitDirectory(repositoryPath));
        }

        public string WorkingDirGitDir()
        {
            return WorkingDirGitDir(_workingdir);
        }

        public static string WorkingDirGitDir(string repositoryPath)
        {
            if (string.IsNullOrEmpty(repositoryPath))
                return repositoryPath;
            var candidatePath = GetGitDirectory(repositoryPath);
            return Directory.Exists(candidatePath) ? candidatePath : repositoryPath;
        }

        /// <summary>
        /// This is a faster function to get the names of all submodules then the 
        /// GetSubmodules() function. The command @git submodule is very slow.
        /// </summary>
        public IList<string> GetSubmodulesLocalPathes(bool recursive = true)
        {
            var configFile = GetSubmoduleConfigFile();
            var submodules = configFile.ConfigSections.Select(configSection => configSection.GetPathValue("path").Trim()).ToList();
            if (recursive)
            {
                for (int i = 0; i < submodules.Count; i++)
                {
                    var submodule = GetSubmodule(submodules[i]);
                    var submoduleConfigFile = submodule.GetSubmoduleConfigFile();
                    var subsubmodules = submoduleConfigFile.ConfigSections.Select(configSection => configSection.GetPathValue("path").Trim()).ToList();
                    for (int j = 0; j < subsubmodules.Count; j++)
                        subsubmodules[j] = submodules[i] + '/' + subsubmodules[j];
                    submodules.InsertRange(i + 1, subsubmodules);
                    i += subsubmodules.Count;
                }
            }
            return submodules;
        }

        public static string FindGitWorkingDir(string startDir)
        {
            if (string.IsNullOrEmpty(startDir))
                return "";

            startDir = startDir.Trim();

            var pathSeparators = new[] { AppSettings.PathSeparator, AppSettings.PathSeparatorWrong };
            var len = startDir.Length;

            while (len > 0 && pathSeparators.Any(s => s == startDir[len - 1]))
                len--;

            startDir = startDir.Substring(0, len) + AppSettings.PathSeparator;

            var dir = startDir;

            while (dir.LastIndexOfAny(pathSeparators) > 0)
            {
                dir = dir.Substring(0, dir.LastIndexOfAny(pathSeparators));

                if (IsValidGitWorkingDir(dir))
                    return dir + AppSettings.PathSeparator;
            }
            return startDir;
        }

        private static Process StartProccess(string fileName, string arguments, string workingDir, bool showConsole)
        {
            GitCommandHelpers.SetEnvironmentVariable();

            string quotedCmd = fileName;
            if (quotedCmd.IndexOf(' ') != -1)
                quotedCmd = quotedCmd.Quote();
            AppSettings.GitLog.Log(quotedCmd + " " + arguments);

            var startInfo = new ProcessStartInfo
                                {
                                    FileName = fileName,
                                    Arguments = arguments,
                                    WorkingDirectory = workingDir
                                };
            if (!showConsole)
            {
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
            }
            return Process.Start(startInfo);
        }

        /// <summary>
        /// Run command, console window is visible
        /// </summary>
        public Process RunExternalCmdDetachedShowConsole(string cmd, string arguments)
        {
            try
            {
                return StartProccess(cmd, arguments, _workingdir, showConsole: true);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Run command, console window is visible, wait for exit
        /// </summary>
        public void RunExternalCmdShowConsole(string cmd, string arguments)
        {
            try
            {
                using (var process = StartProccess(cmd, arguments, _workingdir, showConsole: true))
                    process.WaitForExit();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Run command, console window is hidden
        /// </summary>
        public static Process RunExternalCmdDetached(string fileName, string arguments, string workingDir)
        {
            try
            {
                return StartProccess(fileName, arguments, workingDir, showConsole: false);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Run command, console window is hidden
        /// </summary>
        public Process RunExternalCmdDetached(string cmd, string arguments)
        {
            return RunExternalCmdDetached(cmd, arguments, _workingdir);
        }

        /// <summary>
        /// Run git command, console window is hidden, redirect output
        /// </summary>
        public Process RunGitCmdDetached(string arguments, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = SystemEncoding;

            return GitCommandHelpers.StartProcess(AppSettings.GitCommand, arguments, _workingdir, encoding);
        }

        /// <summary>
        /// Run command, cache results, console window is hidden, wait for exit, redirect output
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public string RunCacheableCmd(string cmd, string arguments = "", Encoding encoding = null)
        {
            if (encoding == null)
                encoding = SystemEncoding;

            byte[] cmdout, cmderr;
            if (GitCommandCache.TryGet(arguments, out cmdout, out cmderr))
                return EncodingHelper.DecodeString(cmdout, cmderr, ref encoding);

            GitCommandHelpers.RunCmdByte(cmd, arguments, _workingdir, null, out cmdout, out cmderr);

            GitCommandCache.Add(arguments, cmdout, cmderr);

            return EncodingHelper.DecodeString(cmdout, cmderr, ref encoding);
        }

        /// <summary>
        /// Run command, console window is hidden, wait for exit, redirect output
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public string RunCmd(string cmd, string arguments, Encoding encoding = null, byte[] stdInput = null)
        {
            int exitCode;
            return RunCmd(cmd, arguments, out exitCode, encoding, stdInput);
        }

        /// <summary>
        /// Run command, console window is hidden, wait for exit, redirect output
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public string RunCmd(string cmd, string arguments, out int exitCode, Encoding encoding = null, byte[] stdInput = null)
        {
            byte[] output, error;
            exitCode = GitCommandHelpers.RunCmdByte(cmd, arguments, _workingdir, stdInput, out output, out error);
            if (encoding == null)
                encoding = SystemEncoding;
            return EncodingHelper.GetString(output, error, encoding);
        }

        /// <summary>
        /// Run git command, console window is hidden, wait for exit, redirect output
        /// </summary>
        public string RunGitCmd(string arguments, out int exitCode, Encoding encoding = null, byte[] stdInput = null)
        {
            return RunCmd(AppSettings.GitCommand, arguments, out exitCode, encoding, stdInput);
        }

        /// <summary>
        /// Run git command, console window is hidden, wait for exit, redirect output
        /// </summary>
        public string RunGitCmd(string arguments, Encoding encoding = null, byte[] stdInput = null)
        {
            int exitCode;
            return RunCmd(AppSettings.GitCommand, arguments, out exitCode, encoding, stdInput);
        }

        /// <summary>Runs a 'git' command with the specified args.</summary>
        public GitCommandResult GitCmd(string args)
        {
            int exitCode;
            string output = RunGitCmd(args, out exitCode);
            return new GitCommandResult(output, exitCode == 0);
        }

        /// <summary>
        /// Run command, console window is hidden, wait for exit, redirect output
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private IEnumerable<string> ReadCmdOutputLines(string cmd, string arguments, string stdInput)
        {
            return GitCommandHelpers.ReadCmdOutputLines(cmd, arguments, _workingdir, stdInput);
        }

        /// <summary>
        /// Run git command, console window is hidden, wait for exit, redirect output
        /// </summary>
        public IEnumerable<string> ReadGitOutputLines(string arguments)
        {
            return ReadCmdOutputLines(AppSettings.GitCommand, arguments, null);
        }

        /// <summary>
        /// Run batch file, console window is hidden, wait for exit, redirect output
        /// </summary>
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
                RunExternalCmdShowConsole(AppSettings.GitCommand, "notes edit " + revision);
            }
        }

        public bool InTheMiddleOfConflictedMerge()
        {
            return !string.IsNullOrEmpty(RunGitCmd("ls-files -z --unmerged"));
        }

        public IList<GitItem> GetConflictedFiles()
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

        public bool HandleConflictSelectSide(string fileName, string side)
        {
            Directory.SetCurrentDirectory(_workingdir);
            fileName = FixPath(fileName);

            side = GetSide(side);

            string result = RunGitCmd(String.Format("checkout-index -f --stage={0} -- \"{1}\"", side, fileName));
            if (!result.IsNullOrEmpty())
            {
                return false;
            }

            result = RunGitCmd(String.Format("add -- \"{0}\"", fileName));
            return result.IsNullOrEmpty();
        }

        public bool HandleConflictsSaveSide(string fileName, string saveAsFileName, string side)
        {
            Directory.SetCurrentDirectory(_workingdir);
            fileName = FixPath(fileName);

            side = GetSide(side);

            var result = RunGitCmd(String.Format("checkout-index --stage={0} --temp -- \"{1}\"", side, fileName));
            if (result.IsNullOrEmpty())
            {
                return false;
            }

            if (!result.StartsWith(".merge_file_"))
            {
                return false;
            }

            // Parse temporary file name from command line result
            var splitResult = result.Split(new[] { "\t", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            if (splitResult.Length != 2)
            {
                return false;
            }

            var temporaryFileName = splitResult[0].Trim();

            if (!File.Exists(temporaryFileName))
            {
                return false;
            }

            var retValue = false;
            try
            {
                if (File.Exists(saveAsFileName))
                {
                    File.Delete(saveAsFileName);
                }
                File.Move(temporaryFileName, saveAsFileName);
                retValue = true;
            }
            catch
            {
            }
            finally
            {
                if (File.Exists(temporaryFileName))
                {
                    File.Delete(temporaryFileName);
                }
            }

            return retValue;
        }

        public void SaveBlobAs(string saveAs, string blob)
        {
            using (var ms = (MemoryStream)GetFileStream(blob)) //Ugly, has implementation info.
            {
                byte[] buf = ms.ToArray();
                if (EffectiveConfigFile.core.autocrlf.Value == AutoCRLFType.True)
                {
                    if (!FileHelper.IsBinaryFile(this, saveAs) && !FileHelper.IsBinaryFileAccordingToContent(buf))
                    {
                        buf = GitConvert.ConvertCrLfToWorktree(buf);
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

            var unmerged = RunGitCmd("ls-files -z --unmerged \"" + filename + "\"").Split(new[] { '\0', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in unmerged)
            {
                int findSecondWhitespace = line.IndexOfAny(new[] { ' ', '\t' });
                string fileStage = findSecondWhitespace >= 0 ? line.Substring(findSecondWhitespace).Trim() : "";

                findSecondWhitespace = fileStage.IndexOfAny(new[] { ' ', '\t' });

                fileStage = findSecondWhitespace >= 0 ? fileStage.Substring(findSecondWhitespace).Trim() : "";

                int stage;
                if (fileStage.Length > 2 && Int32.TryParse(fileStage[0].ToString(), out stage) && stage >= 1 && stage <= 3)
                {
                    fileNames[stage - 1] = fileStage.Substring(2);
                }
            }

            return fileNames;
        }

        public string[] GetConflictedSubmoduleHashes(string filename)
        {
            filename = FixPath(filename);

            var hashes = new string[3];

            var unmerged = RunGitCmd("ls-files -z --unmerged \"" + filename + "\"").Split(new[] { '\0', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in unmerged)
            {
                int findSecondWhitespace = line.IndexOfAny(new[] { ' ', '\t' });
                string fileStage = findSecondWhitespace >= 0 ? line.Substring(findSecondWhitespace).Trim() : "";

                findSecondWhitespace = fileStage.IndexOfAny(new[] { ' ', '\t' });

                string hash = findSecondWhitespace >= 0 ? fileStage.Substring(0, findSecondWhitespace).Trim() : "";
                fileStage = findSecondWhitespace >= 0 ? fileStage.Substring(findSecondWhitespace).Trim() : "";

                int stage;
                if (fileStage.Length > 2 && Int32.TryParse(fileStage[0].ToString(), out stage) && stage >= 1 && stage <= 3)
                {
                    hashes[stage - 1] = hash;
                }
            }

            return hashes;
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
            if (EnvUtils.RunningOnUnix())
            {
                RunExternalCmdDetachedShowConsole("gitk", "");
            }
            else
            {
                RunExternalCmdDetached("cmd.exe", "/c \"\"" + AppSettings.GitCommand.Replace("git.cmd", "gitk.cmd")
                                                              .Replace("bin\\git.exe", "cmd\\gitk.cmd")
                                                              .Replace("bin/git.exe", "cmd/gitk.cmd") + "\" --branches --tags --remotes\"");
            }
        }

        public void RunGui()
        {
            if (EnvUtils.RunningOnUnix())
            {
                RunExternalCmdDetachedShowConsole(AppSettings.GitCommand, "gui");
            }
            else
            {
                RunExternalCmdDetached("cmd.exe", "/c \"\"" + AppSettings.GitCommand + "\" gui\"");
            }
        }

        /// <summary>Runs a bash or shell command.</summary>
        public Process RunBash(string bashCommand = null)
        {
            if (EnvUtils.RunningOnUnix())
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

                return RunExternalCmdDetachedShowConsole(cmd, args);
            }
            else
            {
                string args;
                if (string.IsNullOrWhiteSpace(bashCommand))
                {
                    args = "--login -i\"";
                }
                else
                {
                    args = "--login -i -c \"" + bashCommand.Replace("\"", "\\\"") + "\"";
                }

                string termCmd = File.Exists(AppSettings.GitBinDir + "bash.exe") ? "bash" : "sh";
                return RunExternalCmdDetachedShowConsole("cmd.exe",
                    string.Format("/c \"\"{0}{1}\" {2}", AppSettings.GitBinDir, termCmd, args));
            }
        }

        public string Init(bool bare, bool shared)
        {
            return RunGitCmd(Smart.Format("init{0: --bare|}{1: --shared=all|}", bare, shared));
        }

        public bool IsMerge(string commit)
        {
            string[] parents = GetParents(commit);
            return parents.Length > 1;
        }

        private static string ProccessDiffNotes(int startIndex, string[] lines)
        {
            int endIndex = lines.Length - 1;
            if (lines[endIndex] == "Notes:")
                endIndex--;

            var message = new StringBuilder();
            bool bNotesStart = false;
            for (int i = startIndex; i <= endIndex; i++)
            {
                string line = lines[i];
                if (bNotesStart)
                    line = "    " + line;
                message.AppendLine(line);
                if (lines[i] == "Notes:")
                    bNotesStart = true;
            }

            return message.ToString();
        }

        public GitRevision GetRevision(string commit, bool shortFormat = false)
        {
            const string formatString =
                /* Hash           */ "%H%n" +
                /* Tree           */ "%T%n" +
                /* Parents        */ "%P%n" +
                /* Author Name    */ "%aN%n" +
                /* Author EMail   */ "%aE%n" +
                /* Author Date    */ "%at%n" +
                /* Committer Name */ "%cN%n" +
                /* Committer EMail*/ "%cE%n" +
                /* Committer Date */ "%ct%n";
            const string messageFormat = "%e%n%B%nNotes:%n%-N";
            string cmd = "log -n1 --format=format:" + formatString + (shortFormat ? "%e%n%s" : messageFormat) + " " + commit;
            var revInfo = RunCacheableCmd(AppSettings.GitCommand, cmd, LosslessEncoding);
            string[] lines = revInfo.Split('\n');
            var revision = new GitRevision(this, lines[0])
            {
                TreeGuid = lines[1],
                ParentGuids = lines[2].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries),
                Author = ReEncodeStringFromLossless(lines[3]),
                AuthorEmail = ReEncodeStringFromLossless(lines[4]),
                Committer = ReEncodeStringFromLossless(lines[6]),
                CommitterEmail = ReEncodeStringFromLossless(lines[7])
            };
            revision.AuthorDate = DateTimeUtils.ParseUnixTime(lines[5]);
            revision.CommitDate = DateTimeUtils.ParseUnixTime(lines[8]);
            revision.MessageEncoding = lines[9];
            if (shortFormat)
            {
                revision.Message = ReEncodeCommitMessage(lines[10], revision.MessageEncoding);
            }
            else
            {
                string message = ProccessDiffNotes(10, lines);

                //commit message is not reencoded by git when format is given
                revision.Body = ReEncodeCommitMessage(message, revision.MessageEncoding);
                revision.Message = revision.Body.Substring(0, revision.Body.IndexOfAny(new[] { '\r', '\n' }));
            }

            return revision;
        }

        public string[] GetParents(string commit)
        {
            string output = RunGitCmd("log -n 1 --format=format:%P \"" + commit + "\"");
            return output.Split(' ');
        }

        public GitRevision[] GetParentsRevisions(string commit)
        {
            string[] parents = GetParents(commit);
            var parentsRevisions = new GitRevision[parents.Length];
            for (int i = 0; i < parents.Length; i++)
                parentsRevisions[i] = GetRevision(parents[i], true);
            return parentsRevisions;
        }

        public string CherryPick(string cherry, bool commit, string arguments)
        {
            return RunGitCmd(GitCommandHelpers.CherryPickCmd(cherry, commit, arguments));
        }

        public string ShowSha1(string sha1)
        {
            return ReEncodeShowString(RunCacheableCmd(AppSettings.GitCommand, "show " + sha1, LosslessEncoding));
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
            return RunGitCmd("rev-parse HEAD").TrimEnd();
        }

        public KeyValuePair<char, string> GetSuperprojectCurrentCheckout()
        {
            if (SuperprojectModule == null)
                return new KeyValuePair<char, string>(' ', "");

            var lines = SuperprojectModule.RunGitCmd("submodule status --cached " + _submodulePath).Split('\n');

            if (lines.Length == 0)
                return new KeyValuePair<char, string>(' ', "");

            string submodule = lines[0];
            if (submodule.Length < 43)
                return new KeyValuePair<char, string>(' ', "");

            var currentCommitGuid = submodule.Substring(1, 40).Trim();
            return new KeyValuePair<char, string>(submodule[0], currentCommitGuid);
        }

        public int CommitCount()
        {
            int count;
            var arguments = "/c \"\"" + AppSettings.GitCommand + "\" rev-list --all --abbrev-commit | wc -l\"";
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
            submodulePath = submodulePath.Replace(AppSettings.PathSeparator, AppSettings.PathSeparatorWrong).TrimEnd(
                    AppSettings.PathSeparatorWrong);
            return submodulePath;
        }

        public string GetSubmoduleNameByPath(string localPath)
        {
            var configFile = GetSubmoduleConfigFile();
            var submodule = configFile.ConfigSections.FirstOrDefault(configSection => configSection.GetPathValue("path").Trim() == localPath);
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
            string dir = Path.Combine(_workingdir, localPath + AppSettings.PathSeparator);
            return Path.GetFullPath(dir); // fix slashes
        }

        public GitModule GetSubmodule(string localPath)
        {
            return new GitModule(GetSubmoduleFullPath(localPath));
        }

        IGitModule IGitModule.GetSubmodule(string submoduleName)
        {
            return GetSubmodule(submoduleName);
        }

        private GitSubmoduleInfo GetSubmoduleInfo(string submodule)
        {
            var gitSubmodule =
                new GitSubmoduleInfo(this)
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

        public IEnumerable<IGitSubmoduleInfo> GetSubmodulesInfo()
        {
            var submodules = ReadGitOutputLines("submodule status");

            string lastLine = null;

            foreach (var submodule in submodules)
            {
                if (submodule.Length < 43)
                    continue;

                if (submodule.Equals(lastLine))
                    continue;

                lastLine = submodule;

                yield return GetSubmoduleInfo(submodule);
            }
        }

        public string FindGitSuperprojectPath(out string submoduleName, out string submodulePath)
        {
            submoduleName = null;
            submodulePath = null;
            if (!IsValidGitWorkingDir())
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
                        if (File.Exists(path + AppSettings.PathSeparator.ToString() + ".gitmodules") &&
                            IsValidGitWorkingDir(path + AppSettings.PathSeparator.ToString()))
                        {
                            superprojectPath = path + AppSettings.PathSeparator.ToString();
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
                var lines = File.ReadLines(_workingdir + ".git");
                foreach (string line in lines)
                {
                    if (line.StartsWith("gitdir:"))
                    {
                        string gitpath = line.Substring(7).Trim();
                        int pos = gitpath.IndexOf("/.git/");
                        if (pos != -1)
                        {
                            gitpath = gitpath.Substring(0, pos + 1).Replace('/', '\\');
                            gitpath = Path.GetFullPath(Path.Combine(_workingdir, gitpath));
                            if (File.Exists(gitpath + ".gitmodules") && IsValidGitWorkingDir(gitpath))
                                superprojectPath = gitpath;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(superprojectPath))
            {
                submodulePath = FixPath(currentPath.Substring(superprojectPath.Length));
                var configFile = GetSubmoduleConfigFile();
                foreach (ConfigSection configSection in configFile.ConfigSections)
                {
                    if (configSection.GetPathValue("path") == FixPath(submodulePath))
                    {
                        submoduleName = configSection.SubSection;
                        return superprojectPath;
                    }
                }
            }

            return null;
        }

        public string GetSubmoduleSummary(string submodule)
        {
            var arguments = string.Format("submodule summary {0}", submodule);
            return RunGitCmd(arguments);
        }

        public string Stash()
        {
            var arguments = GitCommandHelpers.StashSaveCmd(AppSettings.IncludeUntrackedFilesInAutoStash);
            return RunGitCmd(arguments);
        }

        public string StashApply(string stash = null)
        {
            return RunGitCmd(string.Format("stash apply {0}", stash));
        }

        /// <summary>Remove all the stashed states.</summary>
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

            var result = RunGitCmd("format-patch -M -C -B --start-number " + start + " \"" + from + "\"..\"" + to +
                                "\" -o \"" + output + "\"");

            return result;
        }

        public string FormatPatch(string from, string to, string output)
        {
            output = FixPath(output);

            var result = RunGitCmd("format-patch -M -C -B \"" + from + "\"..\"" + to + "\" -o \"" + output + "\"");

            return result;
        }


        public string Tag(string tagName, string revision, bool annotation, bool force)
        {
            if (annotation)
                return RunGitCmd(string.Format("tag \"{0}\" -a {1} -F \"{2}\\TAGMESSAGE\" -- \"{3}\"", tagName.Trim(), (force ? "-f" : ""), WorkingDirGitDir(), revision));
            return RunGitCmd(string.Format("tag {0} \"{1}\" \"{2}\"", (force ? "-f" : ""), tagName.Trim(), revision));
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

        /// <summary>Run 'git push {remote}'.</summary>
        public string Push(string remote)
        {
            return RunGitCmd("push \"" + FixPath(remote).Trim() + "\"");
        }

        /// <summary>Run 'git push' using the specified push options.</summary>
        public string Push(GitPush push)
        {
            return RunGitCmd(push.ToString());
        }

        /// <summary>Tries to start Pageant for the specified remote repo (using the remote's PuTTY key file).</summary>
        /// <returns>true if the remote has a PuTTY key file; otherwise, false.</returns>
        public bool StartPageantForRemote(string remote)
        {
            var sshKeyFile = GetPuttyKeyFileForRemote(remote);
            if (string.IsNullOrEmpty(sshKeyFile) || !File.Exists(sshKeyFile))
                return false;

            StartPageantWithKey(sshKeyFile);
            return true;
        }

        public static void StartPageantWithKey(string sshKeyFile)
        {
            RunExternalCmdDetached(AppSettings.Pageant, "\"" + sshKeyFile + "\"", "");
        }

        public string GetPuttyKeyFileForRemote(string remote)
        {
            if (string.IsNullOrEmpty(remote) ||
                string.IsNullOrEmpty(AppSettings.Pageant) ||
                !AppSettings.AutoStartPageant ||
                !GitCommandHelpers.Plink())
                return "";

            return GetPathSetting(string.Format("remote.{0}.puttykeyfile", remote));
        }

        public static bool PathIsUrl(string path)
        {
            return path.Contains(AppSettings.PathSeparator.ToString()) || path.Contains(AppSettings.PathSeparatorWrong.ToString());
        }

        public string FetchCmd(string remote, string remoteBranch, string localBranch, bool? fetchTags = false)
        {
            var progressOption = "";
            if (GitCommandHelpers.VersionInUse.FetchCanAskForProgress)
                progressOption = "--progress ";

            if (string.IsNullOrEmpty(remote) && string.IsNullOrEmpty(remoteBranch) && string.IsNullOrEmpty(localBranch))
                return "fetch " + progressOption;

            return "fetch " + progressOption + GetFetchArgs(remote, remoteBranch, localBranch, fetchTags);
        }

        public string PullCmd(string remote, string remoteBranch, string localBranch, bool rebase, bool? fetchTags = false)
        {
            var pullArgs = "";
            if (GitCommandHelpers.VersionInUse.FetchCanAskForProgress)
                pullArgs = "--progress ";

            if (rebase)
                pullArgs = "--rebase".Combine(" ", pullArgs);

            return "pull " + pullArgs + GetFetchArgs(remote, remoteBranch, localBranch, fetchTags);
        }

        private string GetFetchArgs(string remote, string remoteBranch, string localBranch, bool? fetchTags)
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
            {
                if (remoteBranch.StartsWith("+"))
                    remoteBranch = remoteBranch.Remove(0, 1);
                remoteBranchArguments = "+" + GitCommandHelpers.GetFullBranchName(remoteBranch);
            }

            string localBranchArguments;
            var remoteUrl = GetPathSetting(string.Format(SettingKeyString.RemoteUrl, remote));

            if (PathIsUrl(remote) && !string.IsNullOrEmpty(localBranch) && string.IsNullOrEmpty(remoteUrl))
                localBranchArguments = ":" + GitCommandHelpers.GetFullBranchName(localBranch);
            else if (string.IsNullOrEmpty(localBranch) || PathIsUrl(remote) || string.IsNullOrEmpty(remoteUrl))
                localBranchArguments = "";
            else
                localBranchArguments = ":" + "refs/remotes/" + remote.Trim() + "/" + localBranch + "";

            string arguments = fetchTags == true ? "--tags" : fetchTags == false ? " --no-tags" : "";

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
            if (Directory.Exists(gitDirectory + "rebase-merge" + AppSettings.PathSeparator.ToString()))
                return gitDirectory + "rebase-merge" + AppSettings.PathSeparator.ToString();
            if (Directory.Exists(gitDirectory + "rebase-apply" + AppSettings.PathSeparator.ToString()))
                return gitDirectory + "rebase-apply" + AppSettings.PathSeparator.ToString();
            if (Directory.Exists(gitDirectory + "rebase" + AppSettings.PathSeparator.ToString()))
                return gitDirectory + "rebase" + AppSettings.PathSeparator.ToString();

            return "";
        }

        private ProcessStartInfo CreateGitStartInfo(string arguments)
        {
            return GitCommandHelpers.CreateProcessStartInfo(AppSettings.GitCommand, arguments, _workingdir, SystemEncoding);
        }

        public string ApplyPatch(string dir, string amCommand)
        {
            var startInfo = CreateGitStartInfo(amCommand);

            using (var process = Process.Start(startInfo))
            {
                var files = Directory.GetFiles(dir);

                if (files.Length == 0)
                    return "";

                foreach (var file in files)
                {
                    using (var fs = new FileStream(file, FileMode.Open))
                    {
                        fs.CopyTo(process.StandardInput.BaseStream);
                    }
                }
                process.StandardInput.Close();
                process.WaitForExit();

                return process.StandardOutput.ReadToEnd().Trim();
            }
        }

        public string StageFiles(IList<GitItemStatus> files, out bool wereErrors)
        {
            var output = "";
            wereErrors = false;
            var startInfo = CreateGitStartInfo("update-index --add --stdin");
            var process = new Lazy<Process>(() => Process.Start(startInfo));
            foreach (var file in files.Where(file => !file.IsDeleted))
            {
                UpdateIndex(process, file.Name);
            }
            if (process.IsValueCreated)
            {
                process.Value.StandardInput.Close();
                process.Value.WaitForExit();
                wereErrors = process.Value.ExitCode != 0;
                output = process.Value.StandardOutput.ReadToEnd().Trim();
            }

            startInfo.Arguments = "update-index --remove --stdin";
            process = new Lazy<Process>(() => Process.Start(startInfo));
            foreach (var file in files.Where(file => file.IsDeleted))
            {
                UpdateIndex(process, file.Name);
            }
            if (process.IsValueCreated)
            {
                process.Value.StandardInput.Close();
                process.Value.WaitForExit();
                wereErrors = wereErrors || process.Value.ExitCode != 0;

                if (!string.IsNullOrEmpty(output))
                    output += Environment.NewLine;
                output += process.Value.StandardOutput.ReadToEnd().Trim();
            }

            return output;
        }

        public string UnstageFiles(IList<GitItemStatus> files)
        {
            var output = "";
            var startInfo = CreateGitStartInfo("update-index --info-only --index-info");
            var process = new Lazy<Process>(() => Process.Start(startInfo));
            foreach (var file in files.Where(file => !file.IsNew))
            {
                process.Value.StandardInput.WriteLine("0 0000000000000000000000000000000000000000\t\"" + FixPath(file.Name) + "\"");
            }
            if (process.IsValueCreated)
            {
                process.Value.StandardInput.Close();
                process.Value.WaitForExit();
                output = process.Value.StandardOutput.ReadToEnd().Trim();
            }

            startInfo.Arguments = "update-index --force-remove --stdin";
            process = new Lazy<Process>(() => Process.Start(startInfo));
            foreach (var file in files.Where(file => file.IsNew))
            {
                UpdateIndex(process, file.Name);
            }
            if (process.IsValueCreated)
            {
                process.Value.StandardInput.Close();
                process.Value.WaitForExit();

                if (!string.IsNullOrEmpty(output))
                    output += Environment.NewLine;
                output += process.Value.StandardOutput.ReadToEnd().Trim();
            }

            return output;
        }

        private static void UpdateIndex(Lazy<Process> process, string filename)
        {
            //process.StandardInput.WriteLine("\"" + FixPath(file.Name) + "\"");
            byte[] bytearr = EncodingHelper.ConvertTo(SystemEncoding,
                                                      "\"" + FixPath(filename) + "\"" + process.Value.StandardInput.NewLine);
            process.Value.StandardInput.BaseStream.Write(bytearr, 0, bytearr.Length);
        }

        public bool InTheMiddleOfBisect()
        {
            return File.Exists(WorkingDirGitDir() + AppSettings.PathSeparator.ToString() + "BISECT_START");
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

        private static string AppendQuotedString(string str1, string str2)
        {
            var m1 = QuotedText.Match(str1);
            var m2 = QuotedText.Match(str2);
            if (!m1.Success || !m2.Success)
                return str1 + str2;
            Debug.Assert(m1.Groups[1].Value == m2.Groups[1].Value);
            return str1.Substring(0, str1.Length - 2) + m2.Groups[2].Value + "?=";
        }

        private static string DecodeString(string str)
        {
            // decode QuotedPrintable text using .NET internal decoder 
            Attachment attachment = Attachment.CreateAttachmentFromString("", str);
            return attachment.Name;
        }

        private static readonly Regex HeadersMatch = new Regex(@"^(?<header_key>[-A-Za-z0-9]+)(?::[ \t]*)(?<header_value>.*)$", RegexOptions.Compiled);
        private static readonly Regex QuotedText = new Regex(@"=\?([\w-]+)\?q\?(.*)\?=$", RegexOptions.Compiled);

        public bool InTheMiddleOfInteractiveRebase()
        {
            return File.Exists(GetRebaseDir() + "git-rebase-todo");
        }

        public IList<PatchFile> GetInteractiveRebasePatchFiles()
        {
            string todoFile = GetRebaseDir() + "git-rebase-todo";
            string[] todoCommits = File.Exists(todoFile) ? File.ReadAllText(todoFile).Trim().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries) : null;

            IList<PatchFile> patchFiles = new List<PatchFile>();

            if (todoCommits != null)
            {
                foreach (string todoCommit in todoCommits)
                {
                    if (todoCommit.StartsWith("#"))
                        continue;

                    string[] parts = todoCommit.Split(' ');

                    if (parts.Length >= 3)
                    {
                        string error = string.Empty;
                        CommitData data = CommitData.GetCommitData(this, parts[1], ref error);

                        PatchFile nextCommitPatch = new PatchFile();
                        nextCommitPatch.Author = string.IsNullOrEmpty(error) ? data.Author : error;
                        nextCommitPatch.Subject = string.IsNullOrEmpty(error) ? data.Body : error;
                        nextCommitPatch.Name = parts[0];
                        nextCommitPatch.Date = string.IsNullOrEmpty(error) ? data.CommitDate.LocalDateTime.ToString() : error;
                        nextCommitPatch.IsNext = patchFiles.Count == 0;

                        patchFiles.Add(nextCommitPatch);
                    }
                }
            }

            return patchFiles;
        }

        public IList<PatchFile> GetRebasePatchFiles()
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
                var file = fullFileName.Substring(fullFileName.LastIndexOf(AppSettings.PathSeparator.ToString()) + 1);
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
                    string key = null;
                    string value = null;
                    foreach (var line in File.ReadLines(GetRebaseDir() + file))
                    {
                        var m = HeadersMatch.Match(line);
                        if (key == null)
                        {
                            if (!String.IsNullOrWhiteSpace(line) && !m.Success)
                                continue;
                        }
                        else if (String.IsNullOrWhiteSpace(line) || m.Success)
                        {
                            value = DecodeString(value);
                            switch (key)
                            {
                                case "From":
                                    if (value.IndexOf('<') > 0 && value.IndexOf('<') < value.Length)
                                        patchFile.Author = value.Substring(0, value.IndexOf('<')).Trim();
                                    else
                                        patchFile.Author = value;
                                    break;
                                case "Date":
                                    if (value.IndexOf('+') > 0 && value.IndexOf('<') < value.Length)
                                        patchFile.Date = value.Substring(0, value.IndexOf('+')).Trim();
                                    else
                                        patchFile.Date = value;
                                    break;
                                case "Subject":
                                    patchFile.Subject = value;
                                    break;
                            }
                        }
                        if (m.Success)
                        {
                            key = m.Groups[1].Value;
                            value = m.Groups[2].Value;
                        }
                        else
                            value = AppendQuotedString(value, line.Trim());

                        if (string.IsNullOrEmpty(line) ||
                            !string.IsNullOrEmpty(patchFile.Author) &&
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

        public string CommitCmd(bool amend, bool signOff = false, string author = "", bool useExplicitCommitMessage = true)
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
                var path = WorkingDirGitDir() + AppSettings.PathSeparator.ToString() + "COMMITMESSAGE\"";
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

        public string[] GetRemotes(bool allowEmpty = true)
        {
            string remotes = RunGitCmd("remote show");
            return allowEmpty ? remotes.Split('\n') : remotes.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public IEnumerable<string> GetSettings(string setting)
        {
            return LocalConfigFile.GetValues(setting);
        }

        public string GetSetting(string setting)
        {
            return LocalConfigFile.GetValue(setting);
        }

        public string GetPathSetting(string setting)
        {
            return GetSetting(setting);
        }

        public string GetEffectiveSetting(string setting)
        {
            return EffectiveConfigFile.GetValue(setting);
        }

        public string GetEffectivePathSetting(string setting)
        {
            return GetEffectiveSetting(setting);
        }

        public void UnsetSetting(string setting)
        {
            SetSetting(setting, null);
        }

        public void SetSetting(string setting, string value)
        {
            LocalConfigFile.SetValue(setting, value);
        }

        public void SetPathSetting(string setting, string value)
        {
            LocalConfigFile.SetPathValue(setting, value);
        }

        public IList<Patch> GetStashedItems(string stashName)
        {
            var patchManager = new PatchManager();
            patchManager.LoadPatch(RunGitCmd("stash show -p " + stashName, LosslessEncoding), false, FilesEncoding);

            return patchManager.Patches;
        }

        public IList<GitStash> GetStashes()
        {
            var list = RunGitCmd("stash list").Split('\n');

            var stashes = new List<GitStash>();
            for (int i = 0; i < list.Length; i++)
            {
                string stashString = list[i];
                if (stashString.IndexOf(':') > 0)
                {
                    stashes.Add(new GitStash(stashString, i));
                }
            }

            return stashes;
        }

        public Patch GetSingleDiff(string @from, string to, string fileName, string oldFileName, string extraDiffArguments, Encoding encoding, bool cacheResult)
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

            if (AppSettings.UsePatienceDiffAlgorithm)
                extraDiffArguments = string.Concat(extraDiffArguments, " --patience");

            var patchManager = new PatchManager();
            var arguments = String.Format("diff {0} -M -C {1} -- {2} {3}", extraDiffArguments, commitRange, fileName, oldFileName);
            string patch;
            if (cacheResult)
                patch = RunCacheableCmd(AppSettings.GitCommand, arguments, LosslessEncoding);
            else
                patch = RunCmd(AppSettings.GitCommand, arguments, LosslessEncoding);
            patchManager.LoadPatch(patch, false, encoding);

            foreach (Patch p in patchManager.Patches)
                if (p.FileNameA.Equals(fileA) && p.FileNameB.Equals(fileB) ||
                    p.FileNameA.Equals(fileB) && p.FileNameB.Equals(fileA))
                    return p;

            return patchManager.Patches.Count > 0 ? patchManager.Patches[patchManager.Patches.Count - 1] : null;
        }

        public Patch GetSingleDiff(string @from, string to, string fileName, string extraDiffArguments, Encoding encoding, bool allowCache)
        {
            return GetSingleDiff(from, to, fileName, null, extraDiffArguments, encoding, allowCache);
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
            return noCache ? RunGitCmd(cmd) : this.RunCacheableCmd(AppSettings.GitCommand, cmd, SystemEncoding);
        }

        public List<GitItemStatus> GetDiffFilesWithSubmodulesStatus(string from, string to)
        {
            var status = GetDiffFiles(from, to);
            GetSubmoduleStatus(status, from, to);
            return status;
        }

        public List<GitItemStatus> GetDiffFiles(string from, string to, bool noCache = false)
        {
            string cmd = "diff -M -C -z --name-status \"" + to + "\" \"" + from + "\"";
            string result = noCache ? RunGitCmd(cmd) : this.RunCacheableCmd(AppSettings.GitCommand, cmd, SystemEncoding);
            return GitCommandHelpers.GetAllChangedFilesFromString(this, result, true);
        }

        public IList<GitItemStatus> GetStashDiffFiles(string stashName)
        {
            bool gitShowsUntrackedFiles = false;

            var resultCollection = GetDiffFiles(stashName, stashName + "^", true);

            if (!gitShowsUntrackedFiles)
            {
                string untrackedTreeHash = RunGitCmd("log " + stashName + "^3 --pretty=format:\"%T\" --max-count=1");
                if (GitRevision.Sha1HashRegex.IsMatch(untrackedTreeHash))
                {
                    var files = GetTreeFiles(untrackedTreeHash, true);
                    resultCollection.AddRange(files);
                }
            }

            return resultCollection;
        }

        public IEnumerable<GitItemStatus> GetUntrackedFiles()
        {
            var status = RunGitCmd("ls-files -z --others --directory --no-empty-directory --exclude-standard");

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
            });
        }

        public IList<GitItemStatus> GetTreeFiles(string treeGuid, bool full)
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
                }).ToList();

            // Doesn't work with removed submodules
            var submodulesList = GetSubmodulesLocalPathes();
            foreach (var item in list)
            {
                if (submodulesList.Contains(item.Name))
                    item.IsSubmodule = true;
            }

            return list;
        }

        public IList<GitItemStatus> GetAllChangedFiles(bool excludeIgnoredFiles = true, UntrackedFilesMode untrackedFiles = UntrackedFilesMode.Default)
        {
            var status = RunGitCmd(GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles, untrackedFiles));

            return GitCommandHelpers.GetAllChangedFilesFromString(this, status);
        }

        public IList<GitItemStatus> GetAllChangedFilesWithSubmodulesStatus(bool excludeIgnoredFiles = true, UntrackedFilesMode untrackedFiles = UntrackedFilesMode.Default)
        {
            var status = GetAllChangedFiles(excludeIgnoredFiles, untrackedFiles);
            GetCurrentSubmoduleStatus(status);
            return status;
        }

        private void GetCurrentSubmoduleStatus(IList<GitItemStatus> status)
        {
            foreach (var item in status)
                if (item.IsSubmodule)
                {
                    var localItem = item;
                    localItem.SubmoduleStatus = Task.Factory.StartNew(() =>
                    {
                        var submoduleStatus = GitCommandHelpers.GetCurrentSubmoduleChanges(this, localItem.Name, localItem.OldName, localItem.IsStaged);
                        if (submoduleStatus != null && submoduleStatus.Commit != submoduleStatus.OldCommit)
                        {
                            var submodule = submoduleStatus.GetSubmodule(this);
                            submoduleStatus.CheckSubmoduleStatus(submodule);
                        }
                        return submoduleStatus;
                    });
                }
        }

        private void GetSubmoduleStatus(IList<GitItemStatus> status, string from, string to)
        {
            status.ForEach(item =>
            {
                if (item.IsSubmodule)
                {
                    item.SubmoduleStatus = Task.Factory.StartNew(() =>
                    {
                        Patch patch = GetSingleDiff(from, to, item.Name, item.OldName, "", SystemEncoding, true);
                        string text = patch != null ? patch.Text : "";
                        var submoduleStatus = GitCommandHelpers.GetSubmoduleStatus(text);
                        if (submoduleStatus.Commit != submoduleStatus.OldCommit)
                        {
                            var submodule = submoduleStatus.GetSubmodule(this);
                            submoduleStatus.CheckSubmoduleStatus(submodule);
                        }
                        return submoduleStatus;
                    });
                }
            });
        }

        public IList<GitItemStatus> GetTrackedChangedFiles()
        {
            var status = RunGitCmd(GitCommandHelpers.GetAllChangedFilesCmd(true, UntrackedFilesMode.No));

            return GitCommandHelpers.GetAllChangedFilesFromString(this, status);
        }

        public IList<GitItemStatus> GetDeletedFiles()
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
            string status = RunGitCmd("diff -M -C -z --cached --name-status", SystemEncoding);

            if (status.Length < 50 && status.Contains("fatal: No HEAD commit to compare"))
            {
                //This command is a little more expensive because it will return both staged and unstaged files
                string command = GitCommandHelpers.GetAllChangedFilesCmd(true, UntrackedFilesMode.No);
                status = RunGitCmd(command, SystemEncoding);
                IList<GitItemStatus> stagedFiles = GitCommandHelpers.GetAllChangedFilesFromString(this, status, false);
                return stagedFiles.Where(f => f.IsStaged).ToList();
            }

            return GitCommandHelpers.GetAllChangedFilesFromString(this, status, true);
        }

        public IList<GitItemStatus> GetStagedFilesWithSubmodulesStatus()
        {
            var status = GetStagedFiles();
            GetCurrentSubmoduleStatus(status);
            return status;
        }

        public IList<GitItemStatus> GetUnstagedFiles()
        {
            return GetAllChangedFiles().Where(x => !x.IsStaged).ToArray();
        }

        public IList<GitItemStatus> GetUnstagedFilesWithSubmodulesStatus()
        {
            return GetAllChangedFilesWithSubmodulesStatus().Where(x => !x.IsStaged).ToArray();
        }

        public IList<GitItemStatus> GitStatus()
        {
            return GitStatus(UntrackedFilesMode.Default, 0);
        }

        public IList<GitItemStatus> GitStatus(UntrackedFilesMode untrackedFilesMode, IgnoreSubmodulesMode ignoreSubmodulesMode)
        {
            string command = GitCommandHelpers.GetAllChangedFilesCmd(true, untrackedFilesMode, ignoreSubmodulesMode);
            string status = RunGitCmd(command);
            return GitCommandHelpers.GetAllChangedFilesFromString(this, status);
        }

        /// <summary>Indicates whether there are any changes to the repository,
        ///  including any untracked files or directories; excluding submodules.</summary>
        public bool IsDirtyDir()
        {
            return GitStatus(UntrackedFilesMode.All, IgnoreSubmodulesMode.Default).Count > 0;
        }

        public Patch GetCurrentChanges(string fileName, string oldFileName, bool staged, string extraDiffArguments, Encoding encoding)
        {
            fileName = string.Concat("\"", FixPath(fileName), "\"");
            if (!string.IsNullOrEmpty(oldFileName))
                oldFileName = string.Concat("\"", FixPath(oldFileName), "\"");

            if (AppSettings.UsePatienceDiffAlgorithm)
                extraDiffArguments = string.Concat(extraDiffArguments, " --patience");

            var args = string.Concat("diff ", extraDiffArguments, " -- ", fileName);
            if (staged)
                args = string.Concat("diff -M -C --cached", extraDiffArguments, " -- ", fileName, " ", oldFileName);

            String result = RunGitCmd(args, LosslessEncoding);
            var patchManager = new PatchManager();
            patchManager.LoadPatch(result, false, encoding);

            return patchManager.Patches.Count > 0 ? patchManager.Patches[patchManager.Patches.Count - 1] : null;
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
            return RunGitCmd("rm --cached \"" + FixPath(file) + "\"");
        }

        public string UnstageFileToRemove(string file)
        {
            return RunGitCmd("reset HEAD -- \"" + FixPath(file) + "\"");
        }

        /// <summary>Dirty but fast. This sometimes fails.</summary>
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
                    return DetachedBranch;
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

        /// <summary>Gets the current branch; or "(no branch)" if HEAD is detached.</summary>
        public string GetSelectedBranch(string repositoryPath)
        {
            string head = GetSelectedBranchFast(repositoryPath);

            if (string.IsNullOrEmpty(head))
            {
                int exitcode;
                head = RunGitCmd("symbolic-ref HEAD", out exitcode);
                if (exitcode == 1)
                    return DetachedBranch;
            }

            return head;
        }

        /// <summary>Gets the current branch; or "(no branch)" if HEAD is detached.</summary>
        public string GetSelectedBranch()
        {
            return GetSelectedBranch(_workingdir);
        }

        /// <summary>Indicates whether HEAD is not pointing to a branch.</summary>
        public bool IsDetachedHead()
        {
            return IsDetachedHead(GetSelectedBranch());
        }

        public bool IsDetachedHead(string branch)
        {
            return DetachedPrefixes.Any(a => branch.StartsWith(a, StringComparison.Ordinal));
        }

        /// <summary>Gets the remote of the current branch; or "origin" if no remote is configured.</summary>
        public string GetCurrentRemote()
        {
            string remote = GetSetting(string.Format("branch.{0}.remote", GetSelectedBranch()));
            return remote;
        }

        /// <summary>Gets the remote branch of the specified local branch; or "" if none is configured.</summary>
        public string GetRemoteBranch(string branch)
        {
            string remote = GetSetting(string.Format("branch.{0}.remote", branch));
            string merge = GetSetting(string.Format("branch.{0}.merge", branch));
            if (String.IsNullOrEmpty(remote) || String.IsNullOrEmpty(merge))
                return "";
            return remote + "/" + (merge.StartsWith("refs/heads/") ? merge.Substring(11) : merge);
        }

        public RemoteActionResult<IList<GitRef>> GetRemoteRefs(string remote, bool tags, bool branches)
        {
            RemoteActionResult<IList<GitRef>> result = new RemoteActionResult<IList<GitRef>>()
            {
                AuthenticationFail = false,
                HostKeyFail = false,
                Result = null
            };

            remote = FixPath(remote);

            var tree = GetTreeFromRemoteRefs(remote, tags, branches);

            // If the authentication failed because of a missing key, ask the user to supply one. 
            if (tree.Contains("FATAL ERROR") && tree.Contains("authentication"))
            {
                result.AuthenticationFail = true;
            }
            else if (tree.ToLower().Contains("the server's host key is not cached in the registry"))
            {
                result.HostKeyFail = true;
            }
            else
            {
                result.Result = GetTreeRefs(tree);
            }

            return result;
        }

        private string GetTreeFromRemoteRefs(string remote, bool tags, bool branches)
        {
            if (tags && branches)
                return RunGitCmd("ls-remote --heads --tags \"" + remote + "\"");
            if (tags)
                return RunGitCmd("ls-remote --tags \"" + remote + "\"");
            if (branches)
                return RunGitCmd("ls-remote --heads \"" + remote + "\"");
            return "";
        }

        public IList<GitRef> GetRefs(bool tags = true, bool branches = true)
        {
            var tree = GetTree(tags, branches);
            return GetTreeRefs(tree);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="option">Ordery by date is slower.</param>
        /// <returns></returns>
        public IList<GitRef> GetTagRefs(GetTagRefsSortOrder option)
        {
            var list = GetRefs(true, false);

            var sortedList = new List<GitRef>();

            if (option == GetTagRefsSortOrder.ByCommitDateAscending)
            {
                sortedList = list.OrderBy(head =>
                {
                    var r = new GitRevision(this, head.Guid);
                    return r.CommitDate;
                }).ToList();
            }
            else if (option == GetTagRefsSortOrder.ByCommitDateDescending)
            {
                sortedList = list.OrderByDescending(head =>
                {
                    var r = new GitRevision(this, head.Guid);
                    return r.CommitDate;
                }).ToList();
            }
            else
                sortedList = new List<GitRef>(list);

            return sortedList;
        }

        public enum GetTagRefsSortOrder
        {
            /// <summary>
            /// default
            /// </summary>
            ByName,

            /// <summary>
            /// slower than ByName
            /// </summary>
            ByCommitDateAscending,

            /// <summary>
            /// slower than ByName
            /// </summary>
            ByCommitDateDescending
        }

        public ICollection<string> GetMergedBranches()
        {
            return RunGitCmd(GitCommandHelpers.MergedBranches()).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private string GetTree(bool tags, bool branches)
        {
            if (tags && branches)
                return RunGitCmd("show-ref --dereference", SystemEncoding);

            if (tags)
                return RunGitCmd("show-ref --tags", SystemEncoding);

            if (branches)
                return RunGitCmd("show-ref --dereference --heads", SystemEncoding);
            return "";
        }

        private IList<GitRef> GetTreeRefs(string tree)
        {
            var itemsStrings = tree.Split('\n');

            var gitRefs = new List<GitRef>();
            var defaultHeads = new Dictionary<string, GitRef>(); // remote -> HEAD
            var remotes = GetRemotes(false);

            foreach (var itemsString in itemsStrings)
            {
                if (itemsString == null || itemsString.Length <= 42)
                    continue;

                var completeName = itemsString.Substring(41).Trim();
                var guid = itemsString.Substring(0, 40);
                var remoteName = GitCommandHelpers.GetRemoteName(completeName, remotes);
                var head = new GitRef(this, guid, completeName, remoteName);
                if (DefaultHeadPattern.IsMatch(completeName))
                    defaultHeads[remoteName] = head;
                else
                    gitRefs.Add(head);
            }

            // do not show default head if remote has a branch on the same commit
            GitRef defaultHead;
            foreach (var gitRef in gitRefs.Where(head => defaultHeads.TryGetValue(head.Remote, out defaultHead) && head.Guid == defaultHead.Guid))
            {
                defaultHeads.Remove(gitRef.Remote);
            }

            gitRefs.AddRange(defaultHeads.Values);

            return gitRefs;
        }

        /// <summary>
        /// Gets branches which contain the given commit.
        /// If both local and remote branches are requested, remote branches are prefixed with "remotes/"
        /// (as returned by git branch -a)
        /// </summary>
        /// <param name="sha1">The sha1.</param>
        /// <param name="getLocal">Pass true to include local branches</param>
        /// <param name="getRemote">Pass true to include remote branches</param>
        /// <returns></returns>
        public IEnumerable<string> GetAllBranchesWhichContainGivenCommit(string sha1, bool getLocal, bool getRemote)
        {
            string args = "--contains " + sha1;
            if (getRemote && getLocal)
                args = "-a " + args;
            else if (getRemote)
                args = "-r " + args;
            else if (!getLocal)
                return new string[] { };
            string info = RunGitCmd("branch " + args);
            if (info.Trim().StartsWith("fatal") || info.Trim().StartsWith("error:"))
                return new List<string>();

            string[] result = info.Split(new[] { '\r', '\n', '*' }, StringSplitOptions.RemoveEmptyEntries);

            // Remove symlink targets as in "origin/HEAD -> origin/master"
            for (int i = 0; i < result.Length; i++)
            {
                string item = result[i].Trim();
                int idx;
                if (getRemote && ((idx = item.IndexOf(" ->")) >= 0))
                {
                    item = item.Substring(0, idx);
                }
                result[i] = item;
            }

            return result;
        }

        /// <summary>
        /// Gets all tags which contain the given commit.
        /// </summary>
        /// <param name="sha1">The sha1.</param>
        /// <returns></returns>
        public IEnumerable<string> GetAllTagsWhichContainGivenCommit(string sha1)
        {
            string info = RunGitCmd("tag --contains " + sha1, SystemEncoding);


            if (info.Trim().StartsWith("fatal") || info.Trim().StartsWith("error:"))
                return new List<string>();
            return info.Split(new[] { '\r', '\n', '*', ' ' }, StringSplitOptions.RemoveEmptyEntries);
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

        public IList<GitItem> GetFileChanges(string file)
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
            string tree = this.RunCacheableCmd(AppSettings.GitCommand, String.Format("ls-tree -z -r --name-only {0}", id), SystemEncoding);
            return tree.Split(new char[] { '\0', '\n' });
        }

        public IList<IGitItem> GetTree(string id, bool full)
        {
            string args = "-z";
            if (full)
                args += " -r";
            var tree = this.RunCacheableCmd(AppSettings.GitCommand, "ls-tree " + args + " \"" + id + "\"", SystemEncoding);

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
                RunCacheableCmd(
                    AppSettings.GitCommand,
                    blameCommand,
                    LosslessEncoding
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
                        var headerParams = line.Split(' ');
                        blameLine.CommitGuid = headerParams[0];
                        if (headerParams.Length >= 3)
                        {
                            blameLine.OriginLineNumber = int.Parse(headerParams[1]);
                            blameLine.FinalLineNumber = int.Parse(headerParams[2]);
                        }
                        blame.Lines.Add(blameLine);
                    }
                }
                catch
                {
                    //Catch all parser errors, and ignore them all!
                    //We should never get here...
                    AppSettings.GitLog.Log("Error parsing output from command: " + blameCommand + "\n\nPlease report a bug!");
                }
            }

            return blame;
        }

        public string GetFileRevisionText(string file, string revision, Encoding encoding)
        {
            return RunCacheableCmd(
                    AppSettings.GitCommand,
                    string.Format("show {0}:\"{1}\"", revision, file.Replace('\\', '/')), encoding);
        }

        public string GetFileText(string id, Encoding encoding)
        {
            return RunCacheableCmd(AppSettings.GitCommand, "cat-file blob \"" + id + "\"", encoding);
        }

        public string GetFileBlobHash(string fileName, string revision)
        {
            if (revision == GitRevision.UnstagedGuid) //working dir changes
            {
                return null;
            }
            if (revision == GitRevision.IndexGuid) //index
            {
                string blob = RunGitCmd(string.Format("ls-files -s \"{0}\"", fileName));
                string[] s = blob.Split(new char[] { ' ', '\t' });
                if (s.Length >= 2)
                    return s[1];

            }
            else
            {
                string blob = RunGitCmd(string.Format("ls-tree -r {0} \"{1}\"", revision, fileName));
                string[] s = blob.Split(new char[] { ' ', '\t' });
                if (s.Length >= 3)
                    return s[2];
            }
            return string.Empty;
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

                using (var process = RunGitCmdDetached("cat-file blob " + blob))
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

        public IEnumerable<string> GetPreviousCommitMessages(int count)
        {
            return GetPreviousCommitMessages("HEAD", count);
        }

        public IEnumerable<string> GetPreviousCommitMessages(string revision, int count)
        {
            string sep = "d3fb081b9000598e658da93657bf822cc87b2bf6";
            string output = RunGitCmd("log -n " + count + " " + revision + " --pretty=format:" + sep + "%e%n%s%n%n%b", LosslessEncoding);
            string[] messages = output.Split(new string[] { sep }, StringSplitOptions.RemoveEmptyEntries);

            if (messages.Length == 0)
                return new string[] { string.Empty };

            return messages.Select(cm =>
                {
                    int idx = cm.IndexOf("\n");
                    string encodingName = cm.Substring(0, idx);
                    cm = cm.Substring(idx + 1, cm.Length - idx - 1);
                    cm = ReEncodeCommitMessage(cm, encodingName);
                    return cm;

                });
        }


        public string MergeBranch(string branch)
        {
            return RunGitCmd(GitCommandHelpers.MergeBranchCmd(branch, true, false, false, null));
        }

        public string OpenWithDifftool(string filename, string oldFileName = "", string revision1 = null, string revision2 = null, string extraDiffArguments = "")
        {
            var output = "";
            if (!filename.IsNullOrEmpty())
                filename = filename.Quote();
            if (!oldFileName.IsNullOrEmpty())
                oldFileName = oldFileName.Quote();

            string args = string.Join(" ", extraDiffArguments, revision2.QuoteNE(), revision1.QuoteNE(), "--", filename, oldFileName);
            RunGitCmdDetached("difftool --gui --no-prompt " + args);
            return output;
        }

        public string RevParse(string revisionExpression)
        {
            string revparseCommand = string.Format("rev-parse \"{0}~0\"", revisionExpression);
            int exitCode;
            string[] resultStrings = RunGitCmd(revparseCommand, out exitCode).Split('\n');
            return exitCode == 0 ? resultStrings[0] : "";
        }

        public string GetMergeBase(string a, string b)
        {
            return RunGitCmd("merge-base " + a + " " + b).TrimEnd();
        }

        public SubmoduleStatus CheckSubmoduleStatus(string commit, string oldCommit, CommitData data, CommitData olddata, bool loaddata = false)
        {
            if (!IsValidGitWorkingDir() || oldCommit == null)
                return SubmoduleStatus.NewSubmodule;

            if (commit == null || commit == oldCommit)
                return SubmoduleStatus.Unknown;

            string baseCommit = GetMergeBase(commit, oldCommit);
            if (baseCommit == oldCommit)
                return SubmoduleStatus.FastForward;
            else if (baseCommit == commit)
                return SubmoduleStatus.Rewind;

            string error = "";
            if (loaddata)
                olddata = CommitData.GetCommitData(this, oldCommit, ref error);
            if (olddata == null)
                return SubmoduleStatus.NewSubmodule;
            if (loaddata)
                data = CommitData.GetCommitData(this, commit, ref error);
            if (data == null)
                return SubmoduleStatus.Unknown;
            if (data.CommitDate > olddata.CommitDate)
                return SubmoduleStatus.NewerTime;
            else if (data.CommitDate < olddata.CommitDate)
                return SubmoduleStatus.OlderTime;
            else if (data.CommitDate == olddata.CommitDate)
                return SubmoduleStatus.SameTime;
            return SubmoduleStatus.Unknown;
        }

        public SubmoduleStatus CheckSubmoduleStatus(string commit, string oldCommit)
        {
            return CheckSubmoduleStatus(commit, oldCommit, null, null, true);
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
            RunGitCmd(string.Format("check-ref-format --branch \"{0}\"", branchName), out exitCode);
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

            return File.Exists(indexLockFile);
        }

        public bool IsRunningGitProcess()
        {
            if (IsLockedIndex())
            {
                return true;
            }

            // Get processes by "ps" command.
            var cmd = Path.Combine(AppSettings.GitBinDir, "ps");
            var arguments = "x";
            if (EnvUtils.RunningOnWindows())
            {
                // "x" option is unimplemented by msysgit and cygwin.
                arguments = "";
            }

            var output = RunCmd(cmd, arguments);
            var lines = output.Split('\n');
            if (lines.Count() >= 2)
                return false;
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
            IList<byte> blist = new List<byte>();
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
                            int code = Convert.ToInt32(octNumber, 8);
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
                        sb.Append(SystemEncoding.GetString(blist.ToArray()));
                        blist.Clear();
                    }

                    sb.Append(c);
                    i++;
                }
            }
            if (blist.Count > 0)
            {
                sb.Append(SystemEncoding.GetString(blist.ToArray()));
                blist.Clear();
            }
            return sb.ToString();
        }

        public static string ReEncodeFileNameFromLossless(string fileName)
        {
            fileName = ReEncodeStringFromLossless(fileName, SystemEncoding);
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

            header = ReEncodeString(header, LosslessEncoding, LogOutputEncoding);
            diffHeader = ReEncodeFileNameFromLossless(diffHeader);
            diffContent = ReEncodeString(diffContent, LosslessEncoding, FilesEncoding);
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

        /// <summary>Gets the path to the git application executable.</summary>
        public string GitCommand
        {
            get
            {
                return AppSettings.GitCommand;
            }
        }

        public Version AppVersion
        {
            get
            {
                return AppSettings.AppVersion;
            }
        }

        public string GravatarCacheDir
        {
            get
            {
                return AppSettings.GravatarCachePath;
            }
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null) { return false; }
            if (obj == this) { return true; }

            GitModule other = obj as GitModule;
            return (other != null) && Equals(other);
        }

        bool Equals(GitModule other)
        {
            return
                string.Equals(_workingdir, other._workingdir) &&
                Equals(_superprojectModule, other._superprojectModule);
        }

        public override int GetHashCode()
        {
            return (_workingdir != null
                ? _workingdir.GetHashCode()
                : 0);
        }

        public override string ToString()
        {
            return GitWorkingDir;
        }
    }
}
