using System.Collections.Generic;

namespace GitUIPluginInterfaces
{
    public interface IGitModule
    {
        IList<IGitSubmodule> GetSubmodules();

        string RunGit(string arguments);

        string RunGit(string arguments, out int exitCode);

        string RunBatchFile(string batchFile);

        string GitWorkingDir { get; }

        string GetGitDirectory();

        bool IsValidGitWorkingDir(string workingDir);

        string GitCommand { get; }

        string GitVersion { get; }

        string GravatarCacheDir { get; }

    }
}
