namespace GitUI.Objects
{
    /// <summary>
    /// Represents a result of <see cref="GitRemoteController.SaveRemote"/> operation.
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
        public bool ShouldUpdateRemote { get; private set; }

        /// <summary>
        /// Gets the output of the save operation (if any).
        /// </summary>
        public string UserMessage { get; private set; }
    }
}