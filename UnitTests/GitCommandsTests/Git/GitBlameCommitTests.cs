using System;
using System.Text;
using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests.Git
{
    [TestFixture]
    public class GitBlameCommitTests
    {
        [Test]
        public void TestToString()
        {
            var committerTime = DateTime.Now;
            var authorTime = DateTime.Now;

            var str = new StringBuilder();

            str.AppendLine("Author: Author");
            str.AppendLine("AuthorTime: " + authorTime);
            str.AppendLine("Committer: committer");
            str.AppendLine("CommitterTime: " + committerTime);
            str.AppendLine("Summary: test summary");
            str.AppendLine();
            str.Append("FileName: fileName.txt");

            var commit = new GitBlameCommit(
                "ObjectId",
                "Author",
                "author@authormail.com",
                authorTime,
                "authorTimeZone",
                "committer",
                "committer@authormail.com",
                committerTime,
                "committerTimeZone",
                "test summary",
                "fileName.txt");

            Assert.AreEqual(str.ToString(), commit.ToString());
        }
    }
}
