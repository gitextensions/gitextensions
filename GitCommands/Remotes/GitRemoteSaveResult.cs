namespace GitCommands.Remotes
{
    /// <summary>
    /// Represents a result of <see cref="GitRemoteManager.SaveRemote"/> operation.
    /// </summary>
    public class GitRemoteSaveResult
    {
        public GitRemoteSaveResult(string message, bool shouldUpdateRemote)
        {
            UserMessage = message;
            ShouldUpdateRemote = shouldUpdateRemote;
        }

        /// <summary>
        /// Indicates whether the "remote update" is desirable after the save operation.
        /// </summary>
        public bool ShouldUpdateRemote { get; }

        /// <summary>
        /// Gets the output of the save operation (if any).
        /// </summary>
        public string UserMessage { get; }
    }
}