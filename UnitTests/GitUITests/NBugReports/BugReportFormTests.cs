using System.Threading;
using FluentAssertions;
using GitUI.NBugReports;
using NUnit.Framework;

namespace GitUITests.NBugReports
{
    [Apartment(ApartmentState.STA)]
    [TestFixture]
    public class BugReportFormTests
    {
        private BugReportForm _form;

        [SetUp]
        public void Setup()
        {
            _form = new BugReportForm();
        }

        [TestCase("", false)]
        [TestCase("\t\r\n\t\t   \r   \n   \r", false)]
        [TestCase("\t\r\n\t\t  a \r   \n   \r", true)]
        public void Test(string input, bool expected)
        {
            _form.GetTestAccessor().CheckContainsInfo(input).Should().Be(expected);
        }
    }
}