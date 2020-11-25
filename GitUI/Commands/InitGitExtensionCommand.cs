using System;

namespace GitUI.Commands
{
    internal sealed class InitGitExtensionCommand : IGitExtensionCommand
    {
        private readonly GitUICommands _gitUICommands;
        private readonly string _dir;

        public InitGitExtensionCommand(
            GitUICommands gitUICommands,
            string dir = null)
        {
            _gitUICommands = gitUICommands ?? throw new ArgumentNullException(nameof(gitUICommands));
            _dir = dir;
        }

        public bool Execute()
        {
            return _gitUICommands
                .StartInitializeDialog(dir: _dir);
        }
    }
}
