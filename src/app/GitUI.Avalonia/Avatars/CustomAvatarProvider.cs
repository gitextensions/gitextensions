using GitCommands;
using GitExtensions.Extensibility;

namespace GitUI.Avatars;

/// <summary>
/// The custom avatar provider allows the user to define one or more custom providers that
/// are queried in order until an avatar image is found.
/// </summary>
public sealed partial class CustomAvatarProvider : IAvatarProvider
{
    private readonly IAvatarDownloader _downloader;
    private readonly IAvatarProvider[] _subProvider;

    private CustomAvatarProvider(IAvatarDownloader downloader, IAvatarProvider[] subProvider)
    {
        _downloader = downloader ?? throw new ArgumentNullException(nameof(downloader));
        _subProvider = subProvider ?? throw new ArgumentNullException(nameof(subProvider));
    }

    public bool PerformsIo => true;

    /// <inheritdoc/>
    public async Task<byte[]?> GetAvatarAsync(string email, string? name, int imageSize)
    {
        UriTemplateData templateData = new(email, name, imageSize);

        foreach (IAvatarProvider provider in _subProvider)
        {
            byte[]? avatar = provider switch
            {
                UriTemplateResolver resolver => await _downloader.DownloadImageAsync(resolver.ResolveTemplate(templateData)),
                _ => await provider.GetAvatarAsync(email, name, imageSize),
            };

            if (avatar is not null)
            {
                return avatar;
            }
        }

        return null;
    }

    /// <summary>
    /// Parses a custom avatar template string and creates an avatar provider from it.
    /// </summary>
    public static IAvatarProvider ParseTemplateString(string customProviderTemplates, IAvatarDownloader downloader)
    {
        ArgumentNullException.ThrowIfNull(downloader);

        IAvatarProvider[] providerParts = [.. customProviderTemplates
            .LazySplit(';')
            .Select(p => p.Trim())
            .Select(p => FromTemplateSegment(downloader, p))
            .WhereNotNull()];

        return new CustomAvatarProvider(downloader, providerParts);
    }

    private static IAvatarProvider? FromTemplateSegment(IAvatarDownloader downloader, string providerTemplate)
    {
        if (providerTemplate.StartsWith('<') && providerTemplate.EndsWith('>'))
        {
            string providerName = providerTemplate[1..^1];

            if (Enum.TryParse(providerName, true, out AvatarProvider provider)
                && provider != AvatarProvider.Custom)
            {
                return AvatarService.CreateAvatarProvider(provider, null, downloader);
            }

            return null;
        }

        return new UriTemplateResolver(providerTemplate);
    }
}
