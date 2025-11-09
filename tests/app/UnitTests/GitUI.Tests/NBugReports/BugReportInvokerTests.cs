using System.ComponentModel;
using System.Security;
using System.Text;
using FluentAssertions;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Settings;
using GitUI.NBugReports;
using NSubstitute;

namespace GitUITests.NBugReports;

[TestFixture]
public sealed class BugReportInvokerTests
{
    private IBugReporter _mockReporter;
    private IBugReporter _originalReporter;

    [SetUp]
    public void Setup()
    {
        _mockReporter = Substitute.For<IBugReporter>();
        _originalReporter = BugReportInvoker.TestAccessor.BugReporterInstance;
        BugReportInvoker.TestAccessor.BugReporterInstance = _mockReporter;
    }

    [TearDown]
    public void TearDown()
    {
        BugReportInvoker.TestAccessor.BugReporterInstance = _originalReporter;
    }

    [Test, TestCaseSource(nameof(ReportFailedToLoadAnAssemblyTestCases))]
    public void Report_should_call_ReportFailedToLoadAnAssembly(Exception exception, bool isTerminating, bool expectedIsRuntimeAssembly)
    {
        BugReportInvoker.Report(exception, isTerminating);

        _mockReporter.Received(1).ReportFailedToLoadAnAssembly(
            exception,
            Arg.Is(isTerminating),
            Arg.Is(expectedIsRuntimeAssembly));
    }

    [Test, TestCaseSource(nameof(IgnoredExceptionsTestCases))]
    public void Report_should_ignore_exception(Exception exception)
    {
        BugReportInvoker.Report(exception, isTerminating: false);

        _mockReporter.DidNotReceiveWithAnyArgs().ReportError(default, default, default, default);
        _mockReporter.DidNotReceiveWithAnyArgs().ReportFailedToLoadAnAssembly(default, default, default);
        _mockReporter.DidNotReceiveWithAnyArgs().ReportDubiousOwnership(default);
    }

    [Test]
    public void Report_should_call_ReportDubiousOwnership_for_ExternalOperationException_with_dubious_ownership_message()
    {
        Exception innerException = new("fatal: detected dubious ownership in repository\r\nconfig --global --add safe.directory /path");
        ExternalOperationException exception = new(
            command: "git",
            arguments: "status",
            workingDirectory: "/path",
            exitCode: 128,
            innerException: innerException);

        BugReportInvoker.Report(exception, isTerminating: false);

        _mockReporter.Received(1).ReportDubiousOwnership(exception);
        _mockReporter.DidNotReceiveWithAnyArgs().ReportError(default, default, default, default);
    }

    [Test, TestCaseSource(nameof(ReportErrorTestCases))]
    public void Report_should_call_ReportError(
        Exception exception,
        string expectedRootError,
        bool expectedIsExternalOperation,
        bool expectedIsUserExternalOperation)
    {
        BugReportInvoker.Report(exception, isTerminating: false);

        _mockReporter.Received(1).ReportError(
            exception,
            expectedRootError,
            Arg.Any<StringBuilder>(),
            Arg.Any<OperationInfo>());
    }

    [Test]
    [Ignore("This will launch an app and wait for user interaction, not suitable for automated tests.")]
    public void Report_should_not_call_reporter_for_terminating_general_exception()
    {
        Exception exception = new("Test error");

        BugReportInvoker.Report(exception, isTerminating: true);

        _mockReporter.DidNotReceiveWithAnyArgs().ReportError(default, default, default, default);
    }

    [Test, TestCaseSource(typeof(TestExceptions), nameof(TestExceptions.GetRootErrorTestCases))]
    public void GetRootError_should_return_innermost_exception_message(Exception exception, string expectedRootError)
    {
        string rootError = BugReportInvoker.TestAccessor.GetRootError(exception);

        rootError.Should().Be(expectedRootError);
    }

