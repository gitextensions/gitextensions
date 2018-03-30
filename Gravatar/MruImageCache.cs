using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Gravatar
{
    /// <summary>
    /// Caches most-recently-used images.
    /// </summary>
    /// <remarks>
    /// Decorates an inner cache, delegating to it as needed.
    /// <para />
    /// If an image is available in memory, the inner cache can be bypassed.
    /// </remarks>
    public sealed class MruImageCache : IImageCache
    {
        private readonly MruCache<string, Image> _cache;
        private readonly IImageCache _inner;

        public MruImageCache([NotNull] IImageCache inner, int capacity = 30)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _cache = new MruCache<string, Image>(capacity);
        }

        event EventHandler IImageCache.Invalidated
        {
            add => _inner.Invalidated += value;
            remove => _inner.Invalidated -= value;
        }

        void IImageCache.AddImage(string imageFileName, Image image)
        {
            if (string.IsNullOrWhiteSpace(imageFileName))
            {
                throw new ArgumentException(nameof(imageFileName));
            }

            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            _inner.AddImage(imageFileName, image);
            _cache.Add(imageFileName, image);
        }

        Task IImageCache.ClearAsync()
        {
            _cache.Clear();
            return _inner.ClearAsync();
        }

        Task IImageCache.DeleteImageAsync(string imageFileName)
        {
            if (string.IsNullOrWhiteSpace(imageFileName))
            {
                throw new ArgumentException(nameof(imageFileName));
            }

            _cache.TryRemove(imageFileName, out _);
            return _inner.DeleteImageAsync(imageFileName);
        }

        Image IImageCache.GetImage(string imageFileName)
        {
            if (string.IsNullOrWhiteSpace(imageFileName))
            {
                throw new ArgumentException(nameof(imageFileName));
            }

            if (_cache.TryGetValue(imageFileName, out var image))
            {
                return image;
            }

            image = _inner.GetImage(imageFileName);

            if (image != null)
            {
                _cache.Add(imageFileName, image);
            }

            return image;
        }

        async Task<Image> IImageCache.GetImageAsync(string imageFileName)
        {
            if (string.IsNullOrWhiteSpace(imageFileName))
            {
                throw new ArgumentException(nameof(imageFileName));
            }

            if (_cache.TryGetValue(imageFileName, out var image))
            {
                return image;
            }

            image = await _inner.GetImageAsync(imageFileName);

            if (image != null)
            {
                _cache.Add(imageFileName, image);
            }

            return image;
        }
    }
}