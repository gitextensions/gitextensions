using System.IO.Abstractions;
using System.Text;
using GitExtUtils;
using GitUI;
using Microsoft.VisualStudio.Threading;

namespace GitCommands
{
    public interface ICommitMessageManager
    {
        /// <summary>
        ///  The path of .git/COMMITMESSAGE, where a prepared (non-merge) commit message is stored.
        /// </summary>
        string CommitMessagePath { get; }

        /// <summary>
        ///  Returns whether .git/MERGE_MSG exists.
        /// </summary>
        bool IsMergeCommit { get; }

        /// <summary>
        ///  The path of .git/MERGE_MSG, where a merge-commit message is stored.
        /// </summary>
        string MergeMessagePath { get; }

        /// <summary>
        ///  Reads the indicator whether the previous commit shall be amended (if <see cref="AppSettings.RememberAmendCommitState"/>).
        /// </summary>
        Task<bool> GetAmendStateAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///  Reads/stores the prepared commit message from/in .git/MERGE_MSG if it exists or else in .git/COMMITMESSAGE.
        /// </summary>
        Task<string> GetMergeOrCommitMessageAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///  Deletes .git/COMMITMESSAGE and the file with the AmendState.
        /// </summary>
        Task ResetCommitMessageAsync();

        /// <summary>
        ///  Stores the indicator whether the previous commit shall be amended (if <see cref="AppSettings.RememberAmendCommitState"/>).
        /// </summary>
        Task SetAmendStateAsync(bool amendState, CancellationToken cancellationToken = default);

        /// <summary>
        ///  Stores the prepared commit message from/in .git/MERGE_MSG if it exists or else in .git/COMMITMESSAGE.
        /// </summary>
        Task SetMergeOrCommitMessageAsync(string? message, CancellationToken cancellationToken = default);

        /// <summary>
        ///  Writes the provided commit message to .git/COMMITMESSAGE.
        ///  The message is formatted depending whether a commit template is used or whether the 2nd line must be empty.
        /// </summary>
        /// <param name="commitMessage">The commit message to write out.</param>
        /// <param name="messageType">The type of message to write out.</param>
        /// <param name="usingCommitTemplate">The indicator whether a commit template is used.</param>
        /// <param name="ensureCommitMessageSecondLineEmpty">The indicator whether empty second line is enforced.</param>
        Task WriteCommitMessageToFileAsync(string commitMessage, CommitMessageType messageType, bool usingCommitTemplate,
                                           bool ensureCommitMessageSecondLineEmpty, CancellationToken cancellationToken = default);
    }

    public sealed class CommitMessageManager : ICommitMessageManager
    {
        private static string CannotReadCommitMessage => "Cannot read commit message";
        private static string CannotSaveCommitMessage => "Cannot save commit message";
        private static string CannotReadAmendState => "Cannot read amend state";
        private static string CannotSaveAmendState => "Cannot save amend state";
        private static string CannotAccessFile => "Exception: \"{0}\" when accessing {1}";

        private readonly string _amendSaveStatePath;

        private readonly IFileSystem _fileSystem;
        private readonly Control _owner;

        // Commit messages are UTF-8 by default unless otherwise in the config file.
        // The git manual states:
        //  git commit and git commit-tree issues a warning if the commit log message
        //  given to it does not look like a valid UTF-8 string, unless you
        //  explicitly say your project uses a legacy encoding. The way to say
        //  this is to have i18n.commitencoding in .git/config file, like this:...
        private readonly Encoding _commitEncoding;

        private string? _overriddenCommitMessage;

        public CommitMessageManager(Control owner, string workingDirGitDir, Encoding commitEncoding, string? overriddenCommitMessage = null)
            : this(owner, workingDirGitDir, commitEncoding, new FileSystem(), overriddenCommitMessage)
        {
        }

        internal CommitMessageManager(Control owner, string workingDirGitDir, Encoding commitEncoding, IFileSystem fileSystem, string? overriddenCommitMessage = null)
        {
            ArgumentNullException.ThrowIfNull(nameof(owner));

            _owner = owner;
            _fileSystem = fileSystem;
            _commitEncoding = commitEncoding;
            _amendSaveStatePath = GetFilePath(workingDirGitDir, "GitExtensions.amend");
            CommitMessagePath = GetFilePath(workingDirGitDir, "COMMITMESSAGE");
            MergeMessagePath = GetFilePath(workingDirGitDir, "MERGE_MSG");
            _overriddenCommitMessage = overriddenCommitMessage;
        }

        public async Task<bool> GetAmendStateAsync(CancellationToken cancellationToken = default)
        {
            bool amendState = false;

            if (AppSettings.RememberAmendCommitState)
            {
                string amendStateRawValue = await ReadFileAsync(_amendSaveStatePath, CannotReadAmendState, cancellationToken: cancellationToken);
                _ = bool.TryParse(amendStateRawValue, out amendState);
            }

            return amendState;
        }

