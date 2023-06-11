using BugReporter;
using BugReporter.Serialization;
using FluentAssertions;

namespace BugReporterTests
{
    [TestFixture]
    [SetCulture("en-US")]
    [SetUICulture("en-US")]
    public sealed class ErrorReportUrlBuilderTests
    {
        private ErrorReportUrlBuilder _builder;

        [SetUp]
        public void Setup()
        {
            _builder = new ErrorReportUrlBuilder();
        }

        [Test]
        public void Build_should_throw_ANE_if_exception_null()
        {
            ((Action)(() => _builder.Build(exception: null, exceptionInfo: null, environmentInfo: null, additionalInfo: null)))
                .Should().Throw<ArgumentNullException>();
        }

        private static (SerializableException exception, string? exceptionInfo, string environmentInfo, string additionalInfo) BuildData()
        {
            SerializableException exception = new(new Exception("OPPS!", new ApplicationException("BAM!", new DivideByZeroException("BOOM!"))));
            string environmentInfo = @"Git Extensions 3.4.3.9999
Build d4b0f48
Git 2.27.0.windows.1
Microsoft Windows NT 10.0.19041.0
.NET Framework 4.8.4360.0
DPI 288dpi (300% scaling)".Replace("\r\n", "\n");
            string additionalInfo = "I was just trying out a bunch of stuff, not sure what is the cause. Could just as well be an issue with Git. I cannot provide the specific Repo I used. Hope I'm not wasting your time.";

            return (exception, exceptionInfo: null, environmentInfo, additionalInfo);
        }

        [Test]
        public void Build_should_serialize_exception_into_url()
        {
            (SerializableException exception, string? exceptionInfo, string environmentInfo, string additionalInfo) bd = BuildData();
            string url = _builder.Build(bd.exception, bd.exceptionInfo, bd.environmentInfo, bd.additionalInfo);
            Assert.AreEqual("template=bug_report.yml&labels=type%3A%20NBug&about=Git%20Extensions%203.4.3.9999%0ABuild%20d4b0f48%0AGit%202.27.0.windows.1%0AMicrosoft%20Windows%20NT%2010.0.19041.0%0A.NET%20Framework%204.8.4360.0%0ADPI%20288dpi%20%28300%25%20scaling%29&description=%60%60%60%0ASystem.Exception%3A%20OPPS%21%0D%0A%20---%3E%20System.ApplicationException%3A%20BAM%21%0D%0A%20---%3E%20System.DivideByZeroException%3A%20BOOM%21%0D%0A%20%20%20---%20End%20of%20inner%20exception%20stack%20trace%20---%0D%0A%20%20%20---%20End%20of%20inner%20exception%20stack%20trace%20---%0A%60%60%60%0A%0AI%20was%20just%20trying%20out%20a%20bunch%20of%20stuff%2C%20not%20sure%20what%20is%20the%20cause.%20Could%20just%20as%20well%20be%20an%20issue%20with%20Git.%20I%20cannot%20provide%20the%20specific%20Repo%20I%20used.%20Hope%20I%27m%20not%20wasting%20your%20time.%0A", url);
        }

        [Test]
        public void CopyText_should_format_text()
        {
            (SerializableException exception, string? exceptionInfo, string environmentInfo, string additionalInfo) bd = BuildData();
            string url = _builder.CopyText(bd.exception, bd.exceptionInfo, bd.environmentInfo, bd.additionalInfo);

            Assert.AreEqual("Git Extensions 3.4.3.9999\nBuild d4b0f48\nGit 2.27.0.windows.1\nMicrosoft Windows NT 10.0.19041.0\n.NET Framework 4.8.4360.0\nDPI 288dpi (300% scaling)```\nSystem.Exception: OPPS!\r\n ---> System.ApplicationException: BAM!\r\n ---> System.DivideByZeroException: BOOM!\r\n   --- End of inner exception stack trace ---\r\n   --- End of inner exception stack trace ---\n```\n\nI was just trying out a bunch of stuff, not sure what is the cause. Could just as well be an issue with Git. I cannot provide the specific Repo I used. Hope I'm not wasting your time.\n", url);
        }
    }
}
