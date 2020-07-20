using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.Git.Extensions;
using GitCommands.Patches;
using GitCommands.Settings;
using GitCommands.Utils;
using GitExtUtils;
using GitUI;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;

namespace GitCommands
{
    /// <summary>Provides manipulation with git module.
    /// <remarks>Several instances may be created for submodules.</remarks></summary>
    [DebuggerDisplay("GitModule ( {" + nameof(WorkingDir) + "} )")]
    public sealed class GitModule : IGitModule
    {
        private const string GitError = "Git Error";
        private static readonly Regex CpEncodingPattern = new Regex("cp\\d+", RegexOptions.Compiled);
        private static readonly IGitDirectoryResolver GitDirectoryResolverInstance = new GitDirectoryResolver();

        public static readonly string NoNewLineAtTheEnd = "\\ No newline at end of file";
        public static CommandCache GitCommandCache { get; } = new CommandCache();

        private readonly object _lock = new object();
        private readonly IIndexLockManager _indexLockManager;
        private readonly ICommitDataManager _commitDataManager;
        private readonly IGitTreeParser _gitTreeParser = new GitTreeParser();
        private readonly IRevisionDiffProvider _revisionDiffProvider = new RevisionDiffProvider();
        private readonly IGitCommandRunner _gitCommandRunner;
        private readonly IExecutable _gitExecutable;

        public GitModule([CanBeNull] string workingDir)
        {
            WorkingDir = (workingDir ?? "").NormalizePath().EnsureTrailingPathSeparator();
            WorkingDirGitDir = GitDirectoryResolverInstance.Resolve(WorkingDir);
            _indexLockManager = new IndexLockManager(this);
            _commitDataManager = new CommitDataManager(() => this);
            _gitExecutable = new Executable(() => AppSettings.GitCommand, WorkingDir);
            _gitCommandRunner = new GitCommandRunner(_gitExecutable, () => SystemEncoding);

            // If this is a submodule, populate relevant properties.
            // If this is not a submodule, these will all be null.
            (SuperprojectModule, SubmodulePath, SubmoduleName) = InitialiseSubmoduleProperties();

            return;

            (GitModule superprojectModule, string submodulePath, string submoduleName) InitialiseSubmoduleProperties()
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
                if (superprojectPath == null && File.Exists(gitDir))
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
                    var configFile = new ConfigFile(Path.Combine(superprojectPath, ".gitmodules"), local: true);

                    foreach (var configSection in configFile.ConfigSections)
                    {
                        if (configSection.GetValue("path") == submodulePath.ToPosixPath())
                        {
                            var submoduleName = configSection.SubSection;
                            var superprojectModule = new GitModule(superprojectPath);

                            return (superprojectModule, submodulePath, submoduleName);
                        }
                    }

                    return (null, submodulePath, null);
                }

                return (null, null, null);

