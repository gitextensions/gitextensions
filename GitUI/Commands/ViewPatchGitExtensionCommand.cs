using System;

namespace GitUI.Commands
{
    internal sealed class ViewPatchGitExtensionCommand : IGitExtensionCommand
    {
        private readonly GitUICommands _gitUICommands;
        private readonly string _patchFile;

        public ViewPatchGitExtensionCommand(
            GitUICommands gitUICommands,
            string patchFile = null)
        {
            _gitUICommands = gitUICommands ?? throw new ArgumentNullException(nameof(gitUICommands));
            _patchFile = patchFile;
        }

        public bool Execute()
        {
            return _gitUICommands
                .StartViewPatchDialog(_patchFile);
        }
    }
}
