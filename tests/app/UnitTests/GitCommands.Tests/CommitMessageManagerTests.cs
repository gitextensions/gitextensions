using System.Collections;
using System.IO.Abstractions;
using System.Text;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using static System.Net.Mime.MediaTypeNames;

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

            PathBase path = Substitute.For<PathBase>();
            path.Combine(Arg.Any<string>(), Arg.Any<string>()).Returns(x => Path.Combine((string)x[0], (string)x[1]));

            _fileSystem = Substitute.For<IFileSystem>();
            _fileSystem.File.Returns(_file);
            _fileSystem.Directory.Returns(_directory);
            _fileSystem.Path.Returns(path);

            _manager = new CommitMessageManager(_workingDirGitDir, _encoding, _fileSystem, overriddenCommitMessage: null);
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
            _manager = new CommitMessageManager(_workingDirGitDir, _encoding, _fileSystem, overriddenCommitMessage: overriddenCommitMessage);
        }

        [TestCase(null)]
        public void Constructor_should_throw(string workingDirGitDir)
        {
            // Arrange
           Encoding encoding = _encoding;

            // Act
            Action act = () => new CommitMessageManager(workingDirGitDir, encoding);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("::")]
        public void Constructor_should_not_throw(string workingDirGitDir)
        {
            CommitMessageManager commitMessageManager = new(workingDirGitDir, _encoding);
            commitMessageManager.Should().NotBeNull();
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

            ClassicAssert.That(correctlyWritten);
        }

        [Test]
        public async Task SetAmendStateAsync_true_should_delete_file_if_not_RememberAmendCommitState()
        {
            bool correctlyDeleted = false;
            _file.Exists(_amendSaveStatePath).Returns(true);
            _file.When(x => x.Delete(_amendSaveStatePath)).Do(_ => correctlyDeleted = true);

            AppSettings.RememberAmendCommitState = false;
            await _manager.SetAmendStateAsync(amendState: true);

            ClassicAssert.That(correctlyDeleted);
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

            ClassicAssert.That(correctlyDeleted);
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
            CommitMessageManager manager = new(_referenceRepository.Module.WorkingDir, _referenceRepository.Module.CommitEncoding, overriddenCommitMessage: null);

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
            GitModule module = _referenceRepository.Module;
            module.SetSetting("i18n.commitencoding", encodingName);
            module.CommitEncoding.Preamble.Length.Should().Be(0);
            CommitMessageManager manager = new(_referenceRepository.Module.WorkingDir, _referenceRepository.Module.CommitEncoding);

            File.Exists(manager.CommitMessagePath).Should().BeFalse();

            string message = "Test message";
            await manager.WriteCommitMessageToFileAsync(message, CommitMessageType.Normal, false, false);

            File.Exists(manager.CommitMessagePath).Should().BeTrue();
            File.ReadAllBytes(manager.CommitMessagePath).Should().BeEquivalentTo(Encoding.ASCII.GetBytes(message + "\r\n"));
        }

        [Test]
        public async Task WriteCommitMessageToFileAsync_should_write_MERGE_MSG()
        {
            CommitMessageManager manager = new(_referenceRepository.Module.WorkingDir, _referenceRepository.Module.CommitEncoding, overriddenCommitMessage: null);

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

            var commitMessage = (await _manager.GetMergeOrCommitMessageAsync());


            commitMessage.Should().BeEmpty();
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

            ClassicAssert.That(correctlyWritten);
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

            ClassicAssert.That(correctlyWritten);
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

            ClassicAssert.That(correctlyWritten);
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

            ClassicAssert.That(correctlyWritten);
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

            ClassicAssert.That(!correctlyWritten);
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

            ClassicAssert.That(deletedA);
            ClassicAssert.That(deletedC);
            ClassicAssert.That(!deletedM);
        }

        // Exception / message box tests

        [Test]
        public async Task GetMergeOrCommitMessageAsync_should_throw_CannotAccessFileException_on_io_error()
        {
            // Arrange
            FileBase file = Substitute.For<FileBase>();
            file.Exists(_commitMessagePath).Returns(true);
            file.Exists(_mergeMessagePath).Returns(false);
            file.ReadAllTextAsync(_commitMessagePath, _encoding, cancellationToken: default)
                .Throws(new IOException("read boom"));

            DirectoryBase directory = Substitute.For<DirectoryBase>();
            PathBase path = Substitute.For<PathBase>();
            path.Combine(Arg.Any<string>(), Arg.Any<string>()).Returns(x => Path.Combine((string)x[0], (string)x[1]));

            IFileSystem fileSystem = Substitute.For<IFileSystem>();
            fileSystem.File.Returns(file);
            fileSystem.Directory.Returns(directory);
            fileSystem.Path.Returns(path);

            CommitMessageManager manager = new(_workingDirGitDir, _encoding, fileSystem, overriddenCommitMessage: null);

            // Act
            Func<Task> act = async () => await manager.GetMergeOrCommitMessageAsync();

            // Assert
            await act.Should().ThrowAsync<CannotAccessFileException>()
                .WithMessage($"*read boom*{_commitMessagePath}*");
        }

        [Test]
        public async Task WriteCommitMessageToFileAsync_should_throw_CannotAccessFileException_on_io_error()
        {
            // Arrange
            FileBase file = Substitute.For<FileBase>();
            DirectoryBase directory = Substitute.For<DirectoryBase>();
            directory.Exists(Arg.Any<string>()).Returns(true);

            file.WriteAllTextAsync(_commitMessagePath, Arg.Any<string>(), _encoding, cancellationToken: default)
                .Throws(new IOException("write boom"));

            PathBase path = Substitute.For<PathBase>();
            path.Combine(Arg.Any<string>(), Arg.Any<string>())
                .Returns(x => Path.Combine((string)x[0], (string)x[1]));

            IFileSystem fileSystem = Substitute.For<IFileSystem>();
            fileSystem.File.Returns(file);
            fileSystem.Directory.Returns(directory);
            fileSystem.Path.Returns(path);

            CommitMessageManager manager = new(_workingDirGitDir, _encoding, fileSystem, overriddenCommitMessage: null);

            // Act
            Func<Task> act = async () => await manager.WriteCommitMessageToFileAsync("msg", CommitMessageType.Normal, false, false);

            // Assert
            await act.Should().ThrowAsync<CannotAccessFileException>()
                .WithMessage($"*write boom*{_commitMessagePath}*");
        }

        [Test]
        public async Task GetAmendStateAsync_should_throw_CannotAccessFileException_on_io_error()
        {
            // Arrange
            FileBase file = Substitute.For<FileBase>();
            file.Exists(_amendSaveStatePath).Returns(true);
            file.ReadAllTextAsync(_amendSaveStatePath, Encoding.Default, cancellationToken: default)
                .Throws(new IOException("amend read boom"));

            DirectoryBase directory = Substitute.For<DirectoryBase>();
            PathBase path = Substitute.For<PathBase>();
            path.Combine(Arg.Any<string>(), Arg.Any<string>())
                .Returns(x => Path.Combine((string)x[0], (string)x[1]));

            IFileSystem fileSystem = Substitute.For<IFileSystem>();
            fileSystem.File.Returns(file);
            fileSystem.Directory.Returns(directory);
            fileSystem.Path.Returns(path);

            CommitMessageManager manager = new(_workingDirGitDir, _encoding, fileSystem, overriddenCommitMessage: null);
            AppSettings.RememberAmendCommitState = true;

            // Act
            Func<Task> act = async () => await manager.GetAmendStateAsync();

            // Assert
            await act.Should().ThrowAsync<CannotAccessFileException>()
                .WithMessage($"*amend read boom*{_amendSaveStatePath}*");
        }

        [Test]
        public async Task SetAmendStateAsync_should_throw_CannotAccessFileException_on_io_error()
        {
            // Arrange
            FileBase file = Substitute.For<FileBase>();
            DirectoryBase directory = Substitute.For<DirectoryBase>();
            directory.Exists(Path.GetDirectoryName(_amendSaveStatePath)).Returns(true);

            file.WriteAllTextAsync(_amendSaveStatePath, true.ToString(), Encoding.Default, cancellationToken: default)
                .Throws(new IOException("amend write boom"));

            PathBase path = Substitute.For<PathBase>();
            path.Combine(Arg.Any<string>(), Arg.Any<string>())
                .Returns(x => Path.Combine((string)x[0], (string)x[1]));

            IFileSystem fileSystem = Substitute.For<IFileSystem>();
            fileSystem.File.Returns(file);
            fileSystem.Directory.Returns(directory);
            fileSystem.Path.Returns(path);

            CommitMessageManager manager = new(_workingDirGitDir, _encoding, fileSystem, overriddenCommitMessage: null);
            AppSettings.RememberAmendCommitState = true;

            // Act
            Func<Task> act = async () => await manager.SetAmendStateAsync(true);

            // Assert
            await act.Should().ThrowAsync<CannotAccessFileException>()
                .WithMessage($"*amend write boom*{_amendSaveStatePath}*");
        }

        [Test, TestCaseSource(typeof(FormatCommitMessageTestData), nameof(FormatCommitMessageTestData.FormatCommitMessageTestCases))]
        public void FormatCommitMessage_should_return_expected(
            string commitMessageText,
            bool usingCommitTemplate,
            bool ensureCommitMessageSecondLineEmpty,
            string expectedMessage,
            string commentString)
        {
            CommitMessageManager cut = new(string.Empty, null, commentString: commentString);
            string commitMessage = cut.FormatCommitMessage(commitMessageText, usingCommitTemplate, ensureCommitMessageSecondLineEmpty);

            commitMessage.Should().Be(expectedMessage);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("#")]
        [TestCase(";")]
        [TestCase("//")]
        public void FormatCommitMessage_should_remove_comment_line_when_using_template(string? commentString)
        {
            // Arrange
            CommitMessageManager cut = new(string.Empty, Encoding.UTF8, commentString: commentString);
            string input = commentString is not null && commentString.Length > 0
                ? $"{commentString}comment\nactual message"
                : "actual message";
            string expected = "actual message" + Environment.NewLine;

            // Act
            string result = cut.FormatCommitMessage(input, usingCommitTemplate: true, ensureCommitMessageSecondLineEmpty: false);

            // Assert
            result.Should().Be(expected);
        }

        private static string ToUnixStyleLineEnding(string? s)
        {
            return s is null ? string.Empty : s.Replace("\r\n", "\n");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("#")]
        [TestCase(";")]
        [TestCase("//")]
        public void FormatCommitMessage_should_keep_comment_line_when_not_using_template(string? commentString)
        {
            CommitMessageManager cut = new(string.Empty, Encoding.UTF8, commentString: commentString);
            string input = commentString is not null && commentString.Length > 0
                ? $"{commentString}comment\nactual message"
                : "actual message";
            string expected = input + "\n";

            string result = cut.FormatCommitMessage(input, usingCommitTemplate: false, ensureCommitMessageSecondLineEmpty: false);

            ToUnixStyleLineEnding(result).Should().Be(expected);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("#")]
        [TestCase(";")]
        [TestCase("//")]
        public void FormatCommitMessage_should_return_empty_for_empty_input(string? commentString)
        {
            // Arrange
            CommitMessageManager cut = new(string.Empty, Encoding.UTF8, commentString: commentString);

            // Act
            string result = cut.FormatCommitMessage(string.Empty, usingCommitTemplate: true, ensureCommitMessageSecondLineEmpty: false);

            // Assert
            result.Should().Be(string.Empty);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("#")]
        [TestCase(";")]
        [TestCase("//")]
        public void FormatCommitMessage_should_return_empty_for_null_input(string? commentString)
        {
            // Arrange
            CommitMessageManager cut = new(string.Empty, Encoding.UTF8, commentString: commentString);

            // Act
            string result = cut.FormatCommitMessage(null, usingCommitTemplate: true, ensureCommitMessageSecondLineEmpty: false);

            // Assert
            result.Should().Be(string.Empty);
        }
        public class FormatCommitMessageTestData
        {
            private static readonly string NL = Environment.NewLine;
            private static readonly string[] CommentStrings = new[] { "#", ";", "//" };

            public static IEnumerable FormatCommitMessageTestCases
            {
                get
                {
                    foreach (string commentString in CommentStrings)
                    {
                        yield return new TestCaseData(new object[] { null, false, false, "", commentString });
                        yield return new TestCaseData(new object[] { null, true, false, "", commentString });
                        yield return new TestCaseData(new object[] { null, false, true, "", commentString });
                        yield return new TestCaseData(new object[] { null, true, true, "", commentString });
                        yield return new TestCaseData(new object[] { "", false, false, "", commentString });
                        yield return new TestCaseData(new object[] { "", true, false, "", commentString });
                        yield return new TestCaseData(new object[] { "", false, true, "", commentString });
                        yield return new TestCaseData(new object[] { "", true, true, "", commentString });
                        yield return new TestCaseData(new object[] { "\n", false, false, NL + NL, commentString });
                        yield return new TestCaseData(new object[] { "\n", true, false, NL + NL, commentString });
                        yield return new TestCaseData(new object[] { "\n", false, true, NL + NL, commentString });
                        yield return new TestCaseData(new object[] { "\n", true, true, NL + NL, commentString });
                        yield return new TestCaseData(new object[] { "1", true, false, "1" + NL, commentString });
                        yield return new TestCaseData(new object[] { commentString + "1", false, false, commentString + "1" + NL, commentString });
                        yield return new TestCaseData(new object[] { commentString + "1", true, false, "", commentString });
                        yield return new TestCaseData(new object[] { "1\n\n3", false, false, "1" + NL + NL + "3" + NL, commentString });
                        yield return new TestCaseData(new object[] { "1\n\n3", false, true, "1" + NL + NL + "3" + NL, commentString });
                        yield return new TestCaseData(new object[] { "1\n2\n3", false, false, "1" + NL + "2" + NL + "3" + NL, commentString });
                        yield return new TestCaseData(new object[] { "1\n2\n3", false, true, "1" + NL + NL + "2" + NL + "3" + NL, commentString });
                        yield return new TestCaseData(new object[] { commentString + "0\n1\n\n3", true, false, "1" + NL + NL + "3" + NL, commentString });
                        yield return new TestCaseData(new object[] { commentString + "0\n1\n\n3", true, true, "1" + NL + NL + "3" + NL, commentString });
                        yield return new TestCaseData(new object[] { commentString + "0\n1\n2\n3", true, false, "1" + NL + "2" + NL + "3" + NL, commentString });
                        yield return new TestCaseData(new object[] { commentString + "0\n1\n2\n3", true, true, "1" + NL + NL + "2" + NL + "3" + NL, commentString });
                        yield return new TestCaseData(new object[] { commentString + "0\n1\n" + commentString + "0\n2\n3", true, true, "1" + NL + NL + "2" + NL + "3" + NL, commentString });
                        yield return new TestCaseData(new object[] { "1\n2\n3\n4\n5\n\n7\n\n\n10", true, true, "1" + NL + NL + "2" + NL + "3" + NL + "4" + NL + "5" + NL + NL + "7" + NL + NL + NL + "10" + NL, commentString });
                        yield return new TestCaseData(new object[] { "1\n2\n3\n4\n5\n\n7\n\n\n10", false, true, "1" + NL + NL + "2" + NL + "3" + NL + "4" + NL + "5" + NL + NL + "7" + NL + NL + NL + "10" + NL, commentString });
                        yield return new TestCaseData(new object[] { "1\n2\n3\n4\n5\n\n7\n\n\n10\n", false, true, "1" + NL + NL + "2" + NL + "3" + NL + "4" + NL + "5" + NL + NL + "7" + NL + NL + NL + "10" + NL + NL, commentString });
                    }
                }
            }
        }
    }
}
