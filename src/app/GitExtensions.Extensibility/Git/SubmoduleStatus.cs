namespace GitExtensions.Extensibility.Git;

public enum SubmoduleStatus
{
    Unknown = 0, // TODO add Modified, basically used as Unknown is now (when there is no information to find details)
    NewSubmodule,
    RemovedSubmodule,
    FastForward,
    Rewind,
    NewerTime,
    OlderTime,
    SameTime // TODO rename to SameCommit
}
