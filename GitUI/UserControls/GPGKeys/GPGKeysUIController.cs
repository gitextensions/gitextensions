using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitCommands.Gpg;
using ResourceManager;

namespace GitUI.UserControls.GPGKeys
{
    public class GPGKeysUIController
    {
        private IGitGpgController _gpgController;

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
            var keys = await _gpgController.GetGpgSecretKeys();
            var displayKeys = keys.Select(k => new GpgKeyDisplayInfo(k.Fingerprint, k.KeyID, k.UserID)).ToArray();
            return displayKeys;
        }
    }

    public class GpgKeyDisplayInfo
    {
        private readonly TranslationString _noKeySelected = new TranslationString("No key selected");

        public GpgKeyDisplayInfo(string fingerprint, string keyID, string userID)
        {
            Fingerprint = fingerprint;
            KeyID = keyID;
            UserID = userID;
        }

        public string Fingerprint { get; set; }

        public string KeyID { get; set; }

        public string UserID { get; set; }

        public string Caption { get => string.IsNullOrEmpty(UserID) ? _noKeySelected.Text : $"{UserID} ({KeyID})"; }

        public override string ToString() => Caption;
    }
}
