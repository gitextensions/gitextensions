using System.Collections.Generic;

namespace GitUIPluginInterfaces
{
    public interface IGitCommands
    {
        IList<IGitSubmodule> GetSubmodules();

        string RunGit(string arguments);
    }
}
