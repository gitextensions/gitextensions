using System;
using System.Drawing;
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
        public void TestGetColor()
        {
            string randomGuid = "b35a3233-8345-43aa-a618-2ca0de12000c";

            GitBlameHeader header = new GitBlameHeader { CommitGuid = randomGuid };

            Color expectedColor = Color.FromArgb(255, 246, 226, 238);

            Assert.AreEqual(expectedColor, header.GetColor());
        }

        [Test]
        public void TestEquals()
        {
            GitBlameHeader header1 = new GitBlameHeader { Author = "Author" };
            GitBlameHeader header2 = new GitBlameHeader { Author = "Author" };

            Assert.IsTrue(header1.Equals(header2));
            Assert.IsFalse(header1.Equals(null));
        }

        [Test]
        public void TestEqualsFails()
        {
            GitBlameHeader header1 = new GitBlameHeader { Author = "Author" };
            GitBlameHeader header2 = new GitBlameHeader();

            Assert.IsFalse(header1.Equals(header2));
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
