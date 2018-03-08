using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace GitUI
{
    public static class PluginLoader
    {
        public static void Load()
        {
            lock (Plugin.LoadedPlugins.Plugins)
            {
                if (Plugin.LoadedPlugins.Plugins.Count > 0)
                {
                    return;
                }

                var file = new FileInfo(Application.ExecutablePath);

                FileInfo[] plugins =
                               Directory.Exists(Path.Combine(file.Directory.FullName, "Plugins"))
                                   ? new DirectoryInfo(Path.Combine(file.Directory.FullName, "Plugins")).GetFiles("*.dll")
                                   : new FileInfo[] { };

                var pluginFiles = plugins.Where(pluginFile =>
                    !pluginFile.Name.StartsWith("System.") &&
                    !pluginFile.Name.StartsWith("ICSharpCode.") &&
                    !pluginFile.Name.StartsWith("Microsoft."));

                foreach (var pluginFile in pluginFiles)
                {
                    try
                    {
                        Debug.WriteLine("Loading plugin...", pluginFile.Name);
                        var types = Assembly.LoadFile(pluginFile.FullName).GetTypes();
                        PluginExtraction.ExtractPluginTypes(types);
                    }
                    catch (SystemException ex)
                    {
                        string exceptionInfo = "Exception info:\r\n";

                        if (ex is ReflectionTypeLoadException rtle)
                        {
                            foreach (var el in rtle.LoaderExceptions)
                            {
                                exceptionInfo += el.Message + "\r\n";
                            }
                        }
                        else
                        {
                            // Walk inner exceptions
                            Exception e = ex;
                            while (true)
                            {
                                exceptionInfo += e.Message + "\r\n";

                                if (e.InnerException == null)
                                {
                                    break;
                                }

                                e = e.InnerException;
                            }
                        }

                        MessageBox.Show(string.Format("Failed to load plugin {0} : \r\n{1}", pluginFile, exceptionInfo));
                        Trace.WriteLine(ex.Message);
                    }
                }
            }
        }
    }
}