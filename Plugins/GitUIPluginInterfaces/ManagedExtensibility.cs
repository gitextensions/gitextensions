using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Windows.Forms;

namespace GitUIPluginInterfaces
{
    public class ManagedExtensibility
    {
        private static CompositionContainer compositionContainer;

        private static readonly object compositionContainerSyncObj = new object();

        /// <summary>
        /// The MEF container.
        /// </summary>
        public static CompositionContainer CompositionContainer
        {
            get
            {
                lock (compositionContainerSyncObj)
                {
                    if (compositionContainer == null)
                    {
                        var pluginsDir = new DirectoryInfo(Directory.GetParent(Application.ExecutablePath).FullName + Path.DirectorySeparatorChar + "Plugins");
                        var catalog = new DirectoryCatalog(pluginsDir.FullName, "*.dll");
                        compositionContainer = new CompositionContainer(catalog);
                    }

                    return compositionContainer;
                }
            }
        }
    }
}