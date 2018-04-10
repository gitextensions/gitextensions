using System;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public sealed class StringExtensionTests
    {
        [TestCase(null, 10, "")]
        [TestCase("Hello", 10, "Hello")]
        [TestCase("Hello", 5, "Hello")]
        [TestCase("Hello", 4, "H...")]
        [TestCase("Hello", 3, "...")]
        [TestCase("Hello", 2, "He")]
        public void ShortenTo_works_as_expected(string s, int length, string expected)
        {
            Assert.AreEqual(expected, s.ShortenTo(length));
        }

        [TestCase("Hello World", "Hello", true)]
        [TestCase("Hello World", " ", true)]
        [TestCase("Hello World", "World", true)]
        [TestCase("Hello World", "", true)]
        [TestCase("Hello World", "Hi", false)]
        [TestCase("Hello World", "ldi", false)]
        public void Contains_works_as_expected(string str, string other, bool expected)
        {
            Assert.AreEqual(expected, str.Contains(other, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}