using GitCommands;

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
            string output = "Git result";
            string error = "No Git error!";
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
            string originalOutput = "Another Git result";
            string originalError = "Still no Git error.";

            _cache.Add("git command", originalOutput, originalError);

            Assert.IsTrue(_cache.TryGet("git command", out string? cachedOutput, out string? cachedError));
            Assert.AreEqual(cachedOutput, originalOutput);
            Assert.AreEqual(cachedError, originalError);
        }

        [Test]
        public void TestTryGetFails()
        {
            Assert.IsFalse(_cache.TryGet(null, out string? output, out string? error));
            Assert.IsFalse(_cache.TryGet("", out output, out error));
            Assert.IsNull(output);
            Assert.IsNull(error);
        }
    }
}
