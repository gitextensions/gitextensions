using System.Text;
using FluentAssertions;
using GitCommands;
using GitExtensions.Extensibility;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace GitCommandsTests.Git;

[TestFixture]
public class OsShellUtilTests
{
    private IExecutable _executable;
    private IProcess _process;

    [SetUp]
    public void SetUp()
    {
        _process = Substitute.For<IProcess>();
        _executable = Substitute.For<IExecutable>();
        _executable.Start(
            Arg.Any<ArgumentString>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<Encoding?>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(_process);

        OsShellUtil.TestAccessor.MockExecutable = _executable;
    }

    [TearDown]
    public void TearDown()
    {
        OsShellUtil.TestAccessor.MockExecutable = null;
    }

    [Test]
    public void Open_should_start_with_shell_execute()
    {
        OsShellUtil.Open("test.txt");

        _executable.Received(1).Start(
            Arg.Is<ArgumentString>(a => a.Length == 0),
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
        int callCount = 0;
        _executable.Start(
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
                callCount++;
                if (callCount == 1)
                {
                    throw new InvalidOperationException("test");
                }

                return _process;
            });

        OsShellUtil.Open("test.txt");

        _executable.Received(2).Start(
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
    public void OpenAs_should_start_rundll32_with_correct_arguments()
    {
        OsShellUtil.OpenAs("test.txt");

        _executable.Received(1).Start(
            Arg.Is<ArgumentString>(a => ((string)a) == "shell32.dll,OpenAs_RunDLL test.txt"),
            createWindow: false,
            redirectInput: false,
            redirectOutput: true,
            Arg.Is<Encoding>(e => e == Encoding.UTF8),
            useShellExecute: false,
            throwOnErrorExit: true,
            Arg.Any<CancellationToken>());
    }

    [Test]
    public void SelectPathInFileExplorer_should_pass_quoted_path_with_select()
    {
        OsShellUtil.SelectPathInFileExplorer(@"C:\some\file.txt");

        _executable.Received(1).Start(
            Arg.Is<ArgumentString>(a => ((string)a) == @"/select, ""C:\some\file.txt"""),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<Encoding?>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public void OpenWithFileExplorer_should_quote_arguments_by_default()
    {
        OsShellUtil.OpenWithFileExplorer(@"C:\some\folder");

        _executable.Received(1).Start(
            Arg.Is<ArgumentString>(a => ((string)a) == @"""C:\some\folder"""),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<Encoding?>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public void OpenWithFileExplorer_should_not_quote_when_quote_is_false()
    {
        OsShellUtil.OpenWithFileExplorer(@"/select, ""C:\some\file.txt""", quote: false);

        _executable.Received(1).Start(
            Arg.Is<ArgumentString>(a => ((string)a) == @"/select, ""C:\some\file.txt"""),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<Encoding?>(),
            Arg.Any<bool>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public void OpenUrlInDefaultBrowser_should_start_with_shell_execute()
    {
        OsShellUtil.OpenUrlInDefaultBrowser("https://example.com");

        _executable.Received(1).Start(
            Arg.Is<ArgumentString>(a => a.Length == 0),
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
    public void OpenUrlInDefaultBrowser_should_not_start_when_url_is_null_or_whitespace(string? url)
    {
        OsShellUtil.OpenUrlInDefaultBrowser(url);

        _executable.DidNotReceive().Start(
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
