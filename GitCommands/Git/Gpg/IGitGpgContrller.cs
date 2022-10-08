using GitUIPluginInterfaces;

namespace GitCommands.Git.Gpg
{
    public partial interface IGitGpgController
    {
        /// <summary>
        /// Obtain the commit verification message, coming from --pretty="format:%GG"
        /// </summary>
        /// <returns>Full string coming from GPG analysis on current revision.</returns>
        string GetCommitVerificationMessage(GitRevision revision);

        /// <summary>
        /// Obtain the commit signature status on current revision.
        /// </summary>
        /// <returns>Enum value that indicate the gpg status for current git revision.</returns>
        Task<CommitStatus> GetRevisionCommitSignatureStatusAsync(GitRevision revision);

        /// <summary>
        /// Obtain the tag status on current revision.
        /// </summary>
        /// <returns>
        /// Enum value that indicate if current git revision has one tag with good signature, one tag with bad signature or more
        /// than one tag.
        /// </returns>
        Task<TagStatus> GetRevisionTagSignatureStatusAsync(GitRevision revision);

        /// <summary>
        /// Obtain the tag verification message for all the tags in current git revision
        /// </summary>
        /// <returns>Full concatenated string coming from GPG analysis on all tags on current git revision.</returns>
        string? GetTagVerifyMessage(GitRevision revision);
    }
}
