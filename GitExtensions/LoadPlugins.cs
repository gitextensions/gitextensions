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

            //Only search for plugins in the plugins folder. This increases performance a little bit.
            //In DEBUG search for plugins in the root folder to make debugging plugins easier.
            #if DEBUG 
            FileInfo [] plugins = file.Directory.GetFiles("Plugins\\*.dll", SearchOption.AllDirectories);
            #else
            FileInfo [] plugins = file.Directory.GetFiles("*.dll", SearchOption.AllDirectories);
            #endif

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
