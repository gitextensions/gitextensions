﻿using System;
using System.Linq;
using GitCommands;
using GitCommands.Git;
using NSubstitute;
using NUnit.Framework;
using ResourceManager;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;

namespace GitExtensionsTest
{
    [TestClass]
    public class CommitInformationTest
    {

        [TestMethod]
        public void CanCreateCommitInformationFromFormatedData()
        {
            var commitGuid = Guid.NewGuid();
            var treeGuid = Guid.NewGuid();
            var parentGuid1 = Guid.NewGuid().ToString();
            var parentGuid2 = Guid.NewGuid().ToString();
            var authorTime = DateTime.UtcNow.AddDays(-3);
            var commitTime = DateTime.UtcNow.AddDays(-2);
            var authorUnixTime = (int)(authorTime - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            var commitUnixTime = (int)(commitTime - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

            var revisionProvider = Substitute.For<IGitRevisionProvider>();
            var parent1Rev = new GitRevision(aModule: null, guid: parentGuid1) {Subject = "Parent1"};
            var parent2Rev = new GitRevision(aModule: null, guid: parentGuid2) {Subject = "Parent2"};
            revisionProvider.GetRevision(parentGuid1, shortFormat: true).Returns(parent1Rev);
            revisionProvider.GetRevision(parentGuid2, shortFormat: true).Returns(parent2Rev);

            var rawData = commitGuid + "\n" +
                          treeGuid + "\n" +
                          parentGuid1 + " " + parentGuid2 + "\n" +
                          "John Doe (Acme Inc) <John.Doe@test.com>\n" +
                          authorUnixTime + "\n" +
                          "Jane Doe (Acme Inc) <Jane.Doe@test.com>\n" +
                          commitUnixTime + "\n" +
                          "\n" +
                          "\tI made a really neato change.\n\n" +
                          "Notes (p4notes):\n" +
                          "\tP4@547123";

            var expectedHeader = "Author:\t\t<a href='mailto:John.Doe@test.com'>John Doe (Acme Inc) &lt;John.Doe@test.com&gt;</a>" + Environment.NewLine +
                                 "Author date:\t3 days ago (" + LocalizationHelpers.GetFullDateString(authorTime) + ")" + Environment.NewLine +
                                 "Committer:\t<a href='mailto:Jane.Doe@test.com'>Jane Doe (Acme Inc) &lt;Jane.Doe@test.com&gt;</a>" + Environment.NewLine +
                                 "Commit date:\t2 days ago (" + LocalizationHelpers.GetFullDateString(commitTime) + ")" + Environment.NewLine +
                                 "Commit hash:\t" + commitGuid + Environment.NewLine +
                                 "Parent(s):\t<a href='gitext://gotocommit/" + parentGuid1 + "'>" + parentGuid1.Substring(0, 10) + "</a> " + parent1Rev.Subject +
                                 Environment.NewLine +
                                 " \t\t<a href='gitext://gotocommit/" + parentGuid2 + "'>" + parentGuid2.Substring(0, 10) + "</a> " + parent2Rev.Subject;

            var expectedBody = "\nI made a really neato change." + Environment.NewLine + Environment.NewLine +
                               "Notes (p4notes):" + Environment.NewLine +
                               "\tP4@547123";

            var commitData = CommitData.CreateFromFormatedData(rawData, new GitModule(""));
            var commitInformation = CommitInformation.GetCommitInfo(revisionProvider, commitData, true);

            Assert.AreEqual(expectedHeader, commitInformation.Header);
            Assert.AreEqual(expectedBody, commitInformation.Body);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CanCreateCommitInformationFromFormatedDataThrowsException()
        {
            CommitInformation.GetCommitInfo(new GitModule(""), null, true);
        }

        [TestMethod]
        public void GetCommitInfoTestWhenDataIsNull()
        {
            var actualResult = CommitInformation.GetCommitInfo(new GitModule(""), "fakesha1");
            Assert.AreEqual("Cannot find commit fakesha1", actualResult.Header);
        }

        [TestMethod]
        public void GetAllBranchesWhichContainGivenCommitTestReturnsEmptyList()
        {
            var module = new GitModule("");
            var actualResult = module.GetAllBranchesWhichContainGivenCommit("fakesha1", false, false);

            Assert.IsNotNull(actualResult);
            Assert.IsTrue(!actualResult.Any());
        }
    }
}
