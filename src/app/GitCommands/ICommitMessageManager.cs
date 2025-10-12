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
}
