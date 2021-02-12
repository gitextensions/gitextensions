using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace GitUI.Avatars
{
    /// <summary>
    /// An avatar provider that combines multiple avatar providers.
    /// </summary>
    public sealed class ChainedAvatarProvider : IAvatarProvider
    {
        private readonly IAvatarProvider[] _avatarProviders;

        public ChainedAvatarProvider(params IAvatarProvider[] avatarProviders)
        {
            _avatarProviders = avatarProviders ?? throw new ArgumentNullException(nameof(avatarProviders));

            if (_avatarProviders.Any(p => p is null))
            {
                throw new ArgumentNullException();
            }
        }

        public ChainedAvatarProvider(IEnumerable<IAvatarProvider> avatarProviders)
        {
            if (avatarProviders is null)
            {
                throw new ArgumentNullException(nameof(avatarProviders));
            }

            _avatarProviders = avatarProviders.ToArray();
        }

        /// <summary>
        /// Gets an avatar images from multiple avatar providers and returns the first hit.
        /// The providers are queried in the same order that was given during construction.
        /// </summary>
        public async Task<Image?> GetAvatarAsync(string email, string? name, int imageSize)
        {
            foreach (var provider in _avatarProviders)
            {
                Image? avatar = null;

                try
                {
                    avatar = await provider.GetAvatarAsync(email, name, imageSize);
                }
                catch
                {
                    // ignore and continue with next provider
                }

                if (avatar is not null)
                {
                    return avatar;
                }
            }

            return null;
        }
    }
}
