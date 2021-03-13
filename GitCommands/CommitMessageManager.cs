using System;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using System.Windows.Forms;
using GitExtUtils;

namespace GitCommands
{
    public interface ICommitMessageManager
    {
        /// <summary>
        /// Reads/stores whether the previous commit shall be amended (if AppSettings.RememberAmendCommitState).
        /// </summary>
        bool AmendState { get; set; }

        /// <summary>
        /// The path of .git/COMMITMESSAGE, where a prepared (non-merge) commit message is stored.
        /// </summary>
        string CommitMessagePath { get; }

        /// <summary>
        /// Returns whether .git/MERGE_MSG exists.
        /// </summary>
        bool IsMergeCommit { get; }

        /// <summary>
        /// The path of .git/MERGE_MSG, where a merge-commit message is stored.
        /// </summary>
        string MergeMessagePath { get; }

        /// <summary>
        /// Reads/stores the prepared commit message from/in .git/MERGE_MSG if it exists or else in .git/COMMITMESSAGE.
        /// </summary>
        string MergeOrCommitMessage { get; set; }

        /// <summary>
        /// Deletes .git/COMMITMESSAGE and the file with the AmendState.
        /// </summary>
        void ResetCommitMessage();

        /// <summary>
        ///  Writes the provided commit message to .git/COMMITMESSAGE.
        ///  The message is formatted depending whether a commit template is used or whether the 2nd line must be empty.
        /// </summary>
        /// <param name="commitMessage">The commit message to write out.</param>
        /// <param name="messageType">The type of message to write out.</param>
        /// <param name="usingCommitTemplate">The indicator whether a commit template is used.</param>
        /// <param name="ensureCommitMessageSecondLineEmpty">The indicator whether empty second line is enforced.</param>
        void WriteCommitMessageToFile(string commitMessage, CommitMessageType messageType, bool usingCommitTemplate, bool ensureCommitMessageSecondLineEmpty);
    }

    public sealed class CommitMessageManager : ICommitMessageManager
    {
        private string CannotReadCommitMessage => "Cannot read commit message";
        private string CannotSaveCommitMessage => "Cannot save commit message";
        private string CannotReadAmendState => "Cannot read amend state";
        private string CannotSaveAmendState => "Cannot save amend state";
        private string CannotAccessFile => "Exception: \"{0}\" when accessing {1}";

        private readonly string _amendSaveStatePath;

        private readonly IFileSystem _fileSystem;

        // Commit messages are UTF-8 by default unless otherwise in the config file.
        // The git manual states:
        //  git commit and git commit-tree issues a warning if the commit log message
        //  given to it does not look like a valid UTF-8 string, unless you
        //  explicitly say your project uses a legacy encoding. The way to say
        //  this is to have i18n.commitencoding in .git/config file, like this:...
        private readonly Encoding _commitEncoding;

        private string? _overriddenCommitMessage;

        public CommitMessageManager(string workingDirGitDir, Encoding commitEncoding, string? overriddenCommitMessage = null)
            : this(workingDirGitDir, commitEncoding, new FileSystem(), overriddenCommitMessage)
        {
        }

        internal CommitMessageManager(string workingDirGitDir, Encoding commitEncoding, IFileSystem fileSystem, string? overriddenCommitMessage = null)
        {
            _fileSystem = fileSystem;
            _commitEncoding = commitEncoding;
            _amendSaveStatePath = GetFilePath(workingDirGitDir, "GitExtensions.amend");
            CommitMessagePath = GetFilePath(workingDirGitDir, "COMMITMESSAGE");
            MergeMessagePath = GetFilePath(workingDirGitDir, "MERGE_MSG");
            _overriddenCommitMessage = overriddenCommitMessage;
        }

        /// <inheritdoc/>
        public bool AmendState
        {
            get
            {
                bool amendState = false;

                if (AppSettings.RememberAmendCommitState)
                {
                    bool.TryParse(ReadFile(_amendSaveStatePath, CannotReadAmendState), out amendState);
                }

                return amendState;
            }
            set
            {
                if (AppSettings.RememberAmendCommitState && value)
                {
                    WriteFile(_amendSaveStatePath, CannotSaveAmendState, true.ToString());
                    return;
                }

                if (_fileSystem.File.Exists(_amendSaveStatePath))
                {
                    _fileSystem.File.Delete(_amendSaveStatePath);
                }
            }
        }

        /// <inheritdoc/>
        public string CommitMessagePath { get; }

        /// <inheritdoc/>
        public bool IsMergeCommit => _fileSystem.File.Exists(MergeMessagePath);

        /// <inheritdoc/>
        public string MergeMessagePath { get; }

        /// <inheritdoc/>
        public string MergeOrCommitMessage
        {
            get
            {
                if (_overriddenCommitMessage is not null)
                {
                    return _overriddenCommitMessage;
                }

                return ReadFile(GetMergeOrCommitMessagePath(), CannotReadCommitMessage, _commitEncoding);
            }
            set
            {
                var content = value ?? string.Empty;

                // do not remember commit message when they have been specified by the command line
                if (content != _overriddenCommitMessage)
                {
                    WriteFile(GetMergeOrCommitMessagePath(), CannotSaveCommitMessage, content, _commitEncoding);
                }
            }
        }

        /// <inheritdoc/>
        public void ResetCommitMessage()
        {
            _overriddenCommitMessage = null;
            _fileSystem.File.Delete(CommitMessagePath);
            _fileSystem.File.Delete(_amendSaveStatePath);
        }

        /// <inheritdoc/>
        public void WriteCommitMessageToFile(string commitMessage, CommitMessageType messageType, bool usingCommitTemplate, bool ensureCommitMessageSecondLineEmpty)
        {
            var formattedCommitMessage = FormatCommitMessage(commitMessage, usingCommitTemplate, ensureCommitMessageSecondLineEmpty);

            string path = messageType == CommitMessageType.Normal ? CommitMessagePath : MergeMessagePath;
            WriteFile(path, CannotSaveCommitMessage, formattedCommitMessage, _commitEncoding);
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

        private string ReadFile(string filePath, string errorTitle, Encoding? encoding = null)
        {
            try
            {
                if (!_fileSystem.File.Exists(filePath))
                {
                    return string.Empty;
                }

                return _fileSystem.File.ReadAllText(filePath, encoding ?? Encoding.Default);
            }
            catch (Exception ex)
            {
                MessageBox.Show(null, string.Format(CannotAccessFile, ex.Message, filePath), errorTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return string.Empty;
            }
        }

        private void WriteFile(string filePath, string errorTitle, string content, Encoding? encoding = null)
        {
            if (!_fileSystem.Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                // The repo no longer exists - ignore silently
                return;
            }

            try
            {
                _fileSystem.File.WriteAllText(filePath, content, encoding ?? Encoding.Default);
            }
            catch (Exception ex)
            {
                // No need to cancel the other operations in FormCommit - just let the user know that something went wrong
                MessageBox.Show(null, string.Format(CannotAccessFile, ex.Message, filePath), errorTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