        public async Task SetAmendStateAsync(bool amendState, CancellationToken cancellationToken = default)
        {
            await TaskScheduler.Default;

            if (AppSettings.RememberAmendCommitState && amendState)
            {
                await WriteFileAsync(_amendSaveStatePath, CannotSaveAmendState, true.ToString(), cancellationToken: cancellationToken);
                return;
            }

            if (_fileSystem.File.Exists(_amendSaveStatePath))
            {
                _fileSystem.File.Delete(_amendSaveStatePath);
            }
        }

        public string CommitMessagePath { get; }

        public bool IsMergeCommit => _fileSystem.File.Exists(MergeMessagePath);

        public string MergeMessagePath { get; }

        public async Task<string> GetMergeOrCommitMessageAsync(CancellationToken cancellationToken = default)
        {
            if (_overriddenCommitMessage is not null)
            {
                return _overriddenCommitMessage;
            }

            return await ReadFileAsync(GetMergeOrCommitMessagePath(), CannotReadCommitMessage, _commitEncoding, cancellationToken);
        }

        public async Task ResetCommitMessageAsync()
        {
            await TaskScheduler.Default;

            _overriddenCommitMessage = null;
            _fileSystem.File.Delete(CommitMessagePath);
            _fileSystem.File.Delete(_amendSaveStatePath);
        }

        public async Task SetMergeOrCommitMessageAsync(string? message, CancellationToken cancellationToken = default)
        {
            await TaskScheduler.Default;

            string content = message ?? string.Empty;

            // do not remember commit message when they have been specified by the command line
            if (content != _overriddenCommitMessage)
            {
                await WriteFileAsync(GetMergeOrCommitMessagePath(), CannotSaveCommitMessage, content, _commitEncoding, cancellationToken);
            }
        }

        public async Task WriteCommitMessageToFileAsync(string commitMessage, CommitMessageType messageType, bool usingCommitTemplate,
                                                        bool ensureCommitMessageSecondLineEmpty, CancellationToken cancellationToken = default)
        {
            await TaskScheduler.Default;

            string formattedCommitMessage = FormatCommitMessage(commitMessage, usingCommitTemplate, ensureCommitMessageSecondLineEmpty);

            string path = messageType == CommitMessageType.Normal ? CommitMessagePath : MergeMessagePath;
            await WriteFileAsync(path, CannotSaveCommitMessage, formattedCommitMessage, _commitEncoding, cancellationToken);
        }

        internal static string FormatCommitMessage(string commitMessage, bool usingCommitTemplate, bool ensureCommitMessageSecondLineEmpty)
        {
            if (string.IsNullOrEmpty(commitMessage))
            {
                return string.Empty;
            }

            StringBuilder formattedCommitMessage = new();

            var lineNumber = 1;
            foreach (var line in commitMessage.LazySplit('\n'))
            {
                // When a committemplate is used, skip comments and do not count them as line.
                // otherwise: "#" is probably not used for comment but for issue number
                if (usingCommitTemplate && line.StartsWith("#"))
                {
                    continue;
                }

                if (ensureCommitMessageSecondLineEmpty && lineNumber == 2 && !string.IsNullOrEmpty(line))
                {
                    formattedCommitMessage.AppendLine();
                }

                formattedCommitMessage.AppendLine(line);

                lineNumber++;
            }

            return formattedCommitMessage.ToString();
        }

        private string GetFilePath(string workingDirGitDir, string fileName) => _fileSystem.Path.Combine(workingDirGitDir, fileName);

        private string GetMergeOrCommitMessagePath() => IsMergeCommit ? MergeMessagePath : CommitMessagePath;

        private async Task<string> ReadFileAsync(string filePath, string errorTitle, Encoding? encoding = null, CancellationToken cancellationToken = default)
        {
            await TaskScheduler.Default;

            try
            {
                if (!_fileSystem.File.Exists(filePath))
                {
                    return string.Empty;
                }

                return await _fileSystem.File.ReadAllTextAsync(filePath, encoding ?? Encoding.Default, cancellationToken);
            }
            catch (Exception ex) when (ex is not (OperationCanceledException or ObjectDisposedException))
            {
                await _owner.SwitchToMainThreadAsync();
                MessageBox.Show(_owner, string.Format(CannotAccessFile, ex.Message, filePath), errorTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return string.Empty;
            }
        }

        private async Task WriteFileAsync(string filePath, string errorTitle, string content, Encoding? encoding = null, CancellationToken cancellationToken = default)
        {
            await TaskScheduler.Default;

            if (!_fileSystem.Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                // The repo no longer exists - ignore silently
                return;
            }

            try
            {
                await _fileSystem.File.WriteAllTextAsync(filePath, content, encoding ?? Encoding.Default, cancellationToken);
            }
            catch (Exception ex) when (ex is not (OperationCanceledException or ObjectDisposedException))
            {
                await _owner.SwitchToMainThreadAsync();

                // No need to cancel the other operations in FormCommit - just let the user know that something went wrong
                MessageBox.Show(_owner, string.Format(CannotAccessFile, ex.Message, filePath), errorTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
