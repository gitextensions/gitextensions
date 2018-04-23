using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.Git.Extensions;
using GitCommands.Patches;
using GitCommands.Settings;
using GitCommands.Utils;
using GitUI;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;
using SmartFormat;

namespace GitCommands
{
    public class GitModuleEventArgs : EventArgs
    {
        public GitModuleEventArgs(GitModule gitModule)
        {
            GitModule = gitModule;
        }

        public GitModule GitModule { get; }
    }

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

    public enum ForcePushOptions
    {
        DoNotForce,
        Force,
        ForceWithLease,
    }

    public struct ConflictedFileData
    {
        public ConflictedFileData(string hash, string filename)
        {
            Hash = hash;
            Filename = filename;
        }

        public string Hash { get; }
        public string Filename { get; }
    }

    [DebuggerDisplay("{" + nameof(Filename) + "}")]
    public struct ConflictData
    {
        public ConflictData(ConflictedFileData @base, ConflictedFileData local,
            ConflictedFileData remote)
        {
            Base = @base;
            Local = local;
            Remote = remote;
        }

        public ConflictedFileData Base { get; }
        public ConflictedFileData Local { get; }
        public ConflictedFileData Remote { get; }

        public string Filename => Local.Filename ?? Base.Filename ?? Remote.Filename;
    }

    /// <summary>Provides manipulation with git module.
    /// <remarks>Several instances may be created for submodules.</remarks></summary>
    [DebuggerDisplay("GitModule ( {" + nameof(WorkingDir) + "} )")]
    public sealed class GitModule : IGitModule
    {
        public static readonly char RefSeparator = '/';
        public static readonly string RefSep = RefSeparator.ToString(CultureInfo.InvariantCulture);

        private const char LineSeparator = '\n';
        public static readonly char ActiveBranchIndicator = '*';

        private static readonly Regex AnsiCodePattern = new Regex(@"\u001B[\u0040-\u005F].*?[\u0040-\u007E]", RegexOptions.Compiled);
        private static readonly Regex CpEncodingPattern = new Regex("cp\\d+", RegexOptions.Compiled);
        private readonly object _lock = new object();
        private readonly IIndexLockManager _indexLockManager;
        private readonly ICommitDataManager _commitDataManager;
        private static readonly IGitDirectoryResolver GitDirectoryResolverInstance = new GitDirectoryResolver();
        private readonly IGitTreeParser _gitTreeParser = new GitTreeParser();
        private readonly IRevisionDiffProvider _revisionDiffProvider = new RevisionDiffProvider();

        public static readonly string NoNewLineAtTheEnd = "\\ No newline at end of file";
        private const string DiffCommandWithStandardArgs = " -c diff.submodule=short diff --no-color ";

        public GitModule(string workingdir)
        {
            _superprojectInit = false;
            WorkingDir = (workingdir ?? "").EnsureTrailingPathSeparator();
            WorkingDirGitDir = GitDirectoryResolverInstance.Resolve(WorkingDir);
            _indexLockManager = new IndexLockManager(this);
            _commitDataManager = new CommitDataManager(() => this);
        }

        /// <summary>
        /// Gets the directory which contains the git repository.
        /// </summary>
        [NotNull]
        public string WorkingDir { get; }

        /// <summary>
        /// Gets the location of .git directory for the current working folder.
        /// </summary>
        [NotNull]
        public string WorkingDirGitDir { get; private set; }

        /// <summary>Gets the path to the git application executable.</summary>
        public string GitCommand => AppSettings.GitCommand;

        public Version AppVersion => AppSettings.AppVersion;

        public string GravatarCacheDir => AppSettings.GravatarCachePath;

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
            {
                return null;
            }

            do
            {
                if (module.SuperprojectModule == null)
                {
                    return module;
                }

                module = module.SuperprojectModule;
            }
            while (module != null);
            return module;
        }

        private RepoDistSettings _effectiveSettings;
        public RepoDistSettings EffectiveSettings
        {
            get
            {
                lock (_lock)
                {
                    if (_effectiveSettings == null)
                    {
                        _effectiveSettings = RepoDistSettings.CreateEffective(this);
                    }
                }

                return _effectiveSettings;
            }
        }

        public ISettingsSource GetEffectiveSettings()
        {
            return EffectiveSettings;
        }

        private RepoDistSettings _localSettings;
        public RepoDistSettings LocalSettings
        {
            get
            {
                lock (_lock)
                {
                    if (_localSettings == null)
                    {
                        _localSettings = new RepoDistSettings(null, EffectiveSettings.SettingsCache);
                    }
                }

                return _localSettings;
            }
        }

        private ConfigFileSettings _effectiveConfigFile;
        public ConfigFileSettings EffectiveConfigFile
        {
            get
            {
                lock (_lock)
                {
                    if (_effectiveConfigFile == null)
                    {
                        _effectiveConfigFile = ConfigFileSettings.CreateEffective(this);
                    }
                }

                return _effectiveConfigFile;
            }
        }

        public ConfigFileSettings LocalConfigFile => new ConfigFileSettings(null, EffectiveConfigFile.SettingsCache);

        IConfigFileSettings IGitModule.LocalConfigFile => LocalConfigFile;

        // encoding for files paths
        private static Encoding _systemEncoding;
        public static Encoding SystemEncoding
        {
            get
            {
                if (_systemEncoding == null)
                {
                    // check whether GitExtensions works with standard msysgit or msysgit-unicode

                    // invoke a git command that returns an invalid argument in its response, and
                    // check if a unicode-only character is reported back. If so assume msysgit-unicode

                    // git config --get with a malformed key (no section) returns:
                    // "error: key does not contain a section: <key>"
                    const string controlStr = "ą"; // "a caudata"
                    string arguments = string.Format("config --get {0}", controlStr);

                    string s = new GitModule("").RunGitCmd(arguments, Encoding.UTF8);
                    if (s != null && s.IndexOf(controlStr) != -1)
                    {
                        _systemEncoding = new UTF8Encoding(false);
                    }
                    else
                    {
                        _systemEncoding = Encoding.Default;
                    }

                    Debug.WriteLine("System encoding: " + _systemEncoding.EncodingName);
                }

                return _systemEncoding;
            }
        }

        // Encoding that let us read all bytes without replacing any char
        // It is using to read output of commands, which may consist of:
        // 1) commit header (message, author, ...) encoded in CommitEncoding, recoded to LogOutputEncoding or not dependent of
        //    pretty parameter (pretty=raw - recoded, pretty=format - not recoded)
        // 2) file content encoded in its original encoding
        // 3) file path (file name is encoded in system default encoding),
        //    when core.quotepath is on, every non ASCII character is escaped
        //    with \ followed by its code as a three digit octal number
        // 4) branch, tag name, errors, warnings, hints encoded in system default encoding
        public static readonly Encoding LosslessEncoding = Encoding.GetEncoding("ISO-8859-1"); // is any better?

        public Encoding FilesEncoding => EffectiveConfigFile.FilesEncoding ?? new UTF8Encoding(false);

        public Encoding CommitEncoding => EffectiveConfigFile.CommitEncoding ?? new UTF8Encoding(false);

        /// <summary>
        /// Encoding for commit header (message, notes, author, committer, emails)
        /// </summary>
        public Encoding LogOutputEncoding => EffectiveConfigFile.LogOutputEncoding ?? CommitEncoding;

        public AppSettings.PullAction LastPullAction
        {
            get => AppSettings.GetEnum("LastPullAction_" + WorkingDir, AppSettings.PullAction.None);
            set => AppSettings.SetEnum("LastPullAction_" + WorkingDir, value);
        }

        public void LastPullActionToFormPullAction()
        {
            if (LastPullAction == AppSettings.PullAction.FetchAll)
            {
                AppSettings.FormPullAction = AppSettings.PullAction.Fetch;
            }
            else if (LastPullAction != AppSettings.PullAction.None)
            {
                AppSettings.FormPullAction = LastPullAction;
            }
        }

        /// <summary>Indicates whether the <see cref="WorkingDir"/> contains a git repository.</summary>
        public bool IsValidGitWorkingDir()
        {
            return IsValidGitWorkingDir(WorkingDir);
        }

        /// <summary>Indicates whether the specified directory contains a git repository.</summary>
        public static bool IsValidGitWorkingDir(string dir)
        {
            if (string.IsNullOrEmpty(dir))
            {
                return false;
            }

            string dirPath = dir.EnsureTrailingPathSeparator();
            string path = dirPath + ".git";

            if (Directory.Exists(path) || File.Exists(path))
            {
                return true;
            }

            return Directory.Exists(dirPath + "info") &&
                   Directory.Exists(dirPath + "objects") &&
                   Directory.Exists(dirPath + "refs");
        }

        /// <summary>
        /// Asks git to resolve the given relativePath
        /// git special folders are located in different directories depending on the kind of repo: submodule, worktree, main
        /// See https://git-scm.com/docs/git-rev-parse#git-rev-parse---git-pathltpathgt
        /// </summary>
        /// <param name="relativePath">A path relative to the .git directory</param>
        public string ResolveGitInternalPath(string relativePath)
        {
            string gitPath = RunGitCmd("rev-parse --git-path " + relativePath.Quote());
            string systemPath = PathUtil.ToNativePath(gitPath.Trim());
            if (systemPath.StartsWith(".git\\"))
            {
                systemPath = Path.Combine(GetGitDirectory(), systemPath.Substring(".git\\".Length));
            }

            return systemPath;
        }

        private string _gitCommonDirectory;

        /// <summary>
        /// Returns git common directory
        /// https://git-scm.com/docs/git-rev-parse#git-rev-parse---git-common-dir
        /// </summary>
        public string GitCommonDirectory
        {
            get
            {
                if (_gitCommonDirectory == null)
                {
                    var commDir = RunGitCmdResult("rev-parse --git-common-dir");
                    _gitCommonDirectory = PathUtil.ToNativePath(commDir.StdOutput.Trim());
                    if (!commDir.ExitedSuccessfully || _gitCommonDirectory == ".git" || _gitCommonDirectory == "." || !Directory.Exists(_gitCommonDirectory))
                    {
                        _gitCommonDirectory = GetGitDirectory();
                    }
                }

                return _gitCommonDirectory;
            }
        }

        /// <summary>Gets the ".git" directory path.</summary>
        private string GetGitDirectory()
        {
            return GetGitDirectory(WorkingDir);
        }

        public static string GetGitDirectory(string repositoryPath)
        {
            return GitDirectoryResolverInstance.Resolve(repositoryPath);
        }

        public bool IsBareRepository()
        {
            return WorkingDir == GetGitDirectory();
        }

        public static bool IsBareRepository(string repositoryPath)
        {
            return repositoryPath == GetGitDirectory(repositoryPath);
        }

        public bool IsSubmodule(string submodulePath)
        {
            var result = RunGitCmdResult("submodule status " + submodulePath);

            if (result.ExitCode == 0

                // submodule removed
                || result.StdError.StartsWith("No submodule mapping found in .gitmodules for path"))
            {
                return true;
            }

            return false;
        }

        public bool HasSubmodules()
        {
            return GetSubmodulesLocalPaths(recursive: false).Any();
        }

        /// <summary>
        /// Gets the local paths of any submodules of this git module.
        /// </summary>
        /// <remarks>
        /// <para>This method obtains its results by parsing the <c>.gitmodules</c> file.</para>
        ///
        /// <para>This approach is a faster than <see cref="GetSubmodulesInfo"/> which
        /// invokes the <c>git submodule</c> command.</para>
        /// </remarks>
        public IReadOnlyList<string> GetSubmodulesLocalPaths(bool recursive = true)
        {
            var submodules = GetSubmodulePaths(this);

            if (recursive)
            {
                for (int i = 0; i < submodules.Count; i++)
                {
                    var submodule = GetSubmodule(submodules[i]);

                    var subsubmodules = GetSubmodulePaths(submodule)
                        .Select(p => Path.Combine(submodules[i], p))
                        .ToList();

                    submodules.InsertRange(i + 1, subsubmodules);
                    i += subsubmodules.Count;
                }
            }

            return submodules;

            List<string> GetSubmodulePaths(GitModule module)
            {
                var configFile = module.GetSubmoduleConfigFile();

                return configFile.ConfigSections
                    .Select(section => section.GetValue("path").Trim())
                    .ToList();
            }
        }

        [NotNull]
        public static string FindGitWorkingDir([CanBeNull] string startDir)
        {
            if (string.IsNullOrEmpty(startDir))
            {
                return "";
            }

            var dir = startDir.Trim();

            do
            {
                if (IsValidGitWorkingDir(dir))
                {
                    return dir.EnsureTrailingPathSeparator();
                }

                dir = PathUtil.GetDirectoryName(dir);
            }
            while (!string.IsNullOrEmpty(dir));

            return startDir;
        }

        [NotNull]
        private static Process StartProccess([NotNull] string fileName, [NotNull] string arguments, [NotNull] string workingDir, bool showConsole)
        {
            EnvironmentConfiguration.SetEnvironmentVariables();

            string quotedCmd = fileName;
            if (quotedCmd.IndexOf(' ') != -1)
            {
                quotedCmd = quotedCmd.Quote();
            }

            var executionStartTimestamp = DateTime.Now;

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

            var startProcess = Process.Start(startInfo);

            startProcess.Exited += (sender, args) =>
            {
                var executionEndTimestamp = DateTime.Now;
                AppSettings.GitLog.Log(quotedCmd + " " + arguments, executionStartTimestamp, executionEndTimestamp);
            };

            return startProcess;
        }

