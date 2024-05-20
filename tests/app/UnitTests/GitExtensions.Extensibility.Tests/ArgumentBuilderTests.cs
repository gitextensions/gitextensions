using FluentAssertions;
using GitExtensions.Extensibility;

namespace GitExtUtilsTests
{
    [TestFixture]
    public sealed class ArgumentBuilderTests
    {
        [Test]
        public void Adds_simple_parameters()
        {
            Test(
                "",
                []);

            Test(
                "foo",
                ["foo"]);

            Test(
                "foo bar",
                ["foo", "bar"]);

            Test(
                "foo bar",
                ["foo", null, "bar"]);

            Test(
                "foo bar",
                ["foo", "", "bar"]);

            Test(
                "",
                [null]);

            void Test(string expected, ArgumentBuilder command)
            {
                Assert.AreEqual(expected, command.ToString());
            }
        }

        [Test]
        public void IsEmpty()
        {
            ArgumentBuilder builder = [];
            builder.IsEmpty.Should().BeTrue();

            builder.Add("test");
            builder.IsEmpty.Should().BeFalse();
        }

        [Test]
        public void Length()
        {
            ArgumentBuilder builder = [];
            builder.GetTestAccessor().Arguments.Length.Should().Be(0);

            builder.Add("test");
            builder.GetTestAccessor().Arguments.Length.Should().Be(4);

            string args = "Lorem ipsum dolor sit amet, solet soleat option mel no.";
            int expectedLength = args.Length;
            builder.AddRange(args.LazySplit(' '));
            builder.GetTestAccessor().Arguments.Length.Should().Be(expectedLength + /* 'test ' */5);
        }

        [TestCase(new[] { (string)null }, 0, "")]
        [TestCase(new[] { "" }, 0, "")]
        [TestCase(new[] { "", null }, 0, "")]
        [TestCase(new[] { "test" }, 4, "test")]
        [TestCase(new[] { "test", "test" }, 9, "test test")]
        [TestCase(new[] { "", "test" }, 4, "test")]
        [TestCase(new[] { "test", null, "test" }, 9, "test test")]
        public void Add(string[] args, int expectedLength, string expected)
        {
            ArgumentBuilder builder = [.. args];

            builder.GetTestAccessor().Arguments.Length.Should().Be(expectedLength);
            builder.ToString().Should().Be(expected);
        }
    }
}
