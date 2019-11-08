using System.Reflection;
using System.Runtime.CompilerServices;
using ApprovalTests;
using ApprovalTests.Namers;
using CommonTestUtils;
using GitCommands;
using GitUIPluginInterfaces;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace GitCommandsTests
{
    public sealed class GitRevisionTests
    {
        [Test]
        public void Should_validate_full_sha1_correctly()
        {
            Assert.True(GitRevision.IsFullSha1Hash("0000000000000000000000000000000000000000"));
            Assert.True(GitRevision.IsFullSha1Hash("1111111111111111111111111111111111111111"));
            Assert.True(GitRevision.IsFullSha1Hash("0123456789abcdefa0123456789abcdefa012345"));

            Assert.False(GitRevision.IsFullSha1Hash("0123456789ABCDEFA0123456789ABCDEFA012345"));
            Assert.False(GitRevision.IsFullSha1Hash("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz"));
            Assert.False(GitRevision.IsFullSha1Hash("00000000000000000000000000000000000000000"));
            Assert.False(GitRevision.IsFullSha1Hash("000000000000000000000000000000000000000"));
            Assert.False(GitRevision.IsFullSha1Hash("0000000000000000000000000000000000000000 "));
            Assert.False(GitRevision.IsFullSha1Hash(" 0000000000000000000000000000000000000000"));
        }

        [TestCase(null)]
        [TestCase("toto")]
        [TestCase("toto\ntata\ntiti")]
        public void Should_have_same_content_When_no_ellipsis(string bodyContent)
        {
            var gitRevision = new GitRevision(ObjectId.Random());
            gitRevision.Body = bodyContent;
            gitRevision.HasMultiLineMessage = true;
            Assert.AreEqual(bodyContent, gitRevision.BodySummary);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Test]
        [TestCase("Too_many_lines")]
        [TestCase("Too_long_lines")]
        public void Should_do_ellipsis(string testName)
        {
            var content = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.{testName}.txt");
            var gitRevision = new GitRevision(ObjectId.Random()) { Body = content, HasMultiLineMessage = true };

            using (ApprovalResults.ForScenario(testName))
            {
                Approvals.Verify(gitRevision.BodySummary);
            }
        }
    }
}