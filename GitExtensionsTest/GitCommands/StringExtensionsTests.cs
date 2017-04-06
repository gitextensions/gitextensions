using System;
using FluentAssertions;
using GitCommands;
using NUnit.Framework;

namespace GitExtensionsTest.GitCommands
{
    [TestFixture]
    public class StringExtensionsTests
    {
        [TestCase(null, null, "")]
        [TestCase(null, true, "")]
        [TestCase(null, false, "")]
        [TestCase("", null, "\"\"")]
        [TestCase("", true, "\"\"")]
        [TestCase("", false, "")]
        [TestCase(" ", null, "\" \"")]
        [TestCase(" ", true, "\" \"")]
        [TestCase(" ", false, "\" \"")]
        public void Quote(string s, bool? quoteEmptyString, string expected)
        {
            string result;
            if (!quoteEmptyString.HasValue)
            {
                result = s.Quote();
            }
            else
            {
                result = s.Quote(quoteEmptyString.Value);
            }

            result.Should().Be(expected);
        }

        [TestCase(null, "'", null, "")]
        [TestCase(null, "'", true, "")]
        [TestCase(null, "'", false, "")]
        [TestCase("", "'", null, "''")]
        [TestCase("", "'", true, "''")]
        [TestCase("", "'", false, "")]
        [TestCase(" ", "'", null, "' '")]
        [TestCase(" ", "'", true, "' '")]
        [TestCase(" ", "'", false, "' '")]
        public void Quote(string s, string quote, bool? quoteEmptyString, string expected)
        {
            string result;
            if (!quoteEmptyString.HasValue)
            {
                result = s.Quote(quote);
            }
            else
            {
                result = s.Quote(quote, quoteEmptyString.Value);
            }

            result.Should().Be(expected);
        }
    }
}
