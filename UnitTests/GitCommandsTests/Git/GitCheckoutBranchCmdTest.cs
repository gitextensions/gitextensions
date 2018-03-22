using System.Collections.Generic;
using System.Linq;
using GitCommands;
using GitCommands.Git;
using NUnit.Framework;

namespace GitCommandsTests.Git
{
    [TestFixture]
    public class GitCheckoutBranchCmdTest
    {
        private GitCheckoutBranchCmd GetInstance(bool remote)
        {
            return new GitCheckoutBranchCmd("branchName", remote);
        }

        [Test]
        public void TestConstructor()
        {
            GitCheckoutBranchCmd cmd = GetInstance(true);
            Assert.IsNotNull(cmd);
            Assert.AreEqual(cmd.BranchName, "branchName");
            Assert.IsTrue(cmd.Remote);
        }

        [Test]
        public void TestConstructorRemoteIsFalse()
        {
            GitCheckoutBranchCmd cmd = GetInstance(false);
            Assert.IsNotNull(cmd);
            Assert.AreEqual(cmd.BranchName, "branchName");
            Assert.IsFalse(cmd.Remote);
        }

        [Test]
        public void TestGitCommandName()
        {
            GitCheckoutBranchCmd cmd = GetInstance(true);
            Assert.AreEqual(cmd.GitComandName(), "checkout");
        }

        [Test]
        public void TestAccessesRemoteIsFalse()
        {
            GitCheckoutBranchCmd cmd = GetInstance(true);
            Assert.IsFalse(cmd.AccessesRemote());
        }

        [Test]
        public void TestCollectArgumentsMergeReset()
        {
            GitCheckoutBranchCmd cmd = GetInstance(true);

            IEnumerable<string> whenMergeChangesOnly = new List<string> { "checkout", "--merge", "\"branchName\"" };
            IEnumerable<string> whenMergeChangesWithRemoteNewBranchCreate = new List<string> { "checkout", "--merge", "-b \"newBranchName\"", "\"branchName\"" };
            IEnumerable<string> whenMergeChangesWithRemoteNewBranchReset = new List<string> { "checkout", "--merge", "-B \"newBranchName\"", "\"branchName\"" };

            IEnumerable<string> whenResetChangesOnly = new List<string> { "checkout", "--force", "\"branchName\"" };
            IEnumerable<string> whenResetChangesWithRemoteNewBranchCreate = new List<string> { "checkout", "--force", "-b \"newBranchName\"", "\"branchName\"" };
            IEnumerable<string> whenResetChangesWithRemoteNewBranchReset = new List<string> { "checkout", "--force", "-B \"newBranchName\"", "\"branchName\"" };

            // Merge
            {
                cmd.LocalChanges = LocalChangesAction.Merge;
                cmd.Remote = false;

                Assert.AreEqual(cmd.ToLine(), whenMergeChangesOnly.Join(" "));

                cmd.NewBranchAction = GitCheckoutBranchCmd.NewBranch.Create;
                cmd.Remote = true;
                cmd.NewBranchName = "newBranchName";

                Assert.AreEqual(cmd.ToLine(), whenMergeChangesWithRemoteNewBranchCreate.Join(" "));

                cmd.NewBranchAction = GitCheckoutBranchCmd.NewBranch.Reset;

                Assert.AreEqual(cmd.ToLine(), whenMergeChangesWithRemoteNewBranchReset.Join(" "));
            }

            // Reset
            {
                cmd.LocalChanges = LocalChangesAction.Reset;
                cmd.Remote = false;

                Assert.AreEqual(cmd.ToLine(), whenResetChangesOnly.Join(" "));

                cmd.NewBranchAction = GitCheckoutBranchCmd.NewBranch.Create;
                cmd.Remote = true;
                cmd.NewBranchName = "newBranchName";

                Assert.AreEqual(cmd.ToLine(), whenResetChangesWithRemoteNewBranchCreate.Join(" "));

                cmd.NewBranchAction = GitCheckoutBranchCmd.NewBranch.Reset;

                Assert.AreEqual(cmd.ToLine(), whenResetChangesWithRemoteNewBranchReset.Join(" "));
            }
        }
    }
}
