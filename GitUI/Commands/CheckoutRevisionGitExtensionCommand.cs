using System;

namespace GitUI.Commands
{
    internal sealed class CheckoutRevisionGitExtensionCommand : IGitExtensionCommand
    {
        private readonly GitUICommands _gitUICommands;

        public CheckoutRevisionGitExtensionCommand(
            GitUICommands gitUICommands)
        {
            _gitUICommands = gitUICommands ?? throw new ArgumentNullException(nameof(gitUICommands));
        }

        public bool Execute()
        {
            return _gitUICommands
                .StartCheckoutRevisionDialog(owner: null);
        }
    }
}
