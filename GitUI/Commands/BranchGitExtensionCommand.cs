using System;

namespace GitUI.Commands
{
    internal sealed class BranchGitExtensionCommand : IGitExtensionCommand
    {
        private readonly GitUICommands _gitUICommands;

        public BranchGitExtensionCommand(
            GitUICommands gitUICommands)
        {
            _gitUICommands = gitUICommands ?? throw new ArgumentNullException(nameof(gitUICommands));
        }

        public bool Execute()
        {
            return _gitUICommands
                .StartCreateBranchDialog();
        }
    }
}
