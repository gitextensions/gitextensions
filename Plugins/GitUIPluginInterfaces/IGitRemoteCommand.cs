using System;

namespace GitUIPluginInterfaces
{
    public interface IGitRemoteCommand
    {
        object OwnerForm { get; set; }
        string Remote { get; set; }
        string Title { get; set; }
        string CommandText { get; set; }
        bool ErrorOccurred { get; }
        string CommandOutput { get; }

        event EventHandler<GitRemoteCommandCompletedEventArgs> Completed;

        void Execute();
    }

    public class GitRemoteCommandCompletedEventArgs : EventArgs
    {
        public IGitRemoteCommand Command { get; }

        public bool IsError { get; set; }

        public bool Handled { get; set; }

        public GitRemoteCommandCompletedEventArgs(IGitRemoteCommand command, bool isError, bool handled)
        {
            Command = command;
            IsError = isError;
            Handled = handled;
        }
    }
}
