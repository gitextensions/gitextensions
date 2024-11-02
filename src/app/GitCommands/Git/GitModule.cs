using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.Git.Extensions;
using GitCommands.Patches;
using GitCommands.Settings;
using GitCommands.Utils;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtUtils;
using GitUI;
using GitUIPluginInterfaces;
using Microsoft;
using Microsoft.VisualStudio.Threading;

namespace GitCommands
{
    /// <summary>Provides manipulation with git module.
    /// <remarks>Several instances may be created for submodules.</remarks></summary>
    [DebuggerDisplay("GitModule ( {" + nameof(WorkingDir) + "} )")]
    public sealed partial class GitModule : IGitModule
    {
        private const string GitError = "Git Error";
        private static readonly IGitDirectoryResolver GitDirectoryResolverInstance = new GitDirectoryResolver();

        // the amount of lines we must skip in order to get to an annotated tag's message when doing git cat-file -p <tag_name>
        private static readonly int StandardCatFileTagHeaderLength = 5;

        public static readonly string NoNewLineAtTheEnd = "\\ No newline at end of file";
        public static CommandCache GitCommandCache { get; } = new();

        private readonly object _lock = new();
        private readonly IIndexLockManager _indexLockManager;
        private readonly ICommitDataManager _commitDataManager;
        private readonly IGitTreeParser _gitTreeParser = new GitTreeParser();
        private readonly IRevisionDiffProvider _revisionDiffProvider = new RevisionDiffProvider();
        private readonly GetAllChangedFilesOutputParser _getAllChangedFilesOutputParser;

        // The executable may use Windows Git (native to the app, always used in special situations) or WSL Git.
        private readonly IGitCommandRunner _gitCommandRunner;
        private readonly IGitCommandRunner _gitWindowsCommandRunner;
        private readonly IExecutable _gitExecutable;
        private readonly IExecutable _gitWindowsExecutable;

        // Parse lines of format:
        //
        // 69a7c7a40230346778e7eebed809773a6bc45268 refs/heads/master
        // 69a7c7a40230346778e7eebed809773a6bc45268 refs/remotes/origin/master
        // 366dfba1abf6cb98d2934455713f3d190df2ba34 refs/tags/2.51
        //
        // Lines may also use \t as a column delimiter, such as output of "ls-remote --heads origin".
        [GeneratedRegex(@"^(?<objectid>[0-9a-f]{40})[ \t](?<refname>.+)$", RegexOptions.Multiline | RegexOptions.ExplicitCapture)]
        private static partial Regex RefRegex();

        [GeneratedRegex(@"^(?<objectid>[0-9a-f]{40}) (?<origlinenum>\d+) (?<finallinenum>\d+)", RegexOptions.ExplicitCapture)]
        private static partial Regex HeaderRegex();

        [GeneratedRegex(@"^(?<name>[^\t]+)\t(?<url>.+?) \((?<direction>fetch|push)\)(?:(?# ignore trailing options)\s*\[[^\]]*])?$", RegexOptions.ExplicitCapture)]
        private static partial Regex RemoteVerboseLineRegex();

        [GeneratedRegex(@"(\\(?<octal>[0-7]{3}))+", RegexOptions.ExplicitCapture)]
        private static partial Regex EscapedOctalCodePointRegex();

        [GeneratedRegex(@"^(?<code>[ -+U])(?<sha>[0-9a-f]{40}) (?<path>.+) \((?<branch>.+)\)$", RegexOptions.ExplicitCapture)]
        private static partial Regex ShaRegex();

        [GeneratedRegex(@"^\s*(?<count>\d+)\s+(?<name>.*)$", RegexOptions.ExplicitCapture)]
        private static partial Regex ShortlogRegex();

        /// <summary>
        /// Name of the WSL distro for the GitExecutable, empty string for the app native Windows Git executable.
        /// This can be seen as the Git "instance" identifier.
        /// </summary>
        private readonly string _wslDistro;

