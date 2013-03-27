using System.Collections.Generic;

namespace GitUIPluginInterfaces
{
    /// <summary>Provides manipulation with git module.</summary>
    public interface IGitModule
    {
        /// <summary>Gets all current git submodules.</summary>
        IEnumerable<IGitSubmodule> GetSubmodules();

        string RunGit(string arguments);

        string RunGit(string arguments, out int exitCode);

        string RunBatchFile(string batchFile);

        /// <summary>Gets the directory which contains the git repository.</summary>
        string GitWorkingDir { get; }

        /// <summary>Gets the ".git" directory path.</summary>
        string GetGitDirectory();

        /// <summary>Indicates whether the specified directory contains a git repository.</summary>
        bool IsValidGitWorkingDir(string workingDir);

        /// <summary>Gets the path to the git application executable.</summary>
        string GitCommand { get; }

        string GitVersion { get; }

        string GravatarCacheDir { get; }

        IList<string> GetSubmodulesLocalPathes();

        IGitModule GetISubmodule(string submoduleName);

        string[] GetRemotes(bool allowEmpty);

        string GetISetting(string setting);

        bool StartPageantForRemote(string remote);

        string RunCmd(string cmd, string arguments);

        string RunCmd(string cmd, string arguments, byte[] stdIn);

        /// <summary>Gets the current branch; or "(no branch)" if HEAD is detached.</summary>
        string GetSelectedBranch();

        /// <summary>true if ".git" directory does NOT exist.</summary>
        bool IsBareRepository();
    }
}
