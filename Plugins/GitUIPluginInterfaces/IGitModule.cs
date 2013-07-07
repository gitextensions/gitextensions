using System;
using System.Collections.Generic;
using System.Text;

namespace GitUIPluginInterfaces
{
    public interface IGitModule
    {
        IEnumerable<IGitSubmodule> GetSubmodules();

        string RunGitCmd(string arguments, Encoding encoding = null, byte[] stdInput = null);

        string RunGitCmd(string arguments, out int exitCode, Encoding encoding = null, byte[] stdInput = null);

        string RunBatchFile(string batchFile);

        string GitWorkingDir { get; }

        string GetGitDirectory();

        bool IsValidGitWorkingDir();

        string GitCommand { get; }

        Version AppVersion { get; }

        string GravatarCacheDir { get; }

        IList<string> GetSubmodulesLocalPathes(bool recursive = true);

        IGitModule GetISubmodule(string submoduleName);

        string[] GetRemotes(bool allowEmpty);

        string GetISetting(string setting);

        bool StartPageantForRemote(string remote);

        string RunCmd(string cmd, string arguments, Encoding encoding = null, byte[] stdIn = null);

        string GetSelectedBranch();

        bool IsBareRepository();

        bool IsRunningGitProcess();
    }
}
