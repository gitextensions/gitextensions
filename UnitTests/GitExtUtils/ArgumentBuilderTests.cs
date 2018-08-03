using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests
{
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
    }
}