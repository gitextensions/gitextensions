﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Linq;

namespace GitUI
{
    internal static class PluginLoader
    {
        private static readonly string[] ExcluedFiles = new[]
            {
                "Microsoft.WindowsAPICodePack.dll",
                "Microsoft.WindowsAPICodePack.Shell.dll",
                "git2.dll"
            };
        public static void Load()
        {
            lock (GitUI.Plugin.LoadedPlugins.Plugins)
            {
                if (GitUI.Plugin.LoadedPlugins.Plugins.Count > 0)
                    return;

                var file = new FileInfo(Application.ExecutablePath);

                // Only search for plugins in the plugins folder. This increases performance a little bit.
                // In DEBUG search for plugins in the root folder to make debugging plugins easier.
#if DEBUG
                var plugins = file.Directory.GetFiles("*.dll", SearchOption.AllDirectories);
#else
                FileInfo[] plugins =
                               Directory.Exists(Path.Combine(file.Directory.FullName, "Plugins"))
                                   ? new DirectoryInfo(Path.Combine(file.Directory.FullName, "Plugins")).GetFiles("*.dll")
                                   : new FileInfo[] { };
#endif

                var pluginFiles = plugins.Where(pluginFile => !ExcluedFiles.Contains(pluginFile.Name) || pluginFile.Name.StartsWith("Microsoft."));
                foreach (var pluginFile in pluginFiles)
                {
                    try
                    {
                        var types = Assembly.LoadFile(pluginFile.FullName).GetTypes();
                        PluginExtraction.ExtractPluginTypes(types);
                    }
                    catch (Exception ex)
                    {
                        string exInfo = "Exception info:\r\n";

                        var rtle = ex as ReflectionTypeLoadException;
                        if (rtle != null)
                        {
                            foreach (var el in rtle.LoaderExceptions)
                                exInfo += el.Message + "\r\n";
                        }
                        else
                        {
                            Action<Exception> getEx = null;
                            getEx = arg => { exInfo += arg.Message + "\r\n"; if (arg.InnerException != null) getEx(arg.InnerException); };
                            getEx(ex);
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