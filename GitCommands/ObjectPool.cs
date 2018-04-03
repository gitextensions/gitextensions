using System.Collections.Generic;

namespace GitCommands
{
    public sealed class ObjectPool<T>
    {
        private readonly Dictionary<T, T> _items;

        public ObjectPool(IEqualityComparer<T> comparer = null, int capacity = 16)
        {
            _items = new Dictionary<T, T>(capacity, comparer);
        }

        public T Intern(T o)
        {
            if (_items.TryGetValue(o, out var cached))
            {
                return cached;
            }

            _items[o] = o;
            return o;
        }

        public int Count => _items.Count;
    }
}