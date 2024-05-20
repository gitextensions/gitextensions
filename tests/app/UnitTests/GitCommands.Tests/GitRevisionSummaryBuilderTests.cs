using System.Reflection;
using System.Runtime.CompilerServices;
using CommonTestUtils;
using GitCommands;

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
        public async Task Should_do_ellipsis(string testName)
        {
            string content = EmbeddedResourceLoader.Load(Assembly.GetExecutingAssembly(), $"{GetType().Namespace}.MockData.{testName}.txt");

            await Verifier.Verify(new GitRevisionSummaryBuilder().BuildSummary(content));
        }
    }
}
