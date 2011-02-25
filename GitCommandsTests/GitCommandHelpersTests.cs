using System;
using GitCommands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GitCommandsTests
{
    [TestClass]
    public class GitCommandsHelperTests
    {
        [TestMethod]
        public void CanGetRelativeDateString()
        {
            Assert.AreEqual("1 minute ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddMinutes(-1)));
            Assert.AreEqual("1 day ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(-1)));
            Assert.AreEqual("1 week ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(-7)));
            Assert.AreEqual("1 month ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(-30)));
            Assert.AreEqual("1 year ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(-365)));

            Assert.AreEqual("2 minutes ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddMinutes(-2)));
            Assert.AreEqual("2 days ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(-2)));
            Assert.AreEqual("2 weeks ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(-14)));
            Assert.AreEqual("2 months ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(-60)));
            Assert.AreEqual("2 years ago", GitCommandHelpers.GetRelativeDateString(DateTime.Now, DateTime.Now.AddDays(-730)));
        }
    }
}
