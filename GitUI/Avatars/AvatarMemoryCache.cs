using System;
using System.Drawing;
using System.Threading.Tasks;
using GitExtUtils;
using JetBrains.Annotations;

namespace GitUI.Avatars
{
    /// <summary>
    /// Caches most-recently-used images.
    /// </summary>
    /// <remarks>
    /// <para>Decorates an inner cache, delegating to it as needed.</para>
    /// <para>If an image is available in memory, the inner cache can be bypassed.</para>
    /// </remarks>
    public sealed class AvatarMemoryCache : IAvatarProvider
    {
        private readonly MruCache<(string email, int imageSize), Image> _cache;
        private readonly IAvatarProvider _inner;

        public AvatarMemoryCache([NotNull] IAvatarProvider inner, int capacity = 30)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _cache = new MruCache<(string email, int imageSize), Image>(capacity);
        }

        /// <inheritdoc />
        event Action IAvatarProvider.CacheCleared
        {
            add => _inner.CacheCleared += value;
            remove => _inner.CacheCleared -= value;
        }

        /// <inheritdoc />
        public async Task<Image> GetAvatarAsync(string email, int imageSize)
        {
            lock (_cache)
            {
                if (_cache.TryGetValue((email, imageSize), out var cachedImage))
                {
                    return cachedImage;
                }
            }

            var image = await _inner.GetAvatarAsync(email, imageSize);

            lock (_cache)
            {
                _cache.Add((email, imageSize), image);

                return image;
            }
        }

        /// <inheritdoc />
        public Task ClearCacheAsync()
        {
            lock (_cache)
            {
                _cache.Clear();
            }

            return _inner.ClearCacheAsync();
        }
    }
}