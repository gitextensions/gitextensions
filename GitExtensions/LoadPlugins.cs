using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using GitUI;
using GitUI.Plugin;
using GitUIPluginInterfaces;

namespace GitExtensions
{
    internal static class PluginLoader
    {
        public static void Load()
        {
            try
            {
                var file = new FileInfo(Application.ExecutablePath);

                // Only search for plugins in the plugins folder. This increases performance a little bit.
                // In DEBUG search for plugins in the root folder to make debugging plugins easier.
#if DEBUG
                var plugins = file.Directory.GetFiles("*.dll", SearchOption.AllDirectories);
#else
                var plugins =
                    Directory.Exists(Path.Combine(file.Directory.FullName, "Plugins"))
                        ? file.Directory.GetFiles("Plugins\\*.dll", SearchOption.AllDirectories)
                        : new FileInfo[] { };
#endif

                foreach (var pluginFile in plugins)
                {
                    try
                    {
                        var types = Assembly.LoadFile(pluginFile.FullName).GetTypes();
                        foreach (var type in types)
                        {
                            if (!typeof(IGitPlugin).IsAssignableFrom(type) || type.IsInterface)
                                continue;

                            var gitPlugin = Activator.CreateInstance(type) as IGitPlugin;
                            if (gitPlugin == null)
                                continue;

                            gitPlugin.Settings = new GitPluginSettingsContainer(gitPlugin.Description);
                            gitPlugin.Register(GitUICommands.Instance);

                            LoadedPlugins.Plugins.Add(gitPlugin);
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                    }
                }
            }
            catch
            {
            }
        }
    }
}