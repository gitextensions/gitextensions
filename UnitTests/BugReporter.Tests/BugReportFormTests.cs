using BugReporter;
using FluentAssertions;
using NUnit.Framework;

namespace GitUITests
{
    [TestFixture]
    public class BugReportFormTests
    {
        [TestCase("", false)]
        [TestCase("\t\r\n\t\t   \r   \n   \r", false)]
        [TestCase("\t\r\n\t\t  a \r   \n   \r", true)]
        public void Test(string input, bool expected)
        {
            BugReportForm.TestAccessor.CheckContainsInfo(input).Should().Be(expected);
        }
    }
}
