using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using GitCommands;
using GitExtUtils;

namespace GitUI.Avatars
{
    /// <summary>
    /// The custom avatar provider allows the user to define one or more custom providers that
    /// are queried in order until an avatar image is found.
    /// </summary>
    /// <remarks>
    /// Details about the usage and syntax of the custom avatar provider can be found in the gitextensions wiki.
    /// </remarks>
    public sealed partial class CustomAvatarProvider : IAvatarProvider
    {
        private readonly IAvatarDownloader _downloader;
        private readonly IAvatarProvider[] _subProvider;

        private CustomAvatarProvider(IAvatarDownloader downloader, IAvatarProvider[] subProvider)
        {
            _downloader = downloader ?? throw new ArgumentNullException(nameof(downloader));
            _subProvider = subProvider ?? throw new ArgumentNullException(nameof(subProvider));
        }

        /// <inheritdoc/>
        public async Task<Image?> GetAvatarAsync(string email, string? name, int imageSize)
        {
            UriTemplateData templateData = new(email, name, imageSize);

            foreach (var provider in _subProvider)
            {
                var avatar = provider switch
                {
                    UriTemplateResolver r => await _downloader.DownloadImageAsync(r.ResolveTemplate(templateData)),
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
        /// Parses a custom avatar template string and creates an <see cref="IAvatarProvider"/> from it.
        /// </summary>
        /// <param name="customProviderTemplates">The custom avatar provider template.</param>
        /// <param name="downloader">The downloader that is used to download avatar images.</param>
        /// <returns>Returns the <see cref="IAvatarProvider"/> described by the template.</returns>
        public static IAvatarProvider ParseTemplateString(string customProviderTemplates, IAvatarDownloader downloader)
        {
            if (downloader is null)
            {
                throw new ArgumentNullException(nameof(downloader));
            }

            var providerParts = customProviderTemplates
                .LazySplit(';')
                .Select(p => p.Trim())
                .Select(p => FromTemplateSegment(downloader, p))
                .WhereNotNull()
                .ToArray();

            // We can't use the chain provider here, because some returned providers (namely UriTemplateResolvers)
            // don't actually fulfill the interface contract of IAvatarProvider. UriTemplateResolver is a special
            // case that exists to prevent variables like hashes from being evaluated/calculated multiple times.

            return new CustomAvatarProvider(downloader, providerParts);
        }

        /// <summary>
        /// Parses a single template segment.
        /// </summary>
        private static IAvatarProvider? FromTemplateSegment(IAvatarDownloader downloader, string providerTemplate)
        {
            // if the segment is a tag like "<Demo>", we extract the name
            // and try to parse it as an AvatarProvider enum.
            if (providerTemplate.StartsWith("<") && providerTemplate.EndsWith(">"))
            {
                var providerName = providerTemplate.Substring(1, providerTemplate.Length - 2);

                if (Enum.TryParse<AvatarProvider>(providerName, true, out var provider)
                    && provider != AvatarProvider.Custom)
                {
                    return AvatarService.CreateAvatarProvider(provider, null, downloader);
                }

                return null;
            }

            // in all other cases assume it's an UriTemplate
            return new UriTemplateResolver(providerTemplate);
        }
    }
}
