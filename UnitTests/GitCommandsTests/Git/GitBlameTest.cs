using System;
using GitCommands;
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;

namespace GitCommandsTests.Git
{
    [TestClass]
    public class GitBlameTest
    {
        [TestMethod]
        public void TestConstructor()
        {
            GitBlame blame = new GitBlame();
            Assert.IsNotNull(blame);
            Assert.IsNotNull(blame.Headers);
            Assert.IsNotNull(blame.Lines);
        }

        [TestMethod]
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

