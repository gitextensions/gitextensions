using GitCommands;
using GitExtensions.Extensibility.Git;
using NSubstitute;

namespace GitCommandsTests.Git;

public sealed class GitRefTests
{
    [Test]
    public void IsTrackingRemote_should_return_true_when_tracking_remote()
    {
        string remoteBranchShortName = "remote_branch";
        string remoteName = "origin";
        IGitRef localBranchRef = SetupLocalBranchWithATrackingReference(remoteBranchShortName, remoteName);

        IGitRef remoteBranchRef = SetupRemoteRef(remoteBranchShortName, remoteName);

        localBranchRef.IsTrackingRemote(remoteBranchRef).Should().BeTrue();
    }

    [Test]
    public void IsTrackingRemote_should_return_false_when_remote_is_null()
    {
        IGitRef localBranchRef = SetupLocalBranchWithATrackingReference("remote_branch", "origin");

        localBranchRef.IsTrackingRemote(null).Should().BeFalse();
    }

    [Test]
    public void IsTrackingRemote_should_return_false_when_tracking_another_remote()
    {
        string remoteBranchShortName = "remote_branch";
        IGitRef localBranchRef = SetupLocalBranchWithATrackingReference(remoteBranchShortName, "origin");

        IGitRef remoteBranchRef = SetupRemoteRef(remoteBranchShortName, "upstream");

        localBranchRef.IsTrackingRemote(remoteBranchRef).Should().BeFalse();
    }

    [Test]
    public void IsTrackingRemote_should_return_false_when_tracking_another_remote_branch()
    {
        IGitRef localBranchRef = SetupLocalBranchWithATrackingReference("one_remote_branch", "origin");

        IGitRef remoteBranchRef = SetupRemoteRef("another_remote_branch", "origin");

        localBranchRef.IsTrackingRemote(remoteBranchRef).Should().BeFalse();
    }

    [Test]
    public void IsTrackingRemote_should_return_false_when_supposedly_local_branch_is_a_remote_ref()
    {
        IGitRef localBranchRef = SetupRemoteRef("a_remote_branch", "origin");

        IGitRef remoteBranchRef = SetupRemoteRef("a_remote_branch", "origin");

        localBranchRef.IsTrackingRemote(remoteBranchRef).Should().BeFalse();
    }

    [Test]
    public void IsTrackingRemote_should_return_false_when_supposedly_remote_branch_is_a_local_ref()
    {
        IGitRef localBranchRef = SetupLocalBranchWithATrackingReference("a_remote_branch", "origin");

        IGitRef remoteBranchRef = SetupLocalBranchWithATrackingReference("a_remote_branch", "origin");

        localBranchRef.IsTrackingRemote(remoteBranchRef).Should().BeFalse();
    }

    [Test]
    public void IsTrackingRemote_should_return_false_when_local_branch_is_tracking_nothing()
    {
        IGitModule localGitModule = Substitute.For<IGitModule>();
        localGitModule.GetEffectiveSetting($"branch.local_branch.merge").Returns(string.Empty);
        localGitModule.GetEffectiveSetting($"branch.local_branch.remote").Returns(string.Empty);
        IGitRef localBranchRef = new GitRef(localGitModule, ObjectId.Random(), "refs/heads/local_branch");

        IGitRef remoteBranchRef = SetupLocalBranchWithATrackingReference("a_remote_branch", "origin");

        localBranchRef.IsTrackingRemote(remoteBranchRef).Should().BeFalse();
    }

    [Test]
    public void Remote_Should_prefix_LocalName_for_Name()
    {
        string remoteName = "origin";
        string name = "local_branch";
        string completeName = $"refs/remotes/{remoteName}/{name}";

        IGitRef remoteBranchRef = SetupRawRemoteRef(remoteName, completeName);
        name.Should().Be(remoteBranchRef.LocalName);
    }

    [Test]
    public void If_Remote_is_not_prefix_of_Name_then_LocalName_should_return_Name()
    {
        // Not standard behavior but seem to occur for git-svn
        string remoteName = "Remote_longer_than_Name";
        string name = "a_short_name";
        string completeName = $"refs/remotes/{name}";

        IGitRef remoteBranchRef = SetupRawRemoteRef(remoteName, completeName);
        name.Should().Be(remoteBranchRef.LocalName);
    }

    private static IGitRef SetupRawRemoteRef(string remoteName, string completeName)
    {
        IGitModule localGitModule = Substitute.For<IGitModule>();
        localGitModule.GetEffectiveSetting($"branch.local_branch.merge").Returns(completeName);
        localGitModule.GetEffectiveSetting($"branch.local_branch.remote").Returns(remoteName);
        return new GitRef(localGitModule, ObjectId.Random(), completeName, remoteName);
    }

    private static IGitRef SetupRemoteRef(string remoteBranchShortName, string remoteName)
    {
        IGitModule remoteGitModule = Substitute.For<IGitModule>();
        return new GitRef(remoteGitModule, ObjectId.Random(), $"refs/remotes/{remoteName}/{remoteBranchShortName}", remoteName);
    }

    private static IGitRef SetupLocalBranchWithATrackingReference(string remoteShortName, string remoteName)
    {
        IGitModule localGitModule = Substitute.For<IGitModule>();
        localGitModule.GetEffectiveSetting($"branch.local_branch.merge").Returns($"refs/heads/{remoteShortName}");
        localGitModule.GetEffectiveSetting($"branch.local_branch.remote").Returns(remoteName);
        return new GitRef(localGitModule, ObjectId.Random(), "refs/heads/local_branch");
    }
}
