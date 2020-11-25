using System;

namespace GitUI.Commands
{
    internal sealed class PushGitExtensionCommand : IGitExtensionCommand
    {
        private readonly GitUICommands _gitUICommands;
        private readonly bool _quiet;

        public PushGitExtensionCommand(
            GitUICommands gitUICommands,
            bool quiet)
        {
            _gitUICommands = gitUICommands ?? throw new ArgumentNullException(nameof(gitUICommands));
            _quiet = quiet;
        }

        public bool Execute()
        {
            return _gitUICommands
                .StartPushDialog(owner: null, _quiet);
        }
    }
}
