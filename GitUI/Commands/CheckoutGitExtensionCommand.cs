using System;

namespace GitUI.Commands
{
    internal sealed class CheckoutGitExtensionCommand : IGitExtensionCommand
    {
        private readonly GitUICommands _gitUICommands;

        public CheckoutGitExtensionCommand(
            GitUICommands gitUICommands)
        {
            _gitUICommands = gitUICommands ?? throw new ArgumentNullException(nameof(gitUICommands));
        }

        public bool Execute()
        {
            return _gitUICommands
                .StartCheckoutBranch(owner: null);
        }
    }
}
