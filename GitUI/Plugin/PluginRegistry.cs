using System.Collections.Generic;
using System.Linq;
using GitCommands;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.RepositoryHosts;
using JetBrains.Annotations;

namespace GitUI
{
    public static class PluginRegistry
    {
        public static IList<IGitPlugin> Plugins { get; } = new List<IGitPlugin>();

        public static List<IRepositoryHostPlugin> GitHosters { get; } = new List<IRepositoryHostPlugin>();

        public static void Initialize()
        {
            lock (Plugins)
            {
                if (Plugins.Count > 0)
                {
                    return;
                }

                foreach (var plugin in ManagedExtensibility.GetExports<IGitPlugin>().Select(lazy => lazy.Value))
                {
                    plugin.SettingsContainer = new GitPluginSettingsContainer(plugin.Name);

                    if (plugin is IRepositoryHostPlugin repositoryHostPlugin)
                    {
                        GitHosters.Add(repositoryHostPlugin);
                    }

                    Plugins.Add(plugin);
                }
            }
        }

        [CanBeNull]
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