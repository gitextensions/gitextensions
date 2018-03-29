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
    }
}