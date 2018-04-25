using ApprovalTests;
using NUnit.Framework;

namespace GitCommandIntegrationTests
{
    [TestFixture]
    public class UnitTest1
    {
        [Test]
        public void TestMethod1()
        {
            Approvals.Verify("ApprovalTests is working");
        }
    }
}
