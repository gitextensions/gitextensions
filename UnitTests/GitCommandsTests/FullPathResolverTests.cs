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
        private string _workingDir = @"c:\dev\repo";
        private FullPathResolver _resolver;


        [SetUp]
        public void Setup()
        {
            if (Type.GetType("Mono.Runtime") != null)
            {
                _workingDir = "/home/user/repo";
            }

            _resolver = new FullPathResolver(() => _workingDir);
        }


        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Resolve_should_throw_if_path_null_or_empty(string path)
        {
            ((Action)(() => _resolver.Resolve(path))).ShouldThrow<ArgumentNullException>();
        }

        [Platform(Include = "Win")]
        [TestCase(@"c:\")]
        public void Resolve_should_return_original_path_if_rooted(string path)
        {
            _resolver.Resolve(path).Should().Be(path);
        }

        [Platform(Exclude = "Win")]
        [TestCase(@"/home/user")]
        public void Resolve_should_return_original_path_if_rooted_Mono(string path)
        {
            _resolver.Resolve(path).Should().Be(path);
        }

        [Platform(Include = "Win")]
        [TestCase("folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\filename.txt")]
        public void Resolve_should_throw_PathTooLongException(string path)
        {
            ((Action)(() => _resolver.Resolve(path))).ShouldThrow<PathTooLongException>();
        }

        [Platform(Exclude = "Win")]
        [TestCase("folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\filename.txt")]
        public void Resolve_should_not_throw_PathTooLongException_Mono(string path)
        {
            ((Action)(() => _resolver.Resolve(path))).ShouldNotThrow<PathTooLongException>();
        }

        [Platform(Include = "Win")]
        [TestCase(@"file")]
        [TestCase("folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\folder\\filename.txt")]
        public void Resolve_should_return_full_path(string path)
        {
            _resolver.Resolve(path).Should().Be($"{_workingDir}\\{path}");
        }
    }
}