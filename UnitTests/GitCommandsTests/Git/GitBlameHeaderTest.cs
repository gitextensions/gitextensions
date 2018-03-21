using System;
using System.Text;
using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests.Git
{
    [TestFixture]
    public class GitBlameHeaderTest
    {
        [Test]
        public void TestConstructor()
        {
            GitBlameHeader header = new GitBlameHeader();
            Assert.IsNotNull(header);
        }

        [Test]
        public void TestToString()
        {
            DateTime committerTime = DateTime.Now;
            DateTime authorTime = DateTime.Now;

            StringBuilder expectedHeader = new StringBuilder();

            expectedHeader.AppendLine("Author: Author");
            expectedHeader.AppendLine("AuthorTime: " + authorTime);
            expectedHeader.AppendLine("Committer: committer");
            expectedHeader.AppendLine("CommitterTime: " + committerTime);
            expectedHeader.AppendLine("Summary: test summary");
            expectedHeader.AppendLine();
            expectedHeader.Append("FileName: fileName.txt");

            GitBlameHeader header = new GitBlameHeader
            {
                Author = "Author",
                AuthorMail = "author@authormail.com",
                AuthorTime = authorTime,
                Committer = "committer",
                CommitterTime = committerTime,
                Summary = "test summary",
                FileName = "fileName.txt"
            };

            Assert.AreEqual(expectedHeader.ToString(), header.ToString());
        }
    }
}
