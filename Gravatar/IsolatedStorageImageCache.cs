using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;

namespace Gravatar
{
    internal class IsolatedStorageImageCache : IImageCache
    {
        public static IsolatedStorageFile GetIsolatedStorageFile()
        {
            return IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
        }

        public void DeleteCachedFile(string imageFileName)
        {
            var isolatedStorage = GetIsolatedStorageFile();

            if (isolatedStorage.GetFileNames(imageFileName).Length != 0)
            {
                isolatedStorage.DeleteFile(imageFileName);
            }
        }

        public void ClearCache()
        {
            foreach (var gravatarFileName in GetIsolatedStorageFile().GetFileNames("*.png"))
                GetIsolatedStorageFile().DeleteFile(gravatarFileName);
        }

        public bool FileIsCached(string imageFileName)
        {
            return GetIsolatedStorageFile().GetFileNames(imageFileName).Length != 0;
        }

        public bool FileIsExpired(string imageFileName, int cacheDays)
        {
            // Very very very ugly function to determine lastwritetime of file in isolated storage
            if (cacheDays == 0)
                return false;

            try
            {
                var propertyInfo = GetIsolatedStorageFile().GetType().GetField("m_RootDir",
                                                                      BindingFlags.NonPublic | BindingFlags.Instance);
                if (propertyInfo != null)
                {
                    var rootDir = propertyInfo.GetValue(GetIsolatedStorageFile()) as string;
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

        public Image LoadImageFromCache(string imageFileName, 
                                               Bitmap defaultBitmap)
        {
            try
            {
                using (var stream = new IsolatedStorageFileStream(imageFileName, FileMode.Open, GetIsolatedStorageFile()))
                {
                    return Image.FromStream(stream);
                }
            }
            catch (ArgumentException)
            {
                // The file is not a valid image, delete it
                GetIsolatedStorageFile().DeleteFile(imageFileName);

                return defaultBitmap;
            }
            catch
            {
                // Other erros, ignore
                return defaultBitmap;
            }
        }

        public void CacheImage(string imageFileName, Stream imageStream)
        {
            using (var output = new IsolatedStorageFileStream(imageFileName, FileMode.Create, GetIsolatedStorageFile()))
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
                                
    }
}