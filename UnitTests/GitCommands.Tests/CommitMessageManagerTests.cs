using System;
using System.Collections;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using NSubstitute;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public class CommitMessageManagerTests
    {
        private const string _commitMessage = "commit message";
        private const string _mergeMessage = "merge message";
        private const string _newMessage = "new message";
        private const string _overriddenCommitMessage = "commandline message";

        // Created once for the fixture
        private ReferenceRepository _referenceRepository;

        private readonly string _workingDirGitDir = @"c:\dev\repo\.git";
        private readonly Encoding _encoding = Encoding.UTF8;

        private readonly string _amendSaveStatePath;
        private readonly string _commitMessagePath;
        private readonly string _mergeMessagePath;
        private readonly bool _rememberAmendCommitState;

        private FileBase _file;
        private DirectoryBase _directory;
        private IFileSystem _fileSystem;
        private CommitMessageManager _manager;

        public CommitMessageManagerTests()
        {
            _amendSaveStatePath = Path.Combine(_workingDirGitDir, "GitExtensions.amend");
            _commitMessagePath = Path.Combine(_workingDirGitDir, "COMMITMESSAGE");
            _mergeMessagePath = Path.Combine(_workingDirGitDir, "MERGE_MSG");
            _rememberAmendCommitState = AppSettings.RememberAmendCommitState;
        }

        [SetUp]
        public void Setup()
        {
            if (_referenceRepository == null)
            {
                _referenceRepository = new ReferenceRepository();
            }
            else
            {
                _referenceRepository.Reset();
            }

            _file = Substitute.For<FileBase>();
            _file.ReadAllText(_commitMessagePath, _encoding).Returns(_commitMessage);
            _file.ReadAllText(_mergeMessagePath, _encoding).Returns(_mergeMessage);
            _directory = Substitute.For<DirectoryBase>();

            var path = Substitute.For<PathBase>();
            path.Combine(Arg.Any<string>(), Arg.Any<string>()).Returns(x => Path.Combine((string)x[0], (string)x[1]));

            _fileSystem = Substitute.For<IFileSystem>();
            _fileSystem.File.Returns(_file);
            _fileSystem.Directory.Returns(_directory);
            _fileSystem.Path.Returns(path);

            _manager = new CommitMessageManager(_workingDirGitDir, _encoding, _fileSystem, overriddenCommitMessage: null);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _referenceRepository.Dispose();
        }

        public void SetupExtra(string overriddenCommitMessage)
        {
            _manager = new CommitMessageManager(_workingDirGitDir, _encoding, _fileSystem, overriddenCommitMessage);
        }

        [TearDown]
        public void TearDown()
        {
            AppSettings.RememberAmendCommitState = _rememberAmendCommitState;
        }

        [TestCase(null)]
        public void Constructor_should_throw(string workingDirGitDir)
        {
            ((Action)(() => new CommitMessageManager(workingDirGitDir, _encoding))).Should().Throw<ArgumentNullException>();
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("::")]
        public void Constructor_should_not_throw(string workingDirGitDir)
        {
            new CommitMessageManager(workingDirGitDir, _encoding).Should().NotBeNull();
        }

        [Test]
        public void AmendState_should_be_false_if_file_is_missing()
        {
            _file.Exists(_amendSaveStatePath).Returns(false);

            AppSettings.RememberAmendCommitState = true;
            _manager.AmendState.Should().BeFalse();
        }

        [Test]
        public void AmendState_should_be_false_if_not_RememberAmendCommitState()
        {
            _file.Exists(_amendSaveStatePath).Returns(true);

            AppSettings.RememberAmendCommitState = false;
            _manager.AmendState.Should().BeFalse();
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\n")]
        [TestCase("0")]
        [TestCase("1")]
        [TestCase("false")]
        [TestCase("yes")]
        [TestCase("on")]
        [TestCase("checked")]
        [TestCase("true.")]
        [TestCase("true\nx")]
        public void AmendState_should_be_false_if_file_contains(string amendText)
        {
            _file.Exists(_amendSaveStatePath).Returns(true);
            _file.ReadAllText(_amendSaveStatePath).Returns(amendText);

            AppSettings.RememberAmendCommitState = true;
            _manager.AmendState.Should().BeFalse();
        }

        [TestCase("true")]
        [TestCase("True")]
        [TestCase("TrUe")]
        [TestCase("true ")]
        [TestCase("true\n")]
        public void AmendState_should_be_true_if_file_contains(string amendText)
        {
            _file.Exists(_amendSaveStatePath).Returns(true);
            _file.ReadAllText(_amendSaveStatePath).Returns(amendText);

            AppSettings.RememberAmendCommitState = true;
            _manager.AmendState.Should().BeTrue();
        }

        [Test]
        public void AmendState_true_should_write_true_to_file()
        {
            bool correctlyWritten = false;
            _file.When(x => x.WriteAllText(_amendSaveStatePath, true.ToString())).Do(_ => correctlyWritten = true);

            AppSettings.RememberAmendCommitState = true;
            _manager.AmendState = true;

            Assert.That(correctlyWritten);
        }

        [Test]
        public void AmendState_true_should_delete_file_if_not_RememberAmendCommitState()
        {
            bool correctlyDeleted = false;
            _file.When(x => x.Delete(_amendSaveStatePath)).Do(_ => correctlyDeleted = true);

            AppSettings.RememberAmendCommitState = false;
            _manager.AmendState = true;

            Assert.That(correctlyDeleted);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void AmendState_false_should_delete_file(bool rememberAmendCommitState)
        {
            bool correctlyDeleted = false;
            _file.When(x => x.Delete(_amendSaveStatePath)).Do(_ => correctlyDeleted = true);

            AppSettings.RememberAmendCommitState = rememberAmendCommitState;
            _manager.AmendState = false;

            Assert.That(correctlyDeleted);
        }

        [Test]
        public void CommitMessagePath()
        {
            _manager.CommitMessagePath.Should().Be(_commitMessagePath);
        }

        [TestCase(false, false)]
        [TestCase(true, true)]
        public void IsMergeCommit(bool fileExists, bool expected)
        {
            _file.Exists(_manager.MergeMessagePath).Returns(x => fileExists);

            _manager.IsMergeCommit.Should().Be(expected);
        }

        [Test]
        public void WriteCommitMessageToFile_should_write_COMMITMESSAGE()
        {
            var manager = new CommitMessageManager(_referenceRepository.Module.WorkingDir, _referenceRepository.Module.CommitEncoding, overriddenCommitMessage: null);

            File.Exists(manager.CommitMessagePath).Should().BeFalse();

            // null message isn't formatted, since we're only interested in testing File.Write logic
            string message = null;
            manager.WriteCommitMessageToFile(message, CommitMessageType.Normal, false, false);

            File.Exists(manager.CommitMessagePath).Should().BeTrue();
        }

        [Test]
        public void WriteCommitMessageToFile_should_write_MERGE_MSG()
        {
            var manager = new CommitMessageManager(_referenceRepository.Module.WorkingDir, _referenceRepository.Module.CommitEncoding, overriddenCommitMessage: null);

            File.Exists(manager.MergeMessagePath).Should().BeFalse();

            // null message isn't formatted, since we're only interested in testing File.Write logic
            string message = null;
            manager.WriteCommitMessageToFile(message, CommitMessageType.Merge, false, false);

            File.Exists(manager.MergeMessagePath).Should().BeTrue();
        }

        [Test]
        public void MergeMessagePath()
        {
            _manager.MergeMessagePath.Should().Be(_mergeMessagePath);
        }

        [Test]
        public void MergeOrCommitMessage_should_return_merge_message_if_exists()
        {
            _file.Exists(_commitMessagePath).Returns(true);
            _file.Exists(_mergeMessagePath).Returns(true);

            _manager.MergeOrCommitMessage.Should().Be(_mergeMessage);

            _manager.IsMergeCommit.Should().BeTrue();
        }

        [Test]
        public void MergeOrCommitMessage_should_return_overridden_message_if_set()
        {
            SetupExtra(overriddenCommitMessage: _overriddenCommitMessage);
            _file.Exists(_commitMessagePath).Returns(true);
            _file.Exists(_mergeMessagePath).Returns(true);

            _manager.MergeOrCommitMessage.Should().Be(_overriddenCommitMessage);

            _manager.IsMergeCommit.Should().BeTrue();
        }

        [Test]
        public void MergeOrCommitMessage_should_return_commit_message_if_exists_and_no_merge_message()
        {
            _file.Exists(_commitMessagePath).Returns(true);
            _file.Exists(_mergeMessagePath).Returns(false);

            _manager.MergeOrCommitMessage.Should().Be(_commitMessage);

            _manager.IsMergeCommit.Should().BeFalse();
        }

        [Test]
        public void MergeOrCommitMessage_should_return_empty_if_no_file_exists()
        {
            _file.Exists(_commitMessagePath).Returns(false);
            _file.Exists(_mergeMessagePath).Returns(false);

            _manager.MergeOrCommitMessage.Should().BeEmpty();

            _manager.IsMergeCommit.Should().BeFalse();
        }

        [Test]
        public void MergeOrCommitMessage_should_return_overridden_if_exist_and_if_no_file_exists()
        {
            SetupExtra(_overriddenCommitMessage);
            _file.Exists(_commitMessagePath).Returns(false);
            _file.Exists(_mergeMessagePath).Returns(false);

            _manager.MergeOrCommitMessage.Should().Be(_overriddenCommitMessage);

            _manager.IsMergeCommit.Should().BeFalse();
        }

        [Test]
        public void MergeOrCommitMessage_should_write_merge_message_if_exists()
        {
            bool correctlyWritten = false;
            _file.When(x => x.WriteAllText(_mergeMessagePath, _newMessage, _encoding)).Do(_ => correctlyWritten = true);
            _file.Exists(_commitMessagePath).Returns(true);
            _file.Exists(_mergeMessagePath).Returns(true);
            _directory.Exists(Path.GetDirectoryName(_commitMessagePath)).Returns(true);

            _manager.MergeOrCommitMessage = _newMessage;

            Assert.That(correctlyWritten);
        }

        [Test]
        public void MergeOrCommitMessage_should_not_write_merge_message_if_exist_if_it_is_the_overridding_commitmessage_exists()
        {
            SetupExtra(_overriddenCommitMessage);
            bool hasBeenWritten = false;
            _file.When(x => x.WriteAllText(_mergeMessagePath, _newMessage, _encoding)).Do(_ => hasBeenWritten = true);
            _file.Exists(_commitMessagePath).Returns(true);
            _file.Exists(_mergeMessagePath).Returns(true);

            _manager.MergeOrCommitMessage = _overriddenCommitMessage;

            hasBeenWritten.Should().BeFalse();
        }

        [Test]
        public void MergeOrCommitMessage_should_write_commit_message_if_exists_and_no_merge_message()
        {
            bool correctlyWritten = false;
            _file.When(x => x.WriteAllText(_commitMessagePath, _newMessage, _encoding)).Do(_ => correctlyWritten = true);
            _file.Exists(_commitMessagePath).Returns(true);
            _file.Exists(_mergeMessagePath).Returns(false);
            _directory.Exists(Path.GetDirectoryName(_commitMessagePath)).Returns(true);

            _manager.MergeOrCommitMessage = _newMessage;

            Assert.That(correctlyWritten);
        }

        [Test]
        public void MergeOrCommitMessage_should_write_commit_message_if_no_file_exists()
        {
            bool correctlyWritten = false;
            _file.When(x => x.WriteAllText(_commitMessagePath, _newMessage, _encoding)).Do(_ => correctlyWritten = true);
            _file.Exists(_commitMessagePath).Returns(false);
            _file.Exists(_mergeMessagePath).Returns(false);
            _directory.Exists(Path.GetDirectoryName(_commitMessagePath)).Returns(true);

            _manager.MergeOrCommitMessage = _newMessage;

            Assert.That(correctlyWritten);
        }

        [Test]
        public void MergeOrCommitMessage_should_write_empty_message_if_null()
        {
            bool correctlyWritten = false;
            _file.When(x => x.WriteAllText(_commitMessagePath, string.Empty, _encoding)).Do(_ => correctlyWritten = true);
            _file.Exists(_commitMessagePath).Returns(false);
            _file.Exists(_mergeMessagePath).Returns(false);
            _directory.Exists(Path.GetDirectoryName(_commitMessagePath)).Returns(true);

            _manager.MergeOrCommitMessage = null;

            Assert.That(correctlyWritten);
        }

        [Test]
        public void MergeOrCommitMessage_should_not_write_if_dir_no_longer_exists()
        {
            bool correctlyWritten = false;
            _file.When(x => x.WriteAllText(_commitMessagePath, string.Empty, _encoding)).Do(_ => correctlyWritten = true);
            _file.Exists(_commitMessagePath).Returns(false);
            _file.Exists(_mergeMessagePath).Returns(false);
            _directory.Exists(Path.GetDirectoryName(_commitMessagePath)).Returns(false);

            _manager.MergeOrCommitMessage = null;

            Assert.That(!correctlyWritten);
        }

        [Test]
        public void ResetCommitMessage()
        {
            bool deletedA = false;
            bool deletedC = false;
            bool deletedM = false;
            _file.When(x => x.Delete(_amendSaveStatePath)).Do(_ => deletedA = true);
            _file.When(x => x.Delete(_commitMessagePath)).Do(_ => deletedC = true);
            _file.When(x => x.Delete(_mergeMessagePath)).Do(_ => deletedM = true);

            _manager.ResetCommitMessage();

            Assert.That(deletedA);
            Assert.That(deletedC);
            Assert.That(!deletedM);
        }

        [Test, TestCaseSource(typeof(FormatCommitMessageTestData), nameof(FormatCommitMessageTestData.FormatCommitMessageTestCases))]
        public void FormatCommitMessage(
        string commitMessageText, bool usingCommitTemplate, bool ensureCommitMessageSecondLineEmpty, string expectedMessage)
        {
            CommitMessageManager.FormatCommitMessage(commitMessageText, usingCommitTemplate, ensureCommitMessageSecondLineEmpty)
                .Should().Be(expectedMessage);
        }

        public class FormatCommitMessageTestData
        {
            private static readonly string NL = Environment.NewLine;

            public static IEnumerable FormatCommitMessageTestCases
            {
                get
                {
                    // string commitMessageText, bool usingCommitTemplate, bool ensureCommitMessageSecondLineEmpty, string expectedMessage
                    yield return new TestCaseData(new object[] { null, false, false, "" });
                    yield return new TestCaseData(new object[] { null, true, false, "" });
                    yield return new TestCaseData(new object[] { null, false, true, "" });
                    yield return new TestCaseData(new object[] { null, true, true, "" });
                    yield return new TestCaseData(new object[] { "", false, false, "" });
                    yield return new TestCaseData(new object[] { "", true, false, "" });
                    yield return new TestCaseData(new object[] { "", false, true, "" });
                    yield return new TestCaseData(new object[] { "", true, true, "" });
                    yield return new TestCaseData(new object[] { "\n", false, false, NL + NL });
                    yield return new TestCaseData(new object[] { "\n", true, false, NL + NL });
                    yield return new TestCaseData(new object[] { "\n", false, true, NL + NL });
                    yield return new TestCaseData(new object[] { "\n", true, true, NL + NL });
                    yield return new TestCaseData(new object[] { "1", true, false, "1" + NL });
                    yield return new TestCaseData(new object[] { "#1", false, false, "#1" + NL });
                    yield return new TestCaseData(new object[] { "#1", true, false, "" });
                    yield return new TestCaseData(new object[] { "1\n\n3", false, false, "1" + NL + NL + "3" + NL });
                    yield return new TestCaseData(new object[] { "1\n\n3", false, true, "1" + NL + NL + "3" + NL });
                    yield return new TestCaseData(new object[] { "1\n2\n3", false, false, "1" + NL + "2" + NL + "3" + NL });
                    yield return new TestCaseData(new object[] { "1\n2\n3", false, true, "1" + NL + NL + "2" + NL + "3" + NL });
                    yield return new TestCaseData(new object[] { "#0\n1\n\n3", true, false, "1" + NL + NL + "3" + NL });
                    yield return new TestCaseData(new object[] { "#0\n1\n\n3", true, true, "1" + NL + NL + "3" + NL });
                    yield return new TestCaseData(new object[] { "#0\n1\n2\n3", true, false, "1" + NL + "2" + NL + "3" + NL });
                    yield return new TestCaseData(new object[] { "#0\n1\n2\n3", true, true, "1" + NL + NL + "2" + NL + "3" + NL });
                    yield return new TestCaseData(new object[] { "#0\n1\n#0\n2\n3", true, true, "1" + NL + NL + "2" + NL + "3" + NL });
                    yield return new TestCaseData(new object[] { "1\n2\n3\n4\n5\n\n7\n\n\n10", true, true, "1" + NL + NL + "2" + NL + "3" + NL + "4" + NL + "5" + NL + NL + "7" + NL + NL + NL + "10" + NL });
                    yield return new TestCaseData(new object[] { "1\n2\n3\n4\n5\n\n7\n\n\n10", false, true, "1" + NL + NL + "2" + NL + "3" + NL + "4" + NL + "5" + NL + NL + "7" + NL + NL + NL + "10" + NL });
                    yield return new TestCaseData(new object[] { "1\n2\n3\n4\n5\n\n7\n\n\n10\n", false, true, "1" + NL + NL + "2" + NL + "3" + NL + "4" + NL + "5" + NL + NL + "7" + NL + NL + NL + "10" + NL + NL });
                }
            }
        }
    }
}
