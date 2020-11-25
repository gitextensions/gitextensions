using System;
using GitUI.CommandsDialogs;
using GitUIPluginInterfaces;

namespace GitUI.Commands
{
    internal sealed class FileHistoryGitExtensionCommand : IGitExtensionCommand
    {
        private const string FilterByRevisionArg = "--filter-by-revision";

        private readonly GitUICommands _gitUICommands;
        private readonly string _fileHistoryFileName;
        private readonly string _revision;
        private readonly string _filterByRevision;
        private readonly bool _showBlame;

        public FileHistoryGitExtensionCommand(
            GitUICommands gitUICommands,
            string fileHistoryFileName,
            string revision,
            string filterByRevision,
            bool showBlame)
        {
            _gitUICommands = gitUICommands ?? throw new ArgumentNullException(nameof(gitUICommands));
            _fileHistoryFileName = fileHistoryFileName;
            _revision = revision;
            _filterByRevision = filterByRevision;
            _showBlame = showBlame;
        }

        public bool Execute()
        {
            if (string.IsNullOrWhiteSpace(_fileHistoryFileName))
            {
                return false;
            }

            GitRevision revision = null;

            if (_revision != null)
            {
                if (!ObjectId.TryParse(_revision, out var objectId))
                {
                    return false;
                }

                revision = new GitRevision(objectId);
            }

            bool filterByRevision = false;

            if (_filterByRevision != null)
            {
                if (_filterByRevision != FilterByRevisionArg)
                {
                    return false;
                }

                filterByRevision = true;
            }

            _gitUICommands.ShowModelessForm(owner: null, requiresValidWorkingDir: true, preEvent: null, postEvent: null,
                () => new FormFileHistory(commands: _gitUICommands, _fileHistoryFileName, revision, filterByRevision, _showBlame));

            return true;
        }
    }
}
