using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;

namespace GitCommandsTests.Git_Commands;

[TestFixture]
public sealed class GitCheckoutBranchCmdTest
{
    [Test]
    public void TestConstructor()
    {
        IGitCommand cmd = Commands.CheckoutBranch("branchName", remote: true);

        ClassicAssert.IsNotNull(cmd);
    }

    [Test]
    public void TestConstructorRemoteIsFalse()
    {
        IGitCommand cmd = Commands.CheckoutBranch("branchName", remote: false);

        ClassicAssert.IsNotNull(cmd);
    }

    [Test]
    public void TestAccessesRemoteIsFalse()
    {
        IGitCommand cmd = Commands.CheckoutBranch("branchName", remote: true);

        ClassicAssert.IsFalse(cmd.AccessesRemote);
    }

    [Test]
    public void TestCollectArgumentsMergeReset()
    {
        // Merge

        ClassicAssert.AreEqual(
            "checkout --merge \"branchName\"",
            Commands.CheckoutBranch("branchName", remote: false, LocalChangesAction.Merge).Arguments);

        ClassicAssert.AreEqual(
            "checkout --merge -b \"newBranchName\" \"branchName\"",
            Commands.CheckoutBranch("branchName", remote: true, LocalChangesAction.Merge, CheckoutNewBranchMode.Create, "newBranchName").Arguments);

        ClassicAssert.AreEqual(
            "checkout --merge -B \"newBranchName\" \"branchName\"",
            Commands.CheckoutBranch("branchName", remote: true, LocalChangesAction.Merge, CheckoutNewBranchMode.Reset, "newBranchName").Arguments);

        // Reset

        ClassicAssert.AreEqual(
            "checkout --force \"branchName\"",
            Commands.CheckoutBranch("branchName", remote: false, LocalChangesAction.Reset).Arguments);

        ClassicAssert.AreEqual(
            "checkout --force -b \"newBranchName\" \"branchName\"",
            Commands.CheckoutBranch("branchName", remote: true, LocalChangesAction.Reset, CheckoutNewBranchMode.Create, "newBranchName").Arguments);

        ClassicAssert.AreEqual(
            "checkout --force -B \"newBranchName\" \"branchName\"",
            Commands.CheckoutBranch("branchName", remote: true, LocalChangesAction.Reset, CheckoutNewBranchMode.Reset, "newBranchName").Arguments);
    }
}