        public GitModule(string? workingDir)
        {
            WorkingDir = (workingDir ?? "").NormalizePath().NormalizeWslPath().EnsureTrailingPathSeparator();
            WorkingDirGitDir = GitDirectoryResolverInstance.Resolve(WorkingDir);
            _indexLockManager = new IndexLockManager(this);
            _commitDataManager = new CommitDataManager(() => this);
            _getAllChangedFilesOutputParser = new GetAllChangedFilesOutputParser(() => this);
            _gitWindowsExecutable = new Executable(() => AppSettings.GitCommand, WorkingDir);
            _gitWindowsCommandRunner = new GitCommandRunner(_gitWindowsExecutable, () => SystemEncoding);

            _wslDistro = AppSettings.WslGitEnabled ? PathUtil.GetWslDistro(WorkingDir) : "";
            if (!string.IsNullOrEmpty(_wslDistro))
            {
                _gitExecutable = new Executable(() => AppSettings.WslGitCommand, WorkingDir, $"-d {_wslDistro} {AppSettings.WslGitPath} ");
                _gitCommandRunner = new GitCommandRunner(_gitExecutable, () => SystemEncoding);
            }
            else
            {
                _gitExecutable = _gitWindowsExecutable;
                _gitCommandRunner = _gitWindowsCommandRunner;
            }

            // If this is a submodule, populate relevant properties.
            // If this is not a submodule, these will all be null.
            (SuperprojectModule, SubmodulePath, SubmoduleName) = InitialiseSubmoduleProperties();

            return;

            (GitModule? superprojectModule, string? submodulePath, string? submoduleName) InitialiseSubmoduleProperties()
            {
                if (!IsValidGitWorkingDir())
                {
                    return (null, null, null);
                }

                string currentPath = WorkingDir.RemoveTrailingPathSeparator();

                // Try to find an ancestor path that contains a .gitmodules file and is a valid work dir
                string superprojectPath = PathUtil.FindAncestors(currentPath).FirstOrDefault(HasGitModulesFile);

                // If we didn't find it, but there's a .git file in the current folder, look for a gitdir:
                // line in that file that points to the location of the .git folder
                string gitDir = Path.Combine(WorkingDir, ".git");
                if (superprojectPath is null && File.Exists(gitDir))
                {
                    IEnumerable<string> lines;
                    try
                    {
                        lines = File.ReadLines(gitDir);
                    }
                    catch (IOException)
                    {
                        // If we cannot read the .git file, assume it's not a submodule
                        // See also special handling of WSL .git symbolic links in PathUtil.IsWslLink()
                        // Symbolic links to submodule .git is not expected and not supported.
                        return (null, null, null);
                    }

                    foreach (string line in lines)
                    {
                        const string gitdir = "gitdir:";

                        if (line.StartsWith(gitdir))
                        {
                            string gitPath = line[gitdir.Length..].Trim();
                            int pos = gitPath.IndexOf("/.git/modules/", StringComparison.Ordinal);
                            if (pos != -1)
                            {
                                gitPath = gitPath[..(pos + 1)].Replace('/', '\\');
                                gitPath = Path.GetFullPath(Path.Combine(WorkingDir, gitPath));
                                if (HasGitModulesFile(gitPath))
                                {
                                    superprojectPath = gitPath;
                                }
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(superprojectPath) && currentPath.ToPosixPath().StartsWith(superprojectPath.ToPosixPath()))
                {
                    string submodulePath = currentPath[superprojectPath.Length..].ToPosixPath();
                    ConfigFile configFile = new(Path.Combine(superprojectPath, ".gitmodules"));

                    foreach (IConfigSection configSection in configFile.ConfigSections)
                    {
                        if (configSection.GetValue("path") == submodulePath.ToPosixPath())
                        {
                            string submoduleName = configSection.SubSection;
                            GitModule superprojectModule = new(superprojectPath);

                            return (superprojectModule, submodulePath, submoduleName);
                        }
                    }

                    return (null, submodulePath, null);
                }

                return (null, null, null);

                static bool HasGitModulesFile(string path)
                    => File.Exists(Path.Combine(path, ".gitmodules")) && IsValidGitWorkingDir(path);
            }
        }

        /// <inherit/>
        public string WorkingDir { get; init; }

        /// <summary>
        /// GitVersion for the default GitExecutable.
        /// </summary>
        public IGitVersion GitVersion => GitCommands.GitVersion.CurrentVersion(GitExecutable, _wslDistro);

        /// <inherit/>
        public IExecutable GitExecutable => _gitExecutable;

        /// <inherit/>
        public IGitCommandRunner GitCommandRunner => _gitCommandRunner;

        /// <summary>
        /// Gets the location of .git directory for the current working folder.
        /// </summary>
        public string WorkingDirGitDir { get; private set; }

        /// <inherit/>
        public string GetPathForGitExecution(string? path) => PathUtil.GetPathForGitExecution(path, _wslDistro);

        /// <inherit/>
        public string GetWindowsPath(string path) => PathUtil.GetWindowsPath(path, _wslDistro);

        /// <summary>
        /// If this module is a submodule, returns its name, otherwise <c>null</c>.
        /// </summary>
        // TODO: remove?
        private string? SubmoduleName { get; }

        public string? SubmodulePath { get; }

        public IGitModule? SuperprojectModule { get; }

        public IGitModule GetTopModule()
        {
            IGitModule topModule = this;
            while (topModule.SuperprojectModule is not null)
            {
                topModule = topModule.SuperprojectModule;
            }

            return topModule;
        }

        private DistributedSettings? _effectiveSettings;

        public DistributedSettings EffectiveSettings
        {
            get
            {
                if (_effectiveSettings is null)
                {
                    lock (_lock)
                    {
                        _effectiveSettings ??= DistributedSettings.CreateEffective(module: this);
                    }
                }

                return _effectiveSettings;
            }
        }

        public SettingsSource GetEffectiveSettings()
        {
            return EffectiveSettings;
        }

        public SettingsSource GetLocalSettings()
        {
            return LocalSettings;
        }

        private DistributedSettings? _localSettings;

        public DistributedSettings LocalSettings
        {
            get
            {
                if (_localSettings is null)
                {
                    lock (_lock)
                    {
                        _localSettings ??= new DistributedSettings(lowerPriority: null, EffectiveSettings.SettingsCache, SettingLevel.Local);
                    }
                }

                return _localSettings;
            }
        }

        private ConfigFileSettings? _effectiveConfigFile;

        public IConfigFileSettings EffectiveConfigFile
        {
            get
            {
                if (_effectiveConfigFile is null)
                {
                    lock (_lock)
                    {
                        _effectiveConfigFile ??= ConfigFileSettings.CreateEffective(module: this);
                    }
                }

                return _effectiveConfigFile;
            }
        }

        public IConfigFileSettings LocalConfigFile
            => new ConfigFileSettings(lowerPriority: null, ((ConfigFileSettings)EffectiveConfigFile).SettingsCache, SettingLevel.Local);

        IConfigFileSettings IGitModule.LocalConfigFile => LocalConfigFile;

        // encoding for files paths
        private static Encoding? _systemEncoding;

        public static Encoding SystemEncoding => _systemEncoding ??= new SystemEncodingReader().Read();

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

        public Encoding FilesEncoding => ((ConfigFileSettings)EffectiveConfigFile).FilesEncoding ?? new UTF8Encoding(false);

        public Encoding CommitEncoding => ((ConfigFileSettings)EffectiveConfigFile).CommitEncoding ?? new UTF8Encoding(false);

        public Encoding LogOutputEncoding => ((ConfigFileSettings)EffectiveConfigFile).LogOutputEncoding ?? CommitEncoding;

        /// <summary>Indicates whether the <see cref="WorkingDir"/> contains a git repository.</summary>
        public bool IsValidGitWorkingDir()
        {
            return IsValidGitWorkingDir(WorkingDir);
        }

        /// <summary>Indicates whether the specified directory contains a git repository.</summary>
        public static bool IsValidGitWorkingDir(string? dir)
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
        /// Asks git to resolve the given relativePath.
        /// git special folders are located in different directories depending on the kind of repo: submodule, worktree, main.
        /// See https://git-scm.com/docs/git-rev-parse#Documentation/git-rev-parse.txt---git-pathltpathgt.
        /// </summary>
        /// <param name="relativePath">A path relative to the .git directory.</param>
        /// <returns>An absolute path in Windows format.</returns>
        public string ResolveGitInternalPath(string relativePath)
        {
            GitArgumentBuilder args = new("rev-parse")
            {
                "--git-path",
                relativePath.Quote()
            };
            string gitPath = _gitExecutable.GetOutput(args).Trim();

            if (gitPath.StartsWith(".git/"))
            {
                gitPath = Path.Combine(GetGitDirectory(), gitPath[".git/".Length..]);
            }

            return GetWindowsPath(gitPath);
        }

        private string? _gitCommonDirectory;
        private readonly object _gitCommonLock = new();

        /// <summary>
        /// Returns git common directory.
        /// https://git-scm.com/docs/git-rev-parse#Documentation/git-rev-parse.txt---git-common-dir.
        /// </summary>
        public string GitCommonDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(WorkingDir))
                {
                    // No directory for this module, so the common directory must be empty too
                    // (should not be retrieved from the GE start directory)
                    return "";
                }

                // Get a cache of the common dir
                // Lock needed as the command is called rapidly when creating the module
                if (_gitCommonDirectory is not null)
                {
                    return _gitCommonDirectory;
                }

                lock (_gitCommonLock)
                {
                    if (_gitCommonDirectory is null)
                    {
                        GitArgumentBuilder args = new("rev-parse") { "--git-common-dir" };
                        ExecutionResult result = _gitExecutable.Execute(args, throwOnErrorExit: false);

                        string dir = GetWindowsPath(result.StandardOutput).Trim();

                        if (!result.ExitedSuccessfully || dir == ".git" || dir == "." || !Directory.Exists(dir))
                        {
                            dir = GetGitDirectory();
                        }

                        _gitCommonDirectory = dir;
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
            GitArgumentBuilder args = new("submodule")
            {
                "status",
                submodulePath
            };
            ExecutionResult result = _gitExecutable.Execute(args, throwOnErrorExit: false);

            return result.ExitedSuccessfully || IsSubmoduleRemoved();

            bool IsSubmoduleRemoved()
                => result.StandardOutput.StartsWith("No submodule mapping found in .gitmodules for path");
        }

        public bool HasSubmodules()
        {
            return GetSubmodulesLocalPaths(recursive: false).Any();
        }

        /// <inheritdoc />
        public IReadOnlyList<string> GetSubmodulesLocalPaths(bool recursive = true)
        {
            List<string> localPaths = [];
            try
            {
                DoGetSubmodulesLocalPaths(this, "", localPaths, recursive);
            }
            catch (GitConfigurationException)
            {
                // swallow any exceptions here, any config exceptions would have been shown to the user already
            }

            return localPaths;

            void DoGetSubmodulesLocalPaths(IGitModule module, string parentPath, List<string> paths, bool recurse)
            {
                List<string> submodulePaths = GetSubmodulePaths(module)
                    .Select(p => Path.Combine(parentPath, p).ToPosixPath())
                    .ToList();

                paths.AddRange(submodulePaths);

                if (recurse)
                {
                    foreach (string submodulePath in submodulePaths)
                    {
                        DoGetSubmodulesLocalPaths(GetSubmodule(submodulePath), submodulePath, paths, recurse);
                    }
                }
            }

            IEnumerable<string> GetSubmodulePaths(IGitModule module)
            {
                ISubmodulesConfigFile configFile = module.GetSubmodulesConfigFile();
                return configFile.ConfigSections
                    .Select(section => section.GetValue("path").Trim()).Where(path => !string.IsNullOrWhiteSpace(path));
            }
        }

        /// <summary>
        /// Searches from <paramref name="startDir"/> and up through the directory
        /// hierarchy for a valid git working directory. If found, the path is returned,
        /// otherwise <c>null</c>.
        /// </summary>
        public static string? TryFindGitWorkingDir(string? startDir)
        {
            string dir = startDir?.Trim();

            while (!string.IsNullOrWhiteSpace(dir))
            {
                if (IsValidGitWorkingDir(dir))
                {
                    return dir.EnsureTrailingPathSeparator();
                }

                dir = Path.GetDirectoryName(dir);
            }

            return null;
        }

        public ExecutionResult Clean(CleanMode mode, bool dryRun = false, bool directories = false, string? paths = null)
        {
            return _gitExecutable.Execute(
                Commands.Clean(mode, dryRun, directories, paths));
        }

        public bool EditNotes(ObjectId commitId)
        {
            GitArgumentBuilder arguments = new("notes") { "edit", commitId };
            string editor = GetEffectiveSetting("core.editor").ToLower();
            bool createWindow = !editor.Contains("gitextensions") && !editor.Contains("notepad");

            return _gitExecutable.RunCommand(arguments, createWindow: createWindow, throwOnErrorExit: false);
        }

        public bool InTheMiddleOfConflictedMerge(bool throwOnErrorExit = true)
        {
            GitArgumentBuilder args = new("ls-files")
            {
                "-z",
                "--unmerged"
            };

            // Do not report errors for commands called in the background
            ExecutionResult result = _gitExecutable.Execute(args, throwOnErrorExit: throwOnErrorExit);
            return result.ExitedSuccessfully && !string.IsNullOrEmpty(result.StandardOutput);
        }

        public bool HandleConflictSelectSide(string fileName, string side)
        {
            Directory.SetCurrentDirectory(WorkingDir);
            GitArgumentBuilder args = new("checkout-index")
            {
                "-f",
                $"--stage={GetSide(side)}",
                "--",
                fileName.ToPosixPath().QuoteNE()
            };
            string output = _gitExecutable.GetOutput(args);

            if (!string.IsNullOrEmpty(output))
            {
                return false;
            }

            args = new GitArgumentBuilder("add")
            {
                "--",
                fileName.ToPosixPath().QuoteNE()
            };
            output = _gitExecutable.GetOutput(args);
            return string.IsNullOrEmpty(output);
        }

        public bool HandleConflictsSaveSide(string fileName, string saveAsFileName, string side)
        {
            Directory.SetCurrentDirectory(WorkingDir);

            GitArgumentBuilder args = new("checkout-index")
            {
                $"--stage={GetSide(side)}",
                "--temp",
                "--",
                fileName.ToPosixPath().QuoteNE()
            };
            string output = _gitExecutable.GetOutput(args);

            if (string.IsNullOrEmpty(output))
            {
                return false;
            }

            if (!output.StartsWith(".merge_file_"))
            {
                return false;
            }

            // Parse temporary file name from command line result
            string[] splitResult = output.Split(Delimiters.TabAndLineFeedAndCarriageReturn, StringSplitOptions.RemoveEmptyEntries);
            if (splitResult.Length != 2)
            {
                return false;
            }

            string temporaryFileName = splitResult[0].Trim();

            if (!File.Exists(temporaryFileName))
            {
                return false;
            }

            bool retValue = false;
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
            using MemoryStream blobStream = GetFileStream(blob);
            if (blobStream is null)
            {
                return;
            }

            byte[] blobData = blobStream.ToArray();
            if (((ConfigFileSettings)EffectiveConfigFile).ByPath("core").GetNullableEnum<AutoCRLFType>("autocrlf") is AutoCRLFType.@true)
            {
                if (!FileHelper.IsBinaryFileName(this, saveAs) && !FileHelper.IsBinaryFileAccordingToContent(blobData))
                {
                    blobData = GitConvert.ConvertCrLfToWorktree(blobData);
                }
            }

            using FileStream stream = File.Create(saveAs);
            stream.Write(blobData, 0, blobData.Length);
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

        public (string? BaseFile, string? LocalFile, string? RemoteFile) CheckoutConflictedFiles(ConflictData unmergedData)
        {
            Directory.SetCurrentDirectory(WorkingDir);

            string baseFile = CheckoutPart(1, unmergedData.Filename + ".BASE", unmergedData.Base.Filename);
            string localFile = CheckoutPart(2, unmergedData.Filename + ".LOCAL", unmergedData.Local.Filename);
            string remoteFile = CheckoutPart(3, unmergedData.Filename + ".REMOTE", unmergedData.Remote.Filename);

            return (baseFile, localFile, remoteFile);

            string? CheckoutPart(int part, string fileName, string? unmerged)
            {
                if (unmerged is not null)
                {
                    GitArgumentBuilder args = new("checkout-index")
                    {
                        "--temp",
                        $"--stage={part}",
                        "--",
                        unmergedData.Filename.QuoteNE()
                    };

                    // Check out the part to a temporary file
                    string output = _gitExecutable.GetOutput(args);

                    string tempFile = Path.Combine(WorkingDir, output.SubstringUntil('\t'));

                    fileName = FindAvailableFileName(Path.Combine(WorkingDir, fileName));

                    try
                    {
                        File.Move(tempFile, fileName);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                    }

                    return fileName;
                }

                return File.Exists(fileName) ? fileName : null;
            }

            string FindAvailableFileName(string basePath)
            {
                // If necessary, append an index to the base path until the file does not exist
                int index = 1;
                string test = basePath;

                while (File.Exists(test) && index < 50)
                {
                    test = basePath + index;
                    index++;
                }

                return test;
            }
        }

        public async Task<ConflictData> GetConflictAsync(string? filename)
        {
            return (await GetConflictsAsync(filename)).SingleOrDefault();
        }

        public async Task<List<ConflictData>> GetConflictsAsync(string? filename = "")
        {
            filename = filename.ToPosixPath();

            List<ConflictData> list = [];
            GitArgumentBuilder args = new("ls-files")
            {
                "-z",
                "--unmerged",
                { !string.IsNullOrWhiteSpace(filename), "--" },
                filename.QuoteNE()
            };

            // ignore non-zero exit code, e.g. in case of missing submodule
            ExecutionResult result = await _gitExecutable.ExecuteAsync(args, throwOnErrorExit: false).ConfigureAwait(false);
            string[] unmerged = result.StandardOutput.Split(Delimiters.NullAndLineFeed, StringSplitOptions.RemoveEmptyEntries);

            ConflictedFileData[] item = new ConflictedFileData[3];

            string? prevItemName = null;

            foreach (string line in unmerged)
            {
                int findSecondWhitespace = line.IndexOfAny(new[] { ' ', '\t' });
                string fileStage = findSecondWhitespace >= 0 ? line[findSecondWhitespace..].Trim() : "";

                findSecondWhitespace = fileStage.IndexOfAny(new[] { ' ', '\t' });

                string hash = findSecondWhitespace >= 0 ? fileStage[..findSecondWhitespace].Trim() : "";
                fileStage = findSecondWhitespace >= 0 ? fileStage[findSecondWhitespace..].Trim() : "";

                if (fileStage.Length > 2 && int.TryParse(fileStage[0].ToString(), out int stage) && stage is (>= 1 and <= 3))
                {
                    string itemName = fileStage[2..];
                    if (prevItemName != itemName && prevItemName is not null)
                    {
                        list.Add(new ConflictData(item[0], item[1], item[2]));
                        item = new ConflictedFileData[3];
                    }

                    item[stage - 1] = new ConflictedFileData(ObjectId.Parse(hash), itemName);
                    prevItemName = itemName;
                }
            }

            if (prevItemName is not null)
            {
                list.Add(new ConflictData(item[0], item[1], item[2]));
            }

            return list;
        }

        public async Task<Dictionary<IGitRef, IGitItem?>> GetSubmoduleItemsForEachRefAsync(string? filename, bool noLocks = false)
        {
            const int maxSuperRefCount = 100;
            RefsFilter refsFilter = (AppSettings.ShowSuperprojectBranches ? RefsFilter.Heads : RefsFilter.NoFilter)
                | (AppSettings.ShowSuperprojectRemoteBranches ? RefsFilter.Remotes : RefsFilter.NoFilter)
                | (AppSettings.ShowSuperprojectTags ? RefsFilter.Tags : RefsFilter.NoFilter);
            if (refsFilter == RefsFilter.NoFilter)
            {
                return [];
            }

            string? command = Commands.GetRefs(refsFilter, noLocks: noLocks, GitRefsSortBy.committerdate, GitRefsSortOrder.Descending, maxSuperRefCount);
            string refList = await _gitExecutable.GetOutputAsync(command).ConfigureAwait(false);
            IReadOnlyList<IGitRef> refs = ParseRefs(refList);

            return refs.ToDictionary(r => r, r => GetSubmoduleCommitHash(filename.ToPosixPath(), r.Name));
        }

        private IGitItem? GetSubmoduleCommitHash(string? filename, string refName)
        {
            GitArgumentBuilder args = new("ls-tree")
            {
                refName.Quote(),
                { !string.IsNullOrWhiteSpace(filename), "--" },
                filename.QuoteNE()
            };
            string output = _gitExecutable.GetOutput(args);

            return _gitTreeParser.ParseSingle(output);
        }

        public int? GetCommitCount(string parent, string child, bool cache = false, bool throwOnErrorExit = true)
        {
            if (parent == child)
            {
                return 0;
            }

            GitArgumentBuilder args = new("rev-list")
            {
                parent.Quote(),
                $"^{child}".Quote(),
                "--count",
                "--"
            };
            ExecutionResult result = _gitExecutable.Execute(args, cache: cache ? GitCommandCache : null, throwOnErrorExit: throwOnErrorExit);
            string output = result.StandardOutput;

            if (int.TryParse(output, out int commitCount))
            {
                return commitCount;
            }

            return null;
        }

        public (int? First, int? Second) GetCommitRangeDiffCount(ObjectId firstId, ObjectId secondId)
        {
            if (firstId == secondId)
            {
                return (0, 0);
            }

            GitArgumentBuilder args = new("rev-list")
            {
                $"{firstId}...{secondId}".Quote(),
                "--count",
                "--left-right"
            };
            string output = _gitExecutable.GetOutput(args, cache: GitCommandCache);

            string[] counts = output.Split(Delimiters.Tab);
            if (counts.Length == 2 && int.TryParse(counts[0], out int first) && int.TryParse(counts[1], out int second))
            {
                return (first, second);
            }

            return (null, null);
        }

        public string GetCommitCountString(ObjectId fromId, string to)
        {
            string from = fromId.IsArtificial ? "HEAD" : fromId.ToString();
            int? removed = GetCommitCount(from, to);
            int? added = GetCommitCount(to, from);

            if (removed is null || added is null)
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

        public void RunGitK()
        {
            if (EnvUtils.RunningOnUnix())
            {
                new Executable("gitk", WorkingDir).Start(createWindow: true);
            }
            else
            {
                // locate gitk based on location of git executable
                string cmd = AppSettings.GitCommand
                    .Replace("bin\\git.exe", "cmd\\gitk")
                    .Replace("bin/git.exe", "cmd/gitk")
                    .Replace("git.exe", "gitk")
                    .Replace("git.cmd", "gitk");

                new Executable("cmd.exe", WorkingDir).Start($"/c \"\"{cmd}\" --branches --tags --remotes\"");
            }
        }

        public void RunGui()
        {
            ArgumentBuilder args;
            if (EnvUtils.RunningOnUnix())
            {
                args = new GitArgumentBuilder("gui");
                _gitExecutable.Start(args, createWindow: true);
            }
            else
            {
                args =
                [
                    "/c",
                    $"\"{AppSettings.GitCommand.QuoteNE()}",
                    "gui\""
                ];
                new Executable("cmd.exe", WorkingDir).Start(args);
            }
        }

        public void RunMergeTool(string? fileName = "", string? customTool = null)
        {
            // Use Windows Git if custom tool is selected as the list is native to the application.
            bool isWindowsGit = !string.IsNullOrWhiteSpace(customTool);
            string gui = (isWindowsGit ? GitCommands.GitVersion.Current : GitVersion).SupportGuiMergeTool ? "--gui" : string.Empty;
            GitArgumentBuilder args = new("mergetool")
            {
                { string.IsNullOrWhiteSpace(customTool), gui, $"--tool={customTool}" },
                { !string.IsNullOrWhiteSpace(fileName), "--" },
                fileName.ToPosixPath().QuoteNE()
            };

            using IProcess process = (isWindowsGit ? _gitWindowsExecutable : _gitExecutable).Start(args, createWindow: true, throwOnErrorExit: false);
            process.WaitForExit();
        }

        public string Init(bool bare, bool shared)
        {
            GitArgumentBuilder args = new("init")
            {
                { bare, "--bare" },
                { shared, "--shared=all" }
            };

            // Note that the output contains the path to the repo for the Git executable.
            // This means that the WSL path is presented in WSL repos, not the Windows path (native to the app).
            string output = _gitExecutable.GetOutput(args);

            WorkingDirGitDir = GitDirectoryResolverInstance.Resolve(WorkingDir);
            return output;
        }

        public bool IsMerge(ObjectId objectId)
        {
            return GetParents(objectId).Count > 1;
        }

        public GitRevision GetRevision(ObjectId? objectId = null, bool shortFormat = false, bool loadRefs = false)
        {
            GitRevision revision = new RevisionReader(this, allBodies: true).GetRevision(objectId?.ToString(), hasNotes: !shortFormat, throwOnError: true, cancellationToken: default)!;

            if (loadRefs)
            {
                revision.Refs = GetRefs(RefsFilter.NoFilter)
                    .Where(r => r.ObjectId == revision.ObjectId)
                    .ToList();
            }

            return revision;
        }

        public IReadOnlyList<ObjectId> GetParents(ObjectId objectId)
        {
            if (objectId.IsArtificial)
            {
                // For WorkTree could Index be returned, but for Index HEAD should be available anyway
                throw new InvalidOperationException(nameof(objectId));
            }

            GitArgumentBuilder args = new("rev-parse")
            {
                $"{objectId}^@".Quote()
            };
            return _gitExecutable.Execute(args, cache: GitCommandCache)
                .StandardOutput
                .Split(Delimiters.NullAndLineFeed, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => ObjectId.Parse(line))
                .ToList();
        }

        public IReadOnlyList<GitRevision> GetParentRevisions(ObjectId objectId)
        {
            return GetParents(objectId)
                .Select(parent => GetRevision(parent, shortFormat: true))
                .ToList();
        }

        public string? ShowObject(ObjectId objectId, bool returnRaw)
        {
            string gitOutput = _gitExecutable
                .GetOutput($"show {objectId}", cache: GitCommandCache, outputEncoding: LosslessEncoding);
            return returnRaw ? gitOutput : ReEncodeShowString(gitOutput);
        }

        public void DeleteTag(string tagName)
        {
            GitArgumentBuilder args = new("tag")
            {
                "-d",
                tagName.QuoteNE()
            };
            _gitExecutable.RunCommand(args);
        }

        /// <summary>
        /// Gets the commit ID of the currently checked out commit.
        /// If the repo is bare, has no commits, detached head or is corrupt, <c>null</c> is returned.
        /// </summary>
        public ObjectId? GetCurrentCheckout()
        {
            GitArgumentBuilder args = new("rev-parse") { "HEAD" };
            ExecutionResult result = _gitExecutable.Execute(args, throwOnErrorExit: false);

            return result.ExitedSuccessfully && ObjectId.TryParse(result.StandardOutput, offset: 0, out ObjectId? objectId)
                ? objectId
                : null;
        }

        public bool TryResolvePartialCommitId(string objectIdPrefix, [NotNullWhen(returnValue: true)] out ObjectId? objectId)
        {
            // If the prefix is already a full SHA1 then return immediately without invoking a git process.
            if (ObjectId.TryParse(objectIdPrefix, out objectId))
            {
                return true;
            }

            GitArgumentBuilder args = new("rev-parse")
            {
                "--verify",
                "--quiet",
                $"{objectIdPrefix}^{{commit}}".Quote()
            };
            ExecutionResult result = _gitExecutable.Execute(args, throwOnErrorExit: false);
            string output = result.StandardOutput.Trim();

            if (output.StartsWith(objectIdPrefix) && ObjectId.TryParse(output, out objectId))
            {
                return true;
            }

            objectId = default;
            return false;
        }

        public async Task<(char Code, ObjectId? CommitId)> GetSuperprojectCurrentCheckoutAsync()
        {
            if (string.IsNullOrEmpty(SuperprojectModule?.WorkingDir))
            {
                return (' ', null);
            }

            GitArgumentBuilder args = new("submodule")
            {
                "status",
                "--cached",
                SubmodulePath.Quote()
            };
            string output = await SuperprojectModule.GitExecutable.GetOutputAsync(args).ConfigureAwait(false);
            string[] lines = output.Split(Delimiters.LineFeed);

            if (lines.Length == 0)
            {
                return (' ', null);
            }

            string submodule = lines[0];

            if (submodule.Length < ObjectId.Sha1CharCount + 3
                || !ObjectId.TryParse(submodule, 1, out ObjectId commitId))
            {
                return (' ', null);
            }

            return (submodule[0], commitId);
        }

        public bool ExistsMergeCommit(string? startRev, string? endRev)
        {
            if (string.IsNullOrEmpty(startRev) || string.IsNullOrEmpty(endRev))
            {
                return false;
            }

            GitArgumentBuilder args = new($"rev-list")
            {
                "--parents",
                "--no-walk",
                "--min-parents=2",
                $"{startRev}..{endRev}".Quote(),
            };

            // Could fail if pulling interactively from remote where the specified branch does not exist
            string mergeCommitsOutput = _gitExecutable.Execute(args, throwOnErrorExit: false).StandardOutput;
            return !string.IsNullOrWhiteSpace(mergeCommitsOutput);
        }

        public ISubmodulesConfigFile GetSubmodulesConfigFile() => new ConfigFile($"{WorkingDir}.gitmodules");

        public string? GetCurrentSubmoduleLocalPath()
        {
            if (SuperprojectModule is null)
            {
                return null;
            }

            DebugHelpers.Assert(WorkingDir.StartsWith(SuperprojectModule.WorkingDir), "Submodule working dir should start with super-project's working dir");

            return Path.GetDirectoryName(
                WorkingDir[SuperprojectModule.WorkingDir.Length..]).ToPosixPath();
        }

        public string GetSubmoduleFullPath(string? localPath)
        {
            if (localPath is null)
            {
                DebugHelpers.Fail("No path for submodule - incorrectly parsed status?");
                return "";
            }

            string dir = Path.Combine(WorkingDir, localPath.EnsureTrailingPathSeparator());
            return Path.GetFullPath(dir); // fix slashes
        }

        public IGitModule GetSubmodule(string? localPath)
        {
            return new GitModule(GetSubmoduleFullPath(localPath));
        }

        IGitModule IGitModule.GetSubmodule(string submoduleName)
        {
            return GetSubmodule(submoduleName);
        }

        public IEnumerable<IGitSubmoduleInfo?> GetSubmodulesInfo()
        {
            ISubmodulesConfigFile? configFile;
            try
            {
                configFile = GetSubmodulesConfigFile();
            }
            catch (GitConfigurationException)
            {
                // swallow any exceptions here, any config exceptions would have been shown to the user already
                configFile = null;
            }

            if (configFile is null)
            {
                yield return null;
            }

            GitArgumentBuilder args = new("submodule") { "status" };
            ExecutionResult result = _gitExecutable.Execute(args);
            LazyStringSplit lines = result.StandardOutput.LazySplit('\n');

            string? lastLine = null;

            foreach (string line in lines)
            {
                if (line == lastLine)
                {
                    continue;
                }

                lastLine = line;

                if (TryParseSubmoduleInfo(line, out GitSubmoduleInfo? info))
                {
                    yield return info;
                }
            }

            bool TryParseSubmoduleInfo(string s, [NotNullWhen(returnValue: true)] out GitSubmoduleInfo? info)
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

                Match match = ShaRegex().Match(s);

                if (!match.Success)
                {
                    info = default;
                    return false;
                }

                char code = match.Groups["code"].Value[0];
                string localPath = match.Groups["path"].Value;
                string branch = match.Groups["branch"].Value;

                if (!ObjectId.TryParse(match.Groups["sha"].Value, out ObjectId? currentCommitId))
                {
                    info = default;
                    return false;
                }

                Validates.NotNull(configFile);

                IConfigSection configSection = configFile.ConfigSections.FirstOrDefault(section => section.GetValue("path").Trim() == localPath);

                Assumes.True(configSection is not null, $"`git submodule status` returned submodule \"{localPath}\" that was not found in .gitmodules");
                Assumes.True(configSection.SubSection is not null, $"Config section must have a non-null sub-section");

                string name = configSection.SubSection.Trim();
                string remotePath = configFile.GetPathValue($"submodule.{name}.url").Trim();

                info = new GitSubmoduleInfo(
                    name, localPath, remotePath, currentCommitId, branch,
                    isInitialized: code != '-',
                    isUpToDate: code != '+');
                return true;
            }
        }

        public string GetSubmoduleSummary(string submodule)
        {
            GitArgumentBuilder args = new("submodule")
            {
                "summary",
                submodule
            };
            return _gitExecutable.GetOutput(args);
        }

        /// <summary>
        /// Reset all changes to HEAD
        /// </summary>
        /// <param name="clean">Clean non ignored files.</param>
        /// <param name="onlyWorkTree">Reset only WorkTree files.</param>
        /// <returns><see langword="true"/> if executed.</returns>
        public bool ResetAllChanges(bool clean, bool onlyWorkTree = false)
        {
            if (onlyWorkTree)
            {
                GitArgumentBuilder args = new("checkout")
                    {
                        "--",
                        "."
                    };
                GitExecutable.GetOutput(args);
            }
            else
            {
                // Reset all changes.
                Reset(ResetMode.Hard);
            }

            if (clean)
            {
                Clean(CleanMode.OnlyNonIgnored, directories: true);
            }

            return true;
        }

        public void Reset(ResetMode mode, string? file = null)
        {
            _gitExecutable.RunCommand(Commands.Reset(mode, commit: null, file));
        }

        /// <summary>
        /// Executes <c>checkout-index</c> ("reset") command which copies files from the index overwriting the working tree.
        /// </summary>
        /// <param name="files">List with (relative path) filenames.</param>
        /// <returns>stdout from Git.</returns>
        public string CheckoutIndexFiles(IReadOnlyList<string> files)
        {
            if (files?.Count is not > 0)
            {
                return string.Empty;
            }

            return _gitExecutable.GetBatchOutput(new GitArgumentBuilder("checkout-index")
                {
                    "--index",
                    "--force",
                    "--"
                }
                .BuildBatchArgumentsForFiles(files));
        }

        /// <summary>
        /// Reset changes to the selected files.
        /// </summary>
        /// <param name="resetId">Id to reset to, null for HEAD</param>
        /// <param name="selectedItems">Items to reset.</param>
        /// <param name="resetAndDelete">Delete new (and renamed) files.</param>
        /// <param name="fullPathResolver"><see cref="IFullPathResolver"/></param>
        /// <param name="output">Error messages from the reset.</param>
        /// <param name="progressAction">Action when unstaging files (to update a progress bar).</param>
        /// <returns><see langword="true"/> if successfully executed</returns>
        public bool ResetChanges(ObjectId? resetId, IReadOnlyList<GitItemStatus> selectedItems, bool resetAndDelete, IFullPathResolver fullPathResolver, out StringBuilder output, Action<BatchProgressEventArgs>? progressAction = null)
        {
            if (resetId?.IsArtificial is true && resetId != ObjectId.IndexId)
            {
                throw new InvalidOperationException(nameof(resetId));
            }

            // unstage first (to reset conflicts)
            if (resetId != ObjectId.IndexId)
            {
                Lazy<List<GitItemStatus>> initialStatus = new(() => GetAllChangedFilesWithSubmodulesStatus().ToList());
                List<GitItemStatus> filesToUnstage = [];
                foreach (GitItemStatus item in selectedItems)
                {
                    if (item.Staged == StagedStatus.Index)
                    {
                        filesToUnstage.Add(item);
                    }
                    else if (initialStatus.Value.FirstOrDefault(i => i.Name == item.Name && i.Staged == StagedStatus.Index) is GitItemStatus gitStatus)
                    {
                        filesToUnstage.Add(gitStatus);
                    }
                }

                BatchUnstageFiles(filesToUnstage, progressAction);
            }

            List<string> filesInUse = [];
            List<string> filesToCheckout = [];
            List<string> filesToReset = [];
            List<string> filesCannotCheckout = [];
            output = new();
            Lazy<List<GitItemStatus>> postUnstageStatus = new(() => GetAllChangedFilesWithSubmodulesStatus().ToList());

            foreach (GitItemStatus item in selectedItems)
            {
                if (resetAndDelete && (DeletableItem(item)
                    || (resetId != ObjectId.IndexId && postUnstageStatus.Value.Any(i => DeletableItem(i) && i.Name == item.Name))))
                {
                    try
                    {
                        string? path = fullPathResolver.Resolve(item.Name);
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                        else if (Directory.Exists(path))
                        {
                            Directory.Delete(path, recursive: true);
                        }
                    }
                    catch (IOException)
                    {
                        filesInUse.Add(item.Name);
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                }

                if (resetId == ObjectId.IndexId)
                {
                    if (!postUnstageStatus.Value.Any(i => i.Name == item.Name))
                    {
                        // Already removed (for instance new file)
                        continue;
                    }

                    if (UnmergedIndex(item, postUnstageStatus))
                    {
                        filesCannotCheckout.Add(item.Name);
                        continue;
                    }

                    filesToCheckout.Add(item.IsRenamed ? item.OldName : item.Name);
                }
                else if (!item.IsNew && !postUnstageStatus.Value.Any(i => i.IsNew && i.Name == item.Name))
                {
                    if (resetId is not null || UnmergedNotIndex(item, postUnstageStatus))
                    {
                        filesToCheckout.Add(item.IsRenamed ? item.OldName : item.Name);
                    }
                    else
                    {
                        // reset to head
                        filesToReset.Add(item.IsRenamed ? item.OldName : item.Name);
                    }
                }
            }

            output.Append(CheckoutIndexFiles(filesToReset));
            output.Append(CheckoutFiles(filesToCheckout, resetId, force: false));

            const char bullet = '\u2022';
            const char noBreakSpace = '\u00a0';
            string separator = $"{Environment.NewLine}{bullet}{noBreakSpace}";
            if (filesInUse.Count > 0)
            {
                output.Append($"The following files are currently in use and will not be reset:{separator}{string.Join(separator, filesInUse)}");
            }

            if (filesCannotCheckout.Count > 0)
            {
                output.Append($"The following files are unmerged and will not be reset:{separator}{string.Join(separator, filesCannotCheckout)}");
            }

            return true;

            static bool DeletableItem(GitItemStatus item) => item.IsNew || item.IsRenamed;

            // For normal commits: 'git-checkout <objectId> --' must be used for Unmerged but will not work on e.g. SkipWorktree.
            static bool UnmergedNotIndex(GitItemStatus item, Lazy<List<GitItemStatus>> status)
                => item.IsUnmerged && !status.Value.Any(i => !i.IsDeleted && !i.IsNew && i.Name == item.Name);

            // 'git-checkout --' must be used for Index (git-reset will copy HEAD to Index, git-restore from 2.25 could be used).
            // However, Unmerged (Conflict) files cannot be checked out to Index.
            static bool UnmergedIndex(GitItemStatus item, Lazy<List<GitItemStatus>> status)
                => status.Value.Any(i => (i.IsUnmerged || i.IsNew) && i.Name == item.Name) || !status.Value.Any(i => i.Name == item.Name);
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

        public string FormatPatch(string from, string to, string output, int? start = null)
        {
            return _gitExecutable.GetOutput(
                new GitArgumentBuilder("format-patch")
                {
                    "--find-renames",
                    "--find-copies",
                    "--break-rewrites",
                    { start is not null, $"--start-number {start}" },
                    { !string.IsNullOrEmpty(from), $"{from.Quote()}..{to.Quote()}", $"--root {to.Quote()}" },
                    $"-o {GetPathForGitExecution(output).Quote()}"
                });
        }

        public string CheckoutFiles(IReadOnlyList<string> files, ObjectId? revision, bool force)
        {
            if (files.Count == 0 || (revision?.IsArtificial is true && revision != ObjectId.IndexId))
            {
                return "";
            }

            // Reset to index has no revision string
            string revStr = revision == ObjectId.IndexId ? "" : revision?.ToString() ?? RevParse("HEAD").ToString();

            // Run batch arguments to work around max command line length on Windows. Fix #6593
            // 3: double quotes + ' '
            // See https://referencesource.microsoft.com/#system/services/monitoring/system/diagnostics/Process.cs,1952
            return _gitExecutable.RunBatchCommand(new GitArgumentBuilder("checkout")
                {
                    { force, "--force" },
                    revStr,
                    "--"
                }
                .BuildBatchArgumentsForFiles(files))
                ?.StandardOutput ?? "";
        }

        public string RemoveFiles(IReadOnlyList<string> files, bool force)
        {
            if (files.Count == 0)
            {
                return "";
            }

            return _gitExecutable.GetBatchOutput(
                new GitArgumentBuilder("rm")
                {
                    { force, "--force" },
                    "--"
                }
                    .BuildBatchArgumentsForFiles(files));
        }

        public string GetPuttyKeyFileForRemote(string? remote)
        {
            if (string.IsNullOrEmpty(remote) ||
                string.IsNullOrEmpty(AppSettings.Pageant) ||
                !AppSettings.AutoStartPageant ||
                !GitSshHelpers.IsPlink)
            {
                return "";
            }

            return GetSetting($"remote.{remote}.puttykeyfile");
        }

        public ArgumentString FetchCmd(string? remote, string? remoteBranch, string? localBranch, bool? fetchTags = false, bool isUnshallow = false, bool pruneRemoteBranches = false, bool pruneRemoteBranchesAndTags = false)
        {
            return new GitArgumentBuilder("fetch", gitOptions: GetFetchOptions())
            {
                "--progress",
                {
                    !string.IsNullOrEmpty(remote) || !string.IsNullOrEmpty(remoteBranch) || !string.IsNullOrEmpty(localBranch),
                    GetFetchArgs(remote, remoteBranch, localBranch, fetchTags, isUnshallow, pruneRemoteBranches, pruneRemoteBranchesAndTags)
                }
            };
        }

        public ArgumentString PullCmd(string remote, string? remoteBranch, bool rebase, bool? fetchTags = false, bool isUnshallow = false)
        {
            return new GitArgumentBuilder("pull", gitOptions: GetFetchOptions())
            {
                { rebase, "--rebase" },
                "--progress",
                GetFetchArgs(remote, remoteBranch, null, fetchTags, isUnshallow)
            };
        }

        private ArgumentString GetFetchArgs(string? remote, string? remoteBranch, string? localBranch, bool? fetchTags, bool isUnshallow, bool pruneRemoteBranches = false, bool pruneRemoteBranchesAndTags = false)
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

            // TODO return ArgumentBuilder and add special case ArgumentBuilder.Add(ArgumentBuilder childBuilder)
            return new ArgumentBuilder
            {
                remote.ToPosixPath()?.Trim().Quote(),
                branchArguments,
                { fetchTags == true, "--tags" },
                { fetchTags == false, "--no-tags" },
                { isUnshallow, "--unshallow" },
                { pruneRemoteBranches || pruneRemoteBranchesAndTags, "--prune --force" },
                { pruneRemoteBranchesAndTags, "--prune-tags" },
            };
        }

        private ArgumentString GetFetchOptions()
        {
            return new ArgumentBuilder
            {
                { string.IsNullOrWhiteSpace(EffectiveConfigFile.GetValue("fetch.parallel")), "-c fetch.parallel=0" },
                { string.IsNullOrWhiteSpace(EffectiveConfigFile.GetValue("submodule.fetchJobs")), "-c submodule.fetchJobs=0" },
            };
        }

        public string GetRebaseDir()
        {
            string gitDirectory = GetGitDirectory();

            string rebaseMergeDir = gitDirectory + "rebase-merge" + Path.DirectorySeparatorChar;
            if (Directory.Exists(rebaseMergeDir))
            {
                return rebaseMergeDir;
            }

            string rebaseApplyDir = gitDirectory + "rebase-apply" + Path.DirectorySeparatorChar;
            if (Directory.Exists(rebaseApplyDir))
            {
                return rebaseApplyDir;
            }

            string rebaseDir = gitDirectory + "rebase" + Path.DirectorySeparatorChar;
            if (Directory.Exists(rebaseDir))
            {
                return rebaseDir;
            }

            return "";
        }

        public string ApplyPatch(string dir, ArgumentString arguments)
        {
            using IProcess process = _gitExecutable.Start(arguments, createWindow: false, redirectInput: true, redirectOutput: true, SystemEncoding);
            string[] files = Directory.GetFiles(dir);

            if (files.Length == 0)
            {
                return "";
            }

            foreach (string file in files)
            {
                using FileStream fs = new(file, FileMode.Open);
                fs.CopyTo(process.StandardInput.BaseStream);
            }

            process.StandardInput.Close();
            process.WaitForExit();

            return process.StandardOutput.ReadToEnd().Trim();
        }

        #region Stage/Unstage

        public bool AssumeUnchangedFiles(IReadOnlyList<GitItemStatus> files, bool assumeUnchanged, out string allOutput)
        {
            files = files.Where(file => file.IsAssumeUnchanged != assumeUnchanged).ToList();

            if (files.Count == 0)
            {
                allOutput = "";
                return true;
            }

            ExecutionResult execution = _gitExecutable.Execute(
                new GitArgumentBuilder("update-index")
                {
                    { assumeUnchanged ? "--assume-unchanged" : "--no-assume-unchanged" },
                    "--stdin"
                },
                inputWriter =>
                {
                    foreach (GitItemStatus file in files)
                    {
                        UpdateIndex(inputWriter, file.Name);
                    }
                },
                SystemEncoding,
                throwOnErrorExit: false);

            allOutput = execution.AllOutput;
            return execution.ExitedSuccessfully;
        }

        public bool SkipWorktreeFiles(IReadOnlyList<GitItemStatus> files, bool skipWorktree, out string allOutput)
        {
            files = files.Where(file => file.IsSkipWorktree != skipWorktree).ToList();

            if (files.Count == 0)
            {
                allOutput = "";
                return true;
            }

            ExecutionResult execution = _gitExecutable.Execute(
                new GitArgumentBuilder("update-index")
                {
                    { skipWorktree ? "--skip-worktree" : "--no-skip-worktree" },
                    "--stdin"
                },
                inputWriter =>
                {
                    foreach (GitItemStatus file in files)
                    {
                        UpdateIndex(inputWriter, file.Name);
                    }
                },
                SystemEncoding);

            allOutput = execution.AllOutput;
            return execution.ExitedSuccessfully;
        }

        /// <summary>
        /// Stage files from worktree to index.
        /// </summary>
        /// <param name="files">The files to set the status for.</param>
        /// <param name="allOutput">stdout and stderr output.</param>
        /// <returns><see langword="true"/> if no errors occurred; otherwise, <see langword="false"/>.</returns>
        public bool StageFiles(IReadOnlyList<GitItemStatus> files, out string allOutput)
        {
            bool wereErrors = false;

            if (files.Count == 0)
            {
                allOutput = "";
                return !wereErrors;
            }

            StringBuilder output = new();
            List<GitItemStatus> nonDeletedFiles = files.Where(file => !file.IsDeleted).ToList();
            List<GitItemStatus> deletedFiles = files.Where(file => file.IsDeleted).ToList();

            if (nonDeletedFiles.Count != 0)
            {
                ExecutionResult execution = _gitExecutable.Execute(
                    UpdateIndexCmd(AppSettings.ShowErrorsWhenStagingFiles),
                    inputWriter =>
                    {
                        foreach (GitItemStatus file in nonDeletedFiles)
                        {
                            UpdateIndex(inputWriter, file.Name);
                        }
                    },
                    SystemEncoding,
                    throwOnErrorExit: false);

                wereErrors |= !execution.ExitedSuccessfully;
                output.AppendLine(execution.AllOutput);
            }

            if (deletedFiles.Count != 0)
            {
                ExecutionResult execution = _gitExecutable.Execute(
                    new GitArgumentBuilder("update-index")
                    {
                        "--remove",
                        "--stdin"
                    },
                    inputWriter =>
                    {
                        foreach (GitItemStatus file in deletedFiles)
                        {
                            UpdateIndex(inputWriter, file.Name);
                        }
                    },
                    SystemEncoding,
                    throwOnErrorExit: false);

                wereErrors |= !execution.ExitedSuccessfully;
                output.Append(execution.AllOutput);
            }

            allOutput = output.ToString();
            return !wereErrors;
        }

        public void StageFile(string file)
        {
            _gitExecutable.RunCommand(
                new GitArgumentBuilder("update-index")
                {
                    "--add",
                    file.ToPosixPath().Quote()
                });
        }

        /// <summary>
        /// Unstage files from index to worktree.
        /// </summary>
        /// <param name="files">The files to set the status for.</param>
        /// <param name="allOutput">stdout and stderr output.</param>
        /// <returns><see langword="true"/> if no errors occurred; otherwise, <see langword="false"/>.</returns>
        public bool UnstageFiles(IReadOnlyList<GitItemStatus> files, out string allOutput)
        {
            bool wereErrors = false;

            if (files.Count == 0)
            {
                allOutput = "";
                return !wereErrors;
            }

            StringBuilder output = new();
            List<GitItemStatus> nonNewFiles = files.Where(file => !file.IsDeleted).ToList();
            List<GitItemStatus> newFiles = files.Where(file => file.IsDeleted).ToList();

            if (nonNewFiles.Count != 0)
            {
                GitArgumentBuilder sb = new("reset") { "--" };
                foreach (GitItemStatus file in nonNewFiles)
                {
                    sb.Add(file.Name.ToPosixPath().QuoteNE());
                }

                ExecutionResult execution = _gitExecutable.Execute(sb);

                output.AppendLine(execution.AllOutput);
            }

            if (newFiles.Count != 0)
            {
                ExecutionResult execution = _gitExecutable.Execute(
                new GitArgumentBuilder("update-index")
                {
                    "--force-remove",
                    "--stdin"
                },
                inputWriter =>
                {
                    foreach (GitItemStatus file in newFiles)
                    {
                        UpdateIndex(inputWriter, file.Name);
                    }
                },
                SystemEncoding);

                output.Append(execution.AllOutput);
            }

            allOutput = output.ToString();
            return !wereErrors;
        }

        public bool BatchUnstageFiles(IEnumerable<GitItemStatus> files, Action<BatchProgressEventArgs>? progressCallback = null)
        {
            List<GitItemStatus> filesToUnstage = [];
            List<string> filesToRemove = [];
            bool shouldRescanChanges = false;
            foreach (GitItemStatus item in files)
            {
                if (!item.IsNew)
                {
                    Assumes.True(item.Name is not null, "Item must have a name");

                    filesToRemove.Add(item.Name);

                    if (item.IsRenamed)
                    {
                        Assumes.True(item.OldName is not null, "Renamed item must have an old name");
                        filesToRemove.Add(item.OldName);
                    }

                    if (item.IsDeleted)
                    {
                        shouldRescanChanges = true;
                    }
                }
                else
                {
                    filesToUnstage.Add(item);
                }
            }

            if (filesToRemove.Count > 0)
            {
                ArgumentString args = Commands.Reset(ResetMode.ResetIndex, "HEAD");
                _gitExecutable.RunBatchCommand(new ArgumentBuilder() { args }
                    .BuildBatchArgumentsForFiles(filesToRemove),
                    progressCallback);
            }

            UnstageFiles(filesToUnstage, out _);

            return shouldRescanChanges;
        }

        public async Task<bool> AddInteractiveAsync(GitItemStatus file)
        {
            GitArgumentBuilder args = new("add")
            {
                "--patch",
                file.Name.Quote()
            };

            using IProcess process = _gitExecutable.Start(args, createWindow: true);
            return await process.WaitForExitAsync() == 0;
        }

        public async Task<bool> ResetInteractiveAsync(GitItemStatus file)
        {
            GitArgumentBuilder args = new("checkout")
            {
                "-p",
                file.Name.Quote()
            };

            using IProcess process = _gitExecutable.Start(args, createWindow: true);
            return await process.WaitForExitAsync() == 0;
        }

        private static void UpdateIndex(StreamWriter inputWriter, string? filename)
        {
            byte[] bytes = EncodingHelper.ConvertTo(
                SystemEncoding,
                $"\"{filename.ToPosixPath()}\"{inputWriter.NewLine}");

            inputWriter.BaseStream.Write(bytes, 0, bytes.Length);
        }

        private static GitArgumentBuilder UpdateIndexCmd(bool showErrorsWhenStagingFiles)
        {
            return new GitArgumentBuilder("update-index", gitOptions:
                            showErrorsWhenStagingFiles
                                ? default
                                : (ArgumentString)"-c core.safecrlf=false")
                    {
                        "--add",
                        "--stdin"
                    };
        }

        #endregion

        public bool InTheMiddleOfBisect()
        {
            return File.Exists(Path.Combine(GetGitDirectory(), "BISECT_START"));
        }

        public bool InTheMiddleOfRebase() => InTheMiddleOfGitOperation("applying");

        public bool InTheMiddleOfPatch() => InTheMiddleOfGitOperation("rebasing");

        private bool InTheMiddleOfGitOperation(string filename)
        {
            string rebaseDir = GetRebaseDir();
            return !File.Exists(rebaseDir + filename) && Directory.Exists(rebaseDir);
        }

        public bool InTheMiddleOfMerge()
        {
            return File.Exists(Path.Combine(GetGitDirectory(), "MERGE_HEAD"));
        }

        public bool InTheMiddleOfAction()
        {
            return InTheMiddleOfConflictedMerge() || InTheMiddleOfRebase();
        }

        public string RemoveRemote(string remoteName)
        {
            GitArgumentBuilder args = new("remote")
            {
                "rm",
                remoteName.QuoteNE()
            };
            return _gitExecutable.GetOutput(args);
        }

        public string RenameRemote(string remoteName, string newName)
        {
            GitArgumentBuilder args = new("remote")
            {
                "rename",
                remoteName.QuoteNE(),
                newName.QuoteNE()
            };
            return _gitExecutable.GetOutput(args);
        }

        public string AddRemote(string? name, string? path)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "Please enter a name.";
            }

            return _gitExecutable.GetOutput(
                new GitArgumentBuilder("remote")
                {
                    "add",
                    name.Quote(),
                    GetPathForGitExecution(path).QuoteNE()
                });
        }

        public IReadOnlyList<string> GetRemoteNames()
        {
            return _gitExecutable
                .Execute("remote")
                .StandardOutput
                .LazySplit('\n', StringSplitOptions.RemoveEmptyEntries)
                .ToList();
        }

        public async Task<IReadOnlyList<Remote>> GetRemotesAsync()
        {
            ExecutionResult result = await _gitExecutable.ExecuteAsync(new GitArgumentBuilder("remote") { "-v" }, throwOnErrorExit: false);
            ////TODO: Handle non-empty result.StandardError if not result.ExitedSuccessfully
            return result.ExitedSuccessfully
                ? ParseRemotes(result)
                : Array.Empty<Remote>();

            IReadOnlyList<Remote> ParseRemotes(ExecutionResult result)
            {
                IEnumerable<string> lines = result.StandardOutput.LazySplit('\n', StringSplitOptions.RemoveEmptyEntries);
                List<Remote> remotes = [];

                // See tests for explanation of the format

                using IEnumerator<string> enumerator = lines.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string remoteLine = enumerator.Current;
                    Match remoteMatch = RemoteVerboseLineRegex().Match(remoteLine);
                    if (!remoteMatch.Success
                        || (remoteMatch.Groups["direction"].Value != "fetch"
                           && remoteMatch.Groups["direction"].Value != "push"))
                    {
                        // Ignore malformed and unknown entries
                        continue;
                    }

                    string name = remoteMatch.Groups["name"].Value;
                    string remoteUrl = remoteMatch.Groups["url"].Value;
                    if (PathUtil.IsLocalFile(remoteUrl))
                    {
                        remoteUrl = GetWindowsPath(remoteUrl).ToPosixPath();
                    }

                    if (remoteMatch.Groups["direction"].Value == "push")
                    {
                        if (remotes.Count <= 0 || name != remotes[^1].Name)
                        {
                            throw new Exception("Unable to update remote pushurl for command output: " + remoteLine);
                        }

                        remotes[^1].PushUrls.Add(remoteUrl);
                        continue;
                    }

                    if (!enumerator.MoveNext())
                    {
                        throw new Exception("Remote URLs should appear in pairs, no pushurl for fetch: " + remoteLine);
                    }

                    string pushLine = enumerator.Current;
                    Match pushMatch = RemoteVerboseLineRegex().Match(pushLine);
                    if (!pushMatch.Success || pushMatch.Groups["direction"].Value != "push")
                    {
                        throw new Exception("Unable to parse git remote push URL line: " + pushLine);
                    }

                    string pushUrl = pushMatch.Groups["url"].Value;
                    if (PathUtil.IsLocalFile(pushUrl))
                    {
                        pushUrl = GetWindowsPath(pushUrl).ToPosixPath();
                    }

                    if (name != pushMatch.Groups["name"].Value)
                    {
                        throw new Exception("Fetch and push remote names must match: " +
                            remoteLine + ", " + pushLine);
                    }

                    remotes.Add(new Remote(name, remoteUrl, pushUrl));
                }

                return remotes;
            }
        }

        public IEnumerable<string> GetSettings(string setting)
        {
            return ((ConfigFileSettings)LocalConfigFile).GetValues(setting);
        }

        public string GetSetting(string setting) => LocalConfigFile.GetValue(setting);
        public T? GetSetting<T>(string setting) where T : struct => LocalConfigFile.GetValue<T>(setting);

        public string GetEffectiveSetting(string setting) => EffectiveConfigFile.GetValue(setting);
        public T? GetEffectiveSetting<T>(string setting) where T : struct => EffectiveConfigFile.GetValue<T>(setting);

        public SettingsSource GetEffectiveSettingsByPath(string path)
        {
            return ((ConfigFileSettings)EffectiveConfigFile).ByPath(path);
        }

        public string? GetGitSetting(string setting, string scopeArg, bool cache = false)
        {
            GitArgumentBuilder args = new("config") { "--includes", scopeArg, "--get", setting };
            ExecutionResult result = GitExecutable.Execute(args, cache: cache ? GitCommandCache : null, throwOnErrorExit: false);

            // Handle no value set, is error code 1: https://git-scm.com/docs/git-config#_description
            const int ConfigKeyInvalidOrNotSet = 1;
            if (result.ExitCode == ConfigKeyInvalidOrNotSet)
            {
                return null;
            }

            result.ThrowIfErrorExit("Error getting config value");

            return result.StandardOutput.Trim();
        }

        public string? GetEffectiveGitSetting(string setting, bool cache = false)
        {
            return GetGitSetting(setting, scopeArg: "", cache);
        }

        public void UnsetSetting(string setting)
        {
            SetSetting(setting, null);
        }

        public void SetSetting(string setting, string? value)
        {
            ((ConfigFileSettings)LocalConfigFile).SetValue(setting, value);
        }

        internal GitArgumentBuilder GetStashesCmd(bool noLocks)
        {
            return new GitArgumentBuilder("stash", gitOptions: noLocks ? (ArgumentString)"--no-optional-locks" : default)
                { "list" };
        }

        public IReadOnlyList<GitStash> GetStashes(bool noLocks = false)
        {
            GitArgumentBuilder args = GetStashesCmd(noLocks);
            string[] lines = _gitExecutable.GetOutput(args).Split(Delimiters.LineFeed);

            List<GitStash> stashes = new(lines.Length);

            foreach (string line in lines)
            {
                if (GitStash.TryParse(line, out GitStash? stash))
                {
                    stashes.Add(stash);
                }
            }

            return stashes;
        }

        public async Task<ExecutionResult> GetSingleDifftoolAsync(
            ObjectId? firstId,
            ObjectId? secondId,
            string? fileName,
            string? oldFileName,
            ArgumentString extraDiffArguments,
            bool cacheResult,
            string extraCacheKey,
            bool isTracked,
            bool useGitColoring,
            CancellationToken cancellationToken)
        {
            // fix refs slashes
            fileName = fileName.ToPosixPath();
            oldFileName = oldFileName.ToPosixPath();
            string? firstRevision = firstId?.ToString();
            string? secondRevision = secondId?.ToString();

            string? diffOptions = _revisionDiffProvider.Get(firstRevision, secondRevision, fileName, oldFileName, isTracked);

            GitArgumentBuilder args = new("difftool", commandConfiguration: null, "--no-pager")
            {
                "--find-renames",
                "--find-copies",
                "-y",
                extraDiffArguments,
                diffOptions
            };

            CommandCache? cache = cacheResult
                        && !string.IsNullOrEmpty(secondRevision)
                        && !string.IsNullOrEmpty(firstRevision)
                        && !secondRevision.IsArtificial()
                        && !firstRevision.IsArtificial()
                ? GitCommandCache
                : null;

            ExecutionResult result = await _gitExecutable.ExecuteAsync(
                args,
                writeInput: null,
                outputEncoding: LosslessEncoding,
                cache: cache,
                extraCacheKey,
                stripAnsiEscapeCodes: !useGitColoring,
                throwOnErrorExit: false,
                cancellationToken: cancellationToken);

            return result;
        }

        public async Task<(Patch? Patch, string? ErrorMessage)> GetSingleDiffAsync(
            ObjectId? firstId,
            ObjectId? secondId,
            string? fileName,
            string? oldFileName,
            string extraDiffArguments,
            Encoding encoding,
            bool cacheResult,
            bool isTracked,
            bool useGitColoring,
            IGitCommandConfiguration commandConfiguration,
            CancellationToken cancellationToken)
        {
            // fix refs slashes
            fileName = fileName.ToPosixPath();
            oldFileName = oldFileName.ToPosixPath();
            string? firstRevision = firstId?.ToString();
            string? secondRevision = secondId?.ToString();

            string? diffOptions = _revisionDiffProvider.Get(firstRevision, secondRevision, fileName, oldFileName, isTracked);

            GitArgumentBuilder args = new("diff", commandConfiguration)
            {
                "--no-ext-diff",
                "--find-renames",
                "--find-copies",
                { useGitColoring, "--color=always" },
                { AppSettings.UseHistogramDiffAlgorithm, "--histogram" },
                extraDiffArguments,
                diffOptions
            };

            CommandCache? cache = cacheResult &&
                        !string.IsNullOrEmpty(secondRevision) &&
                        !string.IsNullOrEmpty(firstRevision) &&
                        !secondRevision.IsArtificial() &&
                        !firstRevision.IsArtificial()
                ? GitCommandCache
                : null;

            ExecutionResult result = await _gitExecutable.ExecuteAsync(
                args,
                cache: cache,
                outputEncoding: LosslessEncoding,
                stripAnsiEscapeCodes: !useGitColoring,
                throwOnErrorExit: false,
                cancellationToken: cancellationToken);
            if (!result.ExitedSuccessfully)
            {
                return (Patch: null, ErrorMessage: $"{result.StandardError}{Environment.NewLine}Git command (exit code: {result.ExitCode}): {args}{Environment.NewLine}");
            }

            string patch = result.StandardOutput;
            IReadOnlyList<Patch> patches = PatchProcessor.CreatePatchesFromString(patch, new Lazy<Encoding>(() => encoding)).ToList();

            return (Patch: GetPatch(patches, fileName, oldFileName), ErrorMessage: null);
        }

        public async Task<ExecutionResult> GetRangeDiffAsync(
            ObjectId firstId,
            ObjectId secondId,
            ObjectId? firstBase,
            ObjectId? secondBase,
            string extraDiffArguments,
            string? pathFilter,
            bool useGitColoring,
            IGitCommandConfiguration commandConfiguration,
            CancellationToken cancellationToken)
        {
            // range-diff is not possible for artificial commits, use HEAD
            string first = firstId.IsArtificial ? "HEAD" : firstId.ToString();
            string second = secondId.IsArtificial ? "HEAD" : secondId.ToString();

            if ((firstBase?.IsArtificial is true) || (secondBase?.IsArtificial is true))
            {
                throw new ArgumentException($"Cannot get range diff for artificial commit base of A: {firstBase} or base of B: {secondBase}.");
            }

            // Supported since Git 2.19 (checks when adding the command)
            GitArgumentBuilder args = new("range-diff", commandConfiguration)
            {
                "--find-renames",
                "--find-copies",
                { AppSettings.UseHistogramDiffAlgorithm, "--histogram" },
                { useGitColoring, "--color=always" },
                extraDiffArguments,
                { firstBase is null || secondBase is null,  $"{first}...{second}", $"{firstBase}..{first} {secondBase}..{second}" },
                { GitVersion.SupportRangeDiffPath && !string.IsNullOrWhiteSpace(pathFilter), "--" },
                { GitVersion.SupportRangeDiffPath && !string.IsNullOrWhiteSpace(pathFilter), pathFilter }
            };

            ExecutionResult result = await _gitExecutable.ExecuteAsync(
                args,
                cache: GitCommandCache,
                outputEncoding: LosslessEncoding,
                stripAnsiEscapeCodes: !useGitColoring,
                throwOnErrorExit: false,
                cancellationToken: cancellationToken);

            return result;
        }

        private static Patch? GetPatch(IReadOnlyList<Patch> patches, string? fileName, string? oldFileName)
        {
            foreach (Patch patch in patches)
            {
                if (fileName == patch.FileNameB && (fileName == patch.FileNameA || oldFileName == patch.FileNameA))
                {
                    return patch;
                }
            }

            return patches.Count != 0
                ? patches[^1]
                : null;
        }

        public string GetStatusText(bool untracked)
        {
            return _gitExecutable.GetOutput(new GitArgumentBuilder("status")
            {
                "-s",
                { untracked, "-u" }
            });
        }

        private ExecutionResult GetGrepFiles(ObjectId objectId, string grepString, CancellationToken cancellationToken = default)
        {
            bool noCache = objectId.IsArtificial;

            return _gitExecutable.Execute(
                new GitArgumentBuilder("grep")
                {
                    "--files-with-matches",
                    "-z",
                    AppSettings.GitGrepUserArguments.Value,
                    { AppSettings.GitGrepIgnoreCase.Value, "--ignore-case" },
                    { AppSettings.GitGrepMatchWholeWord.Value, "--word-regexp" },
                    grepString,
                    !objectId.IsArtificial ? objectId.ToString() : objectId == ObjectId.IndexId ? "--cached" : "",
                    "--"
                },
                cache: noCache ? null : GitCommandCache,
                throwOnErrorExit: false,
                cancellationToken: cancellationToken);
        }

        public IReadOnlyList<GitItemStatus> GetGrepFilesStatus(ObjectId objectId, string grepString, CancellationToken cancellationToken)
        {
            List<GitItemStatus> result = [];
            ExecutionResult exec = GetGrepFiles(objectId, grepString, cancellationToken);
            if (!exec.ExitedSuccessfully)
            {
                // Cannot see difference from error and no matches
                return [];
            }

            foreach (string file in exec.StandardOutput.LazySplit('\0', StringSplitOptions.RemoveEmptyEntries))
            {
                int startIndex = file.IndexOf(':') + 1;
                result.Add(new GitItemStatus(file[startIndex..])
                {
                    GrepString = grepString,

                    // Assume this file is handled by Git, may not be entirely correct for worktree
                    IsTracked = true
                });
            }

            return result;
        }

        public async Task<ExecutionResult> GetGrepFileAsync(
            ObjectId objectId,
            string fileName,
            ArgumentString extraArgs,
            string grepString,
            bool useGitColoring,
            IGitCommandConfiguration commandConfiguration,
            CancellationToken cancellationToken)
        {
            bool noCache = objectId.IsArtificial;

            GitArgumentBuilder args = new("grep", commandConfiguration: commandConfiguration)
            {
                "--line-number",
                { !useGitColoring, "--column" },
                { useGitColoring, "--color=always" },
                extraArgs,
                AppSettings.GitGrepUserArguments.Value,
                { AppSettings.GitGrepIgnoreCase.Value, "--ignore-case" },
                { AppSettings.GitGrepMatchWholeWord.Value, "--word-regexp" },
                grepString,
                !objectId.IsArtificial ? objectId.ToString() : objectId == ObjectId.IndexId ? "--cached" : "",
                "--",
                fileName
            };

            return await _gitExecutable.ExecuteAsync(
                args,
                cache: noCache ? null : GitCommandCache,
                throwOnErrorExit: false,
                stripAnsiEscapeCodes: !useGitColoring,
                cancellationToken: cancellationToken);
        }

        public ExecutionResult GetDiffFiles(string? firstRevision, string? secondRevision, bool noCache = false, bool nullSeparated = false, CancellationToken cancellationToken = default)
        {
            noCache = noCache || firstRevision.IsArtificial() || secondRevision.IsArtificial();

            // It is possible that a commit does not exist, should not raise an error to the user:
            // * Create a commit in a submodule, do not push
            // * Commit submodule changes
            // * Open the repo in a worktree clone
            // * Checkout the commit
            // * Select the submodule in the diff tab
            // This should not raise a popup to the user, but describe the error message
            return _gitExecutable.Execute(
                new GitArgumentBuilder("diff")
                {
                    "--no-ext-diff",
                    "--find-renames",
                    "--find-copies",
                    "--name-status",
                    { nullSeparated, "-z" },
                    _revisionDiffProvider.Get(firstRevision, secondRevision)
                },
                cache: noCache ? null : GitCommandCache,
                throwOnErrorExit: false,
                cancellationToken: cancellationToken);
        }

        public IReadOnlyList<GitItemStatus> GetDiffFilesWithSubmodulesStatus(ObjectId? firstId, ObjectId? secondId, ObjectId? parentToSecond, CancellationToken cancellationToken)
        {
            StagedStatus stagedStatus = GetStagedStatus(firstId, secondId, parentToSecond);
            IReadOnlyList<GitItemStatus> status = GetDiffFilesWithUntracked(firstId?.ToString(), secondId?.ToString(), stagedStatus, cancellationToken: cancellationToken);
            GetSubmoduleStatus(status, firstId, secondId, cancellationToken);
            return status;
        }

        /// <summary>
        /// If possible, find if files in a diff are index or worktree.
        /// </summary>
        /// <param name="firstId">from revision string.</param>
        /// <param name="secondId">to revision.</param>
        /// <param name="parentToSecond">The parent for the second revision.</param>
        /// <remarks>Git revisions are required to determine if <see cref="StagedStatus"/> allows stage/unstage.</remarks>
        public static StagedStatus GetStagedStatus(ObjectId? firstId, ObjectId? secondId, ObjectId? parentToSecond)
        {
            StagedStatus staged;
            if (firstId == ObjectId.IndexId && secondId == ObjectId.WorkTreeId)
            {
                staged = StagedStatus.WorkTree;
            }
            else if (firstId == parentToSecond && secondId == ObjectId.IndexId)
            {
                staged = StagedStatus.Index;
            }
            else if (firstId is not null && !firstId.IsArtificial &&
                     secondId is not null && !secondId.IsArtificial)
            {
                // This cannot be a worktree/index file
                staged = StagedStatus.None;
            }
            else
            {
                staged = StagedStatus.Unknown;
            }

            return staged;
        }

        public IReadOnlyList<GitItemStatus> GetDiffFilesWithUntracked(string? firstRevision, string? secondRevision, StagedStatus stagedStatus, bool noCache = false,
            CancellationToken cancellationToken = default)
        {
            if (stagedStatus is StagedStatus.WorkTree or StagedStatus.Index)
            {
                IReadOnlyList<GitItemStatus> status = GetAllChangedFilesWithSubmodulesStatus(cancellationToken: cancellationToken);
                return status.Where(x => x.Staged == stagedStatus).ToList();
            }

            ExecutionResult exec = GetDiffFiles(firstRevision, secondRevision, noCache: noCache, nullSeparated: true, cancellationToken);
            List<GitItemStatus> result = GetDiffChangedFilesFromString(exec.StandardOutput, stagedStatus);
            if (!exec.ExitedSuccessfully)
            {
                result.Add(createErrorGitItemStatus(exec.StandardError));
            }

            if (firstRevision == GitRevision.WorkTreeGuid || secondRevision == GitRevision.WorkTreeGuid)
            {
                // For worktree the untracked must be added too
                // Note that this may add a second GitError, this is a separate Git command
                GitItemStatus[] files = GetAllChangedFilesWithSubmodulesStatus(cancellationToken: cancellationToken).Where(x =>
                    ((x.Staged == StagedStatus.WorkTree && x.IsNew) || x.IsStatusOnly)).ToArray();
                if (firstRevision == GitRevision.WorkTreeGuid)
                {
                    // The file is seen as "deleted" in 'to' revision
                    foreach (GitItemStatus item in files)
                    {
                        item.IsNew = false;
                        item.IsDeleted = true;
                        result.Add(item);
                    }
                }
                else
                {
                    result.AddRange(files);
                }
            }

            return result;
        }

        public IReadOnlyList<GitItemStatus> GetStashDiffFiles(string stashName)
        {
            List<GitItemStatus> resultCollection = GetDiffFilesWithUntracked(stashName + "^", stashName, StagedStatus.None, true).ToList();

            // add - optionally stashed - untracked files
            GitArgumentBuilder args = new("log")
            {
                $"{stashName}^3".Quote(),
                "--pretty=format:\"%T\"",
                "--max-count=1"
            };
            ExecutionResult executionResult = _gitExecutable.Execute(args, throwOnErrorExit: false);
            if (executionResult.ExitedSuccessfully && ObjectId.TryParse(executionResult.StandardOutput, out ObjectId? treeId))
            {
                IReadOnlyList<GitItemStatus> files = GetTreeFiles(treeId, full: true);

                resultCollection.AddRange(files);
            }

            return resultCollection;
        }

        public IReadOnlyList<GitItemStatus> GetTreeFiles(ObjectId commitId, bool full)
        {
            IEnumerable<INamedGitItem> tree = GetTree(commitId, full);

            List<GitItemStatus> list = tree
                .Select(file => new GitItemStatus(name: file.Name)
                {
                    IsTracked = true,
                    IsNew = true,
                    IsChanged = false,
                    IsDeleted = false,
                    TreeGuid = file.ObjectId,
                    Staged = StagedStatus.None
                }).ToList();

            // Doesn't work with removed submodules
            IReadOnlyList<string> submodulesList = GetSubmodulesLocalPaths();
            foreach (GitItemStatus item in list)
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
            UntrackedFilesMode untrackedFiles = UntrackedFilesMode.Default, CancellationToken cancellationToken = default)
        {
            ExecutionResult exec = _gitExecutable.Execute(Commands.GetAllChangedFiles(excludeIgnoredFiles, untrackedFiles), throwOnErrorExit: false, cancellationToken: cancellationToken);
            List<GitItemStatus> result = _getAllChangedFilesOutputParser.Parse(exec.StandardOutput).ToList();
            if (!exec.ExitedSuccessfully)
            {
                // No simple way to pass the error message, create fake file
                result.Add(createErrorGitItemStatus(exec.StandardError));
            }

            if (!excludeAssumeUnchangedFiles || !excludeSkipWorktreeFiles)
            {
                GitArgumentBuilder args = new("ls-files") { "-v" };
                string lsOutput = _gitExecutable.GetOutput(args);

                if (!excludeAssumeUnchangedFiles)
                {
                    result.AddRange(GetAssumeUnchangedFilesFromString(lsOutput));
                }

                if (!excludeSkipWorktreeFiles)
                {
                    result.AddRange(GetSkipWorktreeFilesFromString(lsOutput, excludeAssumeUnchangedFiles));
                }
            }

            return result;

            static IReadOnlyList<GitItemStatus> GetAssumeUnchangedFilesFromString(string lsString)
            {
                List<GitItemStatus> result = [];

                foreach (string line in lsString.LazySplit('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    char statusCharacter = line[0];
                    if (char.IsUpper(statusCharacter))
                    {
                        // git-ls-files -v will return lowercase status characters for assume unchanged files
                        continue;
                    }

                    // Get a default status object, then set AssumeUnchanged
                    string fileName = line.SubstringAfter(' ');
                    GitItemStatus gitItemStatus = GitItemStatus.GetDefaultStatus(fileName);
                    gitItemStatus.IsAssumeUnchanged = true;
                    result.Add(gitItemStatus);
                }

                return result;
            }

            static IReadOnlyList<GitItemStatus> GetSkipWorktreeFilesFromString(string lsString, bool excludeAssumeUnchangedFiles)
            {
                List<GitItemStatus> result = [];

                foreach (string line in lsString.LazySplit('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    // If both AssumeUnchange and SkipWorktree is set, status will be 's',
                    // already handled in GetAssumeUnchangedFilesFromString()
                    char statusCharacter = line[0];
                    const char SkippedStatus = 'S';
                    const char SkippedStatusAssumeUnchanged = 's';
                    if (statusCharacter is not SkippedStatus && (!excludeAssumeUnchangedFiles || statusCharacter is not SkippedStatusAssumeUnchanged))
                    {
                        continue;
                    }

                    // Get a default status object, then set SkipWorktree
                    string fileName = line.SubstringAfter(' ');
                    GitItemStatus gitItemStatus = GitItemStatus.GetDefaultStatus(fileName);
                    gitItemStatus.IsSkipWorktree = true;
                    result.Add(gitItemStatus);
                }

                return result;
            }
        }

        public IReadOnlyList<GitItemStatus> GetAllChangedFilesWithSubmodulesStatus(CancellationToken cancellationToken = default)
            => GetAllChangedFilesWithSubmodulesStatus(excludeIgnoredFiles: true, excludeAssumeUnchangedFiles: true, excludeSkipWorktreeFiles: true, untrackedFiles: UntrackedFilesMode.Default, cancellationToken);

        public IReadOnlyList<GitItemStatus> GetAllChangedFilesWithSubmodulesStatus(bool excludeIgnoredFiles, bool excludeAssumeUnchangedFiles, bool excludeSkipWorktreeFiles,
            UntrackedFilesMode untrackedFiles, CancellationToken cancellationToken)
        {
            IReadOnlyList<GitItemStatus> status = GetAllChangedFiles(excludeIgnoredFiles, excludeAssumeUnchangedFiles, excludeSkipWorktreeFiles, untrackedFiles, cancellationToken);
            GetCurrentSubmoduleStatus(status);
            return status;
        }

        private void GetCurrentSubmoduleStatus(IReadOnlyList<GitItemStatus> status)
        {
            foreach (GitItemStatus item in status)
            {
                if (item.IsSubmodule)
                {
                    GitItemStatus localItem = item;
                    localItem.SetSubmoduleStatus(ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                    {
                        return await SubmoduleHelpers.GetCurrentSubmoduleChangesAsync(this, localItem.Name, localItem.OldName, localItem.Staged == StagedStatus.Index)
                            .ConfigureAwait(false);
                    }));
                }
            }
        }

        private void GetSubmoduleStatus(IReadOnlyList<GitItemStatus> status, ObjectId? firstId, ObjectId? secondId, CancellationToken cancellationToken)
        {
            foreach (GitItemStatus item in status.Where(i => i.IsSubmodule))
            {
                item.SetSubmoduleStatus(
                    ThreadHelper.JoinableTaskFactory.RunAsync(
                        async () =>
                        {
                            await TaskScheduler.Default.SwitchTo(alwaysYield: true);
                            return await SubmoduleHelpers.GetCurrentSubmoduleChangesAsync(this, item.Name, item.OldName, firstId, secondId, cancellationToken)
                                .ConfigureAwait(false);
                        }));
            }
        }

        public IReadOnlyList<GitItemStatus> GetIndexFiles()
        {
            GitArgumentBuilder args = new("diff")
            {
                "--no-ext-diff",
                "--find-renames",
                "--find-copies",
                "-z",
                "--name-status",
                "--cached"
            };
            ExecutionResult exec = _gitExecutable.Execute(args, throwOnErrorExit: false);
            if (exec.ExitedSuccessfully)
            {
                return GetDiffChangedFilesFromString(exec.StandardOutput, StagedStatus.Index);
            }

            // This command is a little more expensive because it will return both staged and unstaged files
            ArgumentString command = Commands.GetAllChangedFiles(excludeIgnoredFiles: true, UntrackedFilesMode.No);
            exec = _gitExecutable.Execute(command, throwOnErrorExit: false);
            List<GitItemStatus> res = _getAllChangedFilesOutputParser.Parse(exec.StandardOutput)
                                            .Where(item => (item.Staged == StagedStatus.Index || item.IsStatusOnly))
                                            .ToList();
            if (!exec.ExitedSuccessfully)
            {
                // No simple way to pass the error message, create fake file
                res.Add(createErrorGitItemStatus(exec.StandardError));
            }

            return res;
        }

        /// <summary>
        /// Parse the output from git-diff --name-status.
        /// </summary>
        /// <param name="statusString">output from the git command.</param>
        /// <param name="staged">required to determine if <see cref="StagedStatus"/> allows stage/unstage.</param>
        /// <returns>list with the parsed GitItemStatus.</returns>
        /// <seealso href="https://git-scm.com/docs/git-diff"/>
        private List<GitItemStatus> GetDiffChangedFilesFromString(string statusString, StagedStatus staged)
            => _getAllChangedFilesOutputParser.GetAllChangedFilesFromString_v1(statusString, true, staged);

        public IReadOnlyList<GitItemStatus> GetIndexFilesWithSubmodulesStatus()
        {
            IReadOnlyList<GitItemStatus> status = GetIndexFiles();
            GetCurrentSubmoduleStatus(status);
            return status;
        }

        public IReadOnlyList<GitItemStatus> GetWorkTreeFiles()
        {
            return GetAllChangedFiles().Where(x => x.Staged == StagedStatus.WorkTree).ToArray();
        }

        public IReadOnlyList<GitItemStatus> GitStatus(UntrackedFilesMode untrackedFilesMode, IgnoreSubmodulesMode ignoreSubmodulesMode = IgnoreSubmodulesMode.None)
        {
            ArgumentString args = Commands.GetAllChangedFiles(true, untrackedFilesMode, ignoreSubmodulesMode);
            ExecutionResult exec = _gitExecutable.Execute(args, throwOnErrorExit: false);
            List<GitItemStatus> result = _getAllChangedFilesOutputParser.Parse(exec.StandardOutput).ToList();
            if (!exec.ExitedSuccessfully)
            {
                // No simple way to pass the error message, create fake file
                result.Add(createErrorGitItemStatus(exec.StandardError));
            }

            return result;
        }

        /// <summary>Indicates whether there are any changes to the repository,
        ///  including any untracked files or directories; excluding submodules.</summary>
        public bool IsDirtyDir()
        {
            return GitStatus(UntrackedFilesMode.All, IgnoreSubmodulesMode.All).Count > 0;
        }

        public async Task<Patch?> GetCurrentChangesAsync(string? fileName, string? oldFileName, bool staged, string extraDiffArguments, Encoding? encoding = null, bool noLocks = false)
        {
            string output = await _gitExecutable.GetOutputAsync(Commands.GetCurrentChanges(fileName, oldFileName, staged, extraDiffArguments, noLocks),
                outputEncoding: LosslessEncoding).ConfigureAwait(false);

            IReadOnlyList<Patch> patches = PatchProcessor.CreatePatchesFromString(output, new Lazy<Encoding>(() => encoding ?? FilesEncoding)).ToList();

            return GetPatch(patches, fileName, oldFileName);
        }

        private async Task<string?> GetFileContentsAsync(string? path)
        {
            GitArgumentBuilder args = new("show") { $"HEAD:{path.ToPosixPath().Quote()}" };
            ExecutionResult result = await _gitExecutable.ExecuteAsync(args, throwOnErrorExit: false).ConfigureAwaitRunInline();

            return result.ExitedSuccessfully
                ? result.StandardOutput
                : null;
        }

        /// <summary>
        /// Get the file contents for the HEAD commit
        /// </summary>
        /// <param name="file">The Git status item</param>
        /// <returns>An awaitable task with the requested file contents.</returns>
        public async Task<string?> GetFileContentsAsync(GitItemStatus file)
        {
            if (!file.IsTracked || (file.Staged == StagedStatus.Index && file.IsNew))
            {
                // File is not in Git
                return null;
            }

            StringBuilder contents = new();
            string? currentContents = await GetFileContentsAsync(file.Name).ConfigureAwaitRunInline();
            if (currentContents is not null)
            {
                contents.Append(currentContents);
            }

            if (file.OldName is not null)
            {
                string? oldContents = await GetFileContentsAsync(file.OldName).ConfigureAwaitRunInline();
                if (oldContents is not null)
                {
                    contents.Append(oldContents);
                }
            }

            return contents.Length > 0 ? contents.ToString() : null;
        }

        public void UnstageFile(string file)
        {
            GitArgumentBuilder args = new("rm")
            {
                "--cached",
                file.ToPosixPath().QuoteNE()
            };
            _gitExecutable.RunCommand(args);
        }

        public void UnstageFileToRemove(string file)
        {
            ArgumentString args = Commands.Reset(ResetMode.ResetIndex, "HEAD", file);
            _gitExecutable.RunCommand(args);
        }

        /// <summary>Dirty but fast. This sometimes fails.</summary>
        public static string GetSelectedBranchFast(string? repositoryPath, bool emptyIfDetached = false)
        {
            if (string.IsNullOrEmpty(repositoryPath))
            {
                return string.Empty;
            }

            string headFileContents;
            try
            {
                // eg. "/path/to/repo/.git/HEAD"
                string headFileName = Path.Combine(GetGitDirectory(repositoryPath), "HEAD");

                if (!File.Exists(headFileName))
                {
                    return string.Empty;
                }

                headFileContents = File.ReadAllText(headFileName, SystemEncoding);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is SecurityException)
            {
                // ignore inaccessible file
                return string.Empty;
            }

            // eg. "ref: refs/heads/master"
            //     "9601551c564b48208bccd50b705264e9bd68140d"

            if (!headFileContents.StartsWith("ref: "))
            {
                return emptyIfDetached ? string.Empty : DetachedHeadParser.DetachedBranch;
            }

            const string prefix = "ref: refs/heads/";

            if (!headFileContents.StartsWith(prefix))
            {
                return string.Empty;
            }

            return headFileContents[prefix.Length..].TrimEnd();
        }

        public string GetSelectedBranch(bool emptyIfDetached = false)
        {
            string head = GetSelectedBranchFast(WorkingDir, emptyIfDetached);

            if (!string.IsNullOrEmpty(head))
            {
                return head;
            }

            GitArgumentBuilder args = new("symbolic-ref")
            {
                "--quiet",
                "HEAD"
            };
            ExecutionResult result = _gitExecutable.Execute(args, throwOnErrorExit: false);

            return result.ExitedSuccessfully
                ? result.StandardOutput
                : emptyIfDetached ? string.Empty : DetachedHeadParser.DetachedBranch;
        }

        public bool IsDetachedHead()
        {
            return DetachedHeadParser.IsDetachedHead(GetSelectedBranch());
        }

        public string GetCurrentRemote()
        {
            string remote = GetSetting(string.Format(SettingKeyString.BranchRemote, GetSelectedBranch()));
            return remote;
        }

        public string GetRemoteBranch(string branch)
        {
            string remote = GetSetting(string.Format(SettingKeyString.BranchRemote, branch));
            string merge = GetSetting($"branch.{branch}.merge");

            if (string.IsNullOrEmpty(remote) || string.IsNullOrEmpty(merge))
            {
                return "";
            }

            return $"{remote}/{merge.RemovePrefix(GitRefName.RefsHeadsPrefix)}";
        }

        public IEnumerable<IGitRef> GetRemoteBranches()
        {
            return GetRefs(RefsFilter.Remotes);
        }

        public IReadOnlyList<IGitRef> GetRemoteServerRefs(string remote, bool tags, bool branches, out string? errorOutput, CancellationToken cancellationToken)
        {
            ExecutionResult executionResult = !tags && !branches
                ? new() // TODO is this an error?
                : _gitExecutable.Execute(new GitArgumentBuilder("ls-remote")
                    {
                        { tags, "--tags" },
                        { branches, "--heads" },
                        remote.ToPosixPath().QuoteNE()
                    },
                    throwOnErrorExit: false,
                    cancellationToken: cancellationToken);

            // TODO AllOutput is parsed at errors, can this be detected better?
            string output = executionResult.AllOutput;

            if (executionResult.ExitedSuccessfully)
            {
                errorOutput = null;
                return ParseRefs(executionResult.StandardOutput);
            }

            errorOutput = output;
            return Array.Empty<IGitRef>();
        }

        /// <summary>
        /// Get the Git refs.
        /// </summary>
        /// <param name="getRef">Combined refs to search for.</param>
        /// <returns>All Git refs.</returns>
        public IReadOnlyList<IGitRef> GetRefs(RefsFilter getRef)
        {
            // We do not want to lock the repo for background operations.
            // The primary use of 'noLocks' is to run git-status the commit count as a background operation,
            // but to run the same in a foreground for FormCommit.
            //
            // Assume that all GetRefs() are done in the background, which may not be correct in the future.
            const bool noLocks = true;

            ArgumentString cmd = Commands.GetRefs(getRef, noLocks, AppSettings.RefsSortBy, AppSettings.RefsSortOrder);
            ExecutionResult result = _gitExecutable.Execute(cmd, throwOnErrorExit: false);
            return result.ExitedSuccessfully
                ? ParseRefs(result.StandardOutput)
                : Array.Empty<IGitRef>();
        }

        public async Task<string[]> GetMergedBranchesAsync(bool includeRemote, bool fullRefname, string? commit, CancellationToken cancellationToken)
        {
            ExecutionResult result = await _gitExecutable
                .ExecuteAsync(Commands.MergedBranches(includeRemote, fullRefname, commit), throwOnErrorExit: false, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            ////TODO: Handle non-empty result.StandardError
            return result.StandardOutput.Split(Delimiters.LineFeed, StringSplitOptions.RemoveEmptyEntries);
        }

        public IEnumerable<string> GetMergedBranches(bool includeRemote = false)
        {
            return _gitExecutable
                .GetOutput(Commands.MergedBranches(includeRemote))
                .LazySplit('\n', StringSplitOptions.RemoveEmptyEntries);
        }

        public IReadOnlyList<string> GetMergedRemoteBranches()
        {
            const string remoteBranchPrefixForMergedBranches = "remotes/";
            const string refsPrefix = "refs/";

            IReadOnlyList<string> remotes = GetRemoteNames();
            ExecutionResult result = _gitExecutable.Execute(Commands.MergedBranches(includeRemote: true));
            LazyStringSplit lines = result.StandardOutput.LazySplit('\n');

            return lines
                .Select(b => b.Trim())
                .Where(b => b.StartsWith(remoteBranchPrefixForMergedBranches))
                .Select(b => string.Concat(refsPrefix, b))
                .Where(b => !string.IsNullOrEmpty(GitRefName.GetRemoteName(b, remotes)))
                .ToList();
        }

        public IReadOnlyList<IGitRef> ParseRefs(string refList)
        {
            MatchCollection matches = RefRegex().Matches(refList);

            List<IGitRef> gitRefs = [];
            Dictionary<string, GitRef> headByRemote = [];

            foreach (Match match in matches)
            {
                string refName = match.Groups["refname"].Value;
                ObjectId objectId = ObjectId.Parse(refList, match.Groups["objectid"]);
                string remoteName = GitRefName.GetRemoteName(refName);
                GitRef head = new(this, objectId, refName, remoteName);

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
            foreach (IGitRef gitRef in gitRefs)
            {
                if (headByRemote.TryGetValue(gitRef.Remote, out GitRef defaultHead) &&
                    gitRef.ObjectId == defaultHead.ObjectId)
                {
                    headByRemote.Remove(gitRef.Remote);
                }
            }

            gitRefs.AddRange(headByRemote.Values);

            return gitRefs;
        }

        public IReadOnlyList<string> GetAllBranchesWhichContainGivenCommit(ObjectId objectId, bool getLocal, bool getRemote, CancellationToken cancellationToken)
        {
            if (!getLocal && !getRemote)
            {
                return Array.Empty<string>();
            }

            GitArgumentBuilder args = new("branch")
            {
                { getRemote && getLocal, "-a" },
                { getRemote && !getLocal, "-r" },
                "--contains",
                objectId
            };
            ExecutionResult exec = _gitExecutable.Execute(args, throwOnErrorExit: false, cancellationToken: cancellationToken);
            if (!exec.ExitedSuccessfully)
            {
                // Error occurred, no matches (no error presented to the user)
                return Array.Empty<string>();
            }

            string[] result = exec.StandardOutput.Split(Delimiters.LineFeedAndCarriageReturn, StringSplitOptions.RemoveEmptyEntries);

            // Remove symlink targets as in "origin/HEAD -> origin/master"
            for (int i = 0; i < result.Length; i++)
            {
                string item = result[i];

                // remove prepended branch state "* ", "+ ", "  "
                if (item.Length >= 2 && item[1] == ' ' && item[0] is (' ' or '*' or '+'))
                {
                    item = item[2..];
                }

                if (getRemote)
                {
                    int idx = item.IndexOf(" ->", StringComparison.Ordinal);
                    if (idx >= 0)
                    {
                        item = item[..idx];
                    }
                }

                result[i] = item;
            }

            return result;
        }

        public IReadOnlyList<string> GetAllTagsWhichContainGivenCommit(ObjectId objectId, CancellationToken cancellationToken)
        {
            ExecutionResult exec = _gitExecutable.Execute($"tag --contains {objectId}", throwOnErrorExit: false, cancellationToken: cancellationToken);
            if (!exec.ExitedSuccessfully)
            {
                // Error occurred, no matches (no error presented to the user)
                return Array.Empty<string>();
            }

            return exec.StandardOutput.Split(Delimiters.GitOutput, StringSplitOptions.RemoveEmptyEntries);
        }

        public string? GetTagMessage(string? tag, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return null;
            }

            tag = tag.Trim();

            ExecutionResult exec = _gitExecutable.Execute($"cat-file -p {tag}", throwOnErrorExit: false, cancellationToken: cancellationToken);
            if (!exec.ExitedSuccessfully)
            {
                // Error occurred, no message (no error presented to the user)
                return null;
            }

            string output = exec.StandardOutput;
            string[] messageLines = output.Split(
                new string[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None);

            if (messageLines.Length <= StandardCatFileTagHeaderLength)
            {
                return null;
            }

            StringBuilder annotationBuilder = new();

            // skip the last line as it will always be an empty line (for nice console output)
            for (int i = StandardCatFileTagHeaderLength; i < messageLines.Length - 1; ++i)
            {
                annotationBuilder.AppendLine(messageLines[i]);
            }

            // return message, trimming off last new line (added by AppendLine)
            return annotationBuilder.ToString().Trim();
        }

        /// <summary>
        /// Returns list of file names which would be ignored.
        /// </summary>
        /// <param name="ignorePatterns">Patterns to ignore (.gitignore syntax).</param>
        public IReadOnlyList<string> GetIgnoredFiles(IEnumerable<string> ignorePatterns)
        {
            List<string> notEmptyPatterns = ignorePatterns
                .Where(pattern => !string.IsNullOrWhiteSpace(pattern))
                .ToList();

            if (notEmptyPatterns.Count != 0)
            {
                string excludeParams =
                    notEmptyPatterns
                    .Select(pattern => "-x " + pattern.Quote())
                    .Join(" ");

                // filter duplicates out of the result because options -c and -m may return
                // same files at times
                return _gitExecutable.GetOutput($"ls-files -z -o -m -c -i {excludeParams}")
                    .Split(Delimiters.NullAndLineFeed, StringSplitOptions.RemoveEmptyEntries)
                    .Distinct()
                    .ToList();
            }
            else
            {
                return Array.Empty<string>();
            }
        }

        public IReadOnlyList<string> GetFullTree(string id)
        {
            return _gitExecutable.GetOutput(
                    $"ls-tree -z -r --name-only {id.Quote()}",
                    cache: GitCommandCache)
                .Split(Delimiters.NullAndLineFeed);
        }

        public IEnumerable<INamedGitItem> GetTree(ObjectId? commitId, bool full)
        {
            GitArgumentBuilder args = new("ls-tree")
            {
                "-z",
                { full, "-r" },
                commitId
            };

            string tree = _gitExecutable.GetOutput(args, cache: GitCommandCache);

            return _gitTreeParser.Parse(tree);
        }

        public GitBlame Blame(string? fileName, string from, Encoding encoding, string? lines, CancellationToken cancellationToken)
        {
            GitArgumentBuilder args = new("blame")
            {
                "--porcelain",
                { AppSettings.DetectCopyInFileOnBlame, "-M" }, // as git-diff --find-renames
                { AppSettings.DetectCopyInAllOnBlame, "-C" }, // as git-diff --find-copies
                { AppSettings.IgnoreWhitespaceOnBlame, "-w" }, // as git-diff --ignore-all-space
                "-l",
                { lines is not null, $"-L {lines}" },
                from.ToPosixPath().Quote(),
                "--",
                fileName.ToPosixPath().Quote()
            };

            ExecutionResult result = _gitExecutable.Execute(
                args,
                cache: GitCommandCache,
                outputEncoding: LosslessEncoding,
                cancellationToken: cancellationToken);

            string output = result.StandardOutput;

            try
            {
                return ParseGitBlame(output, encoding);
            }
            catch
            {
                // Catch all parser errors, and ignore them!
                // We should never get here...
                Debug.WriteLine("Error parsing output from command: {0}\n\nPlease report a bug!", args);

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

            Dictionary<ObjectId, GitBlameCommit> commitByObjectId = [];

            // Pre-allocate the list with a capacity estimated from the approximate git blame length to describe a file line
            const int GitBlameLengthPerLineHeuristicValue = 120;
            List<GitBlameLine> lines = new(capacity: Math.Min(Math.Max(256, output.Length / GitBlameLengthPerLineHeuristicValue), 5000));

            bool hasCommitHeader;
            ObjectId? objectId;
            int finalLineNumber;
            int originLineNumber;
            string? author;
            string? authorMail;
            DateTime authorTime;
            string? authorTimeZone;
            string? committer;
            string? committerMail;
            DateTime committerTime;
            string? committerTimeZone;
            string? summary;
            string? filename;

            Reset();

            foreach (string line in output.LazySplit('\n').Select(l => l.TrimEnd('\r')))
            {
                Match match = HeaderRegex().Match(line);

                if (match.Success)
                {
                    objectId = ObjectId.Parse(line, match.Groups["objectid"]);
                    finalLineNumber = int.Parse(match.Groups["finallinenum"].Value);
                    originLineNumber = int.Parse(match.Groups["origlinenum"].Value);
                }
                else if (line.StartsWith("\t"))
                {
                    // The contents of the actual line is output after the above header, prefixed by a TAB. This is to allow adding more header elements later.
                    string text = ReEncodeStringFromLossless(line[1..], encoding);

                    GitBlameCommit commit;
                    if (hasCommitHeader)
                    {
                        // TODO quite a few nullable suppressions here (via ! character) which should be addressed as they hint at a design flaw

                        if (!commitByObjectId.TryGetValue(objectId!, out GitBlameCommit? commitData))
                        {
                            commit = new GitBlameCommit(
                                objectId!,
                                author!,
                                authorMail!,
                                authorTime,
                                authorTimeZone!,
                                committer!,
                                committerMail!,
                                committerTime,
                                committerTimeZone!,
                                summary!,
                                filename!);
                            commitByObjectId[objectId!] = commit;
                        }
                        else
                        {
                            if (filename == commitData.FileName)
                            {
                                commit = commitData;
                            }
                            else
                            {
                                commit = new GitBlameCommit(
                                    commitData.ObjectId,
                                    commitData.Author,
                                    commitData.AuthorMail,
                                    commitData.AuthorTime,
                                    commitData.AuthorTimeZone,
                                    commitData.Committer,
                                    commitData.CommitterMail,
                                    commitData.CommitterTime,
                                    commitData.CommitterTimeZone,
                                    commitData.Summary,
                                    filename!);
                            }
                        }
                    }
                    else
                    {
                        commit = commitByObjectId[objectId!];
                    }

                    lines.Add(new GitBlameLine(commit, finalLineNumber, originLineNumber, text));

                    // Start a new header
                    Reset();
                }
                else if (line.StartsWith("author "))
                {
                    author = ReEncodeStringFromLossless(line["author ".Length..]);
                    hasCommitHeader = true;
                }
                else if (line.StartsWith("author-mail "))
                {
                    authorMail = ReEncodeStringFromLossless(line["author-mail ".Length..]);
                    hasCommitHeader = true;
                }
                else if (line.StartsWith("author-time "))
                {
                    authorTime = DateTimeUtils.ParseUnixTime(line["author-time ".Length..]);
                    hasCommitHeader = true;
                }
                else if (line.StartsWith("author-tz "))
                {
                    authorTimeZone = line["author-tz ".Length..];
                    hasCommitHeader = true;
                }
                else if (line.StartsWith("committer "))
                {
                    committer = ReEncodeStringFromLossless(line["committer ".Length..]);
                    hasCommitHeader = true;
                }
                else if (line.StartsWith("committer-mail "))
                {
                    committerMail = line["committer-mail ".Length..];
                    hasCommitHeader = true;
                }
                else if (line.StartsWith("committer-time "))
                {
                    committerTime = DateTimeUtils.ParseUnixTime(line["committer-time ".Length..]);
                    hasCommitHeader = true;
                }
                else if (line.StartsWith("committer-tz "))
                {
                    committerTimeZone = line["committer-tz ".Length..];
                    hasCommitHeader = true;
                }
                else if (line.StartsWith("summary "))
                {
                    summary = ReEncodeStringFromLossless(line["summary ".Length..]);
                    hasCommitHeader = true;
                }
                else if (line.StartsWith("filename "))
                {
                    filename = ReEncodeFileNameFromLossless(line["filename ".Length..]);
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

        public string GetFileText(ObjectId id, Encoding encoding, bool stripAnsiEscapeCodes)
        {
            GitArgumentBuilder args = new("cat-file")
            {
                "blob",
                id.ToString().QuoteNE()
            };

            return _gitExecutable.GetOutput(
                args,
                cache: GitCommandCache,
                outputEncoding: encoding,
                stripAnsiEscapeCodes: stripAnsiEscapeCodes);
        }

        public ObjectId? GetFileBlobHash(string fileName, ObjectId objectId)
        {
            if (objectId == ObjectId.WorkTreeId || objectId == ObjectId.CombinedDiffId)
            {
                throw new ArgumentException($"Tried to get blob for unsupported revision: {objectId} and file: {fileName}");
            }

            // TODO use regex for parsing
            if (objectId == ObjectId.IndexId)
            {
                GitArgumentBuilder args = new("ls-files")
                {
                    "-s",
                    { !string.IsNullOrWhiteSpace(fileName), "--" },
                    fileName.QuoteNE()
                };

                // index
                string[] lines = _gitExecutable.GetOutput(args).Split(Delimiters.TabAndSpace);

                if (lines.Length >= 2)
                {
                    return ObjectId.Parse(lines[1]);
                }
            }
            else
            {
                GitArgumentBuilder args = new("ls-tree")
                {
                    "-r",
                    objectId,
                    { !string.IsNullOrWhiteSpace(fileName), "--" },
                    fileName.QuoteNE()
                };
                string[] lines = _gitExecutable.GetOutput(args).Split(Delimiters.TabAndSpace);
                if (lines.Length >= 3)
                {
                    return ObjectId.Parse(lines[2]);
                }
            }

            return null;
        }

        public MemoryStream? GetFileStream(string blob)
        {
            // TODO why return a stream here? should just return a byte[]

            try
            {
                GitArgumentBuilder args = new("cat-file")
                {
                    "blob",
                    blob
                };
                using IProcess process = _gitCommandRunner.RunDetached(CancellationToken.None, args, redirectOutput: true);
                MemoryStream stream = new();
                process.StandardOutput.BaseStream.CopyTo(stream);
                stream.Position = 0;
                return stream;
            }
            catch (Win32Exception ex)
            {
                Trace.WriteLine(ex);
                return null;
            }
        }

        public IEnumerable<string?> GetPreviousCommitMessages(int count, string revision, string authorPattern)
        {
            GitArgumentBuilder args = new("log")
            {
                "-z",
                $"-n {count}",
                revision.Quote(),
                @"--pretty=""format:%B""",
                { !string.IsNullOrEmpty(authorPattern), string.Concat("--author=\"", authorPattern, "\"") }
            };

            ExecutionResult result = _gitExecutable.Execute(args, outputEncoding: LosslessEncoding, throwOnErrorExit: false);
            if (!result.ExitedSuccessfully)
            {
                return new[] { string.Empty };
            }

            string[] messages = result.StandardOutput.Split(Delimiters.Null, StringSplitOptions.RemoveEmptyEntries);
            return messages.Length == 0
                ? new[] { string.Empty }
                : messages.Select(ReEncodeCommitMessage);
        }

        public string GetCustomDiffMergeTools(bool isDiff, CancellationToken cancellationToken)
        {
            // Use a global list of custom tools, always use Windows tools (native paths for the app).
            // Note that --gui has no effect here
            GitArgumentBuilder args = new(isDiff ? "difftool" : "mergetool") { "--tool-help" };
            ExecutionResult result = _gitWindowsExecutable.Execute(args, cancellationToken: cancellationToken);
            return result.StandardOutput;
        }

        public void OpenWithDifftoolDirDiff(string? firstRevision, string? secondRevision, string? customTool = null)
        {
            OpenWithDifftool(filename: null, firstRevision: firstRevision, secondRevision: secondRevision, extraDiffArguments: "--dir-diff", customTool: customTool);
        }

        public void OpenWithDifftool(string? filename, string? oldFileName = "", string? firstRevision = GitRevision.IndexGuid, string? secondRevision = GitRevision.WorkTreeGuid, string? extraDiffArguments = null, bool isTracked = true, string? customTool = null)
        {
            // Use Windows Git if custom tool is selected as the list is native to the application.
            (string.IsNullOrWhiteSpace(customTool) ? _gitCommandRunner : _gitWindowsCommandRunner)
                .RunDetached(new GitArgumentBuilder("difftool")
            {
                { string.IsNullOrWhiteSpace(customTool), "--gui", $"--tool={customTool}" },
                "--find-renames",
                "--find-copies",
                "--no-prompt",
                extraDiffArguments,
                _revisionDiffProvider.Get(firstRevision, secondRevision, filename, oldFileName, isTracked)
            });
        }

        /// <summary>
        /// Compare two Git commitish; blob or rev:path.
        /// Does nothing if either input is null or empty.
        /// </summary>
        /// <param name="firstGitCommit">commitish.</param>
        /// <param name="secondGitCommit">commitish.</param>
        public void OpenFilesWithDifftool(string? firstGitCommit, string? secondGitCommit, string? customTool = null)
        {
            if (string.IsNullOrWhiteSpace(firstGitCommit) || string.IsNullOrWhiteSpace(secondGitCommit))
            {
                return;
            }

            // Use Windows Git if custom tool is selected as the list is native to the application.
            (string.IsNullOrWhiteSpace(customTool) ? _gitCommandRunner : _gitWindowsCommandRunner)
                .RunDetached(new GitArgumentBuilder("difftool")
            {
                { string.IsNullOrWhiteSpace(customTool), "--gui", $"--tool={customTool}" },
                "--find-renames",
                "--find-copies",
                "--no-prompt",
                firstGitCommit.QuoteNE(),
                secondGitCommit.QuoteNE()
            });
        }

        public ObjectId? RevParse(string? revisionExpression)
        {
            if (string.IsNullOrWhiteSpace(revisionExpression) || revisionExpression.Length > 260)
            {
                return null;
            }

            if (ObjectId.TryParse(revisionExpression, out ObjectId? objectId))
            {
                return objectId;
            }

            GitArgumentBuilder args = new("rev-parse")
            {
                "--quiet",
                "--verify",
                $"{revisionExpression}~0".Quote()
            };
            ExecutionResult result = _gitExecutable.Execute(args, throwOnErrorExit: false);

            return result.ExitedSuccessfully && ObjectId.TryParse(result.StandardOutput, offset: 0, out objectId)
                ? objectId
                : null;
        }

        public ObjectId? GetMergeBase(ObjectId a, ObjectId b)
        {
            if (a == b)
            {
                return a;
            }

            GitArgumentBuilder args = new("merge-base")
            {
                a,
                b
            };
            ExecutionResult result = _gitExecutable.Execute(args, cache: GitCommandCache, throwOnErrorExit: false);
            string output = result.StandardOutput;

            return ObjectId.TryParse(output, offset: 0, out ObjectId? objectId)
                ? objectId
                : null;
        }

        public SubmoduleStatus CheckSubmoduleStatus(ObjectId? commit, ObjectId? oldCommit, CommitData? data, CommitData? oldData, bool loadData)
        {
            // Submodule directory must exist to run commands, unknown otherwise
            if (!IsValidGitWorkingDir() || oldCommit is null)
            {
                return SubmoduleStatus.NewSubmodule;
            }

            if (commit is null)
            {
                // Actually removed submodule, no special status for this uncommon status
                return SubmoduleStatus.Unknown;
            }

            if (commit == oldCommit)
            {
                return SubmoduleStatus.Unknown;
            }

            ObjectId? baseOid = GetMergeBase(commit, oldCommit);
            if (baseOid is null)
            {
                return SubmoduleStatus.Unknown;
            }

            ObjectId baseCommit = baseOid;
            if (baseCommit == oldCommit)
            {
                return SubmoduleStatus.FastForward;
            }
            else if (baseCommit == commit)
            {
                return SubmoduleStatus.Rewind;
            }

            if (loadData)
            {
                oldData = _commitDataManager.GetCommitData(oldCommit.ToString(), cache: true);
            }

            if (oldData is null)
            {
                return SubmoduleStatus.NewSubmodule;
            }

            if (loadData)
            {
                data = _commitDataManager.GetCommitData(commit.ToString(), cache: true);
            }

            if (data is null)
            {
                return SubmoduleStatus.Unknown;
            }

            if (data.CommitDate > oldData.CommitDate)
            {
                return SubmoduleStatus.NewerTime;
            }
            else if (data.CommitDate < oldData.CommitDate)
            {
                return SubmoduleStatus.OlderTime;
            }
            else if (data.CommitDate == oldData.CommitDate)
            {
                return SubmoduleStatus.SameTime;
            }

            return SubmoduleStatus.Unknown;
        }

        public bool CheckBranchFormat(string branchName)
        {
            ArgumentNullException.ThrowIfNull(branchName);

            if (string.IsNullOrWhiteSpace(branchName))
            {
                return false;
            }

            GitArgumentBuilder args = new("check-ref-format")
            {
                "--branch",
                branchName.QuoteNE()
            };
            return _gitExecutable.Execute(args, throwOnErrorExit: false).ExitedSuccessfully;
        }

        public string FormatBranchName(string branchName)
        {
            ArgumentNullException.ThrowIfNull(branchName);

            string fullBranchName = GitRefName.GetFullBranchName(branchName);

            if (RevParse(fullBranchName) is null)
            {
                return branchName;
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
            string cmd = Path.Combine(AppSettings.LinuxToolsDir, "ps");
            string[] lines = new Executable(cmd).GetOutput("x").Split(Delimiters.LineFeed);

            if (lines.Length <= 2)
            {
                return false;
            }

            LazyStringSplit headers = lines[0].LazySplit(' ', StringSplitOptions.RemoveEmptyEntries);
            int commandIndex = headers.IndexOf(header => header == "COMMAND");
            for (int i = 1; i < lines.Length; i++)
            {
                string[] columns = lines[i].Split(Delimiters.Space, StringSplitOptions.RemoveEmptyEntries);
                if (commandIndex < columns.Length)
                {
                    string command = columns[commandIndex];
                    if (command.EndsWith("/git"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Un-escapes any octal code points embedded within <paramref name="s"/>.
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
        [return: NotNullIfNotNull("s")]
        public static string? UnescapeOctalCodePoints(string? s)
        {
            if (s is null)
            {
                return null;
            }

            return EscapedOctalCodePointRegex().Replace(
                s,
                match =>
                {
                    try
                    {
                        return SystemEncoding.GetString(
                            match.Groups["octal"]
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

        [return: NotNullIfNotNull("fileName")]
        public static string? ReEncodeFileNameFromLossless(string? fileName)
        {
            fileName = ReEncodeStringFromLossless(fileName, SystemEncoding);
            return UnescapeOctalCodePoints(fileName);
        }

        [return: NotNullIfNotNull("s")]
        public static string? ReEncodeString(string? s, Encoding fromEncoding, Encoding toEncoding)
        {
            if (s is null || fromEncoding.WebName == toEncoding.WebName)
            {
                return s;
            }

            byte[] bytes = fromEncoding.GetBytes(s);
            return toEncoding.GetString(bytes);
        }

        /// <summary>
        /// Re-encodes string from Commands.LosslessEncoding to toEncoding.
        /// </summary>
        [return: NotNullIfNotNull("s")]
        public static string? ReEncodeStringFromLossless(string? s, Encoding? toEncoding)
        {
            if (toEncoding is null)
            {
                return s;
            }

            return ReEncodeString(s, LosslessEncoding, toEncoding);
        }

        [return: NotNullIfNotNull("s")]
        public string? ReEncodeStringFromLossless(string? s)
        {
            return ReEncodeStringFromLossless(s, LogOutputEncoding);
        }

        public string? ReEncodeCommitMessage(string s)
        {
            return ReEncodeStringFromLossless(s, LogOutputEncoding)?.Trim();
        }

        /// <summary>
        /// header part of show result is encoded in logoutputencoding (including re-encoded commit message).
        /// diff part is raw data in file's original encoding.
        /// s should be encoded in LosslessEncoding.
        /// </summary>
        [return: NotNullIfNotNull("s")]
        public string? ReEncodeShowString(string? s)
        {
            if (string.IsNullOrEmpty(s))
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
                header = s[..p];
                diff = s[p..];
            }
            else
            {
                header = string.Empty;
                diff = s;
            }

            p = diff.IndexOf("@@");
            if (p > 0)
            {
                diffHeader = diff[..p];
                diffContent = diff[p..];
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

        public override string ToString()
        {
            return WorkingDir;
        }

        public string? GetLocalTrackingBranchName(string remoteName, string branch)
        {
            string branchName = remoteName.Length > 0 ? branch[(remoteName.Length + 1)..] : branch;
            foreach (IConfigSection section in LocalConfigFile.GetConfigSections())
            {
                if (section.SectionName == "branch" && section.GetValue("remote") == remoteName)
                {
                    string remoteBranch = section.GetValue("merge").Replace("refs/heads/", string.Empty);
                    if (remoteBranch == branchName)
                    {
                        return section.SubSection;
                    }
                }
            }

            return branchName;
        }

        public IReadOnlyList<GitItemStatus> GetCombinedDiffFileList(ObjectId mergeCommitObjectId)
        {
            GitArgumentBuilder args = new("diff-tree")
            {
                "--name-only",
                "-z",
                "--cc",
                "--no-commit-id",
                mergeCommitObjectId
            };

            string fileList = _gitExecutable.GetOutput(args);

            if (string.IsNullOrWhiteSpace(fileList))
            {
                return Array.Empty<GitItemStatus>();
            }

            LazyStringSplit files = fileList.LazySplit('\0', StringSplitOptions.RemoveEmptyEntries);

            return files.Select(
                file => new GitItemStatus(name: file)
                {
                    IsChanged = true,
                    IsTracked = true,
                    IsDeleted = false,
                    IsNew = false,
                    Staged = StagedStatus.None
                }).ToList();
        }

        public bool GetCombinedDiffContent(
            ObjectId revisionOfMergeCommit,
            string filePath,
            string extraArgs,
            Encoding encoding,
            out string diffOfConflict,
            bool useGitColoring,
            IGitCommandConfiguration commandConfiguration,
            CancellationToken cancellationToken)
        {
            GitArgumentBuilder args = new("diff-tree", commandConfiguration)
            {
                { AppSettings.OmitUninterestingDiff, "--cc", "-c -p" },
                "--no-commit-id",
                { AppSettings.UseHistogramDiffAlgorithm, "--histogram" },
                { useGitColoring, "--color=always" },
                extraArgs,
                revisionOfMergeCommit,
                "--",
                filePath.ToPosixPath().Quote()
            };

            ExecutionResult result = _gitExecutable.Execute(
                args,
                cache: GitCommandCache,
                outputEncoding: LosslessEncoding,
                stripAnsiEscapeCodes: !useGitColoring,
                throwOnErrorExit: false,
                cancellationToken: cancellationToken);

            if (!result.ExitedSuccessfully)
            {
                diffOfConflict = result.AllOutput;
                return false;
            }

            if (string.IsNullOrEmpty(result.StandardOutput))
            {
                diffOfConflict = "";
                return true;
            }

            List<Patch> patches = PatchProcessor.CreatePatchesFromString(result.StandardOutput, new Lazy<Encoding>(() => encoding)).ToList();

            Patch? patch = GetPatch(patches, filePath, filePath);
            if (patch is null)
            {
                diffOfConflict = "";
                return false;
            }

            diffOfConflict = patch.Text ?? "";
            return true;
        }

        public bool StopTrackingFile(string filename)
        {
            GitArgumentBuilder args = new("rm")
            {
                "--cached",
                filename.ToPosixPath().Quote()
            };
            return _gitExecutable.Execute(args, throwOnErrorExit: false).ExitedSuccessfully;
        }

        /// <summary>
        /// Finds the most recent tag that is reachable from a commit
        /// </summary>
        /// <param name="commitId">The commit where to start searching</param>
        /// <returns>Tag name if it exists, otherwise null</returns>
        public string? GetDescribe(ObjectId commitId, CancellationToken cancellationToken)
        {
            GitArgumentBuilder args = new("describe")
            {
                "--tags",
                "--first-parent",
                "--abbrev=40",
                commitId
            };

            ExecutionResult exec = _gitExecutable.Execute(args, throwOnErrorExit: false);
            return exec.ExitedSuccessfully
                ? exec.StandardOutput.TrimEnd()
                : null;
        }

        private static GitItemStatus createErrorGitItemStatus(string gitOutput)
        {
            return new GitItemStatus(name: GitError) { IsStatusOnly = true, ErrorMessage = gitOutput.Replace('\0', '\t') };
        }

        public (int TotalCount, Dictionary<string, int> CountByName) GetCommitsByContributor(DateTime? since = null, DateTime? until = null)
        {
            Dictionary<string, int> countByName = [];
            int totalCommits = 0;

            GitArgumentBuilder args = new("shortlog")
            {
                "--all",
                "-s",
                "-n",
                "--no-merges",
                GetDateParameter("--since", since),
                GetDateParameter("--until", until)
            };

            ExecutionResult result = _gitExecutable.Execute(args);
            LazyStringSplit lines = result.StandardOutput.LazySplit('\n');

            foreach (string line in lines)
            {
                Match match = ShortlogRegex().Match(line);

                if (!match.Success)
                {
                    continue;
                }

                int count = int.Parse(match.Groups["count"].Value);
                string name = match.Groups["name"].Value;

                totalCommits += count;

                if (!countByName.TryGetValue(name, out int oldCount))
                {
                    countByName[name] = count;
                }
                else
                {
                    // Sometimes this happen because of wrong encoding
                    countByName[name] = oldCount + count;
                }
            }

            return (totalCommits, countByName);

            string GetDateParameter(string param, DateTime? date)
            {
                return date is not null
                    ? $"{param}=\"{date:yyyy-MM-dd hh:mm:ss}\""
                    : "";
            }
        }

        internal TestAccessor GetTestAccessor() => new(this);

        internal readonly struct TestAccessor
        {
            private readonly GitModule _gitModule;

            public TestAccessor(GitModule gitModule)
            {
                _gitModule = gitModule;
            }

            public GitArgumentBuilder UpdateIndexCmd(bool showErrorsWhenStagingFiles) => GitModule.UpdateIndexCmd(showErrorsWhenStagingFiles);

            public List<GitItemStatus> GetDiffChangedFilesFromString(string statusString, StagedStatus staged)
                => _gitModule.GetDiffChangedFilesFromString(statusString, staged);

            public StagedStatus GetStagedStatus(ObjectId? firstId, ObjectId? secondId, ObjectId? parentToSecond)
                => GitModule.GetStagedStatus(firstId, secondId, parentToSecond);
        }
    }
}
