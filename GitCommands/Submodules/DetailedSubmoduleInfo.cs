using GitCommands.Git;

namespace GitCommands.Submodules
{
    public class DetailedSubmoduleInfo
    {
        public bool IsDirty { get; set; }
        public SubmoduleStatus? Status { get; set; }
        public string AddedAndRemovedText { get; set; }
    }
}
