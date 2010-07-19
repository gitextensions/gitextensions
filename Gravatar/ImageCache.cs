using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;

namespace Gravatar
{
    internal class ImageCache
    {
        public static IsolatedStorageFile GetIsolatedStorageFile()
        {
            return IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
        }

        public static void DeleteCachedFile(string imageFileName)
        {
            var isolatedStorage = GetIsolatedStorageFile();

            if (isolatedStorage.GetFileNames(imageFileName).Length != 0)
            {
                isolatedStorage.DeleteFile(imageFileName);
            }
        }

        public static void ClearCache()
        {
            foreach (var gravatarFileName in GetIsolatedStorageFile().GetFileNames("*.png"))
                GetIsolatedStorageFile().DeleteFile(gravatarFileName);
        }

        public static bool FileIsExpired(IsolatedStorageFile isolatedStorage, string imageFileName, int cacheDays)
        {
            // Very very very ugly function to determine lastwritetime of file in isolated storage
            if (cacheDays == 0)
                return false;

            try
            {
                var propertyInfo = isolatedStorage.GetType().GetField("m_RootDir",
                                                                      BindingFlags.NonPublic | BindingFlags.Instance);
                if (propertyInfo != null)
                {
                    var rootDir = propertyInfo.GetValue(isolatedStorage) as string;
                    if (rootDir != null)
                    {
                        var file = new FileInfo(rootDir + imageFileName);
                        if (file.Exists)
                        {
                            if (file.LastWriteTime < DateTime.Now.AddDays(-cacheDays))
                                return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            return false;
        }

        public static Image LoadImageFromCache(string imageFileName, IsolatedStorageFile isolatedStorage,
                                               Bitmap defaultBitmap)
        {
            try
            {
                using (var stream = new IsolatedStorageFileStream(imageFileName, FileMode.Open, isolatedStorage))
                {
                    return Image.FromStream(stream);
                }
            }
            catch (ArgumentException)
            {
                // The file is not a valid image, delete it
                isolatedStorage.DeleteFile(imageFileName);

                return defaultBitmap;
            }
            catch
            {
                // Other erros, ignore
                return defaultBitmap;
            }
        }
    }
}