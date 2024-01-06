using System.Diagnostics.CodeAnalysis;
using System.Text;
using GitExtUtils;

namespace GitUIPluginInterfaces
{
    /// <summary>Provides manipulation with git module.</summary>
    public interface IGitModule
    {
        void ClearGitCommandBetweenRefreshCache();
        IConfigFileSettings LocalConfigFile { get; }

        string AddRemote(string remoteName, string? path);
        IReadOnlyList<IGitRef> GetRefs(RefsFilter getRef);
        IEnumerable<string> GetSettings(string setting);
        IEnumerable<INamedGitItem> GetTree(ObjectId? commitId, bool full);

        /// <summary>
        /// Removes the registered remote by running <c>git remote rm</c> command.
        /// </summary>
        /// <param name="remoteName">The remote name.</param>
        string RemoveRemote(string remoteName);

        /// <summary>
        /// Renames the registered remote by running <c>git remote rename</c> command.
        /// </summary>
        /// <param name="remoteName">The current remote name.</param>
        /// <param name="newName">The new remote name.</param>
        string RenameRemote(string remoteName, string newName);

        /// <summary>
        /// Parses the revisionExpression as a git reference and returns an <see cref="ObjectId"/>.
        /// </summary>
        /// <param name="revisionExpression">An expression like HEAD or commit hash that can be parsed as a git reference.</param>
        /// <returns>An ObjectID representing that git reference</returns>
        ObjectId? RevParse(string revisionExpression);

        void SetSetting(string setting, string value);
        void UnsetSetting(string setting);

        Encoding CommitEncoding { get; }

        IConfigFileSettings EffectiveConfigFile { get; }

        Encoding FilesEncoding { get; }

        /// <summary>
        /// Returns git common directory.
        /// https://git-scm.com/docs/git-rev-parse#Documentation/git-rev-parse.txt---git-common-dir.
        /// </summary>
        string GitCommonDirectory { get; }

        /// <summary>
        /// Gets the default Git executable associated with this module.
        /// This executable can be non-native (i.e. WSL).
        /// </summary>
        IExecutable GitExecutable { get; }

        /// <summary>
        /// Gets the access to the current git executable associated with this module.
        /// This command runner can be non-native (i.e. WSL).
        /// </summary>
        IGitCommandRunner GitCommandRunner { get; }

        /// <summary>
        /// Encoding for commit header (message, notes, author, committer, emails).
        /// </summary>
        Encoding LogOutputEncoding { get; }

        /// <summary>
        /// If this module is a submodule, returns its path, otherwise <c>null</c>.
        /// </summary>
        string? SubmodulePath { get; }

        /// <summary>
        ///  Gets the super-project of the current git module, if any.
        /// </summary>
        /// <value>
        ///  If this module is a submodule, returns its super-project <see cref="IGitModule"/>, otherwise <c>null</c>.
        /// </value>
        public IGitModule? SuperprojectModule { get; }

        /// <summary>
        /// Gets the directory which contains the git repository.
        /// </summary>
        string WorkingDir { get; }

        /// <summary>
        /// Gets the location of .git directory for the current working folder.
        /// </summary>
        string WorkingDirGitDir { get; }

        /// <summary>
        /// Asks git to resolve the given relativePath
        /// git special folders are located in different directories depending on the kind of repo: submodule, worktree, main
        /// See https://git-scm.com/docs/git-rev-parse#Documentation/git-rev-parse.txt---git-pathltpathgt
        /// </summary>
        /// <param name="relativePath">A path relative to the .git directory</param>
        string ResolveGitInternalPath(string relativePath);

        /// <summary>Indicates whether the specified directory contains a git repository.</summary>
        bool IsValidGitWorkingDir();

        /// <summary>Indicates HEAD is not pointing to a branch (i.e. it is detached).</summary>
        bool IsDetachedHead();

        /// <summary>
        /// Convert the path for the Git executable. For WSL Git, the path will be adjusted.
        /// </summary>
        /// <param name="path">The Windows (native) path as seen by the application.</param>
        /// <returns>The Posix path if Windows Git, WSL path for WSL Git.</returns>
        public string GetPathForGitExecution(string? path);

        /// <summary>
        /// Convert a path to Windows application (native) format.
        /// </summary>
        /// <param name="path">Path as seen by the Git executable, possibly WSL Git.</param>
        /// <returns>The path in Windows format with native file separators.</returns>
        public string GetWindowsPath(string path);

        bool TryResolvePartialCommitId(string objectIdPrefix, [NotNullWhen(returnValue: true)] out ObjectId? objectId);

