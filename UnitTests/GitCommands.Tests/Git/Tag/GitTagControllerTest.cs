using System;
using System.IO;
using System.IO.Abstractions;
using System.Windows.Forms;
using GitCommands.Git.Tag;
using GitUIPluginInterfaces;
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
        private IGitUICommands _uiCommands;

        [SetUp]
        public void Setup()
        {
            _tagMessageFile = Path.Combine(_workingDir, "TAGMESSAGE");

            _fileSystem = Substitute.For<IFileSystem>();
            _fileSystem.File.Returns(Substitute.For<FileBase>());

            _uiCommands = Substitute.For<IGitUICommands>();
            _uiCommands.GitModule.WorkingDir.Returns(_workingDir);

            _controller = new GitTagController(_uiCommands, _fileSystem);
        }

        [Test]
        public void CreateTagWithMessageThrowsIfTheWindowIsNull()
        {
            var args = CreateAnnotatedTagArgs();
            Assert.Throws<ArgumentNullException>(() => _controller.CreateTag(args, null));
        }

        [Test]
        public void CreateTagWithMessageWritesTagMessageFile()
        {
            var args = CreateAnnotatedTagArgs();

            _controller.CreateTag(args, CreateTestingWindow());

            _fileSystem.File.Received(1).WriteAllText(_tagMessageFile, "hello world");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void CreateTagWithMessageDeletesTheTemporaryFileForUiResult(bool uiResult)
        {
            var args = CreateAnnotatedTagArgs();

            _fileSystem.File.Exists(Arg.Is<string>(s => s != null)).Returns(true);

            _uiCommands.StartCommandLineProcessDialog(Arg.Any<IWin32Window>(), Arg.Any<GitCreateTagCmd>())
                .Returns(uiResult);

            Assert.AreEqual(uiResult, _controller.CreateTag(args, CreateTestingWindow()));

            _fileSystem.File.Received(1).Delete(_tagMessageFile);
        }

        [Test]
        public void PassesCreatedArgsAndWindowToCommands()
        {
            var args = CreateAnnotatedTagArgs();
            var window = CreateTestingWindow();

            _controller.CreateTag(args, window);

            _uiCommands.Received(1).StartCommandLineProcessDialog(
                window, Arg.Is<GitCreateTagCmd>(c => c.CreateTagArguments == args));
        }

        private static IWin32Window CreateTestingWindow()
        {
            return Substitute.For<IWin32Window>();
        }

        private static GitCreateTagArgs CreateAnnotatedTagArgs()
        {
            return new GitCreateTagArgs("tagname", ObjectId.Parse("0000000000000000000000000000000000000000"), TagOperation.Annotate, "hello world");
        }
    }
}
