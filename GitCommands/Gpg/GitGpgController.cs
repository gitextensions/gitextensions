using GitCommands.Config;
using GitCommands.Gpg;
using GitExtUtils;
using Microsoft.Extensions.Caching.Memory;

namespace GitCommands.Git.Gpg
{
    partial class GitGpgController : IGitGpgController
    {
        /// <inheritdoc/>
        public string GetDefaultKey() => GetModule().GetEffectiveGitSetting(SettingKeyString.UserSigningKey, false).Value;

        public async Task<IEnumerable<GpgKeyInfo>> GetGpgSecretKeysAsync()
        {
            if (KeyCache.Value.TryGetValue<IEnumerable<GpgKeyInfo>>(nameof(GetGpgSecretKeysAsync), out var cachedKeys))
            {
                return cachedKeys;
            }

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

            GpgKeyInfo[] result = output.DistinctBy(k => k.KeyID).ToArray();

            lock (KeyCache.Value)
            {
                KeyCache.Value.Set(nameof(GetGpgSecretKeysAsync), result, new MemoryCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3), Size = 1 });
            }

            await Task.Delay(10000);
            return result;
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

        public static void ClearKeyCache()
        {
            if (KeyCache.IsValueCreated)
            {
                lock (KeyCache.Value)
                {
                    var tmp = KeyCache.Value;
                    KeyCache = SetupKeyCache();
                    tmp.Dispose();
                }
            }
        }

        private static Lazy<MemoryCache> KeyCache { get; set; } = SetupKeyCache();
        private static Lazy<MemoryCache> SetupKeyCache() => new(() => new MemoryCache(new MemoryCacheOptions() { ExpirationScanFrequency = TimeSpan.FromMinutes(1.5), SizeLimit = 1 }));
    }
}
