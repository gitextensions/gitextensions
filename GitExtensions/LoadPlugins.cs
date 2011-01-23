using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Threading;

namespace GitExtensions
{
    internal static class PluginLoader
    {
        private static readonly PluginExtraction extractor = new PluginExtraction();
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
                    extractor.ExtractPluginTypes(types);
                }
                catch (Exception ex)
                {
                    string exInfo = "Exception info:\r\n";

                    var rtle = ex as ReflectionTypeLoadException;
                    if (rtle != null)
                    {
                        exInfo = rtle.LoaderExceptions.Aggregate(exInfo, (current, el) => current + (el.Message + "\r\n"));
                    }
                    else
                    {
                        Action<Exception> getEx = null;
                        Action<Exception> localGetEx = getEx;
                        getEx = arg =>
                                    {
                                        exInfo += arg.Message + "\r\n";
                                        if (arg.InnerException != null)
                                        {
                                            if (localGetEx != null)
                                            {
                                                localGetEx(arg.InnerException);
                                            }
                                        }
                                    };
                        getEx(ex);
                    }

                    MessageBox.Show(string.Format("Failed to load plugin {0} : \r\n{1}", pluginFile, exInfo));
                    Trace.WriteLine(ex.Message);
                }
            }
        }
    }
}