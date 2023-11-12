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
        private readonly Encoding _logOutputEncoding = Encoding.UTF8;
        private readonly long _sixMonths = new DateTimeOffset(new DateTime(2021, 01, 01)).ToUnixTimeSeconds();

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void BuildArguments_should_be_NUL_terminated()
        {
            RevisionReader reader = RevisionReader.TestAccessor.RevisionReader(new GitModule(""), _logOutputEncoding, _sixMonths);
            ArgumentBuilder args = reader.GetTestAccessor().BuildArguments("", "");

            args.ToString().Should().Contain(" log -z ");
        }

        [Test]
        public void TryParseRevisionshould_return_false_if_argument_is_invalid()
        {
            ArraySegment<byte> chunk = null;
            RevisionReader reader = RevisionReader.TestAccessor.RevisionReader(new(""), _logOutputEncoding, _sixMonths);

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
        [TestCase("illegal_timestamp", false)]
        [TestCase("multi_pathfilter", true)]
        [TestCase("no_subject", true)]
        [TestCase("normal", true)]
        [TestCase("short_sha", false)]
        [TestCase("simple_pathfilter", true)]
        [TestCase("subject_no_body", true)]
        [TestCase("empty_commit", true)]
        [TestCase("vertical_tab", true)]
        [TestCase("reflogselector", true, true, false)]
        [TestCase("reflogselector_empty", true, true)]
        [TestCase("notes_data", true, false, true, true)]
        [TestCase("notes_empty", true, false, true, true)]
        [TestCase("subject_starts_with_newline", true)]
        public async Task TryParseRevision_test(string testName, bool expectedReturn, bool hasReflogSelector = false, bool hasEmptyReflogSelector = true, bool hasNotes = false)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData/RevisionReader", testName + ".bin");
            ArraySegment<byte> chunk = File.ReadAllBytes(path);
            RevisionReader reader = RevisionReader.TestAccessor.RevisionReader(new GitModule(""), _logOutputEncoding, _sixMonths);
            reader.GetTestAccessor().SetParserAttributes(hasReflogSelector, hasNotes);

            // Set to a high value so Debug.Assert do not raise exceptions
            reader.GetTestAccessor().NoOfParseError = 100;
            reader.GetTestAccessor().TryParseRevision(chunk, out GitRevision rev)
                .Should().Be(expectedReturn);

            if (!expectedReturn)
            {
                return;
            }

            // No LocalTime for the time stamps
            JsonSerializerSettings timeZoneSettings = new()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };

            if (hasEmptyReflogSelector)
            {
                rev.ReflogSelector.Should().BeNull();
            }
            else
            {
                rev.ReflogSelector.Should().NotBeNull();
            }

            await Verifier.VerifyJson(JsonConvert.SerializeObject(rev, timeZoneSettings))
                .UseParameters(testName);
        }
    }
}
