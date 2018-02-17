﻿﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using GitUI.Plugin;

namespace GitUI
{
    public static class PluginLoader
    {
        public static void Load()
        {
            lock (LoadedPlugins.Plugins)
            {
                if (LoadedPlugins.Plugins.Count > 0)
                    return;

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
                        string exInfo = "Exception info:\r\n";

                        if (ex is ReflectionTypeLoadException rtle)
                        {
                            foreach (var el in rtle.LoaderExceptions)
                                exInfo += el.Message + "\r\n";
                        }
                        else
                        {
                            void GetEx(Exception arg)
                            {
                                while (true)
                                {
                                    exInfo += arg.Message + "\r\n";
                                    if (arg.InnerException == null)
                                        break;
                                    arg = arg.InnerException;
                                }
                            }

                            GetEx(ex);
                        }

                        MessageBox.Show(string.Format("Failed to load plugin {0} : \r\n{1}", pluginFile, exInfo));
                        Trace.WriteLine(ex.Message);
                    }
                }
            }
        }

        public static bool RunningOnWindows()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    return true;
                default:
                    return false;
            }
        }
    }
}