using GitCommands;

namespace GitUI.Browsing
{
    internal interface ICanLatestSelectedRevision
    {
        GitRevision LatestSelectedRevision { get; }
    }
}
