using GitExtUtils;

namespace GitUI.Avatars;

/// <summary>
/// Caches most-recently-used encoded images.
/// </summary>
/// <remarks>
/// <para>Decorates an inner cache, delegating to it as needed.</para>
/// <para>If an image is available in memory, the inner cache can be bypassed.</para>
/// </remarks>
public sealed class AvatarMemoryCache : IAvatarProvider, IAvatarCacheCleaner
{
    private readonly Lock _cacheLock = new();
    private readonly MruCache<(string email, int imageSize), byte[]> _cache;
    private readonly Lock _requestedLock = new();
    private readonly HashSet<(string email, int imageSize)> _requested = new(6);
    private readonly IAvatarProvider _inner;

    public AvatarMemoryCache(IAvatarProvider inner, int capacity = 30)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _cache = new MruCache<(string email, int imageSize), byte[]>(capacity);
    }

    public bool PerformsIo => false;

    /// <inheritdoc />
    public event EventHandler? CacheCleared;

    /// <inheritdoc />
    public async Task<byte[]?> GetAvatarAsync(string email, string? name, int imageSize)
    {
        (string email, int imageSize) key = (email, imageSize);
        lock (_cacheLock)
        {
            if (_cache.TryGetValue(key, out byte[]? cachedImage))
            {
                return cachedImage;
            }
        }

        if (IsRequestInProgress(key))
        {
            for (int i = 0; i < 10_000; i++)
            {
                lock (_cacheLock)
                {
                    if (_cache.TryGetValue(key, out byte[]? cachedImage))
                    {
                        return cachedImage;
                    }
                }

                if (!IsRequestInProgress(key))
                {
                    break;
                }

                await Task.Delay(5);
            }
        }

        try
        {
            lock (_requestedLock)
            {
                _requested.Add(key);
            }

            byte[]? image = await _inner.GetAvatarAsync(email, name, imageSize);

            if (image is not null)
            {
                lock (_cacheLock)
                {
                    _cache.Add(key, image);
                }
            }

            return image;
        }
        finally
        {
            lock (_requestedLock)
            {
                _requested.Remove(key);
            }
        }

        bool IsRequestInProgress((string email, int imageSize) avatarKey)
        {
            lock (_requestedLock)
            {
                return _requested.Contains(avatarKey);
            }
        }
    }

    /// <inheritdoc />
    public Task ClearCacheAsync()
    {
        lock (_cacheLock)
        {
            _cache.Clear();
        }

        lock (_requestedLock)
        {
            _requested.Clear();
        }

        CacheCleared?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }
}
