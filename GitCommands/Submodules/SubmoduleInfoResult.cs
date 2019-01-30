using System.Collections.Generic;
using JetBrains.Annotations;

namespace GitCommands.Submodules
{
    /// <summary>
    /// Complete set of gathered submodule information.
    /// </summary>
    public class SubmoduleInfoResult
    {
        public IList<SubmoduleInfo> OurSubmodules { get; } = new List<SubmoduleInfo>();
        public IList<SubmoduleInfo> SuperSubmodules { get; } = new List<SubmoduleInfo>();

        // Always set to the top-most module.
        [NotNull]
        public SubmoduleInfo TopProject { get; set; }

        // Set to the current module's parent, or null if current module is the top one.
        // If current module's parent is also the top-most module, then SuperProject == TopProject.
        [CanBeNull]
        public SubmoduleInfo SuperProject { get; set; }

        public string CurrentSubmoduleName { get; set; }
    }
}
