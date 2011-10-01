using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Collections.Generic;

namespace Gravatar
{
    public class GravatarService
    {
        static IImageCache cache;
        static object gravatarServiceLock = new object();

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
        /// Provides a mapping for the image defaults.
        /// </summary>
        private static Dictionary<FallBackService, string> fallBackStrings = new Dictionary<FallBackService, string>
        {
            { FallBackService.None, "404" },
            { FallBackService.MysteryMan, "mm" },
            { FallBackService.Identicon, "identicon" },
            { FallBackService.MonsterId, "monsterid" },
            { FallBackService.Wavatar, "wavatar" },
            { FallBackService.Retro, "retro" },
        };

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

        public static void ClearImageCache()
        {
            cache.ClearCache();
        }

        public static void RemoveImageFromCache(string imageFileName)
        {
            cache.DeleteCachedFile(imageFileName);
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

        public static void GetImageFromGravatar(string imageFileName, string email, int authorImageSize, FallBackService fallBack)
        {
            try
            {
                var baseUrl = String.Concat("http://www.gravatar.com/avatar/{0}?d=identicon&s=",
                                            authorImageSize, "&r=g");

                if (fallBack == FallBackService.Identicon)
                    baseUrl += "&d=identicon";
                if (fallBack == FallBackService.MonsterId)
                    baseUrl += "&d=monsterid";
                if (fallBack == FallBackService.Wavatar)
                    baseUrl += "&d=wavatar";

                //hash the email address
                var emailHash = HashEmail(email);

                //format our url to the Gravatar
                var imageUrl = String.Format(baseUrl, emailHash);

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