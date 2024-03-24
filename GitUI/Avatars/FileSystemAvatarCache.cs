using System.Diagnostics;
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
        private readonly IAvatarProvider _inner;
        private readonly IFileSystem _fileSystem;
        private readonly string _cacheDir;
        private readonly int _cacheDays;

        public FileSystemAvatarCache(IAvatarProvider inner, IFileSystem? fileSystem = null)
        {
            _inner = inner;
            _fileSystem = fileSystem ?? new FileSystem();

            _cacheDays = AppSettings.AvatarImageCacheDays;
            if (_cacheDays < 1)
            {
                const int DefaultCacheDays = 30;
                _cacheDays = DefaultCacheDays;
            }

            _cacheDir = AppSettings.AvatarImageCachePath;
            if (!_fileSystem.Directory.Exists(_cacheDir))
            {
                _fileSystem.Directory.CreateDirectory(_cacheDir);
            }
        }

        /// <inheritdoc />
        public event EventHandler? CacheCleared;

        public bool PerformsIo => true;

        /// <inheritdoc />
        public async Task<Image?> GetAvatarAsync(string email, string? name, int imageSize)
        {
            if (!_inner.PerformsIo)
            {
                return await _inner.GetAvatarAsync(email, name, imageSize);
            }

            string path = Path.Combine(_cacheDir, $"{email}.{imageSize}px.png");

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
                try
                {
                    // Workaround to avoid the "A generic error occurred in GDI+." exception when saving
                    // where copying the image in a new one allows the save on disk to be successful...
                    using Bitmap newImage = new(image);
                    using Stream output = _fileSystem.File.OpenWrite(path);
                    newImage.Save(output, ImageFormat.Png);
                }
                catch
                {
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

                return null;

                bool HasExpired()
                {
                    IFileInfo info = _fileSystem.FileInfo.New(path);

                    if (!info.Exists)
                    {
                        return true;
                    }

                    if (AppSettings.AvatarProvider == AvatarProvider.None)
                    {
                        // No need to refresh because the image returned is always the same.
                        return false;
                    }

                    if (info.LastWriteTime < DateTime.Now.AddDays(-_cacheDays))
                    {
                        TryDelete();
                        return true;
                    }

                    return false;
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
            string cachePath = AppSettings.AvatarImageCachePath;

            if (_fileSystem.Directory.Exists(cachePath))
            {
                try
                {
                    foreach (string file in _fileSystem.Directory.GetFiles(cachePath))
                    {
                        try
                        {
                            _fileSystem.File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            // do nothing
                            Trace.WriteLine($"Failed to delete file '{file}'. Error: {ex}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // do nothing
                    Trace.WriteLine($"Failed to enumerate files. Error: {ex}");
                }
            }

            if (CacheCleared is not null)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                CacheCleared.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
