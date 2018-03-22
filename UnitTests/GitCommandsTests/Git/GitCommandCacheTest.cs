using System.Linq;
using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests.Git
{
    [TestFixture]
    public class GitCommandCacheTest
    {
        [TearDown]
        public void Cleanup()
        {
            GitCommandCache.CleanCache();
        }

        [Test]
        public void TestAdd()
        {
            byte[] output = { 11, 12 };
            byte[] error = { 13, 14 };
            string[] expectedCachedCommand = { "git command" };

            GitCommandCache.Add("git command", output, error);

            Assert.IsTrue(expectedCachedCommand.SequenceEqual(GitCommandCache.CachedCommands()));
        }

        [Test]
        public void TestAddCannotCache()
        {
            GitCommandCache.Add(null, null, null);
            Assert.IsFalse(GitCommandCache.CachedCommands().Any());
        }

        [Test]
        public void TestTryGet()
        {
            byte[] originalOutput = { 11, 12 };
            byte[] originalError = { 13, 14 };

            GitCommandCache.Add("git command", originalOutput, originalError);

            Assert.IsTrue(GitCommandCache.TryGet("git command", out var cachedOutput, out var cachedError));
            Assert.AreEqual(cachedOutput, originalOutput);
            Assert.AreEqual(cachedError, originalError);
        }

        [Test]
        public void TestClean()
        {
            byte[] output = { 11, 12 };
            byte[] error = { 13, 14 };
            string[] expectedCachedCommand = { "git command" };

            GitCommandCache.Add("git command", output, error);
            Assert.IsTrue(expectedCachedCommand.SequenceEqual(GitCommandCache.CachedCommands()));
            GitCommandCache.CleanCache();
            Assert.IsFalse(GitCommandCache.CachedCommands().Any());
        }

        [Test]
        public void TestTryGetFails()
        {
            Assert.IsFalse(GitCommandCache.TryGet(null, out var output, out var error));
            Assert.IsFalse(GitCommandCache.TryGet(string.Empty, out output, out error));
            Assert.IsNull(output);
            Assert.IsNull(error);
        }
    }
}
