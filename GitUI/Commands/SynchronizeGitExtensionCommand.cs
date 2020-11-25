using System;

namespace GitUI.Commands
{
    internal sealed class SynchronizeGitExtensionCommand : IGitExtensionCommand
    {
        private readonly IGitExtensionCommand _commitCommand;
        private readonly IGitExtensionCommand _pullCommand;
        private readonly IGitExtensionCommand _pushCommand;

        public SynchronizeGitExtensionCommand(
            IGitExtensionCommand commitCommand,
            IGitExtensionCommand pullCommand,
            IGitExtensionCommand pushCommand)
        {
            _commitCommand = commitCommand ?? throw new ArgumentNullException(nameof(commitCommand));
            _pullCommand = pullCommand ?? throw new ArgumentNullException(nameof(pullCommand));
            _pushCommand = pushCommand ?? throw new ArgumentNullException(nameof(pushCommand));
        }

        public bool Execute()
        {
            bool successful = true;

            successful = _commitCommand.Execute() && successful;
            successful = _pullCommand.Execute() && successful;
            successful = _pushCommand.Execute() && successful;

            return successful;
        }
    }
}
