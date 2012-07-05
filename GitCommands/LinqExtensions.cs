using System;
using System.Collections.Generic;
using System.Text;

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
                throw new ArgumentNullException("Argument keySelector can not be null");

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

        public static string Join(this IEnumerable<string> source, string separator)
        {
            return string.Join(separator, source.ToArray());
        }

    }
}
