using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ApprovalTests;
using ApprovalTests.Namers;
using BugReporter.Serialization;
using NUnit.Framework;

namespace BugReporterTests
{
    [TestFixture]
    public sealed class SerializableExceptionTests
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        [Test, TestCaseSource(nameof(TestCases))]
        public void ToString(string testName, Action action)
        {
            using (ApprovalResults.ForScenario(testName))
            {
                string message = string.Empty;
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    message = new SerializableException(ex).ToString();
                }

                Approvals.Verify(Sanitize(message));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Test, TestCaseSource(nameof(TestCases))]
        public void ToString_should_be_same_from_round_trip(string testName, Action action)
        {
            using (ApprovalResults.ForScenario(testName))
            {
                string message = string.Empty;
                string xml = string.Empty;
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    xml = new SerializableException(ex).ToXmlString();
                }

                SerializableException exception = SerializableException.FromXmlString(xml);

                Approvals.Verify(Sanitize(exception.ToString()));
            }
        }

        public static IEnumerable<TestCaseData> TestCases
        {
            get
            {
                yield return new TestCaseData(nameof(Code.SimpleException), (Action)(() => Code.SimpleException()));
                yield return new TestCaseData(nameof(Code.NestedException), (Action)(() => Code.DoubleNestedException()));
            }
        }

        private static string Sanitize(string exceptionMessage)
        {
            // message contains physical file paths, which are machine specific
            StringBuilder m = new();
            foreach (string line in exceptionMessage.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                Match match = Regex.Match(line, @".*(?<path>\sin\s.*)");
                if (match.Success)
                {
                    m.AppendLine(line.Replace(match.Groups["path"].Value, string.Empty));
                }
                else
                {
                    m.AppendLine(line);
                }
            }

            return m.ToString();
        }

        private static class Code
        {
            public static void SimpleException()
            {
                throw new DivideByZeroException("BOOM!");
            }

            public static void NestedException()
            {
                try
                {
                    SimpleException();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("BAM!", ex);
                }
            }

            public static void DoubleNestedException()
            {
                try
                {
                    NestedException();
                }
                catch (Exception ex)
                {
                    throw new Exception("OPPS!", ex);
                }
            }
        }
    }
}
