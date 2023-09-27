using GitCommands;
using GitCommands.Git.Commands;

namespace GitCommandsTests.Git.Commands
{
    [TestFixture]
    public sealed class GitCheckoutBranchCmdTest
    {
        [TestCase(false)]
        [TestCase(true)]
        public void TestConstructor(bool isRemote)
        {
            GitCheckoutBranchCmd cmd = new("branchName", isRemote);

            Assert.AreEqual("branchName", cmd.BranchName);
            Assert.AreEqual(isRemote, cmd.Remote);
            Assert.AreEqual(LocalChangesAction.DontChange, cmd.LocalChanges);
            Assert.AreEqual(CheckoutNewBranchMode.DontCreate, cmd.NewBranchMode);
            Assert.IsFalse(cmd.AccessesRemote);
            Assert.IsTrue(cmd.ChangesRepoState);
        }

        [TestCase(LocalChangesAction.DontChange, "checkout \"branch\"")]
        [TestCase(LocalChangesAction.Merge, "checkout --merge \"branch\"")]
        [TestCase(LocalChangesAction.Reset, "checkout --force \"branch\"")]
        [TestCase(LocalChangesAction.Stash, "checkout \"branch\"")]
        public void Assert_local_action(LocalChangesAction action, string expected)
        {
            Assert.AreEqual(expected, new GitCheckoutBranchCmd("branch", remote: false, action, CheckoutNewBranchMode.Create).Arguments);
        }

        [TestCase(LocalChangesAction.Merge, CheckoutNewBranchMode.DontCreate, "checkout --merge \"branchName\"")]
        [TestCase(LocalChangesAction.Merge, CheckoutNewBranchMode.Create, "checkout --merge -b \"newBranchName\" \"branchName\"")]
        [TestCase(LocalChangesAction.Merge, CheckoutNewBranchMode.Reset, "checkout --merge -B \"newBranchName\" \"branchName\"")]
        [TestCase(LocalChangesAction.Reset, CheckoutNewBranchMode.DontCreate, "checkout --force \"branchName\"")]
        [TestCase(LocalChangesAction.Reset, CheckoutNewBranchMode.Create, "checkout --force -b \"newBranchName\" \"branchName\"")]
        [TestCase(LocalChangesAction.Reset, CheckoutNewBranchMode.Reset, "checkout --force -B \"newBranchName\" \"branchName\"")]
        public void Assert_remote_action(LocalChangesAction action, CheckoutNewBranchMode checkoutMode, string expected)
        {
            Assert.AreEqual(expected, new GitCheckoutBranchCmd("branchName", remote: true, action, checkoutMode, "newBranchName").Arguments);
        }
    }
}
