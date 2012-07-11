using System;
using System.Drawing;
using System.IO;

namespace Gravatar
{
    internal class DirectoryImageCache : IImageCache
    {
        private static Object padlock = new Object();

        string cachePath;
        public string Path { get { return cachePath; } }

        public DirectoryImageCache(string cachePath)
        {
            this.cachePath = cachePath;
        }

        public void ClearCache()
        {
            lock (padlock)
            {
                if (!Directory.Exists(cachePath))
                    return;

                foreach (string file in Directory.GetFiles(cachePath))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    { }
                }
            }
        }

        public void DeleteCachedFile(string imageFileName)
        {
            lock (padlock)
            {
                if (File.Exists(cachePath + imageFileName))
                {
                    try
                    {

                        File.Delete(cachePath + imageFileName);
                    }
                    catch
                    { }
                }
            }
        }

        public bool FileIsCached(string imageFileName)
        {
            lock (padlock)
            {
                return File.Exists(cachePath + imageFileName);
            }
        }

        public bool FileIsExpired(string imageFileName, int cacheDays)
        {
            var file = new FileInfo(cachePath + imageFileName);
            if (file.Exists)
            {
                if (file.LastWriteTime < DateTime.Now.AddDays(-cacheDays))
                    return true;
            }

            return false;
        }

        public Image LoadImageFromCache(string imageFileName, Bitmap defaultBitmap)
        {
            lock (padlock)
            {
                try
                {
                    if (!File.Exists(cachePath + imageFileName))
                        return null;

                    using (Stream fileStream = new FileStream(cachePath + imageFileName, FileMode.Open, FileAccess.Read))
                    {
                        return Image.FromStream(fileStream);
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        public void CacheImage(string imageFileName, Stream imageStream)
        {
            if (!Directory.Exists(cachePath))
                Directory.CreateDirectory(cachePath);

            lock (padlock)
            {
                try
                {
                    using (var output = new FileStream(cachePath + imageFileName, FileMode.Create))
                    {
                        byte[] buffer = new byte[1024];
                        int read;

                        if (imageStream != null)
                            while ((read = imageStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                output.Write(buffer, 0, read);
                            }
                    }
                }
                catch
                {
                }
            }
        }

    }
}
