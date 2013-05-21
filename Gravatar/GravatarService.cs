using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Text.RegularExpressions;

namespace Gravatar
{
    /// <summary>
    /// Services that gravatar provides in order to provide avatars in
    /// the absence of a user-uploaded image.
    /// </summary>
    public enum FallBackService
    {
        /// <summary>
        /// Return an HTTP 404 respose.
        /// </summary>
        None,

        /// <summary>
        /// Return a cartoon-style silhouette.
        /// </summary>
        MysteryMan,

        /// <summary>
        /// Return a generated monster based on the email hash.
        /// </summary>
        MonsterId,

        /// <summary>
        /// Return a generated face based on the email hash.
        /// </summary>
        Wavatar,

        /// <summary>
        /// Return a geometric pattern based on the email hash.
        /// </summary>
        Identicon,

        /// <summary>
        /// Return an 8-bit-style face based on the email hash.
        /// </summary>
        Retro,
    }

    /// <summary>
    /// Specifies the maximum rating of a given gravatar image request.
    /// </summary>
    public enum Rating
    {
        /// <summary>
        /// Suitable for all audiences.
        /// </summary>
        G,

        /// <summary>
        /// May contain rude gestures, provocatively dressed indiviiduals,
        /// the lesser swear words, or mild violence
        /// </summary>
        PG,

        /// <summary>
        /// May contain such things as harsh profanity, intense violence,
        /// nudity, or hard drug use.
        /// </summary>
        R,

        /// <summary>
        /// May contain hardcore sexual imagery or extremely disturbing
        /// violence.
        /// </summary>
        X,
    }

    public class GravatarService
    {
        static IImageCache cache;
        static object gravatarServiceLock = new object();

        /// <summary>
        /// Provides a mapping for the image defaults.
        /// </summary>
        private static Dictionary<FallBackService, string> fallBackStrings = new Dictionary<FallBackService, string>
        {
            { FallBackService.None, "404" },
            { FallBackService.Identicon, "identicon" },
            { FallBackService.MonsterId, "monsterid" },
            { FallBackService.Wavatar, "wavatar" },
            { FallBackService.Retro, "retro" },
        };

        /// <summary>
        /// Gets a collection of <see cref="FallBackService"/> for which
        /// the <see cref="GravatarService"/> will provide dynamically
        /// generated avatars.
        /// </summary>
        /// <remarks>
        /// This collection will also contain <see cref="FallBackService.None"/>,
        /// so it is suitable for display directly to end users.
        /// </remarks>
        public static ICollection<FallBackService> DynamicServices
        {
            get
            {
                return fallBackStrings.Keys;
            }
        }

        public static void ClearImageCache()
        {
            if (cache != null)
            {
                cache.ClearCache();
            }
        }

        public static void RemoveImageFromCache(string imageFileName)
        {
            if (cache != null)
            {
                cache.DeleteCachedFile(imageFileName);
            }
        }

        public static Image GetImageFromCache(string imageFileName, string email, int cacheDays,
                                             int imageSize, string imageCachePath, FallBackService fallBack)
        {
            try
            {
                if (cache == null)
                    cache = new DirectoryImageCache(imageCachePath); //or: new IsolatedStorageImageCache();

                // If the user image is not cached yet, download it from gravatar and store it in the isolatedStorage
                if (!cache.FileIsCached(imageFileName) ||
                    cache.FileIsExpired(imageFileName, cacheDays))
                {
                    return null;
                }
                if (cache.FileIsCached(imageFileName))
                {
                    return cache.LoadImageFromCache(imageFileName, null);
                }

            }
            catch (Exception ex)
            {
                //catch IO errors
                Trace.WriteLine(ex.Message);
            }
            return null;
        }

