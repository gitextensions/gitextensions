using System;

namespace GitUI.Commands
{
    internal sealed class SettingsGitExtensionCommand : IGitExtensionCommand
    {
        private readonly GitUICommands _gitUICommands;

        public SettingsGitExtensionCommand(
            GitUICommands gitUICommands)
        {
            _gitUICommands = gitUICommands ?? throw new ArgumentNullException(nameof(gitUICommands));
        }

        public bool Execute()
        {
            return _gitUICommands
                .StartSettingsDialog();
        }
    }
}
