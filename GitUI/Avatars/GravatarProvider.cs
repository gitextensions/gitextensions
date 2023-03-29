using System.Security.Cryptography;
using System.Text;
using GitCommands;

namespace GitUI.Avatars
{
    public sealed class GravatarProvider : IAvatarProvider, IDisposable
    {
        // For details about the rating see https://en.gravatar.com/site/implement/images#rating
        // "g": suitable for display on all websites with any audience type.
        private const string _rating = "g";

        private readonly IAvatarDownloader _downloader;
        private readonly AvatarFallbackType? _fallback;
        private readonly bool _forceFallback;

        // Hash specified at http://en.gravatar.com/site/implement/hash/
        private readonly MD5 _md5 = MD5.Create();

        private readonly string _queryString;

        public GravatarProvider(
            IAvatarDownloader downloader,
            AvatarFallbackType? fallback,
            bool forceFallback = false)
        {
            _downloader = downloader ?? throw new ArgumentNullException(nameof(downloader));
            _fallback = fallback;
            _forceFallback = forceFallback;

            string fallbackString = SerializeFallbackType(_fallback) ?? "404";
            _queryString = $"r={_rating}&d={fallbackString}";

            if (_forceFallback)
            {
                _queryString += "&f=y";
            }

            _queryString += "&s=";
        }

        public static bool IsFallbackSupportedByGravatar(AvatarFallbackType fallback)
        {
            return SerializeFallbackType(fallback) is not null;
        }

        /// <inheritdoc/>
        public Task<Image?> GetAvatarAsync(string email, string? name, int imageSize)
        {
            var hash = ComputeHash(email);

            UriBuilder uri = new("https", "www.gravatar.com")
            {
                Path = $"/avatar/{hash}",
                Query = _queryString + imageSize
            };

            var avatarUri = uri.Uri;

            // TODO NULLABLE UriBuilder.Uri doesn't appear to be nullable
            if (avatarUri is null)
            {
                return Task.FromResult<Image?>(null);
            }

            return _downloader.DownloadImageAsync(avatarUri);
        }

        private string ComputeHash(string email)
        {
            // Gravatar doesn't specify an encoding
            var emailBytes = Encoding.UTF8.GetBytes(email.Trim().ToLowerInvariant());
            var hashBytes = _md5.ComputeHash(emailBytes);

            return HexString.FromByteArray(hashBytes);
        }

        private static string? SerializeFallbackType(AvatarFallbackType? fallback)
        {
            return fallback switch
            {
                AvatarFallbackType.Identicon => "identicon",
                AvatarFallbackType.MonsterId => "monsterid",
                AvatarFallbackType.Wavatar => "wavatar",
                AvatarFallbackType.Retro => "retro",
                AvatarFallbackType.Robohash => "robohash",
                _ => null,
            };
        }

        public void Dispose()
        {
            _md5.Dispose();
        }
    }
}
