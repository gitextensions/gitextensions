using GitCommands.Git;

namespace GitCommands.Submodules
{
    public class DetailedSubmoduleInfo
    {
        public bool IsDirty { get; set; }
        public SubmoduleStatus? Status
            => RawStatus?.Status ?? SubmoduleStatus.Unknown;
        public string? AddedAndRemovedText
            => RawStatus?.AddedAndRemovedString() ?? string.Empty;
        public GitSubmoduleStatus? RawStatus { get; set; }
    }
}
