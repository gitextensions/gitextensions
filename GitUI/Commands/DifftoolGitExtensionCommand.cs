using System;

namespace GitUI.Commands
{
    internal sealed class DifftoolGitExtensionCommand : IGitExtensionCommand
    {
        private readonly GitUICommands _gitUICommands;
        private readonly string _fileName;

        public DifftoolGitExtensionCommand(
            GitUICommands gitUICommands,
            string fileName)
        {
            _gitUICommands = gitUICommands ?? throw new ArgumentNullException(nameof(gitUICommands));
            _fileName = fileName;
        }

        public bool Execute()
        {
            return _gitUICommands.Module
                .OpenWithDifftool(_fileName) == string.Empty;
        }
    }
}
