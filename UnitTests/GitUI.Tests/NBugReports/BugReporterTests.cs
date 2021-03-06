using System;
using System.Collections;
using System.Text;
using FluentAssertions;
using GitExtensions;
using GitExtUtils;
using GitUI.NBugReports;
using NUnit.Framework;

namespace GitUITests.NBugReports
{
    [TestFixture]
    public sealed class BugReporterTests
    {
        [Test, TestCaseSource(typeof(TestExceptions), "TestCases")]
        public void Append(Exception exception, string expectedRootError, string expectedText)
        {
            StringBuilder text = new();
            string rootError = BugReporter.Append(text, exception);
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
        private const string _operation = "operation";
        private const string _object = "object";
        private const string _arguments = "arguments";
        private const string _directory = "directory";

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
                    new ExternalOperationException(_operation, _object, _arguments, _directory, new Exception(_messageOuter, new Exception(_messageInner)))),
                    _messageInner,
                    $"Context: {_context}{Environment.NewLine}"
                    + $"Operation: {_operation} {_object}{Environment.NewLine}"
                    + $"Arguments: {_arguments}{Environment.NewLine}"
                    + $"Directory: {_directory}{Environment.NewLine}");
                yield return new TestCaseData(new UserExternalOperationException(null,
                    new ExternalOperationException(null, null, null, null, new Exception(_messageInner))),
                    _messageInner,
                    "");
            }
        }
    }
}
