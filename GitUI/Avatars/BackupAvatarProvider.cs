using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace GitUI.Avatars
{
    public sealed class BackupAvatarProvider : IAvatarProvider
    {
        private readonly IAvatarProvider _inner;
        private readonly Image _backupImage;
        private readonly Dictionary<int, Image> _cacheBackupImage = new Dictionary<int, Image>();

        public BackupAvatarProvider(IAvatarProvider inner, Image backupImage)
        {
            _inner = inner;
            _backupImage = backupImage;
        }

        /// <inheritdoc />
        public event Action CacheCleared
        {
            add => _inner.CacheCleared += value;
            remove => _inner.CacheCleared -= value;
        }

        /// <inheritdoc />
        public async Task<Image> GetAvatarAsync(string email, string name, int imageSize)
        {
            try
            {
                return await _inner.GetAvatarAsync(email, name, imageSize) ?? BackupImageResized();
            }
            catch
            {
                return BackupImageResized();
            }

            Image BackupImageResized()
            {
                lock (_cacheBackupImage)
                {
                    if (_cacheBackupImage.TryGetValue(imageSize, out var image))
                    {
                        return image;
                    }

                    var backupImageResized = (imageSize == _backupImage.Height)
                        ? _backupImage
                        : new Bitmap(_backupImage, new Size(imageSize, imageSize));
                    _cacheBackupImage.Add(imageSize, backupImageResized);
                    return backupImageResized;
                }
            }
        }

        /// <inheritdoc />
        public Task ClearCacheAsync()
        {
            _cacheBackupImage.Clear();
            return _inner.ClearCacheAsync();
        }
    }
}