        string GetSubmoduleFullPath(string localPath);

        IEnumerable<IGitSubmoduleInfo?> GetSubmodulesInfo();

        /// <summary>
        /// Gets the local paths of any submodules of this git module.
        /// </summary>
        /// <remarks>
        /// <para>This method obtains its results by parsing the <c>.gitmodules</c> file.</para>
        ///
        /// <para>This approach is a faster than <see cref="GetSubmodulesInfo"/> which
        /// invokes the <c>git submodule</c> command.</para>
        /// </remarks>
        IReadOnlyList<string> GetSubmodulesLocalPaths(bool recursive = true);

        IGitModule GetSubmodule(string submoduleName);

        /// <summary>
        /// Retrieves registered remotes by running <c>git remote show</c> command.
        /// </summary>
        /// <returns>Registered remotes.</returns>
        IReadOnlyList<string> GetRemoteNames();

        /// <summary>
        /// Gets the commit ID of the currently checked out commit.
        /// If the repo is bare or has no commits, <c>null</c> is returned.
        /// </summary>
        ObjectId? GetCurrentCheckout();

        /// <summary>Gets the remote of the current branch; or "" if no remote is configured.</summary>
        string GetCurrentRemote();

        GitRevision GetRevision(ObjectId? objectId = null, bool shortFormat = false, bool loadRefs = false);

        Task<IReadOnlyList<Remote>> GetRemotesAsync();

        string GetSetting(string setting);

        /// <summary>
        ///  Gets the config setting from git converted in an expected C# value type (bool, int, etc.).
        /// </summary>
        /// <typeparam name="T">The expected type of the git setting.</typeparam>
        /// <param name="setting">The git setting key.</param>
        /// <returns>The value converted to the <typeparamref name="T" /> type; <see langword="null"/> if the settings is not set.</returns>
        /// <exception cref="Settings.GitConfigFormatException">
        ///  The value of the git setting <paramref name="setting" /> cannot be converted in the specified type <typeparamref name="T" />.
        /// </exception>
        T? GetSetting<T>(string setting) where T : struct;

        string GetEffectiveSetting(string setting);

        /// <summary>
        ///  Gets the config setting from git converted in an expected C# value type (bool, int, etc.).
        /// </summary>
        /// <typeparam name="T">The expected type of the git setting.</typeparam>
        /// <param name="setting">The git setting key.</param>
        /// <returns>The value converted to the <typeparamref name="T" /> type; <see langword="null"/> if the settings is not set.</returns>
        /// <exception cref="Settings.GitConfigFormatException">
        ///  The value of the git setting <paramref name="setting" /> cannot be converted in the specified type <typeparamref name="T" />.
        /// </exception>
        T? GetEffectiveSetting<T>(string setting) where T : struct;

        /// <summary>
        /// Get the config setting from git according to the scope.
        /// </summary>
        /// <param name="setting">The setting key.</param>
        /// <param name="scopeArg">The scope for the config like "--global" according to https://git-scm.com/docs/git-config#_description. An empty string is the effective settings.</param>
        /// <param name="cache"><see langword="true"/> if the result shall be cached.</param>
        /// <returns>The value of the setting or <see langword="null"/> if the value is not set.</returns>
        string? GetGitSetting(string setting, string scopeArg, bool cache = false);

        /// <summary>
        /// Get the effective config setting from git.
        /// </summary>
        /// <param name="setting">The setting key.</param>
        /// <param name="cache"><see langword="true"/> if the result shall be cached.</param>
        /// <returns>The value of the setting or <see langword="null"/> if the value is not set.</returns>
        string? GetEffectiveGitSetting(string setting, bool cache = false);

        SettingsSource GetEffectiveSettingsByPath(string path);

        /// <summary>
        /// Gets the name of the currently checked out branch.
        /// </summary>
        /// <param name="emptyIfDetached">Defines the value returned if HEAD is detached. <see langword="true"/> to return <see cref="string.Empty"/>; <see langword="false"/> to return "(no branch)".</param>
        /// <returns>
        /// The name of the branch (for example: "main"); the value requested by <paramref name="emptyIfDetached"/>, if HEAD is detached.
        /// </returns>
        string GetSelectedBranch(bool emptyIfDetached = false);

        /// <summary>true if ".git" directory does NOT exist.</summary>
        bool IsBareRepository();

        bool IsRunningGitProcess();

        SettingsSource GetEffectiveSettings();
        SettingsSource GetLocalSettings();

        string? ReEncodeStringFromLossless(string? s);

