using GitExtensions.Extensibility.Git;

namespace GitCommands.Submodules
{
    /// <summary>
    /// Complete set of gathered submodule information.
    /// </summary>
    public class SubmoduleInfoResult
    {
        // Module that was used to gather submodule info
        public IGitModule? Module { get; internal set; }

        // List of SubmoduleInfo for all submodules (recursively) under current module.
        public IList<SubmoduleInfo> OurSubmodules { get; } = new List<SubmoduleInfo>();

        // List of SubmoduleInfo for all submodules under TopProject.
        public IList<SubmoduleInfo> AllSubmodules { get; } = new List<SubmoduleInfo>();

        // Always set to the top-most module.
        public SubmoduleInfo? TopProject { get; internal set; }

        // Set to the current module's parent, or null if current module is the top one.
        // If current module's parent is also the top-most module, then SuperProject == TopProject.
        public SubmoduleInfo? SuperProject { get; internal set; }

        // Name of current module, if it is a submodule. If current module is TopProject, will be null.
        public string? CurrentSubmoduleName { get; internal set; }

        /// <summary>
        /// GitItemStatus for the current submodule
        /// </summary>
        public IReadOnlyList<GitItemStatus>? CurrentSubmoduleStatus { get; internal set; }
    }
}
