using System.Collections.Generic;
using System.Linq;
using GitUIPluginInterfaces.RepositoryHosts;
using GitCommands;

namespace GitUI.RepoHosting
{
    public class RepoHosts
    {
        public static List<IRepositoryHostPlugin> GitHosters { get; private set; }
        static RepoHosts()
        {
            GitHosters = new List<IRepositoryHostPlugin>();
        }

        public static IRepositoryHostPlugin TryGetGitHosterForCurrentWorkingDir()
        {
            if (!Settings.ValidWorkingDir())
                return null;

            return GitHosters.FirstOrDefault(gitHoster => gitHoster.CurrentWorkingDirRepoIsRelevantToMe);
        }
    }
}
