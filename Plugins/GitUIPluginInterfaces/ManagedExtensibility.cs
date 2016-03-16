using System.ComponentModel.Composition.Hosting;
using System.IO;

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
                        var catalog = new DirectoryCatalog("." + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar, "*.dll");
                        compositionContainer = new CompositionContainer(catalog);
                    }

                    return compositionContainer;
                }
            }
        }
    }
}