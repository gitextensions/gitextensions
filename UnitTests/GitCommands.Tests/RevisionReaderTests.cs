using System.Runtime.CompilerServices;
using System.Text;
using FluentAssertions;
using GitCommands;
using GitExtUtils;
using GitUIPluginInterfaces;
using Newtonsoft.Json;

namespace GitCommandsTests
{
    [TestFixture]
    public sealed class RevisionReaderTests
    {
        private Encoding _logOutputEncoding = Encoding.UTF8;
        private long _sixMonths = new DateTimeOffset(new DateTime(2021, 01, 01)).ToUnixTimeSeconds();

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void BuildArguments_should_be_NUL_terminated()
        {
            RevisionReader reader = RevisionReader.TestAccessor.RevisionReader(new GitModule(""), hasReflogSelector: false, _logOutputEncoding, _sixMonths);
            ArgumentBuilder args = reader.GetTestAccessor().BuildArguments("", "");

            args.ToString().Should().Contain(" log -z ");
        }

        [Test]
        public void TryParseRevisionshould_return_false_if_argument_is_invalid()
        {
            ArraySegment<byte> chunk = null;
            RevisionReader reader = RevisionReader.TestAccessor.RevisionReader(new(""), hasReflogSelector: false, _logOutputEncoding, _sixMonths);

            // Set to a high value so Debug.Assert do not raise exceptions
            reader.GetTestAccessor().NoOfParseError = 100;
            bool res = reader.GetTestAccessor().TryParseRevision(chunk, out _);
            res.Should().BeFalse();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]

        // Avoid launching the difftool at differences
        // APPVEYOR should be detected automatically, this forces the setting (also in local tests)
        // The popup will hang the tests without failure information
        [Test]
        [TestCase("bad_parentid", false)]
        [TestCase("bad_parentid_length", false)]
        [TestCase("bad_sha", false)]
        [TestCase("empty", false)]
        [TestCase("illegal_timestamp", true, false, true)]
        [TestCase("multi_pathfilter", true)]
        [TestCase("no_subject", true)]
        [TestCase("normal", true)]
        [TestCase("short_sha", false)]
        [TestCase("simple_pathfilter", true)]
        [TestCase("subject_no_body", true)]
        [TestCase("empty_commit", true)]
        [TestCase("reflogselector", true, true)]
        public async Task TryParseRevision_test(string testName, bool expectedReturn, bool hasReflogSelector = false, bool serialThrows = false)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData/RevisionReader", testName + ".bin");
            ArraySegment<byte> chunk = File.ReadAllBytes(path);
            RevisionReader reader = RevisionReader.TestAccessor.RevisionReader(new GitModule(""), hasReflogSelector, _logOutputEncoding, _sixMonths);

            // Set to a high value so Debug.Assert do not raise exceptions
            reader.GetTestAccessor().NoOfParseError = 100;
            reader.GetTestAccessor().TryParseRevision(chunk, out GitRevision rev)
                .Should().Be(expectedReturn);
            if (hasReflogSelector)
            {
                rev.ReflogSelector.Should().NotBeNull();
            }

            // No LocalTime for the time stamps
            JsonSerializerSettings timeZoneSettings = new()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };

            if (serialThrows)
            {
                Action act = () => JsonConvert.SerializeObject(rev);
                act.Should().Throw<JsonSerializationException>();
            }
            else if (expectedReturn)
            {
                await Verifier.VerifyJson(JsonConvert.SerializeObject(rev, timeZoneSettings))
                    .UseParameters(testName);
            }
        }
    }
}
