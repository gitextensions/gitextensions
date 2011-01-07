using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitUIPluginInterfaces;
using GitUI.Plugin;
using GitCommands;

namespace GitUI.RepoHosting
{
    public class RepoHosts
    {
        public static List<IGitHostingPlugin> GitHosters { get; private set; }
        static RepoHosts()
        {
            GitHosters = new List<IGitHostingPlugin>();
        }

        public static IGitHostingPlugin TryGetGitHosterForCurrentWorkingDir()
        {
            if (!Settings.ValidWorkingDir())
                return null;

            foreach (var gitHoster in GitHosters)
            {
                if (gitHoster.CurrentWorkingDirRepoIsRelevantToMe)
                    return gitHoster;
            }

            return null;
        }
    }
}
