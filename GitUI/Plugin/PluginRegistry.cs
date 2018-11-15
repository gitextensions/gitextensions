using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        /// <param name="postInitialiseAsync">A function to execute once plugins are loaded.</param>
        public static Task InitializeAsync(Func<Task> postInitialiseAsync)
        {
            lock (Plugins)
            {
                if (Plugins.Count > 0)
                {
                    return Task.CompletedTask;
                }

                try
                {
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

#pragma warning disable VSTHRD105 // Avoid method overloads that assume TaskScheduler.Current
                return postInitialiseAsync()
                    .ContinueWith(t => PluginsRegistered = true);
#pragma warning restore VSTHRD105 // Avoid method overloads that assume TaskScheduler.Current
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