using System.Collections.Generic;

namespace GitUIPluginInterfaces
{
    public interface IGitCommands
    {
        IList<IGitSubmodule> GetSubmodules();

        string RunGit(string arguments);

        string RunGit(string arguments, out int exitCode);

        string RunBatchFile(string batchFile);
    }
}
