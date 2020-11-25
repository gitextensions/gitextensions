using System;

namespace GitUI.Commands
{
    internal sealed class CommitGitExtensionCommand : IGitExtensionCommand
    {
        private readonly GitUICommands _gitUICommands;
        private readonly string _commitMessage;
        private readonly bool _showOnlyWhenChanges;

        public CommitGitExtensionCommand(
            GitUICommands gitUICommands,
            string commitMessage = null,
            bool showOnlyWhenChanges = false)
        {
            _gitUICommands = gitUICommands ?? throw new ArgumentNullException(nameof(gitUICommands));
            _commitMessage = commitMessage;
            _showOnlyWhenChanges = showOnlyWhenChanges;
        }

        public bool Execute()
        {
            if (_gitUICommands.Module.IsBareRepository())
            {
                return false;
            }

            return _gitUICommands
                .StartCommitDialog(owner: null, _commitMessage, _showOnlyWhenChanges);
        }
    }
}
