using System;

namespace GitUI
{
    public class GitUICommandsChangedEventArgs : EventArgs
    {
        public GitUICommandsChangedEventArgs(GitUICommands oldCommands)
        {
            OldCommands = oldCommands;
        }

        public GitUICommands OldCommands { get; }
    }

    public class GitUICommandsSourceEventArgs : EventArgs
    {
        public GitUICommandsSourceEventArgs(IGitUICommandsSource gitUiCommandsSource)
        {
            GitUICommandsSource = gitUiCommandsSource;
        }

        public IGitUICommandsSource GitUICommandsSource { get; }
    }

    /// <summary>Provides <see cref="GitUICommands"/> and a change notification.</summary>
    public interface IGitUICommandsSource
    {
        /// <summary>Raised after <see cref="UICommands"/> changes.</summary>
        event EventHandler<GitUICommandsChangedEventArgs> UICommandsChanged;

        /// <summary>Gets the <see cref="GitUICommands"/> value.</summary>
        GitUICommands UICommands { get; }
    }
}
