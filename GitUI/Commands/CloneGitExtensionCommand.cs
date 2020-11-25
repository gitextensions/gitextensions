using System;

namespace GitUI.Commands
{
    internal sealed class CloneGitExtensionCommand : IGitExtensionCommand
    {
        private readonly GitUICommands _gitUICommands;
        private readonly string _url;
        private readonly bool _openedFromProtocolHandler;

        public CloneGitExtensionCommand(
            GitUICommands gitUICommands,
            string url = null,
            bool openedFromProtocolHandler = false)
        {
            _gitUICommands = gitUICommands ?? throw new ArgumentNullException(nameof(gitUICommands));
            _url = url;
            _openedFromProtocolHandler = openedFromProtocolHandler;
        }

        public bool Execute()
        {
            return _gitUICommands
                .StartCloneDialog(owner: null, _url, _openedFromProtocolHandler);
        }
    }
}
