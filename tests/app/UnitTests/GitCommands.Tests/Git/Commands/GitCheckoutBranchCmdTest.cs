using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;

namespace GitCommandsTests.Git_Commands;
public sealed class GitCheckoutBranchCmdTest
{
    [Test]
    public void TestConstructor()
    {
        IGitCommand cmd = Commands.CheckoutBranch("branchName", remote: true);

        cmd.Should().NotBeNull();
    }

    [Test]
    public void TestConstructorRemoteIsFalse()
    {
        IGitCommand cmd = Commands.CheckoutBranch("branchName", remote: false);

        cmd.Should().NotBeNull();
    }

    [Test]
    public void TestAccessesRemoteIsFalse()
    {
        IGitCommand cmd = Commands.CheckoutBranch("branchName", remote: true);

        cmd.AccessesRemote.Should().BeFalse();
    }

    [Test]
    public void TestCollectArgumentsMergeReset()
    {
        // Merge

        Commands.CheckoutBranch("branchName", remote: false, LocalChangesAction.Merge).Arguments.Should().Be("checkout --merge \"branchName\"");

        Commands.CheckoutBranch("branchName", remote: true, LocalChangesAction.Merge, CheckoutNewBranchMode.Create, "newBranchName").Arguments.Should().Be("checkout --merge -b \"newBranchName\" \"branchName\"");

        Commands.CheckoutBranch("branchName", remote: true, LocalChangesAction.Merge, CheckoutNewBranchMode.Reset, "newBranchName").Arguments.Should().Be("checkout --merge -B \"newBranchName\" \"branchName\"");

        // Reset

        Commands.CheckoutBranch("branchName", remote: false, LocalChangesAction.Reset).Arguments.Should().Be("checkout --force \"branchName\"");

        Commands.CheckoutBranch("branchName", remote: true, LocalChangesAction.Reset, CheckoutNewBranchMode.Create, "newBranchName").Arguments.Should().Be("checkout --force -b \"newBranchName\" \"branchName\"");

        Commands.CheckoutBranch("branchName", remote: true, LocalChangesAction.Reset, CheckoutNewBranchMode.Reset, "newBranchName").Arguments.Should().Be("checkout --force -B \"newBranchName\" \"branchName\"");
    }
}
