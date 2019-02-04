using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace GitUIPluginInterfaces
{
    /// <summary>Provides manipulation with git module.</summary>
    public interface IGitModule
    {
        [NotNull]
        IConfigFileSettings LocalConfigFile { get; }

        string AddRemote(string remoteName, string path);
        IReadOnlyList<IGitRef> GetRefs(bool tags = true, bool branches = true);
        IEnumerable<string> GetSettings(string setting);
        IEnumerable<IGitItem> GetTree([CanBeNull] ObjectId commitId, bool full);

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
        /// Parses the revisionExpression as a git reference and returns an <see cref="ObjectId"/>./>
        /// </summary>
        /// <param name="revisionExpression">An expression like HEAD or commit hash that can be parsed as a git reference.</param>
        /// <returns>An ObjectID representing that git reference</returns>
        ObjectId RevParse(string revisionExpression);

        void SetSetting(string setting, string value);
        void UnsetSetting(string setting);

        /// <summary>Gets the directory which contains the git repository.</summary>
        string WorkingDir { get; }

        /// <summary>
        /// Gets the access to the current git executable associated with this module.
        /// </summary>
        IExecutable GitExecutable { get; }

        /// <summary>
        /// Gets the access to the current git executable associated with this module.
        /// </summary>
        [NotNull]
        IGitCommandRunner GitCommandRunner { get; }

        /// <summary>
        /// Gets the location of .git directory for the current working folder.
        /// </summary>
        string WorkingDirGitDir { get; }

        /// <summary>
        /// Asks git to resolve the given relativePath
        /// git special folders are located in different directories depending on the kind of repo: submodule, worktree, main
        /// See https://git-scm.com/docs/git-rev-parse#git-rev-parse---git-pathltpathgt
        /// </summary>
        /// <param name="relativePath">A path relative to the .git directory</param>
        string ResolveGitInternalPath(string relativePath);

        /// <summary>Indicates whether the specified directory contains a git repository.</summary>
        bool IsValidGitWorkingDir();

        /// <summary>Indicates HEAD is not pointing to a branch (i.e. it is detached).</summary>
        bool IsDetachedHead();

        [ContractAnnotation("=>false,objectId:null")]
        [ContractAnnotation("=>true,objectId:notnull")]
        bool TryResolvePartialCommitId(string objectIdPrefix, out ObjectId objectId);

        string GetSubmoduleFullPath(string localPath);

        IEnumerable<IGitSubmoduleInfo> GetSubmodulesInfo();

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
        [CanBeNull]
        ObjectId GetCurrentCheckout();

        /// <summary>Gets the remote of the current branch; or "" if no remote is configured.</summary>
        string GetCurrentRemote();

        string GetSetting(string setting);
        string GetEffectiveSetting(string setting);

        bool StartPageantForRemote(string remote);

        /// <summary>Gets the current branch; or "(no branch)" if HEAD is detached.</summary>
        string GetSelectedBranch();

        /// <summary>true if ".git" directory does NOT exist.</summary>
        bool IsBareRepository();

        bool IsRunningGitProcess();

        [NotNull]
        ISettingsSource GetEffectiveSettings();

        string ReEncodeStringFromLossless(string s);

        string ReEncodeCommitMessage(string s, string toEncodingName);

        string GetDescribe(ObjectId commitId);

        (int totalCount, Dictionary<string, int> countByName) GetCommitsByContributor(DateTime? since = null, DateTime? until = null);
    }
}
