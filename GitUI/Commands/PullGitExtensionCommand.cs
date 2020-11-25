using System;

namespace GitUI.Commands
{
    internal sealed class PullGitExtensionCommand : IGitExtensionCommand
    {
        private readonly GitUICommands _gitUICommands;
        private readonly bool _quiet;
        private readonly string _remoteBranch;

        public PullGitExtensionCommand(
            GitUICommands gitUICommands,
            bool quiet,
            string remoteBranch)
        {
            _gitUICommands = gitUICommands ?? throw new ArgumentNullException(nameof(gitUICommands));
            _quiet = quiet;
            _remoteBranch = remoteBranch;
        }

        public bool Execute()
        {
            if (_quiet)
            {
                return _gitUICommands
                    .StartPullDialogAndPullImmediately(remoteBranch: _remoteBranch);
            }

            return _gitUICommands
                .StartPullDialog(remoteBranch: _remoteBranch);
        }
    }
}
