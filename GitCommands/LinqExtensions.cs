using System.Collections.Generic;

namespace System.Linq
{

    public static class LinqExtensions
    {

        //
        // Summary:
        //     Creates a System.Collections.Generic.Dictionary<TKey,List<TValue>> from an System.Collections.Generic.IEnumerable<T>
        //     according to a specified key selector function.
        //
        // Parameters:
        //   source:
        //     An System.Collections.Generic.IEnumerable<T> to create a System.Collections.Generic.Dictionary<TKey,List<TValue>>
        //     from.
        //
        //   keySelector:
        //     A function to extract a key from each element.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        //   TKey:
        //     The type of the key returned by keySelector.
        //
        // Returns:
        //     A System.Collections.Generic.Dictionary<TKey,List<TValue>> that contains keys and
        //     values.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     source or keySelector is null.  -or- keySelector produces a key that is null.

        public static Dictionary<TKey, List<TSource>> ToDictionaryOfList<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (keySelector == null)
                throw new ArgumentNullException("keySelector");

            Dictionary<TKey, List<TSource>> result = new Dictionary<TKey, List<TSource>>();

            foreach (TSource sourceElement in source)
            {
                TKey key = keySelector(sourceElement);
                if (key == null)
                {
                    var ex = new ArgumentNullException("KeySelector produced a key that is null. See exception data for source.");
                    ex.Data.Add("source", sourceElement);
                    throw ex;
                }

                List<TSource> list;
                if (!result.TryGetValue(key, out list))
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
                throw new ArgumentNullException("keySelector");

            HashSet<TKey> result = new HashSet<TKey>();

            foreach (TSource sourceElement in source)
            {
                TKey key = keySelector(sourceElement);
                if (key == null)
                {
                    var ex = new ArgumentNullException("KeySelector produced a key that is null. See exception data for source.");
                    ex.Data.Add("source", sourceElement);
                    throw ex;
                }

                result.Add(key);
            }

            return result;
        }

        public static string Join(this IEnumerable<string> source, string separator)
        {
            return string.Join(separator, source.ToArray());
        }


        //
        // Summary:
        //     Sorts the elements of a sequence in ascending order by using a specified
        //     comparer.
        //
        // Parameters:
        //   source:
        //     A sequence of values to order.
        //
        //   keySelector:
        //     A function to extract a key from an element.
        //
        //   comparer:
        //     A function to compare keys.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        //   TKey:
        //     The type of the key returned by keySelector.
        //
        // Returns:
        //     An System.Linq.IOrderedEnumerable<TElement> whose elements are sorted according
        //     to a key.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     source or keySelector is null.
        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, TKey, int> comparer)
        {
            FuncComparer<TKey> fc = new FuncComparer<TKey>(comparer);

            return source.OrderBy(keySelector, fc);
        }

        private class FuncComparer<T> : IComparer<T>
        {
            private Func<T, T, int> comparer;

            public FuncComparer(Func<T, T, int> comparer)
            {
                this.comparer = comparer;
            }

            public int Compare(T x, T y)
            {
                return comparer(x, y);
            }
        }


        //
        // Summary:
        //     Transforms each element of an array into a new form by incorporating the
        //     element's index.
        //
        // Parameters:
        //   source:
        //     A sequence of values to invoke a transform function on.
        //
        //   transformer:
        //     A transform function to apply to each source element; the second parameter
        //     of the function represents the index of the source element.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     source or selector is null.
        public static void Select<TSource>(this TSource[] source, Func<TSource, int, TSource> transformer)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (transformer == null)
                throw new ArgumentNullException("transformer");

            for (int i = 0; i < source.Length; i++)
                source[i] = transformer(source[i], i);
        }

        //
        // Summary:
        //     Transforms each element of an array into a new form.
        //
        // Parameters:
        //   source:
        //     A sequence of values to invoke a transform function on.
        //
        //   transformer:
        //     A transform function to apply to each element.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     source or selector is null.
        public static void Transform<TSource>(this TSource[] source, Func<TSource, TSource> transformer)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (transformer == null)
                throw new ArgumentNullException("transformer");

            for (int i = 0; i < source.Length; i++)
                source[i] = transformer(source[i]);
        }

        public static void AddAll<T>(this IList<T> list, IEnumerable<T> elementsToAdd)
        {
            foreach (T t in elementsToAdd)
                list.Add(t);
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T t in enumerable)
                action(t);
        }

        public static IEnumerable<T> Unwrap<T>(this IEnumerable<IEnumerable<T>> enumerable)
        {
            if (enumerable == null)
                yield break;

            foreach (var subEnum in enumerable)
                if (subEnum != null)
                    foreach (T t in subEnum)
                        yield return t;
        }
    }
}
