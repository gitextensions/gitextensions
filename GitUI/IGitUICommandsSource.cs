using System;

namespace GitUI
{
    public sealed class GitUICommandsChangedEventArgs : EventArgs
    {
        public GitUICommandsChangedEventArgs(GitUICommands? oldCommands)
        {
            OldCommands = oldCommands;
        }

        public GitUICommands? OldCommands { get; }
    }

    /// <summary>Provides <see cref="GitUICommands"/> and a change notification.</summary>
    public interface IGitUICommandsSource
    {
        /// <summary>Raised after <see cref="UICommands"/> changes.</summary>
        event EventHandler<GitUICommandsChangedEventArgs> UICommandsChanged;

        /// <summary>Gets the <see cref="GitUICommands"/> value.</summary>
        /// <exception cref="InvalidOperationException">Attempting to get a value when none has been set.</exception>
        GitUICommands UICommands { get; }
    }
}
