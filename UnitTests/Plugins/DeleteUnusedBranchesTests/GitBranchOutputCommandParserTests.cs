using DeleteUnusedBranches;
using FluentAssertions;
using NUnit.Framework;

namespace DeleteUnusedBranchesTests
{
    [TestFixture]
    public class GitBranchOutputCommandParserTests
    {
        private GitBranchOutputCommandParser _parser;

        [SetUp]
        public void Setup()
        {
            _parser = new GitBranchOutputCommandParser();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("    ")]
        public void GetBranchNames_should_return_empty_list(string commandOutput)
        {
            _parser.GetBranchNames(commandOutput).Should().BeEmpty();
        }

        [TestCase]
        public void GetBranchNames_should_return_parse_correctly()
        {
            var commandOutput = @"  feature/branch-1
* fix/branch-2
  master

";

            _parser.GetBranchNames(commandOutput)
                   .Should()
                   .BeEquivalentTo("feature/branch-1", "fix/branch-2", "master");
        }
    }
}