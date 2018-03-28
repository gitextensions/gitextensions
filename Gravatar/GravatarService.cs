using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
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
            var image = await _cache.GetImageAsync(imageFileName, null);
            if (image == null)
            {
                if (email.EndsWith(GitHubPrivateEmailSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    image = await LoadFromGitHubAsync(imageFileName, email, imageSize);
                }
                else
                {
                    image = await LoadFromGravatarAsync(imageFileName, email, imageSize, GetDefaultImageType(defaultImageType));
                }
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

        /// <summary>
        /// Builds a <see cref="Uri"/> corresponding to a given email address.
        /// </summary>
        /// <param name="email">The email address for which to build the <see cref="Uri"/>.</param>
        /// <param name="size">The size of the image to request.  The default is 32.</param>
        /// <param name="useHttps">Indicates whether or not the request should be performed over Secure HTTP.</param>
        /// <param name="rating">The maximum rating of the returned image.</param>
        /// <param name="defaultImageType">The Gravatar service that will be used for fall-back.</param>
        /// <returns>The constructed <see cref="Uri"/>.</returns>
        private static Uri BuildGravatarUrl(string email, int size, bool useHttps, Rating rating, DefaultImageType defaultImageType)
        {
            var builder = new UriBuilder("http://www.gravatar.com/avatar/");
            if (useHttps)
            {
                builder.Scheme = "https";
            }

            builder.Path += HashEmail(email);

            var query = string.Format("s={0}&r={1}&d={2}",
                size,
                rating.ToString().ToLowerInvariant(),
                GetDefaultImageString(defaultImageType));

            builder.Query = query;

            return builder.Uri;
        }

        /// <summary>
        /// Provides a mapping for the image defaults.
        /// </summary>
        private static string GetDefaultImageString(DefaultImageType defaultImageType)
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

        /// <summary>
        /// Generates an email hash as per the Gravatar specifications.
        /// </summary>
        /// <param name="email">The email to hash.</param>
        /// <returns>The hash of the email.</returns>
        /// <remarks>
        /// The process of creating the hash are specified at http://en.gravatar.com/site/implement/hash/
        /// </remarks>
        private static string HashEmail(string email)
        {
            return MD5.CalcMD5(email.Trim().ToLowerInvariant());
        }

        private async Task<Image> LoadFromGitHubAsync(string imageFileName, string email, int imageSize)
        {
            string imageUrl = null;
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
                    imageUrl = builder.Uri.AbsoluteUri;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            if (string.IsNullOrEmpty(imageUrl))
            {
                return null;
            }

            return await DownloadImageAsync(new Uri(imageUrl), imageFileName);
        }

        private Task<Image> LoadFromGravatarAsync(string imageFileName, string email, int imageSize, DefaultImageType defaultImageType)
        {
            var imageUrl = BuildGravatarUrl(email, imageSize, false, Rating.G, defaultImageType);
            return DownloadImageAsync(imageUrl, imageFileName);
        }

        private async Task<Image> DownloadImageAsync(Uri imageUrl, string imageFileName)
        {
            try
            {
                using (var webClient = new WebClient { Proxy = WebRequest.DefaultWebProxy })
                {
                    webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    using (var imageStream = await webClient.OpenReadTaskAsync(imageUrl))
                    {
                        await _cache.AddImageAsync(imageFileName, imageStream);
                    }

                    return await _cache.GetImageAsync(imageFileName, null);
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
