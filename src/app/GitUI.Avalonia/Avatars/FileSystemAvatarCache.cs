using System.Diagnostics;
using System.IO.Abstractions;
using GitCommands;

namespace GitUI.Avatars;

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
    public async Task<byte[]?> GetAvatarAsync(string email, string? name, int imageSize)
    {
        if (!_inner.PerformsIo)
        {
            return await _inner.GetAvatarAsync(email, name, imageSize);
        }

        string path = Path.Join(_cacheDir, $"{email}.{imageSize}px.png");
        byte[]? image = ReadImage();

        if (image is not null)
        {
            return image;
        }

        image = await _inner.GetAvatarAsync(email, name, imageSize);

        if (image is not null)
        {
            WriteImage(image);
        }

        return image;

        bool HasExpired()
        {
            IFileInfo info = _fileSystem.FileInfo.New(path);

            if (!info.Exists)
            {
                return true;
            }

            if (AppSettings.AvatarProvider == AvatarProvider.None)
            {
                return false;
            }

            if (info.LastWriteTime < DateTime.Now.AddDays(-_cacheDays))
            {
                TryDelete();
                return true;
            }

            return false;
        }

        byte[]? ReadImage()
        {
            if (HasExpired())
            {
                return null;
            }

            try
            {
                byte[] imageData = _fileSystem.File.ReadAllBytes(path);
                return AvatarImage.GetPixelSize(imageData) is null ? null : imageData;
            }
            catch
            {
                return null;
            }
        }

        void TryDelete()
        {
            try
            {
                _fileSystem.File.Delete(path);
            }
            catch
            {
            }
        }

        void WriteImage(byte[] imageData)
        {
            try
            {
                _fileSystem.File.WriteAllBytes(path, imageData);
            }
            catch
            {
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
                        Trace.WriteLine($"Failed to delete file '{file}'. Error: {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
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
