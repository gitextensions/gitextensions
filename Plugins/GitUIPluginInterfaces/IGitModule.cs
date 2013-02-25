using System;
using System.Collections.Generic;

namespace GitUIPluginInterfaces
{
    /// <summary>Provides manipulation with git module.</summary>
    public interface IGitModule
    {
        IEnumerable<IGitSubmodule> GetSubmodules();

        string RunGit(string arguments);

        string RunGit(string arguments, out int exitCode);

        string RunBatchFile(string batchFile);

        string GitWorkingDir { get; }

        string GetGitDirectory();

        bool IsValidGitWorkingDir(string workingDir);

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

        string GetSelectedBranch();

        bool IsBareRepository();
    }
}