        [NotNull]
        public string StripAnsiCodes(string input)
        {
            // The following does return the original string if no ansi codes are found
            return AnsiCodePattern.Replace(input, "");
        }

        /// <summary>
        /// Run command, console window is visible
        /// </summary>
        public Process RunExternalCmdDetachedShowConsole(string cmd, string arguments)
        {
            try
            {
                return StartProccess(cmd, arguments, WorkingDir, showConsole: true);
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
                using (var process = StartProccess(cmd, arguments, WorkingDir, showConsole: true))
                {
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Run command, console window is hidden
        /// </summary>
        [CanBeNull]
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
        [CanBeNull]
        public Process RunExternalCmdDetached(string cmd, string arguments)
        {
            return RunExternalCmdDetached(cmd, arguments, WorkingDir);
        }

        /// <summary>
        /// Run git command, console window is hidden, redirect output
        /// </summary>
        [NotNull]
        public Process RunGitCmdDetached(string arguments, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = SystemEncoding;
            }

            return GitCommandHelpers.StartProcess(AppSettings.GitCommand, arguments, WorkingDir, encoding);
        }

        /// <summary>
        /// Run command, cache results, console window is hidden, wait for exit, redirect output
        /// </summary>
        [NotNull]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public string RunCacheableCmd(string cmd, string arguments = "", Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = SystemEncoding;
            }

            byte[] cmdout, cmderr;
            if (!GitCommandCache.TryGet(arguments, out cmdout, out cmderr))
            {
                GitCommandHelpers.RunCmdByte(cmd, arguments, WorkingDir, null, out cmdout, out cmderr);

                GitCommandCache.Add(arguments, cmdout, cmderr);
            }

            return StripAnsiCodes(EncodingHelper.DecodeString(cmdout, cmderr, ref encoding));
        }

        /// <summary>
        /// Run command, console window is hidden, wait for exit, redirect output
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public CmdResult RunCmdResult(string cmd, string arguments, Encoding encoding = null, byte[] stdInput = null)
        {
            int exitCode = GitCommandHelpers.RunCmdByte(cmd, arguments, WorkingDir, stdInput, out var output, out var error);
            if (encoding == null)
            {
                encoding = SystemEncoding;
            }

            return new CmdResult
            {
                StdOutput = output == null ? string.Empty : StripAnsiCodes(encoding.GetString(output)),
                StdError = error == null ? string.Empty : StripAnsiCodes(encoding.GetString(error)),
                ExitCode = exitCode
            };
        }

        /// <summary>
        /// Run command, console window is hidden, wait for exit, redirect output
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public async Task<string> RunCmdAsync(string cmd, string arguments, Encoding encoding = null, byte[] stdInput = null)
        {
            return await Task.FromResult(RunCmdResult(cmd, arguments, encoding, stdInput).GetString()).ConfigureAwait(false);
        }

        /// <summary>
        /// Run git command, console window is hidden, wait for exit, redirect output
        /// </summary>
        public string RunGitCmd(string arguments, Encoding encoding = null, byte[] stdInput = null)
        {
            return ThreadHelper.JoinableTaskFactory.Run(() =>
            {
                return RunCmdAsync(AppSettings.GitCommand, arguments, encoding, stdInput);
            });
        }

        /// <summary>
        /// Run git command, console window is hidden, wait for exit, redirect output
        /// </summary>
        public CmdResult RunGitCmdResult(string arguments, Encoding encoding = null, byte[] stdInput = null)
        {
            return RunCmdResult(AppSettings.GitCommand, arguments, encoding, stdInput);
        }

        /// <summary>
        /// Run command, console window is hidden, wait for exit, redirect output
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private IEnumerable<string> ReadCmdOutputLines(string cmd, string arguments, string stdInput)
        {
            return GitCommandHelpers.ReadCmdOutputLines(cmd, arguments, WorkingDir, stdInput);
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
        public async Task<string> RunBatchFileAsync(string batchFile)
        {
            string tempFileName = Path.ChangeExtension(Path.GetTempFileName(), ".cmd");
            using (var writer = new StreamWriter(tempFileName))
            {
                await writer.WriteLineAsync("@prompt $G").ConfigureAwait(false);
                await writer.WriteAsync(batchFile).ConfigureAwait(false);
            }

            string result = await RunCmdAsync("cmd.exe", "/C \"" + tempFileName + "\"").ConfigureAwait(false);
            File.Delete(tempFileName);
            return result;
        }

        public void EditNotes(string revision)
        {
            string editor = GetEffectiveSetting("core.editor").ToLower();
            if (editor.Contains("gitextensions") || editor.Contains("notepad"))
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

        public bool HandleConflictSelectSide(string fileName, string side)
        {
            Directory.SetCurrentDirectory(WorkingDir);
            fileName = fileName.ToPosixPath();

            side = GetSide(side);

            string result = RunGitCmd(string.Format("checkout-index -f --stage={0} -- \"{1}\"", side, fileName));
            if (!result.IsNullOrEmpty())
            {
                return false;
            }

            result = RunGitCmd(string.Format("add -- \"{0}\"", fileName));
            return result.IsNullOrEmpty();
        }

        public bool HandleConflictsSaveSide(string fileName, string saveAsFileName, string side)
        {
            Directory.SetCurrentDirectory(WorkingDir);
            fileName = fileName.ToPosixPath();

            side = GetSide(side);

            var result = RunGitCmd(string.Format("checkout-index --stage={0} --temp -- \"{1}\"", side, fileName));
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
            using (var ms = (MemoryStream)GetFileStream(blob)) // Ugly, has implementation info.
            {
                byte[] buf = ms.ToArray();
                if (EffectiveConfigFile.core.autocrlf.ValueOrDefault == AutoCRLFType.@true)
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
            {
                side = "3";
            }

            if (side.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
            {
                side = "2";
            }

            if (side.Equals("BASE", StringComparison.CurrentCultureIgnoreCase))
            {
                side = "1";
            }

            return side;
        }

        public string[] CheckoutConflictedFiles(ConflictData unmergedData)
        {
            Directory.SetCurrentDirectory(WorkingDir);

            var filename = unmergedData.Filename;

            string[] fileNames =
                {
                    filename + ".BASE",
                    filename + ".LOCAL",
                    filename + ".REMOTE"
                };

            var unmerged = new[] { unmergedData.Base.Filename, unmergedData.Local.Filename, unmergedData.Remote.Filename };

            for (int i = 0; i < unmerged.Length; i++)
            {
                if (unmerged[i] == null)
                {
                    continue;
                }

                var tempFile =
                    RunGitCmd("checkout-index --temp --stage=" + (i + 1) + " -- \"" + filename + "\"");
                tempFile = tempFile.Split('\t')[0];
                tempFile = Path.Combine(WorkingDir, tempFile);

                var newFileName = Path.Combine(WorkingDir, fileNames[i]);
                try
                {
                    fileNames[i] = newFileName;
                    var index = 1;
                    while (File.Exists(fileNames[i]) && index < 50)
                    {
                        fileNames[i] = newFileName + index;
                        index++;
                    }

                    File.Move(tempFile, fileNames[i]);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }

            if (!File.Exists(fileNames[0]))
            {
                fileNames[0] = null;
            }

            if (!File.Exists(fileNames[1]))
            {
                fileNames[1] = null;
            }

            if (!File.Exists(fileNames[2]))
            {
                fileNames[2] = null;
            }

            return fileNames;
        }

        public ConflictData GetConflict(string filename)
        {
            return GetConflicts(filename).SingleOrDefault();
        }

        public List<ConflictData> GetConflicts(string filename = "")
        {
            filename = filename.ToPosixPath();

            var list = new List<ConflictData>();

            var unmerged = RunGitCmd("ls-files -z --unmerged " + filename.QuoteNE()).Split(new[] { '\0', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var item = new ConflictedFileData[3];

            string prevItemName = null;

            foreach (var line in unmerged)
            {
                int findSecondWhitespace = line.IndexOfAny(new[] { ' ', '\t' });
                string fileStage = findSecondWhitespace >= 0 ? line.Substring(findSecondWhitespace).Trim() : "";

                findSecondWhitespace = fileStage.IndexOfAny(new[] { ' ', '\t' });

                string hash = findSecondWhitespace >= 0 ? fileStage.Substring(0, findSecondWhitespace).Trim() : "";
                fileStage = findSecondWhitespace >= 0 ? fileStage.Substring(findSecondWhitespace).Trim() : "";

                if (fileStage.Length > 2 && int.TryParse(fileStage[0].ToString(), out var stage) && stage >= 1 && stage <= 3)
                {
                    var itemName = fileStage.Substring(2);
                    if (prevItemName != itemName && prevItemName != null)
                    {
                        list.Add(new ConflictData(item[0], item[1], item[2]));
                        item = new ConflictedFileData[3];
                    }

                    item[stage - 1] = new ConflictedFileData(hash, itemName);
                    prevItemName = itemName;
                }
            }

            if (prevItemName != null)
            {
                list.Add(new ConflictData(item[0], item[1], item[2]));
            }

            return list;
        }

        public IReadOnlyList<string> GetSortedRefs()
        {
            const string command = "for-each-ref --sort=-committerdate --sort=-taggerdate --format=\"%(refname)\" refs/";

            var tree = RunGitCmd(command, SystemEncoding);

            return tree.Split();
        }

        public Dictionary<IGitRef, IGitItem> GetSubmoduleItemsForEachRef(string filename, Func<IGitRef, bool> showRemoteRef)
        {
            string command = GetSortedRefsCommand();

            if (command == null)
            {
                return new Dictionary<IGitRef, IGitItem>();
            }

            filename = filename.ToPosixPath();

            var refList = RunGitCmd(command, SystemEncoding);

            var refs = ParseRefs(refList);

            return refs.Where(showRemoteRef).ToDictionary(r => r, r => GetSubmoduleCommitHash(filename, r.Name));
        }

        private static string GetSortedRefsCommand()
        {
            if (AppSettings.ShowSuperprojectRemoteBranches)
            {
                return "for-each-ref --sort=-committerdate --format=\"%(objectname) %(refname)\" refs/";
            }

            if (AppSettings.ShowSuperprojectBranches || AppSettings.ShowSuperprojectTags)
            {
                return "for-each-ref --sort=-committerdate --format=\"%(objectname) %(refname)\""
                    + (AppSettings.ShowSuperprojectBranches ? " refs/heads/" : null)
                    + (AppSettings.ShowSuperprojectTags ? " refs/tags/" : null);
            }

            return null;
        }

        private IGitItem GetSubmoduleCommitHash(string filename, string refName)
        {
            string str = RunGitCmd("ls-tree " + refName + " \"" + filename + "\"");
            return _gitTreeParser.ParseSingle(str);
        }

        public int? GetCommitCount(string parentHash, string childHash)
        {
            string result = RunGitCmd("rev-list " + parentHash + " ^" + childHash + " --count");
            if (int.TryParse(result, out var commitCount))
            {
                return commitCount;
            }

            return null;
        }

        public string GetCommitCountString(string from, string to)
        {
            int? removed = GetCommitCount(from, to);
            int? added = GetCommitCount(to, from);

            if (removed == null || added == null)
            {
                return "";
            }

            if (removed == 0 && added == 0)
            {
                return "=";
            }

            return
                (removed > 0 ? ("-" + removed) : "") +
                (added > 0 ? ("+" + added) : "");
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
                RunExternalCmdDetached("cmd.exe", "/c \"\"" + AppSettings.GitCommand.Replace("git.cmd", "gitk")
                                                              .Replace("bin\\git.exe", "cmd\\gitk")
                                                              .Replace("bin/git.exe", "cmd/gitk") + "\" --branches --tags --remotes\"");
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
                string cmd = termEmuCmds.FirstOrDefault(termEmuCmd => !string.IsNullOrEmpty(ThreadHelper.JoinableTaskFactory.Run(() => RunCmdAsync("which", termEmuCmd))));

                if (string.IsNullOrEmpty(cmd))
                {
                    cmd = "bash";
                    args = "--login -i";
                }

                return RunExternalCmdDetachedShowConsole(cmd, args);
            }
            else
            {
                string shellPath;
                if (PathUtil.TryFindShellPath("git-bash.exe", out shellPath))
                {
                    return RunExternalCmdDetachedShowConsole(shellPath, string.Empty);
                }

                string args;
                if (string.IsNullOrWhiteSpace(bashCommand))
                {
                    args = "--login -i\"";
                }
                else
                {
                    args = "--login -i -c \"" + bashCommand.Replace("\"", "\\\"") + "\"";
                }

                args = "/c \"\"{0}\" " + args;

                if (PathUtil.TryFindShellPath("bash.exe", out shellPath))
                {
                    return RunExternalCmdDetachedShowConsole("cmd.exe", string.Format(args, shellPath));
                }

                if (PathUtil.TryFindShellPath("sh.exe", out shellPath))
                {
                    return RunExternalCmdDetachedShowConsole("cmd.exe", string.Format(args, shellPath));
                }

                return RunExternalCmdDetachedShowConsole("cmd.exe", @"/K echo git bash command not found! :( Please add a folder containing 'bash.exe' to your PATH...");
            }
        }

        public string Init(bool bare, bool shared)
        {
            var result = RunGitCmd(Smart.Format("init{0: --bare|}{1: --shared=all|}", bare, shared));
            WorkingDirGitDir = GitDirectoryResolverInstance.Resolve(WorkingDir);
            return result;
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
            {
                endIndex--;
            }

            var message = new StringBuilder();
            bool notesStart = false;
            for (int i = startIndex; i <= endIndex; i++)
            {
                string line = lines[i];
                if (notesStart)
                {
                    line = "    " + line;
                }

                message.AppendLine(line);
                if (lines[i] == "Notes:")
                {
                    notesStart = true;
                }
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
            var revision = new GitRevision(lines[0])
            {
                TreeGuid = lines[1],
                ParentGuids = lines[2].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries),
                Author = ReEncodeStringFromLossless(lines[3]),
                AuthorEmail = ReEncodeStringFromLossless(lines[4]),
                Committer = ReEncodeStringFromLossless(lines[6]),
                CommitterEmail = ReEncodeStringFromLossless(lines[7]),
                AuthorDate = DateTimeUtils.ParseUnixTime(lines[5]),
                CommitDate = DateTimeUtils.ParseUnixTime(lines[8]),
                MessageEncoding = lines[9]
            };
            if (shortFormat)
            {
                revision.Subject = ReEncodeCommitMessage(lines[10], revision.MessageEncoding);
            }
            else
            {
                string message = ProccessDiffNotes(10, lines);

                // commit message is not reencoded by git when format is given
                revision.Body = ReEncodeCommitMessage(message, revision.MessageEncoding);
                revision.Subject = revision.Body.Substring(0, revision.Body.IndexOfAny(new[] { '\r', '\n' }));
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
            {
                parentsRevisions[i] = GetRevision(parents[i], true);
            }

            return parentsRevisions;
        }

        public string ShowSha1(string sha1)
        {
            return ReEncodeShowString(RunCacheableCmd(AppSettings.GitCommand, "show " + sha1, LosslessEncoding));
        }

        public string DeleteTag(string tagName)
        {
            return RunGitCmd(GitCommandHelpers.DeleteTagCmd(tagName));
        }

        public string GetCurrentCheckout()
        {
            return RunGitCmd("rev-parse HEAD").TrimEnd();
        }

        public bool IsExistingCommitHash(string sha1Fragment, out string fullSha1)
        {
            string revParseResult = RunGitCmd(string.Format("rev-parse --verify --quiet {0}^{{commit}}", sha1Fragment));
            revParseResult = revParseResult.Trim();
            if (revParseResult.IsNotNullOrWhitespace() && revParseResult.StartsWith(sha1Fragment))
            {
                fullSha1 = revParseResult;
                return true;
            }
            else
            {
                fullSha1 = null;
                return false;
            }
        }

        public KeyValuePair<char, string> GetSuperprojectCurrentCheckout()
        {
            if (SuperprojectModule == null)
            {
                return new KeyValuePair<char, string>(' ', "");
            }

            var lines = SuperprojectModule.RunGitCmd("submodule status --cached " + _submodulePath).Split('\n');

            if (lines.Length == 0)
            {
                return new KeyValuePair<char, string>(' ', "");
            }

            string submodule = lines[0];
            if (submodule.Length < 43)
            {
                return new KeyValuePair<char, string>(' ', "");
            }

            var currentCommitGuid = submodule.Substring(1, 40).Trim();
            return new KeyValuePair<char, string>(submodule[0], currentCommitGuid);
        }

        public bool ExistsMergeCommit(string startRev, string endRev)
        {
            if (startRev.IsNullOrEmpty() || endRev.IsNullOrEmpty())
            {
                return false;
            }

            string revisions = RunGitCmd("rev-list --parents --no-walk " + startRev + ".." + endRev);
            string[] revisionsTab = revisions.Split('\n');

            bool IsTwoSha1Hashes(string parents)
            {
                string[] tab = parents.Split(' ');
                return tab.Length > 2 && tab.All(parent => GitRevision.Sha1HashRegex.IsMatch(parent));
            }

            return revisionsTab.Any(IsTwoSha1Hashes);
        }

        public ConfigFile GetSubmoduleConfigFile()
        {
            return new ConfigFile(WorkingDir + ".gitmodules", true);
        }

        public string GetCurrentSubmoduleLocalPath()
        {
            if (SuperprojectModule == null)
            {
                return null;
            }

            Debug.Assert(WorkingDir.StartsWith(SuperprojectModule.WorkingDir), "Submodule working dir should start with super-project's working dir");

            return PathUtil.GetDirectoryName(
                WorkingDir.Substring(SuperprojectModule.WorkingDir.Length).ToPosixPath());
        }

        public string GetSubmoduleFullPath(string localPath)
        {
            if (localPath == null)
            {
                Debug.Fail("No path for submodule - incorrectly parsed status?");
                return "";
            }

            string dir = Path.Combine(WorkingDir, localPath.EnsureTrailingPathSeparator());
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

        public IEnumerable<IGitSubmoduleInfo> GetSubmodulesInfo()
        {
            var lines = ReadGitOutputLines("submodule status");

            string lastLine = null;

            var configFile = GetSubmoduleConfigFile();

            foreach (var line in lines)
            {
                if (line == lastLine)
                {
                    continue;
                }

                lastLine = line;

                if (TryParseSubmoduleInfo(line, out var info))
                {
                    yield return info;
                }
            }

            bool TryParseSubmoduleInfo(string s, out GitSubmoduleInfo info)
            {
                // Parse an output line from `git submodule status`. Lines have one of the following forms:
                //
                //  6f213088cf4343efe4c570d87659b5f87fc05a4b Externals/Git.hub (heads/master)
                // -ed1dbf01e32ffe6c0b84210183cc2ff6ca448717 Externals/NBug (heads/master)
                // +0daff15503915230aa9436c0fee6a95d5bf3273f Externals/conemu-inside (heads/master)
                // U6868f2b4a39fc894c44711c8903407da596acbf5 GitExtensionsDoc (heads/master)
                //
                // The first character of each line is a prefix with the following meanings:
                //
                // - ' ' if the submodule is initialised and has no changes
                // - '-' if the submodule is not initialized
                // - '+' if the currently checked out submodule commit does not match the SHA-1 found in the index of the containing repository
                // - 'U' if the submodule has merge conflicts
                //
                // Then we have:
                //
                // - the SHA-1 of the currently checked out commit of the submodule
                // - the submodule path
                // - the output of git describe for the SHA-1

                var match = Regex.Match(s, @"^([ -+U])([0-9a-f]{40}) (.+) \((.+)\)$");

                if (!match.Success)
                {
                    info = null;
                    return false;
                }

                var code = match.Groups[1].Value[0];
                var currentCommitGuid = match.Groups[2].Value;
                var localPath = match.Groups[3].Value;
                var branch = match.Groups[4].Value;

                var configSection = configFile.ConfigSections.FirstOrDefault(section => section.GetValue("path").Trim() == localPath);

                Trace.Assert(configSection != null, $"`git submodule status` returned submodule \"{localPath}\" that was not found in .gitmodules");

                var name = configSection.SubSection.Trim();
                var remotePath = configFile.GetPathValue($"submodule.{name}.url").Trim();

                info = new GitSubmoduleInfo(
                    name, localPath, remotePath, currentCommitGuid, branch,
                    isInitialized: code != '-',
                    isUpToDate: code != '+');
                return true;
            }
        }

        public string FindGitSuperprojectPath(out string submoduleName, out string submodulePath)
        {
            submoduleName = null;
            submodulePath = null;
            if (!IsValidGitWorkingDir())
            {
                return null;
            }

            string superprojectPath = null;

            string currentPath = Path.GetDirectoryName(WorkingDir); // remove last slash
            if (!string.IsNullOrEmpty(currentPath))
            {
                string path = Path.GetDirectoryName(currentPath);
                for (int i = 0; i < 5; i++)
                {
                    if (string.IsNullOrEmpty(path))
                    {
                        break;
                    }

                    if (File.Exists(Path.Combine(path, ".gitmodules")) &&
                        IsValidGitWorkingDir(path))
                    {
                        superprojectPath = path.EnsureTrailingPathSeparator();
                        break;
                    }

                    // Check upper directory
                    path = Path.GetDirectoryName(path);
                }
            }

            if (File.Exists(WorkingDir + ".git") &&
                superprojectPath == null)
            {
                var lines = File.ReadLines(WorkingDir + ".git");
                foreach (string line in lines)
                {
                    if (line.StartsWith("gitdir:"))
                    {
                        string gitpath = line.Substring(7).Trim();
                        int pos = gitpath.IndexOf("/.git/modules/");
                        if (pos != -1)
                        {
                            gitpath = gitpath.Substring(0, pos + 1).Replace('/', '\\');
                            gitpath = Path.GetFullPath(Path.Combine(WorkingDir, gitpath));
                            if (File.Exists(gitpath + ".gitmodules") && IsValidGitWorkingDir(gitpath))
                            {
                                superprojectPath = gitpath;
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(superprojectPath) && currentPath.StartsWith(superprojectPath))
            {
                submodulePath = currentPath.Substring(superprojectPath.Length).ToPosixPath();
                var configFile = new ConfigFile(superprojectPath + ".gitmodules", true);
                foreach (ConfigSection configSection in configFile.ConfigSections)
                {
                    if (configSection.GetValue("path") == submodulePath.ToPosixPath())
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

        public string ResetSoft(string commit, string file = null)
        {
            var args = new ArgumentBuilder
            {
                "reset --soft",
                commit.QuoteNE(),
                "--",
                file.QuoteNE()
            };

            return RunGitCmd(args.ToString());
        }

        public string ResetMixed(string commit, string file = null)
        {
            var args = new ArgumentBuilder
            {
                "reset --mixed",
                commit.QuoteNE(),
                "--",
                file.QuoteNE()
            };

            return RunGitCmd(args.ToString());
        }

        public string ResetHard(string commit, string file = null)
        {
            var args = new ArgumentBuilder
            {
                "reset --hard",
                commit.QuoteNE(),
                "--",
                file.QuoteNE()
            };

            return RunGitCmd(args.ToString());
        }

        public string ResetFile(string file)
        {
            file = file.ToPosixPath();
            return RunGitCmd("checkout-index --index --force -- \"" + file + "\"");
        }

        /// <summary>
        /// Determines whether the given repository has index.lock file.
        /// </summary>
        /// <returns><see langword="true"/> is index is locked; otherwise <see langword="false"/>.</returns>
        public bool IsIndexLocked()
        {
            return _indexLockManager.IsIndexLocked();
        }

        /// <summary>
        /// Delete index.lock in the current working folder.
        /// </summary>
        /// <param name="includeSubmodules">
        ///     If <see langword="true"/> all submodules will be scanned for index.lock files and have them delete, if found.
        /// </param>
        /// <exception cref="FileDeleteException">Unable to delete specific index.lock.</exception>
        public void UnlockIndex(bool includeSubmodules)
        {
            _indexLockManager.UnlockIndex(includeSubmodules);
        }

        public string FormatPatch(string from, string to, string output, int start)
        {
            output = output.ToPosixPath();

            var result = RunGitCmd("format-patch -M -C -B --start-number " + start + " \"" + from + "\"..\"" + to +
                                "\" -o \"" + output + "\"");

            return result;
        }

        public string FormatPatch(string from, string to, string output)
        {
            output = output.ToPosixPath();

            var result = RunGitCmd("format-patch -M -C -B \"" + from + "\"..\"" + to + "\" -o \"" + output + "\"");

            return result;
        }

        public string CheckoutFiles(IEnumerable<string> fileList, string revision, bool force)
        {
            string files = fileList.Select(s => s.Quote()).Join(" ");
            if (files.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            if (revision == GitRevision.UnstagedGuid)
            {
                Debug.Assert(false, "Unexpectedly reset to unstaged - should be blocked in GUI");

                // Not an error to user, just nothing happens
                return "";
            }

            if (revision == GitRevision.IndexGuid)
            {
                revision = "";
            }
            else
            {
                revision = revision.QuoteNE();
            }

            return RunGitCmd("checkout " + force.AsForce() + revision + " -- " + files);
        }

        public string RemoveFiles(IEnumerable<string> fileList, bool force)
        {
            string files = fileList.Select(s => s.Quote()).Join(" ");
            if (files.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            return RunGitCmd("rm " + force.AsForce() + " -- " + files);
        }

        /// <summary>Tries to start Pageant for the specified remote repo (using the remote's PuTTY key file).</summary>
        /// <returns>true if the remote has a PuTTY key file; otherwise, false.</returns>
        public bool StartPageantForRemote(string remote)
        {
            var sshKeyFile = GetPuttyKeyFileForRemote(remote);
            if (string.IsNullOrEmpty(sshKeyFile) || !File.Exists(sshKeyFile))
            {
                return false;
            }

            StartPageantWithKey(sshKeyFile);
            return true;
        }

        public static void StartPageantWithKey(string sshKeyFile)
        {
            // ensure pageant is loaded, so we can wait for loading a key in the next command
            // otherwise we'll stuck there waiting until pageant exits
            var pageantProcName = Path.GetFileNameWithoutExtension(AppSettings.Pageant);
            if (Process.GetProcessesByName(pageantProcName).Length == 0)
            {
                Process pageantProcess = RunExternalCmdDetached(AppSettings.Pageant, "", "");
                pageantProcess.WaitForInputIdle();
            }

            GitCommandHelpers.RunCmd(AppSettings.Pageant, "\"" + sshKeyFile + "\"");
        }

        public string GetPuttyKeyFileForRemote(string remote)
        {
            if (string.IsNullOrEmpty(remote) ||
                string.IsNullOrEmpty(AppSettings.Pageant) ||
                !AppSettings.AutoStartPageant ||
                !GitCommandHelpers.Plink())
            {
                return "";
            }

            return GetSetting(string.Format("remote.{0}.puttykeyfile", remote));
        }

        public string FetchCmd(string remote, string remoteBranch, string localBranch, bool? fetchTags = false, bool isUnshallow = false, bool prune = false)
        {
            var args = new ArgumentBuilder
            {
                "fetch",
                { GitCommandHelpers.VersionInUse.FetchCanAskForProgress, "--progress" },
                {
                    !string.IsNullOrEmpty(remote) || !string.IsNullOrEmpty(remoteBranch) || !string.IsNullOrEmpty(localBranch),
                    GetFetchArgs(remote, remoteBranch, localBranch, fetchTags, isUnshallow, prune)
                }
            };

            return args.ToString();
        }

        public string PullCmd(string remote, string remoteBranch, bool rebase, bool? fetchTags = false, bool isUnshallow = false, bool prune = false)
        {
            var args = new ArgumentBuilder
            {
                "pull",
                { rebase, "--rebase" },
                { GitCommandHelpers.VersionInUse.FetchCanAskForProgress, "--progress" },
                GetFetchArgs(remote, remoteBranch, null, fetchTags, isUnshallow, prune && !rebase)
            };

            return args.ToString();
        }

        private string GetFetchArgs(string remote, string remoteBranch, string localBranch, bool? fetchTags, bool isUnshallow, bool prune)
        {
            // Remove spaces...
            remoteBranch = remoteBranch?.Replace(" ", "");
            localBranch = localBranch?.Replace(" ", "");

            string branchArguments = "";

            if (!string.IsNullOrEmpty(remoteBranch))
            {
                if (remoteBranch.StartsWith("+"))
                {
                    remoteBranch = remoteBranch.Remove(0, 1);
                }

                branchArguments = "+" + FormatBranchName(remoteBranch);

                if (!string.IsNullOrEmpty(localBranch))
                {
                    branchArguments += ":" + GitRefName.GetFullBranchName(localBranch);
                }
            }

            var args = new ArgumentBuilder
            {
                remote.ToPosixPath().Trim().Quote(),
                branchArguments,
                { fetchTags == true, "--tags" },
                { fetchTags == false, "--no-tags" },
                { isUnshallow, "--unshallow" },
                { prune, "--prune" }
            };

            return args.ToString();
        }

        public string GetRebaseDir()
        {
            string gitDirectory = GetGitDirectory();
            if (Directory.Exists(gitDirectory + "rebase-merge" + Path.DirectorySeparatorChar))
            {
                return gitDirectory + "rebase-merge" + Path.DirectorySeparatorChar;
            }

            if (Directory.Exists(gitDirectory + "rebase-apply" + Path.DirectorySeparatorChar))
            {
                return gitDirectory + "rebase-apply" + Path.DirectorySeparatorChar;
            }

            if (Directory.Exists(gitDirectory + "rebase" + Path.DirectorySeparatorChar))
            {
                return gitDirectory + "rebase" + Path.DirectorySeparatorChar;
            }

            return "";
        }

        /// <summary>Creates a 'git push' command using the specified parameters.</summary>
        /// <param name="remote">Remote repository that is the destination of the push operation.</param>
        /// <param name="force">If a remote ref is not an ancestor of the local ref, overwrite it.
        /// <remarks>This can cause the remote repository to lose commits; use it with care.</remarks></param>
        /// <param name="track">For every branch that is up to date or successfully pushed, add upstream (tracking) reference.</param>
        /// <param name="recursiveSubmodules">If '1', check whether all submodule commits used by the revisions to be pushed are available on a remote tracking branch; otherwise, the push will be aborted.</param>
        /// <returns>'git push' command with the specified parameters.</returns>
        public string PushAllCmd(string remote, ForcePushOptions force, bool track, int recursiveSubmodules)
        {
            // TODO make an enum for RecursiveSubmodulesOption and add to ArgumentBuilderExtensions
            var args = new ArgumentBuilder
            {
                "push",
                force,
                { track, "-u" },
                { recursiveSubmodules == 1, "--recurse-submodules=check" },
                { recursiveSubmodules == 2, "--recurse-submodules=on-demand" },
                { GitCommandHelpers.VersionInUse.PushCanAskForProgress, "--progress" },
                "--all",
                remote.ToPosixPath().Trim().Quote()
            };

            return args.ToString();
        }

        /// <summary>Creates a 'git push' command using the specified parameters.</summary>
        /// <param name="remote">Remote repository that is the destination of the push operation.</param>
        /// <param name="fromBranch">Name of the branch to push.</param>
        /// <param name="toBranch">Name of the ref on the remote side to update with the push.</param>
        /// <param name="force">If a remote ref is not an ancestor of the local ref, overwrite it.
        /// <remarks>This can cause the remote repository to lose commits; use it with care.</remarks></param>
        /// <param name="track">For every branch that is up to date or successfully pushed, add upstream (tracking) reference.</param>
        /// <param name="recursiveSubmodules">If '1', check whether all submodule commits used by the revisions to be pushed are available on a remote tracking branch; otherwise, the push will be aborted.</param>
        /// <returns>'git push' command with the specified parameters.</returns>
        public string PushCmd([NotNull] string remote, [NotNull] string fromBranch, [CanBeNull] string toBranch, ForcePushOptions force, bool track, int recursiveSubmodules)
        {
            // This method is for pushing to remote branches, so fully qualify the
            // remote branch name with refs/heads/.
            fromBranch = FormatBranchName(fromBranch);
            toBranch = GitRefName.GetFullBranchName(toBranch);

            if (string.IsNullOrEmpty(fromBranch) && !string.IsNullOrEmpty(toBranch))
            {
                fromBranch = "HEAD";
            }

            // TODO make an enum for RecursiveSubmodulesOption and add to ArgumentBuilderExtensions
            var args = new ArgumentBuilder
            {
                "push",
                force,
                { track, "-u" },
                { recursiveSubmodules == 1, "--recurse-submodules=check" },
                { recursiveSubmodules == 2, "--recurse-submodules=on-demand" },
                { GitCommandHelpers.VersionInUse.PushCanAskForProgress, "--progress" },
                remote.ToPosixPath().Trim().Quote(),
                { string.IsNullOrEmpty(toBranch), fromBranch },
                { !string.IsNullOrEmpty(toBranch), $"{fromBranch}:{toBranch}" }
            };

            return args.ToString();
        }

        private ProcessStartInfo CreateGitStartInfo(string arguments)
        {
            return GitCommandHelpers.CreateProcessStartInfo(AppSettings.GitCommand, arguments, WorkingDir, SystemEncoding);
        }

        public string ApplyPatch(string dir, string amCommand)
        {
            var startInfo = CreateGitStartInfo(amCommand);

            using (var process = Process.Start(startInfo))
            {
                var files = Directory.GetFiles(dir);

                if (files.Length == 0)
                {
                    return "";
                }

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

        public string AssumeUnchangedFiles(IReadOnlyList<GitItemStatus> files, bool assumeUnchanged, out bool wereErrors)
        {
            var output = "";
            string error = "";
            wereErrors = false;
            var startInfo = CreateGitStartInfo("update-index --" + (assumeUnchanged ? "" : "no-") + "assume-unchanged --stdin");
            var processReader = new Lazy<SynchronizedProcessReader>(() => new SynchronizedProcessReader(Process.Start(startInfo)));

            foreach (var file in files.Where(file => file.IsAssumeUnchanged != assumeUnchanged))
            {
                UpdateIndex(processReader, file.Name);
            }

            if (processReader.IsValueCreated)
            {
                processReader.Value.Process.StandardInput.Close();
                processReader.Value.WaitForExit();
                output = processReader.Value.OutputString(SystemEncoding);
                error = processReader.Value.ErrorString(SystemEncoding);
            }

            return output.Combine(Environment.NewLine, error);
        }

        public string SkipWorktreeFiles(IReadOnlyList<GitItemStatus> files, bool skipWorktree)
        {
            var output = "";
            string error = "";
            var startInfo = CreateGitStartInfo("update-index --" + (skipWorktree ? "" : "no-") + "skip-worktree --stdin");
            var processReader = new Lazy<SynchronizedProcessReader>(() => new SynchronizedProcessReader(Process.Start(startInfo)));

            foreach (var file in files.Where(file => file.IsSkipWorktree != skipWorktree))
            {
                UpdateIndex(processReader, file.Name);
            }

            if (processReader.IsValueCreated)
            {
                processReader.Value.Process.StandardInput.Close();
                processReader.Value.WaitForExit();
                output = processReader.Value.OutputString(SystemEncoding);
                error = processReader.Value.ErrorString(SystemEncoding);
            }

            return output.Combine(Environment.NewLine, error);
        }

        public string StageFiles(IReadOnlyList<GitItemStatus> files, out bool wereErrors)
        {
            var output = "";
            string error = "";
            wereErrors = false;
            var startInfo = CreateGitStartInfo("update-index --add --stdin");
            var processReader = new Lazy<SynchronizedProcessReader>(() => new SynchronizedProcessReader(Process.Start(startInfo)));

            foreach (var file in files.Where(file => !file.IsDeleted))
            {
                UpdateIndex(processReader, file.Name);
            }

            if (processReader.IsValueCreated)
            {
                processReader.Value.Process.StandardInput.Close();
                processReader.Value.WaitForExit();
                wereErrors = processReader.Value.Process.ExitCode != 0;
                output = processReader.Value.OutputString(SystemEncoding);
                error = processReader.Value.ErrorString(SystemEncoding);
            }

            startInfo.Arguments = "update-index --remove --stdin";
            processReader = new Lazy<SynchronizedProcessReader>(() => new SynchronizedProcessReader(Process.Start(startInfo)));
            foreach (var file in files.Where(file => file.IsDeleted))
            {
                UpdateIndex(processReader, file.Name);
            }

            if (processReader.IsValueCreated)
            {
                processReader.Value.Process.StandardInput.Close();
                processReader.Value.WaitForExit();
                output = output.Combine(Environment.NewLine, processReader.Value.OutputString(SystemEncoding));
                error = error.Combine(Environment.NewLine, processReader.Value.ErrorString(SystemEncoding));
                wereErrors = wereErrors || processReader.Value.Process.ExitCode != 0;
            }

            return output.Combine(Environment.NewLine, error);
        }

        public string UnstageFiles(IReadOnlyList<GitItemStatus> files)
        {
            var output = "";
            string error = "";
            var startInfo = CreateGitStartInfo("update-index --info-only --index-info");
            var processReader = new Lazy<SynchronizedProcessReader>(() => new SynchronizedProcessReader(Process.Start(startInfo)));
            foreach (var file in files.Where(file => !file.IsNew))
            {
                processReader.Value.Process.StandardInput.WriteLine("0 0000000000000000000000000000000000000000\t\"" + file.Name.ToPosixPath() + "\"");
            }

            if (processReader.IsValueCreated)
            {
                processReader.Value.Process.StandardInput.Close();
                processReader.Value.WaitForExit();
                output = processReader.Value.OutputString(SystemEncoding);
                error = processReader.Value.ErrorString(SystemEncoding);
            }

            startInfo.Arguments = "update-index --force-remove --stdin";
            processReader = new Lazy<SynchronizedProcessReader>(() => new SynchronizedProcessReader(Process.Start(startInfo)));
            foreach (var file in files.Where(file => file.IsNew))
            {
                UpdateIndex(processReader, file.Name);
            }

            if (processReader.IsValueCreated)
            {
                processReader.Value.Process.StandardInput.Close();
                processReader.Value.WaitForExit();
                output = output.Combine(Environment.NewLine, processReader.Value.OutputString(SystemEncoding));
                error = error.Combine(Environment.NewLine, processReader.Value.ErrorString(SystemEncoding));
            }

            return output.Combine(Environment.NewLine, error);
        }

        private static void UpdateIndex(Lazy<SynchronizedProcessReader> processReader, string filename)
        {
            ////process.StandardInput.WriteLine("\"" + ToPosixPath(file.Name) + "\"");
            byte[] bytearr = EncodingHelper.ConvertTo(SystemEncoding,
                                                      "\"" + filename.ToPosixPath() + "\"" + processReader.Value.Process.StandardInput.NewLine);
            processReader.Value.Process.StandardInput.BaseStream.Write(bytearr, 0, bytearr.Length);
        }

        public bool InTheMiddleOfBisect()
        {
            return File.Exists(Path.Combine(GetGitDirectory(), "BISECT_START"));
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

        public bool InTheMiddleOfAction()
        {
            return InTheMiddleOfConflictedMerge() || InTheMiddleOfRebase();
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
            {
                return str1 + str2;
            }

            Debug.Assert(m1.Groups[1].Value == m2.Groups[1].Value, "m1.Groups[1].Value == m2.Groups[1].Value");
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

        public IReadOnlyList<PatchFile> GetInteractiveRebasePatchFiles()
        {
            string todoFile = GetRebaseDir() + "git-rebase-todo";
            string[] todoCommits = File.Exists(todoFile) ? File.ReadAllText(todoFile).Trim().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries) : null;

            var patchFiles = new List<PatchFile>();

            if (todoCommits != null)
            {
                string commentChar = EffectiveConfigFile.GetString("core.commentChar", "#");

                foreach (string todoCommit in todoCommits)
                {
                    if (todoCommit.StartsWith(commentChar))
                    {
                        continue;
                    }

                    string[] parts = todoCommit.Split(' ');

                    if (parts.Length >= 3)
                    {
                        CommitData data = _commitDataManager.GetCommitData(parts[1], out var error);

                        patchFiles.Add(new PatchFile
                        {
                            Author = error ?? data.Author,
                            Subject = error ?? data.Body,
                            Name = parts[0],
                            Date = error ?? data.CommitDate.LocalDateTime.ToString(),
                            IsNext = patchFiles.Count == 0
                        });
                    }
                }
            }

            return patchFiles;
        }

        public IReadOnlyList<PatchFile> GetRebasePatchFiles()
        {
            var patchFiles = new List<PatchFile>();

            var nextFile = GetNextRebasePatch();

            int.TryParse(nextFile, out var next);

            var files = Directory.Exists(GetRebaseDir())
                ? Directory.GetFiles(GetRebaseDir())
                : Array.Empty<string>();

            foreach (var fullFileName in files)
            {
                var file = PathUtil.GetFileName(fullFileName);
                if (!int.TryParse(file, out var n))
                {
                    continue;
                }

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
                    string value = "";
                    foreach (var line in File.ReadLines(GetRebaseDir() + file))
                    {
                        var m = HeadersMatch.Match(line);
                        if (key == null)
                        {
                            if (!string.IsNullOrWhiteSpace(line) && !m.Success)
                            {
                                continue;
                            }
                        }
                        else if (string.IsNullOrWhiteSpace(line) || m.Success)
                        {
                            value = DecodeString(value);
                            switch (key)
                            {
                                case "From":
                                    if (value.IndexOf('<') > 0 && value.IndexOf('<') < value.Length)
                                    {
                                        var author = RFC2047Decoder.Parse(value);
                                        patchFile.Author = author.Substring(0, author.IndexOf('<')).Trim();
                                    }
                                    else
                                    {
                                        patchFile.Author = value;
                                    }

                                    break;
                                case "Date":
                                    if (value.IndexOf('+') > 0 && value.IndexOf('<') < value.Length)
                                    {
                                        patchFile.Date = value.Substring(0, value.IndexOf('+')).Trim();
                                    }
                                    else
                                    {
                                        patchFile.Date = value;
                                    }

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
                        {
                            value = AppendQuotedString(value, line.Trim());
                        }

                        if (string.IsNullOrEmpty(line) ||
                            (!string.IsNullOrEmpty(patchFile.Author) &&
                            !string.IsNullOrEmpty(patchFile.Date) &&
                            !string.IsNullOrEmpty(patchFile.Subject)))
                        {
                            break;
                        }
                    }
                }

                patchFiles.Add(patchFile);
            }

            return patchFiles;
        }

        public string CommitCmd(bool amend, bool signOff = false, string author = "", bool useExplicitCommitMessage = true, bool noVerify = false, bool gpgSign = false, string gpgKeyId = "")
        {
            var args = new ArgumentBuilder
            {
                "commit",
                { amend, "--amend" },
                { noVerify, "--no-verify" },
                { signOff, "--signoff" },
                { !string.IsNullOrEmpty(author), $"--author=\"{author?.Trim().Trim('"')}\"" },
                { gpgSign && string.IsNullOrWhiteSpace(gpgKeyId), "-S" },
                { gpgSign && !string.IsNullOrWhiteSpace(gpgKeyId), $"-S{gpgKeyId}" },
                { useExplicitCommitMessage, $"-F \"{Path.Combine(GetGitDirectory(), "COMMITMESSAGE")}\"" }
            };

            return args.ToString();
        }

        /// <summary>
        /// Removes the registered remote by running <c>git remote rm</c> command.
        /// </summary>
        /// <param name="remoteName">The remote name.</param>
        public string RemoveRemote(string remoteName)
        {
            return RunGitCmd("remote rm \"" + remoteName + "\"");
        }

        /// <summary>
        /// Renames the registered remote by running <c>git remote rename</c> command.
        /// </summary>
        /// <param name="remoteName">The current remote name.</param>
        /// <param name="newName">The new remote name.</param>
        public string RenameRemote(string remoteName, string newName)
        {
            return RunGitCmd("remote rename \"" + remoteName + "\" \"" + newName + "\"");
        }

        public string RenameBranch(string name, string newName)
        {
            return RunGitCmd("branch -m \"" + name + "\" \"" + newName + "\"");
        }

        public string AddRemote(string name, string path)
        {
            var location = path.ToPosixPath();

            if (string.IsNullOrEmpty(name))
            {
                return "Please enter a name.";
            }

            return
                string.IsNullOrEmpty(location)
                    ? RunGitCmd(string.Format("remote add \"{0}\" \"\"", name))
                    : RunGitCmd(string.Format("remote add \"{0}\" \"{1}\"", name, location));
        }

        public string[] GetRemotes(bool allowEmpty = true)
        {
            string remotes = RunGitCmd("remote show");

            // TODO why allowEmpty? splitting on \n always produces a meaningless blank line at the end
            return allowEmpty
                ? remotes.Split('\n')
                : remotes.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public IEnumerable<string> GetSettings(string setting)
        {
            return LocalConfigFile.GetValues(setting);
        }

        public string GetSetting(string setting)
        {
            return LocalConfigFile.GetValue(setting);
        }

        public string GetEffectiveSetting(string setting)
        {
            return EffectiveConfigFile.GetValue(setting);
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

        public IReadOnlyList<GitStash> GetStashes()
        {
            var lines = RunGitCmd("stash list").Split('\n');

            var stashes = new List<GitStash>(lines.Length);

            foreach (var line in lines)
            {
                if (GitStash.TryParse(line, out var stash))
                {
                    stashes.Add(stash);
                }
            }

            return stashes;
        }

        [CanBeNull]
        public Patch GetSingleDiff(
            [CanBeNull] string firstRevision, [CanBeNull] string secondRevision,
            [CanBeNull] string fileName, [CanBeNull] string oldFileName,
            [NotNull] string extraDiffArguments, [NotNull] Encoding encoding,
            bool cacheResult, bool isTracked = true)
        {
            // fix refs slashes
            fileName = fileName?.ToPosixPath();
            oldFileName = oldFileName?.ToPosixPath();
            firstRevision = firstRevision?.ToPosixPath();
            secondRevision = secondRevision?.ToPosixPath();

            string diffOptions = _revisionDiffProvider.Get(firstRevision, secondRevision, fileName, oldFileName, isTracked);

            var args = new ArgumentBuilder
            {
                DiffCommandWithStandardArgs,
                extraDiffArguments,
                { AppSettings.UsePatienceDiffAlgorithm, "--patience" },
                "-M -C",
                diffOptions
            };

            cacheResult = cacheResult &&
                !secondRevision.IsArtificial() &&
                !firstRevision.IsArtificial() &&
                !secondRevision.IsNullOrEmpty() &&
                !firstRevision.IsNullOrEmpty();

            var patch = cacheResult
                ? RunCacheableCmd(AppSettings.GitCommand, args.ToString(), LosslessEncoding)
                : ThreadHelper.JoinableTaskFactory.Run(() => RunCmdAsync(AppSettings.GitCommand, args.ToString(), LosslessEncoding));

            var patches = PatchProcessor.CreatePatchesFromString(patch, encoding).ToList();

            return GetPatch(patches, fileName, oldFileName);
        }

        [CanBeNull]
        private static Patch GetPatch([NotNull, ItemNotNull] IReadOnlyList<Patch> patches, [CanBeNull] string fileName, [CanBeNull] string oldFileName)
        {
            foreach (Patch p in patches)
            {
                if (fileName == p.FileNameB && (fileName == p.FileNameA || oldFileName == p.FileNameA))
                {
                    return p;
                }
            }

            return patches.Count != 0
                ? patches[patches.Count - 1]
                : null;
        }

        public string GetStatusText(bool untracked)
        {
            string cmd = "status -s";
            if (untracked)
            {
                cmd = cmd + " -u";
            }

            return RunGitCmd(cmd);
        }

        public string GetDiffFilesText(string firstRevision, string secondRevision, bool noCache = false)
        {
            string cmd = DiffCommandWithStandardArgs + "-M -C --name-status " + _revisionDiffProvider.Get(firstRevision, secondRevision);
            return noCache ? RunGitCmd(cmd) : RunCacheableCmd(AppSettings.GitCommand, cmd, SystemEncoding);
        }

        public IReadOnlyList<GitItemStatus> GetDiffFilesWithSubmodulesStatus(string firstRevision, string secondRevision)
        {
            var status = GetDiffFiles(firstRevision, secondRevision);
            GetSubmoduleStatus(status, firstRevision, secondRevision);
            return status;
        }

        public IReadOnlyList<GitItemStatus> GetDiffFiles(string firstRevision, string secondRevision, bool noCache = false)
        {
            noCache = noCache || firstRevision.IsArtificial() || secondRevision.IsArtificial();
            string cmd = DiffCommandWithStandardArgs + "-M -C -z --name-status " + _revisionDiffProvider.Get(firstRevision, secondRevision);
            string result = noCache ? RunGitCmd(cmd) : RunCacheableCmd(AppSettings.GitCommand, cmd, SystemEncoding);
            var resultCollection = GitCommandHelpers.GetAllChangedFilesFromString(this, result, true).ToList();
            if (firstRevision == GitRevision.UnstagedGuid || secondRevision == GitRevision.UnstagedGuid)
            {
                // For unstaged the untracked must be added too
                var files = GetUnstagedFilesWithSubmodulesStatus().Where(item => item.IsNew);
                if (firstRevision == GitRevision.UnstagedGuid)
                {
                    // The file is seen as "deleted" in 'to' revision
                    foreach (var item in files)
                    {
                        item.IsNew = false;
                        item.IsDeleted = true;
                        resultCollection.Add(item);
                    }
                }
                else
                {
                    resultCollection.AddRange(files);
                }
            }

            return resultCollection;
        }

        public IReadOnlyList<GitItemStatus> GetStashDiffFiles(string stashName)
        {
            var resultCollection = GetDiffFiles(stashName + "^", stashName, true).ToList();

            // shows untracked files
            string untrackedTreeHash = RunGitCmd("log " + stashName + "^3 --pretty=format:\"%T\" --max-count=1");
            if (GitRevision.Sha1HashRegex.IsMatch(untrackedTreeHash))
            {
                var files = GetTreeFiles(untrackedTreeHash, true);
                resultCollection.AddRange(files);
            }

            return resultCollection;
        }

        public IReadOnlyList<GitItemStatus> GetTreeFiles(string treeGuid, bool full)
        {
            var tree = GetTree(treeGuid, full);

            var list = tree
                .Select(file => new GitItemStatus
                {
                    IsTracked = true,
                    IsNew = true,
                    IsChanged = false,
                    IsDeleted = false,
                    IsStaged = false,
                    Name = file.Name,
                    TreeGuid = file.Guid
                }).ToList();

            // Doesn't work with removed submodules
            var submodulesList = GetSubmodulesLocalPaths();
            foreach (var item in list)
            {
                if (submodulesList.Contains(item.Name))
                {
                    item.IsSubmodule = true;
                }
            }

            return list;
        }

        public IReadOnlyList<GitItemStatus> GetAllChangedFiles(bool excludeIgnoredFiles = true,
            bool excludeAssumeUnchangedFiles = true, bool excludeSkipWorktreeFiles = true,
            UntrackedFilesMode untrackedFiles = UntrackedFilesMode.Default)
        {
            var status = RunGitCmd(GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles, untrackedFiles));
            var result = GitCommandHelpers.GetAllChangedFilesFromString(this, status).ToList();

            if (!excludeAssumeUnchangedFiles || !excludeSkipWorktreeFiles)
            {
                string lsOutput = RunGitCmd("ls-files -v");
                if (!excludeAssumeUnchangedFiles)
                {
                    result.AddRange(GitCommandHelpers.GetAssumeUnchangedFilesFromString(lsOutput));
                }

                if (!excludeSkipWorktreeFiles)
                {
                    result.AddRange(GitCommandHelpers.GetSkipWorktreeFilesFromString(lsOutput));
                }
            }

            return result;
        }

        public IReadOnlyList<GitItemStatus> GetAllChangedFilesWithSubmodulesStatus(bool excludeIgnoredFiles = true,
            bool excludeAssumeUnchangedFiles = true, bool excludeSkipWorktreeFiles = true,
            UntrackedFilesMode untrackedFiles = UntrackedFilesMode.Default)
        {
            var status = GetAllChangedFiles(excludeIgnoredFiles, excludeAssumeUnchangedFiles, excludeSkipWorktreeFiles, untrackedFiles);
            GetCurrentSubmoduleStatus(status);
            return status;
        }

        private void GetCurrentSubmoduleStatus(IReadOnlyList<GitItemStatus> status)
        {
            foreach (var item in status)
            {
                if (item.IsSubmodule)
                {
                    var localItem = item;
                    localItem.SetSubmoduleStatus(ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                    {
                        await TaskScheduler.Default.SwitchTo(alwaysYield: true);

                        var submoduleStatus = GitCommandHelpers.GetCurrentSubmoduleChanges(this, localItem.Name, localItem.OldName, localItem.IsStaged);
                        if (submoduleStatus != null && submoduleStatus.Commit != submoduleStatus.OldCommit)
                        {
                            var submodule = submoduleStatus.GetSubmodule(this);
                            submoduleStatus.CheckSubmoduleStatus(submodule);
                        }

                        return submoduleStatus;
                    }));
                }
            }
        }

        private void GetSubmoduleStatus(IReadOnlyList<GitItemStatus> status, string firstRevision, string secondRevision)
        {
            status.ForEach(item =>
            {
                if (item.IsSubmodule)
                {
                    item.SetSubmoduleStatus(ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                    {
                        await TaskScheduler.Default.SwitchTo(alwaysYield: true);

                        Patch patch = GetSingleDiff(firstRevision, secondRevision, item.Name, item.OldName, "", SystemEncoding, true);
                        string text = patch != null ? patch.Text : "";
                        var submoduleStatus = GitCommandHelpers.GetSubmoduleStatus(text, this, item.Name);
                        if (submoduleStatus.Commit != submoduleStatus.OldCommit)
                        {
                            var submodule = submoduleStatus.GetSubmodule(this);
                            submoduleStatus.CheckSubmoduleStatus(submodule);
                        }

                        return submoduleStatus;
                    }));
                }
            });
        }

        public IReadOnlyList<GitItemStatus> GetStagedFiles()
        {
            string status = RunGitCmd(DiffCommandWithStandardArgs + "-M -C -z --cached --name-status", SystemEncoding);

            if (status.Length < 50 && status.Contains("fatal: No HEAD commit to compare"))
            {
                // This command is a little more expensive because it will return both staged and unstaged files
                string command = GitCommandHelpers.GetAllChangedFilesCmd(true, UntrackedFilesMode.No);
                status = RunGitCmd(command, SystemEncoding);
                IReadOnlyList<GitItemStatus> stagedFiles = GitCommandHelpers.GetAllChangedFilesFromString(this, status, false);
                return stagedFiles.Where(f => f.IsStaged).ToList();
            }

            return GitCommandHelpers.GetAllChangedFilesFromString(this, status, true);
        }

        public IReadOnlyList<GitItemStatus> GetStagedFilesWithSubmodulesStatus()
        {
            var status = GetStagedFiles();
            GetCurrentSubmoduleStatus(status);
            return status;
        }

        public IReadOnlyList<GitItemStatus> GetUnstagedFiles()
        {
            return GetAllChangedFiles().Where(x => !x.IsStaged).ToArray();
        }

        public IReadOnlyList<GitItemStatus> GetUnstagedFilesWithSubmodulesStatus()
        {
            return GetAllChangedFilesWithSubmodulesStatus().Where(x => !x.IsStaged).ToArray();
        }

        public IReadOnlyList<GitItemStatus> GitStatus(UntrackedFilesMode untrackedFilesMode, IgnoreSubmodulesMode ignoreSubmodulesMode = IgnoreSubmodulesMode.None)
        {
            string command = GitCommandHelpers.GetAllChangedFilesCmd(true, untrackedFilesMode, ignoreSubmodulesMode);
            string status = RunGitCmd(command);
            return GitCommandHelpers.GetAllChangedFilesFromString(this, status);
        }

        /// <summary>Indicates whether there are any changes to the repository,
        ///  including any untracked files or directories; excluding submodules.</summary>
        public bool IsDirtyDir()
        {
            return GitStatus(UntrackedFilesMode.All, IgnoreSubmodulesMode.All).Count > 0;
        }

        public Patch GetCurrentChanges(string fileName, string oldFileName, bool staged, string extraDiffArguments, Encoding encoding)
        {
            var args = new ArgumentBuilder
            {
                DiffCommandWithStandardArgs,
                { staged, "-M -C --cached" },
                extraDiffArguments,
                { AppSettings.UsePatienceDiffAlgorithm, "--patience" },
                "--",
                fileName.ToPosixPath().Quote(),
                { staged, oldFileName?.ToPosixPath().Quote() }
            };

            string result = RunGitCmd(args.ToString(), LosslessEncoding);
            var patches = PatchProcessor.CreatePatchesFromString(result, encoding).ToList();

            return GetPatch(patches, fileName, oldFileName);
        }

        private string GetFileContents(string path)
        {
            var contents = RunGitCmdResult(string.Format("show HEAD:\"{0}\"", path.ToPosixPath()));
            if (contents.ExitCode == 0)
            {
                return contents.StdOutput;
            }

            return null;
        }

        public string GetFileContents(GitItemStatus file)
        {
            var contents = new StringBuilder();

            string currentContents = GetFileContents(file.Name);
            if (currentContents != null)
            {
                contents.Append(currentContents);
            }

            if (file.OldName != null)
            {
                string oldContents = GetFileContents(file.OldName);
                if (oldContents != null)
                {
                    contents.Append(oldContents);
                }
            }

            return contents.Length > 0 ? contents.ToString() : null;
        }

        public string StageFile(string file)
        {
            return RunGitCmd("update-index --add" + " \"" + file.ToPosixPath() + "\"");
        }

        public string StageFileToRemove(string file)
        {
            return RunGitCmd("update-index --remove" + " \"" + file.ToPosixPath() + "\"");
        }

        public string UnstageFile(string file)
        {
            return RunGitCmd("rm --cached \"" + file.ToPosixPath() + "\"");
        }

        public string UnstageFileToRemove(string file)
        {
            return RunGitCmd("reset HEAD -- \"" + file.ToPosixPath() + "\"");
        }

        /// <summary>Dirty but fast. This sometimes fails.</summary>
        public static string GetSelectedBranchFast(string repositoryPath)
        {
            if (string.IsNullOrEmpty(repositoryPath))
            {
                return string.Empty;
            }

            // eg. "/path/to/repo/.git/HEAD"
            var headFileName = Path.Combine(GetGitDirectory(repositoryPath), "HEAD");

            if (!File.Exists(headFileName))
            {
                return string.Empty;
            }

            var headFileContents = File.ReadAllText(headFileName, SystemEncoding);

            // eg. "ref: refs/heads/master"
            //     "9601551c564b48208bccd50b705264e9bd68140d"

            if (!headFileContents.StartsWith("ref: "))
            {
                return DetachedHeadParser.DetachedBranch;
            }

            const string prefix = "ref: refs/heads/";

            if (!headFileContents.StartsWith(prefix))
            {
                return string.Empty;
            }

            return headFileContents.Substring(prefix.Length).TrimEnd();
        }

        /// <summary>Gets the current branch; or "(no branch)" if HEAD is detached.</summary>
        public string GetSelectedBranch(string repositoryPath)
        {
            string head = GetSelectedBranchFast(repositoryPath);

            if (string.IsNullOrEmpty(head))
            {
                var result = RunGitCmdResult("symbolic-ref HEAD");
                if (result.ExitCode == 1)
                {
                    return DetachedHeadParser.DetachedBranch;
                }

                return result.StdOutput;
            }

            return head;
        }

        /// <summary>Gets the current branch; or "(no branch)" if HEAD is detached.</summary>
        public string GetSelectedBranch()
        {
            return GetSelectedBranch(WorkingDir);
        }

        /// <summary>Indicates whether HEAD is not pointing to a branch.</summary>
        public bool IsDetachedHead()
        {
            return DetachedHeadParser.IsDetachedHead(GetSelectedBranch());
        }

        /// <summary>Gets the remote of the current branch; or "" if no remote is configured.</summary>
        public string GetCurrentRemote()
        {
            string remote = GetSetting(string.Format(SettingKeyString.BranchRemote, GetSelectedBranch()));
            return remote;
        }

        /// <summary>Gets the remote branch of the specified local branch; or "" if none is configured.</summary>
        public string GetRemoteBranch(string branch)
        {
            string remote = GetSetting(string.Format(SettingKeyString.BranchRemote, branch));
            string merge = GetSetting(string.Format("branch.{0}.merge", branch));
            if (string.IsNullOrEmpty(remote) || string.IsNullOrEmpty(merge))
            {
                return "";
            }

            return remote + "/" + (merge.StartsWith("refs/heads/") ? merge.Substring(11) : merge);
        }

        public IEnumerable<IGitRef> GetRemoteBranches()
        {
            return GetRefs().Where(r => r.IsRemote);
        }

        public RemoteActionResult<IReadOnlyList<IGitRef>> GetRemoteServerRefs(string remote, bool tags, bool branches)
        {
            var result = new RemoteActionResult<IReadOnlyList<IGitRef>>
            {
                AuthenticationFail = false,
                HostKeyFail = false,
                Result = null
            };

            remote = remote.ToPosixPath();

            result.CmdResult = RunLsRemote();

            var output = result.CmdResult.StdOutput;

            // If the authentication failed because of a missing key, ask the user to supply one.
            if (output.Contains("FATAL ERROR") && output.Contains("authentication"))
            {
                result.AuthenticationFail = true;
            }
            else if (output.ToLower().Contains("the server's host key is not cached in the registry"))
            {
                result.HostKeyFail = true;
            }
            else if (result.CmdResult.ExitedSuccessfully)
            {
                result.Result = ParseRefs(output);
            }

            return result;

            CmdResult RunLsRemote()
            {
                if (tags && branches)
                {
                    return RunGitCmdResult($"ls-remote --heads --tags \"{remote}\"");
                }

                if (tags)
                {
                    return RunGitCmdResult($"ls-remote --tags \"{remote}\"");
                }

                if (branches)
                {
                    return RunGitCmdResult($"ls-remote --heads \"{remote}\"");
                }

                return new CmdResult();
            }
        }

        public IReadOnlyList<IGitRef> GetRefs(bool tags = true, bool branches = true)
        {
            var refList = GetRefList();

            return ParseRefs(refList);

            string GetRefList()
            {
                if (tags && branches)
                {
                    return RunGitCmd("show-ref --dereference", SystemEncoding);
                }

                if (tags)
                {
                    return RunGitCmd("show-ref --tags", SystemEncoding);
                }

                if (branches)
                {
                    return RunGitCmd(@"for-each-ref --sort=-committerdate refs/heads/ --format=""%(objectname) %(refname)""", SystemEncoding);
                }

                return "";
            }
        }

        /// <param name="option">Ordery by date is slower.</param>
        public IReadOnlyList<IGitRef> GetTagRefs(GetTagRefsSortOrder option)
        {
            var list = GetRefs(true, false);

            List<IGitRef> sortedList;
            if (option == GetTagRefsSortOrder.ByCommitDateAscending)
            {
                sortedList = list.OrderBy(head =>
                {
                    var r = new GitRevision(head.Guid);
                    return r.CommitDate;
                }).ToList();
            }
            else if (option == GetTagRefsSortOrder.ByCommitDateDescending)
            {
                sortedList = list.OrderByDescending(head =>
                {
                    var r = new GitRevision(head.Guid);
                    return r.CommitDate;
                }).ToList();
            }
            else
            {
                sortedList = new List<IGitRef>(list);
            }

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

        public IReadOnlyList<string> GetMergedBranches(bool includeRemote = false)
        {
            return RunGitCmd(GitCommandHelpers.MergedBranches(includeRemote)).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public IReadOnlyList<string> GetMergedRemoteBranches()
        {
            const string remoteBranchPrefixForMergedBranches = "remotes/";
            const string refsPrefix = "refs/";

            string[] mergedBranches = RunGitCmd(GitCommandHelpers.MergedBranches(includeRemote: true)).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var remotes = GetRemotes(allowEmpty: false);

            return mergedBranches
                .Select(b => b.Trim())
                .Where(b => b.StartsWith(remoteBranchPrefixForMergedBranches))
                .Select(b => string.Concat(refsPrefix, b))
                .Where(b => !string.IsNullOrEmpty(GitRefName.GetRemoteName(b, remotes)))
                .ToList();
        }

        [NotNull, ItemNotNull]
        public IReadOnlyList<IGitRef> ParseRefs([NotNull] string refList)
        {
            // Parse lines of format:
            //
            // 69a7c7a40230346778e7eebed809773a6bc45268 refs/heads/master
            // 69a7c7a40230346778e7eebed809773a6bc45268 refs/remotes/origin/master
            // 366dfba1abf6cb98d2934455713f3d190df2ba34 refs/tags/2.51
            //
            // Lines may also use \t as a column delimiter, such as output of "ls-remote --heads origin".

            var regex = new Regex(@"^(?<objectid>[0-9a-f]{40})[ \t](?<refname>.+)$", RegexOptions.Multiline);

            var matches = regex.Matches(refList);

            var gitRefs = new List<IGitRef>();
            var headByRemote = new Dictionary<string, GitRef>();

            foreach (Match match in matches)
            {
                var refName = match.Groups["refname"].Value;
                var objectId = match.Groups["objectid"].Value;
                var remoteName = GitRefName.GetRemoteName(refName);
                var head = new GitRef(this, objectId, refName, remoteName);

                if (GitRefName.IsRemoteHead(refName))
                {
                    headByRemote[remoteName] = head;
                }
                else
                {
                    gitRefs.Add(head);
                }
            }

            // do not show default head if remote has a branch on the same commit
            foreach (var gitRef in gitRefs)
            {
                if (headByRemote.TryGetValue(gitRef.Remote, out var defaultHead) &&
                    gitRef.Guid == defaultHead.Guid)
                {
                    headByRemote.Remove(gitRef.Remote);
                }
            }

            gitRefs.AddRange(headByRemote.Values);

            return gitRefs;
        }

        /// <summary>Gets the branch names, with the active branch, if applicable, listed first.
        /// The active branch will be indicated by a "*", so ensure to Trim before processing.</summary>
        public IEnumerable<string> GetBranchNames()
        {
            return RunGitCmd("branch", SystemEncoding)
                .Split(LineSeparator)
                .Where(branch => !string.IsNullOrWhiteSpace(branch)) // first is ""
                .OrderByDescending(branch => branch.Contains(ActiveBranchIndicator)) // * for current branch
                .ThenBy(r => r)
                .Select(line => line.Trim());
        }

        /// <summary>
        /// Gets branches which contain the given commit.
        /// If both local and remote branches are requested, remote branches are prefixed with "remotes/"
        /// (as returned by git branch -a)
        /// </summary>
        /// <param name="sha1">The sha1.</param>
        /// <param name="getLocal">Pass true to include local branches</param>
        /// <param name="getRemote">Pass true to include remote branches</param>
        public IEnumerable<string> GetAllBranchesWhichContainGivenCommit(string sha1, bool getLocal, bool getRemote)
        {
            if (!getLocal && !getRemote)
            {
                return Enumerable.Empty<string>();
            }

            var args = new ArgumentBuilder
            {
                "branch",
                { getRemote && getLocal, "-a" },
                { getRemote && !getLocal, "-r" },
                $"--contains {sha1}"
            };

            string info = RunGitCmd(args.ToString());

            if (IsGitErrorMessage(info))
            {
                return Enumerable.Empty<string>();
            }

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
        public IEnumerable<string> GetAllTagsWhichContainGivenCommit(string sha1)
        {
            string info = RunGitCmd("tag --contains " + sha1, SystemEncoding);

            if (IsGitErrorMessage(info))
            {
                return Enumerable.Empty<string>();
            }

            return info.Split(new[] { '\r', '\n', '*', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Returns tag's message. If the lightweight tag is passed, corresponding commit message
        /// is returned.
        /// </summary>
        public string GetTagMessage(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return null;
            }

            tag = tag.Trim();

            string info = RunGitCmd("tag -l -n10 " + tag, SystemEncoding);

            if (IsGitErrorMessage(info))
            {
                return null;
            }

            if (!info.StartsWith(tag))
            {
                return null;
            }

            info = info.Substring(tag.Length).Trim();
            if (info.Length == 0)
            {
                return null;
            }

            return info;
        }

        /// <summary>
        /// Returns list of filenames which would be ignored
        /// </summary>
        /// <param name="ignorePatterns">Patterns to ignore (.gitignore syntax)</param>
        public IReadOnlyList<string> GetIgnoredFiles(IEnumerable<string> ignorePatterns)
        {
            var notEmptyPatterns = ignorePatterns
                .Where(pattern => !pattern.IsNullOrWhiteSpace())
                .ToList();

            if (notEmptyPatterns.Count() != 0)
            {
                var excludeParams =
                    notEmptyPatterns
                    .Select(pattern => "-x " + pattern.Quote())
                    .Join(" ");

                // filter duplicates out of the result because options -c and -m may return
                // same files at times
                return RunGitCmd("ls-files -z -o -m -c -i " + excludeParams)
                    .Split(new[] { '\0', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Distinct()
                    .ToList();
            }
            else
            {
                return new string[] { };
            }
        }

        public IReadOnlyList<string> GetFullTree(string id)
        {
            string tree = RunCacheableCmd(AppSettings.GitCommand, string.Format("ls-tree -z -r --name-only {0}", id), SystemEncoding);
            return tree.Split('\0', '\n');
        }

        public IEnumerable<IGitItem> GetTree(string id, bool full)
        {
            var args = new ArgumentBuilder
            {
                "ls-tree",
                "-z",
                { full, "-r" },
                id.Quote()
            };

            var tree = GitRevision.IsFullSha1Hash(id)
                ? RunCacheableCmd(AppSettings.GitCommand, args.ToString(), SystemEncoding)
                : ThreadHelper.JoinableTaskFactory.Run(() => RunCmdAsync(AppSettings.GitCommand, args.ToString(), SystemEncoding));

            return _gitTreeParser.Parse(tree);
        }

        public GitBlame Blame(string fileName, string from, Encoding encoding, string lines = null)
        {
            var args = new ArgumentBuilder
            {
                "blame",
                "--porcelain",
                { AppSettings.DetectCopyInFileOnBlame, "-M" },
                { AppSettings.DetectCopyInAllOnBlame, "-C" },
                { AppSettings.IgnoreWhitespaceOnBlame, "-w" },
                "-l",
                { lines != null, $"-L {lines}" },
                from.ToPosixPath().Quote(),
                "--",
                fileName.ToPosixPath().Quote()
            };

            var output = RunCacheableCmd(AppSettings.GitCommand, args.ToString(), LosslessEncoding);

            try
            {
                return ParseGitBlame(output, encoding);
            }
            catch
            {
                // Catch all parser errors, and ignore them all!
                // We should never get here...
                AppSettings.GitLog.Log("Error parsing output from command: " + args + "\n\nPlease report a bug!", DateTime.Now, DateTime.Now);

                return new GitBlame(Array.Empty<GitBlameLine>());
            }
        }

        internal GitBlame ParseGitBlame(string output, Encoding encoding)
        {
            // "git blame --porcelain" produces a stable, machine-readable format which we parse here.
            // Each line in the file is associated with a commit, and one commit may originate many lines
            // in the file, both in contiguous chunks and disparate ranges.
            //
            // The command output walks the file from top to bottom, line by line.
            // Each time a new commit is encountered, its 'header' is presented.
            // Later chunks from the same commit will refer back to this header by
            // the object ID (SHA-1).
            //
            // After this header, the lines from the file are presented, indented.
            //
            // Here is an excerpt from the middle of GE's README.md file:

            // e3268019c66da7534414e9562ececdee5d455b1b 6 6 1
            // author RussKie
            // author-mail <russkie@gmail.com>
            // author-time 1510573702
            // author-tz +1100
            // committer RussKie
            // committer-mail <russkie@gmail.com>
            // committer-time 1510573702
            // committer-tz +1100
            // summary chore: Reformat readme
            // previous cd032fd48a6693b080f38d87388bc0f601db4e02 README.md
            // filename README.md
            //         ## Overview
            // 957ff3ce9193fec3bd2578378e71676841804935 4 7 1
            //
            // e3268019c66da7534414e9562ececdee5d455b1b 8 8 1
            //         Git Extensions is a standalone UI tool for managing git repositories.<br />

            // Where one commit is responsible for multiple contiguous lines, they appear as follows:

            // e3268019c66da7534414e9562ececdee5d455b1b 11 13 9
            //         <table>
            // e3268019c66da7534414e9562ececdee5d455b1b 12 14
            //           <tr>
            // e3268019c66da7534414e9562ececdee5d455b1b 13 15
            //             <th>&nbsp;</th>
            // e3268019c66da7534414e9562ececdee5d455b1b 14 16
            //             <th>Windows</th>
            // e3268019c66da7534414e9562ececdee5d455b1b 15 17
            //             <th>Linux/Mac</th>
            // e3268019c66da7534414e9562ececdee5d455b1b 16 18
            //           </tr>
            // e3268019c66da7534414e9562ececdee5d455b1b 17 19
            //           <tr>
            // e3268019c66da7534414e9562ececdee5d455b1b 18 20
            //             <td>Requirement</td>
            // e3268019c66da7534414e9562ececdee5d455b1b 19 21

            // On the first line here, there are three trailing numbers. The third is the number of
            // lines from the one commit.

            // There are three 'chunks' here. The first includes a commit header.
            // The second and third refer back to a header that would have been presented earlier.
            // We see the content of the chunks, where the first is a markdown header, the second
            // is a blank line, and third is an introductory paragraph about the project.

            var commitByObjectId = new Dictionary<string, GitBlameCommit>();
            var lines = new List<GitBlameLine>(capacity: 256);

            var headerRegex = new Regex(@"^(?<objectid>[0-9a-f]{40}) (?<origlinenum>\d+) (?<finallinenum>\d+)", RegexOptions.Compiled);

            bool hasCommitHeader;
            string objectId;
            int finalLineNumber;
            int originLineNumber;
            string author;
            string authorMail;
            DateTime authorTime;
            string authorTimeZone;
            string committer;
            string committerMail;
            DateTime committerTime;
            string committerTimeZone;
            string summary;
            string filename;

            Reset();

            foreach (var line in output.Split('\n').Select(l => l.TrimEnd('\r')))
            {
                var match = headerRegex.Match(line);

                if (match.Success)
                {
                    objectId = match.Groups["objectid"].Value;
                    finalLineNumber = int.Parse(match.Groups["finallinenum"].Value);
                    originLineNumber = int.Parse(match.Groups["origlinenum"].Value);
                }
                else if (line.StartsWith("\t"))
                {
                    // The contents of the actual line is output after the above header, prefixed by a TAB. This is to allow adding more header elements later.
                    var text = ReEncodeStringFromLossless(line.Substring(1), encoding);

                    GitBlameCommit commit;
                    if (hasCommitHeader)
                    {
                        commit = new GitBlameCommit(
                            objectId,
                            author,
                            authorMail,
                            authorTime,
                            authorTimeZone,
                            committer,
                            committerMail,
                            committerTime,
                            committerTimeZone,
                            summary,
                            filename);
                        commitByObjectId[objectId] = commit;
                    }
                    else
                    {
                        commit = commitByObjectId[objectId];
                    }

                    lines.Add(new GitBlameLine(commit, finalLineNumber, originLineNumber, text));

                    // Start a new header
                    Reset();
                }
                else if (line.StartsWith("author "))
                {
                    author = ReEncodeStringFromLossless(line.Substring("author ".Length));
                    hasCommitHeader = true;
                }
                else if (line.StartsWith("author-mail "))
                {
                    authorMail = ReEncodeStringFromLossless(line.Substring("author-mail ".Length));
                    hasCommitHeader = true;
                }
                else if (line.StartsWith("author-time "))
                {
                    authorTime = DateTimeUtils.ParseUnixTime(line.Substring("author-time ".Length));
                    hasCommitHeader = true;
                }
                else if (line.StartsWith("author-tz "))
                {
                    authorTimeZone = line.Substring("author-tz ".Length);
                    hasCommitHeader = true;
                }
                else if (line.StartsWith("committer "))
                {
                    committer = ReEncodeStringFromLossless(line.Substring("committer ".Length));
                    hasCommitHeader = true;
                }
                else if (line.StartsWith("committer-mail "))
                {
                    committerMail = line.Substring("committer-mail ".Length);
                    hasCommitHeader = true;
                }
                else if (line.StartsWith("committer-time "))
                {
                    committerTime = DateTimeUtils.ParseUnixTime(line.Substring("committer-time ".Length));
                    hasCommitHeader = true;
                }
                else if (line.StartsWith("committer-tz "))
                {
                    committerTimeZone = line.Substring("committer-tz ".Length);
                    hasCommitHeader = true;
                }
                else if (line.StartsWith("summary "))
                {
                    summary = ReEncodeStringFromLossless(line.Substring("summary ".Length));
                    hasCommitHeader = true;
                }
                else if (line.StartsWith("filename "))
                {
                    filename = ReEncodeFileNameFromLossless(line.Substring("filename ".Length));
                    hasCommitHeader = true;
                }
            }

            return new GitBlame(lines);

            void Reset()
            {
                hasCommitHeader = false;
                objectId = null;
                finalLineNumber = -1;
                originLineNumber = -1;
                author = null;
                authorMail = null;
                authorTime = DateTime.MinValue;
                authorTimeZone = null;
                committer = null;
                committerMail = null;
                committerTime = DateTime.MinValue;
                committerTimeZone = null;
                summary = null;
                filename = null;
            }
        }

        public string GetFileText(string id, Encoding encoding)
        {
            return RunCacheableCmd(AppSettings.GitCommand, "cat-file blob \"" + id + "\"", encoding);
        }

        public string GetFileBlobHash(string fileName, string revision)
        {
            if (revision == GitRevision.UnstagedGuid)
            {
                // working directory changes
                Debug.Assert(false, "Tried to get blob for unstaged file");
                return null;
            }

            if (revision == GitRevision.IndexGuid)
            {
                // index
                string blob = RunGitCmd(string.Format("ls-files -s \"{0}\"", fileName));
                string[] s = blob.Split(' ', '\t');
                if (s.Length >= 2)
                {
                    return s[1];
                }
            }
            else
            {
                string blob = RunGitCmd(string.Format("ls-tree -r {0} \"{1}\"", revision, fileName));
                string[] s = blob.Split(' ', '\t');
                if (s.Length >= 3)
                {
                    return s[2];
                }
            }

            return string.Empty;
        }

        public Stream GetFileStream(string blob)
        {
            try
            {
                var newStream = new MemoryStream();

                using (var process = RunGitCmdDetached("cat-file blob " + blob))
                {
                    process.StandardOutput.BaseStream.CopyTo(newStream);
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

        public IEnumerable<string> GetPreviousCommitMessages(int count, string revision = "HEAD")
        {
            const string sep = "d3fb081b9000598e658da93657bf822cc87b2bf6";
            string output = RunGitCmd("log -n " + count + " " + revision + " --pretty=format:" + sep + "%e%n%s%n%n%b", LosslessEncoding);
            string[] messages = output.Split(new[] { sep }, StringSplitOptions.RemoveEmptyEntries);

            if (messages.Length == 0)
            {
                return new[] { string.Empty };
            }

            return messages.Select(cm =>
                {
                    int idx = cm.IndexOf("\n");
                    string encodingName = cm.Substring(0, idx);
                    cm = cm.Substring(idx + 1, cm.Length - idx - 1);
                    cm = ReEncodeCommitMessage(cm, encodingName);
                    return cm;
                });
        }

        public string OpenWithDifftool(string filename, string oldFileName = "", string firstRevision = GitRevision.IndexGuid, string secondRevision = GitRevision.UnstagedGuid, string extraDiffArguments = null, bool isTracked = true)
        {
            var args = new ArgumentBuilder
            {
                "difftool --gui --no-prompt",
                extraDiffArguments,
                _revisionDiffProvider.Get(firstRevision, secondRevision, filename, oldFileName, isTracked)
            };

            RunGitCmdDetached(args.ToString());

            // This method is supposed to return an error message, but the detached process is untracked
            // TODO track the process somehow, so errors can be reported
            return "";
        }

        public string RevParse(string revisionExpression)
        {
            string revparseCommand = string.Format("rev-parse \"{0}~0\"", revisionExpression);
            var result = RunGitCmdResult(revparseCommand);
            return result.ExitCode == 0 ? result.StdOutput.Split('\n')[0] : "";
        }

        public string GetMergeBase(string a, string b)
        {
            return RunGitCmd("merge-base " + a + " " + b).TrimEnd();
        }

        public SubmoduleStatus CheckSubmoduleStatus(string commit, string oldCommit, CommitData data, CommitData olddata, bool loaddata = false)
        {
            if (!IsValidGitWorkingDir() || oldCommit == null)
            {
                return SubmoduleStatus.NewSubmodule;
            }

            if (commit == null || commit == oldCommit)
            {
                return SubmoduleStatus.Unknown;
            }

            string baseCommit = GetMergeBase(commit, oldCommit);
            if (baseCommit == oldCommit)
            {
                return SubmoduleStatus.FastForward;
            }
            else if (baseCommit == commit)
            {
                return SubmoduleStatus.Rewind;
            }

            if (loaddata)
            {
                olddata = _commitDataManager.GetCommitData(oldCommit, out _);
            }

            if (olddata == null)
            {
                return SubmoduleStatus.NewSubmodule;
            }

            if (loaddata)
            {
                data = _commitDataManager.GetCommitData(commit, out _);
            }

            if (data == null)
            {
                return SubmoduleStatus.Unknown;
            }

            if (data.CommitDate > olddata.CommitDate)
            {
                return SubmoduleStatus.NewerTime;
            }
            else if (data.CommitDate < olddata.CommitDate)
            {
                return SubmoduleStatus.OlderTime;
            }
            else if (data.CommitDate == olddata.CommitDate)
            {
                return SubmoduleStatus.SameTime;
            }

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
        /// <returns>true if <paramref name="branchName"/> is valid reference name, otherwise false.</returns>
        public bool CheckBranchFormat([NotNull] string branchName)
        {
            if (branchName == null)
            {
                throw new ArgumentNullException(nameof(branchName));
            }

            if (branchName.IsNullOrWhiteSpace())
            {
                return false;
            }

            branchName = branchName.Replace("\"", "\\\"");

            var result = RunGitCmdResult(string.Format("check-ref-format --branch \"{0}\"", branchName));
            return result.ExitCode == 0;
        }

        /// <summary>
        /// Format branch name, check if name is valid for repository.
        /// </summary>
        /// <param name="branchName">Branch name to test.</param>
        /// <returns>Well formed branch name.</returns>
        [NotNull]
        private string FormatBranchName([NotNull] string branchName)
        {
            if (branchName == null)
            {
                throw new ArgumentNullException(nameof(branchName));
            }

            string fullBranchName = GitRefName.GetFullBranchName(branchName);
            if (string.IsNullOrEmpty(RevParse(fullBranchName)))
            {
                fullBranchName = branchName;
            }

            return fullBranchName;
        }

        public bool IsRunningGitProcess()
        {
            if (_indexLockManager.IsIndexLocked())
            {
                return true;
            }

            if (EnvUtils.RunningOnWindows())
            {
                return Process.GetProcessesByName("git").Length > 0;
            }

            // Get processes by "ps" command.
            var cmd = Path.Combine(AppSettings.GitBinDir, "ps");
            const string arguments = "x";
            var output = ThreadHelper.JoinableTaskFactory.Run(() => RunCmdAsync(cmd, arguments));
            var lines = output.Split('\n');
            if (lines.Length >= 2)
            {
                return false;
            }

            var headers = lines[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var commandIndex = Array.IndexOf(headers, "COMMAND");
            for (int i = 1; i < lines.Length; i++)
            {
                var columns = lines[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (commandIndex < columns.Length)
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

        private static readonly Regex _escapedOctalCodePointRegex = new Regex(@"(\\([0-7]{3}))+", RegexOptions.Compiled);

        /// <summary>
        /// Unescapes any octal code points embedded within <paramref name="s"/>.
        /// </summary>
        /// <remarks>
        /// If no portions of <paramref name="s"/> contain escaped data, then <paramref name="s"/> is returned.
        /// <para />
        /// If <paramref name="s"/> is <c>null</c> then an empty string is returned.
        /// </remarks>
        /// <example>
        /// <code>UnescapeOctalCodePoints(@"\353\221\220\353\213\244") == "두다"</code>
        /// </example>
        /// <param name="s">The string to unescape.</param>
        /// <returns>The unescaped string, or <paramref name="s"/> if no escaped values were present, or <c>""</c> if <paramref name="s"/> is <c>null</c>.</returns>
        [ContractAnnotation("s:null=>null")]
        public static string UnescapeOctalCodePoints([CanBeNull] string s)
        {
            if (s == null)
            {
                return null;
            }

            return _escapedOctalCodePointRegex.Replace(
                s,
                match =>
                {
                    try
                    {
                        return SystemEncoding.GetString(
                            match.Groups[2]
                                .Captures.Cast<Capture>()
                                .Select(c => Convert.ToByte(c.Value, 8))
                                .ToArray());
                    }
                    catch (OverflowException)
                    {
                        // Octal values greater than 377 overflow a single byte.
                        // These should not be present in the input string.
                        return match.Value;
                    }
                });
        }

        [ContractAnnotation("fileName:null=>null")]
        [ContractAnnotation("fileName:notnull=>notnull")]
        public static string ReEncodeFileNameFromLossless([CanBeNull] string fileName)
        {
            fileName = ReEncodeStringFromLossless(fileName, SystemEncoding);
            return UnescapeOctalCodePoints(fileName);
        }

        [ContractAnnotation("s:null=>null")]
        [ContractAnnotation("s:notnull=>notnull")]
        public static string ReEncodeString([CanBeNull] string s, [NotNull] Encoding fromEncoding, [NotNull] Encoding toEncoding)
        {
            if (s == null || fromEncoding.HeaderName == toEncoding.HeaderName)
            {
                return s;
            }

            var bytes = fromEncoding.GetBytes(s);
            return toEncoding.GetString(bytes);
        }

        /// <summary>
        /// reencodes string from GitCommandHelpers.LosslessEncoding to toEncoding
        /// </summary>
        [ContractAnnotation("s:null=>null")]
        [ContractAnnotation("s:notnull=>notnull")]
        public static string ReEncodeStringFromLossless([CanBeNull] string s, [CanBeNull] Encoding toEncoding)
        {
            if (toEncoding == null)
            {
                return s;
            }

            return ReEncodeString(s, LosslessEncoding, toEncoding);
        }

        [ContractAnnotation("s:null=>null")]
        [ContractAnnotation("s:notnull=>notnull")]
        public string ReEncodeStringFromLossless([CanBeNull] string s)
        {
            return ReEncodeStringFromLossless(s, LogOutputEncoding);
        }

        // there was a bug: Git before v1.8.4 did not recode commit message when format is given
        // Lossless encoding is used, because LogOutputEncoding might not be lossless and not recoded
        // characters could be replaced by replacement character while reencoding to LogOutputEncoding
        public string ReEncodeCommitMessage(string s, [CanBeNull] string toEncodingName)
        {
            bool isABug = !GitCommandHelpers.VersionInUse.LogFormatRecodesCommitMessage;

            Encoding encoding;
            try
            {
                if (isABug)
                {
                    if (toEncodingName.IsNullOrEmpty())
                    {
                        encoding = Encoding.UTF8;
                    }
                    else if (toEncodingName.Equals(LosslessEncoding.HeaderName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // no recoding is needed
                        encoding = null;
                    }
                    else if (CpEncodingPattern.IsMatch(toEncodingName))
                    {
                        // Encodings written as e.g. "cp1251", which is not a supported encoding string
                        encoding = Encoding.GetEncoding(int.Parse(toEncodingName.Substring(2)));
                    }
                    else
                    {
                        encoding = Encoding.GetEncoding(toEncodingName);
                    }
                }
                else
                {
                    // bug is fixed in Git v1.8.4, Git recodes commit message to LogOutputEncoding
                    encoding = LogOutputEncoding;
                }
            }
            catch (Exception)
            {
                return s + "\n\n! Unsupported commit message encoding: " + toEncodingName + " !";
            }

            return ReEncodeStringFromLossless(s, encoding);
        }

        /// <summary>
        /// header part of show result is encoded in logoutputencoding (including reencoded commit message)
        /// diff part is raw data in file's original encoding
        /// s should be encoded in LosslessEncoding
        /// </summary>
        [ContractAnnotation("s:null=>null")]
        [ContractAnnotation("s:notnull=>notnull")]
        public string ReEncodeShowString(string s)
        {
            if (s.IsNullOrEmpty())
            {
                return s;
            }

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

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj == this)
            {
                return true;
            }

            return obj is GitModule other && Equals(other);
        }

        private bool Equals(GitModule other)
        {
            return
                string.Equals(WorkingDir, other.WorkingDir) &&
                Equals(_superprojectModule, other._superprojectModule);
        }

        public override int GetHashCode()
        {
            return WorkingDir.GetHashCode();
        }

        public override string ToString()
        {
            return WorkingDir;
        }

        public string GetLocalTrackingBranchName(string remoteName, string branch)
        {
            var branchName = remoteName.Length > 0 ? branch.Substring(remoteName.Length + 1) : branch;
            foreach (var section in LocalConfigFile.GetConfigSections())
            {
                if (section.SectionName == "branch" && section.GetValue("remote") == remoteName)
                {
                    var remoteBranch = section.GetValue("merge").Replace("refs/heads/", string.Empty);
                    if (remoteBranch == branchName)
                    {
                        return section.SubSection;
                    }
                }
            }

            return branchName;
        }

        public IReadOnlyList<GitItemStatus> GetCombinedDiffFileList(string shaOfMergeCommit)
        {
            var fileList = RunGitCmd("diff-tree --name-only -z --cc --no-commit-id " + shaOfMergeCommit);

            if (string.IsNullOrWhiteSpace(fileList))
            {
                return Array.Empty<GitItemStatus>();
            }

            var files = fileList.Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);

            return files.Select(
                file => new GitItemStatus
                {
                    IsChanged = true,
                    IsConflict = true,
                    IsTracked = true,
                    IsDeleted = false,
                    IsStaged = false,
                    IsNew = false,
                    Name = file,
                }).ToList();
        }

        public string GetCombinedDiffContent(GitRevision revisionOfMergeCommit, string filePath, string extraArgs, Encoding encoding)
        {
            var args = new ArgumentBuilder
            {
                "diff-tree",
                { AppSettings.OmitUninterestingDiff, "--cc", "-c -p" },
                "--no-commit-id",
                extraArgs,
                revisionOfMergeCommit.Guid,
                { AppSettings.UsePatienceDiffAlgorithm, "--patience" },
                "--",
                filePath
            };

            var patch = RunCacheableCmd(AppSettings.GitCommand, args.ToString(), LosslessEncoding);

            if (string.IsNullOrWhiteSpace(patch))
            {
                return "";
            }

            var patches = PatchProcessor.CreatePatchesFromString(patch, encoding).ToList();

            return GetPatch(patches, filePath, filePath).Text;
        }

        public bool HasLfsSupport()
        {
            return RunGitCmdResult("lfs version").ExitedSuccessfully;
        }

        public bool StopTrackingFile(string filename)
        {
            return RunGitCmdResult("rm --cached " + filename).ExitedSuccessfully;
        }

        /// <summary>
        /// Determines whether a git command's output indicates an error occurred.
        /// </summary>
        /// <param name="gitOutput">The output from the git command, to inspect.</param>
        /// <returns><c>true</c> if the command detailed an error, otherwise <c>false</c>.</returns>
        public static bool IsGitErrorMessage(string gitOutput)
        {
            return Regex.IsMatch(gitOutput, @"^\s*(error:|fatal)");
        }
    }
}
