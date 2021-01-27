using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace GitUI.Avatars
{
    public sealed class StaticImageAvatarProvider : IAvatarProvider
    {
        private readonly Image _image;
        private readonly Dictionary<int, Image> _sizeCache = new Dictionary<int, Image>();

        public StaticImageAvatarProvider(Image image)
        {
            _image = image;
            _sizeCache.Add(_image.Height, image);
        }

        /// <inheritdoc />
        public Task<Image> GetAvatarAsync(string email, string name, int imageSize)
        {
            return Task.FromResult(GetCachedResizedImage(imageSize));
        }

        private Image GetCachedResizedImage(int imageSize)
        {
            lock (_sizeCache)
            {
                if (_sizeCache.TryGetValue(imageSize, out var image))
                {
                    return image;
                }

                var resizedImage = new Bitmap(_image, new Size(imageSize, imageSize));
                _sizeCache.Add(imageSize, resizedImage);

                return resizedImage;
            }
        }
    }
}
