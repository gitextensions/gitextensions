using System.Web;

namespace GitUI.Avatars;

public sealed partial class CustomAvatarProvider
{
    /// <summary>
        /// An avatar provider that takes <see cref="UriTemplateData"/> as input to provide <see cref="Uri"/>s.
        /// </summary>
        /// <remarks>
        /// The template resolver claims to be an <see cref="IAvatarProvider"/> but that's not really true.
        /// It only implements the interface to allow the parent class <see cref="CustomAvatarProvider"/> to
        /// store providers of two different types without using <see cref="object"/> as base type.
        /// A cleaner alternative would be for <see cref="CustomAvatarProvider"/> to use a discriminated union,
        /// but sadly C# doesn't offer them at the moment and external dependency just for that case
        /// would probably be overkill.
        /// </remarks>
    private sealed class UriTemplateResolver : IAvatarProvider
    {
        private readonly Func<UriTemplateData, string> _templateResolver;

        public UriTemplateResolver(string uriTemplate)
        {
            // create a formatter, so we only have to parse the template once
            // and reuse the formatter for a lot of inputs / user.
            _templateResolver = BuildResolver(uriTemplate);
        }

        public bool PerformsIo => false;

        public Task<byte[]?> GetAvatarAsync(string email, string? name, int imageSize)
        {
            // see remarks for details why we do this.
            throw new InvalidOperationException($"UriTemplateResolvers can't be used as a regular IAvatarProvider, use '{nameof(ResolveTemplate)}'.");
        }

        /// <summary>
                /// Resolves and returns a <see cref="Uri"/> based on the template given
                /// during construction and the data provided for <paramref name="templateData"/>.
                /// </summary>
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
