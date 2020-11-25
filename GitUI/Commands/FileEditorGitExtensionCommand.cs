using System;

namespace GitUI.Commands
{
    internal sealed class FileEditorGitExtensionCommand : IGitExtensionCommand
    {
        private readonly GitUICommands _gitUICommands;
        private readonly string _fileName;

        public FileEditorGitExtensionCommand(
            GitUICommands gitUICommands,
            string fileName)
        {
            _gitUICommands = gitUICommands ?? throw new ArgumentNullException(nameof(gitUICommands));
            _fileName = fileName;
        }

        public bool Execute()
        {
            return _gitUICommands
                .StartFileEditorDialog(_fileName);
        }
    }
}
