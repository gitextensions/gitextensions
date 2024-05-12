namespace GitExtensions.Extensibility.Git;

public enum DiffBranchStatus
{
    Unknown = 0,
    OnlyAChange,
    OnlyBChange,
    SameChange,

    // Concurrent changes, different in first(A) and second(B)
    UnequalChange
}
