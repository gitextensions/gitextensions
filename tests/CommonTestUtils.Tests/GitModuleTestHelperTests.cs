using FluentAssertions;

namespace CommonTestUtils.Tests
{
    [TestFixture]
    public class GitModuleTestHelperTests
    {
        [Test]
        public void Should_detect_CI_build()
        {
            foreach (System.Collections.DictionaryEntry varName in Environment.GetEnvironmentVariables())
            {
                TestContext.WriteLine("ENV: {0}={1}", varName.Key, varName.Value);
            }

            GitModuleTestHelper.IsCIBuild.Should().BeTrue();
        }
    }
}
