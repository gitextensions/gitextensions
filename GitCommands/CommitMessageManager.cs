using System;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace GitCommands
{
    public interface ICommitMessageManager
    {
        /// <summary>
        /// Reads/stores whether the previous commit shall be amended (if AppSettings.RememberAmendCommitState).
        /// </summary>
        bool AmendState { get; set; }

        /// <summary>
        /// The pathname of the file where a prepared (non-merge) commit message is stored.
        /// </summary>
        string CommitMessagePath { get; }

        /// <summary>
        /// Returns whether .git/MERGE_MSG exists.
        /// </summary>
        bool IsMergeCommit { get; }

        /// <summary>
        /// Reads/stores the prepared commit message from/in .git/MERGE_MSG if it exists or else in .git/COMMITMESSAGE.
        /// </summary>
        string MergeOrCommitMessage { get; set; }

        /// <summary>
        /// Deletes .git/COMMITMESSAGE and the file with the AmendState.
        /// </summary>
        void ResetCommitMessage();
    }

    public sealed class CommitMessageManager : ICommitMessageManager
    {
        private string CannotReadCommitMessage => "Cannot read commit message";
        private string CannotSaveCommitMessage => "Cannot save commit message";
        private string CannotAccessFile => "Exception: \"{0}\" when accessing {1}";

        private readonly string _amendSaveStatePath;
        private readonly string _commitMessagePath;
        private readonly string _mergeMessagePath;

        private Encoding _commitEncoding;
        private IFileSystem _fileSystem;

        [CanBeNull]
        private string _overriddenCommitMessage;

        public CommitMessageManager(string workingDirGitDir, Encoding commitEncoding, string overriddenCommitMessage = null)
            : this(workingDirGitDir, commitEncoding, new FileSystem(), overriddenCommitMessage)
        {
        }

        internal CommitMessageManager(string workingDirGitDir, Encoding commitEncoding, IFileSystem fileSystem, string overriddenCommitMessage = null)
        {
            _fileSystem = fileSystem;
            _commitEncoding = commitEncoding;
            _amendSaveStatePath = GetFilePath(workingDirGitDir, "GitExtensions.amend");
            _commitMessagePath = GetFilePath(workingDirGitDir, "COMMITMESSAGE");
            _mergeMessagePath = GetFilePath(workingDirGitDir, "MERGE_MSG");
            _overriddenCommitMessage = overriddenCommitMessage;
        }

        public bool AmendState
        {
            get
            {
                bool amendState = false;

                if (AppSettings.RememberAmendCommitState && _fileSystem.File.Exists(_amendSaveStatePath))
                {
                    bool.TryParse(_fileSystem.File.ReadAllText(_amendSaveStatePath), out amendState);
                }

                return amendState;
            }
            set
            {
                if (AppSettings.RememberAmendCommitState && value)
                {
                    _fileSystem.File.WriteAllText(_amendSaveStatePath, true.ToString());
                }
                else
                {
                    _fileSystem.File.Delete(_amendSaveStatePath);
                }
            }
        }

        public string CommitMessagePath => _commitMessagePath;

        public bool IsMergeCommit => _fileSystem.File.Exists(_mergeMessagePath);

        public string MergeOrCommitMessage
        {
            get
            {
                if (_overriddenCommitMessage != null)
                {
                    return _overriddenCommitMessage;
                }

                var (file, exists) = GetMergeOrCommitMessagePath();
                string result;
                try
                {
                    result = exists ? _fileSystem.File.ReadAllText(file, _commitEncoding) : string.Empty;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(null, string.Format(CannotAccessFile, ex.Message, file),
                        CannotReadCommitMessage, MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    result = string.Empty;
                }

                return result;
            }
            set
            {
                var content = value ?? string.Empty;

                // do not remember commit message when they have been specified by the command line
                if (content != _overriddenCommitMessage)
                {
                    var path = GetMergeOrCommitMessagePath().filePath;
                    if (!_fileSystem.Directory.Exists(Path.GetDirectoryName(path)))
                    {
                        // The repo no longer exists
                        return;
                    }

                    try
                    {
                        _fileSystem.File.WriteAllText(path, content, _commitEncoding);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(null, string.Format(CannotAccessFile, ex.Message, path),
                            CannotSaveCommitMessage, MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);
                    }
                }
            }
        }

        public void ResetCommitMessage()
        {
            _overriddenCommitMessage = null;
            _fileSystem.File.Delete(_commitMessagePath);
            _fileSystem.File.Delete(_amendSaveStatePath);
        }

        private string GetFilePath(string workingDirGitDir, string fileName) => _fileSystem.Path.Combine(workingDirGitDir, fileName);

        private (string filePath, bool fileExists) GetMergeOrCommitMessagePath()
        {
            if (IsMergeCommit)
            {
                return (_mergeMessagePath, fileExists: true);
            }

            return (_commitMessagePath, _fileSystem.File.Exists(_commitMessagePath));
        }
    }
}
