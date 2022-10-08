using GitCommands.Config;
using GitCommands.Gpg;
using GitExtUtils;

namespace GitCommands.Git.Gpg
{
    partial class GitGpgController : IGitGpgController
    {
        /// <inheritdoc/>
        public string GetDefaultKey() => GetModule().GetEffectiveGitSetting(SettingKeyString.UserSigningKey, false).Value;

        public async Task<IEnumerable<GpgKeyInfo>> GetGpgSecretKeysAsync()
        {
            var args = new ArgumentBuilder()
            {
                "-K", // Secret Keys
                "--with-colons",
                "--keyid-format LONG"
            };

            string txt = await GetModule().GpgExecutable.GetOutputAsync(args);
            string? defaultKey = GetDefaultKey();
            IEnumerable<GpgKeyInfo> output = _parser.ParseKeysOutput(txt, defaultKey);

            // filter to only signing keys
            output = output.Where(c => c.Capabilities.HasFlag(Capabilities.Sign));

            // filter so that only valid and unknown validities show
            output = output.Where(c =>
                c.Validity == KeyValidity.Valid || c.Validity == KeyValidity.Unknown);

            // filter disabled keys
            output = output.Where(c => !c.Disabled);

            return output.DistinctBy(k => k.KeyID).ToArray();
        }

        /// <inheritdoc/>
        public bool AreTagsSignedByDefault()
        {
            string? output = GetModule().GetEffectiveGitSetting(SettingKeyString.TagGPGSign, false).Value;

            bool result = false;
            return bool.TryParse(output, out result) && result;
        }

        /// <inheritdoc/>
        public bool AreCommitSignedByDefault()
        {
            string? output = GetModule().GetEffectiveGitSetting(SettingKeyString.CommitGPGSign, false).Value;

            bool result = false;
            return bool.TryParse(output, out result) && result;
        }
    }
}
