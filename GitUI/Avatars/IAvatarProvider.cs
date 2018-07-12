using System;
using System.Drawing;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GitUI.Avatars
{
    public interface IAvatarProvider
    {
        /// <summary>
        /// Raised when images cached by this implementation (if any)
        /// </summary>
        event Action CacheCleared;

        /// <summary>
        /// Provides the avatar image for the associated email at the requested size.
        /// </summary>
        [NotNull]
        [ItemCanBeNull]
        Task<Image> GetAvatarAsync([NotNull] string email, int imageSize);

        /// <summary>
        /// Clears any cached images before raising <see cref="CacheCleared"/>.
        /// </summary>
        Task ClearCacheAsync();
    }
}