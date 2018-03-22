using System.Collections.Generic;
using System.Linq;

namespace System
{
    public static class DeconstructionExtensions
    {
        /// <summary>
        /// Supports C# 7 deconstruction of <see cref="KeyValuePair{TKey,TValue}"/>.
        /// </summary>
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> source, out TKey key, out TValue value)
        {
            key = source.Key;
            value = source.Value;
        }

        /// <summary>
        /// Supports C# 7 deconstruction of <see cref="IGrouping{TKey,TElement}"/>.
        /// </summary>
        public static void Deconstruct<TKey, TElement>(this IGrouping<TKey, TElement> source, out TKey key, out IEnumerable<TElement> elements)
        {
            key = source.Key;
            elements = source;
        }

        /// <summary>
        /// Supports C# 7 deconstruction of <see cref="Tuple{T1}"/>.
        /// </summary>
        public static void Deconstruct<T1>(this Tuple<T1> source, out T1 item1)
        {
            item1 = source.Item1;
        }

        /// <summary>
        /// Supports C# 7 deconstruction of <see cref="Tuple{T1,T2}"/>.
        /// </summary>
        public static void Deconstruct<T1, T2>(this Tuple<T1, T2> source, out T1 item1, out T2 item2)
        {
            item1 = source.Item1;
            item2 = source.Item2;
        }

        /// <summary>
        /// Supports C# 7 deconstruction of <see cref="Tuple{T1,T2,T3}"/>.
        /// </summary>
        public static void Deconstruct<T1, T2, T3>(this Tuple<T1, T2, T3> source, out T1 item1, out T2 item2, out T3 item3)
        {
            item1 = source.Item1;
            item2 = source.Item2;
            item3 = source.Item3;
        }
    }
}