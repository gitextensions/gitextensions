using System;
using System.Linq;
using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public sealed class ArrayExtensionsTests
    {
        [Test]
        public void Subsequence()
        {
            var nums = Enumerable.Range(0, 10).ToArray();

            Assert.AreEqual(
                new[] { 0, 1, 2, 3 },
                nums.Subsequence(0, 4));
            Assert.AreEqual(
                new[] { 1, 2, 3, 4 },
                nums.Subsequence(1, 4));

            Assert.AreEqual(
                nums,
                nums.Subsequence(0, 10));

            Assert.AreEqual(
                Array.Empty<int>(),
                nums.Subsequence(0, 0));

            Assert.AreEqual(
                Array.Empty<int>(),
                nums.Subsequence(9, 0));
        }

        [Test]
        public void Append()
        {
            Assert.AreEqual(
                new[] { 0, 1 },
                new[] { 0 }.Append(1));
            Assert.AreEqual(
                new[] { 0 },
                Array.Empty<int>().Append(0));
            Assert.AreEqual(
                new[] { 0, 1, 2 },
                Array.Empty<int>().Append(0).Append(1).Append(2));
        }
    }
}