using System.Text;
using System.Text.RegularExpressions;
using BugReporter.Serialization;

namespace BugReporterTests
{
    [TestFixture]
    [SetCulture("en-US")]
    [SetUICulture("en-US")]
    public sealed class SerializableExceptionTests
    {
        [Test, TestCaseSource(nameof(TestCases))]
        public async Task ToString(string testName, Action action)
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

            await Verifier.Verify(Sanitize(message))
                .UseParameters(testName);
        }

        [Test, TestCaseSource(nameof(TestCases))]
        public async Task ToString_should_be_same_from_round_trip(string testName, Action action)
        {
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

            await Verifier.Verify(Sanitize(exception.ToString()))
                .UseParameters(testName);
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
                m.AppendLine(Regex.Replace(line, @"^(?<keep>.*)(?<codeLocationToBeRemoved>\sin\s.*)$", "${keep}"));
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
