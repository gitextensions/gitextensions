namespace GitExtensions.Extensibility.Git;

public enum SubmoduleStatus
{
    Unknown = 0,
    Modified,
    NewSubmodule,
    RemovedSubmodule,
    SameCommit,
    FastForward,
    Rewind,
    NewerTime,
    OlderTime,
}
