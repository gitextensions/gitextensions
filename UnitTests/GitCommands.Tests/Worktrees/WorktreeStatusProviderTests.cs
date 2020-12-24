using System.IO;
using System.Threading.Tasks;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitCommands.Worktrees;
using NUnit.Framework;

namespace GitCommandsTests.Worktrees
{
    [SetCulture("en-US")]
    [SetUICulture("en-US")]
    [TestFixture]

    [Parallelizable]
    internal class WorktreeStatusProviderTests
    {
        private IWorktreeStatusProvider _provider;
        private bool _isInit;
        private GitModuleTestHelper _repo;
        private GitModule _repoModule;

        [SetUp]
        public void SetUp()
        {
            if (_isInit)
            {
                return;
            }

            _isInit = true;
            _provider = new WorktreeStatusProvider();
            _repo = new GitModuleTestHelper("repo");

            _repo.AddWorktree(_repo, "../repo2");
            _repoModule = _repo.Module;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _provider.Dispose();
            _repo.Dispose();
        }

        [NonParallelizable]
        [Test]
        public async Task UpdateWorktree_valid_result()
        {
            var result = await WorktreeTestHelpers.UpdateWorktreeStructureAndWaitForResultAsync(_provider, _repoModule);

            result.Should().NotBeNull();
            var worktrees = result.Worktrees;
            worktrees.Count.Should().Be(2);
            worktrees[0].Branch.Should().Be("refs/heads/master");
            worktrees[1].Branch.Should().Be("refs/heads/repo2");
        }
    }
}
