using System;
using System.Collections;
using System.Text;
using FluentAssertions;
using GitExtUtils;
using GitUI.NBugReports;
using NUnit.Framework;

namespace GitUITests.NBugReports
{
    [TestFixture]
    public sealed class BugReportInvokerTests
    {
        [Test, TestCaseSource(typeof(TestExceptions), "TestCases")]
        public void Append(Exception exception, string expectedRootError, string expectedText)
        {
            StringBuilder text = new();
            string rootError = BugReportInvoker.Append(text, exception);
            rootError.Should().Be(expectedRootError);
            text.ToString().Should().Be(expectedText);
        }
    }

    public class TestExceptions
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
                    _messageOuter,
                    "");
                yield return new TestCaseData(new Exception(_messageOuter, new Exception(_messageMiddle)),
                    _messageMiddle,
                    "");
                yield return new TestCaseData(new Exception(_messageOuter, new Exception(_messageMiddle, new Exception(_messageInner))),
                    _messageInner,
                    "");
                yield return new TestCaseData(new UserExternalOperationException(_context,
                    new ExternalOperationException(_command, _arguments, _directory, _exitCode, new Exception(_messageOuter, new Exception(_messageInner)))),
                    _messageInner,
                    $"{_context}{Environment.NewLine}"
                    + $"Exit code: {_exitCode}{Environment.NewLine}{Environment.NewLine}"
                    + $"Command: {_command}{Environment.NewLine}"
                    + $"Arguments: {_arguments}{Environment.NewLine}"
                    + $"Working directory: {_directory}{Environment.NewLine}");
                yield return new TestCaseData(new UserExternalOperationException(context: null,
                    new ExternalOperationException(null, null, null, null, new Exception(_messageInner))),
                    _messageInner,
                    "");
            }
        }
    }
}
