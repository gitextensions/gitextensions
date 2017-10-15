using System;
using System.IO;
using System.IO.Abstractions;
using GitCommands.Git;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;
using TestMethod = NUnit.Framework.TestAttribute;

namespace GitCommandsTests.Git
{
    [TestFixture]
    public class GitTagControllerTest
    {
        private const string TagName = "bla";
        private const string Revision = "0123456789";
        private const string TagMessage = "foo";
        private const string KeyId = "A9876F";
        private readonly string _workingDir = TestContext.CurrentContext.TestDirectory;
        private string _tagMessageFile;
        private IGitModule _module;
        private IGitTagController _controller;
        private IGitUICommands _uiCommands;
        private IFileSystem _fileSystem;

        [SetUp]
        public void Setup()
        {
            _tagMessageFile = Path.Combine(_workingDir, "TAGMESSAGE");
            _module = Substitute.For<IGitModule>();
            _uiCommands = Substitute.For<IGitUICommands>();
            _module.WorkingDirGitDir.Returns(x => _workingDir);
            _fileSystem = Substitute.For<IFileSystem>();
            _fileSystem.File.Returns(Substitute.For<FileBase>());

            _controller = new GitTagController(_uiCommands, _module, _fileSystem);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Tag_sign_with_default_gpg(bool force)
        {
            var args = new GitCreateTagArgs(TagName, Revision, TagOperation.SignWithDefaultKey, TagMessage, KeyId, force);
            var cmd = new GitCreateTagCmd(args, _tagMessageFile);

            var cmdLine = cmd.ToLine();

            var expectedCmdLine = $"tag{(force ? " -f" : "")} -s -F \"{_tagMessageFile}\" \"{TagName}\" -- \"{Revision}\"";
            Assert.AreEqual(expectedCmdLine, cmdLine);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        [ExpectedException(typeof (ArgumentException))]
        public void Tag_name_null(string tagName)
        {
            var args = new GitCreateTagArgs(tagName, Revision);
            var cmd = new GitCreateTagCmd(args, _tagMessageFile);

            cmd.Validate();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        [ExpectedException(typeof(ArgumentException))]
        public void Tag_revision_null(string revision)
        {
            var args = new GitCreateTagArgs(TagName, revision);
            var cmd = new GitCreateTagCmd(args, _tagMessageFile);

            cmd.Validate();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        [ExpectedException(typeof(ArgumentException))]
        public void Tag_key_id_null(string signKeyId)
        {
            var args = new GitCreateTagArgs(TagName, Revision, TagOperation.SignWithSpecificKey, signKeyId: signKeyId);
            var cmd = new GitCreateTagCmd(args, _tagMessageFile);

            cmd.Validate();
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void Tag_operation_not_supported()
        {
            var args = new GitCreateTagArgs(TagName, Revision, (TagOperation)10);
            var cmd = new GitCreateTagCmd(args, _tagMessageFile);

            cmd.ToLine();
        }

        [TestCase(TagOperation.Lightweight)]
        [TestCase(TagOperation.Annotate)]
        [TestCase(TagOperation.SignWithDefaultKey)]
        [TestCase(TagOperation.SignWithSpecificKey)]
        public void Tag_supported_operation(TagOperation operation)
        {
            var args = new GitCreateTagArgs(TagName, Revision, operation, signKeyId: KeyId, force: true);
            var cmd = new GitCreateTagCmd(args, _tagMessageFile);

            var actualCmdLine = cmd.ToLine();

            var switches = "";
            switch (operation)
            {
                case TagOperation.Lightweight:
                    break;
                case TagOperation.Annotate:
                    switches = $" -a -F \"{_tagMessageFile}\"";
                    break;
                case TagOperation.SignWithDefaultKey:
                    switches = $" -s -F \"{_tagMessageFile}\"";
                    break;
                case TagOperation.SignWithSpecificKey:
                    switches = $" -u {KeyId} -F \"{_tagMessageFile}\"";
                    break;
            }

            var expectedCmdLine = $"tag -f{switches} \"{TagName}\" -- \"{Revision}\"";

            Assert.AreEqual(expectedCmdLine, actualCmdLine);
        }

        [TestMethod]
        public void CreateTagWithMessageAssignsTagMessageFile()
        {
            var args = new GitCreateTagArgs(TagName, Revision, TagOperation.Annotate);

            _uiCommands.StartCommandLineProcessDialog(Arg.Do<IGitCommand>(
                cmd =>
                {
                    var createTagCmd = cmd as GitCreateTagCmd;
                    Assert.AreEqual(_tagMessageFile, createTagCmd.TagMessageFileName);
                }
                ), null);

            _controller.CreateTag(args, null);
            _uiCommands.Received(1).StartCommandLineProcessDialog(Arg.Any<IGitCommand>(), null);
        }

        [TestMethod]
        public void CreateTagUsesGivenArgs()
        {
            var args = new GitCreateTagArgs(TagName, Revision, TagOperation.Lightweight, TagMessage, KeyId);

            _uiCommands.StartCommandLineProcessDialog(Arg.Do<IGitCommand>(
                cmd =>
                {
                    var createTagCmd = cmd as GitCreateTagCmd;
                    Assert.AreEqual(args, createTagCmd.Arguments);
                }
                ), null);

            _controller.CreateTag(args, null);
            _uiCommands.Received(1).StartCommandLineProcessDialog(Arg.Any<IGitCommand>(), null);
        }

        [TestMethod]
        public void CreateTagWithMessageWritesTagMessageFile()
        {
            var args = new GitCreateTagArgs(TagName, Revision, TagOperation.Annotate, TagMessage);

            _controller.CreateTag(args, null);
            _fileSystem.File.Received(1).WriteAllText(_tagMessageFile, TagMessage);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CreateTagReturnsCmdResult(bool expectedCmdResult)
        {
            var args = new GitCreateTagArgs(TagName, Revision, TagOperation.Annotate);
            _uiCommands.StartCommandLineProcessDialog(Arg.Any<IGitCommand>(), null).Returns(expectedCmdResult);

            var actualCmdResult = _controller.CreateTag(args, null);
            Assert.AreEqual(expectedCmdResult, actualCmdResult);
        }

    }
}
