using System;

namespace GitUI
{
    /// <summary>Represents the method that handles the GitUICommandsChanged event of an <see cref="IGitUICommandsSource"/>.</summary>
    public delegate void GitUICommandsChangedEventHandler(IGitUICommandsSource sender, GitUICommands oldCommands);
    
    public class GitUICommandsSourceEventArgs : EventArgs
    {
        public GitUICommandsSourceEventArgs(IGitUICommandsSource gitUiCommandsSource)
        {
            GitUICommandsSource = gitUiCommandsSource;
        }

        public IGitUICommandsSource GitUICommandsSource { get; private set; }
    }

    /// <summary>Provides <see cref="GitUICommands"/> and a change notification.</summary>
    public interface IGitUICommandsSource
    {
        /// <summary>Raised after <see cref="UICommands"/> changes.</summary>
        event GitUICommandsChangedEventHandler GitUICommandsChanged;
        /// <summary>Gets the <see cref="GitUICommands"/> value.</summary>
        GitUICommands UICommands { get; }
    }
}
