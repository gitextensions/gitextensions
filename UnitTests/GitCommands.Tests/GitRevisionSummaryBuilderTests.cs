using System.Reflection;
using System.Runtime.CompilerServices;
using ApprovalTests;
using ApprovalTests.Namers;
using CommonTestUtils;
using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests
{
    public sealed class GitRevisionSummaryBuilderTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void Should_return_null_When_body_is_null_or_whitespace(string bodyContent)
        {
            Assert.AreEqual(null, new GitRevisionSummaryBuilder().BuildSummary(bodyContent));
        }

        [TestCase("toto")]
        [TestCase("toto\ntata\ntiti")]
        public void Should_have_same_content_When_no_ellipsis(string bodyContent)
        {
            Assert.AreEqual(bodyContent, new GitRevisionSummaryBuilder().BuildSummary(bodyContent));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Test]
        [TestCase("Too_many_lines")]
        [TestCase("Too_long_lines")]
        public void Should_do_ellipsis(string testName)
        {
            var content = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.{testName}.txt");

            using (ApprovalResults.ForScenario(testName))
            {
                Approvals.Verify(new GitRevisionSummaryBuilder().BuildSummary(content));
            }
        }
    }
}
