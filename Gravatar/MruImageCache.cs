using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

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
        private readonly int _cleanAtSize;
        private readonly int _cleanToSize;

        private readonly ConcurrentDictionary<string, Entry> _entryByFileName = new ConcurrentDictionary<string, Entry>();
        private readonly IImageCache _inner;

        public MruImageCache([NotNull] IImageCache inner, int cleanAtSize = 30, int cleanToSize = 25)
        {
            if (cleanToSize >= cleanAtSize)
            {
                throw new ArgumentException($"{nameof(cleanAtSize)} must be less than {nameof(cleanToSize)}.");
            }

            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _cleanToSize = cleanToSize;
            _cleanAtSize = cleanAtSize;
        }

        event EventHandler IImageCache.Invalidated
        {
            add => _inner.Invalidated += value;
            remove => _inner.Invalidated -= value;
        }

        void IImageCache.AddImage(string imageFileName, Image image)
        {
            if (string.IsNullOrWhiteSpace(imageFileName))
            {
                throw new ArgumentException(nameof(imageFileName));
            }

            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

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
            if (string.IsNullOrWhiteSpace(imageFileName))
            {
                throw new ArgumentException(nameof(imageFileName));
            }

            _entryByFileName.TryRemove(imageFileName, out _);
            return _inner.DeleteImageAsync(imageFileName);
        }

        Image IImageCache.GetImage(string imageFileName)
        {
            if (string.IsNullOrWhiteSpace(imageFileName))
            {
                throw new ArgumentException(nameof(imageFileName));
            }

            if (_entryByFileName.TryGetValue(imageFileName, out var entry))
            {
                entry.LastAccesedAt = DateTime.UtcNow;
                return entry.Image;
            }

            var image = _inner.GetImage(imageFileName);

            if (image != null)
            {
                UpdateEntry(imageFileName, image);
            }

            return image;
        }

        async Task<Image> IImageCache.GetImageAsync(string imageFileName)
        {
            if (string.IsNullOrWhiteSpace(imageFileName))
            {
                throw new ArgumentException(nameof(imageFileName));
            }

            if (_entryByFileName.TryGetValue(imageFileName, out var entry))
            {
                entry.LastAccesedAt = DateTime.UtcNow;
                return entry.Image;
            }

            var image = await _inner.GetImageAsync(imageFileName);

            if (image != null)
            {
                UpdateEntry(imageFileName, image);
            }

            return image;
        }

        private void UpdateEntry([NotNull] string imageFileName, [NotNull] Image image)
        {
            if (string.IsNullOrWhiteSpace(imageFileName))
            {
                throw new ArgumentException(nameof(imageFileName));
            }

            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            _entryByFileName[imageFileName] = new Entry(imageFileName, image);

            if (_entryByFileName.Count > _cleanAtSize)
            {
                RemoveOldEntries();
            }

            void RemoveOldEntries()
            {
                // Build list of all entries, sorted by their last access times
                var sortedEntries = _entryByFileName.Values.OrderBy(e => e.LastAccesedAt).ToList();

                // The first items in that sorted list are the oldest ones.
                // Take as many as we need to remove, and remove them from the dictionary.
                foreach (var entry in sortedEntries.Take(_entryByFileName.Count - _cleanToSize))
                {
                    _entryByFileName.TryRemove(entry.FileName, out _);
                }
            }
        }

        private sealed class Entry
        {
            [NotNull] public string FileName { get; }
            [NotNull] public Image Image { get; }
            public DateTime LastAccesedAt { get; set; }

            public Entry([NotNull] string fileName, [NotNull] Image image)
            {
                FileName = fileName;
                Image = image;
                LastAccesedAt = DateTime.UtcNow;
            }
        }
    }
}