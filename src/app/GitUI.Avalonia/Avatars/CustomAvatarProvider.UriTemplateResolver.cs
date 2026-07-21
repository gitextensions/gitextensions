using System.Web;

namespace GitUI.Avatars;

public sealed partial class CustomAvatarProvider
{
    private sealed class UriTemplateResolver : IAvatarProvider
    {
        private readonly Func<UriTemplateData, string> _templateResolver;

        public UriTemplateResolver(string uriTemplate)
        {
            _templateResolver = BuildResolver(uriTemplate);
        }

        public bool PerformsIo => false;

        public Task<byte[]?> GetAvatarAsync(string email, string? name, int imageSize)
        {
            throw new InvalidOperationException($"UriTemplateResolvers can't be used as a regular IAvatarProvider, use '{nameof(ResolveTemplate)}'.");
        }

        public Uri? ResolveTemplate(UriTemplateData templateData)
        {
            ArgumentNullException.ThrowIfNull(templateData);

            string rawUri = _templateResolver(templateData);
            return string.IsNullOrWhiteSpace(rawUri) ? null : new Uri(rawUri);
        }

        private static Func<UriTemplateData, string> BuildResolver(string template)
            => TemplateFormatter.Create(template, TemplateDataResolve);

        private static Func<UriTemplateData, string?> TemplateDataResolve(string name)
        {
            return name.Trim().ToLowerInvariant() switch
            {
                "name" => user => HttpUtility.UrlEncode(user.Name),
                "email" => user => HttpUtility.UrlEncode(user.NormalizedEmail),
                "md5" => user => user.EmailMd5.Value,
                "sha1" => user => user.EmailSha1.Value,
                "sha256" => user => user.EmailSha256.Value,
                "imagesize" => user => user.ImageSize.ToString(),
                _ => _ => null,
            };
        }
    }
}
