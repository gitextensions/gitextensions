using FluentAssertions;
using GitExtUtils;
using NUnit.Framework;

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
                new ArgumentBuilder());

            Test(
                "foo",
                new ArgumentBuilder { "foo" });

            Test(
                "foo bar",
                new ArgumentBuilder { "foo", "bar" });

            Test(
                "foo bar",
                new ArgumentBuilder { "foo", null, "bar" });

            Test(
                "foo bar",
                new ArgumentBuilder { "foo", "", "bar" });

            Test(
                "",
                new ArgumentBuilder { null });

            void Test(string expected, ArgumentBuilder command)
            {
                Assert.AreEqual(expected, command.ToString());
            }
        }

        [Test]
        public void IsEmpty()
        {
            var builder = new ArgumentBuilder();
            builder.IsEmpty.Should().BeTrue();

            builder.Add("test");
            builder.IsEmpty.Should().BeFalse();
        }

        [Test]
        public void Length()
        {
            var builder = new ArgumentBuilder();
            builder.GetTestAccessor().Arguments.Length.Should().Be(0);

            builder.Add("test");
            builder.GetTestAccessor().Arguments.Length.Should().Be(4);

            var args = "Lorem ipsum dolor sit amet, solet soleat option mel no.";
            var expectedLength = args.Length;
            builder.AddRange(args.Split(' '));
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
            var builder = new ArgumentBuilder();

            foreach (string arg in args)
            {
                builder.Add(arg);
            }

            builder.GetTestAccessor().Arguments.Length.Should().Be(expectedLength);
            builder.ToString().Should().Be(expected);
        }
    }
}