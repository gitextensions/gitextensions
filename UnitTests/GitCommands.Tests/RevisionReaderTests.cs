using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using ApprovalTests.Reporters.ContinuousIntegration;
using FluentAssertions;
using GitCommands;
using GitUIPluginInterfaces;
using Newtonsoft.Json;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public sealed class RevisionReaderTests
    {
        private RevisionReader _revisionReader;

        private Func<string?, Encoding?> _getEncodingByGitName;
        private Encoding _logOutputEncoding = Encoding.UTF8;
        private long _sixMonths = new DateTimeOffset(new DateTime(2021, 01, 01)).ToUnixTimeSeconds();

        [SetUp]
        public void Setup()
        {
            _revisionReader = new RevisionReader();

            // The normal encoding is _logOutputEncoding ("i18n.logoutputencoding") since Git 1.8.4
            _getEncodingByGitName = (encoding) => _logOutputEncoding;
        }

        [TestCase(0, false)]
        [TestCase(1, true)]
        public void BuildArguments_should_add_maxcount_if_requested(int maxCount, bool expected)
        {
            var args = _revisionReader.GetTestAccessor().BuildArgumentsBuildArguments(maxCount, RefFilterOptions.All, "", "", "");

            if (expected)
            {
                args.ToString().Should().Contain($" --max-count={maxCount} ");
            }
            else
            {
                args.ToString().Should().NotContain(" --max-count=");
            }
        }

        [Test]
        public void BuildArguments_should_be_NUL_terminated()
        {
            var args = _revisionReader.GetTestAccessor().BuildArgumentsBuildArguments(-1, RefFilterOptions.All, "", "", "");

            args.ToString().Should().Contain(" log -z ");
        }

        [TestCase(RefFilterOptions.FirstParent, false)]
        [TestCase(RefFilterOptions.FirstParent | RefFilterOptions.Reflogs, false)]
        [TestCase(RefFilterOptions.All, false)]
        [TestCase(RefFilterOptions.All | RefFilterOptions.Reflogs, true)]
        public void BuildArguments_should_add_reflog_if_requested(RefFilterOptions refFilterOptions, bool expected)
        {
            var args = _revisionReader.GetTestAccessor().BuildArgumentsBuildArguments(-1, refFilterOptions, "", "", "");

            if (expected)
            {
                args.ToString().Should().Contain(" --reflog ");
            }
            else
            {
                args.ToString().Should().NotContain(" --reflog ");
            }
        }

        /* first 'parent first' */
        [TestCase(RefFilterOptions.FirstParent, " --first-parent ", null)]
        [TestCase(RefFilterOptions.FirstParent | RefFilterOptions.NoMerges, " --first-parent ", null)]
        [TestCase(RefFilterOptions.All, null, " --first-parent ")]
        /* if not 'first parent', then 'all' */
        [TestCase(RefFilterOptions.FirstParent, null, " --all ")]
        [TestCase(RefFilterOptions.FirstParent | RefFilterOptions.All, null, " --all ")]
        [TestCase(RefFilterOptions.All, " --all ", null)]
        /* if not 'first parent' and not 'all' - selected branches, if requested */
        [TestCase(RefFilterOptions.FirstParent | RefFilterOptions.Remotes, " --first-parent ", " --branches=")]
        [TestCase(RefFilterOptions.All | RefFilterOptions.Remotes, " --all ", " --branches=")]
        [TestCase(RefFilterOptions.Branches, " --branches=", null)]
        /* if not 'first parent' and not 'all' - *ALL* remotes, if requested */
        [TestCase(RefFilterOptions.FirstParent | RefFilterOptions.Remotes, " --first-parent ", " --remotes ")]
        [TestCase(RefFilterOptions.All | RefFilterOptions.Remotes, " --all ", " --remotes ")]
        [TestCase(RefFilterOptions.Remotes, " --remotes ", null)]
        /* if not 'first parent' and not 'all' - *ALL* tags, if requested */
        [TestCase(RefFilterOptions.FirstParent | RefFilterOptions.Tags, " --first-parent ", " --tags ")]
        [TestCase(RefFilterOptions.All | RefFilterOptions.Tags, " --all ", " --tags ")]
        [TestCase(RefFilterOptions.Tags, " --tags ", null)]
        public void BuildArguments_check_parameters(RefFilterOptions refFilterOptions, string expectedToContain, string notExpectedToContain)
        {
            var args = _revisionReader.GetTestAccessor().BuildArgumentsBuildArguments(-1, refFilterOptions, "my_*", "my_revision", "my_path");

            if (expectedToContain is not null)
            {
                args.ToString().Should().Contain(expectedToContain);
            }

            if (notExpectedToContain is not null)
            {
                args.ToString().Should().NotContain(notExpectedToContain);
            }
        }

        [Test]
        public void TryParseRevisionshould_return_false_if_argument_is_invalid()
        {
            ArraySegment<byte> chunk = null;

            bool res = RevisionReader.TestAccessor.TryParseRevision(chunk, _getEncodingByGitName, _logOutputEncoding, _sixMonths, out _);
            res.Should().BeFalse();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]

        // Avoid launching the difftool at differences
        // APPVEYOR should be detected automatically, this forces the setting (also in local tests)
        // The popup will hang the tests without failure information
        [UseReporter(typeof(AppVeyorReporter))]
        [Test]
        [TestCase("bad_parentid", false)]
        [TestCase("bad_parentid_length", false)]
        [TestCase("bad_sha", false)]
        [TestCase("empty", false)]
        [TestCase("illegal_timestamp", true, true)]
        [TestCase("multi_pathfilter", true)]
        [TestCase("no_encoding", false)]
        [TestCase("no_subject", true)]
        [TestCase("normal", true)]
        [TestCase("short_sha", false)]
        [TestCase("simple_pathfilter", true)]
        [TestCase("subject_no_body", true)]
        [TestCase("empty_commit", true)]
        public void TryParseRevision_test(string testName, bool expectedReturn, bool serialThrows = false)
        {
            using (ApprovalResults.ForScenario(testName.Replace(' ', '_')))
            {
                string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData/RevisionReader", testName + ".bin");
                ArraySegment<byte> chunk = File.ReadAllBytes(path);

                // Set to a high value so Debug.Assert do not raise exceptions
                RevisionReader.TestAccessor.NoOfParseError = 100;
                RevisionReader.TestAccessor.TryParseRevision(chunk, _getEncodingByGitName, _logOutputEncoding, _sixMonths, out GitRevision rev)
                    .Should().Be(expectedReturn);

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
                    Approvals.VerifyJson(JsonConvert.SerializeObject(rev, timeZoneSettings));
                }
            }
        }
    }
}
