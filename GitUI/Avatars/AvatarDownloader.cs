﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GitCommands;
using Microsoft.VisualStudio.Threading;

namespace GitUI.Avatars
{
    public sealed class AvatarDownloader : IAvatarProvider
    {
        private readonly IAvatarGenerator _avatarGenerator;
        private const int MaxConcurrentDownloads = 10;

        /* A brief skim through the Git Extensions repo history shows GitHub emails with the following user names:
         *
         * 25421792+mserfli
         * 33052757+freza-tm
         * gpongelli
         * odie2
         * palver123
         * RaMMicHaeL
         * SamuelLongchamps
         */
        private static readonly Regex _gitHubEmailRegex = new(@"^(\d+\+)?(?<username>[^@]+)@users\.noreply\.github\.com$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly SemaphoreSlim _downloadSemaphore = new(initialCount: MaxConcurrentDownloads);

        private static readonly ConcurrentDictionary<Uri, (DateTime, Task<Image>)> _downloads = new ConcurrentDictionary<Uri, (DateTime, Task<Image>)>();

        /// <inheritdoc />
        public event Action CacheCleared;

        public AvatarDownloader(IAvatarGenerator avatarGenerator)
        {
            _avatarGenerator = avatarGenerator;
        }

        /// <inheritdoc />
        public async Task<Image> GetAvatarAsync(string email, string name, int imageSize)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException(nameof(email));
            }

            if (AppSettings.AvatarProvider == AvatarProvider.AuthorInitials)
            {
                return _avatarGenerator.GenerateAvatarImage(email, name, imageSize);
            }

            // check network connectivity
            if (!NativeMethods.InternetGetConnectedState(out _, 0))
            {
                return null;
            }

            var match = _gitHubEmailRegex.Match(email);

            return match.Success
                ? await LoadFromGitHubAsync()
                : await LoadFromGravatarAsync();

            async Task<Image> LoadFromGitHubAsync()
            {
                try
                {
                    var username = match.Groups["username"].Value;
                    var client = new Git.hub.Client();
                    var user = await client.GetUserAsync(username);
                    if (!string.IsNullOrEmpty(user?.AvatarUrl))
                    {
                        var builder = new UriBuilder(user.AvatarUrl);
                        var query = new StringBuilder(builder.Query.TrimStart('?'));
                        query.Append(query.Length == 0 ? "?" : "&");
                        query.Append("s=").Append(imageSize);
                        builder.Query = query.ToString();
                        var imageUrl = builder.Uri;

                        return await DownloadImageAsync(imageUrl);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }

                return null;
            }

            Task<Image> LoadFromGravatarAsync()
            {
                return DownloadImageAsync(BuildGravatarUrl());

                Uri BuildGravatarUrl()
                {
                    var d = GetDefaultImageString();
                    var hash = CalculateHash();

                    return new Uri($"http://www.gravatar.com/avatar/{hash}?s={imageSize}&r=g&d={d}");

                    string GetDefaultImageString()
                    {
                        switch (AppSettings.GravatarFallbackAvatarType)
                        {
                            case GravatarFallbackAvatarType.Identicon: return "identicon";
                            case GravatarFallbackAvatarType.MonsterId: return "monsterid";
                            case GravatarFallbackAvatarType.Wavatar: return "wavatar";
                            case GravatarFallbackAvatarType.Retro: return "retro";
                            case GravatarFallbackAvatarType.Robohash: return "robohash";
                            case GravatarFallbackAvatarType.AuthorInitials: return "404";
                            default: return "404";
                        }
                    }

                    string CalculateHash()
                    {
                        // Hash specified at http://en.gravatar.com/site/implement/hash/
                        using (var md5 = new MD5CryptoServiceProvider())
                        {
                            // Gravatar doesn't specify an encoding
                            var emailBytes = Encoding.UTF8.GetBytes(email.Trim().ToLowerInvariant());

                            var hashBytes = md5.ComputeHash(emailBytes);

                            // TODO can combine with more efficient logic from ObjectId if that PR is merged
                            var builder = new StringBuilder(capacity: 32);

                            foreach (var b in hashBytes)
                            {
                                builder.Append(b.ToString("x2"));
                            }

                            return builder.ToString();
                        }
                    }
                }
            }

            async Task<Image> DownloadImageAsync(Uri imageUrl)
            {
                ClearOldCacheEntries();

                var errorCount = 0;

                while (true)
                {
                    var (_, task) = _downloads.GetOrAdd(imageUrl, _ => (DateTime.UtcNow, Download()));

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

                async Task<Image> Download()
                {
                    // WebClient.OpenReadTaskAsync can block before returning a task, so make sure we
                    // make such calls on the thread pool, and limit the number of concurrent requests.

                    // Get onto background thread
                    await TaskScheduler.Default;

                    // Limit the number of concurrent download requests
                    await _downloadSemaphore.WaitAsync();

                    try
                    {
                        using (var webClient = new WebClient { Proxy = WebRequest.DefaultWebProxy })
                        {
                            webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;

                            using (var imageStream = await webClient.OpenReadTaskAsync(imageUrl))
                            {
                                return Image.FromStream(imageStream);
                            }
                        }
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

                void ClearOldCacheEntries()
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

        /// <inheritdoc />
        public Task ClearCacheAsync()
        {
            CacheCleared?.Invoke();

            return Task.CompletedTask;
        }
    }
}