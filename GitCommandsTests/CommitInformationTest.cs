#if !NUNIT
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Category = Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute;
#else
using NUnit.Framework;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestContext = System.Object;
using TestProperty = NUnit.Framework.PropertyAttribute;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
#endif
using System;
using System.Linq;
using GitCommands;
using System.Net;

namespace GitCommandsTests
{
    [TestClass]
    public class CommitInformationTest
    {
        private static string GetCurrentDir()
        {
            string path = typeof(FindValidworkingDirTest).Assembly.Location;

            return path.Substring(0, path.LastIndexOf('\\'));
        }

        private GitModule _Module;
        private GitModule Module
        {
            get
            {
                if (_Module == null)
                    _Module = new GitModule(GetCurrentDir());
                return _Module;
            }
        }

        [TestMethod]
        public void CanCreateCommitInformationFromCommitData()
        {
            string error = "";
            CommitData data = CommitData.GetCommitData(Module, "5003a05de05307b83b0c8a81ab282613231b1a9f", ref error);
            Assert.IsNotNull(data);
            Assert.AreNotEqual(data.AuthorDate, data.CommitDate);

            var expectedHeader = "Author:\t\t<a href='mailto:getit@xs4all.nl'>Wilbert van Dolleweerd &lt;getit@xs4all.nl&gt;</a>" + Environment.NewLine +
                                 "Author date:\t3 years ago (" + data.AuthorDate.ToLocalTime().ToString("ddd MMM dd HH':'mm':'ss yyyy") + ")" + Environment.NewLine +
                                 "Committer:\t<a href='mailto:Henk_Westhuis@hotmail.com'>Henk Westhuis &lt;Henk_Westhuis@hotmail.com&gt;</a>" + Environment.NewLine +
                                 "Commit date:\t3 years ago (" + data.CommitDate.ToLocalTime().ToString("ddd MMM dd HH':'mm':'ss yyyy") + ")" + Environment.NewLine +
                                 "Commit hash:\t" + data.Guid + Environment.NewLine +
                                 "Parent(s):\t<a href='gitex://gotocommit/" + data.ParentGuids[0] + "'>" + data.ParentGuids[0].Substring(0, 10) + "</a>";

            // TODO: Check notes
            var expectedBody = "\n\n" + WebUtility.HtmlEncode(data.Body.Trim()) + "\n\n";

            var commitInformation = CommitInformation.GetCommitInfo(data);

            Assert.AreEqual(expectedHeader, commitInformation.Header);
            Assert.AreEqual(expectedBody, commitInformation.Body);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanCreateCommitInformationFromCommitDataThrowsException()
        {
            CommitInformation.GetCommitInfo(data: null);
        }

        [TestMethod]
        public void GetCommitInfoTestWhenDataIsNull()
        {
            var actualResult = CommitInformation.GetCommitInfo(Module, "fakesha1");
            Assert.AreEqual("Cannot find commit fakesha1", actualResult.Header);
        }

        [TestMethod]
        public void GetAllBranchesWhichContainGivenCommitTestReturnsEmptyList()
        {
            var actualResult = CommitInformation.GetAllBranchesWhichContainGivenCommit(Module, "fakesha1", false, false);

            Assert.IsNotNull(actualResult);
            Assert.IsTrue(!actualResult.Any());
        }
    }
}
