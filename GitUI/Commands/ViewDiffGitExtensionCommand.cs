using System;

namespace GitUI.Commands
{
    internal sealed class ViewDiffGitExtensionCommand : IGitExtensionCommand
    {
        private readonly GitUICommands _gitUICommands;

        public ViewDiffGitExtensionCommand(
            GitUICommands gitUICommands)
        {
            _gitUICommands = gitUICommands ?? throw new ArgumentNullException(nameof(gitUICommands));
        }

        public bool Execute()
        {
            return _gitUICommands
                .StartCompareRevisionsDialog();
        }
    }
}
