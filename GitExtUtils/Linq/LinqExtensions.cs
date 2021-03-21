using System.Collections.Generic;
using GitExtUtils;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace

namespace System.Linq
{
    public static class LinqExtensions
    {
        [MustUseReturnValue]
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T>? comparer = null) => new HashSet<T>(source, comparer ?? EqualityComparer<T>.Default);

        [MustUseReturnValue]
        public static HashSet<TKey> ToHashSet<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            if (keySelector is null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            var result = new HashSet<TKey>();

            foreach (var element in source)
            {
                var key = keySelector(element);

                if (key is null)
                {
                    throw new ArgumentException(
                        "Key selector produced a key that is null. See exception data for source.",
                        nameof(keySelector))
                    {
                        Data = { { "source", element } }
                    };
                }

                result.Add(key);
            }

            return result;
        }

        [Pure]
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
        [LinqTunnel]
        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TKey, TKey, int> comparer)
        {
            return source.OrderBy(keySelector, new DelegateComparer<TKey>(comparer));
        }

        private sealed class DelegateComparer<T> : IComparer<T>
        {
            private readonly Func<T, T, int> _comparer;

            public DelegateComparer(Func<T, T, int> comparer)
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
        public static void Transform<TSource>(
            this TSource[] source,
            [InstantHandle] Func<TSource, TSource> transformer)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (transformer is null)
            {
                throw new ArgumentNullException(nameof(transformer));
            }

            for (int i = 0; i < source.Length; i++)
            {
                source[i] = transformer(source[i]);
            }
        }

        public static void AddAll<T>(this IList<T> list, [InstantHandle] IEnumerable<T> elementsToAdd)
        {
            foreach (T t in elementsToAdd)
            {
                list.Add(t);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, [InstantHandle] Action<T> action)
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

        [Pure]
        public static IReadOnlyList<T> AsReadOnlyList<T>(this IEnumerable<T> source)
        {
            switch (source)
            {
                case IReadOnlyList<T> readOnlyList:
                {
                    return readOnlyList;
                }

                case ICollection<T> collection:
                {
                    if (collection.Count == 0)
                    {
                        return Array.Empty<T>();
                    }

                    var items = new T[collection.Count];
                    collection.CopyTo(items, 0);
                    return items;
                }
            }

            using var e = source.GetEnumerator();
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

        [Pure]
        [MustUseReturnValue]
        public static int IndexOf<T>(
            this IEnumerable<T> source,
            [InstantHandle] Func<T, bool> predicate)
        {
            var index = 0;

            foreach (var element in source)
            {
                if (predicate(element))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        [Pure]
        [MustUseReturnValue]
        public static TResult[] ToArray<TSource, TResult>(
            this IReadOnlyList<TSource> source,
            [InstantHandle] Func<TSource, TResult> map)
        {
            var array = new TResult[source.Count];

            for (var i = 0; i < source.Count; i++)
            {
                array[i] = map(source[i]);
            }

            return array;
        }

        [Pure]
        [MustUseReturnValue]
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
        {
            foreach (var item in source)
            {
                if (item is not null)
                {
                    yield return item;
                }
            }
        }

        [Pure]
        [MustUseReturnValue]
        public static IEnumerable<string> WhereNotNullOrWhiteSpace(this IEnumerable<string?> source)
        {
            foreach (var item in source)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    yield return item;
                }
            }
        }
    }
}
