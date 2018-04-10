using System;

namespace GitUI.CommandsDialogs
{
    public class RemoteRenamedEventArgs : EventArgs
    {
        public string OriginalName { get; }
        public string NewName { get; }

        public RemoteRenamedEventArgs(string originalName, string newName)
        {
            OriginalName = originalName;
            NewName = newName;
        }
    }
}