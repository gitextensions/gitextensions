using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace GitExtUtils
{
    /// <summary>
    /// Associative cache with fixed capacity that expires the last used item first.
    /// </summary>
    /// <typeparam name="TKey">Type of keys in the cache.</typeparam>
    /// <typeparam name="TValue">Type of values in the cache</typeparam>
    public sealed class MruCache<TKey, TValue>
    {
        private readonly Dictionary<TKey, LinkedListNode<Entry>> _nodeByKey;
        private readonly LinkedList<Entry> _entries = new LinkedList<Entry>();

        /// <summary>
        /// Gets the maximum number of entries that this cache will hold.
        /// When full, attempts to add new entries will expire the last used entry.
        /// </summary>
        public int Capacity { get; }

        /// <summary>
        /// Initialises a new instance of <see cref="MruCache{TKey,TValue}"/>.
        /// </summary>
        /// <param name="capacity">The maximum number of entries the cache will hold.</param>
        public MruCache(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Must be greater than zero.");
            }

            Capacity = capacity;
            _nodeByKey = new Dictionary<TKey, LinkedListNode<Entry>>(capacity);
        }

        public IReadOnlyList<TKey> Keys => _nodeByKey.Keys.ToList();

        /// <summary>
        /// Adds an entry to the cache.
        /// </summary>
        /// <remarks>
        /// If an entry already exists with this key, it will be replaced.
        /// </remarks>
        /// <param name="key">The key that uniquely identifies this entry in the cache.</param>
        /// <param name="value">The value to store against the provided key.</param>
        public void Add(TKey key, TValue value)
        {
            var entry = new Entry(key, value);

            if (_nodeByKey.TryGetValue(key, out var node))
            {
                node.Value = entry;
                _entries.Remove(node);
                _entries.AddFirst(node);
            }
            else
            {
                if (_entries.Count == Capacity)
                {
                    var last = _entries.Last;
                    _entries.RemoveLast();
                    _nodeByKey.Remove(last.Value.Key);
                }

                node = _entries.AddFirst(entry);
                _nodeByKey[key] = node;
            }
        }

        /// <summary>
        /// Clears all items from the cache.
        /// </summary>
        public void Clear()
        {
            _nodeByKey.Clear();
            _entries.Clear();
        }

        /// <summary>
        /// Finds a cache entry, if it exists.
        /// </summary>
        /// <param name="key">The key that uniquely identifies the cache entry.</param>
        /// <param name="value">The cached value if found, otherwise <c>default</c>.</param>
        /// <returns><c>true</c> if <paramref name="key"/> exists in the cache, otherwise <c>false</c>.</returns>
        [ContractAnnotation("=>true,value:notnull")]
        [ContractAnnotation("=>false,value:null")]
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (!_nodeByKey.TryGetValue(key, out var node))
            {
                value = default;
                return false;
            }

            _entries.Remove(node);
            _entries.AddFirst(node);

            value = node.Value.Value;
            return true;
        }

        /// <summary>
        /// Removes an entry from the cache, if it exists.
        /// </summary>
        /// <param name="key">The key that uniquely identifies the cache entry.</param>
        /// <param name="value">The removed value if found, otherwise <c>default</c>.</param>
        /// <returns><c>true</c> if <paramref name="key"/> was removed from the cache, otherwise <c>false</c>.</returns>
        [ContractAnnotation("=>true,value:notnull")]
        [ContractAnnotation("=>false,value:null")]
        public bool TryRemove(TKey key, out TValue value)
        {
            if (!_nodeByKey.TryGetValue(key, out var node))
            {
                value = default;
                return false;
            }

            value = node.Value.Value;
            _nodeByKey.Remove(key);
            _entries.Remove(node);
            return true;
        }

        private readonly struct Entry
        {
            public TKey Key { get; }
            public TValue Value { get; }

            public Entry(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }
        }
    }
}