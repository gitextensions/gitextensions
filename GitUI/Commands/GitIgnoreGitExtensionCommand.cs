using System;

namespace GitUI.Commands
{
    internal sealed class GitIgnoreGitExtensionCommand : IGitExtensionCommand
    {
        private readonly GitUICommands _gitUICommands;

        public GitIgnoreGitExtensionCommand(
            GitUICommands gitUICommands)
        {
            _gitUICommands = gitUICommands ?? throw new ArgumentNullException(nameof(gitUICommands));
        }

        public bool Execute()
        {
            return _gitUICommands
                .StartEditGitIgnoreDialog(owner: null, localExcludes: false);
        }
    }
}
