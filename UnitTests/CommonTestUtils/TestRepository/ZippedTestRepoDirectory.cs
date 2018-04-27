using System.IO;
using System.IO.Compression;
using NUnit.Framework;

namespace CommonTestUtils.TestRepository
{
    public sealed class ZippedTestRepoDirectory : TestRepository
    {
        private readonly string _repoName;

        public ZippedTestRepoDirectory(string repoName)
        {
            _repoName = repoName;
        }

        protected override void CreateContent()
        {
            ZipFile.ExtractToDirectory(PackedSourceFilePath, ContentTargetPath);
        }

        private string PackedSourceFilePath => Path.Combine(TestContext.CurrentContext.TestDirectory, "repos", _repoName + ".zip");
    }
}