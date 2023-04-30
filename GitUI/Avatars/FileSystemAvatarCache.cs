using System.Drawing.Imaging;
using System.IO.Abstractions;
using GitCommands;

namespace GitUI.Avatars
{
    /// <summary>
    /// Decorates an avatar provider, adding persistent caching to the file system.
    /// </summary>
    public sealed class FileSystemAvatarCache : IAvatarProvider, IAvatarCacheCleaner
    {
        private const int DefaultCacheDays = 30;

        private readonly IAvatarProvider _inner;
        private readonly IFileSystem _fileSystem;

        public FileSystemAvatarCache(IAvatarProvider inner, IFileSystem? fileSystem = null)
        {
            _inner = inner;
            _fileSystem = fileSystem ?? new FileSystem();
        }

        /// <inheritdoc />
        public event EventHandler? CacheCleared;

        /// <inheritdoc />
        public async Task<Image?> GetAvatarAsync(string email, string? name, int imageSize)
        {
            string cacheDir = AppSettings.AvatarImageCachePath;
            string path = Path.Combine(cacheDir, $"{email}.{imageSize}px.png");

            Image image = ReadImage();

            if (image is not null)
            {
                return image;
            }

            image = await _inner.GetAvatarAsync(email, name, imageSize);

            if (image is not null)
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
                    using Stream output = _fileSystem.File.OpenWrite(path);
                    image.Save(output, ImageFormat.Png);
                }
                catch
                {
                    // Workaround to avoid the "A generic error occurred in GDI+." exception when saving
                    // where copying the image in a new one allows the save on disk to be successful...
                    try
                    {
                        using (Bitmap newImage = new(image))
                        {
                            using Stream output = _fileSystem.File.OpenWrite(path);
                            newImage.Save(output, ImageFormat.Png);
                        }
                    }
                    catch
                    {
                    }
                }
            }

            Image? ReadImage()
            {
                if (!HasExpired())
                {
                    try
                    {
                        using Stream stream = _fileSystem.File.OpenRead(path);
                        return Image.FromStream(stream);
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
                    IFileInfo info = _fileSystem.FileInfo.FromFileName(path);

                    if (!info.Exists)
                    {
                        return true;
                    }

                    if (AppSettings.AvatarProvider == AvatarProvider.None)
                    {
                        // No need to refresh because the image returned is always the same.
                        return false;
                    }

                    int cacheDays = AppSettings.AvatarImageCacheDays;
                    if (cacheDays < 1)
                    {
                        cacheDays = DefaultCacheDays;
                    }

                    return info.LastWriteTime < DateTime.Now.AddDays(-cacheDays);
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

            CacheCleared?.Invoke(this, EventArgs.Empty);
        }
    }
}
