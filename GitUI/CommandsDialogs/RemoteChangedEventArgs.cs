namespace GitUI.CommandsDialogs
{
    public class RemoteChangedEventArgs
    {
        public RemoteChangedEventArgs(string remoteName)
        {
            RemoteName = remoteName;
        }

        public string RemoteName { get; }
    }
}