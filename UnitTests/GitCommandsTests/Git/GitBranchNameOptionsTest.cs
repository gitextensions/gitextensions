using System;
using FluentAssertions;
using GitCommands.Git;
using NUnit.Framework;

namespace GitCommandsTests.Git
{
    [SetCulture("en-US")]
    [SetUICulture("en-US")]
    [TestFixture]
    public class GitBranchNameOptionsTest
    {
        [TestCase(null, "")]
        [TestCase("", "")]
        public void ReplacementToken_can_be_null_or_empty(string token, string expected)
        {
            var options = new GitBranchNameOptions(token);

            options.ReplacementToken.Should().Be(expected);
        }

        [Test]
        public void ReplacementToken_cant_be_multichar()
        {
            ((Action)(() => new GitBranchNameOptions("aaa"))).Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Replacement token must be a single character\r\nParameter name: replacementToken");
        }

        [TestCase(" ", ' ')]
        [TestCase("^", '^')]
        [TestCase("~", '~')]
        [TestCase(":", ':')]
        public void ReplacementToken_cant_be_invalid(string token, char expected)
        {
            ((Action)(() => new GitBranchNameOptions(token))).Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage(string.Format("Replacement token invalid: '{0}'\r\nParameter name: replacementToken", expected));
        }
    }
}
