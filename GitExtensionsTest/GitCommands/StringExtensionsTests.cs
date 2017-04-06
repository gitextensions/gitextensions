using System;
using FluentAssertions;
using NUnit.Framework;

namespace GitExtensionsTest.GitCommands
{
    [TestFixture]
    public class StringExtensionsTests
    {
        [TestCase(null, "")]
        [TestCase("", "\"\"")]
        [TestCase(" ", "\" \"")]
        public void Quote_default(string s, string expected)
        {
            var result = s.Quote();

            result.Should().Be(expected);
        }

        [TestCase(null, "'", "")]
        [TestCase("", "'", "''")]
        [TestCase(" ", "'", "' '")]
        public void Quote(string s, string quote, string expected)
        {
            var result = s.Quote(quote);

            result.Should().Be(expected);
        }

        [TestCase(null, "")]
        [TestCase("", "")]
        [TestCase(" ", "\" \"")]
        public void QuoteNE(string s, string expected)
        {
            var result = s.QuoteNE();

            result.Should().Be(expected);
        }
    }
}