using System.Collections.Generic;

namespace System.Linq
{
    public static class LinqExtensions
    {
        /// <summary>
        /// Creates a <see cref="Dictionary{TKey,TValue}"/> from an <see cref="IEnumerable{T}"/> according to a specified
        /// <paramref name="keySelector"/> function.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> to create a <see cref="Dictionary{TKey,TValue}"/> from.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> that contains keys and values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <c>null</c>
        /// or <paramref name="keySelector"/> produces a key that is <c>null</c>.</exception>
        public static Dictionary<TKey, List<TSource>> ToDictionaryOfList<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            Dictionary<TKey, List<TSource>> result = new Dictionary<TKey, List<TSource>>();

            foreach (TSource sourceElement in source)
            {
                TKey key = keySelector(sourceElement);

                if (key == null)
                {
                    var ex = new ArgumentException("Key selector produced a key that is null. See exception data for source.", nameof(keySelector));
                    ex.Data.Add("source", sourceElement);
                    throw ex;
                }

                if (!result.TryGetValue(key, out var list))
                {
                    list = new List<TSource>();
                    result[key] = list;
                }

                list.Add(sourceElement);
            }

            return result;
        }

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
    }
}
