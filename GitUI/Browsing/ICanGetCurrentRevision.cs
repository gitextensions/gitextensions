using GitCommands;

namespace GitUI.Browsing
{
    internal interface ICanGetCurrentRevision
    {
        GitRevision GetCurrentRevision();
    }
}
