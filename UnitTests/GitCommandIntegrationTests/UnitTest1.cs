using FluentAssertions;
using NUnit.Framework;

namespace GitCommandIntegrationTests
{
    [TestFixture]
    public class UnitTest1
    {
        [Test]
        public void TestMethod1()
        {
            true.Should().Be(true);
        }
    }
}
