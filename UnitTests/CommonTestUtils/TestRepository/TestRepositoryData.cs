using System;
using System.IO;
using NUnit.Framework;

namespace CommonTestUtils.TestRepository
{
    public abstract class TestRepositoryData : ITestRepositoryData
    {
        private readonly string _randomId;

        protected TestRepositoryData()
        {
            _randomId = Guid.NewGuid().ToString("N");
        }

        protected string ContentTargetPath =>
            Path.Combine(TestContext.CurrentContext.TestDirectory, ".testworkdir", _randomId);
        protected abstract void CreateContent();

        string ITestRepositoryData.ContentPath => new Lazy<string>(CreatedContentPath).Value;

        private string CreatedContentPath()
        {
            CreateContent();
            return ContentTargetPath;
        }

        public void Dispose()
        {
            CleanupCreatedDirectory();
        }

        private void CleanupCreatedDirectory()
        {
            try
            {
                Directory.Delete(ContentTargetPath, true);
            }
            catch
            {
                // ignore any error
            }
        }
    }
}