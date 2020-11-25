using System;

namespace GitUI.Commands
{
    internal sealed class AddFilesGitExtensionCommand : IGitExtensionCommand
    {
        private readonly GitUICommands _gitUICommands;
        private readonly string _filter;

        public AddFilesGitExtensionCommand(
            GitUICommands gitUICommands,
            string filter = null)
        {
            _gitUICommands = gitUICommands ?? throw new ArgumentNullException(nameof(gitUICommands));
            _filter = filter;
        }

        public bool Execute()
        {
            return _gitUICommands
                .StartAddFilesDialog(owner: null, _filter);
        }
    }
}
