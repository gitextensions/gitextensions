using System;
using System.Collections.Generic;
using GitUI.Plugin;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.RepositoryHosts;

namespace GitUI
{
    static public class PluginExtraction
    {
        static public void ExtractPluginTypes(IEnumerable<Type> pluginTypes)
        {
            foreach (var type in pluginTypes)
            {
                if (!typeof(IGitPlugin).IsAssignableFrom(type) || type.IsInterface || type.IsAbstract)
                    continue;

                var gitPlugin = Activator.CreateInstance(type) as IGitPlugin;
                if (gitPlugin == null)
                    continue;

                gitPlugin.SettingsContainer = new GitPluginSettingsContainer(gitPlugin.Name);

                var gitRepositoryHostPlugin = gitPlugin as IRepositoryHostPlugin;
                if (gitRepositoryHostPlugin != null)
                    RepoHosts.GitHosters.Add(gitRepositoryHostPlugin);

                LoadedPlugins.Plugins.Add(gitPlugin);
            }
        }
    }
}