    [TestCase("System.Data.Common", true)]
    [TestCase("System.IO", true)]
    [TestCase("Microsoft.CSharp", true)]
    [TestCase("Microsoft.VisualBasic", true)]
    [TestCase("system.data.common", true)]
    [TestCase("MICROSOFT.Extensions", true)]
    [TestCase("CustomAssembly", false)]
    [TestCase("MyApp.Core", false)]
    [TestCase("", false)]
    [TestCase(null, false)]
    public void IsDotNetFrameworkAssembly_should_identify_framework_assemblies(string assemblyName, bool expected)
    {
        bool result = BugReportInvoker.TestAccessor.IsDotNetFrameworkAssembly(assemblyName);

        result.Should().Be(expected);
    }

    [TestCase("vcruntime140_cor3.dll", true)]
    [TestCase("vcruntime140.dll", true)]
    [TestCase("VCRUNTIME140_COR3.DLL", true)]
    [TestCase("some_vcruntime_file.dll", true)]
    [TestCase("CustomDll.dll", false)]
    [TestCase("user32.dll", false)]
    [TestCase("", false)]
    [TestCase(null, false)]
    public void IsVCRuntimeDll_should_identify_vcruntime_dlls(string dllName, bool expected)
    {
        bool result = BugReportInvoker.TestAccessor.IsVCRuntimeDll(dllName);

        result.Should().Be(expected);
    }

    private static IEnumerable<TestCaseData> ReportFailedToLoadAnAssemblyTestCases
    {
        get
        {
            yield return new TestCaseData(
                new FileNotFoundException("Could not load file or assembly 'System.Data.Common'", "System.Data.Common"),
                false,
                true)
                .SetName("FileNotFoundException with System assembly");

            yield return new TestCaseData(
                new FileNotFoundException("Could not load file or assembly 'Microsoft.CSharp'", "Microsoft.CSharp"),
                false,
                true)
                .SetName("FileNotFoundException with Microsoft assembly");

            yield return new TestCaseData(
                new FileNotFoundException("Could not load file or assembly 'vcruntime140.dll'", "vcruntime140.dll"),
                false,
                true)
                .SetName("FileNotFoundException with vcruntime dll");

            yield return new TestCaseData(
                new FileNotFoundException("Could not load file or assembly 'CustomAssembly'", "CustomAssembly"),
                false,
                false)
                .SetName("FileNotFoundException with custom assembly");

            yield return new TestCaseData(
                new FileNotFoundException("Could not load file or assembly 'CustomAssembly'", "CustomAssembly"),
                true,
                false)
                .SetName("FileNotFoundException with isTerminating true");

            yield return new TestCaseData(
                new DllNotFoundException("Unable to load DLL 'vcruntime140_cor3.dll'"),
                false,
                true)
                .SetName("DllNotFoundException with vcruntime");
        }
    }

    private static IEnumerable<TestCaseData> IgnoredExceptionsTestCases
    {
        get
        {
            InvalidOperationException invalidOpException = new("Test exception");
            yield return new TestCaseData(CreateExceptionWithStackTrace(invalidOpException, "at System.Windows.Forms.ListViewGroup.get_AccessibilityObject()"))
                .SetName("InvalidOperationException with ListViewGroup accessibility");

            yield return new TestCaseData(new OperationCanceledException())
                .SetName("OperationCanceledException");

            yield return new TestCaseData(new TaskCanceledException())
                .SetName("TaskCanceledException");
        }
    }

