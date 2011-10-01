using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;

namespace Gravatar
{
    public class GravatarService
    {
        static IImageCache cache;
        static object gravatarServiceLock = new object();

        public enum FallBackService
        {
            None,
            MysteryMan,
            MonsterId,
            Wavatar,
            Identicon,
            Retro,
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