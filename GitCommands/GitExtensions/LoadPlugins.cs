using System;
using System.Collections.Generic;
using System.Text;
using GitUI;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using GitUIPluginInterfaces;

namespace GitExtensions
{
    static class PluginLoader
    {
        public static void Load()
        {
            FileInfo file = new FileInfo(Application.ExecutablePath);
            FileInfo [] plugins = file.Directory.GetFiles("*.dll", SearchOption.AllDirectories);

            foreach (FileInfo pluginFile in plugins)
            {
                try
                {
                    Type[] types = Assembly.LoadFile(pluginFile.FullName).GetTypes();
                    foreach (Type type in types)
                    {
                        if (typeof(IGitPlugin).IsAssignableFrom(type) && !type.IsInterface)
                        {
                            IGitPlugin gitPlugin = Activator.CreateInstance(type) as IGitPlugin;
                            if (gitPlugin != null)
                            {
                                gitPlugin.Settings = new GitPluginSettingsContainer(gitPlugin.Description);
                                gitPlugin.Register(GitUICommands.Instance);

                                GitUIPluginCollection.Plugins.Add(gitPlugin);
                            }
                        }
                    }
                }
                catch
                {
                }
            }
        }
    }
}
