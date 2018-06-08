using System.Collections.Generic;
using GitUIPluginInterfaces;

namespace GitUI
{
    internal sealed class SuperProjectInfo
    {
        public string CurrentBranch;
        public string Conflict_Base;
        public string Conflict_Remote;
        public string Conflict_Local;
        public Dictionary<string, List<IGitRef>> Refs;
    }
}