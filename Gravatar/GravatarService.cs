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
        public static void ClearImageCache()
        {
            ImageCache.ClearCache();
        }

        public static void RemoveImageFromCache(string imageFileName)
        {
            ImageCache.DeleteCachedFile(imageFileName);
        }

        public static void LoadCachedImage(string imageFileName, string email, Bitmap defaultBitmap, int cacheDays,
                                             int imageSize, Action<Image> onChangedImage)
        {
            var isolatedStorage = ImageCache.GetIsolatedStorageFile();

            // If the user image is not cached yet, download it from gravatar and store it in the isolatedStorage
            if (isolatedStorage.GetFileNames(imageFileName).Length == 0 ||
                ImageCache.FileIsExpired(isolatedStorage, imageFileName, cacheDays))
            {
                onChangedImage(defaultBitmap);

                GetImageFromGravatar(imageFileName, email, imageSize, isolatedStorage);
            }
            if (isolatedStorage.GetFileNames(imageFileName).Length != 0)
            {
                onChangedImage(ImageCache.LoadImageFromCache(imageFileName, isolatedStorage,
                                                             defaultBitmap));
            }
            else
            {
                onChangedImage(defaultBitmap);
            }
        }

        public static void GetImageFromGravatar(string imageFileName, string email, int authorImageSize,
                                                IsolatedStorageFile isolatedStorage)
        {
            try
            {
                var baseUrl = String.Concat("http://www.gravatar.com/avatar/{0}?d=identicon&s=",
                                            authorImageSize, "&r=g");

                //hash the email address
                var emailHash = MD5.CalcMD5(email.ToLower());

                //format our url to the Gravatar
                var imageUrl = String.Format(baseUrl, emailHash);

                var webClient = new WebClient {Proxy = WebRequest.DefaultWebProxy};
                webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;

                var imageStream = webClient.OpenRead(imageUrl);

                using (var output = new IsolatedStorageFileStream(imageFileName, FileMode.Create, isolatedStorage))
                {
                    var buffer = new byte[1024];
                    int read;

                    if (imageStream != null)
                        while ((read = imageStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            output.Write(buffer, 0, read);
                        }
                }
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
                    StartInfo = {FileName = @"http://www.gravatar.com"}
                }.Start();
        }
    }
}