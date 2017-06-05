using System.Collections.Generic;
using System.Linq;
using GitCommands;
using GitCommands.Git;
using NUnit.Framework;

namespace GitExtensionsTest.GitCommands.Git
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
            var cmd = GetInstance(true);
            Assert.IsNotNull(cmd);
            Assert.AreEqual(cmd.BranchName, "branchName");
            Assert.IsTrue(cmd.Remote);
        }

        [Test]
        public void TestConstructorRemoteIsFalse()
        {
            var cmd = GetInstance(false);
            Assert.IsNotNull(cmd);
            Assert.AreEqual(cmd.BranchName, "branchName");
            Assert.IsFalse(cmd.Remote);
        }

        [Test]
        public void TestGitCommandName()
        {
            var cmd = GetInstance(true);
            Assert.AreEqual(cmd.GitComandName(), "checkout");
        }

        [Test]
        public void TestAccessesRemoteIsFalse()
        {
            var cmd = GetInstance(true);
            Assert.IsFalse(cmd.AccessesRemote());
        }

        [TestCase((string)null)]
        [TestCase("")]
        public void CollectArguments_should_set_branchName_if_null(string branchName)
        {
            var cmd = new GitCheckoutBranchCmd(branchName, true);
            cmd.LocalChanges = LocalChangesAction.Merge;
            cmd.Remote = false;

            Assert.IsTrue(cmd.CollectArguments().SequenceEqual(new List<string> { "--merge", "" }));
        }

        [Test]
        public void CollectArguments_merge_changes_only()
        {
            var cmd = GetInstance(true);
            cmd.LocalChanges = LocalChangesAction.Merge;
            cmd.Remote = false;

            Assert.IsTrue(cmd.CollectArguments().SequenceEqual(new List<string> { "--merge", "\"branchName\"" }));
        }

        [Test]
        public void CollectArguments_merge_with_remote_NewBranch_Create()
        {
            var cmd = GetInstance(true);
            cmd.LocalChanges = LocalChangesAction.Merge;
            cmd.NewBranchAction = GitCheckoutBranchCmd.NewBranch.Create;
            cmd.Remote = true;
            cmd.NewBranchName = "newBranchName";

            Assert.IsTrue(cmd.CollectArguments().SequenceEqual(new List<string> { "--merge", "-b \"newBranchName\"", "\"branchName\"" }));
        }

        [Test]
        public void CollectArguments_merge_with_remote_NewBranch_Reset()
        {
            var cmd = GetInstance(true);
            cmd.LocalChanges = LocalChangesAction.Merge;
            cmd.NewBranchAction = GitCheckoutBranchCmd.NewBranch.Reset;
            cmd.Remote = true;
            cmd.NewBranchName = "newBranchName";

            Assert.IsTrue(cmd.CollectArguments().SequenceEqual(new List<string> { "--merge", "-B \"newBranchName\"", "\"branchName\"" }));
        }

        [Test]
        public void CollectArguments_reset_changes_only()
        {
            var cmd = GetInstance(true);
            cmd.LocalChanges = LocalChangesAction.Reset;
            cmd.Remote = false;

            Assert.IsTrue(cmd.CollectArguments().SequenceEqual(new List<string> { "--force", "\"branchName\"" }));
        }

        [Test]
        public void CollectArguments_reset_with_remote_NewBranch_Create()
        {
            var cmd = GetInstance(true);
            cmd.LocalChanges = LocalChangesAction.Reset;
            cmd.NewBranchAction = GitCheckoutBranchCmd.NewBranch.Create;
            cmd.Remote = true;
            cmd.NewBranchName = "newBranchName";

            Assert.IsTrue(cmd.CollectArguments().SequenceEqual(new List<string> { "--force", "-b \"newBranchName\"", "\"branchName\"" }));
        }

        [Test]
        public void CollectArguments_reset_with_remote_NewBranch_Reset()
        {
            var cmd = GetInstance(true);
            cmd.LocalChanges = LocalChangesAction.Reset;
            cmd.NewBranchAction = GitCheckoutBranchCmd.NewBranch.Reset;
            cmd.Remote = true;
            cmd.NewBranchName = "newBranchName";

            Assert.IsTrue(cmd.CollectArguments().SequenceEqual(new List<string> { "--force", "-B \"newBranchName\"", "\"branchName\"" }));
        }
    }
}

