using System.Linq;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.RepositoryHosts;

namespace GitUI
{
    public static class PluginLoader
    {
        public static void Load()
        {
            lock (Plugin.LoadedPlugins.Plugins)
            {
                if (Plugin.LoadedPlugins.Plugins.Count > 0)
                {
                    return;
                }

                foreach (var plugin in ManagedExtensibility.GetExports<IGitPlugin>().Select(lazy => lazy.Value))
                {
                    plugin.SettingsContainer = new GitPluginSettingsContainer(plugin.Name);
                    if (plugin is IRepositoryHostPlugin repositoryHostPlugin)
                    {
                        RepoHosts.GitHosters.Add(repositoryHostPlugin);
                    }

                    Plugin.LoadedPlugins.Plugins.Add(plugin);
                }
            }
        }
    }
}