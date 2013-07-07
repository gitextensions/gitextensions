using System;
using System.Collections.Generic;
using System.Text;

namespace GitUIPluginInterfaces
{
    public interface IGitModule
    {
        string RunCmd(string cmd, string arguments, Encoding encoding = null, byte[] stdIn = null);

        string RunGitCmd(string arguments, Encoding encoding = null, byte[] stdInput = null);

        string RunGitCmd(string arguments, out int exitCode, Encoding encoding = null, byte[] stdInput = null);

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
