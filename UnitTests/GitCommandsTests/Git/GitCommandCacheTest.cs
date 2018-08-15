using System.Linq;
using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests.Git
{
    [TestFixture]
    public class GitCommandCacheTest
    {
        private CommandCache _cache;

        [SetUp]
        public void SetUp()
        {
            _cache = new CommandCache();
        }

        [Test]
        public void TestAdd()
        {
            byte[] output = { 11, 12 };
            byte[] error = { 13, 14 };
            string[] expectedCachedCommand = { "git command" };

            _cache.Add("git command", output, error);

            Assert.IsTrue(expectedCachedCommand.SequenceEqual(_cache.GetCachedCommands()));
        }

        [Test]
        public void TestAddCannotCache()
        {
            _cache.Add(null, null, null);
            Assert.IsFalse(_cache.GetCachedCommands().Any());
        }

        [Test]
        public void TestTryGet()
        {
            byte[] originalOutput = { 11, 12 };
            byte[] originalError = { 13, 14 };

            _cache.Add("git command", originalOutput, originalError);

            Assert.IsTrue(_cache.TryGet("git command", out var cachedOutput, out var cachedError));
            Assert.AreEqual(cachedOutput, originalOutput);
            Assert.AreEqual(cachedError, originalError);
        }

        [Test]
        public void TestTryGetFails()
        {
            Assert.IsFalse(_cache.TryGet(null, out var output, out var error));
            Assert.IsFalse(_cache.TryGet("", out output, out error));
            Assert.IsNull(output);
            Assert.IsNull(error);
        }
    }
}
