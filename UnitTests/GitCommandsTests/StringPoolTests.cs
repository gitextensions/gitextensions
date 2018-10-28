using System;
using System.Collections.Generic;
using System.Linq;
using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public sealed class StringPoolTests
    {
        [Test]
        public void Intern()
        {
            var pool = new StringPool();

            const string source = "abcabcabcabcabc";

            Assert.AreEqual(
                "",
                pool.Intern(source, 0, 0));
            Assert.AreEqual(
                "a",
                pool.Intern(source, 0, 1));
            Assert.AreEqual(
                "ab",
                pool.Intern(source, 0, 2));
            Assert.AreEqual(
                "abc",
                pool.Intern(source, 0, 3));

            Assert.AreEqual(4, pool.Count);

            Assert.AreEqual(
                "",
                pool.Intern(source, 3, 0));
            Assert.AreEqual(
                "a",
                pool.Intern(source, 3, 1));
            Assert.AreEqual(
                "ab",
                pool.Intern(source, 3, 2));
            Assert.AreEqual(
                "abc",
                pool.Intern(source, 3, 3));

            Assert.AreEqual(4, pool.Count);

            Assert.AreSame(
                pool.Intern(source, 0, 3),
                pool.Intern(source, 0, 3));
            Assert.AreSame(
                pool.Intern(source, 0, 3),
                pool.Intern(source, 3, 3));

            Assert.AreNotEqual(
                pool.Intern(source, 0, 3),
                pool.Intern(source, 0, 2));
            Assert.AreNotEqual(
                pool.Intern(source, 0, 3),
                pool.Intern(source, 1, 3));

            Assert.AreEqual(5, pool.Count);
        }

        [Test]
        public void EqualsAtIndex()
        {
            const string s = "01234567890123456789012345678901234567890123456789012345678901234567890123456789";

            for (var index = 0; index <= s.Length; index++)
            {
                for (var length = 0; length < s.Length - index; length++)
                {
                    const string format = "index={0} length={1}";

                    Assert.True(StringPool.EqualsAtIndex(s, index, s.Substring(index, length)), format, index, length);

                    Assert.False(StringPool.EqualsAtIndex(s, index, s.Substring(index, length) + 'Z'), format, index, length);

                    if (index > 0 && length > 0)
                    {
                        Assert.False(StringPool.EqualsAtIndex(s, index - 1, s.Substring(index, length)), format, index, length);
                        Assert.False(StringPool.EqualsAtIndex(s, index, s.Substring(index - 1, length)), format, index, length);
                    }

                    if (index + length < s.Length)
                    {
                        if (length == 0)
                        {
                            Assert.True(StringPool.EqualsAtIndex(s, index + 1, s.Substring(index, length)), format, index, length);
                            Assert.True(StringPool.EqualsAtIndex(s, index, s.Substring(index + 1, length)), format, index, length);
                        }
                        else
                        {
                            Assert.False(StringPool.EqualsAtIndex(s, index + 1, s.Substring(index, length)), format, index, length);
                            Assert.False(StringPool.EqualsAtIndex(s, index, s.Substring(index + 1, length)), format, index, length);
                        }
                    }
                }
            }
        }

        [Test]
        public void GetSubstringHashCode()
        {
            const string s = "01234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ`¬¦!\"£$€%^&*()-_=+[]{};:'@#~/?,.<>\\|";

            for (var i = 0; i < 10; i++)
            {
                Console.Out.WriteLine($"0x{StringPool.GetSubstringHashCode(s, 0, i):X2}");
            }

            Assert.AreEqual(0x162A16FE, (uint)StringPool.GetSubstringHashCode(s, 0, 0));
            Assert.AreEqual(0x05E19FCE, (uint)StringPool.GetSubstringHashCode(s, 0, 1));
            Assert.AreEqual(0x05E4405D, (uint)StringPool.GetSubstringHashCode(s, 0, 2));
            Assert.AreEqual(0xFC2C8D57, (uint)StringPool.GetSubstringHashCode(s, 0, 3));
            Assert.AreEqual(0xFC833FEA, (uint)StringPool.GetSubstringHashCode(s, 0, 4));
            Assert.AreEqual(0x36D35466, (uint)StringPool.GetSubstringHashCode(s, 0, 5));
            Assert.AreEqual(0x42005971, (uint)StringPool.GetSubstringHashCode(s, 0, 6));
            Assert.AreEqual(0x4B54D52B, (uint)StringPool.GetSubstringHashCode(s, 0, 7));
            Assert.AreEqual(0xBC227B3E, (uint)StringPool.GetSubstringHashCode(s, 0, 8));
            Assert.AreEqual(0xCB2B1F36, (uint)StringPool.GetSubstringHashCode(s, 0, 9));

            Assert.AreEqual(3779978672, (uint)StringPool.GetSubstringHashCode(s, 0, s.Length));
        }

        /*
        Strings hashed 200,000 with minimum length 3
        461 collisions over 445 indices
        Most crowded bucket had 2 items
        Average clashing bucket length 1.036
        */

        [Test, Ignore("For hash analysis only, has no assertions")]
        public void AnalyzeHashFunctionDistribution()
        {
            var seenHashes = new HashSet<int>();
            var collisions = new List<int>();

            const int hashCount = 200_000;
            const int sourceWidth = 40;
            const int minLength = 3;

            var remaining = hashCount;

            while (remaining > 0)
            {
                for (var start = 0; start < sourceWidth; start++)
                {
                    var maxLength = sourceWidth - start;

                    for (var length = minLength; length < maxLength && remaining != 0; length++, remaining--)
                    {
                        var s = CreateRandomString(sourceWidth);

                        var hash = StringPool.GetSubstringHashCode(s, start, length);

                        if (!seenHashes.Add(hash))
                        {
                            collisions.Add(hash);
                        }
                    }
                }
            }

            var crowdedBucketCount = collisions.Distinct().Count();
            var max = collisions.GroupBy(hash => hash).Select(g => g.Count()).Max();

            Console.Out.WriteLine(
                $"Strings hashed {hashCount:#,0} with minimum length {minLength}\n" +
                $"{collisions.Count} collisions over {crowdedBucketCount} indices\n" +
                $"Most crowded bucket had {max} items\n" +
                $"Average clashing bucket length {(double)collisions.Count / crowdedBucketCount:0.###}");
        }

        private static readonly Random _random = new Random();

        private static string CreateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            var charsLength = chars.Length;
            var stringChars = new char[length];

            for (var i = 0; i < length; i++)
            {
                var randomIndex = _random.Next(charsLength);

                stringChars[i] = chars[randomIndex];
            }

            return new string(stringChars);
        }
    }
}
