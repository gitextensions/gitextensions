using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GitCommands.Git;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public class GitCommandResultTest
    {
        [Test]
        [TestCase("")]
        [TestCase("\na\n\nb\n\n")]
        public void Output_should_return_original_output(string output)
        {
            var result = new GitCommandResult(output);

            result.Output.Should().BeEquivalentTo(output);
        }

        [Test]
        [TestCaseSource(nameof(Lines_should_split_output_to_lines_Source))]
        public void Lines_should_split_output_to_lines(string output, string[] lines)
        {
            var result = new GitCommandResult(output);

            result.Lines.Should().BeEquivalentTo(lines, options => options.WithStrictOrdering());
        }

        public static IEnumerable<object[]> Lines_should_split_output_to_lines_Source()
        {
            yield return new object[] { "a", new[] { "a" } };
            yield return new object[] { "a\nb", new[] { "a", "b" } };
        }

        [Test]
        [TestCaseSource(nameof(Lines_should_skip_empty_lines_Source))]
        public void Lines_should_skip_empty_lines(string output, string[] lines)
        {
            var result = new GitCommandResult(output);

            result.Lines.Should().BeEquivalentTo(lines, options => options.WithStrictOrdering());
        }

        public static IEnumerable<object[]> Lines_should_skip_empty_lines_Source()
        {
            yield return new object[] { "\n", Enumerable.Empty<string>() };
            yield return new object[] { "\na\n\nb\n\n", new[] { "a", "b" } };
        }

        [Test]
        [TestCase("", false)]
        [TestCase("error: unknown option `x'", true)]
        [TestCase("         error: unknown option `x'", true)]
        [TestCase("@@ -2817,11 +2817,15 @@ .....\n" +
                  "         error: unknown option `x'", false)]
        public void Result_should_indicate_error(string output, bool shouldBeError)
        {
            var result = new GitCommandResult(output);

            result.IsError.Should().Be(shouldBeError);
        }
    }
}