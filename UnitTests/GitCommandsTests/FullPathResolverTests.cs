using System;
using System.IO;
using FluentAssertions;
using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public class FullPathResolverTests
    {
        private readonly string _workingDir = @"c:\dev\repo";
        private FullPathResolver _resolver;

        [SetUp]
        public void Setup()
        {
            _resolver = new FullPathResolver(() => _workingDir);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Resolve_should_throw_if_path_null_or_empty(string path)
        {
            ((Action)(() => _resolver.Resolve(path))).Should().Throw<ArgumentNullException>();
        }

        [TestCase(@"c:\")]
        public void Resolve_should_return_original_path_if_rooted(string path)
        {
            _resolver.Resolve(path).Should().Be(path);
        }

        [TestCase("folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\filename.txt")]
        public void Resolve_should_throw_PathTooLongException(string path)
        {
            ((Action)(() => _resolver.Resolve(path))).Should().Throw<PathTooLongException>();
        }

        [TestCase(@"file")]
        [TestCase("folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\filename.txt")]
        public void Resolve_should_return_full_path(string path)
        {
            _resolver.Resolve(path).Should().Be($"{_workingDir}\\{path}");
        }
    }
}