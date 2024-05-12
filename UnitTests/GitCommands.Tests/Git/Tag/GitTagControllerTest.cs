using System.IO.Abstractions;
using GitCommands.Git;
using GitCommands.Git.Tag;
using GitExtensions.Extensibility.Git;
using NSubstitute;

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
            _uiCommands.Module.WorkingDir.Returns(_workingDir);
            _uiCommands.Module.GetPathForGitExecution(_tagMessageFile).Returns(_tagMessageFile);

            _controller = new GitTagController(_uiCommands, _fileSystem);
        }

        [Test]
        public void CreateTagWithMessageThrowsIfTheWindowIsNull()
        {
            GitCreateTagArgs args = CreateAnnotatedTagArgs();
            Assert.Throws<ArgumentNullException>(() => _controller.CreateTag(args, parentWindow: null));
        }

        [Test]
        public void CreateTagWithMessageWritesTagMessageFile()
        {
            GitCreateTagArgs args = CreateAnnotatedTagArgs();

            _controller.CreateTag(args, CreateTestingWindow());

            _fileSystem.File.Received(1).WriteAllText(_tagMessageFile, "hello world");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void CreateTagWithMessageDeletesTheTemporaryFileForUiResult(bool uiResult)
        {
            GitCreateTagArgs args = CreateAnnotatedTagArgs();

            _fileSystem.File.Exists(Arg.Is<string>(s => s != null)).Returns(true);

            _uiCommands.StartCommandLineProcessDialog(Arg.Any<IWin32Window>(), Arg.Is<IGitCommand>(cmd => cmd.Arguments.StartsWith("tag")))
                .Returns(uiResult);

            Assert.AreEqual(uiResult, _controller.CreateTag(args, CreateTestingWindow()));

            _fileSystem.File.Received(1).Delete(_tagMessageFile);
        }

        [Test]
        public void PassesCreatedArgsAndWindowToCommands()
        {
            GitCreateTagArgs args = CreateAnnotatedTagArgs();
            IWin32Window window = CreateTestingWindow();

            _controller.CreateTag(args, window);

            _uiCommands.Received(1).StartCommandLineProcessDialog(window, Arg.Is<IGitCommand>(cmd => cmd.Arguments.StartsWith("tag")));
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
