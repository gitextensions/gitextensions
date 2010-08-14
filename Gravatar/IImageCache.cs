using System;
using System.Drawing;
using System.IO;
namespace Gravatar
{
    interface IImageCache
    {
        void ClearCache();
        void DeleteCachedFile(string imageFileName);
        bool FileIsCached(string imageFileName);
        bool FileIsExpired(string imageFileName, int cacheDays);
        Image LoadImageFromCache(string imageFileName, System.Drawing.Bitmap defaultBitmap);
        void CacheImage(string imageFileName, Stream imageStream);
    }
}
