using System;

namespace GitUI.Commands
{
    internal sealed class MergeGitExtensionCommand : IGitExtensionCommand
    {
        private readonly GitUICommands _gitUICommands;
        private readonly string _branch;

        public MergeGitExtensionCommand(
            GitUICommands gitUICommands,
            string branch = null)
        {
            _gitUICommands = gitUICommands ?? throw new ArgumentNullException(nameof(gitUICommands));
            _branch = branch;
        }

        public bool Execute()
        {
            return _gitUICommands
                .StartMergeBranchDialog(owner: null, _branch);
        }
    }
}