                bool HasGitModulesFile(string path)
                    => File.Exists(Path.Combine(path, ".gitmodules")) && IsValidGitWorkingDir(path);
            }
        }

        /// <summary>
        /// Gets the directory which contains the git repository.
        /// </summary>
        [NotNull]
        public string WorkingDir { get; }

        /// <summary>
        /// Gets the access to the current git executable associated with this module.
        /// </summary>
        [NotNull]
        public IExecutable GitExecutable => _gitExecutable;

        /// <summary>
        /// Gets the access to the current git executable associated with this module.
        /// </summary>
        [NotNull]
        public IGitCommandRunner GitCommandRunner => _gitCommandRunner;

        /// <summary>
        /// Gets the location of .git directory for the current working folder.
        /// </summary>
        [NotNull]
        public string WorkingDirGitDir { get; private set; }

        /// <summary>
        /// If this module is a submodule, returns its name, otherwise <c>null</c>.
        /// </summary>
        [CanBeNull]
        public string SubmoduleName { get; }

        /// <summary>
        /// If this module is a submodule, returns its path, otherwise <c>null</c>.
        /// </summary>
        [CanBeNull]
        public string SubmodulePath { get; }

        /// <summary>
        /// If this module is a submodule, returns its superproject <see cref="GitModule"/>, otherwise <c>null</c>.
        /// </summary>
        /// TODO: Add to IGitModule and return IGitModule
        [CanBeNull]
        public GitModule SuperprojectModule { get; }

        /// <summary>
        /// If this module is a submodule, returns the top-most parent module, otherwise it returns itself.
        /// </summary>
        /// TODO: Add to IGitModule and return IGitModule
        [NotNull]
        public GitModule GetTopModule()
        {
            GitModule topModule = this;
            while (topModule.SuperprojectModule != null)
            {
                topModule = topModule.SuperprojectModule;
            }

            return topModule;
        }

        private RepoDistSettings _effectiveSettings;

        [NotNull]
        public RepoDistSettings EffectiveSettings
        {
            get
            {
                if (_effectiveSettings == null)
                {
                    lock (_lock)
                    {
                        if (_effectiveSettings == null)
                        {
                            _effectiveSettings = RepoDistSettings.CreateEffective(this);
                        }
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

        [NotNull]
        public RepoDistSettings LocalSettings
        {
            get
            {
                if (_localSettings == null)
                {
                    lock (_lock)
                    {
                        if (_localSettings == null)
                        {
                            _localSettings = new RepoDistSettings(null, EffectiveSettings.SettingsCache, SettingLevel.Local);
                        }
                    }
                }

                return _localSettings;
            }
        }

        private ConfigFileSettings _effectiveConfigFile;

        [NotNull]
        public ConfigFileSettings EffectiveConfigFile
        {
            get
            {
                if (_effectiveConfigFile == null)
                {
                    lock (_lock)
                    {
                        if (_effectiveConfigFile == null)
                        {
                            _effectiveConfigFile = ConfigFileSettings.CreateEffective(this);
                        }
                    }
                }

                return _effectiveConfigFile;
            }
        }

        public ConfigFileSettings LocalConfigFile => new ConfigFileSettings(null, EffectiveConfigFile.SettingsCache, SettingLevel.Local);

        IConfigFileSettings IGitModule.LocalConfigFile => LocalConfigFile;

        // encoding for files paths
        private static Encoding _systemEncoding;

        [NotNull]
        public static Encoding SystemEncoding
        {
            get
            {
                if (_systemEncoding == null)
                {
                    _systemEncoding = new SystemEncodingReader().Read();
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
        [NotNull] public static readonly Encoding LosslessEncoding = Encoding.GetEncoding("ISO-8859-1"); // is any better?

        [NotNull]
        public Encoding FilesEncoding => EffectiveConfigFile.FilesEncoding ?? new UTF8Encoding(false);

        [NotNull]
        public Encoding CommitEncoding => EffectiveConfigFile.CommitEncoding ?? new UTF8Encoding(false);

        /// <summary>
        /// Encoding for commit header (message, notes, author, committer, emails)
        /// </summary>
        [NotNull]
        public Encoding LogOutputEncoding => EffectiveConfigFile.LogOutputEncoding ?? CommitEncoding;

        /// <summary>Indicates whether the <see cref="WorkingDir"/> contains a git repository.</summary>
        public bool IsValidGitWorkingDir()
        {
            return IsValidGitWorkingDir(WorkingDir);
        }

        /// <summary>Indicates whether the specified directory contains a git repository.</summary>
        public static bool IsValidGitWorkingDir([CanBeNull] string dir)
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
        /// See https://git-scm.com/docs/git-rev-parse#Documentation/git-rev-parse.txt---git-pathltpathgt
        /// </summary>
        /// <param name="relativePath">A path relative to the .git directory</param>
        public string ResolveGitInternalPath(string relativePath)
        {
            var args = new GitArgumentBuilder("rev-parse")
            {
                "--git-path",
                relativePath.Quote()
            };
            var gitPath = _gitExecutable.GetOutput(args);

            var systemPath = gitPath.Trim().ToNativePath();

            if (systemPath.StartsWith(".git\\"))
            {
                systemPath = Path.Combine(GetGitDirectory(), systemPath.Substring(".git\\".Length));
            }

            return systemPath;
        }

        private string _gitCommonDirectory;
        private readonly object _gitCommonLock = new object();

        /// <summary>
        /// Returns git common directory
        /// https://git-scm.com/docs/git-rev-parse#Documentation/git-rev-parse.txt---git-common-dir
        /// </summary>
        public string GitCommonDirectory
        {
            get
            {
                // Get a cache of the common dir
                // Lock needed as the command is called rapidly when creating the module
                if (_gitCommonDirectory != null)
                {
                    return _gitCommonDirectory;
                }

                lock (_gitCommonLock)
                {
                    if (_gitCommonDirectory == null)
                    {
                        var args = new GitArgumentBuilder("rev-parse") { "--git-common-dir" };
                        var result = _gitExecutable.Execute(args);

                        var dir = result.StandardOutput.Trim().ToNativePath();

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
            var args = new GitArgumentBuilder("submodule")
            {
                "status",
                submodulePath
            };
            var result = _gitExecutable.Execute(args);

            return result.ExitCode == 0 || IsSubmoduleRemoved();

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
            var localPaths = new List<string>();
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
                    .Select(section => section.GetValue("path").Trim());
            }
        }

        /// <summary>
        /// Searches from <paramref name="startDir"/> and up through the directory
        /// hierarchy for a valid git working directory. If found, the path is returned,
        /// otherwise <c>null</c>.
        /// </summary>
        [CanBeNull]
        public static string TryFindGitWorkingDir([CanBeNull] string startDir)
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

        public ExecutionResult Clean(CleanMode mode, bool dryRun = false, bool directories = false, string paths = null)
        {
            return _gitExecutable.Execute(
                GitCommandHelpers.CleanCmd(mode, dryRun, directories, paths));
        }

        public bool EditNotes(ObjectId commitId)
        {
            var arguments = new GitArgumentBuilder("notes") { "edit", commitId };
            var editor = GetEffectiveSetting("core.editor").ToLower();
            var createWindow = !editor.Contains("gitextensions") && !editor.Contains("notepad");

            return _gitExecutable.RunCommand(arguments, createWindow: createWindow);
        }

        public bool InTheMiddleOfConflictedMerge()
        {
            var args = new GitArgumentBuilder("ls-files")
            {
                "-z",
                "--unmerged"
            };
            return !string.IsNullOrEmpty(_gitExecutable.GetOutput(args));
        }

        public bool HandleConflictSelectSide(string fileName, string side)
        {
            Directory.SetCurrentDirectory(WorkingDir);
            var args = new GitArgumentBuilder("checkout-index")
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

            var args = new GitArgumentBuilder("checkout-index")
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
            var splitResult = output.Split(new[] { "\t", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
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
            using (var blobStream = GetFileStream(blob))
            {
                var blobData = blobStream.ToArray();
                if (EffectiveConfigFile.core.autocrlf.ValueOrDefault == AutoCRLFType.@true)
                {
                    if (!FileHelper.IsBinaryFileName(this, saveAs) && !FileHelper.IsBinaryFileAccordingToContent(blobData))
                    {
                        blobData = GitConvert.ConvertCrLfToWorktree(blobData);
                    }
                }

                using (var stream = File.Create(saveAs))
                {
                    stream.Write(blobData, 0, blobData.Length);
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

        public (string baseFile, string localFile, string remoteFile) CheckoutConflictedFiles(ConflictData unmergedData)
        {
            Directory.SetCurrentDirectory(WorkingDir);

            var baseFile = CheckoutPart(1, unmergedData.Filename + ".BASE", unmergedData.Base.Filename);
            var localFile = CheckoutPart(2, unmergedData.Filename + ".LOCAL", unmergedData.Local.Filename);
            var remoteFile = CheckoutPart(3, unmergedData.Filename + ".REMOTE", unmergedData.Remote.Filename);

            return (baseFile, localFile, remoteFile);

            string CheckoutPart(int part, string fileName, string unmerged)
            {
                if (unmerged != null)
                {
                    var args = new GitArgumentBuilder("checkout-index")
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

        public async Task<ConflictData> GetConflictAsync(string filename)
        {
            return (await GetConflictsAsync(filename)).SingleOrDefault();
        }

        public async Task<List<ConflictData>> GetConflictsAsync(string filename = "")
        {
            filename = filename.ToPosixPath();

            var list = new List<ConflictData>();
            var args = new GitArgumentBuilder("ls-files")
            {
                "-z",
                "--unmerged",
                { !string.IsNullOrWhiteSpace(filename), "--" },
                filename.QuoteNE()
            };

            var unmerged = (await _gitExecutable
                .GetOutputAsync(args)
                .ConfigureAwait(false))
                .Split(new[] { '\0', '\n' }, StringSplitOptions.RemoveEmptyEntries);

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

                    item[stage - 1] = new ConflictedFileData(ObjectId.Parse(hash), itemName);
                    prevItemName = itemName;
                }
            }

            if (prevItemName != null)
            {
                list.Add(new ConflictData(item[0], item[1], item[2]));
            }

            return list;
        }

        public async Task<Dictionary<IGitRef, IGitItem>> GetSubmoduleItemsForEachRefAsync(string filename, Func<IGitRef, bool> showRemoteRef, bool noLocks = false)
        {
            string command = GetSortedRefsCommand(noLocks: noLocks);

            if (command == null)
            {
                return new Dictionary<IGitRef, IGitItem>();
            }

            filename = filename.ToPosixPath();

            var refList = await _gitExecutable.GetOutputAsync(command).ConfigureAwait(false);

            var refs = ParseRefs(refList);

            return refs.Where(showRemoteRef).ToDictionary(r => r, r => GetSubmoduleCommitHash(filename, r.Name));
        }

        internal ArgumentString GetSortedRefsCommand(bool noLocks = false)
        {
            if (AppSettings.ShowSuperprojectRemoteBranches)
            {
                return new GitArgumentBuilder("for-each-ref", gitOptions:
                    noLocks && GitVersion.Current.SupportNoOptionalLocks
                        ? (ArgumentString)"--no-optional-locks"
                        : default)
                {
                    "--sort=-committerdate",
                    "--format=\"%(objectname) %(refname)\"",
                    "refs/"
                };
            }

            if (AppSettings.ShowSuperprojectBranches || AppSettings.ShowSuperprojectTags)
            {
                return new GitArgumentBuilder("for-each-ref", gitOptions:
                    noLocks && GitVersion.Current.SupportNoOptionalLocks
                        ? (ArgumentString)"--no-optional-locks"
                        : default)
                {
                    "--sort=-committerdate",
                    "--format=\"%(objectname) %(refname)\"",
                    { AppSettings.ShowSuperprojectBranches, "refs/heads/" },
                    { AppSettings.ShowSuperprojectTags, " refs/tags/" }
                };
            }

            return "";
        }

        [CanBeNull]
        private IGitItem GetSubmoduleCommitHash(string filename, string refName)
        {
            var args = new GitArgumentBuilder("ls-tree")
            {
                refName,
                { !string.IsNullOrWhiteSpace(filename), "--" },
                filename.QuoteNE()
            };
            var output = _gitExecutable.GetOutput(args);

            return _gitTreeParser.ParseSingle(output);
        }

        public int? GetCommitCount(string parentHash, string childHash)
        {
            var args = new GitArgumentBuilder("rev-list")
            {
                parentHash,
                $"^{childHash}",
                "--count"
            };
            var output = _gitExecutable.GetOutput(args);

            if (int.TryParse(output, out var commitCount))
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

        public void RunMergeTool([CanBeNull] string fileName = "", [CanBeNull] string customTool = null)
        {
            var gui = GitVersion.Current.SupportGuiMergeTool ? "--gui" : string.Empty;
            var args = new GitArgumentBuilder("mergetool")
            {
                { string.IsNullOrWhiteSpace(customTool), gui, $"--tool={customTool}" },
                { !string.IsNullOrWhiteSpace(fileName), "--" },
                fileName.ToPosixPath().QuoteNE()
            };
            using (var process = _gitExecutable.Start(args, createWindow: true))
            {
                process.WaitForExit();
            }
        }

        public string Init(bool bare, bool shared)
        {
            var args = new GitArgumentBuilder("init")
            {
                { bare, "--bare" },
                { shared, "--shared=all" }
            };
            var output = _gitExecutable.GetOutput(args);

            WorkingDirGitDir = GitDirectoryResolverInstance.Resolve(WorkingDir);
            return output;
        }

        public bool IsMerge(ObjectId objectId)
        {
            return GetParents(objectId).Count > 1;
        }

        public GitRevision GetRevision([CanBeNull] ObjectId objectId = null, bool shortFormat = false, bool loadRefs = false)
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
                /* Committer Date */ "%ct%n" +
                /* Encoding       */ "%e%n";

            var format = formatString + (shortFormat ? "%s" : "%B%nNotes:%n%-N");

            var args = new GitArgumentBuilder("log")
            {
                "-n1",
                $"--pretty=format:{format}",
                objectId
            };

            var revInfo = _gitExecutable.GetOutput(args, cache: GitCommandCache, outputEncoding: LosslessEncoding);

            // TODO improve parsing to reduce temporary string (see similar code in RevisionReader)
            string[] lines = revInfo.Split('\n');

            var revision = new GitRevision(ObjectId.Parse(lines[0]))
            {
                TreeGuid = ObjectId.Parse(lines[1]),
                ParentIds = lines[2].SplitBySpace().Select(line => ObjectId.Parse(line)).ToList(),
                Author = ReEncodeStringFromLossless(lines[3]),
                AuthorEmail = ReEncodeStringFromLossless(lines[4]),
                Committer = ReEncodeStringFromLossless(lines[6]),
                CommitterEmail = ReEncodeStringFromLossless(lines[7]),
                AuthorDate = DateTimeUtils.ParseUnixTime(lines[5]),
                CommitDate = DateTimeUtils.ParseUnixTime(lines[8]),
                MessageEncoding = lines[9]
            };

            revision.HasNotes = !shortFormat;
            if (shortFormat)
            {
                revision.Subject = ReEncodeCommitMessage(lines[10], revision.MessageEncoding);
            }
            else
            {
                string message = ProcessDiffNotes(10);

                // commit message is not re-encoded by git when format is given
                revision.Body = ReEncodeCommitMessage(message, revision.MessageEncoding);
                revision.Subject = revision.Body.Substring(0, revision.Body.IndexOfAny(new[] { '\r', '\n' }));
            }

            if (loadRefs)
            {
                revision.Refs = GetRefs()
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

                var message = new StringBuilder();
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

        public IReadOnlyList<ObjectId> GetParents(ObjectId commitId)
        {
            var args = new GitArgumentBuilder("log")
            {
                "-n 1",
                "--format=format:%P",
                commitId
            };
            return _gitExecutable
                .GetOutput(args)
                .SplitBySpace()
                .Select(line => ObjectId.Parse(line))
                .ToList();
        }

        public IReadOnlyList<GitRevision> GetParentRevisions(ObjectId commitId)
        {
            return GetParents(commitId)
                .Select(parent => GetRevision(parent, shortFormat: true))
                .ToList();
        }

        [CanBeNull]
        public string ShowObject(ObjectId objectId)
        {
            return ReEncodeShowString(_gitExecutable
                .GetOutput($"show {objectId}", cache: GitCommandCache, outputEncoding: LosslessEncoding));
        }

        public void DeleteTag(string tagName)
        {
            var args = new GitArgumentBuilder("tag")
            {
                "-d",
                tagName.QuoteNE()
            };
            _gitExecutable.RunCommand(args);
        }

        /// <summary>
        /// Gets the commit ID of the currently checked out commit.
        /// If the repo is bare, has no commits or is corrupt, <c>null</c> is returned.
        /// </summary>
        [CanBeNull]
        public ObjectId GetCurrentCheckout()
        {
            var args = new GitArgumentBuilder("rev-parse") { "HEAD" };
            var result = _gitExecutable.Execute(args);

            return result.ExitCode == 0 && ObjectId.TryParse(result.StandardOutput, offset: 0, out var objectId)
                ? objectId
                : null;
        }

        public bool TryResolvePartialCommitId(string objectIdPrefix, out ObjectId objectId)
        {
            // If the prefix is already a full SHA1 then return immediately without invoking a git process.
            if (ObjectId.TryParse(objectIdPrefix, out objectId))
            {
                return true;
            }

            var args = new GitArgumentBuilder("rev-parse")
            {
                "--verify",
                "--quiet",
                $"{objectIdPrefix}^{{commit}}"
            };
            var output = _gitExecutable.GetOutput(args).Trim();

            if (output.StartsWith(objectIdPrefix) && ObjectId.TryParse(output, out objectId))
            {
                return true;
            }

            objectId = default;
            return false;
        }

        public async Task<(char code, ObjectId currentCommitId)> GetSuperprojectCurrentCheckoutAsync()
        {
            if (SuperprojectModule == null)
            {
                return (' ', null);
            }

            var args = new GitArgumentBuilder("submodule")
            {
                "status",
                "--cached",
                SubmodulePath.Quote()
            };
            var output = await SuperprojectModule.GitExecutable.GetOutputAsync(args).ConfigureAwait(false);
            var lines = output.Split('\n');

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

        public bool ExistsMergeCommit(string startRev, string endRev)
        {
            if (string.IsNullOrEmpty(startRev) || string.IsNullOrEmpty(endRev))
            {
                return false;
            }

            var args = new GitArgumentBuilder($"rev-list")
            {
                "--parents",
                "--no-walk",
                $"{startRev}..{endRev}"
            };

            return _gitExecutable
                .GetOutputLines(args)
                .Any(IsTwoSha1Hashes);

            bool IsTwoSha1Hashes(string parents)
            {
                // TODO use Regex here to avoid allocations
                string[] tab = parents.Split(' ');
                return tab.Length > 2 && tab.All(parent => GitRevision.Sha1HashRegex.IsMatch(parent));
            }
        }

        public ConfigFile GetSubmoduleConfigFile()
            => new ConfigFile(WorkingDir + ".gitmodules", true);

        [CanBeNull]
        public string GetCurrentSubmoduleLocalPath()
        {
            if (SuperprojectModule == null)
            {
                return null;
            }

            Debug.Assert(WorkingDir.StartsWith(SuperprojectModule.WorkingDir), "Submodule working dir should start with super-project's working dir");

            return Path.GetDirectoryName(
                WorkingDir.Substring(SuperprojectModule.WorkingDir.Length)).ToPosixPath();
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
            ConfigFile configFile;
            try
            {
                configFile = GetSubmoduleConfigFile();
            }
            catch (GitConfigurationException)
            {
                // swallow any exceptions here, any config exceptions would have been shown to the user already
                configFile = null;
            }

            if (configFile == null)
            {
                yield return null;
            }

            var args = new GitArgumentBuilder("submodule") { "status" };
            var lines = _gitExecutable.GetOutputLines(args);

            string lastLine = null;

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

                var configSection = configFile.ConfigSections.FirstOrDefault(section => section.GetValue("path").Trim() == localPath);

                Trace.Assert(configSection != null, $"`git submodule status` returned submodule \"{localPath}\" that was not found in .gitmodules");

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
            var args = new GitArgumentBuilder("submodule")
            {
                "summary",
                submodule
            };
            return _gitExecutable.GetOutput(args);
        }

        public void Reset(ResetMode mode, string file = null)
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
            if (files == null || files.Count == 0)
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
                    "-M -C -B",
                    { start != null, $"--start-number {start}" },
                    $"{from.Quote()}..{to.Quote()}",
                    $"-o {output.ToPosixPath().Quote()}"
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
                revision = null;
            }

            // Run batch arguments to work around max command line length on Windows. Fix #6593
            // 3: double quotes + ' '
            // See https://referencesource.microsoft.com/#system/services/monitoring/system/diagnosticts/Process.cs,1952
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

            return _gitExecutable.GetOutput(
                new GitArgumentBuilder("rm")
                {
                    { force, "--force" },
                    "--",
                    files.Select(f => f.ToPosixPath().Quote())
                });
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
            var pageantExecutable = new Executable(AppSettings.Pageant);

            // ensure pageant is loaded, so we can wait for loading a key in the next command
            // otherwise we'll stuck there waiting until pageant exits
            if (!IsPageantRunning())
            {
                // NOTE we leave the process to dangle here
                var process = pageantExecutable.Start("");

                process.WaitForInputIdle();
            }

            pageantExecutable.RunCommand(sshKeyFile.Quote());

            bool IsPageantRunning()
            {
                var pageantProcName = Path.GetFileNameWithoutExtension(AppSettings.Pageant);
                return Process.GetProcessesByName(pageantProcName).Length != 0;
            }
        }

        public string GetPuttyKeyFileForRemote([CanBeNull] string remote)
        {
            if (string.IsNullOrEmpty(remote) ||
                string.IsNullOrEmpty(AppSettings.Pageant) ||
                !AppSettings.AutoStartPageant ||
                !GitCommandHelpers.Plink())
            {
                return "";
            }

            return GetSetting($"remote.{remote}.puttykeyfile");
        }

        public ArgumentString FetchCmd([CanBeNull] string remote, [CanBeNull] string remoteBranch, [CanBeNull] string localBranch, bool? fetchTags = false, bool isUnshallow = false, bool prune = false)
        {
            return new GitArgumentBuilder("fetch")
            {
                { GitVersion.Current.FetchCanAskForProgress, "--progress" },
                {
                    !string.IsNullOrEmpty(remote) || !string.IsNullOrEmpty(remoteBranch) || !string.IsNullOrEmpty(localBranch),
                    GetFetchArgs(remote, remoteBranch, localBranch, fetchTags, isUnshallow, prune)
                }
            };
        }

        public ArgumentString PullCmd(string remote, string remoteBranch, bool rebase, bool? fetchTags = false, bool isUnshallow = false, bool prune = false)
        {
            return new GitArgumentBuilder("pull")
            {
                { rebase, "--rebase" },
                { GitVersion.Current.FetchCanAskForProgress, "--progress" },
                GetFetchArgs(remote, remoteBranch, null, fetchTags, isUnshallow, prune && !rebase)
            };
        }

        private ArgumentString GetFetchArgs(string remote, string remoteBranch, string localBranch, bool? fetchTags, bool isUnshallow, bool prune)
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
                remote.ToPosixPath().Trim().Quote(),
                branchArguments,
                { fetchTags == true, "--tags" },
                { fetchTags == false, "--no-tags" },
                { isUnshallow, "--unshallow" },
                { prune, "--prune" }
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
                { GitVersion.Current.PushCanAskForProgress, "--progress" },
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
        public ArgumentString PushCmd([NotNull] string remote, [NotNull] string fromBranch, [CanBeNull] string toBranch, ForcePushOptions force, bool track, int recursiveSubmodules)
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
            return new GitArgumentBuilder("push")
            {
                force,
                { track, "-u" },
                { recursiveSubmodules == 1, "--recurse-submodules=check" },
                { recursiveSubmodules == 2, "--recurse-submodules=on-demand" },
                { GitVersion.Current.PushCanAskForProgress, "--progress" },
                remote.ToPosixPath().Trim().Quote(),
                { string.IsNullOrEmpty(toBranch), fromBranch },
                { !string.IsNullOrEmpty(toBranch), $"{fromBranch}:{toBranch}" }
            };
        }

        public string ApplyPatch(string dir, ArgumentString amCommand)
        {
            using (var process = _gitExecutable.Start(amCommand, createWindow: false, redirectInput: true, redirectOutput: true, SystemEncoding))
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

        #region Stage/Unstage

        public string AssumeUnchangedFiles(IReadOnlyList<GitItemStatus> files, bool assumeUnchanged, out bool wereErrors)
        {
            files = files.Where(file => file.IsAssumeUnchanged != assumeUnchanged).ToList();

            if (files.Count == 0)
            {
                wereErrors = false;
                return "";
            }

            var execution = _gitExecutable.Execute(
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
                SystemEncoding);

            wereErrors = !execution.ExitedSuccessfully;
            return execution.AllOutput;
        }

        public string SkipWorktreeFiles(IReadOnlyList<GitItemStatus> files, bool skipWorktree)
        {
            files = files.Where(file => file.IsSkipWorktree != skipWorktree).ToList();

            if (files.Count == 0)
            {
                return "";
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

            return execution.AllOutput;
        }

        public string StageFiles(IReadOnlyList<GitItemStatus> files, out bool wereErrors)
        {
            wereErrors = false;

            if (files.Count == 0)
            {
                return "";
            }

            var output = new StringBuilder();
            var nonDeletedFiles = files.Where(file => !file.IsDeleted).ToList();
            var deletedFiles = files.Where(file => file.IsDeleted).ToList();

            if (nonDeletedFiles.Count != 0)
            {
                var execution = _gitExecutable.Execute(
                    UpdateIndexCmd(AppSettings.ShowErrorsWhenStagingFiles),
                    inputWriter =>
                    {
                        foreach (var file in nonDeletedFiles)
                        {
                            UpdateIndex(inputWriter, file.Name);
                        }
                    },
                    SystemEncoding);

                wereErrors |= !execution.ExitedSuccessfully;
                output.AppendLine(execution.AllOutput);
            }

            if (deletedFiles.Count != 0)
            {
                var execution = _gitExecutable.Execute(
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
                    SystemEncoding);

                wereErrors |= !execution.ExitedSuccessfully;
                output.Append(execution.AllOutput);
            }

            return output.ToString();
        }

        [MustUseReturnValue]
        public bool StageFile(string file)
        {
            return _gitExecutable.RunCommand(
                new GitArgumentBuilder("update-index")
                {
                    "--add",
                    file.ToPosixPath().Quote()
                });
        }

        public string UnstageFiles(IReadOnlyList<GitItemStatus> files)
        {
            if (files.Count == 0)
            {
                return "";
            }

            var output = new StringBuilder();
            var nonNewFiles = files.Where(file => !file.IsDeleted).ToList();
            var newFiles = files.Where(file => file.IsDeleted).ToList();

            if (nonNewFiles.Count != 0)
            {
                var execution = _gitExecutable.Execute(
                    new GitArgumentBuilder("update-index")
                    {
                        "--info-only",
                        "--index-info"
                    },
                    inputWriter =>
                    {
                        foreach (var file in nonNewFiles)
                        {
                            inputWriter.WriteLine($"0 0000000000000000000000000000000000000000\t\"{EscapeOctalCodePoints(file.Name.ToPosixPath())}\"");
                        }
                    },
                    SystemEncoding);

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

            return output.ToString();
        }

        /// <summary>
        /// Batch unstage files using <see cref="ExecutableExtensions.RunBatchCommand(IExecutable, ICollection{BatchArgumentItem}, Action{BatchProgressEventArgs}, byte[], bool)"/>
        /// </summary>
        /// <param name="selectedItems">Selected file items</param>
        /// <param name="action">Progress update callback</param>
        /// <returns><see langword="true" /> if changes should be rescanned; otherwise <see langword="false" /></returns>.
        public bool BatchUnstageFiles(IEnumerable<GitItemStatus> selectedItems, Action<BatchProgressEventArgs> action = null)
        {
            var files = new List<GitItemStatus>();
            var filesToRemove = new List<string>();
            var shouldRescanChanges = false;
            foreach (var item in selectedItems)
            {
                if (!item.IsNew)
                {
                    filesToRemove.Add(item.Name);

                    if (item.IsRenamed)
                    {
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

            UnstageFiles(files);

            return shouldRescanChanges;
        }

        public async Task<bool> AddInteractiveAsync(GitItemStatus file)
        {
            var args = new GitArgumentBuilder("add")
            {
                "-p",
                file.Name.Quote()
            };

            using (var process = _gitExecutable.Start(args, createWindow: true))
            {
                return await process.WaitForExitAsync() == 0;
            }
        }

        public async Task<bool> ResetInteractiveAsync(GitItemStatus file)
        {
            var args = new GitArgumentBuilder("checkout")
            {
                "-p",
                file.Name.Quote()
            };

            using (var process = _gitExecutable.Start(args, createWindow: true))
            {
                return await process.WaitForExitAsync() == 0;
            }
        }

        private static void UpdateIndex(StreamWriter inputWriter, string filename)
        {
            var bytes = EncodingHelper.ConvertTo(
                SystemEncoding,
                $"\"{filename.ToPosixPath()}\"{inputWriter.NewLine}");

            inputWriter.BaseStream.Write(bytes, 0, bytes.Length);
        }

        private GitArgumentBuilder UpdateIndexCmd(bool showErrorsWhenStagingFiles)
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

                var patchFile =
                    new PatchFile
                    {
                        Name = file,
                        FullName = fullFileName,
                        IsApplied = n < next,
                        IsNext = n == next
                    };

                if (File.Exists(rebaseDir + file))
                {
                    string key = null;
                    string value = "";
                    foreach (var line in File.ReadLines(rebaseDir + file))
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

        public ArgumentString CommitCmd(bool amend, bool signOff = false, string author = "", bool useExplicitCommitMessage = true, bool noVerify = false, bool gpgSign = false, string gpgKeyId = "")
        {
            return new GitArgumentBuilder("commit")
            {
                { amend, "--amend" },
                { noVerify, "--no-verify" },
                { signOff, "--signoff" },
                { !string.IsNullOrEmpty(author), $"--author=\"{author?.Trim().Trim('"')}\"" },
                { gpgSign && string.IsNullOrWhiteSpace(gpgKeyId), "-S" },
                { gpgSign && !string.IsNullOrWhiteSpace(gpgKeyId), $"-S{gpgKeyId}" },
                { useExplicitCommitMessage, $"-F \"{Path.Combine(GetGitDirectory(), "COMMITMESSAGE")}\"" }
            };
        }

        public string RemoveRemote(string remoteName)
        {
            var args = new GitArgumentBuilder("remote")
            {
                "rm",
                remoteName.QuoteNE()
            };
            return _gitExecutable.GetOutput(args);
        }

        public string RenameRemote(string remoteName, string newName)
        {
            var args = new GitArgumentBuilder("remote")
            {
                "rename",
                remoteName.QuoteNE(),
                newName.QuoteNE()
            };
            return _gitExecutable.GetOutput(args);
        }

        public string RenameBranch(string name, string newName)
        {
            var args = new GitArgumentBuilder("branch")
            {
                "-m",
                name.QuoteNE(),
                newName.QuoteNE()
            };
            return _gitExecutable.GetOutput(args);
        }

        public string AddRemote([CanBeNull] string name, string path)
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
                    path?.ToPosixPath().QuoteNE()
                });
        }

        public IReadOnlyList<string> GetRemoteNames()
        {
            return _gitExecutable
                .GetOutputLines("remote")
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToList();
        }

        private static readonly Regex _remoteVerboseLineRegex = new Regex(@"^(?<name>[^	]+)\t(?<url>.+?) \((?<direction>fetch|push)\)$", RegexOptions.Compiled);

        public async Task<IReadOnlyList<Remote>> GetRemotesAsync()
        {
            return ParseRemotes(await _gitExecutable.GetOutputLinesAsync("remote -v"));

            IReadOnlyList<Remote> ParseRemotes(IEnumerable<string> lines)
            {
                var remotes = new List<Remote>();

                // See tests for explanation of the format

                using (var enumerator = lines.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var remoteLine = enumerator.Current;
                        if (remoteLine.IndexOf("not a git repository", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            // An invalid module is not an error; we simply return an empty list of remotes
                            return remotes;
                        }

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
                        if (name != pushMatch.Groups["name"].Value)
                        {
                            throw new Exception("Fetch and push remote names must match: " +
                                remoteLine + ", " + pushLine);
                        }

                        remotes.Add(new Remote(name, remoteUrl, pushUrl));
                    }
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

        public void SetSetting(string setting, string value)
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
            return new GitArgumentBuilder("stash", gitOptions:
                    noLocks && GitVersion.Current.SupportNoOptionalLocks
                        ? (ArgumentString)"--no-optional-locks"
                        : default)
                { "list" };
        }

        public IReadOnlyList<GitStash> GetStashes(bool noLocks = false)
        {
            var args = GetStashesCmd(noLocks);
            var lines = _gitExecutable.GetOutput(args).Split('\n');

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
            [CanBeNull] ObjectId firstId, [CanBeNull] ObjectId secondId,
            [CanBeNull] string fileName, [CanBeNull] string oldFileName,
            [NotNull] string extraDiffArguments, [NotNull] Encoding encoding,
            bool cacheResult, bool isTracked = true)
        {
            // fix refs slashes
            fileName = fileName?.ToPosixPath();
            oldFileName = oldFileName?.ToPosixPath();
            string firstRevision = firstId?.ToString()?.ToPosixPath();
            string secondRevision = secondId?.ToString()?.ToPosixPath();

            string diffOptions = _revisionDiffProvider.Get(firstRevision, secondRevision, fileName, oldFileName, isTracked);

            var args = new GitArgumentBuilder("diff")
            {
                "--no-color",
                extraDiffArguments,
                { AppSettings.UseHistogramDiffAlgorithm, "--histogram" },
                "-M -C",
                diffOptions
            };

            var cache = cacheResult &&
                        !string.IsNullOrEmpty(secondRevision) &&
                        !string.IsNullOrEmpty(firstRevision) &&
                        !secondRevision.IsArtificial() &&
                        !firstRevision.IsArtificial()
                ? GitCommandCache
                : null;

            var patch = _gitExecutable.GetOutput(
                args,
                cache: cache,
                outputEncoding: LosslessEncoding);

            var patches = PatchProcessor.CreatePatchesFromString(patch, new Lazy<Encoding>(() => encoding)).ToList();

            return GetPatch(patches, fileName, oldFileName);
        }

        [CanBeNull]
        private static Patch GetPatch([NotNull, ItemNotNull] IReadOnlyList<Patch> patches, [CanBeNull] string fileName, [CanBeNull] string oldFileName)
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

        public string GetDiffFiles(string firstRevision, string secondRevision, bool noCache = false, bool nullSeparated = false)
        {
            noCache = noCache || firstRevision.IsArtificial() || secondRevision.IsArtificial();

            return _gitExecutable.GetOutput(
                new GitArgumentBuilder("diff")
                {
                    "--no-color",
                    "-M -C",
                    "--name-status",
                    { nullSeparated, "-z" },
                    _revisionDiffProvider.Get(firstRevision, secondRevision)
                },
                cache: noCache ? null : GitCommandCache);
        }

        public IReadOnlyList<GitItemStatus> GetDiffFilesWithSubmodulesStatus(ObjectId firstId, ObjectId secondId, ObjectId parentToSecond)
        {
            var stagedStatus = GitCommandHelpers.GetStagedStatus(firstId, secondId, parentToSecond);
            var status = GetDiffFilesWithUntracked(firstId?.ToString(), secondId?.ToString(), stagedStatus);
            GetSubmoduleStatus(status, firstId, secondId);
            return status;
        }

        public IReadOnlyList<GitItemStatus> GetDiffFilesWithUntracked(string firstRevision, string secondRevision, StagedStatus stagedStatus, bool noCache = false)
        {
            var output = GetDiffFiles(firstRevision, secondRevision, noCache: noCache, nullSeparated: true);
            var result = GitCommandHelpers.GetDiffChangedFilesFromString(this, output, stagedStatus).ToList();

            if (IsGitErrorMessage(output))
            {
                // No simple way to pass the error message, create fake file
                result.Add(createErrorGitItemStatus(output));
            }

            if (firstRevision == GitRevision.WorkTreeGuid || secondRevision == GitRevision.WorkTreeGuid)
            {
                // For worktree the untracked must be added too
                // Note that this may add a second GitError, this is a separate Git command
                var files = GetAllChangedFilesWithSubmodulesStatus().Where(x =>
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

            // shows untracked files
            var args = new GitArgumentBuilder("log")
            {
                $"{stashName}^3",
                "--pretty=format:\"%T\"",
                "--max-count=1"
            };
            var untrackedTreeHash = _gitExecutable.GetOutput(args);

            if (ObjectId.TryParse(untrackedTreeHash, out var treeId))
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
                .Select(file => new GitItemStatus
                {
                    IsTracked = true,
                    IsNew = true,
                    IsChanged = false,
                    IsDeleted = false,
                    Name = file.Name,
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
            UntrackedFilesMode untrackedFiles = UntrackedFilesMode.Default)
        {
            var output = _gitExecutable.GetOutput(
                GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles, untrackedFiles));

            var result = GitCommandHelpers.GetStatusChangedFilesFromString(this, output).ToList();
            if (IsGitErrorMessage(output))
            {
                // No simple way to pass the error message, create fake file
                result.Add(createErrorGitItemStatus(output));
            }

            if (!excludeAssumeUnchangedFiles || !excludeSkipWorktreeFiles)
            {
                var args = new GitArgumentBuilder("ls-files") { "-v" };
                string lsOutput = _gitExecutable.GetOutput(args);

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
                        var submoduleStatus = await GitCommandHelpers.GetCurrentSubmoduleChangesAsync(this, localItem.Name, localItem.OldName, localItem.Staged == StagedStatus.Index)
                        .ConfigureAwait(false);
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

        private void GetSubmoduleStatus(IReadOnlyList<GitItemStatus> status, ObjectId firstId, ObjectId secondId)
        {
            foreach (var item in status.Where(i => i.IsSubmodule))
            {
                item.SetSubmoduleStatus(
                    ThreadHelper.JoinableTaskFactory.RunAsync(
                        async () =>
                        {
                            await TaskScheduler.Default.SwitchTo(alwaysYield: true);

                            Patch patch = GetSingleDiff(firstId, secondId, item.Name, item.OldName, "", SystemEncoding, true);
                            string text = patch != null ? patch.Text : "";
                            var submoduleStatus = GitCommandHelpers.ParseSubmoduleStatus(text, this, item.Name);
                            if (submoduleStatus != null && submoduleStatus.Commit != submoduleStatus.OldCommit)
                            {
                                var submodule = submoduleStatus.GetSubmodule(this);
                                submoduleStatus.CheckSubmoduleStatus(submodule);
                            }

                            return submoduleStatus;
                        }));
            }
        }

        public IReadOnlyList<GitItemStatus> GetIndexFiles()
        {
            var output = _gitExecutable.GetOutput(
                new GitArgumentBuilder("diff")
                {
                    "--no-color",
                    "-M",
                    "-C",
                    "-z",
                    "--cached",
                    "--name-status"
                });

            if (output.Length < 50 && output.Contains("fatal: No HEAD commit to compare"))
            {
                // This command is a little more expensive because it will return both staged and unstaged files
                var command = GitCommandHelpers.GetAllChangedFilesCmd(excludeIgnoredFiles: true, UntrackedFilesMode.No);

                output = _gitExecutable.GetOutput(command);

                var res = GitCommandHelpers.GetStatusChangedFilesFromString(this, output)
                    .Where(item => (item.Staged == StagedStatus.Index || item.IsStatusOnly))
                    .ToList();
                if (IsGitErrorMessage(output))
                {
                    // No simple way to pass the error message, create fake file
                    res.Add(createErrorGitItemStatus(output));
                }

                return res;
            }

            var result = GitCommandHelpers.GetDiffChangedFilesFromString(this, output, StagedStatus.Index).ToList();
            if (IsGitErrorMessage(output))
            {
                // No simple way to pass the error message, create fake file
                result.Add(createErrorGitItemStatus(output));
            }

            return result;
        }

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
            var args = GitCommandHelpers.GetAllChangedFilesCmd(true, untrackedFilesMode, ignoreSubmodulesMode);
            var output = _gitExecutable.GetOutput(args);
            var result = GitCommandHelpers.GetStatusChangedFilesFromString(this, output).ToList();
            if (IsGitErrorMessage(output))
            {
                // No simple way to pass the error message, create fake file
                result.Add(createErrorGitItemStatus(output));
            }

            return result;
        }

        /// <summary>Indicates whether there are any changes to the repository,
        ///  including any untracked files or directories; excluding submodules.</summary>
        public bool IsDirtyDir()
        {
            return GitStatus(UntrackedFilesMode.All, IgnoreSubmodulesMode.All).Count > 0;
        }

        internal GitArgumentBuilder GetCurrentChangesCmd(string fileName, [CanBeNull] string oldFileName, bool staged,
            string extraDiffArguments, bool noLocks)
        {
            return new GitArgumentBuilder("diff", gitOptions:
                    noLocks && GitVersion.Current.SupportNoOptionalLocks
                        ? (ArgumentString)"--no-optional-locks"
                        : default)
                {
                    "--no-color",
                    { staged, "-M -C --cached" },
                    extraDiffArguments,
                    { AppSettings.UseHistogramDiffAlgorithm, "--histogram" },
                    "--",
                    fileName.ToPosixPath().Quote(),
                    { staged, oldFileName?.ToPosixPath().Quote() }
                };
        }

        [CanBeNull]
        public async Task<Patch> GetCurrentChangesAsync(string fileName, [CanBeNull] string oldFileName, bool staged, string extraDiffArguments, Encoding encoding = null, bool noLocks = false)
        {
            var output = await _gitExecutable.GetOutputAsync(GetCurrentChangesCmd(fileName, oldFileName, staged, extraDiffArguments, noLocks),
                outputEncoding: LosslessEncoding).ConfigureAwait(false);

            IReadOnlyList<Patch> patches = PatchProcessor.CreatePatchesFromString(output, new Lazy<Encoding>(() => encoding ?? FilesEncoding)).ToList();

            return GetPatch(patches, fileName, oldFileName);
        }

        [CanBeNull]
        private async Task<string> GetFileContentsAsync(string path)
        {
            var args = new GitArgumentBuilder("show") { $"HEAD:{path.ToPosixPath().Quote()}" };
            var result = await _gitExecutable.ExecuteAsync(args).ConfigureAwaitRunInline();

            return result.ExitCode == 0
                ? result.StandardOutput
                : null;
        }

        [CanBeNull]
        public async Task<string> GetFileContentsAsync(GitItemStatus file)
        {
            var contents = new StringBuilder();

            string currentContents = await GetFileContentsAsync(file.Name).ConfigureAwaitRunInline();
            if (currentContents != null)
            {
                contents.Append(currentContents);
            }

            if (file.OldName != null)
            {
                string oldContents = await GetFileContentsAsync(file.OldName).ConfigureAwaitRunInline();
                if (oldContents != null)
                {
                    contents.Append(oldContents);
                }
            }

            return contents.Length > 0 ? contents.ToString() : null;
        }

        public void UnstageFile(string file)
        {
            var args = new GitArgumentBuilder("rm")
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
        public static string GetSelectedBranchFast([CanBeNull] string repositoryPath, bool setDefaultIfEmpty = true)
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
        /// Gets the current branch
        /// </summary>
        /// <param name="setDefaultIfEmpty">Return "(no branch)" if detached</param>
        /// <returns>Current branchname</returns>
        public string GetSelectedBranch(bool setDefaultIfEmpty)
        {
            string head = GetSelectedBranchFast(WorkingDir, setDefaultIfEmpty);

            if (!string.IsNullOrEmpty(head))
            {
                return head;
            }

            var args = new GitArgumentBuilder("symbolic-ref") { "HEAD" };
            var result = _gitExecutable.Execute(args);

            return result.ExitCode == 0
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

            var executionResult = !tags && !branches
                ? new ExecutionResult() // TODO is this an error?
                : _gitExecutable.Execute(new GitArgumentBuilder("ls-remote")
                    {
                        { tags, "--tags" },
                        { branches, "--heads" },
                        remote.ToPosixPath().QuoteNE()
                    });

            var output = executionResult.AllOutput;

            // If the authentication failed because of a missing key, ask the user to supply one.
            if (output.Contains("FATAL ERROR") && output.Contains("authentication"))
            {
                result.AuthenticationFail = true;
            }
            else if (output.Contains("the server's host key is not cached in the registry", StringComparison.InvariantCultureIgnoreCase))
            {
                result.HostKeyFail = true;
            }
            else if (executionResult.ExitedSuccessfully)
            {
                result.Result = ParseRefs(output);
            }

            return result;
        }

        public IReadOnlyList<IGitRef> GetRefs(bool tags = true, bool branches = true)
        {
            return GetRefs(tags, branches, false);
        }

        public IReadOnlyList<IGitRef> GetRefs(bool tags, bool branches, bool noLocks)
        {
            var refList = _gitExecutable.GetOutput(GetRefsCmd(tags: tags, branches: branches, noLocks: noLocks));

            return ParseRefs(refList);
        }

        internal GitArgumentBuilder GetRefsCmd(bool tags, bool branches, bool noLocks)
        {
            GitArgumentBuilder cmd;

            if (tags)
            {
                cmd = new GitArgumentBuilder("show-ref", gitOptions:
                    noLocks && GitVersion.Current.SupportNoOptionalLocks
                        ? (ArgumentString)"--no-optional-locks"
                        : default)
                {
                    { branches, "--dereference", "--tags" },
                };
            }
            else if (branches)
            {
                // branches only
                cmd = new GitArgumentBuilder("for-each-ref", gitOptions:
                    noLocks && GitVersion.Current.SupportNoOptionalLocks
                        ? (ArgumentString)"--no-optional-locks"
                        : default)
                {
                    "--sort=-committerdate",
                    @"refs/heads/",
                    @"--format=""%(objectname) %(refname)"""
                };
            }
            else
            {
                throw new ArgumentException("GetRefs: Neither branches nor tags requested");
            }

            return cmd;
        }

        /// <param name="option">Order by date is slower.</param>
        public IReadOnlyList<IGitRef> GetTagRefs(GetTagRefsSortOrder option)
        {
            var list = GetRefs(true, false);

            switch (option)
            {
                case GetTagRefsSortOrder.ByCommitDateAscending:
                    return list.OrderBy(GetDate).ToList();
                case GetTagRefsSortOrder.ByCommitDateDescending:
                    return list.OrderByDescending(GetDate).ToList();
                default:
                    return list;
            }

            // BUG this sorting logic has no effect as CommitDate is not set by the GitRevision constructor
            DateTime GetDate(IGitRef head) => new GitRevision(head.ObjectId).CommitDate;
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

        public async Task<string[]> GetMergedBranchesAsync(bool includeRemote = false, bool fullRefname = false, string commit = null)
            => (await _gitExecutable
                .GetOutputAsync(GitCommandHelpers.MergedBranchesCmd(includeRemote, fullRefname, commit))
                .ConfigureAwait(false))
                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        public IReadOnlyList<string> GetMergedBranches(bool includeRemote = false)
        {
            return _gitExecutable
                .GetOutput(GitCommandHelpers.MergedBranchesCmd(includeRemote))
                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public IReadOnlyList<string> GetMergedRemoteBranches()
        {
            const string remoteBranchPrefixForMergedBranches = "remotes/";
            const string refsPrefix = "refs/";

            var remotes = GetRemoteNames();

            return _gitExecutable
                .GetOutputLines(GitCommandHelpers.MergedBranchesCmd(includeRemote: true))
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
                var objectId = ObjectId.Parse(refList, match.Groups["objectid"]);
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
        /// (as returned by git branch -a)
        /// </summary>
        /// <param name="objectId">The sha1.</param>
        /// <param name="getLocal">Pass true to include local branches</param>
        /// <param name="getRemote">Pass true to include remote branches</param>
        public IReadOnlyList<string> GetAllBranchesWhichContainGivenCommit(ObjectId objectId, bool getLocal, bool getRemote)
        {
            if (!getLocal && !getRemote)
            {
                return Array.Empty<string>();
            }

            var output = _gitExecutable.GetOutput(
                new GitArgumentBuilder("branch")
                {
                    { getRemote && getLocal, "-a" },
                    { getRemote && !getLocal, "-r" },
                    "--contains",
                    objectId
                });

            if (IsGitErrorMessage(output))
            {
                return Array.Empty<string>();
            }

            var result = output.Split(new[] { '\r', '\n', '*', '+' }, StringSplitOptions.RemoveEmptyEntries);

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
            var output = _gitExecutable.GetOutput($"tag --contains {objectId}");

            if (IsGitErrorMessage(output))
            {
                return Array.Empty<string>();
            }

            return output.Split(new[] { '\r', '\n', '*', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Returns tag's message. If the lightweight tag is passed, corresponding commit message
        /// is returned.
        /// </summary>
        [CanBeNull]
        public string GetTagMessage(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return null;
            }

            tag = tag.Trim();

            var output = _gitExecutable.GetOutput($"tag -l -n10 {tag}");

            /*
             * $ git tag -l -n10 1.50
             * 1.50            Added close checkbox to process dialog
             *
             * $ git tag -l -n10 1.57
             * 1.57            Minor changes.
             *
             *     Packed with Git-1.6.1 to avoid a bug in Git-1.6.2
             *     When installing 64bit version, both 32bit and 64bit shell extensions will be registered
             *     Application settings are saved when closing settings dialog instead of when application exits
             *     Revert commit handles merge conflicts better
             *     Diff in browse dialog now shows the diff between revisions if 2 revisions are selected
             *     Bug solved: files in diff viewer are not shown correctly when 2 revisions are selected
             *     Format path dialog improved
             */

            if (IsGitErrorMessage(output))
            {
                return null;
            }

            if (!output.StartsWith(tag))
            {
                return null;
            }

            output = output.Substring(tag.Length).Trim();
            if (output.Length == 0)
            {
                return null;
            }

            return output;
        }

        /// <summary>
        /// Returns list of file names which would be ignored
        /// </summary>
        /// <param name="ignorePatterns">Patterns to ignore (.gitignore syntax)</param>
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
                    .Split(new[] { '\0', '\n' }, StringSplitOptions.RemoveEmptyEntries)
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
                .Split('\0', '\n');
        }

        public IEnumerable<IGitItem> GetTree(ObjectId commitId, bool full)
        {
            var args = new GitArgumentBuilder("ls-tree")
            {
                "-z",
                { full, "-r" },
                commitId
            };

            var tree = _gitExecutable.GetOutput(args, cache: GitCommandCache);

            return _gitTreeParser.Parse(tree);
        }

        public GitBlame Blame(string fileName, string from, Encoding encoding, string lines = null)
        {
            var args = new GitArgumentBuilder("blame")
            {
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

            var output = _gitExecutable.GetOutput(args, cache: GitCommandCache, outputEncoding: LosslessEncoding);

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

            var commitByObjectId = new Dictionary<ObjectId, GitBlameCommit>();
            var lines = new List<GitBlameLine>(capacity: 256);

            var headerRegex = new Regex(@"^(?<objectid>[0-9a-f]{40}) (?<origlinenum>\d+) (?<finallinenum>\d+)", RegexOptions.Compiled);

            bool hasCommitHeader;
            ObjectId objectId;
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
                        if (!commitByObjectId.ContainsKey(objectId))
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
                            var commitData = commitByObjectId[objectId];
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
                                    filename);
                            }
                        }
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

        public string GetFileText(ObjectId id, Encoding encoding)
        {
            var args = new GitArgumentBuilder("cat-file")
            {
                "blob",
                id.ToString().QuoteNE()
            };
            return _gitExecutable.GetOutput(
                args,
                cache: GitCommandCache,
                outputEncoding: encoding);
        }

        [CanBeNull]
        public ObjectId GetFileBlobHash(string fileName, ObjectId objectId)
        {
            if (objectId == ObjectId.WorkTreeId || objectId == ObjectId.CombinedDiffId)
            {
                throw new ArgumentException($"Tried to get blob for unsupported revision: {objectId} and file: {fileName}");
            }

            // TODO use regex for parsing
            if (objectId == ObjectId.IndexId)
            {
                var args = new GitArgumentBuilder("ls-files")
                {
                    "-s",
                    { !string.IsNullOrWhiteSpace(fileName), "--" },
                    fileName.QuoteNE()
                };

                // index
                var lines = _gitExecutable.GetOutput(args).Split(' ', '\t');

                if (lines.Length >= 2)
                {
                    return ObjectId.Parse(lines[1]);
                }
            }
            else
            {
                var args = new GitArgumentBuilder("ls-tree")
                {
                    "-r",
                    objectId,
                    { !string.IsNullOrWhiteSpace(fileName), "--" },
                    fileName.QuoteNE()
                };
                var lines = _gitExecutable.GetOutput(args).Split(' ', '\t');
                if (lines.Length >= 3)
                {
                    return ObjectId.Parse(lines[2]);
                }
            }

            return null;
        }

        [CanBeNull]
        public MemoryStream GetFileStream(string blob)
        {
            // TODO why return a stream here? should just return a byte[]

            try
            {
                var args = new GitArgumentBuilder("cat-file")
                {
                    "blob",
                    blob
                };
                using (var process = _gitCommandRunner.RunDetached(args, redirectOutput: true))
                {
                    var stream = new MemoryStream();
                    process.StandardOutput.BaseStream.CopyTo(stream);
                    process.WaitForExit();
                    stream.Position = 0;
                    return stream;
                }
            }
            catch (Win32Exception ex)
            {
                Trace.WriteLine(ex);
                return null;
            }
        }

        public IEnumerable<string> GetPreviousCommitMessages(int count, string revision = "HEAD", string authorPattern = "")
        {
            var args = new GitArgumentBuilder("log")
            {
                "-z",
                $"-n {count}",
                revision,
                "--pretty=format:%e%n%s%n%n%b",
                { !string.IsNullOrEmpty(authorPattern), string.Concat("--author=\"", authorPattern, "\"") }
            };

            var messages = _gitExecutable.GetOutput(
                args,
                outputEncoding: LosslessEncoding).Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);

            if (messages.Length == 0)
            {
                return new[] { string.Empty };
            }

            return messages.Select(cm =>
                {
                    int idx = cm.IndexOf('\n');
                    string encodingName = cm.Substring(0, idx);
                    cm = cm.Substring(idx + 1, cm.Length - idx - 1);
                    cm = ReEncodeCommitMessage(cm, encodingName);
                    return cm;
                });
        }

        /// <summary>
        /// Get a list of diff/merge tools known by Git
        /// </summary>
        /// <param name="isDiff">diff or merge</param>
        /// <returns>the list</returns>
        public async Task<List<string>> GetCustomDiffMergeToolsAsync(bool isDiff)
        {
            // Note that --gui has no effect here
            var args = new GitArgumentBuilder(isDiff ? "difftool" : "mergetool") { "--tool-help" };
            string output = await _gitExecutable.GetOutputAsync(args);
            return GitCommandHelpers.ParseCustomDiffMergeTool(output);
        }

        public string OpenWithDifftoolDirDiff(string firstRevision, string secondRevision)
        {
            return OpenWithDifftool(null, firstRevision: firstRevision, secondRevision: secondRevision, extraDiffArguments: "--dir-diff");
        }

        public string OpenWithDifftool(string filename, string oldFileName = "", string firstRevision = GitRevision.IndexGuid, string secondRevision = GitRevision.WorkTreeGuid, string extraDiffArguments = null, bool isTracked = true, string customTool = null)
        {
            _gitCommandRunner.RunDetached(new GitArgumentBuilder("difftool")
            {
                { string.IsNullOrWhiteSpace(customTool), "--gui", $"--tool={customTool}" },
                "--no-prompt",
                "-M -C",
                extraDiffArguments,
                _revisionDiffProvider.Get(firstRevision, secondRevision, filename, oldFileName, isTracked)
            });

            // This method is supposed to return an error message, but the detached process is untracked
            // TODO track the process somehow, so errors can be reported
            return "";
        }

        /// <summary>
        /// Compare two Git commitish; blob or rev:path
        /// </summary>
        /// <param name="firstGitCommit">commitish</param>
        /// <param name="secondGitCommit">commitish</param>
        /// <returns>empty string</returns>
        public string OpenFilesWithDifftool(string firstGitCommit, string secondGitCommit)
        {
            if (string.IsNullOrWhiteSpace(firstGitCommit) || string.IsNullOrWhiteSpace(secondGitCommit))
            {
                return null;
            }

            _gitCommandRunner.RunDetached(new GitArgumentBuilder("difftool")
            {
                "--gui",
                "--no-prompt",
                "-M -C",
                firstGitCommit.QuoteNE(),
                secondGitCommit.QuoteNE()
            });

            return "";
        }

        [CanBeNull]
        public ObjectId RevParse(string revisionExpression)
        {
            if (string.IsNullOrWhiteSpace(revisionExpression) || revisionExpression.Length > 260)
            {
                return null;
            }

            if (ObjectId.TryParse(revisionExpression, out var objectId))
            {
                return objectId;
            }

            var args = new GitArgumentBuilder("rev-parse") { $"\"{revisionExpression}~0\"" };
            var result = _gitExecutable.Execute(args);

            return result.ExitCode == 0 && ObjectId.TryParse(result.StandardOutput, offset: 0, out objectId)
                ? objectId
                : null;
        }

        [CanBeNull]
        public ObjectId GetMergeBase(ObjectId a, ObjectId b)
        {
            var args = new GitArgumentBuilder("merge-base")
            {
                a,
                b
            };
            var output = _gitExecutable.GetOutput(args);

            return ObjectId.TryParse(output, offset: 0, out var objectId)
                ? objectId
                : null;
        }

        public SubmoduleStatus CheckSubmoduleStatus([CanBeNull] ObjectId commit, [CanBeNull] ObjectId oldCommit, CommitData data, CommitData oldData, bool loadData = false)
        {
            // Submodule directory must exist to run commands, unknown otherwise
            if (!IsValidGitWorkingDir() || oldCommit == null)
            {
                return SubmoduleStatus.NewSubmodule;
            }

            if (commit == null)
            {
                // Actually removed submodule, no special status for this uncommon status
                return SubmoduleStatus.Unknown;
            }

            if (commit == oldCommit)
            {
                return SubmoduleStatus.Unknown;
            }

            ObjectId baseOid = GetMergeBase(commit, oldCommit);
            if (baseOid == null)
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
                oldData = _commitDataManager.GetCommitData(oldCommit.ToString(), out _);
            }

            if (oldData == null)
            {
                return SubmoduleStatus.NewSubmodule;
            }

            if (loadData)
            {
                data = _commitDataManager.GetCommitData(commit.ToString(), out _);
            }

            if (data == null)
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

        public SubmoduleStatus CheckSubmoduleStatus([CanBeNull] ObjectId commit, [CanBeNull] ObjectId oldCommit)
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

            if (string.IsNullOrWhiteSpace(branchName))
            {
                return false;
            }

            branchName = branchName.Replace("\"", "\\\"");
            var args = new GitArgumentBuilder("check-ref-format")
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
        [CanBeNull]
        private string FormatBranchName([NotNull] string branchName)
        {
            if (branchName == null)
            {
                throw new ArgumentNullException(nameof(branchName));
            }

            string fullBranchName = GitRefName.GetFullBranchName(branchName);

            if (RevParse(fullBranchName) == null)
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
            var lines = new Executable(cmd).GetOutput("x").Split('\n');

            if (lines.Length <= 2)
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

        /// <summary>
        /// Escapes a UTF8 string <paramref name="s"/> into octal code points.
        /// </summary>
        /// <remarks>
        /// If <paramref name="s"/> is <c>null</c> then an empty string is returned.
        /// </remarks>
        /// <example>
        /// <code>EscapeOctalCodePoints("두다") == @"\353\221\220\353\213\244"</code>
        /// </example>
        /// <param name="s">The string to escape.</param>
        /// <returns>The escaped string, or <c>""</c> if <paramref name="s"/> is <c>null</c>.</returns>
        public static string EscapeOctalCodePoints([CanBeNull] string s)
        {
            if (s == null)
            {
                return null;
            }

            var resultBuilder = new StringBuilder(s.Length);

            for (int i = 0; i < s.Length; i++)
            {
                var charSubstring = s.Substring(i, 1);
                var charBytes = Encoding.UTF8.GetBytes(charSubstring);
                if (charBytes.Length == 1)
                {
                    resultBuilder.Append(charSubstring);
                }
                else
                {
                    foreach (var charByte in charBytes)
                    {
                        resultBuilder.AppendFormat(@"\{0}", Convert.ToString(charByte, toBase: 8));
                    }
                }
            }

            return resultBuilder.ToString();
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
        /// Re-encodes string from GitCommandHelpers.LosslessEncoding to toEncoding
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
        // characters could be replaced by replacement character while re-encoding to LogOutputEncoding
        [CanBeNull]
        public string ReEncodeCommitMessage(string s, [CanBeNull] string toEncodingName)
        {
            Encoding encoding;
            try
            {
                encoding = GetEncodingByGitName(toEncodingName);
            }
            catch
            {
                return s + "\n\n! Unsupported commit message encoding: " + toEncodingName + " !";
            }

            return ReEncodeStringFromLossless(s, encoding);
        }

        public Encoding GetEncodingByGitName(string encodingName)
        {
            bool isABug = !GitVersion.Current.LogFormatRecodesCommitMessage;

            if (isABug)
            {
                if (string.IsNullOrEmpty(encodingName))
                {
                    return Encoding.UTF8;
                }
                else if (encodingName.Equals(LosslessEncoding.HeaderName, StringComparison.InvariantCultureIgnoreCase))
                {
                    // no recoding is needed
                    return null;
                }
                else if (CpEncodingPattern.IsMatch(encodingName))
                {
                    // Encodings written as e.g. "cp1251", which is not a supported encoding string
                    return Encoding.GetEncoding(int.Parse(encodingName.Substring(2)));
                }
                else
                {
                    return Encoding.GetEncoding(encodingName);
                }
            }
            else
            {
                // bug is fixed in Git v1.8.4, Git recodes commit message to LogOutputEncoding
                return LogOutputEncoding;
            }
        }

        /// <summary>
        /// header part of show result is encoded in logoutputencoding (including re-encoded commit message)
        /// diff part is raw data in file's original encoding
        /// s should be encoded in LosslessEncoding
        /// </summary>
        [ContractAnnotation("s:null=>null")]
        [ContractAnnotation("s:notnull=>notnull")]
        public string ReEncodeShowString(string s)
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
            var args = new GitArgumentBuilder($"diff-tree")
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

            var files = fileList.Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);

            return files.Select(
                file => new GitItemStatus
                {
                    IsChanged = true,
                    IsTracked = true,
                    IsDeleted = false,
                    IsNew = false,
                    Name = file,
                    Staged = StagedStatus.None
                }).ToList();
        }

        [CanBeNull]
        public string GetCombinedDiffContent(ObjectId revisionOfMergeCommit, string filePath, string extraArgs, Encoding encoding)
        {
            var args = new GitArgumentBuilder("diff-tree")
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

        public bool HasLfsSupport()
        {
            var args = new GitArgumentBuilder("lfs") { "version" };
            return _gitExecutable.RunCommand(args);
        }

        public bool StopTrackingFile(string filename)
        {
            var args = new GitArgumentBuilder("rm")
            {
                "--cached",
                filename.ToPosixPath().Quote()
            };
            return _gitExecutable.Execute(args).ExitedSuccessfully;
        }

        [CanBeNull]
        public string GetDescribe(ObjectId commitId)
        {
            var args = new GitArgumentBuilder("describe")
            {
                "--tags",
                "--first-parent",
                "--abbrev=40",
                commitId
            };
            var output = _gitExecutable.GetOutput(args).TrimEnd();

            if (IsGitErrorMessage(output))
            {
                return null;
            }

            return output;
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

        private static GitItemStatus createErrorGitItemStatus(string gitOutput)
        {
            return new GitItemStatus { Name = GitError, IsStatusOnly = true, ErrorMessage = gitOutput.Replace('\0', '\t') };
        }

        public (int totalCount, Dictionary<string, int> countByName) GetCommitsByContributor(DateTime? since = null, DateTime? until = null)
        {
            var countByName = new Dictionary<string, int>();
            var totalCommits = 0;

            var regex = new Regex(@"^\s*(?<count>\d+)\s+(?<name>.*)$");
            var args = new GitArgumentBuilder("shortlog")
            {
                "--all",
                "-s",
                "-n",
                "--no-merges",
                GetDateParameter("--since", since),
                GetDateParameter("--until", until)
            };

            var lines = _gitExecutable.GetOutputLines(args);

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
                return date != null
                    ? $"{param}=\"{date:yyyy-MM-dd hh:mm:ss}\""
                    : "";
            }
        }

        internal TestAccessor GetTestAccessor() => new TestAccessor(this);

        internal readonly struct TestAccessor
        {
            private readonly GitModule _gitModule;

            public TestAccessor(GitModule gitModule)
            {
                _gitModule = gitModule;
            }

            public GitArgumentBuilder UpdateIndexCmd(bool showErrorsWhenStagingFiles) => _gitModule.UpdateIndexCmd(showErrorsWhenStagingFiles);
        }
    }
}
