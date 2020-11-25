using System;

namespace GitUI.Commands
{
    internal sealed class RebaseGitExtensionCommand : IGitExtensionCommand
    {
        private readonly GitUICommands _gitUICommands;
        private readonly string _onto;

        public RebaseGitExtensionCommand(
            GitUICommands gitUICommands,
            string onto)
        {
            _gitUICommands = gitUICommands ?? throw new ArgumentNullException(nameof(gitUICommands));
            _onto = onto;
        }

        public bool Execute()
        {
            return _gitUICommands
                .StartRebaseDialog(owner: null, _onto);
        }
    }
}