        string ReEncodeCommitMessage(string s);

        string? GetDescribe(ObjectId commitId, CancellationToken cancellationToken = default);

        (int totalCount, Dictionary<string, int> countByName) GetCommitsByContributor(DateTime? since = null, DateTime? until = null);

        void SaveBlobAs(string saveAs, string blob);
        Task<(char code, ObjectId commitId)> GetSuperprojectCurrentCheckoutAsync();
        Task<Patch?> GetCurrentChangesAsync(string? fileName, string? oldFileName, bool staged, string extraDiffArguments, Encoding? encoding = null, bool noLocks = false);
        Task<string?> GetFileContentsAsync(GitItemStatus file);
        IReadOnlyList<GitStash> GetStashes(bool noLocks);
        IReadOnlyList<GitItemStatus> GetWorkTreeFiles();
        SubmoduleStatus CheckSubmoduleStatus(ObjectId? commit, ObjectId? oldCommit, CommitData? data, CommitData? oldData, bool loadData);
        bool ResetAllChanges(bool clean, bool onlyWorkTree = false);

        /// <summary>
        /// Get a list of diff/merge tools known by Git.
        /// This normally requires long time (up to tenths of seconds)
        /// </summary>
        /// <param name="isDiff">diff or merge.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>the Git output.</returns>
        string GetCustomDiffMergeTools(bool isDiff, CancellationToken cancellationToken);
        Task<(Patch? patch, string? errorMessage)> GetSingleDiffAsync(ObjectId? firstId, ObjectId? secondId, string? fileName, string? oldFileName, string extraDiffArguments, Encoding encoding, bool cacheResult, bool isTracked);
        int? GetCommitCount(string parent, string child, bool cache, bool throwOnErrorExit);

        /// <summary>
        ///  Gets the top-most parent module of the current git submodule.
        /// </summary>
        /// <value>
        ///  If this module is a submodule, returns its top-most parent module, otherwise <see langword="null"/>.
        /// </value>
        IGitModule GetTopModule();
        string? GetCurrentSubmoduleLocalPath();
        ISubmodulesConfigFile GetSubmodulesConfigFile();
        string GetStatusText(bool untracked);
        ExecutionResult GetDiffFiles(string? firstRevision, string? secondRevision, bool noCache, bool nullSeparated, CancellationToken cancellationToken);
        bool InTheMiddleOfBisect();
        IReadOnlyList<GitItemStatus> GetDiffFilesWithUntracked(string? firstRevision, string? secondRevision, StagedStatus stagedStatus, bool noCache, CancellationToken cancellationToken);
        bool IsDirtyDir();
        Task<string> GetRangeDiffAsync(ObjectId firstId, ObjectId secondId, ObjectId? firstBase, ObjectId? secondBase, string extraDiffArguments, string? pathFilter, CancellationToken cancellationToken);
        bool InTheMiddleOfPatch();
        bool InTheMiddleOfConflictedMerge(bool throwOnErrorExit = true);
        bool InTheMiddleOfAction();
        string ApplyPatch(string dirText, ArgumentString arguments);
        bool InTheMiddleOfRebase();
        bool InTheMiddleOfMerge();
        IReadOnlyList<GitItemStatus> GetDiffFilesWithSubmodulesStatus(ObjectId? firstId, ObjectId? secondId, ObjectId? parentToSecond, CancellationToken cancellationToken);
        IReadOnlyList<GitItemStatus> GetIndexFilesWithSubmodulesStatus();
        ObjectId? GetFileBlobHash(string fileName, ObjectId objectId);
        void OpenFilesWithDifftool(string? firstGitCommit, string? secondGitCommit, string? customTool);
        IReadOnlyList<string> GetIgnoredFiles(IEnumerable<string> ignorePatterns);
        void UnlockIndex(bool includeSubmodules);
        bool EditNotes(ObjectId objectId);
        ArgumentString FetchCmd(string? remote, string? remoteBranch, string? localBranch, bool? fetchTags = false, bool isUnshallow = false, bool pruneRemoteBranches = false, bool pruneRemoteBranchesAndTags = false);
        void RunGui();
        void RunGitK();
        ObjectId? GetMergeBase(ObjectId a, ObjectId b);
        (int? first, int? second) GetCommitRangeDiffCount(ObjectId firstId, ObjectId secondId);
        IReadOnlyList<GitItemStatus> GetCombinedDiffFileList(ObjectId mergeCommitObjectId);
        IReadOnlyList<GitItemStatus> GetTreeFiles(ObjectId treeGuid, bool full);
        IReadOnlyList<string> GetFullTree(string id);

