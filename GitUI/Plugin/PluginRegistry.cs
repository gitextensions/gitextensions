using System.Collections.Generic;
using System.Linq;
using GitCommands;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.Events;
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
                foreach (var plugin in Plugins)
                {
                    plugin.SettingsContainer.SetSettingsSource(gitUiCommands.GitModule.GetEffectiveSettings());

                    if (plugin is ILoadHandler handler)
                    {
                        handler.OnLoad(gitUiCommands);
                    }
                }

                gitUiCommands.PostCommit += OnPostCommit;
                gitUiCommands.PostRepositoryChanged += OnPostRepositoryChanged;
                gitUiCommands.PostSettings += OnPostSettings;
                gitUiCommands.PostUpdateSubmodules += OnPostUpdateSubmodules;
                gitUiCommands.PostBrowseInitialize += OnPostBrowseInitialize;
                gitUiCommands.PostRegisterPlugin += OnPostRegisterPlugin;
                gitUiCommands.PreCommit += OnPreCommit;
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
                gitUiCommands.PostCommit -= OnPostCommit;
                gitUiCommands.PostRepositoryChanged -= OnPostRepositoryChanged;
                gitUiCommands.PostSettings -= OnPostSettings;
                gitUiCommands.PostUpdateSubmodules -= OnPostUpdateSubmodules;
                gitUiCommands.PostBrowseInitialize -= OnPostBrowseInitialize;
                gitUiCommands.PostRegisterPlugin -= OnPostRegisterPlugin;
                gitUiCommands.PreCommit -= OnPreCommit;

                foreach (var plugin in Plugins)
                {
                    if (plugin is IUnloadHandler handler)
                    {
                        handler.OnUnload(gitUiCommands);
                    }

                    plugin.SettingsContainer.SetSettingsSource(null);
                }
            }

            PluginsRegistered = false;
        }

        #region Events

        private static void OnPostCommit(object sender, GitUIPostActionEventArgs e)
        {
            foreach (var plugin in Plugins)
            {
                if (plugin is IPostCommitHandler handler)
                {
                    handler.OnPostCommit(e);
                }
            }
        }

        private static void OnPostRepositoryChanged(object sender, GitUIEventArgs e)
        {
            foreach (var plugin in Plugins)
            {
                if (plugin is IPostRepositoryChangedHandler handler)
                {
                    handler.OnPostRepositoryChanged(e);
                }
            }
        }

        private static void OnPostSettings(object sender, GitUIPostActionEventArgs e)
        {
            foreach (var plugin in Plugins)
            {
                if (plugin is IPostSettingsHandler handler)
                {
                    handler.OnPostSettings(e);
                }
            }
        }

        private static void OnPostUpdateSubmodules(object sender, GitUIPostActionEventArgs e)
        {
            foreach (var plugin in Plugins)
            {
                if (plugin is IPostUpdateSubmodulesHandler handler)
                {
                    handler.OnPostUpdateSubmodules(e);
                }
            }
        }

        private static void OnPostBrowseInitialize(object sender, GitUIEventArgs e)
        {
            foreach (var plugin in Plugins)
            {
                if (plugin is IPostBrowseInitializeHandler handler)
                {
                    handler.OnPostBrowseInitialize(e);
                }
            }
        }

        private static void OnPostRegisterPlugin(object sender, GitUIEventArgs e)
        {
            foreach (var plugin in Plugins)
            {
                if (plugin is IPostRegisterPluginHandler handler)
                {
                    handler.OnPostRegisterPlugin(e);
                }
            }
        }

        private static void OnPreCommit(object sender, GitUIEventArgs e)
        {
            foreach (var plugin in Plugins)
            {
                if (plugin is IPreCommitHandler handler)
                {
                    handler.OnPreCommit(e);
                }
            }
        }

        #endregion Events
    }
}
