using System.Collections.Generic;

namespace GitCommands.Submodules
{
    /// <summary>Complete set of gathered submodule information.</summary>
    public class SubmoduleInfoResult
    {
        public readonly List<SubmoduleInfo> OurSubmodules = new List<SubmoduleInfo>();
        public readonly List<SubmoduleInfo> SuperSubmodules = new List<SubmoduleInfo>();
        public SubmoduleInfo TopProject;
        public SubmoduleInfo Superproject;
        public string CurrentSubmoduleName;
    }
}
