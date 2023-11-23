using FluentAssertions;
using GitExtensions.Plugins.DeleteUnusedBranches;

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
  HEAD
* (HEAD detached at dce3f9f)
  +_valid_branch_name_starting_with_+
";

            _parser.GetBranchNames(commandOutput)
                   .Should()
                   .BeEquivalentTo("feature/branch-1", "fix/branch-2", "master", "+_valid_branch_name_starting_with_+");
        }

        [TestCase]
        public void GetBranchNames_should_support_worktrees()
        {
            string commandOutput = @"+ branch_in_worktree
* master
+ +_valid_branch_name_starting_with_+
";

            _parser.GetBranchNames(commandOutput)
                   .Should()
                   .BeEquivalentTo("branch_in_worktree", "master", "+_valid_branch_name_starting_with_+");
        }
    }
}
