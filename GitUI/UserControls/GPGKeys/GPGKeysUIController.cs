using GitCommands.Git.Gpg;
using GitCommands.Git.Tag;

namespace GitUI.UserControls.GPGKeys
{
    public class GPGKeysUIController
    {
        private readonly IGitGpgController _gpgController;

        public GPGKeysUIController(IGitGpgController gpgController)
        {
            _gpgController = gpgController;
        }

        public GPGKeysUIController()
        {
            // just empty
        }

        public async Task<IEnumerable<GpgKeyDisplayInfo>> GetKeysAsync()
        {
            var keys = await _gpgController.GetGpgSecretKeysAsync();
            var displayKeys = keys.Select(k => new GpgKeyDisplayInfo(k.Fingerprint, k.KeyID, k.UserID, k.IsDefault))
                                  .OrderBy(k => k.UserId)
                                  .ThenBy(k => k.KeyId)
                                  .ToArray();
            return displayKeys;
        }

        public bool AreTagsSignedByDefault()
        {
            return _gpgController.AreTagsSignedByDefault();
        }

        /// <inheritdoc/>
        public bool AreCommitSignedByDefault()
        {
            return _gpgController.AreCommitSignedByDefault();
        }

        public bool ValidateTagSign(TagOperation tagOperation, string keyID)
        {
            return tagOperation switch
            {
                TagOperation.SignWithSpecificKey when string.IsNullOrWhiteSpace(keyID) => false,
                TagOperation.SignWithDefaultKey when string.IsNullOrWhiteSpace(keyID) => false,
                _ => true
            };
        }

        public bool ValidateCommitSign(bool signCommit, string keyID)
        {
            if (signCommit && string.IsNullOrWhiteSpace(keyID))
            {
                return false;
            }

            return true;
        }

        public string GetDefaultKey()
        {
            return _gpgController.GetDefaultKey();
        }

        public IGitGpgController GPGController { get => _gpgController; }
    }
}
