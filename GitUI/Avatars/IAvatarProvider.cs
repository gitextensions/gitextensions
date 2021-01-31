using System.Drawing;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GitUI.Avatars
{
    /// <summary>
    /// A source and cache agnostic provider for avatar images.
    /// </summary>
    public interface IAvatarProvider
    {
        /// <summary>
        /// Provides the avatar image for the associated email at the requested size.
        /// </summary>
        [NotNull]
        [ItemCanBeNull]
        Task<Image> GetAvatarAsync([NotNull] string email, string name, int imageSize);
    }
}
