using System;
using System.IO;
using System.Linq;
using GitCommands;
using NUnit.Framework;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;

namespace GitExtensionsTest
{
    [TestClass]
    public class CommitInformationTest
    {
        private static string GetCurrentDir()
        {
            string path = typeof(CommitInformationTest).Assembly.CodeBase.Replace("file:///", "");
            path = Path.GetDirectoryName(path);

            return GitModule.FindGitWorkingDir(path);
        }

        private GitModule _module;

        [TestInitialize()]
        public void Initialize()
        {
            _module = new GitModule(GetCurrentDir());
            _module = _module.SuperprojectModule;
        }

        [TestMethod, Category("libgit2sharp")]
        public void CanCreateCommitInformationFromFormatedData()
        {
            string error = "";
            CommitData data = CommitData.GetCommitData(_module, "5003a05de05307b83b0c8a81ab282613231b1a9f", ref error);
            Assert.IsNotNull(data);
            Assert.AreNotEqual(data.AuthorDate, data.CommitDate);
            DateTime currentDate = DateTime.UtcNow;
            DateTime authorDate = data.AuthorDate.LocalDateTime;
            DateTime commitDate = data.CommitDate.LocalDateTime;

            var expectedHeader = "Author:\t\t<a href='mailto:getit@xs4all.nl'>Wilbert van Dolleweerd &lt;getit@xs4all.nl&gt;</a>" + Environment.NewLine +
                                 "Author date:\t" + GitCommandHelpers.GetRelativeDateString(currentDate, authorDate) + " (" + GitCommandHelpers.GetFullDateString(data.AuthorDate) + ")" + Environment.NewLine +
                                 "Committer:\t<a href='mailto:Henk_Westhuis@hotmail.com'>Henk Westhuis &lt;Henk_Westhuis@hotmail.com&gt;</a>" + Environment.NewLine +
                                 "Commit date:\t" + GitCommandHelpers.GetRelativeDateString(currentDate, commitDate) + " (" + GitCommandHelpers.GetFullDateString(data.CommitDate) + ")" + Environment.NewLine +
                                 "Commit hash:\t" + data.Guid + Environment.NewLine +
                                 "Parent(s):\t<a href='gitex://gotocommit/" + data.ParentGuids[0] + "'>" + data.ParentGuids[0].Substring(0, 10) + "</a>";

            var expectedBody = "\nAdd correct reference to NetSpell.SpellChecker.dll\n\n" +
                "Signed-off-by: Henk Westhuis &lt;Henk_Westhuis@hotmail.com&gt;\n" + Environment.NewLine +
                "Notes:" + Environment.NewLine + "\tTest git notes";

            var commitInformation = CommitInformation.GetCommitInfo(data, true);

            Assert.AreEqual(expectedHeader, commitInformation.Header);
            Assert.AreEqual(expectedBody, commitInformation.Body);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanCreateCommitInformationFromFormatedDataThrowsException()
        {
            CommitInformation.GetCommitInfo(null, true);
        }

        [TestMethod, Category("libgit2sharp")]
        public void GetCommitInfoTestWhenDataIsNull()
        {
            var actualResult = CommitInformation.GetCommitInfo(_module, "fakesha1");
            Assert.AreEqual("Cannot find commit fakesha1", actualResult.Header);
        }

        [TestMethod, Category("libgit2sharp")]
        public void GetAllBranchesWhichContainGivenCommitTestReturnsEmptyList()
        {
            var module = _module;
            var actualResult = module.GetAllBranchesWhichContainGivenCommit("fakesha1", false, false);

            Assert.IsNotNull(actualResult);
            Assert.IsTrue(!actualResult.Any());
        }
    }
}
