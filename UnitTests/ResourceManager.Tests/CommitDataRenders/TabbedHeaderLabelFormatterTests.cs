using FluentAssertions;
using NUnit.Framework;
using ResourceManager.CommitDataRenders;

namespace ResourceManagerTests.CommitDataRenders
{
    [TestFixture]
    public class TabbedHeaderLabelFormatterTests
    {
        private TabbedHeaderLabelFormatter _formatter;

        [SetUp]
        public void Setup()
        {
            _formatter = new TabbedHeaderLabelFormatter();
        }

        [TestCase(null, 10, ":			")]
        [TestCase("", 10, ":			")]
        [TestCase("", 16, ":				")]
        [TestCase(" ", 10, " :		")]
        [TestCase("a", 10, "a:		")]
        [TestCase("a", 8, "a:		")]
        [TestCase("abc", 1, "abc:")]
        [TestCase("John Doe <John.Doe@test.com>", 38, "John Doe &lt;John.Doe@test.com&gt;:	")]
        [TestCase("John Doe <John.Doe@test.com>", 40, "John Doe &lt;John.Doe@test.com&gt;:		")]
        public void FormatLabel_should_render_correctly(string given, int desiredLength, string expected)
        {
            _formatter.FormatLabel(given, desiredLength).Should().Be(expected);
        }
    }
}