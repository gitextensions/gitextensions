using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Gravatar
{
    // TODO image cache should differentiate by image size, as on a system where DPI changes (eg. laptop/monitor screens) the cached images may be innapropriate

    public interface IImageCache
    {
        /// <summary>
        /// Raised whenever the cache is changed by adding, removing or clearing all images.
        /// </summary>
        event EventHandler Invalidated;

        /// <summary>
        /// Adds an image to the cache.
        /// </summary>
        /// <param name="imageFileName">The image file name.</param>
        /// <param name="image">The image to add to the cache.</param>
        void AddImage(string imageFileName, Image image);

        /// <summary>
        /// Clears the cache by deleting all images.
        /// </summary>
        Task ClearAsync();

        /// <summary>
        /// Deletes the specified image from the cache.
        /// </summary>
        /// <param name="imageFileName">The image file name.</param>
        Task DeleteImageAsync(string imageFileName);

        /// <summary>
        /// Retrieves the image from the cache.
        /// </summary>
        /// <param name="imageFileName">The image file name.</param>
        Image GetImage(string imageFileName);

        /// <summary>
        /// Retrieves the image from the cache.
        /// </summary>
        /// <param name="imageFileName">The image file name.</param>
        Task<Image> GetImageAsync(string imageFileName);
    }
}