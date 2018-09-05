using System.Collections.Generic;

namespace GitCommands.Submodules
{
    /// <summary>
    /// Complete set of gathered submodule information.
    /// </summary>
    public class SubmoduleInfoResult
    {
        public IList<SubmoduleInfo> OurSubmodules { get; } = new List<SubmoduleInfo>();
        public IList<SubmoduleInfo> SuperSubmodules { get; } = new List<SubmoduleInfo>();
        public SubmoduleInfo TopProject { get; set; }
        public SubmoduleInfo SuperProject { get; set; }
        public string CurrentSubmoduleName { get; set; }
    }
}
