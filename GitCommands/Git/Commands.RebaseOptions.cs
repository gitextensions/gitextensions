namespace GitCommands.Git;

public static partial class Commands
{
    public record RebaseOptions
    {
        public string? BranchName { get; set; }
        public string? From { get; set; }
        public string? OnTo { get; set; }

        // Common options
        public bool Interactive { get; init; }
        public bool PreserveMerges { get; init; }
        public bool AutoSquash { get; init; }
        public bool AutoStash { get; init; }
        public bool IgnoreDate { get; init; }
        public bool CommitterDateIsAuthorDate { get; init; }
        public bool? UpdateRefs { get; init; }
        public bool SupportRebaseMerges { get; init; } = true;
    }
}
