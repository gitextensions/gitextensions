using System;
using System.IO;
using System.IO.Compression;
using NUnit.Framework;

namespace GitCommandIntegrationTests
{
    internal sealed class TestRepoDirectory : IDisposable
    {
        private readonly string _repoName;
        private readonly string _randomId;

        public TestRepoDirectory(string repoName)
        {
            _repoName = repoName;
            _randomId = Guid.NewGuid().ToString("N");

            Unpack();
        }

        private void Unpack()
        {
            ZipFile.ExtractToDirectory(PackedSourceFilePath, PathToExtractedContent);
        }

        private string PackedSourceFilePath => Path.Combine(TestContext.CurrentContext.TestDirectory, "repos", _repoName + ".zip");
        public string PathToExtractedContent => Path.Combine(TestContext.CurrentContext.TestDirectory, ".testworkdir", _randomId);

        public void Dispose()
        {
            CleanupExtractedDirectory();
            GC.SuppressFinalize(this);
        }

        private void CleanupExtractedDirectory()
        {
            try
            {
                Directory.Delete(PathToExtractedContent, true);
            }
            catch
            {
                // ignore any error
            }
        }
    }
}