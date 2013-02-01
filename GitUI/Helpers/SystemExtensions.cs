using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitUI
{
    /// <summary>Provides members helpul to <see cref="System"/> and related members.</summary>
    static class SystemExtensions
    {
        /// <summary>Gets an aggregate hash code from a list of <paramref name="hashes"/>.</summary>
        public static int GetHash(this IEnumerable<int> hashes)
        {
            return hashes.Aggregate((prev, curr) => (prev * 397) ^ curr);
        }

        /// <summary>Gets an aggregate hash code from a list of <paramref name="items"/>, 
        /// using each item's <see cref="object.GetHashCode"/> method.</summary>
        public static int GetHash(this IEnumerable items)
        {
            return GetHash(items.OfType<object>());
        }

        /// <summary>Gets an aggregate hash code from a list of <paramref name="items"/>, 
        /// using each item's <see cref="object.GetHashCode"/> method.</summary>
        public static int GetHash<T>(this IEnumerable<T> items)
        {
            return GetHash(items.Select(item => item.GetHashCode()));
        }

        /// <summary>Gets a hash code from a list of <paramref name="items"/>.</summary>
        public static int GetHash<T>(this IEnumerable<T> items, Func<T, int> selector)
        {
            return GetHash(items.Select(selector));
        }

        /// <summary>Gets an aggregate hash code from a list of <paramref name="hashes"/>.</summary>
        public static int GetHash(params int[] hashes)
        {
            return GetHash(hashes.AsEnumerable());
        }
    }
}
