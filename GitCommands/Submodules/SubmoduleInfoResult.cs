using System.Collections.Generic;
using JetBrains.Annotations;

namespace GitCommands.Submodules
{
    /// <summary>
    /// Complete set of gathered submodule information.
    /// </summary>
    public class SubmoduleInfoResult
    {
        // List of SubmoduleInfo for all submodules (recursively) under current module.
        public IList<SubmoduleInfo> OurSubmodules { get; } = new List<SubmoduleInfo>();

        // List of SubmoduleInfo for all submodules under TopProject.
        // Only populated if current module is a submodule (i.e. is not TopProject)
        public IList<SubmoduleInfo> SuperSubmodules { get; } = new List<SubmoduleInfo>();

        // Always set to the top-most module.
        [NotNull]
        public SubmoduleInfo TopProject { get; internal set; }

        // Set to the current module's parent, or null if current module is the top one.
        // If current module's parent is also the top-most module, then SuperProject == TopProject.
        [CanBeNull]
        public SubmoduleInfo SuperProject { get; internal set; }

        // Name of current module, if it is a submodule. If current module is TopProject, will be null.
        [CanBeNull]
        public string CurrentSubmoduleName { get; internal set; }
    }
}
