using System;
using System.Linq;
using GitCommands;
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestMethod = NUnit.Framework.TestAttribute;

namespace GitCommandsTests.Git
{

    [TestClass]
    public class GitCommandCacheTest
    {
        [TestCleanup]
        public void Cleanup()
        {
            GitCommandCache.CleanCache();
        }

        [TestMethod]
        public void TestAdd()
        {
            byte[] output = new byte[] { 11, 12 };
            byte[] error = new byte[] { 13, 14 };
            string[] expectedCachedCommand = { "git command" };

            GitCommandCache.Add("git command", output, error);

            Assert.IsTrue(expectedCachedCommand.SequenceEqual(GitCommandCache.CachedCommands()));

        }

        [TestMethod]
        public void TestAddCannotCache()
        {
            GitCommandCache.Add(null, null, null);
            Assert.IsFalse(GitCommandCache.CachedCommands().Any());
        }

        [TestMethod]
        public void TestTryGet()
        {
            byte[] originalOutput = new byte[] { 11, 12 };
            byte[] originalError = new byte[] { 13, 14 };

            GitCommandCache.Add("git command", originalOutput, originalError);

            Assert.IsTrue(GitCommandCache.TryGet("git command", out var cachedOutput, out var cachedError));
            Assert.AreEqual(cachedOutput, originalOutput);
            Assert.AreEqual(cachedError, originalError);
        }

        [TestMethod]
        public void TestClean()
        {
            byte[] output = new byte[] { 11, 12 };
            byte[] error = new byte[] { 13, 14 };
            string[] expectedCachedCommand = { "git command" };

            GitCommandCache.Add("git command", output, error);
            Assert.IsTrue(expectedCachedCommand.SequenceEqual(GitCommandCache.CachedCommands()));
            GitCommandCache.CleanCache();
            Assert.IsFalse(GitCommandCache.CachedCommands().Any());
        }

        [TestMethod]
        public void TestTryGetFails()
        {
            Assert.IsFalse(GitCommandCache.TryGet(null, out var output, out var error));
            Assert.IsFalse(GitCommandCache.TryGet(String.Empty, out output, out error));
            Assert.IsNull(output);
            Assert.IsNull(error);
        }
    }
}

