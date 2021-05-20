using System;
using System.Linq;
using GitCommands;
using GitUI.Properties;

namespace GitUI.Avatars
{
    public static class AvatarService
    {
        private static readonly InitialsAvatarProvider InitialsAvatarProvider;
        private static readonly StaticImageAvatarProvider UserImageAvatarProvider;

        private static readonly HotSwapAvatarProvider HotSwapProvider;

        static AvatarService()
        {
            InitialsAvatarProvider = new();
            UserImageAvatarProvider = new(Images.User80);

            HotSwapProvider = new();
            (DefaultProvider, CacheCleaner) = SetupCachingAndFallback();

            UpdateAvatarProvider();
        }

        public static IAvatarProvider DefaultProvider { get; }
        public static IAvatarCacheCleaner CacheCleaner { get; }

        /// <summary>
        /// Updates the internal avatar provider chain to
        /// reflect the current active (according to <see cref="AppSettings"/> provider.
        /// </summary>
        public static void UpdateAvatarProvider()
        {
            HotSwapProvider.Provider = CreateAvatarProvider(AppSettings.AvatarProvider, AppSettings.AvatarFallbackType);
        }

        /// <summary>
        /// Creates an <see cref="IAvatarProvider"/> based on the given
        /// <see cref="AvatarProvider"/> and <see cref="AvatarFallbackType"/> enum options.
        /// </summary>
        /// <returns>Returns an avatar provider that can be used to resolve user avatars.</returns>
        public static IAvatarProvider? CreateAvatarProvider(
            AvatarProvider provider,
            AvatarFallbackType? fallbackType,
            IAvatarDownloader? downloader = null)
        {
            // initialize download only if needed (some options, like local providers, don't need a downloader)
            // and use the downloader provided as parameter if possible.
            Lazy<IAvatarDownloader> lazyDownloader = new(() => downloader ?? new AvatarDownloader());

            // build collection of (non-null) providers
            var providers = new[]
            {
                BuildMainProvider(),
                BuildFallbackProvider()
            }
            .WhereNotNull()
            .ToArray();

            // only create chained avatar overhead if really needed
            return providers.Length switch
            {
                0 => null,
                1 => providers[0],
                _ => new ChainedAvatarProvider(providers),
            };

            // Local methods to build requested main and fallback providers:

            IAvatarProvider? BuildMainProvider()
            {
                return provider switch
                {
                    AvatarProvider.Default => BuildDefaultMainProvider(),
                    AvatarProvider.Custom => CustomAvatarProvider.ParseTemplateString(AppSettings.CustomAvatarTemplate, lazyDownloader.Value),
                    _ => null,
                };
            }

            IAvatarProvider BuildDefaultMainProvider()
            {
                // 1. query GitHub (with non-reply and regular email addresses)
                //    GitHub might internally fall back to Gravatar, so only need a single request in most cases.

                // 2. resolve via Gravatar (for users that don't have a GitHub account)
                //    this request also directly provides the fallback if it's Gravatar compatible.

                return new ChainedAvatarProvider(
                    new GithubAvatarProvider(lazyDownloader.Value),
                    new GravatarProvider(lazyDownloader.Value, fallbackType));
            }

            IAvatarProvider BuildFallbackProvider()
            {
                return fallbackType switch
                {
                    // Local fallback provider
                    AvatarFallbackType.AuthorInitials => InitialsAvatarProvider,

                    null => InitialsAvatarProvider,

                    // Use Gravatar as fallback provider
                    var type when GravatarProvider.IsFallbackSupportedByGravatar(fallbackType.Value) => new GravatarProvider(lazyDownloader.Value, type, true),

                    // Use author initials for unknown types
                    _ => InitialsAvatarProvider,
                };
            }
        }

        private static (IAvatarProvider provider, IAvatarCacheCleaner cacheCleaner) SetupCachingAndFallback()
        {
            FileSystemAvatarCache persistentCacheProvider = new(HotSwapProvider);
            AvatarMemoryCache memoryCachedProvider = new(persistentCacheProvider);

            MultiCacheCleaner cacheCleaner = new(persistentCacheProvider, memoryCachedProvider);

            SafetynetAvatarProvider mainProvider
                = new(
                    new ChainedAvatarProvider(
                        memoryCachedProvider,
                        UserImageAvatarProvider));

            return (mainProvider, cacheCleaner);
        }
    }
}
