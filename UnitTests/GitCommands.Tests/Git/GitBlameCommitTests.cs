using System;
using System.Text;
using GitCommands;
using GitUIPluginInterfaces;
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
            var commitHash = ObjectId.Random();

            var str = new StringBuilder();

            str.AppendLine("Author: Author");
            str.AppendLine("Author date: " + authorTime);
            str.AppendLine("Committer: committer");
            str.AppendLine("Commit date: " + committerTime);
            str.AppendLine("Commit hash: " + commitHash.ToShortString());
            str.AppendLine("Summary: test summary");
            str.AppendLine();
            str.Append("FileName: fileName.txt");

            var commit = new GitBlameCommit(
                commitHash,
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
