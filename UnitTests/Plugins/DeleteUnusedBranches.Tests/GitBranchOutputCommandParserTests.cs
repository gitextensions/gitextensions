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

        [Test]
        public void GetBranchNames_should_return_empty_list(
            [Values(null, "", "   ")] string commandOutput,
            [Values(true, false)] bool isRemote)
        {
            _parser.GetBranchNames(commandOutput, isRemote).Should().BeEmpty();
        }

        [TestCase]
        public void GetBranchNames_should_return_parse_correctly()
        {
            string commandOutput = @"  feature/branch-1
* fix/branch-2
  master
  HEAD
* (HEAD detached at dce3f9f)
  +_valid_branch_name_starting_with_+
  space/after 
  symbolic_ref -> origin/master
";

            _parser.GetBranchNames(commandOutput, isRemote: false)
                   .Should()
                   .BeEquivalentTo("feature/branch-1", "fix/branch-2", "master", "+_valid_branch_name_starting_with_+", "space/after", "symbolic_ref");
        }

        [TestCase]
        public void GetBranchNames_parse_for_remote()
        {
            string commandOutput = @"  just_remote_that_should_not_occur_but_is_tested
  myremote/mybranch
  other_remote/+_valid_branch_name_starting_with_+
  +remote/+branch+
  space/after 
  origin/symbolic_ref -> origin/master
  ignore_remote_HEAD/HEAD
";

            _parser.GetBranchNames(commandOutput, isRemote: true)
                   .Should()
                   .BeEquivalentTo("just_remote_that_should_not_occur_but_is_tested", "myremote/mybranch", "other_remote/+_valid_branch_name_starting_with_+", "+remote/+branch+", "space/after", "origin/symbolic_ref");
        }

        [TestCase]
        public void GetBranchNames_should_support_worktrees()
        {
            string commandOutput = @"+ branch_in_worktree
* master
+ +_valid_branch_name_starting_with_+
";

            _parser.GetBranchNames(commandOutput, isRemote: false)
                   .Should()
                   .BeEquivalentTo("branch_in_worktree", "master", "+_valid_branch_name_starting_with_+");
        }
    }
}
