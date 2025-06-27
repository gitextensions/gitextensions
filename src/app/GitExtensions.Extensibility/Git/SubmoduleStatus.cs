namespace GitExtensions.Extensibility.Git;

public enum SubmoduleStatus
{
    Unknown = 0,
    NewSubmodule,
    RemovedSubmodule,
    FastForward,
    Rewind,
    NewerTime,
    OlderTime,
    SameTime
}
