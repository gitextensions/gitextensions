using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using GitCommands;

namespace GitUI.Avatars
{
    public sealed class BackupAvatarProvider : IAvatarProvider
    {
        private readonly IAvatarProvider _inner;
        private readonly Image _backupImage;

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
                return await _inner.GetAvatarAsync(email, name, imageSize) ?? _backupImage;
            }
            catch
            {
                return _backupImage;
            }
        }

        /// <inheritdoc />
        public Task ClearCacheAsync() => _inner.ClearCacheAsync();
    }
}