        /// <summary>
        /// Gets branches which contain the given commit.
        /// If both local and remote branches are requested, remote branches are prefixed with "remotes/"
        /// (as returned by git branch -a).
        /// </summary>
        /// <param name="objectId">The sha1.</param>
        /// <param name="getLocal">Pass true to include local branches.</param>
        /// <param name="getRemote">Pass true to include remote branches.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        IReadOnlyList<string> GetAllBranchesWhichContainGivenCommit(ObjectId objectId, bool getLocal, bool getRemote, CancellationToken cancellationToken = default);

        /// <summary>
        /// Uses check-ref-format to ensure that a branch name is well formed.
        /// </summary>
        /// <param name="branchName">Branch name to test.</param>
        /// <returns>true if <paramref name="branchName"/> is valid reference name, otherwise false.</returns>
        bool CheckBranchFormat(string branchName);
        string? GetLocalTrackingBranchName(string remoteName, string branch);
        string GetCommitCountString(string from, string to);
        IReadOnlyList<GitItemStatus> GetAllChangedFilesWithSubmodulesStatus(CancellationToken cancellationToken);
        IReadOnlyList<GitItemStatus> GetAllChangedFilesWithSubmodulesStatus(bool excludeIgnoredFiles, bool excludeAssumeUnchangedFiles, bool excludeSkipWorktreeFiles, UntrackedFilesMode untrackedFiles, CancellationToken cancellationToken);
        bool ResetChanges(ObjectId? resetId, IReadOnlyList<GitItemStatus> selectedItems, bool resetAndDelete, IFullPathResolver fullPathResolver, out StringBuilder output, Action<BatchProgressEventArgs>? progressAction);
        bool HasSubmodules();
        void OpenWithDifftool(string? filename, string? oldFileName = "", string? firstRevision = GitRevision.IndexGuid, string? secondRevision = GitRevision.WorkTreeGuid, string? extraDiffArguments = null, bool isTracked = true, string? customTool = null);
        void OpenWithDifftoolDirDiff(string? firstRevision, string? secondRevision, string? customTool);
        IReadOnlyList<ObjectId> GetParents(ObjectId objectId);
        IReadOnlyList<GitRevision> GetParentRevisions(ObjectId objectId);
        Task<ConflictData> GetConflictAsync(string? filename);
        Task<Dictionary<IGitRef, IGitItem?>> GetSubmoduleItemsForEachRefAsync(string? filename, bool noLocks);

        /// <summary>
        /// Returns tag's message. If the lightweight tag is passed, corresponding commit message
        /// is returned.
        /// </summary>
        string? GetTagMessage(string? tag, CancellationToken cancellationToken = default);
        void UnstageFile(string file);
        bool UnstageFiles(IReadOnlyList<GitItemStatus> files, out string allOutput);
        bool StageFile(string file);
        bool StageFiles(IReadOnlyList<GitItemStatus> files, out string allOutput);
        IEnumerable<IGitRef> GetRemoteBranches();
        IEnumerable<string?> GetPreviousCommitMessages(int count, string revision, string authorPattern);
        Task<bool> AddInteractiveAsync(GitItemStatus file);
        string GetRebaseDir();

        /// <summary>
        ///  Unstage files in batch.
        /// </summary>
        /// <param name="files">The list of files to unstage.</param>
        /// <param name="progressCallback">The progress update callback.</param>
        /// <returns><see langword="true" /> if changes should be rescanned; <see langword="false" />, otherwise.</returns>.
        public bool BatchUnstageFiles(IEnumerable<GitItemStatus> files, Action<BatchProgressEventArgs>? progressCallback = null);

        bool StopTrackingFile(string filename);

        /// <summary>
        ///  Set/unset whether given files are assumed unchanged by git-status.
        /// </summary>
        /// <param name="files">The list of files to set the status for.</param>
        /// <param name="assumeUnchanged">The status value.</param>
        /// <param name="allOutput">stdout and stderr output.</param>
        /// <returns><see langword="true"/> if no errors occurred; otherwise; <see langword="false"/>, otherwise.</returns>
        bool AssumeUnchangedFiles(IReadOnlyList<GitItemStatus> files, bool assumeUnchanged, out string allOutput);

        /// <summary>
        /// Performs <c>git-checkout</c> for the given files.
        /// </summary>
        /// <param name="files">The list of files to checkout.</param>
        /// <param name="revision">The revision to checkout; <see langword="null"/> is handled as <c>HEAD</c>.</param>
        /// <param name="force">Indicates whether to perform a forced checkout.</param>
        string CheckoutFiles(IReadOnlyList<string> files, ObjectId? revision, bool force);

