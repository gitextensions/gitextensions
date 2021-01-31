using System;
using System.Threading.Tasks;

namespace GitUI.Avatars
{
    /// <summary>
    /// Allows the consumer to clear a cache.
    /// </summary>
    public interface IAvatarCacheCleaner
    {
        /// <summary>
        /// Raised after <see cref="ClearCacheAsync"/> is finished clearing the cache.
        /// </summary>
        event EventHandler CacheCleared;

        /// <summary>
        /// Clears any cached content before raising <see cref="CacheCleared"/>.
        /// </summary>
        Task ClearCacheAsync();
    }
}
