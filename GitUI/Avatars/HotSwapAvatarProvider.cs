using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;

namespace GitUI.Avatars
{
    /// <summary>
    /// A helper provider that wraps another avatar provider.
    /// </summary>
    /// <remarks>
    /// The wrapper is used to support hot swapping (changing a provider without changing the reference of the root provider)
    /// It also catches and logs exceptions and works as a simple NullProvider if <see cref="Provider"/> is not set (set to null).
    /// </remarks>
    public sealed class HotSwapAvatarProvider : IAvatarProvider
    {
        /// <summary>
        /// Gets or sets the currently active provider.
        /// </summary>
        public IAvatarProvider? Provider { get; set; }

        public Task<Image?> GetAvatarAsync(string email, string? name, int imageSize)
        {
            try
            {
                return Provider?.GetAvatarAsync(email, name, imageSize) ?? Task.FromResult<Image?>(null);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return Task.FromResult<Image?>(null);
            }
        }
    }
}
