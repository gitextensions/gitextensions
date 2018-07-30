using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using GitCommands;

namespace GitUI.Avatars
{
    /// <summary>
    /// Decorates an avatar provider, adding persistent caching to the file system.
    /// </summary>
    public sealed class AvatarPersistentCache : IAvatarProvider
    {
        private const int DefaultCacheDays = 30;

        private readonly IAvatarProvider _inner;
        private readonly IFileSystem _fileSystem;

        public AvatarPersistentCache(IAvatarProvider inner, IFileSystem fileSystem = null)
        {
            _inner = inner;
            _fileSystem = fileSystem ?? new FileSystem();
        }

        /// <inheritdoc />
        event Action IAvatarProvider.CacheCleared
        {
            add => _inner.CacheCleared += value;
            remove => _inner.CacheCleared -= value;
        }

        /// <inheritdoc />
        public async Task<Image> GetAvatarAsync(string email, int imageSize)
        {
            var cacheDir = AppSettings.AvatarImageCachePath;
            var path = Path.Combine(cacheDir, $"{email}.{imageSize}px.png");

            var image = ReadImage()
                ?? await _inner.GetAvatarAsync(email, imageSize);

            if (image != null)
            {
                WriteImage();
            }

            return image;

            void WriteImage()
            {
                if (!_fileSystem.Directory.Exists(cacheDir))
                {
                    _fileSystem.Directory.CreateDirectory(cacheDir);
                }

                try
                {
                    using (var output = _fileSystem.File.OpenWrite(path))
                    {
                        image.Save(output, ImageFormat.Png);
                    }
                }
                catch
                {
                    // do nothing
                }
            }

            Image ReadImage()
            {
                if (!HasExpired())
                {
                    try
                    {
                        using (var stream = _fileSystem.File.OpenRead(path))
                        {
                            return Image.FromStream(stream);
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                }

                TryDelete();
                return null;

                bool HasExpired()
                {
                    var info = _fileSystem.FileInfo.FromFileName(path);

                    var cacheDays = AppSettings.AvatarImageCacheDays;
                    if (cacheDays < 1)
                    {
                        cacheDays = DefaultCacheDays;
                    }

                    return !info.Exists ||
                           info.LastWriteTime < DateTime.Now.AddDays(-cacheDays);
                }

                void TryDelete()
                {
                    try
                    {
                        _fileSystem.File.Delete(path);
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
        }

        /// <inheritdoc />
        public async Task ClearCacheAsync()
        {
            var cachePath = AppSettings.AvatarImageCachePath;

            if (_fileSystem.Directory.Exists(cachePath))
            {
                await Task.Run(
                    () =>
                    {
                        foreach (var file in _fileSystem.Directory.GetFiles(cachePath))
                        {
                            try
                            {
                                _fileSystem.File.Delete(file);
                            }
                            catch
                            {
                                // do nothing
                            }
                        }
                    });
            }

            await _inner.ClearCacheAsync();
        }
    }
}
