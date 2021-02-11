using System.Drawing;
using System.Threading.Tasks;

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
        Task<Image?> GetAvatarAsync(string email, string? name, int imageSize);
    }
}
