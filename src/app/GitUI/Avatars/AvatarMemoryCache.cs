using GitExtUtils;

namespace GitUI.Avatars
{
    /// <summary>
    /// Caches most-recently-used images.
    /// </summary>
    /// <remarks>
    /// <para>Decorates an inner cache, delegating to it as needed.</para>
    /// <para>If an image is available in memory, the inner cache can be bypassed.</para>
    /// </remarks>
    public sealed class AvatarMemoryCache : IAvatarProvider, IAvatarCacheCleaner
    {
        private readonly MruCache<(string email, int imageSize), Image> _cache;
        private HashSet<(string email, int imageSize)> _requested = new(6);
        private readonly IAvatarProvider _inner;

        public AvatarMemoryCache(IAvatarProvider inner, int capacity = 30)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _cache = new MruCache<(string email, int imageSize), Image>(capacity);
        }

        public bool PerformsIo => false;

        /// <inheritdoc />
        public event EventHandler? CacheCleared;

        /// <inheritdoc />
        public async Task<Image?> GetAvatarAsync(string email, string? name, int imageSize)
        {
            (string email, int imageSize) key = (email, imageSize);
            lock (_cache)
            {
                if (_cache.TryGetValue(key, out Image? cachedImage))
                {
                    return cachedImage;
                }
            }

            // The revision grid could trigger same request multiple times at the same time
            // => Wait that the first finish and result is added to the cache.
            if (IsRequestInProgress(key))
            {
                for (int i = 0; i < 10_000; i++)
                {
                    lock (_cache)
                    {
                        if (_cache.TryGetValue(key, out Image? cachedImage))
                        {
                            return cachedImage;
                        }
                    }

                    if (!IsRequestInProgress(key))
                    {
                        // Early exit when the image is not in the cache and key is no more in the requests list
                        // => the request has failed!
                        break;
                    }

                    await Task.Delay(5); // Approximative time to read from disk to not slow down the grid scrolling
                }
            }

            try
            {
                lock (_requested)
                {
                    _requested.Add(key);
                }

                Image image = await _inner.GetAvatarAsync(email, name, imageSize);

                if (image is not null)
                {
                    lock (_cache)
                    {
                        _cache.Add(key, image);
                    }
                }

                return image;
            }
            finally
            {
                lock (_requested)
                {
                    _requested.Remove(key);
                }
            }

            bool IsRequestInProgress((string email, int imageSize) avatarKey)
            {
                lock (_requested)
                {
                    return _requested.Contains(avatarKey);
                }
            }
        }

        /// <inheritdoc />
        public Task ClearCacheAsync()
        {
            lock (_cache)
            {
                foreach ((string email, int imageSize) key in _cache.Keys)
                {
                    if (_cache.TryGetValue(key, out Image cachedImage))
                    {
                        cachedImage.Dispose();
                    }
                }

                _cache.Clear();
            }

            lock (_requested)
            {
                _requested.Clear();
            }

            CacheCleared?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }
    }
}
