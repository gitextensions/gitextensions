using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.Git.Commands;
using GitCommands.Git.Extensions;
using GitCommands.Patches;
using GitCommands.Settings;
using GitCommands.Utils;
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
    public sealed class GitModule : IGitModule
    {
        private const string GitError = "Git Error";
        private static readonly Regex CpEncodingPattern = new("cp\\d+", RegexOptions.Compiled);
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
        private static readonly Regex _refRegex = new(@"^(?<objectid>[0-9a-f]{40})[ \t](?<refname>.+)$", RegexOptions.Multiline | RegexOptions.Compiled);

        private static readonly Regex _headerRegex = new(@"^(?<objectid>[0-9a-f]{40}) (?<origlinenum>\d+) (?<finallinenum>\d+)", RegexOptions.Compiled);

        /// <summary>
        /// Name of the WSL distro for the GitExecutable, empty string for the app native Windows Git executable.
        /// This can be seen as the Git "instance" identifier.
        /// </summary>
        private readonly string _wslDistro;

        public GitModule(string? workingDir)
        {
            WorkingDir = (workingDir ?? "").NormalizePath().EnsureTrailingPathSeparator();
            WorkingDirGitDir = GitDirectoryResolverInstance.Resolve(WorkingDir);
            _indexLockManager = new IndexLockManager(this);
            _commitDataManager = new CommitDataManager(() => this);
            _getAllChangedFilesOutputParser = new GetAllChangedFilesOutputParser(() => this);
            _gitWindowsExecutable = new Executable(() => AppSettings.GitCommand, WorkingDir);
            _gitWindowsCommandRunner = new GitCommandRunner(_gitWindowsExecutable, () => SystemEncoding);

            _wslDistro = AppSettings.WslGitEnabled ? PathUtil.GetWslDistro(WorkingDir) : "";
            if (!string.IsNullOrEmpty(_wslDistro))
            {
                _gitExecutable = new Executable(() => AppSettings.WslGitCommand, WorkingDir, $"-d {_wslDistro} git ");
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

                var currentPath = WorkingDir.RemoveTrailingPathSeparator();

                // Try to find an ancestor path that contains a .gitmodules file and is a valid work dir
                var superprojectPath = PathUtil.FindAncestors(currentPath).FirstOrDefault(HasGitModulesFile);

                // If we didn't find it, but there's a .git file in the current folder, look for a gitdir:
                // line in that file that points to the location of the .git folder
                var gitDir = Path.Combine(WorkingDir, ".git");
                if (superprojectPath is null && File.Exists(gitDir))
                {
                    foreach (var line in File.ReadLines(gitDir))
                    {
                        const string gitdir = "gitdir:";

                        if (line.StartsWith(gitdir))
                        {
                            string gitPath = line.Substring(gitdir.Length).Trim();
                            int pos = gitPath.IndexOf("/.git/modules/", StringComparison.Ordinal);
                            if (pos != -1)
                            {
                                gitPath = gitPath.Substring(0, pos + 1).Replace('/', '\\');
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
                    var submodulePath = currentPath.Substring(superprojectPath.Length).ToPosixPath();
                    ConfigFile configFile = new(Path.Combine(superprojectPath, ".gitmodules"), local: true);

                    foreach (var configSection in configFile.ConfigSections)
                    {
                        if (configSection.GetValue("path") == submodulePath.ToPosixPath())
                        {
                            var submoduleName = configSection.SubSection;
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
        public GitVersion GitVersion => GitVersion.CurrentVersion(GitExecutable, _wslDistro);

        /// <inherit/>
        public IExecutable GitExecutable => _gitExecutable;

        /// <inherit/>
        public IGitCommandRunner GitCommandRunner => _gitCommandRunner;

        /// <summary>
        /// Gets the location of .git directory for the current working folder.
        /// </summary>
        public string WorkingDirGitDir { get; private set; }

        /// <inherit/>
        public string GetGitExecPath(string? path) => PathUtil.GetGitExecPath(path, _wslDistro);

        /// <inherit/>
        public string GetWindowsPath(string path) => PathUtil.GetWindowsPath(path, _wslDistro);

        /// <summary>
        /// If this module is a submodule, returns its name, otherwise <c>null</c>.
        /// </summary>
        public string? SubmoduleName { get; }

        /// <summary>
        /// If this module is a submodule, returns its path, otherwise <c>null</c>.
        /// </summary>
        public string? SubmodulePath { get; }

        /// <summary>
        /// If this module is a submodule, returns its superproject <see cref="GitModule"/>, otherwise <c>null</c>.
        /// </summary>
        /// TODO: Add to IGitModule and return IGitModule
        public GitModule? SuperprojectModule { get; }

        /// <summary>
        /// If this module is a submodule, returns the top-most parent module, otherwise it returns itself.
        /// </summary>
        /// TODO: Add to IGitModule and return IGitModule
        public GitModule GetTopModule()
        {
            GitModule topModule = this;
            while (topModule.SuperprojectModule is not null)
            {
                topModule = topModule.SuperprojectModule;
            }

            return topModule;
        }

        private RepoDistSettings? _effectiveSettings;

        public RepoDistSettings EffectiveSettings
        {
            get
            {
                if (_effectiveSettings is null)
                {
                    lock (_lock)
                    {
                        _effectiveSettings ??= RepoDistSettings.CreateEffective(this);
                    }
                }

                return _effectiveSettings;
            }
        }

        public ISettingsSource GetEffectiveSettings()
        {
            return EffectiveSettings;
        }

        private RepoDistSettings? _localSettings;

        public RepoDistSettings LocalSettings
        {
            get
            {
                if (_localSettings is null)
                {
                    lock (_lock)
                    {
                        _localSettings ??= new RepoDistSettings(null, EffectiveSettings.SettingsCache, SettingLevel.Local);
                    }
                }

                return _localSettings;
            }
        }

        private ConfigFileSettings? _effectiveConfigFile;

        public ConfigFileSettings EffectiveConfigFile
        {
            get
            {
                if (_effectiveConfigFile is null)
                {
                    lock (_lock)
                    {
                        _effectiveConfigFile ??= ConfigFileSettings.CreateEffective(this);
                    }
                }

                return _effectiveConfigFile;
            }
        }

        public ConfigFileSettings LocalConfigFile => new(null, EffectiveConfigFile.SettingsCache, SettingLevel.Local);

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

        public Encoding FilesEncoding => EffectiveConfigFile.FilesEncoding ?? new UTF8Encoding(false);

        public Encoding CommitEncoding => EffectiveConfigFile.CommitEncoding ?? new UTF8Encoding(false);

        /// <summary>
        /// Encoding for commit header (message, notes, author, committer, emails).
        /// </summary>
        public Encoding LogOutputEncoding => EffectiveConfigFile.LogOutputEncoding ?? CommitEncoding;

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
            List<string> localPaths = new();
            try
            {
                DoGetSubmodulesLocalPaths(this, "", localPaths, recursive);
            }
            catch (GitConfigurationException)
            {
                // swallow any exceptions here, any config exceptions would have been shown to the user already
            }

            return localPaths;

            void DoGetSubmodulesLocalPaths(GitModule module, string parentPath, List<string> paths, bool recurse)
            {
                var submodulePaths = GetSubmodulePaths(module)
                    .Select(p => Path.Combine(parentPath, p).ToPosixPath())
                    .ToList();

                paths.AddRange(submodulePaths);

                if (recurse)
                {
                    foreach (var submodulePath in submodulePaths)
                    {
                        DoGetSubmodulesLocalPaths(GetSubmodule(submodulePath), submodulePath, paths, recurse);
                    }
                }
            }

            IEnumerable<string> GetSubmodulePaths(GitModule module)
            {
                ConfigFile configFile = module.GetSubmoduleConfigFile();

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
            var dir = startDir?.Trim();

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
                GitCommandHelpers.CleanCmd(mode, dryRun, directories, paths));
        }

        public bool EditNotes(ObjectId commitId)
        {
            GitArgumentBuilder arguments = new("notes") { "edit", commitId };
            var editor = GetEffectiveSetting("core.editor").ToLower();
            var createWindow = !editor.Contains("gitextensions") && !editor.Contains("notepad");

            return _gitExecutable.RunCommand(arguments, createWindow: createWindow);
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
            var output = _gitExecutable.GetOutput(args);

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
            var output = _gitExecutable.GetOutput(args);

            if (string.IsNullOrEmpty(output))
            {
                return false;
            }

            if (!output.StartsWith(".merge_file_"))
            {
                return false;
            }

            // Parse temporary file name from command line result
            var splitResult = output.Split(Delimiters.TabAndLineFeedAndCarriageReturn, StringSplitOptions.RemoveEmptyEntries);
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
            using var blobStream = GetFileStream(blob);
            if (blobStream is null)
            {
                return;
            }

            var blobData = blobStream.ToArray();
            if (EffectiveConfigFile.ByPath("core").GetNullableEnum<AutoCRLFType>("autocrlf") is AutoCRLFType.@true)
            {
                if (!FileHelper.IsBinaryFileName(this, saveAs) && !FileHelper.IsBinaryFileAccordingToContent(blobData))
                {
                    blobData = GitConvert.ConvertCrLfToWorktree(blobData);
                }
            }

            using var stream = File.Create(saveAs);
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

        public (string? baseFile, string? localFile, string? remoteFile) CheckoutConflictedFiles(ConflictData unmergedData)
        {
            Directory.SetCurrentDirectory(WorkingDir);

            var baseFile = CheckoutPart(1, unmergedData.Filename + ".BASE", unmergedData.Base.Filename);
            var localFile = CheckoutPart(2, unmergedData.Filename + ".LOCAL", unmergedData.Local.Filename);
            var remoteFile = CheckoutPart(3, unmergedData.Filename + ".REMOTE", unmergedData.Remote.Filename);

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
                    var output = _gitExecutable.GetOutput(args);

                    var tempFile = Path.Combine(WorkingDir, output.SubstringUntil('\t'));

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
                var index = 1;
                var test = basePath;

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

            List<ConflictData> list = new();
            GitArgumentBuilder args = new("ls-files")
            {
                "-z",
                "--unmerged",
                { !string.IsNullOrWhiteSpace(filename), "--" },
                filename.QuoteNE()
            };

            // ignore non-zero exit code, e.g. in case of missing submodule
            ExecutionResult result = await _gitExecutable.ExecuteAsync(args, throwOnErrorExit: false).ConfigureAwait(false);
            var unmerged = result.StandardOutput.Split(Delimiters.NullAndLineFeed, StringSplitOptions.RemoveEmptyEntries);

            var item = new ConflictedFileData[3];

            string? prevItemName = null;

            foreach (var line in unmerged)
            {
                int findSecondWhitespace = line.IndexOfAny(new[] { ' ', '\t' });
                string fileStage = findSecondWhitespace >= 0 ? line.Substring(findSecondWhitespace).Trim() : "";

                findSecondWhitespace = fileStage.IndexOfAny(new[] { ' ', '\t' });

                string hash = findSecondWhitespace >= 0 ? fileStage.Substring(0, findSecondWhitespace).Trim() : "";
                fileStage = findSecondWhitespace >= 0 ? fileStage.Substring(findSecondWhitespace).Trim() : "";

                if (fileStage.Length > 2 && int.TryParse(fileStage[0].ToString(), out var stage) && stage is (>= 1 and <= 3))
                {
                    var itemName = fileStage.Substring(2);
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
            var refsFilter = (AppSettings.ShowSuperprojectBranches ? RefsFilter.Heads : RefsFilter.NoFilter)
                | (AppSettings.ShowSuperprojectRemoteBranches ? RefsFilter.Remotes : RefsFilter.NoFilter)
                | (AppSettings.ShowSuperprojectTags ? RefsFilter.Tags : RefsFilter.NoFilter);
            if (refsFilter == RefsFilter.NoFilter)
            {
                return new Dictionary<IGitRef, IGitItem?>();
            }

            string? command = GitCommandHelpers.GetRefsCmd(refsFilter, noLocks: noLocks, GitRefsSortBy.committerdate, GitRefsSortOrder.Descending, maxSuperRefCount);
            var refList = await _gitExecutable.GetOutputAsync(command).ConfigureAwait(false);
            var refs = ParseRefs(refList);

            return refs.ToDictionary(r => r, r => GetSubmoduleCommitHash(filename.ToPosixPath(), r.Name));
        }

        private IGitItem? GetSubmoduleCommitHash(string? filename, string refName)
        {
            GitArgumentBuilder args = new("ls-tree")
            {
                refName,
                { !string.IsNullOrWhiteSpace(filename), "--" },
                filename.QuoteNE()
            };
            var output = _gitExecutable.GetOutput(args);

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
                parent,
                $"^{child}",
                "--count"
            };
            ExecutionResult result = _gitExecutable.Execute(args, cache: cache ? GitCommandCache : null, throwOnErrorExit: throwOnErrorExit);
            string output = result.StandardOutput;

            if (int.TryParse(output, out var commitCount))
            {
                return commitCount;
            }

            return null;
        }

        public (int? first, int? second) GetCommitRangeDiffCount(ObjectId firstId, ObjectId secondId)
        {
            if (firstId == secondId)
            {
                return (0, 0);
            }

            GitArgumentBuilder args = new("rev-list")
            {
                $"{firstId}...{secondId}",
                "--count",
                "--left-right"
            };
            var output = _gitExecutable.GetOutput(args, cache: GitCommandCache);

            var counts = output.Split(Delimiters.Tab);
            if (counts.Length == 2 && int.TryParse(counts[0], out var first) && int.TryParse(counts[1], out var second))
            {
                return (first, second);
            }

            return (null, null);
        }

        public string GetCommitCountString(string from, string to)
        {
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
                var cmd = AppSettings.GitCommand
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
                args = new ArgumentBuilder()
                {
                    "/c",
                    $"\"{AppSettings.GitCommand.QuoteNE()}",
                    "gui\""
                };
                new Executable("cmd.exe", WorkingDir).Start(args);
            }
        }

        public void RunMergeTool(string? fileName = "", string? customTool = null)
        {
            // Use Windows Git if custom tool is selected as the list is native to the application.
            bool isWindowsGit = !string.IsNullOrWhiteSpace(customTool);
            string gui = (isWindowsGit ? GitVersion.Current : GitVersion).SupportGuiMergeTool ? "--gui" : string.Empty;
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
            var output = _gitExecutable.GetOutput(args);

            WorkingDirGitDir = GitDirectoryResolverInstance.Resolve(WorkingDir);
            return output;
        }

        public bool IsMerge(ObjectId objectId)
        {
            return GetParents(objectId).Count > 1;
        }

        public GitRevision GetRevision(ObjectId? objectId = null, bool shortFormat = false, bool loadRefs = false)
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

            string format = formatString + (shortFormat ? "%s" : "%B%nNotes:%n%-N");

            GitArgumentBuilder args = new("log")
            {
                "-n1",
                $"--pretty=format:{format}",
                objectId
            };

            // cache output only if revision is specified
            CommandCache? cache = objectId is null ? null : GitCommandCache;

            string revInfo = _gitExecutable.GetOutput(args, cache: cache, outputEncoding: LosslessEncoding);

            // TODO improve parsing to reduce temporary string (see similar code in RevisionReader)
            string[] lines = revInfo.Split(Delimiters.LineFeed);

            GitRevision revision = new(ObjectId.Parse(lines[0]))
            {
                TreeGuid = ObjectId.Parse(lines[1]),
                ParentIds = lines[2].LazySplit(' ', StringSplitOptions.RemoveEmptyEntries).Select(line => ObjectId.Parse(line)).ToList(),
                Author = ReEncodeStringFromLossless(lines[3]),
                AuthorEmail = ReEncodeStringFromLossless(lines[4]),
                Committer = ReEncodeStringFromLossless(lines[6]),
                CommitterEmail = ReEncodeStringFromLossless(lines[7]),
                AuthorUnixTime = long.Parse(lines[5]),
                CommitUnixTime = long.Parse(lines[8])
            };

            revision.HasNotes = !shortFormat;
            if (shortFormat)
            {
                revision.Subject = ReEncodeCommitMessage(lines[9]) ?? "";
            }
            else
            {
                string message = ProcessDiffNotes(startIndex: 9);

                // commit message is not re-encoded by git when format is given
                // See also RevisionReader for parsing commit body
                string body = ReEncodeCommitMessage(message);
                revision.Body = body;

                ReadOnlySpan<char> span = (body ?? "").AsSpan();
                int endSubjectIndex = span.IndexOf('\n');
                revision.Subject = endSubjectIndex >= 0
                    ? span.Slice(0, endSubjectIndex).TrimEnd().ToString()
                    : body ?? "";
            }

            if (loadRefs)
            {
                revision.Refs = GetRefs(RefsFilter.NoFilter)
                    .Where(r => r.ObjectId == revision.ObjectId)
                    .ToList();
            }

            return revision;

            string ProcessDiffNotes(int startIndex)
            {
                var endIndex = lines.Length - 1;

                if (lines[endIndex] == "Notes:")
                {
                    endIndex--;
                }

                StringBuilder message = new();
                var inNoteSection = false;

                for (var i = startIndex; i <= endIndex; i++)
                {
                    if (inNoteSection)
                    {
                        message.Append("    ");
                    }

                    message.AppendLine(lines[i]);

                    if (lines[i] == "Notes:")
                    {
                        inNoteSection = true;
                    }
                }

                return message.ToString();
            }
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
                $"{objectId}^@"
            };
            return _gitExecutable.Execute(args, cache: GitCommandCache)
                .StandardOutput
                .Split(Delimiters.NullAndLineFeed, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => ObjectId.Parse(line))
                .ToList();
        }

        public IReadOnlyList<GitRevision> GetParentRevisions(ObjectId commitId)
        {
            return GetParents(commitId)
                .Select(parent => GetRevision(parent, shortFormat: true))
                .ToList();
        }

        public string? ShowObject(ObjectId objectId)
        {
            return ReEncodeShowString(_gitExecutable
                .GetOutput($"show {objectId}", cache: GitCommandCache, outputEncoding: LosslessEncoding));
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

            return result.ExitedSuccessfully && ObjectId.TryParse(result.StandardOutput, offset: 0, out var objectId)
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
                $"{objectIdPrefix}^{{commit}}"
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

        public async Task<(char code, ObjectId? currentCommitId)> GetSuperprojectCurrentCheckoutAsync()
        {
            if (SuperprojectModule is null)
            {
                return (' ', null);
            }

            GitArgumentBuilder args = new("submodule")
            {
                "status",
                "--cached",
                SubmodulePath.Quote()
            };
            var output = await SuperprojectModule.GitExecutable.GetOutputAsync(args).ConfigureAwait(false);
            var lines = output.Split(Delimiters.LineFeed);

            if (lines.Length == 0)
            {
                return (' ', null);
            }

            string submodule = lines[0];

            if (submodule.Length < 43)
            {
                return (' ', null);
            }

            return (submodule[0], ObjectId.Parse(submodule, 1));
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
                $"{startRev}..{endRev}"
            };

            // Could fail if pulling interactively from remote where the specified branch does not exist
            string mergeCommitsOutput = _gitExecutable.Execute(args, throwOnErrorExit: false).StandardOutput;
            return !string.IsNullOrWhiteSpace(mergeCommitsOutput);
        }

        public ConfigFile GetSubmoduleConfigFile()
            => new(WorkingDir + ".gitmodules", true);

        public string? GetCurrentSubmoduleLocalPath()
        {
            if (SuperprojectModule is null)
            {
                return null;
            }

            Debug.Assert(WorkingDir.StartsWith(SuperprojectModule.WorkingDir), "Submodule working dir should start with super-project's working dir");

            return Path.GetDirectoryName(
                WorkingDir.Substring(SuperprojectModule.WorkingDir.Length)).ToPosixPath();
        }

        public string GetSubmoduleFullPath(string? localPath)
        {
            if (localPath is null)
            {
                Debug.Fail("No path for submodule - incorrectly parsed status?");
                return "";
            }

            string dir = Path.Combine(WorkingDir, localPath.EnsureTrailingPathSeparator());
            return Path.GetFullPath(dir); // fix slashes
        }

        public GitModule GetSubmodule(string? localPath)
        {
            return new GitModule(GetSubmoduleFullPath(localPath));
        }

        IGitModule IGitModule.GetSubmodule(string submoduleName)
        {
            return GetSubmodule(submoduleName);
        }

        public IEnumerable<IGitSubmoduleInfo?> GetSubmodulesInfo()
        {
            ConfigFile? configFile;
            try
            {
                configFile = GetSubmoduleConfigFile();
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
            var lines = result.StandardOutput.LazySplit('\n');

            string? lastLine = null;

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

                var match = Regex.Match(s, @"^([ -+U])([0-9a-f]{40}) (.+) \((.+)\)$");

                if (!match.Success)
                {
                    info = default;
                    return false;
                }

                var code = match.Groups[1].Value[0];
                var localPath = match.Groups[3].Value;
                var branch = match.Groups[4].Value;

                if (!ObjectId.TryParse(match.Groups[2].Value, out var currentCommitId))
                {
                    info = default;
                    return false;
                }

                Validates.NotNull(configFile);

                var configSection = configFile.ConfigSections.FirstOrDefault(section => section.GetValue("path").Trim() == localPath);

                Assumes.True(configSection is not null, $"`git submodule status` returned submodule \"{localPath}\" that was not found in .gitmodules");
                Assumes.True(configSection.SubSection is not null, $"Config section must have a non-null sub-section");

                var name = configSection.SubSection.Trim();
                var remotePath = configFile.GetPathValue($"submodule.{name}.url").Trim();

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

        public void Reset(ResetMode mode, string? file = null)
        {
            _gitExecutable.RunCommand(GitCommandHelpers.ResetCmd(mode, null, file));
        }

        public string ResetFile(string file)
        {
            return _gitExecutable.GetOutput(
                new GitArgumentBuilder("checkout-index")
            {
                "--index",
                "--force",
                "--",
                file.ToPosixPath().Quote()
            });
        }

        public string ResetFiles(IReadOnlyList<string> files)
        {
            if (files is null || files.Count == 0)
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
                    $"-o {GetGitExecPath(output).Quote()}"
                });
        }

        public void CheckoutFiles(IReadOnlyList<string> files, ObjectId revision, bool force)
        {
            if (files.Count == 0)
            {
                return;
            }

            if (revision == ObjectId.IndexId)
            {
                // Reset to index has no revision
                // All other artificial commits are errors
                revision = null!;
            }

            // Run batch arguments to work around max command line length on Windows. Fix #6593
            // 3: double quotes + ' '
            // See https://referencesource.microsoft.com/#system/services/monitoring/system/diagnostics/Process.cs,1952
            _gitExecutable.RunBatchCommand(new GitArgumentBuilder("checkout")
                {
                    { force, "--force" },
                    revision,
                    "--"
                }
                .BuildBatchArgumentsForFiles(files));
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

        /// <summary>Tries to start Pageant for the specified remote repo (using the remote's PuTTY key file).</summary>
        /// <returns>true if the remote has a PuTTY key file; otherwise, false.</returns>
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

            var rebaseMergeDir = gitDirectory + "rebase-merge" + Path.DirectorySeparatorChar;
            if (Directory.Exists(rebaseMergeDir))
            {
                return rebaseMergeDir;
            }

            var rebaseApplyDir = gitDirectory + "rebase-apply" + Path.DirectorySeparatorChar;
            if (Directory.Exists(rebaseApplyDir))
            {
                return rebaseApplyDir;
            }

            var rebaseDir = gitDirectory + "rebase" + Path.DirectorySeparatorChar;
            if (Directory.Exists(rebaseDir))
            {
                return rebaseDir;
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
        public ArgumentString PushAllCmd(string remote, ForcePushOptions force, bool track, int recursiveSubmodules)
        {
            // TODO make an enum for RecursiveSubmodulesOption and add to ArgumentBuilderExtensions
            return new GitArgumentBuilder("push")
            {
                force,
                { track, "-u" },
                { recursiveSubmodules == 1, "--recurse-submodules=check" },
                { recursiveSubmodules == 2, "--recurse-submodules=on-demand" },
                "--progress",
                "--all",
                remote.ToPosixPath().Trim().Quote()
            };
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
        public ArgumentString PushCmd(string remote, string fromBranch, string? toBranch, ForcePushOptions force, bool track, int recursiveSubmodules)
        {
            // This method is for pushing to remote branches, so fully qualify the
            // remote branch name with refs/heads/.
            fromBranch = FormatBranchName(fromBranch)!;
            toBranch = GitRefName.GetFullBranchName(toBranch);

            if (string.IsNullOrEmpty(fromBranch) && !string.IsNullOrEmpty(toBranch))
            {
                fromBranch = "HEAD";
            }

            // TODO make an enum for RecursiveSubmodulesOption and add to ArgumentBuilderExtensions
            return new GitArgumentBuilder("push")
            {
                force,
                { track, "-u" },
                { recursiveSubmodules == 1, "--recurse-submodules=check" },
                { recursiveSubmodules == 2, "--recurse-submodules=on-demand" },
                "--progress",
                remote.ToPosixPath().Trim().Quote(),
                { string.IsNullOrEmpty(toBranch), fromBranch },
                { !string.IsNullOrEmpty(toBranch), $"{fromBranch}:{toBranch}" }
            };
        }

        public string ApplyPatch(string dir, ArgumentString amCommand)
        {
            using var process = _gitExecutable.Start(amCommand, createWindow: false, redirectInput: true, redirectOutput: true, SystemEncoding);
            var files = Directory.GetFiles(dir);

            if (files.Length == 0)
            {
                return "";
            }

            foreach (var file in files)
            {
                using FileStream fs = new(file, FileMode.Open);
                fs.CopyTo(process.StandardInput.BaseStream);
            }

            process.StandardInput.Close();
            process.WaitForExit();

            return process.StandardOutput.ReadToEnd().Trim();
        }

        #region Stage/Unstage

        /// <summary>
        /// Set/unset whether given items are assumed unchanged by git-status.
        /// </summary>
        /// <param name="files">The files to set the status for.</param>
        /// <param name="assumeUnchanged">The status value.</param>
        /// <param name="allOutput">stdout and stderr output.</param>
        /// <returns><see langword="true"/> if no errors occurred; otherwise, <see langword="false"/>.</returns>
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
                    foreach (var file in files)
                    {
                        UpdateIndex(inputWriter, file.Name);
                    }
                },
                SystemEncoding,
                throwOnErrorExit: false);

            allOutput = execution.AllOutput;
            return execution.ExitedSuccessfully;
        }

        /// <summary>
        /// Set/unset whether given items are not flagged as changed by git-status.
        /// </summary>
        /// <param name="files">The files to set the status for.</param>
        /// <param name="skipWorktree">The status value.</param>
        /// <param name="allOutput">stdout and stderr output.</param>
        /// <returns><see langword="true"/> if no errors occurred; otherwise, <see langword="false"/>.</returns>
        public bool SkipWorktreeFiles(IReadOnlyList<GitItemStatus> files, bool skipWorktree, out string allOutput)
        {
            files = files.Where(file => file.IsSkipWorktree != skipWorktree).ToList();

            if (files.Count == 0)
            {
                allOutput = "";
                return true;
            }

            var execution = _gitExecutable.Execute(
                new GitArgumentBuilder("update-index")
                {
                    { skipWorktree ? "--skip-worktree" : "--no-skip-worktree" },
                    "--stdin"
                },
                inputWriter =>
                {
                    foreach (var file in files)
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
            var nonDeletedFiles = files.Where(file => !file.IsDeleted).ToList();
            var deletedFiles = files.Where(file => file.IsDeleted).ToList();

            if (nonDeletedFiles.Count != 0)
            {
                ExecutionResult execution = _gitExecutable.Execute(
                    UpdateIndexCmd(AppSettings.ShowErrorsWhenStagingFiles),
                    inputWriter =>
                    {
                        foreach (var file in nonDeletedFiles)
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
                        foreach (var file in deletedFiles)
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

        public bool StageFile(string file)
        {
            return _gitExecutable.RunCommand(
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
            var nonNewFiles = files.Where(file => !file.IsDeleted).ToList();
            var newFiles = files.Where(file => file.IsDeleted).ToList();

            if (nonNewFiles.Count != 0)
            {
                GitArgumentBuilder sb = new("reset") { "--" };
                foreach (var file in nonNewFiles)
                {
                    sb.Add(file.Name.ToPosixPath().QuoteNE());
                }

                var execution = _gitExecutable.Execute(sb);

                output.AppendLine(execution.AllOutput);
            }

            if (newFiles.Count != 0)
            {
                var execution = _gitExecutable.Execute(
                new GitArgumentBuilder("update-index")
                {
                    "--force-remove",
                    "--stdin"
                },
                inputWriter =>
                {
                    foreach (var file in newFiles)
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

        /// <summary>
        /// Batch unstage files using <see cref="ExecutableExtensions.RunBatchCommand(IExecutable, ICollection{BatchArgumentItem}, Action{BatchProgressEventArgs}, byte[], bool)"/>.
        /// </summary>
        /// <param name="selectedItems">Selected file items.</param>
        /// <param name="action">Progress update callback.</param>
        /// <returns><see langword="true" /> if changes should be rescanned; otherwise <see langword="false" />.</returns>.
        public bool BatchUnstageFiles(IEnumerable<GitItemStatus> selectedItems, Action<BatchProgressEventArgs>? action = null)
        {
            List<GitItemStatus> files = new();
            List<string> filesToRemove = new();
            var shouldRescanChanges = false;
            foreach (var item in selectedItems)
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
                    files.Add(item);
                }
            }

            if (filesToRemove.Count > 0)
            {
                var args = GitCommandHelpers.ResetCmd(ResetMode.ResetIndex, "HEAD");
                _gitExecutable.RunBatchCommand(new ArgumentBuilder() { args }
                    .BuildBatchArgumentsForFiles(filesToRemove),
                    action);
            }

            UnstageFiles(files, out _);

            return shouldRescanChanges;
        }

        private async Task<bool> ExpressIntentToAddAsync(GitItemStatus file)
        {
            return await _gitExecutable.RunCommandAsync(
                new GitArgumentBuilder("add")
                {
                    "--intent-to-add",
                    file.Name.Quote()
                });
        }

        public async Task<bool> AddInteractiveAsync(GitItemStatus file)
        {
            if (file.IsNew)
            {
                bool result = await ExpressIntentToAddAsync(file);
                if (!result)
                {
                    return result;
                }
            }

            GitArgumentBuilder args = new("add")
            {
                "--patch",
                file.Name.Quote()
            };

            using var process = _gitExecutable.Start(args, createWindow: true);
            return await process.WaitForExitAsync() == 0;
        }

        public async Task<bool> ResetInteractiveAsync(GitItemStatus file)
        {
            GitArgumentBuilder args = new("checkout")
            {
                "-p",
                file.Name.Quote()
            };

            using var process = _gitExecutable.Start(args, createWindow: true);
            return await process.WaitForExitAsync() == 0;
        }

        private static void UpdateIndex(StreamWriter inputWriter, string? filename)
        {
            var bytes = EncodingHelper.ConvertTo(
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

        public bool InTheMiddleOfRebase()
        {
            return !File.Exists(GetRebaseDir() + "applying") && Directory.Exists(GetRebaseDir());
        }

        public bool InTheMiddleOfPatch()
        {
            return !File.Exists(GetRebaseDir() + "rebasing") && Directory.Exists(GetRebaseDir());
        }

        public bool InTheMiddleOfMerge()
        {
            return File.Exists(Path.Combine(GetGitDirectory(), "MERGE_HEAD"));
        }

        public bool InTheMiddleOfAction()
        {
            return InTheMiddleOfConflictedMerge() || InTheMiddleOfRebase();
        }

        private string GetNextRebasePatch()
        {
            var file = GetRebaseDir() + "next";

            return File.Exists(file)
                ? File.ReadAllText(file).Trim()
                : "";
        }

        private static readonly Regex HeadersMatch = new(@"^(?<header_key>[-A-Za-z0-9]+)(?::[ \t]*)(?<header_value>.*)$", RegexOptions.Compiled);
        private static readonly Regex QuotedText = new(@"=\?([\w-]+)\?q\?(.*)\?=$", RegexOptions.Compiled);

        private string RebaseTodoFilePath => GetRebaseDir() + "git-rebase-todo.backup";
        private string CurrentFilePath => GetRebaseDir() + "stopped-sha";

        public bool InTheMiddleOfInteractiveRebase() => File.Exists(RebaseTodoFilePath);

        public IReadOnlyList<PatchFile> GetInteractiveRebasePatchFiles()
        {
            string todoFile = RebaseTodoFilePath;
            string[]? todoCommits = File.Exists(todoFile) ? File.ReadAllText(todoFile).Trim().Split(Delimiters.LineFeedAndCarriageReturn, StringSplitOptions.RemoveEmptyEntries) : null;

            List<PatchFile> patchFiles = new();

            if (todoCommits is not null)
            {
                string commentChar = EffectiveConfigFile.GetString("core.commentChar", "#");

                string? currentCommitShortHash = File.Exists(CurrentFilePath) ? File.ReadAllText(CurrentFilePath).Trim() : null;
                var isCurrentFound = false;
                foreach (string todoCommit in todoCommits)
                {
                    if (todoCommit.StartsWith(commentChar))
                    {
                        continue;
                    }

                    string[] parts = todoCommit.Split(Delimiters.Space);

                    if (parts.Length < 3)
                    {
                        continue;
                    }

                    string commitHash = parts[1];
                    CommitData? data = _commitDataManager.GetCommitData(commitHash, out var error);
                    var isApplying = currentCommitShortHash is not null && commitHash.StartsWith(currentCommitShortHash);
                    isCurrentFound |= isApplying;

                    patchFiles.Add(new PatchFile
                    {
                        Author = error ?? data?.Author,
                        ObjectId = data?.ObjectId,
                        Subject = error ?? data?.Body,
                        Action = parts[0],
                        Date = error ?? data?.CommitDate.LocalDateTime.ToString(),
                        IsNext = isApplying,
                        IsApplied = !isCurrentFound,
                    });
                }
            }

            return patchFiles;
        }

        public IReadOnlyList<PatchFile> GetRebasePatchFiles()
        {
            List<PatchFile> patchFiles = new();

            var nextFile = GetNextRebasePatch();

            int.TryParse(nextFile, out var next);

            var rebaseDir = GetRebaseDir();

            var files = Directory.Exists(rebaseDir)
                ? Directory.GetFiles(rebaseDir)
                : Array.Empty<string>();

            foreach (var fullFileName in files)
            {
                var file = PathUtil.GetFileName(fullFileName);
                if (!int.TryParse(file, out var n))
                {
                    continue;
                }

                PatchFile patchFile =
                    new()
                    {
                        Name = file,
                        FullName = fullFileName,
                        IsApplied = n < next,
                        IsNext = n == next
                    };

                if (File.Exists(rebaseDir + file))
                {
                    string? key = null;
                    string value = "";
                    foreach (var line in File.ReadLines(rebaseDir + file))
                    {
                        var m = HeadersMatch.Match(line);
                        if (key is null)
                        {
                            if (!string.IsNullOrWhiteSpace(line) && !m.Success)
                            {
                                continue;
                            }
                        }
                        else if (string.IsNullOrWhiteSpace(line) || m.Success)
                        {
                            // decode QuotedPrintable text using .NET internal decoder
                            value = Attachment.CreateAttachmentFromString("", value).Name;
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
                        else if (!string.IsNullOrEmpty(line))
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

            string AppendQuotedString(string str1, string str2)
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
        }

        public ArgumentString CommitCmd(bool amend, bool signOff = false, string author = "", bool useExplicitCommitMessage = true, bool noVerify = false, bool gpgSign = false, string gpgKeyId = "", bool allowEmpty = false, bool resetAuthor = false)
        {
            return new GitArgumentBuilder("commit")
            {
                { amend, "--amend" },
                { noVerify, "--no-verify" },
                { signOff, "--signoff" },
                { !string.IsNullOrEmpty(author), $"--author=\"{author?.Trim().Trim('"')}\"" },
                { gpgSign && string.IsNullOrWhiteSpace(gpgKeyId), "-S" },
                { gpgSign && !string.IsNullOrWhiteSpace(gpgKeyId), $"-S{gpgKeyId}" },
                { useExplicitCommitMessage, $"-F \"{GetGitExecPath(Path.Combine(GetGitDirectory(), "COMMITMESSAGE"))}\"" },
                { allowEmpty, "--allow-empty" },
                { resetAuthor && amend, "--reset-author" }
            };
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

        public string RenameBranch(string name, string newName)
        {
            GitArgumentBuilder args = new("branch")
            {
                "-m",
                name.QuoteNE(),
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
                    GetGitExecPath(path).QuoteNE()
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

        private static readonly Regex _remoteVerboseLineRegex = new(@"^(?<name>[^	]+)\t(?<url>.+?) \((?<direction>fetch|push)\)$", RegexOptions.Compiled);

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
                List<Remote> remotes = new();

                // See tests for explanation of the format

                using var enumerator = lines.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var remoteLine = enumerator.Current;
                    var remoteMatch = _remoteVerboseLineRegex.Match(remoteLine);
                    if (!remoteMatch.Success
                        || (remoteMatch.Groups["direction"].Value != "fetch"
                           && remoteMatch.Groups["direction"].Value != "push"))
                    {
                        // Ignore malformed and unknown entries
                        continue;
                    }

                    var name = remoteMatch.Groups["name"].Value;
                    var remoteUrl = remoteMatch.Groups["url"].Value;
                    if (PathUtil.IsLocalFile(remoteUrl))
                    {
                        remoteUrl = GetWindowsPath(remoteUrl).ToPosixPath();
                    }

                    if (remoteMatch.Groups["direction"].Value == "push")
                    {
                        if (remotes.Count <= 0 || name != remotes[remotes.Count - 1].Name)
                        {
                            throw new Exception("Unable to update remote pushurl for command output: " + remoteLine);
                        }

                        remotes[remotes.Count - 1].PushUrls.Add(remoteUrl);
                        continue;
                    }

                    if (!enumerator.MoveNext())
                    {
                        throw new Exception("Remote URLs should appear in pairs, no pushurl for fetch: " + remoteLine);
                    }

                    var pushLine = enumerator.Current;
                    var pushMatch = _remoteVerboseLineRegex.Match(pushLine);
                    if (!pushMatch.Success || pushMatch.Groups["direction"].Value != "push")
                    {
                        throw new Exception("Unable to parse git remote push URL line: " + pushLine);
                    }

                    var pushUrl = pushMatch.Groups["url"].Value;
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

        public void SetSetting(string setting, string? value)
        {
            LocalConfigFile.SetValue(setting, value);
        }

        // TODO: remove
        public void SetPathSetting(string setting, string value)
        {
            LocalConfigFile.SetPathValue(setting, value);
        }

        internal GitArgumentBuilder GetStashesCmd(bool noLocks)
        {
            return new GitArgumentBuilder("stash", gitOptions: noLocks ? (ArgumentString)"--no-optional-locks" : default)
                { "list" };
        }

        public IReadOnlyList<GitStash> GetStashes(bool noLocks = false)
        {
            var args = GetStashesCmd(noLocks);
            var lines = _gitExecutable.GetOutput(args).Split(Delimiters.LineFeed);

            List<GitStash> stashes = new(lines.Length);

            foreach (var line in lines)
            {
                if (GitStash.TryParse(line, out var stash))
                {
                    stashes.Add(stash);
                }
            }

            return stashes;
        }

        public async Task<Patch?> GetSingleDiffAsync(
            ObjectId? firstId, ObjectId? secondId,
            string? fileName, string? oldFileName,
            string extraDiffArguments, Encoding encoding,
            bool cacheResult, bool isTracked = true)
        {
            // fix refs slashes
            fileName = fileName.ToPosixPath();
            oldFileName = oldFileName.ToPosixPath();
            string? firstRevision = firstId?.ToString().ToPosixPath();
            string? secondRevision = secondId?.ToString().ToPosixPath();

            string? diffOptions = _revisionDiffProvider.Get(firstRevision, secondRevision, fileName, oldFileName, isTracked);

            GitArgumentBuilder args = new("diff")
            {
                "--find-renames",
                "--find-copies",
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

            bool nonZeroGitExitCode = firstId == ObjectId.WorkTreeId && secondId is not null && !isTracked;
            ExecutionResult result = await _gitExecutable.ExecuteAsync(
                args,
                cache: cache,
                outputEncoding: LosslessEncoding,
                throwOnErrorExit: !nonZeroGitExitCode);

            string patch = result.StandardOutput;
            IReadOnlyList<Patch> patches = PatchProcessor.CreatePatchesFromString(patch, new Lazy<Encoding>(() => encoding)).ToList();

            return GetPatch(patches, fileName, oldFileName);
        }

        public async Task<string> GetRangeDiffAsync(
            ObjectId firstId,
            ObjectId secondId,
            ObjectId? firstBase,
            ObjectId? secondBase,
            string extraDiffArguments,
            string? pathFilter,
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
            GitArgumentBuilder args = new("range-diff")
            {
                "--find-renames",
                "--find-copies",
                { AppSettings.UseHistogramDiffAlgorithm, "--histogram" },
                extraDiffArguments,
                { firstBase is null || secondBase is null,  $"{first}...{second}", $"{firstBase}..{first} {secondBase}..{second}" },
                { GitVersion.SupportRangeDiffPath && !string.IsNullOrWhiteSpace(pathFilter), "--" },
                { GitVersion.SupportRangeDiffPath && !string.IsNullOrWhiteSpace(pathFilter), pathFilter }
            };

            ExecutionResult result = await _gitExecutable.ExecuteAsync(
                args,
                cache: GitCommandCache,
                outputEncoding: LosslessEncoding,
                cancellationToken: cancellationToken);

            return result.StandardOutput;
        }

        private static Patch? GetPatch(IReadOnlyList<Patch> patches, string? fileName, string? oldFileName)
        {
            foreach (var patch in patches)
            {
                if (fileName == patch.FileNameB && (fileName == patch.FileNameA || oldFileName == patch.FileNameA))
                {
                    return patch;
                }
            }

            return patches.Count != 0
                ? patches[patches.Count - 1]
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
            var stagedStatus = GetStagedStatus(firstId, secondId, parentToSecond);
            var status = GetDiffFilesWithUntracked(firstId?.ToString(), secondId?.ToString(), stagedStatus, cancellationToken: cancellationToken);
            GetSubmoduleStatus(status, firstId, secondId);
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
                var status = GetAllChangedFilesWithSubmodulesStatus(cancellationToken: cancellationToken);
                return status.Where(x => x.Staged == stagedStatus).ToList();
            }

            ExecutionResult exec = GetDiffFiles(firstRevision, secondRevision, noCache: noCache, nullSeparated: true, cancellationToken);
            var result = GetDiffChangedFilesFromString(exec.StandardOutput, stagedStatus);
            if (!exec.ExitedSuccessfully)
            {
                result.Add(createErrorGitItemStatus(exec.StandardError));
            }

            if (firstRevision == GitRevision.WorkTreeGuid || secondRevision == GitRevision.WorkTreeGuid)
            {
                // For worktree the untracked must be added too
                // Note that this may add a second GitError, this is a separate Git command
                var files = GetAllChangedFilesWithSubmodulesStatus(cancellationToken: cancellationToken).Where(x =>
                    ((x.Staged == StagedStatus.WorkTree && x.IsNew) || x.IsStatusOnly)).ToArray();
                if (firstRevision == GitRevision.WorkTreeGuid)
                {
                    // The file is seen as "deleted" in 'to' revision
                    foreach (var item in files)
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
            var resultCollection = GetDiffFilesWithUntracked(stashName + "^", stashName, StagedStatus.None, true).ToList();

            // add - optionally stashed - untracked files
            GitArgumentBuilder args = new("log")
            {
                $"{stashName}^3",
                "--pretty=format:\"%T\"",
                "--max-count=1"
            };
            ExecutionResult executionResult = _gitExecutable.Execute(args, throwOnErrorExit: false);
            if (executionResult.ExitedSuccessfully && ObjectId.TryParse(executionResult.StandardOutput, out ObjectId? treeId))
            {
                var files = GetTreeFiles(treeId, full: true);

                resultCollection.AddRange(files);
            }

            return resultCollection;
        }

        public IReadOnlyList<GitItemStatus> GetTreeFiles(ObjectId commitId, bool full)
        {
            var tree = GetTree(commitId, full);

            var list = tree
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
            UntrackedFilesMode untrackedFiles = UntrackedFilesMode.Default, CancellationToken cancellationToken = default)
        {
            ExecutionResult exec = _gitExecutable.Execute(GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles, untrackedFiles), throwOnErrorExit: false, cancellationToken: cancellationToken);
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
                    result.AddRange(GetSkipWorktreeFilesFromString(lsOutput));
                }
            }

            return result;
        }

        private static IReadOnlyList<GitItemStatus> GetAssumeUnchangedFilesFromString(string lsString)
        {
            List<GitItemStatus> result = new();

            foreach (string line in lsString.LazySplit('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                char statusCharacter = line[0];
                if (char.IsUpper(statusCharacter))
                {
                    continue;
                }

                string fileName = line.SubstringAfter(' ');
                GitItemStatus gitItemStatus = GitItemStatusConverter.FromStatusCharacter(StagedStatus.WorkTree, fileName, statusCharacter);
                gitItemStatus.IsAssumeUnchanged = true;
                result.Add(gitItemStatus);
            }

            return result;
        }

        private static IReadOnlyList<GitItemStatus> GetSkipWorktreeFilesFromString(string lsString)
        {
            List<GitItemStatus> result = new();

            foreach (string line in lsString.LazySplit('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                char statusCharacter = line[0];

                string fileName = line.SubstringAfter(' ');
                GitItemStatus gitItemStatus = GitItemStatusConverter.FromStatusCharacter(StagedStatus.WorkTree, fileName, statusCharacter);
                if (gitItemStatus.IsSkipWorktree)
                {
                    result.Add(gitItemStatus);
                }
            }

            return result;
        }

        public IReadOnlyList<GitItemStatus> GetAllChangedFilesWithSubmodulesStatus(bool excludeIgnoredFiles = true,
            bool excludeAssumeUnchangedFiles = true, bool excludeSkipWorktreeFiles = true,
            UntrackedFilesMode untrackedFiles = UntrackedFilesMode.Default, CancellationToken cancellationToken = default)
        {
            var status = GetAllChangedFiles(excludeIgnoredFiles, excludeAssumeUnchangedFiles, excludeSkipWorktreeFiles, untrackedFiles, cancellationToken);
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
                        return await SubmoduleHelpers.GetCurrentSubmoduleChangesAsync(this, localItem.Name, localItem.OldName, localItem.Staged == StagedStatus.Index)
                            .ConfigureAwait(false);
                    }));
                }
            }
        }

        private void GetSubmoduleStatus(IReadOnlyList<GitItemStatus> status, ObjectId? firstId, ObjectId? secondId)
        {
            foreach (var item in status.Where(i => i.IsSubmodule))
            {
                item.SetSubmoduleStatus(
                    ThreadHelper.JoinableTaskFactory.RunAsync(
                        async () =>
                        {
                            await TaskScheduler.Default.SwitchTo(alwaysYield: true);
                            return await SubmoduleHelpers.GetCurrentSubmoduleChangesAsync(this, item.Name, item.OldName, firstId, secondId)
                                .ConfigureAwait(false);
                        }));
            }
        }

        public IReadOnlyList<GitItemStatus> GetIndexFiles()
        {
            GitArgumentBuilder args = new("diff")
            {
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
            ArgumentString command = GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles: true, UntrackedFilesMode.No);
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
            var status = GetIndexFiles();
            GetCurrentSubmoduleStatus(status);
            return status;
        }

        public IReadOnlyList<GitItemStatus> GetWorkTreeFiles()
        {
            return GetAllChangedFiles().Where(x => x.Staged == StagedStatus.WorkTree).ToArray();
        }

        public IReadOnlyList<GitItemStatus> GitStatus(UntrackedFilesMode untrackedFilesMode, IgnoreSubmodulesMode ignoreSubmodulesMode = IgnoreSubmodulesMode.None)
        {
            ArgumentString args = GitCommandHelpers.GetAllChangedFilesCmd(true, untrackedFilesMode, ignoreSubmodulesMode);
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
            var output = await _gitExecutable.GetOutputAsync(GitCommandHelpers.GetCurrentChangesCmd(fileName, oldFileName, staged, extraDiffArguments, noLocks),
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
            var args = GitCommandHelpers.ResetCmd(ResetMode.ResetIndex, "HEAD", file);
            _gitExecutable.RunCommand(args);
        }

        /// <summary>Dirty but fast. This sometimes fails.</summary>
        public static string GetSelectedBranchFast(string? repositoryPath, bool setDefaultIfEmpty = true)
        {
            if (string.IsNullOrEmpty(repositoryPath))
            {
                return string.Empty;
            }

            string headFileContents;
            try
            {
                // eg. "/path/to/repo/.git/HEAD"
                var headFileName = Path.Combine(GetGitDirectory(repositoryPath), "HEAD");

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
                return setDefaultIfEmpty ? DetachedHeadParser.DetachedBranch : string.Empty;
            }

            const string prefix = "ref: refs/heads/";

            if (!headFileContents.StartsWith(prefix))
            {
                return string.Empty;
            }

            return headFileContents.Substring(prefix.Length).TrimEnd();
        }

        /// <summary>
        /// Gets the current branch.
        /// </summary>
        /// <param name="setDefaultIfEmpty">Return "(no branch)" if detached.</param>
        /// <returns>Current branch name.</returns>
        public string GetSelectedBranch(bool setDefaultIfEmpty)
        {
            string head = GetSelectedBranchFast(WorkingDir, setDefaultIfEmpty);

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
                : setDefaultIfEmpty ? DetachedHeadParser.DetachedBranch : string.Empty;
        }

        public string GetSelectedBranch()
        {
            return GetSelectedBranch(true);
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

        /// <summary>Gets the remote branch of the specified local branch; or "" if none is configured.</summary>
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

        public RemoteActionResult<IReadOnlyList<IGitRef>> GetRemoteServerRefs(string remote, bool tags, bool branches)
        {
            RemoteActionResult<IReadOnlyList<IGitRef>> result = new()
            {
                AuthenticationFail = false,
                HostKeyFail = false,
                Result = null
            };

            ExecutionResult executionResult = !tags && !branches
                ? new() // TODO is this an error?
                : _gitExecutable.Execute(new GitArgumentBuilder("ls-remote")
                    {
                        { tags, "--tags" },
                        { branches, "--heads" },
                        remote.ToPosixPath().QuoteNE()
                    },
                    throwOnErrorExit: false);

            // TODO AllOutput is parsed at errors, can this be detected better?
            string output = executionResult.AllOutput;

            if (executionResult.ExitedSuccessfully)
            {
                result.Result = ParseRefs(executionResult.StandardOutput);
            }
            else if (output.Contains("FATAL ERROR") && output.Contains("authentication"))
            {
                // If the authentication failed because of a missing key, ask the user to supply one.
                result.AuthenticationFail = true;
            }
            else if (output.Contains("the server's host key is not cached in the registry", StringComparison.InvariantCultureIgnoreCase))
            {
                result.HostKeyFail = true;
            }

            return result;
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

            ArgumentString cmd = GitCommandHelpers.GetRefsCmd(getRef, noLocks, AppSettings.RefsSortBy, AppSettings.RefsSortOrder);
            ExecutionResult result = _gitExecutable.Execute(cmd, throwOnErrorExit: false);
            return result.ExitedSuccessfully
                ? ParseRefs(result.StandardOutput)
                : Array.Empty<IGitRef>();
        }

        public async Task<string[]> GetMergedBranchesAsync(bool includeRemote = false, bool fullRefname = false, string? commit = null)
        {
            ExecutionResult result = await _gitExecutable
                .ExecuteAsync(GitCommandHelpers.MergedBranchesCmd(includeRemote, fullRefname, commit), throwOnErrorExit: false)
                .ConfigureAwait(false);
            ////TODO: Handle non-empty result.StandardError
            return result.StandardOutput.Split(Delimiters.LineFeed, StringSplitOptions.RemoveEmptyEntries);
        }

        public IEnumerable<string> GetMergedBranches(bool includeRemote = false)
        {
            return _gitExecutable
                .GetOutput(GitCommandHelpers.MergedBranchesCmd(includeRemote))
                .LazySplit('\n', StringSplitOptions.RemoveEmptyEntries);
        }

        public IReadOnlyList<string> GetMergedRemoteBranches()
        {
            const string remoteBranchPrefixForMergedBranches = "remotes/";
            const string refsPrefix = "refs/";

            var remotes = GetRemoteNames();
            ExecutionResult result = _gitExecutable.Execute(GitCommandHelpers.MergedBranchesCmd(includeRemote: true));
            var lines = result.StandardOutput.LazySplit('\n');

            return lines
                .Select(b => b.Trim())
                .Where(b => b.StartsWith(remoteBranchPrefixForMergedBranches))
                .Select(b => string.Concat(refsPrefix, b))
                .Where(b => !string.IsNullOrEmpty(GitRefName.GetRemoteName(b, remotes)))
                .ToList();
        }

        public IReadOnlyList<IGitRef> ParseRefs(string refList)
        {
            var matches = _refRegex.Matches(refList);

            List<IGitRef> gitRefs = new();
            Dictionary<string, GitRef> headByRemote = new();

            foreach (Match match in matches)
            {
                var refName = match.Groups["refname"].Value;
                var objectId = ObjectId.Parse(refList, match.Groups["objectid"]);
                var remoteName = GitRefName.GetRemoteName(refName);
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
            foreach (var gitRef in gitRefs)
            {
                if (headByRemote.TryGetValue(gitRef.Remote, out var defaultHead) &&
                    gitRef.ObjectId == defaultHead.ObjectId)
                {
                    headByRemote.Remove(gitRef.Remote);
                }
            }

            gitRefs.AddRange(headByRemote.Values);

            return gitRefs;
        }

        /// <summary>
        /// Gets branches which contain the given commit.
        /// If both local and remote branches are requested, remote branches are prefixed with "remotes/"
        /// (as returned by git branch -a).
        /// </summary>
        /// <param name="objectId">The sha1.</param>
        /// <param name="getLocal">Pass true to include local branches.</param>
        /// <param name="getRemote">Pass true to include remote branches.</param>
        public IReadOnlyList<string> GetAllBranchesWhichContainGivenCommit(ObjectId objectId, bool getLocal, bool getRemote)
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
            ExecutionResult exec = _gitExecutable.Execute(args, throwOnErrorExit: false);
            if (!exec.ExitedSuccessfully)
            {
                // Error occurred, no matches (no error presented to the user)
                return Array.Empty<string>();
            }

            var result = exec.StandardOutput.Split(new[] { '\r', '\n', '*', '+' }, StringSplitOptions.RemoveEmptyEntries);

            // Remove symlink targets as in "origin/HEAD -> origin/master"
            for (var i = 0; i < result.Length; i++)
            {
                var item = result[i].Trim();

                if (getRemote)
                {
                    int idx = item.IndexOf(" ->", StringComparison.Ordinal);
                    if (idx >= 0)
                    {
                        item = item.Substring(0, idx);
                    }
                }

                result[i] = item;
            }

            return result;
        }

        /// <summary>
        /// Gets all tags which contain the given commit.
        /// </summary>
        /// <param name="objectId">The sha1.</param>
        public IReadOnlyList<string> GetAllTagsWhichContainGivenCommit(ObjectId objectId)
        {
            ExecutionResult exec = _gitExecutable.Execute($"tag --contains {objectId}", throwOnErrorExit: false);
            if (!exec.ExitedSuccessfully)
            {
                // Error occurred, no matches (no error presented to the user)
                return Array.Empty<string>();
            }

            return exec.StandardOutput.Split(new[] { '\r', '\n', '*', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Returns tag's message. If the lightweight tag is passed, corresponding commit message
        /// is returned.
        /// </summary>
        public string? GetTagMessage(string? tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return null;
            }

            tag = tag.Trim();

            ExecutionResult exec = _gitExecutable.Execute($"cat-file -p {tag}", throwOnErrorExit: false);
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
            var notEmptyPatterns = ignorePatterns
                .Where(pattern => !string.IsNullOrWhiteSpace(pattern))
                .ToList();

            if (notEmptyPatterns.Count != 0)
            {
                var excludeParams =
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
                    $"ls-tree -z -r --name-only {id}",
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

            var tree = _gitExecutable.GetOutput(args, cache: GitCommandCache);

            return _gitTreeParser.Parse(tree);
        }

        public GitBlame Blame(string? fileName, string from, Encoding encoding, string? lines = null, CancellationToken cancellationToken = default)
        {
            GitArgumentBuilder args = new("blame")
            {
                "--porcelain",
                { AppSettings.DetectCopyInFileOnBlame, "-M" },
                { AppSettings.DetectCopyInAllOnBlame, "-C" },
                { AppSettings.IgnoreWhitespaceOnBlame, "-w" },
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

            var output = result.StandardOutput;

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

            Dictionary<ObjectId, GitBlameCommit> commitByObjectId = new();
            List<GitBlameLine> lines = new(capacity: 256);

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

            foreach (var line in output.LazySplit('\n').Select(l => l.TrimEnd('\r')))
            {
                var match = _headerRegex.Match(line);

                if (match.Success)
                {
                    objectId = ObjectId.Parse(line, match.Groups["objectid"]);
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
                        // TODO quite a few nullable suppressions here (via ! character) which should be addressed as they hint at a design flaw

                        if (!commitByObjectId.ContainsKey(objectId!))
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
                            var commitData = commitByObjectId[objectId!];
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

        public string GetFileText(ObjectId id, Encoding encoding)
        {
            GitArgumentBuilder args = new("cat-file")
            {
                "blob",
                id.ToString().QuoteNE()
            };
            return _gitExecutable.GetOutput(
                args,
                cache: GitCommandCache,
                outputEncoding: encoding);
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
                var lines = _gitExecutable.GetOutput(args).Split(Delimiters.TabAndSpace);

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
                var lines = _gitExecutable.GetOutput(args).Split(Delimiters.TabAndSpace);
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
                using var process = _gitCommandRunner.RunDetached(args, redirectOutput: true);
                MemoryStream stream = new();
                process.StandardOutput.BaseStream.CopyTo(stream);
                process.WaitForExit();
                stream.Position = 0;
                return stream;
            }
            catch (Win32Exception ex)
            {
                Trace.WriteLine(ex);
                return null;
            }
        }

        public IEnumerable<string?> GetPreviousCommitMessages(int count, string revision = "HEAD", string authorPattern = "")
        {
            GitArgumentBuilder args = new("log")
            {
                "-z",
                $"-n {count}",
                revision,
                "--pretty=format:%B",
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

        /// <summary>
        /// Get a list of diff/merge tools known by Git.
        /// This normally requires long time (up to tenths of seconds)
        /// </summary>
        /// <param name="isDiff">diff or merge.</param>
        /// <returns>the Git output.</returns>
        public string GetCustomDiffMergeTools(bool isDiff, CancellationToken cancellationToken)
        {
            // Use a global list of custom tools, always use Windows tools (native paths for the app).
            // Note that --gui has no effect here
            GitArgumentBuilder args = new(isDiff ? "difftool" : "mergetool") { "--tool-help" };
            ExecutionResult result = _gitWindowsExecutable.Execute(args, cancellationToken: cancellationToken);
            return result.StandardOutput;
        }

        public string OpenWithDifftoolDirDiff(string? firstRevision, string? secondRevision, string? customTool = null)
        {
            return OpenWithDifftool(null, firstRevision: firstRevision, secondRevision: secondRevision, extraDiffArguments: "--dir-diff", customTool: customTool);
        }

        public string OpenWithDifftool(string? filename, string? oldFileName = "", string? firstRevision = GitRevision.IndexGuid, string? secondRevision = GitRevision.WorkTreeGuid, string? extraDiffArguments = null, bool isTracked = true, string? customTool = null)
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

            // This method is supposed to return an error message, but the detached process is untracked
            // TODO track the process somehow, so errors can be reported
            return "";
        }

        /// <summary>
        /// Compare two Git commitish; blob or rev:path.
        /// </summary>
        /// <param name="firstGitCommit">commitish.</param>
        /// <param name="secondGitCommit">commitish.</param>
        /// <returns>empty string, or null if either input is null.</returns>
        public string? OpenFilesWithDifftool(string? firstGitCommit, string? secondGitCommit, string? customTool = null)
        {
            if (string.IsNullOrWhiteSpace(firstGitCommit) || string.IsNullOrWhiteSpace(secondGitCommit))
            {
                return null;
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

            return "";
        }

        public ObjectId? RevParse(string? revisionExpression)
        {
            if (string.IsNullOrWhiteSpace(revisionExpression) || revisionExpression.Length > 260)
            {
                return null;
            }

            if (ObjectId.TryParse(revisionExpression, out var objectId))
            {
                return objectId;
            }

            GitArgumentBuilder args = new("rev-parse")
            {
                "--quiet",
                "--verify",
                $"\"{revisionExpression}~0\""
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

            return ObjectId.TryParse(output, offset: 0, out var objectId)
                ? objectId
                : null;
        }

        public SubmoduleStatus CheckSubmoduleStatus(ObjectId? commit, ObjectId? oldCommit, CommitData? data, CommitData? oldData, bool loadData = false)
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

            var baseCommit = baseOid;
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
                oldData = _commitDataManager.GetCommitData(oldCommit.ToString(), out _, cache: true);
            }

            if (oldData is null)
            {
                return SubmoduleStatus.NewSubmodule;
            }

            if (loadData)
            {
                data = _commitDataManager.GetCommitData(commit.ToString(), out _, cache: true);
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

        public SubmoduleStatus CheckSubmoduleStatus(ObjectId? commit, ObjectId? oldCommit)
        {
            return CheckSubmoduleStatus(commit, oldCommit, null, null, true);
        }

        /// <summary>
        /// Uses check-ref-format to ensure that a branch name is well formed.
        /// </summary>
        /// <param name="branchName">Branch name to test.</param>
        /// <returns>true if <paramref name="branchName"/> is valid reference name, otherwise false.</returns>
        public bool CheckBranchFormat(string branchName)
        {
            if (branchName is null)
            {
                throw new ArgumentNullException(nameof(branchName));
            }

            if (string.IsNullOrWhiteSpace(branchName))
            {
                return false;
            }

            branchName = branchName.Replace("\"", "\\\"");
            GitArgumentBuilder args = new("check-ref-format")
            {
                "--branch",
                branchName.QuoteNE()
            };
            return _gitExecutable.RunCommand(args);
        }

        /// <summary>
        /// Format branch name, check if name is valid for repository.
        /// </summary>
        /// <param name="branchName">Branch name to test.</param>
        /// <returns>Well formed branch name.</returns>
        private string? FormatBranchName(string branchName)
        {
            if (branchName is null)
            {
                throw new ArgumentNullException(nameof(branchName));
            }

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
            var cmd = Path.Combine(AppSettings.GitBinDir, "ps");
            string[] lines = new Executable(cmd).GetOutput("x").Split(Delimiters.LineFeed);

            if (lines.Length <= 2)
            {
                return false;
            }

            var headers = lines[0].LazySplit(' ', StringSplitOptions.RemoveEmptyEntries);
            var commandIndex = headers.IndexOf(header => header == "COMMAND");
            for (int i = 1; i < lines.Length; i++)
            {
                var columns = lines[i].Split(Delimiters.Space, StringSplitOptions.RemoveEmptyEntries);
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

        private static readonly Regex _escapedOctalCodePointRegex = new(@"(\\([0-7]{3}))+", RegexOptions.Compiled);

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

            var bytes = fromEncoding.GetBytes(s);
            return toEncoding.GetString(bytes);
        }

        /// <summary>
        /// Re-encodes string from GitCommandHelpers.LosslessEncoding to toEncoding.
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

        public override string ToString()
        {
            return WorkingDir;
        }

        public string? GetLocalTrackingBranchName(string remoteName, string branch)
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
            GitArgumentBuilder args = new("diff-tree")
            {
                "--name-only",
                "-z",
                "--cc",
                "--no-commit-id",
                shaOfMergeCommit
            };

            var fileList = _gitExecutable.GetOutput(args);

            if (string.IsNullOrWhiteSpace(fileList))
            {
                return Array.Empty<GitItemStatus>();
            }

            var files = fileList.LazySplit('\0', StringSplitOptions.RemoveEmptyEntries);

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

        public string? GetCombinedDiffContent(ObjectId revisionOfMergeCommit, string filePath, string extraArgs, Encoding encoding)
        {
            GitArgumentBuilder args = new("diff-tree")
            {
                { AppSettings.OmitUninterestingDiff, "--cc", "-c -p" },
                "--no-commit-id",
                extraArgs,
                revisionOfMergeCommit,
                { AppSettings.UseHistogramDiffAlgorithm, "--histogram" },
                "--",
                filePath.ToPosixPath().Quote()
            };

            var patch = _gitExecutable.GetOutput(args, cache: GitCommandCache, outputEncoding: LosslessEncoding);

            if (string.IsNullOrWhiteSpace(patch))
            {
                return "";
            }

            var patches = PatchProcessor.CreatePatchesFromString(patch, new Lazy<Encoding>(() => encoding)).ToList();

            return GetPatch(patches, filePath, filePath)?.Text;
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
        public string? GetDescribe(ObjectId commitId)
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

        public (int totalCount, Dictionary<string, int> countByName) GetCommitsByContributor(DateTime? since = null, DateTime? until = null)
        {
            Dictionary<string, int> countByName = new();
            var totalCommits = 0;

            Regex regex = new(@"^\s*(?<count>\d+)\s+(?<name>.*)$");
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
            var lines = result.StandardOutput.LazySplit('\n');

            foreach (var line in lines)
            {
                var match = regex.Match(line);

                if (!match.Success)
                {
                    continue;
                }

                var count = int.Parse(match.Groups["count"].Value);
                var name = match.Groups["name"].Value;

                totalCommits += count;

                if (!countByName.TryGetValue(name, out var oldCount))
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
