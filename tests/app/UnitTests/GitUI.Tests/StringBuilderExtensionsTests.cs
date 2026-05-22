using System.Text;
using GitExtUtils;

namespace GitUITests;

public sealed class StringBuilderExtensionsTests
{
    [TestCase("foo", "foo")]
    [TestCase("\"foo bar\"", "foo bar")]
    [TestCase("\"foo\tbar\"", "foo\tbar")]
    [TestCase("\"foo bar\"", "\"foo bar\"")]
    [TestCase("\"\"", "")]
    [TestCase("\" \"", " ")]
    public void AppendQuoted(string expected, string source)
    {
        new StringBuilder().AppendQuoted(source).ToString().Should().Be(expected);
    }
}
