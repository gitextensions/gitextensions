using System;
using GitCommands;

namespace CommonTestUtils.TestRepository
{
    public sealed class TestRepository : ITestRepository
    {
        private readonly ITestRepositoryData _repositoryData;
        private readonly Lazy<GitModule> _gitModuleFactory;

        public TestRepository(ITestRepositoryData repositoryData)
        {
            _repositoryData = repositoryData;
            _gitModuleFactory = new Lazy<GitModule>(() => new GitModule(repositoryData.ContentPath));
        }

        public GitModule Module => _gitModuleFactory.Value;

        public void Dispose()
        {
            _repositoryData?.Dispose();
        }
    }
}