using System.Collections;
using System.Text;
using FluentAssertions;
using GitExtensions.Extensibility;
using GitUI.NBugReports;

namespace GitUITests.NBugReports;

[TestFixture]
public sealed class ExceptionExtensionsTests
{
    [Test, TestCaseSource(typeof(TestExceptions), nameof(TestExceptions.TestCases))]
    public void GetExceptionInfo_should_return_expected(Exception exception, string expectedText)
    {
        StringBuilder text = ExceptionExtensions.GetExceptionInfo(exception);
        text.ToString().Should().Be(expectedText);
    }

    private class TestExceptions
    {
        private const string _messageOuter = "outer";
        private const string _messageMiddle = "middle";
        private const string _messageInner = "inner";
        private const string _context = "context";
        private const string _command = "command";
        private const string _arguments = "arguments";
        private const string _directory = "directory";
        private const int _exitCode = 128;

        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(new Exception(_messageOuter),
                    "");

                yield return new TestCaseData(new Exception(_messageOuter, new Exception(_messageMiddle)),
                    "");

                yield return new TestCaseData(new Exception(_messageOuter, new Exception(_messageMiddle, new Exception(_messageInner))),
                    "");

                yield return new TestCaseData(new UserExternalOperationException(context: null, new ExternalOperationException(null, null, null, null, new Exception(_messageInner))),
                    "");

                yield return new TestCaseData(new UserExternalOperationException(_context,
                    new ExternalOperationException(_command, _arguments, _directory, _exitCode, new Exception(_messageOuter, new Exception(_messageInner)))),
                    $"{_context}{Environment.NewLine}"
                    + $"Exit code: {_exitCode}{Environment.NewLine}"
                    + $"Command: {_command}{Environment.NewLine}"
                    + $"Arguments: {_arguments}{Environment.NewLine}"
                    + $"Working directory: {_directory}{Environment.NewLine}");
            }
        }
    }
}
