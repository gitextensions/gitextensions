using System;

namespace GitUI.Commands
{
    internal sealed class MergeToolGitExtensionCommand : IGitExtensionCommand
    {
        private readonly GitUICommands _gitUICommands;
        private readonly bool _quiet;

        public MergeToolGitExtensionCommand(
            GitUICommands gitUICommands,
            bool quiet)
        {
            _gitUICommands = gitUICommands ?? throw new ArgumentNullException(nameof(gitUICommands));
            _quiet = quiet;
        }

        public bool Execute()
        {
            if (!_quiet || _gitUICommands.Module.InTheMiddleOfConflictedMerge())
            {
                return _gitUICommands
                    .StartResolveConflictsDialog();
            }

            return true;
        }
    }
}
