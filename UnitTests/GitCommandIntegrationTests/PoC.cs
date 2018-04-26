using ApprovalTests;
using GitCommands;
using NUnit.Framework;

namespace GitCommandIntegrationTests
{
    /// <summary>
    /// Three simple test cases for git commands copypasted from GitModule.GetRefs.
    /// They are not intended to be fit for any purpose except demonstration of test execution.
    /// </summary>
    [TestFixture]
    public class PoC
    {
        private const string TestRepoName = "poc";

        [Test]
        public void Listing_both_tags_and_branches_should_list_all_tags_and_branches_with_hash_and_refname_columns()
        {
            RunGitCommandOnTestRepo(@"show-ref --dereference");
        }

        [Test]
        public void Listing_tags_should_list_all_tags_with_hash_and_refname_columns()
        {
            RunGitCommandOnTestRepo(@"show-ref --tags");
        }

        [Test]
        public void Listing_branches_should_list_all_branches_with_hash_and_refname_columns()
        {
            RunGitCommandOnTestRepo(@"for-each-ref --sort=-committerdate refs/heads/ --format=""%(objectname) %(refname)""");
        }

        private static void RunGitCommandOnTestRepo(string gitCommandLine)
        {
            using (ITestRepositoryData tempDir = new ZippedTestRepoDirectory(TestRepoName))
            {
                var gitModule = new GitModule(tempDir.ContentPath);
                var output = gitModule.RunGitCmdResult(gitCommandLine, GitModule.SystemEncoding).StdOutput;

                Approvals.Verify(output);
            }
        }
    }
}
