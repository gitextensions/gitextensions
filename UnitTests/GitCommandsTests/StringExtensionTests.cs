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
    }
}