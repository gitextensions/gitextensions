using System;
using System.Drawing;
using System.Threading.Tasks;
using JetBrains.Annotations;

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
        /// <exception cref="ArgumentException"><paramref name="imageFileName"/> is <c>null</c> or white-space.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="image"/> is <c>null</c>.</exception>
        void AddImage([NotNull] string imageFileName, [NotNull] Image image);

        /// <summary>
        /// Clears the cache by deleting all images.
        /// </summary>
        [NotNull]
        Task ClearAsync();

        /// <summary>
        /// Deletes the specified image from the cache.
        /// </summary>
        /// <param name="imageFileName">The image file name.</param>
        /// <exception cref="ArgumentException"><paramref name="imageFileName"/> is <c>null</c> or white-space.</exception>
        [NotNull]
        Task DeleteImageAsync([NotNull] string imageFileName);

        /// <summary>
        /// Retrieves the image from the cache.
        /// </summary>
        /// <param name="imageFileName">The image file name.</param>
        /// <exception cref="ArgumentException"><paramref name="imageFileName"/> is <c>null</c> or white-space.</exception>
        [CanBeNull]
        Image GetImage([NotNull] string imageFileName);

        /// <summary>
        /// Retrieves the image from the cache.
        /// </summary>
        /// <param name="imageFileName">The image file name.</param>
        /// <exception cref="ArgumentException"><paramref name="imageFileName"/> is <c>null</c> or white-space.</exception>
        [NotNull]
        [ItemCanBeNull]
        Task<Image> GetImageAsync([NotNull] string imageFileName);
    }
}