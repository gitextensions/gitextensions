using System;
using System.Collections.Generic;
using GitUI;
using GitUI.Plugin;
using GitUI.RepoHosting;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.RepositoryHosts;

namespace GitExtensions
{
    public class PluginExtraction
    {
        public void ExtractPluginTypes(IEnumerable<Type> pluginTypes)
        {
            foreach (var type in pluginTypes)
            {
                if (!typeof(IGitPlugin).IsAssignableFrom(type) || type.IsInterface)
                    continue;

                var gitPlugin = Activator.CreateInstance(type) as IGitPlugin;
                if (gitPlugin == null)
                    continue;

                gitPlugin.Settings = new GitPluginSettingsContainer(gitPlugin.Description);
                gitPlugin.Register(GitUICommands.Instance);

                if (gitPlugin is IRepositoryHostPlugin)
                    RepoHosts.GitHosters.Add(gitPlugin as IRepositoryHostPlugin);

                LoadedPlugins.Plugins.Add(gitPlugin);
            }
        }
    }
}