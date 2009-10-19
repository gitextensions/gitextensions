using System;
using System.Collections.Generic;
using System.Text;
using GitUIPluginInterfaces;

namespace GitUI
{
    public static class GitUIPluginCollection
    {
        private static readonly IList<IGitPlugin> plugins = new List<IGitPlugin>();
        public static IList<IGitPlugin> Plugins
        {
            get
            {
                return plugins;
            }
        }

    }
}
