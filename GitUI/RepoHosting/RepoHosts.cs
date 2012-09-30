using System.Collections.Generic;
using System.Linq;
using GitCommands;
using GitUIPluginInterfaces.RepositoryHosts;

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
            if (!GitModule.Current.ValidWorkingDir())
                return null;

            return GitHosters.FirstOrDefault(gitHoster => gitHoster.CurrentWorkingDirRepoIsRelevantToMe);
        }
    }
}
