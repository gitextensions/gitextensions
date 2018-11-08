using FluentAssertions;
using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public sealed class RevisionReaderTests
    {
        private RevisionReader _revisionReader;

        [SetUp]
        public void Setup()
        {
            _revisionReader = new RevisionReader();
        }

        [Test]
        public void BuildArguments_should_do_something()
        {
            var args = _revisionReader.GetTestAccessor().BuildArgumentsBuildArguments(/* args */);
            args.ToString().Should().Be(".....");
        }
    }
}