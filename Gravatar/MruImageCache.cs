using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Gravatar
{
    /// <summary>
    /// Caches most-recently-used images.
    /// </summary>
    /// <remarks>
    /// Decorates an inner cache, delegating to it as needed.
    /// <para />
    /// If an image is available in memory, the inner cache can be bypassed.
    /// </remarks>
    public sealed class MruImageCache : IImageCache
    {
        private const int CleanAtSize = 30;
        private const int CleanToSize = 25;

        private readonly ConcurrentDictionary<string, Entry> _entryByFileName = new ConcurrentDictionary<string, Entry>();
        private readonly IImageCache _inner;

        public MruImageCache(IImageCache inner)
        {
            _inner = inner;
        }

        event EventHandler IImageCache.Invalidated
        {
            add => _inner.Invalidated += value;
            remove => _inner.Invalidated -= value;
        }

        void IImageCache.AddImage(string imageFileName, Image image)
        {
            _inner.AddImage(imageFileName, image);
            UpdateEntry(imageFileName, image);
        }

        Task IImageCache.ClearAsync()
        {
            _entryByFileName.Clear();
            return _inner.ClearAsync();
        }

        Task IImageCache.DeleteImageAsync(string imageFileName)
        {
            _entryByFileName.TryRemove(imageFileName, out _);
            return _inner.DeleteImageAsync(imageFileName);
        }

        Image IImageCache.GetImage(string imageFileName)
        {
            if (_entryByFileName.TryGetValue(imageFileName, out var entry))
            {
                entry.LastAccesedAt = DateTime.UtcNow;
                return entry.Image;
            }

            return _inner.GetImage(imageFileName);
        }

        async Task<Image> IImageCache.GetImageAsync(string imageFileName)
        {
            if (_entryByFileName.TryGetValue(imageFileName, out var entry))
            {
                entry.LastAccesedAt = DateTime.UtcNow;
                return entry.Image;
            }

            var image = await _inner.GetImageAsync(imageFileName);

            UpdateEntry(imageFileName, image);

            return image;
        }

        private void UpdateEntry(string imageFileName, Image image)
        {
            _entryByFileName[imageFileName] = new Entry(imageFileName, image);

            if (_entryByFileName.Count > CleanAtSize)
            {
                RemoveOldEntries();
            }

            void RemoveOldEntries()
            {
                var sortedEntries = new SortedList<DateTime, Entry>(_entryByFileName.Count);

                // Build list of all entries, sorted by their last access times
                foreach (var entry in _entryByFileName.Values)
                {
                    sortedEntries.Add(entry.LastAccesedAt, entry);
                }

                // The first items in that sorted list are the oldest ones.
                // Take as many as we need to remove, and remove them from the dictionary.
                foreach (var entry in sortedEntries.Values.Take(_entryByFileName.Count - CleanToSize))
                {
                    _entryByFileName.TryRemove(entry.FileName, out _);
                }
            }
        }

        private sealed class Entry
        {
            public string FileName { get; }
            public Image Image { get; }
            public DateTime LastAccesedAt { get; set; }

            public Entry(string fileName, Image image)
            {
                FileName = fileName;
                Image = image;
                LastAccesedAt = DateTime.UtcNow;
            }
        }
    }
}