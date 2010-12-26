using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitUIPluginInterfaces;
using GitUI.Plugin;

namespace GitUI.RepoHosting
{
    public class RepoHosts
    {
        public static List<IGitHostingPlugin> GitHosters { get; private set; }
        static RepoHosts()
        {
            GitHosters = new List<IGitHostingPlugin>();
        }
    }
}
