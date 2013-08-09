using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GitUIPluginInterfaces
{
    public interface IGitModule : IDisposable
    {
        /// <summary>
        /// Run git command, console window is hidden, redirect output
        /// </summary>
        Process RunGitCmdDetached(string arguments, Encoding encoding = null);

        /// <summary>
        /// Run git command, console window is hidden, wait for exit, redirect output
        /// </summary>
        string RunGitCmd(string arguments, out int exitCode, Encoding encoding = null, byte[] stdInput = null);

        /// <summary>
        /// Run git command, console window is hidden, wait for exit, redirect output
        /// </summary>
        string RunGitCmd(string arguments, Encoding encoding = null, byte[] stdInput = null);

        /// <summary>
        /// Run command, console window is hidden, wait for exit, redirect output
        /// </summary>
        string RunCmd(string cmd, string arguments, out int exitCode, Encoding encoding = null, byte[] stdIn = null);

        /// <summary>
        /// Run command, console window is hidden, wait for exit, redirect output
        /// </summary>
        string RunCmd(string cmd, string arguments, Encoding encoding = null, byte[] stdIn = null);

        string RunBatchFile(string batchFile);

        string GitWorkingDir { get; }

        string GetGitDirectory();

        bool IsValidGitWorkingDir();

        string GitCommand { get; }

        Version AppVersion { get; }

        string GravatarCacheDir { get; }

        IEnumerable<IGitSubmoduleInfo> GetSubmodulesInfo();

        IList<string> GetSubmodulesLocalPathes(bool recursive = true);

        IGitModule GetSubmodule(string submoduleName);

        string[] GetRemotes(bool allowEmpty);

        string GetSetting(string setting);

        bool StartPageantForRemote(string remote);

        string GetSelectedBranch();

        bool IsBareRepository();

        bool IsRunningGitProcess();
    }
}
