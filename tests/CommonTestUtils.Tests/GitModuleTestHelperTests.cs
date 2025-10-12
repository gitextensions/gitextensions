using FluentAssertions;

namespace CommonTestUtils.Tests
{
    [TestFixture]
    public class GitModuleTestHelperTests
    {
        [Test]
        public void Should_detect_CI_build()
        {
            GitModuleTestHelper.IsCIBuild.Should().BeTrue();
        }
    }
}
