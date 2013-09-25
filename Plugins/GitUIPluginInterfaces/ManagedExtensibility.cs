using System.ComponentModel.Composition.Hosting;

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
                        var catalog = new DirectoryCatalog(@".\Plugins\", "*.dll");
                        compositionContainer = new CompositionContainer(catalog);
                    }

                    return compositionContainer;
                }
            }
        }
    }
}