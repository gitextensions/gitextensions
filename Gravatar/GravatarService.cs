using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Gravatar
{
    public interface IAvatarService
    {
        /// <summary>
        /// Loads avatar either from the local cache or from the remote service.
        /// </summary>
        Task<Image> GetAvatarAsync(string email, int imageSize, string defaultImageType);

        /// <summary>
        /// Removes the avatar from the local cache.
        /// </summary>
        Task DeleteAvatarAsync(string email);

        /// <summary>
        /// Translates the serialised image type into the corresponding enum value.
        /// </summary>
        /// <param name="imageType">The serialised image type.</param>
        DefaultImageType GetDefaultImageType(string imageType);
    }

    public class GravatarService : IAvatarService
    {
        private readonly IImageCache _cache;
        private readonly IImageNameProvider _avatarImageNameProvider;

        private const string GitHubPrivateEmailSuffix = "@users.noreply.github.com";

        public GravatarService(IImageCache imageCache, IImageNameProvider avatarImageNameProvider)
        {
            _cache = imageCache;
            _avatarImageNameProvider = avatarImageNameProvider;
        }

        public GravatarService(IImageCache imageCache)
            : this(imageCache, new AvatarImageNameProvider())
        {
        }

        /// <summary>
        /// Loads avatar either from the local cache or from the remote service.
        /// </summary>
        public async Task<Image> GetAvatarAsync(string email, int imageSize, string defaultImageType)
        {
            var imageFileName = _avatarImageNameProvider.Get(email);

            var image = await _cache.GetImageAsync(imageFileName);

            if (image == null)
            {
                if (email.EndsWith(GitHubPrivateEmailSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    image = await LoadFromGitHubAsync(email, imageSize);
                }
                else
                {
                    image = await LoadFromGravatarAsync(email, imageSize, GetDefaultImageType(defaultImageType));
                }

                // Store image to cache for future
                _cache.AddImage(imageFileName, image);
            }

            return image;
        }

        /// <summary>
        /// Removes the avatar from the local cache.
        /// </summary>
        public async Task DeleteAvatarAsync(string email)
        {
            var imageFileName = _avatarImageNameProvider.Get(email);
            await _cache.DeleteImageAsync(imageFileName);
        }

        /// <summary>
        /// Translates the serialised image type into the corresponding enum value.
        /// </summary>
        /// <param name="imageType">The serialised image type.</param>
        public DefaultImageType GetDefaultImageType(string imageType)
        {
            if (!Enum.TryParse(imageType, true, out DefaultImageType defaultImageType))
            {
                defaultImageType = DefaultImageType.None;
            }

            return defaultImageType;
        }

        private static async Task<Image> LoadFromGitHubAsync(string email, int imageSize)
        {
            try
            {
                int suffixPosition = email.IndexOf(GitHubPrivateEmailSuffix, StringComparison.OrdinalIgnoreCase);
                string username = email.Substring(0, suffixPosition);
                var client = new Git.hub.Client();
                var user = client.GetUser(username);
                if (!string.IsNullOrEmpty(user?.AvatarUrl))
                {
                    var builder = new UriBuilder(user.AvatarUrl);
                    var query = new StringBuilder(builder.Query.TrimStart('?'));
                    query.Append(query.Length == 0 ? "?" : "&");
                    query.AppendFormat("s={0}", imageSize);
                    builder.Query = query.ToString();
                    var imageUrl = builder.Uri.AbsoluteUri;

                    return await DownloadImageAsync(imageUrl);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            return null;
        }

        private static Task<Image> LoadFromGravatarAsync(string email, int imageSize, DefaultImageType defaultImageType)
        {
            return DownloadImageAsync(BuildGravatarUrl());

            string BuildGravatarUrl()
            {
                var d = GetDefaultImageString();
                var hash = CalculateHash();

                return $"http://www.gravatar.com/avatar/{hash}?s={imageSize}&r=g&d={d}";

                string GetDefaultImageString()
                {
                    switch (defaultImageType)
                    {
                        case DefaultImageType.Identicon: return "identicon";
                        case DefaultImageType.MonsterId: return "monsterid";
                        case DefaultImageType.Wavatar: return "wavatar";
                        case DefaultImageType.Retro: return "retro";
                        default: return "404";
                    }
                }

                string CalculateHash()
                {
                    // Hash specified at http://en.gravatar.com/site/implement/hash/
                    using (var md5 = new MD5CryptoServiceProvider())
                    {
                        // Gravatar doesn't specify an encoding
                        var emailBytes = Encoding.UTF8.GetBytes(email.Trim().ToLowerInvariant());

                        var hashBytes = md5.ComputeHash(emailBytes);

                        // TODO can combine with more efficient logic from ObjectId if that PR is merged
                        var builder = new StringBuilder(capacity: 32);

                        foreach (var b in hashBytes)
                        {
                            builder.Append(b.ToString("x2"));
                        }

                        return builder.ToString();
                    }
                }
            }
        }

        private static async Task<Image> DownloadImageAsync(string imageUrl)
        {
            try
            {
                using (var webClient = new WebClient { Proxy = WebRequest.DefaultWebProxy })
                {
                    webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;

                    using (var imageStream = await webClient.OpenReadTaskAsync(imageUrl))
                    {
                        return Image.FromStream(imageStream);
                    }
                }
            }
            catch (Exception ex)
            {
                // catch IO errors
                Trace.WriteLine(ex.Message);
            }

            return null;
        }
    }
}
