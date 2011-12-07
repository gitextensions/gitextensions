using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Threading;
using GitCommands;

namespace GitExtensions
{
    internal static class PluginLoader
    {
        public static void LoadAsync()
        {
            // Create the thread object, passing in the Alpha.Beta method
            // via a ThreadStart delegate. This does not start the thread.
            var oThread = new Thread(Load);

            // Start the thread
            oThread.Start();

        }

        public static void Load()
        {
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

            foreach (var pluginFile in plugins)
            {
                if (pluginFile.FullName.Contains("Microsoft.WindowsAPICodePack"))
                {
                    continue;
                }
                
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