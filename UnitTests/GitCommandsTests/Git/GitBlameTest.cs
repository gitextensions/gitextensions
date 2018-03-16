using System;
using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests.Git
{
    [TestFixture]
    public class GitBlameTest
    {
        [Test]
        public void TestConstructor()
        {
            GitBlame blame = new GitBlame();
            Assert.IsNotNull(blame);
            Assert.IsNotNull(blame.Headers);
            Assert.IsNotNull(blame.Lines);
        }

        [Test]
        public void TestFindHeaderForCommitGuid()
        {
            GitBlame blame = new GitBlame();
            string expectedCommitGuid = Guid.NewGuid().ToString();

            GitBlameHeader expectedHeader = new GitBlameHeader { CommitGuid = expectedCommitGuid };

            blame.Headers.Add(expectedHeader);
            Assert.IsTrue(blame.FindHeaderForCommitGuid(expectedCommitGuid).Equals(expectedHeader));
        }
    }
}
