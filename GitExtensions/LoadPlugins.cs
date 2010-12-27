using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using GitUI;
using GitUI.Plugin;
using GitUIPluginInterfaces;
using System.Threading;
using GitUI.RepoHosting;

namespace GitExtensions
{
    internal static class PluginLoader
    {
        public static void LoadAsync()
        {
            // Create the thread object, passing in the Alpha.Beta method
            // via a ThreadStart delegate. This does not start the thread.
            Thread oThread = new Thread(new ThreadStart(Load));

            // Start the thread
            oThread.Start();

        }

        public static void Load()
        {
            //Github.GithubPlugin ghp = new Github.GithubPlugin();

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

                        if (gitPlugin is IGitHostingPlugin)
                            RepoHosts.GitHosters.Add(gitPlugin as IGitHostingPlugin);
                          
                        LoadedPlugins.Plugins.Add(gitPlugin);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Failed to load plugin {0} : \r\n{1}", pluginFile, ex.Message));
                    Trace.WriteLine(ex.Message);
                }
            }
        }
    }
}