using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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
        private static readonly ConcurrentDictionary<Uri, (DateTime, Task<Image>)> _downloads = new();

        public async Task<Image> DownloadImageAsync(Uri imageUrl)
        {
            if (imageUrl == null)
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

        private async Task<Image> DownloadAsync(Uri imageUrl)
        {
            // WebClient.OpenReadTaskAsync can block before returning a task, so make sure we
            // make such calls on the thread pool, and limit the number of concurrent requests.

            // Get onto background thread
            await TaskScheduler.Default;

            // Limit the number of concurrent download requests
            await _downloadSemaphore.WaitAsync();

            try
            {
                using var webClient = new WebClient { Proxy = WebRequest.DefaultWebProxy };
                webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;

                using var imageStream = await webClient.OpenReadTaskAsync(imageUrl);
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

            foreach (var pair in _downloads)
            {
                var (time, _) = pair.Value;

                if (now - time > TimeSpan.FromSeconds(30))
                {
                    _downloads.TryRemove(pair.Key, out _);
                }
            }
        }
    }
}
