using FluentAssertions;
using GitCommands;
using GitCommands.Git;
using NUnit.Framework;

namespace GitCommandsTests.Git
{
    [TestFixture]
    public class RevisionDiffProviderTest
    {
        // See RevisionDiffProvider.ArtificialToDiffOptions() for possible "aliases" for artificial commits
        // All variants are not tested in all situations

        private RevisionDiffProvider _revisionDiffProvider;

        [SetUp]
        public void Setup()
        {
            _revisionDiffProvider = new RevisionDiffProvider();
        }

#if !DEBUG
        // Testcases that should assert in debug; should not occur but undefined behavior that should be blocked in GUI
        // Cannot compare unstaged to unstaged or staged to staged but give predictive output in release builds

        // Two empty parameters will compare working dir to index
        [TestCase(null)]
        [TestCase("")]
        [TestCase(GitRevision.UnstagedGuid)]
        public void RevisionDiffProvider_should_return_empty_if_To_is_UnstagedGuid(string firstRevision)
        {
            _revisionDiffProvider.Get(firstRevision, GitRevision.UnstagedGuid).Should().BeEmpty();
        }

        // Two staged revisions gives duplicated options, no reason to clean
        [TestCase("^")]
        [TestCase(GitRevision.IndexGuid)]
        public void RevisionDiffProvider_should_return_cached_if_both_IndexGuid(string firstRevision)
        {
            _revisionDiffProvider.Get(firstRevision, GitRevision.IndexGuid).Should().Be("--cached --cached");
        }
#endif

        [TestCase(GitRevision.IndexGuid, GitRevision.UnstagedGuid)]
        [TestCase("^", "")]
        [TestCase(GitRevision.IndexGuid, null)]
        public void RevisionDiffProvider_staged_to_unstaged(string firstRevision, string secondRevision)
        {
            _revisionDiffProvider.Get(firstRevision, secondRevision).Should().BeEmpty();
        }

        [TestCase(GitRevision.UnstagedGuid, GitRevision.IndexGuid)]
        [TestCase("", "^")]
        public void RevisionDiffProvider_unstaged_to_staged(string firstRevision, string secondRevision)
        {
            _revisionDiffProvider.Get(firstRevision, secondRevision).Should().Be("-R");
        }

        [TestCase(GitRevision.UnstagedGuid + "^^")]
        [TestCase(GitRevision.IndexGuid + "^")]
        [TestCase("HEAD")]
        public void RevisionDiffProvider_head_to_unstaged(string firstRevision)
        {
            _revisionDiffProvider.Get(firstRevision, GitRevision.UnstagedGuid).Should().Be("\"HEAD\"");
        }

        [TestCase(GitRevision.IndexGuid + "^", "^")]
        [TestCase("HEAD", GitRevision.IndexGuid)]
        public void RevisionDiffProvider_head_to_staged(string firstRevision, string secondRevision)
        {
            _revisionDiffProvider.Get(firstRevision, secondRevision).Should().Be("--cached \"HEAD\"");
        }

        [TestCase(GitRevision.IndexGuid, "HEAD")]
        public void RevisionDiffProvider_staged_to_head(string firstRevision, string secondRevision)
        {
            _revisionDiffProvider.Get(firstRevision, secondRevision).Should().Be("-R --cached \"HEAD\"");
        }

        [TestCase("HEAD", "123456789")]
        public void RevisionDiffProvider_normal1(string firstRevision, string secondRevision)
        {
            _revisionDiffProvider.Get(firstRevision, secondRevision).Should().Be("\"HEAD\" \"123456789\"");
        }

        [TestCase("123456789", "HEAD")]
        public void RevisionDiffProvider_normal2(string firstRevision, string secondRevision)
        {
            _revisionDiffProvider.Get(firstRevision, secondRevision).Should().Be("\"123456789\" \"HEAD\"");
        }

        // Standard usage when filename is included
        [TestCase("123456789", GitRevision.UnstagedGuid, "a.txt", null, true)]
        public void RevisionDiffProvider_fileName_tracked(string firstRevision, string secondRevision, string fileName, string oldFileName, bool isTracked)
        {
            _revisionDiffProvider.Get(firstRevision, secondRevision, fileName, oldFileName, isTracked).Should().Be("\"123456789\"  -- \"a.txt\"");
        }

        // If fileName is null, ignore oldFileName and tracked
        [TestCase("123456789", "HEAD", null, "b.txt", true)]
        public void RevisionDiffProvider_fileName_null_with_oldname(string firstRevision, string secondRevision, string fileName, string oldFileName, bool isTracked)
        {
            _revisionDiffProvider.Get(firstRevision, secondRevision, fileName, oldFileName, isTracked).Should().Be("\"123456789\" \"HEAD\"");
        }

        // Include old filename if is included
        [TestCase("123456789", "234567890", "a.txt", "b.txt", true)]
        public void RevisionDiffProvider_fileName_oldfilename(string firstRevision, string secondRevision, string fileName, string oldFileName, bool isTracked)
        {
            _revisionDiffProvider.Get(firstRevision, secondRevision, fileName, oldFileName, isTracked).Should().Be("\"123456789\" \"234567890\" -- \"a.txt\" \"b.txt\"");
        }

        // normal testcase when untracked is set
        [TestCase(GitRevision.IndexGuid, GitRevision.UnstagedGuid, "a.txt", null, false)]
        public void RevisionDiffProvider_fileName_untracked(string firstRevision, string secondRevision, string fileName, string oldFileName, bool isTracked)
        {
            _revisionDiffProvider.Get(firstRevision, secondRevision, fileName, oldFileName, isTracked).Should().Be("--no-index -- \"/dev/null\" \"a.txt\"");
        }

        // If fileName is null, ignore oldFileName and tracked
        [TestCase(GitRevision.IndexGuid, GitRevision.UnstagedGuid, null, "b.txt", false)]
        public void RevisionDiffProvider_fileName_null_Untracked(string firstRevision, string secondRevision, string fileName, string oldFileName, bool isTracked)
        {
            _revisionDiffProvider.Get(firstRevision, secondRevision, fileName, oldFileName, isTracked).Should().BeEmpty();
        }

        // Ignore revisions for untracked
        [TestCase("123456789", "234567890", "a.txt", "b.txt", false)]
        public void RevisionDiffProvider_fileName_oldfilename_Untracked(string firstRevision, string secondRevision, string fileName, string oldFileName, bool isTracked)
        {
            _revisionDiffProvider.Get(firstRevision, secondRevision, fileName, oldFileName, isTracked).Should().Be("--no-index -- \"/dev/null\" \"a.txt\"");
        }
    }
}
