﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GitUIPluginInterfaces
{
    /// <summary>Provides manipulation with git module.</summary>
    public interface IGitModule
    {
        IConfigFileSettings LocalConfigFile { get; }

        string AddRemote(string remoteName, string path);
        IList<IGitRef> GetRefs(bool tags = true, bool branches = true);
        IEnumerable<string> GetSettings(string setting);
        IList<IGitItem> GetTree(string id, bool full);

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
        void SetSetting(string setting, string value);
        void UnsetSetting(string setting);

        /// <summary>
        /// Run git command, console window is hidden, redirect output
        /// </summary>
        Process RunGitCmdDetached(string arguments, Encoding encoding = null);

        /// <summary>
        /// Run git command, console window is hidden, wait for exit, redirect output
        /// </summary>
        string RunGitCmd(string arguments, Encoding encoding = null, byte[] stdInput = null);

        /// <summary>
        /// Run git command, console window is hidden, wait for exit, redirect output
        /// </summary>
        CmdResult RunGitCmdResult(string arguments, Encoding encoding = null, byte[] stdInput = null);

        /// <summary>
        /// Run command, console window is hidden, wait for exit, redirect output
        /// </summary>
        string RunCmd(string cmd, string arguments, Encoding encoding = null, byte[] stdIn = null);

        /// <summary>
        /// Run command, console window is hidden, wait for exit, redirect output
        /// </summary>
        CmdResult RunCmdResult(string cmd, string arguments, Encoding encoding = null, byte[] stdInput = null);

        string RunBatchFile(string batchFile);

        /// <summary>Gets the directory which contains the git repository.</summary>
        string WorkingDir { get; }

        /// <summary>Gets the ".git" directory path.</summary>
        string GetGitDirectory();

        /// <summary>
        /// Asks git to resolve the given relativePath
        /// git special folders are located in different directories depending on the kind of repo: submodule, worktree, main
        /// See https://git-scm.com/docs/git-rev-parse#git-rev-parse---git-pathltpathgt
        /// </summary>
        /// <param name="relativePath">A path relative to the .git directory</param>
        /// <returns></returns>
        string ResolveGitInternalPath(string relativePath);

        /// <summary>Indicates whether the specified directory contains a git repository.</summary>
        bool IsValidGitWorkingDir();

        /// <summary>Indicates whether the repository is in a 'detached HEAD' state.</summary>
        bool IsDetachedHead();

        /// <summary>Gets the path to the git application executable.</summary>
        string GitCommand { get; }

        Version AppVersion { get; }

        string GravatarCacheDir { get; }

        IEnumerable<IGitSubmoduleInfo> GetSubmodulesInfo();

        IList<string> GetSubmodulesLocalPaths(bool recursive = true);

        IGitModule GetSubmodule(string submoduleName);

        /// <summary>
        /// Retrieves registered remotes by running <c>git remote show</c> command.
        /// </summary>
        /// <returns>Registered remotes.</returns>
        string[] GetRemotes();

        /// <summary>
        /// Retrieves registered remotes by running <c>git remote show</c> command.
        /// </summary>
        /// <param name="allowEmpty"></param>
        /// <returns>Registered remotes.</returns>
        string[] GetRemotes(bool allowEmpty);

        string GetSetting(string setting);
        string GetEffectiveSetting(string setting);

        bool StartPageantForRemote(string remote);

        /// <summary>Gets the current branch; or "(no branch)" if HEAD is detached.</summary>
        string GetSelectedBranch();

        /// <summary>true if ".git" directory does NOT exist.</summary>
        bool IsBareRepository();

        bool IsRunningGitProcess();

        ISettingsSource GetEffectiveSettings();
    }
}
