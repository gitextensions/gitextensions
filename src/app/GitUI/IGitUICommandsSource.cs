using GitExtensions.Extensibility.Git;

namespace GitUI
{
    public sealed class GitUICommandsChangedEventArgs : EventArgs
    {
        public GitUICommandsChangedEventArgs(IGitUICommands? oldCommands)
        {
            OldCommands = oldCommands;
        }

        public IGitUICommands? OldCommands { get; }
    }

    /// <summary>Provides <see cref="IGitUICommands"/> and a change notification.</summary>
    public interface IGitUICommandsSource
    {
        /// <summary>Raised after <see cref="UICommands"/> changes.</summary>
        event EventHandler<GitUICommandsChangedEventArgs> UICommandsChanged;

        /// <summary>Gets the <see cref="IGitUICommands"/> value.</summary>
        /// <exception cref="InvalidOperationException">Attempting to get a value when none has been set.</exception>
        IGitUICommands UICommands { get; }
    }
}