        static bool IsValidEmail(string strIn)
        {
            // Return true if strIn is in valid e-mail format.
            return Regex.IsMatch(strIn, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }

        public static void CacheImage(string imageFileName, string email, int imageSize, 
                                        FallBackService fallBack)
        {
            try
            {
                if (cache == null)
                    return;

                if (IsValidEmail(email) && !cache.FileIsCached(imageFileName))
                {
                    //Lock added to make sure gravatar doesn't block this ip..
                    lock (gravatarServiceLock)
                    {
                        GetImageFromGravatar(imageFileName, email, imageSize, fallBack);
                    }
                }
            }
            catch (Exception ex)
            {
                //catch IO errors
                Trace.WriteLine(ex.Message);
            }
        }

        public static void LoadCachedImage(string imageFileName, string email, Bitmap defaultBitmap, int cacheDays,
                                             int imageSize, string imageCachePath, Action<Image> onChangedImage,
                                             FallBackService fallBack)
        {
            Image image = GetImageFromCache(imageFileName, email, cacheDays, imageSize, imageCachePath, fallBack);

            try
            {
                if (image == null)
                {
                    onChangedImage(defaultBitmap);

                    //Lock added to make sure gravatar doesn't block this ip..
                    lock (gravatarServiceLock)
                    {
                        if (GetImageFromCache(imageFileName, email, cacheDays, imageSize, imageCachePath, fallBack) == null)
                            GetImageFromGravatar(imageFileName, email, imageSize, fallBack);
                    }
                }
                image = GetImageFromCache(imageFileName, email, cacheDays, imageSize, imageCachePath, fallBack);
                if (image != null)
                {
                    onChangedImage(image);
                }
                else
                {
                    onChangedImage(defaultBitmap);
                }
            }
            catch (Exception ex)
            {
                //catch IO errors
                Trace.WriteLine(ex.Message);
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


        /// <summary>
        /// BuildGravatarUrl with default parameters
        /// </summary>
        /// <param name="email">The email address for which to build the <see cref="System.Uri"/>.</param>
        /// <returns>The constructed <see cref="System.Uri"/>.</returns>
        private static Uri BuildGravatarUrl(string email)
        {
            return BuildGravatarUrl(email, 32, false, Rating.G, FallBackService.None);
        }

        /// <summary>
        /// Builds a <see cref="System.Uri"/> corresponding to a given email address.
        /// </summary>
        /// <param name="email">The email address for which to build the <see cref="System.Uri"/>.</param>
        /// <param name="size">The size of the image to request.  The default is 32.</param>
        /// <param name="useHttps">Indicates whether or not the request should be performed over Secure HTTP.</param>
        /// <param name="rating">The mazimum rating of the returned image.</param>
        /// <param name="fallBack">The Gravatar service that will be used for fall-back.</param>
        /// <returns>The constructed <see cref="System.Uri"/>.</returns>
        private static Uri BuildGravatarUrl(string email, int size, bool useHttps, Rating rating, FallBackService fallBack)
        {
            var builder = new UriBuilder("http://www.gravatar.com/avatar/");

            if (useHttps)
            {
                builder.Scheme = "https";
            }

            builder.Path += HashEmail(email);

            string d;
            if (!fallBackStrings.TryGetValue(fallBack, out d))
            {
                d = "404";
            }

            var query = string.Format("s={0}&r={1}&d={2}",
                size.ToString(),
                rating.ToString().ToLowerInvariant(),
                d);

            builder.Query = query;

            return builder.Uri;
        }

        public static void GetImageFromGravatar(string imageFileName, string email, int authorImageSize, FallBackService fallBack)
        {
            try
            {
                var imageUrl = BuildGravatarUrl(email,
                    authorImageSize,
                    false,
                    Rating.G,
                    fallBack);

                var webClient = new WebClient { Proxy = WebRequest.DefaultWebProxy };
                webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;

                var imageStream = webClient.OpenRead(imageUrl);

                cache.CacheImage(imageFileName, imageStream);
            }
            catch (Exception ex)
            {
                //catch IO errors
                Trace.WriteLine(ex.Message);
            }
        }

        public static void OpenGravatarRegistration()
        {
            new Process
                {
                    EnableRaisingEvents = false,
                    StartInfo = { FileName = @"http://www.gravatar.com" }
                }.Start();
        }
    }
}