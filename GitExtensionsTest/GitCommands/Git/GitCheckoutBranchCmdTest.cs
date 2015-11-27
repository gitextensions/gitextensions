using System.Collections.Generic;
using System.Linq;
using GitCommands;
using GitCommands.Git;
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;

namespace GitExtensionsTest.Git
{
    [TestClass]
    public class GitCheckoutBranchCmdTest
    {
        private GitCheckoutBranchCmd GetInstance(bool remote)
        {
            return new GitCheckoutBranchCmd("branchName", remote);
        }

        [TestMethod]
        public void TestConstructor()
        {
            GitCheckoutBranchCmd cmd = GetInstance(true);
            Assert.IsNotNull(cmd);
            Assert.AreEqual(cmd.BranchName, "branchName");
            Assert.IsTrue(cmd.Remote);
        }

        [TestMethod]
        public void TestConstructorRemoteIsFalse()
        {
            GitCheckoutBranchCmd cmd = GetInstance(false);
            Assert.IsNotNull(cmd);
            Assert.AreEqual(cmd.BranchName, "branchName");
            Assert.IsFalse(cmd.Remote);
        }

        [TestMethod]
        public void TestGitCommandName()
        {
            GitCheckoutBranchCmd cmd = GetInstance(true);
            Assert.AreEqual(cmd.GitComandName(), "checkout");
        }

        [TestMethod]
        public void TestAccessesRemoteIsFalse()
        {
            GitCheckoutBranchCmd cmd = GetInstance(true);
            Assert.IsFalse(cmd.AccessesRemote());
        }

        [TestMethod]
        public void TestCollectArgumentsMergeReset()
        {
            GitCheckoutBranchCmd cmd = GetInstance(true);

            IEnumerable<string> whenMergeChangesOnly = new List<string> { "--merge", "\"branchName\"" };
            IEnumerable<string> whenMergeChangesWithRemoteNewBranchCreate = new List<string> { "--merge", "-b \"newBranchName\"", "\"branchName\"" };
            IEnumerable<string> whenMergeChangesWithRemoteNewBranchReset = new List<string> { "--merge", "-B \"newBranchName\"", "\"branchName\"" };

            IEnumerable<string> whenResetChangesOnly = new List<string> { "--force", "\"branchName\"" };
            IEnumerable<string> whenResetChangesWithRemoteNewBranchCreate = new List<string> { "--force", "-b \"newBranchName\"", "\"branchName\"" };
            IEnumerable<string> whenResetChangesWithRemoteNewBranchReset = new List<string> { "--force", "-B \"newBranchName\"", "\"branchName\"" };

            //Merge
            {
                cmd.LocalChanges = LocalChangesAction.Merge;
                cmd.Remote = false;

                Assert.IsTrue(cmd.CollectArguments().SequenceEqual(whenMergeChangesOnly));

                cmd.NewBranchAction = GitCheckoutBranchCmd.NewBranch.Create;
                cmd.Remote = true;
                cmd.NewBranchName = "newBranchName";

                Assert.IsTrue(cmd.CollectArguments().SequenceEqual(whenMergeChangesWithRemoteNewBranchCreate));

                cmd.NewBranchAction = GitCheckoutBranchCmd.NewBranch.Reset;

                Assert.IsTrue(cmd.CollectArguments().SequenceEqual(whenMergeChangesWithRemoteNewBranchReset));
            }

            //Reset
            {
                cmd.LocalChanges = LocalChangesAction.Reset;
                cmd.Remote = false;

                Assert.IsTrue(cmd.CollectArguments().SequenceEqual(whenResetChangesOnly));

                cmd.NewBranchAction = GitCheckoutBranchCmd.NewBranch.Create;
                cmd.Remote = true;
                cmd.NewBranchName = "newBranchName";

                Assert.IsTrue(cmd.CollectArguments().SequenceEqual(whenResetChangesWithRemoteNewBranchCreate));

                cmd.NewBranchAction = GitCheckoutBranchCmd.NewBranch.Reset;

                Assert.IsTrue(cmd.CollectArguments().SequenceEqual(whenResetChangesWithRemoteNewBranchReset));
            }

        }
    }
}

