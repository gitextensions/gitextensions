using GitCommands.Gpg;

namespace GitCommands.Git.Gpg
{
    public partial interface IGitGpgController
    {
        /// <summary>
        /// Gets whether commits are signed by default
        /// </summary>
        /// <returns>true for default to signed commits</returns>
        bool AreCommitSignedByDefault();

        /// <summary>
        /// Gets the user's default signing key from git config
        /// </summary>
        /// <returns>The key id of the gpg key to sign with.</returns>
        string GetDefaultKey();

        Task<IEnumerable<GpgKeyInfo>> GetGpgSecretKeysAsync();

        /// <summary>
        /// Gets whether tags are signed by default
        /// </summary>
        /// <returns>true for default to signed tags</returns>
        bool AreTagsSignedByDefault();
    }
}