    private static IEnumerable<TestCaseData> ReportErrorTestCases
    {
        get
        {
            Exception innerException = new("Script failed");
            ExternalOperationException externalException = new(
                command: "script.sh",
                arguments: "arg1",
                workingDirectory: "/path",
                exitCode: 1,
                innerException: innerException);

            yield return new TestCaseData(
                new UserExternalOperationException("User script error", externalException),
                "Script failed",
                true,
                true)
                .SetName("UserExternalOperationException");

            yield return new TestCaseData(
                new GitConfigFormatException("Invalid git config format"),
                "Invalid git config format",
                false,
                true)
                .SetName("GitConfigFormatException");

            yield return new TestCaseData(
                new IOException("File access denied"),
                "File access denied",
                true,
                false)
                .SetName("IOException");

            yield return new TestCaseData(
                new SecurityException("Security access denied"),
                "Security access denied",
                true,
                false)
                .SetName("SecurityException");

            yield return new TestCaseData(
                new DirectoryNotFoundException("Directory not found"),
                "Directory not found",
                true,
                false)
                .SetName("DirectoryNotFoundException");

            yield return new TestCaseData(
                new PathTooLongException("Path too long"),
                "Path too long",
                true,
                false)
                .SetName("PathTooLongException");

            yield return new TestCaseData(
                new Win32Exception(5, "Access is denied"),
                "Access is denied",
                true,
                false)
                .SetName("Win32Exception");

            Exception gitInnerException = new("git error");
            yield return new TestCaseData(
                new ExternalOperationException(
                    command: AppSettings.GitCommand,
                    arguments: "status",
                    workingDirectory: "/path",
                    exitCode: 1,
                    innerException: gitInnerException),
                "git error",
                true,
                true)
                .SetName("ExternalOperationException with git command");

            Exception wslInnerException = new("wsl error");
            yield return new TestCaseData(
                new ExternalOperationException(
                    command: AppSettings.WslCommand,
                    arguments: "git status",
                    workingDirectory: "/path",
                    exitCode: 1,
                    innerException: wslInnerException),
                "wsl error",
                true,
                true)
                .SetName("ExternalOperationException with wsl command");

            yield return new TestCaseData(
                new Exception("Error occurred in mingw64/libexec/git-core"),
                "Error occurred in mingw64/libexec/git-core",
                false,
                true)
                .SetName("Exception with msysgit path");

            yield return new TestCaseData(
                new Exception("General error"),
                "General error",
                false,
                false)
                .SetName("General exception");

            yield return new TestCaseData(
                new FileNotFoundException("File 'test.txt' not found", "test.txt"),
                "File 'test.txt' not found",
                true,
                false)
                .SetName("FileNotFoundException without assembly load prefix");
        }
    }

    private static Exception CreateExceptionWithStackTrace(Exception exception, string stackTrace)
    {
        System.Reflection.FieldInfo stackTraceField = typeof(Exception).GetField("_stackTraceString", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        stackTraceField?.SetValue(exception, stackTrace);

        return exception;
    }

    private static class TestExceptions
    {
        private const string _messageOuter = "outer";
        private const string _messageMiddle = "middle";
        private const string _messageInner = "inner";
        private const string _context = "context";
        private const string _command = "command";
        private const string _arguments = "arguments";
        private const string _directory = "directory";
        private const int _exitCode = 128;

        public static IEnumerable<TestCaseData> GetRootErrorTestCases
        {
            get
            {
                yield return new TestCaseData(
                    new Exception(_messageOuter),
                    _messageOuter)
                    .SetName("Single exception");

                yield return new TestCaseData(
                    new Exception(_messageOuter, new Exception(_messageMiddle)),
                    _messageMiddle)
                    .SetName("Two nested exceptions");

                yield return new TestCaseData(
                    new Exception(_messageOuter, new Exception(_messageMiddle, new Exception(_messageInner))),
                    _messageInner)
                    .SetName("Three nested exceptions");

                yield return new TestCaseData(
                    new UserExternalOperationException(_context,
                        new ExternalOperationException(_command, _arguments, _directory, _exitCode,
                            new Exception(_messageOuter, new Exception(_messageInner)))),
                    _messageInner)
                    .SetName("UserExternalOperationException with nested exceptions");

                yield return new TestCaseData(
                    new UserExternalOperationException(context: null,
                        new ExternalOperationException(null, null, null, null, new Exception(_messageInner))),
                    _messageInner)
                    .SetName("UserExternalOperationException with null context");
            }
        }
    }
}
