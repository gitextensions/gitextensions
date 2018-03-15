using System;
using System.Collections.Generic;
using GitUI.Plugin;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.RepositoryHosts;

namespace GitUI
{
    public static class PluginExtraction
    {
        public static void ExtractPluginTypes(IEnumerable<Type> pluginTypes)
        {
            foreach (var type in pluginTypes)
            {
                if (!typeof(IGitPlugin).IsAssignableFrom(type) || type.IsInterface || type.IsAbstract)
                {
                    continue;
                }

                if (Activator.CreateInstance(type) is IGitPlugin gitPlugin)
                {
                    gitPlugin.SettingsContainer = new GitPluginSettingsContainer(gitPlugin.Name);

                    if (gitPlugin is IRepositoryHostPlugin gitRepositoryHostPlugin)
                    {
                        RepoHosts.GitHosters.Add(gitRepositoryHostPlugin);
                    }

                    LoadedPlugins.Plugins.Add(gitPlugin);
                }
            }
        }
    }
}