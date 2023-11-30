using System.Diagnostics;
using GitExtUtils;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.RepositoryHosts;
using Microsoft;

namespace GitUI
{
    public static class PluginRegistry
    {
        public static IList<IGitPlugin> Plugins { get; } = new List<IGitPlugin>();

        public static List<IRepositoryHostPlugin> GitHosters { get; } = [];

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
                    IEnumerable<IGitPlugin> plugins = ManagedExtensibility.GetExports<IGitPlugin>()
                        .Select(lazy =>
                            {
                                try
                                {
                                    return lazy.Value;
                                }
                                catch (Exception ex)
                                {
                                    FailedPluginWrapper wrapper = new(ex);
                                    DebugHelpers.Fail($"{wrapper.Name}. Error: {ex.Demystify()}");
                                    return wrapper;
                                }
                            });

                    foreach (IGitPlugin plugin in plugins)
                    {
                        Validates.NotNull(plugin.Description);

                        // Description for old plugin setting processing as key
                        plugin.SettingsContainer = new GitPluginSettingsContainer(plugin.Id, plugin.Description);

                        if (plugin is IRepositoryHostPlugin repositoryHostPlugin)
                        {
                            GitHosters.Add(repositoryHostPlugin);
                        }

                        Plugins.Add(plugin);
                    }
                }
                catch (Exception ex)
                {
                    DebugHelpers.Fail($"Fail to load plugins. Error: {ex.Demystify()}");
                }
            }
        }

        public static IRepositoryHostPlugin? TryGetGitHosterForModule(IGitModule module)
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
