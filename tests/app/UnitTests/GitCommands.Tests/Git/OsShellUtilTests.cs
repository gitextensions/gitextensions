using System.Text;
using FluentAssertions;
using GitCommands;
using GitExtensions.Extensibility;
using NSubstitute;

namespace GitCommandsTests.Git;

[TestFixture]
public sealed class OsShellUtilTests
{
    private IExecutable _mockExecutable;
    private IProcess _mockProcess;

    [SetUp]
    public void SetUp()
    {
        _mockExecutable = Substitute.For<IExecutable>();
        _mockProcess = Substitute.For<IProcess>();
        _mockExecutable.Start(
            Arg.Any<ArgumentString>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<Encoding?>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(_mockProcess);

        OsShellUtil.GetTestAccessor().MockExecutable = _mockExecutable;
    }

    [TearDown]
    public void TearDown()
    {
        OsShellUtil.GetTestAccessor().MockExecutable = null;
    }

    [Test]
    public void Open_should_start_executable_with_shell_execute()
    {
        const string filePath = @"C:\temp\file.txt";

        OsShellUtil.Open(filePath);

        _mockExecutable.Command = filePath;
        _mockExecutable.Received(1).Start(
            Arg.Is<ArgumentString>(a => a.Arguments == null || a.Arguments == ""),
            createWindow: false,
            redirectInput: false,
            redirectOutput: false,
            outputEncoding: null,
            useShellExecute: true,
            throwOnErrorExit: false,
            Arg.Any<CancellationToken>());
    }

    [Test]
    public void Open_should_fall_back_to_OpenAs_when_Start_throws()
    {
        const string filePath = @"C:\temp\file.txt";

        bool firstCall = true;
        _mockExecutable.Start(
            Arg.Any<ArgumentString>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<Encoding?>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                if (firstCall)
                {
                    firstCall = false;
                    throw new InvalidOperationException("cannot open");
                }

                return _mockProcess;
            });

        OsShellUtil.Open(filePath);

        _mockExecutable.Received(2).Start(
            Arg.Any<ArgumentString>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<Encoding?>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
        _mockExecutable.Command = "rundll32.exe";
    }

    [Test]
    public void OpenAs_should_start_rundll32_with_shell32_open_as()
    {
        const string filePath = @"C:\temp\file.txt";

        OsShellUtil.OpenAs(filePath);

        _mockExecutable.Command = "rundll32.exe";
        _mockExecutable.Received(1).Start(
            Arg.Is<ArgumentString>(a => a.Arguments != null && a.Arguments.Contains("shell32.dll,OpenAs_RunDLL") && a.Arguments.Contains(filePath)),
            createWindow: false,
            redirectInput: false,
            redirectOutput: true,
            outputEncoding: Encoding.UTF8,
            useShellExecute: false,
            throwOnErrorExit: true,
            Arg.Any<CancellationToken>());
    }

    [Test]
    public void OpenWithFileExplorer_should_start_explorer_with_quoted_arguments_by_default()
    {
        const string arguments = @"C:\temp";

        OsShellUtil.OpenWithFileExplorer(arguments);

        _mockExecutable.Command = "explorer.exe";
        _mockExecutable.Received(1).Start(
            Arg.Any<ArgumentString>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<Encoding?>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public void OpenWithFileExplorer_should_start_explorer_without_quoting_when_specified()
    {
        const string arguments = @"/select, ""C:\temp\file.txt""";

        OsShellUtil.OpenWithFileExplorer(arguments, quote: false);

        _mockExecutable.Command = "explorer.exe";
        _mockExecutable.Received(1).Start(
            Arg.Is<ArgumentString>(a => a.Arguments != null && a.Arguments == arguments),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<Encoding?>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public void SelectPathInFileExplorer_should_start_explorer_with_select_argument()
    {
        const string filePath = @"C:\temp\file.txt";

        OsShellUtil.SelectPathInFileExplorer(filePath);

        _mockExecutable.Command = "explorer.exe";
        _mockExecutable.Received(1).Start(
            Arg.Is<ArgumentString>(a => a.Arguments != null && a.Arguments.Contains("/select,") && a.Arguments.Contains(filePath)),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<Encoding?>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public void OpenUrlInDefaultBrowser_should_start_executable_with_url()
    {
        const string url = "https://github.com/gitextensions/gitextensions";

        OsShellUtil.OpenUrlInDefaultBrowser(url);

        _mockExecutable.Command = url;
        _mockExecutable.Received(1).Start(
            Arg.Is<ArgumentString>(a => a.Arguments == null || a.Arguments == ""),
            createWindow: false,
            redirectInput: false,
            redirectOutput: false,
            outputEncoding: null,
            useShellExecute: true,
            throwOnErrorExit: false,
            Arg.Any<CancellationToken>());
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void OpenUrlInDefaultBrowser_should_not_start_for_null_or_whitespace_url(string? url)
    {
        OsShellUtil.OpenUrlInDefaultBrowser(url);

        _mockExecutable.DidNotReceive().Start(
            Arg.Any<ArgumentString>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<Encoding?>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }
}
