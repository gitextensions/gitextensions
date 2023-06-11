using System.Collections;
using System.IO.Abstractions;
using System.Text;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using NSubstitute;

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
        private readonly Encoding _encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        private readonly string _amendSaveStatePath;
        private readonly string _commitMessagePath;
        private readonly string _mergeMessagePath;
        private readonly bool _rememberAmendCommitState;

        private FileBase _file;
        private DirectoryBase _directory;
        private IFileSystem _fileSystem;
        private CommitMessageManager _manager;

        // We don't expect any failures so that we won't be switching to the main thread or showing messages
        private readonly Control? _owner = null!;

        public CommitMessageManagerTests()
        {
            _amendSaveStatePath = Path.Combine(_workingDirGitDir, "GitExtensions.amend");
            _commitMessagePath = Path.Combine(_workingDirGitDir, "COMMITMESSAGE");
            _mergeMessagePath = Path.Combine(_workingDirGitDir, "MERGE_MSG");
            _rememberAmendCommitState = AppSettings.RememberAmendCommitState;
        }

        [OneTimeSetUp]
        public void Init()
        {
            AppSettings.LoadSettings();
        }

        [SetUp]
        public void Setup()
        {
            ReferenceRepository.ResetRepo(ref _referenceRepository);

            _file = Substitute.For<FileBase>();
            _file.ReadAllTextAsync(_commitMessagePath, _encoding, cancellationToken: default).Returns(_commitMessage);
            _file.ReadAllTextAsync(_mergeMessagePath, _encoding, cancellationToken: default).Returns(_mergeMessage);
            _directory = Substitute.For<DirectoryBase>();

            var path = Substitute.For<PathBase>();
            path.Combine(Arg.Any<string>(), Arg.Any<string>()).Returns(x => Path.Combine((string)x[0], (string)x[1]));

            _fileSystem = Substitute.For<IFileSystem>();
            _fileSystem.File.Returns(_file);
            _fileSystem.Directory.Returns(_directory);
            _fileSystem.Path.Returns(path);

            _manager = new CommitMessageManager(_owner, _workingDirGitDir, _encoding, _fileSystem, overriddenCommitMessage: null);
        }

        [TearDown]
        public void TearDown()
        {
            AppSettings.RememberAmendCommitState = _rememberAmendCommitState;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _referenceRepository.Dispose();
        }

        public void SetupExtra(string overriddenCommitMessage)
        {
            _manager = new CommitMessageManager(_owner, _workingDirGitDir, _encoding, _fileSystem, overriddenCommitMessage);
        }

        [TestCase(null)]
        public void Constructor_should_throw(string workingDirGitDir)
        {
            ((Action)(() => new CommitMessageManager(_owner, workingDirGitDir, _encoding))).Should().Throw<ArgumentNullException>();
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("::")]
        public void Constructor_should_not_throw(string workingDirGitDir)
        {
            new CommitMessageManager(_owner, workingDirGitDir, _encoding).Should().NotBeNull();
        }

        [Test]
        public async Task GetAmendStateAsync_should_be_false_if_file_is_missing()
        {
            _file.Exists(_amendSaveStatePath).Returns(false);

            AppSettings.RememberAmendCommitState = true;
            (await _manager.GetAmendStateAsync()).Should().BeFalse();
        }

        [Test]
        public async Task GetAmendStateAsync_should_be_false_if_not_RememberAmendCommitState()
        {
            _file.Exists(_amendSaveStatePath).Returns(true);

            AppSettings.RememberAmendCommitState = false;
            (await _manager.GetAmendStateAsync()).Should().BeFalse();
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
        public async Task GetAmendStateAsync_should_be_false_if_file_contains(string amendText)
        {
            _file.Exists(_amendSaveStatePath).Returns(true);
            _file.ReadAllText(_amendSaveStatePath, Encoding.Default).Returns(amendText);

            AppSettings.RememberAmendCommitState = true;
            (await _manager.GetAmendStateAsync()).Should().BeFalse();
        }

        [TestCase("true")]
        [TestCase("True")]
        [TestCase("TrUe")]
        [TestCase("true ")]
        [TestCase("true\n")]
        public async Task GetAmendStateAsync_should_be_true_if_file_contains(string amendText)
        {
            _file.Exists(_amendSaveStatePath).Returns(true);
            _file.ReadAllTextAsync(_amendSaveStatePath, Encoding.Default, cancellationToken: default).Returns(amendText);

            AppSettings.RememberAmendCommitState = true;
            (await _manager.GetAmendStateAsync()).Should().BeTrue();
        }

        [Test]
        public async Task SetAmendStateAsync_true_should_write_true_to_file()
        {
            bool correctlyWritten = false;
            _file.When(x => x.WriteAllTextAsync(_amendSaveStatePath, true.ToString(), Encoding.Default, cancellationToken: default)).Do(_ => correctlyWritten = true);
            _directory.Exists(Path.GetDirectoryName(_amendSaveStatePath)).Returns(true);

            AppSettings.RememberAmendCommitState = true;
            await _manager.SetAmendStateAsync(amendState: true);

            Assert.That(correctlyWritten);
        }

        [Test]
        public async Task SetAmendStateAsync_true_should_delete_file_if_not_RememberAmendCommitState()
        {
            bool correctlyDeleted = false;
            _file.Exists(_amendSaveStatePath).Returns(true);
            _file.When(x => x.Delete(_amendSaveStatePath)).Do(_ => correctlyDeleted = true);

            AppSettings.RememberAmendCommitState = false;
            await _manager.SetAmendStateAsync(amendState: true);

            Assert.That(correctlyDeleted);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task SetAmendStateAsync_false_should_delete_file(bool rememberAmendCommitState)
        {
            bool correctlyDeleted = false;
            _file.When(x => x.Delete(_amendSaveStatePath)).Do(_ => correctlyDeleted = true);
            _file.Exists(_amendSaveStatePath).Returns(true);

            AppSettings.RememberAmendCommitState = rememberAmendCommitState;
            await _manager.SetAmendStateAsync(amendState: false);

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
        public async Task WriteCommitMessageToFileAsync_should_write_COMMITMESSAGE()
        {
            CommitMessageManager manager = new(_owner, _referenceRepository.Module.WorkingDir, _referenceRepository.Module.CommitEncoding, overriddenCommitMessage: null);

            File.Exists(manager.CommitMessagePath).Should().BeFalse();

            // null message isn't formatted, since we're only interested in testing File.Write logic
            string message = null;
            await manager.WriteCommitMessageToFileAsync(message, CommitMessageType.Normal, false, false);

            File.Exists(manager.CommitMessagePath).Should().BeTrue();
        }

        [TestCase("utf-8")]
        [TestCase("Utf-8")]
        [TestCase("UTF-8")]
        public async Task WriteCommitMessageToFileAsync_no_bom(string encodingName)
        {
            _referenceRepository.Module.EffectiveConfigFile.SetString("i18n.commitEncoding", encodingName);
            _referenceRepository.Module.CommitEncoding.Preamble.Length.Should().Be(0);
            CommitMessageManager manager = new(_owner, _referenceRepository.Module.WorkingDir, _referenceRepository.Module.CommitEncoding);

            File.Exists(manager.CommitMessagePath).Should().BeFalse();

            string message = "Test message";
            await manager.WriteCommitMessageToFileAsync(message, CommitMessageType.Normal, false, false);

            File.Exists(manager.CommitMessagePath).Should().BeTrue();
            File.ReadAllBytes(manager.CommitMessagePath).Should().BeEquivalentTo(Encoding.ASCII.GetBytes(message + "\r\n"));
        }

        [Test]
        public async Task WriteCommitMessageToFileAsync_should_write_MERGE_MSG()
        {
            CommitMessageManager manager = new(_owner, _referenceRepository.Module.WorkingDir, _referenceRepository.Module.CommitEncoding, overriddenCommitMessage: null);

            File.Exists(manager.MergeMessagePath).Should().BeFalse();

            // null message isn't formatted, since we're only interested in testing File.Write logic
            string message = null;
            await manager.WriteCommitMessageToFileAsync(message, CommitMessageType.Merge, false, false);

            File.Exists(manager.MergeMessagePath).Should().BeTrue();
        }

        [Test]
        public void MergeMessagePath()
        {
            _manager.MergeMessagePath.Should().Be(_mergeMessagePath);
        }

        [Test]
        public async Task GetMergeOrCommitMessageAsync_should_return_merge_message_if_exists()
        {
            _file.Exists(_commitMessagePath).Returns(true);
            _file.Exists(_mergeMessagePath).Returns(true);

            (await _manager.GetMergeOrCommitMessageAsync()).Should().Be(_mergeMessage);

            _manager.IsMergeCommit.Should().BeTrue();
        }

        [Test]
        public async Task GetMergeOrCommitMessageAsync_should_return_overridden_message_if_set()
        {
            SetupExtra(overriddenCommitMessage: _overriddenCommitMessage);
            _file.Exists(_commitMessagePath).Returns(true);
            _file.Exists(_mergeMessagePath).Returns(true);

            (await _manager.GetMergeOrCommitMessageAsync()).Should().Be(_overriddenCommitMessage);

            _manager.IsMergeCommit.Should().BeTrue();
        }

        [Test]
        public async Task GetMergeOrCommitMessageAsync_should_return_commit_message_if_exists_and_no_merge_message()
        {
            _file.Exists(_commitMessagePath).Returns(true);
            _file.Exists(_mergeMessagePath).Returns(false);

            (await _manager.GetMergeOrCommitMessageAsync()).Should().Be(_commitMessage);

            _manager.IsMergeCommit.Should().BeFalse();
        }

        [Test]
        public async Task GetMergeOrCommitMessageAsync_should_return_empty_if_no_file_exists()
        {
            _file.Exists(_commitMessagePath).Returns(false);
            _file.Exists(_mergeMessagePath).Returns(false);

            (await _manager.GetMergeOrCommitMessageAsync()).Should().BeEmpty();

            _manager.IsMergeCommit.Should().BeFalse();
        }

        [Test]
        public async Task GetMergeOrCommitMessageAsync_should_return_overridden_if_exist_and_if_no_file_exists()
        {
            SetupExtra(_overriddenCommitMessage);
            _file.Exists(_commitMessagePath).Returns(false);
            _file.Exists(_mergeMessagePath).Returns(false);

            (await _manager.GetMergeOrCommitMessageAsync()).Should().Be(_overriddenCommitMessage);

            _manager.IsMergeCommit.Should().BeFalse();
        }

        [Test]
        public async Task SetMergeOrCommitMessageAsync_should_write_merge_message_if_exists()
        {
            bool correctlyWritten = false;
            _file.When(x => x.WriteAllTextAsync(_mergeMessagePath, _newMessage, _encoding, cancellationToken: default)).Do(_ => correctlyWritten = true);
            _file.Exists(_commitMessagePath).Returns(true);
            _file.Exists(_mergeMessagePath).Returns(true);
            _directory.Exists(Path.GetDirectoryName(_commitMessagePath)).Returns(true);

            await _manager.SetMergeOrCommitMessageAsync(_newMessage);

            Assert.That(correctlyWritten);
        }

        [Test]
        public async Task SetMergeOrCommitMessageAsync_should_not_write_merge_message_if_exist_if_it_is_the_overridding_commitmessage_exists()
        {
            SetupExtra(_overriddenCommitMessage);
            bool hasBeenWritten = false;
            _file.When(x => x.WriteAllTextAsync(_mergeMessagePath, _newMessage, _encoding, cancellationToken: default)).Do(_ => hasBeenWritten = true);
            _file.Exists(_commitMessagePath).Returns(true);
            _file.Exists(_mergeMessagePath).Returns(true);

            await _manager.SetMergeOrCommitMessageAsync(_overriddenCommitMessage);

            hasBeenWritten.Should().BeFalse();
        }

        [Test]
        public async Task SetMergeOrCommitMessageAsync_should_write_commit_message_if_exists_and_no_merge_message()
        {
            bool correctlyWritten = false;
            _file.When(x => x.WriteAllTextAsync(_commitMessagePath, _newMessage, _encoding, cancellationToken: default)).Do(_ => correctlyWritten = true);
            _file.Exists(_commitMessagePath).Returns(true);
            _file.Exists(_mergeMessagePath).Returns(false);
            _directory.Exists(Path.GetDirectoryName(_commitMessagePath)).Returns(true);

            await _manager.SetMergeOrCommitMessageAsync(_newMessage);

            Assert.That(correctlyWritten);
        }

        [Test]
        public async Task SetMergeOrCommitMessageAsync_should_write_commit_message_if_no_file_exists()
        {
            bool correctlyWritten = false;
            _file.When(x => x.WriteAllTextAsync(_commitMessagePath, _newMessage, _encoding, cancellationToken: default)).Do(_ => correctlyWritten = true);
            _file.Exists(_commitMessagePath).Returns(false);
            _file.Exists(_mergeMessagePath).Returns(false);
            _directory.Exists(Path.GetDirectoryName(_commitMessagePath)).Returns(true);

            await _manager.SetMergeOrCommitMessageAsync(_newMessage);

            Assert.That(correctlyWritten);
        }

        [Test]
        public async Task SetMergeOrCommitMessageAsync_should_write_empty_message_if_null()
        {
            bool correctlyWritten = false;
            _file.When(x => x.WriteAllTextAsync(_commitMessagePath, string.Empty, _encoding, cancellationToken: default)).Do(_ => correctlyWritten = true);
            _file.Exists(_commitMessagePath).Returns(false);
            _file.Exists(_mergeMessagePath).Returns(false);
            _directory.Exists(Path.GetDirectoryName(_commitMessagePath)).Returns(true);

            await _manager.SetMergeOrCommitMessageAsync(message: null);

            Assert.That(correctlyWritten);
        }

        [Test]
        public async Task SetMergeOrCommitMessageAsync_should_not_write_if_dir_no_longer_exists()
        {
            bool correctlyWritten = false;
            _file.When(x => x.WriteAllTextAsync(_commitMessagePath, string.Empty, _encoding, cancellationToken: default)).Do(_ => correctlyWritten = true);
            _file.Exists(_commitMessagePath).Returns(false);
            _file.Exists(_mergeMessagePath).Returns(false);
            _directory.Exists(Path.GetDirectoryName(_commitMessagePath)).Returns(false);

            await _manager.SetMergeOrCommitMessageAsync(message: null);

            Assert.That(!correctlyWritten);
        }

        [Test]
        public async Task ResetCommitMessageAsync()
        {
            bool deletedA = false;
            bool deletedC = false;
            bool deletedM = false;
            _file.When(x => x.Delete(_amendSaveStatePath)).Do(_ => deletedA = true);
            _file.When(x => x.Delete(_commitMessagePath)).Do(_ => deletedC = true);
            _file.When(x => x.Delete(_mergeMessagePath)).Do(_ => deletedM = true);

            await _manager.ResetCommitMessageAsync();

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
