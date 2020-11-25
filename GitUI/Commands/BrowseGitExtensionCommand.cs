using System;
using GitUIPluginInterfaces;

namespace GitUI.Commands
{
    internal sealed class BrowseGitExtensionCommand : IGitExtensionCommand
    {
        private readonly GitUICommands _gitUICommands;
        private readonly string _filter;
        private readonly ObjectId _selectedCommit;

        public BrowseGitExtensionCommand(
            GitUICommands gitUICommands,
            string filter = "",
            ObjectId selectedCommit = null)
        {
            _gitUICommands = gitUICommands ?? throw new ArgumentNullException(nameof(gitUICommands));
            _filter = filter;
            _selectedCommit = selectedCommit;
        }

        public bool Execute()
        {
            return _gitUICommands
                .StartBrowseDialog(filter: _filter, selectedCommit: _selectedCommit);
        }
    }
}
