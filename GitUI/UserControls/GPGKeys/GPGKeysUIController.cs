using GitCommands.Git.Tag;
using GitCommands.Gpg;

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
                                  .OrderBy(k => k.UserID)
                                  .ThenBy(k => k.KeyID)
                                  .ToArray();
            return displayKeys;
        }

        public async Task<bool> GetTagGPGSignAsync()
        {
            return await _gpgController.GetTagGPGSignAsync();
        }

        /// <inheritdoc/>
        public Task<bool> GetCommitGPGSignAsync()
        {
            return _gpgController.GetCommitGPGSignAsync();
        }

        public async Task<bool> ValidateTagSignAsync(TagOperation tagOperation, string keyID)
        {
            string defaultKey = await _gpgController.GetDefaultKeyAsync();
            if ((tagOperation == TagOperation.SignWithSpecificKey) && string.IsNullOrWhiteSpace(keyID))
            {
                return false;
            }
            else if (tagOperation == TagOperation.SignWithDefaultKey && string.IsNullOrWhiteSpace(defaultKey))
            {
                return false;
            }

            return true;
        }

        public bool ValidateCommitSign(bool signCommit, string keyID)
        {
            if (signCommit && string.IsNullOrWhiteSpace(keyID))
            {
                return false;
            }

            return true;
        }

        public Task<string?> GetDefaultKeyAsync()
        {
            return _gpgController.GetDefaultKeyAsync();
        }

        public IGitGpgController GPGController { get => _gpgController; }
    }
}
