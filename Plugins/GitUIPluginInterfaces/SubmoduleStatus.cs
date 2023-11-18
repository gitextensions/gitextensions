namespace GitUIPluginInterfaces
{
    public enum SubmoduleStatus
    {
        Unknown = 0,
        NewSubmodule,
        FastForward,
        Rewind,
        NewerTime,
        OlderTime,
        SameTime
    }
}
