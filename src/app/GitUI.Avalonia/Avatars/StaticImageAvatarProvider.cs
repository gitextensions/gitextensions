namespace GitUI.Avatars;

public sealed class StaticImageAvatarProvider : IAvatarProvider
{
    private readonly byte[] _image;
    private readonly Lock _sizeCacheLock = new();
    private readonly Dictionary<int, byte[]> _sizeCache = [];

    public StaticImageAvatarProvider(byte[] image)
    {
        _image = image;
        int imageHeight = AvatarImage.GetPixelSize(image)?.Height ?? 0;
        _sizeCache.Add(imageHeight, image);
    }

    public bool PerformsIo => false;

    /// <inheritdoc />
    public Task<byte[]?> GetAvatarAsync(string email, string? name, int imageSize)
        => Task.FromResult<byte[]?>(GetCachedResizedImage(imageSize));

    private byte[] GetCachedResizedImage(int imageSize)
    {
        lock (_sizeCacheLock)
        {
            if (_sizeCache.TryGetValue(imageSize, out byte[]? image))
            {
                return image;
            }

            byte[] resizedImage = AvatarImage.Resize(_image, imageSize);
            _sizeCache.Add(imageSize, resizedImage);
            return resizedImage;
        }
    }
}
