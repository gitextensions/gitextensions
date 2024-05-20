namespace GitUI.Avatars
{
    public sealed class StaticImageAvatarProvider : IAvatarProvider
    {
        private readonly Image _image;
        private readonly Dictionary<int, Image> _sizeCache = [];

        public StaticImageAvatarProvider(Image image)
        {
            _image = image;
            _sizeCache.Add(_image.Height, image);
        }

        public bool PerformsIo => false;

        /// <inheritdoc />
        public Task<Image?> GetAvatarAsync(string email, string? name, int imageSize)
        {
            return Task.FromResult<Image?>(GetCachedResizedImage(imageSize));
        }

        private Image GetCachedResizedImage(int imageSize)
        {
            lock (_sizeCache)
            {
                if (_sizeCache.TryGetValue(imageSize, out Image image))
                {
                    return image;
                }

                Bitmap resizedImage = new(_image, imageSize, imageSize);
                _sizeCache.Add(imageSize, resizedImage);

                return resizedImage;
            }
        }
    }
}
