using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable ForCanBeConvertedToForeach

namespace GitCommands
{
    /// <summary>
    /// Pool for string instances, with the goal of drawing from the pool without requiring
    /// a precisely trimmed string for the query.
    /// </summary>
    public sealed class StringPool
    {
        private object[] _buckets = new object[4];
        private int _capacity = 3;

        /// <summary>
        /// Gets the number of items unique strings in the pool.
        /// </summary>
        public int Count { get; private set; }

        public string Intern(string source, Capture capture) => Intern(source, capture.Index, capture.Length);

        public string Intern(string source, int index, int length)
        {
            if (Count >= _capacity)
            {
                Grow();
            }

            var hash = GetSubstringHashCode(source, index, length);
            var pos = (uint)hash % _buckets.Length;
            var bucket = _buckets[pos];

            if (bucket is string s)
            {
                if (EqualsAtIndex(source, index, s))
                {
                    return s;
                }

                Count++;

                var substring = source.Substring(index, length);
                _buckets[pos] = new List<string>(2) { s, substring };
                return substring;
            }

            if (bucket is List<string> list)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    if (item.Length == length && EqualsAtIndex(source, index, item))
                    {
                        return item;
                    }
                }

                Count++;

                var substring = source.Substring(index, length);
                list.Add(source.Substring(index, length));
                return substring;
            }

            if (bucket == null)
            {
                Count++;

                var substring = source.Substring(index, length);
                _buckets[pos] = substring;
                return substring;
            }

            Debug.Fail("Bucket had prohibited type");
            return null;
        }

        private void Grow()
        {
            _capacity *= 2;

            var newBuckets = new object[_buckets.Length * 2];

            for (var i = 0; i < _buckets.Length; i++)
            {
                var entry = _buckets[i];

                if (entry == null)
                {
                    continue;
                }

                switch (entry)
                {
                    case string s:
                        HashString(s);
                        break;
                    case List<string> list:
                        for (var j = 0; j < list.Count; j++)
                        {
                            HashString(list[j]);
                        }

                        break;
                }
            }

            _buckets = newBuckets;

            void HashString(string s)
            {
                var hash = GetSubstringHashCode(s, 0, s.Length);
                var newPos = Math.Abs(hash % newBuckets.Length);
                var newBucket = newBuckets[newPos];

                switch (newBucket)
                {
                    case null:
                        newBuckets[newPos] = s;
                        break;
                    case string old:
                        newBuckets[newPos] = new List<string>(2) { old, s };
                        break;
                    case List<string> list:
                        list.Add(s);
                        break;
                }
            }
        }

        #region Zero-allocation equality and hashing from substrings

        [Pure]
        internal static unsafe bool EqualsAtIndex(string source, int index, string comparand)
        {
            var len = comparand.Length;

            if (index + comparand.Length > source.Length)
            {
                throw new InvalidOperationException("Index and length extend beyond end of source string.");
            }

            fixed (char* pc = comparand)
            fixed (char* ps = source)
            {
                // Create writeable pointers to equivalent positions in each string
                var c = pc;
                var s = &ps[index];

                // Loop every 10 characters (20 bytes each loop)
                while (len >= 10)
                {
                    // Compare an int by int, 5 times
                    if (*(int*)c != *(int*)s ||
                        *(int*)(c + 2) != *(int*)(s + 2) ||
                        *(int*)(c + 4) != *(int*)(s + 4) ||
                        *(int*)(c + 6) != *(int*)(s + 6) ||
                        *(int*)(c + 8) != *(int*)(s + 8))
                    {
                        return false;
                    }

                    // Update the pointers.
                    // We delay this (rather than using ++ inline) to avoid
                    // CPU stall on flushing writes.
                    c += 10;
                    s += 10;
                    len -= 10;
                }

                // Loop every 2 characters (4 bytes)
                while (len > 1 && *(int*)c == *(int*)s)
                {
                    // Compare int by int (4 bytes each loop)
                    c += 2;
                    s += 2;
                    len -= 2;
                }

                // If last byte has odd index, check it too
                return len == 0 || (len == 1 && *c == *s);
            }
        }

        [Pure]
        internal static unsafe int GetSubstringHashCode(string str, int index, int length)
        {
            if (index + length > str.Length)
            {
                throw new InvalidOperationException("Index and length extend beyond end of string.");
            }

            // Slightly modified version of https://referencesource.microsoft.com/#mscorlib/system/string.cs,827

            // Pin the object
            fixed (char* src = str)
            {
                // Accumulators
                var hash1 = 0x1505;
                var hash2 = hash1;

                // Pointer to the first character of the hash
                var s = src + index;

                // Pointer to the last character of the hash
                var end = s + length;

                var rem = length % 2;

                // If there are an odd number of items, ignore the last one for now
                end -= rem;

                // Walk characters, alternating between hash1 and hash2
                while (s < end)
                {
                    // Add character to hash 1
                    hash1 = ((hash1 << 5) + hash1) ^ *s++;

                    // Add character to hash 2
                    hash2 = ((hash2 << 5) + hash2) ^ *s++;
                }

                if (rem == 1)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ *s;
                }

                // Combine the two hashes for the final hash
                return hash2 + (hash1 * 0x5D588B65);
            }
        }

        #endregion
    }
}