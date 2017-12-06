using FluentAssertions;
using GitCommands;
using GitCommands.Git;
using NUnit.Framework;

namespace GitCommandsTests.Git
{
    [TestFixture]
    public class RevisionDiffProviderTest
    {
        //See RevisionDiffProvider.ArtificialToDiffOptions() for possible "aliases" for artificial commits
        //All variants are not tested in all situations

        private RevisionDiffProvider _revisionDiffProvider;

        [SetUp]
        public void Setup()
        {
            _revisionDiffProvider = new RevisionDiffProvider();
        }

#if !DEBUG
        //Testcases that should assert in debug; should not occur but undefined behavior that should be blocked in GUI
        //Cannot compare unstaged to unstaged or staged to staged but give predictive output in release builds

        //Two empty parameters will compare working dir to index
        [TestCase(null)]
        [TestCase("")]
        [TestCase(GitRevision.UnstagedGuid)]
        public void RevisionDiffProvider_should_return_empty_if_To_is_UnstagedGuid(string from)
        {
            _revisionDiffProvider.Get(from, GitRevision.UnstagedGuid).Should().BeEmpty();
        }

        //Two staged revisions gives duplicated options, no reason to clean
        [TestCase("^")]
        [TestCase(GitRevision.IndexGuid)]
        public void RevisionDiffProvider_should_return_cached_if_both_IndexGuid(string from)
        {
            _revisionDiffProvider.Get(from, GitRevision.IndexGuid).Should().Be("--cached --cached");
        }
#endif

        [TestCase(GitRevision.IndexGuid, GitRevision.UnstagedGuid)]
        [TestCase("^", "")]
        [TestCase(GitRevision.IndexGuid, null)]
        public void RevisionDiffProvider_staged_to_unstaged(string from, string to)
        {
            _revisionDiffProvider.Get(from, to).Should().BeEmpty();
        }

        [TestCase(GitRevision.UnstagedGuid, GitRevision.IndexGuid)]
        [TestCase("", "^")]
        public void RevisionDiffProvider_unstaged_to_staged(string from, string to)
        {
            _revisionDiffProvider.Get(from, to).Should().Be("-R");
        }

        [TestCase(GitRevision.UnstagedGuid + "^^")]
        [TestCase(GitRevision.IndexGuid + "^")]
        [TestCase("HEAD")]
        public void RevisionDiffProvider_head_to_unstaged(string from)
        {
            _revisionDiffProvider.Get(from, GitRevision.UnstagedGuid).Should().Be("\"HEAD\"");
        }

        [TestCase(GitRevision.IndexGuid + "^", "^")]
        [TestCase("HEAD", GitRevision.IndexGuid)]
        public void RevisionDiffProvider_head_to_staged(string from, string to)
        {
            _revisionDiffProvider.Get(from, to).Should().Be("--cached \"HEAD\"");
        }

        [TestCase(GitRevision.IndexGuid, "HEAD")]
        public void RevisionDiffProvider_staged_to_head(string from, string to)
        {
            _revisionDiffProvider.Get(from, to).Should().Be("-R --cached \"HEAD\"");
        }

        [TestCase("HEAD", "123456789")]
        public void RevisionDiffProvider_normal1(string from, string to)
        {
            _revisionDiffProvider.Get(from, to).Should().Be("\"HEAD\" \"123456789\"");
        }

        [TestCase("123456789", "HEAD")]
        public void RevisionDiffProvider_normal2(string from, string to)
        {
            _revisionDiffProvider.Get(from, to).Should().Be("\"123456789\" \"HEAD\"");
        }
    }
}

