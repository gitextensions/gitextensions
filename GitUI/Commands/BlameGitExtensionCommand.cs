using System;
using GitUI.CommandsDialogs;

namespace GitUI.Commands
{
    internal sealed class BlameGitExtensionCommand : IGitExtensionCommand
    {
        private readonly GitUICommands _gitUICommands;
        private readonly string _fileName;
        private readonly int? _initialLine;

        public BlameGitExtensionCommand(
            GitUICommands gitUICommands,
            string fileName,
            int? initialLine)
        {
            _gitUICommands = gitUICommands ?? throw new ArgumentNullException(nameof(gitUICommands));
            _fileName = fileName;
            _initialLine = initialLine;
        }

        public bool Execute()
        {
            return _gitUICommands
                .DoActionOnRepoWithNoChanges(action: () =>
                {
                    using var form = new FormBlame(_gitUICommands, _fileName, revision: null, _initialLine);

                    form.ShowDialog();

                    return true;
                });
        }
    }
}
