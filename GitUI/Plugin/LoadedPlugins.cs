using System.Collections.Generic;
using GitUIPluginInterfaces;

namespace GitUI.Plugin
{
    public static class LoadedPlugins
    {
        static LoadedPlugins()
        {
            Plugins = new List<IGitPlugin>();
        }

        public static IList<IGitPlugin> Plugins { get; set; }
    }
}
