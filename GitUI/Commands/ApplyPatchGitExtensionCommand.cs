using System;

namespace GitUI.Commands
{
    internal sealed class ApplyPatchGitExtensionCommand : IGitExtensionCommand
    {
        private readonly GitUICommands _gitUICommands;
        private readonly string _fileName;

        public ApplyPatchGitExtensionCommand(
            GitUICommands gitUICommands,
            string fileName = null)
        {
            _gitUICommands = gitUICommands ?? throw new ArgumentNullException(nameof(gitUICommands));
            _fileName = fileName;
        }

        public bool Execute()
        {
            return _gitUICommands
                .StartApplyPatchDialog(owner: null, _fileName);
        }
    }
}
