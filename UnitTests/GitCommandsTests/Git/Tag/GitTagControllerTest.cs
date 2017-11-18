using System.IO;
using System.IO.Abstractions;
using GitCommands.Git.Tag;
using NSubstitute;
using NUnit.Framework;

namespace GitCommandsTests.Git.Tag
{
    [TestFixture]
    public class GitTagControllerTest
    {
        private readonly string _workingDir = TestContext.CurrentContext.TestDirectory;
        private string _tagMessageFile;
        private IGitTagController _controller;
        private IFileSystem _fileSystem;

        [SetUp]
        public void Setup()
        {
            _tagMessageFile = Path.Combine(_workingDir, "TAGMESSAGE");

            _fileSystem = Substitute.For<IFileSystem>();
            _fileSystem.File.Returns(Substitute.For<FileBase>());

            _controller = new GitTagController(() => _workingDir, _fileSystem);
        }


        [Test]
        public void CreateTagWithMessageWritesTagMessageFile()
        {
            var args = new GitCreateTagArgs("tagname", "00000", TagOperation.Annotate, "hello world");

            _controller.GetCreateTagCommand(args);

            _fileSystem.File.Received(1).WriteAllText(_tagMessageFile, "hello world");
        }
    }
}
