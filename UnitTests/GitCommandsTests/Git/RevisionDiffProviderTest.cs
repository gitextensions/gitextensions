using System;
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
        // Test cases that should assert in debug; should not occur but undefined behavior that should be blocked in GUI
        // Cannot compare worktree to worktree or index to index but give predictive output in release builds

        // Two empty parameters will compare working dir to index
        [TestCase(null)]
        [TestCase("")]
        [TestCase(GitRevision.WorkTreeGuid)]
        public void RevisionDiffProvider_should_return_empty_if_To_is_WorkTreeGuid(string firstRevision)
        {
            _revisionDiffProvider.Get(firstRevision, GitRevision.WorkTreeGuid).Arguments.Should().BeEmpty();
        }

        // Two index revisions gives duplicated options, no reason to clean
        [TestCase("^")]
        [TestCase(GitRevision.IndexGuid)]
        public void RevisionDiffProvider_should_return_cached_if_both_IndexGuid(string firstRevision)
        {
            _revisionDiffProvider.Get(firstRevision, GitRevision.IndexGuid).Arguments.Should().Be("--cached --cached");
        }
#endif

        // Combined Diff artificial commit should not be included in diffs
        [TestCase(GitRevision.CombinedDiffGuid, "")]
        [TestCase("", GitRevision.CombinedDiffGuid)]
        public void RevisionDiffProvider_should_throw_if_any_combined_diff(string firstRevision, string secondRevision)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _revisionDiffProvider.Get(firstRevision, secondRevision));
        }

        [TestCase(GitRevision.IndexGuid, GitRevision.WorkTreeGuid)]
        [TestCase("^", "")]
        [TestCase(GitRevision.IndexGuid, null)]
        public void RevisionDiffProvider_index_to_worktree(string firstRevision, string secondRevision)
        {
            _revisionDiffProvider.Get(firstRevision, secondRevision).Arguments.Should().BeEmpty();
        }

        [TestCase(GitRevision.WorkTreeGuid, GitRevision.IndexGuid)]
        [TestCase("", "^")]
        public void RevisionDiffProvider_worktree_to_index(string firstRevision, string secondRevision)
        {
            _revisionDiffProvider.Get(firstRevision, secondRevision).Arguments.Should().Be("-R");
        }

        [TestCase(GitRevision.WorkTreeGuid + "^^")]
        [TestCase(GitRevision.IndexGuid + "^")]
        [TestCase("HEAD")]
        public void RevisionDiffProvider_head_to_worktree(string firstRevision)
        {
            _revisionDiffProvider.Get(firstRevision, GitRevision.WorkTreeGuid).Arguments.Should().Be("\"HEAD\"");
        }

        [TestCase(GitRevision.IndexGuid + "^", "^")]
        [TestCase("HEAD", GitRevision.IndexGuid)]
        public void RevisionDiffProvider_head_to_index(string firstRevision, string secondRevision)
        {
            _revisionDiffProvider.Get(firstRevision, secondRevision).Arguments.Should().Be("--cached \"HEAD\"");
        }

        [TestCase(GitRevision.IndexGuid, "HEAD")]
        public void RevisionDiffProvider_index_to_head(string firstRevision, string secondRevision)
        {
            _revisionDiffProvider.Get(firstRevision, secondRevision).Arguments.Should().Be("-R --cached \"HEAD\"");
        }

        [TestCase("HEAD", "123456789")]
        public void RevisionDiffProvider_normal1(string firstRevision, string secondRevision)
        {
            _revisionDiffProvider.Get(firstRevision, secondRevision).Arguments.Should().Be("\"HEAD\" \"123456789\"");
        }

        [TestCase("123456789", "HEAD")]
        public void RevisionDiffProvider_normal2(string firstRevision, string secondRevision)
        {
            _revisionDiffProvider.Get(firstRevision, secondRevision).Arguments.Should().Be("\"123456789\" \"HEAD\"");
        }

        // Standard usage when filename is included
        [TestCase("123456789", GitRevision.WorkTreeGuid, "a.txt", null, true)]
        public void RevisionDiffProvider_fileName_tracked(string firstRevision, string secondRevision, string fileName, string oldFileName, bool isTracked)
        {
            _revisionDiffProvider.Get(firstRevision, secondRevision, fileName, oldFileName, isTracked).Arguments.Should().Be("\"123456789\" -- \"a.txt\"");
        }

        // If fileName is null, ignore oldFileName and tracked
        [TestCase("123456789", "HEAD", null, "b.txt", true)]
        public void RevisionDiffProvider_fileName_null_with_old_name(string firstRevision, string secondRevision, string fileName, string oldFileName, bool isTracked)
        {
            _revisionDiffProvider.Get(firstRevision, secondRevision, fileName, oldFileName, isTracked).Arguments.Should().Be("\"123456789\" \"HEAD\"");
        }

        // Include old filename if is included
        [TestCase("123456789", "234567890", "a.txt", "b.txt", true)]
        public void RevisionDiffProvider_fileName_old_filename(string firstRevision, string secondRevision, string fileName, string oldFileName, bool isTracked)
        {
            _revisionDiffProvider.Get(firstRevision, secondRevision, fileName, oldFileName, isTracked).Arguments.Should().Be("\"123456789\" \"234567890\" -- \"a.txt\" \"b.txt\"");
        }

        // normal test case when untracked is set
        [TestCase(GitRevision.IndexGuid, GitRevision.WorkTreeGuid, "a.txt", null, false)]
        public void RevisionDiffProvider_fileName_untracked(string firstRevision, string secondRevision, string fileName, string oldFileName, bool isTracked)
        {
            _revisionDiffProvider.Get(firstRevision, secondRevision, fileName, oldFileName, isTracked).Arguments.Should().Be("--no-index -- \"/dev/null\" \"a.txt\"");
        }

        // If fileName is null, ignore oldFileName and tracked
        [TestCase(GitRevision.IndexGuid, GitRevision.WorkTreeGuid, null, "b.txt", false)]
        public void RevisionDiffProvider_fileName_null_Untracked(string firstRevision, string secondRevision, string fileName, string oldFileName, bool isTracked)
        {
            _revisionDiffProvider.Get(firstRevision, secondRevision, fileName, oldFileName, isTracked).Arguments.Should().BeEmpty();
        }

        // Ignore revisions for untracked
        [TestCase("123456789", "234567890", "a.txt", "b.txt", false)]
        public void RevisionDiffProvider_fileName_old_filename_Untracked(string firstRevision, string secondRevision, string fileName, string oldFileName, bool isTracked)
        {
            _revisionDiffProvider.Get(firstRevision, secondRevision, fileName, oldFileName, isTracked).Arguments.Should().Be("--no-index -- \"/dev/null\" \"a.txt\"");
        }
    }
}
