using System;
using System.Threading;
using System.Threading.Tasks;
using CommonTestUtils;
using GitCommands;
using GitUI;
using GitUI.CommandsDialogs;
using LibGit2Sharp;
using NUnit.Framework;
using ObjectId = GitUIPluginInterfaces.ObjectId;
using ResetMode = LibGit2Sharp.ResetMode;

namespace GitUITests.CommandsDialogs.CommitDialog
{
    [Apartment(ApartmentState.STA)]
    public class FormFileHistoryTests
    {
        // Created once for the fixture
        private ReferenceRepository _referenceRepository;
        private GitModuleTestHelper _gitModuleTestHelper;

        // Created once for each test
        private GitUICommands _commands;

        [OneTimeSetUp]
        public void SetUpFixture()
        {
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _referenceRepository.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            if (_referenceRepository == null)
            {
                _gitModuleTestHelper = new GitModuleTestHelper();
                _referenceRepository = new ReferenceRepository(_gitModuleTestHelper);
            }
            else
            {
                _referenceRepository.Reset();
            }

            _commands = new GitUICommands(_referenceRepository.Module);
        }

        [TearDown]
        public void TearDown()
        {
            _commands = null;
        }

        [Test]
        public async Task FormFileHistory_should_produce_revision_filter_for_file_not_in_current_HEAD()
        {
            // Arrange repository with new file in another branch
            GitRevision featureBranchBCommitRevision;

            using (var repository = new Repository(_referenceRepository.Module.WorkingDir))
            {
                repository.Reset(ResetMode.Hard);

                var initialHead = repository.Head;

                // Create another branch with a new file commited to it
                var featureBranch = repository.CreateBranch("feature-b");
                Commands.Checkout(repository, featureBranch);
                _gitModuleTestHelper.CreateRepoFile("B.txt", "B");
                repository.Index.Add("B.txt");
                repository.Index.Write();

                var message = "B commit message";
                var author = new Signature("GitUITests", "unittests@gitextensions.com", DateTimeOffset.Now);
                var committer = author;
                var options = new CommitOptions();
                var commit = repository.Commit(message, author, committer, options);
                featureBranchBCommitRevision = new GitRevision(ObjectId.Parse(commit.Id.ToString()));

                // Return back to initial HEAD
                Commands.Checkout(repository, initialHead);
            }

            Task<(string revision, string path)> fileChangesFilterBuildTask = null;

            RunFormTest(
                form =>
                {
                    fileChangesFilterBuildTask = form.GetTestAccessor().FileChangesFilterBuildTask;
                    return fileChangesFilterBuildTask;
                }, "B.txt", featureBranchBCommitRevision);

            Assert.NotNull(fileChangesFilterBuildTask);
            Assert.AreEqual("B.txt".Quote(), (await fileChangesFilterBuildTask).path);
        }

        private void RunFormTest(Func<FormFileHistory, Task> testDriverAsync, string fileName, GitRevision revision)
        {
            UITest.RunForm(
                () =>
                {
                    _commands.StartFileHistoryDialog(owner: null, fileName, revision);
                },
                testDriverAsync);
        }
    }
}