        void DeleteTag(string tagName);

        string? ShowObject(ObjectId objectId);

        IReadOnlyList<GitItemStatus> GetStashDiffFiles(string stashName);

        IReadOnlyList<GitItemStatus> GetAllChangedFiles(bool excludeIgnoredFiles = true,
            bool excludeAssumeUnchangedFiles = true, bool excludeSkipWorktreeFiles = true,
            UntrackedFilesMode untrackedFiles = UntrackedFilesMode.Default, CancellationToken cancellationToken = default);

        bool HandleConflictsSaveSide(string fileName, string saveAsFileName, string side);

        void RunMergeTool(string? fileName = "", string? customTool = null);

        bool HandleConflictSelectSide(string fileName, string side);

        void Reset(ResetMode mode, string? file = null);

        (string? baseFile, string? localFile, string? remoteFile) CheckoutConflictedFiles(ConflictData unmergedData);

        bool IsSubmodule(string submodulePath);

        Task<List<ConflictData>> GetConflictsAsync(string? filename = "");
        string FormatPatch(string from, string to, string output, int? start = null);

        // TODO: convert to IGitCommand
        ArgumentString PullCmd(string source, string curRemoteBranch, bool checked1, bool? v, bool checked2);

        bool ExistsMergeCommit(string? startRev, string? endRev);

        string GetFileText(ObjectId id, Encoding encoding);

        MemoryStream? GetFileStream(string blob);

        IReadOnlyList<GitItemStatus> GitStatus(UntrackedFilesMode untrackedFilesMode, IgnoreSubmodulesMode ignoreSubmodulesMode = IgnoreSubmodulesMode.None);

        /// <summary>
        ///  Tries to start Pageant for the specified remote repo (using the remote's PuTTY key file).
        /// </summary>
        /// <returns><see langword="true"/> if the remote has a PuTTY key file; <see langword="false"/>, otherwise.</returns>
        string GetPuttyKeyFileForRemote(string? remote);

        /// <summary>
        /// GitVersion for the default GitExecutable.
        /// </summary>
        IGitVersion GitVersion { get; }

        string? GetCombinedDiffContent(ObjectId revisionOfMergeCommit, string filePath, string extraArgs, Encoding encoding);
        bool IsMerge(ObjectId objectId);
        IEnumerable<string> GetMergedBranches(bool includeRemote = false);
        Task<string[]> GetMergedBranchesAsync(bool includeRemote, bool fullRefname, string? commit, CancellationToken cancellationToken);
        IReadOnlyList<string> GetMergedRemoteBranches();
        IReadOnlyList<IGitRef> GetRemoteServerRefs(string remote, bool tags, bool branches, out string? errorOutput, CancellationToken cancellationToken);

        /// <summary>
        /// Format branch name, check if name is valid for repository.
        /// </summary>
        /// <param name="branchName">Branch name to test.</param>
        /// <returns>Well formed branch name.</returns>
        string FormatBranchName(string branchName);

        /// <summary>
        /// Set/unset whether given items are not flagged as changed by git-status.
        /// </summary>
        /// <param name="files">The files to set the status for.</param>
        /// <param name="skipWorktree">The status value.</param>
        /// <param name="allOutput">stdout and stderr output.</param>
        /// <returns><see langword="true"/> if no errors occurred; otherwise, <see langword="false"/>.</returns>
        bool SkipWorktreeFiles(IReadOnlyList<GitItemStatus> files, bool skipWorktree, out string allOutput);

        Task<bool> ResetInteractiveAsync(GitItemStatus file);

        IReadOnlyList<IGitRef> ParseRefs(string refList);

        /// <summary>
        /// Gets all tags which contain the given commit.
        /// </summary>
        /// <param name="objectId">The sha1.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        IReadOnlyList<string> GetAllTagsWhichContainGivenCommit(ObjectId objectId, CancellationToken cancellationToken);

        /// <summary>
        ///  Gets the remote branch.
        /// </summary>
        /// <returns>
        ///  The remote branch of the specified local branch; or "" if none is configured.
        /// </returns>
        string GetRemoteBranch(string branch);

        string RenameBranch(string name, string newName);

        GitBlame Blame(string? fileName, string from, Encoding encoding, string? lines = null, CancellationToken cancellationToken = default);
        IReadOnlySet<string> GetReflogHashes();
    }
}
