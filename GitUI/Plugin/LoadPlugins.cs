﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Linq;

namespace GitUI
{
    public static class PluginLoader
    {
        private static readonly string[] ExcluedFiles = new[]
            {
                "Microsoft.WindowsAPICodePack.dll",
                "Microsoft.WindowsAPICodePack.Shell.dll",
                "git2.dll"
            };
        public static void Load()
        {
            lock (Plugin.LoadedPlugins.Plugins)
            {
                if (Plugin.LoadedPlugins.Plugins.Count > 0)
                    return;

                var file = new FileInfo(Application.ExecutablePath);

                FileInfo[] plugins =
                               Directory.Exists(Path.Combine(file.Directory.FullName, "Plugins"))
                                   ? new DirectoryInfo(Path.Combine(file.Directory.FullName, "Plugins")).GetFiles("*.dll")
                                   : new FileInfo[] { };

                foreach (var pluginFile in plugins)
                {
                    try
                    {
                        var types = Assembly.LoadFile(pluginFile.FullName).GetTypes();
                        Debug.WriteLine("Loading plugin...", pluginFile.Name);
                        PluginExtraction.ExtractPluginTypes(types);
                    }
                    catch (SystemException ex)
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