namespace GitExtensions.Extensibility.Git;

public enum DiffBranchStatus
{
    Unknown = 0,
    SameChange,
    OnlyAChange,
    OnlyBChange,

    // Concurrent changes, different in first(A) and second(B)
    UnequalChange
}
