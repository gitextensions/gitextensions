using GitCommands;
using GitUI.Properties;

namespace GitUI.Avatars;

public static class AvatarService
{
    private static readonly InitialsAvatarProvider InitialsAvatarProvider;
    private static readonly StaticImageAvatarProvider UserImageAvatarProvider;
    private static readonly HotSwapAvatarProvider HotSwapProvider;

    static AvatarService()
    {
        InitialsAvatarProvider = new();
        UserImageAvatarProvider = new(AvatarImage.Encode(Images.User80));

        HotSwapProvider = new();
        (DefaultProvider, CacheCleaner) = SetupCachingAndFallback();
        UpdateAvatarProvider();
    }

    public static IAvatarProvider DefaultProvider { get; }

    public static IAvatarCacheCleaner CacheCleaner { get; }

    /// <summary>
    /// Updates the internal avatar provider chain to reflect the active settings.
    /// </summary>
    public static void UpdateAvatarProvider()
    {
        HotSwapProvider.Provider = CreateAvatarProvider(AppSettings.AvatarProvider, AppSettings.AvatarFallbackType);
    }

    /// <summary>
    /// Creates an avatar provider based on the given provider and fallback options.
    /// </summary>
    public static IAvatarProvider? CreateAvatarProvider(
        AvatarProvider provider,
        AvatarFallbackType? fallbackType,
        IAvatarDownloader? downloader = null)
    {
        Lazy<IAvatarDownloader> lazyDownloader = new(() => downloader ?? new AvatarDownloader());
        IAvatarProvider[] providers = [.. new[]
        {
            BuildMainProvider(),
            BuildFallbackProvider()
        }
        .WhereNotNull()];

        return providers.Length switch
        {
            0 => null,
            1 => providers[0],
            _ => new ChainedAvatarProvider(providers),
        };

        IAvatarProvider BuildDefaultMainProvider()
            => new ChainedAvatarProvider(
                new GithubAvatarProvider(lazyDownloader.Value),
                new GravatarProvider(lazyDownloader.Value, fallbackType));

        IAvatarProvider BuildFallbackProvider()
        {
            return fallbackType switch
            {
                AvatarFallbackType.AuthorInitials => InitialsAvatarProvider,
                null => InitialsAvatarProvider,
                AvatarFallbackType type when GravatarProvider.IsFallbackSupportedByGravatar(type) => new GravatarProvider(lazyDownloader.Value, type, true),
                _ => InitialsAvatarProvider,
            };
        }

        IAvatarProvider? BuildMainProvider()
        {
            return provider switch
            {
                AvatarProvider.Default => BuildDefaultMainProvider(),
                AvatarProvider.Custom => CustomAvatarProvider.ParseTemplateString(AppSettings.CustomAvatarTemplate, lazyDownloader.Value),
                _ => null,
            };
        }
    }

    public static void UpdateAvatarInitialFontsSettings()
    {
        InitialsAvatarProvider.UpdateFontsSettings();
    }

    private static (IAvatarProvider provider, IAvatarCacheCleaner cacheCleaner) SetupCachingAndFallback()
    {
        FileSystemAvatarCache persistentCacheProvider = new(HotSwapProvider);
        AvatarMemoryCache memoryCachedProvider = new(persistentCacheProvider, AppSettings.AvatarCacheSize);
        MultiCacheCleaner cacheCleaner = new(memoryCachedProvider, persistentCacheProvider);
        SafetynetAvatarProvider mainProvider = new(
            new ChainedAvatarProvider(
                memoryCachedProvider,
                UserImageAvatarProvider));

        return (mainProvider, cacheCleaner);
    }
}
