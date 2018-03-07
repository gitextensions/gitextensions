using System.Collections.Generic;
using System.Linq;
using GitCommands;
using GitUIPluginInterfaces.RepositoryHosts;

namespace GitUI
{
    public class RepoHosts
    {
        public static List<IRepositoryHostPlugin> GitHosters { get; }
        static RepoHosts()
        {
            GitHosters = new List<IRepositoryHostPlugin>();
        }

        public static IRepositoryHostPlugin TryGetGitHosterForModule(GitModule module)
        {
            if (!module.IsValidGitWorkingDir())
            {
                return null;
            }

            return GitHosters.FirstOrDefault(gitHoster => gitHoster.GitModuleIsRelevantToMe(module));
        }
    }
}
