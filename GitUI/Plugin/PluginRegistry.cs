﻿using System.Collections.Generic;
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

        public static bool PluginsRegistered { get; private set; }

        /// <summary>
        /// Initialises all available plugins on the background thread.
        /// </summary>
        public static void Initialize()
        {
            lock (Plugins)
            {
                if (Plugins.Count > 0)
                {
                    return;
                }

                try
                {
                    ManagedExtensibility.SetUserPluginsPath(AppSettings.UserPluginsPath);

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
                catch
                {
                    // no-op
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

            return GitHosters.FirstOrDefault(gitHoster => gitHoster.GitModuleIsRelevantToMe());
        }

        public static void Register(IGitUICommands gitUiCommands)
        {
            if (PluginsRegistered)
            {
                return;
            }

            PluginsRegistered = true;

            lock (Plugins)
            {
                Plugins.ForEach(p => p.Register(gitUiCommands));
            }
        }

        public static void Unregister(IGitUICommands gitUiCommands)
        {
            if (!PluginsRegistered)
            {
                return;
            }

            lock (Plugins)
            {
                Plugins.ForEach(p => p.Unregister(gitUiCommands));
            }

            PluginsRegistered = false;
        }
    }
}
