using GitCommands.Git;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;
using System;
using System.IO;
using System.IO.Abstractions;
using TestMethod = NUnit.Framework.TestAttribute;

namespace GitExtensionsTest.GitCommands.Git
{
    class GitTagControllerTest
    {
        private const string TagName = "bla";
        private const string Revision = "0123456789";
        private const string TagMessage = "foo";
        private const string KeyId = "A9876F";
        private string WorkingDir = TestContext.CurrentContext.TestDirectory;
        private IGitModule _module;
        private IGitTagController _controller;
        private IGitUICommands _uiCommands;
        private IFileSystem _fileSystem;

        [SetUp]
        public void Setup()
        {
            _module = Substitute.For<IGitModule>();
            _uiCommands = Substitute.For<IGitUICommands>();
            _module.GetGitDirectory().Returns(x => WorkingDir);
            _fileSystem = Substitute.For<IFileSystem>();
            _fileSystem.File.Returns(Substitute.For<FileBase>());

            _controller = new GitTagController(_uiCommands, _module, _fileSystem);
        }

        private GitCreateTagArgs CreateDefaultArgs()
        {
            GitCreateTagArgs args = new GitCreateTagArgs();
            args.Revision = Revision;
            args.TagName = TagName;
            args.TagMessage = TagMessage;
            args.SignKeyId = KeyId;

            return args;
        }

        private GitCreateTagCmd CreateDefaultCommand()
        {
            GitCreateTagCmd cmd = new GitCreateTagCmd(CreateDefaultArgs());
            cmd.TagMessageFileName = Path.Combine(WorkingDir, "TAGMESSAGE");
            return cmd;
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Tag_sign_with_default_gpg(bool force)
        {
            GitCreateTagCmd cmd = CreateDefaultCommand();
            cmd.Args.Force = force;
            cmd.Args.OperationType = TagOperation.SignWithDefaultKey;

            string cmdLine = cmd.ToLine();

            string expectedCmdLine = $"tag{(force ? " -f" : "")} -s -F \"{Path.Combine(WorkingDir, "TAGMESSAGE")}\" \"{TagName}\" -- \"{Revision}\"";
            Assert.AreEqual(expectedCmdLine, cmdLine);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void Tag_name_null()
        {
            GitCreateTagCmd cmd = CreateDefaultCommand();
            cmd.Args.TagName = null;
            cmd.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Tag_revision_null()
        {
            GitCreateTagCmd cmd = CreateDefaultCommand();
            cmd.Args.Revision = null;
            cmd.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Tag_key_id_null()
        {
            GitCreateTagCmd cmd = CreateDefaultCommand();
            cmd.Args.OperationType = TagOperation.SignWithSpecificKey;
            cmd.Args.SignKeyId = null;

            cmd.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void Tag_operation_not_supported()
        {
            GitCreateTagCmd cmd = CreateDefaultCommand();
            cmd.Args.OperationType = (TagOperation)10;

            cmd.ToLine();
        }

        [TestCase(TagOperation.Lightweight)]
        [TestCase(TagOperation.Annotate)]
        [TestCase(TagOperation.SignWithDefaultKey)]
        [TestCase(TagOperation.SignWithSpecificKey)]
        public void Tag_supported_operation(TagOperation operation)
        {
            GitCreateTagCmd cmd = CreateDefaultCommand();
            cmd.Args.OperationType = operation;
            cmd.Args.Force = true;

            string actualCmdLine = cmd.ToLine();

            string switches = "";

            switch (operation)
            {
                case TagOperation.Lightweight:
                    break;
                case TagOperation.Annotate:
                    switches = $" -a -F \"{Path.Combine(WorkingDir, "TAGMESSAGE")}\"";
                    break;
                case TagOperation.SignWithDefaultKey:
                    switches = $" -s -F \"{Path.Combine(WorkingDir, "TAGMESSAGE")}\"";
                    break;
                case TagOperation.SignWithSpecificKey:
                    switches = $" -u {KeyId} -F \"{Path.Combine(WorkingDir, "TAGMESSAGE")}\"";
                    break;
            }

            string expectedCmdLine = $"tag -f{switches} \"{TagName}\" -- \"{Revision}\"";

            Assert.AreEqual(expectedCmdLine, actualCmdLine);
        }

        [TestMethod]
        public void CreateTagWithMessageAssignsTagMessageFile()
        {
            GitCreateTagArgs args = CreateDefaultArgs();
            args.OperationType = TagOperation.Annotate;

            _uiCommands.StartCommandLineProcessDialog(Arg.Do<IGitCommand>(
                cmd =>
                {
                    GitCreateTagCmd createTagCmd = cmd as GitCreateTagCmd;
                    Assert.AreEqual(Path.Combine(WorkingDir, "TAGMESSAGE"), createTagCmd.TagMessageFileName);
                }
                ), null);

            _controller.CreateTag(args, null);
            _uiCommands.Received(1).StartCommandLineProcessDialog(Arg.Any<IGitCommand>(), null);
        }

        [TestMethod]
        public void CreateTagUsesGivenArgs()
        {
            GitCreateTagArgs args = CreateDefaultArgs();

            _uiCommands.StartCommandLineProcessDialog(Arg.Do<IGitCommand>(
                cmd =>
                {
                    GitCreateTagCmd createTagCmd = cmd as GitCreateTagCmd;
                    Assert.AreEqual(args, createTagCmd.Args);
                }
                ), null);

            _controller.CreateTag(args, null);
            _uiCommands.Received(1).StartCommandLineProcessDialog(Arg.Any<IGitCommand>(), null);
        }

        [TestMethod]
        public void CreateTagWithMessageWritesTagMessageFile()
        {
            GitCreateTagArgs args = CreateDefaultArgs();
            args.OperationType = TagOperation.Annotate;

            _controller.CreateTag(args, null);
            var tagMagPath = Path.Combine(WorkingDir, "TAGMESSAGE");
            _fileSystem.File.Received(1).WriteAllText(tagMagPath, TagMessage);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CreateTagReturnsCmdResult(bool expectedCmdResult)
        {
            GitCreateTagArgs args = CreateDefaultArgs();
            args.OperationType = TagOperation.Annotate;
            _uiCommands.StartCommandLineProcessDialog(Arg.Any<IGitCommand>(), null).Returns(expectedCmdResult);

            bool actualCmdResult = _controller.CreateTag(args, null);
            Assert.AreEqual(expectedCmdResult, actualCmdResult);
        }

    }
}
