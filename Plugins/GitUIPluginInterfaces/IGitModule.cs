using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GitUIPluginInterfaces
{
    /// <summary>Provides manipulation with git module.</summary>
    public interface IGitModule
    {
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

        /// <summary>Indicates whether the specified directory contains a git repository.</summary>
        bool IsValidGitWorkingDir();

        /// <summary>Gets the path to the git application executable.</summary>
        string GitCommand { get; }

        Version AppVersion { get; }

        string GravatarCacheDir { get; }

        IEnumerable<IGitSubmoduleInfo> GetSubmodulesInfo();

        IList<string> GetSubmodulesLocalPathes(bool recursive = true);

        IGitModule GetSubmodule(string submoduleName);

        string[] GetRemotes(bool allowEmpty);

        string GetSetting(string setting);

        bool StartPageantForRemote(string remote);

        /// <summary>Gets the current branch; or "(no branch)" if HEAD is detached.</summary>
        string GetSelectedBranch();

        /// <summary>true if ".git" directory does NOT exist.</summary>
        bool IsBareRepository();

        bool IsRunningGitProcess();
    }
}
