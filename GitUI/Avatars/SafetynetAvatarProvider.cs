using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;

namespace GitUI.Avatars
{
    /// <summary>
    /// A provider proxy that makes sure that the requested image size is reasonable.
    /// </summary>
    /// <remarks>
    /// If the image size is less than one it will be set to a default value (64px)
    /// and the upper size is limited to 512px to prevent unreasonable avatar sizes.
    ///
    /// If the inner provider crashes or returns null an "emergency fallback" is provided.
    /// </remarks>
    public sealed class SafetynetAvatarProvider : IAvatarProvider
    {
        private const int _upperSizeLimit = 512;
        private const int _defaultSize = 64;

        private readonly IAvatarProvider _avatarProvider;
        private readonly Lazy<Image> _safetyNetFallback = new(GenerateSafetynetFallback);

        public SafetynetAvatarProvider(IAvatarProvider avatarProvider)
        {
            _avatarProvider = avatarProvider ?? throw new ArgumentNullException(nameof(avatarProvider));
        }

        public async Task<Image?> GetAvatarAsync(string email, string? name, int imageSize)
        {
            if (imageSize < 1)
            {
                imageSize = _defaultSize;
            }

            if (imageSize > _upperSizeLimit)
            {
                imageSize = _upperSizeLimit;
            }

            try
            {
                var image = await _avatarProvider.GetAvatarAsync(email, name, imageSize);

                if (image is not null)
                {
                    return image;
                }
            }
            catch (Exception ex)
            {
                // Something went wrong. Log, ignore and proceed with fallback.
                Trace.WriteLine(ex.Message);
            }

            return _safetyNetFallback.Value;
        }

        private static Image GenerateSafetynetFallback()
        {
            Bitmap bmp = new(1, 1);
            bmp.SetPixel(0, 0, Color.Red);
            return bmp;
        }
    }
}
