using NUnit.Framework;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestContext = System.Object;
using TestProperty = NUnit.Framework.PropertyAttribute;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;

using System;
using System.Text;
using GitCommands;
using System.Collections.Generic;

namespace GitExtensionsTest.Git
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

