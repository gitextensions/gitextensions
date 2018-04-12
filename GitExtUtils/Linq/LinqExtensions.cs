using System.Collections.Generic;

namespace System.Linq
{
    public static class LinqExtensions
    {
        public static HashSet<TKey> ToHashSet<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            HashSet<TKey> result = new HashSet<TKey>();

            foreach (TSource sourceElement in source)
            {
                TKey key = keySelector(sourceElement);

                if (key == null)
                {
                    var ex = new ArgumentException("Key selector produced a key that is null. See exception data for source.", nameof(keySelector));
                    ex.Data.Add("source", sourceElement);
                    throw ex;
                }

                result.Add(key);
            }

            return result;
        }

        public static IEnumerable<TValue> SelectMany<TValue>(
            this IEnumerable<IEnumerable<TValue>> source)
        {
            return source.SelectMany(i => i);
        }

        public static string Join(this IEnumerable<string> source, string separator)
        {
            return string.Join(separator, source);
        }

        /// <summary>
        /// Sorts the elements of a sequence in ascending order by using a specified <paramref name="comparer"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <param name="comparer">A function to compare keys.</param>
        /// <returns>An <see cref="IOrderedEnumerable{TElement}"/> whose elements are sorted according to a key.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <c>null</c>.</exception>
        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, TKey, int> comparer)
        {
            FuncComparer<TKey> fc = new FuncComparer<TKey>(comparer);

            return source.OrderBy(keySelector, fc);
        }

        private class FuncComparer<T> : IComparer<T>
        {
            private readonly Func<T, T, int> _comparer;

            public FuncComparer(Func<T, T, int> comparer)
            {
                _comparer = comparer;
            }

            public int Compare(T x, T y)
            {
                return _comparer(x, y);
            }
        }

        /// <summary>
        /// Transforms each element of an array into a new form.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to invoke a transform function on.</param>
        /// <param name="transformer">A transform function to apply to each element.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="transformer"/> is <c>null</c>.</exception>
        public static void Transform<TSource>(this TSource[] source, Func<TSource, TSource> transformer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (transformer == null)
            {
                throw new ArgumentNullException(nameof(transformer));
            }

            for (int i = 0; i < source.Length; i++)
            {
                source[i] = transformer(source[i]);
            }
        }

        public static void AddAll<T>(this IList<T> list, IEnumerable<T> elementsToAdd)
        {
            foreach (T t in elementsToAdd)
            {
                list.Add(t);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T t in enumerable)
            {
                action(t);
            }
        }

        public static void Swap<T>(this IList<T> list, int index1, int index2)
        {
            var temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
        }

        public static IReadOnlyList<T> AsReadOnlyList<T>(this IEnumerable<T> source)
        {
            if (source is IReadOnlyList<T> readOnlyList)
            {
                return readOnlyList;
            }

            if (source is ICollection<T> collection)
            {
                var count = collection.Count;

                if (count == 0)
                {
                    return Array.Empty<T>();
                }

                var items = new T[count];
                collection.CopyTo(items, 0);
                return items;
            }

            using (var e = source.GetEnumerator())
            {
                if (!e.MoveNext())
                {
                    return Array.Empty<T>();
                }

                var list = new List<T>();

                do
                {
                    list.Add(e.Current);
                }
                while (e.MoveNext());

                return list;
            }
        }
    }
}
