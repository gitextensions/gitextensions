using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;

namespace Gravatar
{
    internal class DirectoryImageCache : IImageCache
    {
        string cachePath;

        public DirectoryImageCache(string cachePath)
        {
            this.cachePath = cachePath;
        }

        public void ClearCache()
        {
            if (!Directory.Exists(cachePath))
                return;

            foreach (string file in Directory.GetFiles(cachePath))
            {
                File.Delete(file);
            }
        }

        public void DeleteCachedFile(string imageFileName)
        {
            if (File.Exists(cachePath + imageFileName))
                File.Delete(cachePath + imageFileName);
        }

        public bool FileIsCached(string imageFileName)
        {
            return File.Exists(cachePath + imageFileName);
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

        public System.Drawing.Image LoadImageFromCache(string imageFileName, System.Drawing.Bitmap defaultBitmap)
        {
            if (!File.Exists(cachePath + imageFileName))
                return null;

            return Image.FromFile(cachePath + imageFileName);
        }

        public void CacheImage(string imageFileName, System.IO.Stream imageStream)
        {
            if (!Directory.Exists(cachePath))
                Directory.CreateDirectory(cachePath);

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

    }
}
