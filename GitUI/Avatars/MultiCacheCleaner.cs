using System;
using System.Linq;
using System.Threading.Tasks;

namespace GitUI.Avatars
{
    /// <summary>
    /// Wraps multiple caches and clears all of them with a single call.
    /// </summary>
    public sealed class MultiCacheCleaner : IAvatarCacheCleaner
    {
        private readonly IAvatarCacheCleaner[] _inner;

        public MultiCacheCleaner(params IAvatarCacheCleaner[] inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));

            if (_inner.Any(p => p == null))
            {
                throw new ArgumentNullException();
            }
        }

        /// <inheritdoc/>
        public event EventHandler? CacheCleared;

        /// <inheritdoc/>
        public async Task ClearCacheAsync()
        {
            foreach (var cacheCleaner in _inner)
            {
                await cacheCleaner.ClearCacheAsync();
            }

            CacheCleared?.Invoke(this, EventArgs.Empty);
        }
    }
}
