using System;
using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests.Git
{
    [TestFixture]
    public sealed class ExecutableExtensionsTests
    {
        private MockExecutable _executable;

        [SetUp]
        public void SetUp()
        {
            _executable = new MockExecutable();
        }

        [TearDown]
        public void TearDown()
        {
            _executable.Verify();
        }

        [Test]
        public void GetOutput_with_cache_hit()
        {
            const string arguments = "abc";

            var cache = new CommandCache();

            cache.Add(
                arguments,
                output: GitModule.SystemEncoding.GetBytes("Hello"),
                error: GitModule.SystemEncoding.GetBytes("World!"));

            var output = _executable.GetOutput(arguments, cache: cache);

            Assert.AreEqual($"Hello{Environment.NewLine}World!", output);

            // Cache should still have a single item
            Assert.AreEqual(1, cache.GetCachedCommands().Count);
        }

        [Test]
        public void GetOutput_with_cache_miss()
        {
            const string arguments = "abc";
            const string commandOutput = "Hello World!";

            // Empty cache
            var cache = new CommandCache();

            using (_executable.StageOutput(arguments, commandOutput))
            {
                var output = _executable.GetOutput(arguments, cache: cache);

                Assert.AreEqual(commandOutput, output);
            }

            // Validate data stored in cache afterwards
            Assert.AreEqual(1, cache.GetCachedCommands().Count);
            Assert.IsTrue(cache.TryGet(arguments, out var outputBytes, out var errorBytes));
            Assert.AreEqual(GitModule.SystemEncoding.GetBytes(commandOutput), outputBytes);
            Assert.IsEmpty(errorBytes);
        }
    }
}