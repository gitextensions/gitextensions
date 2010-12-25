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

        public enum FallBackService
        {
            MonsterId,
            Wavatar,
            Identicon
        }

        public static void ClearImageCache()
        {
            cache.ClearCache();
        }

        public static void RemoveImageFromCache(string imageFileName)
        {
            cache.DeleteCachedFile(imageFileName);
        }

        public static void LoadCachedImage(string imageFileName, string email, Bitmap defaultBitmap, int cacheDays,
                                             int imageSize, string imageCachePath, Action<Image> onChangedImage,
                                             FallBackService fallBack)
        {
            try
            {
                if (cache == null)
                    cache = new DirectoryImageCache(imageCachePath); //or: new IsolatedStorageImageCache();

                // If the user image is not cached yet, download it from gravatar and store it in the isolatedStorage
                if (!cache.FileIsCached(imageFileName) ||
                    cache.FileIsExpired(imageFileName, cacheDays))
                {
                    onChangedImage(defaultBitmap);

                    GetImageFromGravatar(imageFileName, email, imageSize, fallBack);
                }
                if (cache.FileIsCached(imageFileName))
                {
                    onChangedImage(cache.LoadImageFromCache(imageFileName,
                                                                 defaultBitmap));
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
                var emailHash = MD5.CalcMD5(email.ToLower());

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