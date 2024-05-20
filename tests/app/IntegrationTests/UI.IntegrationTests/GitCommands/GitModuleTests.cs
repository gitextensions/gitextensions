using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitExtensions.Extensibility.Git;

namespace GitCommandsTests
{
    [TestFixture]
    public sealed class GitModuleTests
    {
        private ReferenceRepository _refRepo;
        private GitModule _gitModule;

        [SetUp]
        public void SetUp()
        {
            _refRepo = new();
            _gitModule = _refRepo.Module;
            Console.WriteLine("Repo path: " + _gitModule.WorkingDir);
        }

        [Test]
        public void GetCommitCount_Should_manage_ambiguous_argument()
        {
            const string ambiguousName = "script";
            CommitData initialCommit = CreateCommitWithAmbiguousFolder("commit1");
            CreateCommitWithAmbiguousFolder("commit2");
            CommitData tipOfMainBranchCommit = CreateCommitWithAmbiguousFolder("commit3");

            // Create a branch with same name than a folder inside repo
            _refRepo.CreateBranch(ambiguousName, tipOfMainBranchCommit.Hash);

            int? exitCode = _gitModule.GetCommitCount(initialCommit.Hash, ambiguousName);

            exitCode.Should().Be(0);

            CommitData CreateCommitWithAmbiguousFolder(string commitMessage)
            {
                string hash = _refRepo.CreateCommitRelative(ambiguousName, "main_file.txt", commitMessage, commitMessage);
                return new CommitData(hash, commitMessage, $"{hash} {commitMessage}", ObjectId.Parse(hash));
            }
        }
    }

    internal record struct CommitData(string Hash, string CommitMessage, string RebaseLine, ObjectId ObjectId)
    {
    }
}
