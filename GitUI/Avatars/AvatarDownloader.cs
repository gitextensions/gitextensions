using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.VisualStudio.Threading;

namespace GitUI.Avatars
{
    /// <summary>
    /// Helps downloading avatar images and implements concurrent requests and automatic retries.
    /// </summary>
    public sealed class AvatarDownloader : IAvatarDownloader
    {
        private const int _maxConcurrentDownloads = 10;

        private static readonly SemaphoreSlim _downloadSemaphore = new(initialCount: _maxConcurrentDownloads);
        private static readonly ConcurrentDictionary<Uri, (DateTime, Task<Image?>)> _downloads = new();
        private static readonly HttpClient _client = new(new HttpClientHandler() { UseProxy = true });

        public async Task<Image?> DownloadImageAsync(Uri? imageUrl)
        {
            if (imageUrl is null)
            {
                return null;
            }

            // check network connectivity
            if (!NativeMethods.InternetGetConnectedState(out _, 0))
            {
                return null;
            }

            ClearOldCacheEntries();

            var errorCount = 0;

            while (true)
            {
                var (_, task) = _downloads.GetOrAdd(imageUrl, _ => (DateTime.UtcNow, DownloadAsync(imageUrl)));

                // If we discover a faulted task, remove it and try again
                if (task.IsFaulted || task.IsCanceled)
                {
                    _downloads.TryRemove(imageUrl, out _);

                    if (++errorCount > 3)
                    {
                        await task;
                    }

                    continue;
                }

                return await task;
            }
        }

        private static async Task<Image?> DownloadAsync(Uri imageUrl)
        {
            // Get onto background thread
            await TaskScheduler.Default;

            // Limit the number of concurrent download requests
            await _downloadSemaphore.WaitAsync();

            try
            {
                using HttpResponseMessage response = await _client.GetAsync(imageUrl);
                using Stream imageStream = await response.Content.ReadAsStreamAsync();
                return Image.FromStream(imageStream);
            }
            catch (Exception ex)
            {
                // catch IO errors
                Trace.WriteLine(ex.Message);
            }
            finally
            {
                _downloadSemaphore.Release();
            }

            return null;
        }

        private void ClearOldCacheEntries()
        {
            var now = DateTime.UtcNow;

            foreach ((var key, (DateTime time, Task<Image?> _)) in _downloads)
            {
                if (now - time > TimeSpan.FromSeconds(30))
                {
                    _downloads.TryRemove(key, out _);
                }
            }
        }
    }
}
