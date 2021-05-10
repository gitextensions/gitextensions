using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GitCommands;

namespace GitUI.Avatars
{
    public sealed class GravatarProvider : IAvatarProvider
    {
        // For details about the rating see https://en.gravatar.com/site/implement/images#rating
        // "g": suitable for display on all websites with any audience type.
        private const string _rating = "g";

        private readonly IAvatarDownloader _downloader;
        private readonly AvatarFallbackType? _fallback;
        private readonly bool _forceFallback;

        public GravatarProvider(
            IAvatarDownloader downloader,
            AvatarFallbackType? fallback,
            bool forceFallback = false)
        {
            _downloader = downloader ?? throw new ArgumentNullException(nameof(downloader));
            _fallback = fallback;
            _forceFallback = forceFallback;
        }

        public static bool IsFallbackSupportedByGravatar(AvatarFallbackType fallback)
        {
            return SerializeFallbackType(fallback) is not null;
        }

        /// <inheritdoc/>
        public Task<Image?> GetAvatarAsync(string email, string? name, int imageSize)
        {
            var fallback = SerializeFallbackType(_fallback) ?? "404";
            var hash = ComputeHash(email);

            UriBuilder uri = new("https", "www.gravatar.com")
            {
                Path = $"/avatar/{hash}",
                Query = $"s={imageSize}&r={_rating}&d={fallback}"
            };

            if (_forceFallback)
            {
                uri.Query += "&f=y";
            }

            var avatarUri = uri.Uri;

            // TODO NULLABLE UriBuilder.Uri doesn't appear to be nullable
            if (avatarUri is null)
            {
                return Task.FromResult<Image?>(null);
            }

            return _downloader.DownloadImageAsync(avatarUri);
        }

        private static string ComputeHash(string email)
        {
            // Hash specified at http://en.gravatar.com/site/implement/hash/
            using MD5CryptoServiceProvider md5 = new();

            // Gravatar doesn't specify an encoding
            var emailBytes = Encoding.UTF8.GetBytes(email.Trim().ToLowerInvariant());
            var hashBytes = md5.ComputeHash(emailBytes);

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
    }
}
